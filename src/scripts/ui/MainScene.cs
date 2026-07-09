using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Scene;
using DeepForest.Rendering;
using DeepForest.Combat;
using DeepForest.Cards.Effects;

namespace DeepForest.UI;

public partial class MainScene : Control
{
	private RichTextLabel _statsLabel = null!;
	private RichTextLabel _mapLabel = null!;
	private Label _systemBannerLabel = null!;
	private VBoxContainer _actionList = null!;
	private HBoxContainer _handContainer = null!;
	private TextureRect _avatarTexture = null!;
	private Control _sceneContainer = null!;
	private TextureRect _baseTexture = null!;
	private TextureRect _groundTexture = null!;
	private TextureRect _leftTerrainTexture = null!;
	private TextureRect _rightTerrainTexture = null!;
	private TextureRect _decalLeftTexture = null!;
	private TextureRect _decalRightTexture = null!;
	private TextureRect _weatherTexture = null!;
	private RichTextLabel _itemLabel = null!;

	private SceneRenderer _sceneRenderer = null!;
	private MapRenderer _mapRenderer = null!;

	private HandUI _handUI = null!;
	private StatsPanel _statsPanel = null!;
	private ActionPanel _actionPanel = null!;
	private MapPanel _mapPanel = null!;
	private SystemBanner _systemBanner = null!;
	private SceneEffect2D _sceneEffect = null!;

	private Panel _mainMenuPanel = null!;
	private SettingsPanelUI _settingsPanel = null!;

	public override void _Ready()
	{
		_statsLabel = GetNode<RichTextLabel>("StatsPanel/StatsLabel");
		_mapLabel = GetNode<RichTextLabel>("MapPanel/MapLabel");
		_systemBannerLabel = GetNode<Label>("SystemBanner/BannerLabel");
		_actionList = GetNode<VBoxContainer>("CenterViewport/ActionPanel/ActionScroll/ActionList");
		_handContainer = GetNode<HBoxContainer>("HandUI/HandList");
		_avatarTexture = GetNode<TextureRect>("AvatarPanel/AvatarTexture");
		
		_sceneContainer = GetNode<Control>("CenterViewport/SceneContainer");
		_baseTexture = GetNode<TextureRect>("CenterViewport/SceneContainer/BaseTexture");
		_groundTexture = GetNode<TextureRect>("CenterViewport/SceneContainer/GroundTexture");
		_leftTerrainTexture = GetNode<TextureRect>("CenterViewport/SceneContainer/LeftTerrainTexture");
		_rightTerrainTexture = GetNode<TextureRect>("CenterViewport/SceneContainer/RightTerrainTexture");
		_decalLeftTexture = GetNode<TextureRect>("CenterViewport/SceneContainer/DecalLeftTexture");
		_decalRightTexture = GetNode<TextureRect>("CenterViewport/SceneContainer/DecalRightTexture");
		_weatherTexture = GetNode<TextureRect>("CenterViewport/SceneContainer/WeatherTexture");
		_itemLabel = GetNode<RichTextLabel>("ItemsPanel/ItemLabel");

		_sceneRenderer = new SceneRenderer(136, 48);
		_mapRenderer = new MapRenderer(30, 12);

		// Initialize wrapper classes
		_handUI = new HandUI(_handContainer, PlayCard, DiscardCardFromHand);
		_statsPanel = new StatsPanel(_statsLabel, _itemLabel);
		_actionPanel = new ActionPanel(_actionList);
		_mapPanel = new MapPanel(_mapLabel);
		_systemBanner = new SystemBanner(_systemBannerLabel);

		_sceneEffect = new SceneEffect2D();
		AddChild(_sceneEffect);
		_sceneEffect.Initialize(_sceneContainer);

		GameState.Instance.LogAdded += OnLogAdded;
		GameState.Instance.PlayerInstance.StatChanged += OnPlayerStatChanged;
		GameState.Instance.DeckInstance.HandChanged += OnHandChanged;
		GameState.Instance.DeckInstance.DeckChanged += OnDeckChanged;
		GameState.Instance.DayChanged += (day) => UpdateSystemBanner();
		GameState.Instance.DepthChanged += (depth) => UpdateSystemBanner();
		GameState.Instance.EndingManagerInstance.EndingTriggered += OnEndingTriggered;

		// Disable mouse-draggable resizing dynamically
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.ResizeDisabled, true);

		// Apply settings values from persistent save on boot
		SaveManager.ApplySettings();

		// Show Title Screen backdrop and Main Menu Overlay
		SetGameplayUiVisible(false);
		ShowTitleBackdrop();
		CreateMainMenu();
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
		_handUI.UpdateHand(GameState.Instance.DeckInstance);
	}

	private void OnDeckChanged()
	{
		UpdateHUD();
	}

	private void UpdateHUD()
	{
		_statsPanel.UpdateHUD(GameState.Instance.PlayerInstance, GameState.Instance.DeckInstance);
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

		bool hasTorch = GameState.Instance.DeckInstance.EquippedCards.Exists(c => c.CardId == CardId.EquipmentTorch) || 
						 GameState.Instance.DeckInstance.Hand.Exists(c => c.CardId == CardId.EquipmentTorch);

		Texture2D LoadSceneTexture(string path)
		{
			if (string.IsNullOrEmpty(path) || !Godot.FileAccess.FileExists(path))
				return null!;
			return GD.Load<Texture2D>(path);
		}

		// 1. Set base perspective
		_baseTexture.Texture = LoadSceneTexture("res://assets/ascii_art/scenes/perspective_template.png");

		// 2. Set ground
		int nodeId = MapManager.Instance.CurrentNodeId;
		string groundVar = (nodeId % 3 == 1) ? "_2" : "";
		string groundPath = $"res://assets/ascii_art/scenes/ground_{activeScene.BottomGround}{groundVar}.png";
		if (groundVar != "" && !Godot.FileAccess.FileExists(groundPath))
		{
			groundPath = $"res://assets/ascii_art/scenes/ground_{activeScene.BottomGround}.png";
		}
		_groundTexture.Texture = LoadSceneTexture(groundPath);

		// 3. Set left/right terrains
		bool leftBlind = StatusEffect.HasLeftEyeBlindness(GameState.Instance.DeckInstance.Hand);
		bool rightBlind = StatusEffect.HasRightEyeBlindness(GameState.Instance.DeckInstance.Hand);

		string leftVar = (nodeId % 2 == 1) ? "_2" : "";
		string leftPath = $"res://assets/ascii_art/scenes/terrain_{activeScene.LeftTerrain}_left{leftVar}.png";
		if (leftVar != "" && !Godot.FileAccess.FileExists(leftPath))
		{
			leftPath = $"res://assets/ascii_art/scenes/terrain_{activeScene.LeftTerrain}_left.png";
		}
		_leftTerrainTexture.Texture = LoadSceneTexture(leftPath);
		_leftTerrainTexture.Visible = !leftBlind;

		string rightVar = (((nodeId + 1) % 2) == 1) ? "_2" : "";
		string rightPath = $"res://assets/ascii_art/scenes/terrain_{activeScene.RightTerrain}_right{rightVar}.png";
		if (rightVar != "" && !Godot.FileAccess.FileExists(rightPath))
		{
			rightPath = $"res://assets/ascii_art/scenes/terrain_{activeScene.RightTerrain}_right.png";
		}
		_rightTerrainTexture.Texture = LoadSceneTexture(rightPath);
		_rightTerrainTexture.Visible = !rightBlind;

		// 4. Set Decals
		string decalLeftPath = "";
		string decalRightPath = "";
		foreach (var decal in activeScene.Decals)
		{
			if (decal.EndsWith("left") || decal.Contains("left"))
				decalLeftPath = $"res://assets/ascii_art/scenes/decal_{decal}.png";
			else if (decal.EndsWith("right") || decal.Contains("right"))
				decalRightPath = $"res://assets/ascii_art/scenes/decal_{decal}.png";
			else
			{
				if (decalLeftPath == "") decalLeftPath = $"res://assets/ascii_art/scenes/decal_{decal}.png";
				else decalRightPath = $"res://assets/ascii_art/scenes/decal_{decal}.png";
			}
		}
		_decalLeftTexture.Texture = LoadSceneTexture(decalLeftPath);
		_decalRightTexture.Texture = LoadSceneTexture(decalRightPath);

		// 5. Set Weather
		string weatherPath = "";
		if (weather == "濃霧" || weather == "Fog")
			weatherPath = "res://assets/ascii_art/scenes/weather_fog.png";
		else if (weather == "暴雨" || weather == "Rain" || weather == "雷暴")
			weatherPath = "res://assets/ascii_art/scenes/weather_rain.png";
		_weatherTexture.Texture = LoadSceneTexture(weatherPath);

		// 6. Set Indoor brightness/shading modulation
		if (GameState.Instance.IsIndoor && !hasTorch)
		{
			float factor = Math.Max(0.05f, 1.0f - (GameState.Instance.IndoorDepth * 0.25f));
			_sceneContainer.SelfModulate = new Color(factor, factor, factor);
		}
		else
		{
			_sceneContainer.SelfModulate = new Color(1.0f, 1.0f, 1.0f);
		}

		// 7. Load and display avatar
		string avatarName = "default_male";
		string expression = "base";
		if (player.CurrentHp < 30) expression = "pain";
		else if (player.CurrentSanity < 30) expression = "insane";

		string texturePath = $"res://assets/ascii_art/avatars/{avatarName}/{expression}.png";
		if (!Godot.FileAccess.FileExists(texturePath))
		{
			texturePath = $"res://assets/ascii_art/avatars/{avatarName}/base.png";
		}
		
		if (Godot.FileAccess.FileExists(texturePath))
		{
			_avatarTexture.Texture = GD.Load<Texture2D>(texturePath);
		}

		if (player.CurrentHp < 30 || expression == "pain")
		{
			_avatarTexture.SelfModulate = new Color(1.0f, 0.4f, 0.4f);
		}
		else if (player.CurrentSanity < 30 || expression == "insane")
		{
			_avatarTexture.SelfModulate = new Color(0.7f, 0.3f, 0.8f);
		}
		else
		{
			_avatarTexture.SelfModulate = new Color(1.0f, 1.0f, 1.0f);
		}
	}

	private void UpdateMap()
	{
		_mapPanel.UpdateMap(_mapRenderer, MapManager.Instance.CurrentNodeId, MapManager.Instance.ExploredNodeIds);
	}

	private void UpdateSystemBanner(string customLog = "")
	{
		_systemBanner.UpdateSystemBanner(customLog);
	}

	private void UpdateActionsList()
	{
		_actionPanel.UpdateActions(ExecuteAction);
	}

	private async void PlayCard(int handIndex)
	{
		Deck deck = GameState.Instance.DeckInstance;
		Player player = GameState.Instance.PlayerInstance;

		if (handIndex < 0 || handIndex >= deck.Hand.Count) return;

		if (GameState.Instance.CurrentInteractionState == GameState.InteractionState.PlanningDiscard)
		{
			Card selected = deck.Hand[handIndex];
			deck.DiscardCard(selected);
			GameState.Instance.AddLog($"【規劃階段一】你選擇棄置了【{selected.CardName}】。");
			
			GameState.Instance.CurrentInteractionState = GameState.InteractionState.PlanningPutBack;
			GameState.Instance.AddLog("【規劃階段二】現在請選擇 1 張手牌放回牌庫頂。");
			
			UpdateHUD();
			UpdateActionsList();
			UpdateSceneAndAvatar();
			OnHandChanged();
			return;
		}
		else if (GameState.Instance.CurrentInteractionState == GameState.InteractionState.PlanningPutBack)
		{
			Card selected = deck.Hand[handIndex];
			deck.Hand.RemoveAt(handIndex);
			deck.DrawPile.Insert(0, selected);
			deck.EmitSignal(Deck.SignalName.HandChanged);
			deck.EmitSignal(Deck.SignalName.DeckChanged);
			
			GameState.Instance.AddLog($"【規劃階段二】你將【{selected.CardName}】放回了牌庫頂。規劃完成！");
			GameState.Instance.CurrentInteractionState = GameState.InteractionState.Normal;
			
			UpdateHUD();
			UpdateActionsList();
			UpdateSceneAndAvatar();
			OnHandChanged();
			return;
		}

		foreach (var child in _handContainer.GetChildren())
		{
			if (child is Button btn) btn.Disabled = true;
		}

		Card card = deck.Hand[handIndex];
		Button cardButton = (Button)_handContainer.GetChild(handIndex);
		
		var tween = cardButton.CreateTween();
		cardButton.PivotOffset = new Vector2(60, 50);
		tween.Parallel().TweenProperty(cardButton, "modulate:a", 0f, 0.35f);
		tween.Parallel().TweenProperty(cardButton, "position:y", cardButton.Position.Y - 40, 0.35f);
		
		await ToSignal(tween, "finished");

		var result = CardPlayHandler.TryPlayCard(card, player, deck, out string message);
		if (result != CardPlayHandler.PlayResult.InvalidCard)
		{
			GameState.Instance.AddLog(message);
		}

		if (deck.Hand.Count == 0)
		{
			GameState.Instance.AddLog("手牌已用盡，重整思緒重新抽牌。");
			bool reshuffled = deck.DrawCards(player.Draw);
			if (reshuffled)
			{
				TurnManager.Instance.TriggerDayChange();
			}
		}

		UpdateHUD();
		UpdateActionsList();
		UpdateSceneAndAvatar();
		OnHandChanged();
	}

	private async void DiscardCardFromHand(int handIndex)
	{
		Deck deck = GameState.Instance.DeckInstance;
		if (handIndex < 0 || handIndex >= deck.Hand.Count) return;

		foreach (var child in _handContainer.GetChildren())
		{
			if (child is Button btn) btn.Disabled = true;
		}

		Card card = deck.Hand[handIndex];
		Button cardButton = (Button)_handContainer.GetChild(handIndex);

		var tween = cardButton.CreateTween();
		cardButton.PivotOffset = new Vector2(60, 50);
		tween.Parallel().TweenProperty(cardButton, "modulate:a", 0f, 0.35f);
		tween.Parallel().TweenProperty(cardButton, "position:y", cardButton.Position.Y + 40, 0.35f);

		await ToSignal(tween, "finished");

		deck.DiscardCard(card);
		GameState.Instance.AddLog($"棄置手牌: {card.CardName}");

		if (deck.Hand.Count == 0)
		{
			GameState.Instance.AddLog("手牌已用盡，重整思緒重新抽牌。");
			bool reshuffled = deck.DrawCards(GameState.Instance.PlayerInstance.Draw);
			if (reshuffled)
			{
				TurnManager.Instance.TriggerDayChange();
			}
		}

		UpdateHUD();
		UpdateActionsList();
		UpdateSceneAndAvatar();
		OnHandChanged();
	}

	private void ExecuteAction(SceneAction action)
	{
		Deck deck = GameState.Instance.DeckInstance;
		Player player = GameState.Instance.PlayerInstance;

		int currentVal = action.ThresholdType switch
		{
			ThresholdType.Str => TurnManager.Instance.AccumulatedStr,
			ThresholdType.Dex => TurnManager.Instance.AccumulatedDex,
			ThresholdType.Wis => TurnManager.Instance.AccumulatedWis,
			ThresholdType.Any => TurnManager.Instance.AccumulatedStr + TurnManager.Instance.AccumulatedDex + TurnManager.Instance.AccumulatedWis,
			_ => 0
		};

		int req = action.ThresholdValue;
		if (action.ThresholdType == ThresholdType.Dex)
		{
			if (EnvironmentSystem.Instance != null)
			{
				req += EnvironmentSystem.Instance.GetDexThresholdModifier();
			}
			if (CardQueryHelper.HasCardEquipped(deck, CardId.EquipmentFlashlight))
			{
				req = Math.Max(0, req - 1);
			}
		}

		if (currentVal < req)
		{
			var forceCard = CardQueryHelper.FindCardAnywhere(deck, CardId.ActionForce);
			if (forceCard != null)
			{
				int missing = req - currentVal;
				player.CurrentHp -= missing * 2;
				deck.Hand.Remove(forceCard); 
				GameState.Instance.AddLog($"【強行】由於你使用了【強行】卡，你付出了 {missing * 2} 點體力代價補足了 {missing} 點點數缺口，強行通過門檻！");
				UpdateHUD();
			}
		}

		var context = new ActionContext {
			GameState = GameState.Instance,
			Player = GameState.Instance.PlayerInstance!,
			Deck = GameState.Instance.DeckInstance!,
			MapManager = MapManager.Instance!,
			TurnManager = TurnManager.Instance!,
			Environment = EnvironmentSystem.Instance!,
			SourceAction = action,
			CurrentScene = (GameState.Instance.IsIndoor ? MapManager.Instance.CurrentIndoorScene : null) ?? MapManager.Instance.Nodes[MapManager.Instance.CurrentNodeId].SceneData
		};

		var result = ActionResolver.Instance.Resolve(action, context);
		if (result.Success)
		{
			GameState.Instance.AddLog(result.LogMessage);
		}

		if (action.EffectType == ActionEffectType.MoveForward)
		{
			_sceneEffect.StepForward();
		}
		else if (action.EffectType == ActionEffectType.CombatClash || 
				 action.EffectType == ActionEffectType.WitchRitual || 
				 action.EffectType == ActionEffectType.EnterCave)
		{
			_sceneEffect.Shake(10.0f, 0.4f);
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
		endingLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		endingLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		endingLabel.AddThemeFontOverride("font", GetNode<Label>("SystemBanner/BannerLabel").GetThemeFont("font"));
		_actionList.AddChild(endingLabel);

		// Back to Main Menu Button
		Button backToMenuBtn = new Button();
		backToMenuBtn.Text = "▶ 返回主選單";
		backToMenuBtn.AddThemeFontOverride("font", GetNode<Label>("SystemBanner/BannerLabel").GetThemeFont("font"));
		backToMenuBtn.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
		backToMenuBtn.Pressed += () => {
			GetTree().ReloadCurrentScene();
		};
		_actionList.AddChild(backToMenuBtn);
	}

	private void SetGameplayUiVisible(bool visible)
	{
		GetNode<Control>("StatsPanel").Visible = visible;
		GetNode<Control>("MapPanel").Visible = visible;
		GetNode<Control>("ItemsPanel").Visible = visible;
		GetNode<Control>("HandUI").Visible = visible;
		GetNode<Control>("SystemBanner").Visible = visible;
		GetNode<Control>("AvatarPanel").Visible = visible;
		GetNode<Control>("CenterViewport/ActionPanel").Visible = visible;
	}

	private void ShowTitleBackdrop()
	{
		_baseTexture.Texture = GD.Load<Texture2D>("res://assets/ascii_art/scenes/perspective_template.png");
		_groundTexture.Texture = GD.Load<Texture2D>("res://assets/ascii_art/scenes/ground_dirt.png");
		_leftTerrainTexture.Texture = GD.Load<Texture2D>("res://assets/ascii_art/scenes/terrain_woodland_left.png");
		_rightTerrainTexture.Texture = GD.Load<Texture2D>("res://assets/ascii_art/scenes/terrain_woodland_right.png");
		_decalLeftTexture.Texture = GD.Load<Texture2D>("res://assets/ascii_art/scenes/decal_tent_left.png");
		_decalRightTexture.Texture = null;
		_weatherTexture.Texture = null;
		
		_leftTerrainTexture.Visible = true;
		_rightTerrainTexture.Visible = true;
	}

	private void CreateMainMenu()
	{
		_mainMenuPanel = new Panel();
		var centerViewport = GetNode<Control>("CenterViewport");
		centerViewport.AddChild(_mainMenuPanel);
		
		_mainMenuPanel.SetAnchorsPreset(LayoutPreset.CenterBottom);
		_mainMenuPanel.GrowHorizontal = GrowDirection.Both;
		_mainMenuPanel.GrowVertical = GrowDirection.Begin;
		_mainMenuPanel.OffsetLeft = -200;
		_mainMenuPanel.OffsetTop = -220;
		_mainMenuPanel.OffsetRight = 200;
		_mainMenuPanel.OffsetBottom = -20;

		var styleBox = new StyleBoxFlat();
		styleBox.BgColor = new Color(0, 0, 0, 1);
		styleBox.BorderColor = new Color(0.223f, 1.0f, 0.078f, 1.0f);
		styleBox.BorderWidthLeft = 2;
		styleBox.BorderWidthTop = 2;
		styleBox.BorderWidthRight = 2;
		styleBox.BorderWidthBottom = 2;
		_mainMenuPanel.AddThemeStyleboxOverride("panel", styleBox);

		var margin = new MarginContainer();
		margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize, 10);
		_mainMenuPanel.AddChild(margin);

		var vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 8);
		margin.AddChild(vbox);

		var title = new Label();
		title.Text = "【 森林深處 │ DEEPFOREST 】";
		var font = GetNode<Label>("SystemBanner/BannerLabel").GetThemeFont("font");
		title.AddThemeFontOverride("font", font);
		title.AddThemeFontSizeOverride("font_size", 14);
		title.AddThemeColorOverride("font_color", new Color(0.223f, 1.0f, 0.078f, 1.0f));
		title.HorizontalAlignment = HorizontalAlignment.Center;
		vbox.AddChild(title);

		vbox.AddChild(new HSeparator());

		var btnStart = new Button();
		btnStart.Text = "▶ 開始遊戲";
		btnStart.AddThemeFontOverride("font", font);
		btnStart.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
		btnStart.Pressed += StartNewGame;
		vbox.AddChild(btnStart);

		var btnContinue = new Button();
		btnContinue.Text = "▶ 繼續遊戲";
		btnContinue.AddThemeFontOverride("font", font);
		
		bool hasSave = SessionSaveSystem.HasSessionSave();
		if (hasSave)
		{
			btnContinue.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
			btnContinue.Pressed += ContinueSavedGame;
		}
		else
		{
			btnContinue.Modulate = new Color(0.3f, 0.4f, 0.2f, 1.0f);
			btnContinue.Disabled = true;
		}
		vbox.AddChild(btnContinue);

		var btnAchievements = new Button();
		btnAchievements.Text = "▶ 歷史成就 (未開放)";
		btnAchievements.AddThemeFontOverride("font", font);
		btnAchievements.Modulate = new Color(0.3f, 0.4f, 0.2f, 1.0f);
		btnAchievements.Disabled = true;
		vbox.AddChild(btnAchievements);

		var btnSettings = new Button();
		btnSettings.Text = "▶ 系統設定";
		btnSettings.AddThemeFontOverride("font", font);
		btnSettings.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
		btnSettings.Pressed += OpenSettings;
		vbox.AddChild(btnSettings);

		var btnExit = new Button();
		btnExit.Text = "▶ 結束遊戲";
		btnExit.AddThemeFontOverride("font", font);
		btnExit.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
		btnExit.Pressed += () => GetTree().Quit();
		vbox.AddChild(btnExit);
	}

	private void StartNewGame()
	{
		SessionSaveSystem.DeleteSession();
		_mainMenuPanel.Visible = false;
		SetGameplayUiVisible(true);
		MapManager.Instance.GenerateMap();

		string charName = "default_male";
		string selected = SaveManager.CurrentSave.SelectedCharacter;
		if (selected == "湯自強") charName = "john";
		else if (selected == "劉淑莉") charName = "sarah";
		else if (selected == "李有志") charName = "leo";
		else if (selected == "李曉琳") charName = "celin";
		else if (selected == "于晞") charName = "nancy";
		else if (selected == "湯明亮") charName = "tommy";

		var characterRes = GD.Load<CharacterData>($"res://src/resources/characters/character_{charName}.tres");
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

	private void ContinueSavedGame()
	{
		bool success = SessionSaveSystem.LoadSession(GameState.Instance, MapManager.Instance);
		if (!success)
		{
			GameState.Instance.AddLog("讀取存檔失敗！");
			return;
		}

		_mainMenuPanel.Visible = false;
		SetGameplayUiVisible(true);

		UpdateHUD();
		UpdateMap();
		UpdateSystemBanner();
		UpdateActionsList();
		UpdateSceneAndAvatar();

		GameState.Instance.AddLog("存檔載入完成。你回到了森林深處。");
	}

	private void OpenSettings()
	{
		_mainMenuPanel.Visible = false;

		if (_settingsPanel == null)
		{
			_settingsPanel = new SettingsPanelUI();
			var centerViewport = GetNode<Control>("CenterViewport");
			centerViewport.AddChild(_settingsPanel);

			_settingsPanel.CustomMinimumSize = new Vector2(320, 260);
			_settingsPanel.SetAnchorsPreset(LayoutPreset.Center);
			_settingsPanel.GrowHorizontal = GrowDirection.Both;
			_settingsPanel.GrowVertical = GrowDirection.Both;
			_settingsPanel.OffsetLeft = -160;
			_settingsPanel.OffsetTop = -130;
			_settingsPanel.OffsetRight = 160;
			_settingsPanel.OffsetBottom = 130;
			
			var font = GetNode<Label>("SystemBanner/BannerLabel").GetThemeFont("font");
			_settingsPanel.Initialize(font);
			_settingsPanel.SettingsClosed += CloseSettings;
		}
		_settingsPanel.Visible = true;
	}

	private void CloseSettings()
	{
		_settingsPanel.Visible = false;
		_mainMenuPanel.Visible = true;
	}
}
