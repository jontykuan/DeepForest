# Original User Request

## 2026-07-02T13:37:47+08:00

You are the E2E Testing Orchestrator for the DeepForest project.
Your mission is to establish the E2E test infrastructure, design the test suite, write the tests (at least 82 test cases across Tier 1, 2, 3, and 4), and provide a way to execute them.
Requirements:
1. Review d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot project/DeepForest/PROJECT.md and d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot project/DeepForest/.agents/ORIGINAL_REQUEST.md.
2. Establish a test runner / test scripts that can programmatically verify:
   - Starting stats, deck, hand limits, and character definitions (.tres) for all 6 characters.
   - Core gameplay mechanics: starting cards, key item exploration acquisition.
   - Tommy's Find Dog event outcomes (Jerry vs. Jerry? mutually exclusive choices).
   - NPC encounter effects (dialogues, trades, combat clash).
   - EndingManager ending validations and persistent StoryUnlock saving/loading.
3. Publish TEST_INFRA.md at the project root explaining the design, layout, and how to execute the tests.
4. Write and check the test cases (e.g., using a standalone test project, NUnit, or a custom script runner integrated into the Godot project).
5. Once all tests are written and ready (even if some fail initially due to missing implementation), compile and publish TEST_READY.md.
Set your working directory to: d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot project/DeepForest/.agents/sub_orch_e2e/.
Make sure to follow the E2E Testing Track guidelines in AGENTS.md. DO NOT write or modify game implementation source code in src/scripts/.
