# ReSharper Helpers
Plugin to extend R# functionality by set of custom actions.

[![ReSharper Helpers R# Plugin](https://img.shields.io/resharper/v/AlexPovar.ReSharperHelpers?label=R%23%20Helpers%20-%20ReSharper)](https://plugins.jetbrains.com/plugin/11665-resharper-helpers)
[![ReSharper Helpers Rider Plugin](https://img.shields.io/resharper/v/AlexPovar.ReSharperHelpers?label=R%23%20Helpers%20-%20Rider)](https://plugins.jetbrains.com/plugin/23302-resharper-helpers)
[![Build status](https://ci.appveyor.com/api/projects/status/5n8xemx7o9wn32nh?svg=true)](https://ci.appveyor.com/project/Zvirja/resharperhelpers)

# Configuration

Plugin could be configured either using the dedicated settings page (search for "ReSharper Helpers" section) or by `.editorconfig` file.

💡 It's recommended to use `.editorconfig` file to store project specific settings.

## Editor Config

The following settings are available.

💡 You can put `.editorconfig` file to a project's folder to configure each project separately.
### Tests project name

**Name:** `resharperhelpers_tests_project_name`

**Format:** String name of existing Tests project

**Example:**
```
[*.cs]
resharperhelpers_tests_project_name = All.Unit
```

By default plugin tries to apply heuristics and discover test project name automatically. If however you have an unusual setup, you can use this setting to configure a custom project name.

Notice, this setting affects both existing tests discovery and new tests creation.

### Test project sub-namespace

**Name:** `resharperhelpers_tests_project_sub_namespace` 

**Format:** Dot-separated list of namespace chunks to prepend to test class name

**Example:**
```
[*.cs]
resharperhelpers_tests_project_sub_namespace = Integration.Core
```

By default it's assumed that both code and test will have the same relative namespace (i.e. excluding default project namespace). This setting allows to customize it by adding extra namespace chunks to test relative namespace when calculating final value. Effectively it allows to nest tests within extra folders.

That could be useful if you have a shared unit tests project and want your projects with code to each have its own root folder.

Notice, this setting affects both existing tests discovery and new tests creation.

### New test class name suffix

**Name:** `resharperhelpers_new_test_class_name_suffix`

**Format:** String value

**Default:** `Tests`

**Example:**
```
[*.cs]
resharperhelpers_new_test_class_name_suffix = Test
```

Suffix to append to class name when creating a new test (e.g. `MyProvider` -> `MyProviderTests`).

### Test class name valid suffixes

**Name:** `resharperhelpers_existing_test_class_name_suffixes`

**Format:** Comma separated list of suffixes

**Default:** `Test,Fixture`

**Example:**
```
[*.cs]
resharperhelpers_existing_test_class_name_suffixes = TestFixture,Validation
```

A list of test suffixes used to find an existing test. Each suffix will be probed one by one. Setting is useful if you have existing code base and some tests have different suffixes.

New test class name suffix is added implicitly.

# Features:

### Create test file/Go to test file
Quick action to go to corresponding test file or create it. Usually, test project name is detected automatically, but could be configured (see above).

### Cleanup modified file
Extend solution, project and directory context menu with additional action to perform cleanup on modified files only. Action uses git to get list of the modified files. All the dirty files (indexed and non-indexed) are included. If you select project or directory (or multiple ones simultaneously), list of files to cleanup is limited to the appropriate scope.

### Navigate to next/prev section in bulb menu
When bulb menu is opened (e.g. after you press `Alt+Enter`), you can use `Ctrl` + `Up/Down` to navigate to next section. Useful if quick actions menu becomes really large.  
If section is very large, it jumps to the middle of the section before jumping to the next/previous one.
![Preview](doc/NavigateToNextPrevSection.gif)

### AutoFixture: [Freeze, Greedy] quick actions
Quick actions to add/remove `[Frozen]` or `[Greedy]` attributes for [AutoFixture xunit integration](https://github.com/AutoFixture/AutoFixture). Allows to specify match criteria for the `[Frozen(Matching.XXX)]` attribute.

### Copy full class name

Extend "Copy Code Reference" menu with an extra item which contains fully qualified class/struct/interface name (including assembly name).
Useful when there is a need to copy type name in a config.

To access the menu put the cursor on class/struct/interface name identifier, hit "Alt+Enter", type "fqn" and hit "Enter". The list will contain an extra entry.

### [Pure] annotation
Quick actions to add/remove `Pure` annotation attribute.

### Chop/one line method arguments
Quick actions to chop method arguments or make them one line (remove all line breaks).
Useful to format method signatures with large number of parameters (e.g. in tests).

### Assert parameter is not null/Assert all
Quick action for method argument to insert assertion statement. Allows to assert all nullable argument using `Assert all` action.
If annotation attributes are available and relevant for argument, they are arranged.

### "Launch solution" build configuration
Custom [R# Build Configuration](https://blog.jetbrains.com/dotnet/2015/10/15/introducing-resharper-build/) that builds solution and runs it. The difference comparing to the "VS Startup" (default) configuration is that the default one builds the startup project (with all dependencies) rather than a solution. If you have projects which you want to be always built, they might be skipped if your startup project doesn't depend on them.

This configuration might be useful when you develop a custom plugin that depends on the startup project. You might want the plugin to be always built and copied to the startup project output directory. In this scenario you can:

1. Mark the plugin project as "Build always".
2. Create and activate the "Launch Solution" build configuration in the `Build & Run` window.


# Pre-release builds
Pre-release builds (develop branch) are published to [custom nuget feed](https://www.myget.org/feed/alexpovar-resharperhelpers-prerelease/package/nuget/AlexPovar.ReSharperHelpers). If you want to use pre-release builds, add the following extension source to ReSharper: `https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2`.
