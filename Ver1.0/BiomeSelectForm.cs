using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using VoronoiMapTrial.Physical_Map;

namespace VoronoiMapTrial
{
    public partial class BiomeSelectForm : System.Windows.Forms.Form
    {
        MainForm formToSend;
        private List<Biome> biomes = new List<Biome>();
        private List<int> activeIndexes = new List<int>();

        public BiomeSelectForm(List<Biome> res, MainForm form)
        {
            InitializeComponent();
            biomes = res;
            formToSend = form;
            for (int i = 0; i < biomes.Count(); i++)
            {
                if (!biomes.ElementAt(i).name.Contains("Sea"))
                {
                    activeIndexes.Add(i);
                    BiomeDataGrid.Rows.Add("-", biomes.ElementAt(i).name, biomes.ElementAt(i).type.type, biomes.ElementAt(i).currentWeight);
                }
                }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            List<Biome> outputBiomes = new List<Biome>();
            for (int i = 0; i < BiomeDataGrid.Rows.Count; i++)
            {
                if (BiomeDataGrid.Rows[i].Cells[0].Value != null)
                {
                    if (BiomeDataGrid.Rows[i].Cells[0].Value.Equals("+"))
                    {
                        outputBiomes.Add(biomes.ElementAt(activeIndexes.ElementAt(i)));
                        outputBiomes.ElementAt(outputBiomes.Count() - 1).currentWeight = Convert.ToInt32(BiomeDataGrid.Rows[i].Cells[3].Value);
                    }
                }
            }
            if (outputBiomes.Count > 0)
                formToSend.setBiomes(outputBiomes);
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Here come the restrictions
        //We have to:
        //1. Fix the Check params with either +/-, Yes/No/, 1/0
        //2. Restrict the Weight parameter to > 0 and < 1000

        private void BiomeDataGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (BiomeDataGrid.CurrentCell.ColumnIndex == 0 && BiomeDataGrid.CurrentCell.Value != null)
            {
                if (!(BiomeDataGrid.CurrentCell.Value.Equals("+")) && !(BiomeDataGrid.CurrentCell.Value.Equals("-")) &&
                    (int)BiomeDataGrid.CurrentCell.Value != 1 && (int)BiomeDataGrid.CurrentCell.Value != 0 &&
                    !(BiomeDataGrid.CurrentCell.Value).Equals("Yes") && !(BiomeDataGrid.CurrentCell.Value).Equals("No"))
                {
                    e.Cancel = true;
                    ErrorLabel.Text = "Error:\nERROR! The Check value must be + or 1 or Yes, or - or 0 or No!";
                }
            }
            if (BiomeDataGrid.CurrentCell.ColumnIndex == 3)
            {
                if ((int)BiomeDataGrid.CurrentCell.Value <= 0 || (int)BiomeDataGrid.CurrentCell.Value > 1000)
                {
                    e.Cancel = true;
                    ErrorLabel.Text = "Error:\nERROR! The Weight value must be between 1 and 1000!";
                }
            }
        }
    }
}
