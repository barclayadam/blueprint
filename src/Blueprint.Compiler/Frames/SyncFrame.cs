namespace Blueprint.Compiler.Frames;

/// <summary>
/// A <see cref="Frame" /> that has no async code.
/// </summary>
public abstract class SyncFrame : Frame
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SyncFrame" /> class.
    /// </summary>
    protected SyncFrame()
        : base(false)
    {
    }
}