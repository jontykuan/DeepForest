using DeepForest.Core;
using DeepForest.Scene;
using DeepForest.Character;

namespace DeepForest.Cards.Effects
{
    [ActionEffect(ActionEffectType.Camp)]
    public class CampEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context)
        {
            if (context.GameState.IsDescentActive)
            {
                string sceneName = context.CurrentScene?.SceneName ?? "";
                if (context.GameState.IsIndoor && MapManager.Instance?.CurrentIndoorScene != null)
                {
                    sceneName = MapManager.Instance.CurrentIndoorScene.SceneName;
                }

                if (sceneName != "點燃的營火")
                {
                    return false;
                }
            }
            return true;
        }

        public ActionResult Execute(ActionContext context)
        {
            var player = context.Player;
            var currentScene = context.CurrentScene;
            var isIndoor = context.GameState.IsIndoor;

            int extraHp = 0;
            int extraSan = 0;
            string envMsg = "在幽暗森林的落葉堆上草草歇息度過一夜。";

            if (currentScene != null)
            {
                string name = currentScene.SceneName;
                if (name == "野營帳篷")
                {
                    envMsg = "在安全溫暖的野營帳篷中歇息，沒有受到森林的侵擾。";
                }
                else if (name.Contains("獵屋") || (isIndoor && name.Contains("小屋")))
                {
                    extraHp = 5;
                    extraSan = 5;
                    envMsg = "殘破的木屋為你遮風避雨，睡得很安穩（額外體力 +5，理智 +5）。";
                }
                else if (name.Contains("沼澤"))
                {
                    extraHp = -10;
                    extraSan = -5;
                    envMsg = "沼澤環境潮濕陰冷、毒蟲肆虐，在此歇息使你的身體受到侵害（額外體力 -10，理智 -5）！";
                }
                else if (name.Contains("遺跡"))
                {
                    extraHp = -5;
                    extraSan = -10;
                    envMsg = "古老遺跡中瀰漫著詭異而陰森的氣息，噩夢使你的精神備受折磨（額外體力 -5，理智 -10）！";
                }
                else if (name.Contains("洞穴"))
                {
                    extraHp = -15;
                    extraSan = -5;
                    envMsg = "冰冷潮濕的岩壁刺骨，且隨時擔心塌方，令你精疲力竭（額外體力 -15，理智 -5）！";
                }
            }

            // Trigger standard day change
            context.TurnManager.TriggerDayChange();

            // Apply modifiers
            if (extraHp != 0) player.CurrentHp = Math.Max(0, player.CurrentHp + extraHp);
            if (extraSan != 0) player.CurrentSanity = Math.Max(0, player.CurrentSanity + extraSan);

            if (player.CharacterData?.CharacterId == CharacterId.Sarah)
            {
                var deck = context.Deck;
                var cardToRemove = deck.DrawPile.Concat(deck.DiscardPile).Concat(deck.Hand)
                    .FirstOrDefault(c => c.CardType == CardType.ActionStr || c.CardType == CardType.ActionDex || c.CardType == CardType.ActionWis);

                if (cardToRemove != null)
                {
                    deck.DrawPile.Remove(cardToRemove);
                    deck.DiscardPile.Remove(cardToRemove);
                    deck.Hand.Remove(cardToRemove);
                    
                    context.GameState.SarahCampRemovals += 1;
                    envMsg += $"\n【思辨整理】劉淑莉在營地冷靜梳理思緒，從牌組中永久移除了【{cardToRemove.CardName}】（累計移除：{context.GameState.SarahCampRemovals}/5）。";

                    if (context.GameState.SarahCampRemovals >= 5 && !CardQueryHelper.HasCardAnywhere(deck, CardId.KeyDivorceAgreement))
                    {
                        Card divorce = CardFactory.CreateCard(CardId.KeyDivorceAgreement);
                        if (divorce != null)
                        {
                            deck.AddCardToDiscardPile(divorce);
                            envMsg += "\n【心境變化】經歷多次深思熟慮，你終於下定決心，獲得了【離婚協議書】！";
                        }
                    }
                }
            }

            return new ActionResult { Success = true, LogMessage = $"【就地歇息】{envMsg}" };
        }
    }

    [ActionEffect(ActionEffectType.LeoCraft)]
    public class LeoCraftEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context)
        {
            return CardQueryHelper.HasCardAnywhere(context.Deck, CardId.ConsumableRepellent) &&
                   CardQueryHelper.HasCardAnywhere(context.Deck, CardId.EquipmentLighter);
        }

        public ActionResult Execute(ActionContext context)
        {
            CardQueryHelper.RemoveCardAnywhere(context.Deck, CardId.ConsumableRepellent);
            CardQueryHelper.RemoveCardAnywhere(context.Deck, CardId.EquipmentLighter);
            
            Card flamethrower = CardFactory.CreateCard(CardId.EquipmentFlamethrower);
            if (flamethrower != null)
            {
                context.Deck.AddCardToDiscardPile(flamethrower);
            }
            
            return new ActionResult
            {
                Success = true,
                LogMessage = "【手工】你將【防蚊噴霧】與【打火機】合成了【噴火器】！"
            };
        }
    }
}
