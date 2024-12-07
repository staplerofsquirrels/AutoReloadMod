# AutoReloadMod
## Installation:  
Install the modloader from: [https://melonwiki.xyz](https://melonwiki.xyz)  
Download the newest release from from [Releases](https://github.com/staplerofsquirrels/AutoReloadMod/releases/latest/)  
Unzip it, then add the files to gamefolder  
This should place AutoReload.dll to the Mods folder and the asset bundle files into Squirrel Stapler_Data\StreamingAssets\AutoReloadMod  
If you did everything well you should have a menu that lets you switch this feature on and off when you open the game.  

## Building it:
Required: Visual Studio, .NET desktop development workload  
Clone the repository  
Open AutoReload\AutoReload.sln in Visual Studio  
Add the BepInEx NuGet [https://nuget.bepinex.dev/upload](https://nuget.bepinex.dev/upload)  
Install the 2019.2.19 version of the UnityEngine.Modules packade  
Add MelonLoader.dll to the references from MelonLoader\net35\  
Add Assembly-CSharp.dll, Unity.TextMeshPro.dll from Squirrel Stapler_Data\Managed  
