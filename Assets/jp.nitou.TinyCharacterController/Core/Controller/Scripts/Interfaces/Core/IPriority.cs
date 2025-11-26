using System.Collections.Generic;

namespace Nitou.TCC.Controller.Interfaces.Core
{
    /// <summary>
    /// 優先度に基づいてオブジェクトを再配置するインターフェース．
    /// </summary>
    public interface IPriority<T>
        where T : class, IPriority<T>
    {
        /// <summary>
        /// 優先度．
        /// </summary>
        int Priority { get; }
    }


    /// <summary>
    /// <see cref="IPriority{T}"/>型の拡張メソッド集．
    /// </summary>
    public static class PriorityExtensions
    {
        /// <summary>
        /// 最も高い優先度を持つクラスを抽出する．
        /// 優先度が0以下のクラスは存在しないものとして扱う．
        /// </summary>
        public static bool TryGetHighestPriority<T>(this IEnumerable<T> values, out T result)
            where T : class, IPriority<T>
        {
            result = null;
            int highestPriority = default;

            foreach (var value in values)
            {
                if (value.Priority <= 0 || value.Priority <= highestPriority)
                    continue;

                result = value;
                highestPriority = value.Priority;
            }

            return highestPriority != 0;
        }
    }
}