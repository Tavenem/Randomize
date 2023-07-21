namespace Tavenem.Randomize.Distributions;

/// <summary>
/// Gives the minimum, maximum, mean, median, mode(s), and variance of a distribution.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="DistributionProperties"/>.
/// </remarks>
/// <param name="Maximum">The exclusive maximum bound of possible values.</param>
/// <param name="Mean">
/// <para>
/// The mean value of the distribution.
/// </para>
/// <para>
/// May be <see cref="double.NaN"/> if the mean is undefined.
/// </para>
/// </param>
/// <param name="Median">
/// <para>
/// The median value of the distribution.
/// </para>
/// <para>
/// May be <see cref="double.NaN"/> if the median is undefined.
/// </para>
/// </param>
/// <param name="Minimum">The inclusive minimum bound of possible values.</param>
/// <param name="Mode">
/// <para>
/// The mode(s) of the distribution.
/// </para>
/// <para>
/// May contain <see cref="double.NaN"/> if the mode is undefined.
/// </para>
/// </param>
/// <param name="Variance">
/// <para>
/// The variance of the distribution.
/// </para>
/// <para>
/// May be <see cref="double.NaN"/> if the variance is undefined.
/// </para>
/// </param>
public readonly record struct DistributionProperties(
    double Maximum,
    double Mean,
    double Median,
    double Minimum,
    IReadOnlyList<double>? Mode,
    double Variance)
{
    /// <summary>
    /// <para>
    /// The mode(s) of the distribution.
    /// </para>
    /// <para>
    /// May contain <see cref="double.NaN"/> if the mode is undefined.
    /// </para>
    /// </summary>
    public IReadOnlyList<double> Mode { get; init; } = Mode
        ?? new List<double>() { double.NaN }.AsReadOnly();
}
