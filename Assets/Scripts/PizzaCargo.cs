using UnityEngine;

public class PizzaCargo : MonoBehaviour
{
    [Header("Pizza Setup")]
    [SerializeField] private GameObject pizzaPrefab;
    [SerializeField] private Transform stackPoint;

    [Header("Stack Settings")]
    [SerializeField] private int pizzaCount = 0;
    [SerializeField] private int maxPizzas = 5;
    [SerializeField] private float pizzaHeight = 0.08f;

    private void Start()
    {
        UpdatePizzaStack();
    }

    public void SetPizzaCount(int amount)
    {
        pizzaCount = Mathf.Clamp(amount, 0, maxPizzas);
        UpdatePizzaStack();
    }

    private void UpdatePizzaStack()
    {
        // Remove existing pizzas
        for (int i = stackPoint.childCount - 1; i >= 0; i--)
        {
            Destroy(stackPoint.GetChild(i).gameObject);
        }

        for (int i = 0; i < pizzaCount; i++)
        {
            GameObject pizza = Instantiate(
                pizzaPrefab,
                stackPoint
            );

            pizza.transform.localPosition = new Vector3(
                0f,
                (pizzaHeight / 2f) + (i * pizzaHeight),
                0f
            );

            float randomY = Random.Range(-10f, 10f);

            pizza.transform.localRotation = Quaternion.Euler(
                0f,
                randomY,
                0f
            );
        }
    }

    public void AddPizza()
    {
        if (pizzaCount >= maxPizzas)
            return;

        pizzaCount++;
        UpdatePizzaStack();
    }
    public bool RemovePizza()
    {
        if (pizzaCount <= 0)
            return false;

        pizzaCount--;
        UpdatePizzaStack();

        return true;
    }

    public bool IsFull()
    {
        return pizzaCount >= maxPizzas;
    }

    public int GetPizzaCount()
    {
        return pizzaCount;
    }
}