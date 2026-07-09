# Challenge Report — Milestone 1.1 Hidden Stats Integration

## Challenge Summary

**Overall risk assessment**: LOW

## Challenges

### [Low] Challenge 1: Absence of Signal Notification for Hidden Stats

- **Assumption challenged**: Hidden stats are assumed to be strictly read-only by gameplay systems (`EventManager`, `EndingManager`) at specific check points, so they don't need real-time monitoring.
- **Attack scenario**: If a narrative event or debug system wants to listen for changes to hidden stats (e.g., to trigger immediate visual/audio cues or achievements when Brutality changes), it cannot do so reactively because `Brutality`, `Corruption`, and `Evil` setters do not emit any signals, unlike the primary stats (`CurrentHp`, `CurrentSanity`, etc.).
- **Blast radius**: Low. Systems must poll or manually check these values during event execution, which increases coupling.
- **Mitigation**: Add a `HiddenStatChanged` signal or emit `StatChanged` for hidden stats if reactive changes are needed.

### [Low] Challenge 2: Direct Private Field Access Bypasses Clamping

- **Assumption challenged**: All changes to hidden stats will go through the public properties (`Brutality`, `Corruption`, `Evil`).
- **Attack scenario**: If any method within the `Player` class modifies `_brutality`, `_corruption`, or `_evil` directly, it will bypass the `Math.Clamp` logic, potentially leading to out-of-bounds stats.
- **Blast radius**: Low, since they are private fields and the current code uses the public properties for initialization.
- **Mitigation**: Enforce C# linting/review guidelines to always modify these values through the public properties.

## Stress Test Results

- **Initialization with out-of-bounds values** → Initializing from `CharacterData` with StartingBrutality=150, StartingCorruption=-50 → Correctly clamped to [0, 100] (100 and 0 respectively) → **PASS**
- **Dynamic modification clamping** → Setting properties `player.Brutality = 105;` → Value read back is `100` → **PASS**

## Unchallenged Areas

- **UI Exposure** — We did not test UI code to ensure hidden stats are not displayed. This is because UI files are outside the scope of Milestone 1.1's core logic verification.
