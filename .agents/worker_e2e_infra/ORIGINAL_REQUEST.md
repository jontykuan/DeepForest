## 2026-07-02T05:43:38Z
You are teamwork_preview_worker.
Your task is to establish the E2E test infrastructure in the project:
1. Create a `tests/` directory at the project root `d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot project/DeepForest/`.
2. Write the following three files:
   - `tests/TestAttribute.cs`: Contains the custom attribute `TestAttribute`.
   - `tests/Assert.cs`: Contains `AssertionException` and helper class `Assert` with methods like `IsTrue`, `IsFalse`, `AreEqual`, `AreNotEqual`, `IsNull`, `IsNotNull`, `Throws`.
   - `tests/TestRunner.cs`: A script inheriting from `SceneTree` that uses reflection to scan all types for `[Test]` attributes, runs them, outputs results, and exits Godot with 0 (all pass) or 1 (failures/errors).
3. Run a build using `dotnet build` from the project directory to verify it compiles successfully.
4. Run the test runner headlessly using:
   `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe --headless --path "d:\cykuan\Godot_v4.6.2-stable_mono_win64\godot project\DeepForest" --script tests/TestRunner.cs`
   Verify it runs and reports 0 tests run.
5. Provide a handoff report at `.agents/worker_e2e_infra/handoff.md` summarizing the created files and test run outcome.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.
