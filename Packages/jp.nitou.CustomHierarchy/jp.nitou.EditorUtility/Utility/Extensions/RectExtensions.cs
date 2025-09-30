using UnityEngine;

namespace Nitou.EditorShared
{
    /// <summary>
    /// <see cref="Rect"/>型の基本的な拡張メソッド集．
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        /// デコンストラクタ．
        /// </summary>
        public static void Deconstruct(this Rect self, out Vector2 position, out Vector2 size)
        {
            position = self.position;
            size = self.size;
        }

        /// <summary>
        /// デコンストラクタ．
        /// </summary>
        public static void Deconstruct(this Rect self, out float x, out float y, out float width, out float height)
        {
            x = self.x;
            y = self.y;
            width = self.width;
            height = self.height;
        }


        // ----------------------------------------------------------------------------

        #region Position Operations

        /// <summary>
        /// 矩形の位置を設定する拡張メソッド．
        /// </summary>
        public static Rect SetPosition(this Rect rect, Vector2 position)
        {
            rect.position = position;
            return rect;
        }

        /// <summary>
        /// X座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetX(this Rect rect, float x)
        {
            rect.x = x;
            return rect;
        }

        /// <summary>
        /// 最大X座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetXMax(this Rect rect, float xMax)
        {
            rect.xMax = xMax;
            return rect;
        }

        /// <summary>
        /// 最小X座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetXMin(this Rect rect, float xMin)
        {
            rect.xMin = xMin;
            return rect;
        }

        /// <summary>
        /// 中心座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetCenter(this Rect rect, Vector2 center)
        {
            rect.center = center;
            return rect;
        }

        /// <summary>
        /// 中心座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetCenter(this Rect rect, float x, float y)
        {
            rect.center = new Vector2(x, y);
            return rect;
        }

        /// <summary>
        /// 中心X座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetCenterX(this Rect rect, float x)
        {
            rect.center = new Vector2(x, rect.center.y);
            return rect;
        }

        /// <summary>
        /// 中心Y座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetCenterY(this Rect rect, float y)
        {
            rect.center = new Vector2(rect.center.x, y);
            return rect;
        }

        /// <summary>
        /// 最大座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetMax(this Rect rect, Vector2 max)
        {
            rect.max = max;
            return rect;
        }

        /// <summary>
        /// 最小座標を設定する拡張メソッド．
        /// </summary>
        public static Rect SetMin(this Rect rect, Vector2 min)
        {
            rect.min = min;
            return rect;
        }

        /// <summary>
        /// 位置を原点にリセットする拡張メソッド．
        /// </summary>
        public static Rect ResetPosition(this Rect rect)
        {
            rect.position = Vector2.zero;
            return rect;
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Size Operations

        /// <summary>
        /// サイズを設定する拡張メソッド．
        /// </summary>
        public static Rect SetSize(this Rect rect, float width, float height)
        {
            rect.size = new Vector2(width, height);
            return rect;
        }

        /// <summary>
        /// 正方形のサイズを設定する拡張メソッド．
        /// </summary>
        public static Rect SetSize(this Rect rect, float widthAndHeight)
        {
            rect.size = new Vector2(widthAndHeight, widthAndHeight);
            return rect;
        }

        /// <summary>
        /// サイズを設定する拡張メソッド．
        /// </summary>
        public static Rect SetSize(this Rect rect, Vector2 size)
        {
            rect.size = size;
            return rect;
        }

        /// <summary>
        /// 幅を設定する拡張メソッド．
        /// </summary>
        public static Rect SetWidth(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        /// <summary>
        /// 高さを設定する拡張メソッド．
        /// </summary>
        public static Rect SetHeight(this Rect rect, float height)
        {
            rect.height = height;
            return rect;
        }

        /// <summary>
        /// 位置を加算する拡張メソッド．
        /// </summary>
        public static Rect AddPosition(this Rect rect, Vector2 move)
        {
            rect.position += move;
            return rect;
        }

        /// <summary>
        /// 位置を加算する拡張メソッド．
        /// </summary>
        public static Rect AddPosition(this Rect rect, float x, float y)
        {
            rect.position += new Vector2(x, y);
            return rect;
        }

        /// <summary>
        /// X座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddX(this Rect rect, float x)
        {
            rect.position += new Vector2(x, 0);
            return rect;
        }

        /// <summary>
        /// Y座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddY(this Rect rect, float y)
        {
            rect.position += new Vector2(0, y);
            return rect;
        }

        /// <summary>
        /// 最大座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddMax(this Rect rect, Vector2 value)
        {
            rect.max += value;
            return rect;
        }

        /// <summary>
        /// 最小座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddMin(this Rect rect, Vector2 value)
        {
            rect.min += value;
            return rect;
        }

        /// <summary>
        /// 最大X座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddXMax(this Rect rect, float value)
        {
            rect.xMax += value;
            return rect;
        }

        /// <summary>
        /// 最小X座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddXMin(this Rect rect, float value)
        {
            rect.xMin += value;
            return rect;
        }

        /// <summary>
        /// 最大Y座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddYMax(this Rect rect, float value)
        {
            rect.yMax += value;
            return rect;
        }

        /// <summary>
        /// 最小Y座標を加算する拡張メソッド．
        /// </summary>
        public static Rect AddYMin(this Rect rect, float value)
        {
            rect.yMin += value;
            return rect;
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Alignment

        /// <summary>
        /// X軸中央に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignCenterX(this Rect rect, float width)
        {
            return new Rect(rect.x + (rect.width - width) / 2, rect.y, width, rect.height);
        }

        /// <summary>
        /// XY軸中央に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignCenterXY(this Rect rect, float width, float height)
        {
            return new Rect(rect.x + (rect.width - width) / 2, rect.y + (rect.height - height) / 2, width, height);
        }

        /// <summary>
        /// XY軸中央に正方形で配置する拡張メソッド．
        /// </summary>
        public static Rect AlignCenterXY(this Rect rect, float size)
        {
            return new Rect(rect.x + (rect.width - size) / 2, rect.y + (rect.height - size) / 2, size, size);
        }

        /// <summary>
        /// Y軸中央に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignCenterY(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y + (rect.height - height) / 2, rect.width, height);
        }

        /// <summary>
        /// 上端に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignTop(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y, rect.width, height);
        }

        /// <summary>
        /// 中央に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignMiddle(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y + (rect.height - height) / 2, rect.width, height);
        }

        /// <summary>
        /// 下端に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignBottom(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.yMax - height, rect.width, height);
        }

        /// <summary>
        /// 左端に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignLeft(this Rect rect, float width)
        {
            return new Rect(rect.x, rect.y, width, rect.height);
        }

        /// <summary>
        /// 右端に配置する拡張メソッド．
        /// </summary>
        public static Rect AlignRight(this Rect rect, float width)
        {
            return new Rect(rect.xMax - width, rect.y, width, rect.height);
        }

        /// <summary>
        /// 右端に配置する拡張メソッド（クランプオプション付き）．
        /// </summary>
        public static Rect AlignRight(this Rect rect, float width, bool clamp)
        {
            return clamp ? new Rect(Mathf.Max(rect.x, rect.xMax - width), rect.y, width, rect.height) : new Rect(rect.xMax - width, rect.y, width, rect.height);
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Expansion

        /// <summary>
        /// 全方向に指定値だけ広げる拡張メソッド．
        /// </summary>
        public static Rect Expand(this Rect rect, float left, float right, float top, float bottom)
        {
            return new Rect(
                rect.x - left,
                rect.y - top,
                rect.width + left + right,
                rect.height + top + bottom);
        }

        /// <summary>
        /// 全方向に指定値だけ広げる拡張メソッド．
        /// </summary>
        public static Rect Expand(this Rect rect, float horizontal, float vertical)
        {
            return new Rect(
                rect.x - horizontal / 2,
                rect.y - vertical / 2,
                rect.width + horizontal,
                rect.height + vertical);
        }

        /// <summary>
        /// 全方向に指定値だけ広げる拡張メソッド．
        /// </summary>
        public static Rect Expand(this Rect rect, float expand)
        {
            return new Rect(
                rect.x - expand,
                rect.y - expand,
                rect.width + 2 * expand,
                rect.height + 2 * expand);
        }

        /// <summary>
        /// 左右方向にそれぞれ指定値だけ広げる拡張メソッド．
        /// </summary>
        public static Rect ExpandX(this Rect rect, float value)
        {
            rect.xMin -= value;
            rect.xMax += value;
            return rect;
        }

        /// <summary>
        /// 上下方向にそれぞれ指定値だけ広げる拡張メソッド．
        /// </summary>
        public static Rect ExpandY(this Rect rect, float value)
        {
            rect.yMin -= value;
            rect.yMax += value;
            return rect;
        }

        /// <summary>
        /// 指定座標を含むように広げる拡張メソッド．
        /// </summary>
        public static Rect ExpandTo(this Rect rect, Vector2 pos)
        {
            if (!rect.Contains(pos))
            {
                rect.xMin = Mathf.Min(rect.xMin, pos.x);
                rect.xMax = Mathf.Max(rect.xMax, pos.x);
                rect.yMin = Mathf.Min(rect.yMin, pos.y);
                rect.yMax = Mathf.Max(rect.yMax, pos.y);
            }

            return rect;
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Padding

        /// <summary>
        /// 全方向に指定値だけ縮小する拡張メソッド．
        /// </summary>
        public static Rect Padding(this Rect rect, float left, float right, float top, float bottom)
        {
            return new Rect(
                rect.x + left,
                rect.y + top,
                rect.width - left - right,
                rect.height - top - bottom);
        }

        /// <summary>
        /// 全方向に指定値だけ縮小する拡張メソッド．
        /// </summary>
        public static Rect Padding(this Rect rect, float horizontal, float vertical)
        {
            return new Rect(
                rect.x + horizontal,
                rect.y + vertical,
                rect.width - 2 * horizontal,
                rect.height - 2 * vertical);
        }

        /// <summary>
        /// 全方向に指定値だけ縮小する拡張メソッド．
        /// </summary>
        public static Rect Padding(this Rect rect, float padding)
        {
            return new Rect(
                rect.x + padding,
                rect.y + padding,
                rect.width - 2 * padding,
                rect.height - 2 * padding);
        }

        /// <summary>
        /// 全方向に指定値だけ縮小する拡張メソッド．
        /// </summary>
        // public static Rect Padding(this Rect rect, Padding padding)
        // {
        //     return new Rect(
        //         rect.x + padding.left,
        //         rect.y + padding.top,
        //         rect.width - padding.Width,
        //         rect.height - padding.Height);
        // }

        /// <summary>
        /// 水平方向にパディングを適用する拡張メソッド．
        /// </summary>
        public static Rect HorizontalPadding(this Rect rect, float padding)
        {
            return new Rect(rect.x + padding, rect.y, rect.width - 2 * padding, rect.height);
        }

        /// <summary>
        /// 水平方向にパディングを適用する拡張メソッド．
        /// </summary>
        public static Rect HorizontalPadding(this Rect rect, float left, float right)
        {
            return new Rect(rect.x + left, rect.y, rect.width - left - right, rect.height);
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Size Constraints


        /// <summary>
        /// 最大高さで制限する拡張メソッド．
        /// </summary>
        public static Rect MaxHeight(this Rect rect, float maxHeight)
        {
            if (rect.height > maxHeight)
            {
                rect.height = maxHeight;
            }

            return rect;
        }

        /// <summary>
        /// 最大幅で制限する拡張メソッド．
        /// </summary>
        public static Rect MaxWidth(this Rect rect, float maxWidth)
        {
            if (rect.width > maxWidth)
            {
                rect.width = maxWidth;
            }

            return rect;
        }

        /// <summary>
        /// 最小高さで制限する拡張メソッド．
        /// </summary>
        public static Rect MinHeight(this Rect rect, float minHeight)
        {
            if (rect.height < minHeight)
            {
                rect.height = minHeight;
            }

            return rect;
        }

        /// <summary>
        /// 最小幅で制限する拡張メソッド．
        /// </summary>
        public static Rect MinWidth(this Rect rect, float minWidth)
        {
            if (rect.width < minWidth)
            {
                rect.width = minWidth;
            }

            return rect;
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Splitting

        /// <summary>
        /// 左半分を取得する拡張メソッド．
        /// </summary>
        public static Rect LeftHalf(this Rect rect)
        {
            rect.width /= 2;
            return rect;
        }

        /// <summary>
        /// 右半分を取得する拡張メソッド．
        /// </summary>
        public static Rect RightHalf(this Rect rect)
        {
            rect.x += rect.width / 2;
            rect.width /= 2;
            return rect;
        }

        /// <summary>
        /// 上半分を取得する拡張メソッド．
        /// </summary>
        public static Rect TopHalf(this Rect rect)
        {
            rect.height /= 2;
            return rect;
        }

        /// <summary>
        /// 下半分を取得する拡張メソッド．
        /// </summary>
        public static Rect BottomHalf(this Rect rect)
        {
            rect.y += rect.height / 2;
            rect.height /= 2;
            return rect;
        }

        /// <summary>
        /// 水平方向に指定数で分割する拡張メソッド．
        /// </summary>
        public static Rect[] HorizontalSplit(this Rect rect, int count, float padding = 2f)
        {
            if (count < 1) 
                throw new System.InvalidOperationException("Count must be greater than 0.");

            var rects = new Rect[count];

            float totalPadding = padding * (count - 1); // ※要素間の隙間
            float width = (rect.width - totalPadding) / count;
            for (int i = 0; i < count; i++)
            {
                rects[i] = new Rect(
                    rect.x + (width + padding) * i,
                    rect.y,
                    width,
                    rect.height);
            }

            return rects;
        }

        /// <summary>
        /// 垂直方向に指定数で分割する拡張メソッド．
        /// </summary>
        public static Rect[] VerticalSplit(this Rect rect, int count, float padding = 2f)
        {
            if (count < 1) 
                throw new System.InvalidOperationException("Count must be greater than 0.");

            var rects = new Rect[count];

            var totalPadding = padding * (count - 1);
            var height = (rect.height - totalPadding) / count;
            for (int i = 0; i < count; i++)
            {
                rects[i] = new Rect(
                    rect.x,
                    rect.y + (height + padding) * i,
                    rect.width,
                    height);
            }

            return rects;
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Geometry

        /// <summary>
        /// 矩形の四隅の座標を取得する拡張メソッド．
        /// </summary>
        public static Vector2[] GetCorners(this Rect rect)
        {
            return new Vector2[]
            {
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.xMin, rect.yMax),
            };
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Utility

        /// <summary>
        /// プレースホルダーかどうかを判定する拡張メソッド．
        /// </summary>
        public static bool IsPlaceholder(this Rect rect)
        {
            return rect == new Rect(0, 0, 0, 0) || rect == new Rect(0, 0, 1, 1);
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region String Conversion

        /// <summary>
        /// x,yの値域を示す文字列へ変換する拡張メソッド．
        /// </summary>
        public static string ToStringAsRange(this Rect rect)
        {
            return string.Format(
                "X : [{0:F2} ~ {1:F2}], Y : [{2:F2} ~ {3:F2}]",
                rect.x,
                rect.x + rect.width,
                rect.y,
                rect.y + rect.height
            );
        }

        #endregion
    }
}