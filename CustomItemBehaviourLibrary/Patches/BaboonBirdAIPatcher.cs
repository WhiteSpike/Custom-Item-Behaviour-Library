﻿using CustomItemBehaviourLibrary.AbstractItems;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(BaboonBirdAI))]
    internal static class BaboonBirdAIPatcher
    {
        [HarmonyPatch(nameof(BaboonBirdAI.DoLOSCheck))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DoLOSCheckTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            PatchCheckItemInContainer(ref index, ref codes);
            return codes;
        }

        /// <summary>
        /// Adds a condition for the baboon bird to pick up the item only if it's not deposited in a wheelbarrow.
        /// If it is, they will not focus on that item and look for alternatives.
        /// This should solve the issues where the baboon birds would camp a wheelbarrow if it had any items in it
        /// </summary>
        /// <param name="index">Current index transpiling through the code instructions of a given method</param>
        /// <param name="codes">Code instructions of a given method</param>
        /// <returns>Index in which it found the necessary code instruction to make replacements or the end if it didn't find any (this means that our comparisons are wrong)</returns>
        private static void PatchCheckItemInContainer(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo checkIfInContainer = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.CheckIfItemInContainer));
            for (; index < codes.Count; index++)
            {
                if (!(codes[index].opcode == OpCodes.Ldloc_S && codes[index].operand.ToString() == "GrabbableObject (18)")) continue;
                if (codes[index + 1].opcode != OpCodes.Ldnull) continue;
                codes.Insert(index + 3, new CodeInstruction(OpCodes.And));
                codes.Insert(index + 3, new CodeInstruction(OpCodes.Not));
                codes.Insert(index + 3, new CodeInstruction(OpCodes.Call, checkIfInContainer));
                codes.Insert(index + 3, new CodeInstruction(OpCodes.Ldloc_S, codes[index].operand));
                break;
            }
            index++;
        }
    }
}
