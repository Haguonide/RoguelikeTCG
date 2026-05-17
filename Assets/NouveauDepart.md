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
│  "Effet si  │
│   adjacent  │
│   à [elem]" │
└─────────────┘
   Coût : X mana
```

---

## Système de synergies d'adjacence (Bonds)

Deux unités **adjacentes** avec des éléments compatibles déclenchent un bond au moment de l'attaque.

### Règle de cumul (validée)
Une unité entourée de deux éléments différents **déclenche les deux bonds simultanément**.
Cela récompense le placement stratégique. Contre-balance naturelle : ça coûte 3 cartes et plusieurs tours à construire — l'ennemi a le temps de casser une case.

```
[🌪️][🔥][⚡]
 bond A  bond B  → les deux s'activent, 🔥 bénéficie des deux effets
```

### Table de synergies (draft)

| Paire        | Nom Bond  | Effet                                  |
|--------------|-----------|----------------------------------------|
| 🔥 + 🌪️    | Brasier   | +1 ATK, splash sur la case adjacente   |
| ❄️ + ❄️    | Blizzard  | Freeze ennemi (passe son attaque)      |
| ⚡ + 🌊    | Foudre    | L'attaque saute sur 2 ennemis          |
| 🌑 + 🌑    | Ombre     | Drain 1 HP ennemi → joueur             |
| 🌿 + any    | Racines   | +1 HP à l'unité adjacente              |

*(Table à compléter / équilibrer)*

---

## Structure d'un tour

### Phase joueur
1. **Pioche** — tirer 3 cartes
2. **Phase de jeu** — placer des unités sur cases vides, jouer des sorts (limité par mana)
3. **Phase d'attaque** :
   - Chaque unité attaque la case directement en face
   - Les bonds d'adjacence se déclenchent avant résolution des dégâts
   - Case vide en face = **fuite de lane** → 1 dégât direct aux HP ennemis
4. **Riposte ennemie** — les unités ennemies survivantes attaquent en retour
5. **Cleanup** — unités mortes en défausse, survivantes restent en place

### Mana
- Croissant : 1 au tour 1, +1 par tour, plafond à 6
- Se régénère entièrement chaque manche

---

## La tension centrale : construire sa ligne

```
Où poser cette unité pour maximiser les bonds
sans laisser de lanes libres qui saignent mes HP ?

[ ][🌪️][ ][ ][ ]  ← état actuel
[🔥 en main]

Option A : poser en P1 → pas adjacent à 🌪️, pas de bond
Option B : poser en P2 → bond 🔥+🌪️ activé, mais P1 vide = fuite de dégâts
Option C : poser en P3 → comble un trou, pas de bond
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
- **Persos existants** : CatSorcerer (🔥 ?), RaccoonNecromancer (🌑 ?)

---

## Ce qui est conservé du projet existant

- Structure roguelike (RunMap, nodes, Forge, Shop, Rest, Event)
- Système de sauvegarde
- Architecture Unity (ScriptableObjects, DOTween, namespaces)
- Scènes MainMenu, CharacterSelect, RunMap

## Ce qui est à réécrire

- Système de combat (grille 3×3 → ligne 2×5, nouveau système d'attaque et synergies)
- Tous les CardData ScriptableObjects
- IA ennemie
- DA et assets visuels
