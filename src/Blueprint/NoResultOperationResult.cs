using System.Threading.Tasks;

namespace Blueprint;

/// <summary>
/// A no-op <see cref="OperationResult" /> that will do <b>nothing</b>. This can be used explicitly but
/// is typically used in fire-and-forget or background tasks which can be handled by infrastructure.
/// </summary>
public sealed class NoResultOperationResult : OperationResult
{
    /// <summary>
    /// The single instance of the <see cref="NoResultOperationResult" /> class that should be
    /// used.
    /// </summary>
    public static readonly NoResultOperationResult Instance = new NoResultOperationResult();

    private NoResultOperationResult()
    {
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(ApiOperationContext context)
    {
        // Intentionally does nothing

        return Task.CompletedTask;
    }
}