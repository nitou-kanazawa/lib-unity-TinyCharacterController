using System;

namespace Nitou {

    /// <summary>
    /// 上下左右の余白を表す構造体．
    /// </summary>
    public struct Padding {

        public float left;
        public float right;
        public float top;
        public float bottom;

        public float Width => left + right;
        public float Height => top + bottom;


        /// ----------------------------------------------------------------------------
        // Public Method

        public Padding(float value) {
            left = right = top = bottom = value;
        }

        public Padding(float horizontal, float vertical) {
            left = right = horizontal;
            top = bottom = vertical;
        }

        public Padding(float left, float right, float top, float bottom) {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }


        /// ----------------------------------------------------------------------------
        // Operator

        //public static Rect operator +(Rect rect, Padding padding) {
        //    rect.xMin -= padding.left;
        //    rect.xMax += padding.right;
        //    rect.yMin -= padding.top;
        //    rect.yMax += padding.bottom;
        //    return rect;
        //}
    }
}
