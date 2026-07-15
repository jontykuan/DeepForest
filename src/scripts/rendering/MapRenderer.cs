using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Scene;
using DeepForest.Core;

namespace DeepForest.Rendering;

public class MapRenderer
{
	private int _width;
	private int _height;
	private TextGrid _buffer;

	public MapRenderer(int width = 30, int height = 16)
	{
		_width = width;
		_height = height;
		_buffer = new TextGrid(_width, _height);
	}

	public TextGrid RenderMap(int currentNodeId, HashSet<int> exploredNodes, List<SceneAction>? activeActions = null)
	{
		_buffer.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		var manager = MapManager.Instance;
		if (manager == null) return _buffer;

		// 取得當前節點
		if (!manager.Nodes.TryGetValue(currentNodeId, out var currentNode))
			return _buffer;

		// 1. 找出候選接續場景 (Top block)
		var sortedConns = currentNode.Connections.OrderBy(id => manager.Nodes[id].X).ToList();
		var targetedNodes = new List<MapNode>();
		if (activeActions != null && activeActions.Count > 0)
		{
			foreach (var act in activeActions)
			{
				bool isTransition = act.TargetNodeId >= 0 || 
				                     act.EffectType == ActionEffectType.MoveForward ||
				                     act.EffectType == ActionEffectType.LeaveIndoor;

				if (isTransition)
				{
					if (act.TargetNodeId >= 0)
					{
						if (manager.Nodes.TryGetValue(act.TargetNodeId, out var n) && !targetedNodes.Contains(n))
							targetedNodes.Add(n);
					}
					else if (act.EffectType == ActionEffectType.MoveForward && sortedConns.Count > 0)
					{
						int nextId = sortedConns[0];
						if (act.ActionName.Contains("左"))
						{
							nextId = sortedConns[0];
						}
						else if (act.ActionName.Contains("右"))
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
						if (manager.Nodes.TryGetValue(nextId, out var n) && !targetedNodes.Contains(n))
							targetedNodes.Add(n);
					}
				}
			}
		}
		else
		{
			// Fallback: show all children
			foreach (var id in sortedConns)
			{
				if (manager.Nodes.TryGetValue(id, out var n))
					targetedNodes.Add(n);
			}
		}

		// 2. 找出前一個場景 (Bottom block)
		MapNode? prevNode = null;
		foreach (var pair in manager.Nodes)
		{
			var n = pair.Value;
			if (exploredNodes.Contains(n.Id) && n.Id != currentNodeId && n.Connections.Contains(currentNodeId))
			{
				prevNode = n;
				break; // Pick the first explored predecessor
			}
		}

		// 3. 繪製三階區塊
		// 中間：當前場景 (Y = 5)
		int curX = 12 - (currentNode.Name.Length); 
		if (curX < 2) curX = 2;
		RenderNodeAt(currentNode, curX, 5, true, true);

		// 下方：前一個場景 (Y = 8)
		if (prevNode != null)
		{
			int prevX = 12 - (prevNode.Name.Length);
			if (prevX < 2) prevX = 2;
			RenderNodeAt(prevNode, prevX, 8, false, true);

			// 畫連線：中 (Y=5) 到 下 (Y=8)
			DrawVerticalLine(15, 6, 7);
		}

		// 上方：候選接續場景 (Y = 2)
		if (targetedNodes.Count == 1)
		{
			var target = targetedNodes[0];
			int tgX = 12 - (target.Name.Length);
			if (tgX < 2) tgX = 2;
			RenderNodeAt(target, tgX, 2, false, exploredNodes.Contains(target.Id));

			// 畫連線
			DrawVerticalLine(15, 3, 4);
		}
		else if (targetedNodes.Count == 2)
		{
			var leftNode = targetedNodes[0];
			var rightNode = targetedNodes[1];

			int leftX = 5 - (leftNode.Name.Length);
			if (leftX < 1) leftX = 1;
			int rightX = 19 - (rightNode.Name.Length);
			if (rightX + 6 >= _width) rightX = _width - 8;

			RenderNodeAt(leftNode, leftX, 2, false, exploredNodes.Contains(leftNode.Id));
			RenderNodeAt(rightNode, rightX, 2, false, exploredNodes.Contains(rightNode.Id));

			// 畫斜線
			_buffer.SetCell(14, 4, new CharCell('\\', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));
			_buffer.SetCell(13, 3, new CharCell('\\', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));

			_buffer.SetCell(16, 4, new CharCell('/', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));
			_buffer.SetCell(17, 3, new CharCell('/', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));
		}
		else if (targetedNodes.Count >= 3)
		{
			var leftNode = targetedNodes[0];
			var centerNode = targetedNodes[1];
			var rightNode = targetedNodes[targetedNodes.Count - 1];

			int leftX = 2;
			int centerX = 12 - (centerNode.Name.Length);
			if (centerX < 9) centerX = 9;
			int rightX = 22;

			RenderNodeAt(leftNode, leftX, 2, false, exploredNodes.Contains(leftNode.Id));
			RenderNodeAt(centerNode, centerX, 2, false, exploredNodes.Contains(centerNode.Id));
			RenderNodeAt(rightNode, rightX, 2, false, exploredNodes.Contains(rightNode.Id));

			// 畫連線
			_buffer.SetCell(11, 4, new CharCell('\\', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));
			_buffer.SetCell(9, 3, new CharCell('\\', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));

			DrawVerticalLine(15, 3, 4);

			_buffer.SetCell(19, 4, new CharCell('/', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));
			_buffer.SetCell(21, 3, new CharCell('/', new Color(0.33f, 0.42f, 0.18f), new Color(0, 0, 0), "map.explored"));
		}

		// 4. 繪製底部通路/分歧路徑
		DrawDynamicPaths(activeActions);

		return _buffer;
	}

	private void RenderNodeAt(MapNode node, int x, int y, bool isCurrent, bool isExplored)
	{
		Color fg;
		string displayText;

		if (isCurrent)
		{
			fg = new Color(0.0f, 0.75f, 1.0f); // 亮藍色
			displayText = $"[{node.Name}]";
		}
		else if (isExplored)
		{
			fg = new Color(0.22f, 1.0f, 0.08f); // 輻射綠
			displayText = $"[{node.Name}]";
		}
		else
		{
			fg = new Color(0.33f, 0.42f, 0.18f); // 灰綠色
			displayText = "[░░]";
		}

		string tag = isCurrent ? "map.current" : (isExplored ? "map.explored" : "map.fog");
		DrawText(displayText, x, y, fg, tag);
	}

	private void DrawVerticalLine(int x, int startY, int endY)
	{
		Color color = new Color(0.33f, 0.42f, 0.18f); // 灰綠色
		for (int y = startY; y <= endY; y++)
		{
			_buffer.SetCell(x, y, new CharCell('│', color, new Color(0, 0, 0), "map.explored"));
		}
	}

	private void DrawDynamicPaths(List<SceneAction>? activeActions)
	{
		Color titleColor = new Color(0.0f, 0.75f, 1.0f); // 亮藍色
		Color textColor = new Color(0.22f, 1.0f, 0.08f); // 輻射綠

		// 判斷玩家是否在室內
		if (GameState.Instance != null && GameState.Instance.IsIndoor)
		{
			int depth = GameState.Instance.IndoorDepth;
			string title = "【室內通路】";
			DrawText(title, 2, 11, titleColor, "map.info");

			// 畫一條漸進深度軸線
			// [入口] ── 1 ── [2] ── 3 ── 4 ──> [深處]
			string axis = "[入口] ── ";
			for (int d = 1; d <= 5; d++)
			{
				if (d == depth)
				{
					axis += $"[{d}]";
				}
				else
				{
					axis += $" {d} ";
				}
				if (d < 5) axis += " ── ";
			}
			axis += " ──> [深處]";

			DrawText(axis, 2, 13, textColor, "map.info");
			return;
		}

		if (activeActions == null || activeActions.Count == 0)
			return;

		// 過濾出與切換場景相關的行動
		var transitionActions = new List<SceneAction>();
		foreach (var act in activeActions)
		{
			bool isTransition = act.TargetNodeId >= 0 || 
			                     act.EffectType == ActionEffectType.MoveForward ||
			                     act.EffectType == ActionEffectType.ExploreIndoor ||
			                     act.EffectType == ActionEffectType.LeaveIndoor ||
			                     act.EffectType == ActionEffectType.ReturnOutdoor ||
			                     act.EffectType == ActionEffectType.EnterNormalCabin ||
			                     act.EffectType == ActionEffectType.EnterStrangeCabin ||
			                     act.EffectType == ActionEffectType.EnterCave;

			if (isTransition)
			{
				transitionActions.Add(act);
			}
		}

		if (transitionActions.Count == 0)
			return;

		string titleText = "【當前可用通路】";
		DrawText(titleText, 2, 11, titleColor, "map.info");

		int startY = 12;
		foreach (var act in transitionActions)
		{
			if (startY >= _height) break;

			string direction = "前方";
			if (act.ActionName.Contains("左"))
			{
				direction = "左側";
			}
			else if (act.ActionName.Contains("右"))
			{
				direction = "右側";
			}
			else if (act.ActionName.Contains("返回") || act.ActionName.Contains("後"))
			{
				direction = "後方";
			}

			string pathText = $"{direction} ──> {act.ActionName}";
			DrawText(pathText, 2, startY, textColor, "map.info");
			startY++;
		}
	}

	private void DrawText(string text, int startX, int y, Color fg, string tag)
	{
		int curX = startX;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			_buffer.SetCell(curX, y, new CharCell(c, fg, new Color(0, 0, 0), tag));
			curX += TextGrid.IsFullWidth(c) ? 2 : 1;
		}
	}
}
