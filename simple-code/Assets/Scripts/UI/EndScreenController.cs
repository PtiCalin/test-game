using TestGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TestGame.UI
{
    public sealed class EndScreenController : MonoBehaviour
    {
        [SerializeField] private Button menuButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (menuButton != null)
                menuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());

            if (quitButton != null)
                quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        }
    }
}
