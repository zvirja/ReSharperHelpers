# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## 7.9.0
- Added support for ReSharper & Rider 2024.3

## 7.8.0
- Added support for ReSharper & Rider 2024.2

## 7.7.2
- Fix plugin to be compatible with whole ReSharper 2024.1.x version family

## 7.7.1
- Added support for ReSharper & Rider 2024.1.3

JetBrains introduced a subtle breaking change in their API, so recompiling is required.
It will make this version incompatible with prior 2024.1.x versions.

## 7.7.0
- Added support for ReSharper & Rider 2024.1

## 7.6.1
- Fixed publish of Rider Plugin. Nothing changed for Visual Studio plugin.

## 7.6.0
- Added support for ReSharper 2023.2
- Publish Rider version in addition to ReSharper. Notice, Rider version lucks some features which are Visual Studio specific.

## 7.5.0
- Added support for ReSharper 2023.2
- Enhanced AutoFixture attribute actions applicability detection
-
## 7.4.0
- Added support for ReSharper 2023.1
- Updated LibGit2Sharp to 0.27.2, so hopefully long paths are supported now

## 7.3.1
- Fixed "Clean git modified" feature

## 7.3.0
- Added support for ReSharper 2022.3

## 7.2.0
- Added support for ReSharper 2022.2

## 7.1.0
- Added support for ReSharper 2022.1

## 7.0.1
- Respect configured file header when creating a test file

## 7.0.1
Support latest R# and VS 2022 🎉

- Move "Copy full class name" action under "Copy Code Reference" action
- Action to create or browse tests now works also for structs and records
- Entirely remove `AssemblyMetadata` configuration in favor of .editorconfig
