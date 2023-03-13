## InfiniteSpears
###### Version: 2.0.9
This is a mod for Rain World v1.9.

### Description
This mod has two main features. You can either  
a) carry one spear on your back, and spawn or despawn spears using it,  
OR  
b) carry multiple spears on your back which behave normally.
In addition, spearmaster can spawn needles directly to its back.

This mod has additional options:
- `(Joke Rifle)` You have infinite ammunition for the joke rifle.
- `(Swallowed Items)` Most swallowed item are duplicated when regurgitating unless your hands are full.

### Installation
0. Update Rain World to version 1.9 if needed.
1. Download the file  `InfiniteSpears.zip` from [Releases](https://github.com/SchuhBaum/InfiniteSpears/releases/tag/v2.0.9).
2. Extract its content in the folder `[Steam]\SteamApps\common\Rain World\RainWorld_Data\StreamingAssets\mods`.
3. Start the game as normal. In the main menu select `Remix` and enable the mod. 

### Bug reports
Please post bugs on the Rain World Discord server in the channel #modding-support:  
https://discord.gg/rainworld

### Contact
If you have feedback, you can message me on Discord `@SchuhBaum#7246` or write an email to SchuhBaum71@gmail.com.

### License
There are two licenses available - MIT and Unlicense. You can choose which one you want to use.  

### Changelog
#### (Rain World v1.9)
v2.0.9:  
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

#### (Rain World v1.5)
v0.20:
- Adds the ability to carry multiple spears on the back.
- Added an option interface to select the number of backspears (needs ConfigMachine).

v0.30:
- Added support for AutoUpdate.
- Fixed an oversight, where you couldn't collect spears when holding two rocks.
- Fixed some bugs, where backspears wouldn't get properly deleted. Linked backspears directly to the AbstractObjectStick, i.e. deactivating the AbstractObjectStick removes the corresponding backspear.
- Restructered code.

v0.36:
- Fixed a bug, where a list was not properly iterated over.
- Fixed a bug, where a backspear was incorrectly drawn in front of slugcat.
- Added support for the Electric Spear mod.
- Fixed a (vanilla?) bug, where the game attempts to push backspears out of shortcuts. This led to repeated sounds.
- This mod is now a BepInEx plugin.
- Added the mod description to the mod config.
- Fixed a bug where the dependency checks would fail when using the modloader Realm.