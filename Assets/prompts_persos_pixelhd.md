# Prompts Personnages — Programme R (Style Hi-Bit Pixel Art)

**Outil validé : Pixellab AI** — meilleurs résultats obtenus sur les tests du 2026-05-13. Résultats très propres, style cohérent entre les persos. Direction DA arrêtée : hi-bit pixel art via Pixellab AI.

Style cible : **hi-bit pixel art** — contours noirs épais, palette vive mais contrainte (~32 couleurs max par perso), shading en aplats avec dithering discret, silhouette lisible, proportions cartoon légèrement stylisées. Référence visuelle : Grail (jeu), Shovel Knight Treasure Trove, Wargroove.

Format : **carré 1:1**, fond transparent, personnage en buste serré centré et remplissant le cadre.

## ✅ Résultats testés (2026-05-13)

| Perso | Statut | Notes |
|---|---|---|
| **Voltaire** | ✅ Validé | Éclairs, manteau vilain, monocle, bracelet cheville — résultat fidèle |
| **Trace** | ✅ Validé | Motion blur dupliqué du générateur = excellent rendu vitesse, combinaison bleue parfaite |
| **Le Bloc** | ✅ Validé (2e essai) | 1er essai : tenue Superman → modif "Change the costume to a worn olive green military uniform with rolled-up sleeves, remove the cape and the Superman logo, replace the yellow belt with a simple tactical belt, keep the uniform slightly torn at the shoulders from his muscles" → résultat parfait |
| **Cendres** | ⏳ À tester | — |

---

## Voltaire — Capitaine, Électricité ✅

**Personnalité :** Se croit le plus intelligent de la pièce. Ex-vilain en liberté conditionnelle, cite des philosophes à contresens, air suffisant permanent. Capitaine du groupe par défaut — personne d'autre ne voulait le rôle.

**Visuel cible :** Homme mince, costume de super-vilain rapiécé avec un col relevé ridicule, cheveux dressés par l'électricité statique permanente, monocle craquelé. Électricité crépitante autour des mains et de la tête. Bracelet électronique de libération conditionnelle visible à la cheville. Pose : index levé comme pour faire la leçon, éclairs autour.

### Pixellab AI
```
hi-bit pixel art character, male supervillain on parole, electricity powers, thin arrogant man with static-spiked white hair, cracked monocle, tattered villain costume with ridiculous raised collar, electronic ankle monitor, lightning crackling around both hands, one finger raised in a lecturing pose, smugly condescending expression, flat shading with dithering, very thick bold black pixel outlines, vibrant limited palette, close-up bust shot cropped at the waist, character filling the entire frame, isolated on white background, square format
```

---

## Cendres — Feu / Explosion

**Personnalité :** Traite le super-héroïsme comme un job alimentaire. Zéro remords, zéro affect. Fait exploser des choses avec la même énergie qu'elle ferait la vaisselle. Toujours l'air de vouloir être ailleurs.

**Visuel cible :** Femme, combinaison de travail ignifugée retroussée aux manches, cheveux tirés en chignon approximatif avec mèches brûlées aux pointes. Flammes aux paumes, petite explosion dans le fond qu'elle ne regarde même pas. Expression : regard vide, légèrement ennuyée. Cendres sur l'uniforme.

### Pixellab AI
```
hi-bit pixel art character, female superhero with fire and explosion powers, deadpan bored expression, practical fire-resistant work uniform with sleeves rolled up, messy bun hairstyle with singed burnt hair tips, flames in both open palms, small explosion happening behind her that she is completely ignoring, ash and soot stains on uniform, absolutely zero emotional investment, flat shading with dithering, very thick bold black pixel outlines, vivid orange and red fire palette, close-up bust shot cropped at the waist, character filling the entire frame, isolated on white background, square format
```

---

## Le Bloc — Force Brute ✅

**Personnalité :** Suit les ordres, mange beaucoup, pose pas de questions. Probablement le plus heureux du groupe. Pas stupide — juste totalement indifférent à la complexité. Fidèle comme un labrador.

**Visuel cible :** Homme immense et massif, uniforme militaire olive retroussé aux manches, visage rond et paisible, regard content. Tient un sandwich à moitié mangé dans une main, poing énorme levé de l'autre.

### Pixellab AI
```
hi-bit pixel art character, enormous male superhero with super strength, huge muscular body with uniform seams bursting at the shoulders, round friendly peaceful face, happy vacant expression, one massive fist raised in the air, other hand holding a half-eaten sandwich, tiny electronic ankle monitor that barely fits, gentle giant energy, flat shading with dithering, very thick bold black pixel outlines, muted earthy color palette with strong accents, close-up bust shot cropped at the waist, character filling the entire frame, isolated on white background, square format
```

**Modification appliquée après 1er essai (tenue Superman → militaire) :**
```
Change the costume to a worn olive green military uniform with rolled-up sleeves, remove the cape and the Superman logo, replace the yellow belt with a simple tactical belt, keep the uniform slightly torn at the shoulders from his muscles
```

---

## Trace — Super-Vitesse ✅

**Personnalité :** Toujours en retard malgré une vitesse supersonique. Paniquée en permanence, essoufflée, regardant sa montre. Personne ne comprend comment c'est possible. Elle non plus.

**Visuel cible :** Femme élancée en combinaison de vitesse avec traînées de mouvement (motion blur pixel art), cheveux épars dans le déplacement. Le générateur a produit un effet de duplication du personnage (ghost trail) qui rend la vitesse encore mieux qu'une simple traînée.

### Pixellab AI
```
hi-bit pixel art character, female speedster superhero always late, slender woman in a speed suit with visible pixel art motion blur trail behind her, wild wind-blown hair mid-movement, looking at her wristwatch with pure panic on her face, speed energy lines streaking behind her, smoking shoes from friction, rushed frantic pose as if arriving very late, kinetic energy glow around legs, flat shading with dithering, very thick bold black pixel outlines, electric blue and yellow speed palette, close-up bust shot cropped at the waist, character filling the entire frame, isolated on white background, square format
```

---

## Notes de génération

### Cohérence du style entre les 4 persos
Pour garantir une palette cohérente entre les membres de Programme R, générer un perso en premier (recommandé : **Cendres** — la palette feu est la plus distinctive), puis utiliser la fonction **Style Reference** de Pixellab pour les suivants.

Couleurs suggérées pour l'équipe :
- Fond uniforme d'équipe : **rouge brique foncé** `#8B2020` + **gris anthracite** `#2A2A2A`
- Accent Voltaire : **jaune électrique** `#FFE000`
- Accent Cendres : **orange brûlé** `#FF6B1A`
- Accent Le Bloc : **brun/terre** `#8B5E3C`
- Accent Trace : **bleu électrique** `#00AAFF`

### Backgrounds transparents
Générer sur fond blanc, retirer sous Photopea (Sélection > Couleur similaire sur le blanc).

### Dimensions finales Unity
Exporter en PNG transparent. Upscale ×4 sous **Upscayl** (modèle Digital Art) avant import. Taille recommandée : **512×512px minimum** en sortie Pixellab, **2048×2048px** après upscale.
