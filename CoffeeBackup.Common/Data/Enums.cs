namespace CoffeeBackup.Common.Data;

public class Enums
{
    public enum Scope
    {
        Undefined = 0,
        // Every injection new instance
        PerDependency = 1,
        // one instance is returned from all requests in the root and all nested scopes
        Single = 2,
        // One lifetime instance
        Lifetime = 3,
    }
}
