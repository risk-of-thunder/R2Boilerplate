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
using System.Runtime.CompilerServices;
using System.Security;
using HarmonyLib;
using System.Security.Permissions;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace EggSharePlugin
{
  [BepInDependency(ItemAPI.PluginGUID)]
  [BepInDependency(LanguageAPI.PluginGUID)]

  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class EggSharePlugin : BaseUnityPlugin
  {
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Ap";
    public const string PluginName = "EggShare";
    public const string PluginVersion = "0.0.1";
    
    private static ItemDef egg;

    public void Awake()
    {
      egg = CreateItemDef();
      
      // // Language Tokens, explained there https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
      
      On.RoR2.ScrapperController.PopRewardPickupQueue += RoR2_ScrapperController_PopRewardPickupQueue;
      On.RoR2.GenericPickupController.GetInfo += RoR2_GenericPickupController_GetInfo;
      IL.RoR2.ShopTerminalBehavior.DropPickup_bool += RoR2_ShopTerminalBehavior_DropPickup;
      IL.EntityStates.DroneScrapper.DroneScrappingToIdle.DropPickup += EntityStates_DroneScrapper_DroneScrappingToIdle_DropPickup;
      IL.EntityStates.MealPrep.CookingToIdle.DropItem += EntityStates_MealPrep_CookingToIdle_DropItem;
      On.RoR2.EquipmentSlot.FireRecycle += RoR2_EquipmentSlot_FireRecycle;

      new Hook(
        typeof(GenericPickupController).GetMethod(nameof(GenericPickupController.AttemptGrant), (BindingFlags)(-1))
      , RoR2_GenericPickupController_AttemptGrant
      , new HookConfig() { Priority = -1 }
      );
      // new Hook(
      //   HarmonyLib.AccessTools.Method(typeof(ItemQualities.ItemCostQualityPatch), nameof(ItemQualities.ItemCostQualityPatch.ShopTerminalBehavior_DropPickup.tryUpgradeQuality))
      // , ItemQualities_ItemCostQualityPatch_tryUpgradeQuality
      // );
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
    // IL hook version
    private void RoR2_ShopTerminalBehavior_DropPickup(
      ILContext il
    ) 
    {
      ILCursor val = new ILCursor(il);
      if (!val.TryGotoNext(
        (Instruction x) => ILPatternMatchingExt.MatchLdarg(x,0)
      , (Instruction x) => ILPatternMatchingExt.MatchLdfld<ShopTerminalBehavior>(x, "dropTransform")
      ))
      { 
        return;
      }
      val.EmitDelegate<System.Func<UniquePickup, UniquePickup>>((System.Func<UniquePickup, UniquePickup>)eggstraterrestrial);
      static UniquePickup eggstraterrestrial(UniquePickup pickup)
      {
        if (pickup.isValid)
        {
          UniquePickup eggified = pickup with {
            pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
          };
          return eggified.MakeEgg((int)pickup.pickupIndex.itemIndex);
        }
        return pickup;
      }
    }
    // On hook version
    // private void RoR2_ShopTerminalBehavior_DropPickup(
    //   On.RoR2.ShopTerminalBehavior.orig_DropPickup_bool orig
    // , ShopTerminalBehavior self
    // , bool isDuplicated
    // )
    // {
    //   var costType = self.GetComponent<PurchaseInteraction>().costType;
    //   if (costType == CostTypeIndex.Money || costType == CostTypeIndex.None || isCommandEnabled)
    //   {
    //     orig(self, isDuplicated);
    //     return;
    //   }
    //   var shop = self.GetComponent<ShopTerminalBehavior>();
    //   UniquePickup old = shop.CurrentPickup() with {};
    //   ItemIndex item = PickupCatalog.GetPickupDef(shop.CurrentPickupIndex())?.itemIndex ?? ItemIndex.None;
      
    //   if(item == ItemIndex.None )
    //   {
    //     orig(self,isDuplicated); 
    //   } else {
    //     UniquePickup eggified = old with {
    //       pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
    //     };
    //     self.SetPickup(
    //       eggified.MakeEgg((int)item)
    //     , self.Networkhidden
    //     );
    //     orig(self,isDuplicated);
    //     self.SetPickup(
    //       old
    //     , self.Networkhidden
    //     );
    //   }
    //     return;
    // }

    // Drone scrapper handling
    private void EntityStates_DroneScrapper_DroneScrappingToIdle_DropPickup(
      ILContext il
    )
    {
      ILCursor val = new ILCursor(il);
      if (!val.TryGotoNext( 
        MoveType.After
      , new System.Func<Instruction,bool>[1]
        {
          (Instruction x) => ILPatternMatchingExt.MatchNewobj<UniquePickup>(x)
        }
      ))
      { 
        return;
      }
      val.EmitDelegate<System.Func<UniquePickup, UniquePickup>>((System.Func<UniquePickup, UniquePickup>)eggstraterrestrial);
      static UniquePickup eggstraterrestrial(UniquePickup pickup)
      {
        if (pickup.isValid)
        {
          UniquePickup eggified = pickup with {
            pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
          };
          return eggified.MakeEgg((int)pickup.pickupIndex.itemIndex);
        }
        return pickup;
      }

      // self.droppedScrap++;
      // if ((bool)EntityStates.DroneScrapper.DroneScrappingToIdle.muzzleflashEffectPrefab)
      // {
      //   EffectManager.SimpleMuzzleFlash(EntityStates.DroneScrapper.DroneScrappingToIdle.muzzleflashEffectPrefab, base.gameObject, EntityStates.DroneScrapper.DroneScrappingToIdle.muzzleString, transmit: false);
      // }
      // if (UnityEngine.Networking.NetworkServer.active)
      // {
      //   self.foundValidScrap = false;
      //   PickupIndex pickupIndex = PickupCatalog.FindScrapIndexForItemTier(self.scrapperController.lastScrappedTier);
      //   if (pickupIndex != PickupIndex.none)
      //   {
      //     self.foundValidScrap = true;
      //     Transform transform = self.FindModelChild(EntityStates.DroneScrapper.DroneScrappingToIdle.ejectionString);
      //     UniquePickup eggified = new UniquePickup {
      //       pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
      //     }.MakeEgg((int)pickupIndex.itemIndex);
      //     PickupDropletController.CreatePickupDroplet(eggified, transform.position, base.transform.TransformVector(EntityStates.DroneScrapper.DroneScrappingToIdle.dropVelocity), isDuplicated: false);
      //   }
      // }
    }
    // cooking handling
    // IL hook version
    private void EntityStates_MealPrep_CookingToIdle_DropItem(
      ILContext il
    )
    {
      ILCursor val = new ILCursor(il);
      if (!val.TryGotoNext( 
        MoveType.After
      , new System.Func<Instruction,bool>[1]
        {
          (Instruction x) => ILPatternMatchingExt.MatchLdfld<EntityStates.MealPrep.MealPrepBaseState>(x, "pickupToDrop")
        }
      ))
      { 
        return;
      }
      val.EmitDelegate<System.Func<UniquePickup, UniquePickup>>((System.Func<UniquePickup, UniquePickup>)eggstraterrestrial);
      static UniquePickup eggstraterrestrial(UniquePickup pickup)
      {
        if (pickup.isValid)
        {
          UniquePickup eggified = pickup with {
            pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
          };
          return eggified.MakeEgg((int)pickup.pickupIndex.itemIndex);
        }
        return pickup;
      }
    }
    // // On hook version
    // private void EntityStates_MealPrep_CookingToIdle_DropItem(
    //   On.EntityStates.MealPrep.CookingToIdle.orig_DropItem orig
    // , EntityStates.MealPrep.CookingToIdle self
    // )
    // {
    //   if ((bool)EntityStates.MealPrep.CookingToIdle.muzzleflashEffectPrefab)
    //   {
    //     EffectManager.SimpleMuzzleFlash(EntityStates.MealPrep.CookingToIdle.muzzleflashEffectPrefab, base.gameObject, EntityStates.MealPrep.CookingToIdle.muzzleString, transmit: false);
    //   }
    //   if (UnityEngine.Networking.NetworkServer.active)
    //   {
    //     Transform transform = self.FindModelChild(EntityStates.MealPrep.CookingToIdle.muzzleString);
    //     UniquePickup eggified = new UniquePickup {
    //       pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)
    //     }.MakeEgg((int)self.pickupToDrop.itemIndex);
    //     PickupDropletController.CreatePickupDroplet(eggified, transform.position, Vector3.up * EntityStates.MealPrep.CookingToIdle.dropUpVelocityStrength + transform.forward * EntityStates.MealPrep.CookingToIdle.dropForwardVelocityStrength);
    //     self.itemsDropped++;
    //   }
    // }
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

    // static UniquePickup ItemQualities_ItemCostQualityPatch_tryUpgradeQuality(
    //   System.Func<UniquePickup,ShopTerminalBehavior,UniquePickup> orig
    // , UniquePickup pickup
    // , ShopTerminalBehavior shopTerminalBehavior)
		// {
		// 	if (pickup.isValid && pickup.pickupIndex == PickupCatalog.FindPickupIndex(egg.itemIndex) )
		// 	{
    //     UniquePickup yolk = pickup with {pickupIndex = PickupCatalog.FindPickupIndex((ItemIndex)pickup.GetYolk())};

    //     UniquePickup newYolk = orig(yolk,shopTerminalBehavior);
    //     UniquePickup newEgg = newYolk with {pickupIndex = PickupCatalog.FindPickupIndex(egg.itemIndex)};
    //     return newEgg.MakeEgg((int)newYolk.pickupIndex.itemIndex);
		// 	}
    //   else
    //   {
    //     return orig(pickup,shopTerminalBehavior);
    //   }
		// }

    // The Update() method is run on every frame of the game.
    private void Update()
    {
      // This if statement checks if the player has currently pressed F2.
      if (Input.GetKeyDown(KeyCode.F2))
      {
        var message = "";
        Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = message });
      }
    }
  }
}
