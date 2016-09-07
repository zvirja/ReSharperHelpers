# ReSharper Helpers
Plugin to extend R# functionality by set of custom actions.

[ReSharper Gallery](https://resharper-plugins.jetbrains.com/packages/AlexPovar.ReSharperHelpers/)

[![Build status](https://ci.appveyor.com/api/projects/status/5n8xemx7o9wn32nh/branch/master?svg=true&passingText=master%20-%20OK&failingText=master%20-%20failing&pendingText=master%20-%20pending)](https://ci.appveyor.com/project/Zvirja/resharperhelpers/branch/master)
[![Build status](https://ci.appveyor.com/api/projects/status/5n8xemx7o9wn32nh/branch/develop?svg=true&passingText=develop%20-%20OK&failingText=develop%20-%20failing&pendingText=develop%20-%20pending)](https://ci.appveyor.com/project/Zvirja/resharperhelpers/branch/develop)

### Features:

Currently, the following features are implemented:  
* `Copy full class name` - Quick action to copy full class name (including assembly name) to clipboard.
* `Cleanup modified files` - Extend solution context menu with additional action to perform cleanup on modified files only. Action uses git to get list of modified files.
* `Assert parameter not null/Assert all` - Assert argument not null (or empty).
* `Save ctor parameter to private get-only property` - Action like `Create and initialize property`, but created property is private and get-only. Also, action copies annotations, applied to the related argument.
* `[Pure] annotation` - Quick actions to add/remove `Pure` annotation attribute.
* `Chop/inline method` - Quick actions to chop or inline method arguments.
* `AutoFixture: Freeze, Greedy` - Quick actions to quickly add [Frozen] or [Greedy] attributes for [AutoFixture xunit integration](https://github.com/AutoFixture/AutoFixture).
* `Create test/Go to test` - Class action to go to corresponding test file or create it. Test project name should be configured in settings.
* `Navigate to next/prev section in bulb menu` - When bulb menu is opened (e.g. after you press `Alt+Enter`), you can use `Ctrl` + `Up/Down` to navigate to next section. Useful if quick actions menu becomes really large.
  ![Preview](doc/NavigateToNextPrevSection.gif)


### Pre-release builds
Pre-release builds (develop branch) are published to [custom nuget feed](https://www.myget.org/feed/alexpovar-resharperhelpers-prerelease/package/nuget/AlexPovar.ReSharperHelpers). If you want to have pre-release builds, add the following NuGet package source to ReSharper: `https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2`.
