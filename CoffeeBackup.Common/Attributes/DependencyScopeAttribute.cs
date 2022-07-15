namespace CoffeeBackup.Common.Attributes;

/// <summary>
/// Used by BulkRegister to define the scope of an injectable implementation
/// </summary>
public class DependencyScopeAttribute : Attribute
{
    public DependencyScopeAttribute(Scope scopeDef)
    {
        ScopeDefinition = scopeDef;
    }
    public Scope ScopeDefinition { get; set; }
}
