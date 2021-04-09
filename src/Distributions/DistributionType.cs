namespace Tavenem.Randomize.Distributions
{
    /// <summary>
    /// The type of distribution used to generate a random value.
    /// </summary>
    public enum DistributionType
    {
        /// <summary>
        /// A continuous, uniform distribution (i.e. real numbers).
        /// </summary>
        ContinuousUniform = 0,

        /// <summary>
        /// A uniform distribution of 32-bit signed integers.
        /// </summary>
        DiscreteUniformSigned = 1,

        /// <summary>
        /// A uniform distribution of 32-bit unsigned integers.
        /// </summary>
        DiscreteUniformUnsigned = 2,

        /// <summary>
        /// A binomial distribution. If a single trial is generated, the Bernoulli distribution.
        /// </summary>
        Binomial = 3,

        /// <summary>
        /// A categorical (discrete) distribution, possibly weighted.
        /// </summary>
        Categorical = 4,

        /// <summary>
        /// The positive half of a normal distribution.
        /// </summary>
        PositiveNormal = 5,

        /// <summary>
        /// An exponential distribution.
        /// </summary>
        Exponential = 6,

        /// <summary>
        /// A log-normal distribution.
        /// </summary>
        LogNormal = 7,

        /// <summary>
        /// A logistic distribution.
        /// </summary>
        Logistic = 8,

        /// <summary>
        /// A normal distribution.
        /// </summary>
        Normal = 9,
    }
}
