using medical_record_service.Interfaces;

namespace medical_record_service.Services
{
    /// <summary>
    /// Background service that runs daily to check for medical records with follow-up dates
    /// matching today. In production, this would trigger notifications via the Notification-Service.
    /// </summary>
    public class FollowUpReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FollowUpReminderService> _logger;

        public FollowUpReminderService(IServiceScopeFactory scopeFactory, ILogger<FollowUpReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FollowUp Reminder Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckFollowUps();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking follow-up reminders.");
                }

                // Run every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CheckFollowUps()
        {
            using var scope = _scopeFactory.CreateScope();
            var recordService = scope.ServiceProvider.GetRequiredService<IRecordService>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var followUps = recordService.GetFollowUpRecords(today);

            if (followUps.Count == 0)
            {
                _logger.LogInformation($"[FollowUp Check] No follow-ups due for {today}.");
                return;
            }

            _logger.LogInformation($"[FollowUp Check] Found {followUps.Count} follow-up(s) due for {today}:");

            foreach (var record in followUps)
            {
                _logger.LogInformation(
                    $"  → Patient {record.PatientId} | Appointment {record.AppointmentId} | " +
                    $"Provider {record.ProviderId} | Diagnosis: {record.Diagnosis}");

                // In production, call the Notification-Service API here:
                // POST http://localhost:5006/api/v1/notifications/send
                // { recipientId: record.PatientId, type: "FOLLOWUP", title: "Follow-Up Reminder", ... }
            }

            await Task.CompletedTask;
        }
    }
}
