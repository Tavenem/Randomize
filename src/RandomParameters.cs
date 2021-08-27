using Tavenem.Randomize.Distributions;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Tavenem.Randomize;

/// <summary>
/// A set of rules for generating random values.
/// </summary>
[JsonConverter(typeof(RandomParametersConverter))]
public readonly struct RandomParameters :
    IEqualityOperators<RandomParameters, RandomParameters>,
    ISpanFormattable,
    ISpanParseable<RandomParameters>
{
    /// <summary>
    /// Default parameters for a binomial distribution, in which the sample size is 1 and the
    /// probability of an individual success is 0.5.
    /// </summary>
    public static RandomParameters Binomial { get; } = NewBinomial(new[] { 1, 0.5 });

    /// <summary>
    /// Default parameters for a continuous, uniform distribution between 0 (inclusive) and 1
    /// (exclusive).
    /// </summary>
    public static RandomParameters Default { get; } = DefaultContinuousUniform;

    /// <summary>
    /// Default parameters for an unweighted categorical (discrete) distribution.
    /// </summary>
    public static RandomParameters DefaultCategorical { get; } = NewCategorical();

    /// <summary>
    /// Default parameters for a continuous, uniform distribution between 0 (inclusive) and 1
    /// (exclusive).
    /// </summary>
    public static RandomParameters DefaultContinuousUniform { get; } = NewContinuousUniform();

    /// <summary>
    /// Default parameters for a uniform distribution of signed 32-bit integers.
    /// </summary>
    public static RandomParameters DefaultDiscreteUniformSigned { get; } = NewDiscreteUniformSigned();

    /// <summary>
    /// Default parameters for a uniform distribution of unsigned 32-bit integers.
    /// </summary>
    public static RandomParameters DefaultDiscreteUniformUnsigned { get; } = NewDiscreteUniformUnsigned();

    /// <summary>
    /// Default parameters for an exponential distribution whose parameter (lambda) is 1.
    /// </summary>
    public static RandomParameters DefaultExponential { get; } = NewExponential(parameters: new[] { 1.0 });

    /// <summary>
    /// Default parameters for a logistic distribution whose location (mean, mu) is 0 and whose
    /// scale (sigma) is 1.
    /// </summary>
    public static RandomParameters DefaultLogistic { get; } = NewLogistic(parameters: new[] { 0.0, 1.0 });

    /// <summary>
    /// Default parameters for a log-normal distribution whose location (mean, mu) is 0 and
    /// whose scale (sigma) is 1.
    /// </summary>
    public static RandomParameters DefaultLogNormal { get; } = NewLogNormal(parameters: new[] { 0.0, 1.0 });

    /// <summary>
    /// Default parameters for a normal distribution whose location (mean, mu) is 0 and whose
    /// standard deviation (sigma) is 1.
    /// </summary>
    public static RandomParameters DefaultNormal { get; } = NewNormal(parameters: new[] { 0.0, 1.0 });

    /// <summary>
    /// Default parameters for the positive half of a normal distribution whose location (mean,
    /// mu) is 0 and whose standard deviation (sigma) is 1.
    /// </summary>
    public static RandomParameters DefaultPositiveNormal { get; } = NewPositiveNormal(parameters: new[] { 0.0, 1.0 });

    /// <summary>
    /// A distribution which always returns zero (as an <see cref="int"/>).
    /// </summary>
    public static RandomParameters Zero { get; } = NewFixedInt32(0);

    /// <summary>
    /// The type of distribution in which the values are generated.
    /// </summary>
    public DistributionType DistributionType { get; }

    /// <summary>
    /// The maximum allowable value.
    /// </summary>
    public double? Maximum { get; }

    /// <summary>
    /// The minimum allowable value.
    /// </summary>
    public double? Minimum { get; }

    /// <summary>
    /// The parameters for the distribution. The number depends on the distribution type.
    /// </summary>
    public double[]? Parameters { get; }

    /// <summary>
    /// The number of decimal places to which non-integral values should be rounded.
    /// </summary>
    public byte? Precision { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RandomParameters"/>.
    /// </summary>
    /// <param name="distributionType">The type of distribution in which the values are
    /// generated.</param>
    /// <param name="minimum">The minimum allowable value.</param>
    /// <param name="maximum">The maximum allowable value.</param>
    /// <param name="parameters">The parameters for the distribution. The number depends on the
    /// distribution type.</param>
    /// <param name="precision">The number of decimal places to which non-integral values should
    /// be rounded.</param>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by either <see
    /// cref="RandomizeOptions.InvalidIntegralRangeResult"/> or <see
    /// cref="RandomizeOptions.InvalidFloatingRangeResult" />, depending on the distribution type.
    /// </remarks>
    public RandomParameters(
        DistributionType distributionType = DistributionType.ContinuousUniform,
        double? minimum = null,
        double? maximum = null,
        double[]? parameters = null,
        byte? precision = null)
    {
        DistributionType = distributionType;
        Maximum = maximum;
        Minimum = minimum;
        Parameters = parameters;
        Precision = precision;
    }

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a binomial distribution.
    /// </summary>
    /// <param name="parameters">An array whose first value is the sample size (N), and whose
    /// second is the normalized probability of an individual success. [0, 1].</param>
    public static RandomParameters NewBinomial(double[]? parameters = null)
        => new(DistributionType.Binomial, parameters: parameters);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a categorical distribution.
    /// </summary>
    /// <param name="parameters">
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
    public static RandomParameters NewCategorical(double[]? parameters = null)
        => new(DistributionType.Categorical, parameters: parameters);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a continuous, uniform
    /// distribution (i.e. real numbers).
    /// </summary>
    /// <param name="minimum">The inclusive lower bound.</param>
    /// <param name="maximum">The exclusive upper bound.</param>
    /// <param name="precision">The number of decimal places to which non-integral values should be rounded.</param>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by <see
    /// cref="RandomizeOptions.InvalidFloatingRangeResult" />.
    /// </remarks>
    public static RandomParameters NewContinuousUniform(
        double minimum = 0,
        double maximum = 1,
        byte? precision = null)
        => new(DistributionType.ContinuousUniform, minimum, maximum, precision: precision);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a uniform distribution of
    /// signed 32-bit integers.
    /// </summary>
    /// <param name="minimum">The inclusive lower bound.</param>
    /// <param name="maximum">The inclusive upper bound.</param>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by <see
    /// cref="RandomizeOptions.InvalidIntegralRangeResult" />.
    /// </remarks>
    public static RandomParameters NewDiscreteUniformSigned(
        double? minimum = null,
        double? maximum = null)
        => new(DistributionType.DiscreteUniformSigned, minimum, maximum);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a uniform distribution of
    /// unsigned 32-bit integers.
    /// </summary>
    /// <param name="minimum">The inclusive lower bound.</param>
    /// <param name="maximum">The inclusive upper bound.</param>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by <see
    /// cref="RandomizeOptions.InvalidIntegralRangeResult" />.
    /// </remarks>
    public static RandomParameters NewDiscreteUniformUnsigned(
        double? minimum = null,
        double? maximum = null)
        => new(DistributionType.DiscreteUniformUnsigned, minimum, maximum);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for an exponential distribution.
    /// </summary>
    /// <param name="maximum">
    /// <para>
    /// A maximum value. Does not affect the shape of the function, but results greater than
    /// this value will not be generated.
    /// </para>
    /// </param>
    /// <param name="parameters">
    /// <para>
    /// An array whose first value is the parameter of the distribution (rate parameter, lambda:
    /// (0, ∞)).
    /// </para>
    /// <para>
    /// If <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If less than or equal to zero, will be set to the smallest value recognized as greater
    /// than zero in this library (<see cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <param name="precision">The number of decimal places to which non-integral values should
    /// be rounded.</param>
    public static RandomParameters NewExponential(
        double? maximum = null,
        double[]? parameters = null,
        byte? precision = null)
        => new(DistributionType.Exponential, maximum: maximum, parameters: parameters, precision: precision);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a distribution which always
    /// returns a fixed <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    public static RandomParameters NewFixedInt32(int value)
        => new(DistributionType.DiscreteUniformSigned, value, value);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a distribution which always
    /// returns a fixed <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    public static RandomParameters NewFixedUInt32(uint value)
        => new(DistributionType.DiscreteUniformUnsigned, value, value);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a distribution which always
    /// returns a fixed real value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <param name="precision">The number of decimal places to which non-integral values should be rounded.</param>
    public static RandomParameters NewFixedReal(double value, byte? precision = null)
        => new(DistributionType.ContinuousUniform, value, value, precision: precision);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a logistic distribution.
    /// </summary>
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
    /// <param name="parameters">
    /// <para>
    /// An array whose first value is the location (mean) of the distribution (mu), and whose
    /// second is the scale of the distribution (sigma: (0, ∞)).
    /// </para>
    /// <para>
    /// If either is <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If sigma is less than or equal to zero, it will be set to the smallest value recognized
    /// as greater than zero in this library (<see
    /// cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <param name="precision">The number of decimal places to which non-integral values should
    /// be rounded.</param>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by <see
    /// cref="RandomizeOptions.InvalidFloatingRangeResult" />.
    /// </remarks>
    public static RandomParameters NewLogistic(
        double? minimum = null,
        double? maximum = null,
        double[]? parameters = null,
        byte? precision = null)
        => new(DistributionType.Logistic, minimum, maximum, parameters, precision);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a log-normal distribution.
    /// </summary>
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
    /// <param name="parameters">
    /// <para>
    /// An array whose first value is the location (mean) of the distribution (mu), and whose
    /// second is the scale of the distribution (sigma: (0, ∞)).
    /// </para>
    /// <para>
    /// If either is <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If sigma is less than or equal to zero, it will be set to the smallest value recognized
    /// as greater than zero in this library (<see
    /// cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <param name="precision">The number of decimal places to which non-integral values should
    /// be rounded.</param>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by <see
    /// cref="RandomizeOptions.InvalidFloatingRangeResult" />.
    /// </remarks>
    public static RandomParameters NewLogNormal(
        double? minimum = null,
        double? maximum = null,
        double[]? parameters = null,
        byte? precision = null)
        => new(DistributionType.LogNormal, minimum, maximum, parameters, precision);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for a normal distribution.
    /// </summary>
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
    /// <param name="parameters">
    /// <para>
    /// An array whose first value is the location (mean) of the distribution (mu), and whose
    /// second is the standard deviation of the distribution (sigma: (0, ∞)).
    /// </para>
    /// <para>
    /// If either is <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If sigma is less than or equal to zero, it will be set to the smallest value recognized
    /// as greater than zero in this library (<see
    /// cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <param name="precision">The number of decimal places to which non-integral values should
    /// be rounded.</param>
    /// <remarks>
    /// If <paramref name="minimum"/> and <paramref name="maximum"/> are both non-<see
    /// langword="null"/> and <paramref name="minimum" /> is greater than <paramref
    /// name="maximum" />, the result is determined by <see
    /// cref="RandomizeOptions.InvalidFloatingRangeResult" />.
    /// </remarks>
    public static RandomParameters NewNormal(
        double? minimum = null,
        double? maximum = null,
        double[]? parameters = null,
        byte? precision = null)
        => new(DistributionType.Normal, minimum, maximum, parameters, precision);

    /// <summary>
    /// Gets a new instance of <see cref="RandomParameters"/> for the positive half of a normal
    /// distribution.
    /// </summary>
    /// <param name="maximum">
    /// <para>
    /// A maximum value. Does not affect the shape of the function, but results greater than
    /// this value will not be generated.
    /// </para>
    /// </param>
    /// <param name="parameters">
    /// <para>
    /// An array whose first value is the location (mean) of the distribution (mu), and whose
    /// second is the standard deviation of the distribution (sigma: (0, ∞)).
    /// </para>
    /// <para>
    /// If either is <see cref="double.NaN"/>, all results will be <see cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If sigma is less than or equal to zero, it will be set to the smallest value recognized
    /// as greater than zero in this library (<see
    /// cref="NumberValues.NearlyZeroDouble"/>).
    /// </para>
    /// </param>
    /// <param name="precision">The number of decimal places to which non-integral values should
    /// be rounded.</param>
    public static RandomParameters NewPositiveNormal(
        double? maximum = null,
        double[]? parameters = null,
        byte? precision = null)
        => new(DistributionType.PositiveNormal, maximum: maximum, parameters: parameters, precision: precision);

    /// <summary>
    /// Converts the specified string representation to a <see cref="RandomParameters"/>
    /// instance.
    /// </summary>
    /// <param name="s">A string containing a <see cref="RandomParameters"/> instance to
    /// convert.</param>
    /// <returns>The <see cref="RandomParameters"/> value equivalent to the <see
    /// cref="RandomParameters"/> instance contained in <paramref name="s"/>.</returns>
    /// <param name="provider">An object that supplies culture-specific formatting
    /// information.</param>
    /// <exception cref="FormatException"><paramref name="s"/> is empty, or contains only white
    /// space, contains invalid <see cref="RandomParameters"/> data, or the format cannot be
    /// determined.</exception>
    public static RandomParameters Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (s.IsEmpty || s.IsWhiteSpace())
        {
            throw new FormatException();
        }
        if (TryParse(s, provider, out var result))
        {
            return result;
        }
        throw new FormatException();
    }

    /// <summary>
    /// Converts the specified string representation to a <see cref="RandomParameters"/>
    /// instance.
    /// </summary>
    /// <param name="s">A string containing a <see cref="RandomParameters"/> instance to
    /// convert.</param>
    /// <returns>The <see cref="RandomParameters"/> value equivalent to the <see
    /// cref="RandomParameters"/> instance contained in <paramref name="s"/>.</returns>
    /// <param name="provider">An object that supplies culture-specific formatting
    /// information.</param>
    /// <exception cref="FormatException"><paramref name="s"/> is empty, or contains only white
    /// space, contains invalid <see cref="RandomParameters"/> data, or the format cannot be
    /// determined.</exception>
    public static RandomParameters Parse(string? s, IFormatProvider? provider = null)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new FormatException();
        }
        if (TryParse(s.AsSpan(), provider, out var result))
        {
            return result;
        }
        throw new FormatException();
    }

    /// <summary>
    /// Converts the specified string representation to a <see cref="RandomParameters"/>
    /// instance. The format of the string representation must match the specified format
    /// exactly, unless it matches the relative day or year format.
    /// </summary>
    /// <param name="s">A string containing a <see cref="RandomParameters"/> instance to
    /// convert.</param>
    /// <param name="format">A format specifier that defines the required format of <paramref
    /// name="s"/>.</param>
    /// <param name="provider">An object that supplies culture-specific formatting
    /// information.</param>
    /// <returns>The <see cref="RandomParameters"/> value equivalent to the <see
    /// cref="RandomParameters"/> instance contained in <paramref name="s"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> or <paramref
    /// name="format"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="s"/> or <paramref name="format"/> is
    /// an empty string (""), or <paramref name="s"/> contains only white space, contains
    /// invalid <see cref="RandomParameters"/> data, or the format cannot be
    /// determined.</exception>
    public static RandomParameters ParseExact(ReadOnlySpan<char> s, string? format = null, IFormatProvider? provider = null)
    {
        if (s.IsEmpty || s.IsWhiteSpace())
        {
            throw new FormatException();
        }
        if (TryParseExact(s, format, provider, out var result))
        {
            return result;
        }
        throw new FormatException();
    }

    /// <summary>
    /// Converts the specified string representation to a <see cref="RandomParameters"/>
    /// instance. The format of the string representation must match the specified format
    /// exactly, unless it matches the relative day or year format.
    /// </summary>
    /// <param name="s">A string containing a <see cref="RandomParameters"/> instance to
    /// convert.</param>
    /// <param name="format">A format specifier that defines the required format of <paramref
    /// name="s"/>.</param>
    /// <param name="provider">An object that supplies culture-specific formatting
    /// information.</param>
    /// <returns>The <see cref="RandomParameters"/> value equivalent to the <see
    /// cref="RandomParameters"/> instance contained in <paramref name="s"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> or <paramref
    /// name="format"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="s"/> or <paramref name="format"/> is
    /// an empty string (""), or <paramref name="s"/> contains only white space, contains
    /// invalid <see cref="RandomParameters"/> data, or the format cannot be
    /// determined.</exception>
    public static RandomParameters ParseExact(string? s, string? format = null, IFormatProvider? provider = null)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new FormatException();
        }
        if (TryParseExact(s.AsSpan(), format, provider, out var result))
        {
            return result;
        }
        throw new FormatException();
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <see cref="TryParse(ReadOnlySpan{char}, IFormatProvider?, out
    /// RandomParameters)"/> method is similar to the <see cref="Parse(ReadOnlySpan{char},
    /// IFormatProvider)"/> method, except that the <see cref="TryParse(ReadOnlySpan{char},
    /// IFormatProvider, out RandomParameters)"/> method does not throw an exception if the
    /// conversion fails.
    /// </para>
    /// </remarks>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out RandomParameters result)
    {
        result = Default;

        if (s.IsEmpty || s.IsWhiteSpace())
        {
            return false;
        }

        return TryParseMultiple(s, provider, out result);
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <see cref="TryParse(string?, IFormatProvider?, out RandomParameters)"/> method is
    /// similar to the <see cref="Parse(string?, IFormatProvider)"/> method, except that the
    /// <see cref="TryParse(string?, IFormatProvider, out RandomParameters)"/> method does not
    /// throw an exception if the conversion fails.
    /// </para>
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, out RandomParameters result)
    {
        result = Default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        return TryParseMultiple(s, provider, out result);
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> s, out RandomParameters result)
    {
        result = Default;

        if (s.IsEmpty || s.IsWhiteSpace())
        {
            return false;
        }

        return TryParseMultiple(s, null, out result);
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? s, out RandomParameters result)
    {
        result = Default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        return TryParseMultiple(s, null, out result);
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded. The format of the string representation must match the specified
    /// format exactly.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="format">The required format of <paramref name="s"/>.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <see cref="TryParseExact(ReadOnlySpan{char}, string?, IFormatProvider?, out
    /// RandomParameters)"/> method is similar to the <see cref="ParseExact(ReadOnlySpan{char},
    /// string, IFormatProvider)"/> method, except that the <see
    /// cref="TryParseExact(ReadOnlySpan{char}, string, IFormatProvider, out
    /// RandomParameters)"/> method does not throw an exception if the conversion fails.
    /// </para>
    /// <para>
    /// Only recognized formats can be parsed successfully. Even recognized formats may not
    /// round-trip values correctly, unless the format specifically designed to do so is chosen
    /// <seealso cref="ToString(string, IFormatProvider)"/>
    /// </para>
    /// </remarks>
    public static bool TryParseExact(ReadOnlySpan<char> s, string? format, IFormatProvider? provider, out RandomParameters result)
    {
        result = Default;

        if (s.IsEmpty || s.IsWhiteSpace())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(format)
            || string.Equals(format, "g", StringComparison.OrdinalIgnoreCase))
        {
            return TryParseGeneral(s, provider, out result);
        }
        else if (string.Equals(format, "r", StringComparison.OrdinalIgnoreCase))
        {
            return TryParseRoundTrip(s, out result);
        }
        throw new ArgumentException("The provided format is unrecognized", nameof(format));
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded. The format of the string representation must match the specified
    /// format exactly.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="format">The required format of <paramref name="s"/>.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <see cref="TryParseExact(string, string, IFormatProvider, out RandomParameters)"/>
    /// method is similar to the <see cref="ParseExact(string, string, IFormatProvider)"/>
    /// method, except that the <see cref="TryParseExact(string, string, IFormatProvider, out
    /// RandomParameters)"/> method does not throw an exception if the conversion fails.
    /// </para>
    /// <para>
    /// Only recognized formats can be parsed successfully. Even recognized formats may not
    /// round-trip values correctly, unless the format specifically designed to do so is chosen
    /// <seealso cref="ToString(string, IFormatProvider)"/>
    /// </para>
    /// </remarks>
    public static bool TryParseExact(string? s, string? format, IFormatProvider? provider, out RandomParameters result)
    {
        result = Default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        return TryParseExact(s.AsSpan(), format, provider, out result);
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded. The format of the string representation must match the specified
    /// format exactly.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="format">The required format of <paramref name="s"/>.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Only recognized formats can be parsed successfully. Even recognized formats may not
    /// round-trip values correctly, unless the format specifically designed to do so is chosen
    /// <seealso cref="ToString(string, IFormatProvider)"/>
    /// </remarks>
    public static bool TryParseExact(ReadOnlySpan<char> s, string? format, out RandomParameters result)
    {
        result = Default;

        if (s.IsEmpty || s.IsWhiteSpace())
        {
            return false;
        }

        return TryParseExact(s, format, null, out result);
    }

    /// <summary>
    /// Attempts to convert the specified string representation of a <see
    /// cref="RandomParameters"/> instance and returns a value that indicates whether the
    /// conversion succeeded. The format of the string representation must match the specified
    /// format exactly.
    /// </summary>
    /// <param name="s">
    /// A string containing a <see cref="RandomParameters"/> instance to convert.
    /// </param>
    /// <param name="format">The required format of <paramref name="s"/>.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="RandomParameters"/> value equivalent
    /// to the <see cref="RandomParameters"/> instance contained in <paramref name="s"/>, if the
    /// conversion succeeded, or <see cref="Default"/> if the conversion failed. The conversion
    /// fails if the <paramref name="s"/> parameter is <see langword="null"/>, or an empty
    /// string (""), or contains only white space, or contains invalid <see
    /// cref="RandomParameters"/> data. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="s"/> parameter was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Only recognized formats can be parsed successfully. Even recognized formats may not
    /// round-trip values correctly, unless the format specifically designed to do so is chosen
    /// <seealso cref="ToString(string, IFormatProvider)"/>
    /// </remarks>
    public static bool TryParseExact(string? s, string? format, out RandomParameters result)
    {
        result = Default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        return TryParseExact(s.AsSpan(), format, null, out result);
    }

    /// <summary>
    /// Gets a <see cref="RandomParameters"/> instance that represents the combination of this
    /// instance and the <paramref name="other"/> instance.
    /// </summary>
    /// <param name="other">An instance to combine with this one.</param>
    /// <returns>A <see cref="RandomParameters"/> instance that represents the combination of
    /// this instance and the <paramref name="other"/> instance.</returns>
    /// <remarks>
    /// <para>
    /// The largest <see cref="Maximum"/> and lowest <see cref="Minimum"/>
    /// are taken, as well as the highest <see cref="Precision"/>. For each parameter present,
    /// the average of both values is used (or the only value present, if it is missing in
    /// either).
    /// </para>
    /// <para>
    /// If the two distribution types are not the same, the one with the higher enumeration
    /// value is used. However, note that distributions of differing types may have incompatible
    /// parameter lists. This should not cause any errors due to the combination method, but the
    /// results may not make sense for the resulting type.
    /// </para>
    /// </remarks>
    public RandomParameters Combine(RandomParameters other)
    {
        double[]? parameters = null;
        var parameterLength = Math.Max(Parameters?.Length ?? 0, other.Parameters?.Length ?? 0);
        if (parameterLength > 0)
        {
            parameters = new double[parameterLength];
            if (Parameters is null)
            {
                if (!(other.Parameters is null))
                {
                    Array.Copy(other.Parameters, 0, parameters, 0, parameterLength);
                }
            }
            else if (other.Parameters is null)
            {
                Array.Copy(Parameters, 0, parameters, 0, parameterLength);
            }
            else
            {
                for (var i = 0; i < parameterLength; i++)
                {
                    parameters[i] = (Parameters[i] + other.Parameters[i]) / 2.0;
                }
            }
        }
        double? minimum;
        if (Minimum.HasValue)
        {
            if (other.Minimum.HasValue)
            {
                minimum = Math.Min(Minimum.Value, other.Minimum.Value);
            }
            else
            {
                minimum = Minimum.Value;
            }
        }
        else
        {
            minimum = other.Minimum;
        }

        double? maximum;
        if (Maximum.HasValue)
        {
            if (other.Maximum.HasValue)
            {
                maximum = Math.Max(Maximum.Value, other.Maximum.Value);
            }
            else
            {
                maximum = Maximum.Value;
            }
        }
        else
        {
            maximum = other.Maximum;
        }

        byte? precision;
        if (Precision.HasValue)
        {
            if (other.Precision.HasValue)
            {
                precision = Math.Max(Precision.Value, other.Precision.Value);
            }
            else
            {
                precision = Precision.Value;
            }
        }
        else
        {
            precision = other.Precision;
        }

        return new RandomParameters(
            (DistributionType)Math.Max((int)DistributionType, (int)other.DistributionType),
            minimum,
            maximum,
            parameters,
            precision);
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref
    /// name="other">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(RandomParameters other)
        => DistributionType == other.DistributionType
        && Maximum == other.Maximum
        && Minimum == other.Minimum
        && (Parameters is null) == (other.Parameters is null)
        && (Parameters is null
        || ((Parameters.Length == other.Parameters!.Length)
        && Parameters.SequenceEqual(other.Parameters)))
        && Precision == other.Precision;

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj">obj</paramref> and this instance
    /// are the same type and represent the same value; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is RandomParameters other && Equals(other);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(
        DistributionType,
        Maximum,
        Minimum,
        Parameters,
        Precision);

    /// <summary>Returns a string representation of this instance.</summary>
    /// <param name="format">
    /// <para>
    /// May be either "g" ("general") or "r" ("round trip"). Case does not matter.
    /// </para>
    /// <para>
    /// <see langword="null"/> or an empty string is also accepted, and resolves as "g".
    /// </para>
    /// <para>
    /// The general format ("g") produces a string similar to "Normal distribution
    /// (0.00;Infinity) [1.00;0.33] r:3" where the numbers in parentheses indicate the minimum
    /// and maximum (if either is provided), the numbers in brackets are the parameters (if
    /// any), and the value after "r:" is the precision.
    /// </para>
    /// <para>
    /// The round-trip format ("r") produces a string similar to
    /// "9:0.0000000000000000;Infinity:1.0000000000000000;0.3300000000000000:3" where the number
    /// before the first colon indicates the distribution type, the numbers between the first
    /// and second semicolon indicate the range, any parameters appear as a semicolon-delimited
    /// list between the second and third semicolon, and the number after the third semicolon is
    /// the precision. All three semicolons are always present, even if there are no parameters.
    /// The limits of the range and any paramaters are displayed in G17 format (to successfully
    /// round-trip the <see cref="double"/> values). The invariant culture is always used to
    /// format the numbers in the round-trip format, regardless of the <paramref
    /// name="provider"/> supplied, to ensure successful round-tripping across systems.
    /// </para>
    /// </param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <returns>A string representation of this instance.</returns>
    public string ToString(string? format, IFormatProvider? provider = null)
    {
        if (string.IsNullOrWhiteSpace(format)
            || string.Equals(format, "g", StringComparison.OrdinalIgnoreCase))
        {
            return ToStringGeneral(provider).ToString();
        }
        else if (string.Equals(format, "r", StringComparison.OrdinalIgnoreCase))
        {
            return ToStringRoundTrip().ToString();
        }
        throw new ArgumentException("The provided format is unrecognized", nameof(format));
    }

    /// <summary>Returns a string representation of this instance.</summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Attempts to write this instance to the given <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The <see cref="Span{T}"/> to write to.</param>
    /// <param name="charsWritten">
    /// When this method returns, this will contains the number of characters written to <paramref name="destination"/>.
    /// </param>
    /// <param name="format">A format string containing formatting specifications.</param>
    /// <param name="provider">
    /// An object that supplies format information about the current instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this instance was successfully written to the
    /// <paramref name="destination"/>; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;
        StringBuilder sb;
        if (format.IsEmpty
            || format.IsWhiteSpace()
            || format.Equals("g".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            sb = ToStringGeneral(provider);
        }
        else if (format.Equals("r".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            sb = ToStringRoundTrip();
        }
        else
        {
            return false;
        }
        if (destination.Length < sb.Length)
        {
            return false;
        }
        sb.CopyTo(0, destination, sb.Length);
        charsWritten += sb.Length;
        return true;
    }

    private static bool TryParseGeneral(in ReadOnlySpan<char> value, IFormatProvider? provider, out RandomParameters result)
    {
        result = Default;

        provider ??= CultureInfo.CurrentCulture;

        DistributionType distributionType;
        var index = value.IndexOf(" distribution");
        if (index == -1)
        {
            return false;
        }
        var slice = value.Slice(0, index);
        if (!Enum.TryParse(typeof(DistributionType), slice.ToString(), out var distTypeValue)
            || distTypeValue is not DistributionType distributionTypeValue)
        {
            return false;
        }
        else
        {
            distributionType = distributionTypeValue;
        }

        double? min = null;
        double? max = null;
        index = value.IndexOf('(');
        if (index != -1)
        {
            var sepIndex = value.IndexOf(';');
            if (sepIndex == -1 || sepIndex <= index + 1)
            {
                return false;
            }
            slice = value.Slice(index + 1, sepIndex - index - 1);
            if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var minValue))
            {
                return false;
            }
            else if (!double.IsNegativeInfinity(minValue))
            {
                min = minValue;
            }

            index = value.IndexOf(')');
            if (index == -1 || index <= sepIndex + 1)
            {
                return false;
            }
            slice = value.Slice(sepIndex + 1, index - sepIndex - 1);
            if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var maxValue))
            {
                return false;
            }
            else if (!double.IsPositiveInfinity(maxValue))
            {
                max = maxValue;
            }
        }

        var parameters = new List<double>();
        index = value.IndexOf('[');
        if (index != -1)
        {
            var sepIndex = value[(index + 1)..].IndexOf(';');
            while (sepIndex != -1)
            {
                slice = value.Slice(index + 1, sepIndex);
                if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var paramValue))
                {
                    return false;
                }
                else
                {
                    parameters.Add(paramValue);
                    index += sepIndex + 1;
                }
                sepIndex = value[(index + 1)..].IndexOf(';');
            }
            var closeIndex = value[(index + 1)..].IndexOf(']');
            if (closeIndex == -1)
            {
                return false;
            }
            slice = value.Slice(index + 1, closeIndex);
            if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var lastParamValue))
            {
                return false;
            }
            else
            {
                parameters.Add(lastParamValue);
            }
        }

        byte? precision = null;
        index = value.IndexOf("r:");
        if (index != -1)
        {
            slice = value[(index + 2)..];
            if (!byte.TryParse(slice, NumberStyles.Integer, provider, out var precisionValue))
            {
                return false;
            }
            else
            {
                precision = precisionValue;
            }
        }

        result = new RandomParameters(distributionType, min, max, parameters.Count == 0 ? null : parameters.ToArray(), precision);
        return true;
    }

    private static bool TryParseMultiple(in ReadOnlySpan<char> s, IFormatProvider? provider, out RandomParameters result)
    {
        if (TryParseExact(s, "g", provider, out result))
        {
            return true;
        }
        if (TryParseExact(s, "r", provider, out result))
        {
            return true;
        }

        result = Zero;
        return false;
    }

    private static bool TryParseRoundTrip(in ReadOnlySpan<char> value, out RandomParameters result)
    {
        result = Default;

        var provider = CultureInfo.InvariantCulture;

        DistributionType distributionType;
        var index = value.IndexOf(':');
        if (index == -1)
        {
            return false;
        }
        var slice = value.Slice(0, index);
        if (!int.TryParse(slice, out var distributionTypeValue)
            || !Enum.IsDefined(typeof(DistributionType), distributionTypeValue))
        {
            return false;
        }
        else
        {
            distributionType = (DistributionType)distributionTypeValue;
        }

        double? min = null;
        double? max = null;
        var sepIndex = value.IndexOf(';');
        if (sepIndex == -1 || sepIndex <= index + 1)
        {
            return false;
        }
        slice = value.Slice(index + 1, sepIndex - index - 1);
        if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var minValue))
        {
            return false;
        }
        else if (!double.IsNegativeInfinity(minValue))
        {
            min = minValue;
        }

        index = value[(sepIndex + 1)..].IndexOf(':');
        if (index is (-1) or < 1)
        {
            return false;
        }
        slice = value.Slice(sepIndex + 1, index);
        if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var maxValue))
        {
            return false;
        }
        else if (!double.IsPositiveInfinity(maxValue))
        {
            max = maxValue;
        }
        index = sepIndex + index + 1;

        var parameters = new List<double>();
        var paramSepIndex = value[(index + 1)..].IndexOf(';');
        while (paramSepIndex != -1)
        {
            slice = value.Slice(index + 1, paramSepIndex);
            if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var paramValue))
            {
                return false;
            }
            else
            {
                parameters.Add(paramValue);
                index += paramSepIndex + 1;
            }
            paramSepIndex = value[(index + 1)..].IndexOf(';');
        }
        var nextIndex = value[(index + 1)..].IndexOf(':');
        if (index == -1)
        {
            return false;
        }
        slice = value.Slice(index + 1, nextIndex);
        if (!double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var lastParamValue))
        {
            return false;
        }
        else
        {
            parameters.Add(lastParamValue);
            index += nextIndex + 1;
        }

        byte? precision = null;
        if (value.Length > index + 1)
        {
            slice = value[(index + 1)..];
            if (!byte.TryParse(slice, NumberStyles.Integer, provider, out var precisionValue))
            {
                return false;
            }
            else
            {
                precision = precisionValue;
            }
        }

        result = new RandomParameters(distributionType, min, max, parameters.Count == 0 ? null : parameters.ToArray(), precision);
        return true;
    }

    private StringBuilder ToStringGeneral(IFormatProvider? provider)
    {
        var nfi = NumberFormatInfo.GetInstance(provider ?? CultureInfo.CurrentCulture);
        var s = new StringBuilder(DistributionType.ToString())
            .Append(" distribution");
        if (Minimum.HasValue || Maximum.HasValue)
        {
            s.Append(" (");
            if (Minimum.HasValue)
            {
                s.Append(Minimum.Value.ToString("f2", nfi));
            }
            else
            {
                s.Append(nfi.NegativeInfinitySymbol);
            }
            s.Append(';');
            if (Maximum.HasValue)
            {
                s.Append(Maximum.Value.ToString("f2", nfi));
            }
            else
            {
                s.Append(nfi.PositiveInfinitySymbol);
            }
            s.Append(')');
        }
        if (Parameters?.Length > 0)
        {
            s.Append(" [");
            for (var i = 0; i < Parameters.Length; i++)
            {
                if (i != 0)
                {
                    s.Append(';');
                }
                s.Append(Parameters[i].ToString("f2", nfi));
            }
            s.Append(']');
        }
        if (Precision.HasValue)
        {
            s.Append(" r:").Append(Precision.Value.ToString(nfi));
        }
        return s;
    }

    private StringBuilder ToStringRoundTrip()
    {
        var s = new StringBuilder(((int)DistributionType).ToString())
            .Append(':');

        if (Minimum.HasValue)
        {
            s.Append(Minimum.Value.ToString("g17", NumberFormatInfo.InvariantInfo));
        }
        else
        {
            s.Append(NumberFormatInfo.InvariantInfo.NegativeInfinitySymbol);
        }

        s.Append(';');

        if (Maximum.HasValue)
        {
            s.Append(Maximum.Value.ToString("f2", NumberFormatInfo.InvariantInfo));
        }
        else
        {
            s.Append(NumberFormatInfo.InvariantInfo.PositiveInfinitySymbol);
        }

        s.Append(':');

        if (Parameters?.Length > 0)
        {
            for (var i = 0; i < Parameters.Length; i++)
            {
                if (i != 0)
                {
                    s.Append(';');
                }
                s.Append(Parameters[i].ToString("g17", NumberFormatInfo.InvariantInfo));
            }
        }

        s.Append(':');

        if (Precision.HasValue)
        {
            s.Append(Precision.Value.ToString(NumberFormatInfo.InvariantInfo));
        }

        return s;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="first">The first value.</param>
    /// <param name="second">The second value.</param>
    /// <returns><see langword="true"/> if <paramref name="first"/> is equal to the <paramref
    /// name="second">other</paramref> parameter; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(RandomParameters first, RandomParameters second) => first.Equals(second);

    /// <summary>
    /// Indicates whether the current object is not equal to another object of the same type.
    /// </summary>
    /// <param name="first">The first value.</param>
    /// <param name="second">The second value.</param>
    /// <returns><see langword="true"/> if <paramref name="first"/> is not equal to the
    /// <paramref name="second">other</paramref> parameter; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(RandomParameters first, RandomParameters second) => !first.Equals(second);
}
