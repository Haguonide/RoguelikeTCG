# CLAUDE.md — Roguelike Deckbuilder Fantasy (Sorciers)

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
4. **Signaler les références manuelles restantes** : si une référence ne peut pas être liée via MCP, lister explicitement ce que le développeur doit faire manuellement dans Unity

### Ce que Claude Code peut faire via MCP
- ✅ Créer / supprimer / renommer des GameObjects
- ✅ Attacher des scripts à des GameObjects
- ✅ Modifier des valeurs simples dans l'Inspector (int, float, string, bool)
- ✅ Exécuter des menu items Unity
- ✅ Créer la structure de la Hierarchy
- ✅ Câbler des références via RunCommand

### Ce qui nécessite une intervention manuelle du développeur
- ⚠️ Assigner des Sprites / Textures dans l'Inspector (drag & drop)
- ⚠️ Créer et configurer des Prefabs
- ⚠️ Assigner des ScriptableObjects dans des listes (rewardCardPool, etc.)

---

## 🎮 Description générale du jeu

Roguelike deckbuilder stratégique, inspiré de **Slay the Spire** et **Wildfrost**.

- **Univers** : Fantasy médiéval — sorciers, nécromanciens, créatures magiques. Ton léger et burlesque.
- **Style graphique** : **Wildfrost** — chibi animal, flat cartoon, thick black outlines, sticker style avec contour blanc. Palette vive par personnage. Lisibilité avant tout.
- **Plateau** : Table d'enchantement vue de dessus — bords décorés (bougies, grimoires, gemmes), centre épuré.
- **Couleurs** : Bois sombre, lueur teal/cyan pour les runes, ambre chaud pour les bougies.
- **Structure narrative** : Progression roguelike à chapitres. Prototype : 1 chapitre fonctionnel.

---

## ⚔️ Système de combat

### Structure de la grille

Le combat se joue sur une **grille 2×5** : 2 lignes (joueur / ennemi), 5 colonnes chacune.

```
┌──────────────────────────────────────┐
│  HP Ennemi : ████████████░░░░  14/20 │
├──────┬──────┬──────┬──────┬──────────┤
│  E0  │  E1  │  E2  │  E3  │   E4    │  ← Row 0 : ligne ennemie
├──────┼──────┼──────┼──────┼──────────┤
│  P0  │  P1  │  P2  │  P3  │   P4    │  ← Row 1 : ligne joueur
└──────┴──────┴──────┴──────┴──────────┘
│  HP Joueur : ████████████████  30/30 │
└──────────────────────────────────────┘
```

- **Duel de colonne** : chaque unité joueur [1,c] fait face à l'unité ennemie [0,c]
- **1 unité maximum par case**
- Les unités survivantes **restent en jeu** d'un tour à l'autre — pas de reset de manches
- **Pas de manches** : le combat dure jusqu'à ce qu'un camp tombe à 0 HP

### Structure d'un tour

**Tour joueur :**
1. **Pioche** — 2 cartes (4 au premier tour du combat)
2. **Pose** — 1 unité sur une case Row 1 vide (coût 0, gratuit)
3. **Sorts** — 0-N sorts (limité par le mana disponible)
4. **Fin de Tour** → phase d'attaque simultanée résolue

**Phase d'attaque simultanée :**
- Chaque unité [1,c] attaque [0,c] :
  - Unité ennemie présente → elle perd HP (mort si HP ≤ 0, elle ne réplique pas)
  - Case ennemie vide → **fuite de lane** : 1 dégât direct aux HP ennemis
- En parallèle, chaque unité [0,c] encore vivante attaque [1,c] :
  - Unité joueur présente → elle perd HP
  - Case joueur vide → **fuite de lane** : 1 dégât direct aux HP joueur
- Les **Bonds d'attaque** se déclenchent avant la résolution des dégâts

**Tour ennemi (après résolution) :**
- L'IA pose 1 unité sur Row 0, joue des sorts
- Pas de phase d'attaque supplémentaire (l'attaque est simultanée dans le tour joueur)

### Mana

- **Pour les sorts uniquement** — les unités sont gratuites
- Croissant : 1 mana au tour 1, +1 par tour joueur, **plafond à 6**
- Se régénère entièrement au début de chaque tour joueur

### Éléments (5)

| Icône | Nom       | Archétype       |
|-------|-----------|-----------------|
| 🔥    | Feu       | Offensif        |
| ❄️    | Glace     | Contrôle        |
| ⚡    | Foudre    | Chain / burst   |
| 🌑    | Ombre     | Drain / debuff  |
| 🌿    | Nature    | Support / heal  |

### Système de Bonds d'adjacence

Deux unités **adjacentes** (gauche/droite sur la même ligne) avec des éléments compatibles déclenchent un **Bond**.

**Règle de cumul :** une unité encadrée par deux éléments différents déclenche les deux Bonds simultanément.

| Paire              | Nom            | Type    | Effet                                                       |
|--------------------|----------------|---------|-------------------------------------------------------------|
| 🔥 + 🔥            | Embrasement    | Attaque | +1 ATK, splash 1 dégât aux cases ennemies adjacentes        |
| ❄️ + ❄️            | Blizzard       | Attaque | Freeze : la cible passe son prochain tour d'attaque          |
| ⚡ + ⚡            | Surcharge      | Attaque | +2 ATK si l'ennemi en face a déjà subi des dégâts ce tour   |
| 🌑 + 🌑            | Abîme          | Attaque | Drain : soigne 1 HP à l'unité attaquante                    |
| 🌿 + 🌿            | Forêt dense    | Passif  | +1 HP max aux deux unités 🌿                                |
| ⚡ + 🔥 ou 🔥 + ⚡ | Éclair ardent  | Attaque | L'attaque saute sur l'unité ennemie en colonne adjacente    |
| ⚡ + 🌑 ou 🌑 + ⚡ | Foudre noire   | Attaque | Ignore les HP bonus de la cible, dégâts bruts               |
| ❄️ + 🌿 ou 🌿 + ❄️ | Givre vivant   | Passif  | La 🌿 régénère 1 HP/tour si adjacente à ❄️                  |
| 🔥 + 🌑 ou 🌑 + 🔥 | Cendres        | Attaque | Si kill : la 🌑 adjacente gagne +1 ATK permanent            |
| 🌿 + 🌑 ou 🌑 + 🌿 | Décomposition  | Passif  | Les unités ennemies en face perdent 1 HP max                |

### Anatomie d'une carte unité

```
┌─────────────┐
│  Nom unité  │
│  [Élément]  │
│             │
│  ATK : X    │
│  HP  : X    │
│             │
│  "Bond :    │
│   effet si  │
│   adjacent" │
└─────────────┘
  (gratuite à poser)
```

- **Coût** : 0 mana — les unités sont gratuites
- **ATK / HP** : stats de l'unité (ATK 1-3, HP 1-5)
- **Bond** : effet déclenché si un voisin compatible est présent

### Anatomie d'une carte sort

```
┌─────────────┐
│  Nom sort   │
│  [Élément]  │
│             │
│  "Effet"    │
└─────────────┘
   Coût : X mana
```

- **Sorts offensifs** : dégâts directs (HP ennemis ou unité ennemie)
- **Sorts utilitaires** : buffs, debuffs, déplacement d'unité, soins

### Deck et main

- **Taille du deck** : 20 cartes minimum
- **Répartition** : libre selon le personnage
- **Main de départ** : 4 cartes
- **Pioche** : 2 cartes au début de chaque tour joueur
- **Main maximale** : 10 cartes
- Deck vide → défausse mélangée pour reformer un nouveau deck

### HP des camps

| Type de combat | HP ennemi |
|----------------|-----------|
| Combat normal  | 20 HP     |
| Combat élite   | 35 HP     |
| Boss           | 50 HP     |

- **HP joueur** : **30 HP** global persistant entre combats — récupération uniquement via nœud Repos

### IA ennemie

- Chaque ennemi a un **`EnemyBehaviorData`** ScriptableObject : deck + stratégie (Aggressive / Defensive / SynergySeeker)
- L'ennemi pose 1 unité sur Row 0, joue des sorts depuis son deck
- Les intentions ennemies ne sont **pas visibles**

---

## 🖥️ Interface de combat

### Layout cible

```
[ HP Ennemi — barre de vie ]

[Portrait joueur]  [  Grille 2×5  ]  [Portrait ennemi]
     HP ↓          [ Row 0 ennemi ]       HP ↓
                   [ Row 1 joueur ]

[Mana / Deck / Défausse]    [ Main du joueur ]    [Fin de Tour]
      ↑ bas-gauche              ↑ centré bas          ↑ bas-droite
```

### Hiérarchie Canvas (scène Combat — état actuel)

```
Canvas/
  FullBG                      — fond plein écran
  GridArea                    — grille 2×5, anchor centré (ACTIF)
    GridAreaToPlay
      GridRow_0               — 5 cases ennemies (Cell_0_0..Cell_0_4)
      GridRow_1               — 5 cases joueur (Cell_1_0..Cell_1_4)
  PortraitPlayer              — anchor gauche (portraits, HP, mana, BtnPass)
  PortraitEnemy               — anchor droite
  CombatUI                    — GO logique, refs TMP câblées
  Hand                        — main joueur, centré bas
  CardSelector                — gestion clics cartes et cellules
  SpellArrow                  — flèche de ciblage pour les sorts
  CardZoomPanel               — zoom clic droit sur une carte
  RelicBar                    — barre or/reliques
  RelicTooltipUI              — tooltip hover reliques
  PauseMenu                   — menu pause
  ScanlinesOverlay            — overlay CRT (inactif)
```

- **BtnPass** (dans PortraitPlayer) → appelle `CombatManager.EndPlayerTurn()`
- **GridCellUI** sur chaque cellule : `row` (0=ennemi, 1=joueur), `col` (0-4)

---

## 🗺️ Système de progression (Roguelike)

### Carte de run

- Forme : arbre vertical scrollable (style Slay the Spire), entièrement visible dès le début
- Un nœud visité est bloqué définitivement
- Chapitre introductif : 10 lignes, maximum 4 nœuds par ligne

### Types de nœuds

| Nœud              | Récompense                             |
|-------------------|----------------------------------------|
| Combat normal     | Choix de cartes + or                   |
| Combat élite      | Relique + or                           |
| Boss              | Relique + or                           |
| Événement narratif| Texte + choix avec conséquences        |
| Marchand          | Acheter carte / relique, vendre carte  |
| Forge             | 3 copies identiques → 1 carte upgradée |
| Repos / Soin      | Gain de HP + suppression d'une carte   |
| Mystère           | Inconnu jusqu'à l'arrivée              |

### Forge — Système de fusion

- **3 copies identiques** → **1 exemplaire upgradé (+)**
- Coût : 0 or — le coût c'est le sacrifice des 3 copies
- Fusion bloquée si le deck descend sous 20 cartes
- Une seule chaîne d'upgrade : Normal → + (pas de ++)

### Récompenses de combat

- Après victoire : choisir 1 carte parmi 3 proposées
- Bouton "Vendre" : Commune 25 or, Uncommon 37 or, Rare 50 or, Epic 75 or, Legendary 100 or

---

## 💎 Reliques

- Obtenues via combats élite, boss, et leveling de compte
- Effets : `DrawExtraCardPerTurn`, `StartWithBonusMana`, `MaxHPBonus`, `HealAfterCombat`

---

## 🦸 Personnages jouables (prototype)

### CatSorcerer — Élément : 🔥 Feu

Sorcier chat. Offensif, explosif. Bonds naturels : Embrasement (🔥+🔥), Éclair ardent (⚡+🔥), Cendres (🔥+🌑).

**Cartes test disponibles :** `Assets/Data/Cards/CatSorcerer/`
- Flamme (Unit, Fire, 1/1)
- Brasier (Unit, Fire, 2/1)
- Embrasement (Unit, Fire, 1/2 — Uncommon)
- Boule de Feu (Spell, Fire, 2 mana — 3 dégâts héros ennemi)

### RaccoonNecromancer — Élément : 🌑 Ombre

Nécromancien raton laveur. Drain, debuff. Bonds naturels : Abîme (🌑+🌑), Foudre noire (⚡+🌑), Décomposition (🌿+🌑).

**Cartes test disponibles :** `Assets/Data/Cards/RaccoonNecromancer/`
- Ombre (Unit, Shadow, 1/1)
- Draine-Âme (Unit, Shadow, 1/2)
- Spectre (Unit, Shadow, 2/1 — Uncommon)
- Drain de Vie (Spell, Shadow, 2 mana — 2 dégâts unité + soins 1)

### Ennemi test

**Cartes test disponibles :** `Assets/Data/Cards/Enemies/TestEnemy/`
- Grunt (Unit, None, 1/1)
- Brute (Unit, None, 2/2)
- Fireball (Spell, Fire, 2 mana — 2 dégâts héros joueur)

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
│   ├── Characters/
│   │   ├── CatSorcerer/
│   │   └── RaccoonNecromancer/
│   ├── Enemies/
│   ├── UI/
│   ├── Boards/
│   └── Effects/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Data/
│   ├── Cards/
│   │   ├── CatSorcerer/
│   │   ├── RaccoonNecromancer/
│   │   └── Enemies/
│   ├── Relics/
│   ├── Characters/
│   └── Events/
├── Prefabs/
│   ├── Cards/
│   ├── UI/
│   └── Nodes/
├── Scenes/
│   ├── MainMenu/
│   ├── Combat/
│   └── RunMap/
├── Scripts/
│   ├── Combat/        — CombatManager, GridManager, BondSystem, DeckManager, ManaManager, TurnManager
│   ├── Cards/         — CardInstance, CardView
│   ├── AI/            — EnemyAI, EnemyBehaviorData
│   ├── RunMap/        — RunMapManager, NodeView, EdgeView, etc.
│   ├── Data/          — CardData, CardEnums, CardEffect, RelicData, CharacterData, EnemyBehaviorData
│   ├── UI/            — CombatUI, HandView, CardSelector, GridCellUI, etc.
│   ├── SaveSystem/    — DiskSave, AccountSave
│   └── Core/          — RunPersistence, AudioManager, SessionLogger
├── Resources/
└── Plugins/
```

---

## 📋 État d'avancement

### ✅ Réalisé

- Scène RunMap (arbre de progression, nœuds, scroll, animation intro, états visuels) ✅
- Scènes MainMenu + CharacterSelect ✅
- Nœuds non-combat (Rest, Forge, Shop, Event, Mystery) — 12 événements narratifs ✅
- Système or, reliques (avec tooltip hover), sauvegarde disque, leveling de compte ✅
- SessionLogger ✅
- **Système de combat 2×5** : GridManager, CombatManager, BondSystem, EnemyAI scriptable ✅
- **CardData** : nouveau format (Element, ATK, HP, sans keyword/passif positionnel) ✅
- **Scène Combat** : grille 2×5 câblée, portraits, main, bouton fin de tour ✅
- **Cartes test** : CatSorcerer (4 cartes), RaccoonNecromancer (4 cartes), TestEnemy (3 cartes) ✅

### 🔄 Priorités actuelles

1. **CharacterData** pour CatSorcerer et RaccoonNecromancer (startingDeck, portrait)
2. **Premier test de combat** — assigner playerCharacter dans CombatManager Inspector
3. **Decks complets** — 20 cartes par personnage
4. **CardView / CardUIBuilder** — adapter l'affichage des cartes au nouveau format (Element, ATK, pas de flèches)
5. **BondSystem feedback visuel** — indicateurs de bonds actifs sur la grille
6. **IA ennemie** — créer un EnemyBehaviorData pour le combat test

### 📌 Post-prototype

- Éléments Glace, Foudre, Nature (personnages supplémentaires)
- Chapitres 2+
- Système d'Ascension

---

## 🔧 Notes techniques

- Tout le code est en **C# Unity**, from scratch. Namespaces : `RoguelikeTCG.Combat`, `RoguelikeTCG.RunMap`, `RoguelikeTCG.UI`, `RoguelikeTCG.Core`, `RoguelikeTCG.Data`.
- **ScriptableObjects** pour toutes les données (cartes, reliques, personnages, events, comportements IA).
- **DOTween** pour toutes les animations — jamais de Coroutine pour les tweens.
- **UI toujours construite dans la scène** (via MCP), jamais en code dans `Start()`. Scripts = logique pure avec refs public assignées depuis la scène.
- Les visuels (images) sont déposés **manuellement par le développeur** dans les dossiers `Art/` prévus.
- Sauvegarder les scènes avec `Path` explicite (ex: `Scenes/Combat/`) pour éviter les doublons.

### Agents Claude disponibles (`.claude/agents/`)

| Agent | Usage |
|---|---|
| `combat-coder` | Toute feature du système de combat (grille 2×5, bonds, IA) |
| `ui-flow-coder` | RunMap, menus, nodes, sauvegarde, futurs écrans |
| `card-balancer` | Design et équilibrage des cartes / decks |
| `unity-builder` | Configuration scènes via Unity MCP |
| `qa-validator` | Vérification features vs design doc |
