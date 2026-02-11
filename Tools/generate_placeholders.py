#!/usr/bin/env python3
"""
Generate all placeholder sprite assets for IOChef Unity project.
Creates colored shapes with labels for heroes, ingredients, kitchen objects, UI elements.
"""

from PIL import Image, ImageDraw, ImageFont
import os
import sys

BASE = "/Users/kalvinzhao/IOChef/Assets/_Game/Art/Sprites"

def ensure_dir(path):
    os.makedirs(path, exist_ok=True)

def draw_text(draw, text, bbox, fill="white"):
    """Draw centered text in bounding box."""
    x1, y1, x2, y2 = bbox
    # Simple centering
    try:
        font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 14)
    except:
        font = ImageFont.load_default()

    # Get text size
    tw = draw.textlength(text, font=font)
    tx = x1 + (x2 - x1 - tw) / 2
    ty = y1 + (y2 - y1 - 18) / 2
    draw.text((tx, ty), text, fill=fill, font=font)

def create_sprite(path, width, height, bg_color, label, shape="rect", border_color=None):
    """Create a simple labeled placeholder sprite."""
    img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    if shape == "circle":
        draw.ellipse([2, 2, width-3, height-3], fill=bg_color, outline=border_color or "white", width=2)
    elif shape == "rounded":
        draw.rounded_rectangle([2, 2, width-3, height-3], radius=8, fill=bg_color, outline=border_color or "white", width=2)
    else:
        draw.rectangle([2, 2, width-3, height-3], fill=bg_color, outline=border_color or "white", width=2)

    # Draw label
    draw_text(draw, label, (0, 0, width, height))

    ensure_dir(os.path.dirname(path))
    img.save(path)

def create_hero_sprite(path, name, color, size=128):
    """Create a hero character placeholder (simple body shape)."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = size // 2, size // 2

    # Head (circle)
    head_r = size // 6
    draw.ellipse([cx - head_r, 10, cx + head_r, 10 + head_r * 2], fill=color, outline="white", width=2)

    # Body (rectangle)
    body_w = size // 3
    body_top = 10 + head_r * 2
    body_bottom = size - 25
    draw.rounded_rectangle([cx - body_w // 2, body_top, cx + body_w // 2, body_bottom],
                           radius=5, fill=color, outline="white", width=2)

    # Chef hat (triangle on top)
    hat_w = head_r + 4
    draw.polygon([(cx - hat_w, 14), (cx, 0), (cx + hat_w, 14)], fill="white")

    # Legs
    leg_w = 8
    draw.rectangle([cx - body_w // 4 - leg_w // 2, body_bottom, cx - body_w // 4 + leg_w // 2, size - 2], fill=color, outline="white")
    draw.rectangle([cx + body_w // 4 - leg_w // 2, body_bottom, cx + body_w // 4 + leg_w // 2, size - 2], fill=color, outline="white")

    # Name label at bottom
    try:
        font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 11)
    except:
        font = ImageFont.load_default()
    tw = draw.textlength(name, font=font)
    draw.text(((size - tw) / 2, size - 14), name, fill="white", font=font)

    ensure_dir(os.path.dirname(path))
    img.save(path)

def create_ingredient_sprite(path, name, color, state_color=None, size=64):
    """Create ingredient placeholder (circle with label)."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Main circle
    draw.ellipse([4, 4, size - 5, size - 5], fill=color, outline=state_color or "white", width=2)

    try:
        font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 10)
    except:
        font = ImageFont.load_default()
    tw = draw.textlength(name, font=font)
    draw.text(((size - tw) / 2, (size - 12) / 2), name, fill="white", font=font)

    ensure_dir(os.path.dirname(path))
    img.save(path)

def main():
    print("Generating placeholder sprites for IOChef...")

    # =========================================
    # HEROES (128x128 character sprites)
    # =========================================
    heroes_dir = f"{BASE}/Heroes"
    heroes = {
        "ChefRush":    {"color": (200, 80, 80),   "label": "Chef Rush"},
        "Blaze":       {"color": (220, 120, 30),   "label": "Blaze"},
        "CalmChef":    {"color": (80, 160, 200),   "label": "Calm Chef"},
        "ChillMaster": {"color": (100, 200, 230),  "label": "Chill Master"},
        "Precision":   {"color": (180, 80, 180),   "label": "Precision"},
        "Momentum":    {"color": (220, 200, 50),   "label": "Momentum"},
        "Alchemist":   {"color": (130, 80, 200),   "label": "Alchemist"},
        "IronWill":    {"color": (120, 120, 140),  "label": "Iron Will"},
    }

    for hero_id, data in heroes.items():
        # Main sprite
        create_hero_sprite(f"{heroes_dir}/{hero_id}/{hero_id}_idle.png", data["label"], data["color"])
        # Portrait (64x64 face)
        create_sprite(f"{heroes_dir}/{hero_id}/{hero_id}_portrait.png", 64, 64, data["color"], data["label"][:2], shape="circle")
        # Walk frames (simplified - 4 directions)
        for direction in ["up", "down", "left", "right"]:
            create_hero_sprite(f"{heroes_dir}/{hero_id}/{hero_id}_walk_{direction}_1.png", data["label"], data["color"])
            create_hero_sprite(f"{heroes_dir}/{hero_id}/{hero_id}_walk_{direction}_2.png", data["label"], data["color"])

    print(f"  Heroes: {len(heroes)} heroes with idle + portrait + walk frames")

    # =========================================
    # INGREDIENTS (64x64 circles, multiple states)
    # =========================================
    ingredients_dir = f"{BASE}/Ingredients"
    ingredients = {
        "Lettuce":    (80, 180, 80),
        "Tomato":     (200, 50, 50),
        "Meat":       (160, 80, 60),
        "Bun":        (210, 180, 120),
        "Cheese":     (240, 210, 60),
        "Bread":      (200, 170, 90),
        "Sausage":    (180, 80, 70),
        "Dough":      (220, 200, 160),
        "Sauce":      (180, 40, 40),
        "Pepperoni":  (160, 40, 40),
        "Pasta":      (230, 220, 140),
        "Fish":       (120, 170, 200),
        "Rice":       (240, 240, 230),
        "Seaweed":    (30, 100, 50),
        "Broth":      (180, 160, 100),
        "Vegetables": (60, 160, 60),
        "Seasoning":  (180, 140, 60),
        "Tortilla":   (220, 200, 150),
        "Noodles":    (230, 210, 130),
    }

    states = {
        "raw":     None,
        "chopped": (255, 255, 100),  # Yellow outline
        "cooked":  (255, 160, 50),   # Orange outline
        "burned":  (60, 60, 60),     # Dark outline
        "plated":  (200, 200, 255),  # Light blue outline
    }

    for ing_name, color in ingredients.items():
        for state, state_color in states.items():
            sc = state_color if state_color else "white"
            create_ingredient_sprite(
                f"{ingredients_dir}/{ing_name}_{state}.png",
                f"{ing_name[:6]}", color, sc
            )

    print(f"  Ingredients: {len(ingredients)} types x {len(states)} states = {len(ingredients)*len(states)} sprites")

    # =========================================
    # KITCHEN OBJECTS (96x96 or 64x96)
    # =========================================
    kitchen_dir = f"{BASE}/Kitchen"

    kitchen_objects = {
        "Counter":        {"color": (140, 120, 100), "w": 64, "h": 64, "shape": "rounded"},
        "CuttingBoard":   {"color": (180, 150, 100), "w": 64, "h": 64, "shape": "rounded"},
        "Cooktop":        {"color": (80, 80, 90),    "w": 64, "h": 64, "shape": "rounded"},
        "Sink":           {"color": (100, 140, 180),  "w": 64, "h": 64, "shape": "rounded"},
        "TrashBin":       {"color": (100, 100, 100),  "w": 64, "h": 64, "shape": "rounded"},
        "PlatingStation": {"color": (200, 200, 200),  "w": 64, "h": 64, "shape": "rounded"},
        "ServePoint":     {"color": (80, 180, 80),    "w": 64, "h": 64, "shape": "rounded"},
        "IngredientShelf":{"color": (160, 140, 100),  "w": 64, "h": 64, "shape": "rounded"},
        "Floor_Tile":     {"color": (200, 190, 170),  "w": 64, "h": 64, "shape": "rect"},
        "Wall_Tile":      {"color": (120, 110, 100),  "w": 64, "h": 64, "shape": "rect"},
    }

    for obj_name, data in kitchen_objects.items():
        create_sprite(
            f"{kitchen_dir}/{obj_name}.png",
            data["w"], data["h"], data["color"], obj_name[:10], shape=data["shape"]
        )

    print(f"  Kitchen: {len(kitchen_objects)} objects")

    # =========================================
    # KITCHEN THEMES (background tiles 256x256)
    # =========================================
    themes = {
        "Modern":    (230, 230, 240),
        "Medieval":  (120, 100, 80),
        "Cyberpunk": (30, 20, 50),
        "Zen":       (180, 200, 160),
        "Steampunk": (160, 130, 80),
        "Enchanted": (80, 140, 100),
    }

    for theme_name, color in themes.items():
        create_sprite(
            f"{kitchen_dir}/Themes/{theme_name}_bg.png",
            256, 256, color, theme_name, shape="rect"
        )
        # Floor tile variant
        darker = tuple(max(0, c - 30) for c in color)
        create_sprite(
            f"{kitchen_dir}/Themes/{theme_name}_floor.png",
            64, 64, darker, f"{theme_name[:4]}F", shape="rect"
        )
        # Counter variant
        lighter = tuple(min(255, c + 20) for c in color)
        create_sprite(
            f"{kitchen_dir}/Themes/{theme_name}_counter.png",
            64, 64, lighter, f"{theme_name[:4]}C", shape="rounded"
        )

    print(f"  Themes: {len(themes)} themes (bg + floor + counter each)")

    # =========================================
    # UI ELEMENTS
    # =========================================
    ui_dir = f"{BASE}/UI"

    # Stars
    for i, star_state in enumerate(["star_empty", "star_filled"]):
        color = (80, 80, 80) if i == 0 else (255, 210, 50)
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        # Star polygon
        import math
        cx, cy, r = 32, 32, 28
        points = []
        for j in range(10):
            angle = math.pi / 2 + j * math.pi / 5
            rad = r if j % 2 == 0 else r * 0.4
            points.append((cx + rad * math.cos(angle), cy - rad * math.sin(angle)))
        draw.polygon(points, fill=color, outline="white")
        ensure_dir(ui_dir)
        img.save(f"{ui_dir}/{star_state}.png")

    # Buttons
    buttons = {
        "btn_play":     {"color": (80, 180, 80),   "label": "PLAY",     "w": 200, "h": 64},
        "btn_retry":    {"color": (200, 160, 50),   "label": "RETRY",    "w": 160, "h": 48},
        "btn_next":     {"color": (80, 140, 200),   "label": "NEXT",     "w": 160, "h": 48},
        "btn_menu":     {"color": (140, 140, 140),  "label": "MENU",     "w": 160, "h": 48},
        "btn_settings": {"color": (120, 120, 130),  "label": "SETTINGS", "w": 160, "h": 48},
        "btn_shop":     {"color": (200, 140, 60),   "label": "SHOP",     "w": 160, "h": 48},
        "btn_pause":    {"color": (100, 100, 110),  "label": "II",       "w": 48,  "h": 48},
        "btn_resume":   {"color": (80, 180, 80),    "label": "RESUME",   "w": 160, "h": 48},
        "btn_claim":    {"color": (80, 200, 120),   "label": "CLAIM",    "w": 120, "h": 40},
    }

    for btn_name, data in buttons.items():
        create_sprite(f"{ui_dir}/{btn_name}.png", data["w"], data["h"], data["color"], data["label"], shape="rounded")

    # Currency icons
    create_sprite(f"{ui_dir}/icon_credits.png", 48, 48, (220, 190, 50), "$", shape="circle")
    create_sprite(f"{ui_dir}/icon_gems.png", 48, 48, (100, 150, 230), "G", shape="circle", border_color=(150, 200, 255))

    # Order card background
    create_sprite(f"{ui_dir}/order_card_bg.png", 80, 100, (60, 50, 50), "Order", shape="rounded")

    # Progress bar
    create_sprite(f"{ui_dir}/progressbar_bg.png", 200, 20, (60, 60, 60), "", shape="rounded")
    create_sprite(f"{ui_dir}/progressbar_fill.png", 200, 20, (80, 200, 120), "", shape="rounded")

    # Joystick (touch input)
    create_sprite(f"{ui_dir}/joystick_bg.png", 128, 128, (40, 40, 40, 120), "", shape="circle")
    create_sprite(f"{ui_dir}/joystick_knob.png", 64, 64, (200, 200, 200, 200), "", shape="circle")

    # Action button
    create_sprite(f"{ui_dir}/btn_action.png", 96, 96, (80, 180, 80, 180), "ACT", shape="circle")

    print(f"  UI: {len(buttons)} buttons + stars + currency icons + HUD elements")

    # =========================================
    # VFX placeholders
    # =========================================
    vfx_dir = f"{BASE}/VFX"

    vfx = {
        "flame_particle":   (255, 120, 20),
        "ice_particle":     (150, 220, 255),
        "magic_particle":   (180, 100, 255),
        "sparkle":          (255, 255, 200),
        "smoke":            (160, 160, 160),
        "celebration":      (255, 220, 50),
        "combo_flash":      (255, 255, 100),
        "shield":           (100, 180, 255),
        "trail":            (200, 200, 50),
    }

    for vfx_name, color in vfx.items():
        img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        draw.ellipse([4, 4, 28, 28], fill=(*color, 180))
        ensure_dir(vfx_dir)
        img.save(f"{vfx_dir}/{vfx_name}.png")

    print(f"  VFX: {len(vfx)} particle placeholders")

    # =========================================
    # Count total
    # =========================================
    total = 0
    for root, dirs, files in os.walk(BASE):
        total += len([f for f in files if f.endswith('.png')])

    print(f"\nTotal: {total} PNG sprites generated in {BASE}/")
    print("Done!")

if __name__ == "__main__":
    main()
