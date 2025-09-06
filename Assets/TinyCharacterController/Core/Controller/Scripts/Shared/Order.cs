
namespace Nitou.TCC.Controller.Shared {

    public static class Order {
        public const int GameObjectPool = -1000;
        public const int IndicatorRegister = -100;

        // ----------------------------------
        // early update
        // ----------------------------------

        // Prepare Process
        public const int PrepareEarlyUpdate = -1000;

        // ----------------------------------
        // update
        // ----------------------------------

        public const int Check = -200;
        public const int Gravity = -100;

        // ���Ȋ���
        public const int Effect = -50;
        // Control
        public const int Control = 5;

        // ----------------------------------
        // �v�Z���ʂ𔽉f ( ExecuteOrder )
        // ----------------------------------

        public const int EarlyUpdateBrain = -101;   // InputSystem���O
        public const int UpdateBrain = 10;          // Brain�̍X�V�^�C�~���O
        public const int PostUpdate = 100;          // Brain�̌�
        public const int UpdateIK = 50;             // Ik�̍X�V�^�C�~���O
    }
}
