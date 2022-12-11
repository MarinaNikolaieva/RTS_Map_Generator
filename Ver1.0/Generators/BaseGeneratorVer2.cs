using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using VoronoiMapTrial.BaseMapVer2;

namespace VoronoiMapTrial.Generators
{
    public class BaseGeneratorVer2
    {
        int width;
        int height;

        double proportion;
        int elementWidth;
        int elementHeight;

        public BaseGeneratorVer2(int width, int height, int div)
        {
            this.width = width;
            this.height = height;
            //div is in how many parts is the Width divided into

            proportion = (double)this.width / (double)this.height;
            elementWidth = this.width / div;
            elementHeight = (int)(elementWidth / proportion);
        }

        private List<Vector2> formCorners(int upLeftX, int upLeftY, int downRightX, int downRightY)
        {
            Vector2 upLeft = new Vector2(upLeftX, upLeftY);
            Vector2 upRight = new Vector2(downRightX, upLeftY);
            Vector2 downLeft = new Vector2(upLeftX, downRightY);
            Vector2 downRight = new Vector2(downRightX, downRightY);
            List<Vector2> list = new List<Vector2>();
            list.Add(upLeft); list.Add(upRight); list.Add(downRight); list.Add(downLeft);
            return list;
        }

        private RectMapPart formMapPart(int upLeftX, int upLeftY, int downRightX, int downRightY, int counter)
        {
            //creating vertices - center and corners
            int centerX = downRightX - elementWidth / 2;
            int centerY = downRightY - elementHeight / 2;
            List<Vector2> list = formCorners(upLeftX, upLeftY, downRightX, downRightY);
            //forming a part and adding it to the list
            RectMapPart part = new RectMapPart(counter, new Vector2(centerX, centerY), list);
            return part;
        }

        public List<RectMapPart> formAllParts(int heightElementNum, int widthElementNum,
            int upLeftX, int upLeftY, int downRightX, int downRightY)
        {
            int counter = 0;
            List<RectMapPart> partList = new List<RectMapPart>();
            for (int i = 0; i < heightElementNum; i++)
            {
                for (int j = 0; j < widthElementNum; j++)
                {
                    RectMapPart part = formMapPart(upLeftX, upLeftY, downRightX, downRightY, counter);
                    partList.Add(part);
                    //preparing for the next step
                    //We're moving towards the Width, the X coordinate
                    counter++;
                    upLeftX += elementWidth;
                    downRightX += elementWidth;
                }
                //Here we have to move towards the Height, the Y coordinate
                upLeftY += elementHeight;
                downRightY += elementHeight;
                upLeftX = 0;
                downRightX = elementWidth - 1;
            }
            return partList;
        }

        private void setNeighbors(List<RectMapPart> partList, int widthElementNum)
        {
            for (int i = 0; i < partList.Count; i++)
            {
                if (partList.ElementAt(i).cornerCoords.ElementAt(0).X != 0)
                {
                    partList.ElementAt(i).neighborIndexes.Add(i - 1);
                }
                if (partList.ElementAt(i).cornerCoords.ElementAt(0).Y != 0)
                {
                    partList.ElementAt(i).neighborIndexes.Add(i - widthElementNum);
                }
                if (partList.ElementAt(i).cornerCoords.ElementAt(2).X != width - 1)
                {
                    partList.ElementAt(i).neighborIndexes.Add(i + 1);
                }
                if (partList.ElementAt(i).cornerCoords.ElementAt(2).Y != height - 1)
                {
                    partList.ElementAt(i).neighborIndexes.Add(i + widthElementNum);
                }
            }
        }

        public List<RectMapPart> run()
        {
            List<RectMapPart> partList = new List<RectMapPart>();
            int widthElementNum = width / elementWidth;
            int heightElementNum = height / elementHeight;
            int upLeftX = 0;
            int upLeftY = 0;
            int downRightX = elementWidth - 1;
            int downRightY = elementHeight - 1;

            partList.AddRange(formAllParts(heightElementNum, widthElementNum, upLeftX, upLeftY, downRightX, downRightY));

            //Set the neighbours of each part. Neighbour = element that shares the SIDE with ours
            setNeighbors(partList, widthElementNum);

            return partList;
        }
    }
}
