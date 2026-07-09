# Project: DeepForest Character & Narrative System

## Architecture
The character progression, custom stats, traits (custom cards), and narrative system are built on top of Godot's C# Resource and Event system.
1. **Character Definition**: Defined via C# class `CharacterData : Resource` and serialized `.tres` files in `src/resources/characters/`.
2. **Custom Traits & Cards**: Customized card definitions (`.tres`) in `src/resources/cards/` mapping to character starting decks.
3. **Narrative events & NPC encounters**: Driven by `EventManager` (to be created as an autoload or core class) and hooked into `SceneEventHandler.OnPlayerEnterNode`.
4. **Endings & Lore Persistence**: Tracked via `EndingManager` and persisted via `StoryUnlock` to `user://story_unlocks.cfg`.
5. **Character Selection**: Implemented as a clean, text-aligned overlay in `MainScene.cs` before gameplay begins.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Character Resources & Traits | Create `.tres` definitions for the 6 characters and custom starting cards. | None | PLANNED |
| 2 | EventManager & NPC encounters | Implement `EventManager.cs`, add random NPC interactions & Tommy's "找到小狗" event. | M1 | PLANNED |
| 3 | EndingManager & Story persistence | Extend `EndingManager.cs` for key item combination checks and lore unlock persistence. | M2 | PLANNED |
| 4 | Character Selection UI | Create a text-based ASCII selection screen overlay in `MainScene.cs` before start. | M1 | PLANNED |
| 5 | Dual-Track Integration & Acceptance | Run full build and test verification, ensure all acceptance criteria pass. | M3, M4 | PLANNED |

## Interface Contracts
### Character Selection & Initialization
- `Player.InitializeFromData(CharacterData)`: Configures player stats from a chosen `.tres` resource.
- `Deck.Initialize(List<Card>)`: Sets up starting cards for the selected character.

### NPC & Story Events
- `EventManager.HandleNpcEncounter(Player player, Deck deck, string characterName)`: Generates the random dialogues, stat trades, or combat triggers for meeting character NPCs.
- `EventManager.HandleFindDogEvent(Player player, Deck deck, int choice)`: Handles Tommy's dog quest, granting "傑利" or "傑利？" exclusively.

### Endings & Persistence
- `EndingManager.CheckEndGameConditions()`: Expanded to check for character-specific key item cards in the deck/hand and triggers specific ending titles.
- `StoryUnlock.UnlockStorySegment(string title, string description)`: Writes unlocked endings and character information to persistent `user://` configuration.

## Code Layout
- `src/resources/characters/`: `.tres` files for John, Sarah, Leo, Celin, Nancy, Tommy.
- `src/resources/cards/`: Custom trait cards (e.g. self-harm card, stress card, Celin's obsession card, etc.).
- `src/scripts/narrative/EventManager.cs`: The core event and dialogue engine.
- `src/scripts/narrative/EndingManager.cs`: Handles check logic for character-specific true/good/bad endings.
- `src/scripts/ui/MainScene.cs`: Character selection UI overlay setup.
