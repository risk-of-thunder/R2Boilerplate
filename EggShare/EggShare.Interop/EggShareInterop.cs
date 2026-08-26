using RoR2;
// using RoR2.UI;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;

[assembly: InternalsVisibleTo("EggSharePlugin")]

namespace EggSharePlugin;

public static class EggShareInterop
{
  public static UniquePickup MakeEgg(this UniquePickup original, int yolk)
  {
    UniquePickup egg = original with {eggshare_yolk = yolk};
    return egg;
  } 
  public static int GetYolk(this UniquePickup up)
  {
    return up.eggshare_yolk;
  }
}
