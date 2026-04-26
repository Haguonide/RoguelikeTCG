# Prompts — Direction Artistique (Tests batteries 2026-04-26)

## Contexte de la session DA

**Objectif :** trouver la direction artistique du jeu via batteries de tests sur Leonardo AI.
**Personnage de test :** Léonard de Vinci — inventeur alcoolique chaotique.
**Pose cible :** action pose — flacon de vin dans une main, prototype raté qui fume/explose dans l'autre.
**Format généré :** 1024×1024 (downscaler à 256×256 pour juger le rendu final).
**Contraintes :**
- Outil principal : Leonardo AI (cohérence personnage disponible)
- Post-processing : Upscayl + Photopea (usage simple)
- Format carte : 256×256, découpe en 3 layers (cadre / illustration / stats UI)
- Solo dev

---

## Critères d'évaluation (à appliquer sur chaque résultat)

| Critère | Question |
|---|---|
| **Silhouette** | Est-ce qu'on comprend la pose en 1 seconde à 256×256 ? |
| **Lisibilité des couleurs** | La palette ressort-elle clairement ? |
| **Ton** | Ça fait rire/sourire ? Ça a du caractère ? |
| **Cohérence promptable** | Si tu regénères, tu retombes dans le même style ? |
| **Effort post-processing** | Photopea simple suffit ou il faut ramer ? |

**Objectif :** 5 images par direction (20 total), trouver 1 winner par direction pour comparer.

---

## Direction A — Caricature sticker

**Modèle Leonardo :** `Leonardo Anime XL` ou `Leonardo Diffusion XL`
**Preset style :** `Illustration` ou `Comic Book`

```
cartoon caricature of Leonardo da Vinci as a chaotic drunken inventor,
big head small body, exaggerated features, bold black outlines,
flat vibrant colors, blue and copper color palette,
action pose holding a wine flask in one hand and a smoking broken
flying machine in the other, laughing maniacally,
sticker art style, white background, game card illustration, 2D flat

Negative :
realistic, photo, 3D render, blurry, detailed background, dark, grim,
watermark, text, signature
```

**Ce qu'on évalue :** silhouette lisible à 256×256, trait qui tient bien, proportion exagérée assumée.

---

## Direction B — Graphic novel sombre (Mignola)

**Modèle Leonardo :** `AlbedoBase XL` ou `Leonardo Diffusion XL`
**Preset style :** `Comic Book`

```
Mike Mignola art style, Hellboy comic aesthetic, Leonardo da Vinci
as an alcoholic inventor, dramatic action pose, holding a wine flask
and a broken contraption with smoke,
heavy black shadows, bold ink lines, limited color palette
blue and copper accent colors on dark background,
graphic novel illustration, game card art, 2D

Negative :
realistic, photo, 3D, bright colors, cheerful, cute, anime,
watermark, text
```

**Ce qu'on évalue :** contraste lisible à 256×256, ambiance qui colle au ton sombre/absurde.

---

## Direction C — Flat cartoon moderne (Dustborn)

**Modèle Leonardo :** `Leonardo Diffusion XL`
**Preset style :** `Illustration`

```
modern flat cartoon character design, graphic novel illustration style,
Leonardo da Vinci reimagined as a chaotic inventor,
action pose holding wine flask and smoking invention,
clean bold outlines, vibrant flat colors, blue copper palette,
stylized proportions, expressive face, white background,
2D game card art, Dustborn game art style

Negative :
realistic, photo, 3D, complex background, too many details,
watermark, text
```

**Ce qu'on évalue :** assez distinct et personnel, ou trop générique ?

---

## Direction Mix A+B — Festif + Heavy contrast (piste à explorer en priorité)

```
caricature cartoon character, Leonardo da Vinci as a drunken
chaotic inventor, bold black ink outlines, vibrant blue and copper
flat colors, high contrast black shadows on colored areas,
action pose laughing while holding wine flask and broken flying machine,
exaggerated big head small body, sticker art style, dark dramatic
background with light spotlight on character,
game card illustration, 2D flat shading with deep shadows

Negative :
realistic, photo, 3D, blurry, watermark, text, anime, too dark overall
```

**Ce qu'on évalue :** est-ce que le mélange festif + contraste fort crée quelque chose d'unique ?

---

## Notes workflow Leonardo AI

- Générer 5 images par direction, noter les winners avec le score des critères ci-dessus
- Pour la cohérence personnage sur Leonardo : utiliser la feature **Character Reference** avec le winner retenu
- Upscayl si nécessaire avant d'importer dans Unity
- Découpe dans Photopea : layer cadre / layer illustration / stats posées en UI Unity
- Si résultat trop sage : ajouter `"exaggerated cartoon proportions, comedy character design, absurdist humor"`
- Si couleurs trop sombres : ajouter `"bright warm color palette, warm lighting, cheerful muted tones"`
- Si expression trop sobre : ajouter `"extremely expressive face, manic energy, wide eyes, raised eyebrow"`
