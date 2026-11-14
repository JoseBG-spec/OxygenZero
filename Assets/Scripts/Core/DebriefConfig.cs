using UnityEngine;

namespace MarsOutpost.Core
{
    [System.Serializable]
    public sealed class DebriefBand
    {
        public string title = "Mission Complete";
        [TextArea(2, 4)] public string message = "Your crew finished the training mission.";
        [Tooltip("This band is chosen when Science Progress is at least this value.")]
        public int minimumScience = 0;
        public Color color = Color.white;
    }

    [CreateAssetMenu(fileName = "DebriefConfig", menuName = "Mars Outpost/Debrief Config", order = 40)]
    public sealed class DebriefConfig : ScriptableObject
    {
        public string reportTitle = "Mission Report";
        [TextArea(2, 4)] public string intro = "Here is how your training mission went.";

        [TextArea(3, 7)] public string sources =
            "NASA OSDR study OSD-314: 'Adaptive response of Arabidopsis seedlings in microgravity and " +
            "Mars reduced gravity environment is enhanced by red light photostimulation' (Villacampa et al., " +
            "International Journal of Molecular Sciences, 2021; doi:10.3390/ijms22020899).\n\n" +
            "NASA CCMC DONKI documentation for space-weather event types and API terms. " +
            "DONKI data is research/prototyping quality; NOAA SWPC is the official forecast source.\n\n" +
            "NASA Mars Trek (trek.nasa.gov/mars) is the reference for future real map layers. " +
            "The current map is a fictional placeholder.";

        [TextArea(2, 4)] public string notNasaNote =
            "This game is a learning simulation, not an official NASA product. Game values, " +
            "landing sites, and space-weather alerts are invented for play.";

        public DebriefBand[] bands = new DebriefBand[0];
        public string replayLabel = "Play Again";
        public string menuLabel = "Main Menu";
    }
}
