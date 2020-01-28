using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blueprint.Core.Authorisation;

namespace Blueprint.Tests.Api
{
    public class TestUserAuthorisationContext : IUserAuthorisationContext, IClaimsHolder
    {
        public List<Claim> Claims { get; set; } = new List<Claim>();

        public bool IsActive { get; set; } = true;

        public bool IsAnonymous { get; set; } = false;

        public string Id => Claims.Find(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "TestUser123";

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
}
