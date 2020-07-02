using UnityEngine;
using System.Collections;
using GYLib.Utils;

namespace GYLib.Utils
{
    public class EnterFrameBehaviour : MonoBehaviour {

        // Use this for initialization
        void Start() {
        }

        // Update is called once per frame
        void Update() {
            EnterFrame.instance.Update();
        }

        void OnDestroy()
        {
            EnterFrame.instance.removeAll();
        }
    }
}