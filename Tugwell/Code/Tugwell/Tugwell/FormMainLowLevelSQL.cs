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
using System.Data.SQLite; // for sql dbase

namespace Tugwell
{
    public partial class FormMain : Form
    {
        #region Create New Dbase and helpers

        private void addOrderTableWithColume(string newCol)
        {//alter table mytable add column colnew char(50)
            string addTableCol = @"ALTER TABLE OrderTable ADD COLUMN " + newCol + @" VARCHAR(512)";

            //System.Data.SQLite.SQLiteConnection.CreateFile(getDbasePathName());        // Create the file which will be hosting our database
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    con.Open();                             // Open the connection to the database

                    com.CommandText = addTableCol;    // Set CommandText to our query that will create a table
                    com.ExecuteNonQuery();                  // Execute the query
                }
            }
        }

        private void createNewDbase()
        {
            #region OrderTable creator string

            // This is the query which will create a new table in our database file. An auto increment column called "ID", and many NVARCHAR type columns
            string createTableQuery1 = @"CREATE TABLE IF NOT EXISTS [OrderTable] (
[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
[PO] NVARCHAR(80)  NULL,
[Date] VARCHAR(10)  NULL,
[EndUser] VARCHAR(512)  NULL,
[Equipment] VARCHAR(512)  NULL,
[VendorName] VARCHAR(512)  NULL,
[JobNumber] VARCHAR(512)  NULL,
[CustomerPO] VARCHAR(512)  NULL,
[VendorNumber] VARCHAR(512)  NULL,
[SalesAss] VARCHAR(512)  NULL,
[SoldTo] VARCHAR(512)  NULL,
[Street1] VARCHAR(512)  NULL,
[Street2] VARCHAR(512)  NULL,
[City] VARCHAR(512)  NULL,
[State] VARCHAR(512)  NULL,
[Zip] VARCHAR(512)  NULL,
[ShipTo] VARCHAR(512)  NULL,
[ShipStreet1] VARCHAR(512)  NULL,
[ShipStreet2] VARCHAR(512)  NULL,
[ShipCity] VARCHAR(512)  NULL,
[ShipState] VARCHAR(512)  NULL,
[ShipZip] VARCHAR(512)  NULL,
[Carrier] VARCHAR(512)  NULL,
[ShipDate] VARCHAR(10)  NULL,
[IsComOrder] VARCHAR(1)  NULL,
[IsComPaid] VARCHAR(1)  NULL,
[Grinder] VARCHAR(512)  NULL,
[SerialNo] VARCHAR(512)  NULL,
[PumpStk] VARCHAR(512)  NULL,
[ReqDate] VARCHAR(10)  NULL,
[SchedShip] VARCHAR(512)  NULL,
[PODate] VARCHAR(10)  NULL,
[POShipVia] VARCHAR(512)  NULL,
[TrackDate1] VARCHAR(10)  NULL,
[TrackBy1] VARCHAR(512)  NULL,
[TrackSource1] VARCHAR(512)  NULL,
[TrackNote1] VARCHAR(512)  NULL,
[TrackDate2] VARCHAR(10)  NULL,
[TrackBy2] VARCHAR(512)  NULL,
[TrackSource2] VARCHAR(512)  NULL,
[TrackNote2] VARCHAR(512)  NULL,
[TrackDate3] VARCHAR(10)  NULL,
[TrackBy3] VARCHAR(512)  NULL,
[TrackSource3] VARCHAR(512)  NULL,
[TrackNote3] VARCHAR(512)  NULL,
[TrackDate4] VARCHAR(10)  NULL,
[TrackBy4] VARCHAR(512)  NULL,
[TrackSource4] VARCHAR(512)  NULL,
[TrackNote4] VARCHAR(512)  NULL,
[TrackDate5] VARCHAR(10)  NULL,
[TrackBy5] VARCHAR(512)  NULL,
[TrackSource5] VARCHAR(512)  NULL,
[TrackNote5] VARCHAR(512)  NULL,
[TrackDate6] VARCHAR(10)  NULL,
[TrackBy6] VARCHAR(512)  NULL,
[TrackSource6] VARCHAR(512)  NULL,
[TrackNote6] VARCHAR(512)  NULL,
[TrackDate7] VARCHAR(10)  NULL,
[TrackBy7] VARCHAR(512)  NULL,
[TrackSource7] VARCHAR(512)  NULL,
[TrackNote7] VARCHAR(512)  NULL,
[TrackDate8] VARCHAR(10)  NULL,
[TrackBy8] VARCHAR(512)  NULL,
[TrackSource8] VARCHAR(512)  NULL,
[TrackNote8] VARCHAR(512)  NULL,
[TrackDate9] VARCHAR(10)  NULL,
[TrackBy9] VARCHAR(512)  NULL,
[TrackSource9] VARCHAR(512)  NULL,
[TrackNote9] VARCHAR(512)  NULL,
[TrackDate10] VARCHAR(10)  NULL,
[TrackBy10] VARCHAR(512)  NULL,
[TrackSource10] VARCHAR(512)  NULL,
[TrackNote10] VARCHAR(512)  NULL,
[TrackDate11] VARCHAR(10)  NULL,
[TrackBy11] VARCHAR(512)  NULL,
[TrackSource11] VARCHAR(512)  NULL,
[TrackNote11] VARCHAR(512)  NULL,
[TrackDate12] VARCHAR(10)  NULL,
[TrackBy12] VARCHAR(512)  NULL,
[TrackSource12] VARCHAR(512)  NULL,
[TrackNote12] VARCHAR(512)  NULL,
[TrackDate13] VARCHAR(10)  NULL,
[TrackBy13] VARCHAR(512)  NULL,
[TrackSource13] VARCHAR(512)  NULL,
[TrackNote13] VARCHAR(512)  NULL,
[TrackDate14] VARCHAR(10)  NULL,
[TrackBy14] VARCHAR(512)  NULL,
[TrackSource14] VARCHAR(512)  NULL,
[TrackNote14] VARCHAR(512)  NULL,
[TrackDate15] VARCHAR(10)  NULL,
[TrackBy15] VARCHAR(512)  NULL,
[TrackSource15] VARCHAR(512)  NULL,
[TrackNote15] VARCHAR(512)  NULL,
[TrackDate16] VARCHAR(10)  NULL,
[TrackBy16] VARCHAR(512)  NULL,
[TrackSource16] VARCHAR(512)  NULL,
[TrackNote16] VARCHAR(512)  NULL,
[TrackDate17] VARCHAR(10)  NULL,
[TrackBy17] VARCHAR(512)  NULL,
[TrackSource17] VARCHAR(512)  NULL,
[TrackNote17] VARCHAR(512)  NULL,
[TrackDate18] VARCHAR(10)  NULL,
[TrackBy18] VARCHAR(512)  NULL,
[TrackSource18] VARCHAR(512)  NULL,
[TrackNote18] VARCHAR(512)  NULL,
[QuotePrice] VARCHAR(512)  NULL,
[Credit] VARCHAR(512)  NULL,
[Freight] VARCHAR(512)  NULL,
[ShopTime] VARCHAR(512)  NULL,
[TotalCost] VARCHAR(512)  NULL,
[GrossProfit] VARCHAR(512)  NULL,
[Profit] VARCHAR(512)  NULL,
[Description] VARCHAR(512)  NULL,
[Quant1] VARCHAR(512)  NULL,
[Descr1] VARCHAR(512)  NULL,
[Costs1] VARCHAR(512)  NULL,
[ECost1] VARCHAR(512)  NULL,
[Quant2] VARCHAR(512)  NULL,
[Descr2] VARCHAR(512)  NULL,
[Costs2] VARCHAR(512)  NULL,
[ECost2] VARCHAR(512)  NULL,
[Quant3] VARCHAR(512)  NULL,
[Descr3] VARCHAR(512)  NULL,
[Costs3] VARCHAR(512)  NULL,
[ECost3] VARCHAR(512)  NULL,
[Quant4] VARCHAR(512)  NULL,
[Descr4] VARCHAR(512)  NULL,
[Costs4] VARCHAR(512)  NULL,
[ECost4] VARCHAR(512)  NULL,
[Quant5] VARCHAR(512)  NULL,
[Descr5] VARCHAR(512)  NULL,
[Costs5] VARCHAR(512)  NULL,
[ECost5] VARCHAR(512)  NULL,
[Quant6] VARCHAR(512)  NULL,
[Descr6] VARCHAR(512)  NULL,
[Costs6] VARCHAR(512)  NULL,
[ECost6] VARCHAR(512)  NULL,
[Quant7] VARCHAR(512)  NULL,
[Descr7] VARCHAR(512)  NULL,
[Costs7] VARCHAR(512)  NULL,
[ECost7] VARCHAR(512)  NULL,
[Quant8] VARCHAR(512)  NULL,
[Descr8] VARCHAR(512)  NULL,
[Costs8] VARCHAR(512)  NULL,
[ECost8] VARCHAR(512)  NULL,
[Quant9] VARCHAR(512)  NULL,
[Descr9] VARCHAR(512)  NULL,
[Costs9] VARCHAR(512)  NULL,
[ECost9] VARCHAR(512)  NULL,
[Quant10] VARCHAR(512)  NULL,
[Descr10] VARCHAR(512)  NULL,
[Costs10] VARCHAR(512)  NULL,
[ECost10] VARCHAR(512)  NULL,
[Quant11] VARCHAR(512)  NULL,
[Descr11] VARCHAR(512)  NULL,
[Costs11] VARCHAR(512)  NULL,
[ECost11] VARCHAR(512)  NULL,
[Quant12] VARCHAR(512)  NULL,
[Descr12] VARCHAR(512)  NULL,
[Costs12] VARCHAR(512)  NULL,
[ECost12] VARCHAR(512)  NULL,
[Quant13] VARCHAR(512)  NULL,
[Descr13] VARCHAR(512)  NULL,
[Costs13] VARCHAR(512)  NULL,
[ECost13] VARCHAR(512)  NULL,
[Quant14] VARCHAR(512)  NULL,
[Descr14] VARCHAR(512)  NULL,
[Costs14] VARCHAR(512)  NULL,
[ECost14] VARCHAR(512)  NULL,
[Quant15] VARCHAR(512)  NULL,
[Descr15] VARCHAR(512)  NULL,
[Costs15] VARCHAR(512)  NULL,
[ECost15] VARCHAR(512)  NULL,
[Quant16] VARCHAR(512)  NULL,
[Descr16] VARCHAR(512)  NULL,
[Costs16] VARCHAR(512)  NULL,
[ECost16] VARCHAR(512)  NULL,
[Quant17] VARCHAR(512)  NULL,
[Descr17] VARCHAR(512)  NULL,
[Costs17] VARCHAR(512)  NULL,
[ECost17] VARCHAR(512)  NULL,
[Quant18] VARCHAR(512)  NULL,
[Descr18] VARCHAR(512)  NULL,
[Costs18] VARCHAR(512)  NULL,
[ECost18] VARCHAR(512)  NULL,
[Quant19] VARCHAR(512)  NULL,
[Descr19] VARCHAR(512)  NULL,
[Costs19] VARCHAR(512)  NULL,
[ECost19] VARCHAR(512)  NULL,
[Quant20] VARCHAR(512)  NULL,
[Descr20] VARCHAR(512)  NULL,
[Costs20] VARCHAR(512)  NULL,
[ECost20] VARCHAR(512)  NULL,
[Quant21] VARCHAR(512)  NULL,
[Descr21] VARCHAR(512)  NULL,
[Costs21] VARCHAR(512)  NULL,
[ECost21] VARCHAR(512)  NULL,
[Quant22] VARCHAR(512)  NULL,
[Descr22] VARCHAR(512)  NULL,
[Costs22] VARCHAR(512)  NULL,
[ECost22] VARCHAR(512)  NULL,
[Quant23] VARCHAR(512)  NULL,
[Descr23] VARCHAR(512)  NULL,
[Costs23] VARCHAR(512)  NULL,
[ECost23] VARCHAR(512)  NULL,
[InvInstructions] VARCHAR(512)  NULL,
[InvNotes] VARCHAR(512)  NULL,
[VendorNotes] VARCHAR(512)  NULL,
[CrMemo] VARCHAR(512)  NULL,
[InvNumber] VARCHAR(512)  NULL,
[InvDate] VARCHAR(10)  NULL,
[Status] VARCHAR(512)  NULL,
[CheckNumbers] VARCHAR(512)  NULL,
[CheckDates] VARCHAR(512)  NULL,
[ComDate1] VARCHAR(10)  NULL,
[ComCheckNumber1] VARCHAR(512)  NULL,
[ComPaid1] VARCHAR(512)  NULL,
[ComDate2] VARCHAR(10)  NULL,
[ComCheckNumber2] VARCHAR(512)  NULL,
[ComPaid2] VARCHAR(512)  NULL,
[ComDate3] VARCHAR(10)  NULL,
[ComCheckNumber3] VARCHAR(512)  NULL,
[ComPaid3] VARCHAR(512)  NULL,
[ComDate4] VARCHAR(10)  NULL,
[ComCheckNumber4] VARCHAR(512)  NULL,
[ComPaid4] VARCHAR(512)  NULL,
[ComDate5] VARCHAR(10)  NULL,
[ComCheckNumber5] VARCHAR(512)  NULL,
[ComPaid5] VARCHAR(512)  NULL,
[ComAmount] VARCHAR(512)  NULL,
[ComBalance] VARCHAR(512)  NULL,
[DeliveryNotes] VARCHAR(512)  NULL,
[PONotes] VARCHAR(512)  NULL,
[Spare1] VARCHAR(512)  NULL,
[Spare2] VARCHAR(512)  NULL,
[Spare3] VARCHAR(512)  NULL,
[Spare4] VARCHAR(512)  NULL,
[Spare5] VARCHAR(512)  NULL
                          )";

            #endregion

            #region QuoteTable creator string

            // This is the query which will create a new table in our database file. An auto increment column called "ID", and many NVARCHAR type columns
            string createTableQuery2 = @"CREATE TABLE IF NOT EXISTS [QuoteTable] (
[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
[PO] NVARCHAR(80)  NULL,
[Date] VARCHAR(10)  NULL,
[Status] VARCHAR(512)  NULL,
[Company] VARCHAR(512)  NULL,
[VendorName] VARCHAR(512)  NULL,
[JobName] VARCHAR(512)  NULL,
[CustomerPO] VARCHAR(512)  NULL,
[VendorNumber] VARCHAR(512)  NULL,
[SalesAss] VARCHAR(512)  NULL,
[SoldTo] VARCHAR(512)  NULL,
[Street1] VARCHAR(512)  NULL,
[Street2] VARCHAR(512)  NULL,
[City] VARCHAR(512)  NULL,
[State] VARCHAR(512)  NULL,
[Zip] VARCHAR(512)  NULL,
[Delivery] VARCHAR(512)  NULL,
[Terms] VARCHAR(10)  NULL,
[FreightSelect] VARCHAR(512)  NULL,
[IsQManual] VARCHAR(2)  NULL,
[IsPManual] VARCHAR(2)  NULL,
[Location] VARCHAR(512)  NULL,
[Equipment] VARCHAR(10)  NULL,
[EquipCategory] VARCHAR(512)  NULL,
[QuotePrice] VARCHAR(512)  NULL,
[Credit] VARCHAR(512)  NULL,
[Freight] VARCHAR(512)  NULL,
[ShopTime] VARCHAR(512)  NULL,
[TotalCost] VARCHAR(512)  NULL,
[GrossProfit] VARCHAR(512)  NULL,
[Profit] VARCHAR(512)  NULL,
[MarkUp] VARCHAR(512)  NULL,
[Description] VARCHAR(512)  NULL,
[Quant1] VARCHAR(512)  NULL,
[Descr1] VARCHAR(512)  NULL,
[Costs1] VARCHAR(512)  NULL,
[ECost1] VARCHAR(512)  NULL,
[Price1] VARCHAR(512)  NULL,
[Quant2] VARCHAR(512)  NULL,
[Descr2] VARCHAR(512)  NULL,
[Costs2] VARCHAR(512)  NULL,
[ECost2] VARCHAR(512)  NULL,
[Price2] VARCHAR(512)  NULL,
[Quant3] VARCHAR(512)  NULL,
[Descr3] VARCHAR(512)  NULL,
[Costs3] VARCHAR(512)  NULL,
[ECost3] VARCHAR(512)  NULL,
[Price3] VARCHAR(512)  NULL,
[Quant4] VARCHAR(512)  NULL,
[Descr4] VARCHAR(512)  NULL,
[Costs4] VARCHAR(512)  NULL,
[ECost4] VARCHAR(512)  NULL,
[Price4] VARCHAR(512)  NULL,
[Quant5] VARCHAR(512)  NULL,
[Descr5] VARCHAR(512)  NULL,
[Costs5] VARCHAR(512)  NULL,
[ECost5] VARCHAR(512)  NULL,
[Price5] VARCHAR(512)  NULL,
[Quant6] VARCHAR(512)  NULL,
[Descr6] VARCHAR(512)  NULL,
[Costs6] VARCHAR(512)  NULL,
[ECost6] VARCHAR(512)  NULL,
[Price6] VARCHAR(512)  NULL,
[Quant7] VARCHAR(512)  NULL,
[Descr7] VARCHAR(512)  NULL,
[Costs7] VARCHAR(512)  NULL,
[ECost7] VARCHAR(512)  NULL,
[Price7] VARCHAR(512)  NULL,
[Quant8] VARCHAR(512)  NULL,
[Descr8] VARCHAR(512)  NULL,
[Costs8] VARCHAR(512)  NULL,
[ECost8] VARCHAR(512)  NULL,
[Price8] VARCHAR(512)  NULL,
[Quant9] VARCHAR(512)  NULL,
[Descr9] VARCHAR(512)  NULL,
[Costs9] VARCHAR(512)  NULL,
[ECost9] VARCHAR(512)  NULL,
[Price9] VARCHAR(512)  NULL,
[Quant10] VARCHAR(512)  NULL,
[Descr10] VARCHAR(512)  NULL,
[Costs10] VARCHAR(512)  NULL,
[ECost10] VARCHAR(512)  NULL,
[Price10] VARCHAR(512)  NULL,
[Quant11] VARCHAR(512)  NULL,
[Descr11] VARCHAR(512)  NULL,
[Costs11] VARCHAR(512)  NULL,
[ECost11] VARCHAR(512)  NULL,
[Price11] VARCHAR(512)  NULL,
[Quant12] VARCHAR(512)  NULL,
[Descr12] VARCHAR(512)  NULL,
[Costs12] VARCHAR(512)  NULL,
[ECost12] VARCHAR(512)  NULL,
[Price12] VARCHAR(512)  NULL,
[Quant13] VARCHAR(512)  NULL,
[Descr13] VARCHAR(512)  NULL,
[Costs13] VARCHAR(512)  NULL,
[ECost13] VARCHAR(512)  NULL,
[Price13] VARCHAR(512)  NULL,
[Quant14] VARCHAR(512)  NULL,
[Descr14] VARCHAR(512)  NULL,
[Costs14] VARCHAR(512)  NULL,
[ECost14] VARCHAR(512)  NULL,
[Price14] VARCHAR(512)  NULL,
[Quant15] VARCHAR(512)  NULL,
[Descr15] VARCHAR(512)  NULL,
[Costs15] VARCHAR(512)  NULL,
[ECost15] VARCHAR(512)  NULL,
[Price15] VARCHAR(512)  NULL,
[Quant16] VARCHAR(512)  NULL,
[Descr16] VARCHAR(512)  NULL,
[Costs16] VARCHAR(512)  NULL,
[ECost16] VARCHAR(512)  NULL,
[Price16] VARCHAR(512)  NULL,
[Quant17] VARCHAR(512)  NULL,
[Descr17] VARCHAR(512)  NULL,
[Costs17] VARCHAR(512)  NULL,
[ECost17] VARCHAR(512)  NULL,
[Price17] VARCHAR(512)  NULL,
[Quant18] VARCHAR(512)  NULL,
[Descr18] VARCHAR(512)  NULL,
[Costs18] VARCHAR(512)  NULL,
[ECost18] VARCHAR(512)  NULL,
[Price18] VARCHAR(512)  NULL,
[Quant19] VARCHAR(512)  NULL,
[Descr19] VARCHAR(512)  NULL,
[Costs19] VARCHAR(512)  NULL,
[ECost19] VARCHAR(512)  NULL,
[Price19] VARCHAR(512)  NULL,
[Quant20] VARCHAR(512)  NULL,
[Descr20] VARCHAR(512)  NULL,
[Costs20] VARCHAR(512)  NULL,
[ECost20] VARCHAR(512)  NULL,
[Price20] VARCHAR(512)  NULL,
[Quant21] VARCHAR(512)  NULL,
[Descr21] VARCHAR(512)  NULL,
[Costs21] VARCHAR(512)  NULL,
[ECost21] VARCHAR(512)  NULL,
[Price21] VARCHAR(512)  NULL,
[Quant22] VARCHAR(512)  NULL,
[Descr22] VARCHAR(512)  NULL,
[Costs22] VARCHAR(512)  NULL,
[ECost22] VARCHAR(512)  NULL,
[Price22] VARCHAR(512)  NULL,
[Quant23] VARCHAR(512)  NULL,
[Descr23] VARCHAR(512)  NULL,
[Costs23] VARCHAR(512)  NULL,
[ECost23] VARCHAR(512)  NULL,
[Price23] VARCHAR(512)  NULL,
[InvNotes] VARCHAR(512)  NULL,
[DeliveryNotes] VARCHAR(512)  NULL,
[QuoteNotes] VARCHAR(512)  NULL,
[Spare1] VARCHAR(512)  NULL,
[Spare2] VARCHAR(512)  NULL,
[Spare3] VARCHAR(512)  NULL,
[Spare4] VARCHAR(512)  NULL,
[Spare5] VARCHAR(512)  NULL
                          )";

            #endregion

            #region CompanyTable creator string

            // This is the query which will create a new table in our database file. An auto increment column called "ID", and many NVARCHAR type columns
            string createTableQueryCompany = @"CREATE TABLE IF NOT EXISTS [CompanyTable] (
[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
[Company] NVARCHAR(512)  NULL,
[Street1] VARCHAR(512)  NULL,
[Street2] VARCHAR(512)  NULL,
[City] VARCHAR(512)  NULL,
[State] VARCHAR(512)  NULL,
[Zip] VARCHAR(512)  NULL,
[Phone] VARCHAR(80)  NULL,
[Fax] VARCHAR(80)  NULL
                          )";

            #endregion

            #region PartsTable creator string

            // This is the query which will create a new table in our database file. An auto increment column called "ID", and many NVARCHAR type columns
            string createTableQueryParts = @"CREATE TABLE IF NOT EXISTS [PartsTable] (
[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
[Category] NVARCHAR(512)  NULL,
[Description] VARCHAR(512)  NULL,
[Price] VARCHAR(512)  NULL
                          )";

            #endregion

            #region ReportsTable creator string

            // This is the query which will create a new table in our database file. An auto increment column called "ID", and many NVARCHAR type columns
            string createTableQueryReports = @"CREATE TABLE IF NOT EXISTS [ReportsTable] (
[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
[TableName] NVARCHAR(512)  NULL,
[Name] NVARCHAR(512)  NULL,
[Columns] VARCHAR(512)  NULL
                          )";

            #endregion


            System.Data.SQLite.SQLiteConnection.CreateFile(getDbasePathName());        // Create the file which will be hosting our database
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    con.Open();                             // Open the connection to the database

                    com.CommandText = createTableQuery1;    // Set CommandText to our query that will create a table
                    com.ExecuteNonQuery();                  // Execute the query

                    com.CommandText = createTableQuery2;    // Set CommandText to our query that will create a table
                    com.ExecuteNonQuery();                  // Execute the query

                    com.CommandText = createTableQueryCompany;// Set CommandText to our query that will create a table
                    com.ExecuteNonQuery();                  // Execute the query

                    com.CommandText = createTableQueryParts;// Set CommandText to our query that will create a table
                    com.ExecuteNonQuery();                  // Execute the query

                    com.CommandText = createTableQueryReports;// Set CommandText to our query that will create a table
                    com.ExecuteNonQuery();                  // Execute the query
                }
            }
        }

        #endregion


        #region Dbase Low-Level Helpers for Orders

        private void vacuumDatabase()
        {
            Killconnection();

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "VACUUM;";

                    try
                    {
                        con.Open();
                        com.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }
        }

        private int _OrderTotal = 0;

        private int getRowCountsFromOrders()
        {
            _log.append("getRowCountsFromOrders start");

            //if (_OrderTotal != 0)
            //{
            //    if (getRandom() > 20)
            //        return _OrderTotal;
            //}

            int? count = 0;
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    try
                    {
                        //con.Open();
                        com.CommandText = "Select COUNT(PO) From OrderTable";      // Select all rows from our database table
                        object o = com.ExecuteScalar();
                        count = Convert.ToInt32(o);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            _log.append("getRowCountsFromOrders end");

            if (count.HasValue)
                _OrderTotal = count.Value;

            return (count == null) ? 0 : count.Value;
        }

        private void deletePOFromOrders(string PO)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    try
                    {
                        // con.Open();
                        com.CommandText = "DELETE From OrderTable Where PO = '" + PO + "'";
                        com.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }
        }

        private List<string> getPOsFromOrders()
        {
            _log.append("getPOsFromOrders start");

            List<string> POs = new List<string>();
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    // Open the connection to the database
                    //com.CommandText = "Select PO FROM OrderTable";

                    try
                    {
                        //con.Open();

                        com.CommandText = "Select PO FROM OrderTable";

                        using (SQLiteDataAdapter DB = new SQLiteDataAdapter(com))
                        //using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            DataSet DS = new DataSet();

                            DB.Fill(DS, "OrderTable");

                            if (DS.Tables["OrderTable"].Rows.Count != 0)
                            {
                                _log.append("getPOsFromOrders while start");

                                DataRow rrr;
                                int max = getRowCountsFromOrders();
                                for (int i = 0; i < max; i++)
                                {
                                    #region reads as strings
                                    rrr = DS.Tables["OrderTable"].Rows[i];
                                    POs.Add(rrr["PO"] as string);
                                    #endregion
                                }

                                _log.append("getPOsFromOrders while end");
                            }
                        }

                        //using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        //{
                        //    _log.append("getPOsFromOrders while start");
                        //    while (reader.Read())
                        //    {
                        //        POs.Add(reader["PO"] as string);
                        //    }
                        //    _log.append("getPOsFromOrders while end");
                        //}
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            _log.append("getPOsFromOrders end");
            return POs;
        }

        private bool readRowOrders(string PO, int rowCount, out List<string> guis)
        {
            _log.append("readRowOrders start");

            guis = new List<string>();

            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    if (PO == "")
                        com.CommandText = "Select * FROM OrderTable";
                    else
                        com.CommandText = "Select * FROM OrderTable Where PO = '" + PO + "'";      // Select all rows from our database table

                    try
                    {

                        //con.Open();

                        _log.append("readRowOrders conn opened");

                        using (SQLiteDataAdapter DB = new SQLiteDataAdapter(com))
                        //using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            DataSet DS = new DataSet();

                            DB.Fill(DS, "OrderTable");

                            _log.append("readRowOrders DB filled");

                            if (DS.Tables["OrderTable"].Rows.Count != 0)
                            {
                                DataRow rrr = DS.Tables["OrderTable"].Rows[rowCount];

                                for (int i = 0; i < Order.NameCount; i++)
                                {
                                    guis.Add(rrr[Order.GetTableName(i)] as string);
                                }
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

            guis = Order.Defaults;

            guis[0] = generateNextOrderPO();
            guis[1] = todaysDate();

            this.textBoxSoldToReadOnly.Text = guis[9]; // mirrors

            return false;
        }


        private int updateRowOrders(List<string> guis)
        {
            _log.append("updateRowOrders start");
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = Order.UpdateRowCommandText;
                    for (int i = 0; i < Order.NameCount; i++)
                    {
                        com.Parameters.AddWithValue("@" + Order.GetTableName(i), guis[i]);
                    }
                    try
                    {
                        int t = com.ExecuteNonQuery();      // Execute the query

                        _log.append("updateRowOrders end1");

                        return t;
                    }
                    catch (SqlException ex)
                    {
                        _log.append("updateRowOrders end2");
                        MessageBox.Show(this, "Database error: " + ex.Message);

                        return 0;
                    }
                }
            }
        }

        private int insertRowOrders(List<string> guis)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = Order.InsertRowCommandText;
                    for (int i = 0; i < Order.NameCount; i++)
                    {
                        com.Parameters.AddWithValue("@" + Order.GetTableName(i), guis[i]);
                    }
                    try
                    {
                        return com.ExecuteNonQuery();      // Execute the query
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

        #region Orders read & Update GUI

        private bool readOrderAndUpdateGUI(string PO, int row)
        {
            _log.append("** " + PO + " row " + row);
            _log.append("readOrderAndUpdateGUI start");

            List<string> guis;
            readRowOrders(PO, row, out guis);

            isDataLoadingOrders = true;
            for (int i = 0; i < Order.NameCount; i++)
            {
                string name = Order.GetGUIName(i);
                Control c = GetControlByName(name);

                if (name.StartsWith("checkBox"))
                {
                    CheckBox cb = (CheckBox)c;
                    cb.Checked = isStringTrue(guis[i]);
                }
                else
                {
                    c.Text = guis[i];
                }
            }

            this.textBoxSoldToReadOnly.Text = guis[9]; // mirrors

            isDataLoadingOrders = false;

            _log.append("readOrderAndUpdateGUI end~");

            bool locked = LowLevelLockChecking(dbType.Order);

            autoSelectSignature();

            _log.append("readOrderAndUpdateGUI end");

            return locked;
        }

        #endregion

        #region Orders update row

        private void updateRowOrders()
        {
            List<string> guis = GUIS_Order;
            int rowsWritten = updateRowOrders(guis);

            this.isDataDirtyOrders = false;
            this.toolStripStatusLabel.Text = "Status Orders: Clean";

            // remove the lock
            if (removeLockOrder() == false)
            {
                MessageBox.Show(this, "Could not find the lock file.  Data loss could have occured on this record.", "Lock Error");
            }
        }

        #endregion

        #region Orders insert row

        private void insertNewDataRowOrders(string PO)
        {
            List<string> guis = GUIS_Order;
            int rowsWritten = insertRowOrders(guis);

            isDataDirtyOrders = false;
            this.toolStripStatusLabel.Text = "Status Orders: Clean";
        }

        #endregion


        #region Dbase Low-Level Helpers for Quotes

        private int getRowCountsFromQuotes()
        {
            int? count = 0;
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select COUNT(*) From QuoteTable";      // Select all rows from our database table

                    try
                    {
                        //con.Open();
                        object o = com.ExecuteScalar();
                        count = Convert.ToInt32(o);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            return (count == null) ? 0 : count.Value;
        }

        private void deletePOFromQuotes(string QPO)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "DELETE From QuoteTable Where PO = '" + QPO + "'";
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

        private List<string> getPOsFromQuotes()
        {
            List<string> POs = new List<string>();

            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "Select PO FROM QuoteTable";

                    try
                    {
                        //con.Open();

                        using (SQLiteDataAdapter DB = new SQLiteDataAdapter(com))
                        //using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            DataSet DS = new DataSet();

                            DB.Fill(DS, "QuoteTable");

                            if (DS.Tables["QuoteTable"].Rows.Count != 0)
                            {

                                DataRow rrr;
                                int max = getRowCountsFromQuotes();
                                for (int i = 0; i < max; i++)
                                {
                                    #region reads as strings
                                    rrr = DS.Tables["QuoteTable"].Rows[i];
                                    POs.Add(rrr["PO"] as string);
                                    #endregion
                                }
                            }
                        }
                        //using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        //{
                        //   while (reader.Read())
                        //   {
                        //       POs.Add(reader["PO"] as string);
                        //   }
                        // }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(this, "Database error: " + ex.Message);
                    }
                }
            }

            return POs;
        }

        private bool readRowQuotes(string QPO, int rowCount, out List<string> guis)
        {
            guis = new List<string>();

            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    if (QPO == "")
                        com.CommandText = "Select * FROM QuoteTable";
                    else
                        com.CommandText = "Select * FROM QuoteTable Where PO = '" + QPO + "'";      // Select all rows from our database table

                    try
                    {
                        //con.Open();

                        using (SQLiteDataAdapter DB = new SQLiteDataAdapter(com))
                        //using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                        {
                            DataSet DS = new DataSet();

                            DB.Fill(DS, "QuoteTable");

                            if (DS.Tables["QuoteTable"].Rows.Count != 0)
                            {

                                DataRow rrr = DS.Tables["QuoteTable"].Rows[rowCount == 0 ? 0 : rowCount - 1];

                                for (int i = 0; i < Quote.NameCount; i++)
                                {
                                    guis.Add(rrr[Quote.GetTableName(i)] as string);
                                }
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

            guis = Quote.Defaults;

            guis[0] = generateNextQuotePO();
            guis[1] = todaysDate();

            this.textBoxQCompanyReadOnly.Text = guis[3]; // mirrors
            this.textBoxQQuotedPriceReadOnly.Text = guis[23]; // mirrors

            return false;
        }

        private int updateRowQuotes(List<string> guis)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = Quote.UpdateRowCommandText;
                    for (int i = 0; i < Quote.NameCount; i++)
                    {
                        com.Parameters.AddWithValue("@" + Quote.GetTableName(i), guis[i]);
                    }
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

        private int insertRowQuotes(List<string> guis)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {

                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = Quote.InsertRowCommandText;
                    for (int i = 0; i < Quote.NameCount; i++)
                    {
                        com.Parameters.AddWithValue("@" + Quote.GetTableName(i), guis[i]);
                    }
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

        #region Quote read & Update GUI

        private bool readQuoteAndUpdateGUI(string QPO, int row)
        {
            List<string> guis;
            readRowQuotes(QPO, row, out guis);

            isDataLoadingQuotes = true;
            for (int i = 0; i < Quote.NameCount; i++)
            {
                string name = Quote.GetGUIName(i);
                Control c = GetControlByName(name);

                if (name.StartsWith("checkBox"))
                {
                    CheckBox cb = (CheckBox)c;
                    cb.Checked = isStringTrue(guis[i]);
                }
                else
                {
                    c.Text = guis[i];
                }
            }

            this.textBoxQCompanyReadOnly.Text = guis[3]; // mirrors
            this.textBoxQQuotedPriceReadOnly.Text = guis[23]; // mirrors

            isDataLoadingQuotes = false;

            bool locked = LowLevelLockChecking(dbType.Quote);

            autoSelectSignature();

            return locked;
        }

        #endregion

        #region Quotes update row

        private void updateRowQuotes()
        {
            List<string> guis = GUIS_Quote;
            int rowsWritten = updateRowQuotes(guis);

            this.isDataDirtyQuotes = false;
            this.toolStripStatusLabel2.Text = "Status Quote: Clean";

            // remove the lock
            if (removeLockQuote() == false)
            {
                MessageBox.Show(this, "Could not find the lock file.  Data loss could have occured on this record.", "Lock Error");
            }

            //MessageBox.Show(rowsWritten.ToString());
        }

        #endregion

        #region Quotes insert row

        private void insertNewDataRowQuotes(string QPO)
        {
            List<string> guis = GUIS_Quote;
            int rowsWritten = insertRowQuotes(guis);

            isDataDirtyQuotes = false;
            this.toolStripStatusLabel2.Text = "Status Quote: Clean";
        }

        #endregion


        #region Database Locking

        private void generateLockName()
        {
            this._lockName = RandomString(8);
        }

        #region place & remove locks

        private bool placeLockOrder()
        {
            try
            {
                removeLockOrder();
                createLock(this.toolStripTextBoxDbasePath.Text + "!_O" + this._lockName + ".tmp", this.textBoxPO.Text);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool placeLockQuote()
        {
            try
            {
                removeLockQuote();
                createLock(this.toolStripTextBoxDbasePath.Text + "!_Q" + this._lockName + ".tmp", this.textBoxQPO.Text);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool removeLockOrder()
        {
            try
            {
                if (File.Exists(this.toolStripTextBoxDbasePath.Text + "!_O" + this._lockName + ".tmp"))
                {
                    File.Delete(this.toolStripTextBoxDbasePath.Text + "!_O" + this._lockName + ".tmp");
                    return true;
                }
            }
            catch { }

            return false;
        }

        private bool removeLockQuote()
        {
            try
            {
                if (File.Exists(this.toolStripTextBoxDbasePath.Text + "!_Q" + this._lockName + ".tmp"))
                {
                    File.Delete(this.toolStripTextBoxDbasePath.Text + "!_Q" + this._lockName + ".tmp");
                    return true;
                }
            }
            catch { }
            
            return false;
        }

        private bool removeLockOrders(string po)
        {
            try
            {
                string[] files = Directory.GetFiles(this.toolStripTextBoxDbasePath.Text, "!_O*.tmp");

                foreach (string file in files)
                {
                    IEnumerable<string> lines = File.ReadLines(file);

                    if (lines.ElementAt(1) == po)
                    {
                        File.Delete(file);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        private bool removeLockQuotes(string po)
        {
            try
            {
                string[] files = Directory.GetFiles(this.toolStripTextBoxDbasePath.Text, "!_Q*.tmp");

                foreach (string file in files)
                {
                    IEnumerable<string> lines = File.ReadLines(file);

                    if (lines.ElementAt(1) == po)
                    {
                        File.Delete(file);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        #endregion

        private void createLock(string fileName, string recordId)
        {
            string nowTicks = DateTime.Now.Ticks.ToString();
            string text = nowTicks + Environment.NewLine + recordId;
            File.WriteAllText(fileName, text);
        }

        private List<string> getLockListOrders()
        {
            List<string> list = new List<string>();

            try
            {

                string[] files = Directory.GetFiles(this.toolStripTextBoxDbasePath.Text, "!_O*.tmp");

                foreach (string file in files)
                {
                    if (file.Contains("!_O" + this._lockName))
                        continue;

                    IEnumerable<string> lines = File.ReadLines(file);
                    list.Add(lines.ElementAt(1));
                }
            }
            catch
            {
                return null;
            }

            return list;
        }

        private List<string> getLockListQuotes()
        {
            List<string> list = new List<string>();

            try
            {
                string[] files = Directory.GetFiles(this.toolStripTextBoxDbasePath.Text, "!_Q*.tmp");

                foreach (string file in files)
                {
                    if (file.Contains("!_Q" + this._lockName))
                        continue;

                    IEnumerable<string> lines = File.ReadLines(file);
                    list.Add(lines.ElementAt(1));
                }
            }
            catch
            {
                return null;
            }

            return list;
        }

        #endregion

        private SQLiteConnection __con = null;

        #region General Helpers

        public SQLiteConnection GetConnection()
        {
            if (__con == null)
            {
                __con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName());

                try
                {
                    __con.Open();
                }
                catch //(SQLException e)
                {
                    Killconnection();
                }
            }

            return __con;
        }

        private void Killconnection()
        {
            if (__con != null)
            {
                try
                {
                    __con.Close();
                }
                catch { }
                __con = null;
            }
        }

        private static Random random = new Random((int)DateTime.Now.Ticks);
        private string RandomString(int Size)
        {
            string input = "0123456789";
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < Size; i++)
            {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }
            return builder.ToString();
        }

        private string getDbasePathName()
        {
            return this.toolStripTextBoxDbasePath.Text.Trim() + this._DBASENAME;
            //+ "; New=true; Version=3; PRAGMA cache_size=20000; PRAGMA page_size=32768; PRAGMA synchronous=off";
        }

        private bool isStringTrue(string value)
        {
            if (value == "")
                return false;
            else
                return (value != "0");
        }

        private string todaysDate()
        {
            return DateTime.Now.ToString(@"MM/dd/yyyy");
        }

        private string todaysDateTime()
        {
            return DateTime.Now.ToString(@"MM/dd/yyyy    HH:mm");
        }

        private DialogResult askImportantQuestion(string question)
        {
            return MessageBox.Show(question, "RMT Question?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        private string generatePDFileName(string type, string ID)
        {
            string docs = this.toolStripTextBoxDbasePath.Text + @"PDF\";//Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string date = DateTime.Now.ToString(@"_hhmm_");

            if (!Directory.Exists(docs))
                Directory.CreateDirectory(docs);

            string filename;
            int count = 1;
            do
            {
                filename = docs + type + "_" + ID + date + count + ".pdf";
                count++;
            } while (System.IO.File.Exists(filename));

            return filename;
        }

        private List<SortableRow> buildSortedRows()
        {
            List<string> posUnsorted = getPOsFromOrders();
            List<SortableRow> POsorted = new List<SortableRow>();

            int row = 1;
            foreach (string PO in posUnsorted)
            {
                SortableRow sr = new SortableRow();
                sr.PO = PO;
                sr.Row = row++;
                POsorted.Add(sr);
            }

            POsorted.Sort();

            return POsorted;
        }

        private List<string> GUIS_Order
        {
            get
            {
                List<string> guis = new List<string>();
                for (int i = 0; i < Order.NameCount; i++)
                {
                    string name = Order.GetGUIName(i);
                    Control c = GetControlByName(name);
                    if (name.StartsWith("checkBox"))
                    {
                        CheckBox cb = (CheckBox)c;
                        guis.Add((cb.Checked) ? "1" : "0");
                    }
                    else
                    {
                        guis.Add(c.Text);
                    }
                }
                return guis;
            }
        }


        private List<string> GUIS_Quote
        {
            get
            {
                List<string> guis = new List<string>();
                for (int i = 0; i < Quote.NameCount; i++)
                {
                    string name = Quote.GetGUIName(i);
                    Control c = GetControlByName(name);
                    if (name.StartsWith("checkBox"))
                    {
                        CheckBox cb = (CheckBox)c;
                        guis.Add((cb.Checked) ? "1" : "0");
                    }
                    else
                    {
                        guis.Add(c.Text);
                    }
                }
                return guis;
            }
        }

        Control GetControlByName(string Name)
        {
            foreach (Control c in this.Controls)
            {
                Control subC = search_me(c, Name);
                if (subC != null)
                    return subC;
            }
            return null;
        }

        Control search_me(System.Windows.Forms.Control ParentCntl, string NameToSearch)
        {
            if (ParentCntl.Name == NameToSearch)
                return ParentCntl;

            foreach (Control ChildCntl in ParentCntl.Controls)
            {
                Control ResultCntl = search_me(ChildCntl, NameToSearch);
                if (ResultCntl != null)
                    return ResultCntl;
            }
            return null;
        }
        #endregion

        #region Debug - import Companies & Parts .... remove later

        private void importCompaniesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importCompaniesDebug();
        }

        private void importPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\" ;
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            string[] lines = File.ReadAllLines(openFileDialog1.FileName);

            FormParts fp = new FormParts(this, getDbasePathName());

            foreach (string line in lines)
            {
                List<string> cols = CSV.Parse(line);

                if (cols.Count() < 3)
                    continue;

                string cat = cols.ElementAt(0);
                string dsr = cols.ElementAt(1);
                string prc = cols.ElementAt(2);

                fp.AddImportPart(cat, dsr, prc);
            }
        }

        private void importCompaniesDebug()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\" ;
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            string[] lines = File.ReadAllLines(openFileDialog1.FileName);

            FormCompany fc = new FormCompany(this, getDbasePathName());

            foreach (string line in lines)
            {
                List<string> cols = CSV.Parse(line);

                if (cols.Count() != 7)
                    continue;

                string name = cols.ElementAt(0);
                string addr = cols.ElementAt(1);
                string city = cols.ElementAt(2);
                string stat = cols.ElementAt(3);
                string zipc = cols.ElementAt(4);
                string phon = cols.ElementAt(5);
                string faxn = cols.ElementAt(6);

                fc.AddImportCompany(name, addr, city, stat, zipc, phon, faxn);
            }
        }

        #endregion

    }
}
