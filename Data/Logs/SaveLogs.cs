using AccrServicesApi.Models.Settings;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IO.Compression;

namespace AccrServicesApi.Data.Logs
{
    public class SaveLogs : ISaveLogs
    {
        private static readonly object ArchiveLock = new();
        private static DateOnly? _lastArchiveDate;
        private readonly IConfiguration _config;
        private readonly string _payloadLogPath;
        private readonly int _retainDays;

        public SaveLogs(IConfiguration config)
        {
            _config = config;
            var configuredPath = config["Logging:PayloadLogPath"];
            if (string.IsNullOrWhiteSpace(configuredPath))
                _payloadLogPath = Path.Combine(AppContext.BaseDirectory, "log", "payloads");
            else if (Path.IsPathRooted(configuredPath))
                _payloadLogPath = configuredPath;
            else
                _payloadLogPath = Path.Combine(AppContext.BaseDirectory, configuredPath);

            _retainDays = Math.Max(1, config.GetValue("Logging:PayloadLogRetainDays", 7));
        }

        public async Task SaveLogAsync(LogEntry logEntry)
        {
            if (!string.IsNullOrEmpty(logEntry.Body) && IsExternalPayload(logEntry.Type))
            {
                try
                {
                    await WriteBodyToFileAsync(logEntry);
                    TryArchiveOldPayloads();
                }
                catch
                {
                   
                }
            }

            var sql = "MKR.PKG_ACCR_SERVICE.SAVE_REQUEST_LOG";

            using (var con = new OracleConnection(_config["ConnectionStrings:DBConnection"]))
            {
                await con.OpenAsync();

                using (var cmd = new OracleCommand(sql, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter("p_HttpMethod", OracleDbType.Varchar2) { Value = logEntry.HttpMethod });
                    cmd.Parameters.Add(new OracleParameter("p_Url", OracleDbType.Varchar2) { Value = logEntry.Url });
                    cmd.Parameters.Add(new OracleParameter("p_Headers", OracleDbType.Varchar2) { Value = logEntry.Headers });
                    cmd.Parameters.Add(new OracleParameter("p_Body", OracleDbType.Clob) { Value = logEntry.Body });
                    cmd.Parameters.Add(new OracleParameter("p_Type", OracleDbType.Varchar2) { Value = logEntry.Type });
                    cmd.Parameters.Add(new OracleParameter("p_StatusCode", OracleDbType.Int32) { Value = logEntry.StatusCode });
                    cmd.Parameters.Add(new OracleParameter("p_InsertDate", OracleDbType.Date) { Value = logEntry.InsertDate });
                    cmd.Parameters.Add(new OracleParameter("p_LogGuid", OracleDbType.Varchar2) { Value = logEntry.LogGuid });
                    cmd.Parameters.Add(new OracleParameter("p_IpAdress", OracleDbType.Varchar2) { Value = logEntry.IpAdress });
                    cmd.Parameters.Add(new OracleParameter("p_Batch_Id", OracleDbType.Long) { Value = logEntry.BatchId });
                    
                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }
        private static bool IsExternalPayload(string? type) =>
            string.Equals(type, "ExternalRequest", StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, "ExternalResponse", StringComparison.OrdinalIgnoreCase);
        private async Task<string> WriteBodyToFileAsync(LogEntry logEntry)
        {
            var dayFolder = Path.Combine(_payloadLogPath, DateTime.Now.ToString("yyyyMMdd"));
            Directory.CreateDirectory(dayFolder);

            var safeType = string.IsNullOrWhiteSpace(logEntry.Type) ? "Unknown" : logEntry.Type;
            var guidPart = string.IsNullOrWhiteSpace(logEntry.LogGuid)
                ? Guid.NewGuid().ToString("N")
                : logEntry.LogGuid.Replace("-", "");
            var batchPart = logEntry.BatchId > 0 ? $"batch{logEntry.BatchId}_" : string.Empty;
            var fileName = $"{batchPart}{safeType}_{guidPart}_{DateTime.Now:HHmmssfff}.json";
            var fullPath = Path.Combine(dayFolder, fileName);

            await File.WriteAllTextAsync(fullPath, logEntry.Body);
            return fullPath;
        }

        private void TryArchiveOldPayloads()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            lock (ArchiveLock)
            {
                if (_lastArchiveDate == today)
                    return;
                _lastArchiveDate = today;
            }

            try
            {
                if (!Directory.Exists(_payloadLogPath))
                    return;

                var todayFolderName = today.ToString("yyyyMMdd");
                var cutoff = today.AddDays(-_retainDays);

                foreach (var dir in Directory.GetDirectories(_payloadLogPath))
                {
                    var name = Path.GetFileName(dir);
                    if (name == todayFolderName)
                        continue;

                    if (!DateOnly.TryParseExact(name, "yyyyMMdd", out var folderDate))
                        continue;

                    if (folderDate < cutoff)
                    {
                        Directory.Delete(dir, recursive: true);
                        continue;
                    }

                    var zipPath = Path.Combine(_payloadLogPath, $"{name}.zip");
                    if (!File.Exists(zipPath))
                        ZipFile.CreateFromDirectory(dir, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);

                    Directory.Delete(dir, recursive: true);
                }

                foreach (var zip in Directory.GetFiles(_payloadLogPath, "????????.zip"))
                {
                    var name = Path.GetFileNameWithoutExtension(zip);
                    if (!DateOnly.TryParseExact(name, "yyyyMMdd", out var zipDate))
                        continue;

                    if (zipDate < cutoff)
                        File.Delete(zip);
                }
            }
            catch
            {
                // Archive uğursuz olsa log yazılması dayandırılmasın
            }
        }
    }
}
