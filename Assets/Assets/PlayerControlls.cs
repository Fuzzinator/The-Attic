// GENERATED AUTOMATICALLY FROM 'Assets/PlayerControlls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerControlls : IInputActionCollection
{
    private InputActionAsset asset;
    public PlayerControlls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControlls"",
    ""maps"": [
        {
            ""name"": ""Main"",
            ""id"": ""5aa430b1-a7ef-482c-a849-a0343f83ffe2"",
            ""actions"": [
                {
                    ""name"": ""Interact"",
                    ""id"": ""e7b6f643-d3ed-4c11-b97d-57e1ea47df5a"",
                    ""expectedControlLayout"": ""Button"",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""bindings"": []
                },
                {
                    ""name"": ""Crouch"",
                    ""id"": ""e22f90e9-58c3-485c-ade0-0083a12c84c2"",
                    ""expectedControlLayout"": ""Button"",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""bindings"": []
                },
                {
                    ""name"": ""DropItem"",
                    ""id"": ""e09d0cd0-0ddd-4035-aa0d-e0085e3eaef9"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""bindings"": []
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""7f1771f9-1ffa-4836-8919-31d599c5bd59"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""f6e98ad8-9795-404b-a72a-0934f2729216"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""ede2b77a-0fb5-4c0f-88e8-47658cd92ccb"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""cd122930-bb35-4632-8ca0-d7e942d86691"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""221069d2-6be1-4913-bb8e-749cb9728ca6"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""c2f0c7dc-573a-4e94-9007-63926fb8f53d"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""eab2126e-e5d7-40d4-ae59-4b5fc8d2bbee"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DropItem"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""5c2f1a1c-16c1-4dc8-a3ae-0dff4cf13cbe"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DropItem"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Main
        m_Main = asset.GetActionMap("Main");
        m_Main_Interact = m_Main.GetAction("Interact");
        m_Main_Crouch = m_Main.GetAction("Crouch");
        m_Main_DropItem = m_Main.GetAction("DropItem");
    }

    ~PlayerControlls()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes
    {
        get => asset.controlSchemes;
    }

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Main
    private InputActionMap m_Main;
    private IMainActions m_MainActionsCallbackInterface;
    private InputAction m_Main_Interact;
    private InputAction m_Main_Crouch;
    private InputAction m_Main_DropItem;
    public struct MainActions
    {
        private PlayerControlls m_Wrapper;
        public MainActions(PlayerControlls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Interact { get { return m_Wrapper.m_Main_Interact; } }
        public InputAction @Crouch { get { return m_Wrapper.m_Main_Crouch; } }
        public InputAction @DropItem { get { return m_Wrapper.m_Main_DropItem; } }
        public InputActionMap Get() { return m_Wrapper.m_Main; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(MainActions set) { return set.Get(); }
        public void SetCallbacks(IMainActions instance)
        {
            if (m_Wrapper.m_MainActionsCallbackInterface != null)
            {
                Interact.started -= m_Wrapper.m_MainActionsCallbackInterface.OnInteract;
                Interact.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnInteract;
                Interact.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnInteract;
                Crouch.started -= m_Wrapper.m_MainActionsCallbackInterface.OnCrouch;
                Crouch.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnCrouch;
                Crouch.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnCrouch;
                DropItem.started -= m_Wrapper.m_MainActionsCallbackInterface.OnDropItem;
                DropItem.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnDropItem;
                DropItem.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnDropItem;
            }
            m_Wrapper.m_MainActionsCallbackInterface = instance;
            if (instance != null)
            {
                Interact.started += instance.OnInteract;
                Interact.performed += instance.OnInteract;
                Interact.canceled += instance.OnInteract;
                Crouch.started += instance.OnCrouch;
                Crouch.performed += instance.OnCrouch;
                Crouch.canceled += instance.OnCrouch;
                DropItem.started += instance.OnDropItem;
                DropItem.performed += instance.OnDropItem;
                DropItem.canceled += instance.OnDropItem;
            }
        }
    }
    public MainActions @Main
    {
        get
        {
            return new MainActions(this);
        }
    }
    public interface IMainActions
    {
        void OnInteract(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
        void OnDropItem(InputAction.CallbackContext context);
    }
}
