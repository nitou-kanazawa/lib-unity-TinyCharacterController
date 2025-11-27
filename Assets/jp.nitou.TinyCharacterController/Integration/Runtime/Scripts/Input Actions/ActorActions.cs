namespace Nitou.TCC.Integration
{
    /// <summary>
    /// This struct contains all the inputs actions available for the character to interact with.
    /// </summary>
    [System.Serializable]
    public struct ActorActions
    {

        // Bool actions
        public BoolAction @jump;
        public BoolAction @attack1;
        public BoolAction @attack2;
        public BoolAction @lockon;
        public BoolAction @interact;
        public BoolAction @dodge;
        public BoolAction @guard;
        public BoolAction @run;


        // Float actions
        public FloatAction @pitch;
        public FloatAction @roll;


        // Vector2 actions
        public Vector2Action @movement;


        // 入力バッファ（オプション機能）
        private InputBuffer _buffer;



        /// <summary>
        /// Reset all the actions.
        /// </summary>
        public void Reset()
        {
            @jump.Reset();
            @attack1.Reset();
            @attack2.Reset();
            @lockon.Reset();
            @interact.Reset();
            @dodge.Reset();
            @guard.Reset();
            @run.Reset();
            @pitch.Reset();
            @roll.Reset();
            @movement.Reset();
        }

        /// <summary>
        /// Initializes all the actions by instantiate them. Each action will be instantiated with its specific type (Bool, Float or Vector2).
        /// </summary>
        /// <param name="enableBuffering">入力バッファリングを有効にするか</param>
        /// <param name="bufferDuration">バッファ保持時間（秒）</param>
        public void InitializeActions(bool enableBuffering = false, float bufferDuration = 0.2f)
        {
            @jump = new BoolAction();
            @jump.Initialize();

            @attack1 = new BoolAction();
            @attack1.Initialize();

            @attack2 = new BoolAction();
            @attack2.Initialize();

            @lockon = new BoolAction();
            @lockon.Initialize();

            @interact = new BoolAction();
            @interact.Initialize();

            @dodge = new BoolAction();
            @dodge.Initialize();

            @guard = new BoolAction();
            @guard.Initialize();

            @run = new BoolAction();
            @run.Initialize();


            @pitch = new FloatAction();
            @pitch.Initialize();

            @roll = new FloatAction();
            @roll.Initialize();

            @movement = new Vector2Action();
            @movement.Initialize();

            // 入力バッファの初期化（オプション）
            if (enableBuffering)
            {
                _buffer = new InputBuffer(bufferDuration);
            }
        }

        /// <summary>
        /// Updates the values of all the actions based on the current input handler (human).
        /// </summary>
        public void SetValues(InputHandler inputHandler)
        {
            if (inputHandler == null) return;

            @jump.value = inputHandler.GetBool("Jump");
            @attack1.value = inputHandler.GetBool("Attack1");
            @attack2.value = inputHandler.GetBool("Attack2");
            @lockon.value = inputHandler.GetBool("LockOn");
            @interact.value = inputHandler.GetBool("Interact");
            @dodge.value = inputHandler.GetBool("Dodge");
            @guard.value = inputHandler.GetBool("Guard");
            @run.value = inputHandler.GetBool("Run");

            @pitch.value = inputHandler.GetFloat("Pitch");
            @roll.value = inputHandler.GetFloat("Roll");

            @movement.value = inputHandler.GetVector2("Movement");

        }

        /// <summary>
        /// Copies the values of all the actions from an existing set of actions.
        /// </summary>
        public void SetValues(ActorActions characterActions)
        {
            @jump.value = characterActions.jump.value;
            @attack1.value = characterActions.attack1.value;
            @attack2.value = characterActions.attack2.value;
            @lockon.value = characterActions.lockon.value;
            @interact.value = characterActions.interact.value;
            @dodge.value = characterActions.dodge.value;
            @guard.value = characterActions.guard.value;
            @run.value = characterActions.run.value;

            @pitch.value = characterActions.pitch.value;
            @roll.value = characterActions.roll.value;
            @movement.value = characterActions.movement.value;
        }

        /// <summary>
        /// Update all the actions internal states.
        /// </summary>
        public void Update(float dt)
        {
            @jump.Update(dt);
            @attack1.Update(dt);
            @attack2.Update(dt);
            @lockon.Update(dt);
            @interact.Update(dt);
            @dodge.Update(dt);
            @guard.Update(dt);
            @run.Update(dt);

            @pitch.Update(dt);
            @roll.Update(dt);

            @movement.Update(dt);

            // 入力バッファの更新と記録
            if (_buffer != null)
            {
                _buffer.Update();

                // Started/Canceledイベントをバッファに記録
                if (@jump.Started) _buffer.RecordPressed("Jump");
                if (@jump.Canceled) _buffer.RecordReleased("Jump");

                if (@attack1.Started) _buffer.RecordPressed("Attack1");
                if (@attack1.Canceled) _buffer.RecordReleased("Attack1");

                if (@attack2.Started) _buffer.RecordPressed("Attack2");
                if (@attack2.Canceled) _buffer.RecordReleased("Attack2");

                if (@dodge.Started) _buffer.RecordPressed("Dodge");
                if (@dodge.Canceled) _buffer.RecordReleased("Dodge");

                if (@guard.Started) _buffer.RecordPressed("Guard");
                if (@guard.Canceled) _buffer.RecordReleased("Guard");

                if (@run.Started) _buffer.RecordPressed("Run");
                if (@run.Canceled) _buffer.RecordReleased("Run");
            }
        }


        // ----------------------------------------------------------------------------
        // Input Buffer Helper Methods

        /// <summary>
        /// 指定時間内にアクションが押されたかチェック（先行入力）
        /// </summary>
        public bool WasJumpPressedRecently(float withinTime = 0.2f)
            => _buffer?.WasPressed("Jump", withinTime) ?? false;

        /// <summary>
        /// 指定時間内に攻撃1が押されたかチェック
        /// </summary>
        public bool WasAttack1PressedRecently(float withinTime = 0.2f)
            => _buffer?.WasPressed("Attack1", withinTime) ?? false;

        /// <summary>
        /// 指定時間内に攻撃2が押されたかチェック
        /// </summary>
        public bool WasAttack2PressedRecently(float withinTime = 0.2f)
            => _buffer?.WasPressed("Attack2", withinTime) ?? false;

        /// <summary>
        /// 指定時間内に回避が押されたかチェック
        /// </summary>
        public bool WasDodgePressedRecently(float withinTime = 0.2f)
            => _buffer?.WasPressed("Dodge", withinTime) ?? false;

        /// <summary>
        /// コマンドシーケンスを検出
        /// </summary>
        /// <param name="sequence">アクション名の配列（例: ["Attack1", "Attack1", "Attack2"]）</param>
        /// <param name="maxDuration">シーケンス全体の許容時間（秒）</param>
        public bool DetectCommandSequence(string[] sequence, float maxDuration = 1.0f)
            => _buffer?.DetectSequence(sequence, maxDuration) ?? false;

        /// <summary>
        /// バッファをクリア
        /// </summary>
        public void ClearBuffer()
            => _buffer?.Clear();

        /// <summary>
        /// バッファのデバッグ情報を取得
        /// </summary>
        public string GetBufferDebugInfo()
            => _buffer?.GetDebugInfo() ?? "Buffer not initialized";
    }
}