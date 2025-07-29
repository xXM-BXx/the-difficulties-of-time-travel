shader_type canvas_item;

uniform float time_speed : hint_range(0.0, 10.0) = 1.0;
uniform vec4 tint_color : source_color = vec4(0.2, 0.8, 1.0, 1.0);
uniform float line_opacity : hint_range(0.0, 1.0, 0.05) = 0.2;
uniform float pixel_height = 2.0;
uniform float line_spacing = 1.0;


varying vec2 world_pos;

void vertex(){
	world_pos = (vec4(VERTEX, 1.0, 1.0) * MODEL_MATRIX).xy;
}

void fragment() {
    vec2 pixel_coord = world_pos;

    // Scanline pattern: every other pixel row is darker
    float line = mod(pixel_coord.y, pixel_height * line_spacing);
    float scanline = mix(1.0 - line_opacity, 1.0, step(pixel_height / 2.0f, line));

	vec4 base = texture(TEXTURE, UV);
    // Tint and apply scanline darkening
    vec4 hologram_color = base * tint_color * scanline;

    COLOR = hologram_color;
	float flicker = 0.95 + 0.05 * sin(TIME *  time_speed * 2.0f);
	COLOR *= flicker;
}
