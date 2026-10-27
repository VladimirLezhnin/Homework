using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DicePanel : MonoBehaviour
{
    [SerializeField] private DiceTable table;
    [SerializeField] private RollInput input;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text rules;
    [SerializeField] private TMP_Text error;
    [SerializeField] private TMP_InputField diceCountInput;
    [SerializeField] private TMP_InputField lossMinimumInput;
    [SerializeField] private TMP_InputField drawMinimumInput;
    [SerializeField] private TMP_InputField winMinimumInput;
    [SerializeField] private Button rollButton;

    private Round model;
    private int settingsDiceCount;

    private void Awake()
    {
        if (table == null || input == null || score == null || rules == null ||
            error == null || diceCountInput == null || lossMinimumInput == null ||
            drawMinimumInput == null || winMinimumInput == null || rollButton == null)
        {
            Debug.LogError("Assign the table, input, labels, input fields and roll button", this);
            enabled = false;
            return;
        }

        Initialize(table.Round);
    }

    private void Initialize(Round round)
    {
        model = round;
        diceCountInput.SetTextWithoutNotify(table.MaxDiceCount.ToString());
        ClearMinimumInputs();
        ApplySettings();
        Refresh();
    }

    private void OnEnable()
    {
        model.Changed += Refresh;
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (model != null)
            model.Changed -= Refresh;
    }

    public void UpdateSettings()
    {
        if (!isActiveAndEnabled || model == null || model.State == RoundState.Rolling || input.IsRebinding)
            return;

        ApplySettings();
        Refresh();
    }

    public void Roll()
    {
        if (!isActiveAndEnabled || model == null || model.State == RoundState.Rolling || input.IsRebinding)
            return;

        if (ApplySettings())
            table.Roll();

        Refresh();
    }

    private bool ApplySettings()
    {
        if (!int.TryParse(diceCountInput.text, out var count) || count < 1 || count > table.MaxDiceCount)
            return RejectSettings($"Введите число кубиков от 1 до {table.MaxDiceCount}");

        if (count != settingsDiceCount)
        {
            ClearMinimumInputs();
            settingsDiceCount = count;
        }

        if (!TryReadMinimum(lossMinimumInput, out var loss) ||
            !TryReadMinimum(drawMinimumInput, out var draw) ||
            !TryReadMinimum(winMinimumInput, out var win))
            return RejectSettings("Введите целые числа или оставьте минимумы пустыми");

        if (!RoundSettings.TryCreate(count, loss, draw, win, out var settings))
            return RejectSettings($"Минимумы: {count}–{(long)count * 6}. " +
                "Поражение < ничья < победа; оставьте место для случайных минимумов");

        table.SetNextRoundSettings(settings);
        error.text = string.Empty;
        return true;
    }

    private bool RejectSettings(string message)
    {
        table.SetNextRoundSettings(null);
        error.text = message;
        return false;
    }

    private void Refresh()
    {
        score.text = model.State switch
        {
            RoundState.Rolling => "Бросаем кости . . .",
            RoundState.Finished => ScoreLabel(ResultLabel(model.Result)),
            RoundState.Cancelled when model.Values.Count > 0 => ScoreLabel(
                $"Не засчитан: {model.Score} < {model.LossMinimum} (минимум поражения)"),
            RoundState.Cancelled => "Сумма: не определена.\n" +
                "Результат броска не получен. Повторите бросок",
            _ => "Ожидаем броска . . ."
        };

        if (model.Settings != null)
        {
            var title = model.State == RoundState.Rolling ? "Текущий бросок" : "Последний бросок";
            rules.text = $"{title}. Кубиков: {model.DiceCount}\n" +
                $"Не засчитан: сумма меньше {model.LossMinimum}\n" +
                $"Поражение: {model.LossMinimum}–{model.DrawMinimum - 1}\n" +
                $"Ничья: {model.DrawMinimum}–{model.WinMinimum - 1}\n" +
                $"Победа: от {model.WinMinimum}";
        }
        else
        {
            var settings = table.NextSettings;
            rules.text = settings == null ? "Исправьте параметры следующего броска" :
                $"Следующий бросок\nКубиков: {settings.DiceCount}\n" +
                $"Минимум поражения: {MinimumLabel(settings.LossMinimum)}\n" +
                $"Минимум ничьей: {MinimumLabel(settings.DrawMinimum)}\n" +
                $"Минимум победы: {MinimumLabel(settings.WinMinimum)}\n" +
                "Ниже минимума поражения — не засчитан";
        }

        RefreshControls();
    }

    private string ScoreLabel(string result) =>
        $"Сумма: {model.Score}\n{result}\nКубики: {string.Join(" + ", model.Values)}";

    private void RefreshControls()
    {
        var editable = model.State != RoundState.Rolling && !input.IsRebinding;
        diceCountInput.interactable = editable;
        lossMinimumInput.interactable = editable;
        drawMinimumInput.interactable = editable;
        winMinimumInput.interactable = editable;
        rollButton.interactable = table.CanRoll;
    }

    private void ClearMinimumInputs()
    {
        lossMinimumInput.SetTextWithoutNotify(string.Empty);
        drawMinimumInput.SetTextWithoutNotify(string.Empty);
        winMinimumInput.SetTextWithoutNotify(string.Empty);
    }

    private static bool TryReadMinimum(TMP_InputField field, out int? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(field.text))
            return true;
        if (!int.TryParse(field.text, out var number))
            return false;

        value = number;
        return true;
    }

    private static string MinimumLabel(int? value) => value?.ToString() ?? "случайно";

    private static string ResultLabel(RoundResult result) => result switch
    {
        RoundResult.Win => "Победа",
        RoundResult.Draw => "Ничья",
        RoundResult.Loss => "Поражение",
        _ => string.Empty
    };
}
