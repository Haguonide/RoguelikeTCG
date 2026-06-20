# CLAUDE.md — Roguelike Card Game (Unity Prototype)

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

## 🎮 Concept du jeu

Roguelike à base de cartes, inspiré de **Slay the Spire**, **Rogue Lords** et **Wildfrost**.
Pas de direction artistique définie pour l'instant — focus total sur le gameplay.

---

## 🗺️ Map & Structure de la Run

- **3 actes** par run
- Navigation à choix de chemin (style Slay the Spire)

### Types de nœuds

| Nœud           | Récompense                                    |
|----------------|-----------------------------------------------|
| Départ         | —                                             |
| Combat Facile  | Or + choix de cartes                          |
| Combat Élite   | Or + choix de cartes (plus intéressant)       |
| Boss           | Relique de run                                |
| Forge          | Dépenser des Runes → choisir un sort permanent|
| Boutique       | Acheter cartes / reliques ; vendre cartes     |
| Repos          | Supprimer une carte du deck (+ options TBD)   |

---

## ⚔️ Système de Combat

### Board

```
┌──────────────────────────────────────────┐
│  HP Héros Ennemi                         │
├──────┬──────┬──────┬──────┬──────────────┤
│  E0  │  E1  │  E2  │  E3  │  Terrain E  │  ← Board ennemi
├──────┼──────┼──────┼──────┼─────────────┤
│  P0  │  P1  │  P2  │  P3  │  Terrain J  │  ← Board joueur
└──────┴──────┴──────┴──────┴─────────────┘
│  HP Héros Joueur                         │
└──────────────────────────────────────────┘
```

- **4 emplacements** de jeu par camp (colonnes 0–3) — 1 unité max par emplacement
- **1 case Terrain** par camp — seul le joueur utilise les Terrains pour l'instant
- Les unités survivantes **restent en jeu** d'un tour à l'autre
- **Duel de colonne** : une unité en P[c] fait face à l'unité en E[c]

### Tour de jeu

- Le joueur qui commence est déterminé **aléatoirement**
- Chaque tour : le joueur joue ses cartes depuis sa main, puis déclare **Fin de Tour**

**Tour joueur :**
1. **Pioche** — +1 carte (+ effets additionnels)
2. **Phase de jeu** — jouer des cartes (unités, terrain, sorts) en dépensant du mana
3. **Fin de Tour** → résolution des attaques

**Résolution des attaques (fin du tour joueur) :**
- Chaque unité alliée P[c] attaque l'unité ennemie E[c] :
  - Unité ennemie présente → perd des PV. Si elle **survit**, elle **contre-attaque** (inflige son ATK à P[c])
  - Case ennemie vide → **attaque directe** aux HP du héros ennemi
- Les unités ennemies non engagées (dont la case alliée en face est vide) **n'attaquent pas** à ce moment

**Tour ennemi :**
- L'IA joue ses cartes depuis son deck
- **Fin du tour ennemi** → même logique de résolution dans l'autre sens

**Conditions de fin :**
- **Victoire** : HP du héros ennemi à 0
- **Défaite** : HP du héros joueur à 0

### Mana

- Tour 1 : **1 mana**
- +1 mana par tour, **plafond à 10**
- Mana non utilisé **ne se cumule pas** — se réinitialise chaque tour

---

## 🃏 Types de Cartes

### Unités
- Coût en **mana**
- Stats : **ATK** et **PV**
- Peuvent avoir des **effets** et **mots-clés** (à définir lors de la phase mécanique)
- Posées sur un emplacement libre du board
- Attaquent chaque fin de tour
- À la mort → **défausse**

```
┌─────────────┐
│  [Coût] 🔥  │
│  Nom unité  │
│             │
│  ATK : X    │
│  PV  : X    │
│             │
│  [Effet /   │
│   Mot-clé]  │
└─────────────┘
```

### Terrain
- Coût en **mana**
- **1 seul actif** à la fois sur la case Terrain du joueur (remplace l'actif si besoin)
- Déclenche une **mission** à accomplir pendant le combat
- À la complétion → **récompense** immédiate, puis **défausse**
- L'ennemi ne joue **pas** de Terrain (pour l'instant)

```
┌─────────────┐
│  [Coût]     │
│  Nom terrain│
│             │
│  MISSION :  │
│  "..."      │
│             │
│  RÉCOMPENSE:│
│  "..."      │
└─────────────┘
```

**Exemples de missions :**
- "Éliminer 2 unités ennemies en 3 tours" → Piocher 2 cartes
- "Avoir 4 unités alliées simultanément" → Obtenir un sort éphémère
- "Infliger 8 dégâts en un seul tour" → +2 mana ce tour

**Exemples de récompenses :**
- Piocher X cartes
- Obtenir un sort éphémère (usage unique ce combat)
- Bonus de mana temporaire
- Soigner X HP au héros

### Sorts
- **Absents du deck de départ**
- Deux origines :
  1. **Permanent** — craftés à la Forge avec des Runes (rejoignent le deck définitivement)
  2. **Éphémère** — récompense de mission Terrain (hors deck, usage unique ce combat)
- Une fois joués → **défausse**

```
┌─────────────┐
│  [Coût]  ⚡ │
│  Nom sort   │
│  ÉPHÉMÈRE   │  ← si applicable
│             │
│  "Effet"    │
└─────────────┘
```

---

## 🔄 Gestion du Deck

| Événement          | Destination |
|--------------------|-------------|
| Unité tuée         | Défausse    |
| Terrain complété   | Défausse    |
| Sort joué          | Défausse    |
| Deck vide          | Défausse mélangée → nouveau Deck |

- **Deck de départ** : 15 cartes
- **Taille max** : aucune
- **Main de départ** : 4 cartes
- **Pioche par tour** : +1 carte (+ effets additionnels)
- **Copies sur le board** : plusieurs exemplaires de la même carte autorisés, sans limite

---

## 💰 Économie

### Mana
- Tour 1 : **1 mana** — +1 par tour — **cap à 10** — se réinitialise chaque tour

### Or
- Gagné pendant la run (combats, événements…)
- Dépensable à la **Boutique**
- Les cartes peuvent être **revendues** contre de l'or

### Runes
- **Plusieurs types** de runes (types à définir lors de la phase mécanique)
- Gagnées en **tuant des unités ennemies** pendant les combats
- Utilisées à la **Forge** pour obtenir des sorts permanents

---

## 🔨 Nœud Forge

- Le joueur dépense des **runes** pour obtenir un sort permanent
- Le joueur choisit parmi **plusieurs sorts proposés** (pas de craft libre)

---

## 🛒 Nœud Boutique

- Achat de cartes, reliques, objets avec de l'or
- Revente de cartes du deck contre de l'or

---

## 🛌 Nœud Repos

- Permet de **supprimer une carte** du deck
- Autres options à définir (soin, recyclage ?)

---

## 🏆 Récompenses

| Source         | Récompense                              |
|----------------|-----------------------------------------|
| Combat Facile  | Or + choix de cartes                    |
| Combat Élite   | Or + choix de cartes (plus intéressant) |
| Boss           | Relique de run                          |

---

## 💎 Reliques

### Reliques de Run
- Obtenues en **récompense de Boss**
- Actives **uniquement pour la run en cours**
- Effets variés (à définir lors de la phase mécanique / héros)

### Progression de Compte
- Chaque compte possède un **niveau global**
- En montant de niveau, on déverrouille :
  - **Passifs globaux** (actifs sur toutes les runs) — ex : +1 mana de départ, +100 or de départ…
  - **Reliques de départ par héros** — chaque héros a ses propres reliques de départ déverrouillables

---

## 🦸 Héros

- **5 héros jouables** (classes / archétypes de deck)
- Dont **1 classe hybride** qui se combine bien avec toutes les mécaniques
- Détails des héros et decks de départ à définir lors de la phase de design des mécaniques

---

## 📌 Ce qui reste à définir (hors scope prototype initial)

- Mots-clés des unités
- Types de runes et coûts des sorts à la Forge
- Détail des 5 héros et leurs decks de départ
- Exemples de missions Terrain et récompenses
- Contenu de la Boutique
- Options du nœud Repos (soin ?)
- Reliques spécifiques
- Direction artistique

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
│   ├── Enemies/
│   ├── UI/
│   ├── Boards/
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
│   └── Nodes/
├── Scenes/
│   ├── MainMenu/
│   ├── Combat/
│   └── RunMap/
├── Scripts/
│   ├── Combat/        — CombatManager, BoardManager, TerrainSystem, RuneSystem, DeckManager, ManaManager, TurnManager, EnemyAI
│   ├── Cards/         — CardInstance, CardView
│   ├── Data/          — CardData, CardEnums, RelicData, CharacterData, EnemyBehaviorData
│   ├── RunMap/        — RunMapManager, NodeView, EdgeView, etc.
│   ├── UI/            — CombatUI, HandView, CardSelector, BoardSlotUI, etc.
│   ├── SaveSystem/    — DiskSave, AccountSave
│   └── Core/          — RunPersistence, AudioManager, SessionLogger
├── Resources/
└── Plugins/
```

---

## 📋 État d'avancement

### ✅ Réalisé (hors combat)

- Scène RunMap (arbre de progression, nœuds, scroll, animation intro, états visuels) ✅
- Scènes MainMenu + CharacterSelect ✅
- Nœuds non-combat (Rest, Forge, Shop, Event) — événements narratifs ✅
- Système or, reliques (avec tooltip hover), sauvegarde disque, leveling de compte ✅
- SessionLogger ✅

### 🔄 À repartir de zéro (combat)

Le système de combat est à reconstruire entièrement pour correspondre aux nouvelles règles :
- Board 5+1 (slots + Terrain) par camp
- Unités avec coût mana
- Résolution attaque → contre-attaque (pas simultané)
- Mana cap 10
- Deck 15 cartes, pioche +1/tour

### 📌 Priorités prototype

1. **CardData** — nouveau format (ATK, PV, coût mana, type : Unit / Terrain / Spell)
2. **BoardManager** — 5 slots + 1 Terrain par camp, logique de combat
3. **CombatManager** — gestion des tours, mana, pioche, fin de tour
4. **TerrainSystem** — mission + récompense + défausse à complétion
5. **EnemyAI** — pose d'unités, gestion de sorts
6. **RuneSystem** — compteur de Runes, persistant entre combats
7. **Forge** — sélection parmi sorts proposés via Runes

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
| `combat-coder` | Toute feature du système de combat (board, terrains, runes, IA) |
| `ui-flow-coder` | RunMap, menus, nodes, sauvegarde, futurs écrans |
| `card-balancer` | Design et équilibrage des cartes / decks |
| `unity-builder` | Configuration scènes via Unity MCP |
| `qa-validator` | Vérification features vs design doc |
