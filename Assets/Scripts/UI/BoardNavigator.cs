using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class BoardNavigator : MonoBehaviour
    {
        public Button[] boardButtons;
        public GameObject[] boardViews;    // large view per board (only active one shown)
        public GameObject[] boardThumbnails; // mini-vignettes for inactive boards (optional)

        private BoardManager boardManager;

        private void Start()
        {
            var bm = FindObjectOfType<BoardManager>();
            if (bm != null) Initialize(bm);
        }

        public void Initialize(BoardManager manager)
        {
            boardManager = manager;
            boardManager.OnActiveBoardChanged += OnBoardChanged;

            for (int i = 0; i < boardButtons.Length; i++)
            {
                int index = i;
                if (boardButtons[i] != null)
                    boardButtons[i].onClick.AddListener(() =>
                    {
                        if (CombatAnimator.Instance != null && CombatAnimator.Instance.IsAnimating) return;
                        boardManager.SetActiveBoard(index);
                    });
            }

            OnBoardChanged(boardManager.ActiveBoardIndex);
        }

        private void OnBoardChanged(int activeIndex)
        {
            for (int i = 0; i < boardViews.Length; i++)
            {
                if (boardViews[i] != null)
                    boardViews[i].SetActive(i == activeIndex);
            }
        }

        private void OnDestroy()
        {
            if (boardManager != null)
                boardManager.OnActiveBoardChanged -= OnBoardChanged;
        }
    }
}
