# IOChef - AI Art Generation Prompts
# Use these with Midjourney, DALL-E, Stable Diffusion, or similar tools.
# After generating, resize to the target resolution and save as PNG with transparency.

---

## STYLE GUIDE (prepend to all prompts)

**Base style prefix** (pick one depending on your art direction):

- **Pixel Art**: `pixel art, 2D sprite, 32-bit style, clean pixels, transparent background, game asset,`
- **Cartoon/Vector**: `2D cartoon, flat shading, vector style, clean lines, bold colors, transparent background, game asset,`
- **Hand-drawn**: `hand-drawn illustration, soft shading, storybook style, warm colors, transparent background, game asset,`

**Negative prompt** (for Stable Diffusion): `3D, realistic, photographic, blurry, low quality, watermark, text, frame, border`

---

## HEROES (128x128 or 256x256, then downscale)

### Chef Rush (Common - Red/Crimson)
```
[style prefix] cute chibi chef character, red chef uniform, white chef hat, determined expression, holding a spatula, simple pose, full body front view, warm red color palette
```

**Skins:**
- Default: `cute chibi chef, classic red chef uniform, tall white chef hat, spatula in hand`
- Casual Wear: `cute chibi character, casual red hoodie, baseball cap, holding a spatula, relaxed pose`
- Cyberpunk: `cute chibi chef, futuristic neon red chef suit, glowing visor, holographic spatula, cyberpunk style`

### Blaze (Epic - Orange/Fire)
```
[style prefix] cute chibi fire chef character, orange and red flame-themed uniform, fiery hair, confident smirk, holding a flaming pan, full body front view, fire color palette, warm glow
```

**Skins:**
- Default: `cute chibi fire chef, orange uniform with flame patterns, fiery orange hair`
- Fire Demon: `cute chibi demon chef, dark red skin, small horns, flame wings, holding fiery pan`
- Space Explorer: `cute chibi astronaut chef, orange space suit, helmet with flame decal, space-themed`
- Celebrity Chef: `cute chibi celebrity chef, gold-trimmed orange uniform, sunglasses, flashy hat`

### Calm Chef (Rare - Light Blue)
```
[style prefix] cute chibi zen chef character, light blue serene uniform, peaceful expression, closed eyes slight smile, holding a teacup, aura of calm, blue color palette
```

**Skins:**
- Default: `cute chibi zen chef, light blue uniform, peaceful expression, gentle blue aura`
- Zen Monk: `cute chibi monk chef, traditional robes, bald head, prayer beads, serene blue glow`
- Philosopher: `cute chibi philosopher, toga, laurel wreath, book in one hand, ladle in other`
- Sage: `cute chibi sage character, long flowing robes, crystal staff, mystical blue glow`

### Chill Master (Legendary - Ice Blue/Cyan)
```
[style prefix] cute chibi ice chef character, cyan and white frozen-themed uniform, ice crystal accents, cool expression, frost particles around hands, full body front view, icy blue palette
```

**Skins:**
- Default: `cute chibi ice chef, cyan uniform with frost patterns, icy blue hair, cold breath visible`
- Ice Mage: `cute chibi ice mage, crystalline robes, ice crown, frozen staff, snowflakes floating`
- Arctic Warrior: `cute chibi arctic warrior, fur-lined ice armor, ice axe, polar bear hood`
- Frozen Sage: `cute chibi frozen sage, translucent ice robes, floating ice shards, wise expression`

### Precision (Rare - Purple)
```
[style prefix] cute chibi precision chef character, purple and silver uniform, monocle, measuring tools on belt, focused calculating expression, full body front view, purple color palette
```

**Skins:**
- Default: `cute chibi precision chef, purple uniform with silver trim, monocle, measuring spoons on belt`
- Sniper Chef: `cute chibi sniper character, dark purple tactical gear, scope monocle, precise tools`
- Scientist: `cute chibi scientist chef, lab coat with purple accents, goggles, test tubes`
- Surgeon: `cute chibi surgeon, purple scrubs, surgical mask, precise scalpel-like knife`

### Momentum (Epic - Yellow/Gold)
```
[style prefix] cute chibi speed chef character, yellow and gold uniform with motion lines, dynamic running pose, speed trail effects, energetic expression, full body, gold color palette
```

**Skins:**
- Default: `cute chibi speed chef, yellow uniform with motion blur streaks, gold accents, energetic`
- Dancer: `cute chibi dancer chef, flowing gold outfit, graceful spinning pose, ribbon trail`
- Figure Skater: `cute chibi figure skater, gold skating outfit, ice trail, graceful pose`
- Parkour Runner: `cute chibi parkour runner, athletic yellow gear, mid-jump pose, speed lines`

### Alchemist (Legendary - Deep Purple)
```
[style prefix] cute chibi alchemist chef character, deep purple magical robes, glowing potion bottles on belt, mystical swirling aura, wizard hat with chef toque blend, full body front view
```

**Skins:**
- Default: `cute chibi alchemist chef, purple robes with arcane symbols, potion bottles, magical glow`
- Wizard: `cute chibi wizard chef, starry dark purple robes, crystal ball, magical sparkles`
- Chemist: `cute chibi chemist, lab coat with purple chemical stains, bubbling flasks, goggles`
- Time Traveler: `cute chibi time traveler, steampunk-purple outfit, clock gears, temporal glow`

### Iron Will (Epic - Silver/Steel)
```
[style prefix] cute chibi armored chef character, silver and steel knight-themed uniform, small shield, determined stoic expression, metallic sheen, full body front view, silver color palette
```

**Skins:**
- Default: `cute chibi armored chef, silver knight apron, small shield, determined expression`
- Knight: `cute chibi knight chef, full plate armor with chef hat crest, sword-shaped knife, shield`
- Samurai: `cute chibi samurai chef, traditional armor with cooking theme, katana-knife, noble pose`
- Viking: `cute chibi viking chef, horned helmet, fur cloak, battle axe shaped like cleaver`

---

## INGREDIENTS (64x64 items, transparent background)

### Base Prompt Pattern
```
[style prefix] [ingredient name], food item, simple clean design, [color] colored, [state description], isolated on transparent background, game item icon
```

### Examples by ingredient:

**Lettuce**: `game icon, fresh lettuce leaf, bright green, crispy edges, food item sprite`
**Tomato**: `game icon, ripe red tomato, round, glossy, food item sprite`
**Meat**: `game icon, raw beef patty, brown-red, circular, food item sprite`
**Bun**: `game icon, golden hamburger bun, sesame seeds on top, food item sprite`
**Cheese**: `game icon, yellow cheese slice with holes, triangular, food item sprite`
**Bread**: `game icon, bread slice, golden brown, toast-ready, food item sprite`
**Sausage**: `game icon, hot dog sausage, reddish brown, elongated, food item sprite`
**Dough**: `game icon, ball of pizza dough, pale beige, round, food item sprite`
**Sauce**: `game icon, red tomato sauce dollop, glossy, food item sprite`
**Fish**: `game icon, fresh fish fillet, pink-silver, food item sprite`
**Rice**: `game icon, bowl of white rice, fluffy, food item sprite`
**Seaweed**: `game icon, dark green nori seaweed sheet, food item sprite`
**Noodles**: `game icon, yellow noodles bundle, wavy, food item sprite`

### State Variations
For each ingredient, generate these states:
- **Raw**: `raw [ingredient], fresh, unprocessed, natural colors`
- **Chopped**: `chopped [ingredient], diced pieces, on cutting board pattern, knife marks`
- **Cooked**: `cooked [ingredient], golden brown, steam rising, warm glow`
- **Burned**: `burned [ingredient], blackened, charred, small smoke wisps, dark`
- **Plated**: `[ingredient] neatly placed on white plate, garnished, presented`

---

## KITCHEN OBJECTS (64x64 top-down or 3/4 view)

```
[style prefix] kitchen [object], top-down 3/4 perspective, clean design, game tile

Counter:        wooden kitchen counter surface, clean, empty, warm brown wood grain
CuttingBoard:   wooden cutting board, knife marks visible, rectangular, light wood
Cooktop:        gas stove burner, metal grate, 2 burner top-down view, dark metal
Sink:           stainless steel kitchen sink, faucet, water, clean blue-silver
TrashBin:       kitchen trash can, step pedal, gray metallic, small
PlatingStation: clean white serving plate station, organized, bright
ServePoint:     service window counter, green "ready" light, serving bell
IngredientShelf: wooden shelf with ingredient boxes, organized, rustic
Floor_Tile:     kitchen floor tile, checkered pattern, clean, neutral
Wall_Tile:      kitchen wall tile, white subway tile pattern, clean
```

---

## KITCHEN THEMES (256x256 backgrounds + 64x64 tile variants)

### Modern
```
[style prefix] modern minimalist kitchen background, white marble counters, stainless steel, clean lines, bright lighting, contemporary design, top-down game view
```

### Medieval
```
[style prefix] medieval castle kitchen background, stone walls, wooden counters, torch sconces, iron cookware, dark warm atmosphere, fantasy game style, top-down view
```

### Cyberpunk
```
[style prefix] cyberpunk neon kitchen background, dark purple atmosphere, neon pink and blue lights, holographic displays, futuristic cooking equipment, top-down game view
```

### Zen
```
[style prefix] zen japanese kitchen background, natural wood and bamboo, minimalist design, bonsai plant, paper lanterns, peaceful warm lighting, top-down game view
```

### Steampunk
```
[style prefix] steampunk kitchen background, brass pipes and gears, Victorian era equipment, copper pots, steam vents, warm amber lighting, mechanical aesthetic, top-down game view
```

### Enchanted
```
[style prefix] enchanted magical forest kitchen background, tree trunk counters, mushroom stools, firefly lights, crystal formations, magical sparkles, fairy tale aesthetic, top-down game view
```

---

## UI ELEMENTS

### Currency Icons
```
[style prefix] game currency coin icon, [gold coin with dollar sign / blue gem crystal], shiny, clean, small icon, game UI element
```

### Star Rating
```
[style prefix] golden star icon, 5-pointed, shiny metallic gold, game rating star, clean
[style prefix] empty star outline, gray, 5-pointed, dim, game rating star placeholder
```

### Buttons
```
[style prefix] game UI button, [green/blue/orange/gray] rounded rectangle, glossy, clean, [PLAY/RETRY/NEXT/MENU] text, mobile game style
```

---

## VFX / PARTICLES (32x32 or 16x16 with transparency)

```
[style prefix] small particle sprite sheet, [fire/ice/magic/sparkle/smoke] particle, glowing, soft edges, transparent background

flame_particle:    small orange-red fire particle, glowing, warm
ice_particle:      small cyan ice crystal particle, sparkling, cold
magic_particle:    small purple magic sparkle particle, arcane glow
sparkle:           small golden sparkle, starburst, bright
smoke:             small gray smoke puff, soft, wispy
celebration:       confetti burst, colorful, festive
combo_flash:       bright yellow flash burst, energy, impact
shield:            small blue transparent shield bubble, protective glow
trail:             speed trail streak, yellow-gold, motion blur
```

---

## TIPS FOR BEST RESULTS

1. **Consistency**: Generate all heroes in one session with the same style prefix
2. **Transparency**: Most AI tools don't do true transparency. Use remove.bg or rembg to clean backgrounds
3. **Batch sizing**: Generate at 512x512 or 1024x1024, then downscale to target resolution
4. **Sprite sheets**: For walk animations, generate a 4-frame sequence and split into individual frames
5. **Color matching**: Use the hex colors from the placeholder sprites as reference:
   - Chef Rush: #C85050
   - Blaze: #DC781E
   - Calm Chef: #50A0C8
   - Chill Master: #64C8E6
   - Precision: #B450B4
   - Momentum: #DCC832
   - Alchemist: #8250C8
   - Iron Will: #78788C
