#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// [参考]
//  コガネブログ: Dictionary型の変数定義が横に長くなるのが気になる https://baba-s.hatenablog.com/entry/2014/02/12/105154#google_vignette

namespace Nitou.EditorShared {

    // [TODO] いい感じの２キーDictionaryを整備したい (2024.08.01)
    // Rererence: https://www.foundations.unity.com/fundamentals/color-palette

    /// <summary>
    /// Unityエディタで使用されているカラー集
    /// </summary>
    public static class EditorColors {

        /// ----------------------------------------------------------------------------
        // Window Colors

        /// <summary>
        /// 背景色
        /// </summary>
        public static Color DefaultBackground =>
            GetColor(EditorGUIUtility.isProSkin ? "#282828" : "#A5A5A5");

        /// <summary>
        /// 背景色（非アクティブハイライト）
        /// </summary>
        public static Color HighlightBackgroundInactive =>
            GetColor(EditorGUIUtility.isProSkin ? "#4D4D4D" : "#AEAEAE");

        /// <summary>
        /// 背景色（ハイライト）
        /// </summary>
        public static Color HighlightBackground =>
            GetColor(EditorGUIUtility.isProSkin ? "#2C5D87" : "#3A72B0");

        /// <summary>
        /// 
        /// </summary>
        public static Color WindowBackground =>
            GetColor(EditorGUIUtility.isProSkin ? "#383838" : "#C8C8C8");

        /// <summary>
        /// 
        /// </summary>
        public static Color InspectorTitlebarBorder =>
            GetColor(EditorGUIUtility.isProSkin ? "#1A1A1A" : "#7F7F7F");


        /// ----------------------------------------------------------------------------
        // Button Colors

        /// <summary>
        /// ボタン背景色
        /// </summary>
        public static Color ButtonBackground =>
            GetColor(EditorGUIUtility.isProSkin ? "#585858" : "#E4E4E4");

        /// <summary>
        /// ボタン背景色（ホバー）
        /// </summary>
        public static Color ButtonBackgroundHover =>
            GetColor(EditorGUIUtility.isProSkin ? "#676767" : "#ECECEC");

        /// <summary>
        /// ボタン背景色（クリック）
        /// </summary>
        public static Color ButtonBackgroundHoverPressedr =>
            GetColor(EditorGUIUtility.isProSkin ? "#4F657F" : "#B0D2FC");


        /// ----------------------------------------------------------------------------
        // Text Colors

        /// <summary>
        /// テキスト色
        /// </summary>
        public static Color DefaultText =>
            GetColor(EditorGUIUtility.isProSkin ? "#D2D2D2" : "#090909");

        /// <summary>
        /// テキスト色（エラー）
        /// </summary>
        public static Color ErrorText =>
            GetColor(EditorGUIUtility.isProSkin ? "#D32222" : "#5A0000");

        /// <summary>
        /// テキスト色（警告）
        /// </summary>
        public static Color WarningText =>
            GetColor(EditorGUIUtility.isProSkin ? "#F4BC02" : "#333308");

        /// <summary>
        /// テキスト色（リンク）
        /// </summary>
        public static Color LinkText =>
            GetColor(EditorGUIUtility.isProSkin ? "#4C7EFF" : "#4C7EFF");



        public class ColorData {
            public readonly string light;
            public readonly string dark;

            public ColorData(string light, string dark) {
                this.light = light;
                this.dark = dark;
            }
        }


        public enum ColorType {
            /// ----------------------------------------------------------------------------
            // Base Layer 1

            // Application Bar
            AppToolbar_Background,
            AppToolbar_Button_Background,
            AppToolbar_Button_Background_Checked,
            AppToolbar_Button_Background_Hover,
            AppToolbar_Button_Background_Pressed,
            AppToolbar_Button_Border,
            AppToolbar_Button_BorderAccent,


            /// ----------------------------------------------------------------------------
            // Base Layer 2

            // Button Colors
            Button_Background,
            Button_Background_Focus,
            Button_Background_Hover,
            Button_Background_Hover_Pressed,
            Button_Background_Pressed,
            Button_Border,
            Button_Border_Accent,
            Button_Border_Accent_Focus,
            Button_Border_Pressed,
            Button_Text,

            // Dropdown
            Dropdown_Background,
            Dropdown_Background_Hover,
            Dropdown_Border,
            Dropdown_BorderAccent,
            Dropdown_Text,

            // Help Boxes
            Helpbox_Background,
            Helpbox_Border,
            Helpbox_Text,

            // Input Field
            InputField_Background,
            InputField_Border,
            InputField_BorderAccent,
            InputField_Border_Focus,
            InputField_Border_Hover,

            // Multi Column Header
            HeaderBar_Background,
            HeaderBar_Column_Background,
            HeaderBar_Column_Background_Hover,
            HeaderBar_Column_Background_Pressed,

            // Object Field
            ObjectField_Background,
            ObjectField_Border,
            ObjectField_Border_Focus,
            ObjectField_Border_Hover,
            ObjectField_Button_Background,
            ObjectField_Button_Background_Hover,

            // Inspector Backgrounds
            Inspector_Titlebar_Background,
            Inspector_Titlebar_Background_Hover,
            Toolbar_Background,

            // Inspector Borders
            Inspector_Titlebar_Border,
            Inspector_Titlebar_Border_Accent,

            // Window Backgrounds
            Default_Background,
            Highlight_Background_Inactive,
            Highlight_Background,
            Highlight_Background_Hover,
            Highlight_Background_Hover_Lighter,
            Window_Background,
            Alternated_Rows_Background,

            // Window Borders
            Default_Border,
            Toolbar_Border,
            Window_Border,


            /// ----------------------------------------------------------------------------
            // Base Layer 3

            // Toolbars
            Toolbar_Button_Background,
            Toolbar_Button_Background_Checked,
            Toolbar_Button_Background_Focus,
            Toolbar_Button_Background_Hover,
            Toolbar_Button_Border,


            /// ----------------------------------------------------------------------------
            // Text

            // Default Text Colors
            Default_Text,
            Default_Text_Hover,
            Error_Text,
            Link_Text,
            Visited_Link_Text,
            Warning_Text,

            // Windows and Component Text Colors
            Highlight_Text,
            Highlight_Text_Inactive,
            Label_Text,
            Label_Text_Focus,
            Preview_Overlay_Text,
            Window_Text,

            // Toolbar Text Colors
            Tab_Text,
            Toolbar_Button_Text,
            Toolbar_Button_Text_Checked,
            Toolbar_Button_Text_Hover,

        }

        private static readonly Dictionary<ColorType, ColorData> ColorHexCodes = new() {
            // Application Bar
            { ColorType.AppToolbar_Background, new ColorData("#8A8A8A", "#191919") },
            { ColorType.AppToolbar_Button_Background, new ColorData("#C8C8C8", "#383838") },
            { ColorType.AppToolbar_Button_Background_Checked, new ColorData("#B0B0B0", "#404040") },
            { ColorType.AppToolbar_Button_Background_Hover, new ColorData("#D0D0D0", "#484848") },
            { ColorType.AppToolbar_Button_Background_Pressed, new ColorData("#C0C0C0", "#505050") },
            { ColorType.AppToolbar_Button_Border, new ColorData("#A0A0A0", "#2E2E2E") },
            { ColorType.AppToolbar_Button_BorderAccent, new ColorData("#9A9A9A", "#2A2A2A") },

            // Button Colors
            { ColorType.Button_Background, new ColorData("#E4E4E4", "#585858") },
            { ColorType.Button_Background_Focus, new ColorData("#BEBEBE", "#6E6E6E") },
            { ColorType.Button_Background_Hover, new ColorData("#ECECEC", "#676767") },
            { ColorType.Button_Background_Hover_Pressed, new ColorData("#B0D2FC", "#4F657F") },
            { ColorType.Button_Background_Pressed, new ColorData("#96C3FB", "#46607C") },
            { ColorType.Button_Border, new ColorData("#B2B2B2", "#303030") },
            { ColorType.Button_Border_Accent, new ColorData("#939393", "#242424") },
            { ColorType.Button_Border_Accent_Focus, new ColorData("#018CFF", "#7BAEFA") },
            { ColorType.Button_Border_Pressed, new ColorData("#707070", "#0D0D0D") },
            { ColorType.Button_Text, new ColorData("#090909", "#EEEEEE") },

            // Dropdown
            { ColorType.Dropdown_Background, new ColorData("#DFDFDF", "#515151") },
            { ColorType.Dropdown_Background_Hover, new ColorData("#E4E4E4", "#585858") },
            { ColorType.Dropdown_Border, new ColorData("#B2B2B2", "#303030") },
            { ColorType.Dropdown_BorderAccent, new ColorData("#939393", "#242424") },
            { ColorType.Dropdown_Text, new ColorData("#090909", "#E4E4E4") },

            // Help Boxes
            { ColorType.Helpbox_Background, new ColorData("rgba(235, 235, 235, 0.2039216)", "rgba(96, 96, 96, 0.2039216)") },
            { ColorType.Helpbox_Border, new ColorData("#A9A9A9", "#232323") },
            { ColorType.Helpbox_Text, new ColorData("#161616", "#BDBDBD") },

            // // Input Field
            { ColorType.InputField_Background, new ColorData("#F0F0F0", "#2A2A2A") },
            { ColorType.InputField_Border, new ColorData("#B7B7B7", "#212121") },
            { ColorType.InputField_BorderAccent, new ColorData("#A0A0A0", "#0D0D0D") },
            { ColorType.InputField_Border_Focus, new ColorData("#1D5087", "#3A79BB") },
            { ColorType.InputField_Border_Hover, new ColorData("#6C6C6C", "#656565") },
            // Multi Column Header

            { ColorType.HeaderBar_Background, new ColorData("#CBCBCB", "#3C3C3C") },
            { ColorType.HeaderBar_Column_Background, new ColorData("#CBCBCB", "#3C3C3C") },
            { ColorType.HeaderBar_Column_Background_Hover, new ColorData("#C1C1C1", "#464646") },
            { ColorType.HeaderBar_Column_Background_Pressed, new ColorData("#EFEFEF", "#505050") },
            // Object Field

            { ColorType.ObjectField_Background, new ColorData("#EDEDED", "#282828") },
            { ColorType.ObjectField_Border, new ColorData("#B0B0B0", "#202020") },
            { ColorType.ObjectField_Border_Focus, new ColorData("#1D5087", "#3A79BB") },
            { ColorType.ObjectField_Border_Hover, new ColorData("#6C6C6C", "#656565") },
            { ColorType.ObjectField_Button_Background, new ColorData("#DEDEDE", "#373737") },
            { ColorType.ObjectField_Button_Background_Hover, new ColorData("#D4D4D4", "#3C3C3C") },
            // Inspector Backgrounds

            { ColorType.Inspector_Titlebar_Background, new ColorData("#F8F8F8", "#2B2B2B") },
            { ColorType.Inspector_Titlebar_Background_Hover, new ColorData("#F4F4F4", "#292929") },
            { ColorType.Toolbar_Background, new ColorData("#E8E8E8", "#3E3E3E") },
            // Inspector Borders

            { ColorType.Inspector_Titlebar_Border, new ColorData("#E0E0E0", "#1E1E1E") },
            { ColorType.Inspector_Titlebar_Border_Accent, new ColorData("#DCDCDC", "#1B1B1B") },

            // Window Backgrounds
            { ColorType.Default_Background, new ColorData("#F3F3F3", "#3F3F3F") },
            { ColorType.Highlight_Background_Inactive, new ColorData("#F1F1F1", "#3C3C3C") },
            { ColorType.Highlight_Background, new ColorData("#E9E9E9", "#353535") },
            { ColorType.Highlight_Background_Hover, new ColorData("#E5E5E5", "#333333") },
            { ColorType.Highlight_Background_Hover_Lighter, new ColorData("#E2E2E2", "#313131") },
        };


        /// ----------------------------------------------------------------------------
            // Private Method

            /// <summary>
            /// HTMLからRGBカラーへの変換
            /// </summary>
        private static Color GetColor(string htmlColor) {
            if (!ColorUtility.TryParseHtmlString(htmlColor, out var color))
                throw new ArgumentException();

            return color;
        }
    }
}
#endif