using System;
using UnityEngine;

namespace TestGame.Builders
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class MazeEntranceTrigger : MonoBehaviour
    {
        public event Action PlayerEntered;

        private void Reset()
        {
            var c = GetComponent<BoxCollider>();
            c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            PlayerEntered?.Invoke();
        }
    }
}
