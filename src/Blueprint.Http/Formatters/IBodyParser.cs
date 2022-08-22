using System.Threading.Tasks;

namespace Blueprint.Http.Formatters;

public interface IBodyParser
{
    /// <summary>
    /// Determines whether this <see cref="IBodyParser"/> can deserialize an object of the
    /// <paramref name="context"/>'s <see cref="BodyParserContext.BodyType"/>.
    /// </summary>
    /// <param name="context">The <see cref="BodyParserContext"/>.</param>
    /// <returns>
    /// <c>true</c> if this <see cref="IBodyParser"/> can deserialize an object of the
    /// <paramref name="context"/>'s <see cref="BodyParserContext.BodyType"/>. <c>false</c> otherwise.
    /// </returns>
    bool CanRead(BodyParserContext context);

    /// <summary>
    /// Reads an object from the request body.
    /// </summary>
    /// <param name="context">The <see cref="BodyParserContext"/>.</param>
    /// <returns>A <see cref="Task"/> that on completion deserializes the request body.</returns>
    Task<object> ReadAsync(BodyParserContext context);
}