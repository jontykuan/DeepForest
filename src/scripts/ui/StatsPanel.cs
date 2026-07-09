using Godot;
using System;
using DeepForest.Character;
using DeepForest.Cards;

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

        public void UpdateHUD(Player player, Deck deck)
        {
            if (_statsLabel == null) return;

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

            if (_itemLabel != null)
            {
                string itemsText = "[ 當前裝備 ]\n\n";
                foreach (var eq in deck.EquippedCards)
                {
                    itemsText += $"[裝備] {eq.CardName}\n";
                }
                _itemLabel.Text = itemsText;
            }
        }
    }
}
