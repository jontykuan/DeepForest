using System;
using System.Linq;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Scene;

namespace DeepForest.Cards.Effects
{
    [ActionEffect(ActionEffectType.MoveForward)]
    public class MoveForwardEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            var state = context.GameState;
            if (state.IsInCombat)
            {
                state.IsInCombat = false;
                state.CurrentEnemy = null;
                state.CombatPlayedCards.Clear();
                state.AddLog("你成功逃離了戰鬥！");
            }

            state.CurrentDepth += 10;
            
            var mapManager = context.MapManager;
            var current = mapManager.Nodes[mapManager.CurrentNodeId];
            string msg = "";

            if (current.Connections.Count > 0)
            {
                var sortedConns = current.Connections.OrderBy(id => mapManager.Nodes[id].X).ToList();
                int nextId = sortedConns[0];

                if (context.SourceAction.ActionName.Contains("左"))
                {
                    nextId = sortedConns[0];
                }
                else if (context.SourceAction.ActionName.Contains("右"))
                {
                    nextId = sortedConns[sortedConns.Count - 1];
                }
                else 
                {
                    if (sortedConns.Count == 3)
                        nextId = sortedConns[1];
                    else
                        nextId = sortedConns[0];
                }

                mapManager.CurrentNodeId = nextId;
                msg = $"前進到了：{mapManager.Nodes[nextId].Name}。";
            }
            else
            {
                msg = "你安全地走出了森林！";
            }

            return new ActionResult { Success = true, LogMessage = msg };
        }
    }

    [ActionEffect(ActionEffectType.ExploreIndoor)]
    public class ExploreIndoorEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.GameState.IndoorDepth++;
            context.MapManager.CurrentIndoorScene = context.MapManager.GenerateIndoorScene(context.GameState.IndoorDepth);
            return new ActionResult { Success = true, LogMessage = $"你進一步深入，環境變得更加漆黑 (深度 {context.GameState.IndoorDepth})。" };
        }
    }

    [ActionEffect(ActionEffectType.LeaveIndoor)]
    public class LeaveIndoorEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.GameState.IsIndoor = false;
            int steps = context.GameState.IndoorDepth;
            context.GameState.CurrentDepth += steps * 10;
            int nextNodeId = context.MapManager.GetRandomDownstreamNode(context.GameState.EntranceNodeId, steps);
            context.MapManager.CurrentNodeId = nextNodeId;
            context.MapManager.CurrentIndoorScene = null;
            return new ActionResult { Success = true, LogMessage = $"你攀爬走出，重見天日！在室內度過了 {steps} 個場景，前進了 {steps * 10} 米深度。" };
        }
    }

    [ActionEffect(ActionEffectType.ReturnOutdoor)]
    public class ReturnOutdoorEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            context.GameState.IsIndoor = false;
            context.GameState.IndoorDepth = 0;
            context.MapManager.CurrentIndoorScene = null;
            return new ActionResult { Success = true, LogMessage = "你沿著來時的路，退回到了地表的室外環境。" };
        }
    }
}
