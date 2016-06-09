using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Data.SqlClient;

namespace Tugwell
{
    public partial class FormGoto : Form
    {
        public FormGoto(FormMain main, string dbasePath, bool isOrderTable)
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

        public bool IsSelected = false;
        public int _Row = -1;
        public string _POName = "";

        private void load()
        {
            this.comboBoxField.Items.Clear();

            if (this._isOrderTable)
            {
                #region OrderTable

                this.comboBoxField.Items.AddRange(new string[] {
                
                "PO",
                "SoldTo",
                "EndUser",
                "Equipment",
                "VendorName",
                "Project",
                "SerialNo",
                "CustomerPO",
                "VendorNumber",
                "CheckNumbers",
                "Date",
                "InvoiceNumber",
                "CheckNumber"});

                //    //"Any Field",
                //"PO",
                //"SoldTo",

                //"EndUser",
                //"Equipment",
                //"VendorName",
                //"Project",
                //"SerialNo",
                //"CustomerPO",
                //"VendorNumber",

                //"CheckNumbers",
                //"Date",

                
                
                //"SalesAss",
                
                //"Street1",
                //"Street2",
                //"City",
                //"State",
                //"Zip",
                //"ShipTo",
                //"ShipStreet1",
                //"ShipStreet2",
                //"ShipCity",
                //"ShipState",
                //"ShipZip",
                //"Carrier",
                //"ShipDate",
                //"IsComOrder",
                //"IsComPaid",
                //"Grinder",
                
                //"PumpStk",
                //"ReqDate",
                //"SchedShip",
                //"PODate",
                //"POShipVia",
                //"TrackDate1",
                //"TrackBy1",
                //"TrackSource1",
                //"TrackNote1",
                //"TrackDate2",
                //"TrackBy2",
                //"TrackSource2",
                //"TrackNote2",
                //"TrackDate3",
                //"TrackBy3",
                //"TrackSource3",
                //"TrackNote3",
                //"TrackDate4",
                //"TrackBy4",
                //"TrackSource4",
                //"TrackNote4",
                //"TrackDate5",
                //"TrackBy5",
                //"TrackSource5",
                //"TrackNote5",
                //"TrackDate6",
                //"TrackBy6",
                //"TrackSource6",
                //"TrackNote6",
                //"TrackDate7",
                //"TrackBy7",
                //"TrackSource7",
                //"TrackNote7",
                //"TrackDate8",
                //"TrackBy8",
                //"TrackSource8",
                //"TrackNote8",
                //"TrackDate9",
                //"TrackBy9",
                //"TrackSource9",
                //"TrackNote9",
                //"TrackDate10",
                //"TrackBy10",
                //"TrackSource10",
                //"TrackNote10",
                //"TrackDate11",
                //"TrackBy11",
                //"TrackSource11",
                //"TrackNote11",
                //"TrackDate12",
                //"TrackBy12",
                //"TrackSource12",
                //"TrackNote12",
                //"TrackDate13",
                //"TrackBy13",
                //"TrackSource13",
                //"TrackNote13",
                //"TrackDate14",
                //"TrackBy14",
                //"TrackSource14",
                //"TrackNote14",
                //"TrackDate15",
                //"TrackBy15",
                //"TrackSource15",
                //"TrackNote15",
                //"TrackDate16",
                //"TrackBy16",
                //"TrackSource16",
                //"TrackNote16",
                //"TrackDate17",
                //"TrackBy17",
                //"TrackSource17",
                //"TrackNote17",
                //"TrackDate18",
                //"TrackBy18",
                //"TrackSource18",
                //"TrackNote18",
                //"QuotePrice",
                //"Credit",
                //"Freight",
                //"ShopTime",
                //"TotalCost",
                //"GrossProfit",
                //"Profit",
                //"Description",
                //"Quant1",
                //"Descr1",
                //"Costs1",
                //"ECost1",
                //"Quant2",
                //"Descr2",
                //"Costs2",
                //"ECost2",
                //"Quant3",
                //"Descr3",
                //"Costs3",
                //"ECost3",
                //"Quant4",
                //"Descr4",
                //"Costs4",
                //"ECost4",
                //"Quant5",
                //"Descr5",
                //"Costs5",
                //"ECost5",
                //"Quant6",
                //"Descr6",
                //"Costs6",
                //"ECost6",
                //"Quant7",
                //"Descr7",
                //"Costs7",
                //"ECost7",
                //"Quant8",
                //"Descr8",
                //"Costs8",
                //"ECost8",
                //"Quant9",
                //"Descr9",
                //"Costs9",
                //"ECost9",
                //"Quant10",
                //"Descr10",
                //"Costs10",
                //"ECost10",
                //"Quant11",
                //"Descr11",
                //"Costs11",
                //"ECost11",
                //"Quant12",
                //"Descr12",
                //"Costs12",
                //"ECost12",
                //"Quant13",
                //"Descr13",
                //"Costs13",
                //"ECost13",
                //"Quant14",
                //"Descr14",
                //"Costs14",
                //"ECost14",
                //"Quant15",
                //"Descr15",
                //"Costs15",
                //"ECost15",
                //"Quant16",
                //"Descr16",
                //"Costs16",
                //"ECost16",
                //"Quant17",
                //"Descr17",
                //"Costs17",
                //"ECost17",
                //"Quant18",
                //"Descr18",
                //"Costs18",
                //"ECost18",
                //"Quant19",
                //"Descr19",
                //"Costs19",
                //"ECost19",
                //"Quant20",
                //"Descr20",
                //"Costs20",
                //"ECost20",
                //"Quant21",
                //"Descr21",
                //"Costs21",
                //"ECost21",
                //"Quant22",
                //"Descr22",
                //"Costs22",
                //"ECost22",
                //"Quant23",
                //"Descr23",
                //"Costs23",
                //"ECost23",
                //"InvInstructions",
                //"InvNotes",
                //"VendorNotes",
                //"CrMemo",
                //"InvNumber",
                //"InvDate",
                //"Status",
                
                //"CheckDates",
                //"ComDate1",
                //"ComCheckNumber1",
                //"ComPaid1",
                //"ComDate2",
                //"ComCheckNumber2",
                //"ComPaid2",
                //"ComDate3",
                //"ComCheckNumber3",
                //"ComPaid3",
                //"ComDate4",
                //"ComCheckNumber4",
                //"ComPaid4",
                //"ComDate5",
                //"ComCheckNumber5",
                //"ComPaid5",
                //"ComAmount",
                //"ComBalance",
                //"DeliveryNotes",
                //"PONotes"});

                #endregion
            }
            else
            {
                #region QuoteTable

                this.comboBoxField.Items.AddRange(new string[] {
                
                "Company",
                "SoldTo",
                "JobName",
                "VendorName",
                "Location",
                "Equipment",
                "SalesAss"});
                ////"Any Field",

                //"PO",
                //"Company",
                //"SoldTo",
                //"EquipCategory",
              
                
                //"VendorName",
                //"Project",
                //"SerialNo",
                //"CustomerPO",
                //"VendorNumber",

               
                //"Date",


                
             
                //"Status",
                
                
                
              
                
                //"SalesAss",
                
                //"Street1",
                //"Street2",
                //"City",
                //"State",
                //"Zip",
                //"Delivery",
                //"Terms",
                //"FreightSelect",
                //"IsQManual",
                //"IsPManual",
                //"Location",
               
                
                //"QuotePrice",
                //"Credit",
                //"Freight",
                //"ShopTime",
                //"TotalCost",
                //"GrossProfit",
                //"Profit",
                //"MarkUp",
                //"Description",
                //"Quant1",
                //"Descr1",
                //"Costs1",
                //"ECost1",
                //"Price1",
                //"Quant2",
                //"Descr2",
                //"Costs2",
                //"ECost2",
                //"Price2",
                //"Quant3",
                //"Descr3",
                //"Costs3",
                //"ECost3",
                //"Price3",
                //"Quant4",
                //"Descr4",
                //"Costs4",
                //"ECost4",
                //"Price4",
                //"Quant5",
                //"Descr5",
                //"Costs5",
                //"ECost5",
                //"Price5",
                //"Quant6",
                //"Descr6",
                //"Costs6",
                //"ECost6",
                //"Price6",
                //"Quant7",
                //"Descr7",
                //"Costs7",
                //"ECost7",
                //"Price7",
                //"Quant8",
                //"Descr8",
                //"Costs8",
                //"ECost8",
                //"Price8",
                //"Quant9",
                //"Descr9",
                //"Costs9",
                //"ECost9",
                //"Price9",
                //"Quant10",
                //"Descr10",
                //"Costs10",
                //"ECost10",
                //"Price10",
                //"Quant11",
                //"Descr11",
                //"Costs11",
                //"ECost11",
                //"Price11",
                //"Quant12",
                //"Descr12",
                //"Costs12",
                //"ECost12",
                //"Price12",
                //"Quant13",
                //"Descr13",
                //"Costs13",
                //"ECost13",
                //"Price13",
                //"Quant14",
                //"Descr14",
                //"Costs14",
                //"ECost14",
                //"Price14",
                //"Quant15",
                //"Descr15",
                //"Costs15",
                //"ECost15",
                //"Price15",
                //"Quant16",
                //"Descr16",
                //"Costs16",
                //"ECost16",
                //"Price16",
                //"Quant17",
                //"Descr17",
                //"Costs17",
                //"ECost17",
                //"Price17",
                //"Quant18",
                //"Descr18",
                //"Costs18",
                //"ECost18",
                //"Price18",
                //"Quant19",
                //"Descr19",
                //"Costs19",
                //"ECost19",
                //"Price19",
                //"Quant20",
                //"Descr20",
                //"Costs20",
                //"ECost20",
                //"Price20",
                //"Quant21",
                //"Descr21",
                //"Costs21",
                //"ECost21",
                //"Price21",
                //"Quant22",
                //"Descr22",
                //"Costs22",
                //"ECost22",
                //"Price22",
                //"Quant23",
                //"Descr23",
                //"Costs23",
                //"ECost23",
                //"Price23",
                //"InvNotes",
                //"DeliveryNotes",
                //"QuoteNotes"});

                #endregion
            }

            this.comboBoxField.SelectedIndex = 0;

            autoLoadOnStart();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private List<int> search(string searchTxt)
        {
            if (searchTxt.Trim().Length == 0)
                return new List<int>(); ;

            // a cheat to make JobName/Number look like Project
            string col = (this.comboBoxField.Text != "Project") ? this.comboBoxField.Text : "JobNumber";

            List<string> theList = getColFromTable(col, (this._isOrderTable) ? "OrderTable" : "QuoteTable");
            List<int> indexList = new List<int>();

            int index = 0;
            foreach (string item in theList)
            {
                if (item.ToLower().Contains(searchTxt.ToLower()))
                    indexList.Add(index);

                index++;
            }

            return indexList;
        }

        private void autoSaveOnEnd()
        {
            string[] lines = new string[] { this.textBoxSearch.Text.Trim(), this.comboBoxField.Text };

            File.WriteAllLines("goto.txt", lines);
        }

        private void autoLoadOnStart()
        {
            // read from file goto
            string[] dataLines = new string[] { };

            try
            { dataLines = File.ReadAllLines("goto.txt"); }
            catch { }

            if (dataLines.Count() < 2)
                return;

            this.textBoxSearch.Text = dataLines[0];
            this.comboBoxField.Text = dataLines[1];


            string s = this.textBoxSearch.Text.Trim();


            List<int> indexList = search(s);
            if (indexList.Count() > 0)
            {
                buildListView(indexList);
            }
            else
            {
                this.listViewSearch.Items.Clear(); 
            }
        }

        private void buttonGoSearch_Click(object sender, EventArgs e)
        {
            string s = this.textBoxSearch.Text.Trim();

            if (s.Length == 0)
            {
                MessageBox.Show(this, "Please type non-spaced search string.");
            }

            List<int> indexList = search(s);

            if (indexList.Count() > 0)
            {
                buildListView(indexList);

                if (indexList.Count() == 1)
                {
                    // auto do it!
                    this.listViewSearch.SelectedIndices.Add(0);
                    selectAndClose();
                }
            }
            else
            {
                this.listViewSearch.Items.Clear();
                MessageBox.Show(this, "Nothing Found!");
            }
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            selectAndClose();
        }

        private void selectAndClose()
        {
            ListViewItem lvi = getSelectedRow();
            if (lvi == null)
                return;

            this.IsSelected = true;

            string row, PO;
            getListViewRow(lvi, out row, out PO);

            this._Row = Convert.ToInt32(row);

            this._POName = PO;

            autoSaveOnEnd();

            this.Close();
        }

        private ListViewItem getSelectedRow()
        {
            if (this.listViewSearch.SelectedItems == null || this.listViewSearch.SelectedItems.Count == 0)
                return null;
            else
                return this.listViewSearch.SelectedItems[0];
        }

        private void buildListView(List<int> indexList)
        {
            List<string> list = getTheList(indexList, (this._isOrderTable) ? "OrderTable" : "QuoteTable");

            this.listViewSearch.Items.Clear();

            list.Sort();

            foreach (string item in list)
            {
                string[] split = item.Split('|');

                ListViewItem lvi = new ListViewItem(split[0]); // PO
                lvi.SubItems.Add(split[1]); // Date
                lvi.SubItems.Add(split[2]); // row
                lvi.SubItems.Add(split[3]); // Vendor Name
                lvi.SubItems.Add(split[4]); // Equipment
                lvi.SubItems.Add(split[5]); // Sold To

                this.listViewSearch.Items.Add(lvi);
            }
        }

        private void getListViewRow(ListViewItem lvi, out string row, out string PO)
        {
            PO = lvi.Text;
            row = lvi.SubItems[2].Text;
        }

        private List<string> getColFromTable(string colName, string tableName)
        {
            List<string> POs = new List<string>();

            System.Data.SQLite.SQLiteConnection con = this._main.GetConnection();
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

        private List<string> getTheList(List<int> rowIndex, string tableName)
        {
            List<string> COs = new List<string>();

            System.Data.SQLite.SQLiteConnection con = this._main.GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + _dbasePath))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select * FROM " + tableName;

                    try
                    {
                        //con.Open();
                        using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            int row = 0;
                            while (reader.Read())
                            {
                                if (rowIndex.Contains(row))
                                {
                                    row++;
                                    string c = (reader["PO"] as string) + "|" + (reader["Date"] as string) + "|" + row.ToString() + "|" + (reader["VendorName"] as string) + "|" + (reader["Equipment"] as string) + "|" + (reader["SoldTo"] as string);

                                    COs.Add(c);
                                    continue;
                                }

                                row++;
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

        private void listViewSearch_DoubleClick(object sender, EventArgs e)
        {
            selectAndClose();
        }
    }
}
