using System;
using System.Collections.Generic;

namespace Core
{
    public sealed class Round
    {
        private readonly Random random = new();

        public event Action Changed;

        public RoundState State { get; private set; }
        public int Score { get; private set; }
        public RoundResult Result { get; private set; }
        public RoundSettings Settings { get; private set; }
        public int DiceCount => Settings?.DiceCount ?? 0;
        public int LossMinimum { get; private set; }
        public int DrawMinimum { get; private set; }
        public int WinMinimum { get; private set; }
        public IReadOnlyList<int> Values { get; private set; } = Array.Empty<int>();

        public bool TryBegin(RoundSettings settings)
        {
            if (State == RoundState.Rolling)
                return false;

            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Score = 0;
            Result = RoundResult.None;
            Values = Array.Empty<int>();

            var maxScore = DiceCount * 6;
            var lossMax = Math.Min(settings.DrawMinimum.HasValue ? settings.DrawMinimum.Value - 1 : maxScore - 2,
                settings.WinMinimum.HasValue ? settings.WinMinimum.Value - 2 : maxScore - 2);

            LossMinimum = settings.LossMinimum ?? random.Next(DiceCount, lossMax + 1);
            var drawMax = settings.WinMinimum.HasValue ? settings.WinMinimum.Value - 1 : maxScore - 1;
            DrawMinimum = settings.DrawMinimum ?? random.Next(LossMinimum + 1, drawMax + 1);
            WinMinimum = settings.WinMinimum ?? random.Next(DrawMinimum + 1, maxScore + 1);

            SetState(RoundState.Rolling);
            return true;
        }

        public void Complete(IReadOnlyList<int> values)
        {
            if (State != RoundState.Rolling)
                throw new InvalidOperationException("The round has not started");
            if (values is null || values.Count != DiceCount)
                throw new ArgumentException("A value for each rolled die is required", nameof(values));

            var score = 0;
            var result = new int[values.Count];
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value is < 1 or > 6)
                    throw new ArgumentOutOfRangeException(nameof(values), "Die values must be between 1 and 6");

                result[i] = value;
                score += value;
            }

            Score = score;
            Values = Array.AsReadOnly(result);

            if (score < LossMinimum)
            {
                Cancel();
                return;
            }

            Result = score >= WinMinimum ? RoundResult.Win :
                score >= DrawMinimum ? RoundResult.Draw : RoundResult.Loss;
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
