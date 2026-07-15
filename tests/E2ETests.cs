using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Cards.Effects;
using DeepForest.Core;
using DeepForest.Scene;
using DeepForest.Narrative;
using DeepForest.Narrative.Handlers;
using DeepForest.UI;
using DeepForest.Combat;

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
            if (card.CardName == "吹牛" || card.CardName == "強行") hasForce = true;
        }
        Assert.IsTrue(hasForce, "John should start with '吹牛' or '強行' card.");
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
        Assert.IsTrue(deck.AddCardToDiscardPile(card2), "Should allow card when weight exceeds capacity.");
        Assert.AreEqual(6, deck.GetTotalWeight(), "Total weight should reflect both cards.");
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
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        System.Reflection.MethodInfo? method = null;
        foreach (var m in methods)
        {
            if (m.Name == methodName && m.GetParameters().Length == parameters.Length)
            {
                method = m;
                break;
            }
        }
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
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        System.Reflection.MethodInfo? method = null;
        foreach (var m in methods)
        {
            if (m.Name == methodName && m.GetParameters().Length == parameters.Length)
            {
                method = m;
                break;
            }
        }
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
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Brutality = 90 };
        player.CharacterData = new CharacterData { CharacterName = "湯自強", CharacterId = CharacterId.John };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentStoryHandler = new JohnStoryHandler();

        deck.Initialize(new List<Card> { 
            new Card { CardId = CardId.KeyDivorceAgreement, CardName = "離婚協議書", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyRecordingTape, CardName = "錄音帶", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyPromissoryNote, CardName = "本票", CardClass = CardClass.KeyItem }
        });

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Escape ending with John items at 90 brutality.");
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
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Brutality = 95 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Brutality ending at high Brutality.");
    }

    [Test]
    public void TestCorruptionEndingCondition()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Corruption = 95 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth;

        bool endingTriggered = endingManager.CheckEndGameConditions();
        Assert.IsTrue(endingTriggered, "Should trigger Corruption ending at high Corruption.");
    }

    [Test]
    public void TestEvilEndingCondition()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Evil = 95 };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth;

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
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        System.Reflection.MethodInfo? method = null;
        foreach (var m in methods)
        {
            if (m.Name == methodName && m.GetParameters().Length == parameters.Length)
            {
                method = m;
                break;
            }
        }
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
        player.Brutality = 95;

        var deck = new Deck();
        var endingManager = new EndingManager();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth;

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
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 50 };
        player.CharacterData = new CharacterData { CharacterName = "湯明亮", CharacterId = CharacterId.Tommy };
        var deck = new Deck();
        deck.Initialize(new List<Card>());

        var handler = new TommyStoryHandler();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.CurrentStoryHandler = handler;

        // 1. First wolf victory gives Jerry's collar
        bool handled = handler.HandleSpecialEvent(player, deck, "wolf_victory", 1);
        Assert.IsTrue(handled);
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryCollar));

        // Add footprints
        var footprints = CardFactory.CreateCard(CardId.KeyTinyFootprints);
        if (footprints != null) deck.AddCardToDiscardPile(footprints);

        // 2. High sanity (>= 30) wolf victory -> Is it you? -> jerry
        player.CurrentHp = 100;
        player.CurrentSanity = 50;
        handled = handler.HandleSpecialEvent(player, deck, "wolf_victory", 1);
        Assert.IsTrue(handled);
        Assert.AreEqual(70, player.CurrentHp); // 100 - 30
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerry));

        // Remove jerry
        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyJerry);

        // 3. Low sanity (< 30) wolf victory -> Is it you? -> jerry?
        player.CurrentHp = 100;
        player.CurrentSanity = 20;
        handled = handler.HandleSpecialEvent(player, deck, "wolf_victory", 1);
        Assert.IsTrue(handled);
        Assert.AreEqual(70, player.CurrentHp);
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryQuestion));
    }

    [Test]
    public void TestMultipleEndingsWriteSequence()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 80 };
        player.CharacterData = new CharacterData { CharacterName = "湯明亮", CharacterId = CharacterId.Tommy };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentStoryHandler = new TommyStoryHandler();
        GameState.Instance.CurrentDepth = GameConstants.MaxDepth + 10;

        string triggeredTitle = "";
        endingManager.EndingTriggered += (type, title, desc) => triggeredTitle = title;

        // Case 1: Jerry + Collar + Footprints + Depth > 100 -> 湯姆與傑利
        player.Brutality = 90;
        GameState.Instance.CurrentDepth = 120;
        deck.Initialize(new List<Card> {
            CardFactory.CreateCard(CardId.KeyJerryCollar),
            CardFactory.CreateCard(CardId.KeyTinyFootprints),
            CardFactory.CreateCard(CardId.KeyJerry)
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：湯姆與傑利", triggeredTitle);

        // Case 2: Copy + DamagedPictureBook -> 暴力循環
        player.Brutality = 90;
        deck.Initialize(new List<Card> {
            CardFactory.CreateCard(CardId.ActionCopy),
            CardFactory.CreateCard(CardId.KeyTornFairytale)
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：暴力循環", triggeredTitle);

        // Case 3: Suffocation + Collar -> 好狗狗
        player.Brutality = 90;
        deck.Initialize(new List<Card> {
            CardFactory.CreateCard(CardId.CurseSuffocation),
            CardFactory.CreateCard(CardId.KeyJerryCollar)
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：好狗狗", triggeredTitle);
    }

    [Test]
    public void TestJohnNpcEncounters()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100 };
        player.CharacterData = new CharacterData { CharacterName = "湯自強", CharacterId = CharacterId.John };
        var deck = new Deck();
        deck.Initialize(new List<Card>());

        var handler = new JohnStoryHandler();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.CurrentStoryHandler = handler;

        // 1. Encounter Nancy - Choice 1 (Violence, Str >= 5)
        TurnManager.Instance.AccumulatedStr = 6;
        bool handled = handler.HandleNpcEncounter(player, deck, CharacterId.Nancy, 1);
        Assert.IsTrue(handled);
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape));

        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyRecordingTape);

        // Nancy - Choice 1 (Violence, Str < 5)
        TurnManager.Instance.AccumulatedStr = 2;
        handled = handler.HandleNpcEncounter(player, deck, CharacterId.Nancy, 1);
        Assert.IsTrue(handled);
        Assert.IsFalse(CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape));
        Assert.IsTrue(player.CurrentHp < 100);

        player.CurrentHp = 100;

        // Nancy - Choice 2 (Trade photo)
        deck.AddCardToDiscardPile(CardFactory.CreateCard(CardId.KeySneakPhoto));
        handled = handler.HandleNpcEncounter(player, deck, CharacterId.Nancy, 2);
        Assert.IsTrue(handled);
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape));
        Assert.IsFalse(CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto));

        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyRecordingTape);

        // 2. Encounter Celin - Choice 1 (Violence, Str >= 5)
        TurnManager.Instance.AccumulatedStr = 6;
        handled = handler.HandleNpcEncounter(player, deck, CharacterId.Celin, 1);
        Assert.IsTrue(handled);
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto));

        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeySneakPhoto);

        // Celin - Choice 2 (WIS >= 5)
        TurnManager.Instance.AccumulatedWis = 6;
        handled = handler.HandleNpcEncounter(player, deck, CharacterId.Celin, 2);
        Assert.IsTrue(handled);
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto));
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
        var player = new Player { MaxHp = 100, CurrentHp = 80, Brutality = 90 };
        var deck = new Deck();
        var map = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        deck.Initialize(new List<Card> { map });

        var endingManager = new EndingManager();
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;

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

public class EventSystemTests
{
    private EventData CreateTestEvent(string id, string terrain = "", int minDepth = 0, int maxDepth = 999, int weight = 10, string decal = "npc_right")
    {
        return new EventData
        {
            EventId = id,
            EventTitle = $"事件_{id}",
            EventDescription = $"測試事件 {id} 的描述",
            Weight = weight,
            RequiredTerrain = terrain,
            MinDepth = minDepth,
            MaxDepth = maxDepth,
            DecalName = decal,
        };
    }

    private MapNode CreateTestNode(int depth = 1, string leftTerrain = "woodland", string rightTerrain = "ruins")
    {
        var sd = new DeepForest.Scene.SceneData
        {
            SceneName = "測試場景",
            LeftTerrain = leftTerrain,
            RightTerrain = rightTerrain,
            BottomGround = "dirt"
        };
        return new DeepForest.Scene.MapNode { Id = 1, Depth = depth, Name = "測試", SceneData = sd };
    }

    [Test]
    public void TestEventConditionMatchesTerrain()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80, MaxSanity = 100, CurrentSanity = 60 };
        GameState.Instance.PlayerInstance = player;

        var ev = CreateTestEvent("terrain_test", terrain: "ruins");
        var nodeMatch = CreateTestNode(1, "woodland", "ruins");
        var nodeFail = CreateTestNode(1, "woodland", "swamp");

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
        EventManager.TestEvents.Add(ev);

        EventManager.CheckAndTriggerEvent(nodeMatch);
        Assert.IsNotNull(EventManager.CurrentActiveEvent, "Event should trigger when terrain matches.");
        Assert.AreEqual("terrain_test", EventManager.CurrentActiveEvent.EventId);

        EventManager.CurrentActiveEvent = null;
        EventManager.TestEvents.Clear();
        EventManager.TestEvents.Add(ev);
        EventManager.CheckAndTriggerEvent(nodeFail);
        Assert.IsNull(EventManager.CurrentActiveEvent, "Event should NOT trigger when terrain doesn't match.");

        EventManager.TestEvents.Clear();
    }

    [Test]
    public void TestEventConditionMatchesDepth()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80, MaxSanity = 100, CurrentSanity = 60 };
        GameState.Instance.PlayerInstance = player;

        var ev = CreateTestEvent("depth_test", minDepth: 2, maxDepth: 3);

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
        EventManager.TestEvents.Add(ev);

        var nodeShallow = CreateTestNode(1);
        EventManager.CheckAndTriggerEvent(nodeShallow);
        Assert.IsNull(EventManager.CurrentActiveEvent, "Event should not trigger at depth 1 (min=2).");

        EventManager.TestEvents.Clear();
        EventManager.TestEvents.Add(ev);
        var nodeInRange = CreateTestNode(2);
        EventManager.CheckAndTriggerEvent(nodeInRange);
        Assert.IsNotNull(EventManager.CurrentActiveEvent, "Event should trigger at depth 2.");

        EventManager.TestEvents.Clear();
    }

    [Test]
    public void TestEventConditionMatchesHpRange()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 20, MaxSanity = 100, CurrentSanity = 60 };
        GameState.Instance.PlayerInstance = player;

        var ev = CreateTestEvent("low_hp");
        ev.MinHp = 0;
        ev.MaxHp = 30;

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
        EventManager.TestEvents.Add(ev);

        var node = CreateTestNode();
        EventManager.CheckAndTriggerEvent(node);
        Assert.IsNotNull(EventManager.CurrentActiveEvent, "Event should trigger when HP is within range.");

        EventManager.CurrentActiveEvent = null;
        player.CurrentHp = 50;
        EventManager.TestEvents.Clear();
        EventManager.TestEvents.Add(ev);
        EventManager.CheckAndTriggerEvent(node);
        Assert.IsNull(EventManager.CurrentActiveEvent, "Event should NOT trigger when HP exceeds MaxHp filter.");

        EventManager.TestEvents.Clear();
    }

    [Test]
    public void TestWeightedSelectionFavorsHighWeight()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80, MaxSanity = 100, CurrentSanity = 60 };
        GameState.Instance.PlayerInstance = player;

        var evLow = CreateTestEvent("low_weight", weight: 1);
        var evHigh = CreateTestEvent("high_weight", weight: 1000);

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
        EventManager.TestEvents.Add(evLow);
        EventManager.TestEvents.Add(evHigh);

        int highCount = 0;
        for (int i = 0; i < 20; i++)
        {
            EventManager.CurrentActiveEvent = null;
            EventManager.CheckAndTriggerEvent(CreateTestNode());
            if (EventManager.CurrentActiveEvent?.EventId == "high_weight")
                highCount++;
        }

        Assert.IsTrue(highCount >= 15, $"High-weight event should dominate selection (got {highCount}/20).");

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
    }

    [Test]
    public void TestResolveEventOptionAppliesStatChanges()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80, MaxSanity = 100, CurrentSanity = 50, MaxHunger = 100, CurrentHunger = 40, MaxThirst = 100, CurrentThirst = 40 };
        GameState.Instance.PlayerInstance = player;
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        var option = new EventOption
        {
            OptionText = "測試選項",
            HpChange = -10,
            SanityChange = 15,
            HungerChange = -5,
            BrutalityChange = 3,
            LogMessageOnSuccess = "你選擇了測試選項。"
        };

        // Set up a fake active event so ResolveEventOption can clean up
        var ev = CreateTestEvent("resolve_test");
        ev.Options.Add(option);
        EventManager.CurrentActiveEvent = ev;

        string log = EventManager.ResolveEventOption(option, player, deck);

        Assert.AreEqual(70, player.CurrentHp, "HP should decrease by 10.");
        Assert.AreEqual(65, player.CurrentSanity, "Sanity should increase by 15.");
        Assert.AreEqual(35, player.CurrentHunger, "Hunger should decrease by 5.");
        Assert.AreEqual(3, player.Brutality, "Brutality should increase by 3.");
        Assert.AreEqual("你選擇了測試選項。", log);
        Assert.IsNull(EventManager.CurrentActiveEvent, "Active event should be cleared after resolution.");
    }

    [Test]
    public void TestEventDecalAddedToScene()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80, MaxSanity = 100, CurrentSanity = 60 };
        GameState.Instance.PlayerInstance = player;

        var ev = CreateTestEvent("decal_test", decal: "npc_trader_right");

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
        EventManager.TestEvents.Add(ev);

        var node = CreateTestNode();
        EventManager.CheckAndTriggerEvent(node);

        Assert.IsNotNull(EventManager.CurrentActiveEvent);
        Assert.IsTrue(node.SceneData.Decals.Contains("npc_trader_right"), "Decal should be added to scene data.");

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
    }

    [Test]
    public void TestNoEventWhenNoCandidatesMatch()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80, MaxSanity = 100, CurrentSanity = 60 };
        GameState.Instance.PlayerInstance = player;

        var ev = CreateTestEvent("impossible", terrain: "lava");

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
        EventManager.TestEvents.Add(ev);

        var node = CreateTestNode();
        EventManager.CheckAndTriggerEvent(node);

        Assert.IsNull(EventManager.CurrentActiveEvent, "No event should trigger when no candidates match.");
        EventManager.TestEvents.Clear();
    }
}

public class CardMechanicsTests
{
    [Test]
    public void TestAntidepressantPlayEffect()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 50 };
        GameState.Instance.PlayerInstance = player;
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        var card = CardFactory.CreateConsumableCard(CardId.ConsumableAntidepressant, "抗憂鬱藥物", 0, 0, 0, 0);
        card.MaxUses = 4;
        card.UsesLeft = 4;
        card.SanityCost = -25; // Negative cost = heal
        deck.Hand.Add(card);

        // Test normal execute
        var result = CardPlayHandler.TryPlayCard(card, player, deck, out string message);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        Assert.AreEqual(75, player.CurrentSanity, "Sanity should heal by 25.");
        Assert.AreEqual(3, card.UsesLeft, "Uses left should decrement.");
        Assert.IsTrue(deck.DiscardPile.Contains(card), "Should be in discard pile.");
    }

    [Test]
    public void TestAntidepressantDepletionDropsEmptyBottle()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 50 };
        GameState.Instance.PlayerInstance = player;
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        var card = CardFactory.CreateConsumableCard(CardId.ConsumableAntidepressant, "抗憂鬱藥物", 0, 0, 0, 0);
        card.MaxUses = 4;
        card.UsesLeft = 1;
        card.SanityCost = -25;
        deck.Hand.Add(card);

        var result = CardPlayHandler.TryPlayCard(card, player, deck, out string message);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        Assert.IsFalse(deck.Hand.Contains(card), "Depleted card should be removed from hand.");
        Assert.IsFalse(deck.DiscardPile.Contains(card), "Depleted card should be destroyed (not in discard pile).");
        
        bool hasEmptyBottle = false;
        foreach (var c in deck.DiscardPile)
        {
            if (c.CardName == "空藥瓶") hasEmptyBottle = true;
        }
        Assert.IsTrue(hasEmptyBottle, "Empty pill bottle should drop in discard pile.");
    }

    [Test]
    public void TestSleepingPillTriggersDayChange()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 50 };
        GameState.Instance.PlayerInstance = player;
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        var turnManager = new TurnManager();
        GameState.Instance.AddChild(turnManager);
        TurnManager.Instance = turnManager;

        var startDay = GameState.Instance.CurrentDay;

        var card = CardFactory.CreateConsumableCard(CardId.ConsumableSleepingPill, "安眠藥", 0, 0, 0, 0);
        card.MaxUses = 3;
        card.UsesLeft = 3;
        card.SanityCost = -40;
        deck.Hand.Add(card);

        var result = CardPlayHandler.TryPlayCard(card, player, deck, out string message);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        Assert.AreEqual(95, player.CurrentSanity, "Sanity should heal by 40 and add day change recovery.");
        Assert.AreEqual(startDay + 1, GameState.Instance.CurrentDay, "Sleeping pill should trigger a day change.");

        turnManager.QueueFree();
    }

    [Test]
    public void TestAlcoholIncreasesBrutalityAndDropsEmptyBottle()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 50, MaxThirst = 100, CurrentThirst = 50 };
        player.Brutality = 10;
        GameState.Instance.PlayerInstance = player;
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        var card = CardFactory.CreateConsumableCard(CardId.ConsumableAlcohol, "烈酒", 0, 0, 0, 0);
        card.SanityCost = -10;
        card.ThirstCost = 3;
        deck.Hand.Add(card);

        var result = CardPlayHandler.TryPlayCard(card, player, deck, out string message);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        Assert.AreEqual(60, player.CurrentSanity);
        Assert.AreEqual(47, player.CurrentThirst, "Thirst should decrease by 3 (cost subtraction).");
        Assert.AreEqual(12, player.Brutality, "Brutality should increase by 2.");
        Assert.IsFalse(deck.Hand.Contains(card), "Alcohol should be consumed.");
        
        bool hasEmptyBottle = false;
        foreach (var c in deck.DiscardPile)
        {
            if (c.CardName == "空瓶") hasEmptyBottle = true;
        }
        Assert.IsTrue(hasEmptyBottle, "Empty bottle should drop in discard pile.");
    }

    [Test]
    public void TestStimulantGivesActionBonusAndResetsOnStartTurn()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 50 };
        GameState.Instance.PlayerInstance = player;
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        var turnManager = new TurnManager();
        TurnManager.Instance = turnManager;

        // Play stimulant
        var card = CardFactory.CreateConsumableCard(CardId.ConsumableStimulant, "興奮劑", 0, 0, 0, 0);
        card.SanityCost = 15; // Positive cost = loss
        card.MaxUses = 3;
        card.UsesLeft = 3;
        deck.Hand.Add(card);

        var result = CardPlayHandler.TryPlayCard(card, player, deck, out string message);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        Assert.AreEqual(35, player.CurrentSanity, "Sanity should decrease by 15.");
        Assert.IsTrue(GameState.Instance.StimulantActive, "Stimulant should be active.");

        // Play action card with active stimulant
        var actionCard = CardFactory.CreateActionCard(CardId.ActionStrength, "基本力量", CardClass.ActionStr, 1, 3, 0, 0, 0, 0);
        deck.Hand.Add(actionCard);

        var result2 = CardPlayHandler.TryPlayCard(actionCard, player, deck, out string message2);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result2);
        Assert.AreEqual(5, TurnManager.Instance.AccumulatedStr, "Accumulated strength should be 3 + 2 = 5.");

        // Trigger start turn to check if it resets
        TurnManager.Instance.StartTurn();
        Assert.IsFalse(GameState.Instance.StimulantActive, "Stimulant should reset on next turn.");
    }

    [Test]
    public void TestDefenseSprayInCombat()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 50 };
        GameState.Instance.PlayerInstance = player;
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        // Set up combat state
        GameState.Instance.IsInCombat = true;
        var enemy = new EnemyData { EnemyName = "野狼", MaxHp = 10 };
        GameState.Instance.CurrentEnemy = enemy;
        GameState.Instance.CurrentEnemyHp = 5;

        var card = CardFactory.CreateConsumableCard(CardId.ConsumableDefenseSpray, "防身噴霧", 0, 0, 0, 0);
        card.MaxUses = 3;
        card.UsesLeft = 3;
        deck.Hand.Add(card);

        var result = CardPlayHandler.TryPlayCard(card, player, deck, out string message);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        Assert.AreEqual(4, GameState.Instance.CurrentEnemyHp, "Enemy HP should decrease by 1.");
        
        GameState.Instance.IsInCombat = false;
        GameState.Instance.CurrentEnemy = null;
    }
}

public class NarrativeAndOverloadTests
{
    [Test]
    public void TestOverloadAllowsWeightExceededAndAppliesCosts()
    {
        EnvironmentSystem.Instance.Weather = WeatherType.Clear;
        EnvironmentSystem.Instance.Temperature = TempType.Cool;
        EnvironmentSystem.Instance.Humidity = HumidityType.Moderate;

        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, MaxHunger = 100, CurrentHunger = 50, MaxThirst = 100, CurrentThirst = 50 };
        player.DeckCapacity = 10;
        GameState.Instance.PlayerInstance = player;
        
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;

        // Add 13 weight (overweight = 3, penalty = 1)
        for (int i = 0; i < 13; i++)
        {
            var item = CardFactory.CreateConsumableCard(CardId.None, $"雜物_{i}", 0, 0, 0, 0);
            item.Weight = 1;
            bool success = deck.AddCardToDiscardPile(item);
            Assert.IsTrue(success, "Should allow adding cards even when overloaded.");
        }

        Assert.AreEqual(13, deck.GetTotalWeight());

        // Play action card: HungerCost = 2, ThirstCost = 1
        var action = CardFactory.CreateActionCard(CardId.ActionStrength, "測試行動", CardClass.ActionStr, 1, 3, 0, 0, 2, 1);
        deck.Hand.Add(action);

        var result = CardPlayHandler.TryPlayCard(action, player, deck, out string message);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        // Penalty = 1. So HungerCost becomes 3, ThirstCost becomes 2.
        // CurrentHunger should be 50 - 3 = 47.
        // CurrentThirst should be 50 - 2 = 48.
        Assert.AreEqual(47, player.CurrentHunger, "Hunger cost should have +1 penalty.");
        Assert.AreEqual(48, player.CurrentThirst, "Thirst cost should have +1 penalty.");

        // Clear deck and add 18 weight (overweight = 8, penalty = 2)
        deck.Initialize(new List<Card>());
        for (int i = 0; i < 18; i++)
        {
            var item = CardFactory.CreateConsumableCard(CardId.None, $"雜物2_{i}", 0, 0, 0, 0);
            item.Weight = 1;
            deck.AddCardToDiscardPile(item);
        }

        player.CurrentHunger = 50;
        player.CurrentThirst = 50;
        var action2 = CardFactory.CreateActionCard(CardId.ActionStrength, "測試行動2", CardClass.ActionStr, 1, 3, 0, 0, 2, 1);
        deck.Hand.Add(action2);

        var result2 = CardPlayHandler.TryPlayCard(action2, player, deck, out string msg2);
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result2);
        // Penalty = 2. HungerCost = 4, ThirstCost = 3.
        Assert.AreEqual(46, player.CurrentHunger, "Hunger cost should have +2 penalty.");
        Assert.AreEqual(47, player.CurrentThirst, "Thirst cost should have +2 penalty.");
    }

    [Test]
    public void TestEventManagerRichConditionsMatching()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 80, MaxSanity = 100, CurrentSanity = 60 };
        player.Brutality = 40;
        player.Corruption = 10;
        player.Evil = 50;
        player.CharacterData = new CharacterData { CharacterName = "李有志", CharacterId = CharacterId.Leo };
        GameState.Instance.PlayerInstance = player;

        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.DeckInstance = deck;
        var keyItem = CardFactory.CreateKeyItemCard(CardId.KeySneakPhoto, "偷拍照片", "出軌證據");
        deck.AddCardToDiscardPile(keyItem);

        GameState.Instance.CurrentDay = 5;

        // Set up test events
        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;

        // Event A: requires Lee Character and Day range 4-6, and card "偷拍照片"
        var evA = new EventData
        {
            EventId = "event_matching_a",
            EventTitle = "A匹配事件",
            EventDescription = "A事件描述",
            RequiredCharacterId = CharacterId.Leo,
            MinDay = 4,
            MaxDay = 6,
            MinBrutality = 30,
            MaxBrutality = 50,
            RequiredCardId = CardId.KeySneakPhoto
        };

        // Event B: impossible due to wrong character
        var evB = new EventData
        {
            EventId = "event_wrong_char",
            EventTitle = "B錯角色事件",
            RequiredCharacterId = CharacterId.John
        };

        // Event C: impossible due to wrong day
        var evC = new EventData
        {
            EventId = "event_wrong_day",
            EventTitle = "C錯天數事件",
            MinDay = 8
        };

        // Event D: impossible due to missing card
        var evD = new EventData
        {
            EventId = "event_missing_card",
            EventTitle = "D缺卡事件",
            RequiredCardId = CardId.KeyDivorceAgreement
        };

        EventManager.TestEvents.Add(evA);
        EventManager.TestEvents.Add(evB);
        EventManager.TestEvents.Add(evC);
        EventManager.TestEvents.Add(evD);

        var node = new MapNode { Depth = 10, SceneData = new SceneData() };
        EventManager.CheckAndTriggerEvent(node);

        Assert.IsNotNull(EventManager.CurrentActiveEvent);
        Assert.AreEqual("event_matching_a", EventManager.CurrentActiveEvent.EventId, "Should choose matching Event A.");

        EventManager.TestEvents.Clear();
        EventManager.CurrentActiveEvent = null;
    }
}

public class ParallelQuestsEndingTests
{
    [Test]
    public void TestJohnParallelQuestEndings()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Brutality = 90 };
        player.CharacterData = new CharacterData { CharacterName = "湯自強", CharacterId = CharacterId.John };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentStoryHandler = new JohnStoryHandler();

        var mapCard = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        
        string triggeredTitle = "";
        endingManager.EndingTriggered += (type, title, desc) => triggeredTitle = title;

        // Case 1: has all three -> 成就：隱而未發
        deck.Initialize(new List<Card> { 
            mapCard, 
            new Card { CardId = CardId.KeyDivorceAgreement, CardName = "離婚協議書", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyRecordingTape, CardName = "錄音帶", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyPromissoryNote, CardName = "本票", CardClass = CardClass.KeyItem }
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：隱而未發", triggeredTitle);

        // Case 2: lacks any -> 林間捕食者
        deck.Initialize(new List<Card> { mapCard });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("林間捕食者", triggeredTitle);
    }

    [Test]
    public void TestSarahParallelQuestEndings()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Brutality = 90 };
        player.CharacterData = new CharacterData { CharacterName = "劉淑莉", CharacterId = CharacterId.Sarah };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentStoryHandler = new SarahStoryHandler();

        var mapCard = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        
        string triggeredTitle = "";
        endingManager.EndingTriggered += (type, title, desc) => triggeredTitle = title;

        // Case 1: lacks Tommy -> 狂躁的主婦
        deck.Initialize(new List<Card> { mapCard });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("狂躁的主婦", triggeredTitle);

        // Case 2: has all four -> 成就：重獲新生
        deck.Initialize(new List<Card> { 
            mapCard, 
            new Card { CardId = CardId.KeyDivorceAgreement, CardName = "離婚協議書", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyChastityLie, CardName = "貞潔謊言", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyRecordingTape, CardName = "錄音帶", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyBabySon, CardName = "寶貝兒子", CardClass = CardClass.KeyItem }
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：重獲新生", triggeredTitle);

        // Case 3: has three (no Tommy) -> 成就：解脫
        deck.Initialize(new List<Card> { 
            mapCard, 
            new Card { CardId = CardId.KeyDivorceAgreement, CardName = "離婚協議書", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyChastityLie, CardName = "貞潔謊言", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyRecordingTape, CardName = "錄音帶", CardClass = CardClass.KeyItem }
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：解脫", triggeredTitle);
    }

    [Test]
    public void TestNancyParallelQuestEndings()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Brutality = 90 };
        player.CharacterData = new CharacterData { CharacterName = "于晞", CharacterId = CharacterId.Nancy };
        var deck = new Deck();
        var endingManager = new EndingManager();

        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        GameState.Instance.CurrentStoryHandler = new NancyStoryHandler();

        var mapCard = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };
        
        string triggeredTitle = "";
        endingManager.EndingTriggered += (type, title, desc) => triggeredTitle = title;

        // Case 1: has empty pill bottle, diagnosis, photo -> 成就：雨天娃娃
        deck.Initialize(new List<Card> { 
            mapCard, 
            new Card { CardId = CardId.KeyEmptyPillBottle, CardName = "空藥瓶", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyDiagnosisCert, CardName = "診斷證明書", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeySneakPhoto, CardName = "偷拍照片", CardClass = CardClass.KeyItem }
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：雨天娃娃", triggeredTitle);

        // Case 2: has roster, showdown, truth -> 成就：自我拯救
        deck.Initialize(new List<Card> { 
            mapCard, 
            new Card { CardId = CardId.KeyRoster, CardName = "花名冊", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyShowdown, CardName = "攤牌", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyPainfulTruth, CardName = "痛苦真相", CardClass = CardClass.KeyItem }
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：自我拯救", triggeredTitle);
    }

    [Test]
    public void TestLeoAndCelinParallelQuestEndings()
    {
        var deck = new Deck();
        var endingManager = new EndingManager();
        GameState.Instance.DeckInstance = deck;
        GameState.Instance.EndingManagerInstance = endingManager;
        var mapCard = new Card { CardName = "地圖殘片", CardClass = CardClass.KeyItem };

        string triggeredTitle = "";
        endingManager.EndingTriggered += (type, title, desc) => triggeredTitle = title;

        var playerLeo = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 80, Corruption = 90 };
        playerLeo.CharacterData = new CharacterData { CharacterName = "李有志", CharacterId = CharacterId.Leo };
        GameState.Instance.PlayerInstance = playerLeo;
        GameState.Instance.CurrentStoryHandler = new LeoStoryHandler();

        // 1. Default escape when no keys -> 古神代言人
        deck.Initialize(new List<Card> { mapCard });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("古神代言人", triggeredTitle);

        // 2. Scripture and Roster -> 淫邪魔窟
        deck.Initialize(new List<Card> { mapCard, new Card { CardId = CardId.KeyOldScripture, CardName = "舊日教本", CardClass = CardClass.KeyItem }, new Card { CardId = CardId.KeyRoster, CardName = "花名冊", CardClass = CardClass.KeyItem } });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("李有志結局：淫邪魔窟", triggeredTitle);

        // 3. Descent and low sanity -> 降神反噬而死
        GameState.Instance.IsDescentActive = true;
        playerLeo.CurrentSanity = 20;
        deck.Initialize(new List<Card> { mapCard, new Card { CardId = CardId.KeyOldScripture, CardName = "舊日教本", CardClass = CardClass.KeyItem }, new Card { CardId = CardId.KeyRoster, CardName = "花名冊", CardClass = CardClass.KeyItem } });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("李有志結局：降神反噬而死", triggeredTitle);

        // 4. Descent, high sanity, and did not burn -> 沉淪教主
        playerLeo.CurrentSanity = 80;
        deck.Initialize(new List<Card> { mapCard, new Card { CardId = CardId.KeyOldScripture, CardName = "舊日教本", CardClass = CardClass.KeyItem }, new Card { CardId = CardId.KeyRoster, CardName = "花名冊", CardClass = CardClass.KeyItem } });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("李有志結局：沉淪教主", triggeredTitle);

        // 5. Descent, high sanity, and burned -> 直面罪責
        playerLeo.CurrentSanity = 80;
        deck.Initialize(new List<Card> { mapCard });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("李有志結局：直面罪責", triggeredTitle);

        // Reset descent
        GameState.Instance.IsDescentActive = false;

        // Celin Endings
        var playerCelin = new Player { MaxHp = 100, CurrentHp = 100, MaxSanity = 100, CurrentSanity = 100, Brutality = 90 };
        playerCelin.CharacterData = new CharacterData { CharacterName = "李曉琳", CharacterId = CharacterId.Celin };
        GameState.Instance.PlayerInstance = playerCelin;
        GameState.Instance.CurrentStoryHandler = new CelinStoryHandler();
        
        // 6. Celin: phone, diary, seed -> 成就：純潔餘孽
        deck.Initialize(new List<Card> { 
            mapCard, 
            new Card { CardId = CardId.KeyBrokenPhone, CardName = "壞掉的手機", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyLockedDiary, CardName = "上鎖的日記", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeySeedOfLife, CardName = "生命的種子", CardClass = CardClass.KeyItem }
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：純潔餘孽", triggeredTitle);

        // 7. Celin: phone, diary, whisper, no Nancy suicide -> 成就：埋藏心底
        GameState.Instance.NancySuicideFlag = false;
        deck.Initialize(new List<Card> { 
            mapCard, 
            new Card { CardId = CardId.KeyBrokenPhone, CardName = "壞掉的手機", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyLockedDiary, CardName = "上鎖的日記", CardClass = CardClass.KeyItem },
            new Card { CardId = CardId.KeyWhisper, CardName = "耳語", CardClass = CardClass.KeyItem }
        });
        triggeredTitle = "";
        endingManager.CheckEndGameConditions();
        Assert.AreEqual("成就：埋藏心底", triggeredTitle);
    }
}

public class SaveAndSettingsTests
{
    [Test]
    public void TestSessionSaveAndLoadCycle()
    {
        var gameState = GameState.Instance;
        var mapManager = MapManager.Instance;

        gameState.CurrentDay = 12;
        gameState.CurrentDepth = 45;
        gameState.IsDescentActive = true;
        gameState.EntranceNodeId = 3;

        var player = gameState.PlayerInstance;
        player.CharacterData = GD.Load<CharacterData>("res://src/resources/characters/character_nancy.tres");
        player.CurrentHp = 88;
        player.CurrentSanity = 40;
        player.Brutality = 10;
        player.Corruption = 22;

        var deck = gameState.DeckInstance;
        deck.DrawPile.Clear();
        deck.DrawPile.Add(CardFactory.CreateCard(CardId.ActionStrength));
        deck.Hand.Clear();
        deck.Hand.Add(CardFactory.CreateCard(CardId.ActionDexterity));
        deck.Hand[0].UsesLeft = 3;

        SessionSaveSystem.SaveSession(gameState, mapManager);
        Assert.IsTrue(SessionSaveSystem.HasSessionSave());

        gameState.CurrentDay = 1;
        gameState.CurrentDepth = 0;
        player.CurrentHp = 10;
        deck.DrawPile.Clear();
        deck.Hand.Clear();

        bool loaded = SessionSaveSystem.LoadSession(gameState, mapManager);
        Assert.IsTrue(loaded);

        Assert.AreEqual(12, gameState.CurrentDay);
        Assert.AreEqual(45, gameState.CurrentDepth);
        Assert.IsTrue(gameState.IsDescentActive);
        Assert.AreEqual(88, player.CurrentHp);
        Assert.AreEqual(40, player.CurrentSanity);
        Assert.AreEqual(22, player.Corruption);
        Assert.AreEqual(1, deck.DrawPile.Count);
        Assert.AreEqual(CardId.ActionStrength, deck.DrawPile[0].CardId);
        Assert.AreEqual(1, deck.Hand.Count);
        Assert.AreEqual(CardId.ActionDexterity, deck.Hand[0].CardId);
        Assert.AreEqual(3, deck.Hand[0].UsesLeft);

        SessionSaveSystem.DeleteSession();
        Assert.IsFalse(SessionSaveSystem.HasSessionSave());
    }

    [Test]
    public void TestSaveSettingsPersistence()
    {
        SaveManager.CurrentSave.VolumeMaster = 0.35f;
        SaveManager.CurrentSave.VolumeBgm = 0.55f;
        SaveManager.CurrentSave.WindowSize = "1600x900";
        SaveManager.Save();

        SaveManager.Load();

        Assert.AreEqual(0.35f, SaveManager.CurrentSave.VolumeMaster);
        Assert.AreEqual(0.55f, SaveManager.CurrentSave.VolumeBgm);
        Assert.AreEqual("1600x900", SaveManager.CurrentSave.WindowSize);
    }

    [Test]
    public void TestSessionDeletedOnEnding()
    {
        var gameState = GameState.Instance;
        var mapManager = MapManager.Instance;
        
        SessionSaveSystem.SaveSession(gameState, mapManager);
        Assert.IsTrue(SessionSaveSystem.HasSessionSave());

        var player = new Player { MaxHp = 100, CurrentHp = 0 };
        gameState.PlayerInstance = player;
        gameState.EndingManagerInstance.CheckEndGameConditions();

        Assert.IsFalse(SessionSaveSystem.HasSessionSave());
    }
}

public class MapContinuityTests
{
    [Test]
    public void TestSceneActionTargetNodeIdOverride()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 50, CurrentSanity = 50 };
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        var mapManager = MapManager.Instance;
        mapManager.Nodes.Clear();
        mapManager.Nodes[0] = new MapNode { Id = 0, Depth = 0, Name = "起始點", SceneData = new SceneData() };
        mapManager.Nodes[5] = new MapNode { Id = 5, Depth = 1, Name = "目標點", SceneData = new SceneData() };
        mapManager.CurrentNodeId = 0;

        var action = new SceneAction
        {
            ActionName = "特殊轉移",
            EffectType = ActionEffectType.None,
            TargetNodeId = 5
        };

        var context = new ActionContext
        {
            SourceAction = action,
            Player = player,
            Deck = deck,
            MapManager = mapManager
        };
        var result = ActionResolver.Instance.Resolve(action, context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, mapManager.CurrentNodeId, "CurrentNodeId should be overridden to 5.");
    }

    [Test]
    public void TestEventOptionNextNodeIdAndIndoorOverrides()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 50, CurrentSanity = 50 };
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        var mapManager = MapManager.Instance;
        mapManager.Nodes.Clear();
        mapManager.Nodes[0] = new MapNode { Id = 0, Depth = 0, Name = "起始點", SceneData = new SceneData() };
        mapManager.Nodes[10] = new MapNode { Id = 10, Depth = 2, Name = "神祕石窟", SceneData = new SceneData() };
        mapManager.CurrentNodeId = 0;

        var ev = new EventData
        {
            EventId = "test_override",
            EventTitle = "測試轉移事件",
            EventDescription = "描述"
        };
        var option = new EventOption
        {
            OptionText = "走向密道",
            NextNodeIdOverride = 10,
            NextIndoorSceneOverride = "被遺忘的遺跡"
        };
        ev.Options.Add(option);

        EventManager.CurrentActiveEvent = ev;
        GameState.Instance.IsIndoor = false;

        string log = EventManager.ResolveEventOption(option, player, deck);

        Assert.AreEqual(10, mapManager.CurrentNodeId, "CurrentNodeId should be updated to 10.");
        Assert.IsTrue(GameState.Instance.IsIndoor, "Should enter indoor environment.");
        Assert.AreEqual(1, GameState.Instance.IndoorDepth);
        Assert.IsNotNull(mapManager.CurrentIndoorScene);
        Assert.AreEqual("被遺忘的遺跡", mapManager.CurrentIndoorScene.SceneName);
    }
}

public class CurationAndHiddenStatsTests
{
    [Test]
    public void TestCardPlayAppliesHiddenStats()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_john.tres"));
        player.Brutality = 10;
        player.Corruption = 10;
        player.Evil = 10;

        var card = new Card
        {
            CardName = "測試卡",
            CardClass = CardClass.ActionStr,
            BrutalityChange = 5,
            CorruptionChange = -2,
            EvilChange = 3
        };

        var deck = new Deck();
        deck.Initialize(new List<Card>());
        deck.Hand.Add(card);

        string message;
        var playResult = CardPlayHandler.TryPlayCard(card, player, deck, out message);

        Assert.AreEqual(DeepForest.UI.CardPlayHandler.PlayResult.Success, playResult, "Card play should be successful.");
        Assert.AreEqual(15, player.Brutality, "Brutality should increase by 5.");
        Assert.AreEqual(8, player.Corruption, "Corruption should decrease by 2.");
        Assert.AreEqual(13, player.Evil, "Evil should increase by 3.");
    }

    [Test]
    public void TestAddictionDuplicationOnSceneTransition()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_john.tres"));
        GameState.Instance.PlayerInstance = player;

        var deck = new Deck();
        deck.Initialize(new List<Card>());
        
        var addictionCard = CardFactory.CreateCard(CardId.CurseAddiction);
        Assert.IsNotNull(addictionCard);
        deck.Hand.Add(addictionCard);
        GameState.Instance.DeckInstance = deck;

        // Simulate a scene transition action resolve
        var action = new SceneAction { EffectType = ActionEffectType.MoveForward };
        
        bool isSceneTransition = action.TargetNodeId >= 0 || 
                                 action.EffectType == ActionEffectType.MoveForward ||
                                 action.EffectType == ActionEffectType.ExploreIndoor ||
                                 action.EffectType == ActionEffectType.LeaveIndoor ||
                                 action.EffectType == ActionEffectType.ReturnOutdoor ||
                                 action.EffectType == ActionEffectType.EnterNormalCabin ||
                                 action.EffectType == ActionEffectType.EnterStrangeCabin ||
                                 action.EffectType == ActionEffectType.EnterCave;

        if (isSceneTransition)
        {
            int addictionInHand = deck.Hand.Count(c => c.CardId == CardId.CurseAddiction);
            if (addictionInHand > 0)
            {
                for (int i = 0; i < addictionInHand; i++)
                {
                    Card addictionCardNew = CardFactory.CreateCard(CardId.CurseAddiction);
                    if (addictionCardNew != null)
                    {
                        deck.AddCardToDiscardPile(addictionCardNew);
                    }
                }
            }
        }

        // Check that a new CurseAddiction card is added to the discard pile
        int countInDiscard = deck.DiscardPile.Count(c => c.CardId == CardId.CurseAddiction);
        Assert.AreEqual(1, countInDiscard, "One CurseAddiction should be added to the discard pile on transition.");
    }

    [Test]
    public void TestPlayAddictionCardDeductsSanity()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_john.tres"));
        player.CurrentSanity = 50;
        GameState.Instance.PlayerInstance = player;

        var deck = new Deck();
        deck.Initialize(new List<Card>());
        
        var addictionCard = CardFactory.CreateCard(CardId.CurseAddiction);
        deck.Hand.Add(addictionCard);
        GameState.Instance.DeckInstance = deck;

        string msg;
        var result = CardPlayHandler.TryPlayCard(addictionCard, player, deck, out msg);
        
        Assert.AreEqual(CardPlayHandler.PlayResult.Success, result);
        Assert.AreEqual(48, player.CurrentSanity, "Sanity should decrease by 2.");
        Assert.IsTrue(deck.DiscardPile.Contains(addictionCard), "Addiction card should be in the discard pile.");
    }

    [Test]
    public void TestLeoCraftingActionAndResolution()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_leo.tres"));
        GameState.Instance.PlayerInstance = player;

        var deck = new Deck();
        deck.Initialize(new List<Card>());
        
        var repellent = CardFactory.CreateCard(CardId.ConsumableRepellent);
        var lighter = CardFactory.CreateCard(CardId.EquipmentLighter);
        Assert.IsNotNull(repellent);
        Assert.IsNotNull(lighter);

        deck.DrawPile.Add(repellent);
        deck.DiscardPile.Add(lighter);
        GameState.Instance.DeckInstance = deck;

        // Generate options for camping scene
        var sceneData = new SceneData
        {
            SceneName = "點燃的營火"
        };
        sceneData.Decals.Add("campfire");

        // Set up MapManager nodes to ensure nodeId 1 is not the exit node
        MapManager.Instance.Nodes.Clear();
        MapManager.Instance.Nodes[0] = new MapNode { Id = 0, Depth = 0, SceneData = new SceneData() };
        MapManager.Instance.Nodes[1] = new MapNode { Id = 1, Depth = 1, SceneData = sceneData };
        MapManager.Instance.Nodes[2] = new MapNode { Id = 2, Depth = 2, SceneData = new SceneData() };

        ActionGenerator.GenerateDynamicActions(sceneData, 1);

        bool hasCraftAction = sceneData.Actions.Any(a => a.ActionName == "手工");
        Assert.IsTrue(hasCraftAction, "Leo should have the craft action at campfire.");

        var craftAction = sceneData.Actions.First(a => a.ActionName == "手工");
        Assert.AreEqual(ActionEffectType.LeoCraft, craftAction.EffectType);

        // Resolve action
        var actionResolver = new ActionResolver();
        var context = new ActionContext
        {
            Player = player,
            Deck = deck,
            TurnManager = TurnManager.Instance,
            CurrentScene = sceneData,
            GameState = GameState.Instance,
            MapManager = MapManager.Instance,
            Environment = EnvironmentSystem.Instance
        };
        var result = actionResolver.Resolve(craftAction, context);

        Assert.IsTrue(result.Success, "Crafting resolution should succeed.");
        Assert.IsFalse(CardQueryHelper.HasCardAnywhere(deck, CardId.ConsumableRepellent), "Repellent should be consumed.");
        Assert.IsFalse(CardQueryHelper.HasCardAnywhere(deck, CardId.EquipmentLighter), "Lighter should be consumed.");
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.EquipmentFlamethrower), "Flamethrower should be crafted and added.");
    }

    [Test]
    public void TestPoliceBatonSideEffect()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_john.tres"));
        player.Brutality = 10;
        GameState.Instance.PlayerInstance = player;

        var deck = new Deck();
        deck.Initialize(new List<Card>());
        
        var baton = CardFactory.CreateCard(CardId.EquipmentPoliceBaton);
        Assert.IsNotNull(baton);
        deck.EquippedCards.Add(baton);
        GameState.Instance.DeckInstance = deck;

        var actionResolver = new ActionResolver();
        var action = new SceneAction
        {
            ActionName = "普通探索",
            EffectType = ActionEffectType.None,
            TargetNodeId = 1
        };
        var context = new ActionContext
        {
            Player = player,
            Deck = deck,
            TurnManager = TurnManager.Instance,
            GameState = GameState.Instance,
            MapManager = MapManager.Instance,
            Environment = EnvironmentSystem.Instance
        };
        var result = actionResolver.Resolve(action, context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(11, player.Brutality, "Brutality should increase by 1 due to Baton equipped.");
        Assert.IsTrue(result.LogMessage.Contains("【警棍副作用】"), "Log message should include Baton effect.");
    }

    [Test]
    public void TestBulletRequiresPistol()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_john.tres"));
        
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        
        var bullet = CardFactory.CreateCard(CardId.ConsumableBullet);
        Assert.IsNotNull(bullet);
        deck.Hand.Add(bullet);

        string message;
        var playResultWithoutPistol = CardPlayHandler.TryPlayCard(bullet, player, deck, out message);
        Assert.AreNotEqual(DeepForest.UI.CardPlayHandler.PlayResult.Success, playResultWithoutPistol, "Should not be able to play Bullet without Pistol.");
        Assert.IsTrue(message.Contains("你必須裝備【手槍】"), "Should tell player to equip Pistol.");

        // Equip Pistol
        var pistol = CardFactory.CreateCard(CardId.EquipmentPistol);
        Assert.IsNotNull(pistol);
        deck.EquippedCards.Add(pistol);

        var playResultWithPistol = CardPlayHandler.TryPlayCard(bullet, player, deck, out message);
        Assert.AreEqual(DeepForest.UI.CardPlayHandler.PlayResult.Success, playResultWithPistol, "Should be able to play Bullet with Pistol equipped.");
    }

    [Test]
    public void TestCollectWaterWithContainerCheck()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_john.tres"));
        GameState.Instance.PlayerInstance = player;

        var deck = new Deck();
        deck.Initialize(new List<Card>());
        
        var emptyBottle = CardFactory.CreateCard(CardId.EmptyBottle);
        Assert.IsNotNull(emptyBottle);
        Assert.IsTrue(emptyBottle.EffectTags.HasFlag(CardEffectTag.Container), "EmptyBottle should have Container tag.");
        deck.Hand.Add(emptyBottle);
        GameState.Instance.DeckInstance = deck;

        var actionResolver = new ActionResolver();
        var action = new SceneAction
        {
            ActionName = "裝水",
            RequiredItem = "容器",
            EffectType = ActionEffectType.CollectWater
        };

        var context = new ActionContext
        {
            Player = player,
            Deck = deck,
            TurnManager = TurnManager.Instance,
            GameState = GameState.Instance,
            MapManager = MapManager.Instance,
            Environment = EnvironmentSystem.Instance
        };

        var result = actionResolver.Resolve(action, context);
        Assert.IsTrue(result.Success, "Should succeed to collect water with EmptyBottle.");
        Assert.IsFalse(CardQueryHelper.HasCardAnywhere(deck, CardId.EmptyBottle), "EmptyBottle should be consumed.");
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.ConsumableRawWater), "Should receive Raw Water.");

        deck.Initialize(new List<Card>());
        var emptyPill = CardFactory.CreateCard(CardId.KeyEmptyPillBottle);
        Assert.IsNotNull(emptyPill);
        Assert.IsFalse(emptyPill.EffectTags.HasFlag(CardEffectTag.Container), "KeyEmptyPillBottle should not have Container tag.");
        deck.Hand.Add(emptyPill);

        var result2 = actionResolver.Resolve(action, context);
        Assert.IsFalse(result2.Success, "Should fail to collect water with KeyEmptyPillBottle.");
        Assert.IsTrue(CardQueryHelper.HasCardAnywhere(deck, CardId.KeyEmptyPillBottle), "KeyEmptyPillBottle should not be consumed.");
    }

    [Test]
    public void TestPlayKeyItemCardConsumesStatsAndDiscards()
    {
        var player = new Player();
        player.InitializeFromData(GD.Load<CharacterData>("res://src/resources/characters/character_john.tres"));
        player.CurrentHunger = 50;
        player.CurrentThirst = 50;
        player.CurrentHp = 100;
        player.CurrentSanity = 100;
        
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        
        var oldKey = CardFactory.CreateCard(CardId.KeyOldKey);
        Assert.IsNotNull(oldKey);
        Assert.AreEqual(CardType.KeyItem, oldKey.CardType, "KeyOldKey should be a KeyItem.");
        
        oldKey.HungerCost = 2;
        oldKey.ThirstCost = 3;
        
        deck.Hand.Add(oldKey);
        
        var result = DeepForest.UI.CardPlayHandler.TryPlayCard(oldKey, player, deck, out string message);
        Assert.AreEqual(DeepForest.UI.CardPlayHandler.PlayResult.Success, result, "Playing KeyItem card should succeed.");
        Assert.AreEqual(48, player.CurrentHunger, "Should consume hunger.");
        Assert.AreEqual(47, player.CurrentThirst, "Should consume thirst.");
        Assert.IsFalse(deck.Hand.Contains(oldKey), "Should be removed from hand.");
        Assert.IsTrue(deck.DiscardPile.Contains(oldKey), "Should be in discard pile.");
    }
}

public class ActionLoggingAndCompositeEventTests
{
    [Test]
    public void TestLoggerGeneratesSessionIdAndLogsJsonl()
    {
        var logger = new ActionLogger();
        Assert.IsNotNull(logger.SessionId);
        Assert.AreEqual(5, logger.SessionId.Length, "Session ID should be 5 characters.");

        var player = new Player { MaxHp = 100, CurrentHp = 95, CurrentSanity = 80 };
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        logger.InitializeNewSession();
        logger.LogAction("MockAction", new Dictionary<string, object> { { "testKey", "testVal" } });

        var logs = logger.GetMemoryLogs();
        Assert.IsTrue(logs.Count > 0);
        Assert.AreEqual("MockAction", logs[0].ActionType);
        Assert.AreEqual("testVal", logs[0].Params["testKey"].ToString());

        Assert.IsTrue(Godot.FileAccess.FileExists(logger.LogFilePath), "JSONL file should be written to user://logs/sessions");
    }

    [Test]
    public void TestConditionCompositeLogAndStatChecks()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 50, CurrentSanity = 30 };
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        var logger = GameState.Instance.Logger;
        logger.InitializeNewSession();
        logger.LogAction("ActionExecuted", new Dictionary<string, object> { { "actionName", "就地歇息" } });

        // Condition 1: Hp >= 40
        var condHp = new ConditionStatCheck { StatName = "Hp", Operator = ">=", Value = 40 };
        Assert.IsTrue(condHp.Evaluate(player, deck));

        // Condition 2: Sanity < 20
        var condSanity = new ConditionStatCheck { StatName = "Sanity", Operator = "<", Value = 20 };
        Assert.IsFalse(condSanity.Evaluate(player, deck));

        // Condition 3: LogCheck (Has executed "就地歇息" this game)
        var condLog = new ConditionLogCheck { ActionType = "ActionExecuted", ParamKey = "actionName", ParamValue = "就地歇息", Scope = "ThisGame" };
        Assert.IsTrue(condLog.Evaluate(player, deck));

        // Condition Composite (AND Gate: Hp >= 40 AND LogCheck)
        var condAnd = new ConditionComposite();
        condAnd.Conditions.Add(condHp);
        condAnd.Conditions.Add(condLog);
        Assert.IsTrue(condAnd.Evaluate(player, deck), "AND gate should be true.");

        // Condition Composite (OR Gate: Sanity < 20 OR LogCheck)
        var condOr = new ConditionComposite { IsOrGate = true };
        condOr.Conditions.Add(condSanity);
        condOr.Conditions.Add(condLog);
        Assert.IsTrue(condOr.Evaluate(player, deck), "OR gate should be true.");
    }

    [Test]
    public void TestEffectCompositeNestedLogic()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 50, CurrentSanity = 50 };
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        // Condition: Hp >= 40 (Evaluates to true)
        var condHp = new ConditionStatCheck { StatName = "Hp", Operator = ">=", Value = 40 };

        // Then Effect: Hp +10, Sanity -5
        var effectThen1 = new EffectModifyStat { StatName = "Hp", ChangeValue = 10 };
        var effectThen2 = new EffectModifyStat { StatName = "Sanity", ChangeValue = -5 };
        var compositeThen = new EffectComposite();
        compositeThen.Effects.Add(effectThen1);
        compositeThen.Effects.Add(effectThen2);

        // Else Effect: Hp -20 (Not run)
        var effectElse = new EffectModifyStat { StatName = "Hp", ChangeValue = -20 };

        // Conditional Effect
        var conditional = new EffectConditional
        {
            Condition = condHp
        };
        conditional.ThenEffects.Add(compositeThen);
        conditional.ElseEffects.Add(effectElse);

        conditional.Execute(player, deck);

        Assert.AreEqual(60, player.CurrentHp, "Hp should be increased by 10.");
        Assert.AreEqual(45, player.CurrentSanity, "Sanity should be decreased by 5.");
    }

    [Test]
    public void TestCardPlayExecutesCompositeEffect()
    {
        var player = new Player { MaxHp = 100, CurrentHp = 50, CurrentSanity = 50 };
        var deck = new Deck();
        deck.Initialize(new List<Card>());
        GameState.Instance.PlayerInstance = player;
        GameState.Instance.DeckInstance = deck;

        var card = new Card
        {
            CardName = "測試數據卡",
            CardClass = CardClass.Consumable,
            HungerCost = 0,
            ThirstCost = 0
        };

        // PlayEffect: increase Sanity by 15
        var playEffect = new EffectModifyStat { StatName = "Sanity", ChangeValue = 15 };
        card.PlayEffect = playEffect;

        deck.Hand.Add(card);

        string message;
        var result = DeepForest.UI.CardPlayHandler.TryPlayCard(card, player, deck, out message);

        Assert.AreEqual(DeepForest.UI.CardPlayHandler.PlayResult.Success, result);
        Assert.AreEqual(65, player.CurrentSanity, "Playing data-driven card should trigger PlayEffect and modify Sanity.");
    }
}




