# How the 3x3 Classic slot works

This document explains the game system by system, in the order a spin flows through them.

## The big picture

```
SlotControls ──(spin / auto)──▶ SlotMachine ──▶ Reel x3 ──▶ WinEvaluator ──▶ PlayerWallet
     ▲  bet, lines, free spins        │  state machine         │ paylines, scatter,   coins, level
     │  jackpot amount                ▼                        ▼ jackpot
LinesController ◀──────────── WinPresenter / JackpotPresenter / BigWinPopup (feedback)
```

| Folder | What lives there |
| --- | --- |
| `Scripts/Data` | `SlotConfig` (ScriptableObject) holds *all* tunables: symbols, reel strips, pay table, spin feel, bet limits, jackpot. |
| `Scripts/Reels` | `Reel` (the rotating cylinder) and `SlotTile` (one symbol face). |
| `Scripts/Lines` | `PaylineView` (one of 20 lines), `LinesController` (selection), `LineStroke` (drawing). |
| `Scripts/Slot` | `SlotMachine` (spin flow), `SlotControls` (bet panel), `WinEvaluator`, `WinPresenter`. |
| `Scripts/Player` | `PlayerWallet`: coins, level, level progress (saved through `ISaveStore`). |
| `Scripts/UI` | Lamps, coin fountain, jackpot celebration, big-win popup, pay table and settings windows, level bar. |
| `Scripts/Audio`, `Scripts/Input` | `ISoundPlayer` + `SoundController`; `SceneButton` + `PointerInput` for world-space buttons. |
| `Scripts/Core/Services` | `GameServices` and the save / random interfaces with their implementations. |
| `Scripts/Editor` | The scene builder: regenerates the whole scene from code (`Slot3x3 > Build Classic Scene`). |

## The reel is a cylinder

Each reel is a vertical cylinder of 30 tiles (12 degrees apart). The tile spacing is 2.4 units, so the radius is
`1.2 / tan(6 deg) = 11.4`. Only the front sliver is visible through a mask (`SpriteMask` + tiles set to
*Visible Inside Mask*). The camera's far plane (65) cuts off the back half of the cylinder so it cannot show through
the transparent parts of the symbols.

The trick that keeps spinning simple: the reel owns a single float, `scroll` - the strip position of the bottom row.
Every tile stores an absolute `StripIndex`, and its angle is `baseAngle + (StripIndex - scroll) * 12`. When a tile
travels around to the back of the cylinder it is recycled by adding one lap (30) to its index, which also hands it
the next symbol of the strip. Spinning is just tweening `scroll`.

A spin has three parts: a short wind-back (reel moves up 7 degrees), the main run (extra laps plus the distance to the
target, linear, ~3 s), and an overshoot that settles back. The reels start 0.1 s apart for the left-to-right stagger.
While the reel moves faster than 10 units/s the tiles show their blurred sprite.

The outcome is decided *before* the reel moves: `SlotMachine` picks a random strip position per reel, and the
symbols in the window afterwards are strip[p] (bottom), strip[p+1] (middle), strip[p+2] (top).

## Reel strips, not random symbols

A reel is not "pick a random symbol per cell": each has a fixed ordered strip (17-18 symbols). That is how a
slot's return-to-player is designed - changing how often a symbol appears on a strip changes the odds. The three
strips are in `ClassicConfigFactory`.

## Paylines and the pay table

There are 20 paylines. Each crosses one window row per reel (see `LineRows` in the scene builder). The pay table is a
list of three-symbol patterns (`-1` = any symbol). A line wins when every non-"any" position matches; if several
patterns match the best one (highest pay or free spins) counts.

**Wild**: for every base pattern the game derives all variants where some symbols are replaced by the wild
(`PayLine.BuildWildVariants`). The first symbol of a line is never replaced, and Free Spin, Scatter, Wild and
Jackpot symbols opt out of substitution. The result is cached as `SlotConfig.FullPayTable`.

**Scatter**: counted anywhere on screen. Exactly five scatters fill the level bar.
**Jackpot**: five or more jackpot symbols anywhere on screen win the progressive "mega" jackpot, which grows by 1 coin per spin.

## Paying out (`SlotMachine.WinRoutine`)

```
coins = sum(pay of winning lines + scatter) x product(payMult)
coins x= line bet            (config.multiplyWinByLineBet)
total bet = line bet x selected lines
free spins = sum(freeSpins) x product(freeSpinsMult)
level progress += 2.0 x winning lines x line bet     (a lost spin adds 0.5)
```
Wins of 5000+ coins open the big-win popup. Free spins and auto spin simply start the next spin when the current
win presentation has finished its first pass.

## Win presentation

`WinPresenter` first sparkles every winning tile, then walks the winning lines one by one (the line flashes, tiles
pulse for 5 s each) and loops until the next spin. `JackpotPresenter` flashes the jackpot display, runs the lamps in
chase mode and fires the coin fountains.

## UI and input

Machine buttons are sprites with 2D colliders (`SceneButton`); `PointerInput` picks the topmost one under the mouse
or finger. The header and the windows are uGUI. `SceneScaler` keeps the background and machine matched to the screen
aspect ratio (design ratio 4:3).

## Building the scene

`SlotSceneBuilder` reconstructs the camera, machine, reels, lines, panel, lamps, header and windows from code so the
scene is reproducible and reviewable. Layout numbers are the machine-local positions measured at the 4:3 design ratio.

## Testing aid

`SlotMachine.debugStops` forces the reels to stop on given strip positions, e.g. `12,10,12` puts three wilds on the
middle line.

## Placeholder art, sound and font (no licensed assets)

Everything you see and hear is made by code in `Scripts/Editor`, so the repository contains no store or licensed assets:

| What | Made by | Where it ends up |
| --- | --- | --- |
| Machine, buttons, plates, coins, lamps | `PlaceholderArt` (rounded boxes, gradients, glows) | `Assets/Art/Generated` |
| The 14 reel symbols and their spin blur | `SymbolArt` + `PixelFont` (a coloured card with a 5x7 pixel label) | `Assets/Art/Generated/Symbols` |
| Music and sound effects | `SynthSounds` (short note lists rendered to 16-bit WAV) | `Assets/Audio/Generated` |
| Text | Unity's built-in `LegacyRuntime.ttf` | - |

The scene builder asks for sprites by a *logical* path such as `Assets/Art/Hud/Star GUI.png`; only the file name is used
to pick the generated PNG. Delete a generated file and it is drawn again on the next build. To re-skin the game, drop real
art over the generated PNGs (same name and size) and rebuild the scene: no game code changes.

## Services: how the code follows SOLID

Gameplay classes never talk to Unity's global systems directly. They ask `GameServices` for an interface:

```
GameServices.Save   : ISaveStore     (PlayerPrefsSaveStore)  <- PlayerWallet, DailyRewardController, SlotControls
GameServices.Random : IRandomSource  (UnityRandomSource)     <- Reel (stop positions, spin time)
GameServices.Sound  : ISoundPlayer   (SoundController)       <- SlotMachine, popups, buttons
```

- **Single responsibility**: `WinEvaluator` finds wins, `Payout` does the payout maths, `WinPresenter` shows them and
  `SlotMachine` only runs the order of a spin. `DailyRewardSchedule` holds the reward rules with no Unity code at all.
- **Open/closed**: new symbols, lines and pay rules are data in `SlotConfig`; new art or audio is a file swap.
- **Liskov substitution**: `MemorySaveStore` and `PlayerPrefsSaveStore`, or `SeededRandomSource` and `UnityRandomSource`,
  can replace each other anywhere without the caller noticing.
- **Interface segregation**: each interface is small and about one thing (saving, random numbers, sound).
- **Dependency inversion**: high-level classes depend on `ISaveStore`, `IRandomSource` and `ISoundPlayer`, not on
  PlayerPrefs, `UnityEngine.Random` or a concrete MonoBehaviour. Tests swap them in one line:

```csharp
GameServices.Save = new MemorySaveStore();       // never touch the real save
GameServices.Random = new SeededRandomSource(7); // the same spins every run
```

## Hold feature

Under each reel is a toggle button (`HoldFeature`). A held reel does not spin on the next spin; at most two reels can be
held (a third hold releases the oldest). Holding costs extra: the total bet is multiplied by 1 / 2 / 4 for none / one /
two held reels (`SlotConfig.holdBetMultipliers`), and the multiplier is shown next to the reels. Wins are still paid as
`pay x line bet`, so the extra stake buys a better chance of lining up symbols. The buttons are locked while the reels
spin and during free spins. Switch it off with `SlotConfig.useHold`.

## Daily reward

`DailyRewardSchedule` holds the rules (no Unity code, unit tested): one reward every 24 h (`dailyRewardIntervalHours`),
the streak advances day by day through `dailyRewards` and wraps after the last day, and letting a whole extra interval
pass restarts it at day 1. `DailyRewardController` stores the last claim time (UTC) and next day through `ISaveStore`, pops the
window up when a reward is waiting and shows a red badge on the header gift button; `DailyRewardPopup` shows the streak
and a collect button that becomes a countdown. The controller's `UtcNow` clock can be replaced to time-travel in tests.

## Running out of credits (developer helpers)

When a bet cannot be afforded the game opens `OutOfCreditsPopup` offering `devRefillCoins` free coins, so testing never
dead-ends. In the settings window a DEVELOPER panel (visible only in the editor and development builds) adds
`devAddCoins` coins or resets the daily reward. Turn `SlotConfig.allowDevRefill` off for a release and put a real shop
behind the popup instead.

## Notes on rare paths

- **Scatter reward**: the config pays out for exactly five scatters on screen, but each reel strip contains only one
  scatter, so at most three can ever appear. The entry is kept on purpose; it is harmless
  and would become reachable if a strip gained more scatters.
- **Wide screens**: `SceneScaler` follows the aspect ratio and `HeaderFitter` stretches the header plate to the view width.
- **Project settings**: `Slot3x3 > Apply Project Settings` sets identity (`com.slot3x3.classic`), landscape-only, Android
  (IL2CPP, ARM64, min API 25), iOS 15 and the generated app icon.
