using Godot;

namespace DeepForest.Rendering;

public struct CharCell
{
	public char Character { get; set; }
	public Color ForegroundColor { get; set; }
	public Color BackgroundColor { get; set; }
	public string Tag { get; set; }
	public bool IsTransparent { get; set; }

	public CharCell(char character, Color fgColor, Color bgColor, string tag = "", bool isTransparent = false)
	{
		Character = character;
		ForegroundColor = fgColor;
		BackgroundColor = bgColor;
		Tag = tag ?? string.Empty;
		IsTransparent = isTransparent;
	}
}
