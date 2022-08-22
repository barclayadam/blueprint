using System;
using System.Collections.Generic;
using System.Security.Claims;
using Blueprint.Authorisation;

namespace Blueprint.Tests;

public class TestUserAuthorisationContext : IUserAuthorisationContext, IClaimsHolder
{
    public List<Claim> Claims { get; set; } = new List<Claim>();

    public bool IsActive { get; set; } = true;

    public bool IsAnonymous { get; set; } = false;

    public string Id => Claims.Find(c => c.Type == "sub")?.Value ?? "TestUser123";

    public string AccountId { get; set; } = "TestAccount456";

    public string Email { get; set; }

    public string Name { get; }

    public void PopulateMetadata(Action<string, object> add)
    {
    }

    public IEnumerable<Claim> GetClaimsByValueType(string valueType)
    {
        return Claims.FindAll(c => c.ValueType == valueType);
    }
}