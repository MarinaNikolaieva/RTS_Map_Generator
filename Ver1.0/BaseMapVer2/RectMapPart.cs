using System;
using System.Collections.Generic;
using System.Numerics;

namespace VoronoiMapTrial.BaseMapVer2
{
    public class RectMapPart : IEquatable<RectMapPart>
    {
        public int index;
        public Vector2 centerCoords;
        public List<Vector2> cornerCoords = new List<Vector2>();
        public List<int> neighborIndexes = new List<int>();

        public RectMapPart(int index, Vector2 coords, List<Vector2> edgeCoords)
        {
            this.index = index;
            this.centerCoords.X = coords.X;
            this.centerCoords.Y = coords.Y;
            this.cornerCoords.AddRange(edgeCoords);
        }

        public bool Equals(RectMapPart other)
        {
            return this.index == other.index;
        }
    }
}
