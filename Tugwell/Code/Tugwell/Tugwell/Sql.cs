using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite; // for sql dbase
using System.Windows.Forms;

namespace Tugwell
{
    internal class Sql
    {
        static private SQLiteConnection __con = null;
        static public string dbName;

        #region Create New Dbase & Helpers

        static public void AddNewTableCol(string tableName, string newCol)
        {//alter table mytable add column colnew char(50)
            string addTableCol = @"ALTER TABLE " + tableName + " ADD COLUMN " + newCol + @" VARCHAR(512)";

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + dbName))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    con.Open();                             // Open the connection to the database

                    com.CommandText = addTableCol;    // Set CommandText to our query that will create a table
                    com.ExecuteNonQuery();                  // Execute the query
                }
            }
        }

        static public void AppendNewTableWithDefaultRow(string version)
        {
            #region New Version Table creator string

            // This is the query which will create a new table in our database file. An auto increment column called "ID", and many NVARCHAR type columns
            string createNewTable = @"CREATE TABLE IF NOT EXISTS [VersionTable] (
[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
[DBVersion] NVARCHAR(512)  NULL,
[Spare1] NVARCHAR(512)  NULL,
[Spare2] VARCHAR(512)  NULL
                          )";
            #endregion

            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + dbName))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = createNewTable;
                    com.ExecuteNonQuery();


                    StringBuilder sb = new StringBuilder();

                    sb.Append("INSERT INTO VersionTable (");
                    sb.Append("DBVersion, ");
                    sb.Append("Spare1, ");
                    sb.Append("Spare2");
                    sb.Append(") VALUES (");
                    sb.Append("@DBVersion, ");
                    sb.Append("@Spare1, ");
                    sb.Append("@Spare2");
                    sb.Append(")");

                    com.CommandText = sb.ToString();

                    com.Parameters.AddWithValue("@DBVersion", version);
                    com.Parameters.AddWithValue("@Spare1", "");
                    com.Parameters.AddWithValue("@Spare2", "");

                    com.ExecuteNonQuery();
                }
            }
        }

        static public List<string> ReadGenericTable(string tableName, int rowIndex, List<string> rowNames)
        {
            List<string> datas = new List<string>();

            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    com.CommandText = "Select * FROM " + tableName;
                    try
                    {
                        using (SQLiteDataAdapter DB = new SQLiteDataAdapter(com))
                        {
                            try
                            {
                                DataSet DS = new DataSet();
                                DB.Fill(DS, tableName);

                                DataRow rrr = DS.Tables[tableName].Rows[rowIndex];

                                for (int i = 0; i < rowNames.Count(); i++)
                                {
                                    datas.Add(rrr[rowNames[i]] as string);
                                }
                                return datas;
                            }
                            catch { }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                    }
                }
            }
            return null;
        }

        static public List<string> GetTableRowNames(string TableName)
        {
            List<string> list = new List<string>();
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + dbName))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "SELECT * FROM " + TableName;

                    SQLiteDataAdapter da = new SQLiteDataAdapter(com);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataColumn col in dt.Columns)
                    {
                        list.Add(col.ColumnName);
                    }
                }
            }
            return list;
        }

        static public void RemoveVersionTable()
        {
            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    try
                    {
                        com.CommandText = "DROP Table VersionTable";
                        com.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                    }
                }
            }
        }

        static public void CreateNewDbase()
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

            Killconnection();

            System.Data.SQLite.SQLiteConnection.CreateFile(dbName);        // Create the file which will be hosting our database
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + dbName))
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

        static public bool VacuumDatabase()
        {
            Killconnection();

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + dbName))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    com.CommandText = "VACUUM;";
                    try
                    {
                        con.Open();
                        com.ExecuteNonQuery();
                    }
                    catch //(Exception ex)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion
        
        #region Dbase Low-Level Helpers

        //static private int _OrderTotal = 0;

        static public int GetRowCounts(FormMain.dbType type)
        {
            int? count = 0;
            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    try
                    {
                        if (type == FormMain.dbType.Order)
                        {
                            com.CommandText = Order.GetRowCountCommandText;
                        }
                        else
                        {
                            com.CommandText = Quote.GetRowCountCommandText;
                        }
                        object o = com.ExecuteScalar();
                        count = Convert.ToInt32(o);
                    }
                    catch// (Exception ex)
                    {
                    }
                }
            }

            //if (type == FormMain.dbType.Order && count.HasValue)
            //{
            //    _OrderTotal = count.Value;
            //}
            return (count == null) ? 0 : count.Value;
        }

        static public void DeletePO(FormMain.dbType type, string PO)
        {
            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    try
                    {
                        if (type == FormMain.dbType.Order)
                        {
                            com.CommandText = Order.DeletePOCommandText(PO);
                        }
                        else
                        {
                            com.CommandText = Quote.DeletePOCommandText(PO);
                        }
                        com.ExecuteNonQuery();

                        //if (type == FormMain.dbType.Order)
                        //    Sql._OrderTotal--;
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                    }
                }
            }
        }

        static public List<string> GetPOs(FormMain.dbType type)
        {
            List<string> POs = new List<string>();
            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    try
                    {
                        if (type == FormMain.dbType.Order)
                            com.CommandText = Order.GetPOsCommandText;
                        else
                            com.CommandText = Quote.GetPOsCommandText;

                        using (SQLiteDataAdapter DB = new SQLiteDataAdapter(com))
                        {
                            DataSet DS = new DataSet();
                            if (type == FormMain.dbType.Order)
                            {
                                DB.Fill(DS, "OrderTable");

                                if (DS.Tables["OrderTable"].Rows.Count != 0)
                                {
                                    DataRow rrr;
                                    int max = GetRowCounts(type);
                                    for (int i = 0; i < max; i++)
                                    {
                                        #region reads as strings
                                        rrr = DS.Tables["OrderTable"].Rows[i];
                                        POs.Add(rrr["PO"] as string);
                                        #endregion
                                    }
                                }
                            }
                            else
                            {
                                DB.Fill(DS, "QuoteTable");

                                if (DS.Tables["QuoteTable"].Rows.Count != 0)
                                {
                                    DataRow rrr;
                                    int max = GetRowCounts(type);
                                    for (int i = 0; i < max; i++)
                                    {
                                        #region reads as strings
                                        rrr = DS.Tables["QuoteTable"].Rows[i];
                                        POs.Add(rrr["PO"] as string);
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                    }
                }
            }
            return POs;
        }

        static public List<string> ReadRow(FormMain.dbType type, string PO, int rowCount, out List<string> guis)
        {
            guis = new List<string>();

            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand("SET ARITHABORT ON", con))
                {
                    if (type == FormMain.dbType.Order)
                    {
                        com.CommandText = Order.ReadRowCommandText(PO);
                    }
                    else
                    {
                        com.CommandText = Quote.ReadRowCommandText(PO);
                    }
                    try
                    {
                        using (SQLiteDataAdapter DB = new SQLiteDataAdapter(com))
                        {
                            DataSet DS = new DataSet();

                            if (type == FormMain.dbType.Order)
                            {
                                DB.Fill(DS, "OrderTable");

                                if (DS.Tables["OrderTable"].Rows.Count != 0)
                                {
                                    DataRow rrr = DS.Tables["OrderTable"].Rows[rowCount];

                                    for (int i = 0; i < Order.NameCount; i++)
                                    {
                                        guis.Add(rrr[Order.GetTableName(i)] as string);
                                    }
                                    return guis;
                                }
                            }
                            else
                            {
                                DB.Fill(DS, "QuoteTable");

                                if (DS.Tables["QuoteTable"].Rows.Count != 0)
                                {

                                    DataRow rrr = DS.Tables["QuoteTable"].Rows[rowCount == 0 ? 0 : rowCount - 1];

                                    for (int i = 0; i < Quote.NameCount; i++)
                                    {
                                        guis.Add(rrr[Quote.GetTableName(i)] as string);
                                    }
                                    return guis;
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                    }
                }
            }
            return Order.Defaults;
        }


        static public int UpdateRow(FormMain.dbType type, List<string> guis)
        {
            Killconnection();
            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    if (type == FormMain.dbType.Order)
                    {
                        com.CommandText = Order.UpdateRowCommandText;
                        for (int i = 0; i < Order.NameCount; i++)
                        {
                            com.Parameters.AddWithValue("@" + Order.GetTableName(i), guis[i]);
                        }
                    }
                    else
                    {
                        com.CommandText = Quote.UpdateRowCommandText;
                        for (int i = 0; i < Quote.NameCount; i++)
                        {
                            com.Parameters.AddWithValue("@" + Quote.GetTableName(i), guis[i]);
                        }
                    }
                    try
                    {
                        int t = com.ExecuteNonQuery();      // Execute the query
                        return t;
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                        return 0;
                    }
                }
            }
        }

        static public int InsertRow(FormMain.dbType type, List<string> guis)
        {
            Killconnection();
            SQLiteConnection con = GetConnection();
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    if (type == FormMain.dbType.Order)
                    {
                        com.CommandText = Order.InsertRowCommandText;
                        for (int i = 0; i < Order.NameCount; i++)
                        {
                            com.Parameters.AddWithValue("@" + Order.GetTableName(i), guis[i]);
                        }
                    }
                    else
                    {
                        com.CommandText = Quote.InsertRowCommandText;
                        for (int i = 0; i < Quote.NameCount; i++)
                        {
                            com.Parameters.AddWithValue("@" + Quote.GetTableName(i), guis[i]);
                        }
                    }
                    try
                    {
                        int cnt = com.ExecuteNonQuery();      // Execute the query
                        //if (type == FormMain.dbType.Order)
                        //    Sql._OrderTotal++;
                        return cnt;
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                        return 0;
                    }
                }
            }
        }

        #endregion
        
        
        #region General Helpers

        static public SQLiteConnection GetConnection()
        {
            if (__con == null)
            {
                __con = new System.Data.SQLite.SQLiteConnection("data source=" + dbName);

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

        static public void Killconnection()
        {
            //_OrderTotal = 0;

            if (__con != null)
            {
                try
                {
                    __con.Close();
                    __con.Dispose();
                }
                catch { }
                __con = null;
            }
        }

        #endregion

        #region Debug - import Companies & Parts .... remove later

        //private void importCompaniesToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    importCompaniesDebug();
        //}

        //private void importPartsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog openFileDialog1 = new OpenFileDialog();

        //    //openFileDialog1.InitialDirectory = "c:\\" ;
        //    openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
        //    openFileDialog1.FilterIndex = 2;
        //    openFileDialog1.RestoreDirectory = true;

        //    if (openFileDialog1.ShowDialog() != DialogResult.OK)
        //        return;
        //    string[] lines = File.ReadAllLines(openFileDialog1.FileName);

        //    FormParts fp = new FormParts(this, getDbasePathName());

        //    foreach (string line in lines)
        //    {
        //        List<string> cols = CSV.Parse(line);

        //        if (cols.Count() < 3)
        //            continue;

        //        string cat = cols.ElementAt(0);
        //        string dsr = cols.ElementAt(1);
        //        string prc = cols.ElementAt(2);

        //        fp.AddImportPart(cat, dsr, prc);
        //    }
        //}

        //private void importCompaniesDebug()
        //{
        //    OpenFileDialog openFileDialog1 = new OpenFileDialog();

        //    //openFileDialog1.InitialDirectory = "c:\\" ;
        //    openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
        //    openFileDialog1.FilterIndex = 2;
        //    openFileDialog1.RestoreDirectory = true;

        //    if (openFileDialog1.ShowDialog() != DialogResult.OK)
        //        return;
        //    string[] lines = File.ReadAllLines(openFileDialog1.FileName);

        //    FormCompany fc = new FormCompany(this, getDbasePathName());

        //    foreach (string line in lines)
        //    {
        //        List<string> cols = CSV.Parse(line);

        //        if (cols.Count() != 7)
        //            continue;

        //        string name = cols.ElementAt(0);
        //        string addr = cols.ElementAt(1);
        //        string city = cols.ElementAt(2);
        //        string stat = cols.ElementAt(3);
        //        string zipc = cols.ElementAt(4);
        //        string phon = cols.ElementAt(5);
        //        string faxn = cols.ElementAt(6);

        //        fc.AddImportCompany(name, addr, city, stat, zipc, phon, faxn);
        //    }
        //}

        #endregion
    }
}
