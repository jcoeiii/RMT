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
    public partial class FormReport : Form
    {
        public FormReport(FormMain main, string dbasePath, bool isOrderTable)
        {
            InitializeComponent();

            this._main = main;
            this._dbasePath = dbasePath;
            this._isOrderTable = isOrderTable;

            if (_isOrderTable)
                this.Text += "OrderTable";
            else
                this.Text += "QuoteTable";

            load();
        }

        private FormMain _main;
        private string _dbasePath;
        private bool _isOrderTable;
        private List<string> _colNames = new List<string>();

        public bool IsSelected = false;

        private void load()
        {
            this.comboBoxField.Items.Clear();
            this.comboBoxField2.Items.Clear();

            if (this._isOrderTable)
            {
                #region OrderTable

                string[] data = new string[] {
                
                    //"Any Field",
                "PO",
                "Date",
                "EndUser",
                "Equipment",
                "VendorName",
                "JobNumber",
                "CustomerPO",
                "VendorNumber",
                "SalesAss",
                "SoldTo",
                "Street1",
                "Street2",
                "City",
                "State",
                "Zip",
                "ShipTo",
                "ShipStreet1",
                "ShipStreet2",
                "ShipCity",
                "ShipState",
                "ShipZip",
                "Carrier",
                "ShipDate",
                "IsComOrder",
                "IsComPaid",
                "Grinder",
                "SerialNo",
                "PumpStk",
                "ReqDate",
                "SchedShip",
                "PODate",
                "POShipVia",
                "TrackDate1",
                "TrackBy1",
                "TrackSource1",
                "TrackNote1",
                "TrackDate2",
                "TrackBy2",
                "TrackSource2",
                "TrackNote2",
                "TrackDate3",
                "TrackBy3",
                "TrackSource3",
                "TrackNote3",
                "TrackDate4",
                "TrackBy4",
                "TrackSource4",
                "TrackNote4",
                "TrackDate5",
                "TrackBy5",
                "TrackSource5",
                "TrackNote5",
                "TrackDate6",
                "TrackBy6",
                "TrackSource6",
                "TrackNote6",
                "TrackDate7",
                "TrackBy7",
                "TrackSource7",
                "TrackNote7",
                "TrackDate8",
                "TrackBy8",
                "TrackSource8",
                "TrackNote8",
                "TrackDate9",
                "TrackBy9",
                "TrackSource9",
                "TrackNote9",
                "TrackDate10",
                "TrackBy10",
                "TrackSource10",
                "TrackNote10",
                "TrackDate11",
                "TrackBy11",
                "TrackSource11",
                "TrackNote11",
                "TrackDate12",
                "TrackBy12",
                "TrackSource12",
                "TrackNote12",
                "TrackDate13",
                "TrackBy13",
                "TrackSource13",
                "TrackNote13",
                "TrackDate14",
                "TrackBy14",
                "TrackSource14",
                "TrackNote14",
                "TrackDate15",
                "TrackBy15",
                "TrackSource15",
                "TrackNote15",
                "TrackDate16",
                "TrackBy16",
                "TrackSource16",
                "TrackNote16",
                "TrackDate17",
                "TrackBy17",
                "TrackSource17",
                "TrackNote17",
                "TrackDate18",
                "TrackBy18",
                "TrackSource18",
                "TrackNote18",
                "QuotePrice",
                "Credit",
                "Freight",
                "ShopTime",
                "TotalCost",
                "GrossProfit",
                "Profit",
                "Description",
                "Quant1",
                "Descr1",
                "Costs1",
                "ECost1",
                "Quant2",
                "Descr2",
                "Costs2",
                "ECost2",
                "Quant3",
                "Descr3",
                "Costs3",
                "ECost3",
                "Quant4",
                "Descr4",
                "Costs4",
                "ECost4",
                "Quant5",
                "Descr5",
                "Costs5",
                "ECost5",
                "Quant6",
                "Descr6",
                "Costs6",
                "ECost6",
                "Quant7",
                "Descr7",
                "Costs7",
                "ECost7",
                "Quant8",
                "Descr8",
                "Costs8",
                "ECost8",
                "Quant9",
                "Descr9",
                "Costs9",
                "ECost9",
                "Quant10",
                "Descr10",
                "Costs10",
                "ECost10",
                "Quant11",
                "Descr11",
                "Costs11",
                "ECost11",
                "Quant12",
                "Descr12",
                "Costs12",
                "ECost12",
                "Quant13",
                "Descr13",
                "Costs13",
                "ECost13",
                "Quant14",
                "Descr14",
                "Costs14",
                "ECost14",
                "Quant15",
                "Descr15",
                "Costs15",
                "ECost15",
                "Quant16",
                "Descr16",
                "Costs16",
                "ECost16",
                "Quant17",
                "Descr17",
                "Costs17",
                "ECost17",
                "Quant18",
                "Descr18",
                "Costs18",
                "ECost18",
                "Quant19",
                "Descr19",
                "Costs19",
                "ECost19",
                "Quant20",
                "Descr20",
                "Costs20",
                "ECost20",
                "Quant21",
                "Descr21",
                "Costs21",
                "ECost21",
                "Quant22",
                "Descr22",
                "Costs22",
                "ECost22",
                "Quant23",
                "Descr23",
                "Costs23",
                "ECost23",
                "InvInstructions",
                "InvNotes",
                "VendorNotes",
                "CrMemo",
                "InvNumber",
                "InvDate",
                "Status",
                "CheckNumbers",
                "CheckDates",
                "ComDate1",
                "ComCheckNumber1",
                "ComPaid1",
                "ComDate2",
                "ComCheckNumber2",
                "ComPaid2",
                "ComDate3",
                "ComCheckNumber3",
                "ComPaid3",
                "ComDate4",
                "ComCheckNumber4",
                "ComPaid4",
                "ComDate5",
                "ComCheckNumber5",
                "ComPaid5",
                "ComAmount",
                "ComBalance",
                "DeliveryNotes",
                "PONotes"};

                this.comboBoxField.Items.AddRange(data);
                this.comboBoxField2.Items.AddRange(data);

                #endregion
            }
            else
            {
                #region QuoteTable

                string[] data = new string[] {

                //"Any Field",
                "PO",
                "Date",
                "Status",
                "Company",
                "VendorName",
                "JobName",
                "CustomerPO",
                "VendorNumber",
                "SalesAss",
                "SoldTo",
                "Street1",
                "Street2",
                "City",
                "State",
                "Zip",
                "Delivery",
                "Terms",
                "FreightSelect",
                "IsQManual",
                "IsPManual",
                "Location",
                "Equipment",
                "EquipCategory",
                "QuotePrice",
                "Credit",
                "Freight",
                "ShopTime",
                "TotalCost",
                "GrossProfit",
                "Profit",
                "MarkUp",
                "Description",
                "Quant1",
                "Descr1",
                "Costs1",
                "ECost1",
                "Price1",
                "Quant2",
                "Descr2",
                "Costs2",
                "ECost2",
                "Price2",
                "Quant3",
                "Descr3",
                "Costs3",
                "ECost3",
                "Price3",
                "Quant4",
                "Descr4",
                "Costs4",
                "ECost4",
                "Price4",
                "Quant5",
                "Descr5",
                "Costs5",
                "ECost5",
                "Price5",
                "Quant6",
                "Descr6",
                "Costs6",
                "ECost6",
                "Price6",
                "Quant7",
                "Descr7",
                "Costs7",
                "ECost7",
                "Price7",
                "Quant8",
                "Descr8",
                "Costs8",
                "ECost8",
                "Price8",
                "Quant9",
                "Descr9",
                "Costs9",
                "ECost9",
                "Price9",
                "Quant10",
                "Descr10",
                "Costs10",
                "ECost10",
                "Price10",
                "Quant11",
                "Descr11",
                "Costs11",
                "ECost11",
                "Price11",
                "Quant12",
                "Descr12",
                "Costs12",
                "ECost12",
                "Price12",
                "Quant13",
                "Descr13",
                "Costs13",
                "ECost13",
                "Price13",
                "Quant14",
                "Descr14",
                "Costs14",
                "ECost14",
                "Price14",
                "Quant15",
                "Descr15",
                "Costs15",
                "ECost15",
                "Price15",
                "Quant16",
                "Descr16",
                "Costs16",
                "ECost16",
                "Price16",
                "Quant17",
                "Descr17",
                "Costs17",
                "ECost17",
                "Price17",
                "Quant18",
                "Descr18",
                "Costs18",
                "ECost18",
                "Price18",
                "Quant19",
                "Descr19",
                "Costs19",
                "ECost19",
                "Price19",
                "Quant20",
                "Descr20",
                "Costs20",
                "ECost20",
                "Price20",
                "Quant21",
                "Descr21",
                "Costs21",
                "ECost21",
                "Price21",
                "Quant22",
                "Descr22",
                "Costs22",
                "ECost22",
                "Price22",
                "Quant23",
                "Descr23",
                "Costs23",
                "ECost23",
                "Price23",
                "InvNotes",
                "DeliveryNotes",
                "QuoteNotes"};

                this.comboBoxField.Items.AddRange(data);
                this.comboBoxField2.Items.AddRange(data);

                #endregion
            }

            this.comboBoxField.SelectedIndex = 0;
            this.comboBoxField2.SelectedIndex = 0;

            buildComboBox();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            List<List<string>> rows = getTheList(this.textBoxFrom.Text, this.textBoxTo.Text, (this._isOrderTable) ? "OrderTable" : "QuoteTable");
            
            CSV csv = new CSV();

            // headers
            csv.StartCol();
            foreach (string col in this._colNames)
            {
                csv.AddCol(col);
            }

            // datas
            foreach (List<string> row in rows)
            {
                csv.StartCol();
                foreach (string col in row)
                {
                    csv.AddCol(col);
                }
            }

            string file = generateCSVFileName();

            csv.Save(file);
            csv.ShowCSV(file);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            this._colNames.Add(this.comboBoxField.Text);

            buildListView();
        }



        private void textBoxDate_DoubleClick(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;

            FormDatePicker dp = new FormDatePicker(box.Text);
            dp.ShowDialog(this);

            if (dp.IsConfirmed)
            {
                box.Text = dp.Date;
            }
        }

        public void textBoxDate_Leave(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;

            try
            {
                DateTime d = Convert.ToDateTime(box.Text);
                box.Text = d.ToString(@"MM/dd/yyyy");
            }
            catch { }
        }

        private void buildListView()
        {
            this.listViewReport.Columns.Clear();

            foreach (string colName in this._colNames)
            {
                this.listViewReport.Columns.Add(colName, 90);
            }

            this.listViewReport.Columns[0].Width = 91;
        }

        private void buildComboBox()
        {
            List<string> names = getNamesFromReportList((this._isOrderTable) ? "OrderTable" : "QuoteTable");

            this.comboBoxPreRecorded.Items.Clear();

            foreach (string name in names)
            {
                this.comboBoxPreRecorded.Items.Add(name);
            }

            if (this.comboBoxPreRecorded.Items.Count > 0)
                this.comboBoxPreRecorded.SelectedIndex = 0;
        }

        private List<string> getColFromTable(string colName, string tableName)
        {
            List<string> POs = new List<string>();

            System.Data.SQLite.SQLiteConnection con = Sql.GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + this._dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select " + colName + " FROM " + tableName;

                    try
                    {
                        //con.Open();
                        using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                POs.Add(reader[colName] as string);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            return POs;
        }

        private List<List<string>> getTheList(string from, string to, string tableName)
        {
            List<List<string>> datas = new List<List<string>>();

            System.Data.SQLite.SQLiteConnection con = Sql.GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select * FROM " + tableName;

                    string f = from.Trim();
                    string t = to.Trim();

                    DateTime? dFrom = null, dTo = null;

                    try
                    {
                        dFrom = Convert.ToDateTime(from);
                        dTo = Convert.ToDateTime(to);
                    }
                    catch
                    { }

                    try
                    {
                        //con.Open();
                        using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                #region Date filter
                                string date = reader["Date"] as string;

                                if (f == "" || date == "")
                                {
                                    // ignore this
                                }
                                else if (t == "")
                                {
                                    // from to all future dates
                                    DateTime rDate = Convert.ToDateTime(date);

                                    if (rDate < dFrom)
                                        continue;
                                }
                                else
                                {
                                    // use complete data range
                                    DateTime rDate = Convert.ToDateTime(date);

                                    if (rDate < dFrom)
                                        continue;
                                    else if (rDate > dTo)
                                        continue;
                                }
                                #endregion

                                #region T filter

                                if (this.checkBoxFilerOutTs.Checked)
                                {
                                    string po = reader["PO"] as string;

                                    if (po.EndsWith("T"))
                                    {
                                        continue;
                                    }
                                }

                                #endregion

                                #region Filter on text, must contain it to be added to datas
                                
                                if (!String.IsNullOrEmpty(this.textBoxFilterText.Text))
                                {
                                    string theText = reader[this.comboBoxField2.Text] as string;

                                    if (!theText.ToLower().Contains(this.textBoxFilterText.Text.ToLower()))
                                        continue;
                                }

                                #endregion

                                List<string> cols = new List<string>();

                                foreach (string colName in this._colNames)
                                {
                                    cols.Add(reader[colName] as string);
                                }

                                datas.Add(cols);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            return datas;
        }

        #region Dbase Low-Level Helpers for Reports

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

        private void deleteReport(string Name)
        {
            System.Data.SQLite.SQLiteConnection con = Sql.GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "DELETE From ReportsTable Where Name = '" + Name + "';";

                    try
                    {
                        //con.Open();
                        com.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }
        }

        private List<string> getNamesFromReportList(string tableName)
        {
            List<string> names = new List<string>();

            System.Data.SQLite.SQLiteConnection con = Sql.GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select * FROM ReportsTable";

                    try
                    {
                        //con.Open();
                        using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string currTable = reader["TableName"] as string;

                                if (currTable != tableName)
                                    continue;

                                //string c = (reader["Category"] as string) + "|" + (reader["Description"] as string) + "|" + (reader["Price"] as string);

                                names.Add(reader["Name"] as string);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            return names;
        }

        private bool getReportRowByName(string TableName, string Name, out string Columns)
        {
            System.Data.SQLite.SQLiteConnection con = Sql.GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select * FROM ReportsTable Where Name = '" + Name + "'";      // Select all rows from our database table

                    try
                    {
                        //con.Open();
                        using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tableName = reader["TableName"] as string;

                                if (tableName != TableName)
                                    continue;

                                Columns = reader["Columns"] as string;

                                //con.Close();
                                return true;
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            Columns = "";

            return false;
        }
        //private int updatePartRow(string Table, string Name, string Columns)
        //{
        //    using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
        //    {
        //        using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
        //        {
        //            #region update string for OrderTable

        //            com.CommandText =
        //            "UPDATE ReportsTable SET Category = @Category, Description = @Description,  Price = @Price " +
        //            "Where Category = @Category and Description = @Description";

        //            #endregion

        //            #region Parameters for OrderTable

        //            com.Parameters.AddWithValue("@Category", Category);
        //            com.Parameters.AddWithValue("@Description", Description);
        //            com.Parameters.AddWithValue("@Price", Price);

        //            #endregion


        //            try
        //            {
        //                con.Open();                         // Open the connection to the database
        //                return com.ExecuteNonQuery();       // Execute the query
        //            }
        //            catch (SqlException ex)
        //            {
        //                MessageBox.Show(this, "Database error: " + ex.Message);
        //                return 0;
        //            }
        //        }
        //    }
        //}

        private int insertReportRow(string TableName, string Name, string Columns)
        {
            System.Data.SQLite.SQLiteConnection con = Sql.GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region insert string for ReportsTable

                    com.CommandText = "INSERT INTO ReportsTable (TableName, Name, Columns) VALUES (@TableName, @Name, @Columns)";

                    #endregion

                    #region Parameters for ReportsTable

                    com.Parameters.AddWithValue("@TableName", TableName);
                    com.Parameters.AddWithValue("@Name", Name);
                    com.Parameters.AddWithValue("@Columns", Columns);

                    #endregion

                    try
                    {
                        //con.Open();                         // Open the connection to the database
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

        private void buttonRemember_Click(object sender, EventArgs e)
        {
            if (this.textBoxReportName.Text.Trim() != "")
            {
                string cols = "";

                bool isFirst = true;
                foreach (string name in this._colNames)
                {
                    cols += (isFirst) ? name : "|" + name;
                    isFirst = false;
                }

                insertReportRow((this._isOrderTable) ? "OrderTable" : "QuoteTable", this.textBoxReportName.Text.Trim(), cols);

                // rebuild it
                buildComboBox();

                MessageBox.Show(this, "Remembered!");
            }
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            string cols;
            if (getReportRowByName((this._isOrderTable) ? "OrderTable" : "QuoteTable", this.comboBoxPreRecorded.Text, out cols))
            {
                string[] split = cols.Split('|');

                if (split.Count() > 0)
                {
                    this._colNames.Clear();

                    foreach (string col in split)
                    {
                        this._colNames.Add(col);
                    }

                    // rebuild it
                    buildListView();
                }
            }
        }

        private string generateCSVFileName()
        {
            string docs = _dbasePath;//Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string date = DateTime.Now.ToString(@"_hhmm_");

            string filename;
            int count = 1;
            do
            {
                filename = System.IO.Path.GetDirectoryName(docs) + @"\Report" + date + count + ".csv";
                count++;
            } while (System.IO.File.Exists(filename));

            return filename;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.comboBoxPreRecorded.Text))
            {
                if (MessageBox.Show("Do you really want to remove '" + this.comboBoxPreRecorded.Text + "'?", "Remove?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    deleteReport(this.comboBoxPreRecorded.Text);

                    // rebuild it
                    buildComboBox();
                }
            }
        }
    }
}