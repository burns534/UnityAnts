using System;

[System.Serializable]
public class Layer : IComparable
{
    public Matrix[] weights;
    public Matrix[] biases;
    public int[] shape;
    public int encounterCount;
    public int depositCount;
    public int collisionCount;
    public float collisionScoreMultiplier = 0.2f;
    public float depositScoreMultiplier = 3f;
    public float Score
    {
        get { return encounterCount + depositCount * depositScoreMultiplier - collisionCount * collisionScoreMultiplier; }
        set { this.encounterCount = 0; this.depositCount = 0; }
    }


    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        if (obj is Layer layer)
        {
            return layer.Score.CompareTo(Score);
        }
        else
        {
            throw new ArgumentException("Object is not a Layer");
        }
    }

    public Layer(int[] shape, float weightDispersal, float biasDispersal)
    {
        this.shape = shape;
        weights = new Matrix[shape.Length - 1];
        biases = new Matrix[shape.Length - 1];

        for (int i = 0; i < shape.Length - 1; i++)
        {
            weights[i] = new Matrix(shape[i], shape[i + 1], true, weightDispersal);
            biases[i] = new Matrix(1, shape[i + 1], true, biasDispersal);
        }
    }
}
