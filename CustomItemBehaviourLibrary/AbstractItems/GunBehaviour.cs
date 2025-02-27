using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CustomItemBehaviourLibrary.AbstractItems
{
    public abstract class GunBehaviour : GrabbableObject
    {
        protected AudioSource audioSource;
        protected AudioClip safetyShootClip;
        protected AudioClip safetyTurningOnClip;
        protected AudioClip safetyTurningOffClip;
        protected AudioClip[] shootClips;
        protected AudioClip noAmmoClip;

        protected ParticleSystem shootParticles;

        protected Coroutine gunReload;

        protected Animator gunAnimator;

        protected SortedList<float, float> earRingingRanges = new();
        protected SortedList<float, int> rangePlayerDamages = new();
		protected SortedList<float, int> rangeEnemyDamages = new();

        protected int maxAmmo;
        protected int currentAmmo;
		protected int ammoUsage;
		protected int ammoObtained;
		private int ammoSlotToUse;
		protected int compatibleAmmoId;

		protected bool isReloading;

        protected bool safetyEnabled;
        protected bool safetyOn;

        protected RaycastHit[] enemyHits;
        protected int maximumHits;
        protected Transform gunShootPoint;

        protected PlayerControllerB previousPlayer;
        protected EnemyAI holdingEnemy;

        private bool shootServerRPC;

		public override void Start()
		{
            base.Start();
            audioSource = GetComponent<AudioSource>();
            gunAnimator = GetComponent<Animator>();
            shootParticles = GetComponent<ParticleSystem>();
			isReloading = false;
            shootServerRPC = false;
			previousPlayer = null;
			InitializeGun();
        }
		protected abstract void InitializeGun();
		/*
        protected virtual void InitializeGun()
        {
            earRingingRanges.Add(5.0f, 1f);
			rangePlayerDamages.Add(5.0f, 10);
            maxRange = 5.0f;
            safetyEnabled = true;
            safetyOn = true;
            maximumAngle = 10f;
            maxAmmo = 30;
            if (currentAmmo <= 0)
                currentAmmo = 30;
            maximumHits = 10;
			enemyHits = new RaycastHit[maximumHits];
			compatibleAmmoId = 1410;
		}
		*/
		public override void DiscardItemFromEnemy()
		{
			base.DiscardItemFromEnemy();
			holdingEnemy = null;
		}
		public override void GrabItemFromEnemy(EnemyAI enemy)
		{
			base.GrabItemFromEnemy(enemy);
			holdingEnemy = enemy;
		}
		public override void EquipItem()
		{
			base.EquipItem();
            previousPlayer = playerHeldBy;
            previousPlayer.equippedUsableItemQE = true;
		}

		public override void DiscardItem()
		{
			base.DiscardItem();
            StopUsingGun();
		}
		public override void PocketItem()
		{
			base.PocketItem();
			StopUsingGun();
		}

		protected virtual void StopUsingGun()
        {
            previousPlayer.equippedUsableItemQE = false;

            if (!isReloading) return;

            if (gunReload != null)
                StopCoroutine(gunReload);

            gunReload = null;
            audioSource.Stop();

			if (previousPlayer != null)
			{
				ResetGunAnimator(ref previousPlayer);
			}

            isReloading = false;
			previousPlayer = null;
		}

		protected abstract void ResetGunAnimator(ref PlayerControllerB previousPlayer);

		public override int GetItemDataToSave()
		{
            if (!itemProperties.saveItemVariable) return 0;
            return currentAmmo;
		}

		public override void LoadItemSaveData(int saveData)
		{
            if (!itemProperties.saveItemVariable) return;
            currentAmmo = saveData;
		}
		public override void ItemInteractLeftRight(bool right)
		{
			base.ItemInteractLeftRight(right);
            if (playerHeldBy == null)
                return;

            if (right)
            {
                StartReload();
            }
            else
            {
				if (!safetyEnabled) return;
				ToggleSafety();
                SetSafetyControlTip();
                HandleSafetyAnimations();
			}
		}
		protected abstract void HandleSafetyAnimations();

		public override void SetControlTipsForItem()
		{
			string[] toolTips = itemProperties.toolTips;
			if (!safetyEnabled || toolTips.Length <= 2)
			{
				HUDManager.Instance.ChangeControlTipMultiple(toolTips, holdingItem: true, itemProperties);
				return;
			}

			if (safetyOn)
			{
				toolTips[2] = "Turn safety off: [Q]";
			}
			else
			{
				toolTips[2] = "Turn safety on: [Q]";
			}

			HUDManager.Instance.ChangeControlTipMultiple(toolTips, holdingItem: true, itemProperties);
		}
		protected virtual void SetSafetyControlTip()
		{
			string changeTo = ((!safetyOn) ? "Turn safety on: [Q]" : "Turn safety off: [Q]");
			if (base.IsOwner)
			{
				HUDManager.Instance.ChangeControlTip(3, changeTo);
			}
		}


		public override void ItemActivate(bool used, bool buttonDown = true)
		{
			base.ItemActivate(used, buttonDown);
            if (isReloading) return;

            if (currentAmmo <= 0)
			{
				StartReload();
                return;
			}
            if (IsSafetyOn())
            {
                audioSource.PlayOneShot(safetyShootClip);
                return;
            }

            if (IsOwner)
                FireGun();
		}

        public virtual void FireGun()
		{
			Vector3 firingPosition = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position - GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.up * 0.45f;
			Vector3 forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;
            ShootGun(firingPosition, forward);
            shootServerRPC = true;
			ShootGunServerRpc(firingPosition, forward);
        }

        [ServerRpc]
        public void ShootGunServerRpc(Vector3 position, Vector3 forward)
        {
            ShootGunClientRpc(position, forward);
        }

        [ClientRpc]
        public void ShootGunClientRpc(Vector3 firingPosition, Vector3 forward)
        {
            if (shootServerRPC) shootServerRPC = false;
            else ShootGun(firingPosition, forward);
        }

        protected virtual void ShootGun(Vector3 firingPosition, Vector3 forward)
        {
            isReloading = false;
			bool heldByLocalPlayer = isHeld && playerHeldBy != null && playerHeldBy == GameNetworkManager.Instance.localPlayerController;
			if (heldByLocalPlayer)
			{
                SetPlayerShootingAnimation();
			}

			RoundManager.PlayRandomClip(audioSource, shootClips, randomize: true, 1f, 1850);
            shootParticles.Play(withChildren: true);
            currentAmmo = Mathf.Clamp(currentAmmo - ammoUsage, 0, maxAmmo);
            HandleShootingPlayer(firingPosition, forward, heldByLocalPlayer);

			if (!IsOwner) return;
			int numHits = FetchEnemyColliders(firingPosition, forward);
			ProcessEnemies(numHits, firingPosition, forward);
		}

        protected virtual void HandleShootingPlayer(Vector3 firingPosition, Vector3 forward, bool heldByLocalPlayer)
        {
			if (GameNetworkManager.Instance.localPlayerController == null) return;
			PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
			float distanceFromShootingPoint = Vector3.Distance(localPlayer.transform.position, gunShootPoint.position);
			bool hitPlayer = CheckForHitOnLocalPlayer(ref localPlayer, firingPosition, forward, heldByLocalPlayer);
            if (!hitPlayer) 
            {
                HandlePossibleRicochet(firingPosition, forward);
			}

            if (hitPlayer)
            {
				HandleHittingLocalPlayer(ref localPlayer, distanceFromShootingPoint);
            }
		}
		protected abstract bool CheckForHitOnLocalPlayer(ref PlayerControllerB localPlayer, Vector3 firingPosition, Vector3 forward, bool heldByLocalPlayer);
		/*
		protected virtual bool CheckForHitOnLocalPlayer(ref PlayerControllerB localPlayer, Vector3 firingPosition, Vector3 forward, bool heldByLocalPlayer)
		{
			Vector3 closestPointToGun = localPlayer.playerCollider.ClosestPoint(firingPosition);
			return !heldByLocalPlayer &&
							!Physics.Linecast(firingPosition, closestPointToGun, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) &&
							Vector3.Angle(forward, closestPointToGun - firingPosition) < maximumAngle;
		}
		*/
        protected virtual void ProcessEnemies(int numHits, Vector3 firingPosition, Vector3 forward)
        {
			List<EnemyAI> processedEnemies = new List<EnemyAI>();
			for (int i = 0; i < numHits; i++)
			{
                EnemyAICollisionDetect enemyAICollision = enemyHits[i].transform.GetComponent<EnemyAICollisionDetect>();
				if (enemyAICollision == null)
				{
					continue;
				}

				EnemyAI mainScript = enemyAICollision.mainScript;
				if (isHeldByEnemy && holdingEnemy == mainScript) // enemy doesn't shoot themselves
				{
					continue;
				}
				IHittable hittable;
				if (Physics.Linecast(firingPosition, enemyHits[i].point, out RaycastHit _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) || !enemyHits[i].transform.TryGetComponent<IHittable>(out hittable))
				{
                    continue;
				}

				float distance = Vector3.Distance(firingPosition, enemyHits[i].point);
                int force = GetForceByDistance(distance);
                if (!processedEnemies.Contains(mainScript) && hittable.Hit(force, forward, playerHeldBy, playHitSFX: true))
                    processedEnemies.Add(mainScript);
			}
		}
        V GetValueBetweenRanges<V>(SortedList<float,V> orderedCollection, float distance)
        {
			V value = default;
			foreach (KeyValuePair<float, V> ranges in orderedCollection)
			{
				if (distance < ranges.Key) continue;
				value = ranges.Value;
				break;
			}
			return value;
		}
        protected virtual int GetForceByDistance(float distance)
        {
			return GetValueBetweenRanges(rangeEnemyDamages, distance);
		}
		protected abstract int FetchEnemyColliders(Vector3 firingPosition, Vector3 forward);
		/*
        protected virtual int FetchEnemyColliders(Vector3 firingPosition, Vector3 forward)
        {
			Ray ray = new Ray(firingPosition - forward * 20f, forward);
			return Physics.RaycastNonAlloc(ray, enemyHits, maxRange, 524288, QueryTriggerInteraction.Collide);
		}
		*/
		protected virtual void HandleHittingLocalPlayer(ref PlayerControllerB localPlayer, float distanceFromShootingPoint)
        {
            int damage = GetValueBetweenRanges(rangePlayerDamages, distanceFromShootingPoint);

            localPlayer.DamagePlayer(damage, hasDamageSFX: true, callRPC: true, CauseOfDeath.Gunshots, 0, fallDamage: false, gunShootPoint.forward * 10f);
		}
        protected virtual void HandleEarRinging(float distanceFromShootingPoint)
        {
			float earRingingTone = GetValueBetweenRanges(earRingingRanges, distanceFromShootingPoint);

			ShakeScreen(earRingingTone);

			if (earRingingTone > 0f && SoundManager.Instance.timeSinceEarsStartedRinging > 16f)
			{
				StartCoroutine(delayedEarsRinging(earRingingTone));
			}
		}
		protected abstract void ShakeScreen(float earRingingTone);
		protected abstract IEnumerator delayedEarsRinging(float earRingingTone);
		/*
		protected virtual IEnumerator delayedEarsRinging(float effectSeverity)
		{
			yield return new WaitForSeconds(0.6f);
			SoundManager.Instance.earsRingingTimer = effectSeverity;
		}
		*/
		protected abstract void HandlePossibleRicochet(Vector3 firingPosition, Vector3 forward);
		/*
		protected virtual void HandlePossibleRicochet(Vector3 firingPosition, Vector3 forward)
        {
			Ray ray = new Ray(firingPosition, forward);
			if (Physics.Raycast(ray, out RaycastHit hitInfo, rangePlayerDamages.Keys[rangePlayerDamages.Count - 1], StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
			{
				gunRicochetAudio.transform.position = ray.GetPoint(hitInfo.distance - 0.5f);
				gunRicochetAudio.Play();
			}
		}
		*/
		protected abstract void SetPlayerShootingAnimation();

        public virtual void StartReload()
        {
            if (isReloading || currentAmmo >= maxAmmo) return;
            if (!CanReload())
			{
				audioSource.PlayOneShot(noAmmoClip);
				return;
			}

			if (!IsOwner) return;

			if (gunReload != null)
				StopCoroutine(gunReload);

			gunReload = StartCoroutine(reloadGunAnimation());
		}

		protected virtual IEnumerator reloadGunAnimation()
		{
			ToggleReloading();
			yield return PrefixReloadGunAnimation();
			yield return HandleReloadLogic();
			yield return PostfixReloadGunAnimation();
			ToggleReloading();
		}

		protected abstract IEnumerator PrefixReloadGunAnimation();
		protected virtual IEnumerator HandleReloadLogic()
		{
			playerHeldBy.DestroyItemInSlot(ammoSlotToUse);
			ammoSlotToUse = -1;
			currentAmmo = Mathf.Clamp(currentAmmo + ammoObtained, 0, maxAmmo);
			yield return null;
		}
		protected abstract IEnumerator PostfixReloadGunAnimation();

        protected virtual bool CanReload()
        {
			int num = FindAmmoInInventory(compatibleAmmoID: compatibleAmmoId);
			if (num == -1)
			{
				return false;
			}

			ammoSlotToUse = num;
			return true;
		}

        protected int FindAmmoInInventory<T>() where T : GrabbableObject
        {
			for (int i = 0; i < playerHeldBy.ItemSlots.Length; i++)
			{
				if (playerHeldBy.ItemSlots[i] == null) continue;
				T ammo = playerHeldBy.ItemSlots[i] as T;

				if (ammo != null)
				{
					return i;
				}
			}

			return -1;
		}

		protected int FindAmmoInInventory(int compatibleAmmoID)
		{
			for (int i = 0; i < playerHeldBy.ItemSlots.Length; i++)
			{
				if (playerHeldBy.ItemSlots[i] == null) continue;
				GunAmmo gunAmmo = playerHeldBy.ItemSlots[i] as GunAmmo;

				if (gunAmmo != null && gunAmmo.ammoType == compatibleAmmoID)
				{
					return i;
				}
			}

			return -1;
		}

        public void ToggleReloading()
        {
            ToggleReloading(!isReloading);
        }

        public void ToggleReloading(bool isReloading)
        {
            this.isReloading = isReloading;
		}

        public bool IsSafetyOn()
        {
            return safetyEnabled && safetyOn;
        }

        public void ToggleSafety()
        {
            ToggleSafety(!safetyOn);
        }

        public void ToggleSafety(bool value)
        {
            safetyOn = value;
            AudioClip playingClip = safetyOn ? safetyTurningOnClip : safetyTurningOffClip;
			audioSource.PlayOneShot(playingClip);
			WalkieTalkie.TransmitOneShotAudio(audioSource, playingClip);
		}
	}
}
