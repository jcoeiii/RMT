using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SqlClient;

namespace Tugwell
{
    public partial class FormParts : Form
    {
        public FormParts(string dbasePath)
        {
            _dbasePath = dbasePath;

            InitializeComponent();

            buildListView("");

            try
            {
                this.comboBoxCategory.SelectedIndex = 0;
            }
            catch { }
        }

        private string _dbasePath;

        private bool _stopComboEvent = false;

        public bool IsSelected = false;
        public string _description = "";
        public string _price = "";

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = getSelectedPart();
            if (lvi == null)
            {
                MessageBox.Show(this, "Please select an item first.");
                return;
            }

            selectAndClose();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormPartsNew fpn = new FormPartsNew(this.comboBoxCategory.Text, "", "");
            fpn.ShowDialog(this);

            if (fpn.IsConfirmed)
            {
                insertPartRow(this.comboBoxCategory.Text, fpn._Description, fpn._Price);

                buildListView("");
            }
        }

        private void buttonModify_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = getSelectedPart();
            if (lvi == null)
                return;

            string description, price;
            getListViewParts(lvi, out description, out price);

            FormPartsNew fpn = new FormPartsNew(this.comboBoxCategory.Text, description, price);
            fpn.ShowDialog(this);

            if (fpn.IsConfirmed)
            {
                updatePartRow(this.comboBoxCategory.Text, fpn._Description, fpn._Price);

                buildListView("");
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = getSelectedPart();
            if (lvi == null)
                return;

            string description, price;
            getListViewParts(lvi, out description, out price);

            deletePart(this.comboBoxCategory.Text, description);

            buildListView("");
        }

        private void buttonNewCategory_Click(object sender, EventArgs e)
        {
            FormPartsNew fpn = new FormPartsNew("", "", "");
            fpn.ShowDialog(this);

            if (fpn.IsConfirmed)
            {
                buildListView(fpn._Category);
            }
        }

        private void selectAndClose()
        {
            ListViewItem lvi = getSelectedPart();
            if (lvi == null)
                return;

            this.IsSelected = true;

            string description, price;
            getListViewParts(lvi, out description, out price);

            this._description = description;
            this._price = price;

            this.Close();
        }

        private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._stopComboEvent)
                return;

            buildListView(this.comboBoxCategory.Text);
        }

        private void buildListView(string comboSelection)
        {
            List<string> list = getPartsList();
            list.Sort();

            this.listViewParts.Items.Clear();

            // build combo
            this._stopComboEvent = true;

            string currentCat = this.comboBoxCategory.Text;
            this.comboBoxCategory.Items.Clear();

            // add items to combo here
            foreach (string item in list)
            {
                string[] split = item.Split('|');

                if (this.comboBoxCategory.Items.Contains(split[0])) // category
                {
                    continue;
                }

                this.comboBoxCategory.Items.Add(split[0]);
            }

            if (comboSelection == "")
            {
                this.comboBoxCategory.Text = currentCat;
            }
            else
            {
                if (!this.comboBoxCategory.Items.Contains(comboSelection))
                    this.comboBoxCategory.Items.Add(comboSelection);
                this.comboBoxCategory.Text = comboSelection;
            }

            this._stopComboEvent = false;

            

            foreach (string item in list)
            {
                string[] split = item.Split('|');

                if (this.comboBoxCategory.Text != split[0])
                    continue;

                ListViewItem lvi = new ListViewItem(split[1]); // description
                lvi.SubItems.Add(split[2]); // price

                this.listViewParts.Items.Add(lvi);
            }
        }

        private ListViewItem getSelectedPart()
        {
            if (this.listViewParts.SelectedItems == null || this.listViewParts.SelectedItems.Count == 0)
                return null;
            else
                return this.listViewParts.SelectedItems[0];
        }

        private void getListViewParts(ListViewItem lvi, out string description, out string price)
        {
            description = lvi.Text;
            price = lvi.SubItems[1].Text;
        }

        public void AddImportPart(string category, string part, string price)
        {
            insertPartRow(category, part, price);
        }

        #region Dbase Low-Level Helpers for Parts

        //private int getRowCounts()
        //{
        //    int? count = 0;

        //    using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
        //    {
        //        using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
        //        {
        //            con.Open();                             // Open the connection to the database

        //            com.CommandText = "Select COUNT(*) From CompanyTable";      // Select all rows from our database table

        //            try
        //            {
        //                object o = com.ExecuteScalar();
        //                count = Convert.ToInt32(o);
        //            }
        //            catch { };
        //        }
        //    }

        //    return (count == null) ? 0 : count.Value;
        //}

        private void deletePart(string Category, string Description)
        {
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "DELETE From PartsTable Where Category = '" + Category.Replace("'", "''") + "' AND Description = '" + Description.Replace("'", "''") + "';";

                    try
                    {
                        con.Open();
                        com.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }
        }

        private List<string> getPartsList()
        {
            List<string> parts = new List<string>();

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select * FROM PartsTable";

                    try
                    {
                        con.Open();
                        using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string c = (reader["Category"] as string) + "|" + (reader["Description"] as string) + "|" + (reader["Price"] as string);

                                parts.Add(c);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            return parts;
        }

        private int updatePartRow(string Category, string Description, string Price)
        {
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region update string for OrderTable

                    com.CommandText =
                    "UPDATE PartsTable SET Category = @Category, Description = @Description,  Price = @Price " +
                    "Where Category = @Category and Description = @Description";

                    #endregion

                    #region Parameters for OrderTable

                    com.Parameters.AddWithValue("@Category", Category);
                    com.Parameters.AddWithValue("@Description", Description);
                    com.Parameters.AddWithValue("@Price", Price);

                    #endregion

                    
                    try
                    {
                        con.Open();                         // Open the connection to the database
                        return com.ExecuteNonQuery();       // Execute the query
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                        return 0;
                    }
                }
            }
        }

        private int insertPartRow(string Category, string Description, string Price)
        {
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region insert string for PartsTable

                    com.CommandText = "INSERT INTO PartsTable (Category, Description, Price) VALUES (@Category, @Description, @Price)";

                    #endregion

                    #region Parameters for PartsTable

                    com.Parameters.AddWithValue("@Category", Category);
                    com.Parameters.AddWithValue("@Description", Description);
                    com.Parameters.AddWithValue("@Price", Price);

                    #endregion

                    try
                    {
                        con.Open();                         // Open the connection to the database
                        return com.ExecuteNonQuery();       // Execute the query
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                        return 0;
                    }
                }
            }
        }

        #endregion

        private void listViewParts_DoubleClick(object sender, EventArgs e)
        {
            selectAndClose();
        }
    }
}