# CLAUDE.md — Contexte du projet : Roguelike Deckbuilder (Super-héros Burlesque)

---

## ⚠️ RÈGLES ABSOLUES DE TRAVAIL

- **Ne jamais modifier des fichiers pendant qu'Unity est en Play Mode.** En dehors du Play Mode, Unity peut rester ouvert sans problème.
- **Ne jamais toucher aux dossiers `Library/`, `Temp/`, `obj/`**. Ces dossiers sont gérés exclusivement par Unity.
- **Ne jamais modifier directement les fichiers `.unity` (scènes) ou `.prefab`** via un éditeur de texte externe. Toute manipulation de scène passe obligatoirement par **Unity MCP**.
- **Respecter l'architecture des dossiers définie ci-dessous.** Ne pas créer de fichiers en dehors des emplacements prévus.

### Workflow obligatoire pour chaque feature

1. **Écrire tous les scripts C#** nécessaires à la feature
2. **Attendre confirmation** que Unity a compilé sans erreur
3. **Configurer la scène via MCP** : créer les GameObjects, attacher les scripts, régler les valeurs dans l'Inspector
4. **Signaler les références manuelles restantes** : si une référence ne peut pas être liée via MCP (ex: glisser-déposer complexe entre objets), lister explicitement ce que le développeur doit faire manuellement dans Unity

### Ce que Claude Code peut faire via MCP
- ✅ Créer / supprimer / renommer des GameObjects
- ✅ Attacher des scripts à des GameObjects
- ✅ Modifier des valeurs simples dans l'Inspector (int, float, string, bool)
- ✅ Exécuter des menu items Unity
- ✅ Créer la structure de la Hierarchy

### Ce qui nécessite une intervention manuelle du développeur
- ⚠️ Lier des références entre objets (glisser un objet dans un champ d'un autre)
- ⚠️ Configurer des Canvas UI complexes
- ⚠️ Créer et configurer des Prefabs
- ⚠️ Toute opération nécessitant un drag & drop dans l'Inspector

---

## 🎮 Description générale du jeu

Roguelike deckbuilder stratégique, inspiré de **Slay the Spire** et **Wildfrost**.

- **Univers** : Super-héros dysfonctionnels et burlesque. Ton Dispatch (workplace comedy meets superheroes) — ex-vilains en liberté conditionnelle, héros sortis de retraite, supers sous CDI qui font ça comme un job 9h-17h. L'antagoniste final : **L'Équipe Numéro Un**, qui a corporatisé le héroïsme.
- **Ton** : Absurde assumé. Humour noir, burlesque maîtrisé. Les ennemis ne sont jamais "evil" — ils sont des agents d'une logique corporative absurde.
- **Structure narrative** : 5 chapitres, chacun correspondant à un département de L'Équipe Numéro Un. Prototype : 1 chapitre fonctionnel.
- **Style graphique** : Cartes carrées, style Marvel Snap (flat cartoon dynamique), palette forte par team. Lisibilité avant tout.

---

## ⚔️ Système de combat

### Structure de la grille

Le combat se joue sur une **grille 3×3 partagée** entre le joueur et l'ennemi.

- **9 cases**, toutes accessibles aux deux joueurs
- **1 unité maximum par case**
- Une case libérée par la mort d'une unité peut être réoccupée
- **Coin flip** en début de combat pour déterminer qui joue en premier
- **1 HP bar par camp** : joueur (global à tous les combats), ennemi (spécifique au combat)
- **Victoire** : HP ennemi tombe à zéro. **Défaite** : HP joueur global tombe à zéro
- Les **intentions ennemies ne sont pas visibles**

Numérotation des cases (référence interne) :
```
1 2 3
4 5 6
7 8 9
```

### Structure d'un tour

1. Le joueur joue **1 unité maximum** (sur une case vide) — **l'unité attaque immédiatement** dans ses directions à la pose
2. Le joueur joue **autant de sorts/utilitaires** que son mana le permet
3. Le joueur clique **"Fin de Tour"**
4. L'ennemi joue son tour (même structure)
5. Répéter jusqu'à **6 tours max par joueur** = **1 manche**

### Anatomie d'une carte unité (format carré)

| Champ | Description |
|---|---|
| **HP (1–3)** | Icônes gouttes : remplies = HP actuel, contour vide = HP perdu |
| **ATK** | Toujours 1 — pas de stat affichée, représenté par les flèches directionnelles avec glow récurrent |
| **Flèches (1–4)** | Directions d'attaque (à la pose uniquement). Option paramètres : affichage permanent ou survol uniquement |
| **Passif positionnel** | Optionnel. Effet déclenché si l'unité est sur coin / bord / centre. Indiqué par mini-schéma grille 3×3 sur la carte |
| **Keyword** | Optionnel — effet permanent toujours actif |
| **Coût mana** | Commun unités et sorts |

> ℹ️ **ATK est toujours 1.** Si un boost est actif (passif positionnel ou sort), une flèche rouge scintillante apparaît sur la carte (grisée quand le passif est inactif mais toujours visible pour indiquer sa présence).

### Passifs positionnels

Couche stratégique indépendante des keywords — une carte peut avoir les deux simultanément.

- **Condition absolue** : coin (cases 1,3,7,9) / bord (cases 2,4,6,8) / centre (case 5)
- **Dynamique** : si l'unité est déplacée (Carte Déplacement), le passif s'active ou se désactive immédiatement
- **Feedback visuel** : particules vertes montantes + son à l'activation / particules rouges-grises descendantes + son à la désactivation
- **Effets possibles** : +1 ATK (ATK=2 à la pose), pioche 1 carte, +1 pt, etc.

### Mécanique d'attaque

Une unité attaque **une seule fois, au moment de sa pose** :
- Elle attaque toutes les **cases pointées par ses flèches**
- **Unité ennemie** sur une case ciblée → **perd 1 HP** (ou 2 si ATK boostée). Si HP = 0 → détruite (va en défausse)
- **Case vide ou unité alliée** → aucun effet
- L'unité attaquante **reste sur la grille** comme pièce positionnelle jusqu'à la fin de la manche

**Sorts de buff ATK** : "la prochaine unité jouée gagne +1 ATK" — le buff se consomme à la pose de l'unité suivante, qu'elle touche ou non une cible.

### Système de scoring

**Score immédiat à la pose** — les points sont encaissés au moment du placement, pas en fin de manche.

| Événement | Points |
|---|---|
| Compléter un motif 3 cases | 4 pts |
| Compléter un motif 4 cases | 6 pts |
| Compléter un motif 5 cases | 9 pts |
| Tuer une unité ennemie | 1 pt (immédiat au kill) |

**Motifs de combat :**
- **3 motifs** sont tirés aléatoirement dans la banque au début de chaque combat
- Ils sont **communs aux deux joueurs** et **fixes pour tout le combat**
- **Premier arrivé premier servi** : le premier joueur qui complète un motif le score et le **ferme pour la manche**
- Un motif fermé se **réouvre au début de la manche suivante** (les deux joueurs peuvent le rechasser)
- Chaque motif peut ainsi être scoré **une fois par joueur par manche** mais pas deux fois par le même joueur
- Le tirage garantit **maximum 2 motifs utilisant la case centrale (5)** par combat

**Banque de motifs :**

*3 cases — 4 pts (8 motifs)*
```
Ligne H    Col V      Diag ↘     Diag ↗     Coin TL    Coin TR    Coin BL    Coin BR
X X X      X . .      X . .      . . X      X X .      . X X      . . .      . . .
. . .      X . .      . X .      . X .      X . .      . . X      X . .      . . X
. . .      X . .      . . X      X . .      . . .      . . .      X X .      . X X
(×3 lignes)(×3 cols)
```
*(les 3 lignes et 3 colonnes sont 6 motifs distincts + 2 diagonales + 4 coins = 12 motifs 3 cases au total)*

*4 cases — 6 pts (10 motifs)*
```
Carré TL   Carré TR   Carré BL   Carré BR   4 Coins
X X .      . X X      . . .      . . .      X . X
X X .      . X X      X X .      . X X      . . .
. . .      . . .      X X .      . X X      X . X

T haut     T bas      T gauche   T droite   L (×4 rotations)
. X .      . . .      . X .      . X .      X . .
X X X      X X X      X X .      . X X      X . .
. . .      . X .      . X .      . X .      X X .
```

*5 cases — 9 pts (5 motifs)*
```
Croix      X total    U haut     U bas      Z
. X .      X . X      X . X      . . .      X X .
X X X      . X .      X . X      X . X      . X .
. X .      X . X      X X X      X X X      . X X
```

**Règles supplémentaires :**
- Les unités restent en jeu après avoir contribué à un motif scoré
- Le keyword **Combo** ajoute +1 pt bonus si le placement complète un motif

### Résolution d'une manche

- Fin de manche : toutes les unités survivantes → **défausse**
- Points comparés : le gagnant inflige **(ses points − points adverses) dégâts** aux HP ennemis
- Si égalité : aucun dégât
- **Le deck ne se réinitialise pas** entre les manches
- Les manches se répètent jusqu'à ce qu'un camp tombe à 0 HP

### Cartes

- **3 types** : Unités, Sorts, Utilitaires
- **Mana unifié** : toutes les cartes coûtent du mana
- **Unités** : posées sur une case vide de la grille, attaquent à la pose selon leurs Flèches, puis restent comme pièce positionnelle
- **Sorts** : effets instantanés (buffs, débuffs, contrôle de board), ne prennent pas de case
- **Utilitaires** : cartes neutres limitées à 2 exemplaires par deck, jamais upgradables :
  - **Carte Déplacement** (coût 1 mana) : déplace une unité alliée vers une case adjacente vide (l'unité n'attaque pas à nouveau)
  - **Carte Repioche** : mélange la main actuelle dans le deck et pioche autant de cartes
- **Raretés** : Commune / Rare / Épique / Légendaire
- Les cartes peuvent être **upgradées** via la Forge (3 copies identiques → 1 carte +)

### Deck et main

- Deck initial : **20 cartes minimum**
- **5 cartes** en main au démarrage
- **1 carte piochée** au début de chaque tour joueur
- Main maximale : **10 cartes**
- Quand le deck est vide, la **défausse** est mélangée pour reformer un nouveau deck
- Pas de cimetière — toutes les unités mortes vont en défausse

### Mana

- **Mana croissant** : 1 au tour 1 de chaque manche, +1 par tour joueur, **plafond à 6**
- Se **régénère entièrement** au début de chaque manche
- Commun à tous les types de cartes

### Keywords (9 validés)

| Keyword | Description | Rareté min |
|---|---|---|
| **Impact** | À la pose, inflige 1 dégât supplémentaire à la première cible touchée | Libre |
| **Épine** | À la mort, inflige 1 dégât à l'unité attaquante | **Rare+** |
| **Explosion** | À la mort, inflige 1 dégât à toutes les unités adjacentes (alliées + ennemies) | **Rare+** |
| **Combo** | Si ce placement complète un motif actif → +1 pt bonus | Libre |
| **Inspiration** | À la pose, pioche 1 carte | Libre |
| **Essaim** | +1 ATK à la pose par unité alliée adjacente | Libre |
| **Dominance** | Si encore en vie en fin de manche, +1 pt | Libre |
| **Percée** | Si tue une unité ennemie (HP=0), attaque aussi la case derrière dans la même direction | Libre |
| **Réveil** | À la pose, chaque unité alliée adjacente attaque à nouveau dans ses directions | Libre |

> **Bouclier supprimé** — rendu redondant par le système HP. Épine et Explosion restreintes Rare+ : empêche une commune de counter une légendaire via kill automatique. **Hâte/Légion/Ralliement supprimés** (liés au CD, système retiré) → remplacés par Impact, Essaim, Réveil.

### IA ennemie

- Joue **1 unité** (priorité : cases qui complètent un motif actif pour l'IA, ou qui bloquent un motif que le joueur est en train de construire)
- Stratégie spécifique par team ennemie
- Les intentions ne sont **pas visibles**

---

## 🖥️ Interface de combat

### Layout cible

```
[ Or | Reliques | Score joueur vs Score ennemi ]     ← barre haut (full width)

[Portrait joueur]  [  Grille 3×3 partagée  ]  [Portrait ennemi]
     HP ↓          [  9 cases carrées       ]       HP ↓
                   [ Motifs actifs (×3)     ]

[Mana / Deck / Défausse]       [ Main du joueur ]   [Fin de Tour]
      ↑ bas-gauche               ↑ centré bas          ↑ bas-droite
```

### Hiérarchie Canvas (scène Combat)

```
Canvas/
  FullBG                      — fond plein écran
  GridArea                    — grille 3×3, anchor centré
    Row_0..2                  — 3 lignes × 3 cases (GridCellUI câblés)
    GridLines                 — lignes de grille (GridLinesDrawer, raycastTarget=false)
    PatternOverlay            — surbrillance des cases cibles des 3 motifs actifs
  PatternDisplay              — affichage des 3 motifs du combat (icônes + statut ouvert/fermé)
  PortraitPlayer              — anchor gauche
    HPLabel / HPText (TMP vert)
  PortraitEnemy               — anchor droite
    HPLabel / HPText (TMP rouge)
  ScoreBar                    — score joueur / score ennemi en temps réel
  BottomInfo                  — bas-gauche : ManaText / DeckText / DiscardText
  Hand                        — main joueur, centré bas
  EndTurnButton               — bas-droite
  CombatUI                    — GO logique, toutes refs TMP câblées
  RelicBar                    — barre or/reliques
```

- **Clic droit sur une carte** : affiche une copie agrandie (CardZoomPanel)
- **Flèches d'attaque** : affichées sur chaque carte posée (indiquent les directions de l'attaque à la pose)
- **Cases slots** : invisibles (fond transparent), grille délimitée par traits via GridLinesDrawer

---

## 🗺️ Système de progression (Roguelike)

### Carte de run

- Forme : **carte avec chemins multiples** (style Slay the Spire).
- **Entièrement visible** dès le début du run.
- Un nœud visité est **bloqué définitivement**.
- Chapitre introductif : **10 lignes**, **maximum 4 nœuds par ligne**.
- **Arbre vertical scrollable**.

### Affichage des nœuds

- **Gris** : nœuds inaccessibles
- **Vert foncé** : nœuds déjà visités
- **Vert clair** : nœuds disponibles (accessibles maintenant)
- Lignes reliant les nœuds, icône simple pour chaque type d'événement

### Types de nœuds

| Nœud | Récompense |
|---|---|
| Combat normal | Choix de cartes + or |
| Combat élite (mini-boss) | Relique + or |
| Boss (fin de chapitre) | Relique + or |
| Événement narratif | Texte + choix avec conséquences |
| Marchand | Acheter carte / relique, vendre carte du deck |
| Forge | Améliorer une carte (3 copies → 1 carte +) |
| Repos / Soin | Gain de HP + suppression d'une carte du deck |
| Mystère | Inconnu jusqu'à l'arrivée |

### Marchand

- Acheter une carte (du pool de la team jouée, cartes de base uniquement)
- Acheter une relique
- Vendre une carte du deck
- (La forge est un nœud séparé)

### Récompenses de combat

- Après victoire : choisir 1 carte parmi 3 proposées (versions de base uniquement — jamais upgradée)
- Bouton "Vendre" sous chaque carte : Commune 25 or, Rare 50 or, Épique 75 or
- Les cartes upgradées (+) s'obtiennent **exclusivement à la Forge**

### Forge — Système de fusion

- Déposer **3 copies identiques** → obtenir **1 exemplaire upgradé (+)**
- **Coût en or : aucun** — le coût c'est le sacrifice des 3 copies
- **Fusion bloquée si le deck descend sous 20 cartes**
- **Une seule chaîne d'upgrade** : Normal → + (pas de ++)

---

## 🏆 Structure d'un Run

- **5 chapitres**, chacun = un département de L'Équipe Numéro Un
- **Chapitre 1** : Département Juridique & Conformité (prototype)
- **Priorité** : chapitre 1 100% fonctionnel avant d'étendre

---

## 💎 Reliques

- Obtenues via combats élite, boss, et leveling de compte
- Effets : `DrawExtraCardPerTurn`, `StartWithBonusMana`, `MaxHPBonus`, `HealAfterCombat`
- Relique débloquable au mini-boss du chapitre intro (overlay de sélection)

---

## 🦸 Personnages

### Teams jouables

#### Programme R — Archétype : Aggro

**Concept :** Ex-vilains en liberté conditionnelle. Suicide Squad / Thunderbolts style. Personne n'a vraiment envie d'être là.
**Keywords naturels :** Impact, Percée, Explosion, Essaim

| Héros | Pouvoir | Personnalité | Rôle |
|---|---|---|---|
| **Voltaire** | Électricité | Se croit le plus intelligent, cite des philosophes à contresens | Capitaine |
| **Cendres** | Feu / explosion | Traite tout ça comme un job alimentaire, zéro remords | |
| **Le Bloc** | Force brute | Suit les ordres, mange beaucoup, pose pas de questions | |
| **Trace** | Super-vitesse | Toujours en retard malgré sa vitesse supersonique | |

#### Les Éternels — Archétype : Combo / Placement

**Concept :** Vieux super-héros sortis de retraite contraints et forcés. Se chamaillent en permanence.
**Keywords naturels :** Combo, Inspiration, Dominance, Réveil

| Héros | Pouvoir | Personnalité | Rôle |
|---|---|---|---|
| **Aciera** | Magnétisme | 74 ans, pragmatique, légèrement terrifiante. Tricote entre deux combats. | Capitaine |
| **Le Maître** | Télékinésie | Calme absolu, parle peu mais juste. A tout vu, rien ne l'étonne. | |
| **Titanio** | Duplication | Raconte ses exploits des années 60 en boucle. Se froisse le dos en combat. | |
| **Glamoura** | Illusion | Utilise l'argot des jeunes à contresens. Ses illusions ressemblent à elle en 1968. | |

### Teams ennemies (prototype)

#### Les Contractuels — combats normaux + élite
Super-héros sous CDI. Pouvoirs réels, motivation inexistante. Font ça comme un job 9h-17h.

| Héros | Pouvoir | Comportement |
|---|---|---|
| **Patrice** | Super-force | 60% de puissance max, préserve son dos |
| **Régine** | Téléportation | S'en sert principalement pour aller chercher son café |
| **Chad** | Boucliers énergétiques | Part à 17h01 peu importe l'état du combat |
| **Marlène** | Duplication | Crée des copies pour glander pendant qu'elles bossent |

#### Les Acquisitions — mini-boss + boss
Équipe d'élite. Professionnels, froids, costume trois pièces. Objectif : "faire une offre".

| Héros | Pouvoir | Rôle |
|---|---|---|
| **Le Partenaire** | Persuasion mentale | Parle de "synergie" et "d'opportunité" |
| **La Clause** | Binding / entrave | "Termes et conditions" |
| **L'Évaluateur** | Scan / analyse | Évalue chaque unité en "valeur de rachat" |
| **Le Liquidateur** | Destruction pure | Activé uniquement si l'acquisition échoue |

### Structure deck par team

- **4 héros × 7 cartes** (1 carte héros ×1 + 1 unité ×1 + 1 unité ×2 + 1 sort ×1 + 1 sort ×2) = 28 cartes
- **+ 2 Cartes Repioche** = **30 cartes total**
- Les Cartes Déplacement sont optionnelles (max 2, remplacent d'autres cartes)

---

## 🃏 Decks

> À designer — les cartes doivent respecter la nouvelle anatomie : HP (1-3) / ATK implicite=1 / Flèches (attaque à la pose) / Passif positionnel (optionnel) / Keyword (optionnel) / Coût.
> Archétypes validés : Programme R (Aggro — Impact/Percée/Explosion/Essaim) et Les Éternels (Combo — Combo/Inspiration/Dominance/Réveil).
> Cible : 4 decks jouables total (2 pour le prototype).

---

## 💾 Sauvegarde

- La **run est sauvegardée** à chaque action importante (`run_save.json` dans `persistentDataPath`)
- Progression de compte sauvegardée séparément (`account_save.json`)

---

## 🗂️ Architecture des dossiers Unity

```
Assets/
├── Art/
│   ├── Cards/
│   │   ├── Units/
│   │   └── Spells/
│   ├── Characters/
│   │   ├── ProgrammeR/
│   │   │   ├── Voltaire/
│   │   │   ├── Cendres/
│   │   │   ├── LeBloc/
│   │   │   └── Trace/
│   │   ├── LesEternels/
│   │   │   ├── Aciera/
│   │   │   ├── LeMaitre/
│   │   │   ├── Titanio/
│   │   │   └── Glamoura/
│   │   └── Enemies/
│   ├── UI/
│   ├── Boards/
│   ├── Icons/
│   │   └── NodeIcons/
│   └── Effects/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Data/
│   ├── Cards/
│   ├── Relics/
│   ├── Characters/
│   └── Events/
├── Prefabs/
│   ├── Cards/
│   ├── UI/
│   ├── Boards/
│   └── Nodes/
├── Scenes/
│   ├── MainMenu/
│   ├── Combat/
│   └── RunMap/
├── Scripts/
│   ├── Combat/
│   ├── Cards/
│   ├── AI/
│   ├── RunMap/
│   ├── Data/
│   ├── UI/
│   ├── SaveSystem/
│   └── Core/
├── Resources/
└── Plugins/
```

---

## 📋 État d'avancement et priorités

### ✅ Réalisé

- Scène RunMap (arbre de progression, nœuds, scroll, animation intro, états visuels) ✅
- Scènes MainMenu + CharacterSelect ✅
- Nœuds non-combat (Rest, Forge, Shop, Event, Mystery) — 12 événements narratifs ✅
- Système or, reliques (avec tooltip hover), sauvegarde disque, leveling de compte ✅
- SessionLogger ✅
- Scène Combat : grille 3×3, portraits, layout bas (mana/deck/défausse), GridLinesDrawer ✅

### 🔄 Priorités actuelles

1. **Adapter les CardData ScriptableObjects** : ajouter HP (1-3), passif positionnel (condition + effet), retirer CD et Bouclier, mettre à jour keywords (Impact/Essaim/Réveil), Épine/Explosion (Rare+, 1 dégât)
2. **Réécrire le système de combat** : attaque à la pose (plus de CD), HP sur les unités, dégâts 1 ou 2 si boost, mort à HP=0, passifs positionnels dynamiques, 6 tours max, mana croissant
3. **Implémenter les passifs positionnels** : détection coin/bord/centre, activation/désactivation au déplacement, feedback visuel (particules + son)
4. **Implémenter le système de motifs** : banque ScriptableObject, tirage aléatoire, scoring à la pose, logique premier arrivé premier servi
5. **Redesigner les decks** Programme R et Les Éternels (2 prototype, 4 cible total)
6. **IA ennemie** (Les Contractuels) — priorité cases complétant un motif ou bloquant le joueur
7. **Animations** : pose sur grille, HP loss, kill, score popup, motif complété, activation/désactivation passif positionnel

### 📌 Post-prototype

- Team 3+ (roster étendu)
- Chapitres 2–5
- Système d'Ascension
- L'Équipe Numéro Un (boss final)

---

## 🔧 Notes techniques

- Tout le code est en **C# Unity**, from scratch. Namespaces : `RoguelikeTCG.Combat`, `RoguelikeTCG.RunMap`, `RoguelikeTCG.UI`, `RoguelikeTCG.Core`, `RoguelikeTCG.Data`.
- **ScriptableObjects** pour toutes les données de jeu (cartes, reliques, personnages, événements).
- **DOTween** pour toutes les animations — jamais de Coroutine pour les tweens.
- **UI toujours construite dans la scène** (via MCP), jamais en code dans `Start()`. Scripts = logique pure avec refs public assignées depuis la scène.
- Les visuels (images) sont déposés **manuellement par le développeur** dans les dossiers `Art/` prévus.
- Sauvegarder les scènes avec `Path` explicite (ex: `Scenes/Combat/`) pour éviter les doublons.

### Agents Claude disponibles (`.claude/agents/`)

| Agent | Usage |
|---|---|
| `combat-coder` | Toute feature du système de combat |
| `ui-flow-coder` | RunMap, menus, nodes, sauvegarde, futurs écrans |
| `card-balancer` | Design et équilibrage des cartes / decks |
| `unity-builder` | Configuration scènes via Unity MCP |
| `qa-validator` | Vérification features vs design doc |
