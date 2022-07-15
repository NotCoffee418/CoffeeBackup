namespace CoffeeBackup.Tests.Common.Logic;

public class NamingTests
{
    [Theory]
    [InlineData("backup-20220304-1630.tar.gz", "2022-03-04 16:30:00")]
    [InlineData("prefixed-backup-20220304-1630.tar.gz", "2022-03-04 16:30:00")]
    [InlineData("/some/path/prefixed-backup-20220304-1630.tar.gz", "2022-03-04 16:30:00")]
    [InlineData("some/path/prefixed-backup-20220304-1630.tar.gz", "2022-03-04 16:30:00")]
    [InlineData("C:\\Some\\Windows\\Path\\prefixed-backup-20220304-1630.tar.gz", "2022-03-04 16:30:00")]
    public void ExtractTimeFromBackupFileName_ExpectValidMatch(string input, string expectedOutputStr)
    {
        // Get instance
        using var mock = AutoMock.GetLoose(b => b.AddMocked());
        var backupNaming = mock.Create<IBackupNaming>();

        // Parse expected output
        DateTime expectedOutput = DateTime.Parse(expectedOutputStr);
        DateTime.SpecifyKind(expectedOutput, DateTimeKind.Utc);

        // Parse actual output
        DateTime? actualOutput = backupNaming.ExtractTimeFromBackupFileName(input);

        // Validate
        Assert.Equal(expectedOutput, actualOutput);
    }

    [Theory]
    [InlineData("nonsense")]
    [InlineData("another-file.tar.gz")]
    [InlineData("invaliddate-backup-20221332-1630.tar.gz")]
    [InlineData("invalidtime-backup-20221332-2561.tar.gz")]
    [InlineData("20220304-1630.tar.gz")]
    public void ExtractTimeFromBackupFileName_ExpectNull(string input)
    {
        // Get instance
        using var mock = AutoMock.GetLoose(b => b.AddMocked());
        var backupNaming = mock.Create<IBackupNaming>();
        
        // Ensure that we get null and not some other time value
        DateTime? actualOutput = backupNaming.ExtractTimeFromBackupFileName(input);
        Assert.Null(actualOutput);
    }
}
