using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeliveryManager : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private InputActionReference Enter;

    [Header("UI")]
    [SerializeField] private TMP_Text deliveryText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TMP_Text winMessage;

    [Header("Game Timer")]
    [SerializeField] private float gameTime = 180f; // 3 minutes

    [Header("Navigation")]
    [SerializeField] private DeliveryArrow deliveryArrow;
    [SerializeField] private Transform pizzaSpawner;

    private PizzaDeliveryPoint[] deliveryPoints;
    private PizzaCargo pizzaCargo;

    private int totalDeliveries;
    private int completedDeliveries;
    private int pointsShown;

    private float timer;
    private bool gameStarted = false;
    private bool gameFinished = false;

    private string winningText =
        "Congratulations! You have saved the town from the evil pizza God";

    private string failText =
        "unfortunetly some people had to be sacrificed to the pizza God";

    private void Start()
    {
        pizzaCargo = FindAnyObjectByType<PizzaCargo>();

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

    private void OnEnable()
    {
        Enter.action.Enable();
    }

    private void OnDisable()
    {
        Enter.action.Disable();
    }

    private void Update()
    {
        if (Enter.action.IsPressed() && !gameStarted)
        {
            startScreen.SetActive(false);
            gameStarted = true;
        }

        if (gameFinished || !gameStarted)
            return;

        // Count down
        timer -= Time.deltaTime;

        if (timer < 0f)
            timer = 0f;

        UpdateTimerUI();

        // Time ran out
        if (timer <= 0f)
        {
            GameOver(false);

            Debug.Log("TIME UP!");
        }
    }

    public void ShowNextDeliveryPoint()
    {
        if (pointsShown >= deliveryPoints.Length)
            return;

        PizzaDeliveryPoint point = deliveryPoints[pointsShown];

        // Reveal the next delivery point
        point.SetVisible(true);

        pointsShown++;

        // Point the arrow directly at the newly revealed point
        if (deliveryArrow != null)
        {
            deliveryArrow.SetTarget(
                point.GetTargetTransform()
            );
        }
    }

    public void CompleteDelivery()
    {
        completedDeliveries++;

        UpdateUI();

        // If there are still pizzas in the truck,
        // point towards the nearest remaining delivery point.
        if (pizzaCargo != null && pizzaCargo.GetPizzaCount() > 0)
        {
            SetNearestActiveDelivery();
        }
        else
        {
            // Truck is empty, so return to the pizza spawner.
            if (deliveryArrow != null && pizzaSpawner != null)
            {
                deliveryArrow.SetTarget(pizzaSpawner);
            }
        }

        if (completedDeliveries >= totalDeliveries)
        {
            GameOver(true);
        }
    }

    public void StartGame()
    {
        gameStarted = true;
    }

    public bool IsGameStarted()
    {
        return gameStarted;
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

    private void GameOver(bool winner)
    {
        if (gameFinished)
            return;

        gameFinished = true;

        endScreen.SetActive(true);

        winMessage.text =
            winner ? winningText : failText;
    }

    private void SetNearestActiveDelivery()
    {
        if (pizzaCargo == null || deliveryArrow == null)
            return;

        PizzaDeliveryPoint[] activePoints =
            FindObjectsByType<PizzaDeliveryPoint>(
                FindObjectsInactive.Exclude
            );

        float closestDistance = Mathf.Infinity;
        Transform closestPoint = null;

        foreach (PizzaDeliveryPoint point in activePoints)
        {
            Vector3 pointPosition =
                point.transform.position;

            Vector3 truckPosition =
                pizzaCargo.transform.position;

            // Ignore height
            pointPosition.y = 0f;
            truckPosition.y = 0f;

            float distance =
                Vector3.Distance(
                    truckPosition,
                    pointPosition
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;

                closestPoint =
                    point.GetTargetTransform();
            }
        }

        if (closestPoint != null)
        {
            deliveryArrow.SetTarget(closestPoint);
        }
    }
}