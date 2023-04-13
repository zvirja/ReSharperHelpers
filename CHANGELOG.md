# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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