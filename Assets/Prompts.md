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

---

# FICHES VISUELLES — Tous les personnages

> Copier le bloc "PERSONNAGE À ILLUSTRER" dans la conversation Claude avec le meta-prompt.
> Les descriptions physiques non définies dans le design doc ont été inventées pour cohérence visuelle — libre à toi de les ajuster.

---

## LES ÉTERNELS

**Palette équipe :** or chaud + argent (golden age + magnétisme métal)
**Costume commun :** combinaisons super-héros années 60-70, légèrement passées mais portées avec dignité

---

### Aciera — Capitaine *(Magnétisme, 74 ans, assume son âge)*

```
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
```

---

### Le Maître *(Télékinésie, ~68 ans, assume son âge)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Le Maître
- Âge approximatif : 68 ans
- Morphologie : homme, grande taille, silhouette mince et élancée, voûté
  avec élégance — pas maigre, mais tout en verticalité, mains longues et fines
- Costume : combinaison super-héros vintage années 60, tons bleu nuit et
  argent sobres, sans cape, col haut, tissus qui drainent bien la lumière,
  usé mais impeccablement entretenu
- Pouvoir : télékinésie — déplace les objets par la pensée, contrôle précis
- Personnalité : calme absolu, sage, économe de ses gestes, rien ne l'étonne
- Pose souhaitée : légèrement penché en avant, les deux mains tendues en avant
  paumes vers le haut, expression de concentration totale et détachée,
  comme s'il soupesait quelque chose d'invisible
- Props / éléments visuels signature : plusieurs objets (blocs de béton,
  débris métalliques) flottent en cercle autour de lui dans un halo d'énergie
  blanche translucide, comme suspendus dans une bulle de temps
- Palette de couleurs : bleu nuit profond + argent pour le costume,
  blanc pur pour l'aura télékinétique, fond sombre avec particules lumineuses
```

---

### Titanio *(Duplication, ~70 ans, nie complètement son âge)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Titanio
- Âge approximatif : 70 ans
- Morphologie : homme, corpulence athlétique légèrement enveloppée par les
  années, menton proéminent, cheveux grisonnants soigneusement peignés en
  arrière, sourcils dramatiquement levés, posture théâtrale d'homme qui
  se croit encore au sommet
- Costume : combinaison super-héros années 60 cramoisie et bronze, épaulettes
  visibles, costume trop serré d'un demi-cran, porte encore avec une fierté
  totale. Plusieurs copies semi-transparentes de lui-même l'entourent.
- Pouvoir : duplication — crée des copies physiques de lui-même
- Personnalité : orgueilleux, théâtral, nie ses douleurs physiques évidentes,
  toujours en représentation
- Pose souhaitée : pose héroïque exagérée — poings sur les hanches, torse bombé,
  menton levé — mais dos légèrement voûté malgré lui. Expression triomphante.
- Props / éléments visuels signature : 2 ou 3 copies de lui translucides
  flottant en arc derrière lui, légèrement désynchronisées, chacune dans
  la même pose exagérée. Aura bronze/cramoisie qui les relie tous.
- Palette de couleurs : cramoisi vif + bronze doré, aura dorée pour les copies,
  fond sombre dramatique avec lumière de scène chaude
```

---

### Glamoura *(Illusion, ~69 ans, nie complètement son âge)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Glamoura
- Âge approximatif : 69 ans
- Morphologie : femme, silhouette élancée, cheveux platines soigneusement
  coiffés à la mode des années 60 (chignon ou brushing haut), maquillage
  dramatique parfaitement appliqué, porte ses rides comme des accessoires.
  Tient encore une posture de mannequin irréprochable.
- Costume : combinaison super-héros années 60 rose shocking et blanc nacré,
  avec détails argentés, léger voile/cape semi-transparent qui flotte, élégant
  et légèrement suranné — elle pense que c'est toujours la mode
- Pouvoir : illusion — crée des illusions visuelles parfaites
- Personnalité : glamour assumé, sincèrement convaincue d'être encore la plus
  belle héroïne en activité, grandiose et légèrement hors du temps
- Pose souhaitée : pose de diva — une main tendue en avant projetant une illusion,
  l'autre main sur la hanche, tête légèrement penchée, sourire éclatant et
  légèrement trop parfait
- Props / éléments visuels signature : l'illusion projetée est une version d'elle-même
  en 1968 — jeune, en costume identique, flottant au bout de sa main comme
  un hologramme rose et blanc. Particules d'illusion rose autour d'elle.
- Palette de couleurs : rose shocking + blanc nacré + argent, hologramme rose vif,
  fond sombre avec reflets irisés, lumière de scène chaleur vintage
```

---

## PROGRAMME R

**Palette équipe :** rouge/orange vif (énergie, feu), gris acier, tons urbains sombres
**Costume commun :** tenues opérationnelles modernes, pas de super-costumes vintage — combinaisons tactiques ou vêtements civils modifiés, look ex-criminel recyclé en agent de terrain
**Ton visuel :** grinçant, nerveux, jamais complètement à l'aise dans le rôle

---

### Voltaire — Capitaine *(Électricité, hacker criminel)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Voltaire
- Âge approximatif : 35 ans
- Morphologie : homme, silhouette mince et nerveuse, cheveux frisés mi-longs
  ébouriffés avec des pointes carbonisées (résidu d'auto-électrocution),
  lunettes épaisses à montures métalliques légèrement brûlées sur les bords,
  cicatrice en zigzag sur la joue gauche, regard vif et condescendant
- Costume : combinaison opérationnelle sombre bleu marine/gris foncé avec
  câbles et composants électroniques intégrés dans les épaulettes et les
  avant-bras, gants isolants partiellement déchirés, insigne "R" discret
- Pouvoir : électricité — génère et contrôle des décharges électriques
- Personnalité : se croit le plus intelligent de la pièce, cite des philosophes
  à contresens, arrogant mais fonctionnel
- Pose souhaitée : bras croisés, légèrement penché en avant, expression
  de quelqu'un qui juge tout le monde autour de lui, arcs d'électricité
  crépitant entre ses doigts comme s'il ne pouvait pas s'en empêcher
- Props / éléments visuels signature : arcs électriques bleu-blanc entre les mains
  et les câbles du costume, lueur bleue derrière les lunettes, léger halo
  électrique autour de la silhouette, cheveux légèrement dressés
- Palette de couleurs : bleu électrique vif + gris anthracite, fond sombre
  avec grilles et câbles en ombres, lueurs bleu-blanc intense
```

---

### Cendres *(Feu / Explosion, arsoniste sous contrat)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Cendres
- Âge approximatif : 28 ans
- Morphologie : femme, silhouette athlétique et compacte, cheveux noirs courts
  coupés au carré, yeux clairs (presque décolorés), expression entièrement
  neutre et professionnelle — comme quelqu'un qui remplit un bon de commande
- Costume : combinaison opérationnelle ignifugée gris anthracite + orange brûlé
  sur les épaules/avant-bras, traces de suie et de roussi volontairement
  intégrées à l'esthétique, sans cape, pratique et fonctionnel
- Pouvoir : pyrokinésie / explosions — génère et contrôle le feu et les déflagrations
- Personnalité : professionnelle froide, zéro affect, traite les combats comme
  une prestation de service, aucun remords
- Pose souhaitée : debout les bras légèrement écartés, paumes ouvertes vers
  l'avant, flammes jaillissant des deux mains, expression complètement blasée
  comme si elle faisait la vaisselle
- Props / éléments visuels signature : flammes orange vif et rouge profond
  jaillissant des deux paumes en jets contrôlés, légère fumée noire montant
  de ses épaules, fond embrasé en arrière-plan flou
- Palette de couleurs : orange vif + rouge brûlé + noir/gris fumée, fond sombre
  avec lueurs chaudes d'incendie, contraste fort lumière/ombre
```

---

### Le Bloc *(Force brute, homme de main)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Le Bloc
- Âge approximatif : 40 ans
- Morphologie : homme, masse corporelle exceptionnelle, très grand et très large,
  cou de taureau, bras comme des troncs, expression débonnaire et vaguement
  confuse — pas menaçant d'intention, juste physiquement écrasant
- Costume : combinaison opérationnelle noire taille XXL manifestement faite
  sur mesure car aucun stock ne convient, coutures aux limites, insigne "R"
  sur l'épaule presque illisible tant l'épaule est large
- Pouvoir : force brute surhumaine, résistance aux dégâts
- Personnalité : gentil par défaut, suit les ordres, mange beaucoup, ne questionne rien
- Pose souhaitée : debout, légèrement voûté comme quelqu'un d'habitué à passer
  sous les cadres de portes, bras ballants naturellement, expression neutre
  et placide. Un poing levé de façon non menaçante, comme pour montrer quelque chose.
- Props / éléments visuels signature : les fissures dans le sol autour de ses pieds
  suggèrent son poids, aura subtile rouge-brique autour des poings,
  knuckles légèrement éraflés, une miette sur l'uniforme (detail optionnel)
- Palette de couleurs : noir profond + rouge brique pour l'aura, peau mate foncée,
  fond sombre industriel, éclairage dur et contrasté
```

---

### Trace *(Super-vitesse, pickpocket de génie)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Trace
- Âge approximatif : 22 ans
- Morphologie : homme, silhouette très fine et élancée, cheveux en désordre
  permanent (vitesse constante), yeux vifs et légèrement insolents, sourire
  en coin de quelqu'un qui pense arriver à temps mais arrive toujours en retard
- Costume : combinaison opérationnelle vert électrique / gris foncé, design
  aérodynamique avec bandes d'accélération sur les jambes, chaussures renforcées
  à semelles spéciales. Montre au poignet (ironie : toujours en retard).
- Pouvoir : super-vitesse — déplacement supersonique, réflexes extrêmes
- Personnalité : arrogant, léger, charme naturel de voleur professionnel,
  sous-estime systématiquement les situations
- Pose souhaitée : mi-course, un pied décollé du sol, corps légèrement incliné
  en avant, bras écartés, sourire désinvolte, pas du tout concentré
- Props / éléments visuels signature : traînée de lignes de vitesse vert électrique
  derrière lui, légère distorsion de l'air autour du corps, montre visible
  au poignet, semelles avec éclat d'énergie cinétique
- Palette de couleurs : vert électrique vif + gris foncé, traînées vitesse
  vert/blanc, fond flouté par la vitesse, éclairage dynamique en mouvement
```

---

## LES CONTRACTUELS *(combats normaux + élite)*

**Palette équipe :** bleu corporate terne + blanc + gris — couleurs d'open-space
**Costume commun :** combinaisons super-héros standardisées, logo L'Équipe Numéro Un visible, couleurs uniformisées et sans âme, badge nominatif, tenues légèrement froissées
**Ton visuel :** fonctionnaire fatigué avec des super-pouvoirs, aucun enthousiasme

---

### Patrice *(Super-force, 60% de puissance, préserve son dos)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Patrice
- Âge approximatif : 52 ans
- Morphologie : homme, corpulence massive mais molle — clairement bâti pour
  la super-force mais n'en utilise qu'une fraction, ventre légèrement en avant,
  épaules qui ont vu des jours meilleurs, visage de quelqu'un qui a renoncé
- Costume : combinaison corporate bleu/gris L'Équipe Numéro Un taille adaptée,
  légèrement détendue, badge "PATRICE" au niveau du torse, ceinture lombaire
  de soutien visible sous la combinaison
- Pouvoir : super-force — jamais utilisée à pleine puissance pour préserver le dos
- Personnalité : résigné, prudent, estime que 60% c'est très bien
- Pose souhaitée : en train de soulever un véhicule d'une main, l'autre dans le dos
  dans un geste de protection lombaire, grimace de précaution pas de douleur,
  regard qui dit "j'aurais pu rester chez moi"
- Props / éléments visuels signature : véhicule soulevé d'une seule main sans effort
  réel, mais posture de précaution exagérée, ceinture lombaire bien visible
- Palette de couleurs : bleu corporate terne + gris, fond open-space/bureau
  ou rue avec lumière plate et peu dramatique
```

---

### Régine *(Téléportation, sert principalement à chercher son café)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Régine
- Âge approximatif : 44 ans
- Morphologie : femme, silhouette moyenne, cheveux châtains attachés en queue
  de cheval pratique, expression de quelqu'un perpétuellement entre deux endroits,
  légèrement ailleurs, regard distrait mais pas désagréable
- Costume : combinaison corporate bleu/gris L'Équipe Numéro Un, badge "RÉGINE",
  café chaud à la main (tasse thermos avec logo d'une chaîne banale)
- Pouvoir : téléportation — instantanée, sans restriction, utilisée pour l'essentiel
- Personnalité : pragmatique domestique, priorités claires, pas hostile juste préoccupée
- Pose souhaitée : en train d'émerger d'un portail de téléportation violet/blanc,
  une jambe encore dans le vide du portail, tenant son café, expression de
  quelqu'un qui vient de chez le boulanger pas d'un combat
- Props / éléments visuels signature : portail de téléportation violet/blanc
  derrière elle, particules de distorsion, café bien visible en premier plan,
  une petite vapeur qui sort de la tasse
- Palette de couleurs : bleu corporate + violet pour les portails, fond terne
  de couloir de bureau, lumière artificielle froide
```

---

### Chad *(Boucliers énergétiques, part à 17h01)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Chad
- Âge approximatif : 31 ans
- Morphologie : homme, silhouette athlétique soignée, cheveux blonds bien coiffés,
  visage de quelqu'un qui fait du sport mais "raisonnablement", montre bien visible
  au poignet — il regarde l'heure
- Costume : combinaison corporate bleu/gris L'Équipe Numéro Un impeccable et repassée,
  badge "CHAD", montre de sport au poignet gauche — pointée vers le spectateur
- Pouvoir : boucliers énergétiques — bulle de force protectrice jaune/ambre
- Personnalité : ponctuel dans l'autre sens, ne fait que ce qui est requis,
  désengagement professionnel complet
- Pose souhaitée : bouclier énergétique déployé d'une main, l'autre main
  regardant ostensiblement sa montre, expression de quelqu'un qui calcule
  combien de temps il reste avant de pouvoir partir
- Props / éléments visuels signature : bouclier hémisphérique jaune/ambre
  semi-transparent d'un côté, montre en évidence de l'autre, regard sur la montre
- Palette de couleurs : bleu corporate + jaune ambre pour le bouclier,
  fond neutre bureau ou rue, lumière plate
```

---

### Marlène *(Duplication, crée des copies pour glander)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Marlène
- Âge approximatif : 38 ans
- Morphologie : femme, silhouette ordinaire, expression détendue de quelqu'un
  qui a résolu le problème du travail, cheveux roux mi-longs souvent sur le visage,
  regard légèrement futé de quelqu'un qui a trouvé un système
- Costume : combinaison corporate bleu/gris L'Équipe Numéro Un, badge "MARLÈNE",
  version elle légèrement défaite — c'est la vraie, les copies sont plus présentables
- Pouvoir : duplication — crée des copies physiques d'elle-même
- Personnalité : ingénieuse dans la paresse, pas malveillante, juste optimisatrice
- Pose souhaitée : bras croisés, détendue, en retrait, pendant que 2-3 copies
  d'elle (en tenue parfaite) sont en train de travailler devant elle,
  expression satisfaite de chef de projet
- Props / éléments visuels signature : 2-3 copies d'elle en action (combat,
  travail) derrière/autour, la vraie Marlène immobile et sereine, les copies
  légèrement plus lumineuses et actives qu'elle
- Palette de couleurs : bleu corporate + orange roux, copies avec halo bleu clair,
  fond lieu de travail ou combat, lumière naturelle plate
```

---

## LES ACQUISITIONS *(mini-boss + boss)*

**Palette équipe :** noir + gris charbon + blanc cassé — costume trois pièces
**Costume commun :** costume trois pièces taillé sur mesure, absolument aucun élément super-héros visible, cravate, pochette, chaussures cirées. Pouvoirs manifestés sans fioriture.
**Ton visuel :** intimidation froide, professionalisme effrayant, aucun affect

---

### Le Partenaire *(Persuasion / influence mentale)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Le Partenaire
- Âge approximatif : 48 ans
- Morphologie : homme, grand et mince, traits lisses et agréables, sourire
  permanent et légèrement trop parfait, cheveux poivre et sel impeccablement
  coiffés, poignée de main dans la pose
- Costume : costume trois pièces anthracite très sombre, cravate bordeaux,
  pochette blanche, boutons de manchettes discrets mais coûteux, aucun élément
  super-héros, badge "L'Équipe Numéro Un" seul indice
- Pouvoir : persuasion / influence mentale — contrôle subtil des décisions d'autrui
- Personnalité : souriant, rassurant, parle de "synergie" et "d'opportunité",
  jamais hostile en surface
- Pose souhaitée : main tendue pour serrer la main, sourire parfait, légèrement
  penché en avant dans une posture d'ouverture — mais les yeux ont une lueur
  d'influence mentale subtile, blanche ou violette, imperceptible
- Props / éléments visuels signature : aura très subtile d'influence mentale
  autour de la main tendue (spirale blanche/violette transparente), sourire
  impeccable, carte de visite dans l'autre main
- Palette de couleurs : anthracite profond + blanc cassé + bordeaux,
  lueur influence blanche/violette très subtile, fond salle de réunion ou couloir
```

---

### La Clause *(Binding / entrave, "termes et conditions")*

```
PERSONNAGE À ILLUSTRER :
- Nom : La Clause
- Âge approximatif : 35 ans
- Morphologie : femme ou homme (non défini — laisser ambigu dans le prompt),
  silhouette précise et anguleuse, mouvements économes et calculés, visage
  sans expression particulière, regard froid et évaluateur
- Costume : costume trois pièces gris acier, cravate grise sombre,
  pochette grise. Un contrat ou document plié dans la poche intérieure visible.
- Pouvoir : binding/entrave — des chaînes ou cordages de lumière immobilisent la cible
- Personnalité : factuel, neutre, ne parle qu'en "termes et conditions",
  aucune cruauté manifeste — juste application de la procédure
- Pose souhaitée : bras tendu, doigts écartés, des chaînes de lumière blanc-gris
  émanant des doigts comme des fils qui s'enroulent vers l'extérieur du cadre
  (vers une cible hors-champ), expression absolument neutre
- Props / éléments visuels signature : chaînes ou filins de lumière blanche
  partant des doigts et s'enroulant, effet de contrat/texte imprimé en hologramme
  transparente autour des liens
- Palette de couleurs : gris acier + blanc froid, liens lumineux blanc-gris,
  fond sombre et neutre, éclairage froid et sans chaleur
```

---

### L'Évaluateur *(Scan / analyse, "valeur de rachat")*

```
PERSONNAGE À ILLUSTRER :
- Nom : L'Évaluateur
- Âge approximatif : 55 ans
- Morphologie : homme, silhouette mince et sèche, lunettes à montures fines
  sur un nez étroit, regard derrière les verres qui scanne et classe en permanence,
  légèrement voûté vers l'avant dans une posture d'observation
- Costume : costume trois pièces noir, cravate très sombre, lunettes. Tient
  un bloc-notes fin ou tablette pour noter ses évaluations.
- Pouvoir : scan / analyse complète — voit les stats, failles, valeur marchande de tout
- Personnalité : cliniquement insultant, précis, traite tout comme un actif
  à quantifier, pas de malice — juste des chiffres
- Pose souhaitée : légèrement de côté, lunettes poussées vers le haut
  pour regarder par-dessus dans un geste condescendant, une main sur le menton
  en évaluation, des hologrammes de données flottant devant lui
- Props / éléments visuels signature : hologrammes de données (chiffres, graphiques,
  jauges) flottant autour de lui comme un HUD, regard avec légère lueur de scan
  derrière les lunettes (rouge ou blanc), bloc-notes visible
- Palette de couleurs : noir + blanc data + rouge scan pour les hologrammes,
  fond sombre avec grilles de données, éclairage froid analytique
```

---

### Le Liquidateur *(Destruction pure, très peu bavard)*

```
PERSONNAGE À ILLUSTRER :
- Nom : Le Liquidateur
- Âge approximatif : inconnu (masqué / dissimulé)
- Morphologie : silhouette massive et monolithique, aucun détail visible —
  combinaison intégrale noire sans logo, casque ou cagoule qui cache entièrement
  le visage, gants épais. Taille imposante mais silhouette nette et précise.
- Costume : combinaison intégrale noire sans aucun détail décoratif, aucun badge,
  aucun logo — différent de tous les autres membres des Acquisitions.
  Une seule couleur. Rien à lire.
- Pouvoir : destruction pure — énergie de destruction totale, dernier recours
- Personnalité : muet ou presque, action pure, aucune discussion
- Pose souhaitée : debout immobile face caméra, bras légèrement écartés,
  énergie de destruction noire/rouge/violette qui pulse autour des mains
  et s'irradie depuis le sol sous ses pieds. Aucun mouvement — juste présence.
- Props / éléments visuels signature : aura de destruction qui craquelle le sol
  autour de lui, énergie noire/rouge émanant des mains et du sol, silhouette
  entourée d'une distorsion visuelle de l'air, fissures lumineuses
- Palette de couleurs : noir total + rouge profond + violet pour l'énergie
  de destruction, fond fracturé et sombre, zéro lumière chaude
```

---

## PROGRAMME R

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







