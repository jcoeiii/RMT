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
    public partial class FormCompanyNew : Form
    {
        public FormCompanyNew(string company, string street1, string street2, string city, string state, string zip, string phone, string fax)
        {
            InitializeComponent();

            this.textBoxSoldTo.Text = company;
            this.textBoxStreet1.Text = street1;
            this.textBoxStreet2.Text = street2;
            this.textBoxCity.Text = city;
            this.comboBoxState.Text = state;
            this.textBoxZip.Text = zip;
            this.textBoxPhone.Text = phone;
            this.textBoxFax.Text = fax;

            if (company != "")
                this.textBoxSoldTo.ReadOnly = true;
        }

        public bool IsConfirmed = false;

        public string _CompanyName = "";
        public string _Street1 = "";
        public string _Street2 = "";
        public string _City = "";
        public string _State = "";
        public string _Zip = "";
        public string _Phone = "";
        public string _Fax = "";

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            if (!this.textBoxSoldTo.ReadOnly)
                if (this.textBoxSoldTo.Text == "")
                    return;

            this.IsConfirmed = true;

            this.textBoxPhone.Text = cleanNumber(this.textBoxPhone.Text);
            this.textBoxFax.Text = cleanNumber(this.textBoxFax.Text);

            this._CompanyName = this.textBoxSoldTo.Text;
            this._Street1 = this.textBoxStreet1.Text;
            this._Street2 = this.textBoxStreet2.Text;
            this._City = this.textBoxCity.Text;
            this._State = this.comboBoxState.Text;
            this._Zip = this.textBoxZip.Text;
            this._Phone = this.textBoxPhone.Text;
            this._Fax = this.textBoxFax.Text;

            this.Close();
        }

        private string cleanNumber(string value)
        {
            value = value.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (value.Length == 7)
            {
                return "(850) " + value.Substring(0, 3) + "-" + value.Substring(3, 4);
            }
            else if (value.Length == 10)
            {
                return "(" + value.Substring(0, 3) + ") " + value.Substring(3, 3) + "-" + value.Substring(6, 4);
            }
            else
            {
                return value;
            }
        }
    }
}
