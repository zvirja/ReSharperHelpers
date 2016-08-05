# ReSharper Helpers
Plugin to extend R# functionality by set of custom actions.

[ReSharper Gallery](https://resharper-plugins.jetbrains.com/packages/AlexPovar.ReSharperHelpers/)

[![Build status](https://ci.appveyor.com/api/projects/status/5n8xemx7o9wn32nh/branch/master?svg=true&passingText=master%20-%20OK)](https://ci.appveyor.com/project/Zvirja/resharperhelpers/branch/master)
[![Build status](https://ci.appveyor.com/api/projects/status/5n8xemx7o9wn32nh/branch/develop?svg=true&passingText=develop%20-%20OK)](https://ci.appveyor.com/project/Zvirja/resharperhelpers/branch/develop)

### Features:

Currently, the following features are implemented:  
* `Copy full class name` - Quick action to copy full class name (including assembly name) to clipboard.
* `Cleanup modified files` - Extend solution context menu with additional action to perform cleanup on modified files only. Action uses git to get list of modified files.
* `Assert parameter not null` - Assert argument not null (or empty).
* `Save ctor parameter to private get-only property` - Action like `Create and initialize property`, but created property is private and get-only. Also, action copies annotations, applied to the related argument.
* `[ItemNotNull]/[ItemCanBeNull] annotations` - Quick actions to quickly assing annotation attributes to parameters/properties.
* `[Pure] annotation` - Quick actions to add/remove `Pure` annotation attribute.
* `Chop/inline method` - Quick actions to chop or inline method arguments.
* `AutoFixture: Freeze, Greedy` - Quick actions to quickly add [Frozen] or [Greedy] attributes.


### Pre-release builds
Pre-release builds (develop branch) are published to [custom nuget feed](https://www.myget.org/feed/Sync/alexpovar-resharperhelpers-prerelease). If you want to have pre-release builds, add the following NuGet package source to ReSharper: `https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2`.
