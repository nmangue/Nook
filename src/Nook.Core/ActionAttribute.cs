namespace Nook.Core;

/// <summary>
/// Indicates that the associated method is a store action
/// that alters the state.
/// </summary>
/// <remarks>
/// Action method should be declared in a implementation of <see cref="IStore{TState}"/>
/// and should return the new <c>TState</c> value.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class ActionAttribute : Attribute
{
    // Nothing to do
}
