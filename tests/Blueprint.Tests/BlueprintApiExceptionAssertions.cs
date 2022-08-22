using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Specialized;

namespace Blueprint.Tests;

public static class BlueprintApiExceptionAssertions
{
    public static async Task<ExceptionAssertions<ApiException>> ThrowApiExceptionAsync(
        this AsyncFunctionAssertions assertions,
        string because = "",
        params object[] becauseArgs)
    {
        return await assertions.ThrowExactlyAsync<ApiException>(because, becauseArgs);
    }

    public static async Task<ExceptionAssertions<ApiException>> WithTitle(
        this Task<ExceptionAssertions<ApiException>> task,
        string expectedTitle,
        string because = "",
        params object[] becauseArgs)
    {
        (await task).And.Title.Should().Be(expectedTitle, because, becauseArgs);

        return await task;
    }

    public static async Task<ExceptionAssertions<ApiException>> WithType(
        this Task<ExceptionAssertions<ApiException>> task,
        string expectedType,
        string because = "",
        params object[] becauseArgs)
    {
        (await task).And.Type.Should().Be(expectedType, because, becauseArgs);

        return await task;
    }

    public static async Task<ExceptionAssertions<ApiException>> WithDetail(
        this Task<ExceptionAssertions<ApiException>> task,
        string expectedDetail,
        string because = "",
        params object[] becauseArgs)
    {
        (await task).And.Detail.Should().Be(expectedDetail, because, becauseArgs);

        return await task;
    }
}