using Mono.Cecil;
using System;
using System.Collections.Generic;
namespace EggSharePlugin;

internal static class EggSharePatcher
{
  public static IEnumerable<string> TargetDLLs
  {
    get
    {
      yield return "RoR2.dll";
    }
  }

  public static void Patch(AssemblyDefinition assemblyDef)
  {
    var uPickup = assemblyDef.MainModule.GetType("RoR2","UniquePickup");
    // var fieldType = assemblyDef.MainModule.ImportReference(typeof(int));
    // var fieldDef = new FieldDefinition("eggshare_yolk", FieldAttributes.Public, fieldType);
    // fieldDef.Offset = 0;
    // uPickup.Fields.Add(fieldDef);
    uPickup.Fields.Add(new FieldDefinition("eggshare_yolk", FieldAttributes.Public, assemblyDef.MainModule.ImportReference(typeof(int))));
  }
}