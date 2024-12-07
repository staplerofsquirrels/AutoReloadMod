# AutoReloadMod
## Installation:  
Install the modloader from: [https://melonwiki.xyz](https://melonwiki.xyz)  
Download the newest release (the .dll file) from from [Releases](https://github.com/staplerofsquirrels/AutoReloadMod/releases/latest/)  
Place the file into your Mods folder inside your gamefolder 

## Building it:
Required: Visual Studio, .NET desktop development workload  
Clone the repository  
Open AutoReload\AutoReload.sln in Visual Studio  
Add the BepInEx NuGet [https://nuget.bepinex.dev/upload](https://nuget.bepinex.dev/upload)  
Install the 2019.2.19 version of the [UnityEngine.Modules](https://nuget.bepinex.dev/packages/unityengine.modules/2019.2.19) packade
Add MelonLoader.dll to the references from MelonLoader\net35\  
Add Assembly-CSharp.dll, Unity.TextMeshPro.dll from Squirrel Stapler_Data\Managed 
