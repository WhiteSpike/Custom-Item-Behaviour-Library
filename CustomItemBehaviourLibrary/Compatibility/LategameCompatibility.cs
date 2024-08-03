using CustomItemBehaviourLibrary.AbstractItems;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.AttributeUpgrades;
using System;
using UnityEngine;

namespace CustomItemBehaviourLibrary.Compatibility
{
    internal static class LategameCompatibility
    {
        public static bool Enabled =>
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades");

        internal static void AddWeight(ContainerBehaviour containerBehaviour)
        {
            containerBehaviour.playerHeldBy.carryWeight -= Mathf.Clamp(BackMuscles.DecreasePossibleWeight(containerBehaviour.itemProperties.weight - 1f), 0, 10f);
            containerBehaviour.playerHeldBy.carryWeight += Mathf.Clamp(BackMuscles.DecreasePossibleWeight(containerBehaviour.totalWeight - 1f), 0, 10f);
        }
        internal static void AddTotalWeight(ContainerBehaviour containerBehaviour)
        {
            containerBehaviour.playerHeldBy.carryWeight += Mathf.Clamp(BackMuscles.DecreasePossibleWeight(containerBehaviour.totalWeight - 1f), 0f, 10f);
        }

        internal static void RemoveTotalWeight(ContainerBehaviour containerBehaviour)
        {
            containerBehaviour.playerHeldBy.carryWeight -= Mathf.Clamp(BackMuscles.DecreasePossibleWeight(containerBehaviour.totalWeight - 1f), 0f, 10f);
        }

        internal static void RemoveWeight(ContainerBehaviour containerBehaviour)
        {
            containerBehaviour.playerHeldBy.carryWeight += Mathf.Clamp(BackMuscles.DecreasePossibleWeight(containerBehaviour.itemProperties.weight - 1f), 0, 10f);
            containerBehaviour.playerHeldBy.carryWeight -= Mathf.Clamp(BackMuscles.DecreasePossibleWeight(containerBehaviour.totalWeight - 1f), 0, 10f);
        }
    }
}
