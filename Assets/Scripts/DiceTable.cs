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

    public Round Round { get; } = new();

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
    }

    private void OnEnable() => input.RollRequested += Roll;

    private void OnDisable()
    {
        if (input != null)
            input.RollRequested -= Roll;

        StopAllCoroutines();
        Round.Cancel();
    }

    private void Roll()
    {
        if (!isActiveAndEnabled || !Round.TryBegin())
            return;

        foreach (var die in dice)
        {
            if (die == null || !die.isActiveAndEnabled)
            {
                CancelRound();
                return;
            }

            die.Roll();
        }

        StartCoroutine(WaitForResult());
    }

    private IEnumerator WaitForResult()
    {
        var stableTime = 0f;
        var elapsed = 0f;
        while (elapsed < timeout)
        {
            yield return physicsStep;
                
            elapsed += Time.fixedDeltaTime;
            stableTime = AllSleeping() ? stableTime + Time.fixedDeltaTime : 0f;
            if (stableTime < settleTime)
                continue;

            for (var i = 0; i < dice.Length; i++)
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

    private bool AllSleeping() => dice.All(die => die && die.IsSleeping);

    private void CancelRound()
    {
        foreach (var die in dice)
            if (die && die.isActiveAndEnabled)
                die.ResetPosition();

        Round.Cancel();
    }
}