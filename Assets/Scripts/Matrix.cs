using UnityEngine;
using System;

[Serializable]
public class Matrix
{
    public int rows = 0;
    public int cols = 0;
    public float[] data;
    public float dispersal;

    public Matrix(int rows, int cols, bool randomize = false, float dispersal = 5.0f)
    {
        data = new float[cols * rows];
        this.rows = rows;
        this.cols = cols;
        if (randomize)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = UnityEngine.Random.Range(-dispersal, dispersal);
            }
        }
    }

    public Matrix(float[] data, int width)
    {
        if (data.Length % width != 0) throw new MException("bad initialize");
        data = new float[data.Length];
        this.cols = width;
        this.rows = data.Length / width;
        for (int i = 0; i < data.Length; i++)
        {
            this.data[i] = data[i];
        }
    }

    public Vector2 Shape()
    {
        return new Vector2(rows, cols);
    }

    public double this[int index]
    {
        get { return data[index]; }
        set { data[index] = (float)value; }
    }

    public double this[int row, int col]
    {
        get { return data[row * cols + col]; }
        set { data[row * cols + col] = (float)value; }
    }

    private static Matrix Add(Matrix m1, Matrix m2)
    {
        //Debug.Log($"adding {m1.Shape()} and {m2.Shape()}");
        if (m1.rows != m2.rows || m1.cols != m2.cols) throw new MException("cols and rows must be equal");
        Matrix result = new Matrix(m1.rows, m1.cols);
        for (int i = 0; i < m1.data.Length; i++)
        {
            result.data[i] = m1.data[i] + m2.data[i];
        }
        return result;
    }

    private static Matrix Multiply(Matrix m1, Matrix m2)
    {
        if (m1.cols != m2.rows) throw new MException("m1 cols must equal m2 rows");
        Matrix result = new Matrix(m1.rows, m2.cols);
        for (int i = 0; i < result.rows; i++)
        {
            for (int j = 0; j < result.cols; j++)
            {
                for (int k = 0; k < m1.cols; k++)
                {
                    result[i, j] += m1[i, k] * m2[k, j];
                }
            }
        }
        return result;
    }

    private static Matrix Multiply(float c, Matrix m)
    {
        Matrix result = new Matrix(m.rows, m.cols);
        for (int i = 0; i < m.data.Length; i++)
        {
            result.data[i] = c * m.data[i];
        }
        return result;
    }

    public static Matrix operator -(Matrix m)
    {
        return Matrix.Multiply(-1, m);
    }

    public static Matrix operator *(Matrix m1, Matrix m2)
    {
        return Matrix.Multiply(m1, m2);
    }

    public static Matrix operator +(Matrix m1, Matrix m2)
    {
        return Matrix.Add(m1, m2);
    }

    public static Matrix operator -(Matrix m1, Matrix m2)
    {
        return Matrix.Add(m1, -m2);
    }

    public void Print()
    {
        string result = "";
        for (int i = 0; i < data.Length; i++)
        {
            result += data[i] + " ";
        }
        result += "\n";
        Debug.Log(result);
    }
}

public class MException : Exception
{
    public MException(string Message)
        : base(Message)
    { }
}