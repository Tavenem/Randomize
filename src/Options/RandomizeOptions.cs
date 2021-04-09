namespace Tavenem.Randomize
{
    /// <summary>
    /// Universal options that affect the behavior of the randomization library.
    /// </summary>
    public static class RandomizeOptions
    {
        /// <summary>
        /// What happens when the minimum bound of the range of a floating point type is greater than
        /// the maximum bound.
        /// </summary>
        public static InvalidFloatingRangeResultOption InvalidFloatingRangeResult { get; set; }

        /// <summary>
        /// What happens when the minimum bound of the range of an integral type is greater than the
        /// maximum bound.
        /// </summary>
        public static InvalidIntegralRangeResultOption InvalidIntegralRangeResult { get; set; }
    }
}
