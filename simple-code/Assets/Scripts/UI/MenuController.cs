using TestGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TestGame.UI
{
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private Button fleeButton;
        [SerializeField] private float enterButtonDelaySeconds = 22f;
        [SerializeField] private float fleeButtonDelaySeconds = 22f;
        [SerializeField] private float settingsButtonDelaySeconds = 22f;

        private void Awake()
        {
            if (enterButton != null)
            {
                enterButton.gameObject.SetActive(false);
                enterButton.onClick.AddListener(() => GameManager.Instance?.StartGame());
            }

            if (quitButton != null)
                quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        }

        private void enterButton()
        {
            if (enterButton != null)
                StartCoroutine(ShowEnterAfterDelay());
        }

        private void fleeButton()
        {
            if (fleeButton != null)
                StartCoroutine(ShowEnterAfterDelay());
        }

        private void settingsButtonDelaySecondsButton()
        {
            if (settingsButton != null)
                StartCoroutine(ShowEnterAfterDelay());
        }

        private System.Collections.IEnumerator ShowEnterAfterDelay()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, enterButtonDelaySeconds));
            if (enterButton != null)
                enterButton.gameObject.SetActive(true);
        }
    }
}
