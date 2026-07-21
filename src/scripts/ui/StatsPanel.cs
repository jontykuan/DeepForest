using Godot;
using System;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Core;
using ColorPalette = DeepForest.Core.ColorPalette;

namespace DeepForest.UI
{
    public class StatsPanel
    {
        private readonly RichTextLabel _statsLabel;
        private readonly RichTextLabel _itemLabel;

        public StatsPanel(RichTextLabel statsLabel, RichTextLabel itemLabel)
        {
            _statsLabel = statsLabel;
            _itemLabel = itemLabel;
        }

        private string GetColoredProgressBar(int current, int max, int divisor, Color activeColor, Color inactiveColor, bool shouldPulse = false)
        {
            int filled = Mathf.Clamp(current / divisor, 0, max / divisor);
            int empty = Mathf.Clamp((max - current) / divisor, 0, max / divisor);
            
            string filledStr = new string('█', filled);
            string emptyStr = new string('░', empty);
            
            string activeHtml = activeColor.ToHtml(false);
            string inactiveHtml = inactiveColor.ToHtml(false);
            
            string result = $"[color=#{activeHtml}]{filledStr}[/color][color=#{inactiveHtml}]{emptyStr}[/color]";
            if (shouldPulse)
            {
                result = $"[pulse freq=2.0 color=#{activeHtml}]{result}[/pulse]";
            }
            return result;
        }

        public void UpdateHUD(Player player, Deck deck)
        {
            if (_statsLabel == null) return;

            var cp = ColorPalette.Instance;
            
            // HP: radiant green, pulses if below 30%
            string hpBar = GetColoredProgressBar(player.CurrentHp, player.MaxHp, 10, cp.RadiantGreen, cp.GrayGreen, player.CurrentHp < 30);
            
            // Sanity: radiant green
            string sanBar = GetColoredProgressBar(player.CurrentSanity, player.MaxSanity, 10, cp.RadiantGreen, cp.GrayGreen);
            
            // Hunger: radiant green
            string hungerBar = GetColoredProgressBar(player.CurrentHunger, player.MaxHunger, 5, cp.RadiantGreen, cp.GrayGreen);
            
            // Thirst: radiant green
            string thirstBar = GetColoredProgressBar(player.CurrentThirst, player.MaxThirst, 5, cp.RadiantGreen, cp.GrayGreen);

            string gColor = cp.RadiantGreen.ToHtml(false);
            string wColor = cp.SilentWhite.ToHtml(false);
            string grColor = cp.GrayGreen.ToHtml(false);

            _statsLabel.Text = $"[color=#{gColor}]┌── 狀態面板 ────────┐[/color]\n" +
                               $"  體力: {player.CurrentHp,3}/{player.MaxHp,3}\n  {hpBar}\n" +
                               $"  理智: {player.CurrentSanity,3}/{player.MaxSanity,3}\n  {sanBar}\n" +
                               $"  飢餓: {player.CurrentHunger,3}/{player.MaxHunger,3}\n  {hungerBar}\n" +
                               $"  口渴: {player.CurrentThirst,3}/{player.MaxThirst,3}\n  {thirstBar}\n" +
                               $"[color=#{gColor}]├── 背包負重 ────────┤[/color]\n" +
                               $"  卡牌重: {deck.GetTotalWeight(),3}/{player.DeckCapacity,3}\n" +
                               $"  手牌: {deck.Hand.Count,2} | [color=#{grColor}]庫: {deck.DrawPile.Count,2}[/color] | [color=#{grColor}]棄: {deck.DiscardPile.Count,2}[/color]\n" +
                               $"[color=#{gColor}]└────────────────────┘[/color]";

            if (_itemLabel != null)
            {
                string itemsText = $"[color=#{gColor}]┌── 當前裝備 ────────┐[/color]\n";
                if (deck.EquippedCards.Count == 0)
                {
                    itemsText += $"  [color=#{grColor}](無裝備)[/color]\n";
                }
                else
                {
                    foreach (var eq in deck.EquippedCards)
                    {
                        itemsText += $"  [color=#{wColor}][裝備][/color] {eq.CardName}\n";
                    }
                }
                itemsText += $"[color=#{gColor}]└────────────────────┘[/color]";
                _itemLabel.Text = itemsText;
            }
        }
    }
}
