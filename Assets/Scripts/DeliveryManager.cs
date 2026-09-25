using UnityEngine;
using TMPro;

public class DeliveryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text deliveryText;

    private int totalDeliveries;
    private int completedDeliveries;

    private void Start()
    {
        PizzaDeliveryPoint[] deliveryPoints =
           FindObjectsByType<PizzaDeliveryPoint>(
               FindObjectsInactive.Include
           );

        totalDeliveries = deliveryPoints.Length;

        UpdateUI();
    }

    public void CompleteDelivery()
    {
        completedDeliveries++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        deliveryText.text =
            completedDeliveries + "/" + totalDeliveries;
    }
}