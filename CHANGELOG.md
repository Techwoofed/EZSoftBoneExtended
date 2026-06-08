# Changelog

## [2.0.0-rc1] - Extended collision system, version by Techwoof - 2026-06-07
### This is a feature-complete version, pending some fixes and more testing. No erros where observed after extensive testing, feel free to compile and test in-game.

### New Features

- **Parent-chain collision feedback**: Collisions now propagate part of their positional correction upward through the bone hierarchy.
	New configurable parameters:
	- Collision Feedback
	- Collision Feedback Falloff
	- Collision Feedback Depth
- **Backward constraint solving**: Added a backward constraint pass after the regular forward bone update.
	New configurable parameters:
	- Backward Constraint Strength
	- Backward Constraint Passes
- **Child orientation propagation**: Added support for propagating the orientation of a displaced or colliding bone into its descendants.
	New configurable parameters:
	- Child Orientation Follow
	- Collision Child Orientation Follow
	- Collision Child Orientation Falloff
	- Collision Child Orientation Depth
- **Native Unity collider support**: Added optional automatic collision detection against standard Unity colliders.
	New configurable parameters:
	- Collide With Native Colliders
	- Native Collider Buffer Size
- **Compound collision proxies for simulated bones**: Added support for defining more accurate collision volumes on individual simulated bones.
- **Bone collision proxy component**: Added a dedicated component for configuring per-bone sphere and capsule proxies, separate from the existing bone collider.

### Changed

- **Collision resolution order**: Collision handling now records the correction applied to each bone and forwards that correction into Parent-chain feedback, backward lenght constraints and child orientation propagation
- **Stiffness target calculation**: The stiffness target can now account for the simulated orientation of the parent segment.
- **Native collider filtering**: Exclusion of colliders that are disabled, are trigger, on the exact same transform, outside the listed layers.
- **Default collision layers**: layers are now 0, 26 and 29 by default (VAM-specific).
- **Non-allocating native collider queries**: Native collider detection uses `Physics.OverlapSphereNonAlloc` with a reusable collider buffer to avoid per-frame garbage allocations.
- **Editor layout**: Added all new features to the editor settings.
- Changed EZSBController to add à listener for the new features.
- Changed the way updates are calculated (NASTY!!) to work around some VAM limitations and avoid EZSB physics flickering in high-load scenarios against native colliders.
	- Using a temporary render-only pose latch we keep the `RevertTransforms()` in `Update()`, cache the solved local pose after `LateUpdate()`, immediately before a camera renders temporarily apply the cached solved pose, and then after that camera finishes rendering, restore whatever pose was present before rendering. Immediately after that camera finishes rendering, restore whatever pose was present before rendering. I know, i'm not proud of this either.
	- A cleaner way would just be to change the `RevertTransforms()` call to `LateUpdate()` instead of `Update()`, but in doing so we would sacrifice Animator support.

### TODO (must be fixed before final submit and review):
- Fix default settings: Defaults settings are not optimal, change so stuff don't look weird on spawn. Child orientation related settings should be 0 by default.
- Add toggle for the workaround to be ON or OFF by the Editor.
- Test if the toggle for the workaround can work in-game without causing issues.
- Cleanup old tests
- Make a proper documentation of the new features and behaviors

### Known issues:
- Class conflict: This plugin must NOT conflict with any existing namespace, otherwise new functionalities won't work. But it still will not break existing stuff.


## [x.x.x] - VAM edit by Hazmhox (all credits for this change goes to https://hub.virtamate.com/members/hazmhox.351/)

- Added EZSBController (hook for https://hub.virtamate.com/resources/ezsoftbone-library.66283/)
- Changed namespace to VAMEZSoftBones

## [1.7.1] - Unreleased

### Changed

- Force module Rewrited
- Calculate force into speed instead of position
- Average speed with last frame

## [1.6.1] - 2020-04-06

### Fixed

- EZNestedEditorAttribute is not suitable for EZSoftBoneMaterial (Default-Material editing should be disabled)

### Changed

- Node can be moved freely if its depth is less than startDepth
- Modified property orders in Inspector

### Added

- Custom startDepth can be specified with function RevertTransforms(int startDepth)

## [1.6.0] - 2020-03-26

### Changed

- EZSoftBoneForce: it drives from ScriptObject now
- EZSoftBone: a force space could be specified for EZSoftBoneForce

## [1.5.2] - 2020-01-06

### Changed

- Garbage Collection Optimize: replace Mathf.Max(a, b, c) with Mathf.Max(a, Mathf.Max(b, c))

## [1.5.1] - 2019-12-13

### Fixed

- Bug fixed on LengthUnification (Wrong length calculation)

### Added

- Added an custom inspector for `EZSoftBone`, not all changes will trigger a reconstruction now

### Changed

- Changed some function names

## [1.5.0] - 2019-12-11

### Added

- End Bones: end bones can be specified
- Length Unification: there are 3 length calculation modes now, just like the "Sibling Constraints"
- Add set accessor to some properties
- Add a public function `Reconstructure` so you can reinitialize the system after you changed properties at runtime.

### Changed

- Change some variables' name
- replace enum SiblingConstraintMode with UnificationMode

## [1.4.0] - 2019-11-25

### Changed

- Change Name to EZSoftBone.

## [1.3.0] - 2019-11-08

### Added

- GravityAligner: A transform can be specified to determine how much the gravity effects the system (inversely correlated to dot product of aligner's y direction and world's y direction)
- SimulateSpace: A transform can be specified as a simulate space, it's useful when the system needs to be updated with a moving object (like the hair in a car)

### Fixed

- Fixed wrong calculations on Iterations

### Changed

- Call RevertTransforms on Update instead of LateUpdate, InternalAnimationUpdate will be called between them

## [1.2.1] - 2019-10-11

### Added

- SiblingRotationConstraints: Rotation will be affected by Sibling Constraints if enabled
- Delta_Min: a constant value (1e-6), Pasue if deltaTime is under this value

## [1.2.0] - 2019-08-23

### Changed

- Now the restrictions' length will be scaled with the related transforms

### Removed

- End Node: That's modeler's responsibility (and remove it makes the code looks so much better)

## [1.1.0] - 2019-07-24

### Added

- Added nested editor for PBMaterials

### Changed

- Package path changed to Assets/EZhex1991/EZPhysicsBone
- Shorten material, collider, and force module's name
- Revised some comments

## [1.0.0] - 2019-06-10

- First Release