# ReSharper Helpers
Plugin to extend R# functionality by set of custom actions.

[ReSharper Gallery](https://resharper-plugins.jetbrains.com/packages/AlexPovar.ReSharperHelpers/)

[![Build status](https://ci.appveyor.com/api/projects/status/5n8xemx7o9wn32nh?svg=true)](https://ci.appveyor.com/project/Zvirja/resharperhelpers)

## Features:

#### Copy full class name
Quick action to copy full class name (including assembly name) to clipboard.

#### Cleanup modified file
Extend solution context menu with additional action to perform cleanup on modified files only. Action uses git to get list of modified files. All dirty files (indexed and not-indexed) are included.

#### Suppress warnings for project
Now you can use `[assembly: SuppressMessage("ReSharper", "id")]` attribute to suppress inspection warnings on project level. It might be useful for the test projects where not all inspections are relevan.

Note, you might need to reload solution and re-analyze files to apply changes.

#### Assert parameter is not null/Assert all
Quick action for method argument to insert assertion statement. Allows to assert all nullable argument using `Assert all` action.
If nullability attribute are available, they are arranged.

#### Save ctor parameter to private get-only property
Quick action like `Create and initialize property`, but created property is private and get-only. Also, action copies annotations, applied to the related argument.

It's recommended to be used after `Assert parameter not null` action which arranged nullability attributes.

#### [Pure] annotation
Quick actions to add/remove `Pure` annotation attribute.

#### Chop/one line method arguments
Quick actions to chop method arguments or make them one line (remove all line breaks).
Useful to format method signatures with large number of parameters (e.g. in tests).

#### AutoFixture: [Freeze, Greedy] quick actions
Quick actions to add/remove `[Frozen]` or `[Greedy]` attributes for [AutoFixture xunit integration](https://github.com/AutoFixture/AutoFixture). Allows to specify match criteria for the `[Frozen(Matching.XXX)]` attribute.

#### Create test file/Go to test file
Quick action to go to corresponding test file or create it. Usually, test project name is detected automatically, but could be configured in plugin settings (in R# options dialog).

#### Navigate to next/prev section in bulb menu
When bulb menu is opened (e.g. after you press `Alt+Enter`), you can use `Ctrl` + `Up/Down` to navigate to next section. Useful if quick actions menu becomes really large.  
If section is very large, it jumps to the middle of the section before jumping to the next/previous one.
![Preview](doc/NavigateToNextPrevSection.gif)



## Pre-release builds
Pre-release builds (develop branch) are published to [custom nuget feed](https://www.myget.org/feed/alexpovar-resharperhelpers-prerelease/package/nuget/AlexPovar.ReSharperHelpers). If you want to use pre-release builds, add the following NuGet package source to ReSharper: `https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2`.
