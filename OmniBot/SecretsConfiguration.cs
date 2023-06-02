using System.Text.Json;

namespace OmniBot;

/*
 *      GOODEVENING AWS
 *   FROM CAPTAIN MIDNIGHT
 *      $0.40/SECRET  ?
 *         NO WAY  !
 * (AZURE/HCP VAULT BEWARE!)
 */

public class SecretsConfigurationProvider : ConfigurationProvider
{
    private readonly string _secretName;

    public SecretsConfigurationProvider(string secretName)
    {
        _secretName = secretName;
    }

    public override void Load()
    {
        // Only use this provider if we're in production
        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development") return;

        var secretString = Environment.GetEnvironmentVariable(_secretName);

        if (secretString == null) throw new ArgumentNullException($"Secret {_secretName} not found");

        // Secrets Manager returns a JSON string of key/value pairs
        var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(secretString)!;

        // Add the key/value pairs to the configuration
        foreach (var (key, value) in secrets) Data.Add($"{key}", value);
    }
}

public class SecretsConfigurationSource : IConfigurationSource
{
    private readonly string _secretName;

    public SecretsConfigurationSource(string secretName)
    {
        _secretName = secretName;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretsConfigurationProvider(_secretName);
    }
}

public static class SecretsConfigurationExtensions
{
    public static IConfigurationBuilder AddSecrets(this IConfigurationBuilder builder, string secretName)
    {
        builder.Add(new SecretsConfigurationSource(secretName));
        return builder;
    }
}