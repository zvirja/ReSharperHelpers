# ResharperTweaks
Custom plugin to extend R# functionality.

Currently, the following features are implemented:  
* Cleanup modified files - Extend solution context menu with additional action to perform cleanup on modified files only. Action uses git to get list of modified files.
* AutoFixture: Freeze, Greedy - Quick actions to quickly add [Frozen] or [Greedy] attributes.
* Chop/inline method - Quick actions to chop or inline method arguments.
* ItemNotNull/ItemCanBeNull annotations - Quick actions to quickly assing annotation attributes to parameters/properties.
* Pure annotation - Quick actions to add/remove `Pure` annotation attribute.
* Copy full class name - Quick action to copy full class name (including assembly name) to clipboard.
* Save ctor parameter to private get-only property - Quick action like `Create and initialize property`, but created property is private and get-only. Also, action copies annotations, applied to the argument.
