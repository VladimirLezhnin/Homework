using System.Collections;
using System.Linq;
using Core;
using UnityEngine;

public sealed class DiceTable : MonoBehaviour
{
    [SerializeField] private RollInput input;
    [SerializeField] private Die[] dice;
    [SerializeField] private float settleTime = 0.25f;
    [SerializeField] private float timeout = 15f;

    private readonly WaitForFixedUpdate physicsStep = new();
    private int[] values;
    private RoundSettings nextSettings;

    public Round Round { get; } = new();
    public int MaxDiceCount => dice?.Length ?? 0;
    public RoundSettings NextSettings => nextSettings;
    public bool CanRoll => isActiveAndEnabled && input != null && !input.IsRebinding && 
                           nextSettings != null && Round.State != RoundState.Rolling;

    private void Awake()
    {
        if (input == null || dice == null || dice.Length == 0 ||
            dice.Any(die => die == null) || dice.Distinct().Count() != dice.Length)
        {
            Debug.LogError("Assign RollInput and a non-empty array of unique dice", this);
            enabled = false;
            return;
        }

        values = new int[dice.Length];
        if (nextSettings == null)
            RoundSettings.TryCreate(dice.Length, null, null, null, out nextSettings);
    }

    private void OnEnable() => input.RollRequested += Roll;

    private void OnDisable()
    {
        if (input != null)
            input.RollRequested -= Roll;

        StopAllCoroutines();
        Round.Cancel();
    }

    public void SetNextRoundSettings(RoundSettings settings)
    {
        if (Round.State == RoundState.Rolling)
            return;

        nextSettings = settings != null && settings.DiceCount <= MaxDiceCount ? settings : null;
    }

    public void Roll()
    {
        if (!CanRoll)
            return;

        var settings = nextSettings;
        if (values.Length != settings.DiceCount)
            values = new int[settings.DiceCount];
        
        RoundSettings.TryCreate(settings.DiceCount, null, null, null, out nextSettings);
        if (!Round.TryBegin(settings))
            return;

        for (var i = 0; i < dice.Length; i++)
        {
            var die = dice[i];
            if (die == null)
            {
                CancelRound();
                return;
            }

            var selected = i < values.Length;
            if (!selected && die.isActiveAndEnabled)
                die.ResetPosition();

            die.gameObject.SetActive(selected);
        }

        if (!AllAvailable())
        {
            CancelRound();
            return;
        }

        for (var i = 0; i < values.Length; i++)
            dice[i].Roll();

        StartCoroutine(WaitForResult());
    }

    private IEnumerator WaitForResult()
    {
        var stableTime = 0f;
        var elapsed = 0f;
        while (elapsed < timeout)
        {
            yield return physicsStep;

            if (!AllAvailable())
            {
                CancelRound();
                yield break;
            }

            elapsed += Time.fixedDeltaTime;
            stableTime = AllSleeping() ? stableTime + Time.fixedDeltaTime : 0f;
            if (stableTime < settleTime)
                continue;

            for (var i = 0; i < values.Length; i++)
            {
                if (dice[i].TryReadValue(out values[i]))
                    continue;

                CancelRound();
                yield break;
            }

            Round.Complete(values);
            yield break;
        }

        CancelRound();
    }

    private bool AllAvailable()
    {
        for (var i = 0; i < values.Length; i++)
            if (!dice[i] || !dice[i].isActiveAndEnabled)
                return false;

        return true;
    }

    private bool AllSleeping()
    {
        for (var i = 0; i < values.Length; i++)
            if (!dice[i].IsSleeping)
                return false;

        return true;
    }

    private void CancelRound()
    {
        foreach (var die in dice)
            if (die && die.isActiveAndEnabled)
                die.ResetPosition();

        Round.Cancel();
    }
}