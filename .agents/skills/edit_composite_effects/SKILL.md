---
name: edit-composite-effects
description: Guide on authoring and modifying nested composite conditions and effects for events, cards, and scene actions in DeepForest.
---

# DeepForest Composite Conditions and Effects Authoring Skill

This skill guides you on how to define, read, and modify data-driven composite nested resources (`EventCondition` and `EventEffect`) for cards, narrative events, and scene actions in Godot 4.6 mono.

---

## 1. Architecture Overview

Both conditions and effects use Godot's Option A nested Resource hierarchy (`res://src/scripts/narrative/EventCondition.cs` and `res://src/scripts/narrative/EventEffect.cs`), allowing developers to model complex logic gates and behavior scripts entirely as `.tres` files.

### Condition Subclasses (`EventCondition`)
- **`ConditionComposite`**: Logical AND/OR gate container.
  - `Conditions` (Array): Child conditions.
  - `IsOrGate` (bool): If true, acts as OR; if false, acts as AND.
- **`ConditionCardLocation`**: Checks card locations.
  - `TargetCardId` (string): Enum value (e.g. `ActionStrength`, `KeyRecordingTape`).
  - `Location` (string): `Hand`, `DrawPile`, `DiscardPile`, `Equipped`, or `Anywhere`.
- **`ConditionStatCheck`**: Checks player properties.
  - `StatName` (string): `Hp`, `Sanity`, `Hunger`, `Thirst`, `Brutality`, `Corruption`, `Evil`, `Day`, `Depth`.
  - `Operator` (string): `<`, `>`, `<=`, `>=`, `==`.
  - `Value` (int): Threshold value.
- **`ConditionLogCheck`**: Queries action history log.
  - `ActionType` (string): `CardPlayed`, `OptionChosen`, `SceneMoved`, etc.
  - `ParamKey` / `ParamValue` (string): Query parameters.
  - `Scope` (string): `ThisScene`, `ThisTurn`, or `ThisGame`.

### Effect Subclasses (`EventEffect`)
- **`EffectComposite`**: Sequence of multiple effects.
  - `Effects` (Array): Child effects executed sequentially.
- **`EffectConditional`**: If-Then-Else conditional branch.
  - `Condition` (EventCondition): Nested condition to check.
  - `ThenEffects` (Array): Run if condition evaluates to true.
  - `ElseEffects` (Array): Run if condition evaluates to false.
- **`EffectModifyStat`**: Alters player parameters.
  - `StatName` (string): `Hp`, `Sanity`, `Hunger`, `Thirst`, `Brutality`, `Corruption`, `Evil`.
  - `ChangeValue` (int): Offset (positive or negative).
- **`EffectCardTransfer`**: Adds, removes, or draws cards.
  - `TargetCardId` (string): Enum value.
  - `Action` (string): `Add` (adds to discard), `Remove` (purges anywhere), `Draw` (draws cards).
  - `Count` (int): Number of cards.
- **`EffectMoveNode`**: Transitions map node.
  - `TargetNodeId` (int): Downstream node index.
- **`EffectSetFlag`**: Sets narrative triggers.
  - `FlagName` (string): `IsDescentActive`, `NancySuicideFlag`, etc.
  - `FlagValue` (bool): Value.

---

## 2. Card Effects Integration

To apply this mechanism to **Card Play Effects**:
1. Declare an `[Export] public EventEffect? PlayEffect { get; set; }` inside `Card.cs`.
2. In `CardPlayHandler.cs`, instead of writing dedicated custom C# strategies, execute:
   ```csharp
   if (card.PlayEffect != null)
   {
       card.PlayEffect.Execute(player, deck);
   }
   ```
This removes hardcoded strategy classes, enabling designers to write card effects completely in `.tres` files.

---

## 3. How to Author/Edit Effects

### Pattern 1: Nested Logical AND (`ConditionComposite`)
Checking if the player has low sanity (<30) **and** is holding the "舊日教本" anywhere:
```ini
[Resource of type ConditionComposite]
IsOrGate = false
Conditions = [
  { "StatName": "Sanity", "Operator": "<", "Value": 30 },
  { "TargetCardId": "KeyOldScripture", "Location": "Anywhere" }
]
```

### Pattern 2: If-Then-Else Branch (`EffectConditional`)
If the player has "舊日教本" in their hand, trigger descent and deduct sanity, else deduct HP:
```ini
[Resource of type EffectConditional]
Condition = [Resource of type ConditionCardLocation (TargetCardId="KeyOldScripture", Location="Hand")]
ThenEffects = [
  [Resource of type EffectSetFlag (FlagName="IsDescentActive", FlagValue=true)],
  [Resource of type EffectModifyStat (StatName="Sanity", ChangeValue=-20)]
]
ElseEffects = [
  [Resource of type EffectModifyStat (StatName="Hp", ChangeValue=-15)]
]
```
