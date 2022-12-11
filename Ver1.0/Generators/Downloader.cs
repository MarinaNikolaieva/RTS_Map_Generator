using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using VoronoiMapTrial.Physical_Map;

namespace VoronoiMapTrial.Generators
{
    public class Downloader
    {
        List<Biome> biomes = new List<Biome>();
        List<Resource> resources = new List<Resource>();
        Bitmap biomeMap;
        Bitmap resourceMap;

        string mapBFilePath;
        string mapRFilePath;
        string mapLegendFilePath;
        string resLegendFilePath;

        public Downloader(List<Biome> biomes, List<Resource> resources, Bitmap biomeMap, Bitmap resourceMap, string mapBFilePath, string mapRFilePath, string mapLegendFilePath, string resLegendFilePath)
        {
            this.biomes = biomes;
            this.resources = resources;
            this.biomeMap = biomeMap;
            this.resourceMap = resourceMap;
            this.mapBFilePath = mapBFilePath;
            this.mapRFilePath = mapRFilePath;
            this.mapLegendFilePath = mapLegendFilePath;
            this.resLegendFilePath = resLegendFilePath;
        }

        public void download()
        {
            //Save both files
            biomeMap.Save(mapBFilePath);
            resourceMap.Save(mapRFilePath);
            //Form legends
            //I think it doesn't have all the spectrum
            //It DOES. You just have to open the rtf file with WordPad, not MS Word
            RichTextBox rtfBox = new RichTextBox();
            rtfBox.Font = new Font("Consolas", 16);
            for (int i = 0; i < biomes.Count(); i++)
            {
                rtfBox.SelectionBackColor = biomes.ElementAt(i).color;
                rtfBox.AppendText("      ");  //6 blank spaces
                rtfBox.SelectionBackColor = Color.White;
                rtfBox.AppendText(" " + biomes.ElementAt(i).name + "\n\n");
            }

            List<string> strings = new List<string>();
            for (int i = 0; i < resources.Count(); i++)
            {
                StringBuilder build = new StringBuilder();

                build.Append(resources.ElementAt(i).code);
                build.Append(" " + resources.ElementAt(i).name);
                strings.Add(build.ToString());
            }
            //Save legends
            rtfBox.SaveFile(mapLegendFilePath);
            File.AppendAllLines(resLegendFilePath, strings);
        }
    }
}
