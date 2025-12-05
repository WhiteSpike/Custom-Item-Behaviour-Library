using CustomItemBehaviourLibrary.AbstractItems;
using CustomItemBehaviourLibrary.Compatibility;
using CustomItemBehaviourLibrary.Manager;
using CustomItemBehaviourLibrary.Misc;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomItemBehaviourLibrary.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerControllerBPatcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerControllerB.DropAllHeldItems))]
        static bool DontDropItems(PlayerControllerB __instance)
        {
            if (!PortableTeleporterBehaviour.TPButtonPressed) return true;

            PortableTeleporterBehaviour.TPButtonPressed = false;
            __instance.isSinking = false;
            __instance.isUnderwater = false;
            __instance.sinkingValue = 0;
            __instance.statusEffectAudio.Stop();
            return false;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.Awake))]
        static void StartPostfix(PlayerControllerB __instance)
        {
            __instance.gameObject.AddComponent<PlayerManager>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        static void KillPlayerPostfix(PlayerControllerB __instance)
        {
            if (__instance != GameNetworkManager.Instance.localPlayerController) return;
            if (PlayerManager.instance.holdingContainer)
            {
                if (LategameCompatibility.Enabled)
                {
                    foreach (GrabbableObject grabbableObject in __instance.ItemSlots)
                    {
                        ContainerBehaviour container = grabbableObject.GetComponent<ContainerBehaviour>();
                        if (container == null) continue;
                        container.UpdatePlayerAttributes(grabbing: false);
                        return;
                    }
                }
                else
                {
                    ContainerBehaviour container = __instance.currentlyHeldObjectServer.GetComponent<ContainerBehaviour>();
                    container.UpdatePlayerAttributes(grabbing: false);
                }
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.SetPlayerSanityLevel))]
        static IEnumerable<CodeInstruction> SetPlayerSanityLevelTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo decreaseSanityIncrease = typeof(PlayerManager).GetMethod(nameof(PlayerManager.DecreaseSanityIncrease));

            int index = 0;
            List<CodeInstruction> codes = new(instructions);

            Tools.FindFloat(ref index, ref codes, findValue: 0.8f, addCode: decreaseSanityIncrease, errorMessage: "Couldn't find first insanity multiplier");
            Tools.FindFloat(ref index, ref codes, findValue: 0.2f, addCode: decreaseSanityIncrease, errorMessage: "Couldn't find first insanity multiplier");
            Tools.FindFloat(ref index, ref codes, findValue: -2f, addCode: decreaseSanityIncrease, errorMessage: "Couldn't find first insanity multiplier");
            Tools.FindFloat(ref index, ref codes, findValue: 0.5f, addCode: decreaseSanityIncrease, errorMessage: "Couldn't find first insanity multiplier");
            Tools.FindFloat(ref index, ref codes, findValue: 0.3f, addCode: decreaseSanityIncrease, errorMessage: "Couldn't find first insanity multiplier");
            Tools.FindFloat(ref index, ref codes, findValue: -3f, addCode: decreaseSanityIncrease, errorMessage: "Couldn't find first insanity multiplier");

            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.Update))]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo reduceMovement = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.CheckIfPlayerCarryingContainerMovement));
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
            MethodInfo reduceLookSensitivity = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.CheckIfPlayerCarryingContainerLookSensitivity));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindFloat(ref index, ref codes, findValue: 0.008f, addCode: reduceLookSensitivity, errorMessage: "Couldn't find look sensitivity value we wanted to influence");
            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.Crouch_performed))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CrouchPerformmedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo carryingContainer = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.CheckIfPlayerCarryingContainer));
            FieldInfo isMenuOpen = typeof(QuickMenuManager).GetField(nameof(QuickMenuManager.isMenuOpen));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindField(ref index, ref codes, findField: isMenuOpen, addCode: carryingContainer, orInstruction: true, errorMessage: "Couldn't find isMenuOpen field");
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyDebug]
        [HarmonyPatch(nameof(PlayerControllerB.GrabObjectClientRpc))]
        static IEnumerable<CodeInstruction> GrabObjectClientRpcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo ContainerUnparentTranspiledFunction = typeof(PlayerControllerBPatcher).GetMethod(nameof(PlayerControllerBPatcher.ContainerUnparentTranspiledFunction));
            MethodInfo SwitchToItemSlot = typeof(PlayerControllerB).GetMethod(nameof(PlayerControllerB.SwitchToItemSlot), BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo currentlyHeldObjectServer = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.currentlyHeldObjectServer));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindMethod(ref index, ref codes, findMethod: SwitchToItemSlot, skip: true);
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Call, operand: ContainerUnparentTranspiledFunction));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: currentlyHeldObjectServer));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldarg_0));
            return codes;
        }
        internal static void ContainerUnparenting(GrabbableObject heldObject)
		{
			if (heldObject == null) return;

            ContainerBehaviour container = heldObject.GetComponentInParent<ContainerBehaviour>();
            if (container == null || heldObject is ContainerBehaviour) return;
            heldObject.transform.SetParent(heldObject.parentObject);
            heldObject.transform.localScale = heldObject.originalScale;
            container.DecrementStoredItems();
        }

        public static void ContainerUnparentTranspiledFunction(GrabbableObject currentlyHeldObjectServer)
        {
            ContainerUnparenting(currentlyHeldObjectServer);
        }
    }
}
