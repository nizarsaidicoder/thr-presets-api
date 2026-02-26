namespace ThrPresetsApi.Api.Common.Utils;

public static class RankingUtils
{
    /// <summary>
    /// Calculates the lower bound of Wilson score confidence interval for a Bernoulli parameter.
    /// Provides a balanced ranking score based on positive ratings and volume.
    /// </summary>
    public static double CalculateWilsonScore(int positiveRatings, int totalRatings)
    {
        if (totalRatings == 0) return 0;

        // 95% confidence interval
        const double z = 1.96;
        var p = (double)positiveRatings / totalRatings;

        var left = p + (z * z) / (2 * totalRatings);
        var right = z * Math.Sqrt((p * (1 - p) + (z * z) / (4 * totalRatings)) / totalRatings);
        var under = 1 + (z * z) / totalRatings;

        return (left - right) / under;
    }
}