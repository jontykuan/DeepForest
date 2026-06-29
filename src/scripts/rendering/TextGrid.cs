using Godot;
using System;
using System.Text;

namespace DeepForest.Rendering;

public class TextGrid
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	private CharCell[,] _cells;

	public TextGrid(int width, int height)
	{
		Width = width;
		Height = height;
		_cells = new CharCell[width, height];
		Clear();
	}

	public CharCell GetCell(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			return default;
		return _cells[x, y];
	}

	public void SetCell(int x, int y, CharCell cell)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			return;

		ResolveCollision(x, y);

		_cells[x, y] = cell;

		if (IsFullWidth(cell.Character))
		{
			if (x + 1 < Width)
			{
				ResolveCollision(x + 1, y);
				_cells[x + 1, y] = new CharCell('\0', cell.ForegroundColor, cell.BackgroundColor, "_", cell.IsTransparent);
			}
		}
	}

	public void Clear(char character = ' ', Color? fg = null, Color? bg = null, string tag = "")
	{
		Color fgColor = fg ?? new Color(0.22f, 1.0f, 0.08f);
		Color bgColor = bg ?? new Color(0, 0, 0);

		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				_cells[x, y] = new CharCell(character, fgColor, bgColor, tag, false);
			}
		}
	}

	public void Blit(TextGrid src, int destX, int destY)
	{
		for (int srcY = 0; srcY < src.Height; srcY++)
		{
			int targetY = destY + srcY;
			if (targetY < 0 || targetY >= Height) continue;

			for (int srcX = 0; srcX < src.Width; srcX++)
			{
				int targetX = destX + srcX;
				if (targetX < 0 || targetX >= Width) continue;

				CharCell srcCell = src.GetCell(srcX, srcY);
				if (srcCell.IsTransparent) continue;
				if (srcCell.Tag == "_") continue;

				SetCell(targetX, targetY, srcCell);
			}
		}
	}

	private void ResolveCollision(int x, int y)
	{
		CharCell current = _cells[x, y];
		if (current.Tag == "_")
		{
			if (x - 1 >= 0)
			{
				_cells[x - 1, y] = new CharCell(' ', _cells[x - 1, y].ForegroundColor, _cells[x - 1, y].BackgroundColor, _cells[x - 1, y].Tag, _cells[x - 1, y].IsTransparent);
			}
			_cells[x, y].Tag = string.Empty;
		}
		else if (IsFullWidth(current.Character))
		{
			if (x + 1 < Width && _cells[x + 1, y].Tag == "_")
			{
				_cells[x + 1, y] = new CharCell(' ', _cells[x + 1, y].ForegroundColor, _cells[x + 1, y].BackgroundColor, string.Empty, _cells[x + 1, y].IsTransparent);
			}
		}
	}

	public static bool IsFullWidth(char c)
	{
		return (c >= 0x4E00 && c <= 0x9FFF) || 
			   (c >= 0x3000 && c <= 0x303F) || 
			   (c >= 0xFF00 && c <= 0xFFEF) || 
			   (c >= 0x1100 && c <= 0x11FF) || 
			   (c >= 0xAC00 && c <= 0xD7AF);   
	}

	public string ToBBCode()
	{
		StringBuilder sb = new StringBuilder();

		for (int y = 0; y < Height; y++)
		{
			Color currentFg = Color.Color8(0, 0, 0, 0); 
			Color currentBg = Color.Color8(0, 0, 0, 0);
			bool inColorTag = false;
			bool inBgTag = false;

			for (int x = 0; x < Width; x++)
			{
				CharCell cell = _cells[x, y];
				if (cell.Tag == "_") continue;

				char c = cell.Character;
				if (c == '\0') c = ' ';

				bool fgChanged = cell.ForegroundColor != currentFg;
				bool bgChanged = cell.BackgroundColor != currentBg;

				if (fgChanged || bgChanged)
				{
					if (inBgTag)
					{
						sb.Append("[/bgcolor]");
						inBgTag = false;
					}
					if (inColorTag)
					{
						sb.Append("[/color]");
						inColorTag = false;
					}

					if (cell.ForegroundColor != Color.Color8(0, 0, 0, 0))
					{
						sb.Append($"[color=#{cell.ForegroundColor.ToHtml(false)}]");
						currentFg = cell.ForegroundColor;
						inColorTag = true;
					}
					if (cell.BackgroundColor != Color.Color8(0, 0, 0, 0))
					{
						sb.Append($"[bgcolor=#{cell.BackgroundColor.ToHtml(false)}]");
						currentBg = cell.BackgroundColor;
						inBgTag = true;
					}
				}

				sb.Append(c);
			}

			if (inBgTag) sb.Append("[/bgcolor]");
			if (inColorTag) sb.Append("[/color]");

			if (y < Height - 1)
			{
				sb.Append("\n");
			}
		}

		return sb.ToString();
	}
}
