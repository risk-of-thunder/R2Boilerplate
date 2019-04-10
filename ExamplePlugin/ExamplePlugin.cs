using BepInEx;
using RoR2;
using UnityEngine;

namespace ExamplePlugin
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a very simple plugin that adds Bandit to the game, and gives you a tier 3 item whenever you press F2.
    //Lets examine what each line of code is for:

    //This attribute specifies that we have a dependency on R2API, as we're using it to add Bandit to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency("com.bepis.r2api")]

    //This attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"
    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.
    [BepInPlugin("com.examplemodder.exampleplugin", "ExamplePlugin", "1.0")]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class ExamplePlugin : BaseUnityPlugin
    {
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
                    unlockableName = ""
                };

                R2API.SurvivorAPI.SurvivorDefinitions.Insert(3, survivor);
            };
        }

        //The Update() method is run on every frame of the game.
        public void Update()
        {
            //This if statement checks if the player has currently pressed F2, and then proceeds into the statement:
            if (Input.GetKeyDown(KeyCode.F2))
            {
                //We grab a list of all available Tier 3 drops:
                var dropList = Run.instance.availableTier3DropList;

                //Randomly get the next item:
                var nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);

                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then finally drop it infront of the player.
                PickupDropletController.CreatePickupDroplet(dropList[nextItem], transform.position, transform.forward * 20f);
            }
        }
    }
}