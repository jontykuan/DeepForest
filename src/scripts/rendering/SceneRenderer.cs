using Godot;
using System.Collections.Generic;

namespace DeepForest.Rendering;

public class SceneRenderer
{
	private int _width;
	private int _height;
	private TextGrid _buffer;

	public SceneRenderer(int width = 68, int height = 24)
	{
		_width = width;
		_height = height;
		_buffer = new TextGrid(_width, _height);
	}

	public TextGrid RenderScene(string sceneType, string weather, List<string> activeSubs)
	{
		_buffer.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		string baseDir = $"res://assets/ascii_art/scenes/{sceneType}";
		string txtPath = $"{baseDir}/base.txt";
		string tagsPath = $"{baseDir}/base.tags";

		TextGrid baseGrid = null;
		if (Godot.FileAccess.FileExists(txtPath))
		{
			baseGrid = AsciiTemplate.Load(txtPath, tagsPath, "scene");
		}
		else
		{
			baseGrid = GenerateFallbackScene(sceneType);
		}

		_buffer.Blit(baseGrid, 0, 0);

		foreach (var sub in activeSubs)
		{
			string subTxtPath = $"{baseDir}/sub/{sub}.txt";
			if (Godot.FileAccess.FileExists(subTxtPath))
			{
				TextGrid subGrid = AsciiTemplate.Load(subTxtPath, null, "scene");
				int x = 0;
				int y = 0;
				if (sub == "campfire") { x = _width / 2 - subGrid.Width / 2; y = _height - subGrid.Height - 3; }
				else if (sub == "tent") { x = 4; y = _height - subGrid.Height - 3; }
				else if (sub == "fish_trap") { x = _width - subGrid.Width - 4; y = _height - subGrid.Height - 3; }
				
				_buffer.Blit(subGrid, x, y);
			}
		}

		ApplyWeatherOverlay(sceneType, weather);

		return _buffer;
	}

	private void ApplyWeatherOverlay(string sceneType, string weather)
	{
		string overlayPath = $"res://assets/ascii_art/scenes/{sceneType}/overlay_{weather}.txt";
		if (Godot.FileAccess.FileExists(overlayPath))
		{
			TextGrid overlayGrid = AsciiTemplate.Load(overlayPath, null, "scene");
			_buffer.Blit(overlayGrid, 0, 0);
		}
		else
		{
			if (weather == "濃霧" || weather == "Fog")
			{
				var rand = new System.Random();
				for (int y = 0; y < _height; y++)
				{
					for (int x = 0; x < _width; x++)
					{
						CharCell cell = _buffer.GetCell(x, y);
						if (cell.Tag == "scene.bg" || cell.Tag == "scene.overlay" || cell.Tag == string.Empty)
						{
							if (rand.NextDouble() < 0.15)
							{
								cell.Character = rand.Next(2) == 0 ? '░' : '▒';
								cell.ForegroundColor = new Color(0.33f, 0.42f, 0.18f);
								_buffer.SetCell(x, y, cell);
							}
						}
					}
				}
			}
			else if (weather == "暴雨" || weather == "Rain" || weather == "雷暴")
			{
				var rand = new System.Random();
				for (int y = 0; y < _height; y++)
				{
					for (int x = 0; x < _width; x++)
					{
						CharCell cell = _buffer.GetCell(x, y);
						if (cell.Tag == "scene.bg" || cell.Tag == "scene.overlay" || cell.Tag == string.Empty)
						{
							if (rand.NextDouble() < 0.08)
							{
								cell.Character = '/';
								cell.ForegroundColor = new Color(0.0f, 0.75f, 1.0f);
								_buffer.SetCell(x, y, cell);
							}
						}
					}
				}
			}
		}
	}

	private TextGrid GenerateFallbackScene(string sceneType)
	{
		TextGrid grid = new TextGrid(_width, _height);
		grid.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		for (int x = 0; x < _width; x++)
		{
			grid.SetCell(x, 0, new CharCell('─', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), "scene.bg"));
			grid.SetCell(x, _height - 1, new CharCell('─', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), "scene.ground"));
		}
		for (int y = 0; y < _height; y++)
		{
			grid.SetCell(0, y, new CharCell('│', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), "scene.left"));
			grid.SetCell(_width - 1, y, new CharCell('│', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), "scene.right"));
		}

		string label = $"[ 場景: {sceneType} ]";
		int startX = _width / 2 - label.Length / 2;
		for (int i = 0; i < label.Length; i++)
		{
			grid.SetCell(startX + i, _height / 2, new CharCell(label[i], new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), "scene.center"));
		}

		string[] tree = {
			"   /\\   ",
			"  /  \\  ",
			" /    \\ ",
			"/______\\",
			"  │  │  "
		};
		
		for (int ty = 0; ty < tree.Length; ty++)
		{
			for (int tx = 0; tx < tree[ty].Length; tx++)
			{
				grid.SetCell(2 + tx, _height - 1 - tree.Length + ty, new CharCell(tree[ty][tx], new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), "scene.left"));
			}
		}

		for (int ty = 0; ty < tree.Length; ty++)
		{
			for (int tx = 0; tx < tree[ty].Length; tx++)
			{
				grid.SetCell(_width - 10 + tx, _height - 1 - tree.Length + ty, new CharCell(tree[ty][tx], new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), "scene.right"));
			}
		}

		for (int y = 1; y < _height - 1; y++)
		{
			for (int x = 1; x < _width - 1; x++)
			{
				CharCell cell = grid.GetCell(x, y);
				if (cell.Tag == string.Empty || cell.Character == ' ')
				{
					if (y < _height / 2)
						cell.Tag = "scene.bg";
					else if (x < _width / 3)
						cell.Tag = "scene.left";
					else if (x > _width * 2 / 3)
						cell.Tag = "scene.right";
					else if (y > _height * 4 / 5)
						cell.Tag = "scene.ground";
					else
						cell.Tag = "scene.center";

					grid.SetCell(x, y, cell);
				}
			}
		}

		return grid;
	}
}
