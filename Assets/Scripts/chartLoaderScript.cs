using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Gotta initialise the namespace!!
using ChartLoader.NET.Utils;
using ChartLoader.NET.Framework;
public class chartLoaderScript : MonoBehaviour
{
    public static ChartReader chartReader;
    // Start is called before the first frame update
    void Start()
    {
        chartReader = new ChartReader();
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
