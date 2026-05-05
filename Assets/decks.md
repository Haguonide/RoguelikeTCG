# Decks Prototype — RoguelikeTCG

> Créer les CardData ScriptableObjects dans Unity via **Assets > Create > RoguelikeTCG > Card**.
> Dossiers : `Assets/Data/Cards/ProgrammeR/` et `Assets/Data/Cards/Enemies/LesContractuels/`
> Les cartes utilitaires (Repioche, Déplacement) sont dans `Assets/Data/Cards/Utilities/`.

---

## Comment créer un CardData (champs Inspector)

| Champ | Description |
|---|---|
| `cardName` | Nom affiché sur la carte |
| `cardType` | Unit / Spell / Utility |
| `rarity` | Common / Rare / Epic / Legendary |
| `manaCost` | Coût en mana (1–4) |
| `hp` | HP unité (1–3) — *Unit uniquement* |
| `countdown` | CD avant 1re attaque (1–4) — *Unit uniquement* |
| `attackDirections` | Flags multi-select : Up / Down / Left / Right |
| `keyword` | Aucun / Hâte / Épine / Explosion / Combo / Inspiration / Légion / Dominance / Percée / Ralliement |
| `positionalCondition` | None / Corner / Edge / Center |
| `positionalEffect` | None / PlusOneATK / MinusOneCD / DrawCard / PlusOnePoint |
| `spellTarget` | PlayerHero / EnemyHero / AllyUnit / EnemyUnit / AllEnemyUnits / AllAllyUnits — *Spell uniquement* |
| `effects` | Liste `CardEffect` (effectType + value) — *Spell uniquement* |
| `artwork` | Sprite de l'illustration |
| `upgradedVersion` | Référence vers la version + |

**ATK est toujours implicite = 1.** Si un passif positionnel `PlusOneATK` est actif, l'attaque devient 2.

---

## Anatomie d'un tour de jeu (rappel)

- 1 unité maximum jouée par tour
- Autant de sorts que le mana permet
- Fin de tour → toutes les unités tick -1 CD → celles à 0 attaquent → CD repart à sa valeur initiale
- 6 tours par joueur par manche — mana démarre à 1 et +1 par tour (plafond 6)

---

## PROGRAMME R — Archétype Aggro (30 cartes)

**Concept :** Ex-vilains en liberté conditionnelle. Keywords dominants : Hâte, Percée, Explosion, Ralliement.  
**Structure :** 4 héros × 7 cartes + 2 Cartes Repioche = 30 cartes

---

### VOLTAIRE — Électricité / Percée

| Carte | Type | Mana | HP | CD | Flèches | Keyword | Passif | Copies |
|---|---|---|---|---|---|---|---|---|
| Voltaire | Unit / Epic | 3 | 2 | 3 | Right + Left | Aucun | — | ×1 |
| Éclair Conduit | Unit / Rare | 2 | 1 | 1 | Right + Left | Hâte | — | ×1 |
| Parafoudre | Unit / Common | 1 | 1 | 2 | Right | Percée | — | ×2 |
| Surcharge | Spell / Rare | 2 | — | — | — | — | — | ×1 |
| Court-Circuit | Spell / Common | 1 | — | — | — | — | — | ×2 |

**Voltaire :** pose coûteuse, HP solide, attaque horizontale.  
**Éclair Conduit :** Hâte = CD démarre à 1 (countdown=1 dans l'Inspector). Attaque au tour suivant.  
**Parafoudre :** Percée — si elle tue une unité, attaque aussi la case derrière.  
**Surcharge :** cible EnemyUnit. effects = [{effectType: Damage, value: 1}]. Inflige 1 dégât.  
**Court-Circuit :** cible AllyUnit. effects = [{effectType: ReduceCountdown, value: 1}]. -1 CD.

---

### CENDRES — Feu / Explosion

| Carte | Type | Mana | HP | CD | Flèches | Keyword | Passif (condition / effet) | Copies |
|---|---|---|---|---|---|---|---|---|
| Cendres | Unit / Epic | 3 | 2 | 3 | Up + Down | Aucun | — | ×1 |
| Bombe à Retardement | Unit / Rare | 2 | 1 | 3 | None | Explosion | — | ×1 |
| Flammèche | Unit / Common | 1 | 1 | 2 | Up | Aucun | Corner → PlusOneATK | ×2 |
| Embrasement | Spell / Rare | 2 | — | — | — | — | — | ×1 |
| Fumée Noire | Spell / Common | 1 | — | — | — | — | — | ×2 |

**Bombe à Retardement :** attackDirections = None (ne fait aucune attaque). À la mort (Explosion) : inflige 1 dégât à toutes les unités adjacentes alliées et ennemies. Rareté Rare minimum obligatoire pour Explosion.  
**Flammèche :** En coin (cases 0,2,6,8) → attaque à 2 dégâts. Sinon : 1 dégât.  
**Embrasement :** cible AllEnemyUnits. effects = [{effectType: Damage, value: 1}].  
**Fumée Noire :** cible AllyUnit. effects = [{effectType: ReduceCountdown, value: 2}]. -2 CD.

---

### LE BLOC — Force brute / Ralliement

| Carte | Type | Mana | HP | CD | Flèches | Keyword | Passif (condition / effet) | Copies |
|---|---|---|---|---|---|---|---|---|
| Le Bloc | Unit / Epic | 4 | 3 | 4 | Up+Down+Left+Right | Aucun | Center → PlusOneATK | ×1 |
| Mur de Chair | Unit / Rare | 2 | 3 | 3 | Up | Dominance | — | ×1 |
| Sbire Musclé | Unit / Common | 2 | 2 | 2 | Up + Down | Ralliement | — | ×2 |
| Ordre Simple | Spell / Common | 1 | — | — | — | — | — | ×1 |
| En Formation | Spell / Rare | 2 | — | — | — | — | — | ×2 |

**Le Bloc :** En centre (case 4) → attaque à 2 dégâts dans les 4 directions. HP=3 : absorbe 3 attaques.  
**Mur de Chair :** Dominance — si encore en vie en fin de manche : +1 pt.  
**Sbire Musclé :** Ralliement — à la pose, -1 CD à toutes les unités alliées adjacentes.  
**Ordre Simple :** cible AllyUnit. effects = [{effectType: ReduceCountdown, value: 1}].  
**En Formation :** cible AllAllyUnits. effects = [{effectType: ReduceCountdown, value: 1}].

---

### TRACE — Super-vitesse / Hâte

| Carte | Type | Mana | HP | CD | Flèches | Keyword | Passif (condition / effet) | Copies |
|---|---|---|---|---|---|---|---|---|
| Trace | Unit / Epic | 3 | 2 | 2 | Left + Right | Aucun | — | ×1 |
| Ligne Directe | Unit / Rare | 2 | 1 | 1 | Right | Hâte | Edge → PlusOneATK | ×1 |
| Faux Départ | Unit / Common | 1 | 1 | 2 | Up | Combo | — | ×2 |
| Vitesse Relative | Spell / Rare | 2 | — | — | — | — | — | ×1 |
| En Retard Comme d'hab | Spell / Common | 1 | — | — | — | — | — | ×2 |

**Ligne Directe :** countdown=1 dans l'Inspector (Hâte). En bord → attaque à 2 dégâts.  
**Faux Départ :** Combo — si ce placement complète un motif actif, +1 pt bonus.  
**Vitesse Relative :** cible AllyUnit. effects = [{effectType: ReduceCountdown, value: 2}].  
**En Retard Comme d'hab :** cible PlayerHero. effects = [{effectType: DrawCard, value: 1}]. Pioche 1 carte.

---

### Utilitaires — 2 cartes

| Carte | Type | Mana | Description |
|---|---|---|---|
| Repioche | Utility / Common | 1 | Mélange la main dans le deck, pioche autant de cartes. Max 2/deck. |
| Déplacement | Utility / Common | 1 | Déplace une unité alliée vers une case adjacente vide, reset son CD. Max 2/deck. |

Ces deux assets existent déjà dans `Assets/Data/Cards/Utilities/`. Vérifier les valeurs.

---

### Récapitulatif Programme R

| Héros | Cartes (×copies) | Total |
|---|---|---|
| Voltaire | Voltaire ×1 · Éclair Conduit ×1 · Parafoudre ×2 · Surcharge ×1 · Court-Circuit ×2 | 7 |
| Cendres | Cendres ×1 · Bombe à Retardement ×1 · Flammèche ×2 · Embrasement ×1 · Fumée Noire ×2 | 7 |
| Le Bloc | Le Bloc ×1 · Mur de Chair ×1 · Sbire Musclé ×2 · Ordre Simple ×1 · En Formation ×2 | 7 |
| Trace | Trace ×1 · Ligne Directe ×1 · Faux Départ ×2 · Vitesse Relative ×1 · En Retard Comme d'hab ×2 | 7 |
| Utilitaires | Repioche ×2 | 2 |
| **TOTAL** | | **30** |

---

## DECK ENNEMI — Les Contractuels (20 cartes)

**Concept :** Super-héros sous CDI. Pouvoirs réels, motivation inexistante. Font ça comme un job 9h-17h.  
**Dossier :** `Assets/Data/Cards/Enemies/LesContractuels/` (créer le dossier si absent)

---

| Carte | Type | Mana | HP | CD | Flèches | Keyword | Passif | Copies |
|---|---|---|---|---|---|---|---|---|
| Patrice | Unit / Common | 2 | 2 | 3 | Up + Down | Aucun | Edge → PlusOneATK | ×2 |
| Régine | Unit / Common | 1 | 1 | 2 | Right | Inspiration | — | ×2 |
| Chad | Unit / Rare | 3 | 3 | 4 | Up | Dominance | — | ×2 |
| Marlène | Unit / Common | 2 | 1 | 2 | Left + Right | Légion | — | ×2 |
| Agent Syndical | Unit / Common | 1 | 1 | 2 | Up | Combo | Corner → PlusOnePoint | ×2 |
| Consultant Externe | Unit / Common | 2 | 2 | 2 | Up + Right | Combo | — | ×2 |
| Note de Frais | Spell / Common | 1 | — | — | — | — | — | ×2 |
| Heure Sup | Spell / Common | 2 | — | — | — | — | — | ×2 |
| Coup de Bureaucratie | Spell / Rare | 2 | — | — | — | — | — | ×2 |
| Repioche | Utility / Common | 1 | — | — | — | — | — | ×2 |

**Notes sorts :**  
- **Note de Frais :** cible AllyUnit. effects = [{ReduceCountdown, 1}].  
- **Heure Sup :** cible AllyUnit. effects = [{ReduceCountdown, 2}].  
- **Coup de Bureaucratie :** cible AllAllyUnits. effects = [{ReduceCountdown, 1}].

**Notes unités :**  
- **Patrice :** En bord → attaque à 2 dégâts. "Préserve son dos."  
- **Régine :** Pioche 1 carte à la pose. "Pour son café."  
- **Chad :** HP=3, survit longtemps. Dominance : +1 pt si encore là fin de manche.  
- **Marlène :** CD réduit par unité alliée adjacente (min 1). Forte en groupe.  
- **Agent Syndical :** En coin → +1 pt à la pose. Bonus de placement.  
- **Consultant Externe :** Combo → +1 pt si complète un motif.

---

## PatternData ScriptableObjects

**À créer dans :** `Assets/Data/Patterns/`  
**Comment créer :** **Create → RoguelikeTCG → Pattern**

Le `PatternManager` a besoin d'un minimum de **3 PatternData** dans sa liste `allPatterns`.  
Pour le prototype, créer au moins les 6 motifs 3-cases et 4 motifs 4-cases ci-dessous.

```
Numérotation des cases :
0 1 2
3 4 5
6 7 8
```

---

### 3 cases — 4 pts (créer au minimum 6)

| Fichier asset | patternName | cellIndices |
|---|---|---|
| `PatternLigneH0` | Ligne haute | [0, 1, 2] |
| `PatternLigneH1` | Ligne milieu | [3, 4, 5] |
| `PatternLigneH2` | Ligne basse | [6, 7, 8] |
| `PatternColV0` | Colonne gauche | [0, 3, 6] |
| `PatternColV1` | Colonne centre | [1, 4, 7] |
| `PatternColV2` | Colonne droite | [2, 5, 8] |
| `PatternDiagDesc` | Diagonale ↘ | [0, 4, 8] |
| `PatternDiagMont` | Diagonale ↗ | [2, 4, 6] |
| `PatternCoinTL` | Coin haut-gauche | [0, 1, 3] |
| `PatternCoinTR` | Coin haut-droite | [1, 2, 5] |
| `PatternCoinBL` | Coin bas-gauche | [3, 6, 7] |
| `PatternCoinBR` | Coin bas-droite | [5, 7, 8] |

---

### 4 cases — 6 pts

| Fichier asset | patternName | cellIndices |
|---|---|---|
| `PatternCarreTL` | Carré haut-gauche | [0, 1, 3, 4] |
| `PatternCarreTR` | Carré haut-droite | [1, 2, 4, 5] |
| `PatternCarreBL` | Carré bas-gauche | [3, 4, 6, 7] |
| `PatternCarreBR` | Carré bas-droite | [4, 5, 7, 8] |
| `PatternQuatreCoin` | 4 Coins | [0, 2, 6, 8] |
| `PatternTHaut` | T haut | [1, 3, 4, 5] |
| `PatternTBas` | T bas | [3, 4, 5, 7] |
| `PatternTGauche` | T gauche | [1, 3, 4, 7] |
| `PatternTDroite` | T droite | [1, 4, 5, 7] |

---

### 5 cases — 9 pts

| Fichier asset | patternName | cellIndices |
|---|---|---|
| `PatternCroix` | Croix | [1, 3, 4, 5, 7] |
| `PatternX` | X total | [0, 2, 4, 6, 8] |
| `PatternUHaut` | U haut | [0, 2, 3, 5, 6] |

---

## CharacterData — Câbler les decks aux personnages

Une fois les cartes créées :

1. **Créer** (si absent) `Assets/Data/Characters/ProgrammeRCharacter.asset`
   - **Create → RoguelikeTCG → Character**
   - `characterName` : "Programme R"
   - `startingDeck` : liste des 30 cartes (pour les doublons, ajouter le même asset deux fois)

2. **Créer** `Assets/Data/Characters/ContractuelsCharacter.asset`
   - `characterName` : "Les Contractuels"
   - `startingDeck` : liste des 20 cartes

3. **Dans la scène Combat**, sur le `CombatManager` :
   - `playerCharacter` → `ProgrammeRCharacter`
   - `enemyCharacter` → `ContractuelsCharacter`

4. **Assigner `allPatterns`** dans le `PatternManager` (enfant de `GridManager`) :
   - Glisser tous les PatternData créés dans la liste `allPatterns`

---

## Notes de création Unity

1. **Pour les doublons en deck :** `startingDeck` est une `List<CardData>` — ajouter deux fois le même asset pour ×2.
2. **Sorts :** remplir la liste `effects` avec un `CardEffect` (effectType + value). Laisser vide les champs Unit.
3. **Hâte :** se configure simplement avec `countdown = 1` dans l'Inspector (le keyword Hâte = CD fixe 1).
4. **Passifs positionnels :** assigner les deux champs `positionalCondition` ET `positionalEffect`. Si l'un des deux est None, le passif est inactif.
5. **Upgrades :** créer d'abord la version `+`, puis l'assigner dans `upgradedVersion` de la version de base.
6. **Artwork :** laisser null pour l'instant, à assigner manuellement quand les sprites sont prêts.
7. **Légion :** le calcul se fait dynamiquement au runtime — pas de config supplémentaire.
