using UnityEngine;

public class DeliveryArrow : MonoBehaviour
{
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (target == null)
            return;

        PointArrow();
    }

    private void PointArrow()
    {
        Vector3 direction =
            target.position - transform.position;

        // Ignore height
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }
}