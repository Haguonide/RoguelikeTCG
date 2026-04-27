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

## Contexte de la session DA

**Objectif :** valider la direction artistique du jeu via batteries de tests sur Leonardo AI.
**Personnage de test :** Léonard de Vinci — inventeur alcoolique raté.
**Format généré :** 1024×1024 (downscaler à 256×256 pour juger le rendu final).
**Contraintes :**
- Outil principal : Leonardo AI (`Phoenix 1.0` — Character Ref pour cohérence cross-cartes)
- Post-processing : Upscayl + Photopea (usage simple)
- Format carte : 256×256, découpe en 3 layers (cadre / illustration / stats UI)
- Solo dev

---

## Description canonique de Léonard de Vinci (éléments visuels fixes)

> Ces éléments doivent apparaître dans TOUS les prompts Léonard, peu importe la variation.

- Vieux bonhomme, inventeur alcoolique à bout de course, légèrement trapu
- Proportions réalistes de la tête (pas chibi)
- Lab coat sale et ouvert
- Béret Renaissance sombre avec des lunettes steampunk repoussées sur le dessus
- Cheveux gris explosifs et sauvages qui dépassent sous le béret
- Très longue barbe grise fluide qui descend jusqu'à la poitrine
- Nez rouge et bulbeux
- Yeux injectés de sang mais avec une lueur de génie
- Bouteille de vin dans la main droite
- Plans froissés dans la main gauche
- Haut du corps entier visible (portrait épaules/torse)
- Fond chaud simple et épuré, un seul personnage, composition centrée

---

## Critères d'évaluation (à appliquer sur chaque résultat)

| Critère | Question |
|---|---|
| **Silhouette** | Est-ce qu'on comprend la pose en 1 seconde à 256×256 ? |
| **Lisibilité des couleurs** | La palette ressort-elle clairement ? |
| **Ton** | Ça fait rire/sourire ? Ça a du caractère ? |
| **Cohérence promptable** | Si tu regénères, tu retombes dans le même style ? |
| **Effort post-processing** | Photopea simple suffit ou il faut ramer ? |

**Objectif :** 5 variations du prompt principal, trouver 1 winner à locker comme Character Ref.

---

## Direction retenue — Flat cartoon moderne

**Modèle Leonardo :** `Phoenix 1.0`
**Refs :** Style Ref avec une image de perso flat cartoon au choix si dispo

### Prompt principal

```
washed-up alcoholic inventor old man, slightly stocky proportions,
realistic head size, dirty open lab coat,
dark Renaissance beret with steampunk goggles pushed up on top,
wild explosive grey hair sticking out under the beret,
very long flowing grey beard reaching chest,
red bulbous nose, bloodshot tired eyes with a glimmer of genius,
wine bottle in right hand, crumpled blueprints in left hand,
full upper body visible, simple clean warm background,
single character, centered composition,
thick bold uniform outlines, flat cel shading,
bright muted warm colors, blue and copper accent palette,
expressive exaggerated face, slightly stocky proportions, not chibi,
instantly readable at small sizes,
2D game card character art, flat cartoon illustration style

Negative :
realistic, photo, 3D render, blurry, complex background,
watermark, text, signature, anime, chibi, tiny head, big head small body
```

### Variations à tester (changer 1 paramètre à la fois)

**Variation 1 — Expression plus folle**
Ajouter après "glimmer of genius" :
```
manic grin, one eyebrow raised, slightly cross-eyed
```

**Variation 2 — Palette plus chaude / lisible**
Ajouter :
```
warm amber lighting, golden hour color grading, warm ivory background
```

**Variation 3 — Trait plus épais / plus lisible à petite taille**
Ajouter :
```
very thick black outlines, high contrast, strong silhouette
```

**Variation 4 — Pose avec plus d'énergie**
Remplacer "full upper body visible" par :
```
leaning forward slightly, casual confident slouch, weight on one side
```

**Variation 5 — Style Ref image**
Ajouter une image de perso flat cartoon (au choix) en Style Ref + baisser le prompt weight à 0.7.

---

## Notes workflow Leonardo AI

- Générer les 5 variations, noter les winners avec le score des critères ci-dessus
- Winner retenu → l'uploader comme **Character Reference** dans Phoenix 1.0 pour toutes les cartes suivantes
- Upscayl avant d'importer dans Unity si besoin
- Découpe dans Photopea : layer cadre / layer illustration / stats posées en UI Unity
- Si couleurs trop sombres : ajouter `"bright warm color palette, warm lighting, cheerful muted tones"`
- Si expression trop sobre : ajouter `"extremely expressive face, manic energy, wide eyes, raised eyebrow"`
- Si proportions trop chibi malgré le negative : ajouter `"adult proportions, realistic body ratio, normal head size"`







