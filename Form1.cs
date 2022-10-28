using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Collections;

using VoronoiMapTrial.Economical_Map;
using VoronoiMapTrial.Physical_Map;

namespace VoronoiMapTrial
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
        }

        int width;
        int height;
        Bitmap map;
        Graphics graph;
        Pen pen = new Pen(Color.Black, 5);
        List<VoronoiPoint> points = new List<VoronoiPoint>();
        static int distance = 4;
        int radius = distance * 2 + 1;

        private void ClearButton_Click(object sender, EventArgs e)
        {
            points.Clear();
            graph.Clear(Color.Transparent);
            PictureBox.Image = map;
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            //This is where all other generators will be
            //This project is growing bigger than I thought it would
        }

        private unsafe void GenerateButton_Click(object sender, EventArgs e)
        {
            if (points.Count != 0)
            {
                points.Clear();
                graph.Clear(Color.Transparent);
            }

            width = PictureBox.Width;
            height = PictureBox.Height;
            map = new Bitmap(width, height);
            map.MakeTransparent();
            graph = Graphics.FromImage(map);
            //pen = new Pen(Color.Black, 5);
            int number = (int)PointNumNumericUpDown.Value;
            if (number < 10)
                number = 10;
            
            if (RandomCheckBox.Checked)
            {
                Random rand = new Random();
                for (int i = 0; i < number; i++)
                {
                    //IDEA make a distance between the point and the border?
                    int x = rand.Next(distance, width - distance);
                    int y = rand.Next(distance, height - distance);
                    //points.Add(new Vector2(x, y));
                    VoronoiPoint point;
                    point.index = -1;
                    point.coords = new Vector2(x, y);
                    point.face = null;
                    points.Add(point);

                    graph.DrawEllipse(pen, x - distance, y - distance, radius, radius);
                }
            }
            PictureBox.Image = map;
        }

        private unsafe void PictureBox_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                width = PictureBox.Width;
                height = PictureBox.Height;
                map = new Bitmap(width, height);
                map.MakeTransparent();
                graph = Graphics.FromImage(map);
            }
            MouseEventArgs mouseEvent = (MouseEventArgs)e;
            VoronoiPoint point;
            point.index = -1;
            point.coords  = new Vector2(mouseEvent.X, mouseEvent.Y);
            point.face = null;
            points.Add(point);
            int x = mouseEvent.X - distance >= 0 ? mouseEvent.X - distance : 0;
            int y = mouseEvent.Y - distance >= 0 ? mouseEvent.Y - distance : 0;
            int w = x + radius < width ? radius : width - x;
            int h = y + radius < height ? radius : height - y;
            graph.DrawEllipse(pen, x, y, w, h);
            PictureBox.Image = map;
        }

        private void BorderGenButton_Click(object sender, EventArgs e)
        {
            if (points.Count != 0)
            {
                TerritoryDivision divide = new TerritoryDivision(points, map);
                Bitmap newMap = divide.run();
                PictureBox.Image = newMap;
            }
        }
    }
}
