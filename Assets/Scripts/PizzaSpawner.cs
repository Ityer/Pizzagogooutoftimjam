using System.Collections;
using UnityEngine;

public class PizzaSpawner : MonoBehaviour
{
    [Header("Pizza Cargo")]
    [SerializeField] private PizzaCargo pizzaCargo;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 2f;

    private Coroutine pizzaRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (pizzaRoutine == null)
        {
            pizzaRoutine = StartCoroutine(AddPizzas());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (pizzaRoutine != null)
        {
            StopCoroutine(pizzaRoutine);
            pizzaRoutine = null;
        }
    }

    private IEnumerator AddPizzas()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            pizzaCargo.AddPizza();
        }
    }

}