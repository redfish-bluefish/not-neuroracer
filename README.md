# Not-NeuroRacer

A simple clone of the game "NeuroRacer" designed by Adam Gazzley and his team, built with C# and MonoGame (XNA). 

The game's design centers around multitasking between a driving game similar to Outrun where the player must keep a constantly accelerating car on the road, and a shape matching game where the player must respond to a given shape+color with the appropriate response. By doing so, it differs from traditional memory training applications by focusing on training multitasking rather than single tasks. Similar to the original, Not-NeuroRacer also features a dynamic difficulty system, where player performance is recorded and taken into account when determining car speed and maximum allowed response time to shapes. In doing so, the game ensures that each participant is challenged to a more or less equal extent, regardless of how familiar or comfortable they are with keyboard controls.

Controls:
- Left + Right Arrow Keys: Steer the car left and right
- A + D: Respond to the on-screen shapes

To simplify data collection, every session exports its results to a .csv file which includes
- Shape Game + Driving Game final level
- Session start & end times

The data can then be compiled and processed for analysis. Participants' short term memory can (and should) also be tested with 3rd party tools.

Areas for Improvement:
- Remove magic numbers from multiple areas in the code (ShapeGameController, Player spritesheet, etc.)
- Add clarifying comments & remove superfluous comments
- Reduce the amount of profanity in commit messages
- Consolidate similar classes into the same file to reduce clutter (e.g. 2DShape+3DShape -> Abstracts, DrivingGameController+ShapeGameController -> GameControllers)
