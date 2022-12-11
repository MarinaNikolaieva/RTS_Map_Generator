using System;
using System.Collections.Generic;
using System.Linq;

using VoronoiMapTrial.BaseMapVer2;

namespace VoronoiMapTrial.Generators
{
    public class SeaLandGenerator
    {
        public int landPercentage;
        public int islandNumber;
        public int acceptableError;

        private double mapArea;

        public List<RectMapPart> mapComponents = new List<RectMapPart>();
        public List<MapComponent> components = new List<MapComponent>();

        public SeaLandGenerator(int landPercentage, int islandNumber, int acceptableError, List<RectMapPart> comps, double mapArea)
        {
            this.landPercentage = landPercentage;
            this.islandNumber = islandNumber;
            this.acceptableError = acceptableError;
            this.mapArea = mapArea;
            mapComponents.AddRange(comps);
            int counter = 0;
            for (int i = 0; i < mapComponents.Count; i++)
            {
                components.Add(new MapComponent(counter, mapComponents.ElementAt(i)));
                counter++;
            }
        }

        private void candidatesFillWithEmpty(List<MapComponent> candidates)
        {
            for (int i = 0; i < islandNumber; i++)
                candidates.Add(new MapComponent(-1, null));
        }

        private double firstCellForOneIsland(Random rand, List<MapComponent> confirmed, List<RectMapPart> confirmedFaces)
        {
            double area = 0.0;
            while (area == 0.0)
            {
                MapComponent component = components.ElementAt(rand.Next(components.Count()));
                if (component.area / mapArea * 100 <= landPercentage + acceptableError)
                {
                    area += component.area;
                    confirmed.Add(component);
                    confirmedFaces.Add(component.face);
                }

            }
            return area;
        }

        private void selectCandidateForIsland(int islandIndex, List<MapComponent> candidates, Random rand)
        {
            bool found = false;
            while (!found)
            {
                MapComponent mapComponent = components.ElementAt(rand.Next(components.Count()));
                if (!candidates.Contains(mapComponent))
                {
                    candidates[islandIndex] = mapComponent;
                    found = true;
                }
            }
        }

        private bool hasConfirmedNeighbor(RectMapPart part, List<RectMapPart> confirmedFaces)
        {
            bool found = false;
            int cont = 0;
            do
            {
                RectMapPart part2 = mapComponents.ElementAt(part.neighborIndexes.ElementAt(cont));
                if (confirmedFaces.Contains(part2))
                {
                    found = true;
                }
                else
                {
                    cont++;
                }
            } while (cont < part.neighborIndexes.Count && !found);
            return found;
        }

        private void clearEmptyCandidates(List<MapComponent> candidates)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates.ElementAt(i) == null)
                {
                    candidates.RemoveAt(i);
                    i = -1;
                }
            }
        }

        private void candidatesToConfirmedMultipleIslandsInit(List<MapComponent> candidates, List<RectMapPart> confirmedFaces,
            Dictionary<int, List<RectMapPart>> islands)
        {
            foreach (var candidate in candidates)
            {
                confirmedFaces.Add(candidate.face);
            }
            for (int i = 0; i < candidates.Count(); i++)
            {
                List<RectMapPart> comp = new List<RectMapPart>();
                comp.Add(candidates.ElementAt(i).face);
                islands.Add(i, comp);
            }
        }

        private void addCandidateForIsland(int islandIndex, List<MapComponent> candidates, Random rand,
            List<RectMapPart> confirmedFaces)
        {
            bool found = true;
            while (found)
            {
                selectCandidateForIsland(islandIndex, candidates, rand);
                RectMapPart part = candidates.ElementAt(islandIndex).face;
                found = hasConfirmedNeighbor(part, confirmedFaces);
                if (found)
                    candidates[islandIndex] = new MapComponent(-1, null);
            }
        }

        private double firstCellsMultipleIslands(List<MapComponent> candidates, Random rand, List<RectMapPart> confirmedFaces,
            List<MapComponent> confirmed, Dictionary<int, List<RectMapPart>> islands)
        {
            double totalArea = 0.0;
            bool done = false;
            while (!done)
            {
                double templateArea = 0.0;
                for (int i = 0; i < islandNumber; i++)
                {
                    addCandidateForIsland(i, candidates, rand, confirmedFaces);
                    templateArea += candidates[i].area;
                }
                if (templateArea / mapArea * 100 <= landPercentage + acceptableError)
                {
                    totalArea += templateArea;
                    clearEmptyCandidates(candidates);
                    confirmed.AddRange(candidates);
                    candidatesToConfirmedMultipleIslandsInit(candidates, confirmedFaces, islands);
                    candidates.Clear();
                    candidatesFillWithEmpty(candidates);
                    done = true;
                }
                else
                    candidates.Clear();
            }
            return totalArea;
        }

        private void clearEmptyConfirmed(List<MapComponent> confirmed)
        {
            for (int i = 0; i < confirmed.Count; i++)  //if empty cells have left, clear them
            {
                if (confirmed.ElementAt(i) == null || confirmed.ElementAt(i).face == null)
                {
                    confirmed.RemoveAt(i);
                    i = -1;
                }
            }
        }

        private int findIslandByPart(RectMapPart part, Dictionary<int, List<RectMapPart>> islands)
        {
            for (int j = 0; j < islands.Count(); j++)
            {
                if (islands[j].Contains(part))
                {
                    //Got it
                    return j;
                }
            }
            return -1;
        }

        private bool isBorderingOtherIsland(Dictionary<int, List<RectMapPart>> islands, RectMapPart part,
            int index, List<MapComponent> candidates, List<RectMapPart> confirmedFaces)
        {
            for (int j = 0; j < islands.Count(); j++)
            {
                if ((islands[j].Contains(part) && j != index) ||
                    (candidates[j].face != null && candidates[j].face.Equals(part) && j != index) ||
                    confirmedFaces.Contains(part))
                //it borders the other island
                {
                    return true;
                }
            }
            return false;
        }

        private double putIslandCandidateIfPossible(Random rand, RectMapPart part, List<RectMapPart> confirmedFaces,
            Dictionary<int, List<RectMapPart>> islands, int index, List<MapComponent> candidates)
        {
            int nex = rand.Next(part.neighborIndexes.Count);  //Select the candidate for becoming land
            RectMapPart part2 = mapComponents.ElementAt(part.neighborIndexes.ElementAt(nex));
            if (!confirmedFaces.Contains(part2))  //Make sure the new cell isn't occupied yet!
            {
                //Now we need to check the neighbours of the cell
                //If it borders the other island, it's not what we need
                int net = rand.Next(part2.neighborIndexes.Count);
                RectMapPart part3 = mapComponents.ElementAt(part2.neighborIndexes.ElementAt(net));
                bool occupied = isBorderingOtherIsland(islands, part3, index, candidates, confirmedFaces);
                if (!occupied)  //it doesn't border any island - we got the needed one
                {
                    candidates[index] = components.ElementAt(part.neighborIndexes.ElementAt(nex));
                    return candidates[index].area;
                }
            }
            return 0.0;
        }

        private double putOneIslandCandidate(RectMapPart part, Random rand, List<RectMapPart> confirmedFaces,
            List<MapComponent> candidates)
        {
            MapComponent comp;
            int nex = rand.Next(part.neighborIndexes.Count);

            RectMapPart part2 = mapComponents.ElementAt(part.neighborIndexes.ElementAt(nex));
            if (!confirmedFaces.Contains(part2) && part2 != null)
            {
                comp = components.ElementAt(part.neighborIndexes.ElementAt(nex));
                candidates.Add(comp);
                return comp.area;
            }
            return 0.0;
        }

        private double islandSpreadStep(List<MapComponent> confirmed, Random rand, List<RectMapPart> confirmedFaces,
            List<MapComponent> candidates, Dictionary<int, List<RectMapPart>> islands)
        {
            double tempArea = 0.0;
            for (int i = 0; i < confirmed.Count(); i++)
            {
                if (islandNumber == 1)
                {
                    RectMapPart part = confirmed.ElementAt(i).face;
                    double tempAreaBuf = putOneIslandCandidate(part, rand, confirmedFaces, candidates);
                    if (tempAreaBuf != 0.0)
                    {
                        tempArea += tempAreaBuf;
                        break;
                    }
                }
                else
                {
                    RectMapPart part = confirmed.ElementAt(i).face;
                    int index = findIslandByPart(part, islands); //Which island are we looking at?

                    tempArea += putIslandCandidateIfPossible(rand, part, confirmedFaces, islands, index, candidates);
                }
            }
            return tempArea;
        }

        private void setConfirmed(List<MapComponent> confirmed)
        {
            //Confirm the elements, set the isLand boolean
            for (int i = 0; i < confirmed.Count(); i++)
            {
                components.ElementAt(confirmed.ElementAt(i).index).isLand = true;
            }
        }

        private bool fitToLowerBorderForOne(List<MapComponent> confirmed, List<MapComponent> candidates, double totalArea,
            List<RectMapPart> confirmedFaces)
        {
            confirmed.AddRange(candidates);
            foreach (var candidate in candidates)
                confirmedFaces.Add(candidate.face);
            candidates.Clear();
            if (totalArea / mapArea * 100 >= landPercentage - acceptableError)
                return true;
            return false;
        }

        private bool fitToLowerBorderForMultiple(Dictionary<int, List<RectMapPart>> islands, List<MapComponent> candidates,
            List<RectMapPart> confirmedFaces, List<MapComponent> confirmed, double totalArea)
        {
            for (int j = 0; j < candidates.Count; j++)
            {
                if (candidates.ElementAt(j).index != -1)
                {
                    islands[j].Add(candidates.ElementAt(j).face);
                    confirmedFaces.Add(candidates.ElementAt(j).face);
                }
            }
            for (int j = 0; j < candidates.Count; j++)
            {
                if (candidates.ElementAt(j).index != -1)
                    confirmed.Add(candidates.ElementAt(j));
            }
            candidates.Clear();
            candidatesFillWithEmpty(candidates);
            if (totalArea / mapArea * 100 >= landPercentage - acceptableError)
                return true;
            return false;
        }

        public List<MapComponent> run()
        {
            List<MapComponent> confirmed = new List<MapComponent>();
            List<MapComponent> candidates = new List<MapComponent>();
            candidatesFillWithEmpty(candidates);
            List<RectMapPart> confirmedFaces = new List<RectMapPart>();
            Dictionary<int, List<RectMapPart>> islands = new Dictionary<int, List<RectMapPart>>();
            Random rand = new Random();
            double totalArea = 0.0;

            //Adding the starting cells
            if (islandNumber == 1)
            {
                totalArea = firstCellForOneIsland(rand, confirmed, confirmedFaces);
            }
            else
            {
                totalArea = firstCellsMultipleIslands(candidates, rand, confirmedFaces, confirmed, islands);
            }

            //PROBLEM! It makes solid land map if more than 1 island is made
            //FIXED, but only partially. Seems like the area isn't calculating correctly
            //Sometimes the islands merge into one, which is forbidden

            int leadCounter = 10000;  //I'll give the generator 10000 trials to reach the needed accurancy
            while (leadCounter != 0)
            {
                //Get bordering cell - candidate for becoming land
                double tempArea = 0.0;

                clearEmptyConfirmed(confirmed);

                tempArea += islandSpreadStep(confirmed, rand, confirmedFaces, candidates, islands);

                //Calculate total percentage of land and go back if it's too much
                if ((totalArea + tempArea) / mapArea * 100 <= landPercentage + acceptableError)
                {
                    if (candidates.Count != 0)
                        leadCounter--;

                    if (islandNumber == 1)
                    {
                        totalArea += tempArea;
                        tempArea = 0.0;
                        if (fitToLowerBorderForOne(confirmed, candidates, totalArea, confirmedFaces))
                            break;
                    }
                    else
                    {
                        totalArea += tempArea;
                        tempArea = 0.0;
                        if (fitToLowerBorderForMultiple(islands, candidates, confirmedFaces, confirmed, totalArea))
                            break;
                    }
                }
                else
                {
                    tempArea = 0.0;
                    candidates.Clear();
                    candidatesFillWithEmpty(candidates);
                }
            }

            setConfirmed(confirmed);

            return components;
        }
    }
}
