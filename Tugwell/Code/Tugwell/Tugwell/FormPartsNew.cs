using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tugwell
{
    public partial class FormPartsNew : Form
    {
        public FormPartsNew(string category, string description, string price)
        {
            InitializeComponent();


            this.textBoxCategory.Text = category;
            this.textBoxDescription.Text = description;
            this.numericUpDownPrice.Text = price;

            if (category == "")
            {
                this.textBoxCategory.ReadOnly = false;
                this.textBoxDescription.ReadOnly = true;
                this.numericUpDownPrice.ReadOnly = true;
            }
            else
            {
                this.textBoxCategory.ReadOnly = true;
                if (description != "")
                    this.textBoxDescription.ReadOnly = true;
                this.numericUpDownPrice.ReadOnly = false;
            }
        }

        public bool IsConfirmed = false;

        public string _Category = "";
        public string _Description = "";
        public string _Price = "";

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            if (!this.textBoxCategory.ReadOnly)
                if (this.textBoxCategory.Text == "")
                    return;

            if (!this.textBoxDescription.ReadOnly)
                if (this.textBoxDescription.Text == "")
                    return;

            this.IsConfirmed = true;

            this._Category = this.textBoxCategory.Text;
            this._Description = this.textBoxDescription.Text;
            this._Price = this.numericUpDownPrice.Text;

            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
