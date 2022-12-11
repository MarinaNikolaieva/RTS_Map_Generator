using System.Drawing;

namespace VoronoiMapTrial.Physical_Map
{
    public class Biome
    {
        public BiomeType type;
        public string name;
        public int ID;
        public int defaultWeight;

        public int currentWeight;

        public Color color;

        public Biome(BiomeType type, string name, int id, Color color)
        {
            this.type = type;
            this.name = name;
            ID = id;
            defaultWeight = 1;
            currentWeight = 1;
            this.color = color;
        }
    }
}
