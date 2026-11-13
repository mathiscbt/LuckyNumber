# Lucky Number

A cozy percussive 2D arcade sumo brawler made in Unity.

Step into the arena! *Push*, *bounce*, and *outmaneuver* your opponent to knock them out. Whether you want to test your skills in a **Tournament** or challenge a friend in **2-Player Local Mode**, every match is a fast-paced, physics-driven showdown.

> **Theme: Lucky Number**

Before entering the arena, **your fate is decided by your Lucky Number.**

It dynamically shapes your sumo wrestler's **10 core attributes**: *Weight*, *Size*, *Aura*, *Force*, *Stamina*, *Cooldown*, *Awkwardness*, *Fatigue*, *Inertia*, and *Delay*. Will your lucky number turn you into an immovable giant or an agile fighter? Adapt your playstyle to your number and claim victory!

---

## Game modes

- **Tournament (vs. AI):** Climb the ranks against AI opponents in Normal or Hard difficulty.
- **2-Player (Local 1v1):** Share the keyboard and battle a friend on the same screen.

## Controls

### Tournament mode

| | Choice 1 | Choice 2 | Choice 3 |
|---|---|---|---|
| **Move** | WASD / ZQSD | Mouse | Arrow Keys |
| **Dash** | SpaceBar | WASD / ZQSD | Right Shift |

### 2-Player mode

| | Player 1 | Player 2 |
|---|---|---|
| **Move** | WASD / ZQSD | Arrow Keys |
| **Dash** | SpaceBar | Right Shift |

## Credits & assets

- **Art & Visuals:** All visual assets were hand-crafted from scratch during the game jam.
- **Audio & Music:** Sound effects and background music sourced from Pixabay.
- **Audio Management:** Powered by BroAudio for Unity.

---

## Getting started

1. Open the project with the Unity version listed in `ProjectSettings/ProjectVersion.txt`.
2. Open `Assets/Scenes/SampleScene.unity` and press Play. The game starts on the main menu.
3. `Assets/Scenes/Training.unity` is the ML-Agents training scene (see [ML training](#ml-training)).

Main packages (see `Packages/manifest.json`): Universal Render Pipeline, Input System, TextMeshPro, Cinemachine, ML-Agents 4.2.0. BroAudio lives in `Assets/BroAudio`.

A WebGL build is available in `Build/` (`index.html`).

## Project structure

```
Assets/
├── Scripts/
│   ├── GameFlow/     Screens, sessions, match lifecycle
│   ├── Gameplay/     Sumo physics, combat, controllers, balance, camera
│   └── UI/           Panels, menus, typewriter, button sounds
├── Prefabs/          Sumo, PlayerSumo, AiSumo, GameArea, SumoSetupPanel1
├── Scenes/           SampleScene (game), Training (ML-Agents)
├── Audios/           Sound effects and music clips
├── BroAudio/         Audio framework (third-party)
├── Animations/, Materials/, Visuals/, Settings/, TextMesh Pro/
```

### GameFlow

- **`GameFlowManager`**: the single entry point for screens. It shows one panel at a time, starts fights, reacts to fight results, and drives the music.
- **`GameSession`**: tournament state. The player's lucky number is removed from 1–9, then 5 opponents are drawn for the 5 rounds.
- **`MatchManager`**: spawns the fighters, tracks who is alive, detects the end of a fight (one side has no sumo left), and gives the survivors a new behaviour afterwards.
- **`SumoSetup`**: the data of one fighter (lucky number, color, AI or player, controls).
- **`GameMode`, `Difficulty`**: enums for VsAi / LocalMulti and Normal / Hard.

### Gameplay

- **`SumoMovement`**: the core of a fighter. It runs the dash state machine (`Normal → Windup → Dashing`), the dash cooldown, and the stun state, and applies forces to the Rigidbody2D.
- **`SumoHealth`**: health. At 0 HP the sumo is stunned for `stunDuration`, then recovers at full health.
- **`SumoVisuals`**: animator triggers, blinking while stunned, squash on elimination, and dash ghosts (`DashGhost`).
- **`SumoUI`**: the dash cooldown and health bars above each sumo.
- **`SumoCamera`**: follows the fighters.
- **`AreaChecker`**: leaving the arena eliminates the sumo.
- **`PlayerAction`**: reads input for a human sumo (scheme chosen in the setup screen).
- **`AiAction`**: AI controller (see [AI](#ai)).
- **`SumoBalanceConfig`** and **`SumoData`**: balance and per-fighter stats (see [Lucky Number balance](#lucky-number-balance)).
- **`LockRotation`**: keeps the sprites upright.
- **`SumoAgent`**: ML-Agents agent used for training.

### UI

- **`UIPanel`**: base class for all panels (`Show`, `Hide`, and a `FadeIn` used by the end-of-match panels).
- **`MainMenuPanel`**, **`ControlSchemePanel`**, **`SetupPanel`** / **`SumoSetupPanel`**: menus and fighter setup (lucky number, color).
- **`StatsPanel`**: pre-fight stats with a typewriter effect.
- **`MatchResultPanel`**, **`TournamentRoundPanel`**, **`TournamentEndPanel`**: results.
- **`SelectionHighlighter`**: highlights the selected option in each group of buttons.
- **`UIButtonSound`**: hover, select and click sounds, added automatically to every button at startup.

## How a match works

1. **Main menu**: pick Tournament or 2-Player.
2. **Control scheme** (Tournament only): pick how you move and dash.
3. **Setup**: choose your lucky number and color (and the second player's in 2-Player mode).
4. **Stats screen**: shows your stats and your opponent's, typed out letter by letter.
5. **Fight**: `MatchManager` spawns the fighters. The last side standing wins.
6. **After the fight**: surviving AI sumos stop chasing an eliminated player. If several remain, they fight each other. If only one remains, it wanders.
7. **Result**: a result panel in 2-Player mode, or the next round / the end of the tournament.

### Tournament rules

- 5 rounds (`GameSession.RoundCount`).
- **Normal:** one opponent per round, with a random lucky number and color.
- **Hard:** the player's lucky number determines how many opponents face them at once, all with the same lucky number and color.

## Lucky Number balance

`SumoBalanceConfig.GenerateDataForNumber(n)` clamps `n` to 1–9 and computes `t = (n − 1) / 8`. Each stat is `Mathf.Lerp(valueAt1, valueAt9, t)`, so every stat moves smoothly from the "1" value to the "9" value. The values are in the `SumoBalanceConfig` asset (`Assets/Scripts/Gameplay/Objects/`), so you can tune the whole game from the inspector.

The stats are: `mass`, `scale`, `drag`, `moveForce`, `maxSpeed`, `cooldown`, `dashWindupTime`, `dashDuration`, `dashForce`, and `stunDuration`.

## AI

`AiAction` runs every frame and does two things.

**Movement** (by priority):
1. If it is near the edge of the arena, it steers back toward the center.
2. If the player is dashing, it dodges sideways.
3. If it is lighter than its target, it backs off when too close and circles at medium range.
4. Otherwise, it moves straight toward its target.

**Dash:**
- No dash during cooldown.
- No dash if the estimated landing point would leave the arena (`IsDashSafe`). This check also applies to dodges.
- Dodges when the player is dashing.
- Otherwise, it dashes when the target is within `maxDashDistance` and it is facing the target (`dashAlignmentThreshold`).

## Audio

- Sounds and music are `SoundID` assets managed by **BroAudio**.
- **Music:** `GameFlowManager` plays the menu track on the main menu and at the end of a fight, and the combat track at the start of a fight. Music volume is set on the `Music` audio type.
- **Sound effects:** dash, hit, fall, victory and defeat are played by the gameplay and UI scripts.
- **UI:** every `Button` gets hover, select and click sounds through `UIButtonSound`. The typewriter in `StatsPanel` plays a tick sound while the player's text is being typed.

## ML training

`SumoAgent` extends ML-Agents' `Agent`. It observes the dash cooldown, whether it is dashing, whether it is stunned, and the opponent's state. Its rewards are:

- `winReward` (+1) and `loseReward` (−1) at the end of a fight.
- A damage reward scaled by `damageRewardScale`.

Training runs in `Assets/Scenes/Training.unity`.

---

## Team notes

- The project was made during a game jam. Scripts are in English and French comments.
- Input is read directly from `Keyboard.current` / `Mouse.current` in `PlayerAction`.
