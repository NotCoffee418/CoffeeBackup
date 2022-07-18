namespace CoffeeBackup.Tests.Lib.Handlers;

public class ArchiveHandlerTests
{
    [Fact]
    public async Task GenerateBackupAsync_GenerateArchiveNoCrash()
    {
        // Get instance
        using var mock = AutoMock.GetLoose(b => b.AddMocked());
        var archiveHandler = mock.Create<IArchiveHandler>();
        
        // Spawn archive
        string sourceDir = ArchiveTestHelper.SpawnArchivableTestFiles();
        string archiveDir = ArchiveTestHelper.GetTestWorkingDir();
        string archiveFile = Path.Combine(archiveDir, "output.tar.gz");
        try
        {
            // Generate the backup
            await archiveHandler.GenerateBackupAsync(sourceDir,new(), archiveFile);

            // Validate
            Assert.True(File.Exists(archiveFile));
        }
        finally
        {
            ArchiveTestHelper.CleanupWorkingDir(sourceDir);
            ArchiveTestHelper.CleanupWorkingDir(archiveDir);
        }
    }

    [Fact]
    public async Task GenerateBackupAsync_GenerateArchiveSkipLockedFile()
    {
        // Get instance
        using var mock = AutoMock.GetLoose(b => b.AddMocked());
        var archiveHandler = mock.Create<IArchiveHandler>();

        // Spawn archive
        string sourceDir = ArchiveTestHelper.SpawnArchivableTestFiles();
        string archiveDir = ArchiveTestHelper.GetTestWorkingDir();
        string archiveFile = Path.Combine(archiveDir, "output.tar.gz");
        try
        {
            // Create a locked file that should be skipped and not crash
            using (Stream file = File.Create(Path.Combine(sourceDir, "lockedfile.txt")))
            {
                // Generate the backup
                await archiveHandler.GenerateBackupAsync(sourceDir, new(), archiveFile);
            }
            
            // Validate
            Assert.True(File.Exists(archiveFile));
        }
        finally
        {
            ArchiveTestHelper.CleanupWorkingDir(sourceDir);
            ArchiveTestHelper.CleanupWorkingDir(archiveDir);
        }
    }
}
