shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest, repeat_disable;
uniform sampler2D ascii_tex : filter_nearest, repeat_disable;
uniform vec2 char_size = vec2(8.0, 16.0);
uniform vec4 tint_color : source_color = vec4(0.22, 1.0, 0.08, 1.0); // Classic radiation green (#39FF14)
uniform vec4 bg_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);      // Pure black
uniform float glitch_intensity : hint_range(0.0, 1.0) = 0.0;
uniform vec2 blindness_mask = vec2(1.0, 1.0); // (left_eye_active, right_eye_active) where 1.0 is normal, 0.0 is blind
uniform bool colored_mode = false;

// Pseudo-random generator for glitches
float rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
    // 1. Get the screen resolution
    vec2 screen_size = vec2(textureSize(screen_texture, 0));
    if (screen_size.x == 0.0) {
        screen_size = 1.0 / SCREEN_PIXEL_SIZE;
    }
    
    // 2. Downsample UVs to get grid cell coordinates
    vec2 cell_count = screen_size / char_size;
    vec2 cell_uv = floor(SCREEN_UV * cell_count) / cell_count;
    
    // 3. Sample the average color of the cell (from the center of the cell)
    vec2 sample_uv = cell_uv + (0.5 * char_size / screen_size);
    vec4 screen_color = texture(screen_texture, sample_uv);
    
    // 4. Calculate luminosity
    float gray = dot(screen_color.rgb, vec3(0.299, 0.587, 0.114));
    
    // 5. Apply blindness mask (left/right eye)
    // Left eye blind -> mask.x = 0.0, affects SCREEN_UV.x < 0.5
    // Right eye blind -> mask.y = 0.0, affects SCREEN_UV.x >= 0.5
    if ((SCREEN_UV.x < 0.5 && blindness_mask.x < 0.5) || (SCREEN_UV.x >= 0.5 && blindness_mask.y < 0.5)) {
        COLOR = bg_color;
    } else {
        // 6. Select character index (0 to 15)
        float char_idx = floor(gray * 16.0);
        char_idx = clamp(char_idx, 0.0, 15.0);
        
        // 7. Apply glitch intensity (randomly swap characters if glitch_intensity > 0)
        if (glitch_intensity > 0.0) {
            float r = rand(cell_uv);
            if (r < glitch_intensity) {
                // Swap with a random character index between 0 and 15
                char_idx = floor(rand(cell_uv + vec2(r, 0.0)) * 16.0);
            }
        }
        
        // 8. Calculate local coordinates within the cell [0, 1]
        vec2 local_uv = fract(SCREEN_UV * cell_count);
        
        // 9. Map local UV to the specific character in the ascii_ramp_texture (horizontal strip of 16 chars)
        vec2 ascii_uv = vec2((char_idx + local_uv.x) / 16.0, local_uv.y);
        
        // 10. Sample character intensity (grayscale, read red channel)
        float ascii_intensity = texture(ascii_tex, ascii_uv).r;
        
        // 11. Determine the color of the text
        vec4 final_text_color = colored_mode ? screen_color : tint_color;
        
        // 12. Mix background and text color
        COLOR = mix(bg_color, final_text_color, ascii_intensity);
    }
}
