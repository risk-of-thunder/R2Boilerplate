using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using RoR2;
using UnityEngine;
using Console = On.RoR2.Console;

namespace PlayerMultiplier
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.ror2.playermultiplier", "PlayerMultiplier", "1.0")]
    public class PlayerMultiplier : BaseUnityPlugin
    {
        public static int Multiplier = 4;

        public void Awake()
        {
            // Needed to register our commands as well when the game registers its own
            IL.RoR2.Console.Awake += il =>
            {
                var c = il.At(0);
                c.GotoNext(x => x.MatchStloc(1));
                c.EmitDelegate<Func<Type[], Type[]>>(orig => orig.Concat(GetType().Assembly.GetTypes()).ToArray());
            };

            IL.RoR2.Run.FixedUpdate += il =>
            {
                var c = il.At(0);
                c.GotoNext(x => x.MatchCallvirt<Run>("set_livingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => x * Multiplier);

                c.GotoNext(x => x.MatchCallvirt<Run>("set_participatingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => x * Multiplier);
            };
        }

        // Random example command to set multiplier with
        [ConCommand(commandName = "wb_set_multiplier", flags = ConVarFlags.None, helpText = "Toggles game pause state.")]
        private static void CCSetMultiplier(ConCommandArgs args)
        {
            if (args.Count != 1 || !int.TryParse(args[0], out Multiplier))
                Debug.Log("Invalid arguments.");
            else
                Debug.Log($"Multiplier set to {Multiplier}. Good luck!");
        }
    }
}