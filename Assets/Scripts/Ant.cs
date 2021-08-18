using UnityEngine;
using System;

public class Ant : MonoBehaviour,  IComparable {
    public float maxSpeed = 4.0f; // 4
    public float minSpeed = 1.0f;
    public float steerStrength = 4.0f; // 2
    public float wanderStrength = 20.0f; // 20
    public float wanderDispersal = 0.2f; // needs to be less than 1.0

    public float viewRadius = 5.0f; // 5
    public float viewAngle = 120.0f; // 120
    public float signalDropFrequency = 0.5f; // 0.5
    public float pickupRadius = 0.1f; // 0.1
    public float size;
    public float energy;
    public float weightDispersal = 20.0f;
    public float biasDispersal = 2.0f;
    public float foodPerceptionRadius = 0.2f;
    public float signalPerceptionRadius = 0.2f;
    public float speedMutation = 0.2f;


    public static int[] networkShape = new int[3] { 11, 6, 2 };

    int handleFoodFrameCount = 0;
    const int handleFoodFrameReset = 10;

    Vector2 position;
    Vector2 velocity;
    Vector2 desiredDirection;

    GameObject activeFood;

    uint frameCount = 0;
    int foodLayer;
    int foodSignal, homeSignal;
    private bool hasFood = false;
    uint signalDropThreshold;

    static Vector3 left = new Vector2(-0.5f, 0.75f);
    static Vector3 middle = new Vector2(0f, 1.0f);
    static Vector3 right = new Vector2(-0.5f, 0.75f);
    float speed;
     

    Matrix inputs;

    public Layer layer;


    void Start() {
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        position = transform.position;
        size = Camera.main.orthographicSize;
        foodLayer = LayerMask.GetMask("Food");
        foodSignal = LayerMask.GetMask("Food Signal");
        homeSignal = LayerMask.GetMask("Home Signal");
        signalDropThreshold = (uint)(60f * signalDropFrequency);
        inputs = new Matrix(1, networkShape[0]);
        // noise for network
        inputs[6] = UnityEngine.Random.Range(-wanderStrength, wanderStrength);
    }

    void Update() {

        inputs[0] = velocity.x;
        inputs[1] = velocity.y;
        inputs[2] = energy;
        inputs[3] = Physics2D.OverlapCircleAll(transform.position + left, signalPerceptionRadius, homeSignal).Length;
        inputs[4] = Physics2D.OverlapCircleAll(transform.position + middle, signalPerceptionRadius, homeSignal).Length;
        inputs[5] = Physics2D.OverlapCircleAll(transform.position + right, signalPerceptionRadius, homeSignal).Length;
        inputs[6] = inputs[6] * (1 - wanderDispersal) + UnityEngine.Random.Range(-wanderStrength, wanderStrength) * wanderDispersal;
        inputs[7] = hasFood ? 1f : 0f;
        inputs[8] = Physics2D.OverlapCircleAll(transform.position + left, signalPerceptionRadius, foodSignal).Length;
        inputs[9] = Physics2D.OverlapCircleAll(transform.position + middle, signalPerceptionRadius, foodSignal).Length;
        inputs[10] = Physics2D.OverlapCircleAll(transform.position + right, signalPerceptionRadius, foodSignal).Length;

        desiredDirection = ForwardPass(inputs).normalized;

        Vector2 desiredVelocity = desiredDirection * speed;


        Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength);

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        //velocity += acceleration * Time.deltaTime;

        // keep the ant in bounds
        Vector2 newPosition = position + velocity * Time.deltaTime;

        //float bound = size / 2f;
        if (newPosition.x < -size || newPosition.x > size || newPosition.y > size || newPosition.y < -size)
        {
            CorrectDirection();
        }

        position += velocity * Time.deltaTime;

        //print($"position: {position}, velocity: {velocity}");

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));

        if (++handleFoodFrameCount == handleFoodFrameReset) {
            HandleFood();
            handleFoodFrameCount = 0;
        }
        if (++frameCount == signalDropThreshold) {
            GameObject newSignal;
            if (hasFood)
            {
                newSignal = ObjectPool.SharedInstance.RequestFoodSignal();
            } else
            {
                newSignal = ObjectPool.SharedInstance.RequestHomeSignal();
            }
            newSignal.transform.position = position;
            frameCount = 0;
        }
    }


    private void CorrectDirection() {
        layer.collisionCount++;
        velocity *= -0.5f;
    }

    private void HandleFood()
    { 
        Collider2D[] allFood = Physics2D.OverlapCircleAll(transform.position, viewRadius, foodLayer);
        if (allFood.Length > 0)
        {
            //GameObject activeFood = allFood[Random.Range(0, allFood.Length)].gameObject;
            GameObject food = allFood[0].gameObject;
            Vector2 dirToFood = (food.transform.position - transform.position).normalized;

            if (Vector2.Angle(desiredDirection, dirToFood) < viewAngle / 2)
            {
                //layer.foodCount++;
                //print("picking up food!");
                //FoodManager.Destroy(food);

                // maybe add this back in later when they are better

                if (Vector3.Distance(transform.position, food.transform.position) < pickupRadius)
                {
                    if (hasFood)
                    {
                        layer.encounterCount++;
                    } else
                    {
                        hasFood = true;
                        //GetComponent<SpriteRenderer>().color = Color.yellow;
                        //print("picking up food!");
                        activeFood = food;
                        activeFood.transform.parent = transform;
                        activeFood.transform.localPosition = new Vector2(0f, 0.5f);
                        activeFood.GetComponent<CircleCollider2D>().enabled = false;
                        layer.encounterCount++;
                    }
                }
            }
        }
    }

    public void RandomInitialization()
    {
        layer = new Layer(networkShape, weightDispersal, biasDispersal);
    }

    public Vector2 ForwardPass(Matrix inputs)
    {
        Matrix temp = inputs;
        //print($"Forward pass: {temp.Shape()} and {(temp * layer.weights[0]).Shape()}");

        for (int i = 0; i < layer.shape.Length - 1; i++)
        {
            temp = (temp * layer.weights[i]) + layer.biases[i];
        }
        return new Vector2(temp.data[0], temp.data[1]);
    }

    public void DropFood()
    {
        if (hasFood) {
            print("Successfully dropping food");
            hasFood = false;
            FoodManager.Destroy(activeFood);
            activeFood = null;
            layer.depositCount++;
        } 
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        if (obj is Ant ant)
        {
            return ant.layer.Score.CompareTo(layer.Score);
        }
        else
        {
            throw new ArgumentException("Object is not a Layer");
        }
    }

    public void Mutate(float rate, float dispersal = 0.1f)
    {
        for (int i = 0; i < layer.weights.Length; i++)
        {
            for (int j = 0; j < layer.weights[i].data.Length * rate; j++)
            {
                layer.weights[i].data[UnityEngine.Random.Range(0, layer.weights[i].data.Length)] += UnityEngine.Random.Range(-dispersal, dispersal);
                layer.biases[i][UnityEngine.Random.Range(0, layer.biases[i].data.Length)] += UnityEngine.Random.Range(-dispersal, dispersal);
            }
        }
    }

    public void ResetPositionAndVelocity(Vector2 position)
    {
        this.position = position;
        transform.position = position;
        this.velocity = Vector2.zero;
        if (hasFood)
        {
            hasFood = false;
            FoodManager.sharedInstance.DestroyFood(activeFood);
            activeFood = null;
        }
        //hasFood = false;
        //FoodManager.sharedInstance.DestroyFood(activeFood);
        //activeFood = null;

        layer.collisionCount = 0;
        layer.depositCount = 0;
        layer.encounterCount = 0;
        speed = Mathf.Clamp(speed + UnityEngine.Random.Range(-speedMutation, speedMutation), minSpeed, maxSpeed);
    }
}

