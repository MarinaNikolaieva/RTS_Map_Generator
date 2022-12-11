using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Numerics;
using System.IO;

using VoronoiMapTrial.Physical_Map;
using VoronoiMapTrial.Generators;
using VoronoiMapTrial.BaseMapVer2;

namespace VoronoiMapTrial
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        static int width;
        static int height;

        static int defaultWidth = 1920;
        static int defaultHeight = 1080;

        Bitmap map = new Bitmap(defaultWidth, defaultHeight);
        Bitmap resMap = new Bitmap(defaultWidth, defaultHeight);
        Graphics graph;
        Graphics resGraph;
        Pen smallPen = new Pen(Color.Black, 2);

        List<MapComponent> mapComponents = new List<MapComponent>();
        List<RectMapPart> mapParts = new List<RectMapPart>();

        List<Biome> biomes = new List<Biome>();
        List<Resource> resources = new List<Resource>();

        List<Biome> selectedBiomes = new List<Biome>();
        List<Resource> selectedResources = new List<Resource>();

        string selectedFolderAddress = "";

        //Some important triggers!
        bool baseGenerated = false;  //have we generated the basis of the map?
        bool seaLandGenerated = false;  //have we made a split between land & sea?
        bool biomesGenerated = false;  //have we generated the biome map?
        bool resourceGenerated = false;  //have we generated the resource map?

        private void Init()
        {
            //What if I make biomes have their color codes in-built? Why not?
            string location = System.Reflection.Assembly.GetEntryAssembly().Location;
            string executableDirectory = Path.GetDirectoryName(location);
            string path = executableDirectory.Replace("\\bin\\Debug", "");
            string biomepath = path + "\\DataFiles\\Biomes.txt";
            string respath = path + "\\DataFiles\\Resources.txt";
            foreach (string line in System.IO.File.ReadAllLines(biomepath))
            {
                string[] parts = line.Split(' ');
                if (parts[2].Equals("WARM") || parts[2].Equals("COLD") || parts[2].Equals("NEUTRAL"))
                {
                    Color c = ColorTranslator.FromHtml(parts[3]);
                    BiomeType t = new BiomeType();
                    switch (parts[2])
                    {
                        case "NEUTRAL":
                            t.type = BiomeType.Type.NEUTRAL;
                            break;
                        case "COLD":
                            t.type = BiomeType.Type.COLD;
                            break;
                        case "WARM":
                            t.type = BiomeType.Type.WARM;
                            break;
                    }
                    biomes.Add(new Biome(t, parts[1], Convert.ToInt32(parts[0]), c));
                }
            }

            foreach (string line in System.IO.File.ReadAllLines(respath))
            {
                string[] parts = line.Split(' ');
                if (parts[2].Equals("LIQUID") || parts[2].Equals("GAS") || parts[2].Equals("ONGROUND") || parts[2].Equals("UNDERGROUND"))
                {
                    ResourceType t = new ResourceType();
                    switch (parts[2])
                    {
                        case "LIQUID":
                            t.type = ResourceType.Type.LIQUID;
                            break;
                        case "GAS":
                            t.type = ResourceType.Type.GAS;
                            break;
                        case "ONGROUND":
                            t.type = ResourceType.Type.ONGROUND;
                            break;
                        case "UNDERGROUND":
                            t.type = ResourceType.Type.UNDERGROUND;
                            break;
                    }
                    resources.Add(new Resource(t, parts[1], Convert.ToInt32(parts[0]), parts[3], 1));
                }
            }
        }

        public MainForm()
        {
            InitializeComponent();
            CurHeightLabel.Text = "Current height: " + MapHeightNumUpDown.Value.ToString();
            CurWidthLabel.Text = "Current width: " + MapWidthNumUpDown.Value.ToString();
            width = (int)MapWidthNumUpDown.Value;
            height = (int)MapHeightNumUpDown.Value;
            Init();
        }

        public void setBiomes(List<Biome> b)
        {
            selectedBiomes.Clear();
            selectedBiomes.AddRange(b);
            for (int i = 0; i < selectedBiomes.Count(); i++)
            {
                biomes.First(bi => bi.ID == selectedBiomes.ElementAt(i).ID).currentWeight = 
                    selectedBiomes.ElementAt(i).currentWeight;
            }
            SelectionBiomeLabel.Text = "You have " + b.Count() + " biomes selected";
        }

        public void setResources(List<Resource> r)
        {
            selectedResources.Clear();
            selectedResources.AddRange(r);
            ResourceSelectLabel.Text = "You have " + r.Count() + " resources selected";
        }

        private void drawBaseVer2(bool biome)
        {
            for (int i = 0; i < mapParts.Count(); i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Vector2 p1 = mapParts.ElementAt(i).cornerCoords.ElementAt(j);
                    Vector2 p2 = mapParts.ElementAt(i).cornerCoords.ElementAt(j + 1 < 4 ? j + 1 : 0);
                    if (biome)
                        graph.DrawLine(smallPen, p1.X, p1.Y, p2.X, p2.Y);
                    else
                        resGraph.DrawLine(smallPen, p1.X, p1.Y, p2.X, p2.Y);
                }
            }

            if (biome)
                PictureBox.Image = map;
            else
                PictureBox.Image = resMap;
        }

        private void drawBiomesVer2()
        {
            graph.Clear(Color.Transparent);
            for (int i = 0; i < mapComponents.Count; i++)
            {
                List<PointF> vertices = new List<PointF>();
                for (int j = 0; j < 4; j++)
                    vertices.Add(new PointF(mapComponents.ElementAt(i).face.cornerCoords.ElementAt(j).X,
                        mapComponents.ElementAt(i).face.cornerCoords.ElementAt(j).Y));
                Brush colorPen = new SolidBrush(biomes.Where(b => b.ID == mapComponents.ElementAt(i).biomeID).Select(b => b.color).First());
                graph.FillPolygon(colorPen, vertices.ToArray());
            }
            PictureBox.Image = map;
            ToggleMapRadButton1.Checked = true;
            ToggleMapRadButton2.Checked = false;
        }

        private void drawResourcesVer3()
        {
            int divideNum = (int)PointNumNumericUpDown.Value;

            resGraph.Clear(Color.White);

            Font font = new Font("Consolas", 32);
            for (int i = 0; i < selectedResources.Count; i++)
            {
                Vector2 symbolPlace = new Vector2(0, 0);  //This will be the coordinates of the first cell with all 4 neighbours
                bool found = false;
                for (int j = 0; j < mapComponents.Count; j++)
                {
                    if (mapComponents.ElementAt(j).resourceID == selectedResources.ElementAt(i).ID)
                    {
                        //Cell's neighbours are placed like LEFT-UP-RIGHT-DOWN if all 4 are present
                        //We need to detect which of the neighbours are there
                        MapComponent comp = mapComponents.ElementAt(j);
                        if (comp.face.neighborIndexes.Count == 4 && !found)
                        {
                            symbolPlace = comp.face.centerCoords;
                            found = true;
                        }

                        if (!comp.face.neighborIndexes.Contains(comp.index - 1) ||
                            mapComponents.ElementAt(comp.index - 1).resourceID != selectedResources.ElementAt(i).ID)  //Left
                        {
                            Vector2 p1 = mapParts.ElementAt(j).cornerCoords.ElementAt(3);  //DownLeft
                            Vector2 p2 = mapParts.ElementAt(j).cornerCoords.ElementAt(0);  //UpLeft
                            resGraph.DrawLine(smallPen, p1.X, p1.Y, p2.X, p2.Y);
                        }
                        if (!comp.face.neighborIndexes.Contains(comp.index + 1) ||
                            mapComponents.ElementAt(comp.index + 1).resourceID != selectedResources.ElementAt(i).ID)  //Right
                        {
                            Vector2 p1 = mapParts.ElementAt(j).cornerCoords.ElementAt(1);  //UpRight
                            Vector2 p2 = mapParts.ElementAt(j).cornerCoords.ElementAt(2);  //DownRight
                            resGraph.DrawLine(smallPen, p1.X, p1.Y, p2.X, p2.Y);
                        }
                        if (!comp.face.neighborIndexes.Contains(comp.index - divideNum) ||
                            mapComponents.ElementAt(comp.index - divideNum).resourceID != selectedResources.ElementAt(i).ID)  //Up
                        {
                            Vector2 p1 = mapParts.ElementAt(j).cornerCoords.ElementAt(0);
                            Vector2 p2 = mapParts.ElementAt(j).cornerCoords.ElementAt(1);
                            resGraph.DrawLine(smallPen, p1.X, p1.Y, p2.X, p2.Y);
                        }
                        if (!comp.face.neighborIndexes.Contains(comp.index + divideNum) ||
                            mapComponents.ElementAt(comp.index + divideNum).resourceID != selectedResources.ElementAt(i).ID)  //Down
                        {
                            Vector2 p1 = mapParts.ElementAt(j).cornerCoords.ElementAt(2);
                            Vector2 p2 = mapParts.ElementAt(j).cornerCoords.ElementAt(3);
                            resGraph.DrawLine(smallPen, p1.X, p1.Y, p2.X, p2.Y);
                        }
                    }

                }
                string symbol = resources.Where(r => r.ID == selectedResources.ElementAt(i).ID).Select(r => r.code).First();
                resGraph.DrawString(symbol, font, Brushes.Black, symbolPlace.X, symbolPlace.Y);
            }
            PictureBox.Image = resMap;
            ToggleMapRadButton2.Checked = true;
            ToggleMapRadButton1.Checked = false;
        }


        private void ClearButton_Click(object sender, EventArgs e)
        {
            graph.Clear(Color.Transparent);
            PictureBox.Image = map;
        }

        private void BiomeSelectButton_Click(object sender, EventArgs e)
        {
            BiomeSelectForm biomeForm;
            if (BiomeTypesRadButton1.Checked)
            {
                selectedBiomes.AddRange(biomes.Where(b => b.type.type == BiomeType.Type.NEUTRAL));
                biomeForm = new BiomeSelectForm(selectedBiomes, this);
                selectedBiomes.Clear();

            }
            else if (BiomeTypesRadButton2.Checked)
            {
                selectedBiomes.AddRange(biomes.Where(b => b.type.type == BiomeType.Type.WARM));
                selectedBiomes.Add(biomes.Where(b => b.ID == 0).First());
                biomeForm = new BiomeSelectForm(selectedBiomes, this);
                selectedBiomes.Clear();

            }
            else if (BiomeTypesRadButton3.Checked)
            {
                selectedBiomes.AddRange(biomes.Where(b => b.type.type == BiomeType.Type.COLD));
                selectedBiomes.Add(biomes.Where(b => b.ID == 0).First());
                biomeForm = new BiomeSelectForm(selectedBiomes, this);
                selectedBiomes.Clear();

            }
            else
            {
                biomeForm = new BiomeSelectForm(biomes, this);
            }

            biomeForm.Show();
        }

        private void ResSelectButton_Click(object sender, EventArgs e)
        {
            ResourceSelectForm resForm = new ResourceSelectForm(resources, this);
            resForm.Show();
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult res = dialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                selectedFolderAddress = dialog.SelectedPath;
                FolderAddressLabel.Text = selectedFolderAddress;
            }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            CurHeightLabel.Text = "Current height: " + MapHeightNumUpDown.Value;
            CurWidthLabel.Text = "Current width: " + MapWidthNumUpDown.Value;
            width = (int)MapWidthNumUpDown.Value;
            height = (int)MapHeightNumUpDown.Value;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            //Throw all parameters to default
            graph.Clear(Color.Transparent);
            resGraph.Clear(Color.White);
            PictureBox.Image = map;
            mapParts.Clear();
            mapComponents.Clear();
            selectedBiomes.Clear();
            selectedResources.Clear();
            MapHeightNumUpDown.Value = defaultHeight;
            MapWidthNumUpDown.Value = defaultWidth;
            MapHeightLabel.Text = "Current height: " + MapHeightNumUpDown.Value;
            MapWidthLabel.Text = "Current width: " + MapWidthNumUpDown.Value;
            SelectionBiomeLabel.Text = "You have 0 biomes selected";
            ResourceSelectLabel.Text = "You have 0 resources selected";
            FolderAddressLabel.Text = " ";
            ErrorLabel.Text = "Error:";
            BiomeErrorLabel.Text = "Error:";
            ResErrorLabel.Text = "Error:";
            BaseErrorLabel.Text = "Error:";
            SeaLandErrorLabel.Text = "Error:";
            FileNameTextBox.Text = "";
            selectedFolderAddress = "";

            baseGenerated = false;
            seaLandGenerated = false;
            biomesGenerated = false;
            resourceGenerated = false;
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            if (!resourceGenerated)
            {
                ErrorLabel.Text = "Error: The map hasn't been fully generated!";
                return;
            }
            if (selectedFolderAddress.Equals(""))
            {
                ErrorLabel.Text = "Error: Selected address is empty!";
                return;
            }
            if (FileNameTextBox.Text.Equals(""))
            {
                ErrorLabel.Text = "Error: File name is empty!";
                return;
            }

            string mapBFilepath = selectedFolderAddress + "\\" + FileNameTextBox.Text + "Map.jpg";
            string mapRFilepath = selectedFolderAddress + "\\" + FileNameTextBox.Text + "Res.jpg";
            string mapLegendFilepath = selectedFolderAddress + "\\" + FileNameTextBox.Text + "MapLegend.rtf";
            string resLegendFilepath = selectedFolderAddress + "\\" + FileNameTextBox.Text + "ResLegend.txt";

            if (File.Exists(mapBFilepath) || File.Exists(mapRFilepath) || File.Exists(mapLegendFilepath) ||
                File.Exists(resLegendFilepath))
            {
                ErrorLabel.Text = "Error: The file already exists!";
                return;
            }

            else
            {
                Downloader downloader = new Downloader(biomes, resources, map, resMap, mapBFilepath, 
                    mapRFilepath, mapLegendFilepath, resLegendFilepath);
                downloader.download();
            }
        }

        private void ToggleMapRadButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (ToggleMapRadButton1.Checked == true)
            {
                ToggleMapRadButton2.Checked = false;
                drawBiomesVer2();
            }
            else
            {
                ToggleMapRadButton2.Checked = true;
                drawResourcesVer3();
            }
        }

        private void ToggleMapRadButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (ToggleMapRadButton2.Checked == true)
            {
                ToggleMapRadButton1.Checked = false;
                drawResourcesVer3();
            }
            else
            {
                ToggleMapRadButton1.Checked = true;
                drawBiomesVer2();
            }
        }

        private void BiomeTypesRadButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (BiomeTypesRadButton1.Checked == true)
            {
                BiomeTypesRadButton2.Checked = false;
                BiomeTypesRadButton3.Checked = false;
                BiomeTypesRadButton4.Checked = false;
            }
        }

        private void BiomeTypesRadButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (BiomeTypesRadButton2.Checked == true)
            {
                BiomeTypesRadButton1.Checked = false;
                BiomeTypesRadButton3.Checked = false;
                BiomeTypesRadButton4.Checked = false;
            }
        }

        private void BiomeTypesRadButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (BiomeTypesRadButton3.Checked == true)
            {
                BiomeTypesRadButton1.Checked = false;
                BiomeTypesRadButton2.Checked = false;
                BiomeTypesRadButton4.Checked = false;
            }
        }

        private void BiomeTypesRadButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (BiomeTypesRadButton4.Checked == true)
            {
                BiomeTypesRadButton1.Checked = false;
                BiomeTypesRadButton2.Checked = false;
                BiomeTypesRadButton3.Checked = false;
            }
        }

        private void BaseMapGenButton_Click(object sender, EventArgs e)
        {
            if (mapParts.Count != 0)
            {
                mapParts.Clear();
                mapComponents.Clear();
                selectedBiomes.Clear();
                selectedResources.Clear();
                graph.Clear(Color.Transparent);
                resGraph.Clear(Color.Transparent);
            }

            map = new Bitmap(width, height);
            map.MakeTransparent();
            graph = Graphics.FromImage(map);

            resMap = new Bitmap(width, height);
            resMap.MakeTransparent();
            resGraph = Graphics.FromImage(resMap);

            if (PointNumNumericUpDown.Value > width)
                BaseErrorLabel.Text = "Error: The number of divisions must be less than map width!";
            else
            {
                int divideNum = (int)PointNumNumericUpDown.Value;

                double proportion = (double)width / (double)height;
                int elementWidth = width / divideNum;
                int elementHeight = (int)(elementWidth / proportion);
                int heightCut = height / elementHeight;

                //The width and height may not divide good, I need to round it down
                width = elementWidth * divideNum;
                height = elementHeight * heightCut;

                CurHeightLabel.Text = "Current height: " + height;
                CurWidthLabel.Text = "Current width: " + width;
                MapWidthNumUpDown.Value = width;
                MapHeightNumUpDown.Value = height;

                BaseGeneratorVer2 bsG = new BaseGeneratorVer2(width, height, (int)PointNumNumericUpDown.Value);
                mapParts.AddRange(bsG.run());
                for (int i = 0; i < mapParts.Count; i++)
                {
                    mapComponents.Add(new MapComponent(mapParts.ElementAt(i).index, mapParts.ElementAt(i)));
                }
                graph.Clear(Color.Transparent);
                resGraph.Clear(Color.Transparent);
                drawBaseVer2(true);
                drawBaseVer2(false);
                baseGenerated = true;
            }
        }

        private void BiomeGenButton_Click(object sender, EventArgs e)
        {
            if (!seaLandGenerated)
                BiomeErrorLabel.Text = "Error: The map isn't split between sea and land!";
            else if (MinHeightNumUpDown.Value >= MaxHeightNumUpDown.Value)
                BiomeErrorLabel.Text = "Error: Maximum height value must be bigger than minimum!";
            else if (selectedBiomes.Count == 0)
                BiomeErrorLabel.Text = "Error: You have no biomes selected!";
            else if (selectedBiomes.Count > mapComponents.Count)
                BiomeErrorLabel.Text = "Error: Number of selected biomes must be less than number of cells!";
            else
            {
                bool hasNeutrals = false;
                for (int i = 0; i < selectedBiomes.Count; i++)
                {
                    if (selectedBiomes.ElementAt(i).type.type == BiomeType.Type.NEUTRAL)
                    {
                        hasNeutrals = true;
                        break;
                    }
                }
                if (!hasNeutrals)
                    BiomeErrorLabel.Text = "Error: You have no neutral biomes to generate!";
                else
                {
                    if (biomesGenerated)  //to prevent the re-generating failures
                    {
                        for (int i = 0; i < mapComponents.Count; i++)
                            if (mapComponents.ElementAt(i).isLand)
                                mapComponents.ElementAt(i).biomeID = -1;
                    }

                    double totalArea = height * width;
                    BiomeGenerator biomeGen = new BiomeGenerator(mapComponents, mapParts, selectedBiomes, totalArea, (int)MinHeightNumUpDown.Value, (int)MaxHeightNumUpDown.Value);
                    mapComponents.Clear();
                    mapComponents.AddRange(biomeGen.run());
                    drawBiomesVer2();
                    biomesGenerated = true;
                }
            }
        }

        private void SeaLandButton_Click(object sender, EventArgs e)
        {
            if (!baseGenerated)
                SeaLandErrorLabel.Text = "Error: Base map wasn't generated yet!";
            else
            {
                double area = height * width;
                SeaLandGenerator SLGen = new SeaLandGenerator((int)LandPercNumUpDown.Value, (int)IslandNumUpDown.Value, 2, mapParts, area);
                mapComponents.Clear();
                mapComponents.AddRange(SLGen.run());
                graph.Clear(Color.Transparent);
                for (int i = 0; i < mapComponents.Count; i++)
                {
                    List<PointF> vertices = new List<PointF>();
                    for (int j = 0; j < 4; j++)
                        vertices.Add(new PointF(mapComponents.ElementAt(i).face.cornerCoords.ElementAt(j).X,
                            mapComponents.ElementAt(i).face.cornerCoords.ElementAt(j).Y));
                    Brush colorPen;
                    if (mapComponents.ElementAt(i).isLand)
                        colorPen = new SolidBrush(Color.DarkGreen);
                    else
                        colorPen = new SolidBrush(Color.Blue);
                    graph.FillPolygon(colorPen, vertices.ToArray());
                }
                PictureBox.Image = map;
                ToggleMapRadButton1.Checked = true;
                ToggleMapRadButton2.Checked = false;
                seaLandGenerated = true;
            }
        }

        private void ResGenButton_Click(object sender, EventArgs e)
        {
            if (!biomesGenerated)
                ResErrorLabel.Text = "Error: The biome map wasn't generated yet!";
            else if (selectedResources.Count == 0)
                ResErrorLabel.Text = "Error: You have no resources selected!";
            else if (selectedResources.Count >= mapComponents.Count)
                ResErrorLabel.Text = "Error: The number of resources must be less than number of cells!";
            else
            {
                if (resourceGenerated)  //preventing the re-generation failures
                {
                    for (int i = 0; i < mapComponents.Count; i++)
                        if (mapComponents.ElementAt(i).isLand)
                            mapComponents.ElementAt(i).resourceID = -1;
                }

                ResourceGenerator resGen = new ResourceGenerator(mapComponents, mapParts, selectedResources);
                List<MapComponent> tempComps = resGen.run();
                mapComponents.Clear();
                mapComponents.AddRange(tempComps);
                tempComps.Clear();
                drawResourcesVer3();
                resourceGenerated = true;
            }
        }
    }
}
