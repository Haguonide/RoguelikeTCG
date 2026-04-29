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

Le combat se joue sur une **grille 4×4 partagée** entre le joueur et l'ennemi, comme un Tic-Tac-Toe.

- **16 cases**, toutes accessibles aux deux joueurs
- **1 unité maximum par case**
- Une case libérée par la mort d'une unité peut être réoccupée
- **Coin flip** en début de combat pour déterminer qui joue en premier
- **1 HP bar par camp** : joueur (global à tous les combats), ennemi (spécifique au combat)
- **Victoire** : HP ennemi tombe à zéro. **Défaite** : HP joueur global tombe à zéro
- Les **intentions ennemies ne sont pas visibles**

### Structure d'un tour

1. Le joueur joue **1 unité maximum** (sur une case vide) + **autant de cartes** (sorts, utilitaires) que son mana le permet
2. Le joueur clique **"Fin de Tour"**
3. Le **compte à rebours** de chaque unité en jeu descend de 1
4. Les unités dont le compte à rebours atteint **0 attaquent** (kill instantané), puis leur CD repart à sa valeur max
5. L'ennemi joue son tour (même structure)
6. Répéter jusqu'à **10 tours par joueur** = **1 manche**

### Anatomie d'une carte unité (format carré)

| Champ | Description |
|---|---|
| **CD (Compte à rebours)** | Tours avant la première attaque (1–4) |
| **Flèches (1–4)** | Directions des cases adjacentes attaquées |
| **Keyword** | Optionnel |
| **Coût mana** | Commun unités et sorts |

> ⚠️ **Pas d'ATK ni de HP sur les unités.** Les kills sont instantanés.

### Mécanique d'attaque

Quand le compte à rebours d'une unité atteint 0 :
- Elle attaque toutes les **cases adjacentes pointées par ses flèches**
- **Unité ennemie** sur une case ciblée → **détruite instantanément** (va en défausse)
- **Case vide ou unité alliée** → aucun effet
- L'unité attaquante **reste sur la grille**, CD repart à sa valeur initiale

### Système de scoring

**Score immédiat à la pose** — les points sont encaissés au moment du placement, pas en fin de manche.

| Combinaison | Points |
|---|---|
| 3 unités alliées en ligne (horizontal/vertical) | 2 pts |
| 3 unités alliées en diagonale | 3 pts |
| Carré 2×2 d'unités alliées | 2 pts |
| Tuer une unité ennemie | 1 pt (immédiat au kill) |

**Règles :**
- Chaque combinaison spécifique (les 3 cases exactes) ne peut scorer **qu'une fois par manche**
- Les unités restent en jeu après avoir scoré — elles peuvent participer à d'**autres combinaisons**
- Le keyword **Combo** ajoute +1 pt bonus si le placement complète une combinaison

### Résolution d'une manche

- Fin de manche : toutes les unités survivantes → **défausse**
- Points comparés : le gagnant inflige **(ses points − points adverses) dégâts** aux HP ennemis
- Si égalité : aucun dégât
- **Le deck ne se réinitialise pas** entre les manches
- Les manches se répètent jusqu'à ce qu'un camp tombe à 0 HP

### Cartes

- **3 types** : Unités, Sorts, Utilitaires
- **Mana unifié** : toutes les cartes coûtent du mana
- **Unités** : posées sur une case vide de la grille, CD/Flèches/Keyword
- **Sorts** : effets instantanés (buffs, débuffs, contrôle de board), ne prennent pas de case
- **Utilitaires** : cartes neutres limitées à 2 exemplaires par deck, jamais upgradables :
  - **Carte Déplacement** (coût 1 mana) : déplace une unité alliée vers une case adjacente vide, reset son CD
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

### Keywords (10 validés)

| Keyword | Description |
|---|---|
| **Hâte** | Le compte à rebours démarre à 1 (frappe le tour suivant) |
| **Bouclier** | Survit à la première attaque reçue cette manche |
| **Épine** | À la mort, détruit une unité ennemie adjacente au choix |
| **Explosion** | À la mort, détruit toutes les unités adjacentes (alliées + ennemies) |
| **Combo** | Si ce placement complète une ligne/diagonale/carré → +1 pt bonus |
| **Inspiration** | À la pose, pioche 1 carte |
| **Légion** | CD -1 par unité alliée adjacente (minimum 1) |
| **Dominance** | Si encore en vie en fin de manche, +1 pt |
| **Percée** | Si tue une unité ennemie, attaque aussi la case derrière dans la même direction |
| **Ralliement** | À la pose, -1 CD à toutes les unités alliées adjacentes |

### IA ennemie

- Joue **1 unité** (priorité : cases qui menacent les unités adverses ou complètent une ligne IA)
- Stratégie spécifique par team ennemie
- Les intentions ne sont **pas visibles**

---

## 🖥️ Interface de combat

### Layout cible

```
[ Or | Reliques | Score joueur vs Score ennemi ]     ← barre haut (full width)

[Portrait joueur]  [  Grille 4×4 partagée  ]  [Portrait ennemi]
     HP ↓          [ 16 cases carrées       ]       HP ↓

[Mana / Deck / Défausse]       [ Main du joueur ]   [Fin de Tour]
      ↑ bas-gauche               ↑ centré bas          ↑ bas-droite
```

### Hiérarchie Canvas (scène Combat)

```
Canvas/
  FullBG                      — fond plein écran
  GridArea                    — grille 4×4, anchor centré
    Row_0..3                  — 4 lignes × 4 cases (GridCellUI câblés)
    GridLines                 — lignes de grille (GridLinesDrawer, raycastTarget=false)
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
- **Flèches d'attaque** : affichées sur chaque carte posée
- **Compte à rebours** : affiché sur chaque unité posée, décrémente à chaque tour
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
**Keywords naturels :** Hâte, Percée, Explosion, Ralliement

| Héros | Pouvoir | Personnalité | Rôle |
|---|---|---|---|
| **Voltaire** | Électricité | Se croit le plus intelligent, cite des philosophes à contresens | Capitaine |
| **Cendres** | Feu / explosion | Traite tout ça comme un job alimentaire, zéro remords | |
| **Le Bloc** | Force brute | Suit les ordres, mange beaucoup, pose pas de questions | |
| **Trace** | Super-vitesse | Toujours en retard malgré sa vitesse supersonique | |

#### Les Éternels — Archétype : Combo / Placement

**Concept :** Vieux super-héros sortis de retraite contraints et forcés. Se chamaillent en permanence.
**Keywords naturels :** Combo, Inspiration, Dominance, Ralliement

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

> À designer — les cartes doivent respecter la nouvelle anatomie : CD / Flèches / Keyword / Coût. Pas d'ATK ni de HP.
> Archétypes validés : Programme R (Aggro — Hâte/Percée/Explosion/Ralliement) et Les Éternels (Combo — Combo/Inspiration/Dominance/Ralliement).

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
- Scène Combat : grille 4×4, portraits, layout bas (mana/deck/défausse), GridLinesDrawer ✅

### 🔄 Priorités actuelles

1. **Redesigner les decks** Programme R et Les Éternels (CD / Flèches / Keyword, pas d'ATK/HP)
2. **Réécrire le système de combat** : kills instantanés, scoring à la pose, cartes Déplacement/Repioche
3. **Adapter les CardData ScriptableObjects** à la nouvelle anatomie (supprimer ATK/HP)
4. **IA ennemie** (Les Contractuels)
5. **Animations** : pose sur grille, kill, score popup

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
