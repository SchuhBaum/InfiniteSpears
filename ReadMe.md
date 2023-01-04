## InfiniteSpears
###### Version: 0.35
This is a mod for Rain World v1.5.

### Description
Every character can carry spears on their back. The number of backspears can be configured (up to seven). If this number is one then the player is able to spawn or despawn spears infinitely.

### Dependencies
- ConfigMachine.dll

### Installation
1. (ModLoader) `BepInEx` and `BOI` can be downloaded from [RainDB](https://www.raindb.net/) under `Tools`.  
  **NOTE:** Rain World's BepInEx is a modified version. Don't download it from GitHub.
2. (Dependency) The mod `ConfigMachine` can be downloaded from [RainDB](https://www.raindb.net/) under `Tools`.
3. Download the file  `InfiniteSpears.dll` from [Releases](https://github.com/SchuhBaum/InfiniteSpears/releases) and place it in the folder `[Steam]\SteamApps\common\Rain World\Mods`.
4. Start `[Steam]\SteamApps\common\Rain World\BOI\BlepOutIn.exe`.
5. Click `Select path` and enter the game's path `[Steam]\SteamApps\common\Rain World`. Enable the mod `InfiniteSpears.dll` and its dependencies. Then launch the game as normal. 

### Contact
If you have feedback, you can message me on Discord `@SchuhBaum#7246` or write an email to SchuhBaum71@gmail.com.

### License
There are two licenses available - MIT and Unlicense. You can choose which one you want to use.  

### Changelog
v0.20:
- Adds the ability to carry multiple spears on the back.
- Added an option interface to select the number of backspears (needs ConfigMachine).

v0.30:
- Added support for AutoUpdate.
- Fixed an oversight, where you couldn't collect spears when holding two rocks.
- Fixed some bugs, where backspears wouldn't get properly deleted. Linked backspears directly to the AbstractObjectStick, i.e. deactivating the AbstractObjectStick removes the corresponding backspear.
- Restructered code.

v0.35:
- Fixed a bug, where a list was not properly iterated over.
- Fixed a bug, where a backspear was incorrectly drawn in front of slugcat.
- Added support for the Electric Spear mod.
- Fixed a (vanilla?) bug, where the game attempts to push backspears out of shortcuts. This led to repeated sounds.
- This mod is now a BepInEx plugin.
- Added the mod description to the mod config.