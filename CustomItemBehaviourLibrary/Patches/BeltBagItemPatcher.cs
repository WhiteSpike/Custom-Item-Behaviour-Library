using CustomItemBehaviourLibrary.AbstractItems;
using CustomItemBehaviourLibrary.Misc;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomItemBehaviourLibrary.Patches
{
    [HarmonyPatch(typeof(BeltBagItem))]
    internal static class BeltBagItemPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeltBagItem.PutObjectInBagLocalClient))]
        static void GrabObjectClientRpcPostfix(GrabbableObject gObject)
        {
            PlayerControllerBPatcher.ContainerUnparenting(gObject);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeltBagItem.ItemInteractLeftRight))]
        static IEnumerable<CodeInstruction> ItemInteractLeftRightTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo isHeldByEnemy = typeof(GrabbableObject).GetField(nameof(GrabbableObject.isHeldByEnemy));
            MethodInfo isContainer = typeof(BeltBagItemPatcher).GetMethod(nameof(BeltBagItemPatcher.IsContainer));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;

            Tools.FindField(ref index, ref codes, findField: isHeldByEnemy, addCode: isContainer, orInstruction: true, errorMessage: "Couldn't find the isHeldByEnemy field");
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldloc_1));
            return codes;
        }

        public static bool IsContainer(GrabbableObject gObject)
        {
            return gObject.GetComponent<ContainerBehaviour>() != null;
        }
    }
}
