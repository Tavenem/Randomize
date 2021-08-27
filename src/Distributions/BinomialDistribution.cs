using Tavenem.Randomize.Generators;

namespace Tavenem.Randomize.Distributions;

/// <summary>
/// <para>
/// Produces pseudo-random numbers in a binomial distribution.
/// </para>
/// <para>
/// If a single trial is generated (the default), gives the Bernoulli distribution.
/// </para>
/// </summary>
public static class BinomialDistribution
{
    /// <summary>
    /// Gets the properties of this distribution, including minimum, maximum, mean, median,
    /// mode(s), and variance.
    /// </summary>
    /// <param name="n">
    /// <para>
    /// The sample size (number of trials). [0, <see cref="uint.MaxValue"/>]
    /// </para>
    /// <para>
    /// The default of 1 trial gives the Bernoulli distribution.
    /// </para>
    /// </param>
    /// <param name="p">
    /// <para>
    /// The normalized probability of an individual success. [0, 1]
    /// </para>
    /// <para>
    /// This value will be truncated to a valid value if it exceeds the allowable bounds.
    /// </para>
    /// </param>
    /// <returns>The properties of this distribution.</returns>
    public static DistributionProperties GetDistributionProperties(uint n = 1, double p = 0.5)
    {
        p = p.Clamp(0, 1);
        return new DistributionProperties(
                maximum: n,
                mean: n * p,
                median: double.NaN,
                minimum: 0,
                mode: new[] { Math.Floor(p * (n + 1)) },
                variance: p * (1 - p) * n);
    }

    /// <summary>
    /// Enumerates sample values from this distribution.
    /// </summary>
    /// <param name="generator">A pseudo-random number generator used to generate
    /// values.</param>
    /// <param name="numberOfSamples">
    /// <para>
    /// The number of sample values to generate. This parameter ensures that operations like
    /// <see cref="Enumerable.ToList"/> will not cause an overflow, by preventing an
    /// infinite enumeration. If more than <see cref="int.MaxValue"/> samples are required, this
    /// method can be called again to "refresh" the count.
    /// </para>
    /// <para>
    /// Values less than zero will be treated as zero.
    /// </para>
    /// </param>
    /// <param name="n">
    /// <para>
    /// The sample size (number of trials). [0, <see cref="uint.MaxValue"/>]
    /// </para>
    /// <para>
    /// The default of 1 trial gives the Bernoulli distribution.
    /// </para>
    /// </param>
    /// <param name="p">
    /// <para>
    /// The normalized probability of an individual success. [0, 1]
    /// </para>
    /// <para>
    /// This value will be truncated to a valid value if it exceeds the allowable bounds.
    /// </para>
    /// </param>
    /// <returns>An <see cref="IEnumerable{T}"/> of sample values from this
    /// distribution.</returns>
    public static IEnumerable<uint> Samples(RandomNumberGenerator? generator = null, int numberOfSamples = 1, uint n = 1, double p = 0.5)
    {
        generator ??= new RandomNumberGenerator();
        var c = 0;
        while (c++ < numberOfSamples)
        {
            yield return Generate(generator, n, p);
        }
    }

    private static uint Generate(RandomNumberGenerator generator, uint n, double p)
    {
        var successes = 0U;
        for (var i = 0; i < n; i++)
        {
            if (generator.NextDouble() <= p)
            {
                successes++;
            }
        }
        return successes;
    }
}
