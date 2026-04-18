# Analyse Gameplay — Roguelike Deckbuilder (Tournoi des Figures Historiques)
*Date : 2026-04-17*

---

## Forces du gameplay

### Le multi-boards — la vraie originalité
C'est la mécanique différenciatrice par excellence. Gérer 1 à 3 fronts simultanément avec seulement 2 cartes/tour force de vraies décisions de priorisation qu'on ne trouve pas dans Slay the Spire. Quel board sacrifier temporairement ? Où concentrer ses ressources ? C'est une tension permanente qui a du potentiel.

### La limite de 2 cartes/tour
Contrainte forte et élégante. Avec jusqu'à 9 lanes à remplir (3×3), on ne peut jamais tout faire. Chaque tour est une décision difficile, ce qui est exactement ce qu'on veut dans un deckbuilder.

### La Forge — système propre et créateur de dilemmes
3 copies → 1 carte+ est un sacrifice net (-2 cartes du deck). C'est une décision de vraie valeur : améliorer une carte ou garder de la densité ? Et le plafond à 20 cartes minimum donne une limite naturelle qui évite les abus.

### Les archetypes sont lisibles
De Vinci (sustain), Marie Curie (debuff/neutralise), César (agression brute) couvrent 3 philosophies de jeu distinctes et compréhensibles dès le premier coup d'œil au deck.

---

## Faiblesses — par ordre de priorité

---

### 1. Les unités n'ont aucune identité individuelle *(problème central)*

**Le vrai problème :** presque toutes les unités n'ont que des stats ATK/HP et rien d'autre. Le Chien de garde (1/3) et le Vitruve (1/4) se différencient uniquement par 1 point de HP. Il n'y a aucune raison *stratégique* de préférer l'un à l'autre — c'est du calcul mathématique pur, pas de la décision.

Dans Wildfrost (référence visuelle), chaque unité a un effet passif ou un timing qui change sa logique d'utilisation. Dans Slay the Spire, même les cartes communes ont un keyword. Ici, les unités sont des blocs stats interchangeables.

**Conséquence concrète :** le joueur n'a pas de raison de s'attacher à ses unités ni d'optimiser leur placement. Le deck devient une soupe de statistiques.

**Quelques effets passifs simples à envisager :**
- *Automate boiteux* → "Quand cette unité meurt, inflige 1 dégât à l'unité ennemie en face" (Épine)
- *Arbalète géante* → "Attaque avant les autres unités" (Initiative)
- *Vitruve* → "Heal 1 au héros allié quand il survit à une attaque" (Résilience)
- *Chien de garde* → "Si la lane en face est vide, attaque le héros ennemi pour 2 au lieu de 1" (Vigilance)
- *Joconde* → "Pioche 1 carte quand posé sur le terrain" (Effet d'entrée)

Ces effets ne cassent pas l'équilibrage mais donnent au placement une dimension tactique réelle.

---

### 2. L'absence d'intentions ennemies frustre sans enrichir

La logique "incertitude = stratégie" est bonne en théorie, mais en pratique dans un deckbuilder au tour par tour, cacher les intentions ennemies retire une dimension fondamentale : **la réponse proactive**. Le joueur joue dans le vide — il optimise ses propres actions sans pouvoir anticiper celles de l'ennemi. Ça se traduit inévitablement par : "pourquoi j'ai perdu ? Je ne savais pas qu'il allait faire ça."

Slay the Spire montre que voir les intentions ennemies *n'enlève pas* la difficulté — ça la déplace vers la planification. C'est beaucoup plus satisfaisant que la mort par surprise.

**Option intermédiaire sans tout révéler :** Ne pas montrer les cartes exactes, mais afficher une icône par board indiquant si l'ennemi va probablement jouer une unité, un sort, ou "passer". Le joueur garde l'incertitude sur le *quoi* mais comprend l'*ampleur* de la menace.

---

### 3. Les lanes n'ont pas de profondeur stratégique

Actuellement, une unité attaque la lane en face ou tape direct si vide. C'est propre — mais le placement est une décision de *ressources* (j'ai 3 slots, je remplis), pas une décision *stratégique* (je place ici pour une raison précise).

Il n'y a pas de :
- Positionnement (certaines unités sont meilleures en flanc/centre)
- Priorité de ciblage
- Interaction entre lanes adjacentes

**Amélioration légère et à faible coût :** Désigner la lane centrale de chaque board comme "lane de percée" — si une unité ennemie occupe la lane centrale sans opposition, elle inflige +1 dégât direct. Ça crée immédiatement une asymétrie qui rend le placement central plus important que les flancs.

---

### 4. Le deck De Vinci est redondant et incohérent thématiquement

**Redondance :** 3× Bouclier parapluie (Heal 4, coût 1) + 2× Revigorant (Heal 8, coût 2) = 5 cartes de heal pur sur 20. Avec une main de 8, c'est fréquent d'avoir 2-3 cartes de heal sans contexte pour les jouer (HP full, rien à faire).

**Incohérence d'archétype :** Réveil explosif (5 dmg héros ennemi, coût 2) est une carte d'aggro dans un deck control/survie. Ce n'est pas nécessairement mauvais — une pression offensive même modeste est utile — mais son nom et son thème ne collent pas du tout avec l'identité de De Vinci l'inventeur alcoolique.

**Suggestion :** Remplacer Réveil explosif par un sort plus "inventeur raté" : un effet qui tente quelque chose d'utile mais avec un downside (ex: +4 ATK à une unité alliée mais cette unité perd 2 HP — *l'invention rate*). Ça renforcerait l'identité narrative tout en créant une décision plus intéressante.

---

### 5. Les personnages n'ont pas de mécanique signature

Les reliques de départ (pioche +1 pour De Vinci, mana +1 pour Curie) sont des petits bonus génériques qui ne changent pas *fondamentalement* comment on joue. Compare avec Slay the Spire où chaque personnage a une ressource unique (Force/Armor, Poison, Orbs d'énergie).

Ici, De Vinci et Curie jouent essentiellement de la même façon — juste avec des cartes différentes. Leurs archetypes viennent des cartes, pas d'une règle propre au personnage.

**Suggestions de mécaniques signature :**
- **De Vinci** → *Bricolage* : une fois par combat, peut réassembler deux cartes unités mortes en une nouvelle unité temporaire (stat = somme / 2). Thème "inventeur qui récupère les pièces".
- **Marie Curie** → *Contamination* : chaque sort joué laisse 1 jeton Poison sur une unité ennemie au choix (1 dmg/tour). Le debuff ATK existant devient une mécanique de setup pour le poison.
- Ces mécaniques n'ont pas besoin d'être complexes — juste 1 règle par personnage qui change l'approche du combat.

---

### 6. Le reward de cartes peut être frustrant (doublons inutiles)

Si un joueur a déjà 3 copies d'une carte commune (le max pour la Forge), voir une 4e copie en reward de combat est un gaspillage pur. Il devrait y avoir un filtre sur les cartes déjà à ×3 dans le pool de reward — ou au minimum, une mécanique de "défausse contre or" si toutes les options sont mauvaises.

---

### 7. La difficulté multi-boards est mal définie

1 board = facile, 3 boards = mini-boss : logique. Mais les ennemis sur 3 boards ont-ils les mêmes stats que sur 1 board ? Si oui, un joueur qui "abandonne" 2 boards et laisse les dégâts directs s'accumuler va trouver son propre équilibre abusif. Il faut que les 3 boards aient des HP ennemis réduits mais que la *complexité de gestion* soit la vraie difficulté — pas juste la somme des HP.

---

## Résumé priorisé

| Priorité | Changement | Effort | Impact |
|---|---|---|---|
| **1** | Ajouter effets passifs à 3-4 unités clés | Faible | Très fort |
| **2** | Filtrer doublons ×3 dans le reward pool | Faible | Moyen |
| **3** | Intentions ennemies partielles (icône menace) | Moyen | Fort |
| **4** | Refondre Réveil explosif → sort thématique De Vinci | Faible | Moyen |
| **5** | Mécanique signature par personnage | Moyen | Très fort |
| **6** | Lane centrale = "lane de percée" (+1 dmg direct) | Faible | Moyen |
| **7** | Clarifier balance HP ennemis multi-boards | Moyen | Fort |

La priorité absolue reste le point 1 : des unités sans effets propres, c'est le problème qui rend tout le reste moins intéressant. C'est le changement qui aurait le plus grand impact sur la sensation de jeu pour le moins d'effort d'implémentation.

---

## Décisions de design — Suite (2026-04-18)

---

### Point 1 — Effets passifs pour TOUTES les unités (validé)

#### De Vinci (Contrôle/Survie)

| Carte | Stats | Effet | Keyword |
|---|---|---|---|
| **Automate boiteux** ×3 | 1/2 | À la mort, inflige 1 dmg à l'unité en face | *Épine* |
| **Joconde** ×2 | 2/2 | À l'entrée sur le terrain, pioche 1 carte | *Inspiration* |
| **Chien de garde** ×2 | 1/3 | Si lane en face vide, dégâts directs ×2 | *Vigilance* |
| **Arbalète géante** ×2 | 3/1 | Si tue l'unité en face, les dégâts excédentaires vont au héros ennemi | *Percée* |
| **Vitruve** ×2 | 1/4 | Fin de tour : si survit à une attaque ce tour, soigne 1 héros allié | *Résilience* |
| **Char d'assaut** (Épique) | 2/5 | Les unités ennemies adjacentes doivent l'attaquer en priorité | *Gardien* |
| **Dragon** (Épique) | 3/3 | À l'entrée, inflige 2 dmg à toutes les unités ennemies sur ce board | *Souffle* |

> L'Arbalète "Percée" compense ses 1 HP fragiles avec un potentiel burst. Le Dragon perd l'attaque différée pour un AoE immédiat — outil de board clear unique dans le deck contrôle.

---

#### Marie Curie (Poison/Débuff)

| Carte | Stats | Effet | Keyword |
|---|---|---|---|
| **Assistante contaminée** ×3 | 1/2 | À la mort, applique −1 ATK à l'unité ennemie en face | *Contagion* |
| **Cobaye volontaire** ×2 | 1/3 | Reçoit −1 dmg de toutes les attaques ennemies (min 1) | *Résistance* |
| **Dealer en blouse blanche** ×2 | 2/2 | Si l'unité en face a ≤0 ATK, inflige +2 dmg directs bonus au héros ennemi | *Exploiter* |
| **Trafiquante de radium** ×2 | 3/2 | À l'entrée, applique −1 ATK à une unité ennemie aléatoire sur ce board | *Contrebande* |
| **Chimiste véreux** ×2 | 2/3 | Fin de tour : si 1+ unité ennemie à 0 ATK sur ce board, soigne 2 héros allié | *Synthèse* |
| **Baron de la Pechblende** (Épique) | 3/4 | Début de chaque tour : inflige 1 dmg à toutes les unités ennemies sur ce board | *Irradiation* |
| **Monstre de Polonium** (Épique) | 4/3 | À la mort, inflige 2 dmg à toutes les unités ennemies sur ce board | *Explosion radioactive* |

> Synergie intentionnelle : Assistante → affaiblit à la mort → Dealer exploite les 0 ATK → Chimiste soigne sur les 0 ATK. Le deck veut mettre les ennemis à 0 ATK avant tout.

---

#### Jules César (Agression/Boss)

| Carte | Stats | Effet | Keyword |
|---|---|---|---|
| **Légionnaire** ×3 | 2/2 | À l'entrée : si ≥1 unité alliée déjà sur ce board, +1 ATK permanent | *Légion* |
| **Centurion agressif** ×2 | 3/2 | Le tour où il est posé, attaque immédiatement (en plus de l'attaque normale) | *Charge* |
| **Gladiateur kamikaze** ×2 | 4/1 | À la mort, inflige 3 dmg au héros ennemi | *Sacrifice offensif* |
| **Garde prétorienne** ×2 | 2/4 | Toutes les unités alliées sur ce board reçoivent −1 dmg | *Escorte* |
| **Tribun militaire** ×2 | 3/3 | À l'entrée, +1 ATK à toutes les unités alliées déjà sur ce board | *Commandement* |
| **Proconsul de la mort** (Épique) | 4/4 | Quand tue une unité ennemie, se soigne de 2 HP | *Conquête* |
| **César lui-même** (Épique) | 5/3 | À l'entrée, inflige 3 dmg à toutes les unités ennemies présentes sur ce board | *Et tu Brute ?* |

> La Garde Prétorienne est l'unité pivot — protège toute la ligne, placement central prioritaire. César lui-même est un nuker d'entrée qui renverse un board, parfait pour le boss final.

---

### Point 2 — Intentions ennemies : montrer la lane de pose (à valider)

**Option A — Lane précise** : afficher un halo sur la lane exacte où l'ennemi va poser son unité.
- *Pro* : crée une réponse proactive forte ("il pose lane 2, je contre-pose lane 2")
- *Con* : risque de rendre le jeu trop lisible, supprime le stress de la surprise

**Option B — Board menacé** : indiquer quel board reçoit une unité, pas la lane exacte.
- Équilibre info utile / incertitude conservée

**Décision proposée** : Option A pour les unités uniquement, pas pour les sorts. Tu sais QUELLE lane sera bloquée, mais les sorts restent surprenants. Placement défensif tactique sans tout dévoiler.

---

### Point 3 — Lanes : percée centrale + classes de position (à valider)

**Lane de percée** : si une unité ennemie occupe la lane centrale sans opposition, elle inflige +1 dmg direct. Pression sur le centre.

**Classes de position** (keyword sur certaines cartes) :
- **"Flanqueur"** (ex: Chien de garde, Trafiquante de radium) : +1 ATK si posé en lane 1 ou 3
- **"Avant-garde"** (ex: Arbalète, Gladiateur) : actif uniquement si aucune unité alliée dans les lanes adjacentes
- **"Colonne vertébrale"** (ex: Vitruve, Garde prétorienne) : bonus actif uniquement si posé lane 2 (centre)

Ces classes sont des keywords sur certaines cartes — le joueur lit la carte, pas de sélection de classe au niveau du personnage.

---

### Point 4 — Sorts De Vinci : révision narrative (thème "invention qui rate")

| Carte | Effet actuel | Nouvel effet proposé |
|---|---|---|
| **Bouclier parapluie troué** ×3 | Heal 4 | Heal 3 + absorbe la prochaine attaque directe sur le héros ce tour (le "trou" laisse passer les attaques d'unités) |
| **Ailes volantes** ×2 | +3 ATK unité alliée | +4 ATK à une unité alliée, mais cette unité perd 2 HP ("l'invention explose au décollage") |
| **Revigorant** ×2 | Heal 8 | Heal 5 + pioche 1 carte ("le revigorant alcoolisé — soigne et embrouille") |
| **Réveil explosif** ×2 | 5 dmg héros ennemi | 3 dmg héros ennemi + 2 dmg à une unité alliée aléatoire ("l'invention se retourne contre l'inventeur") |

> Cohérence thématique : chaque sort De Vinci "rate" légèrement. Renforce l'identité d'inventeur alcoolique.

---

### Point 5 — Mécaniques signature (validé)

**De Vinci — "Bricolage"**
Une fois par combat : si ≥2 unités alliées sont mortes sur un board, peut placer une "unité de fortune" avec ATK = somme des ATK / 2 (arrondi haut) et HP = 2. Disparaît en fin de combat.
Interface : bouton "Bricoler" actif sur le board concerné uniquement si la condition est remplie.

**Marie Curie — "Contamination"**
Passif permanent : chaque fois que Curie joue un sort, place automatiquement 1 jeton Poison sur une unité ennemie au choix (si aucune unité → sur le héros ennemi). Le Poison inflige 1 dmg au début de chaque tour ennemi.
Interface : icône verte sur l'unité empoisonnée avec compteur de tours.

---

### Point 6 — Reward de cartes (à décider)

Trois approches :

| Option | Mécanique | Pro | Con |
|---|---|---|---|
| **A — Filtre strict** | Si ≥3 copies dans le deck, la carte n'apparaît plus en reward | Simple, propre | Peut réduire les options dans les chapitres avancés |
| **B — Option "Transmuter"** | Si une carte est déjà ×3, affiche "Vendre X or" à la place | Toujours utile | Complexifie l'UI de reward |
| **C — Garantie nouveauté** | Toujours au moins 1 carte non-×3 parmi les 3 proposées | Jamais frustrant | Peut exclure des cartes si collection avancée |

**Proposition** : C + B en fallback. 1 nouvelle carte garantie dans les 3 options. Les 2 autres peuvent être des doublons, mais affichent "Vendre X or" si elles sont à ×3.

---

### Point 7 — Difficulté multi-boards (à décider)

**Option A — HP ennemis décroissants**
- 1 board : 55 HP
- 2 boards : 35 HP chacun
- 3 boards : 25 HP chacun
La complexité de gestion est la vraie difficulté, pas le HP total.

**Option B — Timer de rage**
Un board laissé sans unité alliée pendant 2 tours consécutifs déclenche un sort ennemi bonus automatique (ex : 5 dmg directs au joueur). Punish l'abandon passif sans bloquer les sacrifices calculés.

**Option C — Condition de victoire par board**
Chaque board doit être clôturé (HP ennemi à 0) pour avancer. Impossible d'ignorer un front.

**Proposition** : A + B combinés. HP réduits sur multi-boards + pénalité sur board abandonné >2 tours.

---

## Vision complète du jeu — Réévaluation (2026-04-18)

### Contenu confirmé (cible de développement)

- **4-5 personnages jouables** avec mécaniques signature distinctes
- **5 chapitres** (4 régions : Le Vice, Le Contrôle, La Guerre, Le Spectacle + Bureau du Dieu)
- **5 boss de région** (un par chapitre/région)
- **5 mini-boss de région** (un par chapitre/région)
- **Familles d'ennemis distinctes par région** — pas juste des stats plus hautes, des patterns et identités propres à chaque région
- **Dialogues et histoire contextuels par personnage** — le récit et les réactions changent selon le perso joué, dans le ton absurde anachronique (ex: César en agent immobilier qui commente ta défaite avec du jargon immobilier)

### Score révisé avec cette vision complète : **74 / 100**

Ce qui change avec le contenu complet :
- Les 4-5 persos donnent des runs fondamentalement différents et une courbe de découverte sur 30-50h
- Les 5 chapitres donnent du sens aux décisions de build ("est-ce que ce build tient jusqu'au chapitre 4 ?")
- Les dialogues contextuels par perso sont un multiplicateur de charme — c'est ce que fait Hades et c'est une des raisons pour lesquelles les joueurs font leur 20e run
- Les familles d'ennemis par région créent de vraies adaptations de deck nécessaires

Ce qui plafonne encore à 74 :
- Les synergies de cartes restent à construire (voir section suivante)
- Si les cartes fonctionnent en isolation, les runs se ressemblent même avec 5 persos

Potentiel réel si bien exécuté : **82-85/100** — niveau Wildfrost / Monster Train.

---

### Priorité critique : les synergies de cartes

**C'est le chantier principal pour dépasser les 74/100.**

Un grand deckbuilder se joue en *builds*, pas en *cartes isolées*. Le joueur doit pouvoir identifier et construire autour d'un moteur spécifique :
- "Le build Poison Curie" : debuff ATK → Dealer exploite → Contamination empile → Baron irradie
- "Le build Bricolage De Vinci" : unités sacrifiables → Bricolage → unité de fortune → relique qui booste les unités temporaires
- "Le build Légion César" : Légionnaires qui se renforcent mutuellement → Tribun qui buff toute la ligne → Garde qui protège

Chaque personnage devrait avoir **2-3 builds identifiables** que le joueur peut viser et optimiser au fil du run. Les cartes ajoutées en récompense, les reliques trouvées, et les décisions de Forge doivent toutes pouvoir s'inscrire dans un de ces axes.

Les keyword interactions sont aussi à prévoir : ex. Poison (Curie) × Débuff ATK = les unités empoisonnées prennent +1 dmg des sorts. Ce genre d'interaction entre mécaniques est ce qui crée les moments "aha" qui fidélisent les joueurs.

---

## Roadmap vers 90-95/100 (validée 2026-04-18)

Ce qui sépare les 85 des 90+ : **chaque run raconte une histoire unique que le joueur veut raconter à quelqu'un d'autre.** Les systèmes ci-dessous sont tous validés et seront implémentés progressivement.

---

### 1. Le Dieu comme personnage actif *(priorité narrative absolue)*

Le levier le plus sous-exploité du concept. Le dieu organise le tournoi mais est absent du gameplay — c'est exactement ce que Hades fait avec Hadès père pour créer 50+ runs de rejouabilité narrative.

**Ce que ça implique :**
- **Commentaires contextuels par run** — le dieu réagit à tes victoires, défaites, et choix inhabituels ("tu as encore pris ce sort nul, je commence à m'inquiéter")
- **Sa fatigue évolue** avec ta progression globale — plus tu avances, plus il est impressionné ou désespéré
- **Interventions occasionnelles** — t'offrir une carte corrompue, saboter un ennemi qu'il trouve "trop fort pour le spectacle", te pénaliser si tu joues trop prudemment ("c'est un tournoi, pas un séjour spa")
- **Son arc narratif** — au fil des runs, le joueur découvre pourquoi il est fatigué et ce qu'il cherche vraiment dans ce successeur
- Les dialogues varient selon le personnage joué (César vs De Vinci vs Curie = ton différent)

---

### 2. Système de Malédictions / Pactes du Dieu

À certains nœuds de la map, le dieu propose un marché louche. Risque/récompense narrativement cohérent avec le ton du jeu.

**Exemples de pactes :**
- "Je t'offre cette relique épique. En échange, une de tes unités commence chaque combat avec −1 HP"
- "Je double ton mana ce combat. En échange, ta main est réduite à 5 cartes pour le reste du chapitre"
- "Je ressuscite ton héros avec 20 HP si tu meurs. En échange, tu commences le prochain combat avec 3 malédictions dans le deck"

Les malédictions sont des cartes inutiles ou négatives ajoutées au deck (ex: *Décret divin* — Sort, coût 0, ne fait rien, occupe une place en main).

Ce système crée des stories mémorables ("j'ai accepté 4 pactes et j'ai quand même gagné") et de la discussion communautaire.

---

### 3. Événements narratifs qui lisent l'état du run

Les événements réagissent à ce que le joueur fait — ils ne sont pas fixes.

**Exemples d'événements contextuels :**
- Si le deck contient 3+ Légionnaires → César t'interpelle ("Ah, tu joues ma légion. Plagiat.")
- Si le héros est à moins de 20 HP → le dieu commente ("Tu ressembles à moi un lundi matin")
- Si le deck ne contient aucun sort → un marchand propose uniquement des sorts avec −50%
- Si le joueur a perdu 3 fois de suite → événement "compassion" du dieu avec un avantage offert
- Si une relique spécifique est équipée → des événements en lien avec cette relique peuvent apparaître

Ces événements transforment chaque run en une histoire qui semble faite pour le joueur.

---

### 4. Système d'Ascension — Les Caprices du Dieu

15-20 modificateurs débloquables progressivement pour les joueurs expérimentés. Chaque niveau rend le jeu plus difficile ou change ses règles fondamentales.

**Exemples de niveaux :**
- Niveau 1 : les ennemis commencent chaque combat avec 1 unité déjà posée
- Niveau 3 : les récompenses de combat proposent 2 cartes au lieu de 3
- Niveau 5 : le mana ne se régénère pas entre les boards d'un même combat
- Niveau 8 : les reliques de pacte ont des effets négatifs renforcés
- Niveau 10 : le dieu retire une carte aléatoire du deck entre chaque combat
- Niveau 15 : les intentions ennemies sont entièrement cachées
- Niveau 20 : les HP ennemis augmentent de 25% sur tous les boards

---

### 5. Le "Feel" — polish animations et sons

Ce qui sépare Balatro (93/100) de jeux mécaniquement équivalents à 70/100. 40% des reviews Steam positives de Balatro mentionnent le mot "satisfying". Ce n'est pas cosmétique — c'est core.

**Ce que ça implique :**
- Son satisfaisant pour chaque action (thunk d'une unité posée, craquement d'une mort, swoosh d'un sort)
- Animation de poids sur les grosses attaques (un boss qui encaisse doit *sentir* la douleur)
- Screen shake dosé sur les moments importants (mort de boss, dégâts critiques)
- Feedback visuel immédiat sur chaque interaction (nombres de dégâts qui volent, flash sur les unités touchées)
- Transition satisfaisante entre boards lors de la résolution multi-board

---

### 6. Interactions secrètes entre cartes

5-6 synergies non documentées que les joueurs découvrent et partagent. Crée du contenu communautaire organique et récompense la maîtrise.

**Exemples :**
- Joconde (pioche 1 à l'entrée) + Dragon (AoE à l'entrée) joués le même tour → la carte piochée par la Joconde est visible dans la résolution du Dragon
- Gladiateur kamikaze (4/1) + Ailes volantes (+4 ATK −2 HP) → le Gladiateur meurt immédiatement mais inflige 3 dmg (Sacrifice offensif) + ses 8 ATK en une seule action
- Cobaye volontaire (Résistance −1 dmg) + Baron Pechblende (Irradiation 1 dmg/tour) → le Cobaye est immunisé à l'Irradiation de son propre camp

---

### Tableau de priorité d'implémentation

| Système | Effort | Impact score |
|---|---|---|
| Le Dieu personnage actif | Élevé | +6-8 pts |
| Malédictions / Pactes du Dieu | Moyen | +4-5 pts |
| Événements contextuels | Moyen | +3-4 pts |
| Système d'Ascension | Faible (une fois la base posée) | +3-4 pts |
| Feel / animations / sons | Élevé | +4-6 pts |
| Interactions secrètes entre cartes | Faible | +2-3 pts |

Avec tout ça : **90-95/100** — un jeu dont les joueurs parlent.
