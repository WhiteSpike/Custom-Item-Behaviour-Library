using CustomItemBehaviourLibrary.AbstractItems;
using CustomItemBehaviourLibrary.Manager;
using CustomItemBehaviourLibrary.Misc;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace CustomItemBehaviourLibrary.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerControllerBPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.Awake))]
        static void StartPostfix(PlayerControllerB __instance)
        {
            __instance.gameObject.AddComponent<PlayerManager>();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.Update))]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo reduceMovement = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.CheckIfPlayerCarryingWheelbarrowMovement));
            FieldInfo carryWeight = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.carryWeight));
            int index = 0;
            List<CodeInstruction> codes = new(instructions);
            Tools.FindField(ref index, ref codes, findField: carryWeight, skip: true, errorMessage: "Couldn't find first carryWeight occurence");
            Tools.FindField(ref index, ref codes, findField: carryWeight, addCode: reduceMovement, errorMessage: "Couldn't find second carryWeight occurence");
            Tools.FindField(ref index, ref codes, findField: carryWeight, addCode: reduceMovement, errorMessage: "Couldn't find second carryWeight occurence");
            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.PlayerLookInput))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PlayerLookInputTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo reduceLookSensitivity = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.CheckIfPlayerCarryingWheelbarrowLookSensitivity));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindFloat(ref index, ref codes, findValue: 0.008f, addCode: reduceLookSensitivity, errorMessage: "Couldn't find look sensitivity value we wanted to influence");
            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.Crouch_performed))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CrouchPerformmedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo carryingWheelbarrow = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.CheckIfPlayerCarryingWheelbarrow));
            FieldInfo isMenuOpen = typeof(QuickMenuManager).GetField(nameof(QuickMenuManager.isMenuOpen));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindField(ref index, ref codes, findField: isMenuOpen, addCode: carryingWheelbarrow, orInstruction: true, errorMessage: "Couldn't find isMenuOpen field");
            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.GrabObjectClientRpc))]
        static void GrabObjectClientRpcPostfix(PlayerControllerB __instance)
        {
            ContainerUnparenting(__instance.currentlyHeldObjectServer);
        }
        static void ContainerUnparenting(GrabbableObject heldObject)
        {
            if (heldObject == null) return;

            ContainerBehaviour container = heldObject.GetComponentInParent<ContainerBehaviour>();
            if (container == null || heldObject is ContainerBehaviour) return;
            heldObject.transform.SetParent(heldObject.parentObject);
            heldObject.transform.localScale = heldObject.originalScale;
            container.DecrementStoredItems();
        }
    }
}
