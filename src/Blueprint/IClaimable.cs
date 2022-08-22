namespace Blueprint;

/// <summary>
/// Identifies an entity / aggregate root that can be 'claimed', insomuch as it has
/// a resource key that uniquely identifies it within a system and allows claims
/// against the resource to be made.
/// </summary>
public interface IClaimable
{
    /// <summary>
    /// Gets the resource key of this claimable entity.
    /// </summary>
    string ResourceKey { get; }
}