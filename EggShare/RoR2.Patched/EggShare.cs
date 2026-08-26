using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using System.Runtime.CompilerServices;

namespace RoR2
{
  public struct UniquePickup
  {
    public int eggshare_yolk;

    public PickupIndex pickupIndex;

    public float decayValue;

    public int upgradeValue;
  }
  public class ShopTerminalBehavior
  {

  }
  public struct PickupIndex{}
}

// namespace ItemQualities
// {
//   static class ItemCostQualityPatch
//   {
//     static UniquePickup tryUpgradeQuality(UniquePickup pickup, ShopTerminalBehavior shopTerminalBehavior)
//     {
//       return new UniquePickup {};
//     }
//   }
// }