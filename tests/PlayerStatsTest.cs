using Godot;
using DeepForest.Character;

namespace DeepForest.Tests;

public class PlayerStatsTest
{
    [Test]
    public void TestInitializePlayerHiddenStats()
    {
        var data = new CharacterData
        {
            StartingBrutality = 10,
            StartingCorruption = 20,
            StartingEvil = 30
        };

        using var player = new Player();
        player.InitializeFromData(data);

        Assert.AreEqual(10, player.Brutality, "Brutality should match StartingBrutality.");
        Assert.AreEqual(20, player.Corruption, "Corruption should match StartingCorruption.");
        Assert.AreEqual(30, player.Evil, "Evil should match StartingEvil.");
    }

    [Test]
    public void TestInitializePlayerHiddenStatsClamping()
    {
        var data = new CharacterData
        {
            StartingBrutality = 150,
            StartingCorruption = -50,
            StartingEvil = 85
        };

        using var player = new Player();
        player.InitializeFromData(data);

        Assert.AreEqual(100, player.Brutality, "Brutality should be clamped to 100.");
        Assert.AreEqual(0, player.Corruption, "Corruption should be clamped to 0.");
        Assert.AreEqual(85, player.Evil, "Evil should match StartingEvil within bounds.");
    }
}
