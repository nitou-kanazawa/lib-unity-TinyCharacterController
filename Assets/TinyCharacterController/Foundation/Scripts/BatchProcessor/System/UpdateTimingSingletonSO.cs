using UnityEngine;
using UnityEngine.Playables;

namespace Nitou.BatchProcessor
{
    /// <summary>
    /// 更新タイミング
    /// </summary>
    public enum UpdateTiming : int
    {
        Update = 0,
        FixedUpdate = 1,
        LateUpdate = 2,
    }


    /// <summary>
    ///
    /// </summary>
    public abstract class UpdateTimingSingletonSO<TSystem> : ScriptableObject
        where TSystem : UpdateTimingSingletonSO<TSystem>
    {
        #region Static

        /// <summary>
        /// <see cref="UpdateTiming"/>の各タイミングをサポートするためのインスタンス
        /// </summary>
        private static readonly TSystem[] Instance = new TSystem[3];

        /// <summary>
        /// インスタンスが生成済みか確認する　（※生成はしない）
        /// </summary>
        public static bool IsCreated(UpdateTiming timing) => Instance[(int)timing] != null;

        /// <summary>
        /// Get the instance. If the instance does not exist, it will be created.
        /// </summary>
        /// <param name="timing">Timing of update</param>
        /// <returns>The instance</returns>
        public static TSystem GetInstance(UpdateTiming timing)
        {
            var index = (int)timing;
            if (IsCreated(timing)) return Instance[index];

            var instance = CreateInstance<TSystem>();
            instance.Timing = timing;
            instance.OnCreate(timing);
            Application.quitting += instance.OnQuit;

            Instance[index] = instance;

            return instance;
        }

        #endregion


        /// <summary>
        /// Execution timing.
        /// </summary>
        public UpdateTiming Timing { get; private set; }

        /// <summary>
        /// Callback called when an instance is created.
        /// This is used to avoid interference with subclasses when implemented in Awake.
        /// It is called after Awake.
        /// </summary>
        protected virtual void OnCreate(UpdateTiming timing) { }

        /// <summary>
        /// Destroy the instance when the application is quitting.
        /// This is to handle EnterPlayMode.
        /// </summary>
        private void OnQuit()
        {
            Application.quitting -= OnQuit;
            DestroyImmediate(this);
        }
    }
}