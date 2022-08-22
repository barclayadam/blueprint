using System;

namespace Blueprint.Http;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class HttpMethodAttribute : Attribute
{
    public HttpMethodAttribute(string httpMethod)
    {
        this.HttpMethod = httpMethod;
    }

    public string HttpMethod { get; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute()
        : base("GET")
    {
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute()
        : base("POST")
    {
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute()
        : base("PUT")
    {
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HttpPatchAttribute : HttpMethodAttribute
{
    public HttpPatchAttribute()
        : base("PATCH")
    {
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HttpDeleteAttribute : HttpMethodAttribute
{
    public HttpDeleteAttribute()
        : base("DELETE")
    {
    }
}
