# ReSharper Helpers
Plugin to extend R# functionality by set of custom actions.

[![Build status](https://ci.appveyor.com/api/projects/status/5n8xemx7o9wn32nh?svg=true)](https://ci.appveyor.com/project/Zvirja/resharperhelpers)

### Installation:
Plugin is published to ReSharper plugins store. Use ReSharper Extensions Manager and find it by name: `ReSharper Helpers by Alex Povar`.

### Features:

Currently, the following features are implemented:  
* `Copy full class name` - Quick action to copy full class name (including assembly name) to clipboard.
* `Cleanup modified files` - Extend solution context menu with additional action to perform cleanup on modified files only. Action uses git to get list of modified files.
* `Assert parameter not null` - Assert argument not null (or empty).
* * `Save ctor parameter to private get-only property` - Action like `Create and initialize property`, but created property is private and get-only. Also, action copies annotations, applied to the related argument.
* `[ItemNotNull]/[ItemCanBeNull] annotations` - Quick actions to quickly assing annotation attributes to parameters/properties.
* `[Pure] annotation` - Quick actions to add/remove `Pure` annotation attribute.
* * `Chop/inline method` - Quick actions to chop or inline method arguments.
* * `AutoFixture: Freeze, Greedy` - Quick actions to quickly add [Frozen] or [Greedy] attributes.


