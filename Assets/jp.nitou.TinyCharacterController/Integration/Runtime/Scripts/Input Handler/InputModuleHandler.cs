using System.Collections.Generic;
using UnityEngine;

namespace Nitou.TCC.Integration
{
    /// <summary>
    /// InputModuleを対象とした入力ハンドラ．
    /// </summary>
    public sealed class InputModuleHandler : InputHandler
    {
        // アクションリスト
        private readonly Dictionary<string, Vector2Action> _vector2Actions = new();


        // ----------------------------------------------------------------------------
        // Override Method 

        /// <summary>
        /// Bool型アクションを取得する．
        /// </summary>
        public override bool GetBool(string actionName)
        {
            bool output = false;
            try
            {
                output = Input.GetButton(actionName);
            }
            catch (System.Exception)
            {
                PrintInputWarning(actionName);
            }

            return output;
        }

        /// <summary>
        /// Float型アクションを取得する．
        /// </summary>
        public override float GetFloat(string actionName)
        {
            float output = default;
            try
            {
                output = Input.GetAxis(actionName);
            }
            catch (System.Exception)
            {
                PrintInputWarning(actionName);
            }

            return output;
        }

        /// <summary>
        /// Vector2型アクションを取得する．
        /// </summary>
        public override Vector2 GetVector2(string actionName)
        {
            // Not officially supported	
            // Example : "Movement"  splits into "Movement X" and "Movement Y"

            bool isFound = _vector2Actions.TryGetValue(actionName, out var vector2Action);
            if (!isFound)
            {
                vector2Action = new Vector2Action(
                    string.Concat(actionName, " X"),
                    string.Concat(actionName, " Y")
                );

                _vector2Actions.Add(actionName, vector2Action);
            }

            Vector2 output = default;
            try
            {
                output = new Vector2(Input.GetAxis(vector2Action.x), Input.GetAxis(vector2Action.y));
            }
            catch (System.Exception)
            {
                PrintInputWarning(vector2Action.x, vector2Action.y);
            }

            return output;
        }


        // ----------------------------------------------------------------------------
        // Private Method 
        private static void PrintInputWarning(string actionName)
        {
            Debug.LogWarning($"{actionName} action not found! Please make sure this action is included in your input settings (axis). If you're only testing the demo scenes from " +
                              "Character Controller Pro please load the input preset included at \"Character Controller Pro/OPEN ME/Presets/.");
        }

        private static void PrintInputWarning(string actionXName, string actionYName)
        {
            Debug.LogWarning($"{actionXName} and/or {actionYName} actions not found! Please make sure both of these actions are included in your input settings (axis). If you're only testing the demo scenes from " +
                              "Character Controller Pro please load the input preset included at \"Character Controller Pro/OPEN ME/Presets/.");
        }


        // ----------------------------------------------------------------------------
        struct Vector2Action
        {
            public string x, y;

            public Vector2Action(string x, string y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}