using UnityEngine;

/// <summary>
/// Visual effect entity for explosion animations.
/// Handles explosion lifecycle and auto-destruction.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>ExplosionFX is a visual-only entity for explosion effects.</para>
/// <para>Key patterns:</para>
/// <list type="bullet">
///   <item>Uses Animator component for explosion animation.</item>
///   <item>StopExplosion() disables animator and destroys after delay.</item>
///   <item>Should be pooled via ExplosionFXPoolManager for performance.</item>
/// </list>
/// <para>Currently uses Destroy() - consider migrating to pool-based return.</para>
/// </remarks>
public class ExplosionFX: Entity
{
    /// <summary>
    /// Stops the explosion animation and destroys the object after a short delay.
    /// Called at the end of explosion animation.
    /// </summary>
    protected void StopExplosion()
    {
        var component = GetComponent<Animator>();
        component.enabled = false;
        Destroy(gameObject, 0.5f);
    }
}