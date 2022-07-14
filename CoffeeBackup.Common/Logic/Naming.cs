using System.Globalization;

namespace CoffeeBackup.Common.Logic;

public class Naming
{
    /// <summary>
    /// Format examples:
    /// backup-20220304-1630.tar.gz
    /// someprefix-backup-20220304-1630.tar.gz
    /// </summary>
    static readonly Regex rBackupFileName = new Regex(@"(.+-)?backup-(\d{8})-(\d{4})\.tar\.gz");
    
    /// <summary>
    /// Extract the time a backup is taken from the application's filename format.
    /// </summary>
    /// <param name="filePathOrName"></param>
    /// <returns>DateTime or null when the file does not conform</returns>
    public static DateTime? ExtractTimeFromBackupFileName(string filePathOrName)
    {
        string fileName = Path.GetFileName(filePathOrName);

        // Validate filename as backup
        if (!rBackupFileName.IsMatch(fileName))
            return null;

        // Reconstruct DateTime from the filename
        Match match = rBackupFileName.Match(fileName);
        DateTime result;
        bool isValid = DateTime.TryParseExact(
            string.Format("{0}{1}", match.Groups[2].Value, match.Groups[3].Value),
            "yyyyMMddHHmm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out result);

        // Validate parse. Can fail when regex matches but numbers don't make sense as a date
        if (!isValid) 
            return null;

        // Indicate that the time is UTC and return
        DateTime.SpecifyKind(result, DateTimeKind.Utc);
        return result;
    }
    
    /// <summary>
    /// Generate a new backup file name conforming to the application's standards
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static string GenerateBackupFileName(string? prefix = null)
    {
        DateTime time = DateTime.UtcNow;
        string fileName = string.Format("backup-{1}-{2}.tar.gz", time.ToString("yyyyMMdd"), time.ToString("HHmm"));
        if (prefix is not null) fileName = prefix + "-" + fileName;
        return fileName;
    }
}
