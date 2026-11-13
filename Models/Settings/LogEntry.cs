namespace AccrServicesApi.Models.Settings
{
    public class LogEntry
    {
        public string LogGuid { get; set; }
        public string HttpMethod { get; set; }
        public string Url { get; set; }
        public string Headers { get; set; }
        public string Body { get; set; }
        public string Type { get; set; } // "Request" or "Response"
        public int? StatusCode { get; set; }
        public DateTime InsertDate { get; set; } = DateTime.Now;
        public string IpAdress { get; set; }
        public long BatchId { get; set; }
    }
}
