# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.1] - 2020-02-25

### Added
- Support for building testable players (`TestablePlayer` component) as a step towards integration with the Unity Test Framework.

### Changed
- Enable Burst for DotNet builds on Windows
- Revert namespace `Unity.Platforms.Build*` change back to `Unity.Build*`.

### Fixed
- Fix Build & Run fallback when build pipeline doesn't have a proper RunStep, BuildOption.AutoRunPlayer was being set too late, thus it didn't have any effect, this is now fixed.
- Build configuration/pipeline assets will now properly apply changes when clicking outside inspector focus.
- Fixed asset cannot be null exception when trying to store build result.

## [0.2.1-preview] - 2020-01-24

### Changed
- Modfied data format for SceneList to contain additional flags to support LiveLink.
- `BuildStepBuildClassicLiveLink` was moved into the `Unity.Scenes.Editor` assembly in `com.unity.entities` package due to dependencies on Entities.
- Refactored `BuildStepBuildClassicPlayer` since it no longer shares its implementation with `BuildStepBuildClassicLiveLink`
- ClassicBuildProfile.GetExecutableExtension made public so that it can be used from other packages.

## [0.2.0-preview.2] - 2020-01-17

## Fixed
- Fix `BuildStepBuildClassicLiveLink` build step to re-generate Live Link player required metadata file.

## [0.2.0-preview.1] - 2020-01-15

### Added
- Platform specific event processing support (new Unity.Platforms.Common assembly).

## [0.2.0-preview] - 2020-01-13

The package `com.unity.build` has been merged in the `com.unity.platforms` package, and includes the following changes since the release of `com.unity.build@0.1.0-preview`:

## Added
- New `BuildStepRunBefore` and `BuildStepRunAfter` attributes which can be optionally added to a `BuildStep` to declare which other steps must be run before or after that step.
- `BuildStep` attribute now support `Name`, `Description` and `Category` properties.
- Added new `RunStep` attribute to configure run step types various properties.

## Changed
- Updated `com.unity.properties` to version `0.10.4-preview`.
- Updated `com.unity.serialization` to version `0.6.4-preview`.
- All classes that should not be derived from are now properly marked as `sealed`.
- All UI related code has been moved into assembly `Unity.Build.Editor`.
- Added support for `[HideInInspector]` attribute for build components, build steps and run steps. Using that attribute will hide the corresponding type from the inspector view.
- Field `BuildStepAttribute.flags` is now obsolete. The attribute `[HideInInspector]` should now be used to hide build steps in inspector or searcher menu.
- Field `BuildStepAttribute.description` is now obsolete: it has been renamed to `BuildStepAttribute.Description`.
- Field `BuildStepAttribute.category` is now obsolete: it has been renamed to `BuildStepAttribute.Category`.
- Interface `IBuildSettingsComponent` is now obsolete: it has been renamed to `IBuildComponent`.
- Class `BuildSettings` is now obsolete: it has been renamed to `BuildConfiguration`.
- Asset extension `.buildsettings` is now obsolete: it has been renamed to `.buildconfiguration`.
- Because all build steps must derive from `BuildStep`, all methods and properties on `IBuildStep` are no longer necessary and have been removed.
- Property `BuildStep.Description` is no longer abstract, and can now be set from attribute `BuildStepAttribute(Description = "...")`.
- Enum `BuildConfiguration` is now obsolete: it has been renamed to `BuildType`.
- Interface `IRunStep` is now obsolete: run steps must derive from `RunStep`.
- Nested `BuildPipeline` build steps are now executed as a flat list from the main `BuildPipeline`, rather than calling `IBuildStep.RunBuildStep` recursively on them.
- Build step cleanup pass will only be executed if the default implementation is overridden, greatly reducing irrelevant logging in `BuildPipelineResult`.
- Class `ComponentContainer` should not be instantiated directly and thus has been properly marked as `abstract`.
- Class `ComponentContainer` is now obsolete: it has been renamed to `HierarchicalComponentContainer`.

## Fixed
- Empty dependencies in inspector are now properly supported again.
- Dependencies label in inspector will now as "Dependencies" again.

## [0.1.8-preview] - 2019-12-11

### Added
- Added Unity.Build.Common files, moved them from com.unity.entities.

## [0.1.7-preview.3] - 2019-12-09

### Changed
- Disabled burst for windows/dotnet/collections checks, because it was broken.

## [0.1.7-preview.2] - 2019-11-12

### Changed
- Changed the way platforms customize builds for dots runtime, in a way that makes buildsettings usage clearer and faster, and more reliable.

## [0.1.7-preview] - 2019-10-25

### Added
- Added `WriteBeeConfigFile` method to pass build target specifc configuration to Bee.

## [0.1.6-preview] - 2019-10-23

### Added
- Re-introduce the concept of "buildable" build targets with the `CanBuild` property.

### Changed
- `GetDisplayName` method changed for `DisplayName` property.
- `GetUnityPlatformName` method changed for `UnityPlatformName` property.
- `GetExecutableExtension` method changed for `ExecutableExtension` property.
- `GetBeeTargetName` method changed for `BeeTargetName` property.

## [0.1.5-preview] - 2019-10-22

### Added
- Added static method `GetBuildTargetFromUnityPlatformName` to find build target that match Unity platform name. If build target is not found, an `UnknownBuildTarget` will be returned.
- Added static method `GetBuildTargetFromBeeTargetName` to find build target that match Bee target name. If build target is not found, an `UnknownBuildTarget` will be returned.

### Changed
- `AvailableBuildTargets` will now contain all build targets regardless of `HideInBuildTargetPopup` value, as well as `UnknownBuildTarget` instances.

## [0.1.4-preview] - 2019-09-26
- Bug fixes  
- Add iOS platform support
- Add desktop platforms package

## [0.1.3-preview] - 2019-09-03

- Bug fixes  

## [0.1.2-preview] - 2019-08-13

### Added
- Added static `AvailableBuildTargets` property to `BuildTarget` class, which provides the list of available build targets for the running Unity editor platform.
- Added static `DefaultBuildTarget` property to `BuildTarget` class, which provides the default build target for the running Unity editor platform.

### Changed
- Support for Unity 2019.1.

## [0.1.1-preview] - 2019-06-10

- Initial release of *Unity.Platforms*.
