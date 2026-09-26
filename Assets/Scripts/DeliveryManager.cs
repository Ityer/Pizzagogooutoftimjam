using UnityEngine;
using TMPro;

public class DeliveryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text deliveryText;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Timer")]
    [SerializeField] private float gameTime = 180f; // 3 minutes

    private PizzaDeliveryPoint[] deliveryPoints;

    private int totalDeliveries;
    private int completedDeliveries;
    private int pointsShown;

    private float timer;
    private bool gameFinished = false;

    private void Start()
    {
        deliveryPoints = FindObjectsByType<PizzaDeliveryPoint>(
            FindObjectsInactive.Include
        );

        totalDeliveries = deliveryPoints.Length;

        // Hide all delivery points at the start
        foreach (PizzaDeliveryPoint point in deliveryPoints)
        {
            point.SetVisible(false);
        }

        timer = gameTime;

        UpdateUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        if (gameFinished)
            return;

        // Count down
        timer -= Time.deltaTime;

        if (timer < 0f)
            timer = 0f;

        UpdateTimerUI();

        // Time ran out
        if (timer <= 0f)
        {
            gameFinished = true;

            Debug.Log("TIME UP!");
        }
    }

    public void ShowNextDeliveryPoint()
    {
        if (pointsShown >= deliveryPoints.Length)
            return;

        PizzaDeliveryPoint point = deliveryPoints[pointsShown];

        point.SetVisible(true);

        pointsShown++;
    }

    public void CompleteDelivery()
    {
        completedDeliveries++;

        UpdateUI();

        // All deliveries completed
        if (completedDeliveries >= totalDeliveries)
        {
            gameFinished = true;

            Debug.Log("ALL DELIVERIES COMPLETE!");
        }
    }

    private void UpdateUI()
    {
        deliveryText.text =
            completedDeliveries + "/" + totalDeliveries;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text =
            minutes.ToString("0") + ":" +
            seconds.ToString("00");
    }
}