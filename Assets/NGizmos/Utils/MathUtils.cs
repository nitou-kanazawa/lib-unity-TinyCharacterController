using UnityEngine;
using System.Collections.Generic;

namespace Nitou.NGizmos
{
    public enum PlaneType
    {
        XY,
        YZ,
        ZX,
    }

    public static class PlaneTypeExtensions
    {
        /// <summary>
        /// 平面に対応した法線ベクトルを取得する．
        /// </summary>
        public static Vector3 GetNormal(this PlaneType type)
        {
            return type switch
            {
                PlaneType.XY => Vector3.forward,
                PlaneType.YZ => Vector3.right,
                PlaneType.ZX => Vector3.up,
                _ => throw new System.NotImplementedException()
            };
        }
    }

    internal static class MathUtils
    {
        /// <summary>
        /// 円の最小分割数（三角形を形成するために必要）
        /// </summary>
        private const int MIN_SEGMENT = 3;

        /// <summary>
        /// デフォルトの円分割数
        /// </summary>
        public const int DEFAULT_CIRCLE_SEGMENTS = 20;

        /// <summary>
        /// 半径に基づいて適切な分割数を計算する
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="defaultSegments">デフォルト分割数</param>
        /// <returns>最適化された分割数</returns>
        public static int CalculateOptimalSegments(float radius, int defaultSegments = DEFAULT_CIRCLE_SEGMENTS)
        {
            if (radius <= 0f) return MIN_SEGMENT;
            
            // 半径が小さい場合は分割数を減らす（最小は3）
            if (radius < 0.1f) return MIN_SEGMENT;
            if (radius < 0.5f) return Mathf.Max(MIN_SEGMENT, defaultSegments / 2);
            if (radius < 1.0f) return Mathf.Max(MIN_SEGMENT, (int)(defaultSegments * 0.75f));
            
            // 半径が大きい場合は分割数を増やす（最大は60）
            if (radius > 10f) return Mathf.Min(60, (int)(defaultSegments * 1.5f));
            
            return defaultSegments;
        }


        /// <summary>
        /// 円周上の座標リストを生成する．
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="segments">分割数（最小値: 3）</param>
        /// <param name="type">生成する平面</param>
        /// <param name="offset">オフセット座標</param>
        /// <param name="includeFullCircle">360度の点を含めるか</param>
        /// <returns>円周上の座標リスト</returns>
        public static List<Vector3> GetCirclePoints(float radius, int segments = 20,
                                                    PlaneType type = PlaneType.ZX,
                                                    Vector3 offset = default,
                                                    bool includeFullCircle = true)
        {
            // 入力値検証
            if (radius < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(radius), "半径は0以上である必要があります");

            if (segments < MIN_SEGMENT)
                throw new System.ArgumentOutOfRangeException(nameof(segments),
                    $"分割数は{MIN_SEGMENT}以上である必要があります");

            var points = new List<Vector3>();
            FillCirclePoints(points, radius, segments, type, offset, includeFullCircle);
            return points;
        }

        /// <summary>
        /// 円周上の座標を取得する．
        /// </summary>
        public static Vector3 GetCirclePoint(float radius, float angle, PlaneType type = PlaneType.ZX)
        {
            if (radius < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(radius), "半径は0以上である必要があります");
                
            var cos = radius * Mathf.Cos(angle);
            var sin = radius * Mathf.Sin(angle);

            return type switch
            {
                PlaneType.XY => new Vector3(cos, sin, 0f),
                PlaneType.YZ => new Vector3(0f, cos, sin),
                PlaneType.ZX => new Vector3(sin, 0f, cos),
                _ => throw new System.ArgumentException($"未対応の平面タイプ: {type}", nameof(type))
            };
        }


        /// <summary>
        /// 既存のリストに円周上の座標を追加する．
        /// </summary>
        public static void FillCirclePoints(List<Vector3> resultPoints, float radius, int segments = 20,
                                            PlaneType type = PlaneType.ZX, Vector3 offset = default,
                                            bool includeFullCircle = true)
        {
            if (resultPoints == null)
                throw new System.ArgumentNullException(nameof(resultPoints));
                
            if (radius < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(radius), "半径は0以上である必要があります");

            var pointCount = Mathf.Max(segments, MIN_SEGMENT);
            var deltaAngle = (Mathf.PI * 2) / pointCount;

            if (includeFullCircle) pointCount++;

            resultPoints.Clear();
            resultPoints.Capacity = pointCount; // 容量を事前確保

            for (int i = 0; i < pointCount; i++)
            {
                resultPoints.Add(GetCirclePoint(radius, i * deltaAngle, type) + offset);
            }
        }
    }
}