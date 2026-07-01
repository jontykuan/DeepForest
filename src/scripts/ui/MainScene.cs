using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Scene;
using DeepForest.Rendering;
using DeepForest.Combat;

namespace DeepForest.UI;

public partial class MainScene : Control
{
	private RichTextLabel _statsLabel = null!;
	private RichTextLabel _mapLabel = null!;
	private Label _systemBannerLabel = null!;
	private VBoxContainer _actionList = null!;
	private HBoxContainer _handContainer = null!;
	private RichTextLabel _avatarLabel = null!;
	private RichTextLabel _sceneLabel = null!;
	private RichTextLabel _itemLabel = null!;

	private SceneRenderer _sceneRenderer = null!;
	private AvatarRenderer _avatarRenderer = null!;
	private MapRenderer _mapRenderer = null!;

	public override void _Ready()
	{
		_statsLabel = GetNode<RichTextLabel>("StatsPanel/StatsLabel");
		_mapLabel = GetNode<RichTextLabel>("MapPanel/MapLabel");
		_systemBannerLabel = GetNode<Label>("SystemBanner/BannerLabel");
		_actionList = GetNode<VBoxContainer>("ActionPanel/ActionList");
		_handContainer = GetNode<HBoxContainer>("HandUI/HandList");
		_avatarLabel = GetNode<RichTextLabel>("AvatarPanel/AvatarLabel");
		_sceneLabel = GetNode<RichTextLabel>("CenterViewport/SceneLabel");
		_itemLabel = GetNode<RichTextLabel>("ItemsPanel/ItemLabel");

		_sceneRenderer = new SceneRenderer(68, 24);
		_avatarRenderer = new AvatarRenderer(20, 16);
		_mapRenderer = new MapRenderer(30, 12);

		GameState.Instance.LogAdded += OnLogAdded;
		GameState.Instance.PlayerInstance.StatChanged += OnPlayerStatChanged;
		GameState.Instance.DeckInstance.HandChanged += OnHandChanged;
		GameState.Instance.DeckInstance.DeckChanged += OnDeckChanged;
		GameState.Instance.DayChanged += (day) => UpdateSystemBanner();
		GameState.Instance.DepthChanged += (depth) => UpdateSystemBanner();
		GameState.Instance.EndingManagerInstance.EndingTriggered += OnEndingTriggered;

		var characterRes = GD.Load<CharacterData>("res://src/resources/characters/character_default_male.tres");
		var player = GameState.Instance.PlayerInstance;
		player.InitializeFromData(characterRes);

		var startingCards = new List<Card>(characterRes.StartingDeck);
		GameState.Instance.DeckInstance.Initialize(startingCards);

		UpdateHUD();
		UpdateMap();
		UpdateSystemBanner();
		UpdateActionsList();
		UpdateSceneAndAvatar();
		
		TurnManager.Instance.StartTurn();
		
		GameState.Instance.AddLog("你醒了過來。四周是一片未知的、死寂的森林。");
	}

	private Card CreateActionCard(string name, CardType type, int weight, int str, int dex, int wis, int hungerCost, int thirstCost)
	{
		var card = new Card();
		card.CardName = name;
		card.CardType = type;
		card.Weight = weight;
		card.StrValue = str;
		card.DexValue = dex;
		card.WisValue = wis;
		card.HungerCost = hungerCost;
		card.ThirstCost = thirstCost;
		card.Description = $"進行 {name}。";
		return card;
	}

	private Card CreateConsumableCard(string name, int hungerRestore, int thirstRestore, int hpRestore, int sanityRestore)
	{
		var card = new Card();
		card.CardName = name;
		card.CardType = CardType.Consumable;
		card.Weight = 1;
		card.HungerCost = -hungerRestore;  
		card.ThirstCost = -thirstRestore;
		card.HpCost = -hpRestore;
		card.SanityCost = -sanityRestore;
		card.Description = $"使用 {name}。回復飢餓 {hungerRestore}，口渴 {thirstRestore}。";
		return card;
	}

	private void OnLogAdded(string message)
	{
		UpdateSystemBanner(message);
	}

	private void OnPlayerStatChanged(string statName, int oldVal, int newVal)
	{
		UpdateHUD();
		UpdateActionsList(); 
		UpdateSceneAndAvatar();
	}

	private void OnHandChanged()
	{
		foreach (Node child in _handContainer.GetChildren())
		{
			child.QueueFree();
		}

		var deck = GameState.Instance.DeckInstance;
		for (int i = 0; i < deck.Hand.Count; i++)
		{
			Card card = deck.Hand[i];
			Button cardButton = new Button();
			cardButton.Text = $"[{card.CardName}]\nCost:H{card.HungerCost}/T{card.ThirstCost}\nSTR:{card.StrValue}/DEX:{card.DexValue}/WIS:{card.WisValue}";
			cardButton.CustomMinimumSize = new Vector2(120, 100);
			
			int index = i;
			cardButton.Pressed += () => PlayCard(index);
			
			if (StatusEffect.HasBrokenFinger(deck.Hand))
			{
				if (i == 0 || i == deck.Hand.Count - 1)
				{
					cardButton.Disabled = true;
					cardButton.Modulate = new Color(0.33f, 0.42f, 0.18f); 
				}
			}

			_handContainer.AddChild(cardButton);
		}
	}

	private void OnDeckChanged()
	{
		UpdateHUD();
	}

	private void UpdateHUD()
	{
		Player player = GameState.Instance.PlayerInstance;
		Deck deck = GameState.Instance.DeckInstance;

		string hpBar = new string('█', player.CurrentHp / 10) + new string('░', (player.MaxHp - player.CurrentHp) / 10);
		string sanBar = new string('█', player.CurrentSanity / 10) + new string('░', (player.MaxSanity - player.CurrentSanity) / 10);
		string hungerBar = new string('█', player.CurrentHunger / 5) + new string('░', (player.MaxHunger - player.CurrentHunger) / 5);
		string thirstBar = new string('█', player.CurrentThirst / 5) + new string('░', (player.MaxThirst - player.CurrentThirst) / 5);

		_statsLabel.Text = $"[ 狀態面板 ]\n\n" +
						   $"體力: {player.CurrentHp}/{player.MaxHp}\n{hpBar}\n" +
						   $"理智: {player.CurrentSanity}/{player.MaxSanity}\n{sanBar}\n" +
						   $"飢餓: {player.CurrentHunger}/{player.MaxHunger}\n{hungerBar}\n" +
						   $"口渴: {player.CurrentThirst}/{player.MaxThirst}\n{thirstBar}\n\n" +
						   $"[ 背包負重 ]\n" +
						   $"卡牌重: {deck.GetTotalWeight()}/{player.DeckCapacity}\n" +
						   $"手牌: {deck.Hand.Count} | 庫: {deck.DrawPile.Count} | 棄: {deck.DiscardPile.Count}";

		string itemsText = "[ 重要物品 & 裝備 ]\n\n";
		foreach (var eq in deck.EquippedCards)
		{
			itemsText += $"[裝備] {eq.CardName}\n";
		}
		foreach (var c in deck.Hand)
		{
			if (c.CardType == CardType.KeyItem)
				itemsText += $"[物品] {c.CardName}\n";
		}
		_itemLabel.Text = itemsText;
	}

	private void UpdateSceneAndAvatar()
	{
		var mapManager = MapManager.Instance;
		var currentNode = mapManager.Nodes[mapManager.CurrentNodeId];
		var player = GameState.Instance.PlayerInstance;
		var env = EnvironmentSystem.Instance;

		string weather = env?.GetWeatherString() ?? "晴朗";

		List<string> subs = new List<string>();
		var activeScene = currentNode.SceneData;

		if (GameState.Instance.IsInCombat && GameState.Instance.CurrentEnemy != null)
		{
			activeScene = new SceneData {
				SceneName = activeScene.SceneName,
				SceneDescription = activeScene.SceneDescription,
				LeftTerrain = activeScene.LeftTerrain,
				RightTerrain = activeScene.RightTerrain,
				BottomGround = activeScene.BottomGround
			};
			foreach (var d in currentNode.SceneData.Decals) activeScene.Decals.Add(d);
			activeScene.Decals.Add(GameState.Instance.CurrentEnemy.DecalName);
		}
		else if (GameState.Instance.IsIndoor && mapManager.CurrentIndoorScene != null)
		{
			activeScene = mapManager.CurrentIndoorScene;
		}
		else if (activeScene.SceneName == "野營帳篷")
		{
			subs.Add("tent");
		}

		bool hasTorch = GameState.Instance.DeckInstance.EquippedCards.Exists(c => c.CardName == "火把") || 
						 GameState.Instance.DeckInstance.Hand.Exists(c => c.CardName == "火把");

		var sceneGrid = _sceneRenderer.RenderScene(activeScene, weather, subs, GameState.Instance.IsIndoor, GameState.Instance.IndoorDepth, hasTorch);

		string avatarName = "default_male";
		string expression = "normal";
		if (player.CurrentHp < 30) expression = "pain";
		else if (player.CurrentSanity < 30) expression = "insane";

		var avatarGrid = _avatarRenderer.RenderAvatar(avatarName, expression, player.CurrentHp, player.CurrentSanity);

		bool leftBlind = StatusEffect.HasLeftEyeBlindness(GameState.Instance.DeckInstance.Hand);
		bool rightBlind = StatusEffect.HasRightEyeBlindness(GameState.Instance.DeckInstance.Hand);

		EffectRenderer.ApplyEffects(sceneGrid, player.CurrentHp, player.CurrentSanity, player.Corruption, leftBlind, rightBlind);
		EffectRenderer.ApplyEffects(avatarGrid, player.CurrentHp, player.CurrentSanity, player.Corruption, leftBlind, rightBlind);

		_sceneLabel.Text = sceneGrid.ToBBCode();
		_avatarLabel.Text = avatarGrid.ToBBCode();
	}

	private void UpdateMap()
	{
		var mapManager = MapManager.Instance;
		var mapGrid = _mapRenderer.RenderMap(mapManager.CurrentNodeId, mapManager.ExploredNodeIds);
		_mapLabel.Text = mapGrid.ToBBCode();
	}

	private void UpdateSystemBanner(string customLog = "")
	{
		var env = EnvironmentSystem.Instance;
		string bannerText = $"第 {GameState.Instance.CurrentDay} 天 │ 森林深度: {GameState.Instance.CurrentDepth}";
		if (env != null)
		{
			float tempF = env.CurrentTempCelsius * 9f / 5f + 32f;
			bannerText += $"|天氣：{env.GetWeatherString()}|溫度：{env.CurrentTempCelsius:F1}°C/{tempF:F1}°F|濕度：{env.CurrentHumidityPercent:F0}%";
		}

		if (!string.IsNullOrEmpty(customLog))
		{
			bannerText += $"\n> {customLog}";
		}
		else if (GameState.Instance.GameLogs.Count > 0)
		{
			bannerText += $"\n> {GameState.Instance.GameLogs[GameState.Instance.GameLogs.Count - 1]}";
		}
		
		_systemBannerLabel.Text = bannerText;
	}

	private void UpdateActionsList()
	{
		foreach (Node child in _actionList.GetChildren())
		{
			child.QueueFree();
		}

		var mapNode = MapManager.Instance.Nodes[MapManager.Instance.CurrentNodeId];
		var sceneData = mapNode.SceneData;

		string sceneName = sceneData.SceneName;
		string sceneDescription = sceneData.SceneDescription;
		List<SceneAction> activeActions = new List<SceneAction>();

		if (GameState.Instance.IsInCombat && GameState.Instance.CurrentEnemy != null)
		{
			var enemy = GameState.Instance.CurrentEnemy;
			string hpStr = enemy.HideHp ? "???/???" : $"{GameState.Instance.CurrentEnemyHp}/{enemy.MaxHp}";
			sceneName = $"【戰鬥】{enemy.EnemyName}";
			sceneDescription = $"一隻【{enemy.EnemyName}】阻擋了你！請從手牌打出屬性卡放入對決區，點擊下方對決進行比拼。";

			if (GameState.Instance.CombatPlayedCards.Count > 0)
			{
				string playedCardsStr = "\n\n【已投入對決區的卡牌】：";
				foreach (var c in GameState.Instance.CombatPlayedCards)
				{
					playedCardsStr += $"[{c.CardName}] ";
				}
				sceneDescription += playedCardsStr;
			}
			else
			{
				sceneDescription += "\n\n【對決區】：目前空無一物。請打出屬性卡進行比拼。";
			}

			activeActions.Add(new SceneAction { 
				ActionName = $"揭露結果 ({enemy.EnemyName} {hpStr})", 
				ThresholdType = ThresholdType.None, 
				ThresholdValue = 0, 
				EffectType = ActionEffectType.CombatClash 
			});

			int fleeDex = enemy.IsAggressive ? 5 : 2;
			activeActions.Add(new SceneAction {
				ActionName = "逃跑並前進",
				ThresholdType = ThresholdType.Dex,
				ThresholdValue = fleeDex,
				EffectType = ActionEffectType.MoveForward
			});
		}
		else
		{
			activeActions.AddRange(sceneData.Actions);
		}

		Label descLabel = new Label();
		descLabel.Text = $"{sceneName}\n{sceneDescription}\n\n[ 可執行行動 ]：";
		_actionList.AddChild(descLabel);

		foreach (var action in activeActions)
		{
			HBoxContainer row = new HBoxContainer();
			Button actionButton = new Button();
			actionButton.Text = GetActionLabelText(action);
			
			bool available = IsActionAvailable(action, out string state);
			
			if (state == "Available")
			{
				actionButton.Modulate = new Color(0.22f, 1.0f, 0.08f); 
			}
			else if (state == "InsufficientPoints")
			{
				actionButton.Modulate = new Color(0.33f, 0.42f, 0.18f); 
				actionButton.Disabled = true;
			}
			else 
			{
				actionButton.Modulate = new Color(1.0f, 0.13f, 0.13f); 
				actionButton.Disabled = true;
			}

			actionButton.Pressed += () => ExecuteAction(action);
			row.AddChild(actionButton);
			_actionList.AddChild(row);
		}
	}

	private string GetActionLabelText(SceneAction action)
	{
		string prefix = action.ThresholdType switch
		{
			ThresholdType.Str => $"[力量 {TurnManager.Instance.AccumulatedStr}/{action.ThresholdValue}]",
			ThresholdType.Dex => $"[靈巧 {TurnManager.Instance.AccumulatedDex}/{action.ThresholdValue}]",
			ThresholdType.Wis => $"[智慧 {TurnManager.Instance.AccumulatedWis}/{action.ThresholdValue}]",
			ThresholdType.Any => $"[行動 {TurnManager.Instance.AccumulatedStr + TurnManager.Instance.AccumulatedDex + TurnManager.Instance.AccumulatedWis}/{action.ThresholdValue}]",
			_ => ""
		};

		if (!string.IsNullOrEmpty(action.RequiredItem))
		{
			prefix += $"[需要: {action.RequiredItem}]";
		}

		if (action.EffectType == ActionEffectType.LootCorpse)
		{
			prefix += "[理智 -1]";
		}

		return $"{prefix} ▶ {action.ActionName}";
	}

	private bool IsActionAvailable(SceneAction action, out string state)
	{
		if (action.EffectType == ActionEffectType.LootCorpse)
		{
			if (GameState.Instance.PlayerInstance.CurrentSanity < 1)
			{
				state = "InsufficientPoints";
				return false;
			}
		}

		if (!string.IsNullOrEmpty(action.RequiredItem))
		{
			bool hasItem = false;
			var deck = GameState.Instance.DeckInstance;
			foreach (var card in deck.EquippedCards)
			{
				if (card.CardName == action.RequiredItem) hasItem = true;
			}
			foreach (var card in deck.Hand)
			{
				if (card.CardName == action.RequiredItem) hasItem = true;
			}
			
			if (!hasItem)
			{
				state = "ConditionsMissing";
				return false;
			}
		}

		int currentVal = action.ThresholdType switch
		{
			ThresholdType.Str => TurnManager.Instance.AccumulatedStr,
			ThresholdType.Dex => TurnManager.Instance.AccumulatedDex,
			ThresholdType.Wis => TurnManager.Instance.AccumulatedWis,
			ThresholdType.Any => TurnManager.Instance.AccumulatedStr + TurnManager.Instance.AccumulatedDex + TurnManager.Instance.AccumulatedWis,
			_ => 0
		};

		int req = action.ThresholdValue;
		if (action.ThresholdType == ThresholdType.Dex && EnvironmentSystem.Instance != null)
		{
			req += EnvironmentSystem.Instance.GetDexThresholdModifier();
		}

		if (currentVal >= req)
		{
			state = "Available";
			return true;
		}
		else
		{
			state = "InsufficientPoints";
			return false;
		}
	}

	private void PlayCard(int handIndex)
	{
		Deck deck = GameState.Instance.DeckInstance;
		Player player = GameState.Instance.PlayerInstance;

		if (handIndex < 0 || handIndex >= deck.Hand.Count) return;

		Card card = deck.Hand[handIndex];

		if (GameState.Instance.IsInCombat)
		{
			if (card.CardType == CardType.ActionStr || card.CardType == CardType.ActionDex || card.CardType == CardType.ActionWis)
			{
				CombatManager.Instance.AddCardToCombatZone(card);
				UpdateHUD();
				UpdateActionsList();
				UpdateSceneAndAvatar();
				return;
			}
		}

		int thirstCost = card.ThirstCost;
		if (card.CardType != CardType.Consumable && thirstCost > 0 && EnvironmentSystem.Instance != null)
		{
			thirstCost += EnvironmentSystem.Instance.GetThirstCostModifier();
		}

		if (player.CurrentHunger < card.HungerCost || player.CurrentThirst < thirstCost ||
			player.CurrentHp < card.HpCost || player.CurrentSanity < card.SanityCost)
		{
			GameState.Instance.AddLog("點數不足，無法打出此卡牌！");
			return;
		}

		player.CurrentHunger -= card.HungerCost;
		player.CurrentThirst -= thirstCost;
		player.CurrentHp -= card.HpCost;
		player.CurrentSanity -= card.SanityCost;

		deck.DiscardCard(card);

		int str = card.StrValue;
		if (str > 0 && StatusEffect.HasBrokenArm(deck.Hand))
		{
			str = Math.Max(1, str / 2); 
		}

		TurnManager.Instance.AccumulatedStr += str;
		TurnManager.Instance.AccumulatedDex += card.DexValue;
		TurnManager.Instance.AccumulatedWis += card.WisValue;

		if (card.CardType == CardType.Equipment)
		{
			deck.EquipCard(card);
			GameState.Instance.AddLog($"裝備了 {card.CardName}。已從牌組移出，加入卸載卡。");
		}
		else if (card.CardType == CardType.Consumable)
		{
			GameState.Instance.AddLog($"使用了 {card.CardName}。回復飢餓/口渴值。");
		}
		else
		{
			GameState.Instance.AddLog($"打出了 {card.CardName}。力量+{str}，靈巧+{card.DexValue}，智慧+{card.WisValue}。");
		}

		UpdateHUD();
		UpdateActionsList();
		UpdateSceneAndAvatar();
	}

	private void ExecuteAction(SceneAction action)
	{
		Player player = GameState.Instance.PlayerInstance;
		player.CurrentHp -= action.HpCostOnComplete;
		GameState.Instance.AddLog($"執行了【{action.ActionName}】，消耗 {action.HpCostOnComplete} 點體力。");

		switch (action.EffectType)
		{
			case ActionEffectType.Camp:
				TurnManager.Instance.TriggerDayChange();
				break;
			case ActionEffectType.Fish:
				var fish = CreateConsumableCard("生魚", 5, -1, 2, -2); 
				GameState.Instance.DeckInstance.DiscardPile.Add(fish);
				GameState.Instance.AddLog("捕獲了生魚，加入棄牌堆。");
				break;
			case ActionEffectType.CollectWater:
				var water = CreateConsumableCard("清水", 0, 8, 0, 0);
				GameState.Instance.DeckInstance.DiscardPile.Add(water);
				GameState.Instance.AddLog("用水瓶裝滿了清水，加入棄牌堆。");
				break;
			case ActionEffectType.PryCellar:
				GameState.Instance.PlayerInstance.Corruption += 5;
				var specialLoot = new Card { CardName = "帶血的日記", CardType = CardType.KeyItem, Weight = 1, Description = "地窖裡發現的帶血日記。" };
				GameState.Instance.DeckInstance.DiscardPile.Add(specialLoot);
				GameState.Instance.AddLog("你撬開了地窖，陰冷的穢祟之氣撲面而來...你獲得了【帶血的日記】，並深入了地窖！");

				// 進入室內狀態
				GameState.Instance.IsIndoor = true;
				GameState.Instance.IndoorDepth = 1;
				GameState.Instance.EntranceNodeId = MapManager.Instance.CurrentNodeId;
				MapManager.Instance.CurrentIndoorScene = MapManager.Instance.GenerateIndoorScene(1);
				break;
			case ActionEffectType.ExploreIndoor:
				GameState.Instance.IndoorDepth++;
				MapManager.Instance.CurrentIndoorScene = MapManager.Instance.GenerateIndoorScene(GameState.Instance.IndoorDepth);
				GameState.Instance.AddLog($"你進一步深入，環境變得更加漆黑 (深度 {GameState.Instance.IndoorDepth})。");
				break;
			case ActionEffectType.ReturnOutdoor:
				GameState.Instance.IsIndoor = false;
				GameState.Instance.IndoorDepth = 0;
				MapManager.Instance.CurrentIndoorScene = null;
				GameState.Instance.AddLog("你沿著來時的路，退回到了地表的室外環境。");
				break;
			case ActionEffectType.LeaveIndoor:
				GameState.Instance.IsIndoor = false;
				int steps = GameState.Instance.IndoorDepth / 2 + 1;
				int nextNodeId = MapManager.Instance.GetRandomDownstreamNode(GameState.Instance.EntranceNodeId, steps);
				MapManager.Instance.CurrentNodeId = nextNodeId;
				MapManager.Instance.CurrentIndoorScene = null;
				GameState.Instance.AddLog($"你攀爬走出，重見天日！空間縮減讓你前進了 {steps} 個關卡。");
				break;
			case ActionEffectType.Search:
				var items = new string[] { "地圖殘片", "生鏽的鑰匙", "帶血的日記" };
				string chosenItem = items[new Random().Next(items.Length)];
				var keyItem = new Card { CardName = chosenItem, CardType = CardType.KeyItem, Weight = 1, Description = $"重要的線索：{chosenItem}。" };
				GameState.Instance.DeckInstance.DiscardPile.Add(keyItem);
				GameState.Instance.AddLog($"找到了【{chosenItem}】，放入背包（棄牌堆）。");
				break;
			case ActionEffectType.MoveForward:
				if (GameState.Instance.IsInCombat)
				{
					GameState.Instance.IsInCombat = false;
					GameState.Instance.CurrentEnemy = null;
					GameState.Instance.CombatPlayedCards.Clear();
					GameState.Instance.AddLog("你成功逃離了戰鬥！");
				}

				GameState.Instance.CurrentDepth += 10;
				
				var mapManager = MapManager.Instance;
				var current = mapManager.Nodes[mapManager.CurrentNodeId];
				if (current.Connections.Count > 0)
				{
					// 若有分歧路徑，隨機選一條（或取第一條）
					int nextId = current.Connections[new Random().Next(current.Connections.Count)];
					mapManager.CurrentNodeId = nextId;
					GameState.Instance.AddLog($"前進到了：{mapManager.Nodes[nextId].Name}。");
				}
				else
				{
					GameState.Instance.AddLog("你安全地走出了森林！");
				}
				break;
			case ActionEffectType.CombatClash:
				CombatManager.Instance.ResolveClash();
				break;
			case ActionEffectType.LootCorpse:
				{
					if (GameState.Instance.PlayerInstance.CurrentSanity < 1)
					{
						GameState.Instance.AddLog("你的理智過低，無法搜刮屍體！");
						break;
					}
					GameState.Instance.PlayerInstance.CurrentSanity -= 1;

					var lastEnemy = CombatManager.Instance.LastDefeatedEnemy;
					if (lastEnemy != null && lastEnemy.LootTable.Count > 0)
					{
						foreach (var loot in lastEnemy.LootTable)
						{
							GameState.Instance.DeckInstance.DiscardPile.Add(loot);
							GameState.Instance.AddLog($"你消耗了 1 點理智，從屍體上搜刮到了：【{loot.CardName}】，放入背包。");
						}
					}
					else
					{
						GameState.Instance.AddLog("屍體上沒有任何有價值的物品。");
					}
					RemoveActionFromCurrentScene("搜屍");
				}
				break;
		}

		TurnManager.Instance.AccumulatedStr = 0;
		TurnManager.Instance.AccumulatedDex = 0;
		TurnManager.Instance.AccumulatedWis = 0;

		UpdateHUD();
		UpdateMap();
		UpdateActionsList();
		UpdateSceneAndAvatar();
		
		if (GameState.Instance.EndingManagerInstance.CheckEndGameConditions())
		{
			return;
		}
		
		TurnManager.Instance.StartTurn();
	}

	private void OnEndingTriggered(int type, string title, string description)
	{
		foreach (Node child in _handContainer.GetChildren())
		{
			if (child is Button btn) btn.Disabled = true;
		}

		foreach (Node child in _actionList.GetChildren())
		{
			child.QueueFree();
		}

		Label endingLabel = new Label();
		endingLabel.Text = $"【 遊戲結束 │ {title} 】\n\n{description}\n\n[ 反覆遊玩以解鎖角色所有的背景故事 ]";
		endingLabel.Modulate = new Color(1.0f, 0.13f, 0.13f);
		endingLabel.AddThemeFontOverride("font", GetNode<Label>("SystemBanner/BannerLabel").GetThemeFont("font"));
		_actionList.AddChild(endingLabel);
	}

	private void RemoveActionFromCurrentScene(string prefix)
	{
		var mapManager = MapManager.Instance;
		var currentNode = mapManager.Nodes[mapManager.CurrentNodeId];
		for (int i = currentNode.SceneData.Actions.Count - 1; i >= 0; i--)
		{
			if (currentNode.SceneData.Actions[i].ActionName.StartsWith(prefix))
			{
				currentNode.SceneData.Actions.RemoveAt(i);
			}
		}
	}
}
