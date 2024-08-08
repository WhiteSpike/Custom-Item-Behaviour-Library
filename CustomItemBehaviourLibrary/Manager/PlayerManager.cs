using UnityEngine;

namespace CustomItemBehaviourLibrary.Manager
{
    internal class PlayerManager : MonoBehaviour
    {
        internal const float DEFAULT_MULTIPLIER = 1.0f;
        internal float sensitivityMultiplier = DEFAULT_MULTIPLIER;
        internal float sloppyMultiplier = DEFAULT_MULTIPLIER;
        internal bool holdingContainer = false;

        internal float sanityOvertimeTimer;
        internal float sanityOvertimeReplenish;

        internal static PlayerManager instance;
        void Start()
        {
            instance = this;
            sanityOvertimeReplenish = 0.0f;
            sanityOvertimeTimer = 0.0f;
        }

        void Update()
        {
            DepleteSanityOvertime();
        }

        internal void DepleteSanityOvertime()
        {
            if (sanityOvertimeReplenish <= 0f) return;
            sanityOvertimeTimer -= Time.deltaTime;

            if (sanityOvertimeTimer <= 0f)
            {
                sanityOvertimeReplenish = 0f;
            }
        }

        internal void AddSanityOvertimeTimer(float sanityOvertimeTimer)
        {
            this.sanityOvertimeTimer += sanityOvertimeTimer;
        }

        internal void SetSanityOvertimeReplenish(float sanityOvertimeReplenish)
        {
            this.sanityOvertimeReplenish = sanityOvertimeReplenish;
        }

        internal static float GetSanityOvertimeReplenish()
        {
            return instance.sanityOvertimeReplenish;
        }

        public static float DecreaseSanityIncrease(float defaultValue)
        {
            return defaultValue - instance.sanityOvertimeReplenish;
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
