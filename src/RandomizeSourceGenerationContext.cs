using System.Text.Json.Serialization;
using Tavenem.Randomize.Distributions;

namespace Tavenem.Randomize;

/// <summary>
/// A <see cref="JsonSerializerContext"/> for <c>Tavenem.Randomize</c>
/// </summary>
[JsonSerializable(typeof(DistributionProperties))]
[JsonSerializable(typeof(RandomParameters))]
public partial class RandomizeSourceGenerationContext
    : JsonSerializerContext
{ }
