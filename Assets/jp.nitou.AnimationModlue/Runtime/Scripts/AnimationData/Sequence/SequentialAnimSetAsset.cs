using System;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;

namespace Nitou.AnimationModule{

    [CreateAssetMenu(
        fileName = "New Sequential_AnimSet",    
        menuName = AssetMenu.Prefix.AnimationData + "Sequential AnimSet"
    )]
    public class SequentialAnimSetAsset : ScriptableObject{

        [SerializeField, Indent] ClipTransition _startMotion;
        [SerializeField, Indent] ClipTransition _loopMotion;
        [SerializeField, Indent] ClipTransition _endMotion;


        public ClipTransition StartClip => _startMotion;

        public ClipTransition LoopClip => _loopMotion;

        public ClipTransition EndClip => _endMotion;
    }


    /// <summary>
    /// 
    /// </summary>
    public class SequentialAnimStates {

        public readonly AnimancerState start;
        public readonly AnimancerState loop;
        public readonly AnimancerState end;

        public SequentialAnimStates(AnimancerState start, AnimancerState loop, AnimancerState end) {
            this.start = start;
            this.loop = loop;
            this.end = end;
        }
    }


    /// <summary>
    /// <see cref="SequentialAnimSetAsset"/>�Ɋւ���g�����\�b�h�W
    /// </summary>
    // public static class SequentialAnimSetAssetExtensitons {
    //
    //     /// <summary>
    //     /// <see cref="AnimancerComponent"/>�ɓo�^����
    //     /// </summary>
    //     public static SequentialAnimStates GetOrCreateStates(this AnimancerPlayable.StateDictionary dicti, SequentialAnimSetAsset animSet) {
    //
    //         // Animancer State
    //         var startState = dicti.GetOrCreate(animSet.StartClip);
    //         var loopState = dicti.GetOrCreate(animSet.LoopClip);
    //         var endState = dicti.GetOrCreate(animSet.EndClip);
    //
    //         // [NOTE]
    //         // Animancer�̃f�t�H���g�ł�Play()���邽�т�state��Events�̓N���A����Ă���
    //         // �������ŃC�x���g�o�^���Ă��Ӗ��Ȃ�
    //
    //
    //         return new SequentialAnimStates(startState, loopState, endState);
    //     }
    // }
}
