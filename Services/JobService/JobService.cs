using AccrServicesApi.Data.JobRepository;
using AccrServicesApi.Models.JobModels;
using AccrServicesApi.Models.Settings;
using AccrServicesApi.Services.ApiClient;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccrServicesApi.Services.JobService
{
	public class JobService : IJobService
	{
		private readonly IAccrJobRepository _repo;
		private readonly ILogger<JobService> _logger;
		private readonly CbarApiSettings _settings;

		public JobService(IAccrJobRepository repo, ILogger<JobService> logger, IOptions<CbarApiSettings> options)
		{
			_repo = repo;
			_logger = logger;
			_settings = options.Value;
		}

		public async Task<JobCreateResponse> CreateJobAsync(JobCreateRequest req, CancellationToken ct)
		{
			try
			{
				if (req.DateFrom.ToDateTime(TimeOnly.MinValue).Date > req.DateTo.ToDateTime(TimeOnly.MinValue).Date)
					throw new ArgumentException("DateFrom DateTo-dan böyük ola bilməz.");

				var jobId = await _repo.CreateJobAsync(req.DateFrom.ToDateTime(TimeOnly.MinValue), req.DateTo.ToDateTime(TimeOnly.MinValue), req.ReportType, req.ContractList, ct);

				return new JobCreateResponse
				{
					JobId = jobId,
					Status = 0
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating job");
				throw;
			}
		}

		public async Task<AccrJobModel> GetNextJobAsync(CancellationToken ct)
		{
			try
			{
				var job = await _repo.GetNextJobAsync(ct);

				if (job == null)
				{
					_logger.LogInformation("No pending jobs found");
					return null;
				}

				_logger.LogInformation("Retrieved next job. JobId: {JobId}, Status: {Status}", job.Id, job.Status);
				return job;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting next job");
				throw;
			}
		}

		public async Task MarkJobAsCompletedAsync(long jobId)
		{
			try
			{
				_logger.LogInformation("Marking Job as completed. JobId: {JobId}", jobId);
				await _repo.MarkJobAsCompletedAsync(jobId);
				_logger.LogInformation("Job marked as completed successfully. JobId: {JobId}", jobId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error marking job as completed for JobId: {JobId}", jobId);
				throw;
			}
		}

		public async Task SetBatchFailedAsync(long batchId, string errorMessage, CancellationToken ct)
		{
			try
			{
				_logger.LogInformation("Marking Batch as failed. BatchId: {BatchId}, Error: {ErrorMessage}", batchId, errorMessage);

				await _repo.SetBatchFailedAsync(batchId, errorMessage, ct);

				_logger.LogInformation("Batch marked as failed successfully. BatchId: {BatchId}", batchId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error marking batch as failed for BatchId: {BatchId}", batchId);
				throw;
			}
		}

		public async Task BuildJobDataAsync(long jobId, int reportType)
		{
			try
			{
				_logger.LogInformation("Building job data for JobId: {JobId}", jobId);
				await _repo.BuildJobDataAsync(jobId, reportType);
				_logger.LogInformation("Job data built successfully for JobId: {JobId}", jobId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error building job data for JobId: {JobId}", jobId);
				throw;
			}
		}

		public async Task<(long BatchId, List<Credit> Credits, List<Borrower> Borrowers, List<Collateral> Collaterals, List<Guarantee> Guarantees)> GetBatchDataAsync(long jobId)
		{
			try
			{
				_logger.LogInformation("Getting batch data for JobId: {JobId}", jobId);
				var result = await _repo.GetBatchDataAsync(jobId);

				_logger.LogInformation(
					"Batch data retrieved for JobId: {JobId}. BatchId: {BatchId}, Credits: {CreditCount}, Borrowers: {BorrowerCount}, Collaterals: {CollateralCount}, Guarantees: {GuaranteeCount}",
					jobId, result.BatchId, result.Credits.Count, result.Borrowers.Count, result.Collaterals.Count, result.Guarantees.Count);

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting batch data for JobId: {JobId}", jobId);
				throw;
			}
		}

		public async Task UpdateBatchRequestIdAsync(long batchId, string requestId, int status)
		{
			try
			{
				_logger.LogInformation(
					"Updating batch status. RequestId: {RequestId}, Status: {Status}",
					requestId, status);

				await _repo.UpdateBatchSetRequestidAsync(batchId, requestId, status);

				_logger.LogInformation("Batch status updated successfully for RequestId: {RequestId}", requestId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating batch status for RequestId: {RequestId}", requestId);
				throw;
			}
		}

		public async Task UpdateBatchStatusByIdAsync(long batchId, int status)
		{
			try
			{
				_logger.LogInformation("Updating batch status by ID. BatchId: {BatchId}, Status: {Status}", batchId, status);

				await _repo.UpdateBatchStatusByIdAsync(batchId, status);

				_logger.LogInformation("Batch status updated successfully by ID. BatchId: {BatchId}", batchId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating batch status by ID for BatchId: {BatchId}", batchId);
				throw;
			}
		}
		public async Task<(long BatchId, string JsonPayload)> GetNextBatchPayloadAsyncoldindexsiz(long jobId)
		{
			_logger.LogInformation($"Job {jobId} üçün kursor məlumatları Oracle-dan çəkilir...");

			var batchData = await GetBatchDataAsync(jobId);

			if (batchData.BatchId == 0 || batchData.Credits == null || !batchData.Credits.Any())
			{
				_logger.LogInformation($"Job {jobId} üçün göndəriləcək yeni paket qalmayıb.");
				return (0, null); // 0 və null qaytarırıq ki, Worker dövrü dayandırsın
			}

			_logger.LogInformation($"Batch {batchData.BatchId} üzrə {batchData.Credits.Count} kredit RAM-da birləşdirilir...");

			foreach (var credit in batchData.Credits)
			{
				credit.Borrower = batchData.Borrowers.FirstOrDefault(b => b.CreditId == credit.Id);
				credit.Collaterals = batchData.Collaterals.Where(c => c.CreditId == credit.Id).ToList();
				credit.Guarantees = batchData.Guarantees.Where(g => g.CreditId == credit.Id).ToList();
			}

			var requestModel = new AccrRequest
			{
				Header = new Header
				{
					BankName = _settings.BankName,
					BankID = _settings.BankID,
					ReportingDate = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
					ResponsiblePerson = new ResponsiblePerson
					{
						Name = _settings.ResponsiblePersonName,
						Jobtitle = _settings.ResponsiblePersonJobTitle,
						PinCode = _settings.ResponsiblePersonPinCode,
						Id = _settings.ResponsiblePersonId,
						DateOfBirth = _settings.ResponsiblePersonDob
					}
				},
				Credits = batchData.Credits
			};

			var jsonOptions = new JsonSerializerOptions
			{
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				WriteIndented = false
			};

			string jsonPayload = JsonSerializer.Serialize(requestModel, jsonOptions);

			_logger.LogInformation($"Batch {batchData.BatchId} üçün JSON hazırdır. Ölçü: {jsonPayload.Length} simvol.");

			// Sadəcə BatchId-ni və Hazır JSON-u Worker-ə qaytarırıq
			return (batchData.BatchId, jsonPayload);
		}

        public async Task SaveBatchValidationErrorsAsync1(long batchId, IReadOnlyList<BatchCreditRef> creditsInOrder, string? apiResponse, CancellationToken ct)
        {
            var errorsByIndex = CbarValidationErrorParser.Parse(apiResponse);

            if (errorsByIndex.Count == 0)
            {
                string fallbackMessage = string.IsNullOrWhiteSpace(apiResponse)
                    ? "API İlkin Xəta: naməlum cavab"
                    : $"API İlkin Xəta: {apiResponse}";

                await SetBatchFailedAsync(batchId, fallbackMessage, ct);
                return;
            }

            _logger.LogInformation(
                "Batch {BatchId} validation xətaları parse olundu. Kredit sayı: {CreditCount}, Xətalı indeks sayı: {ErrorIndexCount}",
                batchId, creditsInOrder.Count, errorsByIndex.Count);

            foreach (var (creditIndex, fieldErrors) in errorsByIndex)
            {
                if (creditIndex < 0 || creditIndex >= creditsInOrder.Count)
                {
                    _logger.LogWarning(
                        "Batch {BatchId}: Credits[{CreditIndex}] siyahıdan kənardır (Count={Count}). Xəta atlanır.",
                        batchId, creditIndex, creditsInOrder.Count);
                    continue;
                }

                var credit = creditsInOrder[creditIndex];

                foreach (var fieldError in fieldErrors)
                {
                    await _repo.LogValidationErrorAsync(
                        batchId,
                        credit.CreditId,
                        credit.AccountNo,
                        "Validation",
                        null,
                        NormalizeErrorCode(fieldError.FieldPath),
                        fieldError.Message,
                        ct);
                }
            }
        }

        public async Task SaveBatchValidationErrorsAsync(long batchId, IReadOnlyList<BatchCreditRef> creditsInOrder, string? apiResponse, CancellationToken ct)
        {
            var errorsByIndex = CbarValidationErrorParser.Parse(apiResponse);

            if (errorsByIndex.Count == 0)
            {
                string fallbackMessage = string.IsNullOrWhiteSpace(apiResponse)
                    ? "API İlkin Xəta: naməlum cavab"
                    : $"API İlkin Xəta: {apiResponse}";

                await SetBatchFailedAsync(batchId, fallbackMessage, CancellationToken.None);
                return;
            }

            _logger.LogInformation(
                "Batch {BatchId} validation xətaları parse olundu. Kredit sayı: {CreditCount}, Xətalı indeks sayı: {ErrorIndexCount}",
                batchId, creditsInOrder.Count, errorsByIndex.Count);

            // Bütün xətaları evvəlcədən siyahıya yığırıq
            var errorList = new List<(long CreditId, string? AccountNo, string ErrorCode, string ErrorMessage)>();

            foreach (var (creditIndex, fieldErrors) in errorsByIndex)
            {
                if (creditIndex < 0 || creditIndex >= creditsInOrder.Count)
                {
                    _logger.LogWarning(
                        "Batch {BatchId}: Credits[{CreditIndex}] siyahıdan kənardır (Count={Count}). Xəta atlanır.",
                        batchId, creditIndex, creditsInOrder.Count);
                    continue;
                }

                var credit = creditsInOrder[creditIndex];

                foreach (var fieldError in fieldErrors)
                {
                    errorList.Add((
                        credit.CreditId,
                        credit.AccountNo,
                        NormalizeErrorCode(fieldError.FieldPath),
                        fieldError.Message
                    ));
                }
            }

            // Bazaya tək bağlantı (single connection) üzərindən göndəririk
            await _repo.LogValidationErrorsBulkAsync(batchId, errorList, ct);
        }

        private static string NormalizeErrorCode(string fieldPath)
        {
            const string prefix = "Credits[";
            if (!fieldPath.StartsWith(prefix, StringComparison.Ordinal))
                return fieldPath;

            var closeBracket = fieldPath.IndexOf(']');
            if (closeBracket < 0 || closeBracket + 1 >= fieldPath.Length)
                return fieldPath;

            return fieldPath[closeBracket + 1] == '.'
                ? fieldPath[(closeBracket + 2)..]
                : fieldPath[(closeBracket + 1)..];
        }

        public async Task<(long BatchId, string JsonPayload, List<BatchCreditRef> CreditsInOrder)> GetNextBatchPayloadAsync(long jobId)
        {
            _logger.LogInformation($"Job {jobId} üçün kursor məlumatları Oracle-dan çəkilir...");

            var batchData = await GetBatchDataAsync(jobId);

            if (batchData.BatchId == 0 || batchData.Credits == null || !batchData.Credits.Any())
            {
                _logger.LogInformation($"Job {jobId} üçün göndəriləcək yeni paket qalmayıb.");
                return (0, null!, new List<BatchCreditRef>());
            }

            _logger.LogInformation($"Batch {batchData.BatchId} üzrə {batchData.Credits.Count} kredit RAM-da birləşdirilir...");

            foreach (var credit in batchData.Credits)
            {
                credit.Borrower = batchData.Borrowers.FirstOrDefault(b => b.CreditId == credit.Id);
                credit.Collaterals = batchData.Collaterals.Where(c => c.CreditId == credit.Id).ToList();
                credit.Guarantees = batchData.Guarantees.Where(g => g.CreditId == credit.Id).ToList();
            }

            var creditsInOrder = batchData.Credits
                .Select(c => new BatchCreditRef(c.Id, c.AccountNo))
                .ToList();

            var requestModel = new AccrRequest
            {
                Header = new Header
                {
                    BankName = _settings.BankName,
                    BankID = _settings.BankID,
                    ReportingDate = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    ResponsiblePerson = new ResponsiblePerson
                    {
                        Name = _settings.ResponsiblePersonName,
                        Jobtitle = _settings.ResponsiblePersonJobTitle,
                        PinCode = _settings.ResponsiblePersonPinCode,
                        Id = _settings.ResponsiblePersonId,
                        DateOfBirth = _settings.ResponsiblePersonDob
                    }
                },
                Credits = batchData.Credits
            };

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            string jsonPayload = JsonSerializer.Serialize(requestModel, jsonOptions);

            _logger.LogInformation($"Batch {batchData.BatchId} üçün JSON hazırdır. Ölçü: {jsonPayload.Length} simvol.");

            return (batchData.BatchId, jsonPayload, creditsInOrder);
        }

    }
}
