using System;
using System.Linq;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Scene;

namespace DeepForest.Cards.Effects
{
    [ActionEffect(ActionEffectType.EnterNormalCabin)]
    public class EnterNormalCabinEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.GameState.IsIndoor = true;
            context.GameState.IndoorDepth = 1;
            context.GameState.EntranceNodeId = context.MapManager.CurrentNodeId;
            context.MapManager.CurrentIndoorScene = context.MapManager.GenerateIndoorScene(1);
            return new ActionResult { Success = true, LogMessage = "你推開木門，進入了陰暗的木屋內。" };
        }
    }

    [ActionEffect(ActionEffectType.EnterStrangeCabin)]
    public class EnterStrangeCabinEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.Player.CurrentSanity -= 10;
            context.GameState.IsIndoor = true;
            context.GameState.IndoorDepth = 3;
            context.GameState.EntranceNodeId = context.MapManager.CurrentNodeId;
            context.MapManager.CurrentIndoorScene = context.MapManager.GenerateIndoorScene(3);
            return new ActionResult { Success = true, LogMessage = "推開血色木門的剎那，刺鼻的血腥與瘋狂念頭撲面而來（理智 -10）！你深入了小屋深處。" };
        }
    }

    [ActionEffect(ActionEffectType.EnterCave)]
    public class EnterCaveEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.Player.CurrentHp -= 15;
            context.GameState.IsIndoor = true;
            context.GameState.IndoorDepth = 1;
            context.GameState.EntranceNodeId = context.MapManager.CurrentNodeId;
            context.MapManager.CurrentIndoorScene = context.MapManager.GenerateIndoorScene(1);
            return new ActionResult { Success = true, LogMessage = "你小心地爬入狹窄的石洞，尖銳的岩石劃傷了你的皮膚（體力 -15）。你進入了黑暗的洞穴。" };
        }
    }

    [ActionEffect(ActionEffectType.TradeHunter)]
    public class TradeHunterEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => context.Player.CurrentHunger >= 10;
        public ActionResult Execute(ActionContext context)
        {
            context.Player.CurrentHunger -= 10;
            var ration = CardFactory.CreateConsumableCard(CardId.ConsumableRations3, "乾糧", 0, 0, -15, 0); 
            bool success = context.Deck.AddCardToDiscardPile(ration);
            string msg = success 
                ? "你將物資與獵人交易（飢餓值 -10），獲得了【乾糧】。" 
                : "你將物資與獵人交易（飢餓值 -10），但背包過重裝不下【乾糧】，只好把乾糧遺棄了！";
            ActionGenerator.RemoveActionFromCurrentScene("與");
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.WitchRitual)]
    public class WitchRitualEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.Player.CurrentHp -= 20;
            context.Player.CurrentSanity -= 10;
            context.Player.Corruption += 15;
            var nightmare = CardFactory.CreateCurseCard(CardId.CurseOldShadow, "舊日殘影", "在魔女指尖跳動的瘋狂殘影。被打出時可使本回合所有智慧卡點數翻倍，但代價是永久扣除 5 點最大理智。");
            nightmare.CardClass = CardClass.Passive; 

            bool success = context.Deck.AddCardToDiscardPile(nightmare);
            string msg = success 
                ? "魔女的指甲深深刺入你的掌心，滾燙 the禁忌知識在血液中燃燒（體力-20，理智-10，穢祟+15）。你獲得了【舊日殘影】。" 
                : "魔女的指甲深深刺入掌心，滾燙的知識在燃燒（體力-20，理智-10，穢祟+15）...但背包裝不下【舊日殘影】，殘影逸散在空氣中。";
            ActionGenerator.RemoveActionFromCurrentScene("接受");
            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.OpenNormalCabinDoor)]
    public class OpenNormalCabinDoorEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            string side = context.SourceAction.ActionName.Contains("左側") ? "左側" : "右側";
            ActionGenerator.RemoveActionFromCurrentScene($"拉開{side}木門");
            ActionGenerator.RemoveActionFromCurrentScene($"撬開{side}木門");

            var currentNode = context.MapManager.Nodes[context.MapManager.CurrentNodeId];
            var actions = (context.GameState.IsIndoor && context.MapManager.CurrentIndoorScene != null)
                ? context.MapManager.CurrentIndoorScene.Actions
                : currentNode.SceneData.Actions;

            actions.Add(new SceneAction {
                ActionName = $"進入{side}木屋",
                ThresholdType = ThresholdType.None,
                ThresholdValue = 0,
                EffectType = ActionEffectType.EnterNormalCabin
            });

            return new ActionResult { Success = true, LogMessage = $"【門已打開】你拉開了{side}的木門，通往室內的通道已開啟。" };
        }
    }

    [ActionEffect(ActionEffectType.OpenStrangeCabinDoor)]
    public class OpenStrangeCabinDoorEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            string side = context.SourceAction.ActionName.Contains("左側") ? "左側" : "右側";
            ActionGenerator.RemoveActionFromCurrentScene(context.SourceAction.ActionName);

            var currentNode = context.MapManager.Nodes[context.MapManager.CurrentNodeId];
            var actions = (context.GameState.IsIndoor && context.MapManager.CurrentIndoorScene != null)
                ? context.MapManager.CurrentIndoorScene.Actions
                : currentNode.SceneData.Actions;

            actions.Add(new SceneAction {
                ActionName = $"進入{side}血色木屋",
                ThresholdType = ThresholdType.None,
                ThresholdValue = 0,
                EffectType = ActionEffectType.EnterStrangeCabin
            });

            return new ActionResult { Success = true, LogMessage = $"【門已打開】血色門推開了，裡頭傳出令人毛骨悚然的精神低語。" };
        }
    }

    [ActionEffect(ActionEffectType.OpenCaveEntrance)]
    public class OpenCaveEntranceEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            string side = context.SourceAction.ActionName.Contains("左側") ? "左側" : "右側";
            ActionGenerator.RemoveActionFromCurrentScene(context.SourceAction.ActionName);

            var currentNode = context.MapManager.Nodes[context.MapManager.CurrentNodeId];
            var actions = (context.GameState.IsIndoor && context.MapManager.CurrentIndoorScene != null)
                ? context.MapManager.CurrentIndoorScene.Actions
                : currentNode.SceneData.Actions;

            actions.Add(new SceneAction {
                ActionName = $"爬入{side}洞穴",
                ThresholdType = ThresholdType.None,
                ThresholdValue = 0,
                EffectType = ActionEffectType.EnterCave
            });

            return new ActionResult { Success = true, LogMessage = $"【通道開啟】洞穴入口的雜物已被清理，現在可以爬入。" };
        }
    }

    [ActionEffect(ActionEffectType.ClearRuinsPassage)]
    [ActionEffect(ActionEffectType.SearchRuinsEntrance)]
    public class ClearRuinsPassageEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            string side = context.SourceAction.ActionName.Contains("左側") ? "左側" : "右側";
            ActionGenerator.RemoveActionFromCurrentScene($"清理{side}塌方通道");
            ActionGenerator.RemoveActionFromCurrentScene($"搜尋{side}遺跡入口");

            var currentNode = context.MapManager.Nodes[context.MapManager.CurrentNodeId];
            var actions = (context.GameState.IsIndoor && context.MapManager.CurrentIndoorScene != null)
                ? context.MapManager.CurrentIndoorScene.Actions
                : currentNode.SceneData.Actions;

            actions.Add(new SceneAction {
                ActionName = $"進入{side}遺跡深處",
                ThresholdType = ThresholdType.None,
                ThresholdValue = 0,
                EffectType = ActionEffectType.EnterStrangeCabin
            });

            return new ActionResult { Success = true, LogMessage = $"【通道開啟】遺跡塌方的石板被推開，露出了地底遺跡的入口。" };
        }
    }
}
