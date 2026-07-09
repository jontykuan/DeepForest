using Godot;
using System;
using DeepForest.Core;

namespace DeepForest.UI
{
    public partial class SettingsPanelUI : Panel
    {
        [Signal] public delegate void SettingsClosedEventHandler();

        private HSlider _masterSlider = null!;
        private Label _masterLabel = null!;
        private HSlider _bgmSlider = null!;
        private Label _bgmLabel = null!;
        private HSlider _sfxSlider = null!;
        private Label _sfxLabel = null!;

        private Font _font = null!;

        public void Initialize(Font font)
        {
            _font = font;

            // 1. Style the panel background
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0, 0, 0, 1); // Pure black
            styleBox.BorderColor = new Color(0.223f, 1.0f, 0.078f, 1.0f); // Radiation green
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthBottom = 2;
            AddThemeStyleboxOverride("panel", styleBox);

            // 2. Main layout container
            var margin = new MarginContainer();
            margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize, 15);
            AddChild(margin);

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 12);
            margin.AddChild(vbox);

            // Title
            var title = new Label();
            title.Text = "【 系統設定 │ SETTINGS 】";
            title.AddThemeFontOverride("font", _font);
            title.AddThemeFontSizeOverride("font_size", 16);
            title.AddThemeColorOverride("font_color", new Color(0.223f, 1.0f, 0.078f, 1.0f));
            title.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(title);

            vbox.AddChild(new HSeparator());

            // 3. Audio Controls
            // Master
            var masterRow = new HBoxContainer();
            _masterLabel = new Label();
            _masterLabel.Text = $"主要音量: {(int)(SaveManager.CurrentSave.VolumeMaster * 100)}%";
            _masterLabel.CustomMinimumSize = new Vector2(120, 0);
            _masterLabel.AddThemeFontOverride("font", _font);
            _masterLabel.AddThemeFontSizeOverride("font_size", 13);
            _masterLabel.AddThemeColorOverride("font_color", new Color(0.223f, 1.0f, 0.078f, 1.0f));
            masterRow.AddChild(_masterLabel);

            _masterSlider = new HSlider();
            _masterSlider.MinValue = 0.0;
            _masterSlider.MaxValue = 1.0;
            _masterSlider.Step = 0.05;
            _masterSlider.Value = SaveManager.CurrentSave.VolumeMaster;
            _masterSlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _masterSlider.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
            _masterSlider.ValueChanged += (val) => {
                SaveManager.CurrentSave.VolumeMaster = (float)val;
                _masterLabel.Text = $"主要音量: {(int)(val * 100)}%";
                SaveManager.ApplyBusVolume("Master", (float)val);
            };
            masterRow.AddChild(_masterSlider);
            vbox.AddChild(masterRow);

            // BGM
            var bgmRow = new HBoxContainer();
            _bgmLabel = new Label();
            _bgmLabel.Text = $"背景音量: {(int)(SaveManager.CurrentSave.VolumeBgm * 100)}%";
            _bgmLabel.CustomMinimumSize = new Vector2(120, 0);
            _bgmLabel.AddThemeFontOverride("font", _font);
            _bgmLabel.AddThemeFontSizeOverride("font_size", 13);
            _bgmLabel.AddThemeColorOverride("font_color", new Color(0.223f, 1.0f, 0.078f, 1.0f));
            bgmRow.AddChild(_bgmLabel);

            _bgmSlider = new HSlider();
            _bgmSlider.MinValue = 0.0;
            _bgmSlider.MaxValue = 1.0;
            _bgmSlider.Step = 0.05;
            _bgmSlider.Value = SaveManager.CurrentSave.VolumeBgm;
            _bgmSlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _bgmSlider.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
            _bgmSlider.ValueChanged += (val) => {
                SaveManager.CurrentSave.VolumeBgm = (float)val;
                _bgmLabel.Text = $"背景音量: {(int)(val * 100)}%";
                SaveManager.ApplyBusVolume("Music", (float)val);
            };
            bgmRow.AddChild(_bgmSlider);
            vbox.AddChild(bgmRow);

            // SFX
            var sfxRow = new HBoxContainer();
            _sfxLabel = new Label();
            _sfxLabel.Text = $"效果音量: {(int)(SaveManager.CurrentSave.VolumeSfx * 100)}%";
            _sfxLabel.CustomMinimumSize = new Vector2(120, 0);
            _sfxLabel.AddThemeFontOverride("font", _font);
            _sfxLabel.AddThemeFontSizeOverride("font_size", 13);
            _sfxLabel.AddThemeColorOverride("font_color", new Color(0.223f, 1.0f, 0.078f, 1.0f));
            sfxRow.AddChild(_sfxLabel);

            _sfxSlider = new HSlider();
            _sfxSlider.MinValue = 0.0;
            _sfxSlider.MaxValue = 1.0;
            _sfxSlider.Step = 0.05;
            _sfxSlider.Value = SaveManager.CurrentSave.VolumeSfx;
            _sfxSlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _sfxSlider.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
            _sfxSlider.ValueChanged += (val) => {
                SaveManager.CurrentSave.VolumeSfx = (float)val;
                _sfxLabel.Text = $"效果音量: {(int)(val * 100)}%";
                SaveManager.ApplyBusVolume("SFX", (float)val);
            };
            sfxRow.AddChild(_sfxSlider);
            vbox.AddChild(sfxRow);

            vbox.AddChild(new HSeparator());

            // 4. Resolution Sizing Row
            var resLabel = new Label();
            resLabel.Text = "視窗解析度:";
            resLabel.AddThemeFontOverride("font", _font);
            resLabel.AddThemeFontSizeOverride("font_size", 13);
            resLabel.AddThemeColorOverride("font_color", new Color(0.223f, 1.0f, 0.078f, 1.0f));
            vbox.AddChild(resLabel);

            var resGrid = new GridContainer();
            resGrid.Columns = 2;
            vbox.AddChild(resGrid);

            string[] sizes = { "1280x720", "1600x900", "1920x1080", "Fullscreen" };
            string[] displayNames = { "1280 x 720", "1600 x 900", "1920 x 1080", "全螢幕" };

            for (int i = 0; i < sizes.Length; i++)
            {
                string sizeKey = sizes[i];
                var btn = new Button();
                btn.Text = displayNames[i];
                btn.AddThemeFontOverride("font", _font);
                btn.AddThemeFontSizeOverride("font_size", 12);
                btn.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
                btn.Pressed += () => {
                    SaveManager.CurrentSave.WindowSize = sizeKey;
                    SaveManager.ApplyWindowSize(sizeKey);
                };
                resGrid.AddChild(btn);
            }

            vbox.AddChild(new HSeparator());

            // 5. Language Label
            var langRow = new HBoxContainer();
            var langLabel = new Label();
            langLabel.Text = "語言設定: 中文 (僅提供此語言)";
            langLabel.AddThemeFontOverride("font", _font);
            langLabel.AddThemeFontSizeOverride("font_size", 13);
            langLabel.AddThemeColorOverride("font_color", new Color(0.223f, 1.0f, 0.078f, 1.0f));
            langRow.AddChild(langLabel);
            vbox.AddChild(langRow);

            vbox.AddChild(new HSeparator());

            // 6. Close Button
            var closeBtn = new Button();
            closeBtn.Text = "確定並關閉";
            closeBtn.AddThemeFontOverride("font", _font);
            closeBtn.AddThemeFontSizeOverride("font_size", 13);
            closeBtn.Modulate = new Color(0.223f, 1.0f, 0.078f, 1.0f);
            closeBtn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            closeBtn.CustomMinimumSize = new Vector2(100, 30);
            closeBtn.Pressed += () => {
                SaveManager.Save();
                EmitSignal(SignalName.SettingsClosed);
            };
            vbox.AddChild(closeBtn);
        }
    }
}
