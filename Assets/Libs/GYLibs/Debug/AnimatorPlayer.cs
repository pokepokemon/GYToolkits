using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public class AnimatorPlayer : MonoBehaviour
{
    public Animator anim;

    public List<AnimationClip> clips;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Reset()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
#if UNITY_EDITOR
        if (anim != null)
        {
            clips = GetClips(anim);
        }
#endif
    }

#if UNITY_EDITOR
    private List<AnimationClip> GetClips(Animator animator)
    {
        UnityEditor.Animations.AnimatorController controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        return GetClipsFromStatemachine(controller.layers[0].stateMachine);
    }

    private List<AnimationClip> GetClipsFromStatemachine(UnityEditor.Animations.AnimatorStateMachine stateMachine)
    {
        List<AnimationClip> list = new List<AnimationClip>();
        for (int i = 0; i != stateMachine.states.Length; ++i)
        {
            UnityEditor.Animations.ChildAnimatorState state = stateMachine.states[i];
            if (state.state.motion is UnityEditor.Animations.BlendTree)
            {
                UnityEditor.Animations.BlendTree blendTree = state.state.motion as UnityEditor.Animations.BlendTree;
                ChildMotion[] childMotion = blendTree.children;
                for (int j = 0; j != childMotion.Length; ++j)
                {
                    list.Add(childMotion[j].motion as AnimationClip);
                }
            }
            else if (state.state.motion != null)
                list.Add(state.state.motion as AnimationClip);
        }
        for (int i = 0; i != stateMachine.stateMachines.Length; ++i)
        {
            list.AddRange(GetClipsFromStatemachine(stateMachine.stateMachines[i].stateMachine));
        }
        
        return list;
    }
#endif
}
