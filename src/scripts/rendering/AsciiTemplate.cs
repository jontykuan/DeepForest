using Godot;
using System;
using DeepForest.Core;

namespace DeepForest.Rendering;

public static class AsciiTemplate
{
	public static TextGrid Load(string txtPath, string? tagsPath = null, string prefix = "scene")
	{
		if (!Godot.FileAccess.FileExists(txtPath))
		{
			DeepForest.Core.Logger.Error("AsciiTemplate: File not found", "Path", txtPath);
			return new TextGrid(1, 1);
		}

		using var txtFile = Godot.FileAccess.Open(txtPath, Godot.FileAccess.ModeFlags.Read);
		string rawText = txtFile.GetAsText();
		string[] txtLines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

		int height = txtLines.Length;
		int width = 0;
		for (int i = 0; i < height; i++)
		{
			int lineLen = 0;
			foreach (char c in txtLines[i])
			{
				lineLen += TextGrid.IsFullWidth(c) ? 2 : 1;
			}
			width = Math.Max(width, lineLen);
		}

		TextGrid grid = new TextGrid(width, height);
		grid.Clear(' ', new Color(0.22f, 1.0f, 0.08f), new Color(0, 0, 0));

		string[]? tagsLines = null;
		if (!string.IsNullOrEmpty(tagsPath) && Godot.FileAccess.FileExists(tagsPath))
		{
			using var tagsFile = Godot.FileAccess.Open(tagsPath, Godot.FileAccess.ModeFlags.Read);
			string tagsText = tagsFile.GetAsText();
			tagsLines = tagsText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		}

		for (int y = 0; y < height; y++)
		{
			string txtLine = txtLines[y];
			string tagLine = (tagsLines != null && y < tagsLines.Length) ? tagsLines[y] : string.Empty;

			int gridX = 0;
			for (int charIdx = 0; charIdx < txtLine.Length; charIdx++)
			{
				char c = txtLine[charIdx];
				if (gridX >= width) break;

				char tagChar = (gridX < tagLine.Length) ? tagLine[gridX] : ' ';
				string tagStr = MapTagChar(tagChar, prefix);

				Color fg = new Color(0.22f, 1.0f, 0.08f);
				Color bg = new Color(0, 0, 0);

				bool isTransparent = (c == ' ' || c == '\0');
				CharCell cell = new CharCell(c, fg, bg, tagStr, isTransparent);
				grid.SetCell(gridX, y, cell);

				gridX += TextGrid.IsFullWidth(c) ? 2 : 1;
			}
		}

		return grid;
	}

	private static string MapTagChar(char tc, string prefix)
	{
		return tc switch
		{
			'B' => $"{prefix}.bg",
			'L' => $"{prefix}.left",
			'C' => $"{prefix}.center",
			'R' => $"{prefix}.right",
			'G' => $"{prefix}.ground",
			'O' => $"{prefix}.overlay",
			'H' => $"avatar.head",
			'F' => $"avatar.face",
			'D' => $"avatar.body",
			'A' => $"avatar.bg",
			_ => string.Empty
		};
	}
}
