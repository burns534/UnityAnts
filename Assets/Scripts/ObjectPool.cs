using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool SharedInstance;
    Queue<GameObject> foodSignals;
    Queue<GameObject> homeSignals;
    public GameObject foodSignal;
    public GameObject homeSignal;

    public int amountToPool = 2000;
    public int maxPoolSize = 5000;
    int activeFoodSignalCount = 0;
    int activeHomeSignalCount = 0;

    public enum Type
    {
        Food, Home
    };

    private void Awake()
    {
        SharedInstance = this;
    }

    private void Start()
    {
        foodSignals = new Queue<GameObject>();
        homeSignals = new Queue<GameObject>();
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(foodSignal);
            tmp.SetActive(false);
            tmp.transform.parent = transform;
            foodSignals.Enqueue(tmp);

            tmp = Instantiate(homeSignal);
            tmp.SetActive(false);
            tmp.transform.parent = transform;
            homeSignals.Enqueue(tmp);
        }
    }

    public GameObject RequestHomeSignal()
    {
        GameObject newSignal;
        if (activeHomeSignalCount++ < homeSignals.Count)
        {
            newSignal = homeSignals.Dequeue();
            homeSignals.Enqueue(newSignal);
            newSignal.SetActive(true);
        }
        else
        {
            if (activeHomeSignalCount < maxPoolSize)
            {
                newSignal = Instantiate(homeSignal);
                newSignal.transform.parent = transform;
                homeSignals.Enqueue(newSignal);
            } else
            {
                newSignal = homeSignals.Dequeue();
                newSignal.SetActive(true);
                homeSignals.Enqueue(newSignal);
            }
        }
        return newSignal;

    }

    public GameObject RequestFoodSignal()
    {
        GameObject newSignal;
        if (activeFoodSignalCount++ < foodSignals.Count)
        {
            newSignal = foodSignals.Dequeue();
            foodSignals.Enqueue(newSignal);
            newSignal.SetActive(true);
        }
        else
        {
            if (activeFoodSignalCount < maxPoolSize)
            {
                newSignal = Instantiate(foodSignal);
                newSignal.transform.parent = transform;
                foodSignals.Enqueue(newSignal);
            }
            else
            {
                newSignal = foodSignals.Dequeue();
                newSignal.SetActive(true);
                foodSignals.Enqueue(newSignal);
            }
        }
        return newSignal;

    }

    public void Withdraw()
    {
        foreach (GameObject signal in foodSignals)
        {
            signal.SetActive(false);
        }

        foreach(GameObject signal in homeSignals)
        {
            signal.SetActive(false);
        }
    }
}
