using UnityEngine;
using UnityEngine.AI;

namespace PatronesAplicados
{
    public class NavMeshSpawnLocator
    {
        private const float SampleMaxDistance = 10f;

        public bool TryGetSpawnPosition(
            Vector3 centerPosition,
            float minRadius,
            float maxRadius,
            out Vector3 validNavMeshPosition)
        {
            Vector2 randomDir2D = Random.insideUnitCircle.normalized;
            if (randomDir2D == Vector2.zero)
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

            validNavMeshPosition = Vector3.zero;
            return false;
        }
    }
}