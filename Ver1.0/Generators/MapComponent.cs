using System;
using System.Linq;

using VoronoiMapTrial.BaseMapVer2;

namespace VoronoiMapTrial.Generators
{
    public class MapComponent : IEquatable<MapComponent>
    {
        public int index;
        public RectMapPart face;
        public double area;

        public bool isLand;

        public double height;
        public int biomeID;
        public int resourceID;

        public MapComponent(int index, RectMapPart face)
        {
            this.index = index;
            this.face = face;

            area = 0.0;
            
            if (face != null)
            {
                int wid = (int)(this.face.cornerCoords.ElementAt(2).X - this.face.cornerCoords.ElementAt(0).X);
                int heig = (int)(this.face.cornerCoords.ElementAt(2).Y - this.face.cornerCoords.ElementAt(0).Y);
                area = wid * heig;
            }

            isLand = false;  //by default all components are water

            height = -1.0;
            biomeID = -1;
            resourceID = -1;
        }

        public bool Equals (MapComponent other)
        {
            return this.index == other.index && this.face.Equals(other.face)
                && this.area == other.area && this.isLand == other.isLand;
        }
    }
}
