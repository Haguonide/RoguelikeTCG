# Prompts — Direction Artistique (Tests batteries 2026-04-26)

---

## Modèles Leonardo AI disponibles (snapshot 2026-04-27)

> Les anciens modèles (Leonardo Anime XL, AlbedoBase XL, Leonardo Diffusion XL) ne sont plus disponibles.

### Featured
| Modèle | Refs supportées | Notes |
|---|---|---|
| Auto | — | Sélectionne automatiquement le meilleur modèle |
| GPT Image 2 | Image Ref | Composition forte, direction créative |
| Nano Banana 2 | Image Ref | Génération rapide, plus de détails |
| Seedream 4.5 | Image Ref | Posters, logos, texte |
| Lucid Origin | Style Ref, Content Ref | ✅ Unlimited — adhérence prompt excellente, rendu HD |
| FLUX.2 Pro | Image Guidance | Haute fidélité |
| GPT Image-1.5 | Image Ref | Contrôle image et détails |

### Other Models
| Modèle | Refs supportées | Notes |
|---|---|---|
| Nano Banana Pro | Image Ref | Gemini 3 Pro, infographies |
| Seedream 4.0 | — | Ultra haute qualité |
| Nano Banana | Image Ref | Edits précis, visuels cohérents |
| Lucid Realism | Style Ref, Content Ref | ✅ Unlimited — cinématique, photos |
| Ideogram 3.0 | — | Génération contrôlée précise |
| GPT Image-1 | Image Ref | OpenAI, état de l'art |
| FLUX.1 Kontext Max | — | Kontext amélioré, max qualité |
| FLUX.1 Kontext | Image Ref | Précis, édition contrôlée |
| FLUX Dev | Style Ref, Content Ref, Elements | ✅ Unlimited — flexible, détaillé |
| FLUX Schnell | Style Ref, Content Ref | ✅ Unlimited — rapide |
| **Phoenix 1.0** | Image to Image, Style Ref, Content Ref, **Character Ref** | ✅ Unlimited — **MEILLEUR pour cohérence personnage** |
| Phoenix 0.9 | Image to Image, Style Ref, Content Ref, Character Ref | ✅ Unlimited — version preview |

### Recommandations pour ce projet
- **Illustrations de cartes (personnages)** → `Phoenix 1.0` (seul à avoir Character Ref = cohérence cross-cartes)
- **Tests de style/direction** → `Lucid Origin` ou `FLUX Dev`
- **Génération rapide** → `FLUX Schnell`

---

## Contexte DA

**Univers :** Super-héros burlesque. 2 teams jouables (Programme R, Les Éternels) + 2 teams ennemies.
**Format généré :** 1024×1024 (downscaler à 256×256 pour juger le rendu final).
**Modèle :** `Phoenix 1.0` — Character Ref pour cohérence cross-cartes.
**Post-processing :** Upscayl + Photopea. Découpe : layer cadre / layer illustration / stats UI.

---

## Directives de style communes (à inclure dans tous les prompts)

```
thick bold uniform outlines, flat cel shading,
bright muted warm colors, expressive face,
realistic head size, not chibi,
instantly readable at small sizes,
2D game card character art, flat cartoon illustration style,
full upper body visible, simple warm background,
single character, centered composition
```

**Negative commun :**
```
realistic, photo, 3D render, blurry, complex background,
watermark, text, signature, anime, chibi
```

---

## Critères d'évaluation (à appliquer sur chaque résultat)

| Critère | Question |
|---|---|
| **Silhouette** | Est-ce qu'on comprend la pose en 1 seconde à 256×256 ? |
| **Lisibilité des couleurs** | La palette ressort-elle clairement ? |
| **Ton** | Ça fait rire/sourire ? Ça a du caractère ? |
| **Cohérence promptable** | Si tu regénères, tu retombes dans le même style ? |
| **Effort post-processing** | Photopea simple suffit ou il faut ramer ? |

---

## LES ÉTERNELS

**Palette équipe :** or chaud + argent (golden age + magnétisme métal)
**Costume :** combinaisons super-héros années 60-70, légèrement passées mais portées avec dignité
**Ton visuel :** usé mais imposant, autorité tranquille

---

### Aciera — Capitaine *(Magnétisme, assume son âge)*

**Signature visuelle :** tricote calmement pendant que des objets métalliques orbitent autour d'elle.

**Modèle :** `FLUX Dev` — Platform Element : `Dynamic`

```
elderly woman superhero, 74 years old, broad-shouldered sturdy build,
short silver-white hair pulled tight, deep wrinkles, piercing cold eyes,
absolutely calm expression, slightly intimidating, commanding presence,
gold and silver vintage superhero suit, short tattered cape,
standing upright, one arm raised with palm facing upward,
several steel cubes levitating and spinning slowly above her open palm,
crackling blue-white magnetic energy connecting her hand to the cubes,
other arm in a natural complementary position, hand resting on hip,
confident authoritative stance, weight evenly planted,
strong dramatic rim lighting from behind, deep shadows across face,
energy glow as single light source illuminating face from below,
character fills the frame, extreme close composition, cut at upper thigh,
abstract background of magnetic field lines and faint metal particles,
deep blue and black atmospheric background, gold energy particles,
Marvel Snap card illustration style, dynamic superhero digital painting,
painterly brushwork, high contrast, vibrant saturated colors,
bold comic book colors, cinematic dramatic lighting, volumetric light rays,
detailed digital art, epic card game illustration

Negative :
realistic photo, 3D render, watermark, text, signature,
anime, chibi, frail, gentle, grandmotherly, smiling warmly,
flat cel shading, plain background, low contrast, muted colors,
full body shot, too much empty space, static pose, knitting, yarn, needles
```

**Ajustements si besoin :**
- Cubes pas lisibles → ajouter `"clearly visible sharp-edged metallic cubes, geometric shapes"`
- Fond trop chargé → `"pure black background, single energy source"`
- Glow trop fort → `"subtle magnetic aura, soft blue glow"`
- Pose trop statique → ajouter `"slight dynamic lean, cape flowing"`
- Trop sombre → ajouter `"strong warm gold accent light on costume"`

---

## PROGRAMME R

*(prompts à venir)*

---

## Notes workflow Leonardo AI

- Winner retenu → uploader comme **Character Reference** dans Phoenix 1.0 pour toutes les cartes de ce perso
- Upscayl avant d'importer dans Unity si besoin
- Si couleurs trop sombres : ajouter `"bright warm color palette, warm lighting, cheerful muted tones"`
- Si proportions trop chibi malgré le negative : ajouter `"adult proportions, realistic body ratio, normal head size"`

---

## Meta-prompt — Générer un prompt Leonardo AI via Claude

> Copier-coller ce bloc dans une conversation Claude (avec le skill Prompt Master activé) en remplaçant les champs entre crochets.

```
Je développe un jeu de cartes roguelike avec des illustrations de personnages
générées sur Leonardo AI. J'ai besoin d'un prompt optimisé pour générer
l'illustration d'un personnage de carte.

STYLE CIBLE :
- Marvel Snap card illustration style
- Dynamic superhero digital painting
- Painterly brushwork, high contrast, vibrant saturated colors
- Dramatic cinematic lighting, strong rim light, deep shadows
- Character fills the frame, cut at upper thigh, extreme close composition
- Atmospheric dark background with energy effects
- Volumetric light rays, motion blur, speed lines si action
- Modèle utilisé : FLUX Dev, Platform Element : Dynamic

CONTRAINTES TECHNIQUES :
- L'image sera downscalée à 256×256 pixels pour le jeu — la silhouette et
  l'expression doivent être lisibles à cette taille
- Un seul personnage, centré, pas de second personnage visible
- Pas de texte, watermark, signature dans l'image

PERSONNAGE À ILLUSTRER :
- Nom : [NOM DU HÉROS]
- Âge approximatif : [ÂGE]
- Morphologie : [DESCRIPTION PHYSIQUE]
- Costume : [DESCRIPTION COSTUME ET COULEURS]
- Pouvoir : [POUVOIR SUPER-HÉROÏQUE]
- Personnalité : [2-3 MOTS CLÉ]
- Pose souhaitée : [DESCRIPTION DE LA POSE ET ACTION]
- Props / éléments visuels signature : [OBJETS, EFFETS, DÉTAILS DISTINCTIFS]
- Palette de couleurs : [COULEURS DOMINANTES + ACCENT]

Génère-moi :
1. Le prompt positif optimisé (en anglais, structuré du général au spécifique)
2. Le prompt négatif adapté
3. 3 ajustements rapides si le résultat n'est pas satisfaisant
```

---

## Aciera — Bloc prêt à envoyer à Claude

```
Je développe un jeu de cartes roguelike avec des illustrations de personnages
générées sur Leonardo AI. J'ai besoin d'un prompt optimisé pour générer
l'illustration d'un personnage de carte.

STYLE CIBLE :
- Marvel Snap card illustration style
- Dynamic superhero digital painting
- Painterly brushwork, high contrast, vibrant saturated colors
- Dramatic cinematic lighting, strong rim light, deep shadows
- Character fills the frame, cut at upper thigh, extreme close composition
- Atmospheric dark background with energy effects
- Volumetric light rays, motion blur, speed lines si action
- Modèle utilisé : FLUX Dev, Platform Element : Dynamic

CONTRAINTES TECHNIQUES :
- L'image sera downscalée à 256×256 pixels pour le jeu — la silhouette et
  l'expression doivent être lisibles à cette taille
- Un seul personnage, centré, pas de second personnage visible
- Pas de texte, watermark, signature dans l'image

PERSONNAGE À ILLUSTRER :
- Nom : Aciera
- Âge approximatif : 74 ans
- Morphologie : femme, carrure large et trapue, robuste, imposante — pas frêle
  du tout, donne l'impression de pouvoir écraser quelque chose sans effort
- Costume : combinaison super-héros vintage années 60, couleurs or chaud et
  argent passés/fanés, courte cape pratique, usé mais porté avec dignité
- Pouvoir : magnétisme — contrôle et lévite les objets métalliques
- Personnalité : pragmatique, calme absolu, légèrement terrifiante,
  complètement indifférente au danger
- Pose souhaitée : debout, un bras levé paume vers le haut, l'autre bras
  dans une position naturelle et complémentaire (main sur la hanche ou
  légèrement en arrière pour l'équilibre), posture assurée et autoritaire
- Props / éléments visuels signature : plusieurs cubes d'acier qui tournoient
  en lévitation au-dessus de sa paume levée, énergie magnétique bleu-blanc
  qui relie sa main aux cubes, légère rotation visible sur les cubes
- Palette de couleurs : or chaud + argent comme couleurs costume, bleu
  électrique profond pour l'énergie, fond sombre quasi noir

Génère-moi :
1. Le prompt positif optimisé (en anglais, structuré du général au spécifique)
2. Le prompt négatif adapté
3. 3 ajustements rapides si le résultat n'est pas satisfaisant
```







