## Until futher notice this mod is no longer being worked on, thank you to those who used it whilst it was being developed.

# VampireModMenu
 
A mod for [Vampire survivors](https://store.steampowered.com/app/1794680/Vampire_Survivors/) that uses [Melon Loader](https://github.com/LavaGang/MelonLoader) to load the mod

# Installation
1) Firstly you need to be using the new engine to get access to it, open **Steam** and go to your **Library** 
2) Find **Vampire survivors** and open its **Properties**
3) Go to the **Betas** tab
4) Type **newenginepls** in the **Beta access code** input and hit enter
5) Download and install [Melon Loader v0.61](https://github.com/LavaGang/MelonLoader/releases/tag/v0.6.1) for Vampire Survivors after it updates with the new engine
6) Run the game at least once, it may freeze or appear to be frozen give it time to do its thing as it is doing stuff in the background 
7) Once it's done close the game and download the latest release of [VampireModMenu](https://github.com/LeCloutPanda/VampireModMenu/releases/latest/download/VampireModMenu.dll) and [LiberationSans.ttf
](https://github.com/LeCloutPanda/VampireModMenu/releases/latest/download/LiberationSans.ttf) and place the dll into your **Mods** folder and the font into your **UserData** folder

# Usage
To open the menu press ``\`` on your keyboard

# Need to report a bug
Please report them in the [Issues](https://github.com/LeCloutPanda/VampireModMenu/issues) section following this [Example Issue](https://github.com/LeCloutPanda/VampireModMenu/issues/1) as a template

# Developers
For those who want to intergrate support I would suggest adding this or equivalent support code to your mods

```cs
DateTime lastModified;

public override void OnLateUpdate()
{
    base.OnLateUpdate();

    DateTime lastWriteTime = File.GetLastWriteTime(filePath);

    if (lastModified != lastWriteTime)
    {
        lastModified = lastWriteTime;
        LoadConfig(); // Your method that loads the config and updates values
    }
}
```
example method 
```cs
private static void LoadConfig()
{
    JObject json = JObject.Parse(File.ReadAllText(ConfigFile) ?? "{}");
    
    config.Name = (string)json.GetValue("Name");
    config.FontFileName = (string)json.GetValue("FontFileName");
    config.Color = (string)json.GetValue("Color");

    FontDirectory = Path.GetFullPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "UserData", config.FontFileName));
}
```

# License 
Distributed under the MIT License. See `LICENSE` for more information.
