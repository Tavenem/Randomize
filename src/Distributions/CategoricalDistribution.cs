using Tavenem.Randomize.Generators;

namespace Tavenem.Randomize.Distributions;

/// <summary>
/// Produces pseudo-random numbers in a categorical (discrete) distribution, possibly weighted.
/// </summary>
public static class CategoricalDistribution
{
    /// <summary>
    /// Gets the properties of this distribution, including minimum, maximum, mean, median,
    /// mode(s), and variance.
    /// </summary>
    /// <param name="k">
    /// <para>
    /// The number of equal-weight categories. (0, <see cref="int.MaxValue"/>].
    /// </para>
    /// <para>
    /// A value less than or equal to zero will be treated as a 1.
    /// </para>
    /// </param>
    /// <returns>The properties of this distribution.</returns>
    public static DistributionProperties GetDistributionProperties(int k)
    {
        k = Math.Max(1, k);
        var weight = 1.0 / k;
        return GetDistributionProperties(Enumerable.Repeat(weight, k).ToList());
    }

    /// <summary>
    /// Gets the properties of this distribution, including minimum, maximum, mean, median,
    /// mode(s), and variance.
    /// </summary>
    /// <param name="weights">
    /// <para>
    /// The normalized probability vector of the categorical distribution.
    /// </para>
    /// <para>
    /// Values do not need to be pre-normalized. They will be normalized if necessary.
    /// </para>
    /// <para>
    /// Any weights which are negative are treated as 0.
    /// </para>
    /// <para>
    /// If <see langword="null"/> or empty, a default set of 3 equal weights will be used.
    /// </para>
    /// </param>
    /// <returns>The properties of this distribution.</returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="weights"/> collection sums to zero.
    /// </exception>
    public static DistributionProperties GetDistributionProperties(IEnumerable<double>? weights = null)
    {
        if (weights?.Any() != true)
        {
            return GetDistributionProperties(3);
        }
        return GetProperties(weights!).properties;
    }

    /// <summary>
    /// Enumerates sample values from this distribution.
    /// </summary>
    /// <param name="k">
    /// <para>
    /// The number of equal-weight categories. [1, <see cref="int.MaxValue"/>].
    /// </para>
    /// <para>
    /// A value less than or equal to zero will be treated as a 1.
    /// </para>
    /// </param>
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
    /// <returns>An <see cref="IEnumerable{T}"/> of sample values from this
    /// distribution.</returns>
    public static IEnumerable<int> Samples(int k, RandomNumberGenerator? generator = null, int numberOfSamples = 1)
    {
        k = Math.Max(1, k);
        var weight = 1.0 / k;
        return Samples(generator, numberOfSamples, Enumerable.Repeat(weight, k).ToList());
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
    /// <param name="weights">
    /// <para>
    /// The normalized probability vector of the categorical distribution.
    /// </para>
    /// <para>
    /// Values do not need to be pre-normalized. They will be normalized if necessary.
    /// </para>
    /// <para>
    /// Any weights which are negative are treated as 0.
    /// </para>
    /// <para>
    /// If <see langword="null"/> or empty, a default set of 3 equal weights will be used.
    /// </para>
    /// </param>
    /// <returns>An <see cref="IEnumerable{T}"/> of sample values from this
    /// distribution.</returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="weights"/> collection sums to zero.
    /// </exception>
    public static IEnumerable<int> Samples(RandomNumberGenerator? generator = null, int numberOfSamples = 1, IEnumerable<double>? weights = null)
    {
        if (weights?.Any() != true)
        {
            return Samples(3, generator, numberOfSamples);
        }
        return Samples(generator ?? new RandomNumberGenerator(), numberOfSamples, GetProperties(weights!).cumulativeDistribution);
    }

    private static int Generate(RandomNumberGenerator generator, double[] cumulativeDistributionFunction)
    {
        var u = generator.NextDouble();
        var minIndex = 0;
        var maxIndex = cumulativeDistributionFunction.Length - 1;
        while (minIndex < maxIndex)
        {
            var index = ((maxIndex - minIndex) / 2) + minIndex;
            var c = cumulativeDistributionFunction[index];
            if (u.IsNearlyEqualTo(c))
            {
                minIndex = index;
                break;
            }
            if (u < c)
            {
                maxIndex = index;
            }
            else
            {
                minIndex = index + 1;
            }
        }
        return minIndex;
    }

    private static (DistributionProperties properties, double[] cumulativeDistribution) GetProperties(IEnumerable<double> weights)
    {
        var totalWeight = weights.Sum();
        if (totalWeight.IsNearlyZero())
        {
            throw new ArgumentException(ErrorMessages.TotalWeightIsZero, nameof(weights));
        }
        var normalizedWeights = weights.ToArray();
        var cumulativeDistributionFunction = new double[normalizedWeights.Length];

        if (!totalWeight.IsNearlyEqualTo(1))
        {
            for (var i = 0; i < normalizedWeights.Length; ++i)
            {
                normalizedWeights[i] = Math.Max(0, normalizedWeights[i]) / totalWeight;
            }
        }

        totalWeight = 0;
        var mean = -1.0;
        var maxWeight = 0.0;
        var maxWeightIndex = 0;
        for (var i = 0; i < normalizedWeights.Length; ++i)
        {
            var w = normalizedWeights[i];
            totalWeight += w;
            cumulativeDistributionFunction[i] = totalWeight;
            mean += w * (i + 1.0);
            if (w > maxWeight)
            {
                maxWeight = w;
                maxWeightIndex = i;
            }
        }

        var halfTotalWeight = totalWeight / 2;
        var median = double.NaN;
        var variance = -1.0;
        for (var i = 0; i < normalizedWeights.Length; ++i)
        {
            if (double.IsNaN(median) && cumulativeDistributionFunction[i] >= halfTotalWeight)
            {
                median = i;
            }
            variance += normalizedWeights[i] * (i + 1 - mean).Square();
        }

        return (
            new DistributionProperties(
                normalizedWeights.Length - 1,
                mean,
                median,
                0,
                new double[] { maxWeightIndex },
                variance),
            cumulativeDistributionFunction);
    }

    private static IEnumerable<int> Samples(RandomNumberGenerator generator, int numberOfSamples, double[] cumulativeDistributionFunction)
    {
        var c = 0;
        while (c++ < numberOfSamples)
        {
            yield return Generate(generator, cumulativeDistributionFunction);
        }
    }
}
