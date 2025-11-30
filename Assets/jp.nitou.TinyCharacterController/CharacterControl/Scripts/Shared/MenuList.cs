namespace Nitou.TCC.CharacterControl.Shared
{
    /// <summary>
    /// エディタメニュー用の文字列を管理．
    /// </summary>
    internal static class MenuList
    {
        // 
        private const string MenuHeader = "TCC/";

        // 
        public const string MenuBrain = MenuHeader + "Brain/";
        public const string MenuControl = MenuHeader + "Control/";
        public const string MenuCheck = MenuHeader + "Check/";
        public const string MenuEffect = MenuHeader + "Effect/";
        public const string Gimmick = MenuHeader + "Gimmick/";
        public const string Ik = MenuHeader + "IK/";
        public const string Utility = MenuHeader + "Utility/";
        public const string Ui = MenuHeader + "UI/";
    }
}