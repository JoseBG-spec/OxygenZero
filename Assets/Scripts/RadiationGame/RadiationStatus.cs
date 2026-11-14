namespace MarsOutpost.Radiation
{
    public enum RadiationStatus
    {
        Safe,
        Warning,
        High
    }

    /// <summary>Pure status thresholds, kept separate for easy testing and tuning.</summary>
    public static class RadiationReadings
    {
        public static RadiationStatus Classify(float reading, float warning, float high)
        {
            if (reading >= high)
            {
                return RadiationStatus.High;
            }

            if (reading >= warning)
            {
                return RadiationStatus.Warning;
            }

            return RadiationStatus.Safe;
        }
    }
}
