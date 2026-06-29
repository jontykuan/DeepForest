import os
import shutil
from PIL import Image, ImageDraw, ImageFont

def main():
    # 1. Define paths
    base_dir = r"d:\cykuan\Godot_v4.6.2-stable_mono_win64\godot project\DeepForest"
    fonts_dir = os.path.join(base_dir, "assets", "fonts")
    textures_dir = os.path.join(base_dir, "assets", "textures")
    shaders_dir = os.path.join(base_dir, "assets", "shaders")
    src_dir = os.path.join(base_dir, "src")
    
    # Create directories if they don't exist
    for d in [fonts_dir, textures_dir, shaders_dir, 
              os.path.join(src_dir, "resources", "cards"),
              os.path.join(src_dir, "resources", "events"),
              os.path.join(src_dir, "scenes"),
              os.path.join(src_dir, "scripts", "core"),
              os.path.join(src_dir, "scripts", "cards"),
              os.path.join(src_dir, "scripts", "character"),
              os.path.join(src_dir, "scripts", "scene"),
              os.path.join(src_dir, "scripts", "combat"),
              os.path.join(src_dir, "scripts", "narrative"),
              os.path.join(src_dir, "scripts", "ui")]:
        os.makedirs(d, exist_ok=True)
        print(f"Created directory: {d}")

    # 2. Copy Consolas font
    src_font = r"C:\Windows\Fonts\consola.ttf"
    dest_font = os.path.join(fonts_dir, "Consolas.ttf")
    if os.path.exists(src_font):
        shutil.copy(src_font, dest_font)
        print(f"Copied Consolas font to {dest_font}")
    else:
        print(f"Warning: {src_font} not found!")

    # 3. Generate ascii_ramp.png
    char_width = 8
    char_height = 16
    font_size = 13  # Consolas size 13 fits nicely in 8x16
    
    # 16-character ramp defined in GAME_DESIGN.md
    ramp = " .,:-=+*oaxhU#%@"
    assert len(ramp) == 16
    
    # Create grayscale image: 16 characters * 8 pixels wide, 16 pixels high
    img = Image.new('L', (16 * char_width, char_height), color=0)
    draw = ImageDraw.Draw(img)
    
    try:
        font = ImageFont.truetype(dest_font, font_size)
    except Exception as e:
        print(f"Failed to load TrueType font, using default: {e}")
        font = ImageFont.load_default()
        
    for i, char in enumerate(ramp):
        # Calculate bounding box to center the character
        bbox = draw.textbbox((0, 0), char, font=font)
        w = bbox[2] - bbox[0]
        h = bbox[3] - bbox[1]
        
        # Calculate horizontal and vertical offsets to center character in the 8x16 cell
        cell_x_offset = i * char_width
        x = cell_x_offset + (char_width - w) // 2 - bbox[0]
        y = (char_height - h) // 2 - bbox[1]
        
        draw.text((x, y), char, fill=255, font=font)
        print(f"Rendered '{char}' at cell {i} (x={x}, y={y})")
        
    dest_ramp = os.path.join(textures_dir, "ascii_ramp.png")
    img.save(dest_ramp)
    print(f"Saved ASCII ramp texture to {dest_ramp}")

if __name__ == "__main__":
    main()
