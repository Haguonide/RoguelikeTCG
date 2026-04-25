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
- **Style graphique** : Inspiré de **Wildfrost** — personnages aux proportions trapues (stocky), contours épais et gras (bold outlines), cel shading flat, couleurs vives légèrement désaturées (bright muted colors). Template de carte en métal industriel avec rivets. Cohérence graphique stricte sur tous les éléments.

---

## ⚔️ Système de combat

### Structure des lanes

Le combat se joue sur des **lanes partagées** visibles simultanément — les unités avancent case par case vers l'ennemi.

| Type de combat | Nombre de lanes |
|---|---|
| Normal | 2 lanes |
| Elite | 2 lanes |
| Mini-boss / Boss final | 3 lanes |

- **6 cases par lane** — cases 0-2 : zone joueur, cases 3-5 : zone ennemie.
- Joueur avance **gauche → droite** (index croissant), Ennemi avance **droite → gauche** (index décroissant).
- Case de déploiement : case 0 (joueur) / case 5 (ennemi). **1 unité max par case.**
- **Coin flip** en début de combat pour déterminer qui joue en premier.
- **1 HP bar par camp** : joueur (global à tous les combats), ennemi (spécifique au combat).
- **Victoire** : HP ennemi tombe à zéro. **Défaite** : HP joueur global tombe à zéro.
- Les **intentions ennemies ne sont pas visibles**.

### Mécanique d'avancement

- **Tour où posée** : summoning sickness — l'unité ne bouge pas (sauf keyword **Charge**).
- **Fin de tour du propriétaire** : l'unité tente d'avancer d'1 case :
  - Case suivante occupée par une unité ennemie → **clash simultané** (les deux infligent leurs dégâts en même temps)
  - Case vide → avance
  - Au-delà de la dernière case → attaque les HP ennemis directement, puis va en **DÉFAUSSE** (recyclable)
- Unité tuée par un clash ou un sort → **CIMETIÈRE** (perdue définitivement, ne revient jamais dans le deck)
- Unité ayant traversé toutes les cases → **DÉFAUSSE** (recyclable quand le deck est vide)

### Cartes

- **2 types** : Unités et Sorts.
- **Mana unifié** : unités ET sorts coûtent du mana.
- **Unités** : valeurs d'attaque et de points de vie, posées sur les cases de déploiement joueur.
- **Sorts** : effets instantanés (dégâts, soins, buffs, débuffs).
  - Ciblage sorts : `PlayerHero`, `EnemyHero`, `AllyUnit`, `EnemyUnit`, `AllEnemyUnits` (AoE)
- **Format** : TCG classique (style Pokémon / Magic). Template métal industriel avec rivets.
- **Raretés** : Commune / Rare / Épique / Légendaire.
- Les cartes peuvent être **upgradées** (version +).

### Deck et main

- Deck initial : **20 cartes** (minimum 20 cartes en permanence).
- **5 cartes** en main au démarrage.
- **2 cartes piochées** au début de chaque tour.
- Main maximale : **10 cartes**.
- Quand le deck est vide, la **défausse** est mélangée pour reformer un nouveau deck.
- Le **cimetière** n'est jamais recyclé — les cartes y sont perdues définitivement.

### Mana

- **Mana croissant** : 1 au tour 1, +1 par tour joueur, **plafond à 6**.
- Se **régénère entièrement** chaque tour (pas d'accumulation).
- Commun aux unités et aux sorts.

### Keywords (16 validés)

| Keyword | Description |
|---|---|
| **Épine** | À la mort, X dégâts à l'unité tueuse |
| **Inspiration** | Pioche X cartes à l'entrée |
| **Vigilance** | Si traverse sans clash → dégâts ×2 aux HP ennemis |
| **Percée** | L'overkill d'un clash saigne sur les HP ennemis |
| **Résilience** | Soigne X HP au héros si survit à un clash ce tour |
| **Légion** | +1 ATK à l'entrée si ≥1 allié présent (toutes lanes) |
| **Conquête** | Soigne X HP au héros quand cette unité tue |
| **Sacrifice offensif** | X dégâts directs aux HP ennemis à la mort |
| **Irradiation** | 1 dégât AoE à toutes les unités ennemies (toutes lanes) au début de chaque tour allié |
| **Explosion radioactive** | AoE X dégâts à toutes les unités ennemies à la mort |
| **Contagion** | -X ATK à l'unité ennemie adjacente à la mort |
| **Exploiter** | +2 dégâts directs si l'unité ennemie en face est à 0 ATK |
| **Charge** | Peut avancer le tour où elle est posée (pas de summoning sickness) |
| **Rapide** | Avance 2 cases par tour au lieu de 1 |
| **Blindage** | -1 dégât reçu de toutes sources |
| **Ralliement** | +1 ATK à toutes les unités alliées présentes à l'entrée |

### IA ennemie

- Joue jusqu'à **2 cartes par tour**.
- Priorise la pose d'unités sur les cases de déploiement libres, puis utilise ses sorts si elle a du mana.
- Stratégie spécifique à César : Légionnaires en early, Centurion + Ordre de charge en mid, Veni Vidi Vendu si ≥4 mana.

---

## 🖥️ Interface de combat

### Layout validé (implémenté 2026-04-21)

```
[ Or | Reliques ]                                    ← barre haut (full width)

[Portrait joueur]  [ Lane 1 ][ Lane 2 ]...  [Portrait ennemi]
     HP ↓          [6 cases par lane]             HP ↓

[Mana / Deck / Défausse / Cimetière]        [ Main du joueur ]   [Fin de Tour]
      ↑ bas-gauche                             ↑ centré bas          ↑ bas-droite
```

### Hiérarchie Canvas (scène Combat)

```
Canvas/
  FullBG                      — fond plein écran
  LanesArea                   — anchor (0.09,0.22)→(0.91,0.86), 4 LaneRow
    LaneRow_1..4              — chacune avec 6 Cell_0..5 (LaneSlotUI câblés)
  PortraitPlayer              — anchor (0,0.22)→(0.09,0.86), gauche
    HPLabel / HPText (TMP vert)
  PortraitEnemy               — anchor (0.91,0.22)→(1,0.86), droite
    HPLabel / HPText (TMP rouge)
  BottomInfo                  — anchor (0,0)→(0.22,0.18), bas-gauche
    ManaText / DeckText / DiscardText / CemeteryText
  Hand                        — main joueur, centré bas
  EndTurnButton               — anchor (0.78,0)→(1,0.12), bas-droite
  CombatUI                    — GO logique, toutes refs TMP câblées
  RelicBar                    — barre or/reliques, visible en Play Mode uniquement
```

- **Clic droit sur une carte** : affiche une copie agrandie (CardZoomPanel). Reclic pour fermer.
- **Journal de combat** : masqué (legacy), remplacé par les infos distribuées dans le layout.

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

## 🃏 Decks (refonte 2026-04-20 — système lanes/mana unifié)

### Léonard de Vinci — Contrôle / Survie (80 HP)

Archétype : unités défensives durables, heal régulier, victoire par usure.
**Mécanique signature — Bricolage** : si ≥2 unités alliées dans le cimetière ce combat, peut placer une unité de fortune (ATK = somme ATK mortes / 2 arrondi haut, HP = 2). Cimetière uniquement (pas défausse).

| Carte | Type | ATK | HP | Keyword | Coût | Rareté | ×copies |
|---|---|---|---|---|---|---|---|
| Automate boiteux | Unité | 1 | 2 | Épine (1 dmg au tueur) | 1 | Commune | ×3 |
| Chien de garde | Unité | 1 | 3 | Vigilance (ATK×2 si traverse sans clash) | 1 | Commune | ×2 |
| Bouclier parapluie troué | Sort | — | — | +3 HP unité alliée + Shield 2 héros | 1 | Commune | ×3 |
| Ailes volantes | Sort | — | — | +4 ATK unité alliée, -2 HP ("l'invention explose") | 2 | Commune | ×2 |
| Vitruve | Unité | 1 | 4 | Résilience (soigne 1 héros si survit à un clash) | 2 | Commune | ×2 |
| Arbalète géante | Unité | 3 | 1 | Percée (overkill saigne sur HP ennemis) | 2 | Rare | ×2 |
| Joconde | Unité | 2 | 2 | Inspiration (pioche 1 à l'entrée) | 2 | Rare | ×2 |
| Revigorant | Sort | — | — | Heal 5 héros + pioche 1 | 3 | Rare | ×2 |
| Réveil explosif | Sort | — | — | 3 dmg AoE unités ennemies lane choisie + 1 dmg unité alliée | 3 | Rare | ×2 |

**Épiques (pool rewards, hors deck de départ) :**
- Char d'assaut (2/6, Blindage, coût 4) — débloqué via leveling
- Dragon (3/3, Souffle AoE toutes lanes à l'entrée, coût 5) — débloqué via leveling

---

### Marie Curie — Poison / Débuff (80 HP)

Archétype : neutralise les unités ennemies via réduction ATK, exploite les 0 ATK. Moins de heal que De Vinci, pas de burst.
**Mécanique signature — Contamination (passive)** : chaque sort joué place automatiquement 1 jeton Poison sur une unité ennemie au choix (ou héros ennemi si aucune unité). Poison : 1 dmg au début de chaque tour ennemi.

| Carte | Type | ATK | HP | Keyword | Coût | Rareté | ×copies |
|---|---|---|---|---|---|---|---|
| Assistante contaminée | Unité | 1 | 2 | Contagion (-1 ATK unité ennemie adjacente à la mort) | 1 | Commune | ×3 |
| Sérum affaiblissant | Sort | — | — | -2 ATK à une unité ennemie ciblée | 1 | Commune | ×3 |
| Cobaye volontaire | Unité | 1 | 3 | Blindage (-1 dmg reçu) | 2 | Commune | ×2 |
| Injection radioactive | Sort | — | — | 2 dmg unité ennemie + Poison 1 | 2 | Commune | ×2 |
| Antidote corrompu | Sort | — | — | Heal 4 héros + Poison 1 sur unité ennemie | 2 | Commune | ×2 |
| Dealer en blouse blanche | Unité | 2 | 2 | Exploiter (+2 dmg directs si ennemi en face à 0 ATK) | 2 | Rare | ×2 |
| Trafiquante de radium | Unité | 3 | 2 | Contrebande (-1 ATK ennemi aléatoire à l'entrée) | 3 | Rare | ×2 |
| Chimiste véreux | Unité | 2 | 3 | Synthèse (soigne 2 HP héros si ≥1 ennemi à 0 ATK en fin de tour) | 2 | Rare | ×2 |
| Brouillard toxique | Sort | — | — | -1 ATK à TOUTES les unités ennemies (toutes lanes) | 3 | Rare | ×2 |

**Épiques (pool rewards, hors deck de départ) :**
- Baron de la Pechblende (3/4, Irradiation, coût 5) — débloqué via leveling
- Monstre de Polonium (4/3, Explosion radioactive AoE 2 dmg à la mort, coût 5) — débloqué via leveling

---

### Jules César — Agression / Puissance brute (100 HP — BOSS)

Archétype : snowball de force, écrase par la pression pure. Stats ~20% supérieures aux jouables. Rôle : Boss final et IA ennemie (pas jouable).

| Carte | Type | ATK | HP | Keyword | Coût | Rareté | ×copies |
|---|---|---|---|---|---|---|---|
| Légionnaire | Unité | 2 | 2 | Légion (+1 ATK si ≥1 allié présent) | 2 | Commune | ×3 |
| Ordre de charge | Sort | — | — | +3 ATK à une unité alliée + Charge | 2 | Commune | ×3 |
| Gladiateur kamikaze | Unité | 4 | 1 | Sacrifice offensif (3 dmg directs à la mort) | 2 | Commune | ×2 |
| Tactique décisive | Sort | — | — | 2 dmg unité ennemie + Ralentissement (ne peut pas avancer ce tour) | 2 | Commune | ×2 |
| Garde prétorienne | Unité | 2 | 4 | Blindage (-1 dmg reçu) | 3 | Commune | ×2 |
| Centurion agressif | Unité | 3 | 2 | Charge (avance le tour où posé) | 3 | Rare | ×2 |
| Tribun militaire | Unité | 3 | 3 | Ralliement (+1 ATK à toutes unités alliées présentes) | 4 | Rare | ×2 |
| Veni Vidi Vendu | Sort | — | — | 5 dmg directs HP ennemis | 4 | Rare | ×2 |
| Formation de combat | Sort | — | — | +1 ATK et +1 HP à toutes unités alliées d'une lane | 3 | Rare | ×2 |

**Épiques :**
- Proconsul de la mort (4/4, Conquête soigne 2 HP, coût 5)
- César lui-même (5/3, Ralliement++ +1 ATK alliés + AoE 3 dmg ennemis, coût 6)

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

### ✅ Réalisé

- Système de combat complet : lanes 2/3/4 selon type, 6 cases, avancement de troupes, summoning sickness, clash simultané, cimetière vs défausse, mana unifié croissant cap 6
- 16 keywords implémentés (Épine, Inspiration, Vigilance, Percée, Résilience, Légion, Conquête, Sacrifice offensif, Irradiation, Explosion radioactive, Contagion, Exploiter, Charge, Rapide, Blindage, Ralliement)
- Mécaniques signature : Bricolage (De Vinci), Contamination + Poison (Marie Curie)
- Animations DOTween complètes : arc Bézier pioche, clash, mort, pose, screen shake, squash-stretch
- Scène RunMap (arbre de progression, nœuds, scroll, animation intro, états visuels)
- Scènes MainMenu + CharacterSelect
- Nœuds non-combat (Rest, Forge, Shop, Event, Mystery) — 12 événements narratifs
- Système or, reliques (avec tooltip hover), sauvegarde disque, leveling de compte
- Assets : portraits De Vinci / Curie / César, template carte métal industriel
- 46 cartes ScriptableObjects (base + upgradées, 3 decks complets)
- SessionLogger (logs par session pour analyser le game flow)

### 🔄 Priorités actuelles

1. **Valider le rythme de jeu** via analyse des logs SessionLogger
2. **Personnage actif du Dieu** — nœuds "Bénédiction", interventions contextuelles pendant combats/événements
3. **3 personnages jouables supplémentaires** — après que De Vinci soit stable et fun

### 📌 Post-prototype v1

- Chapitres suivants (régions, structure narrative)
- Roster complet (Napoléon, Ada Lovelace, Attila, Shakespeare, Cléopâtre…)
- Système d'Ascension (15-20 modificateurs de difficulté)
- Synergies secrètes + équilibrage fin

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
