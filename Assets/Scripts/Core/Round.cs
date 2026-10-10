using System;
using System.Collections.Generic;

namespace Core
{
    public sealed class Round
    {
        public event Action Changed;

        public RoundState State { get; private set; }
        public int Score { get; private set; }

        public bool TryBegin()
        {
            if (State == RoundState.Rolling)
                return false;

            Score = 0;
            SetState(RoundState.Rolling);
            return true;
        }

        public void Complete(IReadOnlyList<int> values)
        {
            if (State != RoundState.Rolling)
                throw new InvalidOperationException("The round has not started");
            if (values is null || values.Count == 0)
                throw new ArgumentException("At least one die is required", nameof(values));

            var score = 0;
            foreach (var value in values)
                score += value;

            Score = score;
            SetState(RoundState.Finished);
        }

        public void Cancel()
        {
            if (State == RoundState.Rolling)
                SetState(RoundState.Cancelled);
        }

        private void SetState(RoundState state)
        {
            State = state;
            Changed?.Invoke();
        }
    }
}
