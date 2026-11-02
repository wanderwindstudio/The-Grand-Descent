using UnityEngine;
using UnityEngine.InputSystem;

// Named input actions. Bindings live in Assets/Settings/GrandDescentControls.inputactions
// (double-click it in Unity to rebind). If that asset is missing, the same defaults are built here.
public static class Controls
{
    public static InputAction Fire, CastVortex, CastExplosion, Dash, Interact, Jump;

    static InputActionMap map;

    public static void Init(InputActionAsset asset)
    {
        map?.Disable(); // from a previous scene load

        if (asset != null)
        {
            map = Object.Instantiate(asset).FindActionMap("Gameplay", throwIfNotFound: true);
        }
        else
        {
            map = new InputActionMap("Gameplay");
            map.AddAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
            map.AddAction("CastVortex", InputActionType.Button, "<Keyboard>/q");
            map.AddAction("CastExplosion", InputActionType.Button, "<Keyboard>/e");
            map.AddAction("Dash", InputActionType.Button, "<Keyboard>/leftShift");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/f");
            map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
        }

        Fire = map.FindAction("Fire", true);
        CastVortex = map.FindAction("CastVortex", true);
        CastExplosion = map.FindAction("CastExplosion", true);
        Dash = map.FindAction("Dash", true);
        Interact = map.FindAction("Interact", true);
        Jump = map.FindAction("Jump", true);
        map.Enable();
    }

    // The key shown on screen for an action, e.g. "Q".
    public static string KeyName(InputAction action) => action.GetBindingDisplayString();
}
