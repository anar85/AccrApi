using AccrServicesApi.Models.JobModels;
using AccrServicesApi.Models.ViewModels;

namespace AccrServicesApi.Services.JobService
{
	public interface IJobService
	{		
		Task<JobCreateResponse> CreateJobAsync(JobCreateRequest req, CancellationToken ct);
		Task<AccrJobModel> GetNextJobAsync(CancellationToken ct);
		Task MarkJobAsCompletedAsync(long jobId);
		Task SetBatchFailedAsync(long batchId, string errorMessage, CancellationToken ct);
		Task BuildJobDataAsync(long jobId, int reportType);
		Task<(long BatchId, List<Credit> Credits, List<Borrower> Borrowers, List<Collateral> Collaterals, List<Guarantee> Guarantees)> GetBatchDataAsync(long jobId);
		Task UpdateBatchRequestIdAsync(long batchId, string requestIdrequestId, int status);
		Task UpdateBatchStatusByIdAsync(long batchId, int status);
        //Task<(long BatchId, string JsonPayload)> GetNextBatchPayloadAsync(long jobId);
        Task SaveBatchValidationErrorsAsync(long batchId, IReadOnlyList<BatchCreditRef> creditsInOrder, string? apiResponse, CancellationToken ct);
        Task<(long BatchId, string JsonPayload, List<BatchCreditRef> CreditsInOrder)> GetNextBatchPayloadAsync(long jobId);
    }
}
