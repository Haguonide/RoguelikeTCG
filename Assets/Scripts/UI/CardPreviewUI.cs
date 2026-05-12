using UnityEngine;
using TMPro;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Gère les panneaux ApercuCarteAlliée / ApercuCarteEnnemie.
    /// Appelé par CardView (main) et GridCellUI (plateau) au survol.
    /// </summary>
    public class CardPreviewUI : MonoBehaviour
    {
        public static CardPreviewUI Instance { get; private set; }

        [Header("Panneaux d'aperçu")]
        public GameObject allyPanel;
        public GameObject enemyPanel;

        [Header("Textes — Allié")]
        public TMP_Text cardNameAlly;
        public TMP_Text cardDescriptionAlly;

        [Header("Textes — Ennemi")]
        public TMP_Text cardNameEnemy;
        public TMP_Text cardDescriptionEnemy;

        private CardView _allyCardView;
        private CardView _enemyCardView;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;

            _allyCardView  = allyPanel?.GetComponentInChildren<CardView>(true);
            _enemyCardView = enemyPanel?.GetComponentInChildren<CardView>(true);

            allyPanel?.SetActive(false);
            enemyPanel?.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ShowForCard(CardInstance card)
        {
            if (card == null) { Hide(); return; }

            if (card.isPlayerCard)
            {
                enemyPanel?.SetActive(false);
                if (_allyCardView != null)
                {
                    _allyCardView.Setup(card);
                    _allyCardView.SetZoomMode(true);
                }
                if (cardNameAlly)        cardNameAlly.text        = card.data.cardName;
                if (cardDescriptionAlly) cardDescriptionAlly.text = card.data.description;
                allyPanel?.SetActive(true);
            }
            else
            {
                allyPanel?.SetActive(false);
                if (_enemyCardView != null)
                {
                    _enemyCardView.Setup(card);
                    _enemyCardView.SetZoomMode(true);
                }
                if (cardNameEnemy)        cardNameEnemy.text        = card.data.cardName;
                if (cardDescriptionEnemy) cardDescriptionEnemy.text = card.data.description;
                enemyPanel?.SetActive(true);
            }
        }

        public void Hide()
        {
            allyPanel?.SetActive(false);
            enemyPanel?.SetActive(false);
        }
    }
}
