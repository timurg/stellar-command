using UnityEngine;

/// <summary>
/// Abstract base class for command/management entities that coordinate other game objects.
/// Does not have physical presence in space but manages game logic.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>Admiral is a non-physical Entity for management/coordination systems.</para>
/// <para>Currently used as base for:</para>
/// <list type="bullet">
///   <item>AdmiralProtection: Manages target distribution between defenders and protected objects.</item>
/// </list>
/// <para>UpdateEntity() is called for frame-based logic updates.</para>
/// <para>Unlike SpaceObject, Admiral has no Rigidbody or physics.</para>
/// </remarks>
public abstract class Admiral : Entity
{
    /// <summary>
    /// Called for per-frame logic updates by external controllers.
    /// Override to implement management logic.
    /// </summary>
    /// <param name="deltaTime">Time since last update.</param>
    public virtual void UpdateEntity(float deltaTime)
    {
    }
}