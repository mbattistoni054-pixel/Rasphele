using UnityEngine;
using UnityEngine.AI;

namespace PatronesAplicados
{
    public class NavMeshSpawnLocator
    {
        private const int MaxRetries = 10;
        private const float SampleMaxDistance = 10f;
        private const float FallbackSampleDistance = 25f;

        public bool TryGetSpawnPosition(Vector3 centerPosition, float minRadius, float maxRadius, out Vector3 validNavMeshPosition)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                Vector2 randomDir2D = Random.insideUnitCircle.normalized;
                if (randomDir2D.sqrMagnitude < 0.0001f)
                {
                    randomDir2D = Vector2.up;
                }

                Vector3 randomDirection = new Vector3(randomDir2D.x, 0f, randomDir2D.y);
                float randomDistance = Random.Range(minRadius, maxRadius);
                Vector3 candidatePoint = centerPosition + (randomDirection * randomDistance);

                if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, SampleMaxDistance, NavMesh.AllAreas))
                {
                    validNavMeshPosition = hit.position;
                    return true;
                }
            }

            Vector3 fallbackCandidate = centerPosition + (Vector3.forward * minRadius);
            if (NavMesh.SamplePosition(fallbackCandidate, out NavMeshHit fallbackHit, FallbackSampleDistance, NavMesh.AllAreas))
            {
                validNavMeshPosition = fallbackHit.position;
                return true;
            }

            if (NavMesh.SamplePosition(centerPosition, out NavMeshHit centerHit, FallbackSampleDistance, NavMesh.AllAreas))
            {
                validNavMeshPosition = centerHit.position;
                return true;
            }

            validNavMeshPosition = centerPosition;
            return false;
        }

        public Vector3 GetRandomPositionOnNavMesh(Vector3 centerPosition, float minRadius, float maxRadius)
        {
            TryGetSpawnPosition(centerPosition, minRadius, maxRadius, out Vector3 result);
            return result;
        }
    }
}