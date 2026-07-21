using Godot;

namespace DeepForest.Rendering;

public class AvatarRenderer
{
	private int _width;
	private int _height;
	private TextGrid _buffer;

	public AvatarRenderer(int width = 40, int height = 25)
	{
		_width = width;
		_height = height;
		_buffer = new TextGrid(_width, _height);
	}

	public TextGrid RenderAvatar(string avatarName, string expression, int currentHp, int currentSanity)
	{
		_buffer.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		string baseDir = $"{DeepForest.Core.ResourcePaths.AvatarsDir}{avatarName}";
		
		string fileName = "base";
		if (currentHp < 30 || expression == "pain")
		{
			fileName = "pain";
		}
		else if (currentSanity < 30 || expression == "insane")
		{
			fileName = "insane";
		}

		string txtPath = $"{baseDir}/{fileName}.txt";
		string tagsPath = $"{baseDir}/{fileName}.tags";

		TextGrid baseGrid;
		if (Godot.FileAccess.FileExists(txtPath))
		{
			baseGrid = AsciiTemplate.Load(txtPath, tagsPath, "avatar");
		}
		else
		{
			string fallbackTxt = $"{baseDir}/base.txt";
			string fallbackTags = $"{baseDir}/base.tags";
			if (Godot.FileAccess.FileExists(fallbackTxt))
			{
				baseGrid = AsciiTemplate.Load(fallbackTxt, fallbackTags, "avatar");
			}
			else
			{
				baseGrid = GenerateFallbackAvatar(avatarName);
			}
		}

		_buffer.Blit(baseGrid, 0, 0, forceOverwrite: true);

		ApplyAvatarEffects(fileName, currentHp, currentSanity);

		return _buffer;
	}

	private void ApplyAvatarEffects(string filePrefix, int hp, int sanity)
	{
		bool isLowHp = hp < 30 || filePrefix == "pain";
		bool isLowSanity = sanity < 30 || filePrefix == "insane";

		for (int y = 0; y < _height; y++)
		{
			for (int x = 0; x < _width; x++)
			{
				CharCell cell = _buffer.GetCell(x, y);

				if (cell.Tag == "avatar.face")
				{
					if (isLowHp)
					{
						cell.ForegroundColor = new Color(1.0f, 0.13f, 0.13f);
					}
					else if (isLowSanity)
					{
						cell.ForegroundColor = new Color(0.4f, 0.0f, 0.4f); 
					}
				}

				if (cell.Tag == "avatar.bg" && isLowSanity)
				{
					var rand = new System.Random(y * _width + x);
					if (rand.NextDouble() < 0.1)
					{
						cell.Character = rand.Next(2) == 0 ? '▁' : '░';
						cell.ForegroundColor = new Color(0.4f, 0.0f, 0.0f); 
					}
				}

				_buffer.SetCell(x, y, cell);
			}
		}
	}

	private TextGrid GenerateFallbackAvatar(string name)
	{
		TextGrid grid = new TextGrid(_width, _height);
		grid.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		// ASCII border box for pixel-perfect alignment
		for (int x = 0; x < _width; x++)
		{
			grid.SetCell(x, 0, new CharCell('-', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));
			grid.SetCell(x, _height - 1, new CharCell('-', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));
		}
		for (int y = 0; y < _height; y++)
		{
			grid.SetCell(0, y, new CharCell('|', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));
			grid.SetCell(_width - 1, y, new CharCell('|', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));
		}
		grid.SetCell(0, 0, new CharCell('+', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));
		grid.SetCell(_width - 1, 0, new CharCell('+', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));
		grid.SetCell(0, _height - 1, new CharCell('+', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));
		grid.SetCell(_width - 1, _height - 1, new CharCell('+', new Color(1, 1, 1), new Color(0, 0, 0), "avatar.bg"));

		// Hardcore American Cthulhu shadow/mask investigator style (density-shaded pure ASCII)
		string[] art = {
			"    .::::::::::.    ",
			"  .##@@@@@@@@@@##.  ",
			" :###@@@@@@@@@@###: ",
			":####@        @####:",
			"####@  (o)  (o)  @####",
			"####@    \\  /    @####",
			"####@    [##]    @####",
			":####@          @####:",
			":#####@        @#####:",
			" :######@@@@@@######: ",
			"  .################.  ",
			"    '::::::::::::'    "
		};

		int startY = _height / 2 - art.Length / 2;
		int startX = _width / 2 - 20 / 2;

		for (int ay = 0; ay < art.Length; ay++)
		{
			for (int ax = 0; ax < art[ay].Length; ax++)
			{
				char c = art[ay][ax];
				string tag = "avatar.body";
				if (ay == 4 && ((ax >= 7 && ax <= 9) || (ax >= 12 && ax <= 14))) tag = "avatar.face";
				if (ay == 5 && (ax >= 9 && ax <= 12)) tag = "avatar.face";
				if (ay == 6 && (ax >= 9 && ax <= 12)) tag = "avatar.face";
				if (ay <= 3) tag = "avatar.head";

				grid.SetCell(startX + ax, startY + ay, new CharCell(c, new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0), tag));
			}
		}

		for (int y = 1; y < _height - 1; y++)
		{
			for (int x = 1; x < _width - 1; x++)
			{
				CharCell cell = grid.GetCell(x, y);
				if (cell.Tag == string.Empty || cell.Character == ' ')
				{
					cell.Tag = "avatar.bg";
					grid.SetCell(x, y, cell);
				}
			}
		}

		return grid;
	}
}
