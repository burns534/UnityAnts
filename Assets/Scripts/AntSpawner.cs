using UnityEngine;
using System.IO;
using System;

public class AntSpawner : MonoBehaviour {
    public GameObject ant;
    public GameObject colony;
    public int antCount = 50; // 50
    public int generations = 5; // 10? depends
    public float mutationRate = 0.5f;
    public float mutationStrength = 0.2f;
    public float survivalRate = 0.2f; 
    public string filename = "save";
    public float generationDuration = 20.0f;
    public bool shouldSave = true;
    float generationStartTimestamp = 0f;
    float bounds;

    int currentGeneration = 0;
    //Layer[] layers;
    Ant[] ants;
    string filepath;
    private bool needsSaving = false;
    private int surviving;
    Vector2 colonyPosition;

    void Start() {
        bounds = Camera.main.orthographicSize;
        surviving = Mathf.Max((int)(survivalRate * antCount), 1);
        if (surviving > antCount) throw new MException("surviving can't be larger than the number of ants silly");
        filepath = Application.persistentDataPath + "/" + filename + ".json";
        FoodManager.sharedInstance.Render();
        colonyPosition = new Vector2(UnityEngine.Random.Range(-bounds + 5f, bounds - 5f), UnityEngine.Random.Range(-bounds + 5f, bounds - 5f));
        GameObject newColony = Instantiate(colony);
        newColony.transform.parent = transform.parent;
        newColony.transform.position = colonyPosition;
        ants = new Ant[antCount];
        GameObject newAnt;
        try {
            print("trying");
            throw new FileNotFoundException();
            //using (StreamReader sr = new StreamReader(filepath))
            //{
            //    string saveJSON = "";
                
            //    string line;
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        saveJSON += line;
            //    }

            //    SaveData data = JsonUtility.FromJson<SaveData>(saveJSON);

            //    if (data.layers.Length != antCount) throw new FileNotFoundException("hahhh");

            //    Layer[] layers = data.layers;
                
            //    for (int i = 0; i < antCount; i++)
            //    {
            //        newAnt = Instantiate(ant);
            //        newAnt.transform.parent = transform;
            //        newAnt.transform.position = colonyPosition;

            //        //newAnt.GetComponent<Ant>().layer = layers[i];
            //        ants[i] = newAnt.GetComponent<Ant>();
            //        ants[i].layer = layers[i];
            //    }
            //    print("loaded ants from file");
            //}
        }
        catch(FileNotFoundException)
        {
            print("no save file found");
            needsSaving = true;
            //layers = new Layer[antCount];

            for (int i = 0; i < antCount; i++)
            {
                newAnt = Instantiate(ant);
                newAnt.transform.parent = transform;
                newAnt.transform.position = colonyPosition;
                //Ant antScript = newAnt.GetComponent<Ant>();
                //antScript.RandomInitialization();
                //layers[i] = antScript.layer;
                ants[i] = newAnt.GetComponent<Ant>();
                ants[i].RandomInitialization();
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
  
        if (Time.time - generationStartTimestamp > generationDuration && currentGeneration < generations)
        {
            needsSaving = true;
            if (currentGeneration++ > generations)
            {
                Application.Quit();
            }
            generationStartTimestamp = Time.time;
            FoodManager.sharedInstance.Render();
            ObjectPool.SharedInstance.Withdraw();

            Array.Sort(ants); // top 10 automatically in top now
       

            //print($"first: {layers[0].foodCount}, last: {layers[antCount - 1].foodCount}");
            
            for (int i = 0; i < ants[0].layer.weights.Length; i++)
            {
                int weightSize = 4 * ants[0].layer.weights[i].data.Length;
                int biasSize = 4 * ants[0].layer.biases[i].data.Length;

                //print($"weightSize: {weightSize}, biasSize: {biasSize}");

                for (int j = surviving; j < ants.Length; j++)
                {
                    Buffer.BlockCopy(ants[j % surviving].layer.weights[i].data, 0, ants[j].layer.weights[i].data, 0, weightSize);
                    Buffer.BlockCopy(ants[j % surviving].layer.biases[i].data, 0, ants[j].layer.biases[i].data, 0, biasSize);
                }
            }

            for (int i = 0; i < surviving; i++)
            {
                ants[i].layer.Score = 0;
                ants[i].ResetPositionAndVelocity(colonyPosition);
  
            }

            for (int i = surviving; i < antCount; i++)
            {
                ants[i].layer.Score = 0;
                ants[i].Mutate(mutationRate, mutationStrength);
                ants[i].ResetPositionAndVelocity(colonyPosition);
            }

            print("performed mutations");
            
        }

    }

    private void OnApplicationQuit()
    {
        if (needsSaving && shouldSave) {
            print("writing save data");
            Layer[] layers = new Layer[antCount];
            for (int i = 0; i < antCount; i++)
            {
                layers[i] = ants[i].layer;
            }
            SaveData saveData = new SaveData(layers);

            string JSONString = JsonUtility.ToJson(saveData, true);
            

            using (StreamWriter sw = new StreamWriter(filepath))
            {
                sw.WriteLine(JSONString);
            }
            needsSaving = false;
        }
    }
}

[System.Serializable]
public struct SaveData
{
    public Layer[] layers;

    public SaveData(Layer[] layers)
    {
        this.layers = layers;
    }
}

