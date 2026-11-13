using AccrServicesApi.Models.Settings;

namespace AccrServicesApi.Data.Logs
{
    public interface ISaveLogs
    {
        Task SaveLogAsync(LogEntry logEntry);
    }
}
