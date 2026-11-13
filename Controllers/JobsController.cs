using AccrServicesApi.Atributes;
using AccrServicesApi.Models.JobModels;
using AccrServicesApi.Models.ViewModels;
using AccrServicesApi.Services.ViewService;
using Microsoft.AspNetCore.Mvc;

namespace AccrServicesApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class JobsController : ControllerBase
	{
		private readonly IUiJobsService _uiJobsService;

		public JobsController(IUiJobsService uiJobsService)
		{
			_uiJobsService = uiJobsService;
		}

		[HttpPost("create")]
		public async Task<ActionResult<JobCreateResponse>> Create([FromBody] JobCreateRequest req, CancellationToken ct)
		{
			var result = await _uiJobsService.CreateJobAsync(req, ct);
			return Ok(result);
		}

		[HttpGet("getjobslist")]
		public async Task<ActionResult<List<JobListItemDto>>> GetJobs([FromQuery] long? jobId, [FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo, [FromQuery] int? status, CancellationToken ct)
		{
			var result = await _uiJobsService.GetJobsListAsync(jobId, dateFrom, dateTo, status, ct);
			return Ok(result);
		}

		[HttpGet("getbatches")]
		public async Task<ActionResult<List<BatchListItemDto>>> GetBatches([FromQuery] long? jobId, [FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo, [FromQuery] int? status, CancellationToken ct)
		{
			var result = await _uiJobsService.GetBatchesAsync(jobId, dateFrom, dateTo, status, ct);
			return Ok(result);
		}

		[HttpGet("/api/batches/{batchId:long}/credits")]
		public async Task<ActionResult<List<CreditListItemDto>>> GetBatchCredits(long batchId, [FromQuery] int? status, CancellationToken ct)
		{
			var result = await _uiJobsService.GetBatchCreditsAsync(batchId, status, ct);
			return Ok(result);
		}

		/// <summary>
		/// Batch-dəki tək kreditin tam qrafikini qaytarır (borrower, collaterals, guarantees və nested sahələr).
		/// </summary>
		[HttpGet("/api/batches/{batchId:long}/credits/{creditId:long}")]
		public async Task<ActionResult<CreditDetailDto>> GetCreditDetail(long batchId, long creditId, CancellationToken ct)
		{
			var result = await _uiJobsService.GetCreditDetailAsync(batchId, creditId, ct);
			if (result == null)
				return NotFound($"BatchId={batchId}, CreditId={creditId} tapılmadı.");

			return Ok(result);
		}

		/// <summary>
		/// Batch-dəki kreditin bütün məlumatlarını yeniləyir. markForRetry=true (default) → retry növbəsinə düşür.
		/// </summary>
		[HttpPut("/api/batches/{batchId:long}/credits/{creditId:long}")]
		public async Task<ActionResult<UpdateCreditResponseDto>> UpdateCredit(
			long batchId,
			long creditId,
			[FromBody] UpdateCreditRequest request,
			CancellationToken ct)
		{
			if (request?.Credit == null)
				return BadRequest("Credit body boş ola bilməz.");

			var result = await _uiJobsService.UpdateCreditAsync(batchId, creditId, request, ct);
			if (!result.Updated)
				return BadRequest(result);

			return Ok(result);
		}

		[HttpGet("credit/{creditId}/errors")]
		public async Task<ActionResult<List<CreditApiErrorDto>>> GetCreditErrors([FromRoute] long creditId, CancellationToken ct)
		{
			var result = await _uiJobsService.GetCreditErrorsAsync(creditId, ct);
			return Ok(result);
		}

		[HttpPost("{jobId:long}/send")]
        [RequestResponseLogging]
        public async Task<ActionResult<SendJobResultDto>> SendJob(long jobId, CancellationToken ct)
		{
			var result = await _uiJobsService.SendJobAsync(jobId, ct);
			return Ok(result);
		}

		[HttpGet("status/{requestId}")]
		public async Task<IActionResult> CheckStatus(string requestId, CancellationToken ct)
		{
			var result = await _uiJobsService.CheckStatusByRequestIdAsync(requestId, ct);
			return Ok(result);

		}

		[HttpPost("/api/batches/{batchId:long}/retry-failed")]
		public async Task<ActionResult<RetryResultDto>> RetryFailedCredits(long batchId, CancellationToken ct)
		{
			var result = await _uiJobsService.RetryFailedCreditsByBatchAsync(batchId, ct);
			return Ok(result);
		}

		[HttpPost("/api/batches/{batchId:long}/credits/retry")]
		public async Task<ActionResult<RetryResultDto>> RetrySelectedCredits(
			long batchId,
			[FromBody] RetryCreditsRequest request,
			CancellationToken ct)
		{
			if (request.CreditIds.Count == 0)
				return BadRequest("CreditIds boş ola bilməz.");

			var result = await _uiJobsService.RetrySelectedCreditsAsync(batchId, request.CreditIds, ct);
			return Ok(result);
		}

		[HttpPost("{jobId:long}/send-next-batch")]
		public async Task<ActionResult<SendBatchResultDto>> SendNextBatch(long jobId, CancellationToken ct)
		{
			var result = await _uiJobsService.SendNextBatchAsync(jobId, ct);
			return Ok(result);
		}

        /// <summary>
        /// Batch üzrə CBAR ExternalRequest / ExternalResponse fayllarını qaytarır.
        /// includeBody=true → JSON body də gəlir (böyük ola bilər).
        /// </summary>
        [HttpGet("/api/batches/{batchId:long}/payload-logs")]
        public async Task<ActionResult<BatchPayloadLogDto>> GetBatchPayloadLogs(long batchId, [FromQuery] bool includeBody = true, CancellationToken ct = default)
        {
            var result = await _uiJobsService.GetBatchPayloadLogsAsync(batchId, includeBody, ct);
            if (result.Files.Count == 0)
                return NotFound($"BatchId={batchId} üçün payload log tapılmadı.");

            return Ok(result);
        }

        /// <summary>
        /// Job-dakı bütün batch-lərin CBAR request/response loglarını qaytarır.
        /// </summary>
        [HttpGet("{jobId:long}/payload-logs")]
        public async Task<ActionResult<JobPayloadLogsDto>> GetJobPayloadLogs(long jobId, [FromQuery] bool includeBody = false, CancellationToken ct = default)
        {
            var result = await _uiJobsService.GetJobPayloadLogsAsync(jobId, includeBody, ct);
            return Ok(result);
        }

    }
}
