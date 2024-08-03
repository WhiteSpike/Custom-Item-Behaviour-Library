using CustomItemBehaviourLibrary.Compatibility;
using CustomItemBehaviourLibrary.Manager;
using GameNetcodeStuff;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.AttributeUpgrades;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CustomItemBehaviourLibrary.AbstractItems
{
    public abstract class ContainerBehaviour : GrabbableObject
    {
        internal const float VELOCITY_APPLY_EFFECT_THRESHOLD = 5.0f;
        public enum Restrictions
        {
            None,
            TotalWeight,
            ItemCount,
            All,
        }
        protected Restrictions restriction;
        private System.Random randomNoise;
        /// <summary>
        /// Component responsible to emit sound when the container's moving
        /// </summary>
        private AudioSource wheelsNoise;
        /// <summary>
        /// The sound the container will be playing while in movement
        /// </summary>
        protected AudioClip[] wheelsClip;
        /// <summary>
        /// How far the sound can be heard from nearby enemies
        /// </summary>
        protected float noiseRange;
        /// <summary>
        /// How long the last sound was played
        /// </summary>
        private float soundCounter;
        /// <summary>
        /// Maximum amount of items the container allows to store inside
        /// </summary>
        protected int maximumAmountItems;
        /// <summary>
        /// Maximum weight allowed to be stored in the container
        /// </summary>
        protected float maximumWeightAllowed;
        /// <summary>
        /// Current amount of items stored in the container
        /// </summary>
        private int currentAmountItems;
        internal float totalWeight;
        /// <summary>
        /// Multiplier to the inserted item's weight when inserted into the container to increase the container's total weight
        /// </summary>
        protected float weightReduceMultiplier;
        /// <summary>
        /// Weight of the container when not carrying any items
        /// </summary>
        protected float defaultWeight;
        /// <summary>
        /// How sloppy the player's movement is when moving with a container
        /// </summary>
        protected float sloppiness;
        /// <summary>
        /// Value multiplied on the look sensitivity of the player who's carrying the container
        /// </summary>
        protected float lookSensitivityDrawback;
        /// <summary>
        /// The GameObject responsible to be containing all of the items stored in the container
        /// </summary>
        private BoxCollider container;
        /// <summary>
        /// Trigger responsible to allow interacting with container's container of items
        /// </summary>
        private InteractTrigger[] triggers;
        protected bool playSounds;
        private Dictionary<Restrictions, Func<bool>> checkMethods;

        private const string NO_ITEMS_TEXT = "No items to deposit...";
        private const string FULL_TEXT = "Too many items in the container";
        private const string TOO_MUCH_WEIGHT_TEXT = "Too much weight in the container...";
        private const string ALL_FULL_TEXT = "Cannot insert any more items in the container...";
        private const string WHEELBARROWCEPTION_TEXT = "You're not allowed to do that...";
        private const string DEPOSIT_TEXT = "Depositing item...";
        private const string START_DEPOSIT_TEXT = "Deposit item: [LMB]";
        private const string WITHDRAW_ITEM_TEXT = "Withdraw item: [LMB]";

        /// <summary>
        /// When the item spawns in-game, store the necessary variables for correct behaviours from the prefab asset
        /// </summary>
        public override void Start()
        {
            base.Start();
            randomNoise = new System.Random(StartOfRound.Instance.randomMapSeed + 80);
            defaultWeight = itemProperties.weight;
            totalWeight = defaultWeight;
            soundCounter = 0f;

            wheelsNoise = GetComponent<AudioSource>();
            triggers = GetComponentsInChildren<InteractTrigger>();
            foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>())
            {
                if (collider.name != "PlaceableBounds") continue;

                container = collider;
                break;
            }
            foreach (InteractTrigger trigger in triggers)
            {
                trigger.onInteract.AddListener(InteractContainer);
                trigger.tag = nameof(InteractTrigger); // Necessary for the interact UI to appear
                trigger.interactCooldown = false;
                trigger.cooldownTime = 0;
            }
            checkMethods = new Dictionary<Restrictions, Func<bool>>
            {
                [Restrictions.ItemCount] = CheckContainerItemCountRestriction,
                [Restrictions.TotalWeight] = CheckContainerWeightRestriction,
                [Restrictions.All] = CheckContainerAllRestrictions
            };

            SetupItemAttributes();
        }

        public float GetSloppiness()
        {
            return sloppiness;
        }
        public float GetLookSensitivityDrawback()
        {
            return lookSensitivityDrawback;
        }
        public override void Update()
        {
            base.Update();
            UpdateContainerSounds();
            UpdateInteractTriggers();
        }
        public void UpdateContainerDrop()
        {
            if (!isHeld) return;
            if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;
            if (currentAmountItems <= 0) return;
            DropAllItemsInContainerServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        private void DropAllItemsInContainerServerRpc()
        {
            DropAllItemsInContainerClientRpc();
        }
        [ClientRpc]
        private void DropAllItemsInContainerClientRpc()
        {
            GrabbableObject[] storedItems = GetComponentsInChildren<GrabbableObject>();
            for(int i = 0; i < storedItems.Length; i++)
            {
                if (storedItems[i] == this) continue; // Don't drop the container
                DropItem(ref storedItems[i]);
            }
            UpdateContainerWeightServerRpc();
        }
        /// <summary>
        /// Copy paste from PlayerControllerB.DropAllHeldItems applied on a singular grabbable object script
        /// </summary>
        /// <param name="grabbableObject"></param>
        private void DropItem(ref GrabbableObject grabbableObject)
        {
            grabbableObject.parentObject = null;
            grabbableObject.heldByPlayerOnServer = false;
            if (isInElevator)
            {
                grabbableObject.transform.SetParent(playerHeldBy.playersManager.elevatorTransform, worldPositionStays: true);
            }
            else
            {
                grabbableObject.transform.SetParent(playerHeldBy.playersManager.propsContainer, worldPositionStays: true);
            }

            playerHeldBy.SetItemInElevator(playerHeldBy.isInHangarShipRoom, isInElevator, grabbableObject);
            grabbableObject.EnablePhysics(enable: true);
            grabbableObject.EnableItemMeshes(enable: true);
            grabbableObject.transform.localScale = grabbableObject.originalScale;
            grabbableObject.isHeld = false;
            grabbableObject.isPocketed = false;
            grabbableObject.startFallingPosition = grabbableObject.transform.parent.InverseTransformPoint(grabbableObject.transform.position);
            grabbableObject.FallToGround(randomizePosition: true);
            grabbableObject.fallTime = UnityEngine.Random.Range(-0.3f, 0.05f);
            grabbableObject.hasHitGround = false;
            if (!grabbableObject.itemProperties.syncDiscardFunction)
            {
                grabbableObject.playerHeldBy = null;
            }
        }
        private void UpdateContainerSounds()
        {
            soundCounter += Time.deltaTime;
            if (!isHeld) return;
            if (wheelsNoise == null) return;
            if (playerHeldBy.thisController.velocity.magnitude == 0f)
            {
                wheelsNoise.Stop();
                return;
            }
            if (soundCounter < 2.0f) return;
            soundCounter = 0f;
            int index = randomNoise.Next(0, wheelsClip.Length);
            if (playSounds) wheelsNoise.PlayOneShot(wheelsClip[index], 0.2f);
            if (playSounds) WalkieTalkie.TransmitOneShotAudio(wheelsNoise, wheelsClip[index], 0.2f);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 0.8f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
        }
        private void UpdateInteractTriggers()
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player == null)
            {
                SetInteractTriggers(false, NO_ITEMS_TEXT);
                return;
            }

            if (!player.isHoldingObject)
            {
                SetInteractTriggers(false, NO_ITEMS_TEXT);
                return;
            }

            GrabbableObject holdingItem = player.currentlyHeldObjectServer;
            if (holdingItem.GetComponent<ContainerBehaviour>() != null)
            {
                SetInteractTriggers(false, WHEELBARROWCEPTION_TEXT);
                return;
            }
            if (CheckContainerRestrictions()) return;
            SetInteractTriggers(true, START_DEPOSIT_TEXT);
        }
        private void SetInteractTriggers(bool interactable = false, string hoverTip = START_DEPOSIT_TEXT)
        {
            foreach (InteractTrigger trigger in triggers)
            {
                trigger.interactable = interactable;
                if (interactable) trigger.hoverTip = hoverTip;
                else trigger.disabledHoverTip = hoverTip;
            }
        }
        void EnableInteractTriggers(bool enabled)
        {
            foreach (InteractTrigger trigger in triggers)
                trigger.gameObject.SetActive(enabled);
        }
        private bool CheckContainerRestrictions()
        {
            if (restriction == Restrictions.None) return false;
            return checkMethods[restriction].Invoke();
        }
        private bool CheckContainerAllRestrictions()
        {
            bool weightCondition = totalWeight > 1f + (maximumWeightAllowed / 100f);
            bool itemCountCondition = currentAmountItems >= maximumAmountItems;
            if (weightCondition || itemCountCondition)
            {
                SetInteractTriggers(interactable: false, hoverTip: ALL_FULL_TEXT);
                return true;
            }
            return false;
        }
        private bool CheckContainerWeightRestriction()
        {
            if (totalWeight > 1f + (maximumWeightAllowed / 100f))
            {
                SetInteractTriggers(interactable: false, hoverTip: TOO_MUCH_WEIGHT_TEXT);
                return true;
            }
            return false;
        }
        private bool CheckContainerItemCountRestriction()
        {
            if (currentAmountItems >= maximumAmountItems)
            {
                SetInteractTriggers(interactable: false, hoverTip: FULL_TEXT);
                return true;
            }
            return false;
        }

        internal void UpdatePlayerAttributes(bool grabbing)
        {
            if (grabbing)
            {
                if (LategameCompatibility.Enabled)
                {
                    LategameCompatibility.AddWeight(this);
                }
                else
                {
                    playerHeldBy.carryWeight -= Mathf.Clamp(itemProperties.weight - 1f, 0, 10f);
                    playerHeldBy.carryWeight += Mathf.Clamp(totalWeight - 1f, 0, 10f);
                }

                PlayerManager.instance.SetSensitivityMultiplier(lookSensitivityDrawback);
                PlayerManager.instance.SetSloppyMultiplier(sloppiness);
                PlayerManager.instance.SetHoldingContainer(true);
            }
            else
            {
                if (LategameCompatibility.Enabled)
                {
                    LategameCompatibility.RemoveWeight(this);
                }
                else
                {
                    playerHeldBy.carryWeight += Mathf.Clamp(itemProperties.weight - 1f, 0, 10f);
                    playerHeldBy.carryWeight -= Mathf.Clamp(totalWeight - 1f, 0, 10f);
                }

                PlayerManager.instance.ResetSensitivityMultiplier();
                PlayerManager.instance.ResetSloppyMultiplier();
                PlayerManager.instance.SetHoldingContainer(false);
            }
        }

        public override void DiscardItem()
        {
            wheelsNoise.Stop();
            if (playerHeldBy && GameNetworkManager.Instance.localPlayerController == playerHeldBy)
            {
                UpdatePlayerAttributes(grabbing: false);
                EnableInteractTriggers(true);
            }

            GrabbableObject[] storedItems = GetComponentsInChildren<GrabbableObject>();
            for (int i = 0; i < storedItems.Length; i++)
            {
                if (storedItems[i] is ContainerBehaviour) continue;
                playerHeldBy.SetItemInElevator(playerHeldBy.isInHangarShipRoom, playerHeldBy.isInElevator, storedItems[i]);
            }
            base.DiscardItem();
        }
        public override void GrabItem()
        {
            base.GrabItem();
            if (playerHeldBy && GameNetworkManager.Instance.localPlayerController == playerHeldBy)
            {
                UpdatePlayerAttributes(grabbing: true);
                EnableInteractTriggers(false);

                if (playerHeldBy.isCrouching) playerHeldBy.Crouch(!playerHeldBy.isCrouching);
            }
        }
        /// <summary>
        /// Setups attributes related to the container item
        /// </summary>
        private void SetupItemAttributes()
        {
            grabbable = true;
            grabbableToEnemies = true;
            itemProperties.toolTips = SetupContainerTooltips();
            SetupScanNodeProperties();
        }
        protected abstract string[] SetupContainerTooltips();

        /// <summary>
        /// Prepares the Scan Node associated with the Container for user display
        /// </summary>
        protected abstract void SetupScanNodeProperties();

        public void DecrementStoredItems()
        {
            UpdateContainerWeightServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        private void UpdateContainerWeightServerRpc()
        {
            UpdateContainerWeightClientRpc();
        }
        [ClientRpc]
        private void UpdateContainerWeightClientRpc()
        {
            GrabbableObject[] storedItems = GetComponentsInChildren<GrabbableObject>();
            if (isHeld && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                if (LategameCompatibility.Enabled)
                {
                    LategameCompatibility.RemoveTotalWeight(this);
                }
                else
                {
                    playerHeldBy.carryWeight -= Mathf.Clamp(totalWeight - 1f, 0f, 10f);
                }
            }
            totalWeight = defaultWeight;
            currentAmountItems = 0;
            for (int i = 0; i < storedItems.Length; i++)
            {
                if (storedItems[i].GetComponent<ContainerBehaviour>() != null) continue;
                currentAmountItems++;
                GrabbableObject storedItem = storedItems[i];
                totalWeight += (storedItem.itemProperties.weight - 1f) * weightReduceMultiplier;
            }
            if (isHeld && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                if (LategameCompatibility.Enabled)
                {
                    LategameCompatibility.AddTotalWeight(this);
                }
                else
                {
                    playerHeldBy.carryWeight += Mathf.Clamp(totalWeight - 1f, 0f, 10f);
                }
            }
        }
        /// <summary>
        /// Action when the interaction bar is completely filled on the container of the container.
        /// It will store the item in the container, allowing it to be carried when grabbing the container
        /// </summary>
        /// <param name="playerInteractor"></param>
        private void InteractContainer(PlayerControllerB playerInteractor)
        {
            if (playerInteractor.isHoldingObject)
            {
                StoreItemInContainer(ref playerInteractor);
            }
        }
        private void StoreItemInContainer(ref PlayerControllerB playerInteractor)
        {
            Collider triggerCollider = container;
            Vector3 vector = RoundManager.RandomPointInBounds(triggerCollider.bounds);
            vector.y = triggerCollider.bounds.max.y;
            vector.y += playerInteractor.currentlyHeldObjectServer.itemProperties.verticalOffset;
            vector = GetComponent<NetworkObject>().transform.InverseTransformPoint(vector);
            playerInteractor.DiscardHeldObject(placeObject: true, parentObjectTo: GetComponent<NetworkObject>(), placePosition: vector, matchRotationOfParent: false);
            UpdateContainerWeightServerRpc();
        }

        public static float CheckIfPlayerCarryingContainerLookSensitivity(float defaultValue)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player == null || player.thisController == null) return defaultValue;
            if (player.thisController.velocity.magnitude <= VELOCITY_APPLY_EFFECT_THRESHOLD) return defaultValue;
            return defaultValue * PlayerManager.instance.GetSensitivityMultiplier();
        }
        public static float CheckIfPlayerCarryingContainerMovement(float defaultValue)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player == null || player.thisController == null) return defaultValue;
            if (player.thisController.velocity.magnitude <= VELOCITY_APPLY_EFFECT_THRESHOLD) return defaultValue;
            return defaultValue * PlayerManager.instance.GetSloppyMultiplier();
        }
        public static bool CheckIfPlayerCarryingContainer()
        {
            return PlayerManager.instance.GetHoldingContainer();
        }

        public static bool CheckIfItemInContainer(GrabbableObject item)
        {
            if (item == null) return false;
            return item.GetComponentInParent<ContainerBehaviour>() != null;
        }
    }
}
