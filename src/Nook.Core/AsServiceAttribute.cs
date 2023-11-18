namespace Nook.Core;

/// <summary>
/// Indicates that the associated parameter should have a value injected 
/// from the service provider on the action method call.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class AsServiceAttribute : Attribute
{
    // Nothing to do
}
