namespace Tugwell
{
    partial class FormReport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxMain = new System.Windows.Forms.GroupBox();
            this.checkBoxFilerOutTs = new System.Windows.Forms.CheckBox();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxTo = new System.Windows.Forms.TextBox();
            this.buttonRemember = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxReportName = new System.Windows.Forms.TextBox();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxPreRecorded = new System.Windows.Forms.ComboBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxField = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFrom = new System.Windows.Forms.TextBox();
            this.listViewReport = new System.Windows.Forms.ListView();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxField2 = new System.Windows.Forms.ComboBox();
            this.textBoxFilterText = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBoxMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxMain
            // 
            this.groupBoxMain.Controls.Add(this.label7);
            this.groupBoxMain.Controls.Add(this.textBoxFilterText);
            this.groupBoxMain.Controls.Add(this.label6);
            this.groupBoxMain.Controls.Add(this.comboBoxField2);
            this.groupBoxMain.Controls.Add(this.checkBoxFilerOutTs);
            this.groupBoxMain.Controls.Add(this.buttonRemove);
            this.groupBoxMain.Controls.Add(this.label5);
            this.groupBoxMain.Controls.Add(this.textBoxTo);
            this.groupBoxMain.Controls.Add(this.buttonRemember);
            this.groupBoxMain.Controls.Add(this.label4);
            this.groupBoxMain.Controls.Add(this.textBoxReportName);
            this.groupBoxMain.Controls.Add(this.buttonSelect);
            this.groupBoxMain.Controls.Add(this.label3);
            this.groupBoxMain.Controls.Add(this.comboBoxPreRecorded);
            this.groupBoxMain.Controls.Add(this.buttonAdd);
            this.groupBoxMain.Controls.Add(this.label2);
            this.groupBoxMain.Controls.Add(this.comboBoxField);
            this.groupBoxMain.Controls.Add(this.buttonCancel);
            this.groupBoxMain.Controls.Add(this.buttonGenerate);
            this.groupBoxMain.Controls.Add(this.label1);
            this.groupBoxMain.Controls.Add(this.textBoxFrom);
            this.groupBoxMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxMain.Location = new System.Drawing.Point(0, 0);
            this.groupBoxMain.Name = "groupBoxMain";
            this.groupBoxMain.Size = new System.Drawing.Size(1024, 108);
            this.groupBoxMain.TabIndex = 2;
            this.groupBoxMain.TabStop = false;
            this.groupBoxMain.Text = "Controls";
            // 
            // checkBoxFilerOutTs
            // 
            this.checkBoxFilerOutTs.AutoSize = true;
            this.checkBoxFilerOutTs.Location = new System.Drawing.Point(379, 49);
            this.checkBoxFilerOutTs.Name = "checkBoxFilerOutTs";
            this.checkBoxFilerOutTs.Size = new System.Drawing.Size(111, 17);
            this.checkBoxFilerOutTs.TabIndex = 19;
            this.checkBoxFilerOutTs.Text = "Filer out T orders?";
            this.checkBoxFilerOutTs.UseVisualStyleBackColor = true;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(870, 21);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 18;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(210, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "To:";
            // 
            // textBoxTo
            // 
            this.textBoxTo.Location = new System.Drawing.Point(239, 47);
            this.textBoxTo.Name = "textBoxTo";
            this.textBoxTo.Size = new System.Drawing.Size(105, 20);
            this.textBoxTo.TabIndex = 1;
            this.textBoxTo.DoubleClick += new System.EventHandler(this.textBoxDate_DoubleClick);
            this.textBoxTo.Leave += new System.EventHandler(this.textBoxDate_Leave);
            // 
            // buttonRemember
            // 
            this.buttonRemember.Location = new System.Drawing.Point(789, 48);
            this.buttonRemember.Name = "buttonRemember";
            this.buttonRemember.Size = new System.Drawing.Size(75, 23);
            this.buttonRemember.TabIndex = 15;
            this.buttonRemember.Text = "Remember";
            this.buttonRemember.UseVisualStyleBackColor = true;
            this.buttonRemember.Click += new System.EventHandler(this.buttonRemember_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(562, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Name:";
            // 
            // textBoxReportName
            // 
            this.textBoxReportName.Location = new System.Drawing.Point(612, 50);
            this.textBoxReportName.Name = "textBoxReportName";
            this.textBoxReportName.Size = new System.Drawing.Size(171, 20);
            this.textBoxReportName.TabIndex = 2;
            // 
            // buttonSelect
            // 
            this.buttonSelect.Location = new System.Drawing.Point(789, 21);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(75, 23);
            this.buttonSelect.TabIndex = 12;
            this.buttonSelect.Text = "Select";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(533, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "PreRecorded:";
            // 
            // comboBoxPreRecorded
            // 
            this.comboBoxPreRecorded.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPreRecorded.FormattingEnabled = true;
            this.comboBoxPreRecorded.Location = new System.Drawing.Point(612, 23);
            this.comboBoxPreRecorded.Name = "comboBoxPreRecorded";
            this.comboBoxPreRecorded.Size = new System.Drawing.Size(171, 21);
            this.comboBoxPreRecorded.TabIndex = 10;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(239, 19);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 9;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Field:";
            // 
            // comboBoxField
            // 
            this.comboBoxField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxField.FormattingEnabled = true;
            this.comboBoxField.Location = new System.Drawing.Point(62, 20);
            this.comboBoxField.Name = "comboBoxField";
            this.comboBoxField.Size = new System.Drawing.Size(171, 21);
            this.comboBoxField.TabIndex = 7;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(415, 19);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(334, 19);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(75, 23);
            this.buttonGenerate.TabIndex = 2;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "From:";
            // 
            // textBoxFrom
            // 
            this.textBoxFrom.Location = new System.Drawing.Point(62, 47);
            this.textBoxFrom.Name = "textBoxFrom";
            this.textBoxFrom.Size = new System.Drawing.Size(105, 20);
            this.textBoxFrom.TabIndex = 0;
            this.textBoxFrom.DoubleClick += new System.EventHandler(this.textBoxDate_DoubleClick);
            this.textBoxFrom.Leave += new System.EventHandler(this.textBoxDate_Leave);
            // 
            // listViewReport
            // 
            this.listViewReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewReport.FullRowSelect = true;
            this.listViewReport.GridLines = true;
            this.listViewReport.Location = new System.Drawing.Point(0, 108);
            this.listViewReport.MultiSelect = false;
            this.listViewReport.Name = "listViewReport";
            this.listViewReport.Size = new System.Drawing.Size(1024, 102);
            this.listViewReport.TabIndex = 3;
            this.listViewReport.UseCompatibleStateImageBehavior = false;
            this.listViewReport.View = System.Windows.Forms.View.Details;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Field:";
            // 
            // comboBoxField2
            // 
            this.comboBoxField2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxField2.FormattingEnabled = true;
            this.comboBoxField2.Location = new System.Drawing.Point(62, 73);
            this.comboBoxField2.Name = "comboBoxField2";
            this.comboBoxField2.Size = new System.Drawing.Size(171, 21);
            this.comboBoxField2.TabIndex = 20;
            // 
            // textBoxFilterText
            // 
            this.textBoxFilterText.Location = new System.Drawing.Point(239, 73);
            this.textBoxFilterText.Name = "textBoxFilterText";
            this.textBoxFilterText.Size = new System.Drawing.Size(290, 20);
            this.textBoxFilterText.TabIndex = 22;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(533, 76);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(202, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "(Filters on matching text; case insensitive)";
            // 
            // FormReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 210);
            this.Controls.Add(this.listViewReport);
            this.Controls.Add(this.groupBoxMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormReport";
            this.Text = "RMT Reports - ";
            this.groupBoxMain.ResumeLayout(false);
            this.groupBoxMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxMain;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxField;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewReport;
        private System.Windows.Forms.TextBox textBoxFrom;
        private System.Windows.Forms.Button buttonRemember;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxReportName;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxPreRecorded;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxTo;
        private System.Windows.Forms.CheckBox checkBoxFilerOutTs;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxFilterText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxField2;
    }
}