# COG Mod Manager
## A simple Console Application Mod Manager for [Slackers - Carts of Glory](https://store.steampowered.com/app/2354000/Slackers__Carts_of_Glory/)

# Installation
Head to the [Download Website](https://cogmm.netlify.app/) and download the COGModManager.exe.

Simply run the COGModManager.exe and it should boot up. If it cannot locate your game directory, you will have to pass it manually.

# Guide
## Patching
Begin by running the `Patch` command to patch the game files. This is **REQUIRED** in order to run mods. If you wish to disable mods without uninstalling them, run the `Unpatch` command to disable mods, and re-run the patch command to enable them.
## Mod Installation
Mods can be installed and uninstalled using the `Install` and `Uninstall` commands. The `Install` command requires either a path to a compatable zip file on your computer, or a URL to a compatable zip file on the web. 

Additionally, if you cannot find any mods for download, there is an official repository of mods built in, that can be installed using the `Repository` command. 

Some mods contain optional add-ons, such as sounds or textures that can be ignored. If you decline the addon during the installation process, re-installing the mod will give you the option to download the add-on again.
## Restoring Game Files
When the application is first run, it will save a copy of the game's files to AppData. Then using the `Restore` command you are able to restore your game files in the event something is corrupted/you want the original files back.

If the mod manager detects an update has taken place since the backup was made, it will warn you when trying to restore. In this situation, the only way to restore game files is to install a fresh copy of the game, or if you're just looking to remove the files added simply unpatching and uninstalling all installed mods will work just fine.
# Creating Mods
## Creating A Mod
While I will not go in depth on how to create mods, [Dark S4M](https://www.youtube.com/@DRKS4M) has made some decent tutorials on creating mods yourself, such as custom characters and maps, which should hopefully help you get started. If you'd like to turn these mods into a zip file compatable with COG Mod Manager, you will need to follow the steps below
## Packaging Mods
In order to package a mod, the file must be set up in a specific manner. Firstly, you need to create a folder somewhere not in the game directory named `CartOfGlory` in which your mods will go. Here, you must put the directory and assets of your mod from the game directory folder. 

Let's assume your game directory is located at `C:\Program Files (x86)\Steam\steamapps\common\Slackers - Carts of Glory` which in this example i will just refer to as GameDirectory. If your mod, say a character mod is located at `GameDirectory > CartOfGlory > Content > Art > Characters_New`, you will need to create the directory in your new `CartOfGlory` folder you created outside the game directory. 

After creating the directory in the folder, you need to move all the files you created as part of your mod into it. Let's say I have created 3 items in my game's Characters_New folder for a custom character. Char_Assets, Aaron_SK.uasset, and Aaron_SK.uexp. I would copy these over to the new Characters_New folder in my mod directory, so the complete directory for file Aaron_SK.uasset would be:
`Mod Directory > CartOfGlory > Content > Art > Characters_New > Aaron_SK.uasset`

After your assests are prepared, you need a manifest file, `manifest.cog`, which comes in the form of a json file with the extention cog. Included in this file is the ability to add custom add-ons for the user, by specifying a title and a path from the root of your mod folder where those files are located. **If you do not want optional add-ons, do not fill out the OptionalAddons array.** 
Here is an example of what that should look like:
```json
{
  "ModName": "Mod Name",
  "ModAuthor": "Your Name",
  "ModVersion": "1.0.0",
  "OptionalAddons": [
    {
      "AddonName": "Custom Sound Effects",
      "Directory": "CartOfGlory/Content/Audio"
    }
  ]
}
```


Finally, zip up the `CartOfGlory` folder you made with all your assets in it along with the `manifest.cog` file. Your zip should not contain any subfolders, and simply look like this:

```
📦MyModName.zip
 ┣ 📂CartOfGlory
 ┣ 📜manifest.cog
```

## Submitting to the Repository

If you would like to submit your mod to the repository, add me on discord (`tankhalfempty`) and send me your mod with a title and short description. If the mod is of decent quality and properly formatted then I will submit it. You will have to manually send me each new version of the mod to be uploaded however, so do your best to make it as complete as possible on the first upload. Keep in mind these mods can be shared on third party platforms aswell, so feel free to do so.


