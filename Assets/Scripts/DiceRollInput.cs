using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

public sealed class DiceRollInput : MonoBehaviour
{
    private const string BindingKey = "DiceGame.RollBinding";
    private const string DefaultBinding = "<Keyboard>/space";
    private const int BindingIndex = 0;
    private const float RebindingTimeout = 10f;

    [SerializeField]
    private InputAction roll = new("Roll", InputActionType.Button, DefaultBinding);

    private RebindingOperation rebinding;

    public event Action RollRequested;
    public event Action BindingChanged;

    public bool IsRebinding => rebinding != null;
    public string BindingLabel => roll.GetBindingDisplayString(BindingIndex);

    private bool CanChangeBinding => isActiveAndEnabled && !IsRebinding;

    private void Awake()
    {
        roll.RemoveBindingOverride(BindingIndex);

        if (IsPointerBinding(roll.bindings[BindingIndex].path))
            roll.ChangeBinding(BindingIndex).WithPath(DefaultBinding);

        LoadBinding();
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

    private void OnDestroy()
    {
        roll.Dispose();
    }

    public void Rebind()
    {
        if (!CanChangeBinding)
            return;

        roll.Disable();

        rebinding = roll.PerformInteractiveRebinding(BindingIndex)
            .WithExpectedControlType("Button")
            .WithControlsExcluding("<Pointer>")
            .WithCancelingThrough("<Keyboard>/escape")
            .WithTimeout(RebindingTimeout)
            .OnComplete(OnRebindingCompleted)
            .OnCancel(FinishRebinding);

        rebinding.Start();
        BindingChanged?.Invoke();
    }

    public void ResetBinding()
    {
        if (!CanChangeBinding)
            return;

        roll.RemoveBindingOverride(BindingIndex);
        ClearSavedBinding();
        BindingChanged?.Invoke();
    }

    private void LoadBinding()
    {
        var path = PlayerPrefs.GetString(BindingKey);

        if (IsPointerBinding(path))
        {
            ClearSavedBinding();
            return;
        }

        if (!string.IsNullOrEmpty(path))
            roll.ApplyBindingOverride(BindingIndex, path);
    }

    private void OnRebindingCompleted(RebindingOperation operation)
    {
        PlayerPrefs.SetString(
            BindingKey,
            roll.bindings[BindingIndex].effectivePath);

        PlayerPrefs.Save();
        FinishRebinding(operation);
    }

    private void FinishRebinding(RebindingOperation operation)
    {
        operation.Dispose();
        rebinding = null;

        if (isActiveAndEnabled)
            roll.Enable();

        BindingChanged?.Invoke();
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        if (context.control.device is Pointer || IsTextInputFocused())
            return;

        RollRequested?.Invoke();
    }

    private static void ClearSavedBinding()
    {
        PlayerPrefs.DeleteKey(BindingKey);
        PlayerPrefs.Save();
    }

    private static bool IsTextInputFocused()
    {
        var eventSystem = EventSystem.current;

        if (eventSystem == null)
            return false;

        var selected = eventSystem.currentSelectedGameObject;

        if (selected == null)
            return false;

        var field = selected.GetComponentInParent<TMP_InputField>();
        return field != null && field.isFocused;
    }

    private static bool IsPointerBinding(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path.StartsWith("<Mouse>", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("<Pointer>", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("<Touchscreen>", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("<Pen>", StringComparison.OrdinalIgnoreCase);
    }
}