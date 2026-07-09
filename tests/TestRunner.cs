using Godot;
using System;
using System.Reflection;
using System.Linq;
using DeepForest.Core;
using DeepForest.Scene;
using DeepForest.Combat;

namespace DeepForest.Tests;

public partial class TestRunner : SceneTree
{
    public override void _Initialize()
    {
        GD.Print("==========================================");
        GD.Print("Starting E2E Test Suite Run...");
        GD.Print("==========================================");

        // Instantiate Autoloads to set Singletons for E2E tests
        var gameState = new GameState();
        GameState.Instance = gameState;
        Root.AddChild(gameState);

        var envSystem = new EnvironmentSystem();
        EnvironmentSystem.Instance = envSystem;
        Root.AddChild(envSystem);

        var mapManager = new MapManager();
        MapManager.Instance = mapManager;
        Root.AddChild(mapManager);

        var turnManager = new TurnManager();
        TurnManager.Instance = turnManager;
        Root.AddChild(turnManager);

        var combatManager = new CombatManager();
        CombatManager.Instance = combatManager;
        Root.AddChild(combatManager);

        int testsRun = 0;
        int testsPassed = 0;
        int testsFailed = 0;

        try
        {
            var assembly = typeof(TestRunner).Assembly;
            var testTypes = assembly.GetTypes();

            foreach (var type in testTypes)
            {
                // Only scan non-abstract classes
                if (type.IsAbstract && type.IsSealed) continue; // Static class is abstract and sealed
                if (type.IsAbstract) continue;

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var hasTestAttr = false;
                    foreach (var attr in method.GetCustomAttributes(true))
                    {
                        if (attr.GetType().Name == "TestAttribute" || attr.GetType().FullName == "DeepForest.Tests.TestAttribute")
                        {
                            hasTestAttr = true;
                            break;
                        }
                    }

                    if (!hasTestAttr) continue;

                    testsRun++;
                    GD.Print($"Running: {type.FullName}.{method.Name}...");

                    try
                    {
                        object? instance = null;
                        if (!method.IsStatic)
                        {
                            instance = Activator.CreateInstance(type);
                        }

                        method.Invoke(instance, null);
                        testsPassed++;
                        GD.Print($"[PASS] {type.FullName}.{method.Name}");
                    }
                    catch (TargetInvocationException tie)
                    {
                        testsFailed++;
                        var innerEx = tie.InnerException ?? tie;
                        GD.PrintErr($"[FAIL] {type.FullName}.{method.Name}: {innerEx.GetType().Name} - {innerEx.Message}");
                        GD.PrintErr(innerEx.StackTrace);
                    }
                    catch (Exception ex)
                    {
                        testsFailed++;
                        GD.PrintErr($"[ERROR] {type.FullName}.{method.Name}: Failed to run or instantiate test class: {ex.GetType().Name} - {ex.Message}");
                        GD.PrintErr(ex.StackTrace);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Fatal error during test discovery: {ex.Message}");
            GD.PrintErr(ex.StackTrace);
            Quit(1);
            return;
        }

        GD.Print("==========================================");
        GD.Print($"Test Run Completed.");
        GD.Print($"Total Tests Run: {testsRun}");
        GD.Print($"Passed: {testsPassed}");
        GD.Print($"Failed: {testsFailed}");
        GD.Print("==========================================");

        if (testsFailed > 0)
        {
            Quit(1);
        }
        else
        {
            Quit(0);
        }
    }
}
