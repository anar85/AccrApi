using AccrServicesApi.Models.ViewModels;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccrServicesApi.Data.JobRepository
{
	public partial class AccrJobRepository
	{
		private static readonly JsonSerializerOptions CreditJsonOptions = new()
		{
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			PropertyNameCaseInsensitive = true,
			WriteIndented = false
		};

		/// <summary>
		/// UI override JSON-larını batch kreditlərinə tətbiq edir (CBAR göndərişində redaktələrin görünməsi üçün).
		/// Proc yoxdursa və ya xəta olarsa, base staging data saxlanılır.
		/// </summary>
		private async Task ApplyCreditOverridesAsync(OracleConnection conn, long batchId, List<Credit> credits)
		{
			if (credits.Count == 0)
				return;

			try
			{
				await using var cmd = conn.CreateCommand();
				cmd.BindByName = true;
				cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_GET_BATCH_OVERRIDES";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;
				cmd.Parameters.Add("P_OVERRIDES_CUR", OracleDbType.RefCursor, ParameterDirection.Output);

				await cmd.ExecuteNonQueryAsync();

				await using var reader = ((OracleRefCursor)cmd.Parameters["P_OVERRIDES_CUR"].Value).GetDataReader();
				var byId = credits.ToDictionary(c => c.Id);

				while (await reader.ReadAsync())
				{
					var creditId = Convert.ToInt64(reader["CREDIT_ID"]);
					var json = reader["CREDIT_JSON"] == DBNull.Value ? null : reader["CREDIT_JSON"]?.ToString();
					if (string.IsNullOrWhiteSpace(json) || !byId.ContainsKey(creditId))
						continue;

					var overridden = JsonSerializer.Deserialize<Credit>(json, CreditJsonOptions);
					if (overridden == null)
						continue;

					overridden.Id = creditId;
					var index = credits.FindIndex(c => c.Id == creditId);
					if (index >= 0)
						credits[index] = overridden;
				}
			}
			catch (OracleException)
			{
				// Proc hələ deploy olunmayıbsa export köhnə staging data ilə davam edir.
			}
		}

		public async Task<CreditDetailDto?> GetCreditDetailAsync(long batchId, long creditId, CancellationToken ct)
		{
			await using var conn = new OracleConnection(_connectionString);
			await conn.OpenAsync(ct);

			await using var cmd = conn.CreateCommand();
			cmd.BindByName = true;
			cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_GET_CREDIT_DATA";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;
			cmd.Parameters.Add("P_CREDIT_ID", OracleDbType.Int64).Value = creditId;

			var pFound = cmd.Parameters.Add("P_FOUND", OracleDbType.Int32);
			pFound.Direction = ParameterDirection.Output;

			var pStatus = cmd.Parameters.Add("P_STATUS", OracleDbType.Int32);
			pStatus.Direction = ParameterDirection.Output;

			var pStatusName = cmd.Parameters.Add("P_STATUS_NAME", OracleDbType.Varchar2, 100);
			pStatusName.Direction = ParameterDirection.Output;

			var pNumDog = cmd.Parameters.Add("P_NUM_DOG", OracleDbType.Varchar2, 100);
			pNumDog.Direction = ParameterDirection.Output;

			var pAccountNo = cmd.Parameters.Add("P_ACCOUNT_NO", OracleDbType.Varchar2, 100);
			pAccountNo.Direction = ParameterDirection.Output;

			var pOverrideJson = cmd.Parameters.Add("P_OVERRIDE_JSON", OracleDbType.Clob);
			pOverrideJson.Direction = ParameterDirection.Output;

			cmd.Parameters.Add("P_CREDITS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
			cmd.Parameters.Add("P_BORROWERS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
			cmd.Parameters.Add("P_COLLATERALS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
			cmd.Parameters.Add("P_GUARANTEES_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
			cmd.Parameters.Add("P_RELATIONS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
			cmd.Parameters.Add("P_SHAREHOLDERS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
			cmd.Parameters.Add("P_FINANCE_REPORTS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
			cmd.Parameters.Add("P_FOREIGN_ECO_CUR", OracleDbType.RefCursor, ParameterDirection.Output);

			await cmd.ExecuteNonQueryAsync(ct);

			var foundValue = pFound.Value?.ToString();
			if (string.IsNullOrWhiteSpace(foundValue) || foundValue == "null" || foundValue == "0")
				return null;

			Credit? credit = null;

			var overrideJson = ReadClobValue(pOverrideJson.Value);
			if (!string.IsNullOrWhiteSpace(overrideJson))
			{
				credit = JsonSerializer.Deserialize<Credit>(overrideJson, CreditJsonOptions);
			}

			if (credit == null)
			{
				var graph = await CreditCursorMapper.ReadAsync(cmd);
				credit = graph.Credits.FirstOrDefault(c => c.Id == creditId) ?? graph.Credits.FirstOrDefault();
			}

			if (credit == null)
				return null;

			credit.Id = creditId;

			var errors = await GetCreditErrorsAsync(creditId, ct);

			return new CreditDetailDto
			{
				CreditId = creditId,
				BatchId = batchId,
				Status = ToInt32OrDefault(pStatus.Value),
				StatusName = ToStringOrEmpty(pStatusName.Value),
				NumDog = ToNullableString(pNumDog.Value),
				AccountNo = ToNullableString(pAccountNo.Value) ?? credit.AccountNo,
				Credit = credit,
				Errors = errors
			};
		}

		public async Task<(bool Updated, string Message)> UpdateCreditDataAsync(long batchId, long creditId, Credit credit, CancellationToken ct)
		{
			credit.Id = creditId;

			var creditJson = JsonSerializer.Serialize(credit, CreditJsonOptions);

			await using var conn = new OracleConnection(_connectionString);
			await conn.OpenAsync(ct);

			await using var cmd = conn.CreateCommand();
			cmd.BindByName = true;
			cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_UPDATE_CREDIT_DATA";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;
			cmd.Parameters.Add("P_CREDIT_ID", OracleDbType.Int64).Value = creditId;
			cmd.Parameters.Add("P_CREDIT_JSON", OracleDbType.Clob).Value = creditJson;

			var pUpdated = cmd.Parameters.Add("P_UPDATED", OracleDbType.Int32);
			pUpdated.Direction = ParameterDirection.Output;

			var pMessage = cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000);
			pMessage.Direction = ParameterDirection.Output;

			await cmd.ExecuteNonQueryAsync(ct);

			var updated = ToInt32OrDefault(pUpdated.Value) == 1;
			var message = ToStringOrEmpty(pMessage.Value);
			if (string.IsNullOrWhiteSpace(message))
				message = updated ? "Kredit məlumatları yeniləndi." : "Kredit yenilənmədi.";

			return (updated, message);
		}

		private static string? ReadClobValue(object? value)
		{
			if (value == null || value == DBNull.Value)
				return null;

			if (value is OracleClob clob)
			{
				if (clob.IsNull || clob.Length == 0)
					return null;
				return clob.Value;
			}

			var text = value.ToString();
			return string.IsNullOrWhiteSpace(text) || text == "null" ? null : text;
		}

		private static int ToInt32OrDefault(object? value)
		{
			if (value == null || value == DBNull.Value)
				return 0;
			var text = value.ToString();
			return int.TryParse(text, out var n) ? n : 0;
		}

		private static string ToStringOrEmpty(object? value)
		{
			if (value == null || value == DBNull.Value)
				return string.Empty;
			var text = value.ToString();
			return string.IsNullOrWhiteSpace(text) || text == "null" ? string.Empty : text;
		}

		private static string? ToNullableString(object? value)
		{
			var text = ToStringOrEmpty(value);
			return string.IsNullOrWhiteSpace(text) ? null : text;
		}
	}
}
