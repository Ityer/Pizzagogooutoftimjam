using UnityEngine;
using UnityEngine.UI;

public class PizzaDeliveryPoint : MonoBehaviour
{
    [Header("Pizza")]
    [SerializeField] private GameObject pizzaPrefab;
    [SerializeField] private Transform pizzaPlacement;

    [Header("Delivery")]
    [SerializeField] private float deliveryTime = 2f;

    [Header("Loading Indicator")]
    [SerializeField] private Image progressImage;

    private PizzaCargo pizzaCargo;
    private DeliveryManager deliveryManager;

    private float timer = 0f;
    private bool truckInside = false;
    private bool delivered = false;

    private void Start()
    {
        deliveryManager = FindAnyObjectByType<DeliveryManager>();

        if (progressImage != null)
        {
            progressImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (!truckInside || delivered)
            return;

        if (pizzaCargo == null)
            return;

        // Don't start/continue delivery without a pizza
        if (pizzaCargo.GetPizzaCount() <= 0)
        {
            timer = 0f;

            if (progressImage != null)
                progressImage.fillAmount = 0f;

            return;
        }

        timer += Time.deltaTime;

        if (progressImage != null)
        {
            progressImage.fillAmount =
                timer / deliveryTime;
        }

        if (timer >= deliveryTime)
        {
            CompleteDelivery();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        pizzaCargo = other.GetComponent<PizzaCargo>();

        if (pizzaCargo == null)
            return;

        truckInside = true;
        timer = 0f;

        if (progressImage != null)
            progressImage.fillAmount = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        truckInside = false;
        timer = 0f;

        if (progressImage != null)
            progressImage.fillAmount = 0f;

        pizzaCargo = null;
    }

    private void CompleteDelivery()
    {
        if (!pizzaCargo.RemovePizza())
        {
            timer = 0f;

            if (progressImage != null)
                progressImage.fillAmount = 0f;

            return;
        }

        // Reset the progress icon
        timer = 0f;

        if (progressImage != null)
            progressImage.fillAmount = 0f;

        // Spawn the delivered pizza
        Quaternion rotation =
            pizzaPlacement.rotation *
            Quaternion.Euler(90f, 0f, 0f);

        Instantiate(
            pizzaPrefab,
            pizzaPlacement.position,
            rotation
        );

        // Update delivery counter
        if (deliveryManager != null)
        {
            deliveryManager.CompleteDelivery();
        }

        delivered = true;

        // Remove the delivery point
        transform.parent.gameObject.SetActive(false);
    }
    public void SetVisible(bool visible)
    {
        transform.parent.gameObject.SetActive(visible);
    }
}