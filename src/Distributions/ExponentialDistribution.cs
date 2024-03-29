﻿using Tavenem.Randomize.Generators;

namespace Tavenem.Randomize.Distributions;

/// <summary>
/// Produces pseudo-random numbers in an exponential distribution.
/// </summary>
public static class ExponentialDistribution
{
    private static readonly double[] _Mode = new[] { 0.0 };

    /// <summary>
    /// Gets the properties of this distribution, including minimum, maximum, mean, median,
    /// mode(s), and variance.
    /// </summary>
    /// <param name="lambda">
    /// <para>
    /// The parameter of the distribution (rate parameter). (0, ∞)
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
    public static DistributionProperties GetDistributionProperties(double lambda = 1)
    {
        if (double.IsNaN(lambda))
        {
            return new DistributionProperties(
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    null,
                    double.NaN);
        }
        lambda = Math.Max(NumberValues.NearlyZeroDouble, lambda);
        return new DistributionProperties(
                double.PositiveInfinity,
                1 / lambda,
                DoubleConstants.Ln2 / lambda,
                0,
                _Mode,
                Math.Pow(lambda, -2));
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
    /// <param name="lambda">
    /// <para>
    /// The parameter of the distribution (rate parameter). (0, ∞)
    /// </para>
    /// <para>
    /// If <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If less than or equal to zero, will be set to the smallest value recognized as greater
    /// than zero in this library (<see cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <param name="maximum">
    /// <para>
    /// A maximum value. Does not affect the shape of the function, but results greater than
    /// this value will not be generated. [0, ∞)
    /// </para>
    /// <para>
    /// Values less than 0 are treated as 0.
    /// </para>
    /// </param>
    /// <returns>An <see cref="IEnumerable{T}"/> of sample values from this
    /// distribution.</returns>
    public static IEnumerable<double> Samples(
        RandomNumberGenerator? generator = null,
        int numberOfSamples = 1,
        double lambda = 1,
        double? maximum = null)
    {
        generator ??= new RandomNumberGenerator();
        var c = 0;
        if (double.IsNaN(lambda))
        {
            while (c++ < numberOfSamples)
            {
                yield return double.NaN;
            }
        }
        else if (maximum.HasValue && maximum.Value.IsNearlyZero())
        {
            while (c++ < numberOfSamples)
            {
                yield return 0.0;
            }
        }
        else
        {
            lambda = Math.Max(NumberValues.NearlyZeroDouble, lambda);
            if (maximum.HasValue)
            {
                maximum = Math.Max(0, maximum.Value);
            }
            while (c++ < numberOfSamples)
            {
                double v;
                do
                {
                    v = Generate(generator, lambda);
                } while (maximum.HasValue && v > maximum.Value);
                yield return v;
            }
        }
    }

    private static double Generate(RandomNumberGenerator generator, double lambda)
    {
        double u;
        do
        {
            u = generator.NextDouble();
        } while (u.IsNearlyZero());
        return -Math.Log(u) / lambda;
    }
}
