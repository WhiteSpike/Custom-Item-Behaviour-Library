using System.Collections.Generic;
using UnityEngine;

namespace CustomItemBehaviourLibrary.AbstractItems
{
    /// <summary>
    /// <para>Item which allows players holding the item to breath underwater, however their vision will be blocked by the model as it will be placed on their head.</para>
    /// </summary>
    public abstract class LookoutBehaviour : GrabbableObject
    {
        static List<LookoutBehaviour> coilHeadItems = new();
        /// <summary>
        /// Wether the instance of the class can stop coil-heads from moving or not
        /// </summary>
        private bool Active;
        protected int maximumRange;

        public override void Start()
        {
            base.Start();
            coilHeadItems.Add(this);
        }
        public override void Update()
        {
            base.Update();
            SetActive(!isHeld && !isHeldByEnemy);
        }
        protected virtual void SetActive(bool enable)
        {
            Active = enable;
        }

        protected virtual bool HasLineOfSightToPosition(Vector3 pos)
        {
            if (!Active) return false;
            float num = Vector3.Distance(transform.position, pos);
            bool result = num < maximumRange && !Physics.Linecast(transform.position, pos, StartOfRound.Instance.collidersRoomDefaultAndFoliage, QueryTriggerInteraction.Ignore);
            return result;
        }

        public static bool HasLineOfSightToPeepers(Vector3 springPosition)
        {
            foreach (LookoutBehaviour peeper in coilHeadItems)
            {
                if (peeper == null)
                {
                    coilHeadItems.Remove(peeper);
                    continue;
                }
                if (peeper.HasLineOfSightToPosition(springPosition)) return true;
            }
            return false;
        }
    }
}
