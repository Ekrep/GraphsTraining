using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TestTriangle : MonoBehaviour
{

    public Point pointPrefab;
    public int num;
    // Start is called before the first frame update
    void Start()
    {
        int sum = 0;
        int iteration = 0;
        while (sum <= num - iteration)
        {
            sum += iteration;
            for (int i = 0; i < iteration; i++)
            {
                Point createdPoint = Instantiate(pointPrefab);
                createdPoint.SetPosition(new Vector3(i - iteration + i, num - iteration));
            }
            iteration++;



        }
    }

}
