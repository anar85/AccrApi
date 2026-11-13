using AccrServicesApi.Models.AccrServiceModels;

namespace AccrServicesApi.Services.ApiClient
{
	public interface IApiClientService
	{
		//Task<bool> SendToApiAsync(long batchId, string jsonPayload);
		Task<ApiSendResult> SendToApiAsync(long batchId, string jsonPayload);
		Task<CheckStatusResponseDto> CheckBatchStatusAsync(string requestId, CancellationToken ct = default);
		//Task SendToApiAsync(long batchId, string jsonPayload);
	}
}
