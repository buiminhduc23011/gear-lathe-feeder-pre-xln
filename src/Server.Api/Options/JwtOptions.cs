namespace Server.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "GearLatheFeeder.Server";

    public string Audience { get; set; } = "GearLatheFeeder.Web";

    public string SigningKey { get; set; } = "GearLatheFeeder.DevSigningKey.2026.03.23.ReplaceInProduction";

    public int ExpirationMinutes { get; set; } = 480;
}
