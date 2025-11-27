#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nitou.TCC.Integration.EditorScripts
{
    /// <summary>
    /// モダンなC#スタイルでActorActions.csを生成するジェネレータ
    /// </summary>
    internal class ActorActionsGenerator
    {
        private readonly string[] _boolActions;
        private readonly string[] _floatActions;
        private readonly string[] _vector2Actions;
        private readonly bool _enableInputBuffer;
        private readonly string _className;

        // インデント定数
        private const string TAB = "    ";
        private const string TAB2 = TAB + TAB;
        private const string TAB3 = TAB2 + TAB;


        // ----------------------------------------------------------------------------
        // Constructor

        public ActorActionsGenerator(
            string[] boolActions,
            string[] floatActions,
            string[] vector2Actions,
            string className = "ActorActions",
            bool enableInputBuffer = true)
        {
            _boolActions = boolActions ?? Array.Empty<string>();
            _floatActions = floatActions ?? Array.Empty<string>();
            _vector2Actions = vector2Actions ?? Array.Empty<string>();
            _className = className;
            _enableInputBuffer = enableInputBuffer;
        }


        // ----------------------------------------------------------------------------
        // Public Methods

        /// <summary>
        /// 完全なActorActions.csコードを生成
        /// </summary>
        public string Generate()
        {
            var sb = new StringBuilder(4096);

            GenerateHeader(sb);
            GenerateStructDeclaration(sb);
            GenerateFieldDefinitions(sb);
            GenerateResetMethod(sb);
            GenerateInitializeMethod(sb);
            GenerateSetValuesFromHandlerMethod(sb);
            GenerateSetValuesFromActionsMethod(sb);
            GenerateUpdateMethod(sb);

            if (_enableInputBuffer)
            {
                GenerateBufferHelperMethods(sb);
            }

            GenerateStructEnd(sb);

            return sb.ToString();
        }


        // ----------------------------------------------------------------------------
        // Private Methods (Code Generation)

        private void GenerateHeader(StringBuilder sb)
        {
            sb.AppendLine("namespace Nitou.TCC.Integration");
            sb.AppendLine("{");
        }

        private void GenerateStructDeclaration(StringBuilder sb)
        {
            sb.AppendLine($"{TAB}/// <summary>");
            sb.AppendLine($"{TAB}/// This struct contains all the inputs actions available for the character to interact with.");
            sb.AppendLine($"{TAB}/// </summary>");
            sb.AppendLine($"{TAB}[System.Serializable]");
            sb.AppendLine($"{TAB}public struct {_className}");
            sb.AppendLine($"{TAB}{{");
        }

        private void GenerateFieldDefinitions(StringBuilder sb)
        {
            // Bool actions
            if (_boolActions.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"{TAB2}// Bool actions");
                foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB2}public BoolAction {variableName};");
                }
            }

            // Float actions
            if (_floatActions.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"{TAB2}// Float actions");
                foreach (var actionName in _floatActions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB2}public FloatAction {variableName};");
                }
            }

            // Vector2 actions
            if (_vector2Actions.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"{TAB2}// Vector2 actions");
                foreach (var actionName in _vector2Actions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB2}public Vector2Action {variableName};");
                }
            }

            // Input buffer field (if enabled)
            if (_enableInputBuffer)
            {
                sb.AppendLine();
                sb.AppendLine($"{TAB2}// 入力バッファ（オプション機能）");
                sb.AppendLine($"{TAB2}private InputBuffer _buffer;");
            }

            sb.AppendLine();
        }

        private void GenerateResetMethod(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// Reset all the actions.");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}public void Reset()");
            sb.AppendLine($"{TAB2}{{");

            foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName}.Reset();");
            }

            foreach (var actionName in _floatActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName}.Reset();");
            }

            foreach (var actionName in _vector2Actions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName}.Reset();");
            }

            sb.AppendLine($"{TAB2}}}");
        }

        private void GenerateInitializeMethod(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// Initializes all the actions by instantiate them. Each action will be instantiated with its specific type (Bool, Float or Vector2).");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}/// <param name=\"enableBuffering\">入力バッファリングを有効にするか</param>");
            sb.AppendLine($"{TAB2}/// <param name=\"bufferDuration\">バッファ保持時間（秒）</param>");
            sb.AppendLine($"{TAB2}public void InitializeActions(bool enableBuffering = false, float bufferDuration = 0.2f)");
            sb.AppendLine($"{TAB2}{{");

            // Bool actions
            foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName} = new BoolAction();");
                sb.AppendLine($"{TAB3}{variableName}.Initialize();");
                sb.AppendLine();
            }

            // Float actions
            foreach (var actionName in _floatActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName} = new FloatAction();");
                sb.AppendLine($"{TAB3}{variableName}.Initialize();");
                sb.AppendLine();
            }

            // Vector2 actions
            foreach (var actionName in _vector2Actions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName} = new Vector2Action();");
                sb.AppendLine($"{TAB3}{variableName}.Initialize();");
                sb.AppendLine();
            }

            // Input buffer initialization
            if (_enableInputBuffer)
            {
                sb.AppendLine($"{TAB3}// 入力バッファの初期化（オプション）");
                sb.AppendLine($"{TAB3}if (enableBuffering)");
                sb.AppendLine($"{TAB3}{{");
                sb.AppendLine($"{TAB3}{TAB}_buffer = new InputBuffer(bufferDuration);");
                sb.AppendLine($"{TAB3}}}");
            }

            sb.AppendLine($"{TAB2}}}");
        }

        private void GenerateSetValuesFromHandlerMethod(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// Updates the values of all the actions based on the current input handler (human).");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}public void SetValues(InputHandler inputHandler)");
            sb.AppendLine($"{TAB2}{{");
            sb.AppendLine($"{TAB3}if (inputHandler == null) return;");
            sb.AppendLine();

            // Bool actions
            foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName}.value = inputHandler.GetBool(\"{actionName}\");");
            }

            // Float actions
            if (_floatActions.Length > 0)
            {
                sb.AppendLine();
                foreach (var actionName in _floatActions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB3}{variableName}.value = inputHandler.GetFloat(\"{actionName}\");");
                }
            }

            // Vector2 actions
            if (_vector2Actions.Length > 0)
            {
                sb.AppendLine();
                foreach (var actionName in _vector2Actions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB3}{variableName}.value = inputHandler.GetVector2(\"{actionName}\");");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"{TAB2}}}");
        }

        private void GenerateSetValuesFromActionsMethod(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// Copies the values of all the actions from an existing set of actions.");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}public void SetValues({_className} characterActions)");
            sb.AppendLine($"{TAB2}{{");

            // Bool actions
            foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                var propertyName = variableName.TrimStart('@');
                sb.AppendLine($"{TAB3}{variableName}.value = characterActions.{propertyName}.value;");
            }

            // Float actions
            if (_floatActions.Length > 0)
            {
                foreach (var actionName in _floatActions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    var propertyName = variableName.TrimStart('@');
                    sb.AppendLine($"{TAB3}{variableName}.value = characterActions.{propertyName}.value;");
                }
            }

            // Vector2 actions
            if (_vector2Actions.Length > 0)
            {
                foreach (var actionName in _vector2Actions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    var propertyName = variableName.TrimStart('@');
                    sb.AppendLine($"{TAB3}{variableName}.value = characterActions.{propertyName}.value;");
                }
            }

            sb.AppendLine($"{TAB2}}}");
        }

        private void GenerateUpdateMethod(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// Update all the actions internal states.");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}public void Update(float dt)");
            sb.AppendLine($"{TAB2}{{");

            // Bool actions
            foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                sb.AppendLine($"{TAB3}{variableName}.Update(dt);");
            }

            // Float actions
            if (_floatActions.Length > 0)
            {
                foreach (var actionName in _floatActions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB3}{variableName}.Update(dt);");
                }
            }

            // Vector2 actions
            if (_vector2Actions.Length > 0)
            {
                foreach (var actionName in _vector2Actions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB3}{variableName}.Update(dt);");
                }
            }

            // Input buffer recording
            if (_enableInputBuffer)
            {
                sb.AppendLine();
                sb.AppendLine($"{TAB3}// 入力バッファの更新と記録");
                sb.AppendLine($"{TAB3}if (_buffer != null)");
                sb.AppendLine($"{TAB3}{{");
                sb.AppendLine($"{TAB3}{TAB}_buffer.Update();");
                sb.AppendLine();

                sb.AppendLine($"{TAB3}{TAB}// Started/Canceledイベントをバッファに記録");

                foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var variableName = ToVariableName(actionName);
                    sb.AppendLine($"{TAB3}{TAB}if ({variableName}.Started) _buffer.RecordPressed(\"{actionName}\");");
                    sb.AppendLine($"{TAB3}{TAB}if ({variableName}.Canceled) _buffer.RecordReleased(\"{actionName}\");");
                    sb.AppendLine();
                }

                sb.AppendLine($"{TAB3}}}");
            }

            sb.AppendLine($"{TAB2}}}");
        }

        private void GenerateBufferHelperMethods(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"{TAB2}// ----------------------------------------------------------------------------");
            sb.AppendLine($"{TAB2}// Input Buffer Helper Methods");
            sb.AppendLine();

            // Generate WasXPressedRecently methods for each bool action
            foreach (var actionName in _boolActions.Where(x => !string.IsNullOrEmpty(x)))
            {
                var variableName = ToVariableName(actionName);
                var propertyName = variableName.TrimStart('@');
                var methodName = $"Was{ToPascalCase(actionName)}PressedRecently";

                sb.AppendLine($"{TAB2}/// <summary>");
                sb.AppendLine($"{TAB2}/// 指定時間内に{actionName}が押されたかチェック（先行入力）");
                sb.AppendLine($"{TAB2}/// </summary>");
                sb.AppendLine($"{TAB2}public bool {methodName}(float withinTime = 0.2f)");
                sb.AppendLine($"{TAB3}=> _buffer?.WasPressed(\"{actionName}\", withinTime) ?? false;");
                sb.AppendLine();
            }

            // Generate common helper methods
            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// コマンドシーケンスを検出");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}/// <param name=\"sequence\">アクション名の配列（例: [\"Attack1\", \"Attack1\", \"Attack2\"]）</param>");
            sb.AppendLine($"{TAB2}/// <param name=\"maxDuration\">シーケンス全体の許容時間（秒）</param>");
            sb.AppendLine($"{TAB2}public bool DetectCommandSequence(string[] sequence, float maxDuration = 1.0f)");
            sb.AppendLine($"{TAB3}=> _buffer?.DetectSequence(sequence, maxDuration) ?? false;");
            sb.AppendLine();

            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// バッファをクリア");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}public void ClearBuffer()");
            sb.AppendLine($"{TAB3}=> _buffer?.Clear();");
            sb.AppendLine();

            sb.AppendLine($"{TAB2}/// <summary>");
            sb.AppendLine($"{TAB2}/// バッファのデバッグ情報を取得");
            sb.AppendLine($"{TAB2}/// </summary>");
            sb.AppendLine($"{TAB2}public string GetBufferDebugInfo()");
            sb.AppendLine($"{TAB3}=> _buffer?.GetDebugInfo() ?? \"Buffer not initialized\";");
        }

        private void GenerateStructEnd(StringBuilder sb)
        {
            sb.AppendLine($"{TAB}}}");
            sb.AppendLine("}");
        }


        // ----------------------------------------------------------------------------
        // Helper Methods

        /// <summary>
        /// アクション名を変数名に変換（@jumpのような形式）
        /// 例: "Jump" -> "@jump", "Attack 1" -> "@attack1"
        /// </summary>
        private static string ToVariableName(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                return string.Empty;

            var words = actionName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            sb.Append('@');

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (i == 0)
                {
                    // 最初の単語は全て小文字
                    sb.Append(char.ToLowerInvariant(word[0]));
                    if (word.Length > 1)
                        sb.Append(word.Substring(1).ToLower());
                }
                else
                {
                    // 2番目以降の単語は先頭を大文字、残りを小文字
                    sb.Append(char.ToUpperInvariant(word[0]));
                    if (word.Length > 1)
                        sb.Append(word.Substring(1).ToLower());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// アクション名をPascalCaseに変換（メソッド名用）
        /// 例: "Attack 1" -> "Attack1", "lock on" -> "LockOn"
        /// </summary>
        private static string ToPascalCase(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                return string.Empty;

            var words = actionName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var word in words)
            {
                sb.Append(char.ToUpperInvariant(word[0]));
                if (word.Length > 1)
                    sb.Append(word.Substring(1).ToLower());
            }

            return sb.ToString();
        }
    }
}
#endif
