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
    public partial class FormCompany : Form
    {
        public FormCompany(string dbasePath)
        {
            _dbasePath = dbasePath;

            InitializeComponent();

            buildListView();
        }

        private string _dbasePath;

        public bool IsSelected = false;
        public string _company = "";
        public string _street1 = "";
        public string _street2 = "";
        public string _city = "";
        public string _state = "";
        public string _zip = "";

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (this.textBoxSearch.Text.Trim().Length <= 2)
                return;

            List<string> list = getCompanyList();

            if (list.Count() == 0)
                return;

            list.Sort();

            int index = 0;

            foreach (string item in list)
            {
                if (item.ToUpper().StartsWith(this.textBoxSearch.Text.ToUpper()))// ||
                    //item.ToUpper().Contains(this.textBoxSearch.Text.ToUpper()))
                {
                    this.listViewCompanies.Items[index].Selected = true;
                    this.listViewCompanies.Select();

                    // ensure visable
                    this.listViewCompanies.EnsureVisible(index);

                    //this.textBoxSearch.Select();

                    return;
                }
                index++;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = getSelectedCompany();
            if (lvi == null)
            {
                MessageBox.Show(this, "Please select an item first.");
                return;
            }

            selectAndClose();
        }


        private void listViewCompanies_DoubleClick(object sender, EventArgs e)
        {
            selectAndClose();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormCompanyNew fcn = new FormCompanyNew("", "", "", "", "", "", "", "");
            fcn.ShowDialog(this);

            if (fcn.IsConfirmed)
            {
                insertRowCompany(fcn._CompanyName, fcn._Street1, fcn._Street2, fcn._City, fcn._State, fcn._Zip, fcn._Phone, fcn._Fax);

                buildListView();
            }
        }

        private void buttonModify_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = getSelectedCompany();
            if (lvi == null)
                return;

            string company, street1, street2, city, state, zip, phone, fax;
            getListViewParts(lvi, out company, out street1, out street2, out city, out state, out zip, out phone, out fax);

            FormCompanyNew fcn = new FormCompanyNew(company, street1, street2, city, state, zip, phone, fax);
            fcn.ShowDialog(this);

            if (fcn.IsConfirmed)
            {
                int index = this.listViewCompanies.SelectedIndices[0];

                updateCompanyRow(fcn._CompanyName, fcn._Street1, fcn._Street2, fcn._City, fcn._State, fcn._Zip, fcn._Phone, fcn._Fax);

                buildListView();


                this.listViewCompanies.Items[index].Selected = true;
                this.listViewCompanies.Select();

                // ensure visable
                this.listViewCompanies.EnsureVisible(index);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = getSelectedCompany();
            if (lvi == null)
                return;

            string company, street1, street2, city, state, zip, phone, fax;
            getListViewParts(lvi, out company, out street1, out street2, out city, out state, out zip, out phone, out fax);

            deleteCompany(company);

            buildListView();
        }

        private void selectAndClose()
        {
            ListViewItem lvi = getSelectedCompany();
            if (lvi == null)
                return;

            this.IsSelected = true;

            string company, street1, street2, city, state, zip, phone, fax;
            getListViewParts(lvi, out company, out street1, out street2, out city, out state, out zip, out phone, out fax);

            this._company = company;
            this._street1 = street1;
            this._street2 = street2;
            this._city = city;
            this._state = state;
            this._zip = zip;

            this.Close();
        }

        private void buildListView()
        {
            List<string> list = getCompanyList();

            this.listViewCompanies.Items.Clear();

            list.Sort();

            foreach (string item in list)
            {
                string[] split = item.Split('|');

                ListViewItem lvi = new ListViewItem(split[0]);
                lvi.SubItems.Add(split[1]); // street1
                lvi.SubItems.Add(split[2]); // street2
                lvi.SubItems.Add(split[3]); // city
                lvi.SubItems.Add(split[4]); // state
                lvi.SubItems.Add(split[5]); // zip
                lvi.SubItems.Add(split[6]); // phone
                lvi.SubItems.Add(split[7]); // fax

                this.listViewCompanies.Items.Add(lvi);
            }
        }

        private ListViewItem getSelectedCompany()
        {
            if (this.listViewCompanies.SelectedItems == null || this.listViewCompanies.SelectedItems.Count == 0)
                return null;
            else
                return this.listViewCompanies.SelectedItems[0];
        }

        private void getListViewParts(ListViewItem lvi, out string company, out string street1, out string street2, out string city, out string state, out string zip, out string phone, out string fax)
        {
            company = lvi.Text;
            street1 = lvi.SubItems[1].Text;
            street2 = lvi.SubItems[2].Text;
            city = lvi.SubItems[3].Text;
            state = lvi.SubItems[4].Text;
            zip = lvi.SubItems[5].Text;
            phone = lvi.SubItems[6].Text;
            fax = lvi.SubItems[7].Text;
        }

        public void AddImportCompany(string name, string street1, string city, string state, string zip, string phone, string fax)
        {
            insertRowCompany(name, street1, "", city, state.ToUpper(), zip, cleanNumber(phone), cleanNumber(fax));
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

        #region Dbase Low-Level Helpers for Company

        //private int getRowCounts()
        //{
        //    int? count = 0;

        //    using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
        //    {
        //        using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
        //        {
        //            //con.Open();                             // Open the connection to the database

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

        private void deleteCompany(string Company)
        {
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "DELETE From CompanyTable Where Company = '" + Company.Replace("'", "''") + "'";

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

        private List<string> getCompanyList()
        {
            List<string> COs = new List<string>();

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select * FROM CompanyTable";

                    try
                    {
                        con.Open();
                        using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string c = (reader["Company"] as string) + "|" + (reader["Street1"] as string) + "|" + (reader["Street2"] as string) + "|" + (reader["City"] as string) + "|" + (reader["State"] as string) + "|" + (reader["Zip"] as string) + "|" + (reader["Phone"] as string) + "|" + (reader["Fax"] as string);

                                COs.Add(c);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            return COs;
        }

        private int updateCompanyRow(string Company, string Street1, string Street2, string City, string State, string Zip, string Phone, string Fax)
        {
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region Update string for CompanyTable

                    com.CommandText =
                    "UPDATE CompanyTable SET Company = @Company, Street1 = @Street1,  Street2 = @Street2,  City = @City,  State = @State,  Zip = @Zip, " +
                    "Phone = @Phone, Fax = @Fax " +
                    "Where Company = @Company";

                    #endregion

                    #region Parameters for CompanyTable

                    com.Parameters.AddWithValue("@Company", Company);
                    com.Parameters.AddWithValue("@Street1", Street1);
                    com.Parameters.AddWithValue("@Street2", Street2);
                    com.Parameters.AddWithValue("@City", City);
                    com.Parameters.AddWithValue("@State", State);
                    com.Parameters.AddWithValue("@Zip", Zip);
                    com.Parameters.AddWithValue("@Phone", Phone);
                    com.Parameters.AddWithValue("@Fax", Fax);

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

        private int insertRowCompany(string Company, string Street1, string Street2, string City, string State, string Zip, string Phone, string Fax)
        {
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region Insert string for CompanyTable

                    com.CommandText =
                    "INSERT INTO CompanyTable (Company, Street1, Street2, City, State, Zip, Phone, Fax) VALUES " +
                    "(@Company, @Street1, @Street2, @City, @State, @Zip, @Phone, @Fax)";

                    #endregion

                    #region Parameters for CompanyTable

                    com.Parameters.AddWithValue("@Company", Company);
                    com.Parameters.AddWithValue("@Street1", Street1);
                    com.Parameters.AddWithValue("@Street2", Street2);
                    com.Parameters.AddWithValue("@City", City);
                    com.Parameters.AddWithValue("@State", State);
                    com.Parameters.AddWithValue("@Zip", Zip);
                    com.Parameters.AddWithValue("@Phone", Phone);
                    com.Parameters.AddWithValue("@Fax", Fax);

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
    }
}
