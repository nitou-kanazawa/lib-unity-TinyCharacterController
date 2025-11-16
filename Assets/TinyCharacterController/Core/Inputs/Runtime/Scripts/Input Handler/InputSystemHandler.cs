using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// <see cref="InputSystem"/>��ΏۂƂ������̓n���h���D
    /// </summary>
    public sealed class InputSystemHandler : InputHandler
    {
        // �A�N�V�������X�g
        [SerializeField] InputActionAsset inputActionsAsset = null;
        
        private readonly Dictionary<string, InputAction> _inputActionsDictionary = new();

        [Space]

        // Action Map �ɂ��t�B���^�����O
        [SerializeField]
        private bool _filterByActionMap = false;

        [ShowIf("_filterByActionMap")] [LabelText("Map name")] [SerializeField, Indent]
        private string _gameplayActionMap = "Gameplay";

        // Control Scheme �ɂ��t�B���^�����O
        [SerializeField] private bool _filterByControlScheme = false;

        [ShowIf("_filterByControlScheme")] [LabelText("Scheme name")] [SerializeField, Indent]
        private string _controlSchemeName = "Keyboard Mouse";


        // ----------------------------------------------------------------------------
        // Lifecycle Events
        private void Awake()
        {
            if (inputActionsAsset == null)
            {
                Debug.LogError($"[{nameof(InputSystemHandler)}] No input actions asset assigned!", this);
                return;
            }

            inputActionsAsset.Enable();

            // Control Scheme によるフィルタリング（安全版）
            if (_filterByControlScheme)
            {
                var scheme = inputActionsAsset.controlSchemes
                    .FirstOrDefault(x => x.name == _controlSchemeName);

                if (!string.IsNullOrEmpty(scheme.bindingGroup))
                {
                    inputActionsAsset.bindingMask = InputBinding.MaskByGroup(scheme.bindingGroup);
                }
                else
                {
                    Debug.LogError($"[{nameof(InputSystemHandler)}] Control scheme '{_controlSchemeName}' not found!", this);
                }
            }

            // Action Map によるフィルタリング（重複検出付き）
            if (_filterByActionMap)
            {
                var actionMap = inputActionsAsset.FindActionMap(_gameplayActionMap);
                if (actionMap != null)
                {
                    RegisterActions(actionMap.actions, _gameplayActionMap);
                }
                else
                {
                    Debug.LogError($"[{nameof(InputSystemHandler)}] Action map '{_gameplayActionMap}' not found!", this);
                }
            }
            else
            {
                foreach (var actionMap in inputActionsAsset.actionMaps)
                {
                    RegisterActions(actionMap.actions, actionMap.name);
                }
            }
        }

        /// <summary>
        /// アクションを辞書に登録（重複チェック付き）
        /// </summary>
        private void RegisterActions(IEnumerable<InputAction> actions, string actionMapName)
        {
            foreach (var action in actions)
            {
                if (!_inputActionsDictionary.ContainsKey(action.name))
                {
                    _inputActionsDictionary.Add(action.name, action);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(InputSystemHandler)}] Duplicate action name '{action.name}' found in action map '{actionMapName}'. Skipping.", this);
                }
            }
        }


        // ----------------------------------------------------------------------------
        // Override Method 

        /// <summary>
        /// Bool�^�A�N�V�����̎擾
        /// </summary>
        public override bool GetBool(string actionName)
        {
            if (!_inputActionsDictionary.TryGetValue(actionName, out var inputAction))
            {
                return false;
            }

            return inputAction.ReadValue<float>() >= InputSystem.settings.defaultButtonPressPoint;
        }

        /// <summary>
        /// Float�^�A�N�V�����̎擾
        /// </summary>
        public override float GetFloat(string actionName)
        {
            if (!_inputActionsDictionary.TryGetValue(actionName, out var inputAction))
            {
                return 0f;
            }

            return inputAction.ReadValue<float>();
        }

        /// <summary>
        /// Vector2�^�A�N�V�����̎擾
        /// </summary>
        public override Vector2 GetVector2(string actionName)
        {
            if (!_inputActionsDictionary.TryGetValue(actionName, out var inputAction))
            {
                return Vector2.zero;
            }

            return inputAction.ReadValue<Vector2>();
        }
    }
}