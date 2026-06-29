using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Scene;

namespace DeepForest.Rendering;

public class MapRenderer
{
	private int _width;
	private int _height;
	private TextGrid _buffer;

	public MapRenderer(int width = 30, int height = 12)
	{
		_width = width;
		_height = height;
		_buffer = new TextGrid(_width, _height);
	}

	public TextGrid RenderMap(int currentNodeId, HashSet<int> exploredNodes)
	{
		_buffer.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		var manager = MapManager.Instance;
		if (manager == null) return _buffer;

		// 1. 先畫所有節點之間的連線
		foreach (var pair in manager.Nodes)
		{
			var u = pair.Value;
			foreach (var nextId in u.Connections)
			{
				if (manager.Nodes.TryGetValue(nextId, out var v))
				{
					DrawConnection(u.X + 3, u.Y, v.X + 3, v.Y);
				}
			}
		}

		// 2. 疊加繪製節點主體
		foreach (var pair in manager.Nodes)
		{
			var node = pair.Value;
			RenderNode(node.Id, node.Name, node.X, node.Y, currentNodeId, exploredNodes);
		}

		return _buffer;
	}

	private void DrawConnection(int x1, int y1, int x2, int y2)
	{
		int startY = Math.Min(y1, y2);
		int endY = Math.Max(y1, y2);
		
		int startX = y1 < y2 ? x1 : x2;
		int endX = y1 < y2 ? x2 : x1;

		int dy = endY - startY;
		if (dy <= 0) return;

		for (int y = startY + 1; y < endY; y++)
		{
			float t = (float)(y - startY) / dy;
			int x = (int)Math.Round(startX + t * (endX - startX));
			
			char c = '│';
			if (endX > startX) c = '\\';
			else if (endX < startX) c = '/';

			Color color = new Color(0.33f, 0.42f, 0.18f); // 灰綠色
			_buffer.SetCell(x, y, new CharCell(c, color, new Color(0, 0, 0), "map.explored"));
		}
	}

	private void RenderNode(int nodeId, string name, int x, int y, int currentNodeId, HashSet<int> exploredNodes)
	{
		bool isCurrent = nodeId == currentNodeId;
		bool isExplored = exploredNodes.Contains(nodeId);

		Color fg;
		string displayText;

		if (isCurrent)
		{
			fg = new Color(0.0f, 0.75f, 1.0f); // 亮藍色
			displayText = $"[{name}]";
		}
		else if (isExplored)
		{
			fg = new Color(0.22f, 1.0f, 0.08f); // 輻射綠
			displayText = $"[{name}]";
		}
		else
		{
			fg = new Color(0.33f, 0.42f, 0.18f); // 灰綠色
			displayText = "[░░]";
		}

		string tag = isCurrent ? "map.current" : (isExplored ? "map.explored" : "map.fog");

		for (int i = 0; i < displayText.Length; i++)
		{
			_buffer.SetCell(x + i, y, new CharCell(displayText[i], fg, new Color(0, 0, 0), tag));
		}
	}
}
