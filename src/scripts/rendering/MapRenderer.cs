using Godot;
using System.Collections.Generic;

namespace DeepForest.Rendering;

public class MapRenderer
{
	private int _width;
	private int _height;
	private TextGrid _buffer;

	public MapRenderer(int width = 30, int height = 8)
	{
		_width = width;
		_height = height;
		_buffer = new TextGrid(_width, _height);
	}

	public TextGrid RenderMap(int currentNodeId, HashSet<int> exploredNodes)
	{
		_buffer.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		DrawLine(14, 1, '/');
		DrawLine(16, 1, '\\');

		DrawLine(7, 3, '│');
		DrawLine(11, 3, '\\');
		DrawLine(19, 3, '/');
		DrawLine(23, 3, '│');

		DrawLine(10, 5, '\\');
		DrawLine(20, 5, '/');

		RenderNode(0, "起點", 12, 6, currentNodeId, exploredNodes);
		RenderNode(1, "河畔", 4, 4, currentNodeId, exploredNodes);
		RenderNode(2, "獵屋", 20, 4, currentNodeId, exploredNodes);
		RenderNode(3, "深處", 4, 2, currentNodeId, exploredNodes);
		RenderNode(4, "祭壇", 20, 2, currentNodeId, exploredNodes);
		RenderNode(5, "出口", 12, 0, currentNodeId, exploredNodes);

		return _buffer;
	}

	private void DrawLine(int x, int y, char c)
	{
		Color color = new Color(0.33f, 0.42f, 0.18f); 
		_buffer.SetCell(x, y, new CharCell(c, color, new Color(0, 0, 0), "map.explored"));
	}

	private void RenderNode(int nodeId, string name, int x, int y, int currentNodeId, HashSet<int> exploredNodes)
	{
		bool isCurrent = nodeId == currentNodeId;
		bool isExplored = exploredNodes.Contains(nodeId);

		Color fg;
		string displayText;

		if (isCurrent)
		{
			fg = new Color(0.0f, 0.75f, 1.0f); 
			displayText = $"[{name}]";
		}
		else if (isExplored)
		{
			fg = new Color(0.22f, 1.0f, 0.08f); 
			displayText = $"[{name}]";
		}
		else
		{
			fg = new Color(0.33f, 0.42f, 0.18f); 
			displayText = "[░░]";
		}

		string tag = isCurrent ? "map.current" : (isExplored ? "map.explored" : "map.fog");

		for (int i = 0; i < displayText.Length; i++)
		{
			_buffer.SetCell(x + i, y, new CharCell(displayText[i], fg, new Color(0, 0, 0), tag));
		}
	}
}
