using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimatorPlayer))]
public class AnimatorPlayerInspector : Editor
{
    public AnimatorPlayer anim;

    private string _animName;

    private void OnEnable()
    {
        anim = target as AnimatorPlayer;
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();

        if (anim.clips != null)
        {
            foreach (var clip in anim.clips)
            {
                if (GUILayout.Button(clip.name))
                {
                    if (anim.anim != null)
                    {
                        anim.anim.Play(clip.name);
                    }
                }
            }
        }

        _animName = EditorGUILayout.TextField("动画名", _animName);
        if (GUILayout.Button("播放"))
        {
            if (anim.anim != null)
            {
                anim.anim.Play(_animName);
            }
        }

        EditorGUILayout.EndVertical();
    }
}
