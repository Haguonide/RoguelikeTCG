# Nouveau Départ — Design Document

## Pivot de direction (2026-05-17)

L'univers super-héros CDI burlesque est **abandonné**. On repart sur :
- **Univers** : Fantasy médiéval, sorciers et magiciens de tout type
- **Style graphique** : Wildfrost (chibi animal, flat cartoon, thick outlines, sticker style)
- **Personnages existants** : CatSorcerer, RaccoonNecromancer (Assets/Art/)
- **Plateau** : Table d'enchantement (fond de scène de combat)

---

## Plateau de jeu : 2 lignes × 5 cases

```
┌──────────────────────────────────────┐
│  HP Ennemi : ████████████░░░░  24/30 │
├──────┬──────┬──────┬──────┬──────────┤
│  E1  │  E2  │  E3  │  E4  │   E5    │  ← ligne ennemie
├──────┼──────┼──────┼──────┼──────────┤
│  P1  │  P2  │  P3  │  P4  │   P5    │  ← ligne joueur
└──────┴──────┴──────┴──────┴──────────┘
│  HP Joueur : ████████████████  30/30 │
└──────────────────────────────────────┘
```

- **10 cases totales** : 5 joueur + 5 ennemi
- Chaque case joueur **fait face** à la case ennemie de même colonne (duel de colonne)
- 1 unité maximum par case
- Les unités survivantes **restent en jeu** d'un tour à l'autre
- **Pas de manches** — le combat dure jusqu'à ce qu'un camp tombe à 0 HP

---

## Éléments (5 définitifs)

| Icône | Nom    | Archétype      |
|-------|--------|----------------|
| 🔥    | Feu    | Offensif       |
| ❄️    | Glace  | Contrôle       |
| ⚡    | Foudre | Chain / burst  |
| 🌑    | Ombre  | Drain / debuff |
| 🌿    | Nature | Support / heal |

---

## Anatomie d'une carte unité

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

- **Coût** : 0 mana — les unités sont gratuites à poser
- **ATK** : dégâts infligés à l'unité en face lors de la phase d'attaque
- **HP** : points de vie, l'unité meurt à 0
- **Bond** : effet déclenché si une unité adjacente compatible est présente (voir table Bonds)

---

## Anatomie d'une carte sort

```
┌─────────────┐
│  Nom sort   │
│  [Élément]  │
│             │
│  "Effet"    │
│             │
└─────────────┘
   Coût : X mana
```

- **Sorts offensifs** : dégâts directs (aux HP ennemis ou à une unité ennemie)
- **Sorts utilitaires** : buffs, debuffs, déplacement d'unité, soins

---

## Structure d'un tour

### Tour joueur
1. **Pioche** — tirer 2 cartes (4 en main au départ du combat)
2. **Phase de jeu** :
   - Poser **1 unité** sur une case joueur vide (gratuit)
   - Jouer **0-N sorts** (limité par le mana disponible)
3. **Phase d'attaque simultanée** :
   - Chaque unité joueur attaque la case ennemie en face
   - Chaque unité ennemie attaque la case joueur en face (simultané)
   - **Si une unité est tuée, elle ne réplique pas** (mort avant résolution)
   - Les Bonds actifs se déclenchent avant la résolution des dégâts
   - Case ennemie vide en face d'une unité joueur → **1 dégât direct aux HP ennemis**
   - Case joueur vide en face d'une unité ennemie → **1 dégât direct aux HP joueur**
4. **Cleanup** — unités mortes en défausse, survivantes restent en place

### Tour ennemi (après le tour joueur)
1. L'IA pose **1 unité** sur une case ennemie vide (selon sa logique scriptée)
2. L'IA joue **0-N sorts** de son deck
3. *(Pas de phase d'attaque supplémentaire — l'attaque est simultanée pendant le tour joueur)*

### Mana
- **Pour les sorts uniquement** — les unités sont gratuites
- Croissant : 1 mana au tour 1, +1 par tour joueur, plafond à 6
- Se régénère entièrement à chaque tour

---

## Système de synergies d'adjacence (Bonds)

Deux unités adjacentes (gauche/droite) avec des éléments compatibles déclenchent un **Bond**.

### Règle de cumul
Une unité encadrée par deux éléments différents **déclenche les deux Bonds simultanément**.

```
[⚡][🔥][🌑]
    ↑
 Bond A (⚡+🔥) ET Bond B (🔥+🌑) s'activent tous les deux
```

### Timing des Bonds
- **Bonds à l'attaque** : s'activent pendant la phase d'attaque (bonus offensifs, chains)
- **Bonds passifs** : actifs en permanence tant que les deux unités sont adjacentes et vivantes

### Table de synergies (validée)

| Paire       | Nom           | Type    | Effet                                                        |
|-------------|---------------|---------|--------------------------------------------------------------|
| 🔥 + 🔥    | Embrasement   | Attaque | +1 ATK, les dégâts splash sur les cases ennemies adjacentes  |
| ❄️ + ❄️    | Blizzard      | Attaque | L'unité ciblée passe son prochain tour d'attaque (Freeze)    |
| ⚡ + ⚡    | Surcharge     | Attaque | Si l'ennemi en face a déjà subi des dégâts ce tour, +2 ATK   |
| 🌑 + 🌑    | Abîme         | Attaque | Drain : soigne 1 HP à l'unité attaquante                     |
| 🌿 + 🌿    | Forêt dense   | Passif  | +1 HP max aux deux unités 🌿                                 |
| ⚡ + 🔥    | Éclair ardent | Attaque | L'attaque saute sur la case ennemie adjacente après impact    |
| ⚡ + 🌑    | Foudre noire  | Attaque | Ignore les HP bonus de la cible, dégâts bruts                |
| ❄️ + 🌿    | Givre vivant  | Passif  | L'unité 🌿 régénère 1 HP par tour si adjacente à ❄️          |
| 🔥 + 🌑    | Cendres       | Attaque | Si l'ennemi en face meurt, l'unité 🌑 adjacente gagne +1 ATK permanent |
| 🌿 + 🌑    | Décomposition | Passif  | Les unités ennemies adjacentes perdent 1 HP max (debuff permanent) |

---

## HP et dégâts

### Unités joueur/ennemi
- **HP** : 1 à 3 selon la carte
- **ATK** : 1 à 3 selon la carte
- Mort à HP = 0 → défausse

### HP des camps
| Type de combat | HP ennemi |
|----------------|-----------|
| Combat normal  | 20 HP     |
| Combat élite   | 35 HP     |
| Boss           | 50 HP     |

- **HP joueur** : global persistant entre les combats — **30 HP de départ**
- Récupération uniquement via nœud Repos sur la RunMap

---

## Deck et main

- **Taille du deck** : 20 cartes minimum
- **Répartition** : libre selon le personnage (pas de ratio imposé unités/sorts)
- **Main de départ** : 4 cartes
- **Pioche** : 2 cartes au début de chaque tour joueur
- **Main maximale** : 10 cartes
- Deck vide → la défausse est mélangée pour reformer un nouveau deck

---

## IA ennemie

- Chaque ennemi a un **ScriptableObject** dédié avec son deck et sa logique de comportement
- L'ennemi joue aussi des sorts (depuis son deck)
- **Comportements possibles** (à définir par ennemi) :
  - Agressif : priorité lanes vides pour fuir et faire des dégâts directs
  - Défensif : priorité bloquer les lanes joueur occupées
  - Synergy-seeking : cherche à construire des Bonds dans sa ligne

---

## La tension centrale : construire sa ligne

```
Où poser cette unité pour maximiser les Bonds
sans laisser de lanes vides qui saignent mes HP ?

[ ][🔥][ ][ ][ ]  ← état actuel, P1 et P3-P5 vides
[⚡ en main]

Option A : poser en P1 → bouche une fuite, pas de Bond
Option B : poser en P3 → Bond ⚡+🔥 activé (Éclair ardent), mais P1 continue de saigner 1 HP/tour
Option C : attendre et jouer un sort de déplacement pour repositionner 🔥 d'abord
```

---

## Boucle roguelike

```
DÉPART
  │
  ▼
┌─────────────┐    victoire    ┌──────────────┐
│   Combat    │ ─────────────► │  Récompense  │
│  (2×5 grid) │                │  (carte +    │
└─────────────┘                │   relique ?) │
       │ défaite               └──────┬───────┘
       ▼                              │
   Game Over              ┌───────────▼──────────┐
                          │      Carte de run     │
                          │  Combat / Shop /      │
                          │  Repos / Forge / etc. │
                          └───────────────────────┘
```

---

## DA validée

- **Style** : Wildfrost — chibi animal, flat cartoon, thick black outlines, sticker style avec contour blanc
- **Plateau** : Table d'enchantement vue de dessus, bords décorés (bougies, grimoires, gemmes), centre uniforme et épuré
- **Couleurs** : bois sombre, lueur teal/cyan pour les runes, ambre chaud pour les bougies
- **Persos existants** : CatSorcerer (🔥 Feu), RaccoonNecromancer (🌑 Ombre)

---

## Ce qui est conservé du projet existant

- Structure roguelike (RunMap, nodes, Forge, Shop, Rest, Event)
- Système de sauvegarde
- Architecture Unity (ScriptableObjects, DOTween, namespaces)
- Scènes MainMenu, CharacterSelect, RunMap

## Ce qui est à réécrire / créer

- Système de combat (grille 3×3 → ligne 2×5, attaque simultanée, Bonds, mana sorts)
- Tous les CardData ScriptableObjects (nouveau format avec élément + Bond)
- IA ennemie scriptable
- DA et assets visuels
