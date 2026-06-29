using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Scene;

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

	public TextGrid RenderScene(SceneData sceneData, string weather, List<string> activeSubs)
	{
		_buffer.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		string baseDir = "res://assets/ascii_art/scenes";
		string templatePath = $"{baseDir}/perspective_template.txt";

		// 試圖從檔案載入，若無則走 C# Fallback 渲染器
		if (Godot.FileAccess.FileExists(templatePath))
		{
			LoadSceneFromFiles(sceneData);
		}
		else
		{
			GenerateFallbackScene(sceneData);
		}

		ApplyWeatherOverlay(weather);

		return _buffer;
	}

	private void LoadSceneFromFiles(SceneData sd)
	{
		string baseDir = "res://assets/ascii_art/scenes";

		// 1. 載入透視基底線條
		TextGrid baseGrid = AsciiTemplate.Load($"{baseDir}/perspective_template.txt", null, "scene");
		_buffer.Blit(baseGrid, 0, 0);

		// 2. 載入地面
		string groundPath = $"{baseDir}/ground_{sd.BottomGround}.txt";
		if (Godot.FileAccess.FileExists(groundPath))
		{
			TextGrid groundGrid = AsciiTemplate.Load(groundPath, null, "scene");
			_buffer.Blit(groundGrid, 0, 0);
		}

		// 3. 載入左側地形
		string leftPath = $"{baseDir}/terrain_{sd.LeftTerrain}_left.txt";
		if (Godot.FileAccess.FileExists(leftPath))
		{
			TextGrid leftGrid = AsciiTemplate.Load(leftPath, null, "scene");
			_buffer.Blit(leftGrid, 0, 0);
		}

		// 4. 載入右側地形
		string rightPath = $"{baseDir}/terrain_{sd.RightTerrain}_right.txt";
		if (Godot.FileAccess.FileExists(rightPath))
		{
			TextGrid rightGrid = AsciiTemplate.Load(rightPath, null, "scene");
			_buffer.Blit(rightGrid, 0, 0);
		}

		// 5. 載入貼圖 (Decals)
		foreach (var decal in sd.Decals)
		{
			string decalPath = $"{baseDir}/decal_{decal}.txt";
			if (Godot.FileAccess.FileExists(decalPath))
			{
				TextGrid decalGrid = AsciiTemplate.Load(decalPath, null, "scene");
				_buffer.Blit(decalGrid, 0, 0);
			}
		}
	}

	private void GenerateFallbackScene(SceneData sd)
	{
		// C# 數學透視象限 Fallback 渲染器 - 2ch AA 風格
		Color fgColor = new Color(0.22f, 1.0f, 0.08f); // 輻射綠
		Color bgColor = new Color(0, 0, 0);

		// 1. 依透視區塊填寫背景與材質
		for (int y = 0; y < _height; y++)
		{
			for (int x = 0; x < _width; x++)
			{
				string quadrant = GetQuadrant(x, y);
				char c = ' ';
				string tag = "scene.bg";

				if (quadrant == "center")
				{
					// 中：遠方黑霧通道
					c = y < 4 ? '▓' : (y < 8 ? '▒' : '░');
					tag = "scene.overlay";
				}
				else if (quadrant == "left")
				{
					// 左：左側地形
					c = GetTerrainChar(sd.LeftTerrain, x, y, isLeft: true);
					tag = "scene.left";
				}
				else if (quadrant == "right")
				{
					// 右：右側地形 (與左側鏡像)
					c = GetTerrainChar(sd.RightTerrain, x, y, isLeft: false);
					tag = "scene.right";
				}
				else if (quadrant == "ground")
				{
					// 下：地面
					c = GetGroundChar(sd.BottomGround, x, y);
					tag = "scene.ground";
				}

				_buffer.SetCell(x, y, new CharCell(c, fgColor, bgColor, tag, c == ' '));
			}
		}

		// 2. 疊加繪製 3D 透視骨架線
		DrawPerspectiveLines();

		// 3. 疊加繪製貼圖 (Decals)
		foreach (var decal in sd.Decals)
		{
			DrawFallbackDecal(decal);
		}
	}

	private string GetQuadrant(int x, int y)
	{
		// 判斷 (x, y) 落在 左、右、下、中 哪一個象限
		// 消失點大約在 y=11, x在 30~38 之間
		bool isCenterPath = false;
		if (y <= 11)
		{
			float leftBorder = 32f - y * (2f / 11f);
			float rightBorder = 36f + y * (2f / 11f);
			if (x >= leftBorder && x <= rightBorder)
				isCenterPath = true;
		}
		else
		{
			if (x >= 30 && x <= 38)
				isCenterPath = true;
		}

		if (isCenterPath) return "center";

		if (x < 30)
		{
			float leftLineY = 20f - 0.3f * x;
			if (y < leftLineY) return "left";
			return "ground";
		}
		else if (x > 38)
		{
			float rightLineY = 11f + 0.31f * (x - 38);
			if (y < rightLineY) return "right";
			return "ground";
		}

		return "ground";
	}

	private char GetTerrainChar(string terrain, int x, int y, bool isLeft)
	{
		// 2ch AA 風格的地形填滿字元
		var rand = new System.Random(y * _width + x);
		switch (terrain)
		{
			case "woodland":
				// 樹林：以斜線和直條勾勒樹幹，點代表樹葉
				if (isLeft)
				{
					if (x == 4 || x == 16 || x == 26) return '｜';
					if ((x == 5 || x == 17) && y % 3 == 0) return '＼';
				}
				else
				{
					if (x == 63 || x == 51 || x == 41) return '｜';
					if ((x == 62 || x == 50) && y % 3 == 0) return '／';
				}
				if (rand.NextDouble() < 0.15) return rand.Next(2) == 0 ? '·' : '*';
				break;

			case "stone_wall":
				// 石壁/磚牆：繪製水平縫隙
				if (y % 4 == 0) return '─';
				if (isLeft && (x + y * 2) % 8 == 0) return '│';
				if (!isLeft && (x - y * 2) % 8 == 0) return '│';
				break;

			case "cabin":
				// 小屋壁面：水平原木線條
				if (y % 2 == 0) return '￣';
				break;

			case "riverside":
				// 河畔：水平波紋
				if (y % 3 == 0 && (x + y) % 6 < 2) return '~';
				if (y % 4 == 1 && (x - y) % 8 < 3) return '≈';
				break;

			case "swamp":
				// 沼澤：散落的蘆葦與水窪
				if (rand.NextDouble() < 0.1) return '▁';
				if (rand.NextDouble() < 0.05) return '"';
				break;

			case "ruins":
				// 遺跡：斷壁殘垣，部分石塊字元
				if (x % 12 == 0 && y > 3) return '║';
				if (rand.NextDouble() < 0.08) return '▧';
				break;
		}
		return ' ';
	}

	private char GetGroundChar(string ground, int x, int y)
	{
		var rand = new System.Random(y * _width + x);
		switch (ground)
		{
			case "grass":
				if (rand.NextDouble() < 0.12) return rand.Next(2) == 0 ? '"' : '·';
				break;
			case "planks":
				// 地板木紋
				if (x % 6 == 0) return '│';
				break;
			case "dirt":
			default:
				if (rand.NextDouble() < 0.08) return '.';
				break;
		}
		return ' ';
	}

	private void DrawPerspectiveLines()
	{
		Color color = new Color(0.33f, 0.42f, 0.18f); // 灰綠暗線
		CharCell cell = new CharCell('\0', color, new Color(0, 0, 0), "scene.overlay");

		// 左斜透視線 (0, 20) 到 (30, 11)
		for (int x = 0; x <= 30; x++)
		{
			int y = (int)Math.Round(20f - 0.3f * x);
			cell.Character = '＼';
			_buffer.SetCell(x, y, cell);
		}

		// 右斜透視線 (67, 20) 到 (38, 11)
		for (int x = 38; x < _width; x++)
		{
			int y = (int)Math.Round(11f + 0.31f * (x - 38));
			cell.Character = '／';
			_buffer.SetCell(x, y, cell);
		}

		// 地平線 (30, 11) 到 (38, 11)
		for (int x = 30; x <= 38; x++)
		{
			cell.Character = '─';
			_buffer.SetCell(x, 11, cell);
		}

		// 左通道壁線
		for (int y = 0; y <= 11; y++)
		{
			int x = (int)Math.Round(32f - y * (2f / 11f));
			cell.Character = '＼';
			_buffer.SetCell(x, y, cell);
		}

		// 右通道壁線
		for (int y = 0; y <= 11; y++)
		{
			int x = (int)Math.Round(36f + y * (2f / 11f));
			cell.Character = '／';
			_buffer.SetCell(x, y, cell);
		}
	}

	private void DrawFallbackDecal(string decal)
	{
		// 在 C# Fallback 中手繪貼圖，保證 2ch AA 風格
		Color fg = new Color(0.22f, 1.0f, 0.08f);
		Color bg = new Color(0, 0, 0);

		if (decal.StartsWith("window"))
		{
			int startX = decal.EndsWith("left") ? 10 : 54;
			string[] art = {
				"┌──┐",
				"│田│",
				"└──┘"
			};
			DrawArtAt(art, startX, 6, "scene.center");
		}
		else if (decal.StartsWith("door"))
		{
			int startX = decal.EndsWith("left") ? 18 : 46;
			string[] art = {
				"┌──┐",
				"│  │",
				"│  │",
				"└──┘"
			};
			DrawArtAt(art, startX, 10, "scene.center");
		}
		else if (decal.StartsWith("sofa"))
		{
			int startX = decal.EndsWith("left") ? 8 : 48;
			string[] art = {
				" _n_ ",
				"[___]"
			};
			DrawArtAt(art, startX, 17, "scene.center");
		}
		else if (decal.StartsWith("npc"))
		{
			int startX = decal.EndsWith("left") ? 14 : 50;
			string[] art = {
				"(o_o)",
				"/│\\ ",
				"/ \\ "
			};
			DrawArtAt(art, startX, 12, "scene.center");
		}
		else if (decal.StartsWith("chest"))
		{
			int startX = decal.EndsWith("left") ? 12 : 52;
			string[] art = {
				"[＝]"
			};
			DrawArtAt(art, startX, 18, "scene.center");
		}
	}

	private void DrawArtAt(string[] art, int startX, int startY, string tag)
	{
		Color fg = new Color(0.22f, 1.0f, 0.08f);
		Color bg = new Color(0, 0, 0);

		for (int y = 0; y < art.Length; y++)
		{
			int targetY = startY + y;
			if (targetY < 0 || targetY >= _height) continue;

			int gridX = startX;
			for (int charIdx = 0; charIdx < art[y].Length; charIdx++)
			{
				char c = art[y][charIdx];
				if (gridX >= _width) break;

				if (c != ' ')
				{
					_buffer.SetCell(gridX, targetY, new CharCell(c, fg, bg, tag, false));
				}
				gridX += TextGrid.IsFullWidth(c) ? 2 : 1;
			}
		}
	}

	private void ApplyWeatherOverlay(string weather)
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
							cell.IsTransparent = false;
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
							cell.IsTransparent = false;
							_buffer.SetCell(x, y, cell);
						}
					}
				}
			}
		}
	}
}
