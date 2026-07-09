# Original User Request

## 2026-07-02T13:28:19+08:00

This project implements the character progression, custom stats, traits (custom cards), starting decks, NPC interactions via the EventManager, and persistent ending/lore unlock tracking for the 6 characters of DeepForest based on `角色設計.txt` and specific gameplay criteria.

Working directory: d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot project/DeepForest
Integrity mode: demo

---

## Requirements

### R1. Character Definitions (`.tres`)
Create `.tres` files for all 6 characters in `src/resources/characters/`. Each file specifies unique starting stats (HP, Sanity, Hunger, Thirst, Draw, HandLimit, DeckCapacity) and starting decks:
1. **湯自強 (John)**: Policeman. Tendency toward high Brutality. Starts with the `"強行"` card.
2. **劉淑莉 (Sarah)**: University professor. Wisdom-focused. Lower starting Sanity or lower Sanity resistance.
3. **李有志 (Leo)**: Mountaineering student. Dexterity-focused. Susceptible to Corruption. Starts with the `"重整"` card.
4. **李曉琳 (Celin)**: Yandere character. Control-type deck blending Brutality and Evil attributes.
5. **于晞 (Nancy)**: Self-harm playstyle (cards cost HP/Sanity to play but yield high values). Higher starting Sanity or higher Sanity resistance.
6. **湯明亮 (Tommy)**: Stressed boy. Average card values are weak, but has a few high-burst cards.

### R2. Story Item Acquisition & Ending Combinations
Story-related key items must **NOT** be in starting decks (lost during disorientation in the forest). They must be found through specific forest exploration events.
- Implement specific key items for each character.
- A character must collect a specific combination of items to unlock their respective true/good/bad endings.

### R3. Special Character Events & Exclusive Card Unlocks
Implement unique events for characters:
- **Tommy's "找到小狗" (Find the Dog) Event**:
  - Mutually exclusive outcomes based on choices/resolution:
    1. **"傑利" (Jerry)**: Equipment card. Reduces the increase rate of Corruption and Evil values when equipped.
    2. **"傑利？" (Jerry?)**: Special action card. While held in hand, it continuously inflicts Corruption/Wound status on the player, but active usage gives extremely high stats (e.g., Str +5, Dex +5, Wis +5).

### R4. NPC Encounters & Interactions
Integrate interactions between characters into `EventManager.cs`. When playing as one character, other characters appear as NPCs in random events. NPC interactions can include:
- Dialogues and story progression.
- Stat changes (HP, Sanity, Hunger, Thirst, Brutality, Corruption, Evil).
- Trading items.
- Triggering a combat clash.

### R5. Persistent Save Tracker for Lore & Endings
Track unlocked endings and collected story items per character in a persistent user save file (under `user://`).

---

## Acceptance Criteria

### Compilation & Structure
- [ ] Code builds successfully with `dotnet build` (0 errors).
- [ ] All 6 character `.tres` resource files are created in `src/resources/characters/` and load correctly in the game.
- [ ] Unique starting cards are defined for each character class.

### Functional Behavior
- [ ] Selecting different characters loads their respective starting stats, HandLimit, and starting deck.
- [ ] Key story items are not present in starting decks but can be successfully acquired in exploration events.
- [ ] Tommy's "找到小狗" event rewards either "傑利" (reduces Corruption/Evil gain rate) or "傑利？" exclusively depending on the choice.
- [ ] The save system correctly persists lore unlock flags and collected unique items to the local user folder across sessions.
- [ ] NPC encounter events successfully trigger and resolve through the existing `EventManager`.
