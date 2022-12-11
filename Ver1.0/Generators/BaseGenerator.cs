using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Numerics;

using VoronoiMapTrial.BaseMap;

namespace VoronoiMapTrial.Generators
{
    internal class BaseGenerator
    {
        private List<Vector2> points;
        private VoronoiDiagram diagram;

        public BaseGenerator(List<Vector2> points)
        {
            this.points = new List<Vector2>();
            this.points.AddRange(points);
        }

        public VoronoiDiagram generateDiagram()
        {
            // Construct diagram
            FortuneAlgorithm algorithm = new FortuneAlgorithm(points);
            algorithm.construct();
            
            // Bound the diagram
            algorithm.bound(new Box(-0.05, -0.05, 1.05, 1.05)); // Take the bounding box slightly bigger than the intersection box
            diagram = algorithm.getDiagram();

            // Intersect the diagram with a box
            bool valid = diagram.intersect(new Box(0.0, 0.0, 1.0, 1.0));
            if (!valid)
            {
                diagram = null;
                throw new Exception("An error occured in the box intersection algorithm");
            }

            return diagram;
        }
    }
}
