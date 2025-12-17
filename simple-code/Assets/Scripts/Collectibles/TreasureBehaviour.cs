using TestGame.Core;
using UnityEngine;

namespace TestGame.Collectibles
{
    [RequireComponent(typeof(Collider))]
    public sealed class TreasureBehaviour : MonoBehaviour
    {
        [Header("Value")]
        [SerializeField] private int value = 25;

        [Header("Animation")]
        [SerializeField] private float rotateSpeed = 70f;
        [SerializeField] private float bobAmplitude = 0.2f;
        [SerializeField] private float bobFrequency = 1.6f;

        [Header("Audio")]
        [SerializeField] private AudioClip pickupSfx;

        private Vector3 _startPos;
    private bool _collected;

        private void Awake()
        {
            _startPos = transform.position;
            var c = GetComponent<Collider>();
            c.isTrigger = true;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            transform.position = _startPos + Vector3.up * (Mathf.Sin(Time.time * bobFrequency) * bobAmplitude);
        }

        private void OnTriggerEnter(Collider other)
        {
            CollectIfPlayer(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            CollectIfPlayer(collision.gameObject);
        }

        private void CollectIfPlayer(GameObject other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            _collected = true;
            GameManager.Instance?.AddCollectible(value, isTreasure: true);
            PlaySfx();
            Destroy(gameObject);
        }

        private void PlaySfx()
        {
            if (pickupSfx == null) return;
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
        }
    }
}
