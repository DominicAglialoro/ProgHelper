# ProgHelper

A Celeste helper mod containing various custom entities and triggers, most of which are intended to improve gameplay consistency and quality of life

## Entities

- **Adjustable Falling Block**: A Falling Block with a cusomizable delay and player wait time. Also adds the option to include attached platforms as part of itself when checking if the player is standing on it, so that the block falls early if the player jumps off of an attached platform

- **Linear Intro Crusher**: An Intro Crusher with linear movement. Can also have a curve that starts accelerating and then becomes linear after a set period. The linear movement makes it easier to get maximum LiftBoost when jumping off of it

## Triggers

- **Camera Constraint Trigger**: Sets a strict boundary on how far the camera can move ahead of or behind the player. Useful in high speed situations where the player may otherwise end up offscreen

- **Camera Hard Border**: Similar to MaddieHelpingHand's Camera Offset Border, but constrains the position of the camera itself, instead of the camera target. Useful in high speed situations where a normal Camera Offset Border may cause the camera to move too slowly as it approaches its target

- **Clip Prevention Trigger**: A trigger that prevents the player from passing entirely through it in a single frame. Helps to ensure that the player does not clip through entities when moving very fast

- **Collider Enlarger**: Alters the size of colliders of entities inside the trigger, when the level is loaded

- **Disable Coyote Jump Trigger**: Clears the player's coyote time and prevents the player from gaining coyote time while inside the trigger. Differs from the Coyote Time Extended Variant, which does not clear existing coyote time when triggered. Useful in situations where the player may want to buffer an upcoming jump, but is unable due to existing coyote time

- **Set Player Speed Trigger**: Immediately sets the player's speed when entered

- **Speed Camera Offset Trigger**: Sets the camera offset based on the player's current speed. Similar to FurryHelper's Momentum Camera Offset Trigger, but uses the player's speed value to control the camera offset, instead of calculating the change in the player's position each frame

- **Wavedash Protection Trigger**: Prevents the player from wall jumping during a down-diagonal dash. Makes it easier to perform reverse wavedashes off platforms that are up against a wall
