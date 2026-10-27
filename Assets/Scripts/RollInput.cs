using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class RollInput : MonoBehaviour
{
    private const string BindingKey = "DiceGame.RollBinding";

    [SerializeField] private InputAction roll = new("Roll", InputActionType.Button, "<Keyboard>/space");

    private InputActionRebindingExtensions.RebindingOperation rebinding;

    public event Action RollRequested;
    public event Action BindingChanged;

    public bool IsRebinding => rebinding != null;
    public string BindingLabel => roll.GetBindingDisplayString(0);

    private void Awake()
    {
        var path = PlayerPrefs.GetString(BindingKey);
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
        var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        var field = selected != null ? selected.GetComponentInParent<TMP_InputField>() : null;
        if (field != null && field.isFocused)
            return;

        RollRequested?.Invoke();
    }

    private void FinishRebinding(InputActionRebindingExtensions.RebindingOperation operation)
    {
        operation.Dispose();
        rebinding = null;
        if (isActiveAndEnabled)
            roll.Enable();

        BindingChanged?.Invoke();
    }
}