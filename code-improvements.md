# Final Year VR Project - Code Improvements

## Priority Improvements

### Already addressed
- `RaycastWeapon` now reuses a shared input-polling helper.
- `SceneLoader` has a cleaned-up serialized field name and typed coroutine invocation.
- Weapon vending spawn behavior now goes through a dedicated spawn factory.

### 1. Break up the weapon controller
`RaycastWeapon` should be decomposed into smaller units:
- trigger and firing logic
- ammo and reload state
- recoil and slide behavior
- visual/audio effects

That would make it easier to add new weapons without copying a giant script.
The new vending spawn factory is a useful pattern to repeat for the rest of the weapon flow.

### 2. Reduce singleton coupling
Systems like `VRUISystem`, `VendingMachine`, and `CreditsManager` appear to rely on singleton-style access. Replacing that with explicit references or an event-driven service layer would make the project easier to test and reason about.

### 3. Separate VR player concerns
`PlayerController`, `PlayerRotation`, `PlayerHealth`, and `LocomotionManager` should have clearer boundaries. Right now they seem to coordinate a single experience from multiple directions, which risks duplicated responsibility.

### 4. Move weapon and hand tuning into data assets
Weapon properties, pose data, and maybe vendor stock should live in ScriptableObjects instead of being buried in MonoBehaviours. That would make balance changes much safer.

### 5. Tighten scene and input handling
Fix any awkward input condition checks and remove typo-prone API usage such as `UseSceenFader`. Small cleanup like this matters more in VR because interaction bugs are more noticeable.

### 6. Make enemy and world interaction more modular
`ZombieAI` and related combat scripts should expose smaller behavior units for targeting, attack timing, damage response, and death effects. That keeps the enemy side more maintainable as the project grows.

## Secondary Cleanups
- Normalize naming across folders and scripts
- Collapse duplicate projectile or weapon helper logic
- Reduce reliance on hardcoded scene references
- Add comments around XR-specific setup so new contributors can follow the rig flow
