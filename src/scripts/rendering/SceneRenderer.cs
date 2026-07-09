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

	public SceneRenderer(int width = 136, int height = 48)
	{
		_width = width;
		_height = height;
		_buffer = new TextGrid(_width, _height);
	}

	public TextGrid RenderScene(SceneData sceneData, string weather, List<string> activeSubs, bool isIndoor = false, int indoorDepth = 0, bool hasTorch = false)
	{
		_buffer.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		string baseDir = "res://assets/ascii_art/scenes";
		string templatePath = $"{baseDir}/perspective_template.txt";

		if (Godot.FileAccess.FileExists(templatePath))
		{
			LoadSceneFromFiles(sceneData);
		}
		else
		{
			GenerateFallbackScene(sceneData);
		}

		ApplyWeatherOverlay(weather);

		if (isIndoor && !hasTorch)
		{
			float factor = Math.Max(0.05f, 1.0f - (indoorDepth * 0.25f));
			for (int y = 0; y < _height; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					CharCell cell = _buffer.GetCell(x, y);
					cell.ForegroundColor = new Color(cell.ForegroundColor.R * factor, cell.ForegroundColor.G * factor, cell.ForegroundColor.B * factor);
					_buffer.SetCell(x, y, cell);
				}
			}
		}

		return _buffer;
	}

	private void LoadSceneFromFiles(SceneData sd)
	{
		string baseDir = "res://assets/ascii_art/scenes";

		// 1. Load perspective base grid
		TextGrid baseGrid = AsciiTemplate.Load($"{baseDir}/perspective_template.txt", null, "scene");
		_buffer.Blit(baseGrid, 0, 0, false);

		// 2. Load ground
		string groundPath = $"{baseDir}/ground_{sd.BottomGround}.txt";
		if (Godot.FileAccess.FileExists(groundPath))
		{
			TextGrid groundGrid = AsciiTemplate.Load(groundPath, null, "scene");
			_buffer.Blit(groundGrid, 0, 0, false);
		}

		// 3. Load left terrain
		string leftPath = $"{baseDir}/terrain_{sd.LeftTerrain}_left.txt";
		if (Godot.FileAccess.FileExists(leftPath))
		{
			TextGrid leftGrid = AsciiTemplate.Load(leftPath, null, "scene");
			_buffer.Blit(leftGrid, 0, 0, false);
		}

		// 4. Load right terrain
		string rightPath = $"{baseDir}/terrain_{sd.RightTerrain}_right.txt";
		if (Godot.FileAccess.FileExists(rightPath))
		{
			TextGrid rightGrid = AsciiTemplate.Load(rightPath, null, "scene");
			_buffer.Blit(rightGrid, 0, 0, false);
		}

		// 5. Load Decals
		foreach (var decal in sd.Decals)
		{
			string decalPath = $"{baseDir}/decal_{decal}.txt";
			if (Godot.FileAccess.FileExists(decalPath))
			{
				TextGrid decalGrid = AsciiTemplate.Load(decalPath, null, "scene");
				_buffer.Blit(decalGrid, 0, 0, true);
			}
		}
	}

	private void GenerateFallbackScene(SceneData sd)
	{
		Color fgColor = new Color(0.22f, 1.0f, 0.08f); // Radiant Green
		Color bgColor = new Color(0, 0, 0);

		for (int y = 0; y < _height; y++)
		{
			for (int x = 0; x < _width; x++)
			{
				string quadrant = GetQuadrant(x, y);
				char c = ' ';
				string tag = "scene.bg";

				if (quadrant == "center")
				{
					// Middle horizon path shadows
					c = y < (_height * 0.16f) ? '%' : (y < (_height * 0.33f) ? '*' : '+');
					tag = "scene.overlay";
				}
				else if (quadrant == "left")
				{
					c = GetTerrainChar(sd.LeftTerrain, x, y, isLeft: true);
					tag = "scene.left";
				}
				else if (quadrant == "right")
				{
					c = GetTerrainChar(sd.RightTerrain, x, y, isLeft: false);
					tag = "scene.right";
				}
				else if (quadrant == "ground")
				{
					float scaleX = _width / 68f;
					if (x < (30 * scaleX))
					{
						string leftGround = GetGroundThemeForTerrain(sd.LeftTerrain);
						c = GetGroundChar(leftGround, x, y);
						tag = "scene.left_ground";
					}
					else if (x > (38 * scaleX))
					{
						string rightGround = GetGroundThemeForTerrain(sd.RightTerrain);
						c = GetGroundChar(rightGround, x, y);
						tag = "scene.right_ground";
					}
					else
					{
						c = GetGroundChar(sd.BottomGround, x, y);
						tag = "scene.ground";
					}
				}

				_buffer.SetCell(x, y, new CharCell(c, fgColor, bgColor, tag, c == ' '));
			}
		}

		DrawPerspectiveLines();

		foreach (var decal in sd.Decals)
		{
			DrawFallbackDecal(decal);
		}
	}

	private string GetQuadrant(int x, int y)
	{
		float scaleX = _width / 68f;
		float scaleY = _height / 24f;
		int logicX = (int)Math.Round(x / scaleX);
		int logicY = (int)Math.Round(y / scaleY);

		bool isCenterPath = false;
		if (logicY <= 11)
		{
			float leftBorder = 32f - logicY * (2f / 11f);
			float rightBorder = 36f + logicY * (2f / 11f);
			if (logicX >= leftBorder && logicX <= rightBorder)
				isCenterPath = true;
		}
		else
		{
			if (logicX >= 30 && logicX <= 38)
				isCenterPath = true;
		}

		if (isCenterPath) return "center";

		if (logicX < 30)
		{
			float leftLineY = 20f - 0.3f * logicX;
			if (logicY < leftLineY) return "left";
			return "ground";
		}
		else if (logicX > 38)
		{
			float rightLineY = 11f + 0.31f * (logicX - 38);
			if (logicY < rightLineY) return "right";
			return "ground";
		}

		return "ground";
	}

	private char GetTerrainChar(string terrain, int x, int y, bool isLeft)
	{
		float scaleX = _width / 68f;
		float scaleY = _height / 24f;
		int logicX = (int)Math.Round(x / scaleX);
		int logicY = (int)Math.Round(y / scaleY);

		var rand = new System.Random(y * _width + x);
		switch (terrain)
		{
			case "woodland":
				if (isLeft)
				{
					if (logicX == 4 || logicX == 16 || logicX == 26) return '|';
					if ((logicX == 5 || logicX == 17) && logicY % 3 == 0) return '\\';
				}
				else
				{
					if (logicX == 63 || logicX == 51 || logicX == 41) return '|';
					if ((logicX == 62 || logicX == 50) && logicY % 3 == 0) return '/';
				}
				if (rand.NextDouble() < 0.15) return rand.Next(2) == 0 ? '.' : '*';
				break;

			case "stone_wall":
				if (logicY % 4 == 0) return '-';
				if (isLeft && (logicX + logicY * 2) % 8 == 0) return '|';
				if (!isLeft && (logicX - logicY * 2) % 8 == 0) return '|';
				break;

			case "cabin":
				if (logicY % 2 == 0) return '-';
				break;

			case "riverside":
				if (logicY % 3 == 0 && (logicX + logicY) % 6 < 2) return '~';
				if (logicY % 4 == 1 && (logicX - logicY) % 8 < 3) return '=';
				break;

			case "swamp":
				if (rand.NextDouble() < 0.1) return '=';
				if (rand.NextDouble() < 0.05) return '"';
				break;

			case "ruins":
				if (logicX % 12 == 0 && logicY > 3) return '|';
				if (rand.NextDouble() < 0.08) return '%';
				break;
		}
		return ' ';
	}

	private string GetGroundThemeForTerrain(string terrain)
	{
		switch (terrain)
		{
			case "woodland":
				return "grass";
			case "cabin":
				return "planks";
			case "ruins":
			case "stone_wall":
				return "stone_tiles";
			case "swamp":
				return "dirt";
			default:
				return "dirt";
		}
	}

	private char GetGroundChar(string ground, int x, int y)
	{
		float scaleX = _width / 68f;
		float scaleY = _height / 24f;
		int logicX = (int)Math.Round(x / scaleX);
		int logicY = (int)Math.Round(y / scaleY);

		var rand = new System.Random(y * _width + x);
		switch (ground)
		{
			case "grass":
				if (rand.NextDouble() < 0.12) return rand.Next(2) == 0 ? '"' : '.';
				break;
			case "planks":
				if (logicX % 6 == 0) return '|';
				break;
			case "stone_tiles":
				if (logicX % 8 == 0 || logicY % 3 == 0) return '+';
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
		float scaleX = _width / 68f;
		float scaleY = _height / 24f;

		// 1. Left perspective line
		int startX = 0;
		int endX = (int)Math.Round(30 * scaleX);
		for (int x = startX; x <= endX; x++)
		{
			float t = (float)(x - startX) / (endX - startX);
			int y = (int)Math.Round((20 * scaleY) * (1 - t) + (11 * scaleY) * t);
			if (x >= 0 && x < _width && y >= 0 && y < _height)
			{
				cell.Character = '\\';
				_buffer.SetCell(x, y, cell);
			}
		}

		// 2. Right perspective line
		startX = (int)Math.Round(38 * scaleX);
		endX = _width - 1;
		for (int x = startX; x <= endX; x++)
		{
			float t = (float)(x - startX) / (endX - startX);
			int y = (int)Math.Round((11 * scaleY) * (1 - t) + (20 * scaleY) * t);
			if (x >= 0 && x < _width && y >= 0 && y < _height)
			{
				cell.Character = '/';
				_buffer.SetCell(x, y, cell);
			}
		}

		// 3. Horizon line
		int startH = (int)Math.Round(30 * scaleX);
		int endH = (int)Math.Round(38 * scaleX);
		int horizonY = (int)Math.Round(11 * scaleY);
		for (int x = startH; x <= endH; x++)
		{
			if (x >= 0 && x < _width)
			{
				cell.Character = '-';
				_buffer.SetCell(x, horizonY, cell);
			}
		}

		// 4. Left corridor wall
		int startY = 0;
		int endY = (int)Math.Round(11 * scaleY);
		for (int y = startY; y <= endY; y++)
		{
			float t = (float)(y - startY) / (endY - startY);
			int x = (int)Math.Round((32 * scaleX) * (1 - t) + (30 * scaleX) * t);
			if (x >= 0 && x < _width && y >= 0 && y < _height)
			{
				cell.Character = '\\';
				_buffer.SetCell(x, y, cell);
			}
		}

		// 5. Right corridor wall
		for (int y = startY; y <= endY; y++)
		{
			float t = (float)(y - startY) / (endY - startY);
			int x = (int)Math.Round((36 * scaleX) * (1 - t) + (38 * scaleX) * t);
			if (x >= 0 && x < _width && y >= 0 && y < _height)
			{
				cell.Character = '/';
				_buffer.SetCell(x, y, cell);
			}
		}
	}

	private void DrawFallbackDecal(string decal)
	{
		Color fg = new Color(0.22f, 1.0f, 0.08f);
		Color bg = new Color(0, 0, 0);
		float scaleX = _width / 68f;
		float scaleY = _height / 24f;

		if (decal.StartsWith("window"))
		{
			int logicStartX = decal.EndsWith("left") ? 10 : 54;
			int startX = (int)Math.Round(logicStartX * scaleX);
			int startY = (int)Math.Round(6 * scaleY);
			string[] art = scaleY > 1.5 ? new string[] {
				"+----+",
				"|####|",
				"|####|",
				"+----+"
			} : new string[] {
				"+--+",
				"|##|",
				"+--+"
			};
			DrawArtAt(art, startX, startY, "scene.center");
		}
		else if (decal.StartsWith("door"))
		{
			int logicStartX = decal.EndsWith("left") ? 18 : 46;
			int startX = (int)Math.Round(logicStartX * scaleX);
			int startY = (int)Math.Round(10 * scaleY);
			string[] art = scaleY > 1.5 ? new string[] {
				"+----+",
				"|    |",
				"|    |",
				"|    |",
				"|    |",
				"+----+"
			} : new string[] {
				"+--+",
				"|  |",
				"|  |",
				"+--+"
			};
			DrawArtAt(art, startX, startY, "scene.center");
		}
		else if (decal.StartsWith("sofa"))
		{
			int logicStartX = decal.EndsWith("left") ? 8 : 48;
			int startX = (int)Math.Round(logicStartX * scaleX);
			int startY = (int)Math.Round(17 * scaleY);
			string[] art = scaleY > 1.5 ? new string[] {
				"  __nn__  ",
				" [______] ",
				" [______] "
			} : new string[] {
				" _n_ ",
				"[___]"
			};
			DrawArtAt(art, startX, startY, "scene.center");
		}
		else if (decal.StartsWith("npc") || decal.StartsWith("enemy"))
		{
			int logicStartX = decal.EndsWith("left") ? 14 : 50;
			int startX = (int)Math.Round(logicStartX * scaleX);
			int startY = (int)Math.Round(12 * scaleY);
			string[] art = scaleY > 1.5 ? new string[] {
				"   #####   ",
				"  #######  ",
				" ######### ",
				" ######### ",
				" ######### ",
				"  #######  "
			} : new string[] {
				"  ###  ",
				" ##### ",
				" ##### "
			};
			DrawArtAt(art, startX, startY, "scene.center");
		}
		else if (decal.StartsWith("chest"))
		{
			int logicStartX = decal.EndsWith("left") ? 12 : 52;
			int startX = (int)Math.Round(logicStartX * scaleX);
			int startY = (int)Math.Round(18 * scaleY);
			string[] art = scaleY > 1.5 ? new string[] {
				"[====]",
				"[====]"
			} : new string[] {
				"[=]"
			};
			DrawArtAt(art, startX, startY, "scene.center");
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
