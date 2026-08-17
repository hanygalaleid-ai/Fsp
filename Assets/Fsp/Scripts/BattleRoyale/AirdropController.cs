using System.Collections;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class AirdropController : MonoBehaviour
    {
        [SerializeField] private GameObject cratePrefab;
        [SerializeField] private Transform[] dropPoints;
        [SerializeField] private float firstDropDelay = 180f;
        [SerializeField] private float repeatDelay = 240f;
        [SerializeField] private float spawnHeight = 120f;

        private Coroutine loop;

        private void OnEnable()
        {
            loop = StartCoroutine(DropLoop());
        }

        private void OnDisable()
        {
            if (loop != null) StopCoroutine(loop);
        }

        private IEnumerator DropLoop()
        {
            yield return new WaitForSeconds(firstDropDelay);
            while (enabled)
            {
                SpawnAirdrop();
                yield return new WaitForSeconds(repeatDelay);
            }
        }

        public void SpawnAirdrop()
        {
            if (cratePrefab == null || dropPoints == null || dropPoints.Length == 0) return;
            Transform point = dropPoints[Random.Range(0, dropPoints.Length)];
            if (point == null) return;

            Vector3 pos = point.position + Vector3.up * spawnHeight;
            Instantiate(cratePrefab, pos, point.rotation);
        }
    }
}
