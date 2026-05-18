using RoguelikeTCG.Data;

namespace RoguelikeTCG.Cards
{
    /// <summary>
    /// Instance d'une carte en jeu. Grille 2x5 — Row 0 = ennemi, Row 1 = joueur.
    /// </summary>
    public class CardInstance
    {
        public CardData data;
        public bool isPlayerCard;

        // Position sur la grille 2x5 (-1 = pas en jeu)
        public int row = -1;
        public int col = -1;

        // HP actuels (initialisés depuis data.hp)
        public int currentHP;

        // ATK courante = data.atk + bonus accumulés
        public int currentATK;

        // Freeze : l'unité passe son attaque ce tour (Bond Blizzard)
        public bool isFrozen;

        // Indique si l'unité a déjà subi des dégâts ce tour (Bond Surcharge)
        public bool tookDamageThisTurn;

        // Bonus ATK permanent (ex. bond Cendres)
        private int _atkBonus;

        public int manaCost => data.manaCost;

        public CardInstance(CardData data, bool isPlayerCard)
        {
            this.data         = data;
            this.isPlayerCard = isPlayerCard;
            this.currentHP    = data.hp;
            this.currentATK   = data.atk;
        }

        /// <summary>
        /// Applique des dégâts. Retourne true si l'unité meurt (HP <= 0).
        /// </summary>
        public bool TakeDamage(int amount)
        {
            if (amount <= 0) return false;
            currentHP -= amount;
            tookDamageThisTurn = true;
            return currentHP <= 0;
        }

        /// <summary>
        /// Ajoute un bonus d'ATK permanent à l'unité.
        /// </summary>
        public void AddATKBonus(int amount)
        {
            _atkBonus  += amount;
            currentATK  = data.atk + _atkBonus;
        }

        public bool IsUnit   => data.cardType == CardType.Unit;
        public bool IsAlive  => currentHP > 0;
        public bool IsOnGrid => row >= 0 && col >= 0;
    }
}
