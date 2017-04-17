namespace Tugwell
{
    partial class FormGoto
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
            this.buttonGoSearch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxField = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.listViewSearch = new System.Windows.Forms.ListView();
            this.columnHeaderPO = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVendorName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderEquipment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSoldTo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderJob = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxMain
            // 
            this.groupBoxMain.Controls.Add(this.buttonGoSearch);
            this.groupBoxMain.Controls.Add(this.label2);
            this.groupBoxMain.Controls.Add(this.comboBoxField);
            this.groupBoxMain.Controls.Add(this.buttonCancel);
            this.groupBoxMain.Controls.Add(this.buttonSelect);
            this.groupBoxMain.Controls.Add(this.label1);
            this.groupBoxMain.Controls.Add(this.textBoxSearch);
            this.groupBoxMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxMain.Location = new System.Drawing.Point(0, 0);
            this.groupBoxMain.Name = "groupBoxMain";
            this.groupBoxMain.Size = new System.Drawing.Size(924, 81);
            this.groupBoxMain.TabIndex = 1;
            this.groupBoxMain.TabStop = false;
            this.groupBoxMain.Text = "Controls";
            // 
            // buttonGoSearch
            // 
            this.buttonGoSearch.Location = new System.Drawing.Point(269, 45);
            this.buttonGoSearch.Name = "buttonGoSearch";
            this.buttonGoSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonGoSearch.TabIndex = 9;
            this.buttonGoSearch.Text = "Go Search";
            this.buttonGoSearch.UseVisualStyleBackColor = true;
            this.buttonGoSearch.Click += new System.EventHandler(this.buttonGoSearch_Click);
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
            this.buttonCancel.Location = new System.Drawing.Point(350, 18);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSelect
            // 
            this.buttonSelect.Location = new System.Drawing.Point(269, 18);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(75, 23);
            this.buttonSelect.TabIndex = 2;
            this.buttonSelect.Text = "Select";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Search:";
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(62, 47);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(171, 20);
            this.textBoxSearch.TabIndex = 0;
            // 
            // listViewSearch
            // 
            this.listViewSearch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderPO,
            this.columnHeaderDate,
            this.columnHeaderRow,
            this.columnHeaderVendorName,
            this.columnHeaderEquipment,
            this.columnHeaderSoldTo,
            this.columnHeaderJob});
            this.listViewSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSearch.FullRowSelect = true;
            this.listViewSearch.GridLines = true;
            this.listViewSearch.Location = new System.Drawing.Point(0, 81);
            this.listViewSearch.MultiSelect = false;
            this.listViewSearch.Name = "listViewSearch";
            this.listViewSearch.Size = new System.Drawing.Size(924, 227);
            this.listViewSearch.TabIndex = 2;
            this.listViewSearch.UseCompatibleStateImageBehavior = false;
            this.listViewSearch.View = System.Windows.Forms.View.Details;
            this.listViewSearch.DoubleClick += new System.EventHandler(this.listViewSearch_DoubleClick);
            // 
            // columnHeaderPO
            // 
            this.columnHeaderPO.Text = "PO";
            this.columnHeaderPO.Width = 80;
            // 
            // columnHeaderDate
            // 
            this.columnHeaderDate.Text = "Date";
            this.columnHeaderDate.Width = 80;
            // 
            // columnHeaderRow
            // 
            this.columnHeaderRow.Text = "Row";
            this.columnHeaderRow.Width = 50;
            // 
            // columnHeaderVendorName
            // 
            this.columnHeaderVendorName.Text = "Vendor Name";
            this.columnHeaderVendorName.Width = 130;
            // 
            // columnHeaderEquipment
            // 
            this.columnHeaderEquipment.Text = "Equipment";
            this.columnHeaderEquipment.Width = 160;
            // 
            // columnHeaderSoldTo
            // 
            this.columnHeaderSoldTo.Text = "Sold To";
            this.columnHeaderSoldTo.Width = 200;
            // 
            // columnHeaderJob
            // 
            this.columnHeaderJob.Text = "Job";
            this.columnHeaderJob.Width = 200;
            // 
            // FormGoto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 308);
            this.Controls.Add(this.listViewSearch);
            this.Controls.Add(this.groupBoxMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormGoto";
            this.Text = "RMT Search (Goto) - ";
            this.groupBoxMain.ResumeLayout(false);
            this.groupBoxMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxMain;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxField;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ListView listViewSearch;
        private System.Windows.Forms.ColumnHeader columnHeaderRow;
        private System.Windows.Forms.ColumnHeader columnHeaderPO;
        private System.Windows.Forms.ColumnHeader columnHeaderDate;
        private System.Windows.Forms.ColumnHeader columnHeaderVendorName;
        private System.Windows.Forms.ColumnHeader columnHeaderEquipment;
        private System.Windows.Forms.ColumnHeader columnHeaderSoldTo;
        private System.Windows.Forms.Button buttonGoSearch;
        private System.Windows.Forms.ColumnHeader columnHeaderJob;
    }
}