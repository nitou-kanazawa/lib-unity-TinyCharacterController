using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Manager;
using Nitou.TCC.Controller.Shared;
using Nitou.TCC.Controller.Smb;

namespace Nitou.TCC.Controller.Check {
    

    /// <summary>
    /// 格納された情報の変化を監視し、追加・削除された要素を抽出するコンポーネント．
    /// キーに変化があった場合、コールバックを発行する．
    ///
    /// キーの追加・削除は AnimationModifierBehaviour から実行されることを想定している．
    /// </summary>
    [AddComponentMenu(MenuList.MenuCheck + nameof(AnimatorModifierCheck))]
    [DisallowMultipleComponent]
    public sealed class AnimatorModifierCheck : MonoBehaviour,
        IAnimationModifierUpdate {

        /// <summary>
        /// 更新タイミングを指定する．
        /// Animator が Physics で動作している場合は FixedUpdate、それ以外の場合は Update を指定する．
        /// </summary>
        [SerializeField] UpdateMode _updateMode = UpdateMode.Update;

        private Animator _animator;
        private readonly Dictionary<int, List<AnimatorModifierBehaviour>> _behaviours = new();

        private readonly List<PropertyName> _keyList = new();
        private readonly List<PropertyName> _currentKeyList = new();
        private readonly List<PropertyName> _removedKeyList = new();
        private readonly List<PropertyName> _addedKeyList = new();

        /// <summary>
        /// キーが追加または削除されたときのコールバック．
        /// </summary>
        public UnityEvent OnChangeKey;


        /// <summary>
        /// フレーム中にキーが追加または削除されたかどうかを判定する．
        /// </summary>
        public bool ChangeKey { get; private set; }

        /// <summary>
        /// 現在アクティブなキーのリスト．
        /// </summary>
        public List<PropertyName> CurrentKeys => _currentKeyList;
        

        /// ----------------------------------------------------------------------------
        // Lifecycle Events

        private void Awake() {
            TryGetComponent(out _animator);
            AnimationModifierSystem.Add(this, _updateMode == UpdateMode.FixedUpdate);
        }

        private void OnDestroy() {
            AnimationModifierSystem.Remove(this, _updateMode == UpdateMode.FixedUpdate);
        }

        void IAnimationModifierUpdate.OnUpdate() {
            // すべてのキー情報を削除して内容をリセットする
            _removedKeyList.Clear();
            _addedKeyList.Clear();
            _keyList.Clear();
            ChangeKey = false;

            AddCurrentKeyList(_animator.GetCurrentAnimatorStateInfo(0));
            AddCurrentKeyList(_animator.GetNextAnimatorStateInfo(0));

            // 前フレームのキー情報と現在フレームのキー情報を比較し、
            // 前フレームに存在しなかったキーがあれば AddedKeyList に追加する
            foreach (var key in _keyList) {
                if (_currentKeyList.Contains(key))
                    continue;

                ChangeKey = true;
                _addedKeyList.Add(key);
            }

            // 前フレームのキーが現在のキーリストに含まれていない場合、
            // RemoveKeyList に追加される
            foreach (var key in _currentKeyList) {
                if (_keyList.Contains(key))
                    continue;

                ChangeKey = true;
                _removedKeyList.Add(key);
            }

            // キーが追加または削除された場合、CurrentList を更新し OnChangeKey を呼び出す
            if (ChangeKey) {
                CopyKeyList();
                OnChangeKey?.Invoke();
            }
        }

        
        // ----------------------------------------------------------------------------
        // Public Method
        
        /// <summary>
        /// キーが保持されているかどうかを確認する．
        /// </summary>
        /// <param name="key">確認するキー</param>
        /// <returns>キーを保持している場合は true</returns>
        public bool HasKey(PropertyName key) => _currentKeyList.Contains(key);

        /// <summary>
        /// キーが保持されているかどうかを確認する．
        /// </summary>
        /// <param name="key">確認するキー</param>
        /// <returns>キーを保持している場合は true</returns>
        public bool HasKey(string key) => HasKey(new PropertyName(key));

        /// <summary>
        /// このフレーム中にキーが削除されたかどうかを確認する．
        /// 同じフレーム内で追加されなかったキーは自動的に削除される．
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>このフレーム中に削除された場合は true</returns>
        public bool IsRemoved(PropertyName key) => _removedKeyList.Contains(key);

        /// <summary>
        /// このフレーム中にキーが削除されたかどうかを確認する．
        /// 同じフレーム内で追加されなかったキーは自動的に削除される．
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>このフレーム中に削除された場合は true</returns>
        public bool IsRemoved(string key) => IsRemoved(new PropertyName(key));

        /// <summary>
        /// このフレーム中にキーが追加されたかどうかを確認する．
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>このフレーム中に追加された場合は true</returns>
        public bool IsAdded(PropertyName key) => _addedKeyList.Contains(key);

        /// <summary>
        /// このフレーム中にキーが追加されたかどうかを確認する．
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>このフレーム中に追加された場合は true</returns>
        public bool IsAdded(string key) => IsAdded(new PropertyName(key));
        

        // ----------------------------------------------------------------------------
        // Private Method

        private void CopyKeyList() {
            _currentKeyList.Clear();
            _currentKeyList.AddRange(_keyList);
        }

        private void AddCurrentKeyList(AnimatorStateInfo stateInfo) {
            var hash = stateInfo.fullPathHash;

            if (_behaviours.ContainsKey(hash) == false) {
                CacheBehaviour(hash);
            }

            // キーリストを登録する
            foreach (var behaviour in _behaviours[hash]) {
                if (behaviour.IsInRange(stateInfo))
                    _keyList.Add(behaviour.Key);
            }
        }

        private void CacheBehaviour(int hash) {
            var behaviours = new List<AnimatorModifierBehaviour>();
            foreach (var behaviour in _animator.GetBehaviours(hash, 0)) {
                if (behaviour is AnimatorModifierBehaviour animationModifierBehaviour)
                    behaviours.Add(animationModifierBehaviour);
            }
            _behaviours.Add(hash, behaviours);
        }


        // ----------------------------------------------------------------------------
        private enum UpdateMode {
            Update = 0,
            FixedUpdate = 1
        }
    }
}
