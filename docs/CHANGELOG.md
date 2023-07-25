# Changelog

## 2.2-preview.2
### Fixed
- Parse error when there are no parameters

## 2.2-preview.1
### Added
- Source generated (de)serialization support
### Changed
- Made `DistributionProperties` a record
- Changed `DistributionProperties.Mode` from `double[]` to `IReadOnlyList<double>`
- Made `RandomParameters` a record
- Replaced `RandomParameters.Parameters` with strongly-typed properties for each distribution
### Updated
- Update to .NET 8 preview

## 2.1
### Updated
- Update to .NET 7

## 2.1.0-preview.2
### Updated
- Update dependencies

## 2.1.0-preview.1
### Updated
- Update to .NET 7 RC
- Remove dependency on preview features

## 2.0.0
### Updated
- Update to release packages

## 2.0.0-preview.1
### Changed
- Update to .NET 6 preview
- Update to C# 10 preview
- Remove dependency on `HugeNumber` and replace with generic math support
### Removed
- Support for non-JSON serialization

## 1.0.1
### Changed
- Improved nullability support

## 1.0.0
### Added
- Initial release