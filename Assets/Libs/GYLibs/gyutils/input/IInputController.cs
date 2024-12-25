using UnityEngine;
using UnityEngine.Events;

namespace GYLibs.control
{
    public interface IInputController
    {
        /// <summary>
        /// 参数2为<see cref="OnClickInputType"/> 
        /// </summary>
        UnityAction<Vector2, int> OnClick { get; set; }

        UnityAction<Vector2, int> OnDown { get; set; }

        void Update();
        void Stop();
    }
}
