using System;
using Blueprint.Http.Formatters;
using Microsoft.Extensions.Logging;

namespace Blueprint.Http;

internal static class BlueprintHttpLoggerExtensions
{
    private static readonly Action<ILogger, IBodyParser, string, Exception> _bodyParserSelected;
    private static readonly Action<ILogger, IBodyParser, string, Exception> _bodyParserRejected;
    private static readonly Action<ILogger, string, Exception> _noBodyParserSelected;
    private static readonly Action<ILogger, Type, Exception> _bodyParsingException;

    private static readonly Action<ILogger, Type, Exception> _attemptingToBindModel;
    private static readonly Action<ILogger, Type, Type, Exception> _doneParsingBody;

    static BlueprintHttpLoggerExtensions()
    {
        _bodyParserSelected = LoggerMessage.Define<IBodyParser, string>(
            LogLevel.Debug,
            new EventId(1, "BodyParserSelected"),
            "Selected body parser '{BodyParser}' for content type '{ContentType}'.");

        _bodyParserRejected = LoggerMessage.Define<IBodyParser, string>(
            LogLevel.Debug,
            new EventId(2, "BodyParserRejected"),
            "Rejected body parser'{BodyParser}' for content type '{ContentType}'.");

        _noBodyParserSelected = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(3, "NoBodyParserSelected"),
            "No body parser was found to support the content type '{ContentType}'");

        _attemptingToBindModel = LoggerMessage.Define<Type>(
            LogLevel.Debug,
            new EventId(4, "AttemptingToBindModel"),
            "Attempting to parse HTTP body of type '{ModelType}'");

        _doneParsingBody = LoggerMessage.Define<Type, Type>(
            LogLevel.Debug,
            new EventId(5, "DoneAttemptingToBindModel"),
            "Done attempting to parse HTTP body type '{ModelType}' with parser {ParserType}.");

        _bodyParsingException = LoggerMessage.Define<Type>(
            LogLevel.Debug,
            new EventId(6, "BodyParsingException"),
            "Body parsing failed with an exception for body type '{ModelType}.");
    }

    public static void BodyParserSelected(
        this ILogger logger,
        IBodyParser bodyParser,
        BodyParserContext formatterContext)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var contentType = formatterContext.HttpContext.Request.ContentType;
            _bodyParserSelected(logger, bodyParser, contentType, null);
        }
    }

    public static void BodyParserRejected(
        this ILogger logger,
        IBodyParser bodyParser,
        BodyParserContext formatterContext)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var contentType = formatterContext.HttpContext.Request.ContentType;
            _bodyParserRejected(logger, bodyParser, contentType, null);
        }
    }

    public static void NoBodyParserSelected(
        this ILogger logger,
        BodyParserContext formatterContext)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var contentType = formatterContext.HttpContext.Request.ContentType;
            _noBodyParserSelected(logger, contentType, null);
        }
    }

    public static void AttemptingToParseBody(this ILogger logger, Type bodyType)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        _attemptingToBindModel(logger, bodyType, null);
    }

    public static void DoneParsingBody(this ILogger logger, Type bodyType, IBodyParser parser)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        _doneParsingBody(logger, bodyType, parser.GetType(), null);
    }

    public static void BodyParsingException(this ILogger logger, Type bodyType, Exception e)
    {
        _bodyParsingException(logger, bodyType, e);
    }
}