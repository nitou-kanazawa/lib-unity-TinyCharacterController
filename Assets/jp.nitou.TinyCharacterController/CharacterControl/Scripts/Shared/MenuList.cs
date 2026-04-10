namespace Nitou.TCC.CharacterControl.Shared
{
    /// <summary>
    /// エディタメニュー用の文字列を管理．
    /// </summary>
    internal static class MenuList
    {
        private const string Header = "TCC/";

        // 主要コンポ
        public const string MenuBrain = Header + "Brain/";
        public const string MenuControl = Header + "Control/";
        public const string MenuCheck = Header + "Check/";
        public const string MenuEffect = Header + "Effect/";
        
        // その他
        public const string Gimmick = Header + "Gimmick/";
        public const string Ik = Header + "IK/";
        public const string Utility = Header + "Utility/";
        public const string Ui = Header + "UI/";
    }
}