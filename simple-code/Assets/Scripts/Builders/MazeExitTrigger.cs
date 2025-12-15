using System;
using TestGame.Core;
using UnityEngine;

namespace TestGame.Builders
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class MazeExitTrigger : MonoBehaviour
    {
        public event Action PlayerExited;

        private void Reset()
        {
            var c = GetComponent<BoxCollider>();
            c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            PlayerExited?.Invoke();
            GameManager.Instance?.LoadYouWin();
        }
    }
}
