namespace AccrServicesApi.Models.ViewModels
{
    public class BatchPayloadLogDto
    {
        public long BatchId { get; set; }
        public long? JobId { get; set; }
        public List<PayloadLogFileDto> Files { get; set; } = new();
        public PayloadLogFileDto? LatestRequest { get; set; }
        public PayloadLogFileDto? LatestResponse { get; set; }
    }

    public class PayloadLogFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // ExternalRequest | ExternalResponse
        public string Day { get; set; } = string.Empty; // yyyyMMdd
        public DateTime CreatedAt { get; set; }
        public long SizeBytes { get; set; }
        public bool FromArchive { get; set; }
        public string? Body { get; set; }
    }

    public class JobPayloadLogsDto
    {
        public long JobId { get; set; }
        public List<BatchPayloadLogDto> Batches { get; set; } = new();
    }
}
