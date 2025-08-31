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
                Debug.Log("No input actions asset found!");
                return;
            }

            inputActionsAsset.Enable();

            // Control Scheme �ɂ��t�B���^�����O
            if (_filterByControlScheme)
            {
                string bindingGroup = inputActionsAsset.controlSchemes.First(x => x.name == _controlSchemeName).bindingGroup;
                inputActionsAsset.bindingMask = InputBinding.MaskByGroup(bindingGroup);
            }

            // Action Map �ɂ��t�B���^�����O
            if (_filterByActionMap)
            {
                var rawInputActions = inputActionsAsset.FindActionMap(_gameplayActionMap).actions;

                foreach (var action in rawInputActions)
                {
                    _inputActionsDictionary.Add(action.name, action);
                }
            }
            else
            {
                for (int i = 0; i < inputActionsAsset.actionMaps.Count; i++)
                {
                    var actionMap = inputActionsAsset.actionMaps[i];

                    foreach (var action in actionMap.actions)
                    {
                        _inputActionsDictionary.Add(action.name, action);
                    }
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