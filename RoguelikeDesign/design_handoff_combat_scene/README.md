# Handoff: Roguelike Combat Scene

## Overview

A turn-based roguelike combat screen blending deck-builder mechanics (Slay-the-Spire-style hand of cards) with grid-placement tactics (3×3 board). Visual identity is **flat cartoon comic-book** with a corporate / superhero satire tone: the "hero" is a bored ex-villain office worker, enemies are corporate agents. Copy is in **French**.

The scene shows a single combat encounter:

- **Top bar**: gold, relics, chapter / round / turn indicator
- **Left & right portraits** with HP bars and status chips (shield, poison)
- **Center CRT screen**: shows 3 placement-pattern objectives ("complete this shape on the grid for a score reward")
- **Two flip-clock scoreboards** flanking the CRT (player / enemy score)
- **3×3 grid arena** in the middle with rows colored by ownership (top = enemy, middle = contested, bottom = player)
- **Hand of cards** at the bottom (drag-and-drop onto the grid)
- **Mana orb + deck + discard piles** bottom-left, **End Turn button** bottom-right
- **Comic-book superhero background**: gradient sky, halftone dots, sunburst action lines, "POW" star bursts, flying caped silhouette, city skyline.

## About the Design Files

The files in `source/` are **design references created in HTML/CSS/React** — a fixed-size 1920×1080 prototype demonstrating intended look, layout, and core interaction (drag-card-onto-grid, end-turn, score patterns). They are **not production code**.

The task is to **recreate this design in your target codebase** using its existing patterns, component library, and styling system. If no environment exists yet, pick the framework best suited to the project (React + Tailwind, Vue, SwiftUI, Unity UI, etc.) and implement there. Keep the visual fidelity; adapt the architecture.

## Fidelity

**High-fidelity (hifi).** Final colors, typography, spacing, shadows, and core interactions are all locked. Reproduce pixel-faithfully. The drag-drop, pattern-scoring, and end-turn flows are working prototypes you can reference for behavior.

## Stage / Canvas

- Fixed authoring size: **1920 × 1080**
- The whole stage is wrapped in `StageScaler` that applies `transform: scale(min(vw/1920, vh/1080))` to letterbox into any viewport. Recreate this scaling in your target environment, OR redesign responsively if your platform has a different fixed aspect (mobile portrait will need a full layout rework — ask the designer).
- Background of the page outside the stage: `#050505`. The stage itself has `border-radius: 4px` and a `0 30px 80px rgba(0,0,0,0.6)` drop shadow.

## Design Tokens

### Colors

```
--ink           #0c1a14   (near-black, used for borders, text on light bg)
--ink-2         #16291f   (slightly lifted ink for dark fills)
--bone          #f6efd9   (warm off-white, primary card surface)
--bone-2        #ead7a7   (slightly darker bone, card stat row)

--player        #ff5a3c   (warm coral — player accent)
--player-2      #c83a22   (deeper coral — shadow / 2nd stop)
--enemy         #6c4bff   (villain violet — enemy accent)
--enemy-2       #4a2dc9   (deeper violet)

--gold          #ffc83a   (CTA gold — End Turn, gold pill, droppable)
--gold-2        #d18d12   (gold shadow)

--mana          #34c1ff   (mana cyan)
--mana-2        #0f74b3   (mana shadow)

--crt           #00ff66   (CRT phosphor green)
--crt-dim       #00702c   (CRT off-pixel)
```

### Typography

Loaded from Google Fonts: `Bungee`, `Bowlby One`, `Fira Code`, `Inter` (weights 500/700/800/900).

| Token | Family | Use |
| --- | --- | --- |
| Display heading | **Bungee**, sans-serif | Tags, labels, button text — uppercase, letter-spaced 1–4px |
| Numerical / impact | **Bowlby One**, sans-serif | Big numbers (mana, score flips, gold pill) |
| Mono / chrome | **Fira Code**, monospace | CRT bezel text, cell coordinates, debug |
| UI body | **Inter** 700/800/900 | Stats, secondary UI |

### Spacing & Radii

Spacing is hand-tuned per component (no formal scale). Common radii:

- Pills / chips: `999px` (full pill) or `6px`
- Cards / units: `14px` (cards), `12px` (cells), `18px` (grid container)
- Mana orb: `50%` (circle)
- Stage: `4px`

### Borders & Shadows (signature look)

The flat-cartoon look depends on **thick black outlines + offset solid shadow**:

- Most elements: `border: 3–6px solid #000`
- "Hard" drop shadow: `box-shadow: 0 6px 0 #000` (no blur, vertical offset only) — sometimes stacked with a soft blurred shadow underneath, e.g. `0 6px 0 #000, 0 14px 30px rgba(0,0,0,.5)`
- Inset highlight: `inset 0 -4px 0 rgba(0,0,0,.25)` for 3D "ridge"
- Buttons press down: `transform: translateY(6px)` + reduce shadow on `:active`

Maintain these exactly — they're what makes the design feel cohesive.

## Screens / Views

There is one screen: **Combat Scene**. Sub-regions:

### 1. Background (`background.jsx`)

Comic-book superhero scene, **all layered absolutely inside the stage**:

1. **Gradient sky** (`.sh-sky`): radial sun-glow at top-center over a vertical gradient `#ffd166 → #ff7a3c → #c83a72 → #4b2a8c → #1a1340 → #08081f`.
2. **Halftone dot overlay** (`.sh-halftone`): two layered radial-gradient dot patterns, multiply blend, masked to fade out by 75% height.
3. **Action-line sunburst**: 28-segment SVG radiating from `cx=960, cy=120`, alternating `rgba(255,220,90,0.18)` / `rgba(255,255,255,0.05)` triangles.
4. **Two "POW" star bursts** (SVG polygon, 20-point stars): yellow `#ffd24a` upper-left at `top:18% left:6%, rot:-12°`; orange `#ff5a3c` upper-right at `top:22% right:7%, rot:15°`. Both have black stroke + drop-shadow.
5. **Flying caped hero silhouette** at top-center: red cape (`#c83a22`), navy body (`#1a2848`), arm pointing forward. ~200×130px.
6. **City skyline** in two layers:
   - **Far skyscrapers** (5–6 buildings, fills `#1a2848` / `#15223e`): smaller, `bottom:140px`
   - **Foreground skyscrapers** (taller, fills `#08111f` / `#0a1426`, `bottom:0`)
   - Each building has a grid of pseudo-randomly-lit windows (`#ffd86b` lit, `#1a2540` dark) — uses a deterministic `(r*7 + c*13) % 5 < 2` formula for variety
   - Two **water towers** and two **antennas** with red blinker tip on tall buildings
7. **Smog gradient** at base (`.sh-fog`): `linear-gradient(180deg, transparent, rgba(8,12,30,0.92))`, 240px tall.

### 2. Top bar (`.topbar`)

- Centered pill at `top: 18px`, ink background, 4px black border, 999px radius, hard shadow stack `0 6px 0 #000, 0 12px 0 rgba(0,0,0,.35)`.
- Contains, left to right:
  - **Gold pill**: yellow `#ffc83a` rounded pill with a coin glyph (`$`) and the gold count.
  - **4 Relic squares** (`48×48px`, 12px radius, 3px border): tinted backgrounds (red/blue/yellow/purple), each holds an emoji glyph. Hover: lifts 2px and rotates -3°.
  - **Turn pill**: `CH.03 — RD {round} — TR {turn}` in Bungee 16px on dark.

### 3. CRT screen (`.crt`)

- Centered at `top: 124px`, `540×170px`.
- **Shell**: dark wood color `#2c2418`, 6px black border, 16px radius, inset padding for the screen hole. Two brown knobs `#5a4a30` extruding from the right side.
- **Bezel top**: tiny mono text `<red blinker>NUMBER ONE INC. // OBJ TRANSMIT` left, `CH.03 • R01 • T01` right.
- **Screen**: radial-gradient dark green `#022b13 → #000`, with two pseudo-element overlays:
  - `::before` — horizontal scanlines (repeating gradient 2px on / 2px off, green @ 6% opacity)
  - `::after` — vignette (radial transparent → black at edges)
- **3 pattern slots** spread across, each shows: pattern name (Fira Code green), 3×3 mini-grid (lit cells glow `#00ff66` with white inset), reward `+N` in white Bowlby. When a pattern scores, animate `scale` 1→1.1→1 over 0.8s and brighten.
- **Bezel foot**: tiny mono text `SIGNAL_OK · KPI_BRIEF v2.7.1 · SCORE_AT_EOR`.

### 4. Scoreboards (`.scorebar.left` / `.right`)

Mechanical split-flap clock style, flanking the CRT.

- Position: `top: 130px; left: 290px` and `right: 290px`.
- **Label pill**: `JOUEUR` / `ENNEMI` in Bungee 12px on ink, 3px border, 6px radius.
- **Flip row**: dark `#1a1a1a` housing, 4px black border, holds two `flip` digits.
- **Flip digit**: `42×60px`, black background, Bowlby 42px digit, color-tinted by side (player coral `#ff8b67`, enemy lavender `#b6a1ff`) with matching glow `text-shadow`. A horizontal divider line at 50% via `::after`.
- **Flip animation**: when the value changes, the digit translates Y 0→-100% over 0.175s (`flipChange` keyframe), then snaps to new digit and translates 100%→0 over 0.175s. Class `.flipping` toggles around it. Implemented in `scorebar.jsx` with refs + `setTimeout`.

### 5. Portraits (`.portrait-block.left` / `.right`)

- `top: 130px`, 230px wide. Left at `left: 36px`, right at `right: 36px`.
- **Round portrait**: `180×180px` circle, 6px black border, hard shadow `0 8px 0 #000` + soft `0 14px 30px rgba(0,0,0,.5)`.
  - Player portrait gradient bg: `#ffb59a → #ff7a55`.
  - Enemy portrait gradient bg: `#b8a4ff → #7a5cff`.
  - Inside: an inline SVG of the character (see `portraits.jsx`). Player is a masked caped hero with coffee mug; enemy is a slick-haired suit with sunglasses and a `№1` badge.
- **Name tag**: ink chip with Bungee 14px name (e.g. `MIDNIGHT MARV` / `DIRECTOR ZERO`).
- **Role tag**: 11px Inter 700 uppercase, letter-spaced 2px, bone-2 color (`EX-VILAIN, CDI` / `HEROISME PRIVE`).
- **HP bar**: 210×wide, ink fill, 4px black border, 12px radius. Inside is a 18px-tall well with a colored fill (player = orange→red gradient, enemy = lavender→violet) and an HP-num label `28/32`.
- **Status row**: chips for shield (blue `#5b7cff`, label `🛡 3`) and poison (green `#6cdb8b`, label `🌿 2`).

### 6. Grid arena (`.arena`)

- Centered at `top: 290px` (M size), `760×560px`.
- **Grid container**: 6px black border, 18px radius, dark gradient interior, hard shadow + inset glow.
- **Cells**: 3×3, gap 12px, 18px padding around. Each cell is dashed-bordered `rgba(246,239,217,0.28)`, 12px radius.
  - **Row tints**: top row (enemy) `rgba(108,75,255,.07)`, bottom row (player) `rgba(255,90,60,.07)`, middle row `rgba(255,200,58,.05)`.
  - **Cell label**: `R{row}C{col}` in Fira Code 10px, top-left, 35% opacity. Toggleable via Tweaks.
  - **Drop states**:
    - `.droppable` (valid drop target while dragging) — gold fill, solid gold border, scale 1.02, glow.
    - `.invalid` (red wash) when hovering over a cell that's not droppable.
- **Side tags** `ENNEMI` / `JOUEUR` outside the grid, vertical-rl Bungee 16px, off-coral / off-lavender colored, with thick black text-shadow ring.

### 7. Unit (placed card on grid) — `cards.jsx` `Unit`

- Fills 92% of the cell, bone background, 4px border, 14px radius.
- **CD badge** (top-left): `26×26` gold square, Bowlby 14, the cooldown number.
- **HP drops** (top-right): up to 5 red teardrop-shaped icons (CSS `clip-path` heart-drop), then `×N` text if more.
- **Art zone**: takes up the middle, gradient by team (coral for player, violet for enemy), giant emoji glyph at 48px.
- **Atk arrows**: a 3×3 mini-pattern of directional arrows (↑↓←→↖↗↙↘) showing where the unit can attack. Patterns are predefined: `cross`, `forward`, `diag`, `all`, `line`. Arrows are white with thick black text-shadow ring.
- **Name strip** at bottom: ink bg, Bungee 11px, top border 3px black.
- Player units have a coral team-glow shadow; enemy units have a violet one.

### 8. Hand of cards (`.hand`) — `cards.jsx` `Card`

- Fixed at `bottom: 12px`, centered, 14px gap, perspective 1200px.
- Each **card**: `150×200px`, bone bg, 5px border, 14px radius, hard shadow + soft drop.
- **Cost orb** (top-left, overhanging by 8px): `38px` circle, mana-cyan, Bowlby 20 white, with cyan inset highlight.
- **Art**: top 60%, gradient by tone (`'' = coral`, `purple`, `green`, `blue`, `gold`), 56px emoji centered.
- **Keyword chip** (optional, e.g. `STAGIAIRE`, `BURNOUT`): gold rectangle, 9px Bungee, just above name strip.
- **Name strip**: ink bg, Bungee 12px.
- **Stats row**: bone-2 bg, 3 mini-stats (HP/CD/ATK) — each is a small colored square icon + value.
- **States**:
  - `:hover` — translateY(-30) scale 1.06, beefier shadow.
  - `.dragging` — opacity 0.25, slight shrink (real card is detached during native drag).
  - `.unaffordable` — desaturated, "NO MANA" stamp at -12° rotation, not draggable.

### 9. Resources (bottom-left, `.resources`)

- **Mana orb**: `110px` radial-gradient blue ball, 5px border, hard + glow shadow. Big `M/Mmax` number in Bowlby 38, blue inset shadow on text. Below it a small ink chip says `MANA`.
- **Deck pile** & **Discard pile**: `90×110` rectangles with stacked ghost-cards drawn via offset box-shadows (deck stacks back-left in dark green-ink; discard stacks back-right in burgundy `#3a1a1a`). Each shows a glyph (`🂠` / `🗑`), a label, and a count chip.

### 10. End Turn (bottom-right, `.end-turn`)

- Big yellow gold button, 5px black border, 16px radius, Bungee 22px.
- Stack shadow `0 8px 0 var(--gold-2), 0 14px 0 #000, 0 20px 30px rgba(0,0,0,.45)` for chunky lift.
- Two-line label: small caption `TON TOUR` over big `FIN DE TOUR`.
- **Enemy-turn state** (`.enemy-turn`): switches to violet `#6c4bff`, label becomes `ATTENDS... / ENNEMI JOUE`, pointer-events none.
- `:hover` lifts 2px; `:active` presses 6px down.

### 11. Toast (`.toast`)

Centered at `top: 320px`, ink chip, Bungee 22px, 4px border, 12px radius, glow shadow. Slides in from -20px with 0→1 fade over 0.4s. Auto-dismisses after 1.4s.

## Interactions & Behavior

State is centralized in `app.jsx`. Reproduce these flows:

### Drag a card from hand to a grid cell
1. `dragstart` on a `.card` → `setDragging(card)`. Card visually shrinks/fades.
2. `dragover` on a `.cell` → check `validCellForCard(idx)`:
   - Cell must be empty.
   - Index must be `>= 3` (player can place in middle row & bottom row).
   - If valid, call `e.preventDefault()` and set `hoverCell` (the cell turns gold). Otherwise highlight red.
3. `drop` on valid cell → if `mana >= card.cost`:
   - Place a unit derived from the card into that cell with `owner: 'player'`.
   - Subtract cost from mana, remove card from hand, increment discard.
   - Show toast `${cardName} DEPLOYE`.
   - Run `checkPatterns(grid)` to see if this completed any objective.
4. Cleanup: `dragend` clears `dragging` and `hoverCell`.

### Pattern scoring
For each pattern in `patterns`, every "1" cell in its 3×3 mask must contain a player-owned unit. If yes, after a 300ms delay add the reward to `pScore`, mark pattern `scored: true`, and toast `MOTIF SCORE +N`.

### End turn
Click `.end-turn`:
1. `setActiveSide('enemy')`, toast `TOUR ENNEMI`. Button switches to violet idle state.
2. After 700ms, enemy AI fills the first empty cell in the top row with an `AGENT №1` unit.
3. After 1900ms total, `setActiveSide('player')`, increment `turn`, increment `maxMana` up to 10 cap, refill `mana = maxMana`, draw a card (random from `STARTER_DECK` if hand < 7), decrement deckCount, toast `TON TOUR`.

### Initial deck (hand-coded for the demo)

```js
const STARTER_DECK = [
  { id: 'c1', name: 'INTERN HEROIQUE',  cost: 1, hp: 2, cd: 1, atk: 'forward', tone: '',       glyph: '🦸', keyword: 'STAGIAIRE' },
  { id: 'c2', name: 'EX-VILAIN PROBA',   cost: 2, hp: 3, cd: 2, atk: 'cross',   tone: 'purple', glyph: '🦹', keyword: 'CONDITIONNELLE' },
  { id: 'c3', name: 'RETRAITE FORCE',    cost: 3, hp: 5, cd: 1, atk: 'all',     tone: 'gold',   glyph: '🧓', keyword: 'GROGNON' },
  { id: 'c4', name: 'CDI-MAN',           cost: 2, hp: 4, cd: 2, atk: 'line',    tone: 'blue',   glyph: '👔' },
  { id: 'c5', name: 'CONSULTANT',        cost: 4, hp: 6, cd: 3, atk: 'diag',    tone: 'green',  glyph: '💼', keyword: 'BURNOUT' },
];
```

### Initial patterns

```js
const INITIAL_PATTERNS = [
  { name: 'COLONNE PURE', pattern: [[1,0,0],[1,0,0],[1,0,0]], reward: 4 },
  { name: 'TRIANGLE',     pattern: [[0,1,0],[1,0,1],[0,0,0]], reward: 6 },
  { name: 'DIAGONALE',    pattern: [[1,0,0],[0,1,0],[0,0,1]], reward: 5 },
];
```

## State Management

State variables held at the App root (use whatever your codebase prefers — React useState, Pinia, Redux, MobX, etc.):

```
turn, round, activeSide ('player' | 'enemy')
pHp, eHp, pShield  (max HP is fixed in the demo: 32 player, 36 enemy)
mana, maxMana (initial 3 / 3, cap 10), gold (42)
deckCount (18), discardCount (4)
pScore, eScore, patterns[]
hand[] : Card objects { uid, name, cost, hp, cd, atk, tone, glyph, keyword? }
grid[9] : (Unit | null)[]   Unit = { uid, name, hp, cd, atk, glyph, owner, tone }
dragging, hoverCell, toast
```

Tweaks panel state (UI-only, non-canonical for production but documents what's customizable):

```
playerColor, enemyColor, goldColor (hex)
showCellLabels (bool), showGuides (bool)
gridSize ('S' | 'M' | 'L')   // S 640×480 top:320, M 760×560 top:290, L 880×640 top:250
portraitsSide ('side')       // currently only one mode used
```

## Animations & Transitions

- **Cell drop highlight**: 0.15s `background`, `border-color`, `transform` ease.
- **Card hover lift**: 0.18s ease.
- **HP fill width**: 0.4s ease (when HP changes).
- **CRT light blink**: 1.6s infinite, 50% opacity dip.
- **Pattern scored pulse**: 0.8s scale 1 → 1.1 → 1.
- **Score flip**: see scoreboard section above; 0.35s total per digit.
- **Toast**: 0.4s slide+fade in; auto-dismiss 1.4s.
- **End-turn press**: instant `transform: translateY(6px)` on `:active` (no transition — feels chunky).

## Assets

No external images. Everything is:

- **Inline SVG** drawn in JSX (portraits, background skyscrapers / hero / POW bursts / sunburst).
- **Emoji glyphs** for unit/card art (🦸🦹🧓👔💼🤵 etc.) — these will render with the platform's emoji font; if you need consistency across platforms, swap to Twemoji or commission illustrations.
- **Google Fonts** (Bungee, Bowlby One, Fira Code, Inter) loaded via `@import` in `styles.css`.

If your codebase has an icon system, replace emoji with named icons. If your team commissions character art, the portraits are the obvious upgrade target.

## Files

In `source/`:

| File | Contents |
| --- | --- |
| `Combat Scene.html` | Document shell — loads React 18 UMD, Babel standalone, then each component file. |
| `styles.css` | All styling, design tokens at top in `:root`. **Read this first** for the visual system. |
| `app.jsx` | Root `App` component, all game state, `StageScaler`, tweaks defaults. |
| `background.jsx` | Superhero comic-book background (sky, halftone, sunburst, POW bursts, skyline, flying hero). Exports both `SuperheroBackground` and a legacy `ForestBackground` alias. |
| `portraits.jsx` | `PortraitBlock` + `HPBar` + the inline-SVG `PortraitArt` (player/enemy). |
| `crt-pattern.jsx` | `CRTPatternDisplay` — retro CRT KPI screen with 3 pattern slots. |
| `scorebar.jsx` | `ScoreBar` + `FlipDigit` — split-flap mechanical scoreboards. |
| `cards.jsx` | `Card` (in-hand) + `Unit` (on-grid) + `AtkArrows` + `ATK_PATTERNS` map. |
| `tweaks-panel.jsx` | Designer's tweak panel (skip in production, but the controls document what's intended to be themable). |

In `screenshots/`: a reference render of the final scene at 1920×1080.

## Implementation Notes / Gotchas

- **Black borders are sacred.** Don't soften them, don't replace with subtle 1px lines — the entire visual language is built on chunky 3–6px black outlines + offset solid shadow. Removing them collapses the cartoon look.
- **No animations on hover for the grid cells** beyond what's listed — the cells should feel like a static board, not a web app.
- **Drag and drop** in the prototype uses native HTML5 DnD. For React Native / SwiftUI / etc. you'll need a gesture-based equivalent (long-press to lift, drag, drop) — pick whatever your stack idiomatically uses.
- **Stage transform-scale** can blur text on some browsers; if your target supports CSS `image-rendering: pixelated` or font hinting flags it's worth tuning.
- **Copy is in French.** Keep it in French unless asked.
- **Game logic in the demo is intentionally minimal** (placeholder enemy AI, fake deck draws). Use it as visual + interaction reference, not as a balance/design spec.
