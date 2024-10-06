# Console Macros

## Introduction

This is a tool for mod creators. If you are looking to download mods for Stardew Valley, you should ignore this.
This mod does nothing on its own. For mod authors, see: [Documentation](#Documentation).

## Install

Download the release from [here](https://github.com/aceynk/ConsoleMacros/releases) and unzip into your Stardew Valley
mods folder.

For help, see: [Modding: Getting Started](https://stardewvalleywiki.com/Modding:Player_Guide/Getting_Started)

## Documentation

This mod uses macros (newline separated SMAPI console commands) registered via file placed in the mod's "Macros" folder
or via [content patcher](https://www.nexusmods.com/stardewvalley/mods/1915?tab=files) edits.

### Register via files

The mod should be packaged with a "Macros" folder. Place a text file with newline separated SMAPI commands in this
folder to allow them to be registered. You will need to [reload the macros](#macro-reload) upon changing or adding new
macros.

An example file could be the following:

```
patch reload myMod
world_setseason summer
player_add (O)74
player_add (O)192
```

Name the file (before the file extension) the name you want the macro to have. For example, ``this_macro.txt`` will
register a macro with the name ``this_macro``.

These commands will run sequentially. You can also run a macro from another macro (but be careful to avoid infinite
loops!).

### Register via Content Patcher

You can also register macros with [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915?tab=files). Target
the asset ``aceynk.macro/Macros`` which is a Dictionary with string keys and string values. An example is below:

```json
{
    "Format": "2.2.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "aceynk.macro/Macros",
            "Entries": {
                "MyMacro": "player_add (O)192\nworld_setday 2\nworld_setseason summer",
                "MySecondMacroWithSpacecore": "world_clear farm debris\nmacro run MyMacro\nplayer_giveexp foraging 100"
            }
        }
    ]
}
```

The above Content Patcher code will register 2 macros ("MyMacro" and "MySecondMacroWithSpacecore").
MySecondMacroWithSpacecore demonstrates calling one macro with another.

### Commands

#### macro run

Usage: ``macro run <macro name>``

Use this to run the given macro by name.

#### macro list

Usage: ``macro list``

List all registered macros from all sources.

#### macro reload

Usage: ``macro reload``

Invalidate the cached macros. Content Patcher ``patch reload`` commands are still necessary for Content Patcher macros
to be refreshed.