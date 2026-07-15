# -*- coding: utf-8 -*-
import argparse
from PIL import Image, ImageOps, ImageEnhance, ImageDraw, ImageFont
import os
import random

WIDTH = 136
HEIGHT = 48
CHAR_W = 8
CHAR_H = 16
IMG_W = WIDTH * CHAR_W
IMG_H = HEIGHT * CHAR_H

CHAR_LIST = " .:-=+*%#@@" # Dark to bright

# Default paths
scenes_dir = r"d:\cykuan\Godot_v4.6.2-stable_mono_win64\godot project\DeepForest\assets\ascii_art\scenes"
font_path = r"d:\cykuan\Godot_v4.6.2-stable_mono_win64\godot project\DeepForest\assets\fonts\SarasaMonoTC-Regular.ttf"

try:
    font = ImageFont.truetype(font_path, 16)
except Exception as e:
    font = ImageFont.load_default()

def render_grid_to_png(grid, filename, transparent=True):
    mode = "RGBA" if transparent else "RGB"
    bg_color = (0, 0, 0, 0) if transparent else (0, 0, 0)
    img = Image.new(mode, (IMG_W, IMG_H), color=bg_color)
    draw = ImageDraw.Draw(img)
    
    fg_color = (57, 255, 20, 255) # Radiant Green
    
    for y in range(HEIGHT):
        row_str = "".join(grid[y])
        for x, char in enumerate(row_str):
            if char != ' ':
                draw.text((x * CHAR_W, y * CHAR_H), char, font=font, fill=fg_color)
                
    dest_path = os.path.join(scenes_dir, filename)
    img.save(dest_path)
    print(f"Generated successfully: {filename}")

def get_left_triangle_fade(x, y):
    if x >= 68:
        return 0.0
    y_top = 22.0 * x / 68.0
    y_bottom = 22.0 + 25.0 * (1.0 - x / 68.0)
    if y < y_top or y > y_bottom:
        return 0.0
    edge_dist = min(y - y_top, y_bottom - y)
    fade = min(edge_dist / 3.0, 1.0)
    fade *= min((68 - x) / 12.0, 1.0)
    return fade

def get_right_triangle_fade(x, y):
    if x < 68:
        return 0.0
    rx = x - 68
    y_top = 22.0 * (1.0 - rx / 67.0)
    y_bottom = 22.0 + 25.0 * (rx / 67.0)
    if y < y_top or y > y_bottom:
        return 0.0
    edge_dist = min(y - y_top, y_bottom - y)
    fade = min(edge_dist / 3.0, 1.0)
    fade *= min((x - 68) / 12.0, 1.0)
    return fade

def get_bottom_triangle_fade(x, y):
    if y < 22:
        return 0.0
    if x < 68:
        y_left_diag = 22.0 + 25.0 * (1.0 - x / 68.0)
        if y < y_left_diag:
            return 0.0
        edge_dist = y - y_left_diag
    else:
        rx = x - 68
        y_right_diag = 22.0 + 25.0 * (rx / 67.0)
        if y < y_right_diag:
            return 0.0
        edge_dist = y - y_right_diag
    
    fade = min(edge_dist / 3.0, 1.0)
    fade *= min((y - 22) / 6.0, 1.0)
    return fade

def get_top_triangle_fade(x, y):
    if y >= 22:
        return 0.0
    if x < 68:
        y_left_diag = 22.0 * x / 68.0
        if y > y_left_diag:
            return 0.0
        edge_dist = y_left_diag - y
    else:
        rx = x - 68
        y_right_diag = 22.0 * (1.0 - rx / 67.0)
        if y > y_right_diag:
            return 0.0
        edge_dist = y_right_diag - y
    
    fade = min(edge_dist / 3.0, 1.0)
    fade *= min((22 - y) / 6.0, 1.0)
    return fade

def process_image(source_path, component_type, name, suffix=""):
    if not os.path.exists(source_path):
        print(f"Error: Source image not found at {source_path}")
        return
        
    try:
        img = Image.open(source_path).convert("L")
        img_scaled = img.resize((WIDTH, HEIGHT), Image.Resampling.LANCZOS)
        img_scaled = ImageOps.autocontrast(img_scaled, cutoff=2)
        img_scaled = ImageEnhance.Contrast(img_scaled).enhance(2.2)
    except Exception as e:
        print(f"Error loading source image: {e}")
        return

    # 1. Left Woodland
    if component_type in ["all", "left"]:
        grid_left = [[" " for _ in range(WIDTH)] for _ in range(HEIGHT)]
        for y in range(HEIGHT):
            for x in range(WIDTH):
                pixel = img_scaled.getpixel((x, y))
                char_idx = int((pixel / 255.0) * (len(CHAR_LIST) - 1))
                char = CHAR_LIST[char_idx]
                fade_l = get_left_triangle_fade(x, y)
                if fade_l > 0.15 and char != ' ':
                    adj_idx = int(char_idx * fade_l)
                    grid_left[y][x] = CHAR_LIST[adj_idx]
        render_grid_to_png(grid_left, f"terrain_{name}_left{suffix}.png", transparent=True)

    # 2. Right Woodland
    if component_type in ["all", "right"]:
        grid_right = [[" " for _ in range(WIDTH)] for _ in range(HEIGHT)]
        for y in range(HEIGHT):
            for x in range(WIDTH):
                pixel = img_scaled.getpixel((x, y))
                char_idx = int((pixel / 255.0) * (len(CHAR_LIST) - 1))
                char = CHAR_LIST[char_idx]
                fade_r = get_right_triangle_fade(x, y)
                if fade_r > 0.15 and char != ' ':
                    adj_idx = int(char_idx * fade_r)
                    grid_right[y][x] = CHAR_LIST[adj_idx]
        render_grid_to_png(grid_right, f"terrain_{name}_right{suffix}.png", transparent=True)

    # 3. Base path (entire image scaled with top/bottom masks)
    if component_type in ["all", "base"]:
        grid_base = [[" " for _ in range(WIDTH)] for _ in range(HEIGHT)]
        for y in range(HEIGHT):
            for x in range(WIDTH):
                fade_sky = get_top_triangle_fade(x, y)
                if fade_sky > 0.0:
                    r_val = random.random()
                    if r_val < 0.015:
                        grid_base[y][x] = "*"
                    elif r_val < 0.04:
                        grid_base[y][x] = "."
                
                fade_path = get_bottom_triangle_fade(x, y)
                if fade_path > 0.0:
                    progress = (y - 22.0) / 26.0
                    path_half_w = 6 + progress * 20
                    dist = abs(x - 68)
                    if dist < path_half_w:
                        val = (x * 13 + y * 7) % 17
                        if val < 2:
                            grid_base[y][x] = "."
                        elif val < 4:
                            grid_base[y][x] = ":"
        render_grid_to_png(grid_base, f"perspective_template{suffix}.png", transparent=False)

    # 4. Ground
    if component_type in ["all", "ground"]:
        grid_gr = [[" " for _ in range(WIDTH)] for _ in range(HEIGHT)]
        for y in range(HEIGHT):
            for x in range(WIDTH):
                fade = get_bottom_triangle_fade(x, y)
                if fade > 0.15:
                    progress = (y - 22.0) / 26.0
                    path_half_w = 6 + progress * 20
                    dist = abs(x - 68)
                    if dist >= path_half_w:
                        pixel = img_scaled.getpixel((x, y))
                        char_idx = int((pixel / 255.0) * (len(CHAR_LIST) - 1))
                        char = CHAR_LIST[char_idx]
                        if char != ' ':
                            adj_idx = int(char_idx * fade)
                            grid_gr[y][x] = CHAR_LIST[adj_idx]
        render_grid_to_png(grid_gr, f"ground_{name}{suffix}.png", transparent=True)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Convert images to diagonally sliced ASCII components.")
    parser.add_argument("--source", required=True, help="Absolute path to the source image.")
    parser.add_argument("--type", default="all", choices=["all", "left", "right", "base", "ground"], help="Type of component to export.")
    parser.add_argument("--name", required=True, help="Identifying name for the terrain/ground.")
    parser.add_argument("--suffix", default="", help="Suffix to append to filenames (e.g. _2).")
    
    args = parser.parse_args()
    process_image(args.source, args.type, args.name, args.suffix)
