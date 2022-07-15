namespace CoffeeBackup.Tests.TestHelpers;

internal static class ArchiveTestHelper
{
    /// <summary>
    /// Spawn test files and return the directory
    /// </summary>
    /// <returns></returns>
    internal static string SpawnArchivableTestFiles()
    {
        // Create a bunch of files in base dir
        string dir = GetTestWorkingDir();
        for (int i = 0; i < 10; i++)
        {
            string file = Path.Combine(dir, $"testfile{i}.txt");
            File.WriteAllText(file, "test");
        }

        string subDir = Path.Combine(dir, "subdir");
        Directory.CreateDirectory(subDir);
        for (int i = 0; i < 10; i++)
        {
            string file = Path.Combine(subDir, $"testsubdirfile{i}.txt");
            File.WriteAllText(file, "testSubdir");
        }

        return dir;
    }
    
    /// <summary>
    /// Get a working directory for tests
    /// </summary>
    /// <param name="subdir"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal static string GetTestWorkingDir()
    {
        string workingDir = Path.Combine(GetWorkingDirBase(), Guid.NewGuid().ToString());

        // Validate and return
        if (Directory.Exists(workingDir))
            throw new Exception("Test is tainted");
        Directory.CreateDirectory(workingDir);
        return workingDir;
    }

    internal static void CleanupWorkingDir(string workingDirPath)
    {
        if (!workingDirPath.StartsWith(GetWorkingDirBase()))
            throw new Exception("Attempting to clean up working dir at invalid path");
        Directory.Delete(workingDirPath, true);
    }

    private static string GetWorkingDirBase()
        => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("impossible"),
            "TestWorkingDir");

}
