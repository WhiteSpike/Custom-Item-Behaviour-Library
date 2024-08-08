using CustomItemBehaviourLibrary.Manager;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace CustomItemBehaviourLibrary.AbstractItems
{
    public abstract class ReplenishSanityBehaviour : GrabbableObject
    {
        /// <summary>
        /// Percentage amount restored when consuming the item
        /// </summary>
        protected float sanityReplenish;
        /// <summary>
        /// Time (in seconds) where the player replenishes sanity upon consuming the item
        /// </summary>
        protected float sanityOvertime;
        /// <summary>
        /// Percentage amount restored during the effect from consuming the item
        /// </summary>
        protected float sanityOvertimeReplenish;
        /// <summary>
        /// Amount of recharges available to use the item
        /// </summary>
        protected int rechargeUsages;
        /// <summary>
        /// Destroy the item if the recharge usages reaches zero
        /// </summary>
        protected bool destroyUponExaustion;

        protected virtual bool CanRestoreSanity(PlayerControllerB player)
        {
            return rechargeUsages > 0;
        }
        protected virtual void RestoreSanity(ref PlayerControllerB player)
        {
            player.insanityLevel = Mathf.Clamp(player.insanityLevel - sanityReplenish*player.maxInsanityLevel, 0f, player.maxInsanityLevel);
            if (sanityOvertime > 0)
            {
                PlayerManager.instance.SetSanityOvertimeReplenish(sanityOvertimeReplenish);
                PlayerManager.instance.AddSanityOvertimeTimer(sanityOvertime);
            }

        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            RestoreSanityServerRpc(new NetworkBehaviourReference(playerHeldBy));
        }

        [ServerRpc(RequireOwnership = false)]
        void RestoreSanityServerRpc(NetworkBehaviourReference player)
        {
            PlayerControllerB playerController;
            if (!player.TryGet(out playerController)) return;
            if (!CanRestoreSanity(playerController)) return;
            RestoreSanityClientRpc(player);
        }

        [ClientRpc]
        void RestoreSanityClientRpc(NetworkBehaviourReference player)
        {
            PlayerControllerB playerController;
            if (!player.TryGet(out playerController)) return;
            rechargeUsages--;
            if (playerController == GameNetworkManager.Instance.localPlayerController)
            {
                RestoreSanity(ref playerController);
                if (rechargeUsages <= 0 && destroyUponExaustion)
                    DestroyObjectInHand(playerHeldBy);
            }
        }
    }
}
