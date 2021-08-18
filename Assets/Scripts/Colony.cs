using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colony : MonoBehaviour
{
    int antMask;
    int frameCheck;
    public int refresh = 20;
    float radius;
    private void Start()
    {
        antMask = LayerMask.GetMask("Ant");
        radius = transform.localScale.x;
    }

    private void Update()
    {
        if (++frameCheck == refresh)
        {
            Collider2D[] ants = Physics2D.OverlapCircleAll(transform.position, radius, antMask);
            foreach (Collider2D ant in ants)
            {
                ant.GetComponent<Ant>().DropFood();
            }
            frameCheck = 0;
        }
    }
}
