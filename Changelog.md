<details>
<summary> 1.2.10 </summary>

- Added configuration for blacklisting items from containers, stopping the player from depositing these items into them.
    - This should be used for items that can cause issues when being deposited into a container.
</details>

<details>
<summary> 1.2.9 </summary>

- Fixed issue with storing items in the containers and removing them afterwards not updating the total amount of items stored in them.

</details>

<details>
<summary> 1.2.8 </summary>

- Recompiled against new Unity Netcode version used in v73.
    - Game versions before v73 need to use 1.2.7 of this mod.
 
</details>

<details>
<summary> 1.2.7 </summary>

- Recompiled to latest release of Lethal Company (v71)
	- Should fix issues related with ``FallToGround`` errors
</details>

<details>
<summary> 1.2.6 </summary>

- Fixed issue with CullFactory related with depositted items.
- Possibly fixed issue with depositted items not being considered in/outside correctly during entrance teleportations.
</details>

<details>
<summary> 1.2.5 </summary>

- Allowed ContainerBehaviour script to allow two sets of colliders, one used when there are no items stored in the item and other for when there are items stored.
  - This is to allow a bigger grab prompt area to reduce situations where it can get stuck while also not adding the inconvenience of trying to pick up the items from the container without accidentaly grabbing the container instead.

</details>

<details>
<summary> 1.2.4 </summary>

- Fixed issue with Bag Belt from latest beta release where containers wouldn't update their holding weight.
- Also doesn't allow to put containers inside the Bag Belt.

</details>

<details>
<summary> 1.2.3 </summary>

- Fixed the same issue again due to changes in version 62 of the main game.

</details>

<details>
<summary> 1.2.2 </summary>

- Fixed issue with LookoutBehaviour not working correctly due to modification in SpringManAI's Update function, leading to the related patch give unwanted result.

</details>

<details>
<summary> 1.2.1 </summary>

- Made enemies not being able to deposited to the containers unless they are dead (due to the new enemy from v60 release).

</details>

<details>
<summary> 1.2.0 </summary>

- Added "ReplenishSanityBehaviour" abstract class which focuses on restoring player's sanity when consumed and provide a sanity rate over time after consuming it.
- Modified "ContainerBehaviour" abstract class to allow making depositted items invisible and to make the deposit triggers inactive when overriding the condition to be active.

</details>

<details>
<summary> 1.1.0 </summary>

- Added "ReplenishBatteryBehaviour" abstract class which focuses on charging items with a battery upon activating the item (Left Click by default)

</details>

<details>
<summary> 1.0.1 </summary>

- Fixed issue with ContainerBehaviour scripts not resetting the movement debuffs when the player dies holding one.
- Fixed issue with Lategame Upgrades Compatibility not being correctly implemented.


</details>
<details>
<summary> 1.0.0 </summary>

- Initial release

</details>