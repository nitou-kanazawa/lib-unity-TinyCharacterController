using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// REF:
//  - Hatena: AnimatorControllerの現在のステート名を取得したりステートの切り替わりを監視したりする仕組みを作る https://light11.hatenadiary.com/search?q=StateMachineBehaviour
//  - qiita: UniRxでアニメーションの開始と終了をスクリプトで受け取る https://qiita.com/nodokamome/items/a5a3610d7adef81d6b77
//  - kanのメモ帳: Animatorのステート(状態)の変更を検知する StateMachineBehaviour https://kan-kikuchi.hatenablog.com/entry/StateMachineBehaviour

namespace Nitou.AnimationModule
{
    /// <summary>
    /// AnimatorControllerの現在ステートを監視する<see cref="StateMachineBehaviour"/>．
    /// 全ステート名や現在のステート情報を提供するStateMachineBehaviour
    /// (※エディタ拡張で自動的にAnimatorのBaseLayerにアタッチされる)
    /// </summary>
    public sealed class AnimatorStateEvent : ObservableStateMachineTrigger
    {
        [SerializeField] private int _layer;
        [SerializeField] private string[] _stateFullPaths;


        /// <summary>
        /// 対象のレイヤー．
        /// </summary>
        public int Layer => _layer;

        /// <summary>
        /// 全ステート情報．
        /// (※AnimatorEditorUtilityによって自動でセットアップ)
        /// </summary>
        public string[] StateFullPaths => _stateFullPaths;

        /// <summary>
        /// 現在のステート名．
        /// </summary>
        public string CurrentStateName { get; private set; }

        /// <summary>
        /// 現在のサブステート名．
        /// </summary>
        public string CurrentSubStateName
        {
            get
            {
                var split = CurrentStateFullPath.Split('.');
                return (split.Length >= 3) ? split[1] : ""; // ※2階層目をサブステートとする
            }
        }

        /// <summary>
        /// 現在のステートのフルパス
        /// </summary>
        public string CurrentStateFullPath { get; private set; }

        /// <summary>
        /// ステート名に対応したハッシュ値
        /// </summary>
        private int[] StateFullPathHashes
        {
            get
            {
                if (_stateFullPathHashes == null)
                {
                    _stateFullPathHashes = _stateFullPaths
                                           .Select(x => Animator.StringToHash(x))
                                           .ToArray();
                }

                return _stateFullPathHashes;
            }
        }

        private int[] _stateFullPathHashes;


        /// <summary>
        /// ステートが変わった時のコールバック
        /// </summary>
        public event System.Action<(string stateName, string stateFullPath)> stateEntered;


        // ----------------------------------------------------------------------------
        // Overide Method 

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // 現在のステート情報を取得
            bool isSuccece = false;
            for (int i = 0; i < StateFullPathHashes.Length; i++)
            {
                var stateFullPathHash = StateFullPathHashes[i];

                // ステートが判明したら
                if (stateInfo.fullPathHash == stateFullPathHash)
                {
                    // ステート名
                    CurrentStateFullPath = _stateFullPaths[i];
                    CurrentStateName = CurrentStateFullPath.Split('.').Last();

                    // コールバック
                    stateEntered?.Invoke((CurrentStateName, CurrentStateFullPath));


                    isSuccece = true;
                    break;
                }
            }

            // マッチするステートが無ければエラー処理
            if (!isSuccece) throw new System.Exception();

            // ベースのイベント処理 (※CurrentStateを更新してから実行)
            base.OnStateEnter(animator, stateInfo, layerIndex); // ※ここは使いやすいストリームを定義するほうが良さそう
        }


        // ----------------------------------------------------------------------------

        #region Static Method

        /// <summary>
        /// Animatorからインスタンスを取得する
        /// </summary>
        public static AnimatorStateEvent Get(Animator animator, int layer)
        {
            return animator.GetBehaviours<AnimatorStateEvent>().First(x => x.Layer == layer);
        }

        #endregion
    }
}