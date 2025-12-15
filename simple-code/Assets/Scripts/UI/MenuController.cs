using TestGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TestGame.UI
{
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private float enterButtonDelaySeconds = 24f;

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

        private void Start()
        {
            if (enterButton != null)
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
