# Scope: Character Resources & Traits (Milestone 1)

## Architecture
- Characters are defined as `CharacterData : Resource` in `src/scripts/character/CharacterData.cs`.
- Custom cards are defined as `Card : Resource` in `src/scripts/cards/Card.cs`.
- Card effects are resolved in `CardPlayHandler.cs` (in `src/scripts/ui/`) and class strategies in `src/scripts/cards/play_effects/`.
- Characters are loaded in `Player.cs` via `InitializeFromData(CharacterData)`.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1.1 | Hidden Stats Integration | Add StartingBrutality, StartingCorruption, StartingEvil exports to CharacterData.cs, load them in Player.cs. | None | IN_PROGRESS (Conv ID: 6fd50f04-4814-4a2d-bcb8-060e451fa3a2) |
| 1.2 | Custom Card Traits | Create custom starting cards for Celin (執念), Nancy (自殘), Tommy (崩潰爆發) in src/resources/cards/ and code their behaviors. | None | PLANNED |
| 1.3 | Character Resources | Create .tres files for John, Sarah, Leo, Celin, Nancy, Tommy in src/resources/characters/ with authoritative stats/decks. | 1.1, 1.2 | PLANNED |
| 1.4 | Compilation & Build | Build using dotnet build and confirm character data loads correctly in unit/smoke tests. | 1.3 | PLANNED |

## Interface Contracts
### Player ↔ CharacterData
- `Player.InitializeFromData(CharacterData)` should load:
  - `Brutality` = `StartingBrutality`
  - `Corruption` = `StartingCorruption`
  - `Evil` = `StartingEvil`
  - And all starting stats (HP, Sanity, Hunger, Thirst, Draw, HandLimit, DeckCapacity).
