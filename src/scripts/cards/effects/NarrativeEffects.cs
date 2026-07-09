using System;
using System.Linq;
using Godot;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Scene;

namespace DeepForest.Cards.Effects
{
    [ActionEffect(ActionEffectType.SearchStoryItem)]
    public class SearchStoryItemEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var player = context.Player;
            var deck = context.Deck;
            string charName = player.CharacterData?.CharacterName ?? "預設角色";

            string itemName = "";
            CardId targetId = CardId.None;
            string logMsg = "";

            if (charName == "湯自強")
            {
                itemName = "本票";
                targetId = CardId.KeyPromissoryNote;
                logMsg = "你在廢墟碎石堆下翻出了一疊本票，上面滿是你的簽名與高額數字...（獲得【本票】）";
            }
            else if (charName == "劉淑莉")
            {
                itemName = "離婚協議書";
                targetId = CardId.KeyDivorceAgreement;
                logMsg = "你打開倒塌木架上的舊公事包，裡面完好地保存著你登山前就擬定好的離婚文件...（獲得【離婚協議書】）";
            }
            else if (charName == "李曉琳")
            {
                itemName = "上鎖的日記";
                targetId = CardId.KeyLockedDiary;
                logMsg = "在空無一人的獵屋抽屜裡，你找回了遺落的上鎖日記本，首頁寫滿了執念，但上面被一把暗鎖扣著...（獲得【上鎖的日記】）";
            }
            else if (charName == "于晞")
            {
                itemName = "錄音帶";
                targetId = CardId.KeyRecordingTape;
                logMsg = "你在落葉覆蓋的樹洞裡摸出了一盒卡式錄音帶，這是你反擊湯自強最致命的武器...（獲得【錄音帶】）";
            }
            else
            {
                logMsg = "你仔細搜尋了周圍，但除了一些雜物外沒有找到特別的東西。";
            }

            if (targetId != CardId.None)
            {
                var card = CardFactory.CreateCard(targetId);
                if (card != null)
                {
                    deck.AddCardToDiscardPile(card);
                    SaveManager.CollectItem(itemName, "在場景探索中找回遺落的個人關鍵道具");
                }
            }

            ActionGenerator.RemoveActionFromCurrentScene("搜尋");
            ActionGenerator.RemoveActionFromCurrentScene("探索右側樹林");
            ActionGenerator.RemoveActionFromCurrentScene("探索左側樹林");

            return new ActionResult { Success = true, LogMessage = logMsg };
        }
    }

    [ActionEffect(ActionEffectType.FindDog)]
    public class FindDogEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var sd = context.CurrentScene;
            ActionGenerator.RemoveActionFromCurrentScene("尋找狗的蹤跡");

            // Add choice actions
            sd.Actions.Add(new SceneAction {
                ActionName = "【抉擇】仔細端詳小狗",
                ThresholdType = ThresholdType.None,
                ThresholdValue = 0,
                EffectType = ActionEffectType.FindDogInspect
            });

            sd.Actions.Add(new SceneAction {
                ActionName = "【抉擇】溫柔抱起小狗",
                ThresholdType = ThresholdType.None,
                ThresholdValue = 0,
                EffectType = ActionEffectType.FindDogEmbrace
            });

            return new ActionResult { 
                Success = true, 
                LogMessage = "【找到小狗】你在前方的灌木叢中看見了瑟瑟發抖的小狗。牠看起來像傑利，但眼神在迷霧中顯得有些空洞與詭異。" 
            };
        }
    }

    [ActionEffect(ActionEffectType.FindDogInspect)]
    public class FindDogInspectEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var deck = context.Deck;
            var card = CardFactory.CreateCard(CardId.KeyJerryQuestion);
            if (card != null)
            {
                deck.AddCardToDiscardPile(card);
            }

            ActionGenerator.RemoveActionFromCurrentScene("【抉擇】");
            return new ActionResult { 
                Success = true, 
                LogMessage = "你走近端詳，發覺這隻『小狗』的皮膚底下有怪異的突起在蠕動，發出微弱而扭曲的低語。你牽起了牠...（獲得【傑利？】）" 
            };
        }
    }

    [ActionEffect(ActionEffectType.FindDogEmbrace)]
    public class FindDogEmbraceEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var deck = context.Deck;
            var card = CardFactory.CreateCard(CardId.KeyJerry);
            if (card != null)
            {
                deck.AddCardToDiscardPile(card);
            }

            ActionGenerator.RemoveActionFromCurrentScene("【抉擇】");
            return new ActionResult { 
                Success = true, 
                LogMessage = "你不去在乎牠是否怪異，上前溫柔地抱起了牠。小狗蹭了蹭你的手，漸漸在你的懷中平靜下來（獲得裝備卡【傑利】）。" 
            };
        }
    }

    [ActionEffect(ActionEffectType.SarahMeetNancy)]
    public class SarahMeetNancyEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var deck = context.Deck;
            var card = CardFactory.CreateCard(CardId.KeyRecordingTape);
            if (card != null)
            {
                deck.AddCardToDiscardPile(card);
                SaveManager.CollectItem("錄音帶", "劉淑莉從于晞手中接過湯自強的犯罪證據");
            }

            ActionGenerator.RemoveActionFromCurrentScene("與");
            return new ActionResult { 
                Success = true, 
                LogMessage = "【NPC互動】于晞神色驚恐地看著你，將一卷錄音帶塞進你手裡，低聲說『這是他犯罪的證據...』隨即逃入霧中（獲得【錄音帶】）。" 
            };
        }
    }

    [ActionEffect(ActionEffectType.LeoMeetTommy)]
    public class LeoMeetTommyEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var deck = context.Deck;
            var player = context.Player;

            // Try to find a rations card
            var rationsCard = CardQueryHelper.FindCardAnywhere(deck, "營養口糧(3/3)") 
                ?? CardQueryHelper.FindCardAnywhere(deck, "營養口糧(2/3)") 
                ?? CardQueryHelper.FindCardAnywhere(deck, "營養口糧(1/3)");

            if (rationsCard != null)
            {
                deck.Hand.Remove(rationsCard);
                deck.DrawPile.Remove(rationsCard);
                deck.DiscardPile.Remove(rationsCard);
                player.CurrentSanity = Math.Min(player.MaxSanity, player.CurrentSanity + 15);
                ActionGenerator.RemoveActionFromCurrentScene("與");
                return new ActionResult {
                    Success = true,
                    LogMessage = "【NPC互動】你分給了嚎啕大哭的湯明亮一些營養口糧，並輕聲安撫他。他感激地向你道謝，隨即朝營地方向跑去（消耗 1 個口糧，理智 +15）。"
                };
            }
            else
            {
                player.Evil += 5;
                ActionGenerator.RemoveActionFromCurrentScene("與");
                return new ActionResult {
                    Success = true,
                    LogMessage = "【NPC互動】你身上沒有食物，冷漠地繞過了他。你的內心變得更加冷酷無情（邪惡 +5）。"
                };
            }
        }
    }

    [ActionEffect(ActionEffectType.JohnMeetSarah)]
    public class JohnMeetSarahEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var player = context.Player;
            player.Brutality += 10;
            player.CurrentHp = Math.Max(0, player.CurrentHp - 10);

            ActionGenerator.RemoveActionFromCurrentScene("與");
            return new ActionResult {
                Success = true,
                LogMessage = "【NPC互動】你與劉淑莉發生了劇烈的爭執，積壓已久的家庭矛盾在此時爆發。你粗暴地推搡她，內心的暴戾與憤怒再度失控（體力 -10，暴戾 +10）。"
            };
        }
    }

    [ActionEffect(ActionEffectType.JohnMeetNancy)]
    public class JohnMeetNancyEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;

        public ActionResult Execute(ActionContext context)
        {
            var player = context.Player;
            player.Evil += 10;

            ActionGenerator.RemoveActionFromCurrentScene("與");
            return new ActionResult {
                Success = true,
                LogMessage = "【NPC互動】你揪住于晞的衣領，威脅她不許洩漏任何口風。她驚叫哭喊著逃入了樹林深處（邪惡 +10）。"
            };
        }
    }
}
