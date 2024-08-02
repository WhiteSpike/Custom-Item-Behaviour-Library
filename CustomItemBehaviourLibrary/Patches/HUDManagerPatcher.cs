using CustomItemBehaviourLibrary.AbstractItems;
using GameNetcodeStuff;
using HarmonyLib;

namespace CustomItemBehaviourLibrary.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HUDManager.MeetsScanNodeRequirements))]
        static void MeetsScanNodeRequirementsPostFix(ScanNodeProperties node, ref bool __result)
        {
            if (node == null) return;
            if (node.transform.parent == null) return;
            GrabbableObject grab = node.transform.parent.GetComponent<GrabbableObject>();
            if (grab == null) return;
            if (grab.gameObject.GetComponentInParent<ContainerBehaviour>() != null) 
            {
                __result = false;
            }
        }
    }
}
