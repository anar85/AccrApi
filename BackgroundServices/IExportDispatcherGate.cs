namespace AccrServicesApi.BackgroundServices
{
    public interface IExportDispatcherGate
    {
        void Activate(TimeSpan duration);

        bool IsActive { get; }

        DateTimeOffset? ActiveUntil { get; }
    }
}
