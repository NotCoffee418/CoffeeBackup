namespace CoffeeBackup.StorageProviders.AmazonS3;

internal static class StorageClassInterpreter
{
    static string[] ValidStorageClassNames { get; } = new[]
    {
        "DEEP_ARCHIVE",
        "GLACIER",
        "GLACIER_IR",
        "INTELLIGENT_TIERING",
        "ONEZONE_IA",
        "OUTPOSTS",
        "REDUCED_REDUNDANCY",
        "STANDARD",
        "STANDARD_IA"
    };

    /// <summary>
    /// Ensure any custom storage class input value is valid, assume the default value if undefined.
    /// </summary>
    /// <param name="storageClassOrEmpty"></param>
    /// <returns></returns>
    internal static bool IsValidStorageClassOrDefault(string? storageClassOrEmpty)
        => string.IsNullOrEmpty(storageClassOrEmpty) || ValidStorageClassNames.Contains(storageClassOrEmpty.ToUpper());

    /// <summary>
    /// Get an instance of AWS SDK's S3StorageClass
    /// </summary>
    /// <param name="storageClassOrEmpty"></param>
    /// <returns></returns>
    internal static S3StorageClass GetStorageClassFromName(string? storageClassOrEmpty)
    {
        if (!IsValidStorageClassOrDefault(storageClassOrEmpty))
            throw new Exception("S3: Invalid storage class configured"); // shouldnt happen

        // Use the default, infrequesntaccess
        if (string.IsNullOrEmpty(storageClassOrEmpty))
            return S3StorageClass.StandardInfrequentAccess;
       
        // otherwise create instance of specified storage type
        return new S3StorageClass(storageClassOrEmpty.ToUpper());
    }
}