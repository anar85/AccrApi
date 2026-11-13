namespace AccrServicesApi.BackgroundServices
{
    public sealed class ExportDispatcherGate : IExportDispatcherGate
    {
        private readonly object _lock = new();
        private readonly ILogger<ExportDispatcherGate> _logger;
        private DateTimeOffset _activeUntil = DateTimeOffset.MinValue;

        public ExportDispatcherGate(ILogger<ExportDispatcherGate> logger)
        {
            _logger = logger;
        }

        public void Activate(TimeSpan duration)
        {
            var until = DateTimeOffset.UtcNow.Add(duration);

            lock (_lock)
            {
                var wasActive = IsActiveUnsafe();
                _activeUntil = until;

                if (!wasActive)
                    _logger.LogInformation("Export Dispatcher aktivləşdirildi. {Until:u} tarixinə qədər işləyəcək.", until);
                else
                    _logger.LogInformation("Export Dispatcher aktivlik müddəti uzadıldı. {Until:u} tarixinə qədər işləyəcək.", until);
            }
        }

        public bool IsActive
        {
            get
            {
                lock (_lock)
                    return IsActiveUnsafe();
            }
        }

        public DateTimeOffset? ActiveUntil
        {
            get
            {
                lock (_lock)
                    return IsActiveUnsafe() ? _activeUntil : null;
            }
        }

        private bool IsActiveUnsafe() => DateTimeOffset.UtcNow < _activeUntil;
    }
}
