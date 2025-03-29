# REPOLib Sdk

**Unity editor tools for creating content for R.E.P.O. (using REPOLib).**

## Features

- Registering custom valuables without code
- Registering custom items without code
- Registering custom enemies without code
- Registering custom levels without code
- Automatic asset bundling and Thunderstore packaging

## Installation

This section assumes you already have a Unity project set up for R.E.P.O. modding.
If not, use a tool like [R.E.P.O. Project Patcher](https://github.com/Kesomannen/unity-repo-project-patcher) first.

**1. Add REPOLib to the project**

- Download REPOLib from Thunderstore ([link](https://thunderstore.io/c/repo/p/Zehs/REPOLib/)).
- Extract the downloaded file.
- Copy `REPOLib.dll` into your project.

> [!TIP]
> To update REPOLib, replace the dll with a newer version's.

**2. Add REPOLib-Sdk to the project**

- In unity, go to `Window > Package Manager`.
- Click the `+` button in the top left and choose `Add package from git URL`.
- Enter `https://github.com/ZehsTeam/REPOLib-Sdk.git`.

> [!TIP]
> To update REPOLib-Sdk, go to the Package Manager, click on the package and then `Update`.

## Usage

### Create a mod

- Create a new folder in your project.
- Right click in the folder, then choose `Create > REPOLib > Mod`.
- Fill in the fields on the `Mod` asset:
  - `Name`: the mod's name, as shown on Thunderstore. This can only contain numbers, letters and underscores.
  - `Author`: the name of your Thunderstore team.
  - `Version`: must be in the format `X.Y.Z`.
  - `Dependencies`: REPOLib should always be included.
  - `Website Url`: Optional.
  - `Icon`: must be a 256x256 PNG file.
  - `Readme`: a longer description of the mod, in a separate file. Supports markdown formatting.
  - `Extra Files`: additional files that will be included in the package, for example a dll containing your scripts.

> [!TIP]
> You can have multiple mods in the same project, as long as they're in separate folders.

### Create a valuable

- Create a valuable prefab.
  - More documentation is on its way here. For now, use the vanilla valuables as a reference. If you used the patcher, they are located in `Assets/REPO/Game/Resources/valuables`.
- Right click in your mod folder (or any subfolder) and choose `Create > REPOLib > Valuable`.
- Fill in the fields:
  - `Prefab`: A reference to your prefab. The prefab does not have to be in the mod folder.
  - `Valuable Presets`: The valuable presets to register the valuable to. The vanilla values are:
    - `Valuables - Generic` (applies to every level)
    - `Valuables - Wizard`
    - `Valuables - Manor`
    - `Valuables - Arctic`

### Create an item
- Create an item prefab.
  - More documentation is on its way here. For now, use the vanilla item prefabs as a reference. If you used the patcher, they are located in `Assets/REPO/Game/Resources/items`.
- Create an `Item` by right clicking and going to `Create > Other > Item`.
  - More documentation is on its way here. For now, use the vanilla items as a reference. If you used the patcher, they are located in `Assets/REPO/Game/Resources/items/items`.
- Right click in your mod folder (or any subfolder) and choose `Create > REPOLib > Item`.
- Fill in the fields:
  - `Prefab`: A reference to your prefab. The prefab does not have to be in the mod folder.

### Create an enemy

- Create an `EnemySetup` by right clicking and going to `Create > Other > Enemy Setup`.
  - As with valuables, use the vanilla ones as reference.
- Right click in your mod folder (or any subfolder) and choose `Create > REPOLib > Enemy`.
- Fill in the fields.

### Create a level

- Create a `Level` by right clicking and going to `Create > Level > Level Preset`.
  - The patcher outputs vanilla levels to `Assets/REPO/Game/ScriptableObjects/Level` and their assets to `Assets/REPO/Game/Resources/level`.
- On the `Level` asset, make sure that the `Valuable Presets` list is empty.
  - This will make generic valuables spawn in your level. If you'd like specific valuables to also spawn, see [Proxy valuable presets](#proxy-valuable-presets).
- Right click in your mod folder (or any subfolder) and choose `Create > REPOLib > Level`.
- Fill in the fields.

### Export a mod

- Select the `Mod` asset and click `Export` in the inspector.
  - In the window you'll see the associated content files found by REPOLib-Sdk.
- Choose an `Output Path`. The path is relative to the Unity project (unless you specify an absolute path).
- Click `Export` and wait. Once finished, a window should appear showing the exported zip file. This file can be uploaded to Thunderstore or locally imported into mod managers.

> [!WARNING]
> REPOLib-Sdk deletes and creates multiple folders and files under the export path. Therefore, it is recommended to use a new, empty folder as the target.

> [!TIP]
> The export window can also be accessed from `Window > REPOLib Exporter`

### Using custom scripts

Unfortunately, custom scripts cannot be developed from inside the Unity editor. Instead, you have to write your scripts elsewhere:

- Create a new C# project for R.E.P.O. modding.
  - There are templates available for this, for example [linkoid's Repo Sdks](https://github.com/linkoid/Repo.Sdks) and [Matty's Mod Templates](https://discord.com/channels/1344557689979670578/1348716513410027601) (discord thread link).
- Write your scripts in that project.
- Build the project and copy the output dll into your Unity project.
- If needed, copy the BepInEx dlls to your project.
  - If you used the [R.E.P.O. Project Patcher](https://github.com/Kesomannen/unity-repo-project-patcher), these will already be in your project.
- Attach the scripts to your prefabs.
- Include the dll in the `Extra Files` field on your `Mod` asset.

> [!TIP]
> You can use a MSBuild target to automatically copy the dll on each build.

### Proxy valuable presets

To use vanilla `Level Valuables` presets in your custom levels, you should not simply reference them in the `Level` asset. This is because the bundle will then contain duplicates of all the vanilla valuables from that preset. Instead, you have to create a "proxy" preset:

- Create a `Level Valuables` anywhere in your project by going to `Create > Level > Level Valuable Preset`
- Name the asset exactly as the vanilla one you want to include (see [Create a valuable](#create-a-valuable)).
- Add your newly created preset to `Level Valuables` in your `Level` asset.

At runtime, REPOLib will match the name and replace your proxy with the real thing.

## Contribute

Anyone is free to contribute.

https://github.com/ZehsTeam/REPOLib-Sdk

## Developer Contact
**Report bugs, suggest features, or provide feedback:**
- **GitHub Issues Page:** [REPOLib](https://github.com/ZehsTeam/REPOLib/issues)
- **Email:** crithaxxog@gmail.com
- **Twitch:** [CritHaxXoG](https://www.twitch.tv/crithaxxog)
- **YouTube:** [Zehs](https://www.youtube.com/channel/UCb4VEkc-_im0h8DKXlwmIAA)

[![kofi](https://i.imgur.com/jzwECeF.png)](https://ko-fi.com/zehsteam)
