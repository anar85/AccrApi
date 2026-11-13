using AccrServicesApi.Services.ApiClient;
using AccrServicesApi.Services.JobService;

namespace AccrServicesApi.BackgroundServices
{
	public sealed class ExportDispatcherWorker : BackgroundService
	{
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan IdleCheckInterval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IExportDispatcherGate _gate;
        private readonly ILogger<ExportDispatcherWorker> _logger;

		public ExportDispatcherWorker(IServiceScopeFactory scopeFactory, IExportDispatcherGate gate, ILogger<ExportDispatcherWorker> logger)
		{
			_scopeFactory = scopeFactory;
			_gate = gate;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation($"Export Dispatcher Worker işə düşdü...{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}");

            var wasActive = false;

            while (!stoppingToken.IsCancellationRequested)
			{
                if (!_gate.IsActive)
                {
                    if (wasActive)
                    {
                        _logger.LogInformation($"Export Dispatcher aktivlik müddəti bitdi. Dayandırıldı.{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}");
                        wasActive = false;
                    }

                    await Task.Delay(IdleCheckInterval, stoppingToken);
                    continue;
                }

                if (!wasActive)
                {
                    _logger.LogInformation("Export Dispatcher işə başladı. Aktiv qalacaq: {Until:u}", _gate.ActiveUntil);
                    wasActive = true;
                }

                try
				{
					using (var scope = _scopeFactory.CreateScope())
					{
						var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();

						var job = await jobService.GetNextJobAsync(stoppingToken);

						if (job != null)
						{
							_logger.LogInformation($"Yeni iş emala başlandı: JobId = {job.Id}");

							await jobService.BuildJobDataAsync(job.Id, job.ReportType);

							_logger.LogInformation($"JobId = {job.Id} üçün məlumat hazırdır. Göndərmə UI-dan gözlənilir.");
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogCritical(ex, "ExportDispatcherWorker daxilində kritik xəta baş verdi! Proses 10 saniyə sonra yenidən cəhd edəcək.");
				}

				await Task.Delay(PollInterval, stoppingToken);
			}

			_logger.LogInformation("Export Dispatcher Worker dayandırıldı.");
		}


		//protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		//{
		//	_logger.LogInformation("Export Dispatcher Worker işə düşdü...");

		//	while (!stoppingToken.IsCancellationRequested)
		//	{
		//		try
		//		{
		//			// Hər dövrdə yeni bir Scope yaradırıq
		//			using (var scope = _scopeFactory.CreateScope())
		//			{
		//				var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
		//				var apiClient = scope.ServiceProvider.GetRequiredService<IApiClientService>();

		//				// 1. Gözləmədə olan yeni iş (Job) axtarırıq
		//				var job = await jobService.GetNextJobAsync(stoppingToken);

		//				if (job != null)
		//				{
		//					_logger.LogInformation($"Yeni iş emala başlandı: JobId = {job.Id}");

		//					// 2. ACCR_CREDITS cədvəli
		//					//await jobService.BuildJobDataAsync(job.Id,job.ReportType);

		//					int sentBatchesCount = 0;

		//					// 3. Paketlər (Batch-lər) bitənə qədər DÖVRƏ SALIRIQ
		//					while (!stoppingToken.IsCancellationRequested)
		//					{
		//						// JobService-dən sadəcə Hazır JSON-u və BatchID-ni çəkirik
		//						var payloadResult = await jobService.GetNextBatchPayloadAsync(job.Id);

		//						// Əgər BatchId 0 gəldisə, deməli bu Job üçün göndəriləcək heç nə qalmayıb
		//						if (payloadResult.BatchId == 0)
		//						{
		//							_logger.LogInformation($"JobId = {job.Id} üçün bütün paketlər göndərildi. İş bitdi.");
		//							break; // While dövründən çıxırıq
		//						}

		//						// 4. Göndərmə servisinə müraciət edib paketi Mərkəzi Banka atırıq
		//						bool isSent = await apiClient.SendToApiAsync(payloadResult.BatchId, payloadResult.JsonPayload);

		//						if (isSent)
		//						{
		//							sentBatchesCount++;
		//							_logger.LogInformation($"Batch {payloadResult.BatchId} uğurla emal edildi və növbəti paketə keçilir.");
		//						}
		//						else
		//						{
		//							_logger.LogWarning($"Batch {payloadResult.BatchId} xəta aldı, lakin proses növbəti paketlə davam edir.");

		//						}
		//						await Task.Delay(60000, stoppingToken);
		//					}
		//					_logger.LogInformation($"JobId = {job.Id} üçün göndəriləcək kredit qalmadı. İş tamamlandı.");
		//					await jobService.MarkJobAsCompletedAsync(job.Id);
		//				}
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			// Əgər sistemdə gözlənilməz "Crash" olarsa, Worker dayanmasın deyə Catch edirik
		//			_logger.LogCritical(ex, "ExportDispatcherWorker daxilində kritik xəta baş verdi! Proses 10 saniyə sonra yenidən cəhd edəcək.");
		//		}

		//		// Bazanı yormamaq üçün 10 saniyə (10000 ms) fasilə veririk
		//		await Task.Delay(1000000, stoppingToken);
		//	}

		//	_logger.LogInformation("Export Dispatcher Worker dayandırıldı.");
		//}

	}
}
