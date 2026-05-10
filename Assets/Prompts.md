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

---

# FICHES VISUELLES — Cartes non-héros

> Même format que les personnages — copier le bloc dans la conversation Claude avec le meta-prompt.
> Les cartes de type Sort (pas d'unité posée) ont une illustration d'effet/scène plutôt que de personnage.

---

## PROGRAMME R — Voltaire *(électricité)*

**Palette Voltaire :** bleu électrique vif + gris anthracite + lueurs bleu-blanc

---

### Éclair Conduit *(unité rapide — frappe dès le tour suivant)*

```
CARTE À ILLUSTRER :
- Nom : Éclair Conduit
- Type : Unité (pas le héros Voltaire lui-même — un sbire électrique)
- Concept visuel : un jeune délinquant de rue électrifié, hoodie sombre
  avec câbles et composants électroniques bricolés cousus dessus,
  pas de super-costume, look urbain underground
- Âge approximatif : 19-20 ans
- Morphologie : homme, mince et nerveux, cheveux dressés électrostatiques,
  bras fine avec veines lumineuses bleues sous la peau
- Pose souhaitée : en pleine course d'élan, bras tendu en avant prêt
  à décharger, expression de concentration intense, corps incliné
- Props / éléments visuels signature : arcs électriques crépitant entre les
  doigts, veines bleues lumineuses visibles sous la peau des avant-bras,
  câbles du hoodie qui crachent des étincelles, traînée bleue derrière lui
- Palette de couleurs : gris anthracite + bleu électrique vif, fond sombre
  urbain, lueurs bleu-blanc intenses
```

---

### Parafoudre *(unité — attire les coups)*

```
CARTE À ILLUSTRER :
- Nom : Parafoudre
- Type : Unité (sbire de Voltaire, rôle de bouclier humain)
- Concept visuel : un homme trapu et stoïque avec des tiges de métal
  vissées sur les épaulières comme des antennes de paratonnerre artisanales,
  armure de fortune soudée, look de quelqu'un qui s'est volontairement
  rendu conducteur de foudre et trouve ça parfaitement normal
- Âge approximatif : 35 ans
- Morphologie : homme, massif et bas sur pattes, cou épais, tête rasée,
  regard vide et résigné — il sait ce que son rôle implique
- Pose souhaitée : debout de face, bras légèrement écartés, paumes ouvertes
  vers le ciel — posture d'accueil/réception, comme invitant la foudre
  à tomber sur lui, expression neutre et fataliste
- Props / éléments visuels signature : 4-5 tiges métalliques soudées
  sur les épaules et la tête comme un paratonnerre ambulant, éclairs
  qui convergent vers lui depuis tous les côtés, corps légèrement fumant,
  électricité qui crépite sur toute la surface de l'armure
- Palette de couleurs : gris métal + bleu électrique, éclairs blanc-jaune,
  fond sombre chargé d'électricité statique
```

---

### Court-Circuit *(sort — effet négatif sur une unité ennemie)*

```
CARTE À ILLUSTRER :
- Nom : Court-Circuit
- Type : Sort (illustration d'effet — pas de personnage central)
- Concept visuel : un court-circuit spectaculaire — un équipement
  électronique (serveur, armure tech, dispositif) qui prend feu de l'intérieur,
  étincelles et fumée jaillissant de toutes les fissures
- Scène souhaitée : gros plan sur l'équipement en train de griller,
  étincelles explosant en gerbes, fumée noire s'échappant, fils fondus,
  écrans qui clignotent et meurent, ambiance d'urgence électrique
- Props / éléments visuels signature : gerbes d'étincelles orange-blanc,
  fumée noire dense, câbles fondus, lueurs de court-circuit bleu-blanc,
  aucun personnage visible — juste l'effet
- Palette de couleurs : noir + orange étincelles + bleu-blanc électrique,
  fumée grise, fond très sombre, contraste fort
```

---

### Surcharge *(sort — buff sur toutes les unités alliées)*

```
CARTE À ILLUSTRER :
- Nom : Surcharge
- Type : Sort (illustration d'effet — onde d'énergie explosive)
- Concept visuel : une onde d'énergie électrique qui explose vers l'extérieur
  depuis un point central — power surge total, comme si une centrale
  électrique lâchait tout d'un coup
- Scène souhaitée : explosion d'arcs électriques partant en étoile
  depuis un centre aveuglant, énergie qui déborde, trop d'électricité
  pour être contenue, lumière bleue-blanche aveuglante au cœur
- Props / éléments visuels signature : arcs concentriques d'électricité
  en expansion, particules d'énergie dispersées, onde de choc visible,
  intensité maximale, pas de personnage — juste la puissance pure
- Palette de couleurs : blanc aveuglant au centre, bleu électrique en
  expansion, fond anthracite saturé, halos d'énergie en couches
```

---

## PROGRAMME R — Cendres *(feu / explosion)*

**Palette Cendres :** orange vif + rouge brûlé + noir fumée

---

### Bombe à Retardement *(unité — explose en mourant, dégâts zone)*

```
CARTE À ILLUSTRER :
- Nom : Bombe à Retardement
- Type : Unité (créature-bombe vivante, pas Cendres elle-même)
- Concept visuel : une créature sphérique qui EST une bombe — corps
  sombre avec veines de lave incandescentes visibles, une mèche
  qui brûle sur le crâne, yeux rouges fixes et vides, expression
  de totale acceptation de son destin
- Morphologie : silhouette compacte et ronde, petit, trapu, aucun cou
  visible, bras courts, posture ramassée comme une bombe qui attend
- Pose souhaitée : statique, centré, regardant fixement le spectateur,
  mèche qui brûle activement sur la tête, fissures de lave visibles
  sur tout le corps, chaleur irradiante visible autour
- Props / éléments visuels signature : mèche brûlante sur le crâne,
  fissures lumineuses orange-rouge sur le corps (comme de la lave),
  légère fumée montant de ces fissures, compte à rebours tacite
  dans le regard — il sait ce qu'il va faire
- Palette de couleurs : noir charbon + orange lave incandescent + rouge
  profond, fond sombre chaud, lueurs de chaleur autour du corps
```

---

### Flammèche *(sort — effet positionnel au coin)*

```
CARTE À ILLUSTRER :
- Nom : Flammèche
- Type : Sort (petit esprit de feu, illustration d'effet ou créature minuscule)
- Concept visuel : un minuscule esprit de feu vivant — une flamme
  anthropomorphique de la taille d'un poing, animée, vive, espiègle,
  qui tournoie dans l'air comme une toupie enflammée
- Scène souhaitée : la flammèche flotte au bout d'un doigt tendu
  (on voit juste la main, pas de personnage complet) ou danse seule
  dans l'air, lumineuse et concentrée, petite mais intense
- Props / éléments visuels signature : corps entièrement en feu
  mais clairement vivant (silhouette animée reconnaissable), traînée
  de chaleur derrière elle, éclat orangé qui illumine l'espace autour,
  tourbillon de braises
- Palette de couleurs : orange vif + jaune brillant + rouge cœur,
  fond très sombre pour maximiser le contraste lumineux, braises
  orange dispersées
```

---

### Embrasement *(sort — dégâts zone toutes unités adjacentes)*

```
CARTE À ILLUSTRER :
- Nom : Embrasement
- Type : Sort (illustration d'effet — conflagration totale)
- Concept visuel : une vague de feu qui dévore tout — déflagration
  massive, pas de personnage, juste la puissance du feu à son maximum
- Scène souhaitée : mur de flammes qui s'étend d'un bord à l'autre
  du cadre, intensité maximale, chaleur visible, fond consumé par
  le feu, rien ne survit à cette vague
- Props / éléments visuels signature : vague de feu tridimensionnelle
  avec profondeur, nuances de rouge / orange / jaune / blanc au cœur,
  débris embrasés qui volent, fumée noire dense au-dessus
- Palette de couleurs : orange brûlant + rouge profond + blanc intense
  au cœur des flammes, fumée noire, fond consumé noir
```

---

### Fumée Noire *(sort — affaiblit une unité ennemie)*

```
CARTE À ILLUSTRER :
- Nom : Fumée Noire
- Type : Sort (illustration d'effet — nuage d'obscurcissement)
- Concept visuel : un nuage de fumée noire et épaisse qui s'enroule
  autour d'une silhouette floue — la cible est visible en ombre
  derrière la fumée mais complètement enveloppée, neutralisée
- Scène souhaitée : fumée qui tourbillonne et étouffe, silhouette
  derrière qui essaie de voir à travers, effet d'enfermement et
  d'asphyxie, aucune flamme — juste la fumée froide et oppressante
- Props / éléments visuels signature : volutes de fumée noire dense
  avec nuances de gris-violet, silhouette à peine visible derrière
  (ombre floue), particules de suie qui flottent, lumière étouffée
- Palette de couleurs : noir charbon + gris-violet sombre, fond quasi
  noir, aucune couleur chaude — effet d'extinction
```

---

## PROGRAMME R — Le Bloc *(force brute)*

**Palette Le Bloc :** noir profond + rouge brique + gris anthracite

---

### Sbire Musclé *(unité — homme de main standard)*

```
CARTE À ILLUSTRER :
- Nom : Sbire Musclé
- Type : Unité (homme de main générique de Le Bloc, pas Le Bloc lui-même)
- Concept visuel : un sbire classique et interchangeable — casque
  intégral noir qui cache complètement le visage, uniforme d'exécutant
  sobre, musculature évidente, attitude d'obéissance totale
- Âge approximatif : indéterminé (masqué)
- Morphologie : homme, grand et musclé mais pas monstrueux — la version
  standard de brute de terrain, moins impressionnant que Le Bloc
- Pose souhaitée : bras croisés, debout de face, poids sur les deux jambes,
  posture d'attente d'ordres — pas d'expression car pas de visage visible
- Props / éléments visuels signature : casque noir intégral opaque
  (aucun oeil visible), uniforme tactique sobre avec insigne "R" petit
  et discret, gants épais, boots renforcées, aura rouge-brique légère
  autour des poings fermés
- Palette de couleurs : noir mat + rouge brique pour les accents,
  fond sombre industriel, éclairage dur et directionnel
```

---

### Mur de Chair *(unité — tank, résistance maximale)*

```
CARTE À ILLUSTRER :
- Nom : Mur de Chair
- Type : Unité (sbire/créature de Le Bloc, encore plus massif que lui)
- Concept visuel : un être humain qui est littéralement un mur —
  largeur exceptionnelle, bras écartés naturellement parce que
  les biceps empêchent de les rapprocher, jambes comme des piliers,
  aucune intention agressive — il EST juste là, il bloque
- Âge approximatif : 45 ans, vieux soldat
- Morphologie : homme, dimensions hors norme — plus large que haut
  presque, épaules qui débordent du cadre, cou inexistant, tête
  petite et carrée posée directement sur les épaules
- Pose souhaitée : de face, bras légèrement écartés (il ne peut pas
  faire autrement), jambes légèrement écartées, posture d'obstacle
  naturel, expression vide et impassible
- Props / éléments visuels signature : fissures légères dans le sol
  sous ses pieds, aura rouge-brique sur tout le corps (résistance),
  uniforme noir aux coutures apparemment testées à la limite
- Palette de couleurs : noir + rouge brique saturé pour l'aura,
  fond très sombre, éclairage rasant qui accentue le volume
```

---

### Ordre Simple *(sort — accélère une unité alliée)*

```
CARTE À ILLUSTRER :
- Nom : Ordre Simple
- Type : Sort (illustration d'un ordre donné — geste d'autorité)
- Concept visuel : un bras massif en gant noir qui pointe vers l'avant
  avec une autorité absolue — aucun doute possible sur ce que cela
  signifie, geste sans ambiguïté, sans fioritures
- Scène souhaitée : gros plan sur la main/bras (on ne voit que ça),
  doigt tendu, geste ferme et décisif, légère aura rouge autour
  du gant, fond sombre épuré
- Props / éléments visuels signature : gant opérationnel noir épais,
  aura rouge autour de la main au moment du geste, ligne d'énergie
  qui part du doigt pointé vers l'avant (direction de l'ordre)
- Palette de couleurs : noir + rouge vif pour l'énergie de l'ordre,
  fond anthracite presque noir, contraste net
```

---

### En Formation *(sort — accélère toutes les unités alliées adjacentes)*

```
CARTE À ILLUSTRER :
- Nom : En Formation
- Type : Sort (illustration d'unité tactique — groupe en mouvement)
- Concept visuel : 3 silhouettes de sbires musclés en formation en V
  qui avancent ensemble, synchronisés, énergie de cohésion entre eux
- Scène souhaitée : vue légèrement de face en contre-plongée,
  les 3 silhouettes en formation serrée qui avancent de concert,
  aura partagée rouge qui les unit, mouvement uniforme et délibéré
- Props / éléments visuels signature : les 3 silhouettes portent
  le même uniforme (sbires de Le Bloc), aura rouge-brique qui
  les entoure tous les trois comme un champ d'énergie partagé,
  sol qui vibre légèrement sous leurs pas synchronisés
- Palette de couleurs : noir + rouge brique pour l'aura collective,
  fond sombre, éclairage dramatique en contre-plongée
```

---

## PROGRAMME R — Trace *(super-vitesse)*

**Palette Trace :** vert électrique vif + gris foncé + blanc de vitesse

---

### Faux Départ *(unité — s'élance dans la mauvaise direction)*

```
CARTE À ILLUSTRER :
- Nom : Faux Départ
- Type : Unité (version comique de Trace ou coureur générique)
- Concept visuel : un sprinter en tenue légère vert électrique qui
  s'est élancé à toute vitesse... mais dans la mauvaise direction,
  réalisant son erreur exactement au moment de l'illustration —
  corps en avant lancé, tête tournée avec une expression d'horreur comique
- Âge approximatif : 20 ans environ, jeune coureur
- Morphologie : homme, silhouette très fine et élancée comme Trace
  mais en version "pas tout à fait aussi coordonné"
- Pose souhaitée : corps projeté en avant à pleine vitesse, mais
  tête et regard tournés en arrière ou de côté avec une expression
  de "oh non pas encore", traînées de vitesse vertes qui partent
  dans la mauvaise direction
- Props / éléments visuels signature : traînées de vitesse vert vif
  qui pointent manifestement dans le mauvais sens, expression
  de panique comic, semelles avec éclat cinétique, chaussure
  peut-être en train de se délacer au pire moment
- Palette de couleurs : vert électrique + gris foncé, traînées
  blanc-vert, fond flouté par la vitesse, ambiance comique rapide
```

---

### Ligne Directe *(unité — frappe immédiate, bonus si sur un bord)*

```
CARTE À ILLUSTRER :
- Nom : Ligne Directe
- Type : Unité (coureur, thème vitesse pure — peut être Trace ou générique)
- Concept visuel : un coureur qui fonce en ligne droite absolue,
  complètement focused, laser total, aucune déviation possible —
  vitesse maximale, trajectoire parfaite
- Morphologie : silhouette très fine et aérodynamique, corps incliné
  en avant à 45°, parfaite posture de sprint
- Pose souhaitée : profil de course — vu de côté, corps parfaitement
  incliné, un pied levé, bras en action de course, regard fixé sur
  un point hors-cadre devant lui, expression de concentration totale
- Props / éléments visuels signature : traînée de vitesse vert électrique
  parfaitement rectiligne derrière lui (pas de courbe), ligne d'énergie
  verte qui matérialise la trajectoire directe, distorsion de l'air
  autour du corps due à la vitesse
- Palette de couleurs : vert électrique intense + gris foncé, ligne
  de vitesse blanche rectiligne, fond flouté horizontal
```

---

### Vitesse Relative *(sort — accélère une unité ennemie pour déclencher son attaque plus tôt)*

```
CARTE À ILLUSTRER :
- Nom : Vitesse Relative
- Type : Sort (illustration d'effet — manipulation du temps/vitesse)
- Concept visuel : une horloge dont les aiguilles tournent à une
  vitesse folle, ou une distorsion temporelle autour d'une silhouette
  floue qui se retrouve forcée d'agir trop vite — relativité de
  la vitesse comme arme
- Scène souhaitée : grande horloge murale aux aiguilles filant
  à toute vitesse, lignes de distorsion temporelle autour, effet
  de "temps accéléré" — ou une silhouette dans un vortex vert
  de vitesse forcée, prise dans un courant qu'elle ne contrôle pas
- Props / éléments visuels signature : aiguilles d'horloge en
  mouvement rapide flou, distorsion de l'espace autour, lignes
  de vitesse vert qui encerclent la cible plutôt que de la propulser,
  effet piège plus que boost
- Palette de couleurs : vert électrique + blanc vif + fond sombre,
  distorsion violette pour l'aspect manipulation temporelle
```

---

### En Retard Comme D'hab *(sort — permet de piocher une carte)*

```
CARTE À ILLUSTRER :
- Nom : En Retard Comme D'hab
- Type : Sort (illustration comique — coureur qui arrive en retard)
- Concept visuel : une personne qui court à toute vitesse, montre
  au poignet bien visible indiquant un retard flagrant, valise
  ou sac qui vole, expression de panique comique totale — malgré
  la super-vitesse, il est encore en retard
- Scène souhaitée : personnage en pleine course comique, un pied
  soulevé, un bras tenant une valise qui s'ouvre et lâche des
  papiers, l'autre bras avec la montre levée vers le visage,
  fond flouté par la vitesse
- Props / éléments visuels signature : montre bien visible montrant
  une heure impossible, valise ouverte qui perd des papiers volants,
  traînées vert électrique derrière lui, expression de catastrophe
  assumée, peut-être un café renversé aussi
- Palette de couleurs : vert électrique + gris, papiers blancs
  qui volent, fond flouté urbain, ambiance course-poursuite comique
```

---

## LES CONTRACTUELS — Cartes non-héros

**Palette équipe :** bleu corporate terne + blanc + gris open-space
**Ton visuel :** fonctionnaire las avec des super-pouvoirs, désengagement professionnel total

---

### Agent Syndical *(unité — bonus positionnel au bord)*

```
CARTE À ILLUSTRER :
- Nom : Agent Syndical
- Type : Unité (personnage — représentant syndical dans l'équipe ennemie)
- Concept visuel : un représentant syndical en tenue corporate débraillée —
  veste bleue ouverte sur chemise froissée, badge délégué syndical bien
  visible, clipboard serré contre la poitrine, regard combatif de quelqu'un
  qui connaît chaque article de la convention collective par cœur
- Âge approximatif : 48 ans
- Morphologie : homme ou femme (non défini), silhouette ordinaire, légère
  bedaine, lunettes, aucune allure de super-héros — tout dans l'attitude
- Pose souhaitée : debout, légèrement penché en avant, doigt levé
  en signe d'avertissement ou de revendication, expression déterminée
  mais bureaucratique — "j'ai le droit" écrit sur le visage
- Props / éléments visuels signature : badge délégué syndical en évidence,
  clipboard avec formulaires, stylo derrière l'oreille, aura bleu
  corporatif terne autour de lui, peut-être une pile de documents
- Palette de couleurs : bleu corporate terne + blanc cassé + gris,
  fond couloir de bureau, lumière néon froide
```

---

### Consultant Externe *(unité — combo)*

```
CARTE À ILLUSTRER :
- Nom : Consultant Externe
- Type : Unité (consultant en mission chez L'Équipe Numéro Un)
- Concept visuel : un consultant en costume impeccable à 3000€/jour —
  sourire commercial parfait et légèrement vide, laptop bag en bandoulière,
  badge "CONSULTANT — ACCÈS LIMITÉ" autour du cou, regard de quelqu'un
  qui facture à l'heure et le fait savoir
- Âge approximatif : 36 ans
- Morphologie : homme ou femme, silhouette soignée et précise, posture
  droite de quelqu'un qui a fait une école de commerce, aucun souci
  apparent — le monde est son tableau Excel
- Pose souhaitée : légèrement de côté, un bras tenant le laptop bag,
  l'autre main tendue comme pour un pitch ou une poignée de main
  commerciale, sourire présent mais pas chaleureux
- Props / éléments visuels signature : badge "CONSULTANT — ACCÈS LIMITÉ"
  bien visible, laptop bag de marque discrète, aura dorée subtile
  (il coûte cher), expression de quelqu'un qui optimise tout ce qu'il voit
- Palette de couleurs : bleu corporate + gris argent pour le costume,
  badge blanc en évidence, fond salle de réunion neutre
```

---

### Note De Frais *(sort — affaiblit une unité ennemie)*

```
CARTE À ILLUSTRER :
- Nom : Note De Frais
- Type : Sort (illustration comique — paperasse administrative comme arme)
- Concept visuel : une note de frais démesurée — un long formulaire
  qui se déroule depuis le haut jusqu'en bas du cadre et au-delà,
  avec des montants astronomiques, tampons, signatures multiples,
  et cases cochées à l'infini
- Scène souhaitée : le formulaire qui se déroule comme un parchemin
  absurde jusqu'au sol et peut-être par-delà le cadre, stylo
  bille suspendu dans l'air en train de rajouter une ligne,
  ambiance de paperasse kafkaïenne
- Props / éléments visuels signature : formulaire N-ARN/47-bis visible
  en entête, montants barrés et récrits en rouge, tampons "APPROUVÉ"
  et "REFUSÉ" qui se contredisent, aura bleu corporate autour
  du document, fond de bureau désenchanté
- Palette de couleurs : blanc formulaire + bleu corporate + rouge tampon,
  fond gris de bureau, lumière artificielle froide
```

---

### Heure Sup *(sort — affaiblit fortement une unité ennemie)*

```
CARTE À ILLUSTRER :
- Nom : Heure Sup
- Type : Sort (illustration — employé épuisé contraint de continuer)
- Concept visuel : un employé épuisé à son bureau sous une lumière
  au néon crue à 22h00, horloge murale visible montrant une heure
  tardive, yeux cernés, costume desserré, qui continue de taper
  sur un clavier avec la résignation de quelqu'un qui a compris
  que ça ne finira jamais
- Scène souhaitée : vue de face sur le bureau encombré, l'employé
  légèrement éclairé par l'écran devant lui, horloge murale
  bien visible en fond, fenêtre noire qui trahit l'heure tardive,
  une tasse de café vide renversée sur le bureau
- Props / éléments visuels signature : horloge montrant 22h15,
  tasse vide renversée, pile de dossiers sur le côté, lumière
  néon froide qui écrase l'ambiance, aura bleu corporatif
  qui pulse légèrement (la contrainte de l'heure sup)
- Palette de couleurs : gris + bleu froid de néon + blanc d'écran,
  fond bureau de nuit, aucune couleur chaude
```

---

### Coup De Bureaucratie *(sort — affaiblit toutes les unités ennemies adjacentes)*

```
CARTE À ILLUSTRER :
- Nom : Coup De Bureaucratie
- Type : Sort (illustration d'effet — avalanche administrative)
- Concept visuel : une avalanche de formulaires, tampons et documents
  qui s'abattent depuis le haut du cadre — pluie de paperasse
  administrative kafkaïenne qui écrase tout, absurde et implacable
- Scène souhaitée : vue légèrement de bas — papiers et formulaires
  qui tombent en masse depuis le haut, tampons géants qui frappent,
  classeurs qui s'ouvrent et répandent leurs pages, énergie
  bureaucratique bleue qui émane de cette avalanche
- Props / éléments visuels signature : formulaires standardisés EN MASSE
  (dizaines), tampons "REFUSÉ" en rouge visible sur certains, logos
  "L'Équipe Numéro Un" sur les documents, aura bleue corporatiste
  qui pulse, ambiance étouffement administratif total
- Palette de couleurs : blanc documents + bleu corporate + rouge tampons,
  fond gris anthracite, pluie de papier qui domine le cadre
```

---

# ASSETS UI — Flèche de pose (drag carte → case)

## Contexte

Dans le jeu, quand le joueur sélectionne une carte en main, une flèche courbée apparaît
entre la carte et la case de la grille visée. L'effet cible est celui de Wildfrost :
un ruban fluide translucide (aqueux/lumineux) avec une tête de flèche stylisée au bout.

L'effet est construit en Unity avec deux sprites séparés :
1. **Tête de flèche** — affichée au bout de la courbe (chevron/flèche)
2. **Corps de flèche** — texture étirée le long de la courbe (ruban avec gradient d'opacité)

Les deux sprites doivent être **blancs sur fond transparent** pour être colorés dynamiquement
via `Image.color` ou `LineRenderer.colorGradient` en Unity.

---

## Meta-prompt — Générer les prompts pour les sprites UI de la flèche

> Copier-coller ce bloc dans une conversation Claude pour générer les prompts Photopea/Leonardo.

```
Je développe un jeu de cartes roguelike en Unity (style Wildfrost/Marvel Snap).
J'ai besoin de créer deux sprites PNG pour une flèche de pose de carte :
quand le joueur sélectionne une carte, une flèche courbée et fluide s'affiche
entre sa carte et la case de la grille visée.

RÉFÉRENCE VISUELLE CIBLE (jeu Wildfrost) :
- Ruban courbé translucide d'apparence aqueuse/cristalline, légèrement lumineux
- La queue de la flèche est fine et s'effile, la tête est un chevron blanc stylisé
- Effet "smooth ribbon" — pas une ligne rigide, plutôt un flux fluide
- Couleur générale : blanc/cyan pâle, légèrement irisé

CONTRAINTES TECHNIQUES UNITY :
- Format : PNG, fond 100% transparent
- Couleur : BLANC pur (le sprite sera teinté en runtime via Image.color)
- Sprite 1 — TÊTE DE FLÈCHE : 128×64 px, chevron pointant vers la droite
  (→), forme nette, légèrement arrondie sur les côtés. Peut avoir un léger
  glow ou emboss blanc pour donner du relief, mais rester lisible à petite taille.
- Sprite 2 — CORPS/RUBAN : 128×32 px, rectangle avec gradient d'opacité
  de gauche (opaque, 100%) à droite (transparent, 0%). Peut avoir un léger
  relief central (plus brillant au milieu, plus sombre sur les bords) pour
  simuler la forme d'un ruban 3D.

UTILISATION EN UNITY :
- La tête sera un GameObject Image positionné au bout d'un LineRenderer ou
  d'une courbe de Bézier
- Le corps sera la texture d'un LineRenderer (tiled/stretched le long de la courbe)
- Les deux seront colorés dynamiquement (blanc → couleur selon contexte)

Génère-moi :
1. Un guide Photopea étape par étape pour créer la TÊTE DE FLÈCHE (128×64 px)
2. Un guide Photopea étape par étape pour créer le CORPS/RUBAN (128×32 px)
3. Si Leonardo AI peut générer ces assets UI : le prompt adapté pour chaque sprite
4. Des alternatives simples si je veux les faire entièrement en code Unity
   (LineRenderer natif, GL.Lines, etc.) sans sprite externe
```




