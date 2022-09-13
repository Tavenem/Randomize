namespace Tavenem.Randomize.Generators;

/// <summary>
/// A pseudo-random number generator.
/// </summary>
public class RandomNumberGenerator
{
    private readonly IGenerator _generator;

    /// <summary>
    /// A lock providing thread safety for the <see cref="bool"/> and <see cref="byte"/>[]
    /// generation algorithms.
    /// </summary>
    private readonly SemaphoreSlim _lock = new(1);

    /// <summary>
    /// A <see cref="uint"/> which can provide up to 32 random <see cref="bool"/> values before
    /// generating a new random number.
    /// </summary>
    private uint _bitBuffer;

    /// <summary>
    /// The number of random <see cref="bool"/> values which can still be generated from <see
    /// cref="_bitBuffer"/>.
    /// </summary>
    private int _bitCount;

    /// <summary>
    /// The seed value.
    /// </summary>
    public uint Seed
    {
        get => _generator.Seed;
        set => Reset(value);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RandomNumberGenerator"/> with the given seed.
    /// </summary>
    /// <param name="seed">The initial seed.</param>
    public RandomNumberGenerator(uint seed) => _generator = new MersenneTwister(seed);

    /// <summary>
    /// Initializes a new instance of <see cref="RandomNumberGenerator"/> with a pseudo-random seed.
    /// </summary>
    public RandomNumberGenerator() => _generator = new MersenneTwister();

    /// <summary>
    /// Gets a random, nonnegative integer less than <see cref="int.MaxValue" />.
    /// </summary>
    /// <returns>A random, nonnegative integer less than <see cref="int.MaxValue" />.</returns>
    public int Next()
    {
        int result;
        do
        {
            result = NextInclusive();
        } while (result == int.MaxValue);
        return result;
    }

    /// <summary>
    /// Gets a random, nonnegative floating-point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating-point number less than 1.</returns>
    public T Next<T>() where T : IFloatingPoint<T> => _generator.Next<T>();

    /// <summary>
    /// Gets a random floating-point number between zero and <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="maxValue">
    /// <para>
    /// The exclusive maximum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If this value is negative, it is considered an exclusive minimum bound instead (and zero
    /// becomes the inclusive maximum bound).
    /// </para>
    /// <para>
    /// If the value satisfies <see cref="INumberBase{TSelf}.IsNaN(TSelf)"/> the result will be the
    /// result of 0/0 (normally <see cref="IFloatingPointIeee754{TSelf}.NaN"/>, but this might
    /// result in an exception if <typeparamref name="T"/> does not implement <see
    /// cref="IFloatingPointIeee754{TSelf}"/>).
    /// </para>
    /// <para>
    /// If the value is positive or negative infinity, it will always be returned as the result.
    /// </para>
    /// </param>
    /// <returns>
    /// A random, nonnegative floating-point number less than <paramref name="maxValue"/>.
    /// </returns>
    public T Next<T>(T maxValue) where T : IFloatingPoint<T>
    {
        if (T.IsNaN(maxValue))
        {
            return T.Zero / T.Zero;
        }
        if (T.IsInfinity(maxValue))
        {
            return maxValue;
        }

        return Next<T>() * maxValue;
    }

    /// <summary>
    /// Gets a random floating-point number greater than or equal to <paramref name="minValue"/> and
    /// less than or equal to <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="minValue">
    /// <para>
    /// The inclusive minimum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If the value satisfies <see cref="INumberBase{TSelf}.IsNaN(TSelf)"/> the result will be <see
    /// cref="IFloatingPointIeee754{TSelf}.NaN"/>.
    /// </para>
    /// <para>
    /// If the value is positive or negative infinity, it will always be returned as the result
    /// unless <paramref name="maxValue"/> is the opposing infinity (in which case either positive
    /// or negative infinity will be returned randomly).
    /// </para>
    /// </param>
    /// <param name="maxValue">
    /// <para>
    /// The exclusive maximum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If the value is satisfies <see cref="INumberBase{TSelf}.IsNaN(TSelf)"/> the result will be
    /// <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.
    /// </para>
    /// <para>
    /// If the value is positive or negative infinity, it will always be returned as the result
    /// unless <paramref name="minValue"/> is the opposing infinity (in which case either positive
    /// or negative infinity will be returned randomly).
    /// </para>
    /// </param>
    /// <returns>A random, nonnegative floating-point number greater than or equal to <paramref
    /// name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
    /// <remarks>
    /// If <paramref name="minValue"/> is greater than <paramref name="maxValue"/>, the result is
    /// determined by <see cref="RandomizeOptions.InvalidFloatingRangeResult"/>.
    /// </remarks>
    public T Next<T>(T minValue, T maxValue) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(minValue)
            || T.IsNaN(maxValue))
        {
            return T.NaN;
        }
        if (minValue > maxValue)
        {
            switch (RandomizeOptions.InvalidFloatingRangeResult)
            {
                case InvalidFloatingRangeResultOption.MinBound:
                    return minValue;
                case InvalidFloatingRangeResultOption.Zero:
                    return T.Zero;
                case InvalidFloatingRangeResultOption.MaxBound:
                    return maxValue;
                case InvalidFloatingRangeResultOption.Swap:
                    (minValue, maxValue) = (maxValue, minValue);
                    break;
                case InvalidFloatingRangeResultOption.Exception:
                    throw new ArgumentOutOfRangeException(nameof(minValue), ErrorMessages.MinAboveMax);
                case InvalidFloatingRangeResultOption.NaN:
                    return T.NaN;
            }
        }

        if (T.IsInfinity(minValue))
        {
            if (T.IsInfinity(maxValue))
            {
                if (T.Sign(minValue) == T.Sign(maxValue))
                {
                    return minValue;
                }
                else
                {
                    return NextBool()
                        ? T.PositiveInfinity
                        : T.NegativeInfinity;
                }
            }
            else
            {
                return minValue;
            }
        }
        if (T.IsInfinity(maxValue))
        {
            return maxValue;
        }

        return minValue + (Next<T>() * (maxValue - minValue));
    }

    /// <summary>
    /// Gets a random, nonnegative integer less than <paramref name="maxValue" />.
    /// </summary>
    /// <param name="maxValue">
    /// <para>
    /// The exclusive maximum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If this value is negative, it is considered an exclusive minimum bound instead (and zero
    /// becomes the inclusive maximum bound).
    /// </para>
    /// </param>
    /// <returns>A random, nonnegative integer less than <paramref name="maxValue" />.</returns>
    public int Next(int maxValue) => (int)(NextDouble() * maxValue);

    /// <summary>
    /// Gets a random integer greater than or equal to <paramref name="minValue" />
    /// and less than <paramref name="maxValue" />.
    /// </summary>
    /// <param name="minValue">The inclusive minimum bound of the random number to be
    /// generated.</param>
    /// <param name="maxValue">The exclusive maximum bound of the random number to be
    /// generated.</param>
    /// <returns>A random integer greater than or equal to <paramref name="minValue" /> and less
    /// than <paramref name="maxValue" />.</returns>
    /// <remarks>
    /// If <paramref name="minValue" /> is greater than <paramref name="maxValue" />, the result
    /// is determined by <see cref="RandomizeOptions.InvalidIntegralRangeResult" />.
    /// </remarks>
    public int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            switch (RandomizeOptions.InvalidIntegralRangeResult)
            {
                case InvalidIntegralRangeResultOption.MinBound:
                    return minValue;
                case InvalidIntegralRangeResultOption.Zero:
                    return 0;
                case InvalidIntegralRangeResultOption.MaxBound:
                    return maxValue;
                case InvalidIntegralRangeResultOption.Swap:
                    (minValue, maxValue) = (maxValue, minValue);
                    break;
                case InvalidIntegralRangeResultOption.Exception:
                    throw new ArgumentOutOfRangeException(nameof(minValue), ErrorMessages.MinAboveMax);
            }
        }

        return minValue + (int)(NextDouble() * (maxValue - (double)minValue));
    }

    /// <summary>
    /// Gets a random boolean value.
    /// </summary>
    /// <returns>A random boolean value.</returns>
    public bool NextBool()
    {
        bool result;
        _lock.Wait();
        if (_bitCount == 0)
        {
            _bitBuffer = NextUInt();
            _bitCount = 31;
            result = (_bitBuffer & 0x1) == 1;
        }
        else
        {
            _bitCount--;
            result = ((_bitBuffer >>= 1) & 0x1) == 1;
        }
        _lock.Release();
        return result;
    }

    /// <summary>
    /// Fills the elements of the given <paramref name="buffer" /> with random <see cref="byte" />
    /// values.
    /// </summary>
    /// <param name="buffer">An array of bytes whose values will be randomized.</param>
    /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is null.</exception>
    public void NextBytes(byte[] buffer)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer), ErrorMessages.NullBuffer);
        }

        var span = new Span<byte>(buffer);
        NextBytes(span);
    }

    /// <summary>
    /// Fills the elements of the given <see cref="Span{T}"/> with random <see cref="byte" />
    /// values.
    /// </summary>
    /// <param name="buffer">A <see cref="Span{T}"/> of bytes whose values will be
    /// randomized.</param>
    public void NextBytes(Span<byte> buffer)
    {
        var i = 0;
        while (i < buffer.Length - 3)
        {
            var u = NextUInt();
            buffer[i++] = (byte)u;
            buffer[i++] = (byte)(u >> 8);
            buffer[i++] = (byte)(u >> 16);
            buffer[i++] = (byte)(u >> 24);
        }
        if (i < buffer.Length)
        {
            var u = NextUInt();
            buffer[i++] = (byte)u;
            if (i < buffer.Length)
            {
                buffer[i++] = (byte)(u >> 8);
                if (i < buffer.Length)
                {
                    buffer[i++] = (byte)(u >> 16);
                    if (i < buffer.Length)
                    {
                        buffer[i++] = (byte)(u >> 24);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets a random, nonnegative floating point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating point number less than 1.</returns>
    public decimal NextDecimal() => _generator.NextDecimal();

    /// <summary>
    /// Gets a random floating point number between zero and <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="maxValue">
    /// <para>
    /// The exclusive maximum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If this value is negative, it is considered an exclusive minimum bound instead (and zero
    /// becomes the inclusive maximum bound).
    /// </para>
    /// <para>
    /// If the value is positive or negative infinity, it will always be returned as the result.
    /// </para>
    /// </param>
    /// <returns>A random, nonnegative floating point number less than <paramref
    /// name="maxValue"/>.</returns>
    public decimal NextDecimal(decimal maxValue) => NextDecimal() * maxValue;

    /// <summary>
    /// Gets a random floating point number greater than or equal to <paramref name="minValue"/>
    /// and less than or equal to <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="minValue">
    /// The inclusive minimum bound of the random number to be generated.
    /// </param>
    /// <param name="maxValue">
    /// The exclusive maximum bound of the random number to be generated.
    /// </param>
    /// <returns>A random floating point number greater than or equal to <paramref
    /// name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
    /// <remarks>
    /// If <paramref name="minValue"/> is greater than <paramref name="maxValue"/>, the result
    /// is determined by <see cref="RandomizeOptions.InvalidIntegralRangeResult"/>.
    /// </remarks>
    public decimal NextDecimal(decimal minValue, decimal maxValue)
    {
        if (minValue > maxValue)
        {
            switch (RandomizeOptions.InvalidIntegralRangeResult)
            {
                case InvalidIntegralRangeResultOption.MinBound:
                    return minValue;
                case InvalidIntegralRangeResultOption.Zero:
                    return 0;
                case InvalidIntegralRangeResultOption.MaxBound:
                    return maxValue;
                case InvalidIntegralRangeResultOption.Swap:
                    (minValue, maxValue) = (maxValue, minValue);
                    break;
                case InvalidIntegralRangeResultOption.Exception:
                    throw new ArgumentOutOfRangeException(nameof(minValue), ErrorMessages.MinAboveMax);
            }
        }

        return minValue + (NextDecimal() * (maxValue - minValue));
    }

    /// <summary>
    /// Gets a random, nonnegative floating point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating point number less than 1.</returns>
    public double NextDouble() => _generator.NextDouble();

    /// <summary>
    /// Gets a random floating point number between zero and <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="maxValue">
    /// <para>
    /// The exclusive maximum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If this value is negative, it is considered an exclusive minimum bound instead (and zero
    /// becomes the inclusive maximum bound).
    /// </para>
    /// <para>
    /// If the value is <see cref="double.NaN"/> the result will also be <see
    /// cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If the value is positive or negative infinity, it will always be returned as the result.
    /// </para>
    /// </param>
    /// <returns>A random, nonnegative floating point number less than <paramref
    /// name="maxValue"/>.</returns>
    public double NextDouble(double maxValue)
    {
        if (double.IsNaN(maxValue))
        {
            return double.NaN;
        }
        if (double.IsInfinity(maxValue))
        {
            return maxValue;
        }

        return NextDouble() * maxValue;
    }

    /// <summary>
    /// Gets a random floating point number greater than or equal to <paramref name="minValue"/>
    /// and less than or equal to <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="minValue">
    /// <para>
    /// The inclusive minimum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If the value is <see cref="double.NaN"/> the result will also be <see
    /// cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If the value is positive or negative infinity, it will always be returned as the result
    /// unless <paramref name="maxValue"/> is the opposing infinity (in which case either
    /// positive or negative infinity will be returned randomly).
    /// </para>
    /// </param>
    /// <param name="maxValue">
    /// <para>
    /// The exclusive maximum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If the value is <see cref="double.NaN"/> the result will also be <see
    /// cref="double.NaN"/>.
    /// </para>
    /// <para>
    /// If the value is positive or negative infinity, it will always be returned as the result
    /// unless <paramref name="minValue"/> is the opposing infinity (in which case either
    /// positive or negative infinity will be returned randomly).
    /// </para>
    /// </param>
    /// <returns>A random, nonnegative integer greater than or equal to <paramref
    /// name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
    /// <remarks>
    /// If <paramref name="minValue"/> is greater than <paramref name="maxValue"/>, the result
    /// is determined by <see cref="RandomizeOptions.InvalidFloatingRangeResult"/>.
    /// </remarks>
    public double NextDouble(double minValue, double maxValue)
    {
        if (double.IsNaN(minValue)
            || double.IsNaN(maxValue))
        {
            return double.NaN;
        }
        if (minValue > maxValue)
        {
            switch (RandomizeOptions.InvalidFloatingRangeResult)
            {
                case InvalidFloatingRangeResultOption.MinBound:
                    return minValue;
                case InvalidFloatingRangeResultOption.Zero:
                    return 0;
                case InvalidFloatingRangeResultOption.MaxBound:
                    return maxValue;
                case InvalidFloatingRangeResultOption.Swap:
                    (minValue, maxValue) = (maxValue, minValue);
                    break;
                case InvalidFloatingRangeResultOption.Exception:
                    throw new ArgumentOutOfRangeException(nameof(minValue), ErrorMessages.MinAboveMax);
                case InvalidFloatingRangeResultOption.NaN:
                    return double.NaN;
            }
        }

        if (double.IsInfinity(minValue))
        {
            if (double.IsInfinity(maxValue))
            {
                if (Math.Sign(minValue) == Math.Sign(maxValue))
                {
                    return minValue;
                }
                else
                {
                    return NextBool()
                        ? double.PositiveInfinity
                        : double.NegativeInfinity;
                }
            }
            else
            {
                return minValue;
            }
        }
        if (double.IsInfinity(maxValue))
        {
            return maxValue;
        }

        return minValue + (NextDouble() * (maxValue - minValue));
    }

    /// <summary>
    /// Gets a random, nonnegative integer less than or equal to <see cref="int.MaxValue" />.
    /// </summary>
    /// <returns>A random, nonnegative integer less than or equal to <see cref="int.MaxValue"
    /// />.</returns>
    public int NextInclusive() => _generator.NextInclusive();

    /// <summary>
    /// Gets a random integer greater than or equal to zero and less than or equal to <paramref
    /// name="maxValue" />.
    /// </summary>
    /// <param name="maxValue">
    /// <para>
    /// The inclusive maximum bound of the random number to be generated.
    /// </para>
    /// <para>
    /// If this value is negative, it is considered an exclusive minimum bound instead (and zero
    /// becomes the inclusive maximum bound).
    /// </para>
    /// </param>
    /// <returns>A random, nonnegative integer greater than or equal to zero and less than or
    /// equal to <paramref name="maxValue" />.</returns>
    public int NextInclusive(int maxValue)
    {
        if (maxValue == 0)
        {
            return 0;
        }
        else if (maxValue == int.MaxValue)
        {
            return (int)(NextDouble() * (int.MaxValue + 1.0));
        }
        else
        {
            return Next(maxValue < 0 ? maxValue - 1 : maxValue + 1);
        }
    }

    /// <summary>
    /// Gets a random integer greater than or equal to <paramref name="minValue" />
    /// and less than or equal to <paramref name="maxValue" />.
    /// </summary>
    /// <param name="minValue">The inclusive minimum bound of the random number to be
    /// generated.</param>
    /// <param name="maxValue">The inclusive maximum bound of the random number to be
    /// generated.</param>
    /// <returns>A random, nonnegative integer greater than or equal to <paramref
    /// name="minValue" /> and less than or equal to <paramref name="maxValue" />.</returns>
    /// <remarks>
    /// If <paramref name="minValue" /> is greater than <paramref name="maxValue" />, the result
    /// is determined by <see cref="RandomizeOptions.InvalidIntegralRangeResult" />.
    /// </remarks>
    public int NextInclusive(int minValue, int maxValue)
    {
        if (maxValue < int.MaxValue)
        {
            return Next(minValue, maxValue + 1);
        }
        else if (minValue > int.MinValue)
        {
            return Next(minValue - 1, maxValue) + 1;
        }
        else
        {
            return int.MinValue + (int)NextDouble((2.0 * int.MaxValue) + 1);
        }
    }

    /// <summary>
    /// Gets a random, unsigned integer less than <see cref="uint.MaxValue" />.
    /// </summary>
    /// <returns>A random, unsigned integer less than <see cref="uint.MaxValue" />.</returns>
    public uint NextUInt()
    {
        uint result;
        do
        {
            result = NextUIntInclusive();
        } while (result == uint.MaxValue);
        return result;
    }

    /// <summary>
    /// Gets a random, unsigned integer less than <paramref name="maxValue" />.
    /// </summary>
    /// <param name="maxValue">The exclusive maximum bound of the random number to be
    /// generated.</param>
    /// <returns>A random, unsigned integer less than <paramref name="maxValue" />.</returns>
    public uint NextUInt(uint maxValue) => (uint)(NextDouble() * maxValue);

    /// <summary>
    /// Gets a random, unsigned integer greater than or equal to <paramref name="minValue" />
    /// and less than <paramref name="maxValue" />.
    /// </summary>
    /// <param name="minValue">The inclusive minimum bound of the random number to be
    /// generated.</param>
    /// <param name="maxValue">The exclusive maximum bound of the random number to be
    /// generated.</param>
    /// <returns>A random integer greater than or equal to <paramref name="minValue" /> and less
    /// than <paramref name="maxValue" />.</returns>
    /// <remarks>
    /// If <paramref name="minValue" /> is greater than <paramref name="maxValue" />, the result
    /// is determined by <see cref="RandomizeOptions.InvalidIntegralRangeResult" />.
    /// </remarks>
    public uint NextUInt(uint minValue, uint maxValue)
    {
        if (minValue > maxValue)
        {
            switch (RandomizeOptions.InvalidIntegralRangeResult)
            {
                case InvalidIntegralRangeResultOption.MinBound:
                    return minValue;
                case InvalidIntegralRangeResultOption.Zero:
                    return 0;
                case InvalidIntegralRangeResultOption.MaxBound:
                    return maxValue;
                case InvalidIntegralRangeResultOption.Swap:
                    (minValue, maxValue) = (maxValue, minValue);
                    break;
                case InvalidIntegralRangeResultOption.Exception:
                    throw new ArgumentOutOfRangeException(nameof(minValue), ErrorMessages.MinAboveMax);
            }
        }

        return minValue + (uint)(NextDouble() * (maxValue - (double)minValue));
    }

    /// <summary>
    /// Gets a random, unsigned integer less than or equal to <see cref="uint.MaxValue" />.
    /// </summary>
    /// <returns>A random, unsigned integer less than or equal to <see cref="uint.MaxValue"
    /// />.</returns>
    public uint NextUIntInclusive() => _generator.NextUIntInclusive();

    /// <summary>
    /// Gets a random integer greater than or equal to zero and less than or equal to <paramref
    /// name="maxValue" />.
    /// </summary>
    /// <param name="maxValue">
    /// <para>
    /// The inclusive maximum bound of the random number to be generated.
    /// </para>
    /// </param>
    /// <returns>A random, nonnegative integer greater than or equal to zero and less than or
    /// equal to <paramref name="maxValue" />.</returns>
    public uint NextUIntInclusive(uint maxValue)
    {
        if (maxValue == 0)
        {
            return 0U;
        }
        else if (maxValue == uint.MaxValue)
        {
            return (uint)(NextDouble() * (uint.MaxValue + 1.0));
        }
        else
        {
            return NextUInt(maxValue + 1);
        }
    }

    /// <summary>
    /// Gets a random, unsigned integer greater than or equal to <paramref name="minValue" />
    /// and less than or equal to <paramref name="maxValue" />.
    /// </summary>
    /// <param name="minValue">The inclusive minimum bound of the random number to be
    /// generated.</param>
    /// <param name="maxValue">The inclusive maximum bound of the random number to be
    /// generated.</param>
    /// <returns>A random integer greater than or equal to <paramref name="minValue" /> and less
    /// than or equal to <paramref name="maxValue" />.</returns>
    /// <remarks>
    /// If <paramref name="minValue" /> is greater than <paramref name="maxValue" />, the result
    /// is determined by <see cref="RandomizeOptions.InvalidIntegralRangeResult" />.
    /// </remarks>
    public uint NextUIntInclusive(uint minValue, uint maxValue)
    {
        if (maxValue < uint.MaxValue)
        {
            return NextUInt(minValue, maxValue + 1);
        }
        else if (minValue > uint.MinValue)
        {
            return NextUInt(minValue - 1, maxValue) + 1;
        }
        else
        {
            return NextUIntInclusive();
        }
    }

    /// <summary>
    /// <para>
    /// Resets the generator, without changing its current seed.
    /// </para>
    /// <para>
    /// An identical series of values will be produced each time this is called.
    /// </para>
    /// </summary>
    public void Reset() => Reset(Seed);

    /// <summary>
    /// <para>
    /// Resets the generator with the provided seed.
    /// </para>
    /// <para>
    /// An identical series of values will be produced each time this is called with the same
    /// seed.
    /// </para>
    /// </summary>
    /// <param name="seed">The seed value.</param>
    public void Reset(uint seed)
    {
        _lock.Wait();

        _bitBuffer = 0U;
        _bitCount = 0;

        _lock.Release();

        _generator.Reset(seed);
    }
}
