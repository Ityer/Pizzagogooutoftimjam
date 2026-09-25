using UnityEngine;

public class PizzaDeliveryPoint : MonoBehaviour
{
    [Header("Pizza")]
    [SerializeField] private GameObject pizzaPrefab;
    [SerializeField] private Transform pizzaPlacement;

    private DeliveryManager deliveryManager;
    private bool delivered = false;

    private void Start()
    {
        deliveryManager = FindAnyObjectByType<DeliveryManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (delivered)
            return;

        if (!other.CompareTag("Player"))
            return;

        PizzaCargo pizzaCargo = other.GetComponent<PizzaCargo>();

        if (pizzaCargo == null)
            return;

        // Try to take one pizza from the truck
        if (!pizzaCargo.RemovePizza())
            return;

        Quaternion rotation =
        pizzaPlacement.rotation *
        Quaternion.Euler(90f, 0f, 0f);

        Instantiate(
            pizzaPrefab,
            pizzaPlacement.position,
            rotation
        );
        deliveryManager.CompleteDelivery();

        delivered = true;

        // Remove the delivery point
        Destroy(gameObject);
    }
}