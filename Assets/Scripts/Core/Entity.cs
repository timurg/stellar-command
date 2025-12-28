using UnityEngine;

/// <summary>
/// Base class for all game entities in Stellar Command.
/// Provides unique identification for every game object.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>This is the ROOT class of the entire game entity hierarchy. ALL game objects inherit from Entity.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item>Generates unique auto-incrementing ID for each entity instance</item>
///   <item>Provides base MonoBehaviour lifecycle through virtual Awake()</item>
/// </list>
/// <para>Inheritance hierarchy: Entity → SpaceObject → Ship/ProjectileBase, Entity → Admiral, Entity → WeaponGun</para>
/// <para>NEVER modify Id generation logic - it's used for object pooling and targeting systems.</para>
/// </remarks>
public abstract class Entity : MonoBehaviour
{
    /// <summary>
    /// Unique identifier for this entity instance. Auto-assigned on Awake.
    /// Used for object tracking, pooling, and targeting systems.
    /// </summary>
    public int Id { get; private set; }
    
    private static int nextId = 0;

    /// <summary>
    /// Initializes the entity with a unique ID.
    /// Override in derived classes but always call base.Awake() first.
    /// </summary>
    protected virtual void Awake()
    {
        Id = nextId++;
    }
}