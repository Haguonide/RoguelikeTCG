# Prompts — Personnages RoguelikeTCG

## Référence personnage : Aciera

**Fiche :** Femme, ~74 ans. Cheveux blancs mi-longs en désordre. Masque noir sur les yeux. Cape bordeaux/prune. Armure partielle dorée sur les épaules/torse. Expression renfrognée, regard perçant. Pouvoir : magnétisme — rochers, métal, objets métalliques en lévitation autour d'elle. Posture d'autorité naturelle, légèrement menaçante. Tricote parfois entre deux combats.

---

## Style A — Clash Royale / Brawl Stars
*(Objectif : coller au langage du BGT et des icônes existantes — outline épais, aplats, sticker style)*

### Midjourney
```
elderly female superhero, 74 years old, short wild white hair, black eye mask, dark burgundy cape, golden shoulder armor, magnetism powers with floating rocks and metal debris, fierce determined expression, Clash Royale game art style, Brawl Stars character design, thick black outline, bold flat colors, simple cel shading, sticker style illustration, white background, full body, dynamic pose, 2D game card art --style raw --ar 1:1 --v 6.1
```

### Leonardo AI
```
elderly female superhero character, 74 years old, short wild white hair, black domino mask, dark burgundy flowing cape, golden partial armor on shoulders and chest, magnetism power with floating rocks and metal shards around her, fierce and slightly terrifying expression, strong authoritative stance, Clash Royale / Brawl Stars art style, thick black outline, flat bold colors with simple one-tone shading, sticker illustration style, white background, full body portrait, 2D mobile game card art
```
**Modèle Leonardo recommandé :** Phoenix 1.0 — Style preset : Illustration

---

## Style B — BD Franco-Belge moderne
*(Objectif : cohérence avec le ton burlesque — références Lastman, Spirou moderne, Franquin)*

### Midjourney
```
elderly female superhero, 74 years old, wild white hair, black eye mask, dark burgundy cape, golden armor pieces, magnetism powers with levitating rocks, grumpy determined expression, Franco-Belgian comic book art style, Lastman graphic novel style, Spirou modern illustration, ligne claire, expressive face, bold clean outlines, flat color fills with hard shadows, dynamic composition, white background, full body, european comic book character design --style raw --ar 1:1 --v 6.1
```

### Leonardo AI
```
elderly female superhero, 74 years old, short wild white hair, black domino mask, dark burgundy cape, golden shoulder armor, magnetism powers, floating rocks and metal shards, fierce grumpy expression, Franco-Belgian comic book style, Lastman / Spirou modern BD aesthetic, clean bold outlines, ligne claire technique, flat colors with a single hard shadow, expressive caricatural face, strong silhouette, white background, full body character illustration, european graphic novel art
```
**Modèle Leonardo recommandé :** Phoenix 1.0 — Style preset : Comic Book

---

## Style C — Scott Pilgrim (animé Netflix — Scott Pilgrim Takes Off)
*(Objectif : tester le style anime-cartoon hybride flat, palette limitée, énergie très lisible)*

### Midjourney
```
elderly female superhero, 74 years old, wild white hair, black eye mask, dark burgundy cape, golden armor, magnetism powers with floating rocks and metal debris, fierce expression, Scott Pilgrim Takes Off animation style, Netflix animated series art style, flat colors, clean anime-influenced linework, limited color palette, bold outlines, graphic and punchy composition, white background, full body, 2D animation character design --style raw --ar 1:1 --v 6.1
```

### Leonardo AI
```
elderly female superhero character, 74 years old, short wild white hair, black eye mask, dark burgundy flowing cape, golden partial armor, magnetism superpower, rocks and metal objects floating around her, grumpy fierce expression, Scott Pilgrim Takes Off Netflix animated series style, flat 2D animation aesthetic, clean sharp outlines, limited bold color palette, anime-influenced proportions without being chibi, punchy graphic composition, white background, full body character design
```
**Modèle Leonardo recommandé :** Phoenix 1.0 — Style preset : Anime

---

## Notes de cohérence inter-styles

- Maintenir systématiquement : cheveux blancs / masque noir / cape bordeaux / touche dorée
- Rochers/métal en lévitation = marqueur de pouvoir à conserver dans tous les tests
- Fond blanc dans tous les prompts (découpe propre pour intégration Unity)
- Tester full body ET buste (pour portrait combat)
- Si un style sort gagnant : reproduire Le Partenaire avec exactement le même prompt base

## Paramètres à ne pas changer entre les générations
- `white background` (jamais de fond texturé pour les persos, intégration Unity)
- `full body` ou `bust portrait` (préciser à chaque fois)
- Référence aux couleurs signature : `dark burgundy cape`, `golden armor`, `white hair`, `black mask`
