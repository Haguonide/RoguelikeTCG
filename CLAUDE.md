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

### Structure
- Combat au **tour par tour**, sur **1 à 3 boards** selon la difficulté :
  - 1 board : combat facile
  - 2 boards : combat moyen
  - 3 boards : mini-boss
  - 1 board : boss final (figure historique)
- Chaque board comporte **3 lanes côté joueur** et **3 lanes côté ennemi**, ainsi qu'un **EnemyHP** spécifique.
- Le joueur a un **HP global** commun à tous les boards.
- **Victoire** : tous les EnemyHP de tous les boards tombent à zéro.
- **Défaite** : le PlayerHP global tombe à zéro.
- Les **intentions ennemies ne sont pas visibles** (part d'incertitude et de stratégie).

### Cartes
- **2 types de cartes** : Unités et Sorts.
- **Unités** : valeurs d'attaque et de points de vie, posées sur les slots joueurs, coût 0 mana.
- **Sorts** : coûtent du mana, appliquent des effets simples (dégâts, soins, buffs).
  - **Sorts ciblant le héros** (allié ou ennemi selon la classe) : cliquer sur le sort dans la main puis sur l'image du héros cible.
    - Héros allié : image placée sous le board allié.
    - Héros ennemi : image placée au-dessus du board ennemi.
  - **Sorts ciblant les unités sur le terrain**.
- **Format des cartes** : TCG classique (style Pokémon / Magic). Template métal industriel avec rivets.
- **Raretés** : Commune / Rare / Épique / Légendaire.
- Les cartes peuvent être **upgradées**.
- Pas d'effets cumulables ou de réactions complexes pendant le combat.

### Deck et main
- Deck initial : **20 cartes**.
- Main maximale : **8 cartes**.
- Pioche : **2 cartes** au début de chaque tour.
- Quand le deck est vide, la défausse est mélangée pour reformer un nouveau deck.
- **Maximum 2 cartes jouées par tour**, tous boards confondus, pour chaque camp.

### Mana
- Se régénère d'**1 point par tour** jusqu'à un plafond configurable.
- Requis uniquement pour les sorts. Les unités coûtent 0 mana.

### Résolution des attaques
- À la fin du tour d'un camp, **toutes les unités attaquent simultanément**.
- Une unité attaque la lane en face si elle est occupée, sinon inflige des dégâts directs au héros adverse (PlayerHP global si ennemi attaque, EnemyHP du board si joueur attaque).
- Lorsque le joueur clique sur **Fin de Tour**, le jeu parcourt les boards un par un (1 à 3) et résout les attaques :
  - **Animation d'attaque** : la carte recule puis réavance (effet de charge).
  - **Animation de mort** : effet de "destruction" stylisé (pas un simple fadeout).
  - **Animation de transition** : balayement visuel entre boards lors de la résolution.

### IA ennemie
- Joue jusqu'à **2 cartes par tour**.
- Priorise la pose d'unités sur les lanes libres, puis utilise ses sorts si elle a du mana.

---

## 🖥️ Interface de combat

- **Haut gauche** : infos joueur (cartes restantes, mana, HP, etc.)
- **Haut droite** : infos des 1 à 3 ennemis (HP + indication si une carte est présente sur leur plateau)
- **Gauche** (quasi toute la hauteur, sans interférer avec les infos joueur) : **journal de combat** (logs des actions joueur et ennemies)
- **3 boutons** pour naviguer entre les différents boards.
- Le board actif est affiché en **grand**, les autres en **mini-vignettes** avec un indicateur de danger si une unité ennemie peut toucher le PlayerHP.
- **Clic droit sur une carte** : affiche une copie agrandie de la carte sur l'écran pour lire ses infos. Un reclic revient à la vue normale.

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
- Acheter une carte (du pool du personnage joué, toutes raretés)
- Acheter une relique
- Vendre une carte du deck
- (La forge est un nœud séparé)

### Récompenses de combat
- Après victoire : choisir 1 carte parmi 3 proposées (tirées du pool du personnage, toutes raretés, **uniquement les versions de base** — jamais de carte upgradée)
- Les cartes upgradées (+) s'obtiennent **exclusivement à la Forge**

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

- **Relique de départ** : attribuée selon le personnage joué.
- **Relique débloquable** : obtenue en battant le mini-boss du niveau introductif.

---

## 🧙 Personnages

### Niveau introductif (prototype en cours)

| Rôle | Personnage | Personnalité |
|---|---|---|
| Joueur | **Léonard de Vinci** | Inventeur alcoolique |
| Joueur (futur) | **Marie Curie** | Baronne de la drogue |
| Mini-boss / ennemis | **Jules César** | Agent immobilier ("Veni Vidi Vendu") |
| Boss final | **Jules César** | — |

### Decks prévus

**Léonard de Vinci — Contrôle / Survie**
Unités défensives solides, heal abondant, dégâts modérés. Survit par les HP.
9 unités illustrées + 4 sorts illustrés, thème inventions ratées mêlant XVe siècle et modernité.

**Marie Curie — Contrôle par l'affaiblissement (Poison/Débuff)**
Neutralise les unités ennemies via des sorts qui réduisent leur ATK avant le clash. Survit en rendant les ennemis inoffensifs. Moins de heal que De Vinci, pas de gros sort burst.
Nouveau type d'effet requis : `ReduceEnemyUnitATK` (valeur négative appliquée à une unité ennemie ciblée).

**Jules César — Agression / Puissance brute (Boss)**
Unités avec meilleurs stats que les persos jouables, sorts 20-30% plus forts à coût égal. Écrase par la puissance brute.
- Autres personnages du roster en cours de définition (Napoléon startup founder, Ada Lovelace, Attila, Shakespeare, Cléopâtre…)

### Assets portraits disponibles
- Léonard de Vinci ✅
- Marie Curie ✅
- Jules César ✅

---

## 💾 Sauvegarde

- La **run est sauvegardée** à chaque action importante, même si le joueur quitte le jeu.
- La run peut être **reprise** lors de la prochaine session.

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
- Scène Combat complète (boards, lanes, deck, mana, IA, animations, UI)
- Scène RunMap (arbre de progression, nœuds, scroll)
- Scène MainMenu + CharacterSelect
- Nœuds non-combat (Rest, Forge, Shop, Event, Mystery)
- Système or, reliques, sauvegarde disque, leveling de compte
- Assets : portraits De Vinci / Curie / César, icônes map, icônes stats (HP/ATK)
- 13 illustrations deck De Vinci (9 unités + 4 sorts)

### 🔄 En cours / Priorité immédiate
- **Réécriture des 3 decks** (De Vinci, Marie Curie, César) avec équilibrage poussé — les cartes existantes sont à remplacer
- **Niveau introductif 100% fonctionnel** pour premiers tests joueurs

### 📌 Après le prototype introductif
- Chapitres suivants (régions, structure narrative)
- Roster complet (en cours de définition)
- Équilibrage approfondi

---

## 🔧 Notes techniques

- Tout le code est en **C# Unity**, from scratch.
- Les visuels (images) seront déposés manuellement par le développeur dans les dossiers `Art/` prévus.
- Utiliser des **ScriptableObjects** pour toutes les données de jeu (cartes, reliques, personnages, événements).
- Le projet doit rester **propre et scalable** dès le départ.
