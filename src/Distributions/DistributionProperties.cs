namespace Tavenem.Randomize.Distributions;

/// <summary>
/// Gives the minimum, maximum, mean, median, mode(s), and variance of a distribution.
/// </summary>
public readonly struct DistributionProperties
{
    /// <summary>
    /// The exclusive maximum bound of possible values.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// <para>
    /// The mean value of the distribution.
    /// </para>
    /// <para>
    /// May be <see cref="double.NaN"/> if the mean is undefined.
    /// </para>
    /// </summary>
    public double Mean { get; }

    /// <summary>
    /// <para>
    /// The median value of the distribution.
    /// </para>
    /// <para>
    /// May be <see cref="double.NaN"/> if the median is undefined.
    /// </para>
    /// </summary>
    public double Median { get; }

    /// <summary>
    /// The inclusive minimum bound of possible values.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// <para>
    /// The mode(s) of the distribution.
    /// </para>
    /// <para>
    /// May contain <see cref="double.NaN"/> if the mode is undefined.
    /// </para>
    /// </summary>
    public double[] Mode { get; }

    /// <summary>
    /// <para>
    /// The variance of the distribution.
    /// </para>
    /// <para>
    /// May contain <see cref="double.NaN"/> if the variance is undefined.
    /// </para>
    /// </summary>
    public double Variance { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DistributionProperties"/>.
    /// </summary>
    /// <param name="maximum">The exclusive maximum bound of possible values.</param>
    /// <param name="mean">
    /// <para>
    /// The mean value of the distribution.
    /// </para>
    /// <para>
    /// May be <see cref="double.NaN"/> if the mean is undefined.
    /// </para>
    /// </param>
    /// <param name="median">
    /// <para>
    /// The median value of the distribution.
    /// </para>
    /// <para>
    /// May be <see cref="double.NaN"/> if the median is undefined.
    /// </para>
    /// </param>
    /// <param name="minimum">The inclusive minimum bound of possible values.</param>
    /// <param name="mode">
    /// <para>
    /// The mode(s) of the distribution.
    /// </para>
    /// <para>
    /// May contain <see cref="double.NaN"/> if the mode is undefined.
    /// </para>
    /// </param>
    /// <param name="variance">
    /// <para>
    /// The variance of the distribution.
    /// </para>
    /// <para>
    /// May contain <see cref="double.NaN"/> if the variance is undefined.
    /// </para>
    /// </param>
    public DistributionProperties(
        double maximum,
        double mean,
        double median,
        double minimum,
        double[] mode,
        double variance)
    {
        Maximum = maximum;
        Mean = mean;
        Median = median;
        Minimum = minimum;
        Mode = mode ?? new[] { double.NaN };
        Variance = variance;
    }
}
