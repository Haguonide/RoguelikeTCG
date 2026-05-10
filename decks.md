# Decks — Design & État des cartes

---

## Changements à appliquer dans l'Inspector Unity
*(suite refonte combat v3 — 2026-05-10 : suppression CD, nouveaux keywords)*

### Procédure générale

1. Ouvrir chaque `.asset` dans l'Inspector Unity
2. Appliquer les changements listés ci-dessous
3. Le champ `countdown` a disparu du script — Unity l'ignorera automatiquement (valeur orpheline dans le YAML, sans conséquence)

---

### Cartes unités — changement de keyword

Les keywords suivants ont été renommés (même valeur entière, donc la donnée sérialisée est déjà correcte — **aucun changement manuel nécessaire** pour ces cartes, juste vérifier que l'affichage dans l'Inspector est cohérent) :

| Ancien keyword | Nouveau keyword | Cartes concernées |
|---|---|---|
| Hâte (1) | Impact (1) | Ligne Directe, Ligne Directe+, Éclair Conduit, Éclair Conduit+ |
| Légion (7) | Essaim (7) | Sbire Musclé, Sbire Musclé+ |
| Ralliement (10) | Réveil (10) | Marlène |

**Note :** Les descriptions affichées en jeu devront être mises à jour pour refléter le nouvel effet :
- **Impact** : "À la pose, inflige 1 dégât supplémentaire à la première cible touchée"
- **Essaim** : "+1 ATK à la pose par unité alliée adjacente"
- **Réveil** : "À la pose, chaque unité alliée adjacente attaque à nouveau dans ses directions"

---

### Cartes sorts — remplacement de ReduceCountdown (effectType: 3)

Le champ `effectType: 3` est désormais `BuffNextUnitATK` ("la prochaine unité jouée gagne +X ATK"). La plupart des sorts l'adoptent directement. Les exceptions sont signalées.

#### Programme R — Cendres

| Carte | Mana | Avant | Après | Notes |
|---|---|---|---|---|
| **Fumée Noire** | 1 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 1 | Cible → aucune (buff passif consommé à la pose) |
| **Fumée Noire+** | 1 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 2 | Upgrade : +2 ATK au lieu de +1 |

#### Programme R — Le Bloc

| Carte | Mana | Avant | Après | Notes |
|---|---|---|---|---|
| **En Formation** | 2 | ReduceCountdown, toutes unités alliées | **BuffAllAllyHP** (effectType 5), effectValue: 1 | +1 HP à toutes les unités alliées sur la grille. SpellTarget: AllAllyUnits (5) — inchangé |
| **En Formation+** | 1 | ReduceCountdown, toutes unités alliées | **BuffAllAllyHP** (effectType 5), effectValue: 1 | Même effet, coût réduit à 1 mana |
| **Ordre Simple** | 1 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 1 | Cible → aucune (buff consommé à la pose) |
| **Ordre Simple+** | 1 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 2 | Upgrade : +2 ATK |

#### Programme R — Trace

| Carte | Mana | Avant | Après | Notes |
|---|---|---|---|---|
| **Vitesse Relative** | 2 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 2 | +2 ATK prochaine unité — Trace "accélère" la pose suivante |
| **Vitesse Relative+** | 2 | ReduceCountdown, toutes unités alliées | **TriggerAllAllyAttack** (effectType 9), effectValue: 0 | Toutes les unités alliées attaquent à nouveau. SpellTarget: AllAllyUnits (5) |

#### Programme R — Voltaire

| Carte | Mana | Avant | Après | Notes |
|---|---|---|---|---|
| **Court-Circuit** | 1 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 1 | Voltaire "court-circuite" la prochaine frappe |
| **Court-Circuit+** | 1 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 2 | Upgrade : +2 ATK |

#### Les Contractuels (ennemi)

| Carte | Mana | Avant | Après | Notes |
|---|---|---|---|---|
| **Coup De Bureaucratie** | 2 | ReduceCountdown, toutes unités alliées | **BuffAllAllyHP** (effectType 5), effectValue: 1 | La bureaucratie les rend plus coriaces. SpellTarget: AllAllyUnits (5) |
| **Heure Sup** | 2 | ReduceCountdown, 1 unité alliée ciblée | **BuffNextUnitATK** (effectType 3), effectValue: 1 | Heures sup = frapper plus fort |
| **Note De Frais** | 1 | ReduceCountdown, 1 unité alliée ciblée | **DrawCard** (effectType 2), effectValue: 2 | La note de frais génère des ressources — pioche 2 cartes |

---

### PositionalEffect — aucun changement manuel requis

`MinusOneCD (2)` renommé en `PlusOneHP (2)` dans l'enum. Aucune carte n'utilisait cette valeur, donc zéro impact sur les assets existants. Cette valeur est maintenant disponible si une future carte a un passif positionnel "+1 HP".

---

### Récapitulatif des effectType à jour

| Valeur | Nom | Effet |
|---|---|---|
| 0 | Damage | Dégâts au héros ou kill unité ciblée |
| 1 | Heal | Soins au héros joueur |
| 2 | DrawCard | Pioche X cartes |
| 3 | BuffNextUnitATK | La prochaine unité jouée gagne +X ATK |
| 4 | DestroyUnit | Détruit instantanément une unité ciblée |
| 5 | BuffAllAllyHP | +X HP à toutes les unités alliées sur la grille |
| 9 | TriggerAllAllyAttack | Toutes les unités alliées attaquent à nouveau |
