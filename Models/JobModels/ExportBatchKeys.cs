namespace AccrServicesApi.Models.JobModels
{
	public sealed class ExportBatchKeys
	{
		public List<string> AccountNos { get; init; } = new();
		public string? NextLastAccountNo { get; init; }
		public bool HasMore { get; init; }
	}
}
