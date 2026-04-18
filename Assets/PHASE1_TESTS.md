# Tests Phase 1 — Liste de vérification

Supprimer chaque ligne au fur et à mesure que le test est validé.

---

## Passifs — On Entry

- [ ] Poser le **Dragon** → toutes les unités ennemies sur ce board prennent 2 dégâts
- [ ] Poser le **Centurion agressif** → l'unité/héros en face prend 3 dégâts instantanément
- [ ] Poser **César lui-même** → AoE 3 dmg sur tout le board ennemi
- [ ] Poser **Tribun militaire** avec des alliés déjà en jeu → leurs ATK augmentent de 1 --------------------------- Marche mais quand l'adversaire pose une carte, l'affichage sur la carte n'est pas modifié pour son atk 
- [ ] Poser **Légionnaire** avec un allié déjà présent → son ATK passe de 2 à 3
- [ ] Poser la **Trafiquante de radium** → une unité ennemie aléatoire perd 1 ATK

---

## Passifs — On Death

- [ ] Faire tuer l'**Automate boiteux** → l'unité tueuse prend 1 dmg          -------------------- Marche mais doit se mettre à jour direct quand l'automate meurt en affichage et non pas à la fin du tour
- [ ] Faire tuer le **Monstre de Polonium** → AoE 2 dmg sur toutes les unités ennemies
- [ ] Faire tuer le **Gladiateur kamikaze** → le héros ennemi prend 3 dmg directs
- [ ] Faire tuer l'**Assistante contaminée** → l'unité tueuse perd 1 ATK

---

## Passifs — Modifications d'attaque

- [ ] **Arbalète géante** tue une unité avec excès de dégâts → le surplus touche le héros     ------------- Marche mais doit changer l'affichage des hp ennemis directement quand l'attaque est faite (check pour TOUTES les cartes) Et si l'adversaire est vaincu, le combat se stop ici, on continue pas d'abord toutes les actions du tour
- [ ] **Proconsul de la mort** tue une unité → le héros allié est soigné de 2
- [ ] **Char d'assaut** reçoit une attaque → dégâts réduits de 2
- [ ] **Garde prétorienne** présente → toutes les unités alliées prennent -1 dmg     --------------------------- Marche mais quand je pose une carte, l'affichage sur la carte n'est pas modifié pour son atk 
- [ ] **Cobaye volontaire** reçoit une attaque → dégâts réduits de 1
- [ ] **Dealer en blouse blanche** face à un ennemi ATK≤0 → +2 dmg directs au héros

---

## Passifs — Début / Fin de round

- [ ] **Baron de la Pechblende** en jeu → au début de chaque tour joueur, 1 dmg AoE automatique
- [ ] **Vitruve** survit à un round sans mourir → soigne 1 HP héros en fin de round
- [ ] **Chimiste véreux** avec un ennemi à 0 ATK sur le board → soigne 2 HP en fin de round

---

## Sorts De Vinci

- [ ] Jouer **Bouclier parapluie troué** → le héros gagne 5 de bouclier (pas HP)           ---------------------- "Marche" mais on doit montrer clairement le shield quelque part / Ne marche pas, une carte ennemie a mis 2 dégats alors que je suis censé avoir 6 de shield
