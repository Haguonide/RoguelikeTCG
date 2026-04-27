# Design — Sélection de team (analyse options)

## Contexte

Refonte de l'univers du jeu : abandon des figures historiques, passage à un univers super-héros burlesque (ton Dispatch). Au lieu de choisir UN personnage au début d'un run, le joueur choisit une **team de héros** avec un capitaine (portrait à côté des HP) et un archétype de gameplay.

---

## Option 1 — Teams préfabriquées ✅ RETENUE

Tu choisis parmi 5-8 teams fixes au lancement d'un run. Chaque team a un nom, des membres définis, un capitaine, un archétype de gameplay, une passive d'équipe, et un pool de cartes propre.

### Points forts
- **Balance maîtrisable** — chaque team est une unité isolée, testée en elle-même
- **Identité narrative forte** — membres avec histoire commune, dialogues possibles, running jokes internes
- **Mécaniques profondes** — une mécanique unique par team entière, pas juste par perso
- **Onboarding simple** — 5-6 options claires avec description d'archétype
- **Scope gérable solo dev** — 5 teams × 3-4 membres = 15-20 persos à illustrer max
- **Migration propre** — CharacterData → TeamData, portrait = capitaine, pool = team

### Points faibles
- Pas de méta-game pré-run
- Scaling coûteux (team entière = chunk de contenu lourd)
- Synergies inter-teams impossibles

> Note : la "rejouabilité plafonnée" n'est pas une vraie faiblesse — Slay the Spire a 4 persos et est un des roguelikes les plus rejoués. La variabilité vient du run lui-même (draft, reliques, events, chemins), pas de la sélection.

### Exemple de teams

| Team | Membres | Archétype | Mécanique signature |
|---|---|---|---|
| Les Retraités de l'ESPAD | 3 anciens super-vilains reconvertis à la caisse d'un supermarché | Attrition / survie | Unités lentes, dures à tuer, bonus après chaque manche gagnée |
| PR & Heroes Inc. | Super-héros sponsorisés qui font plus de pub que de combat | Ressources / mana | Chaque combo scoreé rapporte de l'or supplémentaire |
| L'Équipe Z | Les ratés officiels, rejetés de toutes les autres teams | Chaos / variance | Effets aléatoires mais potentiellement broken, cartes "bugged" |
| La Cellule Syndicale | Ex-sbires qui ont fait grève et monté leur syndicat | Swarm / légion | Bonus si 4+ unités alliées sur la grille |
| Les Stagiaires Musclés | Jeunes héros incompétents mais hyper motivés | Combo / momentum | Chaque carte jouée dans le même tour augmente le prochain effet |

---

## Option 2 — Roster libre (non retenue pour le prototype)

Pool de 15-25 héros, le joueur en choisit 3-4 avant le run. Deck de départ = fusion des pools de base.

### Points forts
- Rejouabilité quasi infinie
- Méta-game avant le run
- Scaling granulaire (1 héros à la fois)
- Synergies secrètes et culture de découverte
- Appropriation du run par le joueur

### Points faibles
- **Balance cauchemardesque** — 15 héros = 455 combos de 3, ingérable en solo sans QA
- Cohérence narrative quasi impossible
- Deck de départ dilué (~7 cartes par héros si team de 3)
- Onboarding difficile sans unlock progressif
- DA volumineuse avant d'atteindre une masse critique
- Design de cartes plus rigide (doit fonctionner sans contexte)

---

## Options hybrides (à explorer post-prototype)

### Hybride A — Slot wildcard
Team fixe (capitaine + 2 membres) + 1 héros libre choisi parmi un sous-roster de "mercenaires". Garde la cohérence narrative et l'équilibre, ajoute une couche de personnalisation. Synergies secrètes contraintes à 1 variable.

### Hybride B — Membres alternatifs par team
Chaque team a un roster de 5-6 membres potentiels, on en joue 3 à la fois. L'archétype reste stable, mais la combinaison interne change le deck de départ et les synergies internes.

---

## Décision

**Option 1 retenue pour le prototype.** Les hybrides A et B sont des features de profondeur à greffer si le jeu trouve son public et que le roster grossit naturellement.
