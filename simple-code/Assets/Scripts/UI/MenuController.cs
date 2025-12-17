using TestGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TestGame.UI
{
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private Button fleeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private float uiDelaySeconds = 22f;

        private void Awake()
        {
            if (enterButton != null)
            {
                enterButton.gameObject.SetActive(false);
                enterButton.onClick.AddListener(() => GameManager.Instance?.StartGame());
            }

            if (fleeButton != null)
            {
                fleeButton.gameObject.SetActive(false);
                fleeButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
            }
            if (settingsButton != null)
            {
                settingsButton.gameObject.SetActive(false);
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
        }

        private void Start()
        {
            StartCoroutine(ShowAfterDelay(enterButton));
            StartCoroutine(ShowAfterDelay(fleeButton));
            StartCoroutine(ShowAfterDelay(settingsButton));
        }

        private System.Collections.IEnumerator ShowAfterDelay(Button button)
        {
            if (button == null)
                yield break;

            yield return new WaitForSeconds(Mathf.Max(0f, uiDelaySeconds));
            button.gameObject.SetActive(true);
        }

        private void OnSettingsClicked()
        {
            Debug.LogWarning("Settings is not implemented yet.");
        }
    }
}
