namespace Tavenem.Randomize.Generators;

/// <summary>
/// A Mersenne Twister pseudo-random number generator with period 2^19937-1.
/// </summary>
public sealed class MersenneTwister : IGenerator
{
    private const int M = 397;
    private const int MMinus1 = M - 1;
    private const int MMinusN = M - N;

    /// <summary>
    /// The number of unsigned random values generated per run.
    /// </summary>
    private const int N = 624;

    private const int NMinus1 = N - 1;
    private const int NMinusM = N - M;

    /// <summary>
    /// A value which produces a <see cref="decimal"/> between 0 (inclusive) and 1 (exclusive)
    /// when multiplied by a non-negative <see cref="int"/>.
    /// </summary>
    private const decimal IntToDecimalMultiplier = 1.0m / (int.MaxValue + 1.0m);

    /// <summary>
    /// A value which produces a <see cref="double"/> between 0 (inclusive) and 1 (exclusive)
    /// when multiplied by a non-negative <see cref="int"/>.
    /// </summary>
    private const double IntToDoubleMultiplier = 1.0 / (int.MaxValue + 1.0);

    /// <summary>
    /// The leasst significant r bits.
    /// </summary>
    private const uint LowerMask = 0x7fffffffU;

    /// <summary>
    /// The most significant w-r bits.
    /// </summary>
    private const uint UpperMask = 0x80000000U;

    private const uint VectorA = 0x9908b0dfU;

    private static readonly uint[] _Mag01 = new[] { 0x0U, VectorA };

    /// <summary>
    /// A lock providing thread safety for the <see cref="bool"/> and <see cref="byte"/>[]
    /// generation algorithms.
    /// </summary>
    private readonly SemaphoreSlim _lock = new(1);

    /// <summary>
    /// The state vector array.
    /// </summary>
    private readonly uint[] _mt = new uint[N];

    /// <summary>
    /// The index of the state vector array which will be accessed next.
    /// </summary>
    private uint _mti;

    private uint _seed;
    /// <summary>
    /// The seed value.
    /// </summary>
    public uint Seed
    {
        get => _seed;
        set => Reset(value);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MersenneTwister"/>.
    /// </summary>
    /// <param name="seed">The initial seed.</param>
    public MersenneTwister(uint seed) => Reset(seed);

    /// <summary>
    /// Initializes a new instance of <see cref="MersenneTwister"/> with a pseudo-random seed.
    /// </summary>
    public MersenneTwister() : this(SeedGenerator.GetNewSeed()) { }

    /// <summary>
    /// Gets a random, nonnegative floating-point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating-point number less than 1.</returns>
    public T Next<T>() where T : IFloatingPoint<T>
    {
        uint y;
        _lock.Wait();
        if (_mti >= N)
        {
            GenerateNUIntValues();
        }
        y = _mt[_mti++];
        _lock.Release();

        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;

        return T.CreateChecked((int)((y ^ (y >> 18)) >> 1))
            * (T.One / (T.CreateChecked(int.MaxValue) + T.One));
    }

    /// <summary>
    /// Gets a random, nonnegative floating point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating point number less than 1.</returns>
    public decimal NextDecimal()
    {
        uint y;
        _lock.Wait();
        if (_mti >= N)
        {
            GenerateNUIntValues();
        }
        y = _mt[_mti++];
        _lock.Release();

        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;

        return (int)((y ^ (y >> 18)) >> 1) * IntToDecimalMultiplier;
    }

    /// <summary>
    /// Gets a random, nonnegative floating point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating point number less than 1.</returns>
    public double NextDouble()
    {
        uint y;
        _lock.Wait();
        if (_mti >= N)
        {
            GenerateNUIntValues();
        }
        y = _mt[_mti++];
        _lock.Release();

        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;

        return (int)((y ^ (y >> 18)) >> 1) * IntToDoubleMultiplier;
    }

    /// <summary>
    /// Gets a random, nonnegative integer less than or equal to <see cref="int.MaxValue" />.
    /// </summary>
    /// <returns>A random, nonnegative integer less than or equal to <see cref="int.MaxValue"
    /// />.</returns>
    public int NextInclusive()
    {
        uint y;
        _lock.Wait();
        if (_mti >= N)
        {
            GenerateNUIntValues();
        }
        y = _mt[_mti++];
        _lock.Release();

        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;

        return (int)((y ^ (y >> 18)) >> 1);
    }

    /// <summary>
    /// Gets a random, unsigned integer less than or equal to <see cref="uint.MaxValue" />.
    /// </summary>
    /// <returns>A random, unsigned integer less than or equal to <see cref="uint.MaxValue"
    /// />.</returns>
    public uint NextUIntInclusive()
    {
        uint y;
        _lock.Wait();
        if (_mti >= N)
        {
            GenerateNUIntValues();
        }
        y = _mt[_mti++];
        _lock.Release();

        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;

        return y ^ (y >> 18);
    }

    /// <summary>
    /// <para>
    /// Resets the generator, without changing its current seed.
    /// </para>
    /// <para>
    /// An identical series of values will be produced each time this is called.
    /// </para>
    /// </summary>
    public void Reset() => Reset(_seed);

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

        _seed = seed;

        _mt[0] = seed & 0xffffffffU;
        for (_mti = 1; _mti < N; _mti++)
        {
            _mt[_mti] = (1812433253U * (_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30))) + _mti;
        }

        _lock.Release();
    }

    private void GenerateNUIntValues()
    {
        int kk;
        uint y;

        for (kk = 0; kk < NMinusM; kk++)
        {
            y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
            _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ _Mag01[y & 0x1U];
        }
        for (; kk < NMinus1; kk++)
        {
            y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
            _mt[kk] = _mt[kk + MMinusN] ^ (y >> 1) ^ _Mag01[y & 0x1U];
        }
        y = (_mt[NMinus1] & UpperMask) | (_mt[0] & LowerMask);
        _mt[NMinus1] = _mt[MMinus1] ^ (y >> 1) ^ _Mag01[y & 0x1U];

        _mti = 0;
    }
}
