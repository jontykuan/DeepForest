using System;
using Godot;
using DeepForest.Character;
using DeepForest.Core;
using DeepForest.UI;
using DeepForest.Combat;

namespace DeepForest.Cards.Effects
{
    [CardPlayEffect("抗憂鬱藥物")]
    public class AntidepressantPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            message = "你服用了【抗憂鬱藥物】（理智 +25）。";
            
            // 30% chance of addiction
            if (Random.Shared.NextSingle() < 0.3f)
            {
                var addictionCard = CardFactory.CreateCard(CardId.CurseAddiction);
                if (addictionCard != null)
                {
                    deck.AddCardToDiscardPile(addictionCard);
                    message += " 感覺身體對藥物的渴求加深了...【成癮】已加入棄牌堆！";
                }
            }

            if (card.UsesLeft <= 0)
            {
                card.UsesLeft = card.MaxUses;
            }
            card.UsesLeft--;

            if (card.UsesLeft > 0)
            {
                deck.DiscardCard(card);
                message += $" 剩餘使用次數: {card.UsesLeft}/{card.MaxUses}。";
            }
            else
            {
                deck.Hand.Remove(card);
                var emptyBottle = CardFactory.CreateCard(CardId.KeyEmptyPillBottle);
                if (emptyBottle != null)
                {
                    deck.AddCardToDiscardPile(emptyBottle);
                    message += " 藥物已用盡！一個【空藥罐】放入了你的背包。";
                }
                else
                {
                    message += " 藥物已用盡！";
                }
            }

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("安眠藥")]
    public class SleepingPillPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            message = "你服用了【安眠藥】（理智 +40）。一陣強烈的睡意襲來，你直接就地躺下紮營。";

            // 30% chance of addiction
            if (Random.Shared.NextSingle() < 0.3f)
            {
                var addictionCard = CardFactory.CreateCard(CardId.CurseAddiction);
                if (addictionCard != null)
                {
                    deck.AddCardToDiscardPile(addictionCard);
                    message += " 感覺精神對藥物的依賴加深了...【成癮】已加入棄牌堆！";
                }
            }

            if (card.UsesLeft <= 0)
            {
                card.UsesLeft = card.MaxUses;
            }
            card.UsesLeft--;

            if (card.UsesLeft > 0)
            {
                deck.DiscardCard(card);
                message += $" 剩餘使用次數: {card.UsesLeft}/{card.MaxUses}。";
            }
            else
            {
                deck.Hand.Remove(card);
                var emptyBottle = CardFactory.CreateCard(CardId.KeyEmptyPillBottle);
                if (emptyBottle != null)
                {
                    deck.AddCardToDiscardPile(emptyBottle);
                    message += " 藥物已用盡！一個【空藥罐】放入了你的背包。";
                }
                else
                {
                    message += " 藥物已用盡！";
                }
            }

            // Force day change
            TurnManager.Instance.TriggerDayChange();

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("烈酒")]
    public class AlcoholPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            player.Brutality = Math.Min(100, player.Brutality + 2);
            message = "你喝下了【烈酒】（理智 +10，口渴 -3，暴戾值 +2）。酒精麻痺了你的神經。";

            // 30% chance of addiction
            if (Random.Shared.NextSingle() < 0.3f)
            {
                var addictionCard = CardFactory.CreateCard(CardId.CurseAddiction);
                if (addictionCard != null)
                {
                    deck.AddCardToDiscardPile(addictionCard);
                    message += " 辛辣的液體使你感到依賴...【成癮】已加入棄牌堆！";
                }
            }

            // Consume single-use card
            deck.Hand.Remove(card);

            // Give empty bottle
            var emptyBottle = CardFactory.CreateConsumableCard(CardId.EmptyBottle, "空瓶", 0, 0, 0, 0);
            emptyBottle.Weight = 2;
            emptyBottle.Description = "空瓶：重量2，在牌組中可以在水源處使用裝水。";
            deck.AddCardToDiscardPile(emptyBottle);
            message += " 剩下了一個【空瓶】放回背包。";

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("興奮劑")]
    public class StimulantPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            message = "你注射了【興奮劑】（理智 -15）。狂暴的力量充斥著你的血管，你在該場景出牌效能將大幅提升！";

            // 30% chance of addiction
            if (Random.Shared.NextSingle() < 0.3f)
            {
                var addictionCard = CardFactory.CreateCard(CardId.CurseAddiction);
                if (addictionCard != null)
                {
                    deck.AddCardToDiscardPile(addictionCard);
                    message += " 身體深處產生了強烈渴望...【成癮】已加入棄牌堆！";
                }
            }

            if (card.UsesLeft <= 0)
            {
                card.UsesLeft = card.MaxUses;
            }
            card.UsesLeft--;

            if (card.UsesLeft > 0)
            {
                deck.DiscardCard(card);
                message += $" 剩餘使用次數: {card.UsesLeft}/{card.MaxUses}。";
            }
            else
            {
                deck.Hand.Remove(card);
                var emptyBottle = CardFactory.CreateCard(CardId.KeyEmptyPillBottle);
                if (emptyBottle != null)
                {
                    deck.AddCardToDiscardPile(emptyBottle);
                    message += " 藥物已用盡！一個【空藥罐】放入了你的背包。";
                }
                else
                {
                    message += " 藥物已用盡！";
                }
            }

            // Set StimulantActive flag
            GameState.Instance.StimulantActive = true;

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("防身噴霧")]
    public class DefenseSprayPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            bool inCombat = GameState.Instance.IsInCombat;
            if (inCombat && CombatManager.Instance != null && CombatManager.Instance.CurrentEnemy != null)
            {
                CombatManager.Instance.CurrentEnemyHp--;
                message = $"你使用了【防身噴霧】，噴射化學液體對【{CombatManager.Instance.CurrentEnemy.EnemyName}】造成了 1 點生命值傷害！";
            }
            else
            {
                message = "你揮舞著【防身噴霧】空噴了一下，甚麼事也沒發生。";
            }

            if (card.UsesLeft <= 0)
            {
                card.UsesLeft = card.MaxUses;
            }
            card.UsesLeft--;

            if (card.UsesLeft > 0)
            {
                deck.DiscardCard(card);
                message += $" 剩餘使用次數: {card.UsesLeft}/{card.MaxUses}。";
            }
            else
            {
                deck.Hand.Remove(card);
                message += " 防身噴霧已噴完！";
            }

            // If in combat, verify if combat should end
            if (inCombat && CombatManager.Instance != null)
            {
                // We use Reflection or call private methods to trigger combat end checks if HP is 0
                // Wait, CombatManager.Instance has CheckCombatEnd, but it's private. Let's see if we can trigger check combat end by invoking ResolveClash with empty cards or reflection, or just let CombatManager handle it.
                // Wait, CombatManager.cs line 167: CheckCombatEnd() checks CurrentEnemyHp.
                // Let's call CheckCombatEnd using reflection so we don't duplicate logic.
                var method = typeof(CombatManager).GetMethod("CheckCombatEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(CombatManager.Instance, null);
                }
            }

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("防蚊噴霧")]
    public class RepellentPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            message = "你使用了【防蚊噴霧】。刺鼻的氣味散發開來，防蚊效果已被施加。";

            if (card.UsesLeft <= 0)
            {
                card.UsesLeft = card.MaxUses;
            }
            card.UsesLeft--;

            if (card.UsesLeft > 0)
            {
                deck.DiscardCard(card);
                message += $" 剩餘使用次數: {card.UsesLeft}/{card.MaxUses}。";
            }
            else
            {
                deck.Hand.Remove(card);
                message += " 防蚊噴霧已噴完！";
            }

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("鐵鉗")]
    public class PliersPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            bool hasCollar = CardQueryHelper.HasCardEquipped(deck, CardId.KeyJerryCollar);
            if (!hasCollar)
            {
                message = "目前裝備區沒有任何鎖定或無法解除的裝備，無需使用鐵鉗。";
                return false;
            }
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);

            Card collar = deck.EquippedCards.FirstOrDefault(c => c.CardId == CardId.KeyJerryCollar);
            if (collar != null)
            {
                deck.EquippedCards.Remove(collar);
                message = "你使用【鐵鉗】強行剪斷並破壞了緊扣在脖子上的【傑利的項圈】！";
            }
            else
            {
                message = "你使用了【鐵鉗】，但沒有找到可以剪斷的鎖定裝備。";
            }

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("子彈")]
    public class BulletPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            if (!CardQueryHelper.HasCardEquipped(deck, CardId.EquipmentPistol))
            {
                message = "你必須裝備【手槍】才能使用【子彈】！";
                return false;
            }
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);
            if (GameState.Instance.IsInCombat && GameState.Instance.CurrentEnemy != null)
            {
                GameState.Instance.CurrentEnemyHp = Math.Max(0, GameState.Instance.CurrentEnemyHp - 15);
                message = $"你裝彈並對【{GameState.Instance.CurrentEnemy.EnemyName}】射擊！造成了 15 點巨大傷害！";
            }
            else
            {
                message = "你裝彈並對空鳴槍，刺耳的槍響在黑暗林間迴盪。（無效發射）";
            }
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("生魚")]
    public class RawFishPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);
            player.CurrentHunger = Math.Max(0, player.CurrentHunger - 10);
            player.CurrentHp = Math.Max(0, player.CurrentHp - 5);
            player.CurrentSanity = Math.Max(0, player.CurrentSanity - 5);

            string extra = "";
            if (Random.Shared.NextDouble() < 0.3)
            {
                Card diarrhea = CardFactory.CreateCard(CardId.InjuryGastroenteritis);
                if (diarrhea != null)
                {
                    deck.AddCardToDiscardPile(diarrhea);
                    extra = " 你感到肚子劇痛，牌組中被加入了【腸胃炎】！";
                }
            }

            message = $"你生吃了【生魚】（飢餓度恢復 10，體力扣除 5，理智扣除 5）。{extra}";
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("生水")]
    public class RawWaterPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);
            player.CurrentThirst = Math.Max(0, player.CurrentThirst - 10);

            string extra = "";
            if (Random.Shared.NextDouble() < 0.3)
            {
                Card diarrhea = CardFactory.CreateCard(CardId.InjuryGastroenteritis);
                if (diarrhea != null)
                {
                    deck.AddCardToDiscardPile(diarrhea);
                    extra = " 你感到肚子劇痛，牌組中被加入了【腸胃炎】！";
                }
            }

            message = $"你飲用了【生水】（口渴度恢復 10）。{extra}";
            return CardPlayHandler.PlayResult.Success;
        }
    }
}
