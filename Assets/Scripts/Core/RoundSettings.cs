namespace Core
{
    public sealed class RoundSettings
    {
        public int DiceCount { get; }
        public int? LossMinimum { get; }
        public int? DrawMinimum { get; }
        public int? WinMinimum { get; }

        private RoundSettings(int diceCount, int? lossMinimum, int? drawMinimum, int? winMinimum)
        {
            DiceCount = diceCount;
            LossMinimum = lossMinimum;
            DrawMinimum = drawMinimum;
            WinMinimum = winMinimum;
        }

        public static bool TryCreate(int diceCount, int? lossMinimum, int? drawMinimum, int? winMinimum, out RoundSettings settings)
        {
            settings = null;
            if (diceCount is < 1 or > int.MaxValue / 6)
                return false;

            var maxScore = diceCount * 6;
            if (!IsInRange(lossMinimum, diceCount, maxScore - 2) ||
                !IsInRange(drawMinimum, diceCount + 1, maxScore - 1) ||
                !IsInRange(winMinimum, diceCount + 2, maxScore))
                return false;

            if (lossMinimum.HasValue && drawMinimum.HasValue && lossMinimum.Value >= drawMinimum.Value ||
                drawMinimum.HasValue && winMinimum.HasValue && drawMinimum.Value >= winMinimum.Value ||
                lossMinimum.HasValue && winMinimum.HasValue && winMinimum.Value - lossMinimum.Value < 2)
                return false;

            settings = new RoundSettings(diceCount, lossMinimum, drawMinimum, winMinimum);
            return true;
        }

        private static bool IsInRange(int? value, int min, int max) =>
            !value.HasValue || value.Value >= min && value.Value <= max;
    }
}