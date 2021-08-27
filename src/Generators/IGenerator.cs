namespace Tavenem.Randomize.Generators;

/// <summary>
/// A pseudo-random number generator.
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// The seed value.
    /// </summary>
    uint Seed { get; }

    /// <summary>
    /// Gets a random, nonnegative floating-point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating-point number less than 1.</returns>
    T Next<T>() where T : IFloatingPoint<T>;

    /// <summary>
    /// Gets a random, nonnegative floating point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating point number less than 1.</returns>
    decimal NextDecimal();

    /// <summary>
    /// Gets a random, nonnegative floating point number less than 1.
    /// </summary>
    /// <returns>A random, nonnegative floating point number less than 1.</returns>
    double NextDouble();

    /// <summary>
    /// Gets a random, nonnegative integer less than or equal to <see cref="int.MaxValue"/>.
    /// </summary>
    /// <returns>A random, nonnegative integer less than or equal to <see
    /// cref="int.MaxValue"/>.</returns>
    int NextInclusive();

    /// <summary>
    /// Gets a random, unsigned integer less than or equal to <see cref="uint.MaxValue"/>.
    /// </summary>
    /// <returns>A random, unsigned integer less than or equal to <see cref="uint.MaxValue"/>.</returns>
    uint NextUIntInclusive();

    /// <summary>
    /// <para>
    /// Resets the generator, without changing its current seed.
    /// </para>
    /// <para>
    /// An identical series of values will be produced each time this is called.
    /// </para>
    /// </summary>
    void Reset();

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
    void Reset(uint seed);
}
