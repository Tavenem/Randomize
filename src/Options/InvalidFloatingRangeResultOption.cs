﻿namespace Tavenem.Randomize;

/// <summary>
/// Indicates the result when a minimum bound is greater than a maximum bound in a
/// floating-point operation.
/// </summary>
public enum InvalidFloatingRangeResultOption
{
    /// <summary>
    /// The minimum bound of the range is returned. This is the default.
    /// </summary>
    MinBound = 0,

    /// <summary>
    /// Zero is returned.
    /// </summary>
    Zero = 1,

    /// <summary>
    /// The maximum bound of the range is returned.
    /// </summary>
    MaxBound = 2,

    /// <summary>
    /// The bounds are swapped.
    /// </summary>
    Swap = 3,

    /// <summary>
    /// An exception is thrown.
    /// </summary>
    Exception = 4,

    /// <summary>
    /// The type's Not-a-Number value is returned.
    /// </summary>
    NaN = 5,
}
