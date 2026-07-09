using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Character;

namespace DeepForest.UI
{
    public class HandUI
    {
        private readonly HBoxContainer _handContainer;
        private readonly Action<int> _onCardSelected;
        private readonly Action<int> _onCardDiscarded;

        public HandUI(HBoxContainer handContainer, Action<int> onCardSelected, Action<int> onCardDiscarded)
        {
            _handContainer = handContainer;
            _onCardSelected = onCardSelected;
            _onCardDiscarded = onCardDiscarded;
        }

        public void UpdateHand(Deck deck)
        {
            if (_handContainer == null) return;

            foreach (Node child in _handContainer.GetChildren())
            {
                child.QueueFree();
            }

            for (int i = 0; i < deck.Hand.Count; i++)
            {
                Card card = deck.Hand[i];
                Button cardButton = new Button();
                cardButton.Text = $"[{card.DisplayName}]\nCost:H{card.HungerCost}/T{card.ThirstCost}\nSTR:{card.StrValue}/DEX:{card.DexValue}/WIS:{card.WisValue}";
                cardButton.CustomMinimumSize = new Vector2(120, 100);
                
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
                
                if (StatusEffect.HasBrokenFinger(deck.Hand))
                {
                    if (i == 0 || i == deck.Hand.Count - 1)
                    {
                        cardButton.Disabled = true;
                        cardButton.Modulate = new Color(0.33f, 0.42f, 0.18f); 
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
