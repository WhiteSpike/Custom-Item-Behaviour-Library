namespace CustomItemBehaviourLibrary.AbstractItems
{
    /// <summary>
    /// <para>Item which allows players holding the item to breath underwater, however their vision will be blocked by the model as it will be placed on their head.</para>
    /// </summary>
    public abstract class UnderwaterBreatherBehaviour : GrabbableObject
    {
        /// <summary>
        /// Instance which controls the drowning timer of the player
        /// </summary>
        private StartOfRound roundInstance;

        public override void Start()
        {
            base.Start();
            roundInstance = StartOfRound.Instance;
        }
        /// <summary>
        /// Check if this item is currently grabbed by a player and if it's the local player and if so, reset their drown timer.
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (CanRetainOxygen())
            {
                roundInstance.drowningTimer = 1f;
            }
        }

        public virtual bool CanRetainOxygen()
        {
            return isHeld && playerHeldBy == GameNetworkManager.Instance.localPlayerController;
        }
    }
}
