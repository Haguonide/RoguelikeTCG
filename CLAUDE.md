# CLAUDE.md — Contexte du projet : Roguelike Deckbuilder (Tournoi des Figures Historiques)

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

Roguelike deckbuilder de type stratégique, inspiré de **Slay the Spire** et **The Royal Writ**.

- **Univers** : Un dieu fatigué organise un tournoi de cartes pour trouver son successeur. Les participants sont des figures historiques réinterprétées de façon absurde et anachronique (ex : Léonard de Vinci inventeur alcoolique, Jules César agent immobilier "Veni Vidi Vendu", Marie Curie baronne de la drogue…).
- **Ton** : Absurde assumé. Humour noir, anachronisme volontaire, grotesque maîtrisé.
- **Structure narrative** : 5 chapitres organisés autour de 4 régions du royaume (Le Vice, Le Contrôle, La Guerre, Le Spectacle). Boss final : Jules César. Destination finale : le Bureau du Dieu.
- **Style graphique** : En cours de définition. Direction retenue : **cartes carrées** (format adapté à la grille 4×4), style **flat + minimal** avec une palette de couleurs forte par personnage. Fond de grille sobre, flèches d'attaque lisibles sur les cartes. Une couleur dominante par personnage (De Vinci : bleu/cuivre, Curie : vert/jaune, César : rouge/or). Lisibilité avant tout — pas de template complexe. Cohérence graphique stricte sur tous les éléments.

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

1. Le joueur joue **1 unité maximum** (sur une case vide) + **autant de sorts** que son mana le permet
2. Le joueur clique **"Fin de Tour"**
3. Le **compte à rebours** de chaque unité en jeu descend de 1
4. Les unités dont le compte à rebours atteint **0 attaquent** (voir ci-dessous), puis leur CD repart à sa valeur max
5. L'ennemi joue son tour (même structure)
6. Répéter jusqu'à **10 tours par joueur** (20 actions totales alternées) = **1 manche**

### Anatomie d'une carte unité (format carré)

| Champ | Description |
|---|---|
| **ATK** | Dégâts infligés lors d'une attaque |
| **HP** | Points de vie |
| **CD (Compte à rebours)** | Tours avant la première attaque (1–4) |
| **Flèches (1–4)** | Directions des cases adjacentes attaquées |
| **Keyword** | Optionnel |
| **Coût mana** | Commun unités et sorts |

### Mécanique d'attaque

Quand le compte à rebours d'une unité atteint 0 :
- Elle attaque toutes les **cases adjacentes pointées par ses flèches**
- **Unité ennemie** sur une case ciblée → reçoit les dégâts (peut mourir)
- **Case vide ou unité alliée** → aucun effet
- Le CD repart à sa valeur initiale (l'unité continue de menacer)

### Système de scoring

**Ce qui rapporte des points :**

| Combinaison | Points |
|---|---|
| 3 unités alliées en ligne (horizontal/vertical) | 2 pts |
| 3 unités alliées en diagonale | 3 pts |
| Carré 2×2 d'unités alliées | 2 pts |
| Tuer une unité ennemie | 1 pt |

**Règles :**
- Une combinaison score **immédiatement** dès qu'elle est complétée
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

- **2 types** : Unités et Sorts
- **Mana unifié** : unités ET sorts coûtent du mana
- **Unités** : posées sur une case vide de la grille, ATK/HP/CD/Flèches
- **Sorts** : effets instantanés (dégâts, soins, buffs, débuffs), ne prennent pas de case
- **Raretés** : Commune / Rare / Épique / Légendaire
- Les cartes peuvent être **upgradées** (version +)

### Deck et main

- Deck initial : **20 cartes** (minimum 20 cartes en permanence)
- **5 cartes** en main au démarrage
- **1 carte piochée** au début de chaque tour joueur
- Main maximale : **10 cartes**
- Quand le deck est vide, la **défausse** est mélangée pour reformer un nouveau deck
- Il n'y a **pas de cimetière** — toutes les unités mortes vont en défausse

### Mana

- **Mana croissant** : 1 au tour 1 de chaque manche, +1 par tour joueur, **plafond à 6**
- Se **régénère entièrement** au début de chaque manche
- Commun aux unités et aux sorts

### Keywords (10 validés)

| Keyword | Description |
|---|---|
| **Hâte** | Le compte à rebours démarre à 1 (frappe le tour suivant) |
| **Blindage** | -1 dégât reçu de toutes sources |
| **Épine** | À la mort, X dégâts aux unités **ennemies** adjacentes |
| **Explosion** | À la mort, X dégâts à **toutes** les unités adjacentes (alliés compris) |
| **Combo** | Si ce placement complète une ligne/diagonale/carré → +1 pt bonus |
| **Inspiration** | À la pose, pioche 1 carte |
| **Légion** | +1 ATK par unité alliée adjacente (calculé au moment de l'attaque) |
| **Dominance** | Si encore vivante à la fin de la manche, +1 pt |
| **Percée** | Si tue une unité ennemie, attaque aussi la case derrière dans la même direction |
| **Ralliement** | À la pose, +1 ATK à toutes les unités alliées adjacentes |

### IA ennemie

- Joue **1 unité** (priorité : cases qui menacent une ligne adversaire ou complètent une ligne IA) + sorts si mana disponible
- Stratégie spécifique à César : unités agressives en early, sorts de buff en mid, Veni Vidi Vendu si ≥4 mana
- Les intentions ne sont **pas visibles**

---

## 🖥️ Interface de combat

### Layout cible (à construire — refonte grille 2026-04-26)

```
[ Or | Reliques | Score joueur vs Score ennemi ]     ← barre haut (full width)

[Portrait joueur]  [  Grille 4×4 partagée  ]  [Portrait ennemi]
     HP ↓          [ 16 cases carrées       ]       HP ↓

[Mana / Deck / Défausse]       [ Main du joueur ]   [Fin de Tour]
      ↑ bas-gauche               ↑ centré bas          ↑ bas-droite
```

### Hiérarchie Canvas cible (scène Combat — à reconstruire)

```
Canvas/
  FullBG                      — fond plein écran
  GridArea                    — grille 4×4, anchor centré (0.15,0.18)→(0.85,0.88)
    Row_0..3                  — 4 lignes × 4 cases (GridCellUI câblés)
  PortraitPlayer              — anchor (0,0.18)→(0.15,0.88), gauche
    HPLabel / HPText (TMP vert)
  PortraitEnemy               — anchor (0.85,0.18)→(1,0.88), droite
    HPLabel / HPText (TMP rouge)
  ScoreBar                    — dans RelicBar, affiche score joueur / score ennemi en temps réel
  BottomInfo                  — anchor (0,0)→(0.22,0.15), bas-gauche
    ManaText / DeckText / DiscardText
  Hand                        — main joueur, centré bas
  EndTurnButton               — anchor (0.78,0)→(1,0.12), bas-droite
  CombatUI                    — GO logique, toutes refs TMP câblées
  RelicBar                    — barre or/reliques, visible en Play Mode uniquement
```

- **Clic droit sur une carte** : affiche une copie agrandie (CardZoomPanel). Reclic pour fermer.
- **Flèches d'attaque** : affichées sur chaque carte posée, indiquent les cases ciblées
- **Compte à rebours** : affiché sur chaque unité posée, décrémente visuellement à chaque tour

---

## 🗺️ Système de progression (Roguelike)

### Carte de run

- Forme : **carte avec chemins multiples** (style Rogue Lords).
- **Entièrement visible** dès le début du run.
- **Déplacement libre** sur tout nœud accessible.
- Un nœud visité est **bloqué définitivement**.
- Chapitre introductif : **10 lignes de longueur**, **maximum 4 nœuds par ligne** (tous chemins confondus).
- Répartition des nœuds : **aléatoire avec règles** (ex : pas deux boss d'affilée).
- **Arbre vertical scrollable** pour voir l'intégralité des chemins.

### Affichage des nœuds

- **Gris** : nœuds inaccessibles.
- **Vert foncé** : nœuds déjà visités.
- **Vert clair** : nœuds disponibles (accessibles maintenant).
- Lignes reliant les nœuds, icône simple pour chaque type d'événement.

### Types de nœuds

| Nœud | Récompense |
|---|---|
| Combat normal | Choix de cartes + or |
| Combat élite (mini-boss) | Relique + or |
| Boss (fin de chapitre) | Relique + or |
| Événement narratif | Texte + choix avec conséquences (visual novel léger) |
| Marchand | Acheter carte / relique, vendre carte du deck |
| Forge | Améliorer une carte |
| Repos / Soin | Gain de HP + suppression d'une carte du deck |
| Mystère | Inconnu jusqu'à l'arrivée |

### Marchand

- Acheter une carte (du pool du personnage joué, cartes de base uniquement)
- Acheter une relique
- Vendre une carte du deck
- (La forge est un nœud séparé)

### Récompenses de combat

- Après victoire : choisir 1 carte parmi 3 proposées (tirées du pool du personnage, **uniquement les versions de base** — jamais de carte upgradée).
- Bouton "Vendre" sous chaque carte proposée : Commune 25 or, Rare 50 or, Épique 75 or.
- Les cartes upgradées (+) s'obtiennent **exclusivement à la Forge**.

### Forge — Système de fusion

- Déposer **3 copies identiques** d'une même carte → obtenir **1 exemplaire de cette carte upgradée (+)**
- **Coût en or : aucun** — le coût, c'est le sacrifice des 3 copies
- Le deck perd 2 cartes nettes (3 → 1)
- **Fusion bloquée si le deck descend sous 20 cartes** (règle de deck minimum)
- **Une seule chaîne d'upgrade** : Normal → + (pas de ++)
- Les cartes upgradées ne sont jamais proposées en récompense de combat ni au Shop

---

## 🏆 Structure d'un Run

- **5 chapitres** au total, organisés autour de 4 régions : Le Vice, Le Contrôle, La Guerre, Le Spectacle.
- **Chapitre introductif** : 10 lignes, sert à tester toutes les features. Priorité : niveau introductif 100% fonctionnel avant d'étendre.
- Le chapitre introductif comporte un **mini-boss** (déblocage d'une relique).
- Chapitres suivants : à définir ultérieurement.

---

## 💎 Reliques

- Les reliques ne sont **plus hardcodées** dans les CharacterData.
- Obtenues via le **système de leveling de compte** (`AccountLevelReward` ScriptableObjects dans `Assets/Resources/LevelRewards/`).
- **Relique débloquable** : obtenue en battant le mini-boss du niveau introductif (overlay de sélection de relique).
- Effets reliques : `DrawExtraCardPerTurn`, `StartWithBonusMana`, `MaxHPBonus`, `HealAfterCombat`.

---

## 🧙 Personnages

### Roster actuel

| Rôle | Personnage | Personnalité | Portrait |
|---|---|---|---|
| Joueur (prototype) | **Léonard de Vinci** | Inventeur alcoolique | ✅ |
| Joueur (futur) | **Marie Curie** | Baronne de la drogue | ✅ |
| Boss / ennemi | **Jules César** | Agent immobilier ("Veni Vidi Vendu") | ✅ |

### Roster futur (en cours de définition)

Napoléon (startup founder), Ada Lovelace, Attila, Shakespeare, Cléopâtre…

**Règle de design cross-roster** : chaque personnage doit avoir un fantasy unique et un keyword signature distincts des autres. Ne pas créer de doublon de mécanique entre personnages.

---

## 🃏 Decks (à redesigner — refonte grille 2026-04-26)

> ⚠️ Les decks ci-dessous sont **à redesigner** pour le nouveau système de grille 4×4.
> Le format des cartes unités change : ATK / HP / CD / Flèches (1–4 directions) / Keyword / Coût.
> Les noms, flavour et archétypes de personnages sont conservés.

### Léonard de Vinci — Contrôle / Survie (80 HP)

Archétype : unités défensives durables, scoring par survie, victory condition via Dominance.
**Mécanique signature — Bricolage** : si ≥2 unités alliées sont mortes cette manche, peut placer gratuitement un "Automate de Fortune" (ATK = somme ATK mortes / 2 arrondi haut, HP = 2, CD = 2, 1 flèche) sur n'importe quelle case vide.

*Cards à redéfinir — archétype : Blindage, Dominance, Inspiration, Épine*

---

### Marie Curie — Poison / Contrôle de zone (80 HP)

Archétype : neutralise les unités ennemies adjacentes, Explosion pour contrôler la grille, Combo pour scorer efficacement.
**Mécanique signature — Contamination (passive)** : chaque sort joué place 1 jeton Poison sur une unité ennemie adjacente à une unité alliée. Poison : 1 dmg au début de chaque tour ennemi.

*Cards à redéfinir — archétype : Explosion, Combo, Épine, Percée*

---

### Jules César — Agression / Pression (100 HP — BOSS)

Archétype : unités à fort ATK, clustering via Légion, Ralliement pour buff en chaîne. Stats ~20% supérieures aux jouables. Rôle : Boss final et IA ennemie (pas jouable).

*Cards à redéfinir — archétype : Hâte, Légion, Ralliement, Blindage*

---

## 💾 Sauvegarde

- La **run est sauvegardée** à chaque action importante (`run_save.json` dans `persistentDataPath`).
- La run peut être **reprise** lors de la prochaine session.
- Progression de compte sauvegardée séparément (`account_save.json`).

---

## 🗂️ Architecture des dossiers Unity (à respecter)

```
Assets/
├── Art/
│   ├── Cards/
│   │   ├── Units/
│   │   └── Spells/
│   ├── Characters/
│   │   ├── LeonardDeVinci/
│   │   ├── MarieCurie/
│   │   └── JulesCesar/
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

### ✅ Réalisé (pré-refonte grille)

- Scène RunMap (arbre de progression, nœuds, scroll, animation intro, états visuels) ✅
- Scènes MainMenu + CharacterSelect ✅
- Nœuds non-combat (Rest, Forge, Shop, Event, Mystery) — 12 événements narratifs ✅
- Système or, reliques (avec tooltip hover), sauvegarde disque, leveling de compte ✅
- Assets : portraits De Vinci / Curie / César ✅
- SessionLogger ✅
- Animations DOTween (arc Bézier pioche, mort, pose, screen shake, squash-stretch) — à adapter au nouveau système

> ⚠️ Le système de combat lanes + les 46 cartes ScriptableObjects sont **obsolètes** depuis la refonte grille du 2026-04-26. Le code combat est à réécrire from scratch.

### 🔄 Priorités actuelles (refonte grille)

1. **Redesigner les 3 decks** (nouvelles stats : ATK / HP / CD / Flèches) pour le système grille
2. **Réécrire le système de combat** : GridManager, GridCellUI, CombatManager (grille), scoring, manche
3. **Reconstruire la scène Combat** via MCP pour la nouvelle grille 4×4
4. **Adapter les animations** au nouveau contexte (pose sur grille, attaque directionnelle, score popup)
5. **IA ennemie** pour la grille (priorité cases stratégiques, complétion de lignes)

### 📌 Post-prototype v1

- Chapitres suivants (régions, structure narrative)
- Roster complet (Napoléon, Ada Lovelace, Attila, Shakespeare, Cléopâtre…)
- Système d'Ascension (15-20 modificateurs de difficulté)
- Synergies secrètes + équilibrage fin
- **Personnage actif du Dieu** — nœuds "Bénédiction", interventions contextuelles

---

## 🔧 Notes techniques

- Tout le code est en **C# Unity**, from scratch. Namespaces : `RoguelikeTCG.Combat`, `RoguelikeTCG.RunMap`, `RoguelikeTCG.UI`, `RoguelikeTCG.Core`, `RoguelikeTCG.Data`.
- Les visuels (images) sont déposés manuellement par le développeur dans les dossiers `Art/` prévus.
- **ScriptableObjects** pour toutes les données de jeu (cartes, reliques, personnages, événements).
- **DOTween** pour toutes les animations — jamais de Coroutine pour les tweens.
- **UI toujours construite dans la scène** (via MCP), jamais en code dans `Start()`. Scripts = logique pure avec refs public assignées depuis la scène.
- Sauvegarder les scènes avec `Path` explicite (ex: `Scenes/MainMenu/`) pour éviter les doublons.

### Agents Claude disponibles (`.claude/agents/`)

| Agent | Usage |
|---|---|
| `combat-coder` | Toute feature du système de combat |
| `ui-flow-coder` | RunMap, menus, nodes, sauvegarde, futurs écrans |
| `card-balancer` | Design et équilibrage des cartes / decks |
| `unity-builder` | Configuration scènes via Unity MCP |
| `qa-validator` | Vérification features vs design doc |
