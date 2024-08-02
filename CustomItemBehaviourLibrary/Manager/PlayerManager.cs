using UnityEngine;

namespace CustomItemBehaviourLibrary.Manager
{
    internal class PlayerManager : MonoBehaviour
    {
        internal const float DEFAULT_MULTIPLIER = 1.0f;
        internal float sensitivityMultiplier = DEFAULT_MULTIPLIER;
        internal float sloppyMultiplier = DEFAULT_MULTIPLIER;
        internal bool holdingContainer = false;

        internal static PlayerManager instance;
        void Awake()
        {
            instance = this;
        }

        internal void SetSensitivityMultiplier(float sensitivityMultiplier)
        {
            this.sensitivityMultiplier = sensitivityMultiplier;
        }

        internal void SetSloppyMultiplier(float sloppyMultiplier)
        {
            this.sloppyMultiplier = sloppyMultiplier;
        }

        internal void SetHoldingContainer(bool holdingContainer)
        {
            this.holdingContainer = holdingContainer;
        }

        internal void ResetSensitivityMultiplier()
        {
            sensitivityMultiplier = DEFAULT_MULTIPLIER;
        }
        internal void ResetSloppyMultiplier()
        {
            sloppyMultiplier = DEFAULT_MULTIPLIER;
        }

        public float GetSensitivityMultiplier()
        {
            return sensitivityMultiplier;
        }
        public float GetSloppyMultiplier()
        {
            return sloppyMultiplier;
        }
        public bool GetHoldingContainer()
        {
            return holdingContainer;
        }
    }
}
