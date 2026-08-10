using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LaboratoryJournal.Models;
using LaboratoryJournal.Options;
using LaboratoryJournal.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace LaboratoryJournal.Tests;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_AddsUserAndRoleClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "test-secret-with-at-least-thirty-two-characters",
            Issuer = "LaboratoryJournal.Tests",
            Audience = "LaboratoryJournal.Tests.Client",
            ExpirationMinutes = 30
        });
        var generator = new JwtTokenGenerator(options);
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "researcher@example.com",
            FullName = "Test Researcher",
            Position = "Engineer"
        };

        var token = generator.GenerateToken(user, ["Researcher"]);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == user.Id);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == user.Email);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Researcher");
    }

    [Fact]
    public void GenerateToken_UsesConfiguredLifetime()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "test-secret-with-at-least-thirty-two-characters",
            Issuer = "LaboratoryJournal.Tests",
            Audience = "LaboratoryJournal.Tests.Client",
            ExpirationMinutes = 15
        });
        var generator = new JwtTokenGenerator(options);
        var beforeGeneration = DateTime.UtcNow;

        var token = generator.GenerateToken(
            new ApplicationUser { Id = "user-1", FullName = "Test Researcher" },
            []);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(15);
        Assert.InRange(jwt.ValidTo, expectedExpiration.AddSeconds(-2), expectedExpiration.AddSeconds(2));
    }
}
