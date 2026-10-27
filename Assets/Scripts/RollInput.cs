using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class RollInput : MonoBehaviour
{
    private const string BindingKey = "DiceGame.RollBinding";
    private const string DefaultBinding = "<Keyboard>/space";

    [SerializeField] private InputAction roll = new("Roll", InputActionType.Button, DefaultBinding);

    private InputActionRebindingExtensions.RebindingOperation rebinding;

    public event Action RollRequested;
    public event Action BindingChanged;

    public bool IsRebinding => rebinding != null;
    public string BindingLabel => roll.GetBindingDisplayString(0);

    private void Awake()
    {
        roll.RemoveBindingOverride(0);
        if (IsPointerBinding(roll.bindings[0].path))
            roll.ChangeBinding(0).WithPath(DefaultBinding);

        var path = PlayerPrefs.GetString(BindingKey);
        if (IsPointerBinding(path))
        {
            PlayerPrefs.DeleteKey(BindingKey);
            PlayerPrefs.Save();
            return;
        }

        if (!string.IsNullOrEmpty(path))
            roll.ApplyBindingOverride(0, path);
    }

    private void OnEnable()
    {
        roll.performed += OnRoll;
        roll.Enable();
    }

    private void OnDisable()
    {
        roll.performed -= OnRoll;
        rebinding?.Cancel();
        roll.Disable();
    }

    private void OnDestroy() => roll.Dispose();

    public void Rebind()
    {
        if (!isActiveAndEnabled || IsRebinding)
            return;

        roll.Disable();
        rebinding = roll.PerformInteractiveRebinding(0)
            .WithExpectedControlType("Button")
            .WithControlsExcluding("<Pointer>")
            .WithCancelingThrough("<Keyboard>/escape")
            .WithTimeout(10f)
            .OnComplete(operation =>
            {
                PlayerPrefs.SetString(BindingKey, roll.bindings[0].effectivePath);
                PlayerPrefs.Save();
                FinishRebinding(operation);
            })
            .OnCancel(FinishRebinding);

        rebinding.Start();
        BindingChanged?.Invoke();
    }

    public void ResetBinding()
    {
        if (!isActiveAndEnabled || IsRebinding)
            return;

        roll.RemoveBindingOverride(0);
        PlayerPrefs.DeleteKey(BindingKey);
        PlayerPrefs.Save();
        BindingChanged?.Invoke();
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        // Pointer input is routed through UI UnityEvents, not the global shortcut.
        if (context.control.device is Pointer)
            return;

        var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        var field = selected != null ? selected.GetComponentInParent<TMP_InputField>() : null;
        if (field != null && field.isFocused)
            return;

        RollRequested?.Invoke();
    }

    private static bool IsPointerBinding(string path) => !string.IsNullOrEmpty(path) &&
        (path.StartsWith("<Mouse>", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("<Pointer>", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("<Touchscreen>", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("<Pen>", StringComparison.OrdinalIgnoreCase));

    private void FinishRebinding(InputActionRebindingExtensions.RebindingOperation operation)
    {
        operation.Dispose();
        rebinding = null;
        if (isActiveAndEnabled)
            roll.Enable();

        BindingChanged?.Invoke();
    }
}
