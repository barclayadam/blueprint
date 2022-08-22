using System.Net;

namespace Blueprint.Http;

/// <summary>
/// <para>
/// A simple <see cref="HttpResult" /> that can be used when no content needs writing, only a status code and (optional)
/// headers.
/// </para>
/// <para>
/// It is recommended to declare return types as a specific subclass of this (i.e. <see cref="StatusCodeResult.Created" />)
/// to provide additional metadata with regards to expected responses to enable a more comprehensive and accurate OpenApi
/// document to be created.
/// </para>
/// </summary>
public partial class StatusCodeResult
{
    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Continue" />.
    /// </summary>
    public sealed class Continue : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Continue" /> class.
        /// </summary>
        public static readonly Continue Instance = new Continue();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Continue" /> class.
        /// </summary>
        private Continue()
            : base(HttpStatusCode.Continue)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.SwitchingProtocols" />.
    /// </summary>
    public sealed class SwitchingProtocols : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.SwitchingProtocols" /> class.
        /// </summary>
        public static readonly SwitchingProtocols Instance = new SwitchingProtocols();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.SwitchingProtocols" /> class.
        /// </summary>
        private SwitchingProtocols()
            : base(HttpStatusCode.SwitchingProtocols)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.OK" />.
    /// </summary>
    public sealed class OK : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.OK" /> class.
        /// </summary>
        public static readonly OK Instance = new OK();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.OK" /> class.
        /// </summary>
        private OK()
            : base(HttpStatusCode.OK)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Created" />.
    /// </summary>
    public sealed class Created : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Created" /> class.
        /// </summary>
        public static readonly Created Instance = new Created();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Created" /> class.
        /// </summary>
        private Created()
            : base(HttpStatusCode.Created)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Accepted" />.
    /// </summary>
    public sealed class Accepted : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Accepted" /> class.
        /// </summary>
        public static readonly Accepted Instance = new Accepted();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Accepted" /> class.
        /// </summary>
        private Accepted()
            : base(HttpStatusCode.Accepted)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.NonAuthoritativeInformation" />.
    /// </summary>
    public sealed class NonAuthoritativeInformation : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.NonAuthoritativeInformation" /> class.
        /// </summary>
        public static readonly NonAuthoritativeInformation Instance = new NonAuthoritativeInformation();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.NonAuthoritativeInformation" /> class.
        /// </summary>
        private NonAuthoritativeInformation()
            : base(HttpStatusCode.NonAuthoritativeInformation)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.NoContent" />.
    /// </summary>
    public sealed class NoContent : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.NoContent" /> class.
        /// </summary>
        public static readonly NoContent Instance = new NoContent();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.NoContent" /> class.
        /// </summary>
        private NoContent()
            : base(HttpStatusCode.NoContent)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.ResetContent" />.
    /// </summary>
    public sealed class ResetContent : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.ResetContent" /> class.
        /// </summary>
        public static readonly ResetContent Instance = new ResetContent();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.ResetContent" /> class.
        /// </summary>
        private ResetContent()
            : base(HttpStatusCode.ResetContent)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.PartialContent" />.
    /// </summary>
    public sealed class PartialContent : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.PartialContent" /> class.
        /// </summary>
        public static readonly PartialContent Instance = new PartialContent();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.PartialContent" /> class.
        /// </summary>
        private PartialContent()
            : base(HttpStatusCode.PartialContent)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.MultipleChoices" />.
    /// </summary>
    public sealed class MultipleChoices : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.MultipleChoices" /> class.
        /// </summary>
        public static readonly MultipleChoices Instance = new MultipleChoices();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.MultipleChoices" /> class.
        /// </summary>
        private MultipleChoices()
            : base(HttpStatusCode.MultipleChoices)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Ambiguous" />.
    /// </summary>
    public sealed class Ambiguous : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Ambiguous" /> class.
        /// </summary>
        public static readonly Ambiguous Instance = new Ambiguous();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Ambiguous" /> class.
        /// </summary>
        private Ambiguous()
            : base(HttpStatusCode.Ambiguous)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.MovedPermanently" />.
    /// </summary>
    public sealed class MovedPermanently : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.MovedPermanently" /> class.
        /// </summary>
        public static readonly MovedPermanently Instance = new MovedPermanently();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.MovedPermanently" /> class.
        /// </summary>
        private MovedPermanently()
            : base(HttpStatusCode.MovedPermanently)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Moved" />.
    /// </summary>
    public sealed class Moved : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Moved" /> class.
        /// </summary>
        public static readonly Moved Instance = new Moved();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Moved" /> class.
        /// </summary>
        private Moved()
            : base(HttpStatusCode.Moved)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Found" />.
    /// </summary>
    public sealed class Found : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Found" /> class.
        /// </summary>
        public static readonly Found Instance = new Found();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Found" /> class.
        /// </summary>
        private Found()
            : base(HttpStatusCode.Found)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Redirect" />.
    /// </summary>
    public sealed class Redirect : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Redirect" /> class.
        /// </summary>
        public static readonly Redirect Instance = new Redirect();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Redirect" /> class.
        /// </summary>
        private Redirect()
            : base(HttpStatusCode.Redirect)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.SeeOther" />.
    /// </summary>
    public sealed class SeeOther : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.SeeOther" /> class.
        /// </summary>
        public static readonly SeeOther Instance = new SeeOther();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.SeeOther" /> class.
        /// </summary>
        private SeeOther()
            : base(HttpStatusCode.SeeOther)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.RedirectMethod" />.
    /// </summary>
    public sealed class RedirectMethod : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.RedirectMethod" /> class.
        /// </summary>
        public static readonly RedirectMethod Instance = new RedirectMethod();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.RedirectMethod" /> class.
        /// </summary>
        private RedirectMethod()
            : base(HttpStatusCode.RedirectMethod)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.NotModified" />.
    /// </summary>
    public sealed class NotModified : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.NotModified" /> class.
        /// </summary>
        public static readonly NotModified Instance = new NotModified();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.NotModified" /> class.
        /// </summary>
        private NotModified()
            : base(HttpStatusCode.NotModified)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.UseProxy" />.
    /// </summary>
    public sealed class UseProxy : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.UseProxy" /> class.
        /// </summary>
        public static readonly UseProxy Instance = new UseProxy();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.UseProxy" /> class.
        /// </summary>
        private UseProxy()
            : base(HttpStatusCode.UseProxy)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Unused" />.
    /// </summary>
    public sealed class Unused : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Unused" /> class.
        /// </summary>
        public static readonly Unused Instance = new Unused();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Unused" /> class.
        /// </summary>
        private Unused()
            : base(HttpStatusCode.Unused)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.TemporaryRedirect" />.
    /// </summary>
    public sealed class TemporaryRedirect : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.TemporaryRedirect" /> class.
        /// </summary>
        public static readonly TemporaryRedirect Instance = new TemporaryRedirect();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.TemporaryRedirect" /> class.
        /// </summary>
        private TemporaryRedirect()
            : base(HttpStatusCode.TemporaryRedirect)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.RedirectKeepVerb" />.
    /// </summary>
    public sealed class RedirectKeepVerb : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.RedirectKeepVerb" /> class.
        /// </summary>
        public static readonly RedirectKeepVerb Instance = new RedirectKeepVerb();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.RedirectKeepVerb" /> class.
        /// </summary>
        private RedirectKeepVerb()
            : base(HttpStatusCode.RedirectKeepVerb)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.BadRequest" />.
    /// </summary>
    public sealed class BadRequest : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.BadRequest" /> class.
        /// </summary>
        public static readonly BadRequest Instance = new BadRequest();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.BadRequest" /> class.
        /// </summary>
        private BadRequest()
            : base(HttpStatusCode.BadRequest)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Unauthorized" />.
    /// </summary>
    public sealed class Unauthorized : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Unauthorized" /> class.
        /// </summary>
        public static readonly Unauthorized Instance = new Unauthorized();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Unauthorized" /> class.
        /// </summary>
        private Unauthorized()
            : base(HttpStatusCode.Unauthorized)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.PaymentRequired" />.
    /// </summary>
    public sealed class PaymentRequired : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.PaymentRequired" /> class.
        /// </summary>
        public static readonly PaymentRequired Instance = new PaymentRequired();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.PaymentRequired" /> class.
        /// </summary>
        private PaymentRequired()
            : base(HttpStatusCode.PaymentRequired)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Forbidden" />.
    /// </summary>
    public sealed class Forbidden : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Forbidden" /> class.
        /// </summary>
        public static readonly Forbidden Instance = new Forbidden();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Forbidden" /> class.
        /// </summary>
        private Forbidden()
            : base(HttpStatusCode.Forbidden)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.NotFound" />.
    /// </summary>
    public sealed class NotFound : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.NotFound" /> class.
        /// </summary>
        public static readonly NotFound Instance = new NotFound();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.NotFound" /> class.
        /// </summary>
        private NotFound()
            : base(HttpStatusCode.NotFound)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.MethodNotAllowed" />.
    /// </summary>
    public sealed class MethodNotAllowed : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.MethodNotAllowed" /> class.
        /// </summary>
        public static readonly MethodNotAllowed Instance = new MethodNotAllowed();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.MethodNotAllowed" /> class.
        /// </summary>
        private MethodNotAllowed()
            : base(HttpStatusCode.MethodNotAllowed)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.NotAcceptable" />.
    /// </summary>
    public sealed class NotAcceptable : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.NotAcceptable" /> class.
        /// </summary>
        public static readonly NotAcceptable Instance = new NotAcceptable();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.NotAcceptable" /> class.
        /// </summary>
        private NotAcceptable()
            : base(HttpStatusCode.NotAcceptable)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.ProxyAuthenticationRequired" />.
    /// </summary>
    public sealed class ProxyAuthenticationRequired : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.ProxyAuthenticationRequired" /> class.
        /// </summary>
        public static readonly ProxyAuthenticationRequired Instance = new ProxyAuthenticationRequired();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.ProxyAuthenticationRequired" /> class.
        /// </summary>
        private ProxyAuthenticationRequired()
            : base(HttpStatusCode.ProxyAuthenticationRequired)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.RequestTimeout" />.
    /// </summary>
    public sealed class RequestTimeout : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.RequestTimeout" /> class.
        /// </summary>
        public static readonly RequestTimeout Instance = new RequestTimeout();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.RequestTimeout" /> class.
        /// </summary>
        private RequestTimeout()
            : base(HttpStatusCode.RequestTimeout)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Conflict" />.
    /// </summary>
    public sealed class Conflict : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Conflict" /> class.
        /// </summary>
        public static readonly Conflict Instance = new Conflict();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Conflict" /> class.
        /// </summary>
        private Conflict()
            : base(HttpStatusCode.Conflict)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.Gone" />.
    /// </summary>
    public sealed class Gone : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.Gone" /> class.
        /// </summary>
        public static readonly Gone Instance = new Gone();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.Gone" /> class.
        /// </summary>
        private Gone()
            : base(HttpStatusCode.Gone)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.LengthRequired" />.
    /// </summary>
    public sealed class LengthRequired : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.LengthRequired" /> class.
        /// </summary>
        public static readonly LengthRequired Instance = new LengthRequired();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.LengthRequired" /> class.
        /// </summary>
        private LengthRequired()
            : base(HttpStatusCode.LengthRequired)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.PreconditionFailed" />.
    /// </summary>
    public sealed class PreconditionFailed : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.PreconditionFailed" /> class.
        /// </summary>
        public static readonly PreconditionFailed Instance = new PreconditionFailed();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.PreconditionFailed" /> class.
        /// </summary>
        private PreconditionFailed()
            : base(HttpStatusCode.PreconditionFailed)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.RequestEntityTooLarge" />.
    /// </summary>
    public sealed class RequestEntityTooLarge : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.RequestEntityTooLarge" /> class.
        /// </summary>
        public static readonly RequestEntityTooLarge Instance = new RequestEntityTooLarge();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.RequestEntityTooLarge" /> class.
        /// </summary>
        private RequestEntityTooLarge()
            : base(HttpStatusCode.RequestEntityTooLarge)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.RequestUriTooLong" />.
    /// </summary>
    public sealed class RequestUriTooLong : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.RequestUriTooLong" /> class.
        /// </summary>
        public static readonly RequestUriTooLong Instance = new RequestUriTooLong();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.RequestUriTooLong" /> class.
        /// </summary>
        private RequestUriTooLong()
            : base(HttpStatusCode.RequestUriTooLong)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.UnsupportedMediaType" />.
    /// </summary>
    public sealed class UnsupportedMediaType : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.UnsupportedMediaType" /> class.
        /// </summary>
        public static readonly UnsupportedMediaType Instance = new UnsupportedMediaType();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.UnsupportedMediaType" /> class.
        /// </summary>
        private UnsupportedMediaType()
            : base(HttpStatusCode.UnsupportedMediaType)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.RequestedRangeNotSatisfiable" />.
    /// </summary>
    public sealed class RequestedRangeNotSatisfiable : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.RequestedRangeNotSatisfiable" /> class.
        /// </summary>
        public static readonly RequestedRangeNotSatisfiable Instance = new RequestedRangeNotSatisfiable();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.RequestedRangeNotSatisfiable" /> class.
        /// </summary>
        private RequestedRangeNotSatisfiable()
            : base(HttpStatusCode.RequestedRangeNotSatisfiable)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.ExpectationFailed" />.
    /// </summary>
    public sealed class ExpectationFailed : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.ExpectationFailed" /> class.
        /// </summary>
        public static readonly ExpectationFailed Instance = new ExpectationFailed();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.ExpectationFailed" /> class.
        /// </summary>
        private ExpectationFailed()
            : base(HttpStatusCode.ExpectationFailed)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.UpgradeRequired" />.
    /// </summary>
    public sealed class UpgradeRequired : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.UpgradeRequired" /> class.
        /// </summary>
        public static readonly UpgradeRequired Instance = new UpgradeRequired();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.UpgradeRequired" /> class.
        /// </summary>
        private UpgradeRequired()
            : base(HttpStatusCode.UpgradeRequired)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.InternalServerError" />.
    /// </summary>
    public sealed class InternalServerError : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.InternalServerError" /> class.
        /// </summary>
        public static readonly InternalServerError Instance = new InternalServerError();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.InternalServerError" /> class.
        /// </summary>
        private InternalServerError()
            : base(HttpStatusCode.InternalServerError)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.NotImplemented" />.
    /// </summary>
    public sealed class NotImplemented : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.NotImplemented" /> class.
        /// </summary>
        public static readonly NotImplemented Instance = new NotImplemented();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.NotImplemented" /> class.
        /// </summary>
        private NotImplemented()
            : base(HttpStatusCode.NotImplemented)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.BadGateway" />.
    /// </summary>
    public sealed class BadGateway : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.BadGateway" /> class.
        /// </summary>
        public static readonly BadGateway Instance = new BadGateway();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.BadGateway" /> class.
        /// </summary>
        private BadGateway()
            : base(HttpStatusCode.BadGateway)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.ServiceUnavailable" />.
    /// </summary>
    public sealed class ServiceUnavailable : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.ServiceUnavailable" /> class.
        /// </summary>
        public static readonly ServiceUnavailable Instance = new ServiceUnavailable();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.ServiceUnavailable" /> class.
        /// </summary>
        private ServiceUnavailable()
            : base(HttpStatusCode.ServiceUnavailable)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.GatewayTimeout" />.
    /// </summary>
    public sealed class GatewayTimeout : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.GatewayTimeout" /> class.
        /// </summary>
        public static readonly GatewayTimeout Instance = new GatewayTimeout();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.GatewayTimeout" /> class.
        /// </summary>
        private GatewayTimeout()
            : base(HttpStatusCode.GatewayTimeout)
        {
        }
    }

    /// <summary>
    /// A <see cref="StatusCodeResult" /> for the status <see cref="HttpStatusCode.HttpVersionNotSupported" />.
    /// </summary>
    public sealed class HttpVersionNotSupported : StatusCodeResult
    {
        /// <summary>
        /// The static instance of the <see cref="StatusCodeResult.HttpVersionNotSupported" /> class.
        /// </summary>
        public static readonly HttpVersionNotSupported Instance = new HttpVersionNotSupported();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeResult.HttpVersionNotSupported" /> class.
        /// </summary>
        private HttpVersionNotSupported()
            : base(HttpStatusCode.HttpVersionNotSupported)
        {
        }
    }
}