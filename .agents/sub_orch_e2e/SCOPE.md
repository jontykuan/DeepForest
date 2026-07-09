# Scope: E2E Testing Track

## Architecture
- Standalone E2E Test Suite running inside Godot using a custom headless TestRunner script.
- Discovers tests using C# reflection and executes them.
- Validates Character Data (.tres), Event outcomes, NPC interactions, Deck states, Ending validations, and Persistence.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Test Infrastructure | Create TestRunner, Assertions, and Attribute setup under `tests/`. | None | PLANNED |
| 2 | Tier 1 & 2 Test Cases | Implement 70 test cases covering features and edge cases. | M1 | PLANNED |
| 3 | Tier 3 & 4 Test Cases | Implement 12 test cases covering combinations and application scenarios. | M2 | PLANNED |
| 4 | Verification & Report | Compile tests, run them, and verify failure patterns match expected missing implementation. | M3 | PLANNED |

## Interface Contracts
- `TestRunner`: Headless entry point via `--script tests/TestRunner.cs`.
- `Assert`: Custom assertion library for testing within Godot.
