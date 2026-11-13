using AccrServicesApi.Models.ViewModels;

namespace AccrServicesApi.Data.Logs
{
    public interface IPayloadLogReader
    {
        Task<BatchPayloadLogDto> GetByBatchIdAsync(long batchId, bool includeBody, CancellationToken ct);
        Task<JobPayloadLogsDto> GetByJobIdAsync(long jobId, IReadOnlyList<long> batchIds, bool includeBody, CancellationToken ct);
    }
}
