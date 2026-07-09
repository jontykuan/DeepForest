## 2026-07-02T13:57:17Z
You are teamwork_preview_worker.
Your task is to create the comprehensive E2E test cases file `tests/E2ETests.cs` at the project root `d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot project/DeepForest/`.

Please write the file exactly as follows:

```csharp
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Narrative;

namespace DeepForest.Tests;

public class CharacterStatsTests
{
    private CharacterData? LoadChar(string name)
    {
        return GD.Load<CharacterData>($"res://src/resources/characters/{name}.tres");
    }

    [Test]
    public void TestJohnStartingStats()
    {
        var data = LoadChar("character_john");
        Assert.IsNotNull(data, "John resource not implemented yet.");
        Assert.AreEqual("湯自強", data.CharacterName);
        Assert.IsTrue(data.MaxHp > 0);
        Assert.IsTrue(data.DeckCapacity >= 30);
    }
    
    [Test]
    public void TestSarahStartingStats()
    {
        var data = LoadChar("character_sarah");
        Assert.IsNotNull(data, "Sarah resource not implemented yet.");
        Assert.AreEqual("劉淑莉", data.CharacterName);
        Assert.IsTrue(data.MaxSanity < 100, "Sarah should have lower starting Sanity.");
    }

    [Test]
    public void TestLeoStartingStats()
    {
        var data = LoadChar("character_leo");
        Assert.IsNotNull(data, "Leo resource not implemented yet.");
        Assert.AreEqual("李有志", data.CharacterName);
    }

    [Test]
    public void TestCelinStartingStats()
    {
        var data = LoadChar("character_celin");
        Assert.IsNotNull(data, "Celin resource not implemented yet.");
        Assert.AreEqual("李曉琳", data.CharacterName);
    }

    [Test]
    public void TestNancyStartingStats()
    {
        var data = LoadChar("character_nancy");
        Assert.IsNotNull(data, "Nancy resource not implemented yet.");
        Assert.AreEqual("于晞", data.CharacterName);
        Assert.IsTrue(data.MaxSanity >= 100, "Nancy should have higher starting Sanity.");
    }

    [Test]
    public void TestTommyStartingStats()
    {
        var data = LoadChar("character_tommy");
        Assert.IsNotNull(data, "Tommy resource not implemented yet.");
        Assert.AreEqual("湯明亮", data.CharacterName);
    }

    [Test]
    public void TestLoadNonExistentCharacter()
    {
        var data = GD.Load<CharacterData>("res://src/resources/characters/character_nonexistent.tres");
        Assert.IsNull(data);
    }

    [Test]
    public void TestPlayerInitializeFromNullData()
    {
        var player = new Player();
        Assert.Throws<NullReferenceException>(() => player.InitializeFromData(null!));
    }

    [Test]
    public void TestCharacterStatsLimits()
    {
        var player = new Player();
        player.MaxHp = 100;
        player.CurrentHp = 150;
        Assert.AreEqual(100, player.CurrentHp);
    }

    [Test]
    public void TestCharacterStatsMinimumClamping()
    {
        var player = new Player();
        player.MaxHp = 100;
        player.CurrentHp = -50;
        Assert.AreEqual(0, player.CurrentHp);
    }
}

public class DeckAndTraitsTests
{
    private CharacterData? LoadChar(string name) => GD.Load<CharacterData>($"res://src/resources/characters/{name}.tres");

    [Test]
    public void TestJohnStartingDeckContainsForce()
    {
        var data = LoadChar("character_john");
        Assert.IsNotNull(data, "John resource not implemented yet.");
        bool hasForce = false;
        foreach (var card in data.StartingDeck)
        {
            if (card.CardName == "強行") hasForce = true;
        }
        Assert.IsTrue(hasForce, "John should start with '強行' card.");
    }

    [Test]
    public void TestLeoStartingDeckContainsReorganize()
    {
        var data = LoadChar("character_leo");
        Assert.IsNotNull(data, "Leo resource not implemented yet.");
        bool hasReorg = false;
        foreach (var card in data.StartingDeck)
        {
            if (card.CardName == "重整") hasReorg = true;
        }
        Assert.IsTrue(hasReorg, "Leo should start with '重整' card.");
    }

    [Test]
    public void TestSarahStartingDeckHasWisdomFocus()
    {
        var data = LoadChar("character_sarah");
        Assert.IsNotNull(data, "Sarah resource not implemented yet.");
        int wisCount = 0;
        foreach (var card in data.StartingDeck)
        {
            if (card.CardClass == CardClass.ActionWis) wisCount++;
        }
        Assert.IsTrue(wisCount >= 3, "Sarah should have wisdom-focused deck.");
    }

    [Test]
    public void TestCelinStartingDeckHasControlFocus()
    {
        var data = LoadChar("character_celin");
        Assert.IsNotNull(data, "Celin resource not implemented yet.");
        bool hasControl = false;
        foreach (var card in data.StartingDeck)
        {
            if (card.CardName.Contains("控制") || card.CardName.Contains("佔有") || card.CardClass == CardClass.ActionStr || card.CardClass == CardClass.Curse) hasControl = true;
        }
        Assert.IsTrue(hasControl, "Celin should have control focus starting deck.");
    }

    [Test]
    public void TestNancyStartingDeckHasSelfHarm()
    {
        var data = LoadChar("character_nancy");
        Assert.IsNotNull(data, "Nancy resource not implemented yet.");
        bool hasSelfHarm = false;
        foreach (var card in data.StartingDeck)
        {
            if (card.HpCost > 0 || card.SanityCost > 0) hasSelfHarm = true;
        }
        Assert.IsTrue(hasSelfHarm, "Nancy should have self-harm deck.");
    }

    [Test]
    public void TestTommyStartingDeckAverageValues()
    {
        var data = LoadChar("character_tommy");
        Assert.IsNotNull(data, "Tommy resource not implemented yet.");
        bool hasTommyDeck = data.StartingDeck.Count > 0;
        Assert.IsTrue(hasTommyDeck);
    }

    [Test]
    public void TestDeckWeightConservation()
    {
        var deck = new Deck();
        var card = new Card();
        card.CardName = "測試武器";
        card.CardClass = CardClass.Equipment;
        card.Weight = 3;
        
        deck.Initialize(new List<Card> { card });
        Assert.AreEqual(3, deck.GetTotalWeight());
        
        deck.DrawCards(1, true);
        var handCard = deck.Hand[0];
        deck.EquipCard(handCard);
        
        Assert.AreEqual(3, deck.GetTotalWeight());
        Assert.AreEqual(0, deck.Hand.Count);
        Assert.AreEqual(1, deck.EquippedCards.Count);
        Assert.AreEqual(1, deck.DiscardPile.Count);
        Assert.AreEqual(3, deck.DiscardPile[0].Weight);
    }

    [Test]
    public void TestDeckCapacityOverflow()
    {
        var deck = new Deck();
        var player = new Player();
        player.DeckCapacity = 5;
        GameState.Instance.PlayerInstance = player;

        var card1 = new Card { Weight = 3 };
        var card2 = new Card { Weight = 3 };

        deck.Initialize(new List<Card> { card1 });
        Assert.IsTrue(deck.AddCardToDiscardPile(card2) == false, "Should block card when weight exceeds capacity.");
    }

    [Test]
    public void TestDrawFromEmptyDeckReshuffles()
    {
        var deck = new Deck();
        var player = new Player { HandLimit = 5 };
        GameState.Instance.PlayerInstance = player;

        var card = new Card { CardName = "卡片" };
        deck.Initialize(new List<Card>());
        deck.DiscardPile.Add(card);

        bool reshuffled = deck.DrawCards(1);
        Assert.IsTrue(reshuffled, "Should trigger reshuffle.");
        Assert.AreEqual(1, deck.Hand.Count);
        Assert.AreEqual(0, deck.DiscardPile.Count);
    }

    [Test]
    public void TestCorruptionCardSanityCostOnDraw()
    {
        var deck = new Deck();
        var player = new Player { HandLimit = 5, MaxSanity = 100, CurrentSanity = 100 };
        GameState.Instance.PlayerInstance = player;

        var card = new Card { CardName = "穢祟附身", EffectTags = CardEffectTag.Corruption };
        deck.Initialize(new List<Card> { card });
        
        deck.DrawCards(1);
        Assert.AreEqual(85, player.CurrentSanity, "Sanity should decrease by 15 when drawing corruption card.");
    }
}

public class StoryItemTests
{
    private CharacterData? LoadChar(string name) => GD.Load<CharacterData>($"res://src/resources/characters/{name}.tres");

    [Test]
    public void TestJohnStartingDeckNoKeyItems()
    {
        var data = LoadChar("character_john");
        if (data == null) return;
        foreach (var card in data.StartingDeck)
        {
            Assert.AreNotEqual(CardClass.KeyItem, card.CardClass);
        }
    }

    [Test]
    public void TestSarahStartingDeckNoKeyItems()
    {
        var data = LoadChar("character_sarah");
        if (data == null) return;
        foreach (var card in data.StartingDeck)
        {
            Assert.AreNotEqual(CardClass.KeyItem, card.CardClass);
        }
    }

    [Test]
    public void TestLeoStartingDeckNoKeyItems()
    {
        var data = LoadChar("character_leo");
        if (data == null) return;
        foreach (var card in data.StartingDeck)
        {
            Assert.AreNotEqual(CardClass.KeyItem, card.CardClass);
        }
    }

    [Test]
    public void TestTommyStartingDeckNoKeyItems()
    {
        var data = LoadChar("character_tommy");
        if (data == null) return;
        foreach (var card in data.StartingDeck)
        {
            Assert.AreNotEqual(CardClass.KeyItem, card.CardClass);
        }
    }

    [Test]
    public void TestAcquireKeyItemDuringExploration()
    {
        var deck = new Deck();
        var player = new Player { DeckCapacity = 30 };
        GameState.Instance.PlayerInstance = player;

        var keyItem = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem, Weight = 1 };
        bool added = deck.AddCardToDiscardPile(keyItem);
        Assert.IsTrue(added);
        Assert.IsTrue(deck.DiscardPile.Contains(keyItem));
    }

    [Test]
    public void TestDuplicateKeyItemPrevention()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestKeyItemWeightCost()
    {
        var deck = new Deck();
        var player = new Player { DeckCapacity = 10 };
        GameState.Instance.PlayerInstance = player;
        var keyItem = new Card { CardName = "重型關鍵物", CardClass = CardClass.KeyItem, Weight = 8 };
        deck.Initialize(new List<Card>());
        Assert.IsTrue(deck.AddCardToDiscardPile(keyItem));
        Assert.AreEqual(8, deck.GetTotalWeight());
    }

    [Test]
    public void TestKeyItemCannotBePlayed()
    {
        var keyItem = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        Assert.AreEqual(CardClass.KeyItem, keyItem.CardClass);
    }

    [Test]
    public void TestRemoveKeyItemCorrectly()
    {
        var deck = new Deck();
        var player = new Player { DeckCapacity = 10 };
        GameState.Instance.PlayerInstance = player;
        var keyItem = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem, Weight = 1 };
        deck.Initialize(new List<Card> { keyItem });
        Assert.AreEqual(1, deck.GetTotalWeight());
        deck.DrawCards(1);
        deck.DiscardHand();
        Assert.AreEqual(1, deck.GetTotalWeight());
    }

    [Test]
    public void TestEndingKeyItemCombinationChecks()
    {
        Assert.IsTrue(true);
    }
}

public class TommyEventTests
{
    private static object? CallStaticMethod(string typeName, string methodName, params object?[] parameters)
    {
        var type = Type.GetType(typeName);
        if (type == null) throw new AssertionException($"Type {typeName} not found.");
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null) throw new AssertionException($"Static method {methodName} not found in {typeName}.");
        return method.Invoke(null, parameters);
    }

    [Test]
    public void TestFindDogEventTrigger()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleFindDogEvent", player, deck, 1);
        }
        catch (AssertionException ex)
        {
            throw new AssertionException("EventManager not implemented yet: " + ex.Message);
        }
    }

    [Test]
    public void TestFindDogChoiceJerryAcquisition()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        deck.Initialize(new List<Card>());

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleFindDogEvent", player, deck, 1);
            bool hasJerry = false;
            foreach (var c in deck.DiscardPile)
            {
                if (c.CardName == "傑利") hasJerry = true;
            }
            Assert.IsTrue(hasJerry, "Should acquire '傑利' card.");
        }
        catch (AssertionException)
        {
            throw;
        }
    }

    [Test]
    public void TestFindDogChoiceJerryQuestionAcquisition()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        deck.Initialize(new List<Card>());

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleFindDogEvent", player, deck, 2);
            bool hasJerryQuestion = false;
            foreach (var c in deck.DiscardPile)
            {
                if (c.CardName == "傑利？") hasJerryQuestion = true;
            }
            Assert.IsTrue(hasJerryQuestion, "Should acquire '傑利？' card.");
        }
        catch (AssertionException)
        {
            throw;
        }
    }

    [Test]
    public void TestFindDogExclusivity()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        deck.Initialize(new List<Card>());

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleFindDogEvent", player, deck, 1);
            foreach (var c in deck.DiscardPile)
            {
                Assert.AreNotEqual("傑利？", c.CardName, "Choice 1 should not add '傑利？'.");
            }
        }
        catch (AssertionException)
        {
            throw;
        }
    }

    [Test]
    public void TestJerryCardEquipBehavior()
    {
        var card = new Card { CardName = "傑利", CardClass = CardClass.Equipment };
        Assert.AreEqual(CardClass.Equipment, card.CardClass);
    }

    [Test]
    public void TestJerryQuestionCardHoldBehavior()
    {
        var card = new Card { CardName = "傑利？", CardClass = CardClass.ActionStr };
        Assert.AreEqual("傑利？", card.CardName);
    }

    [Test]
    public void TestJerryQuestionCardPlayStats()
    {
        var card = new Card { CardName = "傑利？", StrValue = 5, DexValue = 5, WisValue = 5 };
        Assert.AreEqual(5, card.StrValue);
        Assert.AreEqual(5, card.DexValue);
        Assert.AreEqual(5, card.WisValue);
    }

    [Test]
    public void TestFindDogEventInvalidChoice()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        deck.Initialize(new List<Card>());

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleFindDogEvent", player, deck, -1);
            Assert.AreEqual(0, deck.DiscardPile.Count);
        }
        catch (AssertionException)
        {
            throw;
        }
    }

    [Test]
    public void TestJerryCardUniqueness()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestJerryQuestionPlayCosts()
    {
        var card = new Card { CardName = "傑利？", SanityCost = 10 };
        Assert.IsTrue(card.SanityCost > 0);
    }
}

public class NpcEncounterTests
{
    private static object? CallStaticMethod(string typeName, string methodName, params object?[] parameters)
    {
        var type = Type.GetType(typeName);
        if (type == null) throw new AssertionException($"Type {typeName} not found.");
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null) throw new AssertionException($"Static method {methodName} not found in {typeName}.");
        return method.Invoke(null, parameters);
    }

    [Test]
    public void TestJohnEncounterNpcSarah()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100 };
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleNpcEncounter", player, deck, "Sarah");
        }
        catch (AssertionException ex)
        {
            throw new AssertionException("EventManager not implemented yet: " + ex.Message);
        }
    }

    [Test]
    public void TestNpcEncounterStatChange()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, Brutality = 10 };
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleNpcEncounter", player, deck, "Sarah");
        }
        catch (AssertionException)
        {
            throw;
        }
    }

    [Test]
    public void TestNpcEncounterTradeItem()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleNpcEncounter", player, deck, "Sarah");
        }
        catch (AssertionException)
        {
            throw;
        }
    }

    [Test]
    public void TestNpcEncounterCombatClash()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleNpcEncounter", player, deck, "Sarah");
        }
        catch (AssertionException)
        {
            throw;
        }
    }

    [Test]
    public void TestNpcEncounterRandomSelection()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestNpcEncounterWithLowSanity()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestNpcEncounterTradeInsufficientStats()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestNpcEncounterCombatClashHpZero()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestNpcEncounterHighCorruptionWarning()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestNpcEncounterInvalidCharacterName()
    {
        var player = new Player();
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleNpcEncounter", player, deck, "InvalidNpc");
        }
        catch (AssertionException)
        {
            throw;
        }
    }
}

public class EndingManagerTests
{
    [Test]
    public void TestEscapeEndingCondition()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDay = 5;
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth;

        var mapCard = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        deck.Initialize(new List<Card> { mapCard });

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Escape ending with Map Item at max depth.");
    }

    [Test]
    public void TestHpZeroDisappearanceEnding()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 0 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Disappearance ending at HP = 0.");
    }

    [Test]
    public void TestSanityZeroDisappearanceEnding()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 0 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Disappearance ending at Sanity = 0.");
    }

    [Test]
    public void TestBrutalityEndingCondition()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Brutality = 85 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Brutality ending at high Brutality.");
    }

    [Test]
    public void TestCorruptionEndingCondition()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Corruption = 85 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Corruption ending at high Corruption.");
    }

    [Test]
    public void TestEvilEndingCondition()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Evil = 85 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Evil ending at high Evil.");
    }

    [Test]
    public void TestDayLimitDisappearanceEnding()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDay = GameConstants.WinDay + 1;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Disappearance ending when days limit is exceeded.");
    }

    [Test]
    public void TestEndingPriorityBrutalityOverHpZero()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 0, Brutality = 90 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered);
    }

    [Test]
    public void TestEndingPriorityCorruptionOverEvil()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, Corruption = 90, Evil = 90 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered);
    }

    [Test]
    public void TestNoEndingWhenHealthyAndExploring()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDay = 2;
        GameState.Instance.CurrentDepth = 5;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsFalse(endingTriggered);
    }
}

public class StoryUnlockTests
{
    [Test]
    public void TestStoryUnlockTriggeredSaving()
    {
        var storyUnlock = new StoryUnlock();
        storyUnlock._Ready();
        storyUnlock.UnlockStorySegment("測試結局", "測試描述內容");
        Assert.IsTrue(storyUnlock.GetUnlockedTitles().Contains("測試結局"));
    }

    [Test]
    public void TestStoryUnlockLoading()
    {
        var storyUnlock = new StoryUnlock();
        storyUnlock._Ready();
        storyUnlock.UnlockStorySegment("測試載入結局", "描述內容");
        storyUnlock.LoadProgress();
        Assert.IsTrue(storyUnlock.GetUnlockedTitles().Contains("測試載入結局"));
    }

    [Test]
    public void TestStoryUnlockDuplicatePrevention()
    {
        var storyUnlock = new StoryUnlock();
        storyUnlock._Ready();
        storyUnlock.UnlockStorySegment("重複結局", "1");
        storyUnlock.UnlockStorySegment("重複結局", "2");
        
        int count = 0;
        foreach (var title in storyUnlock.GetUnlockedTitles())
        {
            if (title == "重複結局") count++;
        }
        Assert.AreEqual(1, count);
    }

    [Test]
    public void TestStoryUnlockGetStoryDescription()
    {
        var storyUnlock = new StoryUnlock();
        storyUnlock._Ready();
        storyUnlock.UnlockStorySegment("描述結局", "這是一段描述文字");
        Assert.AreEqual("這是一段描述文字", storyUnlock.GetStoryDescription("描述結局"));
    }

    [Test]
    public void TestStoryUnlockGetUnlockedTitles()
    {
        var storyUnlock = new StoryUnlock();
        storyUnlock._Ready();
        Assert.IsNotNull(storyUnlock.GetUnlockedTitles());
    }

    [Test]
    public void TestStoryUnlockEmptySaveFileHandling()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestStoryUnlockInvalidConfigFormat()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestStoryUnlockPersistenceFilePath()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestStoryUnlockClearAllProgress()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestStoryUnlockMultipleWrites()
    {
        var storyUnlock = new StoryUnlock();
        storyUnlock._Ready();
        storyUnlock.UnlockStorySegment("結局A", "A");
        storyUnlock.UnlockStorySegment("結局B", "B");
        Assert.IsTrue(storyUnlock.GetUnlockedTitles().Contains("結局A"));
        Assert.IsTrue(storyUnlock.GetUnlockedTitles().Contains("結局B"));
    }
}

public class CrossFeatureTests
{
    private static object? CallStaticMethod(string typeName, string methodName, params object?[] parameters)
    {
        var type = Type.GetType(typeName);
        if (type == null) throw new AssertionException($"Type {typeName} not found.");
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null) throw new AssertionException($"Static method {methodName} not found in {typeName}.");
        return method.Invoke(null, parameters);
    }

    [Test]
    public void TestJohnBrutalityEndingWithPolicemanDeck()
    {
        var john = GD.Load<CharacterData>("res://src/resources/characters/character_john.tres");
        if (john == null) return;

        var player = new Player();
        player.InitializeFromData(john);
        player.Brutality = 85;

        var deck = new Deck();
        var endingManager = new EndingManager();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool triggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(triggered);
    }

    [Test]
    public void TestSarahLowSanityNpcEncounterAndDisappearance()
    {
        var sarah = GD.Load<CharacterData>("res://src/resources/characters/character_sarah.tres");
        if (sarah == null) return;

        var player = new Player();
        player.InitializeFromData(sarah);
        player.CurrentSanity = 10;

        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleNpcEncounter", player, deck, "John");
        }
        catch (AssertionException)
        {
        }
    }

    [Test]
    public void TestLeoCorruptionDogEventInteraction()
    {
        var leo = GD.Load<CharacterData>("res://src/resources/characters/character_leo.tres");
        if (leo == null) return;

        var player = new Player();
        player.InitializeFromData(leo);
        var deck = new Deck();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        try
        {
            CallStaticMethod("DeepForest.Narrative.EventManager", "HandleFindDogEvent", player, deck, 2);
        }
        catch (AssertionException)
        {
        }
    }

    [Test]
    public void TestCelinEvilEndingWithMapItems()
    {
        var celin = GD.Load<CharacterData>("res://src/resources/characters/character_celin.tres");
        if (celin == null) return;

        var player = new Player();
        player.InitializeFromData(celin);
        player.Evil = 90;

        var deck = new Deck();
        var endingManager = new EndingManager();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth;

        var mapCard = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        deck.Initialize(new List<Card> { mapCard });

        bool triggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(triggered);
    }

    [Test]
    public void TestNancySelfHarmStartingDeckHPZeroDisappearance()
    {
        var nancy = GD.Load<CharacterData>("res://src/resources/characters/character_nancy.tres");
        if (nancy == null) return;

        var player = new Player();
        player.InitializeFromData(nancy);
        player.CurrentHp = 5;

        player.CurrentHp -= 5;
        Assert.AreEqual(0, player.CurrentHp);

        var deck = new Deck();
        var endingManager = new EndingManager();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

        bool triggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(triggered);
    }

    [Test]
    public void TestTommyDogEventJerrySanityClash()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestMultipleEndingsWriteSequence()
    {
        Assert.IsTrue(true);
    }
}

public class RealWorldScenarioTests
{
    [Test]
    public void Scenario1JohnPolicemanBrutalityPath()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, Brutality = 0 };
        player.Brutality += 20;
        player.Brutality += 30;
        player.Brutality += 40;
        Assert.IsTrue(player.Brutality >= 80);

        var endingManager = new EndingManager();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = new Deck();
        GameState.Instance.EndingManagerInstance = endingManager;

        bool triggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(triggered);
    }

    [Test]
    public void Scenario2SarahProfessorEscapePath()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80 };
        var deck = new Deck();
        var map = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        deck.Initialize(new List<Card> { map });

        var endingManager = new EndingManager();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth;

        bool triggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(triggered);
    }

    [Test]
    public void Scenario3LeoMountaineerDisappearancePath()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100 };
        GameState.Instance.CurrentDay = GameConstants.WinDay + 5;
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = new Deck();

        var endingManager = new EndingManager();
        GameState.Instance.EndingManagerInstance = endingManager;

        bool triggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(triggered);
    }

    [Test]
    public void Scenario4NancySelfHarmSurviveAndTrade()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100 };
        player.CurrentHp -= 30;
        Assert.AreEqual(70, player.CurrentHp);
        player.CurrentHp += 20;
        Assert.AreEqual(90, player.CurrentHp);
    }

    [Test]
    public void Scenario5TommyStressedDogSelection()
    {
        var player = new Player { Corruption = 50 };
        int corruptionGain = 20;
        bool hasJerry = true;
        if (hasJerry)
        {
            corruptionGain /= 2;
        }
        player.Corruption += corruptionGain;
        Assert.AreEqual(60, player.Corruption);
    }
}
```
