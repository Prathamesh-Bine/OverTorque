# OverTorque

**OverTorque** is a high-octane, physics-based 3D arcade survival game. Take control of a kinetic sphere in a neon-drenched arena, master your momentum, and ram geometric enemies into the abyss. 

## 🎮 How to Play
* **Survive & Scale:** The ultimate goal is to survive as long as possible. The system actively fights back, increasing the enemy count and difficulty the longer you stay alive.
* **Collect Data:** Gather glowing Data Crystals to gain **10 EXP** each. Your EXP directly scales your physical knockback power—the more EXP you hold, the harder you hit.
* **Stay on the Grid:** Falling off the arena platform incurs a strict **-30 EXP** penalty. 
* **System Failure:** If you fall off the edge and your EXP drops to **0**, it is Game Over.

## ⌨️ Controls
* **W A S D** - Move the Sphere
* **Hold Space** - Charge Linear Dash (Release to ram and stun enemies for 2 seconds)
* **Hold Space + Shift** - Charge Sonic AoE Burst (Releases a localized shockwave)
* **Escape** - Pause Game

## ⚠️ Threat Analysis
Enemies despawn after 20 seconds. New enemies will aggressively try to spawn between you and your next EXP Crystal to block your path.
* **Cube:** Fast and lightweight.
* **Prism:** Balanced speed and weight.
* **Pentagon:** Massive, heavy, and hits hard.

## 🔴 Overdrive Mode (7-Minute Mark)
Reaching the 7-minute mark is the ultimate survival objective. Surviving for 7 minutes triggers **Overdrive Mode**:
* The arena's glowing neon grid permanently shifts from blue to an aggressive red.
* The system unlocks the maximum enemy spawn cap (4 concurrent enemies).
* Enemies permanently gain a **+10% boost** to Speed and Knockback Power every 60 seconds thereafter.

## 🛠️ Installation & Setup
1. Clone this repository to your local machine.
2. Open the project via Unity Hub (Developed on Unity 6 / 6000.0.x).
3. In the Project window, navigate to `Assets/Scenes` and open the `MainMenu` scene.
4. Press Play in the editor to run the game, or go to `File > Build Settings` to compile a standalone build.
