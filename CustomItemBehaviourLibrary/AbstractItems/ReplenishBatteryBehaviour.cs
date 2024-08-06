using Unity.Netcode;
using UnityEngine;

namespace CustomItemBehaviourLibrary.AbstractItems
{
    /// <summary>
    /// Behaviour which acts as an item that can replenish items' battery life if they use so
    /// </summary>
    public abstract class ReplenishBatteryBehaviour : GrabbableObject
    {
        /// <summary>
        /// The percentage of battery life it replenishes when used on other items.
        /// </summary>
        protected float rechargePercentage = 1f;
        /// <summary>
        /// Amount of items the item can be used to replenish battery life on other items.
        /// </summary>
        protected float rechargeUsages = 1;
        /// <summary>
        /// When recharging an item, its battery life can go over 100%
        /// </summary>
        protected bool canOvercharge = false;
        /// <summary>
        /// When the item runs out of usages to recharge, it makes the item disappear.
        /// </summary>
        protected bool destroyUponOutOfUsages = false;

        /// <summary>
        /// </summary>
        /// <returns>If the item is capable of recharging items or not</returns>
        protected abstract bool CanRechargeItems();
        /// <summary>
        /// Looks for a GrabbableObject object to recharge its battery life
        /// </summary>
        /// <returns>The GrabbableObject that the behaviour found to recharge or null if nothing was found</returns>
        protected abstract GrabbableObject GrabItemToRecharge();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="grabbableObject">The GrabbableObject object that we wish to recharge the battery of</param>
        /// <returns>We can replenish the provided item's battery life or not</returns>
        protected abstract bool CanRechargeItem(GrabbableObject grabbableObject);
        [ServerRpc(RequireOwnership = false)]
        void RechargeItemServerRpc(NetworkBehaviourReference grabbableObject)
        {
            RechargeItemClientRpc(grabbableObject);
        }
        [ClientRpc]
        void RechargeItemClientRpc(NetworkBehaviourReference grabbableObject)
        {
            grabbableObject.TryGet(out GrabbableObject grabbableObject1);
            RechargeItem(grabbableObject1);
        }
        /// <summary>
        /// Replenishes the provided item's battery life
        /// </summary>
        /// <param name="grabbableObject">The item we replenish the battery life of</param>
        protected virtual void RechargeItem(GrabbableObject grabbableObject)
        {
            grabbableObject.insertedBattery.charge = Mathf.Clamp(grabbableObject.insertedBattery.charge + rechargePercentage, 0f, canOvercharge ? float.MaxValue : 1f);
            grabbableObject.insertedBattery.empty = false;
            rechargeUsages--;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (!CanRechargeItems())
            {
                if (destroyUponOutOfUsages)
                    DestroyObjectInHand(playerHeldBy);
                return;
            }
            GrabbableObject grabbableObject = GrabItemToRecharge();
            if (grabbableObject == null || !CanRechargeItem(grabbableObject)) return;
            if (IsServer)
                RechargeItemClientRpc(new NetworkBehaviourReference(grabbableObject));
            else RechargeItemServerRpc(new NetworkBehaviourReference(grabbableObject));
        }
    }
}
