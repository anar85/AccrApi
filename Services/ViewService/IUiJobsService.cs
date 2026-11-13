using AccrServicesApi.Models.AccrServiceModels;
using AccrServicesApi.Models.JobModels;
using AccrServicesApi.Models.ViewModels;

namespace AccrServicesApi.Services.ViewService
{
	public interface IUiJobsService
	{
		Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
		Task<JobCreateResponse> CreateJobAsync(JobCreateRequest req, CancellationToken ct);
		Task<List<JobListItemDto>> GetJobsListAsync(long? jobId, DateOnly? dateFrom, DateOnly? dateTo, int? status, CancellationToken ct);
		Task<List<BatchListItemDto>> GetBatchesAsync(long? jobId, DateOnly? dateFrom, DateOnly? dateTo, int? status, CancellationToken ct);
		Task<List<CreditListItemDto>> GetBatchCreditsAsync(long batchId, int? status, CancellationToken ct);
		Task<CreditDetailDto?> GetCreditDetailAsync(long batchId, long creditId, CancellationToken ct);
		Task<UpdateCreditResponseDto> UpdateCreditAsync(long batchId, long creditId, UpdateCreditRequest request, CancellationToken ct);
		Task<List<CreditApiErrorDto>> GetCreditErrorsAsync(long creditId, CancellationToken ct);
		Task<RetryResultDto> RetryFailedCreditsByBatchAsync(long batchId, CancellationToken ct);
		Task<RetryResultDto> RetrySelectedCreditsAsync(long batchId, List<long> creditIds, CancellationToken ct);
		Task<SendBatchResultDto> SendNextBatchAsync(long jobId, CancellationToken ct);
		Task<SendJobResultDto> SendJobAsync(long jobId, CancellationToken ct);
		Task<CheckStatusResponseDto> CheckStatusByRequestIdAsync(string requestId, CancellationToken ct);
        Task<BatchPayloadLogDto> GetBatchPayloadLogsAsync(long batchId, bool includeBody, CancellationToken ct);
        Task<JobPayloadLogsDto> GetJobPayloadLogsAsync(long jobId, bool includeBody, CancellationToken ct);
    }
}
