using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public GameObject food;
    public int maxFood = 200; // 200
    public float dispersal = 0.84f; // 0.84f
    public float clumping = 2.0f; // 2.0f
    Queue<GameObject> foodPool;
    public static FoodManager sharedInstance;
    float bound;

    private void Awake()
    {
        sharedInstance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        foodPool = new Queue<GameObject>();
        bound = Camera.main.orthographicSize - clumping;

        GameObject newFood;
        for (int i = 0; i < maxFood; i++)
        {
            newFood = Instantiate(food);
            newFood.transform.parent = transform;
            newFood.SetActive(false);
            foodPool.Enqueue(newFood);
        }
        
    }

    public void DestroyFood(GameObject food)
    {
        //if (food) {
        //    foodPool.Enqueue(food);
        //    food.SetActive(false);
        //    food.transform.parent = transform;
        //    food.GetComponent<CircleCollider2D>().enabled = true;
        //}
        foodPool.Enqueue(food);
        food.SetActive(false);
        food.transform.parent = transform;
        food.GetComponent<CircleCollider2D>().enabled = true;
    }

    public void Render()
    {
        GameObject newFood;
        Vector2 position = new Vector2(Random.Range(-bound, bound), Random.Range(-bound, bound));
        for (int i = 0; i < foodPool.Count;)
        {
            if (Random.Range(0, 1.0f) < dispersal)
            {
                newFood = foodPool.Dequeue();
                newFood.transform.position = new Vector2(position.x + Random.Range(-clumping, clumping), position.y + Random.Range(-clumping, clumping));
                newFood.SetActive(true);
                i++;
            }
            else
            {
                position = new Vector2(Random.Range(-bound, bound), Random.Range(-bound, bound));
            }
        }
    }


}
