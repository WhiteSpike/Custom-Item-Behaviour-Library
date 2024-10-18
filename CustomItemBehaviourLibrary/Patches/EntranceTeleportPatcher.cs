using CustomItemBehaviourLibrary.AbstractItems;
using CustomItemBehaviourLibrary.Misc;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomItemBehaviourLibrary.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal static class EntranceTeleportPatcher
    {
        [HarmonyPatch(nameof(EntranceTeleport.TeleportPlayerClientRpc))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TeleportPlayerClientRpcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo ToggleStoredItemsBooleans = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.ToggleStoredItemsBooleans));

            FieldInfo IsEntranceToBuilding = typeof(EntranceTeleport).GetField(nameof(EntranceTeleport.isEntranceToBuilding));
            FieldInfo playersManager = typeof(EntranceTeleport).GetField(nameof(EntranceTeleport.playersManager));
            FieldInfo allPlayerScripts = typeof(StartOfRound).GetField(nameof(StartOfRound.allPlayerScripts));
            FieldInfo ItemSlots = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.ItemSlots));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;

            Tools.FindField(ref index, ref codes, findField: IsEntranceToBuilding, skip: true);
            Tools.FindField(ref index, ref codes, findField: IsEntranceToBuilding, skip: true);
            index++;
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Call, operand: ToggleStoredItemsBooleans));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: IsEntranceToBuilding));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldarg_0));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldelem_Ref));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldloc_0));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: ItemSlots));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldelem_Ref));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldarg_1));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: allPlayerScripts));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: playersManager));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldarg_0));
            return codes;
        }

        [HarmonyPatch(nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyTranspiler]
        [HarmonyDebug]
        static IEnumerable<CodeInstruction> TeleportPlayerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo ToggleStoredItemsBooleans = typeof(ContainerBehaviour).GetMethod(nameof(ContainerBehaviour.ToggleStoredItemsBooleans));

            FieldInfo IsEntranceToBuilding = typeof(EntranceTeleport).GetField(nameof(EntranceTeleport.isEntranceToBuilding));
            MethodInfo NetworkInstance = AccessTools.DeclaredPropertyGetter(typeof(GameNetworkManager), nameof(GameNetworkManager.Instance));
            FieldInfo localPlayerController = typeof(GameNetworkManager).GetField(nameof(GameNetworkManager.localPlayerController));
            FieldInfo ItemSlots = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.ItemSlots));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;

            Tools.FindField(ref index, ref codes, findField: IsEntranceToBuilding, skip: true);
            Tools.FindField(ref index, ref codes, findField: IsEntranceToBuilding, skip: true);
            index++;
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Call, operand: ToggleStoredItemsBooleans));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: IsEntranceToBuilding));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldarg_0));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldelem_Ref));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldloc_2));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: ItemSlots));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Ldfld, operand: localPlayerController));
            codes.Insert(index, new CodeInstruction(opcode: OpCodes.Call, operand: NetworkInstance));
            return codes;
        }
    }
}
