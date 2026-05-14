# Final Year VR Project

## What This Project Is
This is a Unity VR final-year project that appears to center on first-person VR interaction, weapons, zombies, vending, and UI systems. The README is minimal, so the project identity is inferred primarily from the scene and script structure.

The main build scene appears to be:
- `Final VR Project/Assets/Scenes/Main Scene.unity`

There are also testing and environment scenes, plus imported model demo scenes.

### Recent cleanup
`RaycastWeapon` now shares its controller-binding polling through a small helper, and `SceneLoader` has a cleaned-up serialized field name for its screen-fader flag.
`WeaponSlot` now uses a selection strategy and a spawn factory, so vending logic is split between choosing a weapon and instantiating it.
The vending and scene-loading paths are safer now, with less hidden coupling around spawn and transition behavior.

## How The Code Works
The runtime appears to be organized around the VR player rig and interactable world objects.

### Player and movement
`PlayerController`, `PlayerRotation`, `PlayerHealth`, and `LocomotionManager` collectively manage the player experience. The locomotion layer likely handles teleport versus smooth movement, while the rotation layer handles snap or smooth turning.

### Hands and grabbing
`HandController` and `Grabber` appear to coordinate pose display and object pickup, which is a core part of any VR interaction loop.

### Weapons and combat
`RaycastWeapon` is the largest combat script and likely owns firing, ammo, reload, eject, recoil, and hit effects. Supporting scripts such as `ProjectileLauncher`, `Projectile`, `Bullet`, `LaserSword`, `Bow`, and `DamageCollider` suggest a multi-weapon sandbox rather than a single weapon type.

### Economy and vending
`VendingMachine`, `WeaponSlot`, `AmmoCrate`, `AmmoDispenser`, and `CreditsManager` suggest a buy-and-upgrade loop for weapons or supplies.

### UI and scene flow
`VRUISystem`, `VRCanvas`, `VRTextInput`, `VRSlider`, and `SceneLoader` handle in-world UI and scene changes.

### Enemies and environment
`ZombieAI` likely uses NavMesh-based pursuit and attacks. The project also includes ragdoll helper code, so enemy death or physical interaction probably plays a visible role.

## Main Design Traits
- This is the most systems-heavy of the projects in the set.
- Many scripts look like focused helpers, but the core weapon and player controllers are likely doing too much.
- The project relies heavily on Unity packages and third-party VR/XR infrastructure.

## Evidence Used
- `Final VR Project/ProjectSettings/EditorBuildSettings.asset`
- `Final VR Project/Packages/manifest.json`
- `Final VR Project/Assets/Scripts/Player/PlayerController.cs`
- `Final VR Project/Assets/Scripts/Locomotion/LocomotionManager.cs`
- `Final VR Project/Assets/Scripts/Grab/Grabber.cs`
- `Final VR Project/Assets/Scripts/Weapons/RaycastWeapon.cs`
- `Final VR Project/Assets/Scripts/VendingMachine/VendingMachine.cs`
- `Final VR Project/Assets/Scripts/enemy/ZombieAI.cs`
- `Final VR Project/Assets/Scripts/UI/VRUISystem.cs`
