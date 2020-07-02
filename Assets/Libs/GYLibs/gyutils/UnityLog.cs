using UnityEngine;
using System.Collections;

public class UnityLog : MonoBehaviour {
    public static readonly UnityLog instance = new UnityLog();

    public void log(string str)
    {
        print(str);
    }
}
