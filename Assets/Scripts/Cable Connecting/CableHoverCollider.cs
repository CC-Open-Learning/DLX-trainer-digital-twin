using UnityEngine;

namespace VARLab.MPCircuits
{
    /// <summary>
    /// This class is used to rotate the lead start and end game objects so that they move out of the way of the lightbulbs
    /// 
    /// THIS FUNCTIONALITY WAS REMOVED IN MPC-694, IF YOU WOULD LIKE TO ADD THIS BACK THEN ATTACH THIS SCRIPT
    /// TO THE "CableHoverCollider" GAME OBJECT FOUND ON THE PREFAB "Cable Base"
    /// </summary>
    public class CableHoverCollider : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            CableConnector c = (GetComponentInParent(typeof(CableConnector)) as CableConnector);

            // Used to rotate the lead when collision has happened
            Vector3 target = c.cableStart.transform.position - c.cableEnd.transform.position;
            target.y = 0f;

            if (target == Vector3.zero) return;

            var lookTowardsStart = Quaternion.LookRotation(-target) * Quaternion.Euler(0f, 90f, 0f);
            var lookTowardsEnd = Quaternion.LookRotation(target) * Quaternion.Euler(0f, -90f, 0f);

            c.cableStart.transform.rotation = lookTowardsEnd;
            c.cableEnd.transform.rotation = lookTowardsStart;

            // Create mesh after moving the lead so that the collider is not misplaced 
            c.CreateMesh();
        }
    }
}
