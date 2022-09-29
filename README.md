![build](https://img.shields.io/github/workflow/status/Tavenem/Randomize/publish/main) [![NuGet downloads](https://img.shields.io/nuget/dt/Tavenem.Randomize)](https://www.nuget.org/packages/Tavenem.Randomize/)

Tavenem.Randomize
==

Tavenem.Randomize provides pseudo-random number generation in various distributions.

For example, to print a single value from a normal distribution:

```csharp
Console.WriteLine(Randomizer.Instance.NormalDistributionSample());
```

To print 10,000 values:

```csharp
foreach (var value in Randomizer.Instance.NormalDistributionSamples(10000))
{
    Console.WriteLine(value);
}
```

Supported distributions:
- [Binomial](https://en.wikipedia.org/wiki/Binomial_distribution)
- [Categorical](https://en.wikipedia.org/wiki/Categorical_distribution)
- [Exponential](https://en.wikipedia.org/wiki/Exponential_distribution)
- [Logistic](https://en.wikipedia.org/wiki/Logistic_distribution)
- [Log-normal](https://en.wikipedia.org/wiki/Log-normal_distribution)
- [Normal](https://en.wikipedia.org/wiki/Normal_distribution)
- "Positive" normal (one half of a normal distribution, giving only values >= mean)

Tavenem.Randomize utilizes a [Mersenne Twister](https://en.wikipedia.org/wiki/Mersenne_Twister) by
default, but also provides a mechanism for replacing the underlying generator with your own,
provided it implements the library's `IGenerator` interface.

The library also provides a serializable `RandomParameters` struct, which can be used to persist or
transport the parameters used to obtain a random value.

Tavenem.Randomize also provides a `Rehydrator` class that allows a deterministic set of random
values to be recreated from the same seed, even if those values are requested out of order.

## Installation

Tavenem.Randomize is available as a [NuGet package](https://www.nuget.org/packages/Tavenem.Randomize/).

## Roadmap

Tavenem.Randomize's latest preview release targets .NET 7, which is also in preview. When a stable release of .NET 7 is published, a new stable release of Tavenem.Randomize will follow shortly.

## Contributing

Contributions are always welcome. Please carefully read the [contributing](docs/CONTRIBUTING.md) document to learn more before submitting issues or pull requests.

## Code of conduct

Please read the [code of conduct](docs/CODE_OF_CONDUCT.md) before engaging with our community, including but not limited to submitting or replying to an issue or pull request.