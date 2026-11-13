using AccrServicesApi.Models.ViewModels;
using System.IO.Compression;

namespace AccrServicesApi.Data.Logs
{
    public partial class PayloadLogReader : IPayloadLogReader
    {
        private readonly string _payloadLogPath;

        public PayloadLogReader(IConfiguration config)
        {
            var configuredPath = config["Logging:PayloadLogPath"];
            if (string.IsNullOrWhiteSpace(configuredPath))
                _payloadLogPath = Path.Combine(AppContext.BaseDirectory, "log", "payloads");
            else if (Path.IsPathRooted(configuredPath))
                _payloadLogPath = configuredPath;
            else
                _payloadLogPath = Path.Combine(AppContext.BaseDirectory, configuredPath);
        }

        public async Task<BatchPayloadLogDto> GetByBatchIdAsync(long batchId, bool includeBody, CancellationToken ct)
        {
            var files = await FindBatchFilesAsync(batchId, includeBody, ct);

            return new BatchPayloadLogDto
            {
                BatchId = batchId,
                Files = files,
                LatestRequest = files
                    .Where(f => f.Type.Equals("ExternalRequest", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => f.CreatedAt)
                    .FirstOrDefault(),
                LatestResponse = files
                    .Where(f => f.Type.Equals("ExternalResponse", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => f.CreatedAt)
                    .FirstOrDefault()
            };
        }

        public async Task<JobPayloadLogsDto> GetByJobIdAsync(long jobId, IReadOnlyList<long> batchIds, bool includeBody, CancellationToken ct)
        {
            var result = new JobPayloadLogsDto { JobId = jobId };

            foreach (var batchId in batchIds.Distinct().OrderBy(x => x))
            {
                var batchLog = await GetByBatchIdAsync(batchId, includeBody, ct);
                batchLog.JobId = jobId;
                result.Batches.Add(batchLog);
            }

            return result;
        }

        private async Task<List<PayloadLogFileDto>> FindBatchFilesAsync(long batchId, bool includeBody, CancellationToken ct)
        {
            var result = new List<PayloadLogFileDto>();
            if (!Directory.Exists(_payloadLogPath))
                return result;

            var prefix = $"batch{batchId}_";

            foreach (var dayDir in Directory.GetDirectories(_payloadLogPath))
            {
                ct.ThrowIfCancellationRequested();
                var day = Path.GetFileName(dayDir);
                foreach (var file in Directory.GetFiles(dayDir, $"{prefix}*.json"))
                {
                    result.Add(await ToDtoAsync(file, day, fromArchive: false, includeBody, ct));
                }
            }

            foreach (var zipPath in Directory.GetFiles(_payloadLogPath, "????????.zip"))
            {
                ct.ThrowIfCancellationRequested();
                var day = Path.GetFileNameWithoutExtension(zipPath);
                result.AddRange(await ReadFromZipAsync(zipPath, day, prefix, includeBody, ct));
            }

            return result
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Type)
                .ToList();
        }

        private static async Task<List<PayloadLogFileDto>> ReadFromZipAsync(
            string zipPath,
            string day,
            string prefix,
            bool includeBody,
            CancellationToken ct)
        {
            var list = new List<PayloadLogFileDto>();

            await using var stream = File.OpenRead(zipPath);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                ct.ThrowIfCancellationRequested();
                var name = Path.GetFileName(entry.FullName);
                if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || !name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                string? body = null;
                if (includeBody)
                {
                    await using var entryStream = entry.Open();
                    using var reader = new StreamReader(entryStream);
                    body = await reader.ReadToEndAsync(ct);
                }

                list.Add(new PayloadLogFileDto
                {
                    FileName = name,
                    Type = ParseType(name),
                    Day = day,
                    CreatedAt = entry.LastWriteTime.DateTime,
                    SizeBytes = entry.Length,
                    FromArchive = true,
                    Body = body
                });
            }

            return list;
        }

        private static async Task<PayloadLogFileDto> ToDtoAsync(
            string fullPath,
            string day,
            bool fromArchive,
            bool includeBody,
            CancellationToken ct)
        {
            var info = new FileInfo(fullPath);
            string? body = null;
            if (includeBody)
                body = await File.ReadAllTextAsync(fullPath, ct);

            return new PayloadLogFileDto
            {
                FileName = info.Name,
                Type = ParseType(info.Name),
                Day = day,
                CreatedAt = info.LastWriteTime,
                SizeBytes = info.Length,
                FromArchive = fromArchive,
                Body = body
            };
        }

        private static string ParseType(string fileName)
        {
            if (fileName.Contains("ExternalRequest", StringComparison.OrdinalIgnoreCase))
                return "ExternalRequest";
            if (fileName.Contains("ExternalResponse", StringComparison.OrdinalIgnoreCase))
                return "ExternalResponse";
            return "Unknown";
        }
    }
}
