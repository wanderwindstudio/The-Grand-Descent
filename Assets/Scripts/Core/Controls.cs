using UnityEngine;
using UnityEngine.InputSystem;

// Named input actions. Bindings live in Assets/Settings/GrandDescentControls.inputactions
// (double-click it in Unity to edit the defaults). If that asset is missing, the same defaults are built here.
// Players rebind keys in Settings; their changes are stored as overrides in settings.json.
public static class Controls
{
    public static InputAction Fire, CastVortex, CastExplosion, Dash, Interact, Jump, Pause;

    // Every rebindable action, in the order the settings screen lists them.
    public static InputAction[] All => new[] { Fire, CastVortex, CastExplosion, Dash, Jump, Interact, Pause };

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
        // Older copies of the asset have no Pause action.
        if (map.FindAction("Pause") == null) map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");

        Fire = map.FindAction("Fire", true);
        CastVortex = map.FindAction("CastVortex", true);
        CastExplosion = map.FindAction("CastExplosion", true);
        Dash = map.FindAction("Dash", true);
        Interact = map.FindAction("Interact", true);
        Jump = map.FindAction("Jump", true);
        Pause = map.FindAction("Pause", true);
        map.Enable();
    }

    // The key shown on screen for an action, e.g. "Q".
    public static string KeyName(InputAction action) => action.GetBindingDisplayString();

    // Load the player's rebinds from settings.
    public static void ApplyOverrides()
    {
        if (map == null) return;
        map.RemoveAllBindingOverrides();
        string json = GameSettings.Current.bindingOverrides;
        if (!string.IsNullOrEmpty(json)) map.LoadBindingOverridesFromJson(json);
    }

    public static void StoreOverrides() => GameSettings.Current.bindingOverrides = map.SaveBindingOverridesAsJson();

    public static void ResetToDefaults()
    {
        map.RemoveAllBindingOverrides();
        StoreOverrides();
    }

    // Rebinding needs the actions switched off while it listens.
    public static void Disable() => map?.Disable();
    public static void Enable() => map?.Enable();

    public static string Label(InputAction action)
    {
        switch (action.name)
        {
            case "CastVortex": return "Cast Vortex";
            case "CastExplosion": return "Cast Explosion";
            default: return action.name;
        }
    }
}
