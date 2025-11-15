namespace Nitou.TCC.Inputs
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
        public void InitializeActions()
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
        }
    }
}