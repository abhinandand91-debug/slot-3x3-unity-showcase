using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>Throws a burst of spinning coins up into the air that fall back under gravity.</summary>
    public class CoinFountain : MonoBehaviour
    {
        [SerializeField] private GameObject[] coinPrefabs;
        [SerializeField] private int coinsCount = 10;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float coinScale = 0.5f;
        [SerializeField] private float gravity = 9.8f;
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private float maxDelay = 1f;
        [SerializeField] private Vector3 velocityMin = new Vector3(-4f, 10f, 0f);
        [SerializeField] private Vector3 velocityMax = new Vector3(4f, 10f, 0f);

        public void Setup(GameObject[] coins)
        {
            coinPrefabs = coins;
        }

        /// <summary>Launches one burst of coins.</summary>
        public void Jump()
        {
            if (coinPrefabs == null || coinPrefabs.Length == 0) return;

            for (int i = 0; i < coinsCount; i++)
            {
                GameObject coin = Instantiate(coinPrefabs[i % coinPrefabs.Length], transform);
                coin.transform.localPosition = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0f);
                coin.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-30f, 30f));
                coin.transform.localScale *= coinScale;
                StartCoroutine(Fly(coin.transform, Random.Range(0f, maxDelay)));
            }
        }

        private IEnumerator Fly(Transform coin, float delay)
        {
            coin.gameObject.SetActive(false);
            yield return new WaitForSeconds(delay);
            if (coin == null) yield break;
            coin.gameObject.SetActive(true);

            Vector3 start = coin.localPosition;
            Vector3 velocity = new Vector3(Random.Range(velocityMin.x, velocityMax.x),
                                           Random.Range(velocityMin.y, velocityMax.y), 0f);
            Vector3 acceleration = new Vector3(0f, -gravity, 0f);

            float time = 0f;
            while (time < lifeTime && coin != null)
            {
                time += Time.deltaTime;
                coin.localPosition = start + velocity * time + acceleration * (time * time * 0.5f);
                yield return null;
            }
            if (coin != null) Destroy(coin.gameObject);
        }
    }
}
