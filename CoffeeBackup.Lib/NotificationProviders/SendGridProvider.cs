namespace CoffeeBackup.Lib.NotificationProviders;

public class SendGridProvider : INotificationProvider
{
    private IConfiguration _configuration;

    public SendGridProvider(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public Task NotifyAsync(BackupReport report)
    {
        var client = new SendGridClient(_configuration.GetRequiredSection("Notify:SendGrid:ApiKey").Get<string>());
        var from = new EmailAddress(
            _configuration.GetRequiredSection("Notify:SendGrid:FromEmail").Get<string>(),
            _configuration.GetRequiredSection("Notify:SendGrid:FromName").Get<string>());
        var to = new EmailAddress(
            _configuration.GetRequiredSection("Notify:SendGrid:ToEmail").Get<string>());

        // Define subject
        string subject = report.Status switch
        {
            BackupStatus.Success => "✔️ Backup successful",
            BackupStatus.Inconclusive => "🕑 Backup may have encountered a problem",
            BackupStatus.Failed => "❌ Backup failed!",
            _ => "❌ Unknown backup status"
        };

        // Status
        StringBuilder content = new();
        content.AppendLine("Backup status: " + report.Status.ToString());
        content.AppendLine("Archive size: " + report.ArchiveSize);
        content.AppendLine();

        // File counts
        content.AppendLine("Files included: " + report.AddedFiles.Count());
        if (report.IgnoredFiles.Count > 0)
        {
            content.AppendLine("The following files were not included:");
            foreach (var skipped in report.IgnoredFiles)
                content.AppendLine(skipped.FilePath + ": " + skipped.Reason);
        }
        if (report.Errors.Count > 0)
        {
            content.AppendLine("Errors:");
            foreach (var error in report.Errors)
                content.AppendLine(error);
        }

        var msg = MailHelper.CreateSingleEmail(from, to, subject, content.ToString(), "");
        _ = client.SendEmailAsync(msg);
        return Task.CompletedTask;
    }
}
