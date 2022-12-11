using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using VoronoiMapTrial.Physical_Map;

namespace VoronoiMapTrial
{
    public partial class ResourceSelectForm : Form
    {
        MainForm formToSend;
        private List<Resource> resources = new List<Resource>();
        public ResourceSelectForm(List<Resource> res, MainForm form)
        {
            InitializeComponent();
            resources = res;
            formToSend = form;
            for (int i = 0; i < resources.Count(); i++)
            {
                ResourceGridView.Rows.Add("-", resources.ElementAt(i).name, resources.ElementAt(i).type.type);
            }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            List<Resource> outputRes = new List<Resource>();
            for (int i = 0; i < ResourceGridView.Rows.Count; i++)
            {
                if (ResourceGridView.Rows[i].Cells[0].Value != null)
                {
                    if (ResourceGridView.Rows[i].Cells[0].Value.Equals("+"))
                    {
                        outputRes.Add(resources.ElementAt(i));
                    }
                }
            }
            if (outputRes.Count > 0)
                formToSend.setResources(outputRes);
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ResourceGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (ResourceGridView.CurrentCell.ColumnIndex == 0 && ResourceGridView.CurrentCell.Value != null)
            {
                if (!(ResourceGridView.CurrentCell.Value.Equals("+")) && !(ResourceGridView.CurrentCell.Value.Equals("-")) &&
                    (int)ResourceGridView.CurrentCell.Value != 1 && (int)ResourceGridView.CurrentCell.Value != 0 &&
                    !(ResourceGridView.CurrentCell.Value).Equals("Yes") && !(ResourceGridView.CurrentCell.Value).Equals("No"))
                {
                    e.Cancel = true;
                    ErrorLabel.Text = "Error:\nERROR! The Check value must be + or 1 or Yes, or - or 0 or No!";
                }
            }
        }
    }
}
