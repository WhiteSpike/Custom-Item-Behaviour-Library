using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CustomItemBehaviourLibrary.AbstractItems
{
    /// <summary>
    /// Base class which represents an item with the ability of teleporting the player back to the ship through vanila teleport
    /// when activated
    /// </summary>
    public abstract class PortableTeleporterBehaviour : GrabbableObject
    {
        internal static bool TPButtonPressed = false;
        /// <summary>
        /// Wether the portable teleporter will allow the player who activated it to keep items when teleported
        /// </summary>
        protected bool keepItems;
        /// <summary>
        /// The normal teleporter of the ship (if not bought, it will be null)
        /// </summary>
        private ShipTeleporter shipTeleporter;

        /// <summary>
        /// Function called when the player activates the item in their hand through left-click
        /// </summary>
        /// <param name="used">Status of the item in terms being used or not</param>
        /// <param name="buttonDown">Pressed down the button or lifted up the press</param>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (!CanUsePortableTeleporter()) return;

            int playerRadarIndex = SearchForPlayerInRadar();
            ShipTeleporter teleporter = GetShipTeleporter();

            TeleportPlayer(playerRadarIndex, ref teleporter);
        }
        /// <summary>
        /// Triggers the vanila teleport on the player holding the portable teleporter
        /// </summary>
        /// <param name="playerRadarIndex">Index of the player holding the portable teleporter</param>
        /// <param name="teleporter">Teleporter that allows to teleport players back to the ship</param>
        protected virtual void TeleportPlayer(int playerRadarIndex, ref ShipTeleporter teleporter)
        {
            if (playerRadarIndex == -1)
            {
                //this shouldn't occur but if it does, this will teleport this client and the server targeted player.
                StartOfRound.Instance.mapScreen.targetedPlayer = playerHeldBy;
                TPButtonPressed = true;
                teleporter.PressTeleportButtonOnLocalClient();
            }
            else
            {
                StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(playerRadarIndex);
                StartCoroutine(WaitToTP(teleporter));
            }
        }
        /// <summary>
        /// Search for the player holding the portable teleporter's radar index used to select in the radar screen of the ship
        /// </summary>
        /// <returns>Radar index of the player holding the portable teleporter</returns>
        private int SearchForPlayerInRadar()
        {
            int thisPlayersIndex = -1;
            for (int i = 0; i < StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
            {
                if (StartOfRound.Instance.mapScreen.radarTargets[i].transform.gameObject.GetComponent<PlayerControllerB>() != playerHeldBy) continue;

                thisPlayersIndex = i;
                break;
            }
            return thisPlayersIndex;
        }
        /// <summary>
        /// Checks if the player holding the portable teleporter can use it to teleport back to the ship
        /// </summary>
        /// <returns>Wether the player can use the item or not</returns>
        protected virtual bool CanUsePortableTeleporter()
        {
            if (itemUsedUp)
            {
                return false;
            }
            ShipTeleporter teleporter = GetShipTeleporter();
            if (teleporter == null || !teleporter.buttonTrigger.interactable)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Searches for the vanila teleporter that allows teleporting players back to the ship
        /// </summary>
        /// <returns>Reference to the vanila teleporter if it exists, false if otherwise</returns>
        private ShipTeleporter GetShipTeleporter()
        {
            if (shipTeleporter != null) return shipTeleporter;

            ShipTeleporter[] tele = FindObjectsOfType<ShipTeleporter>();
            ShipTeleporter NotInverseTele = null;
            foreach (ShipTeleporter shipTeleporter in tele)
            {
                if (shipTeleporter.isInverseTeleporter) continue;

                NotInverseTele = shipTeleporter;
                break;
            }
            shipTeleporter = NotInverseTele;
            return shipTeleporter;
        }
        /// <summary>
        /// Starts the teleporting process of the player holding the portable teleporter and checks if it should break the item due to random chance
        /// </summary>
        /// <param name="tele">Vanila teleporter that allows to teleport players back to the ship</param>
        /// <returns></returns>
        protected virtual IEnumerator WaitToTP(ShipTeleporter tele)
        {
            // if we don't do a little wait we'll tp the previously seleccted player.
            yield return new WaitForSeconds(0.15f);
            if (keepItems) ReqUpdateTpDropStatusServerRpc();
            tele.PressTeleportButtonOnLocalClient();
        }

        /// <summary>
        /// Sets the status of player using the portable teleporter to true
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ReqUpdateTpDropStatusServerRpc()
        {
            ChangeTPButtonPressedClientRpc();
        }

        /// <summary>
        /// Sets the status of player using the portable teleporter to true
        /// </summary>
        [ClientRpc]
        private void ChangeTPButtonPressedClientRpc()
        {
            TPButtonPressed = true;
        }
    }
}
