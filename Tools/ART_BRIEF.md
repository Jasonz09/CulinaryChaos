# IOChef - Art Production Brief & Naming Convention

## Target Specifications

| Asset Type        | Resolution   | Format | Transparency | Animation |
|-------------------|-------------|--------|-------------|-----------|
| Hero Idle         | 128x128     | PNG    | Yes         | 4 frames  |
| Hero Walk         | 128x128     | PNG    | Yes         | 8 frames (2 per direction) |
| Hero Portrait     | 64x64       | PNG    | Yes         | No        |
| Ingredient        | 64x64       | PNG    | Yes         | No        |
| Kitchen Object    | 64x64       | PNG    | Yes         | No        |
| Theme Background  | 256x256     | PNG    | No (tiled)  | No        |
| Theme Tile        | 64x64       | PNG    | No (tiled)  | No        |
| UI Button         | varies      | PNG    | Yes (9-slice) | No      |
| UI Icon           | 48x48       | PNG    | Yes         | No        |
| VFX Particle      | 32x32       | PNG    | Yes         | Optional  |
| Star              | 64x64       | PNG    | Yes         | No        |

---

## File Naming Convention

```
Assets/_Game/Art/Sprites/
├── Heroes/
│   └── {HeroId}/
│       ├── {HeroId}_idle.png              (default facing forward)
│       ├── {HeroId}_idle_{1-4}.png        (animation frames)
│       ├── {HeroId}_walk_{dir}_{1-2}.png  (dir = up/down/left/right)
│       ├── {HeroId}_portrait.png          (face close-up for UI)
│       ├── {HeroId}_interact.png          (cooking/pickup pose)
│       └── Skins/
│           └── {SkinId}/
│               ├── {HeroId}_{SkinId}_idle.png
│               └── {HeroId}_{SkinId}_walk_{dir}_{frame}.png
│
├── Ingredients/
│   └── {IngredientName}_{state}.png       (state = raw/chopped/cooked/burned/plated)
│
├── Kitchen/
│   ├── {ObjectName}.png                   (Counter, Cooktop, CuttingBoard, etc.)
│   ├── {ObjectName}_active.png            (optional: in-use state)
│   └── Themes/
│       └── {ThemeName}_{variant}.png      (variant = bg/floor/counter/wall)
│
├── UI/
│   ├── btn_{name}.png                     (buttons)
│   ├── icon_{name}.png                    (small icons)
│   ├── star_{state}.png                   (empty/filled)
│   ├── order_card_bg.png
│   ├── progressbar_{part}.png             (bg/fill)
│   ├── joystick_{part}.png                (bg/knob)
│   └── btn_action.png
│
└── VFX/
    └── {effect_name}.png                  (particle textures)
```

**Naming rules:**
- All lowercase with underscores (except HeroId and ThemeName which use PascalCase for folder names)
- States always go at the end: `Tomato_chopped.png`
- Directions: `up`, `down`, `left`, `right`
- Animation frames: 1-indexed: `_1.png`, `_2.png`

---

## Complete Asset List

### Heroes (8 heroes x ~12 sprites each = ~96 sprites)

| Hero ID       | Color Hex | Rarity    | Frames Needed |
|---------------|-----------|-----------|---------------|
| ChefRush      | #C85050   | Common    | idle(4) + walk(8) + portrait + interact = 14 |
| Blaze         | #DC781E   | Epic      | 14 + flame VFX overlay |
| CalmChef      | #50A0C8   | Rare      | 14 + calm aura overlay |
| ChillMaster   | #64C8E6   | Legendary | 14 + ice VFX overlay |
| Precision     | #B450B4   | Rare      | 14 + precision rings overlay |
| Momentum      | #DCC832   | Epic      | 14 + speed trail overlay |
| Alchemist     | #8250C8   | Legendary | 14 + magic aura overlay |
| IronWill      | #78788C   | Epic      | 14 + shield overlay |

### Skins (3 per hero x 8 = 24 skins x ~14 sprites = ~336 skin sprites)

Priority order for production:
1. Default skins (already counted above)
2. ChefRush skins (most players use starter hero)
3. Blaze skins (battle pass hero, high visibility)
4. Legendary hero skins (ChillMaster, Alchemist)
5. Remaining hero skins

### Ingredients (19 types x 5 states = 95 sprites)

| Ingredient  | Color Reference | States |
|-------------|----------------|--------|
| Lettuce     | #50B450        | raw, chopped, cooked, burned, plated |
| Tomato      | #C83232        | raw, chopped, cooked, burned, plated |
| Meat        | #A0503C        | raw, chopped, cooked, burned, plated |
| Bun         | #D2B478        | raw, chopped, cooked, burned, plated |
| Cheese      | #F0D23C        | raw, chopped, cooked, burned, plated |
| Bread       | #C8AA5A        | raw, chopped, cooked, burned, plated |
| Sausage     | #B45046        | raw, chopped, cooked, burned, plated |
| Dough       | #DCC8A0        | raw, chopped, cooked, burned, plated |
| Sauce       | #B42828        | raw, chopped, cooked, burned, plated |
| Pepperoni   | #A02828        | raw, chopped, cooked, burned, plated |
| Pasta       | #E6DC8C        | raw, chopped, cooked, burned, plated |
| Fish        | #78AAC8        | raw, chopped, cooked, burned, plated |
| Rice        | #F0F0E6        | raw, chopped, cooked, burned, plated |
| Seaweed     | #1E6432        | raw, chopped, cooked, burned, plated |
| Broth       | #B4A064        | raw, chopped, cooked, burned, plated |
| Vegetables  | #3CA03C        | raw, chopped, cooked, burned, plated |
| Seasoning   | #B48C3C        | raw, chopped, cooked, burned, plated |
| Tortilla    | #DCC896        | raw, chopped, cooked, burned, plated |
| Noodles     | #E6D282        | raw, chopped, cooked, burned, plated |

### Kitchen Objects (10 base + 6 themes x 3 variants = 28 sprites)

Base objects: Counter, CuttingBoard, Cooktop, Sink, TrashBin, PlatingStation, ServePoint, IngredientShelf, Floor_Tile, Wall_Tile

Theme variants (bg + floor + counter): Modern, Medieval, Cyberpunk, Zen, Steampunk, Enchanted

### UI Elements (~25 sprites)

Buttons (9), currency icons (2), stars (2), order card, progress bars (2), joystick (2), action button, plus any additional HUD frames.

### VFX Particles (9 sprites)

flame, ice, magic, sparkle, smoke, celebration, combo_flash, shield, trail

---

## Total Asset Count

| Category     | Count  | Priority |
|-------------|--------|----------|
| Hero Base    | ~112   | P0 (must have) |
| Ingredients  | 95     | P0 |
| Kitchen      | 28     | P0 |
| UI           | ~25    | P0 |
| VFX          | 9      | P1 (nice to have) |
| Hero Skins   | ~336   | P2 (post-launch) |
| **Total**    | **~605** | |

**MVP minimum** (P0 only): ~260 sprites
**Full launch**: ~605 sprites

---

## Art Style Recommendations

### Option A: Pixel Art (Fastest for solo dev)
- 16x16 or 32x32 base, upscaled 2-4x
- Tools: Aseprite, Piskel (free), Pixilart (free web)
- Estimated time: 2-3 weeks for MVP
- Examples: Overcooked pixel demakes, Cooking Mama pixel fan art

### Option B: Cartoon Vector (Most polished look)
- Vector-based with flat shading
- Tools: Illustrator, Inkscape (free), Affinity Designer
- Estimated time: 4-6 weeks for MVP
- Examples: Overcooked official style, Cooking Fever

### Option C: AI-Generated + Touch-up (Fastest to market)
- Generate with Midjourney/DALL-E using prompts in AI_ART_PROMPTS.md
- Clean up in Photoshop/GIMP (remove backgrounds, fix inconsistencies)
- Estimated time: 1-2 weeks for MVP
- Risk: Style consistency requires careful prompting

### Option D: Asset Store / Marketplace
- Purchase pre-made 2D cooking game assets
- Customize colors to match brand
- Fastest option but less unique

---

## Freelancer Brief (for Fiverr/Upwork)

> **Project**: 2D mobile cooking game (Overcooked-style, single player)
> **Art Style**: [Pixel Art / Cartoon / Your choice]
> **Deliverables**:
> - 8 hero characters with idle (4-frame), walk (8-frame), portrait, and interact sprites
> - 19 food ingredients in 5 states each (95 icons)
> - 10 kitchen equipment sprites
> - 6 kitchen themes (background + floor + counter variants)
> - UI button and icon set (~25 elements)
> - All assets as individual PNGs with transparency
> - Follow naming convention in this document
>
> **References**: Overcooked, Cooking Mama, Cooking Fever
> **Budget range**: $500-2000 depending on style and revisions
> **Timeline**: 2-4 weeks
