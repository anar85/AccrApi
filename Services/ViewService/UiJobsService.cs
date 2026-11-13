using AccrServicesApi.BackgroundServices;
using AccrServicesApi.Data.JobRepository;
using AccrServicesApi.Data.Logs;
using AccrServicesApi.Models.AccrServiceModels;
using AccrServicesApi.Models.JobModels;
using AccrServicesApi.Models.ViewModels;
using AccrServicesApi.Services.ApiClient;
using AccrServicesApi.Services.JobService;

namespace AccrServicesApi.Services.ViewService
{
	public class UiJobsService : IUiJobsService
	{
        private static readonly TimeSpan DispatcherActiveDuration = TimeSpan.FromHours(1);
        private readonly IAccrJobRepository _repo;
		private readonly IJobService _jobService;
		private readonly IApiClientService _apiClient;
        private readonly IExportDispatcherGate _dispatcherGate;
        private readonly IPayloadLogReader _payloadLogReader;
        private readonly ILogger<UiJobsService> _logger;

		public UiJobsService(IAccrJobRepository repo, IJobService jobService, IApiClientService apiClient, IExportDispatcherGate dispatcherGate, IPayloadLogReader payloadLogReader, ILogger<UiJobsService> logger)
		{
			_repo = repo;
			_jobService = jobService;
			_apiClient = apiClient;
			_dispatcherGate = dispatcherGate;
            _payloadLogReader = payloadLogReader;
            _logger = logger;
		}

		public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
		{
			bool hasAccess = await _repo.CheckUserAccessAsync(request.Username);

			if (!hasAccess)
			{
				return new LoginResponseDto
				{
					IsSuccess = false,
					ErrorCode = 403,
					ErrorMessage = "Sizin bu sistemə giriş hüququnuz yoxdur və ya hesabınız deaktiv edilib."
				};
			}
			var result = await _repo.AuthenticateUserAsync(request);

			return result;
		}
		public async Task<JobCreateResponse> CreateJobAsync(JobCreateRequest req, CancellationToken ct)
		{
			if (req.DateFrom.ToDateTime(TimeOnly.MinValue).Date > req.DateTo.ToDateTime(TimeOnly.MinValue).Date)
				throw new ArgumentException("DateFrom DateTo-dan böyük ola bilməz.");

			var jobId = await _repo.CreateJobAsync(
				req.DateFrom.ToDateTime(TimeOnly.MinValue),
				req.DateTo.ToDateTime(TimeOnly.MinValue),
				req.ReportType,
				req.ContractList,
				ct);
            _dispatcherGate.Activate(DispatcherActiveDuration);

            return new JobCreateResponse
			{
				JobId = jobId,
				Status = (int)BatchStatus.Pending
			};
		}

		public async Task<List<JobListItemDto>> GetJobsListAsync(long? jobId, DateOnly? dateFrom, DateOnly? dateTo, int? status, CancellationToken ct)
		{
			return await _repo.GetJobsListAsync(
				jobId,
				dateFrom?.ToDateTime(TimeOnly.MinValue),
				dateTo?.ToDateTime(TimeOnly.MaxValue),
				status,
				ct);
		}

		public async Task<List<BatchListItemDto>> GetBatchesAsync(long? jobId, DateOnly? dateFrom, DateOnly? dateTo, int? status, CancellationToken ct)
		{
			return await _repo.GetBatchesAsync(jobId, dateFrom, dateTo, status, ct);
		}

		public async Task<List<CreditListItemDto>> GetBatchCreditsAsync(long batchId, int? status, CancellationToken ct)
		{
			return await _repo.GetBatchCreditsAsync(batchId, status, ct);
		}

		public async Task<CreditDetailDto?> GetCreditDetailAsync(long batchId, long creditId, CancellationToken ct)
		{
			return await _repo.GetCreditDetailAsync(batchId, creditId, ct);
		}

		public async Task<UpdateCreditResponseDto> UpdateCreditAsync(long batchId, long creditId, UpdateCreditRequest request, CancellationToken ct)
		{
			if (request?.Credit == null)
			{
				return new UpdateCreditResponseDto
				{
					CreditId = creditId,
					BatchId = batchId,
					Updated = false,
					Message = "Credit body boş ola bilməz."
				};
			}

			var (updated, message) = await _repo.UpdateCreditDataAsync(batchId, creditId, request.Credit, ct);
			var markedForRetry = false;

			if (updated && request.MarkForRetry)
			{
				var accepted = await _repo.MarkSelectedCreditsForRetryAsync(batchId, new List<long> { creditId }, ct);
				markedForRetry = accepted > 0;
			}

			_logger.LogInformation(
				"Credit update. BatchId={BatchId}, CreditId={CreditId}, Updated={Updated}, MarkedForRetry={MarkedForRetry}",
				batchId, creditId, updated, markedForRetry);

			return new UpdateCreditResponseDto
			{
				CreditId = creditId,
				BatchId = batchId,
				Updated = updated,
				MarkedForRetry = markedForRetry,
				Message = updated
					? (markedForRetry
						? $"{message} Kredit retry növbəsinə əlavə edildi."
						: message)
					: message
			};
		}

		public async Task<List<CreditApiErrorDto>> GetCreditErrorsAsync(long creditId, CancellationToken ct)
		{
			return await _repo.GetCreditErrorsAsync(creditId, ct);
		}

		public async Task<RetryResultDto> RetryFailedCreditsByBatchAsync(long batchId, CancellationToken ct)
		{
			var accepted = await _repo.MarkFailedCreditsForRetryAsync(batchId, ct);
			_logger.LogInformation("Retry failed credits requested for BatchId: {BatchId}. Accepted: {Accepted}", batchId, accepted);

			return new RetryResultDto
			{
				RequestedCount = accepted,
				AcceptedCount = accepted,
				SkippedCount = 0,
				FailedCount = 0,
				Message = accepted == 0
					? "Retry üçün failed credit tapılmadı."
					: "Failed creditlər retry növbəsinə əlavə edildi."
			};
		}

		public async Task<RetryResultDto> RetrySelectedCreditsAsync(long batchId, List<long> creditIds, CancellationToken ct)
		{
			var requested = creditIds?.Count ?? 0;
			var accepted = await _repo.MarkSelectedCreditsForRetryAsync(batchId, creditIds ?? new List<long>(), ct);
			var skipped = Math.Max(0, requested - accepted);
			_logger.LogInformation(
				"Retry selected credits requested for BatchId: {BatchId}. Requested: {Requested}, Accepted: {Accepted}",
				batchId, requested, accepted);

			return new RetryResultDto
			{
				RequestedCount = requested,
				AcceptedCount = accepted,
				SkippedCount = skipped,
				FailedCount = 0,
				Message = accepted == 0
					? "Seçilmiş creditlər üçün retry qeyd olunmadı."
					: "Seçilmiş creditlər retry növbəsinə əlavə edildi."
			};
		}

		public async Task<SendBatchResultDto> SendNextBatchAsync(long jobId, CancellationToken ct)
		{
			var payload = await _jobService.GetNextBatchPayloadAsync(jobId);

			if (payload.BatchId == 0)
			{
				return new SendBatchResultDto
				{
					BatchId = 0,
					IsSent = false,
					Message = "Göndəriləcək paket qalmayıb."
				};
			}

			//bool isSent = await _apiClient.SendToApiAsync(payload.BatchId, payload.JsonPayload);
			var sendResult = await _apiClient.SendToApiAsync(payload.BatchId, payload.JsonPayload);

			//if (isSent)
			if (sendResult.IsSent)
				_logger.LogInformation("Job {JobId}, Batch {BatchId} uğurla göndərildi.", jobId, payload.BatchId);
			else
			{
				_logger.LogWarning("Job {JobId}, Batch {BatchId} göndərilərkən xəta: {Message}", jobId, payload.BatchId, sendResult.Message);
				try
				{
					//string dbErrorMessage = $"API İlkin Xəta: {sendResult.StatusCode} - {sendResult.Message}";
					//await _jobService.SetBatchFailedAsync(payload.BatchId, dbErrorMessage, ct);
                    await _jobService.SaveBatchValidationErrorsAsync(
                        payload.BatchId,
                        payload.CreditsInOrder,
                        sendResult.ApiResponse,
                        CancellationToken.None);
                }
				catch (Exception dbEx)
				{
					_logger.LogError(dbEx, "Xəta statusunu bazaya yazarkən problem yaşandı! BatchId: {BatchId}", payload.BatchId);
				}

			}
				

			return new SendBatchResultDto
			{
				BatchId = payload.BatchId,
				IsSent = sendResult.IsSent,
				Message = sendResult.Message,
				StatusCode = sendResult.StatusCode,
				RequestId = sendResult.RequestId,
				ApiResponse = sendResult.ApiResponse
			};
			//return new SendBatchResultDto
			//{
			//	BatchId = payload.BatchId,
			//	IsSent = isSent,
			//	Message = isSent ? "Paket uğurla göndərildi." : "Paket göndərilərkən xəta baş verdi."
			//};
		}

		public async Task<SendJobResultDto> SendJobAsync(long jobId, CancellationToken ct)
		{
			int sentCount = 0;
			int failedCount = 0;
			var batchResults = new List<SendBatchResultDto>();

			while (!ct.IsCancellationRequested)
			{
				var batchResult = await SendNextBatchAsync(jobId, ct);

				if (batchResult.BatchId == 0)
					break;
				batchResults.Add(batchResult);

				if (batchResult.IsSent)
					sentCount++;
				else
					failedCount++;
			}

			await _jobService.MarkJobAsCompletedAsync(jobId);

			_logger.LogInformation("Job {JobId} göndərilməsi tamamlandı. Uğurlu: {SentCount}, Uğursuz: {FailedCount}",	jobId, sentCount, failedCount);

			string resultMessage;
			if (sentCount == 0 && failedCount == 0)
			{
				resultMessage = "Göndəriləcək yeni paket tapılmadı (və ya artıq göndərilib).";
			}
			else if (failedCount == 0)
			{
				resultMessage = "Bütün paketlər uğurla göndərildi.";
			}
			else
			{
				resultMessage = $"{sentCount} paket göndərildi, {failedCount} paket uğursuz oldu.";
			}

			return new SendJobResultDto
			{
				JobId = jobId,
				SentCount = sentCount,
				FailedCount = failedCount,
				IsCompleted = true,
				BatchResults = batchResults,
				Message = resultMessage
			};
		}

		public async Task<CheckStatusResponseDto> CheckStatusByRequestIdAsync(string requestId, CancellationToken ct)
		{
			_logger.LogInformation("RequestId {RequestId} üçün CBAR-dan status yoxlanılır...", requestId);

			var statusResult = await _apiClient.CheckBatchStatusAsync(requestId, ct);

			return statusResult;
		}

        public async Task<BatchPayloadLogDto> GetBatchPayloadLogsAsync(long batchId, bool includeBody, CancellationToken ct)
        {
            return await _payloadLogReader.GetByBatchIdAsync(batchId, includeBody, ct);
        }

        public async Task<JobPayloadLogsDto> GetJobPayloadLogsAsync(long jobId, bool includeBody, CancellationToken ct)
        {
            var batches = await _repo.GetBatchesAsync(jobId, null, null, null, ct);
            var batchIds = batches.Select(b => b.BatchId).ToList();
            return await _payloadLogReader.GetByJobIdAsync(jobId, batchIds, includeBody, ct);
        }
    }
}
