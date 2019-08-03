using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace ExamplePlugin
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a very simple plugin that adds Bandit to the game, and gives you a tier 3 item whenever you press F2.
    //Lets examine what each line of code is for:

    //This attribute specifies that we have a dependency on R2API, as we're using it to add Bandit to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.
    [BepInPlugin(modGUID, modname, version)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class ExamplePlugin : BaseUnityPlugin
    {

        //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). We like to use the java package notation, which is "com.[your name here].[your plugin name here]"
        public const string modGUID = "com.examplemodder.ExamplePlugin";

        //The modname should be the last thing in the modGUID, this is however not required.
        public const string modname = "ExamplePlugin";

        //The version of the mod, this should go [major versions].[minor versions].[bugfixes/small features]
        public const string version = "1.0.0";

        //The config file for the mod. this allows user configurable setup for the mod.
        //The path normally is contained in the Paths.ConfigPath with the modGUID to prevent conflicts
        //The false makes sure that the config file only exists if you actually have something to save in it.
        internal static ConfigFile configFile = new ConfigFile(Paths.ConfigPath + "\\" + modGUID + ".cfg", false);

        //Stores the config option for later use.
        private static ConfigWrapper<int> TierConfig { get; set; }

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            //Here we are subscribing to the SurvivorCatalogReady event, which is run when the subscriber catalog can be modified.
            //We insert Bandit as a new character here, which is then automatically added to the internal game catalog and reconstructed.
            R2API.SurvivorAPI.SurvivorCatalogReady += (s, e) =>
            {
                var survivor = new SurvivorDef
                {
                    bodyPrefab = BodyCatalog.FindBodyPrefab("BanditBody"),
                    descriptionToken = "BANDIT_DESCRIPTION",
                    displayPrefab = Resources.Load<GameObject>("Prefabs/Characters/BanditDisplay"),
                    primaryColor = new Color(0.8039216f, 0.482352942f, 0.843137264f),
                    unlockableName = "Bandit"
                };

                R2API.SurvivorAPI.AddSurvivorOnReady(survivor);
            };

            //Creates a config option in the catagory "ExampleCatagory" with the name "TierToDrop" and a description and default value of 3.
            //Saves it to TierConfig to allow getting the value later.
            TierConfig = configFile.Wrap("ExampleCatagory", "TierToDrop", "What tier to drop on F2 (1=common, 2=uncommon, 3=rare, 4=lunar)", 3);
        }

        //The Update() method is run on every frame of the game.
        public void Update()
        {
            //This if statement checks if the player has currently pressed F2, and then proceeds into the statement:
            if (Input.GetKeyDown(KeyCode.F2))
            {
                //We are going to grap a list of all available Tier drops based upon the value of the config TierToDrop
                List<PickupIndex> dropList;

                //checks if the value of the config is one of the listed ones and grabs the correct droplist or else goes to default.
                switch (TierConfig.Value)
                {
                    case 1:
                        dropList = Run.instance.availableTier1DropList;
                        break;
                    case 2:
                        dropList = Run.instance.availableTier2DropList;
                        break;
                    case 3:
                        dropList = Run.instance.availableTier3DropList;
                        break;
                    case 4:
                        dropList = Run.instance.availableLunarDropList;
                        break;

                    //if the value is not an accepted one give warning in the console.
                    default:
                        Debug.LogWarning("Invalid value for " + TierConfig.Definition.Key + ": " + TierConfig.Value);
                        dropList = Run.instance.availableTier3DropList;
                        break;
                }


                //Randomly get the next item from the droplist:
                var nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);

                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then finally drop it infront of the player.
                PickupDropletController.CreatePickupDroplet(dropList[nextItem], transform.position, transform.forward * 20f);
            }
        }
    }
}