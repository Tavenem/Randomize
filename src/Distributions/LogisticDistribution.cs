﻿using Tavenem.Randomize.Generators;

namespace Tavenem.Randomize.Distributions;

/// <summary>
/// Produces pseudo-random numbers in a logistic distribution.
/// </summary>
public static class LogisticDistribution
{
    /// <summary>
    /// Gets the properties of this distribution, including minimum, maximum, mean, median,
    /// mode(s), and variance.
    /// </summary>
    /// <param name="mu">
    /// <para>
    /// The location (mean) of the distribution.
    /// </para>
    /// <para>
    /// If <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// </param>
    /// <param name="sigma">
    /// <para>
    /// The scale of the distribution. (0, ∞)
    /// </para>
    /// <para>
    /// If <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If less than or equal to zero, will be set to the smallest value recognized as greater
    /// than zero in this library (<see cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <returns>The properties of this distribution.</returns>
    public static DistributionProperties GetDistributionProperties(double mu = 0, double sigma = 1)
    {
        if (double.IsNaN(mu)
            || double.IsNaN(sigma))
        {
            return new DistributionProperties(
                double.NaN,
                double.NaN,
                double.NaN,
                double.NaN,
                null,
                double.NaN);
        }
        sigma = Math.Max(NumberValues.NearlyZeroDouble, sigma);
        return new DistributionProperties(
            double.PositiveInfinity,
            mu,
            mu,
            double.NegativeInfinity,
            new[] { mu },
            sigma.Square() * DoubleConstants.PiSquared / 3);
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
    /// <param name="mu">
    /// <para>
    /// The location (mean) of the distribution.
    /// </para>
    /// <para>
    /// If <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// </param>
    /// <param name="sigma">
    /// <para>
    /// The scale of the distribution. (0, ∞)
    /// </para>
    /// <para>
    /// If <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If less than or equal to zero, will be set to the smallest value recognized as greater
    /// than zero in this library (<see cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <param name="minimum">
    /// <para>
    /// A minimum value. Does not affect the shape of the function, but results less than this
    /// value will not be generated.
    /// </para>
    /// </param>
    /// <param name="maximum">
    /// <para>
    /// A maximum value. Does not affect the shape of the function, but results greater than
    /// this value will not be generated.
    /// </para>
    /// </param>
    /// <returns>An <see cref="IEnumerable{T}"/> of sample values from this
    /// distribution.</returns>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by <see
    /// cref="RandomizeOptions.InvalidFloatingRangeResult" />.
    /// </remarks>
    public static IEnumerable<double> Samples(
        RandomNumberGenerator? generator = null,
        int numberOfSamples = 1,
        double mu = 0,
        double sigma = 1,
        double? minimum = null,
        double? maximum = null)
    {
        generator ??= new RandomNumberGenerator();
        var c = 0;
        if (double.IsNaN(mu)
            || double.IsNaN(sigma))
        {
            while (c++ < numberOfSamples)
            {
                yield return double.NaN;
            }
        }
        else if (minimum.HasValue
            && maximum.HasValue
            && minimum.Value.IsNearlyEqualTo(maximum.Value))
        {
            while (c++ < numberOfSamples)
            {
                yield return minimum.Value;
            }
        }
        else
        {
            sigma = Math.Max(NumberValues.NearlyZeroDouble, sigma);
            if (minimum.HasValue
                && maximum.HasValue
                && minimum.Value > maximum.Value)
            {
                switch (RandomizeOptions.InvalidFloatingRangeResult)
                {
                    case InvalidFloatingRangeResultOption.MinBound:
                        maximum = minimum;
                        break;
                    case InvalidFloatingRangeResultOption.Zero:
                        maximum = 0;
                        minimum = 0;
                        break;
                    case InvalidFloatingRangeResultOption.MaxBound:
                        minimum = maximum;
                        break;
                    case InvalidFloatingRangeResultOption.Swap:
                        (minimum, maximum) = (maximum, minimum);
                        break;
                    case InvalidFloatingRangeResultOption.Exception:
                        throw new ArgumentOutOfRangeException(nameof(minimum), ErrorMessages.MinAboveMax);
                    case InvalidFloatingRangeResultOption.NaN:
                        while (c++ < numberOfSamples)
                        {
                            yield return double.NaN;
                        }
                        yield break;
                }
            }
            while (c++ < numberOfSamples)
            {
                double v;
                do
                {
                    v = Generate(generator, mu, sigma);
                }
                while ((minimum.HasValue && v < minimum.Value)
                    || (maximum.HasValue && v > maximum.Value));
                yield return v;
            }
        }
    }

    private static double Generate(
        RandomNumberGenerator generator,
        double mu,
        double sigma)
    {
        double u;
        do
        {
            u = generator.NextDouble();
        } while ((u * (1 - u)).IsNearlyZero());
        return mu + (sigma * Math.Log(u / (1 - u)));
    }
}
