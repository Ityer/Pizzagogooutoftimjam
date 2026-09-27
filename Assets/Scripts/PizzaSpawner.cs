using UnityEngine;
using UnityEngine.UI;

public class PizzaSpawner : MonoBehaviour
{
    [Header("Pizza Cargo")]
    [SerializeField] private PizzaCargo pizzaCargo;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 2f;

    [Header("Loading Indicator")]
    [SerializeField] private Image progressImage;

    private float timer = 0f;
    private bool truckInside = false;

    private void Start()
    {
        if (progressImage != null)
        {
            progressImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (!truckInside)
            return;

        // Stop loading if the truck is already full
        if (pizzaCargo.IsFull())
        {
            timer = 0f;

            if (progressImage != null)
            {
                progressImage.fillAmount = 0f;
            }

            return;
        }

        timer += Time.deltaTime;

        // Update radial progress
        if (progressImage != null)
        {
            progressImage.fillAmount =
                timer / spawnInterval;
        }

        // Pizza ready
        if (timer >= spawnInterval)
        {
            pizzaCargo.AddPizza();

            DeliveryManager deliveryManager =
                FindAnyObjectByType<DeliveryManager>();

            if (deliveryManager != null)
            {
                deliveryManager.ShowNextDeliveryPoint();
            }

            timer = 0f;

            if (progressImage != null)
            {
                progressImage.fillAmount = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        truckInside = true;
        timer = 0f;

        if (progressImage != null)
        {
            progressImage.fillAmount = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        truckInside = false;
        timer = 0f;

        if (progressImage != null)
        {
            progressImage.fillAmount = 0f;
        }
    }
}