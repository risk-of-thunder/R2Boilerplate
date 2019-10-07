using BepInEx;
using RoR2;
using UnityEngine;
using RoR2.Networking;
using UnityEngine.Networking;
using R2API.Utils;
using MonoMod.Cil;
using System.Reflection;
using BepInEx.Configuration;

namespace YourUsername
{
    /// <summary>
    /// WARNING! THIS IS THE BASE TEMPLATE, BUILDING THIS PROJECT WILL EXECUTE A UTILITY/HELPER WHICH FINDS YOUR DEFAULT ROR2 INSTALL LOCATION,
    /// AND WILL PLACE THE BUILT MOD/DLL INTO A /Plugins/dev/ FOLDER! Source: https://github.com/Paddywaan/R2Mods/blob/master/QuerySteamGameLoc
    /// You can prevent this automated execution by removing the first block inside the "Build-Events" tab of the Project Properties, and replacing it with the hardcoded alternative:
    /// if $(ConfigurationName) == Debug (copy /Y "$(TargetDir)$(TargetFileName)" "[YOUR STEAM LIBRARY GOES HERE]\steamapps\common\Risk of Rain 2\BepInEx\plugins\dev\$(TargetFileName)")
    /// </summary>
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.YourUserName." + modname, modname, modver)]
    public class BepinTemplate : BaseUnityPlugin
    {
        private const string modname = "BepinTemplate", modver = "1.0.0";
        public void Awake()
        {

        }
    }
}