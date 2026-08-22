using BepInEx;
using R2API;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace EggSharePlugin
{
  // This is an example plugin that can be put in
  // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
  // It's a small plugin that adds a relatively simple item to the game,
  // and gives you that item whenever you press F2.

  // This attribute specifies that we have a dependency on a given BepInEx Plugin,
  // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
  // You don't need this if you're not using R2API in your plugin,
  // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
  [BepInDependency(ItemAPI.PluginGUID)]

  // This one is because we use a .language file for language tokens
  // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
  [BepInDependency(LanguageAPI.PluginGUID)]

  // This attribute is required, and lists metadata for your plugin.

  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  // This is the main declaration of our plugin class.
  // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
  // BaseUnityPlugin itself inherits from MonoBehaviour,
  // so you can use this as a reference for what you can declare and use in your plugin class
  // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
  public class EggSharePlugin : BaseUnityPlugin
  {
    // The Plugin GUID should be a unique ID for this plugin,
    // which is human readable (as it is used in places like the config).
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Ap";
    public const string PluginName = "EggShare";
    public const string PluginVersion = "0.0.1";
    // If we see this PluginGUID as it is on thunderstore,
    // we will deprecate this mod.
    // Change the PluginAuthor and the PluginName !
    
    // We need our item definition to persist through our functions, and therefore make it a class field.
    private static ItemDef egg;

  
    // The Awake() method is run at the very start when the game is initialized.
    public void Awake()
    {
      // Init our logging class so that we can properly log for debugging
      Log.Init(Logger);

      // First let's define our item
      // egg = ScriptableObject.CreateInstance<ItemDef>();
      egg = CreateItemDef();
      
      // // Language Tokens, explained there https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
      
      // egg.name              = "EggShare";
      // egg.nameToken         = "EggShare";
      // egg.pickupToken       = "ITEM_EGGSHARE_PICKUP";
      // egg.descriptionToken  = "ITEM_EGGSHARE_DESC";
      // egg.loreToken         = "ITEM_EGGSHARE_LORE";
      // egg.tier              = ItemTier.VoidBoss;
      // egg.tags = [
      //   ItemTag.CommandArtifactBlacklist
      // ];
	    // // egg._itemTierDef.colorIndex = ColorCatalog.ColorIndex.VoidItemDark;
	    // // egg._itemTierDef.darkColorIndex = ColorCatalog.ColorIndex.VoidItem;
	    // // egg._itemTierDef.isDroppable = false;
	    // // egg._itemTierDef.canScrap = false;
	    // // egg._itemTierDef.canRestack = false;
	    // // egg._itemTierDef.canRebirth = false;
	    // // egg._itemTierDef.pickupRules = ItemTierDef.PickupRules.ConfirmAll;
      // // Instead of loading the itemtierdef directly, you can also do this like below as a workaround
      // // egg.deprecatedTier = ItemTier.Tier2;

      // // You can create your own icons and prefabs through assetbundles, but to keep this boilerplate brief, we'll be using question marks.
      // egg.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
      // egg.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
      // egg.canRemove = false;
      // egg.hidden = true;

      // var displayRules = new ItemDisplayRuleDict(null);

      // // Then finally add it to R2API
      // ItemAPI.Add(new CustomItem(egg, displayRules));

      // But now we have defined an item, but it doesn't do anything yet. So we'll need to define that ourselves.
      On.RoR2.ScrapperController.PopRewardPickupQueue += RoR2_ScrapperController_PopRewardPickupQueue;
      On.RoR2.GenericPickupController.GetInfo += RoR2_GenericPickupController_GetInfo;
      On.RoR2.ShopTerminalBehavior.DropPickup_bool += RoR2_ShopTerminalBehavior_DropPickup;
      On.EntityStates.DroneScrapper.DroneScrappingToIdle.DropPickup += EntityStates_DroneScrapper_DroneScrappingToIdle_DropPickup;
      On.EntityStates.MealPrep.CookingToIdle.DropItem += EntityStates_MealPrep_CookingToIdle_DropItem;
      On.RoR2.EquipmentSlot.FireRecycle += RoR2_EquipmentSlot_FireRecycle;

      new Hook(
        typeof(GenericPickupController).GetMethod(nameof(GenericPickupController.AttemptGrant), (BindingFlags)(-1))
      , RoR2_GenericPickupController_AttemptGrant
      , new HookConfig() { Priority = -1 }
      );
      // new Hook(
      //   typeof(ShopTerminalBehavior).GetMethod(nameof(ShopTerminalBehavior.DropPickup), (BindingFlags)(-1))
      // , RoR2_ShopTerminalBehavior_DropPickup
      // , new HookConfig() { Priority = -1 }
      // );
      // GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
    }

    // scrappers handling
    private UniquePickup RoR2_ScrapperController_PopRewardPickupQueue(
      On.RoR2.ScrapperController.orig_PopRewardPickupQueue orig
    , ScrapperController self
    )
    {
      var item =(int)(orig(self).pickupIndex.itemIndex);
      UniquePickup drop =  new UniquePickup {pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)}.MakeEgg(item);
      
      Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = "A special gift just for you!" });
      return drop;
    }
    
    
    private ItemDef CreateItemDef()
    {
      ItemDef itemDef = ScriptableObject.CreateInstance<ItemDef>();

      // Tokens
      
      itemDef.name              = "EggShare";
      itemDef.nameToken         = "EggShare";
      itemDef.pickupToken       = "ITEM_EGGSHARE_PICKUP";
      itemDef.descriptionToken  = "ITEM_EGGSHARE_DESC";
      itemDef.loreToken         = "ITEM_EGGSHARE_LORE";

      // Grab item model from assetbundle (replace with mystery if it doesn't exist. No-tier items not having a prefab is normal)
      GameObject prefab =  Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
      Sprite sprite = itemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();

      // Grab item sprite from assetbundle (replace with mystery if it doesn't exist)
      itemDef.pickupIconSprite = sprite;
      itemDef.pickupModelPrefab = prefab;

      // Item Tier (If item is a DLC tier, make it a DLC requirement)
      itemDef.deprecatedTier = ItemTier.VoidBoss;

      // If this item can be removed by any means in-game (typically for no-tier items)
      itemDef.canRemove = false;

      // If this item cannot be claimed by normal means
      itemDef.hidden = true;

      // Tags to describe the item
      itemDef.tags = [
        ItemTag.CommandArtifactBlacklist
      , ItemTag.AIBlacklist
      , ItemTag.CannotDuplicate
      , ItemTag.CannotSteal
      , ItemTag.CannotCopy
      , ItemTag.IgnoreForDropList
      , ItemTag.SacrificeBlacklist
      ];

      // Display where the item will be kept around the survivor
      var displayRules = new ItemDisplayRuleDict(null);

      // Add it to R2API
      ItemAPI.Add(new CustomItem(itemDef, displayRules));

      // Return the ItemDef
      return itemDef;
    }


    // cauldrons printers handling
    private void RoR2_ShopTerminalBehavior_DropPickup(
      On.RoR2.ShopTerminalBehavior.orig_DropPickup_bool orig
    , ShopTerminalBehavior self
    , bool isDuplicated
    )
    {
      var costType = self.GetComponent<PurchaseInteraction>().costType;
      if (costType == CostTypeIndex.Money || costType == CostTypeIndex.None || isCommandEnabled)
      {
        orig(self, isDuplicated);
        return;
      }
      var shop = self.GetComponent<ShopTerminalBehavior>();
      UniquePickup old = shop.CurrentPickup() with {};
      ItemIndex item = PickupCatalog.GetPickupDef(shop.CurrentPickupIndex())?.itemIndex ?? ItemIndex.None;
      
      if(item == ItemIndex.None )
      {
        orig(self,isDuplicated); 
      } else {
        UniquePickup eggified = old with {
          pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
        };
        self.SetPickup(
          eggified.MakeEgg((int)item)
        , self.Networkhidden
        );
        orig(self,isDuplicated);
        self.SetPickup(
          old
        , self.Networkhidden
        );
      }
        return;
    }

    // Drone scrapper handling
    private void EntityStates_DroneScrapper_DroneScrappingToIdle_DropPickup(
      // System.Action<EntityStates.DroneScrapper.DroneScrappingToIdle> orig
      On.EntityStates.DroneScrapper.DroneScrappingToIdle.orig_DropPickup orig
    , EntityStates.DroneScrapper.DroneScrappingToIdle self
    ) 
    {
      self.droppedScrap++;
      if ((bool)EntityStates.DroneScrapper.DroneScrappingToIdle.muzzleflashEffectPrefab)
      {
        EffectManager.SimpleMuzzleFlash(EntityStates.DroneScrapper.DroneScrappingToIdle.muzzleflashEffectPrefab, base.gameObject, EntityStates.DroneScrapper.DroneScrappingToIdle.muzzleString, transmit: false);
      }
      if (UnityEngine.Networking.NetworkServer.active)
      {
        self.foundValidScrap = false;
        PickupIndex pickupIndex = PickupCatalog.FindScrapIndexForItemTier(self.scrapperController.lastScrappedTier);
        if (pickupIndex != PickupIndex.none)
        {
          self.foundValidScrap = true;
          Transform transform = self.FindModelChild(EntityStates.DroneScrapper.DroneScrappingToIdle.ejectionString);
          UniquePickup eggified = new UniquePickup {
            pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
          }.MakeEgg((int)pickupIndex.itemIndex);
          PickupDropletController.CreatePickupDroplet(eggified, transform.position, base.transform.TransformVector(EntityStates.DroneScrapper.DroneScrappingToIdle.dropVelocity), isDuplicated: false);
        }
      }
    }
    private void EntityStates_MealPrep_CookingToIdle_DropItem(
      On.EntityStates.MealPrep.CookingToIdle.orig_DropItem orig
    , EntityStates.MealPrep.CookingToIdle self
    )
    {
      if ((bool)EntityStates.MealPrep.CookingToIdle.muzzleflashEffectPrefab)
      {
        EffectManager.SimpleMuzzleFlash(EntityStates.MealPrep.CookingToIdle.muzzleflashEffectPrefab, base.gameObject, EntityStates.MealPrep.CookingToIdle.muzzleString, transmit: false);
      }
      if (UnityEngine.Networking.NetworkServer.active)
      {
        Transform transform = self.FindModelChild(EntityStates.MealPrep.CookingToIdle.muzzleString);
        UniquePickup eggified = new UniquePickup {
          pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
        }.MakeEgg((int)self.pickupToDrop.itemIndex);
        PickupDropletController.CreatePickupDroplet(eggified, transform.position, Vector3.up * EntityStates.MealPrep.CookingToIdle.dropUpVelocityStrength + transform.forward * EntityStates.MealPrep.CookingToIdle.dropForwardVelocityStrength);
        self.itemsDropped++;
      }
    }
    // on grant
    private static void RoR2_GenericPickupController_AttemptGrant(
      System.Action<GenericPickupController,CharacterBody> orig
    , GenericPickupController self
    , CharacterBody body
    )
    {
      if(self.pickup.pickupIndex.itemIndex != egg.itemIndex)
      {
        orig(self, body);
        return;
      }

      UniquePickup originalItem = self.pickup;
      UniquePickup yolk = originalItem with {pickupIndex = PickupCatalog.FindPickupIndex((ItemIndex)(originalItem.GetYolk()))};

      self.pickup = yolk with {};
      orig(self, body);
    }

    // Inspecting handling
    private InspectInfo RoR2_GenericPickupController_GetInfo(
      On.RoR2.GenericPickupController.orig_GetInfo orig
    , GenericPickupController self
    )
    {
      if(self.pickup.pickupIndex != PickupCatalog.FindPickupIndex(egg.itemIndex))
      {
        return orig(self);
      }
      InspectInfo inspectInfo = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex((ItemIndex)self.pickup.GetYolk()));
      inspectInfo.upgradeCount = self.pickup.upgradeValue;
      return inspectInfo;
    }
    
    private static bool isCommandEnabled => RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Command);

    private bool RoR2_EquipmentSlot_FireRecycle(
      On.RoR2.EquipmentSlot.orig_FireRecycle orig,
      EquipmentSlot self
    )
    {
      GenericPickupController pickupController = self.currentTarget.pickupController;
      UniquePickup oldEgg = pickupController.pickup;
      
      if(oldEgg.pickupIndex != PickupCatalog.FindPickupIndex(egg.itemIndex))
      {
        return orig(self);
      }

      UniquePickup yolk = oldEgg with {pickupIndex = PickupCatalog.FindPickupIndex((ItemIndex)oldEgg.GetYolk())};

      pickupController.pickup = yolk;
      bool result = orig(self);
      UniquePickup newYolk = pickupController.pickup;
      UniquePickup newEgg = newYolk with {pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)};
      pickupController.pickup = newEgg.MakeEgg((int)newYolk.pickupIndex.itemIndex);
      return result;
    }

    // The Update() method is run on every frame of the game.
    private void Update()
    {
      // This if statement checks if the player has currently pressed F2.
      if (Input.GetKeyDown(KeyCode.F2))
      {
        var message = "Hi! You pressed F2 :thumbsup:";
        Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = message });
      }
    }
  }
}
