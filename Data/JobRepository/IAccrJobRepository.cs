using AccrServicesApi.Models.JobModels;
using AccrServicesApi.Models.ViewModels;

namespace AccrServicesApi.Data.JobRepository
{
	public interface IAccrJobRepository
	{
		Task<LoginResponseDto> AuthenticateUserAsync(LoginRequestDto request);
		Task<bool> CheckUserAccessAsync(string username);
		Task<long> CreateJobAsync(DateTime dateFrom, DateTime dateTo, int reportType, IEnumerable<string> contractList, CancellationToken ct);		
		Task<ExportBatchKeys> GetNextExportKeysAsync(long jobId, DateOnly dateFrom, DateOnly dateTo, string? lastAccountNo, CancellationToken ct);
		
		//yeni
		Task<AccrJobModel> GetNextJobAsync(CancellationToken ct);
		Task MarkJobAsCompletedAsync(long jobId);
		Task SetBatchFailedAsync(long batchId, string errorMessage, CancellationToken ct);
		Task BuildJobDataAsync(long jobId, int reportType);
		Task<(long BatchId, List<Credit> Credits, List<Borrower> Borrowers, List<Collateral> Collaterals, List<Guarantee> Guarantees)> GetBatchDataAsync(long jobId);
		Task UpdateBatchSetRequestidAsync(long batchId, string requestId, int status);
		Task UpdateBatchStatusByIdAsync(long batchId, int status);

		//ui
		Task<List<JobListItemDto>> GetJobsListAsync(long? jobId, DateTime? dateFrom, DateTime? dateTo, int? status, CancellationToken ct);
		Task<List<BatchListItemDto>> GetBatchesAsync(long? jobId, DateOnly? dateFrom, DateOnly? dateTo, int? status, CancellationToken ct);
		Task<List<CreditListItemDto>> GetBatchCreditsAsync(long batchId, int? status, CancellationToken ct);
		Task<CreditDetailDto?> GetCreditDetailAsync(long batchId, long creditId, CancellationToken ct);
		Task<(bool Updated, string Message)> UpdateCreditDataAsync(long batchId, long creditId, Credit credit, CancellationToken ct);
		Task<List<CreditApiErrorDto>> GetCreditErrorsAsync(long creditId, CancellationToken ct);
		Task<int> MarkFailedCreditsForRetryAsync(long batchId, CancellationToken ct);
		Task<int> MarkSelectedCreditsForRetryAsync(long batchId, List<long> creditIds, CancellationToken ct);
        //Task LogApiErrorAsync(long batchId, string accountNo, string errorPath, string errorMessage);
        Task LogValidationErrorAsync(long batchId, long creditId, string? accountNo, string errorType, string? documentNo, string errorCode, string errorMessage, CancellationToken ct);
		Task LogValidationErrorsBulkAsync(long batchId, List<(long CreditId, string? AccountNo, string ErrorCode, string ErrorMessage)> errors, CancellationToken ct);
    }
}
