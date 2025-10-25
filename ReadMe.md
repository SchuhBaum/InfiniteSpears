## InfiniteSpears
###### Version: 2.3.1
This is a mod for Rain World v1.11.

### Description
This mod has two main features. You can either  
- carry one spear on your back, and spawn or despawn spears using it,  
OR  
- carry multiple spears on your back which behave normally.

In addition, spearmaster can spawn needles directly to its back.

This mod has additional options:
- `(Joke Rifle)` You have infinite ammunition for the joke rifle.
- `(Swallowed Items)` Most swallowed item are duplicated when regurgitating unless your hands are full.

### Installation
0. Update Rain World to version 1.11 if needed.
1. Download the file  `InfiniteSpears.zip` from [Releases](https://github.com/SchuhBaum/InfiniteSpears/releases/tag/v2.3.1).
2. Extract its content in the folder `[Steam]\SteamApps\common\Rain World\RainWorld_Data\StreamingAssets\mods`.
3. Start the game as normal. In the main menu select `Remix` and enable the mod. 

### Bug reports & FAQ
See the corresponding sections on the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2928752589) for the mod `SBCameraScroll`.

### Contact
If you have feedback, you can message me on Discord `@schuhbaum` or write an email to SchuhBaum71@gmail.com.

### License
See the file LICENSE-MIT.

### Changelog
#### (Rain World v1.11)
v2.3.1:
- (swallowed items) Switched to a whitelist model. Only whitelisted items can
  get duplicated. Otherwise, creating items in a corrupted state might freeze
  the game.
  - The mod logs when an item is not whitelisted but potentially could be.
  - Whitelisted GraffitiBombs.

v2.3.0:
- Bumped version requirement to v1.11.

#### (Rain World v1.10)
v2.2.8:
- Fixed a bug where the IL hook for GrabUpdate would conflict with the one from
  Rain Meadow. This caused an invalid program exception on startup.

v2.2.7:
- Restored using a default value if no slugcat is found. Otherwise, it might crash.

v2.2.6:
- Added a separate slider for all custom slugcats.

v2.2.5:
- Fixed a bug where the player would run and fall really fast when using a warp
  point while carrying a backspear.

v2.2.4:
- Changed the requirement to v1.10 instead of v1.10.1.

v2.2.3:
- Added a slider for the Watcher slugcat to set the number of backspears.

#### (Rain World v1.9)
v2.2.2:
- (smallowed items) Fixed a bug where pebble pearls could freeze the game. I
just blacklisted them.

v2.2.1:
- (slugs and spears) Changed the controls. You need to hold up/down to interact with slugs. Do the opposite for spears.

v2.2.0:
- (slugs and spears) Added this option (disabled by default). When enabled, you can carry slugs and spears on your back at the same time.

v2.1.9:
- Made the IL hook more robust.
- (swallowed items) Fixed some bugs where specific consumables could not be duplicated.
- IL hooks should no longer be logged multiple times when other mods add these IL hooks as well.
- Option specific hooks are no longer initialized every cycle. Instead they are initialized when starting the game or changing the options.
- Added pdb file for debugging.
- Fixed a bug where you could spawn needles to your back while carrying a slugcat.
- Added for each slugcat a separate slider for the number of backspears.
- Potentially fixed a bug where the game would freeze because you had more than 7 backspears (duplicate glitch?).
- Fixed a bug where you could carry backspears and a slugpup on your back at the same time.
- Added that retrieving a spear from your back is prioritized when you can dual wield them (like Spearmaster).

v2.1.0:  
- Added support for Rain World 1.9.
- Removed AutoUpdate.
- Added options to exclude certain characters. Saint is excluded by default.
- Spearmaster can spawn needles to his back. Spawning spears from backspears is prioritized.
- Fixed a where the game would crash when a player was initialized multiple times.
- Restructured code.
- You can spawn needles to your back even when blacklisted since you can have a backspear perk as well.
- Fixed a bug where the player would gain invincibility.
- Fixed a bug where you would drop spears prematurely when being stunned. For example, being grabbed by leeches counts as stun.
- Added a consistency check. There seems to be cases where spears on your back are deactivated but the mode of the spears is not changed.
- (joke rifle) Added this option (disabled by default).
- (swallowed items) Added this option (disabled by default).
- Duplicated items should have the same randomness now. For example the color for slugpups should match (if the swallow everything mod is used).

#### (Rain World v1.5)
v0.36:
- Fixed a bug, where a list was not properly iterated over.
- Fixed a bug, where a backspear was incorrectly drawn in front of slugcat.
- Added support for the Electric Spear mod.
- Fixed a (vanilla?) bug, where the game attempts to push backspears out of shortcuts. This led to repeated sounds.
- This mod is now a BepInEx plugin.
- Added the mod description to the mod config.
- Fixed a bug where the dependency checks would fail when using the modloader Realm.

v0.30:
- Added support for AutoUpdate.
- Fixed an oversight, where you couldn't collect spears when holding two rocks.
- Fixed some bugs, where backspears wouldn't get properly deleted. Linked backspears directly to the AbstractObjectStick, i.e. deactivating the AbstractObjectStick removes the corresponding backspear.
- Restructered code.

v0.20:
- Adds the ability to carry multiple spears on the back.
- Added an option interface to select the number of backspears (needs ConfigMachine).
