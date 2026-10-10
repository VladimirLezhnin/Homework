using Core;
using TMPro;
using UnityEngine;

public sealed class DicePanel : MonoBehaviour
{
    [SerializeField] private DiceTable table;
    [SerializeField] private RollInput input;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text binding;

    private void OnEnable()
    {
        table.Round.Changed += RefreshScore;
        input.BindingChanged += RefreshBinding;
        RefreshScore();
        RefreshBinding();
    }
        
    private void Start() => RefreshBinding();

    private void OnDisable()
    {
        table.Round.Changed -= RefreshScore;
        input.BindingChanged -= RefreshBinding;
    }

    private void RefreshScore() => score.text = table.Round.State switch
    {
        RoundState.Rolling => "Бросаем кости . . .",
        RoundState.Finished => $"Сумма: {table.Round.Score}",
        RoundState.Cancelled => "Бросок не засчитан. Повторите бросок",
        _ => "Ожидаем броска . . ."
    };

    private void RefreshBinding() => binding.text = input.IsRebinding
        ? "Нажмите новую кнопку или Esc для отмены"
        : $"Бросок: {input.BindingLabel}";
}