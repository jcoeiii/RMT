using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tugwell
{
    public partial class CustomComboBox : ComboBox
    {
        public CustomComboBox()
        {
            InitializeComponent();

            this.SelectedIndexChanged += new System.EventHandler(this.CustomComboBox_SelectedIndexChanged);
            this.DropDownStyle = ComboBoxStyle.DropDown;
            this.BackColor = System.Drawing.SystemColors.Window;
        }

        private bool Locked = false;

        public override string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                if (!String.IsNullOrEmpty(value) && this.Items.Contains(value))
                {
                    this.Locked = true;
                    this.BackColor = System.Drawing.SystemColors.InactiveCaption;
                }
                else
                {
                    this.Locked = false;
                    this.BackColor = System.Drawing.SystemColors.Window;
                }

                base.Text = value;
            }
        }

        private void CustomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // show locked state if index is 1+
            if (this.SelectedIndex > 0)
            {
                this.Locked = true;
                this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            }
            else
            {
                this.Locked = false;
                this.BackColor = System.Drawing.SystemColors.Window;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = this.Locked;
            base.OnKeyPress(e);
        }
    }
}