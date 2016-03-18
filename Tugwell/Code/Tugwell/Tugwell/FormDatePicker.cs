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
    public partial class FormDatePicker : Form
    {
        public FormDatePicker(string dateString)
        {
            InitializeComponent();

            setCurrentDate(dateString);
            displayDate();
        }

        public bool IsConfirmed = false;
        public string Date = "";

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            this.IsConfirmed = true;
            this.Close();
        }

        private void setCurrentDate(string dateString)
        {
            try
            {
                DateTime d = Convert.ToDateTime(dateString);
                this.monthCalendar1.SetDate(d);
            }
            catch { }
        }           

        private void displayDate()
        {
            this.textBoxDate.Text = this.monthCalendar1.SelectionRange.Start.ToString(@"MM/dd/yyyy");
            this.Date = this.textBoxDate.Text;
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            displayDate();
        }
    }
}
