using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Character;
using ColorPalette = DeepForest.Core.ColorPalette;

namespace DeepForest.UI
{
    public class HandUI
    {
        private readonly HBoxContainer _handContainer;
        private readonly Action<int> _onCardSelected;
        private readonly Action<int> _onCardDiscarded;

        private Panel _detailPanel = null!;
        private RichTextLabel _detailLabel = null!;

        public HandUI(HBoxContainer handContainer, Action<int> onCardSelected, Action<int> onCardDiscarded)
        {
            _handContainer = handContainer;
            _onCardSelected = onCardSelected;
            _onCardDiscarded = onCardDiscarded;

            InitializeDetailPanel();
        }

        private void InitializeDetailPanel()
        {
            var cp = ColorPalette.Instance;
            
            _detailPanel = new Panel();
            _detailPanel.Visible = false;
            _detailPanel.CustomMinimumSize = new Vector2(240, 110);
            _detailPanel.Size = new Vector2(240, 110);
            _detailPanel.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            var detailStyle = new StyleBoxFlat
            {
                BgColor = new Color(0, 0, 0, 0.95f),
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                BorderColor = cp.RadiantGreen,
                CornerRadiusTopLeft = 4,
                CornerRadiusTopRight = 4,
                CornerRadiusBottomRight = 4,
                CornerRadiusBottomLeft = 4
            };
            _detailPanel.AddThemeStyleboxOverride("panel", detailStyle);
            
            _detailLabel = new RichTextLabel();
            _detailLabel.BbcodeEnabled = true;
            _detailLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            _detailLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 10);
            _detailPanel.AddChild(_detailLabel);
            
            _handContainer.GetParent().AddChild(_detailPanel);
        }

        private Color GetCardTypeColor(CardType cardType)
        {
            var cp = ColorPalette.Instance;
            return cardType switch
            {
                CardType.ActionStr or CardType.ActionDex or CardType.ActionWis => cp.RadiantGreen,
                CardType.Equipment or CardType.Passive => cp.BrightBlue,
                CardType.Consumable => cp.AmberWarning,
                CardType.Curse or CardType.Injury => cp.BloodRed,
                CardType.KeyItem => cp.SilentWhite,
                _ => cp.RadiantGreen
            };
        }

        public void UpdateHand(Deck deck)
        {
            if (_handContainer == null) return;
            if (_detailPanel != null) _detailPanel.Visible = false;

            foreach (Node child in _handContainer.GetChildren())
            {
                child.QueueFree();
            }

            var cp = ColorPalette.Instance;

            for (int i = 0; i < deck.Hand.Count; i++)
            {
                Card card = deck.Hand[i];
                Button cardButton = new Button();
                cardButton.CustomMinimumSize = new Vector2(120, 100);
                cardButton.Text = ""; // Use RichTextLabel child instead

                // Custom Styleboxes per card type
                Color typeColor = GetCardTypeColor(card.CardType);
                
                var normalStyle = new StyleBoxFlat
                {
                    BgColor = new Color(0, 0, 0, 1),
                    BorderWidthLeft = 1,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,
                    BorderColor = typeColor,
                    CornerRadiusTopLeft = 4,
                    CornerRadiusTopRight = 4,
                    CornerRadiusBottomRight = 4,
                    CornerRadiusBottomLeft = 4
                };
                cardButton.AddThemeStyleboxOverride("normal", normalStyle);

                var hoverStyle = new StyleBoxFlat
                {
                    BgColor = new Color(0.04f, 0.1f, 0.04f, 1),
                    BorderWidthLeft = 1,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,
                    BorderColor = cp.SilentWhite,
                    CornerRadiusTopLeft = 4,
                    CornerRadiusTopRight = 4,
                    CornerRadiusBottomRight = 4,
                    CornerRadiusBottomLeft = 4
                };
                cardButton.AddThemeStyleboxOverride("hover", hoverStyle);

                var pressedStyle = new StyleBoxFlat
                {
                    BgColor = new Color(0.08f, 0.2f, 0.08f, 1),
                    BorderWidthLeft = 1,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,
                    BorderColor = typeColor,
                    CornerRadiusTopLeft = 4,
                    CornerRadiusTopRight = 4,
                    CornerRadiusBottomRight = 4,
                    CornerRadiusBottomLeft = 4
                };
                cardButton.AddThemeStyleboxOverride("pressed", pressedStyle);

                var disabledStyle = new StyleBoxFlat
                {
                    BgColor = new Color(0, 0, 0, 1),
                    BorderWidthLeft = 1,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,
                    BorderColor = cp.GrayGreen,
                    CornerRadiusTopLeft = 4,
                    CornerRadiusTopRight = 4,
                    CornerRadiusBottomRight = 4,
                    CornerRadiusBottomLeft = 4
                };
                cardButton.AddThemeStyleboxOverride("disabled", disabledStyle);

                // Nested RichTextLabel for BBCode layout
                RichTextLabel cardLabel = new RichTextLabel();
                cardLabel.BbcodeEnabled = true;
                cardLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
                cardLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 6);
                cardLabel.ScrollActive = false;

                string typeColorHex = typeColor.ToHtml(false);
                string costRedHex = cp.BloodRed.ToHtml(false);
                string costBlueHex = cp.BrightBlue.ToHtml(false);
                string grayHex = cp.GrayGreen.ToHtml(false);
                string paleGrayHex = cp.PaleGray.ToHtml(false);

                string costText = $"[color=#{costRedHex}]飢:-{card.HungerCost}[/color] [color=#{costBlueHex}]渴:-{card.ThirstCost}[/color]";
                string statsText = $"[color=#{paleGrayHex}]S{card.StrValue} D{card.DexValue} W{card.WisValue}[/color]";
                string weightText = $"[color=#{grayHex}]重:{card.Weight}[/color]";

                cardLabel.Text = $"[center][color=#{typeColorHex}]{card.DisplayName}[/color]\n" +
                                 $"{costText}\n" +
                                 $"{statsText}\n" +
                                 $"{weightText}[/center]";
                cardButton.AddChild(cardLabel);
                
                int index = i;
                cardButton.Pressed += () => _onCardSelected(index);
                
                cardButton.GuiInput += (InputEvent @event) => {
                    if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
                    {
                        if (mouseEvent.ButtonIndex == MouseButton.Right)
                        {
                            _onCardDiscarded(index);
                        }
                    }
                };

                // Hover Effects & Detail Popup
                cardButton.MouseEntered += () => {
                    if (cardButton.Disabled) return;

                    // Hover scale animation
                    var scaleTween = cardButton.CreateTween();
                    scaleTween.TweenProperty(cardButton, "scale", new Vector2(1.08f, 1.08f), 0.15f)
                              .SetTrans(Tween.TransitionType.Quad)
                              .SetEase(Tween.EaseType.Out);

                    // Position and show detail panel
                    _detailLabel.Text = $"[center][color=#{typeColorHex}]{card.DisplayName}[/color][/center]\n" +
                                        $"[color=#{paleGrayHex}]{card.Description}[/color]";
                    
                    Vector2 btnGlobalPos = cardButton.GlobalPosition;
                    Vector2 parentGlobalPos = ((Control)_handContainer.GetParent()).GlobalPosition;
                    Vector2 localPos = btnGlobalPos - parentGlobalPos;

                    _detailPanel.Position = new Vector2(localPos.X + (cardButton.Size.X - _detailPanel.Size.X) / 2, localPos.Y - _detailPanel.Size.Y - 8);
                    _detailPanel.Visible = true;
                };

                cardButton.MouseExited += () => {
                    var scaleTween = cardButton.CreateTween();
                    scaleTween.TweenProperty(cardButton, "scale", Vector2.One, 0.15f)
                              .SetTrans(Tween.TransitionType.Quad)
                              .SetEase(Tween.EaseType.Out);

                    _detailPanel.Visible = false;
                };
                
                if (StatusEffect.HasBrokenFinger(deck.Hand))
                {
                    if (i == 0 || i == deck.Hand.Count - 1)
                    {
                        cardButton.Disabled = true;
                        cardButton.Modulate = cp.GrayGreen; 
                    }
                }

                _handContainer.AddChild(cardButton);

                cardButton.PivotOffset = new Vector2(60, 50);
                cardButton.Scale = Vector2.Zero;
                var tween = cardButton.CreateTween();
                tween.TweenInterval(i * 0.08f); 
                tween.TweenProperty(cardButton, "scale", Vector2.One, 0.45f)
                     .SetTrans(Tween.TransitionType.Back)
                     .SetEase(Tween.EaseType.Out);
            }
        }
    }
}
