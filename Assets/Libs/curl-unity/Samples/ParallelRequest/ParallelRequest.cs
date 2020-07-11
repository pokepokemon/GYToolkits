using CurlUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallelRequest : MonoBehaviour
{
    public string[] m_links;

    private CurlMulti multi;

    void Start()
    {
        multi = new CurlMulti();
        int count = 0;
        foreach(var link in m_links)
        {
            var easy = new CurlEasy();
            easy.uri = new Uri(link);
            easy.debug = true;
            easy.connectionTimeout = 3000;
            easy.outputPath = "D:/page" + (count++) + ".html";
            easy.MultiPerform(multi);
        }
    }
}
