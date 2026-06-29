using Godot;
using System;

namespace DeepForest.Rendering;

public static class EffectRenderer
{
	private static readonly char[] GlitchChars = { '░', '▒', '▓', '█', '?', '@', '#', '$', '%', '^', '&', '*', '詛', '咒', '穢', '祟', '■' };
	private static readonly string[] Whispers = { "牠在看著你", "快點離開", "深淵低語", "無路可逃", "理智崩解", "不可直視", "黑暗蔓延" };

	public static void ApplyEffects(TextGrid grid, int hp, int sanity, int corruption, bool leftBlind, bool rightBlind)
	{
		float sanityPercent = (float)sanity / 100f;
		var rand = new Random();

		if (leftBlind)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				for (int x = 0; x < grid.Width / 2; x++)
				{
					CharCell cell = grid.GetCell(x, y);
					if (cell.Tag != "_")
					{
						cell.Character = ' ';
						cell.ForegroundColor = new Color(0, 0, 0);
						grid.SetCell(x, y, cell);
					}
				}
			}
		}

		if (rightBlind)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				for (int x = grid.Width / 2; x < grid.Width; x++)
				{
					CharCell cell = grid.GetCell(x, y);
					if (cell.Tag != "_")
					{
						cell.Character = ' ';
						cell.ForegroundColor = new Color(0, 0, 0);
						grid.SetCell(x, y, cell);
					}
				}
			}
		}

		if (sanityPercent < 0.3f)
		{
			float glitchChance = 0.05f + (0.3f - sanityPercent) * 0.4f;
			for (int y = 0; y < grid.Height; y++)
			{
				for (int x = 0; x < grid.Width; x++)
				{
					CharCell cell = grid.GetCell(x, y);
					if (cell.Tag == "_" || cell.Character == ' ' || cell.ForegroundColor == new Color(0, 0, 0)) continue;

					if (rand.NextDouble() < glitchChance)
					{
						cell.Character = GlitchChars[rand.Next(GlitchChars.Length)];
						if (sanityPercent < 0.15f)
						{
							cell.ForegroundColor = new Color(0.4f, 0.0f, 0.0f); 
						}
						grid.SetCell(x, y, cell);
					}
				}
			}
		}

		if (sanityPercent < 0.15f)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				for (int x = 0; x < grid.Width; x++)
				{
					CharCell cell = grid.GetCell(x, y);
					if (cell.Tag == "_") continue;

					if (cell.ForegroundColor == new Color(0.22f, 1.0f, 0.08f))
					{
						cell.ForegroundColor = new Color(0.8f, 0.1f, 0.1f); 
						grid.SetCell(x, y, cell);
					}
				}
			}
		}

		if (corruption > 20)
		{
			double whisperChance = (corruption - 20) * 0.002;
			for (int y = 1; y < grid.Height - 1; y++)
			{
				for (int x = 2; x < grid.Width - 10; x++)
				{
					CharCell cell = grid.GetCell(x, y);
					if (cell.Tag == "scene.bg" || cell.Tag == "avatar.bg")
					{
						if (rand.NextDouble() < whisperChance)
						{
							string whisper = Whispers[rand.Next(Whispers.Length)];
							bool canInject = true;
							for (int i = 0; i < whisper.Length; i++)
							{
								CharCell targetCell = grid.GetCell(x + i, y);
								if (targetCell.Character != ' ' && targetCell.Character != '\0')
								{
									canInject = false;
									break;
								}
							}

							if (canInject)
							{
								Color whisperColor = new Color(0.4f, 0.0f, 0.0f); 
								for (int i = 0; i < whisper.Length; i++)
								{
									grid.SetCell(x + i, y, new CharCell(whisper[i], whisperColor, new Color(0, 0, 0), "scene.overlay"));
								}
								x += whisper.Length; 
							}
						}
					}
				}
			}
		}
	}
}
