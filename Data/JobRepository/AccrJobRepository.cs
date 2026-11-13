using AccrServicesApi.Models.JobModels;
using AccrServicesApi.Models.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Globalization;
using System.Runtime;

namespace AccrServicesApi.Data.JobRepository
{
    public partial class AccrJobRepository : IAccrJobRepository
    {
        private readonly string _connectionString;

        public AccrJobRepository(IConfiguration cfg)
        {
            _connectionString = cfg.GetConnectionString("DBConnection")
                      ?? throw new InvalidOperationException("Missing ConnectionStrings:Oracle");
        }

        public async Task<LoginResponseDto> AuthenticateUserAsync(LoginRequestDto request)
        {
            var response = new LoginResponseDto();

            using (var connection = new OracleConnection(_connectionString))
            {
                using (var command = new OracleCommand("PKG_CRYPTO.GET_PASSWORD", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("P_USERNAME", OracleDbType.Varchar2).Value = request.Username;
                    command.Parameters.Add("P_PASSWORD", OracleDbType.Varchar2).Value = request.Password;

                    command.Parameters.Add("O_ORA_USER", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
                    command.Parameters.Add("O_ORA_PASS", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
                    command.Parameters.Add("O_ERROR_CODE", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                    command.Parameters.Add("O_ERROR_MESSAGE", OracleDbType.Varchar2, 500).Direction = ParameterDirection.Output;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    response.ErrorCode = Convert.ToInt32(command.Parameters["O_ERROR_CODE"].Value.ToString());
                    response.ErrorMessage = command.Parameters["O_ERROR_MESSAGE"].Value.ToString();

                    if (response.ErrorCode == 0)
                    {
                        response.IsSuccess = true;
                        response.OraUser = command.Parameters["O_ORA_USER"].Value.ToString();
                        response.OraPass = command.Parameters["O_ORA_PASS"].Value.ToString();
                    }
                    else
                    {
                        response.IsSuccess = false;
                    }
                }
            }

            return response;
        }

        public async Task<bool> CheckUserAccessAsync(string username)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                using (var command = new OracleCommand("MKR.PKG_ACCR_SERVICE.CHK_USER_ACCESS", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("P_USERNAME", OracleDbType.Varchar2).Value = username;

                    command.Parameters.Add("O_HAS_ACCESS", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    int hasAccess = Convert.ToInt32(command.Parameters["O_HAS_ACCESS"].Value.ToString());

                    return hasAccess == 1;
                }
            }
        }

        public async Task<long> CreateJobAsync(DateTime dateFrom, DateTime dateTo, int reportType, IEnumerable<string> contractList, CancellationToken ct)
        {
            try
            {
                await using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync(ct);

                await using var cmd = conn.CreateCommand();
                cmd.BindByName = true;
                cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_CREATE_JOB";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("P_DATE_FROM", OracleDbType.Date).Value = dateFrom;
                cmd.Parameters.Add("P_DATE_TO", OracleDbType.Date).Value = dateTo;
                cmd.Parameters.Add("P_REPORT_TYPE", OracleDbType.Int32).Value = reportType;
                string contractCsv = null;
                if (contractList != null && contractList.Any())
                {
                    contractCsv = string.Join(",", contractList);
                }

                cmd.Parameters.Add("P_CONTRACT_LIST", OracleDbType.Clob).Value = string.IsNullOrEmpty(contractCsv) ? DBNull.Value : (object)contractCsv;
                var outJobId = new OracleParameter("P_JOB_ID", OracleDbType.Int64)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outJobId);

                await cmd.ExecuteNonQueryAsync(ct);

                var jobId = outJobId.Value?.ToString();
                if (string.IsNullOrWhiteSpace(jobId))
                    throw new InvalidOperationException("PROC_CREATE_JOB returned empty P_JOB_ID.");

                return Convert.ToInt64(jobId);
            }
            catch (OracleException ex)
            {
                var msg =
                    $"Oracle error in PROC_CREATE_JOB. " +
                    $"Code={ex.Number}, Message={ex.Message}. " +
                    $"DataSource={_connectionString}";

                throw;
            }
            catch (Exception ex)
            {
                var msg =
                    $"Unexpected error in CreateJobAsync. " +
                    $"Message={ex.Message}";

                Console.Error.WriteLine(msg);
                throw;
            }
        }

        public async Task<AccrJobModel> GetNextJobAsync(CancellationToken ct)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync(ct);

                using (var cmd = new OracleCommand("MKR.PKG_ACCR_SERVICE.PROC_GET_NEXT_JOB", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_JOB_ID", OracleDbType.Decimal, ParameterDirection.Output);
                    cmd.Parameters.Add("P_DATE_FROM", OracleDbType.Date, ParameterDirection.Output);
                    cmd.Parameters.Add("P_DATE_TO", OracleDbType.Date, ParameterDirection.Output);
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Decimal, ParameterDirection.Output);
                    cmd.Parameters.Add("P_REPORT_TYPE", OracleDbType.Int32, ParameterDirection.Output);

                    await cmd.ExecuteNonQueryAsync(ct);

                    if (cmd.Parameters["P_JOB_ID"].Value == DBNull.Value ||
                        cmd.Parameters["P_JOB_ID"].Value.ToString() == "null")
                    {
                        return null;
                    }

                    return new AccrJobModel
                    {
                        Id = Convert.ToInt64(cmd.Parameters["P_JOB_ID"].Value.ToString()),
                        DateFrom = Convert.ToDateTime(cmd.Parameters["P_DATE_FROM"].Value.ToString()),
                        DateTo = Convert.ToDateTime(cmd.Parameters["P_DATE_TO"].Value.ToString()),
                        Status = Convert.ToInt32(cmd.Parameters["P_STATUS"].Value.ToString()),
                        ReportType = Convert.ToInt32(cmd.Parameters["P_REPORT_TYPE"].Value.ToString())
                    };
                }
            }
        }

        public async Task MarkJobAsCompletedAsync(long jobId)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new OracleCommand("MKR.PKG_ACCR_SERVICE.PROC_MARK_JOB_AS_COMPLETED", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("P_JOB_ID", OracleDbType.Decimal, jobId, ParameterDirection.Input);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SetBatchFailedAsync(long batchId, string errorMessage, CancellationToken ct)
        {
            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_SET_BATCH_FAILED";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;

            string safeErrorMsg = string.IsNullOrWhiteSpace(errorMessage)
                                    ? "Naməlum API Xətası"
                                    : (errorMessage.Length > 3900 ? errorMessage.Substring(0, 3900) : errorMessage);

            cmd.Parameters.Add("P_ERROR_MSG", OracleDbType.Varchar2).Value = safeErrorMsg;

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task LogValidationErrorAsync(long batchId, long creditId, string? accountNo, string errorType, string? documentNo, string errorCode, string errorMessage, CancellationToken ct)
        {
            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_LOG_VALIDATION_ERROR";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;
            cmd.Parameters.Add("P_CREDIT_ID", OracleDbType.Int64).Value = creditId;
            cmd.Parameters.Add("P_ACCOUNT_NO", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(accountNo) ? (object)DBNull.Value : accountNo;
            cmd.Parameters.Add("P_ERROR_TYPE", OracleDbType.Varchar2).Value = TruncateOrDefault(errorType, "Validation", 100);
            cmd.Parameters.Add("P_DOCUMENT_NO", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(documentNo) ? (object)DBNull.Value : documentNo;
            cmd.Parameters.Add("P_ERROR_CODE", OracleDbType.Varchar2).Value = TruncateOrDefault(errorCode, "Validation", 500);
            cmd.Parameters.Add("P_ERROR_MESSAGE", OracleDbType.Varchar2).Value = TruncateOrDefault(errorMessage, "Naməlum validation xətası", 3900);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task LogValidationErrorsBulkAsync(long batchId, List<(long CreditId, string? AccountNo, string ErrorCode, string ErrorMessage)> errors, CancellationToken ct)
        {
            if (errors == null || errors.Count == 0) return;

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_LOG_VALIDATION_ERROR";
            cmd.CommandType = CommandType.StoredProcedure;

            // Parametrləri hazırlayırıq
            var pBatchId = cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64);
            var pCreditId = cmd.Parameters.Add("P_CREDIT_ID", OracleDbType.Int64);
            var pAccountNo = cmd.Parameters.Add("P_ACCOUNT_NO", OracleDbType.Varchar2);
            var pErrorType = cmd.Parameters.Add("P_ERROR_TYPE", OracleDbType.Varchar2);
            var pDocumentNo = cmd.Parameters.Add("P_DOCUMENT_NO", OracleDbType.Varchar2);
            var pErrorCode = cmd.Parameters.Add("P_ERROR_CODE", OracleDbType.Varchar2);
            var pErrorMessage = cmd.Parameters.Add("P_ERROR_MESSAGE", OracleDbType.Varchar2);

            foreach (var err in errors)
            {
                pBatchId.Value = batchId;
                pCreditId.Value = err.CreditId;
                pAccountNo.Value = string.IsNullOrWhiteSpace(err.AccountNo) ? (object)DBNull.Value : err.AccountNo;
                pErrorType.Value = "Validation";
                pDocumentNo.Value = DBNull.Value;
                pErrorCode.Value = TruncateOrDefault(err.ErrorCode, "Validation", 500);
                pErrorMessage.Value = TruncateOrDefault(err.ErrorMessage, "Naməlum validation xətası", 3900);

                await cmd.ExecuteNonQueryAsync(ct);
            }
        }

        private static string TruncateOrDefault(string? value, string fallback, int maxLength)
        {
            var text = string.IsNullOrWhiteSpace(value) ? fallback : value;
            return text.Length > maxLength ? text.Substring(0, maxLength) : text;
        }

        public async Task BuildJobDataAsync(long jobId, int reportType)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();
                string procedureName = reportType == 2
                        ? "MKR.PKG_ACCR_SERVICE.PROC_BUILD_JSON_DATA_MONTHLY"
                        : "MKR.PKG_ACCR_SERVICE.PROC_BUILD_JSON_DATA";

                using (var cmd = new OracleCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;

                    cmd.Parameters.Add("P_JOB_ID", OracleDbType.Decimal, jobId, ParameterDirection.Input);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<(long BatchId, List<Credit> Credits, List<Borrower> Borrowers, List<Collateral> Collaterals, List<Guarantee> Guarantees)> GetBatchDataAsync(long jobId)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand("MKR.PKG_ACCR_SERVICE.PROC_GET_BATCH_DATA", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parametrlər
                    cmd.Parameters.Add("P_JOB_ID", OracleDbType.Decimal, jobId, ParameterDirection.Input);
                    cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Decimal, ParameterDirection.Output);
                    cmd.Parameters.Add("P_CREDITS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
                    cmd.Parameters.Add("P_BORROWERS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
                    cmd.Parameters.Add("P_COLLATERALS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
                    cmd.Parameters.Add("P_GUARANTEES_CUR", OracleDbType.RefCursor, ParameterDirection.Output);

                    // YENİ KURSORLAR (Array-lər üçün)
                    cmd.Parameters.Add("P_RELATIONS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
                    cmd.Parameters.Add("P_SHAREHOLDERS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
                    cmd.Parameters.Add("P_FINANCE_REPORTS_CUR", OracleDbType.RefCursor, ParameterDirection.Output);
                    cmd.Parameters.Add("P_FOREIGN_ECO_CUR", OracleDbType.RefCursor, ParameterDirection.Output);

                    await cmd.ExecuteNonQueryAsync();

                    // 1. Batch ID-ni oxuyuruq
                    var batchIdParam = cmd.Parameters["P_BATCH_ID"].Value;
                    if (batchIdParam == DBNull.Value || batchIdParam.ToString() == "null" || batchIdParam.ToString() == "0")
                        return (0, new List<Credit>(), new List<Borrower>(), new List<Collateral>(), new List<Guarantee>());

                    var batchId = Convert.ToInt64(batchIdParam.ToString());
                    // Eyni kursor mapping məntiqi — CreditCursorMapper.cs (köhnə GetBatchDataAsync oxuma bloku)
                    var graph = await CreditCursorMapper.ReadAsync(cmd);
                    return (batchId, graph.Credits, graph.Borrowers, graph.Collaterals, graph.Guarantees);
                }
            }
        }


        public async Task UpdateBatchSetRequestidAsync(long batchId, string requestId, int status)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand("MKR.PKG_ACCR_SERVICE.PROC_SET_BATCH_REQUEST", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Long, batchId, ParameterDirection.Input);
                    cmd.Parameters.Add("P_REQUEST_ID", OracleDbType.Varchar2, requestId, ParameterDirection.Input);
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Decimal, status, ParameterDirection.Input);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateBatchStatusByIdAsync(long batchId, int status)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand("MKR.PKG_ACCR_SERVICE.PROC_UPDATE_BATCH_STATUS_BY_ID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Decimal, batchId, ParameterDirection.Input);
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Decimal, status, ParameterDirection.Input);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }



        //UI

        public async Task<List<JobListItemDto>> GetJobsListAsync(long? jobId, DateTime? dateFrom, DateTime? dateTo, int? status, CancellationToken ct)
        {
            var result = new List<JobListItemDto>();

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_GET_JOBS_LIST";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_JOB_ID", OracleDbType.Int64).Value = (object)jobId ?? DBNull.Value;
            cmd.Parameters.Add("P_DATE_FROM", OracleDbType.Date).Value = (object)dateFrom ?? DBNull.Value;
            cmd.Parameters.Add("P_DATE_TO", OracleDbType.Date).Value = (object)dateTo ?? DBNull.Value;
            cmd.Parameters.Add("P_STATUS", OracleDbType.Int32).Value = (object)status ?? DBNull.Value;

            var outCursor = new OracleParameter("P_JOBS_CUR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outCursor);

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(new JobListItemDto
                {
                    JobId = reader["JOB_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["JOB_ID"]),
                    ReportDateFrom = reader["REPORT_DATE_FROM"] == DBNull.Value ? null : Convert.ToDateTime(reader["REPORT_DATE_FROM"]),
                    ReportDateTo = reader["REPORT_DATE_TO"] == DBNull.Value ? null : Convert.ToDateTime(reader["REPORT_DATE_TO"]),
                    ReportType = reader["REPORT_TYPE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["REPORT_TYPE"]),
                    ReportTypeName = reader["REPORT_TYPE_NAME"] == DBNull.Value ? string.Empty : reader["REPORT_TYPE_NAME"]?.ToString(),
                    Status = reader["STATUS"] == DBNull.Value ? 0 : Convert.ToInt32(reader["STATUS"]),
                    StatusName = reader["STATUS_NAME"] == DBNull.Value ? string.Empty : reader["STATUS_NAME"]?.ToString(),
                    TotalCount = reader["TOTAL_COUNT"] == DBNull.Value ? null : Convert.ToInt32(reader["TOTAL_COUNT"]),
                    CreatedAt = reader["CREATED_AT"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CREATED_AT"]),
                    UpdatedAt = reader["UPDATED_AT"] == DBNull.Value ? null : Convert.ToDateTime(reader["UPDATED_AT"]),
                    LastError = reader["LAST_ERROR"] == DBNull.Value ? null : reader["LAST_ERROR"]?.ToString()
                });
            }

            return result;
        }

        public async Task<List<BatchListItemDto>> GetBatchesAsync(long? jobId, DateOnly? dateFrom, DateOnly? dateTo, int? status, CancellationToken ct)
        {
            var result = new List<BatchListItemDto>();

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_GET_JOB_BATCHES";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_JOB_ID", OracleDbType.Int64).Value = jobId.HasValue ? jobId.Value : DBNull.Value;
            cmd.Parameters.Add("P_DATE_FROM", OracleDbType.Date).Value = dateFrom.HasValue ? dateFrom.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
            cmd.Parameters.Add("P_DATE_TO", OracleDbType.Date).Value = dateTo.HasValue ? dateTo.Value.ToDateTime(TimeOnly.MaxValue) : DBNull.Value;
            cmd.Parameters.Add("P_STATUS", OracleDbType.Int32).Value = status.HasValue ? status.Value : DBNull.Value;

            var cursor = new OracleParameter("P_BATCHES_CUR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(cursor);

            await cmd.ExecuteNonQueryAsync(ct);

            await using var reader = ((OracleRefCursor)cursor.Value).GetDataReader();
            while (await reader.ReadAsync(ct))
            {
                result.Add(new BatchListItemDto
                {
                    BatchId = Convert.ToInt64(reader["BATCH_ID"]),
                    JobId = Convert.ToInt64(reader["JOB_ID"]),
                    CreatedAt = Convert.ToDateTime(reader["CREATED_AT"]),
                    Status = Convert.ToInt32(reader["STATUS"]),
                    StatusName = reader["STATUS_NAME"] == DBNull.Value ? string.Empty : reader["STATUS_NAME"]?.ToString(),
                    RequestId = reader["REQUEST_ID"] == DBNull.Value ? string.Empty : reader["REQUEST_ID"]?.ToString(),
                    TotalCreditCount = Convert.ToInt32(reader["TOTAL_CREDIT_COUNT"]),
                    FailedCreditCount = Convert.ToInt32(reader["FAILED_CREDIT_COUNT"]),
                    CallbackStatus = reader["CALLBACK_STATUS"] == DBNull.Value ? string.Empty : reader["CALLBACK_STATUS"]?.ToString(),
                    CallbackDate = reader["CALLBACK_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["CALLBACK_DATE"])
                });
            }

            return result;
        }

        public async Task<List<CreditListItemDto>> GetBatchCreditsAsync(long batchId, int? status, CancellationToken ct)
        {
            var result = new List<CreditListItemDto>();

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_GET_BATCH_CREDITS";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;
            cmd.Parameters.Add("P_STATUS", OracleDbType.Int32).Value = status.HasValue ? status.Value : DBNull.Value;

            var cursor = new OracleParameter("P_CREDITS_CUR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(cursor);

            await cmd.ExecuteNonQueryAsync(ct);

            await using var reader = ((OracleRefCursor)cursor.Value).GetDataReader();
            while (await reader.ReadAsync(ct))
            {
                result.Add(new CreditListItemDto
                {
                    CreditId = Convert.ToInt64(reader["CREDIT_ID"]),
                    BatchId = Convert.ToInt64(reader["BATCH_ID"]),
                    NumDog = reader["NUM_DOG"]?.ToString(),
                    AccountNo = reader["ACCOUNT_NO"]?.ToString(),
                    Status = Convert.ToInt32(reader["STATUS"]),
                    StatusName = reader["STATUS_NAME"] == DBNull.Value ? string.Empty : reader["STATUS_NAME"]?.ToString(),
                    ErrorMessage = reader["ERROR_MESSAGE"] == DBNull.Value ? null : reader["ERROR_MESSAGE"]?.ToString(),
                    RetryCount = reader["RETRY_COUNT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RETRY_COUNT"]),
                    LastRetryAt = reader["LAST_RETRY_AT"] == DBNull.Value ? null : Convert.ToDateTime(reader["LAST_RETRY_AT"])
                });
            }

            return result;
        }

        public async Task<List<CreditApiErrorDto>> GetCreditErrorsAsync(long creditId, CancellationToken ct)
        {
            var result = new List<CreditApiErrorDto>();

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_GET_CREDIT_ERRORS";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_CREDIT_ID", OracleDbType.Int64).Value = creditId;

            var outCursor = new OracleParameter("P_ERRORS_CUR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outCursor);

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(new CreditApiErrorDto
                {
                    ErrorId = reader["ERROR_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ERROR_ID"]),
                    BatchId = reader["BATCH_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["BATCH_ID"]),
                    CreditId = reader["CREDIT_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CREDIT_ID"]),
                    NumDog = reader["NUM_DOG"] == DBNull.Value ? string.Empty : reader["NUM_DOG"]?.ToString(),
                    ErrorType = reader["ERROR_TYPE"] == DBNull.Value ? string.Empty : reader["ERROR_TYPE"]?.ToString(),
                    DocumentNo = reader["DOCUMENT_NO"] == DBNull.Value ? string.Empty : reader["DOCUMENT_NO"]?.ToString(),
                    ErrorCode = reader["ERROR_CODE"] == DBNull.Value ? string.Empty : reader["ERROR_CODE"]?.ToString(),
                    ErrorMessage = reader["ERROR_MESSAGE"] == DBNull.Value ? string.Empty : reader["ERROR_MESSAGE"]?.ToString(),
                    CreatedAt = reader["CREATED_AT"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CREATED_AT"])
                });
            }

            return result;
        }
        public async Task<int> MarkFailedCreditsForRetryAsync(long batchId, CancellationToken ct)
        {
            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_RETRY_FAILED_CREDITS";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;
            var affected = new OracleParameter("P_AFFECTED_COUNT", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(affected);

            await cmd.ExecuteNonQueryAsync(ct);
            return Convert.ToInt32(affected.Value?.ToString() ?? "0");
        }

        public async Task<int> MarkSelectedCreditsForRetryAsync(long batchId, List<long> creditIds, CancellationToken ct)
        {
            if (creditIds.Count == 0)
                return 0;

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "MKR.PKG_ACCR_SERVICE.PROC_RETRY_SELECTED_CREDITS";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Int64).Value = batchId;
            cmd.Parameters.Add("P_CREDIT_IDS_CSV", OracleDbType.Varchar2).Value = string.Join(",", creditIds.Distinct());

            var affected = new OracleParameter("P_AFFECTED_COUNT", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(affected);

            await cmd.ExecuteNonQueryAsync(ct);
            return Convert.ToInt32(affected.Value?.ToString() ?? "0");
        }

        public async Task<ExportBatchKeys> GetNextExportKeysAsync(long jobId, DateOnly dateFrom, DateOnly dateTo, string lastAccountNo, CancellationToken ct)
        {
            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = @"
								BEGIN
								  MKR.PKG_ACCR_SERVICE.PROC_GET_ACCR_EXPORT_DATA(
									P_JOB_ID               => :P_JOB_ID,
									P_DATE_FROM            => TO_DATE(:P_DATE_FROM, 'DD.MM.YYYY'),
									P_DATE_TO              => TO_DATE(:P_DATE_TO,   'DD.MM.YYYY') + (86399/86400), -- 23:59:59
									P_LAST_ACCOUNT_NO      => :P_LAST_ACCOUNT_NO,
									P_LIMIT                => :P_LIMIT,
									P_NEXT_LAST_ACCOUNT_NO => :P_NEXT_LAST_ACCOUNT_NO,
									P_HAS_MORE             => :P_HAS_MORE,
									P_CREDITS              => :P_CREDITS,
									P_COLLATERALS          => :P_COLLATERALS,
									P_GUARANTEES           => :P_GUARANTEES
								  );
								END;";

            // DATE string-lər (saatsız)
            var fromStr = dateFrom.ToString("dd.MM.yyyy");
            var toStr = dateTo.ToString("dd.MM.yyyy");

            cmd.Parameters.Add("P_JOB_ID", OracleDbType.Int64).Value = jobId;

            cmd.Parameters.Add("P_DATE_FROM", OracleDbType.Varchar2, 10).Value = fromStr;
            cmd.Parameters.Add("P_DATE_TO", OracleDbType.Varchar2, 10).Value = toStr;

            var last = string.IsNullOrWhiteSpace(lastAccountNo) ? null : lastAccountNo.Trim();
            cmd.Parameters.Add("P_LAST_ACCOUNT_NO", OracleDbType.Varchar2, 50).Value =
                (object?)last ?? DBNull.Value;

            cmd.Parameters.Add("P_LIMIT", OracleDbType.Int32).Value = 2000;

            var pNext = new OracleParameter("P_NEXT_LAST_ACCOUNT_NO", OracleDbType.Varchar2, 200)
            { Direction = ParameterDirection.Output };
            var pHasMore = new OracleParameter("P_HAS_MORE", OracleDbType.Int32)
            { Direction = ParameterDirection.Output };

            var pCredits = new OracleParameter("P_CREDITS", OracleDbType.RefCursor)
            { Direction = ParameterDirection.Output };
            var pColl = new OracleParameter("P_COLLATERALS", OracleDbType.RefCursor)
            { Direction = ParameterDirection.Output };
            var pGuar = new OracleParameter("P_GUARANTEES", OracleDbType.RefCursor)
            { Direction = ParameterDirection.Output };

            cmd.Parameters.Add(pNext);
            cmd.Parameters.Add(pHasMore);
            cmd.Parameters.Add(pCredits);
            cmd.Parameters.Add(pColl);
            cmd.Parameters.Add(pGuar);

            await cmd.ExecuteNonQueryAsync(ct);

            // Debug üçün
            Console.WriteLine($"[REPO] jobId={jobId}, fromStr={fromStr}, toStr={toStr}, next={pNext.Value}, hasMore={pHasMore.Value}");

            var result = new ExportBatchKeys
            {
                NextLastAccountNo = pNext.Value == null || pNext.Value == DBNull.Value ? null : pNext.Value.ToString(),
                HasMore = (pHasMore.Value?.ToString() ?? "0") == "1"
            };

            using var rdr = ((OracleRefCursor)pCredits.Value).GetDataReader();
            while (await rdr.ReadAsync(ct))
            {
                var acc = rdr["accountNo"]?.ToString();
                if (!string.IsNullOrWhiteSpace(acc))
                    result.AccountNos.Add(acc);
            }

            return result;
        }



    }
}
