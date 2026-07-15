using System;
using System.Linq;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Scene;
using DeepForest.Character;

namespace DeepForest.Cards.Effects
{
    [ActionEffect(ActionEffectType.Fish)]
    public class FishEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            var fish = CardFactory.CreateConsumableCard(CardId.ConsumableRawFish, "生魚", -2, 2, -5, 1); 
            bool success = context.Deck.AddCardToDiscardPile(fish);
            string msg = success ? "捕獲了生魚，放入背包。" : "【過重】你的背包負重不足以容納【生魚】，只好將其棄置！";
            ActionGenerator.RemoveActionFromCurrentScene("捕魚");
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.CollectWater)]
    public class CollectWaterEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => CardQueryHelper.HasCardAnywhere(context.Deck, CardEffectTag.Container);
        public ActionResult Execute(ActionContext context)
        {
            var deck = context.Deck;
            var emptyBottle = CardQueryHelper.FindCardAnywhere(deck, CardEffectTag.Container);
            if (emptyBottle == null)
            {
                return new ActionResult { Success = false, LogMessage = "你沒有容器，無法裝水！" };
            }

            // Remove empty bottle from deck
            deck.Hand.Remove(emptyBottle);
            deck.DrawPile.Remove(emptyBottle);
            deck.DiscardPile.Remove(emptyBottle);
            deck.EquippedCards.Remove(emptyBottle);

            var rawWater = CardFactory.CreateConsumableCard(CardId.ConsumableRawWater, "生水", 0, 0, -10, -10);
            rawWater.Weight = 5;
            rawWater.Description = "生水：重量5，使用時恢復飢餓 10，口渴 10。生飲有可能導致生病。喝完後剩下一個空瓶。";

            if (deck.AddCardToDiscardPile(rawWater))
            {
                ActionGenerator.RemoveActionFromCurrentScene("裝水");
                return new ActionResult { Success = true, LogMessage = $"你將【{emptyBottle.CardName}】灌滿了河水，獲得了【生水】放入背包。" };
            }
            else
            {
                deck.AddCardToDiscardPile(emptyBottle);
                return new ActionResult { Success = false, LogMessage = $"【過重】你的背包負重不足以容納裝滿水後的【生水】（重量 5），無法將【{emptyBottle.CardName}】裝水！" };
            }
        }
    }

    [ActionEffect(ActionEffectType.Search)]
    public class SearchEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            string chosenItem = "老舊鑰匙";
            var keyItem = CardFactory.CreateKeyItemCard(CardId.KeyOldKey, chosenItem, $"重要的線索：{chosenItem}。");
            bool success = context.Deck.AddCardToDiscardPile(keyItem);
            string msg = success ? $"找到了【{chosenItem}】，放入背包。" : $"【過重】你的背包負重不足以容納【{chosenItem}】，你只能無奈將其丟棄！";
            ActionGenerator.RemoveActionFromCurrentScene(context.SourceAction.ActionName);
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.PryCellar)]
    public class PryCellarEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.Player.Corruption += 5;
            string msg = "你撬開了地窖，陰冷的穢祟之氣撲面而來...你動手深入了地窖！（穢祟+5）";

            context.GameState.IsIndoor = true;
            context.GameState.IndoorDepth = 1;
            context.GameState.EntranceNodeId = context.MapManager.CurrentNodeId;
            context.MapManager.CurrentIndoorScene = context.MapManager.GenerateIndoorScene(1);

            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.LootCorpse)]
    public class LootCorpseEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => context.Player.CurrentSanity >= 1;
        public ActionResult Execute(ActionContext context)
        {
            context.Player.CurrentSanity -= 1;
            var lastEnemy = Combat.CombatManager.Instance.LastDefeatedEnemy;
            string msg = "";
            
            if (lastEnemy != null)
            {
                // Apply numerical adjustments based on the enemy
                if (lastEnemy.EnemyName.Contains("狼") || lastEnemy.EnemyName.Contains("野狼"))
                {
                    context.Player.Brutality = Math.Min(100, context.Player.Brutality + 2);
                    context.Player.CurrentSanity = Math.Max(0, context.Player.CurrentSanity - 5);
                    msg += "剝取野狼的屍體讓你沾滿鮮血，但心智感到一絲血腥的恐懼（暴戾 +2，理智 -5）。\n";
                }
                else if (lastEnemy.EnemyName.Contains("教徒") || lastEnemy.EnemyName.Contains("邪教"))
                {
                    context.Player.Corruption = Math.Min(100, context.Player.Corruption + 3);
                    context.Player.CurrentSanity = Math.Max(0, context.Player.CurrentSanity - 5);
                    msg += "搜刮邪教徒的遺物讓你沾染了詭異的邪念與精神不適（穢祟 +3，理智 -5）。\n";
                }

                if (lastEnemy.LootTable.Count > 0)
                {
                    foreach (var loot in lastEnemy.LootTable)
                    {
                        CardFactory.AssignEffectTags(loot);
                        if (context.Deck.AddCardToDiscardPile(loot))
                        {
                            msg += $"從屍體上搜刮到了遺物：【{loot.CardName}】，放入背包。\n";
                        }
                        else
                        {
                            msg += $"【過重】背包過重，無法容納【{loot.CardName}】，你將其遺留原地！\n";
                        }
                    }
                }
                else
                {
                    msg += "屍體上沒有任何有價值的物品。";
                }
            }
            else
            {
                msg = "屍體上沒有任何有價值的物品。";
            }

            // Remove loot actions from current scene
            var toRemove = context.CurrentScene.Actions.Where(a => a.ActionName.Contains("拾取遺物") || a.ActionName.Contains("搜屍")).Select(a => a.ActionName).ToList();
            foreach (var actName in toRemove)
            {
                ActionGenerator.RemoveActionFromCurrentScene(actName);
            }

            return new ActionResult { Success = true, LogMessage = msg.TrimEnd() };
        }
    }

    [ActionEffect(ActionEffectType.OpenWoodChest)]
    public class OpenWoodChestEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            var woodLoots = new string[] { "清水", "生魚", "木材" };
            string item = woodLoots[Random.Shared.Next(woodLoots.Length)];
            Card card;
            if (item == "清水") card = CardFactory.CreateConsumableCard(CardId.ConsumableWater, "清水", 0, 0, 0, -8);
            else if (item == "生魚") card = CardFactory.CreateConsumableCard(CardId.ConsumableRawFish, "生魚", -2, 2, -5, 1);
            else card = CardFactory.CreateConsumableCard(CardId.KeyWood, "木材", -5, 0, 0, 0); 

            bool success = context.Deck.AddCardToDiscardPile(card);
            string msg = success ? $"你開啟了木箱，獲得了：【{item}】，放入背包。" : $"【過重】背包過重，無法裝下【{item}】，你只好將其留在木箱中！";
            ActionGenerator.RemoveActionFromCurrentScene("開啟");
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.OpenIronChest)]
    public class OpenIronChestEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            var tools = new string[] { "火把", "水瓶" };
            string item = tools[Random.Shared.Next(tools.Length)];
            CardId id = item == "火把" ? CardId.EquipmentTorch : CardId.EquipmentWaterFlask;
            var card = CardFactory.CreateEquipmentCard(id, item, $"實用的冒險工具：{item}。", 2);
            bool success = context.Deck.AddCardToDiscardPile(card);
            string msg = success ? $"你合力撬開了鐵箱，獲得了：【{item}】，放入背包。" : $"【過重】背包過重，無法裝下【{item}】，你將其遺留在鐵箱內！";
            ActionGenerator.RemoveActionFromCurrentScene("撬開");
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.TouchCursedChest)]
    public class TouchCursedChestEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.Player.CurrentSanity -= 10;
            context.Player.Corruption += 10;
            var sword = CardFactory.CreateEquipmentCard(CardId.EquipmentKnife, "柴刀", "沾滿暗紅鏽跡的開山柴刀。可於戰鬥中使力量卡牌點數 +1。", 3);
            sword.StrValue = 1;

            bool success = context.Deck.AddCardToDiscardPile(sword);
            string msg = success 
                ? "當你觸碰那刻印箱的瞬間，刺骨的冰冷低語湧入腦海（理智-10，穢祟+10）...你獲得了【柴刀】！" 
                : "當你觸碰刻印箱的瞬間，刺骨低語湧入腦海（理智-10，穢祟+10）...但背包裝不下【柴刀】，你將其掉在地上！";
            ActionGenerator.RemoveActionFromCurrentScene("觸摸");
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.UsePliersToRemoveCollar)]
    public class UsePliersToRemoveCollarEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => 
            CardQueryHelper.HasCardEquipped(context.Deck, CardId.KeyJerryCollar) &&
            CardQueryHelper.HasCardAnywhere(context.Deck, CardId.KeyPliers);

        public ActionResult Execute(ActionContext context)
        {
            CardQueryHelper.RemoveCardAnywhere(context.Deck, CardId.KeyPliers);

            Card collar = context.Deck.EquippedCards.FirstOrDefault(c => c.CardId == CardId.KeyJerryCollar);
            if (collar != null)
            {
                context.Deck.EquippedCards.Remove(collar);
            }

            string msg = "你消耗了【鐵鉗】，強行剪斷並破壞了緊扣在脖子上的【傑利的項圈】！項圈已被銷毀。";
            ActionGenerator.RemoveActionFromCurrentScene("使用鐵鉗剪斷項圈");
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.SearchCampStart)]
    public class SearchCampStartEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            int initialHandCount = context.Deck.Hand.Count;
            context.Deck.DrawCards(2);
            int drawn = context.Deck.Hand.Count - initialHandCount;
            string msg = $"你仔細搜索了營地周圍，抽了 {drawn} 張牌。";
            ActionGenerator.RemoveActionFromCurrentScene(context.SourceAction.ActionName);
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }
}
