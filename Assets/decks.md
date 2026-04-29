# Design des decks — Référence de création

> Créer les CardData ScriptableObjects dans Unity via Assets > Create > RoguelikeTCG > Card.
> Dossiers : `Assets/Data/Cards/ProgrammeR/` et `Assets/Data/Cards/LesEternels/`
> Chaque carte a une version de base et une version upgradée (+) liée via le champ `upgradedVersion`.
> Les cartes utilitaires (Repioche, Déplacement) sont dans `Assets/Data/Cards/Utilities/`.

---

## Rappel anatomie carte unité

| Champ | Valeurs |
|---|---|
| cardType | Unit |
| countdown | 1–3 |
| attackDirections | Up / Down / Left / Right (combinables) |
| keyword | voir liste ci-dessous |
| manaCost | 1–4 |

**Keywords disponibles :** Hâte (CD fixe 1), Bouclier (absorbe 1re attaque), Épine (à la mort : kill 1 ennemi adj), Explosion (à la mort : kill tous adj), Combo (+1 pt si complète un motif), Inspiration (pose : pioche 1), Légion (CD -1 par allié adjacent), Dominance (survit à la manche : +1 pt), Percée (si kill : attaque case derrière), Ralliement (pose : -1 CD aux alliés adjacents)

---

## Cartes utilitaires (communes aux deux decks)

| Nom | Type | Mana | Description |
|---|---|---|---|
| Carte Repioche | Utility | 0 | Mélange la main dans le deck et pioche autant de cartes. Jamais upgradable. Max 2 par deck. |
| Carte Déplacement | Utility | 1 | Déplace une unité alliée vers une case adjacente vide. Reset son CD. Jamais upgradable. Max 2 par deck. |

---

## PROGRAMME R — Archétype Aggro (30 cartes)

**Concept :** Ex-vilains en liberté conditionnelle. Keywords dominants : Hâte, Percée, Explosion, Ralliement.
**Structure :** 4 héros × 7 cartes + 2 Cartes Repioche = 30 cartes

---

### VOLTAIRE — Électricité / Percée

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Voltaire | Unit / Rare | 3 | 2 | Right + Left | Percée | ×1 | CD 1 |
| Arc Électrique | Unit / Commune | 3 | 2 | Up + Down | Percée | ×1 | + Right aussi (3 flèches) |
| Étincelle | Unit / Commune | 1 | 2 | Right | Aucun | ×2 | CD 1 |
| Court-Circuit | Sort / Rare | 4 | — | — | — | ×1 | Mana 3 |
| Surtension | Sort / Commune | 2 | — | — | — | ×2 | DrawCard 3 au lieu de 2 |

**Court-Circuit :** cible EnemyUnit, effet DestroyUnit — détruit instantanément une unité ennemie.
**Surtension :** cible PlayerHero, effet DrawCard value=2 — pioche 2 cartes.

---

### CENDRES — Feu / Explosion

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Cendres | Unit / Rare | 4 | 2 | Right + Down | Explosion | ×1 | CD 1 |
| Bombe à Retardement | Unit / Commune | 2 | 3 | None | Explosion | ×1 | Mana 1 |
| Flammèche | Unit / Commune | 1 | 2 | Right | Aucun | ×2 | Right + Down |
| Embrasement | Sort / Rare | 3 | — | — | — | ×1 | Mana 2 |
| Fumée Noire | Sort / Commune | 1 | — | — | — | ×2 | DrawCard 2 au lieu de 1 |

**Bombe à Retardement :** attackDirections = None. Ne fait rien jusqu'à sa mort, puis Explosion détruit tout autour.
**Embrasement :** cible AllAllyUnits, effet ReduceCountdown value=1 — réduit le CD de toutes les unités alliées de 1.
**Fumée Noire :** cible PlayerHero, effet DrawCard value=1.

---

### LE BLOC — Force brute / Ralliement

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Le Bloc | Unit / Rare | 4 | 2 | Up + Down + Left + Right | Ralliement | ×1 | CD 1 |
| Muraille | Unit / Commune | 2 | 3 | Right + Left | Aucun | ×1 | CD 2 |
| Sbire Musclé | Unit / Commune | 1 | 2 | Right | Aucun | ×2 | Right + Down |
| Ordre Simple | Sort / Commune | 2 | — | — | — | ×1 | ReduceCountdown 2 au lieu de 1 |
| En Formation | Sort / Commune | 1 | — | — | — | ×2 | DrawCard 2 au lieu de 1 |

**Le Bloc posé au centre (case 4) :** Ralliement réduit le CD des 4 unités adjacentes. Attaque dans les 4 directions. Très fort en centre.
**Ordre Simple :** cible AllyUnit, effet ReduceCountdown value=1 — réduit le CD d'une unité alliée ciblée de 1.
**En Formation :** cible PlayerHero, effet DrawCard value=1.

---

### TRACE — Super-vitesse / Hâte

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Trace | Unit / Rare | 3 | 1 | Right + Left | Hâte | ×1 | Right + Left + Up + Down |
| Ligne Directe | Unit / Commune | 3 | 1 | Right | Percée | ×1 | Right + Down |
| Faux Départ | Unit / Commune | 1 | 2 | Right + Left | Aucun | ×2 | CD 1 |
| Vitesse Relative | Sort / Commune | 2 | — | — | — | ×1 | cible AllAllyUnits |
| En Retard Comme d'hab | Sort / Commune | 2 | — | — | — | ×2 | DrawCard 3 au lieu de 2 |

**Trace :** CD=1 donc attaque dès le tour suivant. Avec Hâte déjà encodé dans countdown=1.
**Vitesse Relative :** cible AllyUnit, effet ReduceCountdown value=2 — réduit le CD d'une unité alliée de 2 (min 1). Version + : cible AllAllyUnits, value=1.
**En Retard Comme d'hab :** cible PlayerHero, effet DrawCard value=2.

---

### Récapitulatif Programme R

| Héros | Cartes | Total copies |
|---|---|---|
| Voltaire | Voltaire ×1, Arc Électrique ×1, Étincelle ×2, Court-Circuit ×1, Surtension ×2 | 7 |
| Cendres | Cendres ×1, Bombe à Retardement ×1, Flammèche ×2, Embrasement ×1, Fumée Noire ×2 | 7 |
| Le Bloc | Le Bloc ×1, Muraille ×1, Sbire Musclé ×2, Ordre Simple ×1, En Formation ×2 | 7 |
| Trace | Trace ×1, Ligne Directe ×1, Faux Départ ×2, Vitesse Relative ×1, En Retard ×2 | 7 |
| Utilitaires | Carte Repioche ×2 | 2 |
| **TOTAL** | | **30** |

---

## LES ÉTERNELS — Archétype Combo/Placement (30 cartes)

**Concept :** Vieux super-héros sortis de retraite. Keywords dominants : Combo, Inspiration, Dominance, Ralliement.
**Structure :** 4 héros × 7 cartes + 2 Cartes Repioche = 30 cartes

---

### ACIERA — Magnétisme / Combo + Dominance

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Aciera | Unit / Rare | 3 | 2 | Right + Down | Combo | ×1 | CD 1 |
| Orbite | Unit / Commune | 2 | 3 | None | Dominance | ×1 | CD 2 |
| Aimant | Unit / Commune | 1 | 2 | Right + Left | Aucun | ×2 | + Keyword Combo |
| Attraction | Sort / Commune | 2 | — | — | — | ×1 | ReduceCountdown 2 au lieu de 1 |
| Champ Magnétique | Sort / Commune | 1 | — | — | — | ×2 | DrawCard 2 au lieu de 1 |

**Orbite :** attackDirections = None. Ne tue rien. Si elle survit à la manche, +1 pt (Dominance).
**Attraction :** cible AllyUnit, effet ReduceCountdown value=1.
**Champ Magnétique :** cible PlayerHero, effet DrawCard value=1.

---

### LE MAÎTRE — Télékinésie / Inspiration

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Le Maître | Unit / Rare | 4 | 2 | Up + Down + Left + Right | Inspiration | ×1 | CD 1 |
| Projection | Unit / Commune | 3 | 2 | Right + Up | Inspiration | ×1 | Right + Up + Down |
| Objet Flottant | Unit / Commune | 1 | 3 | Down | Aucun | ×2 | CD 2 |
| Concentration | Sort / Commune | 3 | — | — | — | ×1 | DrawCard 4 au lieu de 3 |
| Télékinésie | Sort / Commune | 2 | — | — | — | ×2 | ReduceCountdown 2 au lieu de 1 |

**Le Maître :** Pose + pioche 1 carte (Inspiration) + attaque dans les 4 directions. Fort en centre.
**Concentration :** cible PlayerHero, effet DrawCard value=3.
**Télékinésie :** cible AllyUnit, effet ReduceCountdown value=1.

---

### TITANIO — Duplication / Dominance + Combo

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Titanio | Unit / Rare | 3 | 2 | Right | Dominance | ×1 | Right + Left |
| Copie Conforme | Unit / Commune | 2 | 2 | Right | Combo | ×1 | CD 1 |
| Doublon | Unit / Commune | 1 | 2 | Down | Aucun | ×2 | Down + Right |
| Exploits de 1968 | Sort / Commune | 2 | — | — | — | ×1 | DrawCard 3 au lieu de 2 |
| Reproduction | Sort / Commune | 3 | — | — | — | ×2 | Mana 2 |

**Titanio :** Si encore en vie en fin de manche, +1 pt. Poser en position sûre (coin ou derrière un allié).
**Exploits de 1968 :** cible PlayerHero, effet DrawCard value=2.
**Reproduction :** cible AllAllyUnits, effet ReduceCountdown value=1.

---

### GLAMOURA — Illusion / Ralliement + Combo

| Carte | Type | Mana | CD | Flèches | Keyword | Copies | Upgrade (+) |
|---|---|---|---|---|---|---|---|
| Glamoura | Unit / Rare | 3 | 2 | Right + Down | Ralliement | ×1 | Right + Down + Up |
| Miroir | Unit / Commune | 2 | 2 | Right + Up | Combo | ×1 | CD 1 |
| Ombre | Unit / Commune | 1 | 3 | Down | Aucun | ×2 | CD 2 |
| Illusion de 1968 | Sort / Rare | 4 | — | — | — | ×1 | Mana 3 |
| Rideau de Fumée | Sort / Commune | 1 | — | — | — | ×2 | DrawCard 2 au lieu de 1 |

**Illusion de 1968 :** cible EnemyUnit, effet DestroyUnit — retire instantanément une unité ennemie du board.
**Rideau de Fumée :** cible PlayerHero, effet DrawCard value=1.

---

### Récapitulatif Les Éternels

| Héros | Cartes | Total copies |
|---|---|---|
| Aciera | Aciera ×1, Orbite ×1, Aimant ×2, Attraction ×1, Champ Magnétique ×2 | 7 |
| Le Maître | Le Maître ×1, Projection ×1, Objet Flottant ×2, Concentration ×1, Télékinésie ×2 | 7 |
| Titanio | Titanio ×1, Copie Conforme ×1, Doublon ×2, Exploits ×1, Reproduction ×2 | 7 |
| Glamoura | Glamoura ×1, Miroir ×1, Ombre ×2, Illusion de 1968 ×1, Rideau de Fumée ×2 | 7 |
| Utilitaires | Carte Repioche ×2 | 2 |
| **TOTAL** | | **30** |

---

## Notes de création Unity

1. **Sorts** : cardType = Spell, remplir la liste `effects` avec un `CardEffect` (effectType + value + target)
2. **Unités** : cardType = Unit, laisser `effects` vide
3. **Utilitaires** : cardType = Utility, pas d'upgrade, max 2 par deck (contrainte gérée par code)
4. **Upgrades** : créer d'abord la version +, puis assigner dans `upgradedVersion` de la version de base
5. **Artwork** : laisser vide pour l'instant, à assigner manuellement quand les sprites sont prêts
