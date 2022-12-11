using System;
using System.Collections.Generic;
using System.Linq;

using VoronoiMapTrial.Physical_Map;
using VoronoiMapTrial.BaseMapVer2;

namespace VoronoiMapTrial.Generators
{
    public class BiomeGenerator
    {
        private class CustomCompare : IComparer<MapComponent>
        {
            public int Compare(MapComponent a, MapComponent b)
            {
                if (a.biomeID < b.biomeID)
                    return -1;
                if (a.biomeID == b.biomeID)
                    return 0;
                return 1;
            }
        }

        List<MapComponent> components = new List<MapComponent>();
        List<RectMapPart> parts = new List<RectMapPart>();
        List<Biome> selectedBiomes = new List<Biome>();
        double totalArea;
        double acceptableError = 5.0;
        List<MapComponent> confirmed = new List<MapComponent>();
        List<MapComponent> candidates = new List<MapComponent>();
        //I wonder how much I need the next array...
        List<Biome> setBiomes = new List<Biome>();
        Random rand = new Random();
        List<int> percentages = new List<int>();
        int minimumHeight;
        int maximumHeight;
        CustomCompare comparer = new CustomCompare();

        int trialCounter = 0;

        public BiomeGenerator(List<MapComponent> components, List<RectMapPart> p, List<Biome> selectedBiomes, double area, int min, int max)
        {
            this.components.AddRange(components);
            this.selectedBiomes.AddRange(selectedBiomes);
            this.parts.AddRange(p);
            totalArea = area;
            minimumHeight = min;
            maximumHeight = max;
            for (int i = 0; i < components.Count(); i++)
            {
                if (!components.ElementAt(i).isLand)
                    components.ElementAt(i).biomeID = 0;
            }
        }

        private void calculatePercentages()
        {
            int totalWeight = 0;
            for (int i = 0; i < selectedBiomes.Count; i++)
                totalWeight += selectedBiomes.ElementAt(i).currentWeight;
            for (int i = 0; i < selectedBiomes.Count; i++)
            {
                int percent = (int)((double)selectedBiomes.ElementAt(i).currentWeight / (double)totalWeight * 100.0);
                percentages.Add(percent);
            }
        }

        private double calculateTotalArea()
        {
            double area = 0.0;
            for (int i = 0; i < components.Count; i++)
            {
                if (components.ElementAt(i).isLand)
                    area += components.ElementAt(i).area;
            }

            //Scaler is the variable needed to increase the percent of a single element
            //Limiter is the variable needed to limit the maximum of a percent
            //The acceptable error must not be more than 20% of the map
            int scaler = 10;
            int limiter = 20;
            double singlePercent = components.ElementAt(0).area / area * 100.0;
            singlePercent *= scaler;
            if (singlePercent > limiter)
                singlePercent = limiter;
            acceptableError = singlePercent > acceptableError ? singlePercent : acceptableError;
            
            return area;
        }

        private bool fitPercentages()
        {
            for (int i = 0; i < selectedBiomes.Count; i++)
            {
                double occupiedArea = 0.0;
                int counter = -1;
                if (confirmed.Count() != 0)
                {
                    while (true)
                    {
                        counter++;
                        if (counter >= confirmed.Count)
                            break;
                        else if (confirmed.ElementAt(counter).biomeID == selectedBiomes.ElementAt(i).ID)
                            occupiedArea += confirmed.ElementAt(counter).area;
                    }
                }
                counter = -1;
                if (candidates.Count() != 0)
                {
                    while (true)
                    {
                        counter++;
                        if (counter >= candidates.Count)
                            break;
                        else if (candidates.ElementAt(counter).biomeID == selectedBiomes.ElementAt(i).ID)
                            occupiedArea += candidates.ElementAt(counter).area;
                    }
                }
                double percent = occupiedArea / totalArea * 100.0;
                if (percent > percentages.ElementAt(i) + acceptableError)
                    return false;
            }
            return true;
        }

        private void setBorderIndexes(RectMapPart part, List<int> indexes)
        {
            for (int k = 0; k < part.neighborIndexes.Count; k++)
                indexes.Add(part.neighborIndexes.ElementAt(k));
        }

        private bool isWarmNear(RectMapPart part, List<MapComponent> whereToLook)
        {
            MapComponent templateC = null;
            if (whereToLook.Where(c => c.face.Equals(part)).Count() != 0)
                templateC = whereToLook.Where(c => c.face.Equals(part)).First();
            if (templateC != null && templateC.biomeID != -1)
            {
                int index = setBiomes.IndexOf(setBiomes.Where(c => c.ID == templateC.biomeID).First());
                if (setBiomes.ElementAt(index).type.type == BiomeType.Type.WARM)
                {
                    return true;
                }
            }
            return false;
        }

        private bool checkCellByWarm(List<int> indexes)
        {
            int curElement = rand.Next(indexes.Count);
            int curIndex = indexes.ElementAt(curElement);
            indexes.RemoveAt(curElement);  //so that they will not repeat
            RectMapPart tw = components.Where(c => c.index == curIndex).Select(c => c.face).First();
            if (isWarmNear(tw, candidates) || isWarmNear(tw, confirmed))
            {
                return true;
            }
            return false;
        }

        private bool checkColdWarm()
        {
            for (int i = 0; i < candidates.Count(); i++)
            {
                BiomeType fix = new BiomeType();
                fix.type = BiomeType.Type.COLD;
                
                if (setBiomes.Where(s => s.ID == candidates.ElementAt(i).biomeID).Select(s => s.type.type).First() == fix.type)
                {
                    RectMapPart face = candidates.ElementAt(i).face;
                    bool found = false;
                    List<int> indexes = new List<int>();
                    setBorderIndexes(face, indexes);
                    do
                    {
                        found = checkCellByWarm(indexes);
                    } while (indexes.Count > 0 && !found);
                    if (found)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void setFirstBiomes()
        {
            for (int i = 0; i < selectedBiomes.Count(); i++)
            {
                while (true)
                {
                    int cellIndex = rand.Next(components.Count);
                    if (!candidates.Contains(components.ElementAt(cellIndex)) && components.ElementAt(cellIndex).isLand)
                    {
                        components.ElementAt(cellIndex).biomeID = selectedBiomes.ElementAt(i).ID;
                        setBiomes.Add(selectedBiomes.ElementAt(i));
                        candidates.Add(components.ElementAt(cellIndex));
                        break;
                    }
                }
            }
        }

        private void candidatesToConfirmedPreInit()
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                confirmed.Add(candidates.ElementAt(i));
                confirmed.ElementAt(confirmed.Count - 1).biomeID = setBiomes.ElementAt(i).ID;
            }
            confirmed.Sort(comparer);
            candidates.Clear();
        }

        private void componentReset()
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                components.ElementAt(candidates.ElementAt(i).index).biomeID = -1;
            }
            candidates.Clear();
        }

        private void preInit()
        {
            totalArea = calculateTotalArea();
            calculatePercentages();

            bool done = false;
            while (!done)
            {
                setFirstBiomes();

                if (checkColdWarm())
                {
                    //We now have some biomes ready
                    //Now we need to calculate the area they occupy
                    confirmed.Sort(comparer);
                    candidates.Sort(comparer);
                    if (fitPercentages())  //if it's okay, move to the next part
                    {
                        candidatesToConfirmedPreInit();
                        done = true;
                    }
                    else  //if not, repeat until it is
                    {
                        componentReset();  //We need to reset the biome ID of cancelled candidates back to -1 here...
                    }
                }
                else
                {
                    componentReset();  //...And here
                }
            }
        }

        private MapComponent checkCellNotConfirmed(List<int> indexes)
        {
            int curElement = rand.Next(indexes.Count);
            int curIndex = indexes.ElementAt(curElement);
            indexes.RemoveAt(curElement);  //so that they will not repeat
            RectMapPart tw = parts.Where(p => p.index == curIndex).First();
            MapComponent comp = components.Find(k => k.face.Equals(tw) && k.isLand && k.biomeID == -1);
            if (!confirmed.Contains(comp) && components.Contains(comp))  // if we found it, stop the loop
            {
                return comp;
            }
            return null;
        }

        private void setNewBiomeCandidates()
        {
            for (int i = 0; i < confirmed.Count(); i++)  //we look through the cells we've already confirmed
            {
                RectMapPart part = confirmed.ElementAt(i).face;
                MapComponent comp;
                bool found = false;
                List<int> indexes = new List<int>();
                setBorderIndexes(part, indexes);
                do  //we try to find a face not in use yet and bordering the cell we're looking at
                {
                    comp = checkCellNotConfirmed(indexes);
                    if (comp != null)
                    {
                        found = true;
                        components.ElementAt(comp.index).biomeID = confirmed.ElementAt(i).biomeID;
                        setBiomes.Add(selectedBiomes.Where(s => s.ID == confirmed.ElementAt(i).biomeID).First());
                        candidates.Add(components.ElementAt(comp.index));
                    }
                    //if we didn't, look on the next one
                } while (indexes.Count > 0 && !found);
            }
        }

        private void candidatesToConfirmedSpread()
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                confirmed.Add(candidates.ElementAt(i));
                confirmed.ElementAt(confirmed.Count - 1).biomeID = candidates.ElementAt(i).biomeID;
            }
            candidates.Clear();
            confirmed.Sort(comparer);
        }

        private bool hasUnoccupiedLand()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components.ElementAt(i).isLand && components.ElementAt(i).biomeID == -1)
                {
                    return true;
                }
            }
            return false;
        }

        private int spread()
        {
            bool quit = false;
            int leadCounter = 10000;
            //All my Land cells must be occupied and all the biomes have to come to a halt
            while (!quit && leadCounter != 0)
            {
                Console.WriteLine("New while loop iteration started");

                setNewBiomeCandidates();
                candidates.Sort(comparer);

                if (!checkColdWarm())
                {
                    Console.WriteLine("ColdWarm check failed");
                    componentReset();
                    trialCounter++;
                    if (trialCounter == 10)
                        return -1;
                }
                Console.WriteLine("ColdWarm check completed");

                if (!fitPercentages())
                {
                    Console.WriteLine("Percentage check failed");
                    componentReset();
                    trialCounter++;
                    if (trialCounter == 10)
                        return -1;
                }
                Console.WriteLine("Percentage fit");

                leadCounter--;
                candidatesToConfirmedSpread();
                if (!hasUnoccupiedLand())
                {
                    Console.WriteLine("Available to quit");
                    quit = true;
                }
            }
            return 0;
        }

        private void setHeightPreInit(int mountainBiomesCount, List<int> mountainBiomesIDs, int minimaxHeight)
        {
            for (int i = 0; i < components.Count(); i++)  //first set heights randomly
            {
                if (components.ElementAt(i).biomeID != 0)
                {
                    if (mountainBiomesCount != 0)
                        components.ElementAt(i).height = rand.Next(minimumHeight, maximumHeight);
                    else
                    {
                        if (mountainBiomesIDs.Contains(components.ElementAt(i).biomeID))
                            components.ElementAt(i).height = rand.Next(minimaxHeight, maximumHeight);
                        else
                            components.ElementAt(i).height = rand.Next(minimumHeight, minimaxHeight);
                    }
                }
            }
        }

        private bool heightBalance(RectMapPart part, List<int> mountainBiomesIDs, double thirdBorder, double quaterBorder,
            int componentPosition)
        {
            bool breaker = true;
            MapComponent comp;
            int count = 0;
            do  //we look for a bordering face with an incorrect height
            {
                //look for the face first
                RectMapPart tw = parts.Where(p => p.index == part.neighborIndexes.ElementAt(count)).First();
                comp = components.Find(k => k.face.Equals(tw) && k.isLand);
                if (!components.Contains(comp))  // if we found it, look at the heights
                    count++;
                //If both biomes are mountain ones, don't change anything.
                //For them, height differences is okay
                else if (mountainBiomesIDs.Contains(comp.biomeID) &&
                    mountainBiomesIDs.Contains(components.ElementAt(componentPosition).biomeID))
                    count++;
                else if (Math.Abs(comp.height - components.ElementAt(componentPosition).height) < thirdBorder)
                    count++;
                else
                {
                    if (comp.height > components.ElementAt(componentPosition).height)
                    {
                        components.ElementAt(componentPosition).height += quaterBorder;  //change the height
                        breaker = false;  //flip the trigger for the loop to continue
                        count++;  //look on the next neighbour
                    }
                    else
                    {
                        components.ElementAt(componentPosition).height -= quaterBorder;
                        breaker = false;
                        count++;
                    }
                }
            } while (count < part.neighborIndexes.Count);
            return breaker;
        }

        private void heightSet()
        {
            Console.WriteLine("Height generator reached");
            int minimaxHeight = 0;
            int mountainBiomes = selectedBiomes.Where(b => b.name.Contains("Mountains")).Count();
            List<int> mountainBiomesIDs = new List<int>();
            if (mountainBiomes == 0)
                minimaxHeight = maximumHeight / 3 * 2;  //let's cut the third of max height if there're no mountains biomes
            else
                mountainBiomesIDs.AddRange(selectedBiomes.Where(b => b.name.Contains("Mountains")).Select(b => b.ID));
            double difference = maximumHeight - minimumHeight;
            double thirdBorder = difference / 3;
            double quaterBorder = difference / 4;

            setHeightPreInit(mountainBiomes, mountainBiomesIDs, minimaxHeight);
            Console.WriteLine("Pre-init finished");

            int outerTrigger = 100;  //I'll give this loop a limit too
            bool innerTrigger = false;  //this trigger here will check if something's changed or not
                                      //As long as something's changing, it will keep the loop running
            while (outerTrigger != 0 && !innerTrigger)  //The Land cells must have their height balanced
            { 
                for (int i = 0; i < components.Count(); i++)
                {
                    if (components.ElementAt(i).isLand)
                    {
                        RectMapPart part = components.ElementAt(i).face;
                        innerTrigger = heightBalance(part, mountainBiomesIDs, thirdBorder, quaterBorder, i);
                    }
                }
                Console.WriteLine("Iteration completed");
                outerTrigger--;
            }
            Console.WriteLine("Height generator quit");
        }

        public List<MapComponent> run()
        {
            while (true)
            {
                Console.WriteLine("New main loop iter started");
                preInit();
                int trigger = spread();
                if (trigger == 0)
                    break;
                else
                {
                    trialCounter = 0;
                    confirmed.Clear();
                    candidates.Clear();
                    percentages.Clear();
                    setBiomes.Clear();
                    for (int i = 0; i < components.Count; i++)
                        if (components.ElementAt(i).isLand)
                            components.ElementAt(i).biomeID = -1;
                }
            }
            heightSet();
            return components;
        }
    }
}
