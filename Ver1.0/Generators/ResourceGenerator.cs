using System;
using System.Collections.Generic;
using System.Linq;

using VoronoiMapTrial.Physical_Map;
using VoronoiMapTrial.BaseMapVer2;

namespace VoronoiMapTrial.Generators
{
    public class ResourceGenerator
    {
        public List<MapComponent> mapComponents = new List<MapComponent>();
        public List<RectMapPart> mapParts = new List<RectMapPart>();
        public List<Resource> selectedResources = new List<Resource>();
        private int landCount = 0;

        public ResourceGenerator(List<MapComponent> mapComponents, List<RectMapPart> mapParts, List<Resource> selectedResources)
        {
            this.mapComponents.AddRange(mapComponents);
            this.mapParts.AddRange(mapParts);
            this.selectedResources.AddRange(selectedResources);
            for (int i = 0; i < mapComponents.Count; i++)
                if (mapComponents.ElementAt(i).isLand)
                    landCount++;
        }

        public bool isPlaced(List<MapComponent> confirmed, int cellIndex, int resourceIndex)
        {
            if (!confirmed.Contains(mapComponents.ElementAt(cellIndex)) && mapComponents.ElementAt(cellIndex).isLand)
            {
                mapComponents.ElementAt(cellIndex).resourceID = selectedResources.ElementAt(resourceIndex).ID;
                confirmed.Add(mapComponents.ElementAt(cellIndex));
                return true;
            }
            return false;
        }

        private void preInit(List<MapComponent> confirmed, Random rand)
        {
            for (int i = 0; i < selectedResources.Count(); i++)
            {
                bool found = false;
                while (!found)
                {
                    found = isPlaced(confirmed, rand.Next(mapComponents.Count), i);
                }
            }
        }

        private void resCellQuantityInit(Dictionary<int, int> dict)
        {
            for (int i = 0; i < selectedResources.Count(); i++)
                dict.Add(selectedResources.ElementAt(i).ID, 1);
        }

        private bool isCellFree(List<MapComponent> confirmed, MapComponent comp)
        {
            if (!confirmed.Contains(comp) && mapComponents.Contains(comp))  // if the cell isn't occupied, return true
            {
                return true;
            }
            //if it is, return false. The outer loop will look for the next one
            return false;
        }

        public MapComponent checkCell(Random rand, List<int> indexes, List<MapComponent> confirmed)
        {
            int curElement = rand.Next(indexes.Count);
            int curIndex = indexes.ElementAt(curElement);
            indexes.RemoveAt(curElement);  //so that they will not repeat
            RectMapPart tw = mapParts.Where(p => p.index == curIndex).First();
            MapComponent comp = mapComponents.Find(k => k.face.Equals(tw) && k.isLand && k.resourceID == -1);
            if (isCellFree(confirmed, comp))
                return comp;
            return null;
        }

        private void setResource(Dictionary<int, int> quantities, List<MapComponent> confirmed, int index, int curCellLimit,
            MapComponent comp, List<MapComponent> candidates)
        {
            //If the cell limit for the resource wasn't reached yet
            if (!(quantities[confirmed.ElementAt(index).resourceID] == curCellLimit))
            {
                comp.resourceID = confirmed.ElementAt(index).resourceID;  //set the resource
                quantities[confirmed.ElementAt(index).resourceID]++;  //increase the quantity
                candidates.Add(comp);  //add to the confirmed cells
                //and move to the next one
            }
        }

        private void setBorderIndexes(RectMapPart part, List<int> indexes)
        {
            for (int k = 0; k < part.neighborIndexes.Count; k++)
                indexes.Add(part.neighborIndexes.ElementAt(k));
        }

        private void resourceSpreading(List<MapComponent> confirmed, List<MapComponent> candidates, Random rand,
            Dictionary<int, int> quantities, int curCellLimit)
        {
            for (int j = 0; j < curCellLimit; j++)  //No point in while loop here
            {
                for (int i = 0; i < confirmed.Count(); i++)
                {
                    RectMapPart part = confirmed.ElementAt(i).face;
                    MapComponent comp = null;
                    bool found = false;
                    List<int> borderIndexes = new List<int>();
                    setBorderIndexes(part, borderIndexes);

                    do  //we try to find a face not in use yet and bordering the cell we're looking at
                    {
                        comp = checkCell(rand, borderIndexes, confirmed);
                        if (comp != null)
                        {
                            found = true;
                        }
                    } while (borderIndexes.Count > 0 && !found);

                    if (found)
                    {
                        setResource(quantities, confirmed, i, curCellLimit, comp, candidates);
                    }
                }
                confirmed.AddRange(candidates);
                candidates.Clear();
            }
        }

        public List<MapComponent> run()
        {
            List<MapComponent> confirmed = new List<MapComponent>();
            List<MapComponent> candidates = new List<MapComponent>();

            Random rand = new Random();

            //Set first cells, add resources to them
            preInit(confirmed, rand);

            //The resources are spread depending on number of cells and number of resources
            //The number of cells the resource can occupy must be limited
            int cellLimit = 30;

            int curCellLimit = landCount / selectedResources.Count() <= cellLimit ? 
                landCount / selectedResources.Count() : cellLimit;
            //PROBLEM! This will for sure result in deserted map if only one resource is selected!

            //Here is where we keep how many cells each resource has
            Dictionary<int, int> resCellQuantities = new Dictionary<int, int>();
            resCellQuantityInit(resCellQuantities);

            resourceSpreading(confirmed, candidates, rand, resCellQuantities, curCellLimit);

            return mapComponents;
        }
    }
}
