using Microsoft.Extensions.Configuration;

namespace TicketManagement.Infrastructure.FeatureFlags;

/// <summary>
/// ✅ NEW: Feature flag service for gradual rollouts and A/B testing
/// </summary>
public interface IFeatureFlagService
{
    bool IsEnabled(string featureName);
    bool IsEnabledForUser(string featureName, int userId);
}

public sealed class FeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;

    public FeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsEnabled(string featureName)
    {
        var flagValue = _configuration[$"FeatureFlags:{featureName}"];
        return bool.TryParse(flagValue, out var isEnabled) && isEnabled;
    }

    public bool IsEnabledForUser(string featureName, int userId)
    {
        // Check if feature is globally enabled
        if (!IsEnabled(featureName))
            return false;

        // Check if user is in rollout percentage
        var rolloutPercentage = _configuration.GetValue<int>($"FeatureFlags:{featureName}:RolloutPercentage", 100);
        
        if (rolloutPercentage >= 100)
            return true;

        // Use user ID for consistent hashing
        var hash = Math.Abs(userId.GetHashCode()) % 100;
        return hash < rolloutPercentage;
    }
}
