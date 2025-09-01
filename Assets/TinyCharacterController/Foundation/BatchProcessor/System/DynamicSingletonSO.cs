using UnityEngine;

// REF:
// - Unity Forums: Do I need to destroy Instantiated ScriptableObjects not used anymore? https://forum.unity.com/threads/do-i-need-to-destroy-instantiated-scriptableobjects-not-used-anymore.1488063/

// NOTE:
// - UnityEngine.Objectから派生したオブジェクトは自分で破棄する必要がある. (Texture2D, Mesh, GameObject, etc)

namespace Nitou.BachProcessor
{
    /// <summary>
    /// Creates a ScriptableObject as a singleton.
    /// </summary>
    public class DynamicSingletonSO<T> : ScriptableObject where T : DynamicSingletonSO<T>
    {
        private static T _instance;

        /// <summary>
        /// インスタンスが生成済みかどうか.
        /// NOTE: When accessing the component at a time when it might be destroyed, like OnDestroy, always check for the existence of the object.
        /// </summary>
        protected static bool IsCreated => _instance != null;

        /// <summary>
        /// Get the instance. If the instance has not been created, it will be instantiated and registered.
        /// </summary>
        protected static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                // create instance and register callback.
                // ScriptableObject is not automatically destroyed in EnterPlayMode,
                // so it is necessary to detect the end of the application and destroy the component.
                _instance = ScriptableObject.CreateInstance<T>();
                Application.quitting += _instance.OnQuit;

                return _instance;
            }
        }

        /// ----------------------------------------------------------------------------

        /// <summary>
        /// アプリケーション終了時の処理．
        /// </summary>
        private void OnQuit()
        {
            Application.quitting -= OnQuit;
            Destroy(this);
        }
    }
}
