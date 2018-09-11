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
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            generateLockName();

            this.Text = "Tugwell V9.6 2018_08_16";
            
            // make sure dbase file is the only one in this folder
            this.toolStripTextBoxDbasePath.Text = @"Z:\Tugwell\DB\";
            this.comboBoxYearControl.Text = "2018";

            //this.toolStripComboBoxSignature.SelectedIndex = 0; // no signature

            bool isAdminVersion = false;
            this.importCompaniesToolStripMenuItem.Enabled = isAdminVersion;
            this.importPartsToolStripMenuItem.Enabled = isAdminVersion;
            this.createNewDbaseToolStripMenuItem.Enabled = isAdminVersion;
            this.displayTableToolStripMenuItem.Enabled = isAdminVersion;
            this.addTableRowToolStripMenuItem.Enabled = isAdminVersion;
            this.removeAllRecordsToolStripMenuItem.Enabled = isAdminVersion;

            // create the timeout timer and force it to run forever
            this._timeoutTimer = new Timer();
            this._timeoutTimer.Interval = 1000;
            this._timeoutTimer.Tick += _timeoutTimer_Tick;
            this._timeoutTimer.Start();

            //loadStartup();
            newTPSToolStripMenuItem_Click(null, null);
        }

        #region Auto-save timeout tick event and auto-locking check over time

        private int countSeconds = 0;

        void _timeoutTimer_Tick(object sender, EventArgs e)
        {
            if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
            {
                if (this._isAction)
                {
                    this._currentTimeout = this._MaxTimeout;
                    this._isAction = false;
                }
                else
                {
                    this._currentTimeout--;

                    if (this._currentTimeout <= 0)
                    {
                        this._currentTimeout = this._MaxTimeout;
                        
                        // auto-save any changes!
                        save(dbType.Order);
                    }
                }
            }
            else if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
            {
                if (this._isAction)
                {
                    this._currentTimeout = this._MaxTimeout;
                    this._isAction = false;
                }
                else
                {
                    this._currentTimeout--;

                    if (this._currentTimeout <= 0)
                    {
                        this._currentTimeout = this._MaxTimeout;

                        // auto-save any changes!
                        save(dbType.Quote);
                    }
                }
            }
            else
            {
                this._currentTimeout = this._MaxTimeout;
            }

            countSeconds++;

            if (countSeconds >= 13)
            {
                countSeconds = 0;

                if (!this.isDataDirtyOrders)
                {
                    LowLevelLockChecking(dbType.Order);
                }
                if (!this.isDataDirtyQuotes)
                {
                    LowLevelLockChecking(dbType.Quote);
                }
            }
        }

        #endregion

        #region Vars...

        public enum companyType { TPS, RMT };
        public enum dbType { Order, Quote };

        public companyType _company = companyType.TPS;

        public static logFile _log = new logFile("log.txt");
        
        private string _DBASENAME = "tugMain.db3";
        private int _POOrderFirst = 30100;
        private int _QuoteNoFirst = 7000;
        private readonly int _MaxTimeout = 60 * 3; // seconds

        private int _currentTimeout = 60 * 3; // seconds

        private string _lockName;

        private Timer _timeoutTimer = null;
        private bool _isAction = false;

        private bool _isOrdersSelected = true;
        
        private bool isDataLoadingOrders = false;
        private bool isDataDirtyOrders = false;
        private int _currentRowOrders = 0;

        private bool isDataLoadingQuotes = false;
        private bool isDataDirtyQuotes = false;
        private int _currentRowQuotes = 0;

        private bool _isLetterControlEnabled = true;
        private List<int> _letterRows = new List<int>();

        private bool onetime_killOrderComboEvent = false;
        private bool onetime_killQuoteComboEvent = false;

        #endregion

        #region Tab Control Main Event

        private void tabControlMainOrdersQuotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControlMainOrdersQuotes.SelectedIndex == 0)
                this._isOrdersSelected = true;
            else
                this._isOrdersSelected = false;

            if (this._isOrdersSelected)
            {
                this.pONumberToolStripMenuItem.Enabled = true;
                this.quoteNumberToolStripMenuItem.Enabled = false;
                this.printCurrentOrderToolStripMenuItem.Enabled = true;
                this.printCurrentQuoteToolStripMenuItem.Enabled = false;
                
                refreshRecordIndicator(dbType.Order);
            }
            else
            {
                this.pONumberToolStripMenuItem.Enabled = false;
                this.quoteNumberToolStripMenuItem.Enabled = true;
                this.printCurrentOrderToolStripMenuItem.Enabled = false;
                this.printCurrentQuoteToolStripMenuItem.Enabled = true;

                refreshRecordIndicator(dbType.Quote);
            }

            //autoSelectSignature();
        }

        #endregion


        #region Loading Startup...

        private void loadStartup()
        {
            const string db_version = "2";
            Sql.dbName = getDbasePathName();
            // read database version and handle in the future
            List<string> items = Sql.ReadGenericTable("VersionTable", 0, new List<string>() { "DBVersion", "Spare1", "Spare2" });
            if (items != null && items.Count() == 3)
            {
                string ver = items[0];
                if (ver == "0")
                {
                    // we need to upgrade the database with more spare fields for Orders
                    //Sql.AddNewTableCol("OrderTable", "Spare6");
                    //Sql.AddNewTableCol("OrderTable", "Spare7");
                    //Sql.AddNewTableCol("OrderTable", "Spare8");
                    //Sql.AddNewTableCol("OrderTable", "Spare9");
                    //Sql.AddNewTableCol("OrderTable", "Spare10");
                    //Sql.AddNewTableCol("OrderTable", "Spare11");
                    //Sql.AddNewTableCol("OrderTable", "Spare12");
                    //Sql.AddNewTableCol("OrderTable", "Spare13");
                    //Sql.AddNewTableCol("OrderTable", "Spare14");
                    //Sql.AddNewTableCol("OrderTable", "Spare15");

                    //// finialize and bring version table to next version
                    //Sql.RemoveVersionTable();
                    //Sql.AppendNewTableWithDefaultRow(db_version);
                }
                else if (ver != db_version)
                {
                    MessageBox.Show(this, "Database version different: " + ver);
                }
            }
            else
            {
                Sql.AppendNewTableWithDefaultRow(db_version);
            }



            this._currentRowQuotes = Sql.GetRowCounts(dbType.Quote);
            this._currentRowOrders = Sql.GetRowCounts(dbType.Order);

            loadGUIforViewing(dbType.Quote);
            loadGUIforViewing(dbType.Order);

            refreshLetterControl(Sql.GetRowCounts(dbType.Order));
        }

        #endregion

        #region Form Closing Event

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            save(dbType.Order);
            save(dbType.Quote);
            Sql.Killconnection();
        }

        #endregion


        #region Refresh Record Indicator(s)

        private int refreshRecordIndicator(dbType t)
        {
            if (t == dbType.Order)
            {
                int count = Sql.GetRowCounts(dbType.Order);

                if (count == 0)
                {
                    this._currentRowOrders = 0;
                }
                else if (this._currentRowOrders == 0)
                {
                    this._currentRowOrders = 1;
                }

                this.textBoxRecordNo.Text = this._currentRowOrders.ToString();
                this.textBoxRecordOf.Text = "of " + count;

                refreshLetterControl(count);

                _log.append("refreshRecordIndicatorOrders end");

                return count;
            }
            else
            {
                int count = Sql.GetRowCounts(dbType.Quote);

                if (count == 0)
                {
                    this._currentRowQuotes = 0;
                }
                else if (this._currentRowQuotes == 0)
                {
                    this._currentRowQuotes = 1;
                }

                this.textBoxRecordNo.Text = this._currentRowQuotes.ToString();
                this.textBoxRecordOf.Text = "of " + count;

                return count;
            }
        }

        #endregion

        #region Letter Control - Quick Jump

        private void refreshLetterControl(int count)
        {
            string currentLetter = this.comboBoxLetterControl.Text;

            this._isLetterControlEnabled = false;
            this.comboBoxLetterControl.Items.Clear();

            if (this.textBoxPO.Text.Length == 0)
            {
                this._isLetterControlEnabled = true;
                return;
            }
            
            //List<string> POs = getPOsFromOrders();
            List<SortableRow> POs = buildSortedRows();

            // load the first record & load GUI for Orders
            //readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
            
            //POs.Sort();

            this._letterRows.Clear();
            List<string> items = new List<string>();
            items.Add("");
            this._letterRows.Add(0);
            //items.Add("<>");
            //this._letterRows.Add(this._currentRowOrders);

            string po = this.textBoxPO.Text.Substring(0, 8);
            int row = 1;
            foreach (SortableRow PO in POs)
            {
                if (PO.PO.StartsWith(po))
                {
                    if (PO.PO.Length > 8)
                    {
                        items.Add(PO.PO.Substring(8, 1));
                        this._letterRows.Add(row);
                    }
                    else
                    {
                        items.Add("<>");
                        this._letterRows.Add(row);
                    }
                }
                row++;
            }

            this.comboBoxLetterControl.Items.AddRange(items.ToArray());
            this.comboBoxLetterControl.SelectedIndex = 0; // default to this after moving around

            this._isLetterControlEnabled = true;
        }

        private void comboBoxLetterControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._isLetterControlEnabled)
            {
                // user is using the quick goto feature!
                if (this.comboBoxLetterControl.SelectedIndex > 0)
                {
                    int gotoThis = this._letterRows[this.comboBoxLetterControl.SelectedIndex];

                    if (this._currentRowOrders != gotoThis)
                    {
                        int saved = this._currentRowOrders;
                        try
                        {
                            int value = gotoThis;

                            int count = Sql.GetRowCounts(dbType.Order);

                            if (count == 0 || value == 0)
                                return;
                            else if (value > count)
                                return;

                            if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                                updateRowOrders();

                            this._currentRowOrders = value;

                            loadGUIforViewing(dbType.Order);
                        }
                        catch
                        {
                            this.textBoxRecordNo.Text = saved.ToString();
                            this._currentRowOrders = saved;
                        }
                    }
                }
            }
        }

        #endregion

        #region Generate Next PO Helpers

        private string generateNextOrderPO()
        {
            _log.append("generateNextOrderPO start");

            List<string> POs = Sql.GetPOs(dbType.Order);

            if (POs.Count == 0)
            {
                _log.append("**" + _POOrderFirst + "-" + generatePOYearEnd(""));
                return _POOrderFirst + "-" + generatePOYearEnd("");
            }
            else
            {
                // make sure the list is ordered
                POs.Sort();

                string last = POs[POs.Count() - 1];
                string[] no = last.Split('-');

                int number = Convert.ToInt32(no[0]);

                number++;

                _log.append("**" + number + "-" + generatePOYearEnd(""));
                return number + "-" + generatePOYearEnd("");
            }
        }

        private string generatePOYearEnd(string endLetter)
        {
            int yy = DateTime.Now.Year - 2000; // not worried about year 2100
            return yy.ToString() + endLetter.ToUpper();
        }

        private string generateNextQuotePO()
        {
            List<string> QPOs = Sql.GetPOs(dbType.Quote);

            if (QPOs.Count == 0)
            {
                return _QuoteNoFirst.ToString();
            }
            else
            {
                // make sure the list is ordered
                QPOs.Sort();

                string last = QPOs[QPOs.Count() - 1];

                int number = Convert.ToInt32(last);

                number++;
                return number.ToString();
            }
        }

        #endregion

        #region Other Button Events (Record control, etc.)

        private void buttonRecordTop_Click(object sender, EventArgs e)
        {
            if (this._isOrdersSelected)
            {
                int count = Sql.GetRowCounts(dbType.Order);

                if (count == 0)
                    return;

                if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                    updateRowOrders();

                this._currentRowOrders = 1;

                loadGUIforViewing(dbType.Order);
            }
            else
            {
                int count = Sql.GetRowCounts(dbType.Quote);

                if (count == 0)
                    return;

                if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                    updateRowQuotes();

                this._currentRowQuotes = 1;

                loadGUIforViewing(dbType.Quote);
            }
        }

        private void buttonRecordGo1Less_Click(object sender, EventArgs e)
        {
            if (this._isOrdersSelected)
            {
                int count = Sql.GetRowCounts(dbType.Order);

                if (count == 0)
                    return;
                else if (this._currentRowOrders <= 1)
                    return;

                if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                    updateRowOrders();

                this._currentRowOrders--;

                loadGUIforViewing(dbType.Order);
            }
            else
            {
                int count = Sql.GetRowCounts(dbType.Quote);

                if (count == 0)
                    return;
                else if (this._currentRowQuotes <= 1)
                    return;

                if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                    updateRowQuotes();

                this._currentRowQuotes--;

                loadGUIforViewing(dbType.Quote);
            }
        }

        private void buttonRecordGo1More_Click(object sender, EventArgs e)
        {
            if (this._isOrdersSelected)
            {
                int count = Sql.GetRowCounts(dbType.Order);

                if (count == 0)
                    return;
                else if (this._currentRowOrders >= count)
                    return;

                if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                    updateRowOrders();

                this._currentRowOrders++;

                loadGUIforViewing(dbType.Order);
            }
            else
            {
                int count = Sql.GetRowCounts(dbType.Quote);

                if (count == 0)
                    return;
                else if (this._currentRowQuotes >= count)
                    return;

                if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                    updateRowQuotes();

                this._currentRowQuotes++;

                loadGUIforViewing(dbType.Quote);
            }
        }

        private void buttonRecordBottom_Click(object sender, EventArgs e)
        {
            if (this._isOrdersSelected)
            {
                int count = Sql.GetRowCounts(dbType.Order);

                if (count == 0)
                    return;

                if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                    updateRowOrders();

                this._currentRowOrders = count;

                loadGUIforViewing(dbType.Order);
            }
            else
            {
                int count = Sql.GetRowCounts(dbType.Quote);

                if (count == 0)
                    return;

                if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                    updateRowQuotes();

                this._currentRowQuotes = count;

                loadGUIforViewing(dbType.Quote);
            }
        }

        private void buttonSelectCompany_Click(object sender, EventArgs e)
        {
            FormCompany fc = new FormCompany(this, getDbasePathName());

            fc.ShowDialog(this);

            if (fc.IsSelected)
            {
                Button b = (Button)sender;

                if ((string)b.Tag == "0")
                {
                    this.textBoxSoldTo.Text = fc._company;
                    this.textBoxSoldTo.Text = fc._company; // prevent reload from clearing this
                    this.textBoxStreet1.Text = fc._street1;
                    this.textBoxStreet2.Text = fc._street2;
                    this.textBoxCity.Text = fc._city;
                    this.comboBoxSoldToState.Text = fc._state;
                    this.textBoxZip.Text = fc._zip;
                }
                else
                {
                    this.textBoxShipTo.Text = fc._company;
                    this.textBoxShipTo.Text = fc._company; // prevent reload from clearing this
                    this.textBoxShipToStreet1.Text = fc._street1;
                    this.textBoxShipToStreet2.Text = fc._street2;
                    this.textBoxShipToCity.Text = fc._city;
                    this.comboBoxShipToState.Text = fc._state;
                    this.textBoxShipToZip.Text = fc._zip;
                }
            }
        }

        private void buttonSelectQCompany_Click(object sender, EventArgs e)
        {
            FormCompany fc = new FormCompany(this, getDbasePathName());

            fc.ShowDialog(this);

            if (fc.IsSelected)
            {
                this.textBoxQCompany.Text = fc._company;
                this.textBoxQCompany.Text = fc._company; // prevent reload from clearing this
                this.textBoxQStreet1.Text = fc._street1;
                this.textBoxQStreet2.Text = fc._street2;
                this.textBoxQCity.Text = fc._city;
                this.comboBoxQState.Text = fc._state;
                this.textBoxQZip.Text = fc._zip;
            }
        }
      
        private void buttonLine1_Click(object sender, EventArgs e)
        {
            FormParts fp = new FormParts(this, getDbasePathName());

            fp.ShowDialog(this);

            if (fp.IsSelected)
            {
                Button b = (Button)sender;
                int line = Convert.ToInt32(b.Tag);

                switch (line)
                {
                    default: break;
                    case 1: this.textBoxOrderDescr1.Text = fp._description; this.textBoxOrderDescr1.Text = fp._description; this.numericUpDownOrderCost1.Text = fp._price; break;
                    case 2: this.textBoxOrderDescr2.Text = fp._description; this.textBoxOrderDescr2.Text = fp._description; this.numericUpDownOrderCost2.Text = fp._price; break;
                    case 3: this.textBoxOrderDescr3.Text = fp._description; this.textBoxOrderDescr3.Text = fp._description; this.numericUpDownOrderCost3.Text = fp._price; break;
                    case 4: this.textBoxOrderDescr4.Text = fp._description; this.textBoxOrderDescr4.Text = fp._description; this.numericUpDownOrderCost4.Text = fp._price; break;
                    case 5: this.textBoxOrderDescr5.Text = fp._description; this.textBoxOrderDescr5.Text = fp._description; this.numericUpDownOrderCost5.Text = fp._price; break;
                    case 6: this.textBoxOrderDescr6.Text = fp._description; this.textBoxOrderDescr6.Text = fp._description; this.numericUpDownOrderCost6.Text = fp._price; break;
                    case 7: this.textBoxOrderDescr7.Text = fp._description; this.textBoxOrderDescr7.Text = fp._description; this.numericUpDownOrderCost7.Text = fp._price; break;
                    case 8: this.textBoxOrderDescr8.Text = fp._description; this.textBoxOrderDescr8.Text = fp._description; this.numericUpDownOrderCost8.Text = fp._price; break;
                    case 9: this.textBoxOrderDescr9.Text = fp._description; this.textBoxOrderDescr9.Text = fp._description; this.numericUpDownOrderCost9.Text = fp._price; break;
                    case 10: this.textBoxOrderDescr10.Text = fp._description; this.textBoxOrderDescr10.Text = fp._description; this.numericUpDownOrderCost10.Text = fp._price; break;
                    case 11: this.textBoxOrderDescr11.Text = fp._description; this.textBoxOrderDescr11.Text = fp._description; this.numericUpDownOrderCost11.Text = fp._price; break;
                    case 12: this.textBoxOrderDescr12.Text = fp._description; this.textBoxOrderDescr12.Text = fp._description; this.numericUpDownOrderCost12.Text = fp._price; break;
                    case 13: this.textBoxOrderDescr13.Text = fp._description; this.textBoxOrderDescr13.Text = fp._description; this.numericUpDownOrderCost13.Text = fp._price; break;
                    case 14: this.textBoxOrderDescr14.Text = fp._description; this.textBoxOrderDescr14.Text = fp._description; this.numericUpDownOrderCost14.Text = fp._price; break;
                    case 15: this.textBoxOrderDescr15.Text = fp._description; this.textBoxOrderDescr15.Text = fp._description; this.numericUpDownOrderCost15.Text = fp._price; break;
                    case 16: this.textBoxOrderDescr16.Text = fp._description; this.textBoxOrderDescr16.Text = fp._description; this.numericUpDownOrderCost16.Text = fp._price; break;
                    case 17: this.textBoxOrderDescr17.Text = fp._description; this.textBoxOrderDescr17.Text = fp._description; this.numericUpDownOrderCost17.Text = fp._price; break;
                    case 18: this.textBoxOrderDescr18.Text = fp._description; this.textBoxOrderDescr18.Text = fp._description; this.numericUpDownOrderCost18.Text = fp._price; break;
                    case 19: this.textBoxOrderDescr19.Text = fp._description; this.textBoxOrderDescr19.Text = fp._description; this.numericUpDownOrderCost19.Text = fp._price; break;
                    case 20: this.textBoxOrderDescr20.Text = fp._description; this.textBoxOrderDescr20.Text = fp._description; this.numericUpDownOrderCost20.Text = fp._price; break;
                    case 21: this.textBoxOrderDescr21.Text = fp._description; this.textBoxOrderDescr21.Text = fp._description; this.numericUpDownOrderCost21.Text = fp._price; break;
                    case 22: this.textBoxOrderDescr22.Text = fp._description; this.textBoxOrderDescr22.Text = fp._description; this.numericUpDownOrderCost22.Text = fp._price; break;
                    case 23: this.textBoxOrderDescr23.Text = fp._description; this.textBoxOrderDescr23.Text = fp._description; this.numericUpDownOrderCost23.Text = fp._price; break;
                }
            }
        }

        private void buttonQLine1_Click(object sender, EventArgs e)
        {
            FormParts fp = new FormParts(this, getDbasePathName());

            fp.ShowDialog(this);

            if (fp.IsSelected)
            {
                Button b = (Button)sender;
                int line = Convert.ToInt32(b.Tag);

                switch (line)
                {
                    default: break;
                    case 1: this.textBoxQDescription1.Text = fp._description; this.textBoxQDescription1.Text = fp._description; this.numericUpDownQCost1.Text = fp._price; break;
                    case 2: this.textBoxQDescription2.Text = fp._description; this.textBoxQDescription2.Text = fp._description; this.numericUpDownQCost2.Text = fp._price; break;
                    case 3: this.textBoxQDescription3.Text = fp._description; this.textBoxQDescription3.Text = fp._description; this.numericUpDownQCost3.Text = fp._price; break;
                    case 4: this.textBoxQDescription4.Text = fp._description; this.textBoxQDescription4.Text = fp._description; this.numericUpDownQCost4.Text = fp._price; break;
                    case 5: this.textBoxQDescription5.Text = fp._description; this.textBoxQDescription5.Text = fp._description; this.numericUpDownQCost5.Text = fp._price; break;
                    case 6: this.textBoxQDescription6.Text = fp._description; this.textBoxQDescription6.Text = fp._description; this.numericUpDownQCost6.Text = fp._price; break;
                    case 7: this.textBoxQDescription7.Text = fp._description; this.textBoxQDescription7.Text = fp._description; this.numericUpDownQCost7.Text = fp._price; break;
                    case 8: this.textBoxQDescription8.Text = fp._description; this.textBoxQDescription8.Text = fp._description; this.numericUpDownQCost8.Text = fp._price; break;
                    case 9: this.textBoxQDescription9.Text = fp._description; this.textBoxQDescription9.Text = fp._description; this.numericUpDownQCost9.Text = fp._price; break;
                    case 10: this.textBoxQDescription10.Text = fp._description; this.textBoxQDescription10.Text = fp._description; this.numericUpDownQCost10.Text = fp._price; break;
                    case 11: this.textBoxQDescription11.Text = fp._description; this.textBoxQDescription11.Text = fp._description; this.numericUpDownQCost11.Text = fp._price; break;
                    case 12: this.textBoxQDescription12.Text = fp._description; this.textBoxQDescription12.Text = fp._description; this.numericUpDownQCost12.Text = fp._price; break;
                    case 13: this.textBoxQDescription13.Text = fp._description; this.textBoxQDescription13.Text = fp._description; this.numericUpDownQCost13.Text = fp._price; break;
                    case 14: this.textBoxQDescription14.Text = fp._description; this.textBoxQDescription14.Text = fp._description; this.numericUpDownQCost14.Text = fp._price; break;
                    case 15: this.textBoxQDescription15.Text = fp._description; this.textBoxQDescription15.Text = fp._description; this.numericUpDownQCost15.Text = fp._price; break;
                    case 16: this.textBoxQDescription16.Text = fp._description; this.textBoxQDescription16.Text = fp._description; this.numericUpDownQCost16.Text = fp._price; break;
                    case 17: this.textBoxQDescription17.Text = fp._description; this.textBoxQDescription17.Text = fp._description; this.numericUpDownQCost17.Text = fp._price; break;
                    case 18: this.textBoxQDescription18.Text = fp._description; this.textBoxQDescription18.Text = fp._description; this.numericUpDownQCost18.Text = fp._price; break;
                    case 19: this.textBoxQDescription19.Text = fp._description; this.textBoxQDescription19.Text = fp._description; this.numericUpDownQCost19.Text = fp._price; break;
                    case 20: this.textBoxQDescription20.Text = fp._description; this.textBoxQDescription20.Text = fp._description; this.numericUpDownQCost20.Text = fp._price; break;
                    case 21: this.textBoxQDescription21.Text = fp._description; this.textBoxQDescription21.Text = fp._description; this.numericUpDownQCost21.Text = fp._price; break;
                    case 22: this.textBoxQDescription22.Text = fp._description; this.textBoxQDescription22.Text = fp._description; this.numericUpDownQCost22.Text = fp._price; break;
                    case 23: this.textBoxQDescription23.Text = fp._description; this.textBoxQDescription23.Text = fp._description; this.numericUpDownQCost23.Text = fp._price; break;
                }
            }
        }

        private void buttonOrdersGeneralCopy_Click(object sender, EventArgs e)
        {
            this.textBoxShipTo.Text = this.textBoxSoldTo.Text;
            this.textBoxShipTo.Text = this.textBoxSoldTo.Text; // prevent reload from cleaning this
            this.textBoxShipToStreet1.Text = this.textBoxStreet1.Text;
            this.textBoxShipToStreet2.Text = this.textBoxStreet2.Text;
            this.textBoxShipToCity.Text = this.textBoxCity.Text;
            this.comboBoxShipToState.Text = this.comboBoxSoldToState.Text;
            this.textBoxShipToZip.Text = this.textBoxZip.Text;
            //MessageBox.Show(this, "Copy Completed");
        }

        #endregion

        #region Hide Worksheet for Walk-ins

        private void checkBoxHide_CheckedChanged(object sender, EventArgs e)
        {
            this.numericUpDownGrossProfit.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownProfit.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownTotalCost.Visible = !this.checkBoxHide.Checked;

            this.numericUpDownOrderCost1.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost1.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost2.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost2.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost3.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost3.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost4.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost4.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost5.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost5.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost6.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost6.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost7.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost7.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost8.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost8.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost9.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost9.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost10.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost10.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost11.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost11.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost12.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost12.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost13.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost13.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost14.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost14.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost15.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost15.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost16.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost16.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost17.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost17.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost18.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost18.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost19.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost19.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost20.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost20.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost21.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost21.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost22.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost22.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderCost23.Visible = !this.checkBoxHide.Checked;
            this.numericUpDownOrderECost23.Visible = !this.checkBoxHide.Checked;
        }

        #endregion


        #region Auto Date formatting textbox

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

        #endregion

        #region Auto numeric tab selection ALL
        
        private void NumericUpDown_Enter(object sender, EventArgs e)
        {
            NumericUpDown num = (NumericUpDown)sender;
            num.Select(0, num.Text.Length);
        }

        #endregion

        #region Auto enter into tab for text boxes -- and auto re-load in case data is stale

        private string _data = "";
        private void textBoxEnter2Tab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SelectNextControl(ActiveControl, true, true, true, true);
                return; // don't make dirty below
            }
            else if (e.KeyCode==Keys.C && e.Control)
            {
                try
                {
                    TextBox box = (TextBox)sender;
                    //box.Paste();
                    //MessageBox.Show("pasted");
                    //string data = Clipboard.GetText();
                    _data = box.SelectedText;
                    //MessageBox.Show("pasted'" + data + "'");
                }
                catch { } 
            }
            else if (e.KeyCode==Keys.V && e.Control)
            {
                e.SuppressKeyPress = true;
                try
                {
                    TextBox box = (TextBox)sender;
                    //box.Paste();
                    //MessageBox.Show("pasted");
                    string data = _data;// Clipboard.GetText();
                    box.SelectedText = data;
                    //MessageBox.Show("pasted'" + data + "'");
                }
                catch { }
            }

            if (_isOrdersSelected)
            {
                // if order was clean, reload to be safe
                if (!this.isDataDirtyOrders)
                {
                    loadGUIAndLock(dbType.Order);
                }
            }
            else
            {
                // if quote was clean, reload to be safe
                if (!this.isDataDirtyQuotes)
                {
                    loadGUIAndLock(dbType.Quote);
                }
            }
        }

        // note: this is only for RichTextBox's only, enter does crazy stuff at times
        private void textBoxEnter3Tab_KeyDown(object sender, KeyEventArgs e)
        {
            bool was_loaded = false;

            RichTextBox rtb = (RichTextBox)sender;
            int start = rtb.SelectionStart;

            if (_isOrdersSelected)
            {
                // if order was clean, reload to be safe
                if (!this.isDataDirtyOrders)
                {
                    was_loaded = loadGUIAndLock(dbType.Order);
                }
            }
            else
            {
                // if quote was clean, reload to be safe
                if (!this.isDataDirtyQuotes)
                {
                    was_loaded = loadGUIAndLock(dbType.Quote);
                }
            }

            if (was_loaded)
            {
                rtb.SelectionStart = start;
            }
        }


        #endregion

        #region Auto trim end for textboxes (special equipment logic)

        private void textBox_Leave(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;
            box.Text = box.Text.TrimEnd();
        }

        private void textBoxEquipment_Leave(object sender, EventArgs e)
        {
            // repeat above work then
            TextBox box = (TextBox)sender;
            box.Text = box.Text.TrimEnd();

            // if description is blank, copy whatever is here
            if (this.textBoxDescription.Text == "")
                this.textBoxDescription.Text = box.Text;
        }

        #endregion


        #region Change Events Orders

        private void textBoxDate_TextChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            dbaseControlChangedEvent();
        }

        private bool killOrderComboEvent = false;
        private void comboBoxSalesAss_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            if (killOrderComboEvent)
                return;
            if (onetime_killOrderComboEvent)
            {
                onetime_killOrderComboEvent = false;
                return;
            }

            // if this is the first combo box change save index, and put back
            int index = ((ComboBox)sender).SelectedIndex;
            killOrderComboEvent = true;

            if (!this.isDataDirtyOrders)
            {
                dbaseControlChangedEvent();
                ((ComboBox)sender).SelectedIndex = index;
            }
            else
            {
                dbaseControlChangedEvent();
            }

            killOrderComboEvent = false;
        }

        private void numericUpDownComAmount_ValueChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            dbaseControlChangedEvent();
        }

        private bool killOrderCheckEvent = false;
        private void checkBoxComOrder_CheckedChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            if (killOrderCheckEvent)
                return;

            // if this is the first combo box change save index, and put back
            bool state = ((CheckBox)sender).Checked;
            killOrderCheckEvent = true;

            if (!this.isDataDirtyOrders)
            {
                dbaseControlChangedEvent();
                ((CheckBox)sender).Checked = state;
            }
            else
            {
                dbaseControlChangedEvent();
            }

            killOrderCheckEvent = false;
        }

        private void dbaseControlChangedEvent()
        {
            this._isAction = true;
            //_log.append("dbaseControlChangedEvent start");

            // any control actually runs here when changed
            if (!isDataLoadingOrders)
            {
                // user must be making changes to the order controls!
                if (LowLevelLockChecking(dbType.Order))
                    return;

                // just in case!
                this.textBoxSoldToReadOnly.Text = this.textBoxSoldTo.Text;

                if (!this.isDataDirtyOrders)
                {
                    loadGUIAndLock(dbType.Order);
                }
            }

            updateGUIStatusBar();

            doTheMathOrders();

            //_log.append("dbaseControlChangedEvent end");
        }

        #endregion

        #region Change Events Quotes

        private void textBoxQuote_TextChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            dbaseControlChangedEvent2();
        }

        private bool killQuoteComboEvent = false;
        private void comboBoxQuote_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            if (killQuoteComboEvent)
                return;
            if (onetime_killQuoteComboEvent)
            {
                onetime_killQuoteComboEvent = false;
                return;
            }

            // if this is the first combo box change save index, and put back
            int index = ((ComboBox)sender).SelectedIndex;
            killQuoteComboEvent = true;

            if (!this.isDataDirtyOrders)
            {
                dbaseControlChangedEvent2();
                ((ComboBox)sender).SelectedIndex = index;
            }
            else
            {
                dbaseControlChangedEvent2();
            }

            killQuoteComboEvent = false;
        }

        private void numericUpDownQuote_ValueChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            dbaseControlChangedEvent2();
        }

        private bool killQuoterCheckEvent = false;
        private void checkBoxQuote_CheckedChanged(object sender, EventArgs e)
        {
            this._isAction = true;
            if (killQuoterCheckEvent)
                return;

            // if this is the first combo box change save index, and put back
            bool state = ((CheckBox)sender).Checked;
            killQuoterCheckEvent = true;

            if (!this.isDataDirtyOrders)
            {
                dbaseControlChangedEvent2();
                ((CheckBox)sender).Checked = state;
            }
            else
            {
                dbaseControlChangedEvent2();
            }

            killQuoterCheckEvent = false;
        }

        private void dbaseControlChangedEvent2()
        {
            this._isAction = true;
            // any control actually runs here when changed
            if (!isDataLoadingQuotes)
            {
                // user must be making changes to the quote controls!
                if (LowLevelLockChecking(dbType.Quote))
                    return;

                // just in case!
                this.textBoxQCompanyReadOnly.Text = this.textBoxQCompany.Text;
                this.textBoxQQuotedPriceReadOnly.Text = this.numericUpDownQQuotePrice.Text;

                if (!this.isDataDirtyQuotes)
                {
                    loadGUIAndLock(dbType.Quote);
                }
            }

            updateGUIStatusBar();

            // quote price manual mode
            this.numericUpDownQQuotePrice.ReadOnly = !this.checkBoxQManual.Checked;
            // pricing's manual mode
            this.numericUpDownQMCost1.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost2.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost3.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost4.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost5.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost6.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost7.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost8.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost9.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost10.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost11.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost12.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost13.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost14.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost15.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost16.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost17.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost18.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost19.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost20.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost21.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost22.ReadOnly = !this.checkBoxPManual.Checked;
            this.numericUpDownQMCost23.ReadOnly = !this.checkBoxPManual.Checked;

            doTheMathQuotes();
        }

        #endregion


        #region Calendar Picker Double Click Event

        private void textBoxDate_DoubleClick(object sender, EventArgs e)
        {
            this._isAction = true;
            TextBox box = (TextBox)sender;

            FormDatePicker dp = new FormDatePicker(box.Text);
            dp.ShowDialog(this);

            if (dp.IsConfirmed)
            {
                box.Text = dp.Date;
                box.Text = dp.Date; // prevent reload from clearing this action
            }
        }

        #endregion

        #region Record # Key Press Event

        private void textBoxRecordNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (this._isOrdersSelected)
                {
                    int saved = this._currentRowOrders;
                    try
                    {
                        int value = Convert.ToInt32(this.textBoxRecordNo.Text);

                        int count = Sql.GetRowCounts(dbType.Order);

                        if (count == 0 || value == 0)
                            return;
                        else if (value > count)
                            return;

                        if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                            updateRowOrders();

                        this._currentRowOrders = value;

                        loadGUIforViewing(dbType.Order);
                    }
                    catch
                    {
                        this.textBoxRecordNo.Text = saved.ToString();
                        this._currentRowOrders = saved;
                    }
                }
                else
                {
                    int saved = this._currentRowQuotes;
                    try
                    {
                        int value = Convert.ToInt32(this.textBoxRecordNo.Text);

                        int count = Sql.GetRowCounts(dbType.Quote);

                        if (count == 0 || value == 0)
                            return;
                        else if (value > count)
                            return;

                        if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                            updateRowQuotes();

                        this._currentRowQuotes = value;

                        loadGUIforViewing(dbType.Quote);
                    }
                    catch
                    {
                        this.textBoxRecordNo.Text = saved.ToString();
                        this._currentRowQuotes = saved;
                    }
                }

                return;
            }

            // all digits and backspace
            string digs = "0123456789\b";
            if (digs.Contains(e.KeyChar))
            {

            }
            else
            {
                e.Handled = true;
            }
        }

        #endregion


        #region Do the Math for Order Commish & Worksheet GUI

        private void doTheMathOrders()
        {
            try
            {
                // Commission calculation
                this.numericUpDownComBalance.Value = this.numericUpDownComAmount.Value - (this.numericUpDownPaid1.Value + this.numericUpDownPaid2.Value + this.numericUpDownPaid3.Value + this.numericUpDownPaid4.Value + this.numericUpDownPaid5.Value);

                // Order worksheet math
                decimal w1 = this.numericUpDownOrderECost1.Value = this.numericUpDownOrderCount1.Value * this.numericUpDownOrderCost1.Value;
                decimal w2 = this.numericUpDownOrderECost2.Value = this.numericUpDownOrderCount2.Value * this.numericUpDownOrderCost2.Value;
                decimal w3 = this.numericUpDownOrderECost3.Value = this.numericUpDownOrderCount3.Value * this.numericUpDownOrderCost3.Value;
                decimal w4 = this.numericUpDownOrderECost4.Value = this.numericUpDownOrderCount4.Value * this.numericUpDownOrderCost4.Value;
                decimal w5 = this.numericUpDownOrderECost5.Value = this.numericUpDownOrderCount5.Value * this.numericUpDownOrderCost5.Value;
                decimal w6 = this.numericUpDownOrderECost6.Value = this.numericUpDownOrderCount6.Value * this.numericUpDownOrderCost6.Value;
                decimal w7 = this.numericUpDownOrderECost7.Value = this.numericUpDownOrderCount7.Value * this.numericUpDownOrderCost7.Value;
                decimal w8 = this.numericUpDownOrderECost8.Value = this.numericUpDownOrderCount8.Value * this.numericUpDownOrderCost8.Value;
                decimal w9 = this.numericUpDownOrderECost9.Value = this.numericUpDownOrderCount9.Value * this.numericUpDownOrderCost9.Value;
                decimal w10 = this.numericUpDownOrderECost10.Value = this.numericUpDownOrderCount10.Value * this.numericUpDownOrderCost10.Value;
                decimal w11 = this.numericUpDownOrderECost11.Value = this.numericUpDownOrderCount11.Value * this.numericUpDownOrderCost11.Value;
                decimal w12 = this.numericUpDownOrderECost12.Value = this.numericUpDownOrderCount12.Value * this.numericUpDownOrderCost12.Value;
                decimal w13 = this.numericUpDownOrderECost13.Value = this.numericUpDownOrderCount13.Value * this.numericUpDownOrderCost13.Value;
                decimal w14 = this.numericUpDownOrderECost14.Value = this.numericUpDownOrderCount14.Value * this.numericUpDownOrderCost14.Value;
                decimal w15 = this.numericUpDownOrderECost15.Value = this.numericUpDownOrderCount15.Value * this.numericUpDownOrderCost15.Value;
                decimal w16 = this.numericUpDownOrderECost16.Value = this.numericUpDownOrderCount16.Value * this.numericUpDownOrderCost16.Value;
                decimal w17 = this.numericUpDownOrderECost17.Value = this.numericUpDownOrderCount17.Value * this.numericUpDownOrderCost17.Value;
                decimal w18 = this.numericUpDownOrderECost18.Value = this.numericUpDownOrderCount18.Value * this.numericUpDownOrderCost18.Value;
                decimal w19 = this.numericUpDownOrderECost19.Value = this.numericUpDownOrderCount19.Value * this.numericUpDownOrderCost19.Value;
                decimal w20 = this.numericUpDownOrderECost20.Value = this.numericUpDownOrderCount20.Value * this.numericUpDownOrderCost20.Value;
                decimal w21 = this.numericUpDownOrderECost21.Value = this.numericUpDownOrderCount21.Value * this.numericUpDownOrderCost21.Value;
                decimal w22 = this.numericUpDownOrderECost22.Value = this.numericUpDownOrderCount22.Value * this.numericUpDownOrderCost22.Value;
                decimal w23 = this.numericUpDownOrderECost23.Value = this.numericUpDownOrderCount23.Value * this.numericUpDownOrderCost23.Value;

                this.numericUpDownTotalCost.Value = (w1 + w2 + w3 + w4 + w5 + w6 + w7 + w8 + w9 + w10 + w11 + w12 + w13 + w14 + w15 + w16 + w17 + w18 + w19 + w20 + w21 + w22 + w23);
                this.numericUpDownProfit.Value = this.numericUpDownQuotePrice.Value - this.numericUpDownTotalCost.Value - this.numericUpDownCredit.Value - this.numericUpDownFreight.Value - this.numericUpDownShopTime.Value;
                this.numericUpDownGrossProfit.Value = (this.numericUpDownQuotePrice.Value == 0) ? 0 :
                    (100 * (this.numericUpDownQuotePrice.Value - this.numericUpDownTotalCost.Value - this.numericUpDownCredit.Value - this.numericUpDownFreight.Value - this.numericUpDownShopTime.Value))
                    / this.numericUpDownQuotePrice.Value;
            }
            catch { MessageBox.Show("Order Numeric out of range"); }
        }

        #endregion

        #region Do the Math for Quote Worksheet GUI

        private bool _running = false;
        private void doTheMathQuotes()
        {
            if (_running)
                return;
            _running = true;

            try
            {
                // Quote worksheet math
                decimal w1 = this.numericUpDownQECost1.Value = this.numericUpDownQQuan1.Value * this.numericUpDownQCost1.Value;
                decimal w2 = this.numericUpDownQECost2.Value = this.numericUpDownQQuan2.Value * this.numericUpDownQCost2.Value;
                decimal w3 = this.numericUpDownQECost3.Value = this.numericUpDownQQuan3.Value * this.numericUpDownQCost3.Value;
                decimal w4 = this.numericUpDownQECost4.Value = this.numericUpDownQQuan4.Value * this.numericUpDownQCost4.Value;
                decimal w5 = this.numericUpDownQECost5.Value = this.numericUpDownQQuan5.Value * this.numericUpDownQCost5.Value;
                decimal w6 = this.numericUpDownQECost6.Value = this.numericUpDownQQuan6.Value * this.numericUpDownQCost6.Value;
                decimal w7 = this.numericUpDownQECost7.Value = this.numericUpDownQQuan7.Value * this.numericUpDownQCost7.Value;
                decimal w8 = this.numericUpDownQECost8.Value = this.numericUpDownQQuan8.Value * this.numericUpDownQCost8.Value;
                decimal w9 = this.numericUpDownQECost9.Value = this.numericUpDownQQuan9.Value * this.numericUpDownQCost9.Value;
                decimal w10 = this.numericUpDownQECost10.Value = this.numericUpDownQQuan10.Value * this.numericUpDownQCost10.Value;
                decimal w11 = this.numericUpDownQECost11.Value = this.numericUpDownQQuan11.Value * this.numericUpDownQCost11.Value;
                decimal w12 = this.numericUpDownQECost12.Value = this.numericUpDownQQuan12.Value * this.numericUpDownQCost12.Value;
                decimal w13 = this.numericUpDownQECost13.Value = this.numericUpDownQQuan13.Value * this.numericUpDownQCost13.Value;
                decimal w14 = this.numericUpDownQECost14.Value = this.numericUpDownQQuan14.Value * this.numericUpDownQCost14.Value;
                decimal w15 = this.numericUpDownQECost15.Value = this.numericUpDownQQuan15.Value * this.numericUpDownQCost15.Value;
                decimal w16 = this.numericUpDownQECost16.Value = this.numericUpDownQQuan16.Value * this.numericUpDownQCost16.Value;
                decimal w17 = this.numericUpDownQECost17.Value = this.numericUpDownQQuan17.Value * this.numericUpDownQCost17.Value;
                decimal w18 = this.numericUpDownQECost18.Value = this.numericUpDownQQuan18.Value * this.numericUpDownQCost18.Value;
                decimal w19 = this.numericUpDownQECost19.Value = this.numericUpDownQQuan19.Value * this.numericUpDownQCost19.Value;
                decimal w20 = this.numericUpDownQECost20.Value = this.numericUpDownQQuan20.Value * this.numericUpDownQCost20.Value;
                decimal w21 = this.numericUpDownQECost21.Value = this.numericUpDownQQuan21.Value * this.numericUpDownQCost21.Value;
                decimal w22 = this.numericUpDownQECost22.Value = this.numericUpDownQQuan22.Value * this.numericUpDownQCost22.Value;
                decimal w23 = this.numericUpDownQECost23.Value = this.numericUpDownQQuan23.Value * this.numericUpDownQCost23.Value;

                this.numericUpDownQTotalCost.Value = w1 + w2 + w3 + w4 + w5 + w6 + w7 + w8 + w9 + w10 + w11 + w12 + w13 + w14 + w15 + w16 + w17 + w18 + w19 + w20 + w21 + w22 + w23;
                //this.numericUpDownQCredit.Value + this.numericUpDownQFreight.Value + this.numericUpDownQShopTime.Value;

                // if not in manual mode, calculate it
                if (!this.checkBoxQManual.Checked)
                {
                    this.numericUpDownQQuotePrice.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQTotalCost.Value);
                    this.textBoxQQuotedPriceReadOnly.Text = this.numericUpDownQQuotePrice.Text; // mirror
                }
                //else if (this.checkBoxPManual.Checked) // sum total of Marked up costs because it (right) is manual mode
                else // complete manual mode
                {
                    this.textBoxQQuotedPriceReadOnly.Text = this.numericUpDownQQuotePrice.Text; // mirror
                }

                {
                    decimal m1 = numericUpDownQMCost1.Value;
                    decimal m2 = numericUpDownQMCost2.Value;
                    decimal m3 = numericUpDownQMCost3.Value;
                    decimal m4 = numericUpDownQMCost4.Value;
                    decimal m5 = numericUpDownQMCost5.Value;
                    decimal m6 = numericUpDownQMCost6.Value;
                    decimal m7 = numericUpDownQMCost7.Value;
                    decimal m8 = numericUpDownQMCost8.Value;
                    decimal m9 = numericUpDownQMCost9.Value;
                    decimal m10 = numericUpDownQMCost10.Value;
                    decimal m11 = numericUpDownQMCost11.Value;
                    decimal m12 = numericUpDownQMCost12.Value;
                    decimal m13 = numericUpDownQMCost13.Value;
                    decimal m14 = numericUpDownQMCost14.Value;
                    decimal m15 = numericUpDownQMCost15.Value;
                    decimal m16 = numericUpDownQMCost16.Value;
                    decimal m17 = numericUpDownQMCost17.Value;
                    decimal m18 = numericUpDownQMCost18.Value;
                    decimal m19 = numericUpDownQMCost19.Value;
                    decimal m20 = numericUpDownQMCost20.Value;
                    decimal m21 = numericUpDownQMCost21.Value;
                    decimal m22 = numericUpDownQMCost22.Value;
                    decimal m23 = numericUpDownQMCost23.Value;

                    this.numericUpDownQCheat.Value = m1 + m2 + m3 + m4 + m5 + m6 + m7 + m8 + m9 + m10 + m11 + m12 + m13 + m14 + m15 + m16 + m17 + m18 + m19 + m20 + m21 + m22 + m23;

                    if (this.numericUpDownQCheat.Value > 0)
                        this.numericUpDownQCheat.Visible = true;
                    else
                        this.numericUpDownQCheat.Visible = false;
                    
                    //this.textBoxQQuotedPriceReadOnly.Text = this.numericUpDownQQuotePrice.Text; // mirror
                }

                this.numericUpDownQProfit.Value = this.numericUpDownQQuotePrice.Value - this.numericUpDownQTotalCost.Value - this.numericUpDownQCredit.Value - this.numericUpDownQFreight.Value - this.numericUpDownQShopTime.Value;
                this.numericUpDownQGrossProfit.Value = (this.numericUpDownQQuotePrice.Value == 0) ? 0 :
                    (100 * (this.numericUpDownQQuotePrice.Value - this.numericUpDownQTotalCost.Value - this.numericUpDownQCredit.Value - this.numericUpDownQFreight.Value - this.numericUpDownQShopTime.Value))
                    / this.numericUpDownQQuotePrice.Value;

                // if not in manual mode, calculate it
                if (!this.checkBoxPManual.Checked)
                {
                    this.numericUpDownQMCost1.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost1.Value);
                    this.numericUpDownQMCost2.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost2.Value);
                    this.numericUpDownQMCost3.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost3.Value);
                    this.numericUpDownQMCost4.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost4.Value);
                    this.numericUpDownQMCost5.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost5.Value);
                    this.numericUpDownQMCost6.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost6.Value);
                    this.numericUpDownQMCost7.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost7.Value);
                    this.numericUpDownQMCost8.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost8.Value);
                    this.numericUpDownQMCost9.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost9.Value);
                    this.numericUpDownQMCost10.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost10.Value);
                    this.numericUpDownQMCost11.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost11.Value);
                    this.numericUpDownQMCost12.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost12.Value);
                    this.numericUpDownQMCost13.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost13.Value);
                    this.numericUpDownQMCost14.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost14.Value);
                    this.numericUpDownQMCost15.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost15.Value);
                    this.numericUpDownQMCost16.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost16.Value);
                    this.numericUpDownQMCost17.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost17.Value);
                    this.numericUpDownQMCost18.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost18.Value);
                    this.numericUpDownQMCost19.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost19.Value);
                    this.numericUpDownQMCost20.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost20.Value);
                    this.numericUpDownQMCost21.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost21.Value);
                    this.numericUpDownQMCost22.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost22.Value);
                    this.numericUpDownQMCost23.Value = roundIt((1 + this.numericUpDownQMarkUp.Value / 100) * this.numericUpDownQECost23.Value);
                }
            }
            catch { MessageBox.Show("Quote Numeric out of range"); }

            _running = false;
        }

        private decimal roundIt(decimal value)
        {
            double num = Math.Pow(10, 0);
            return (decimal)(Math.Ceiling((double)value * num) / num);
        }

        #endregion


        #region Menu Strip Events - saving, printing, new orders, new quotes, default GUI, etc.

        // saving
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save(_isOrdersSelected ? dbType.Order : dbType.Quote);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            save(_isOrdersSelected ? dbType.Order : dbType.Quote);
        }

        private void save(dbType t)
        {
            if (t == dbType.Order)
            {
                if (refreshRecordIndicator(dbType.Order) > 0)
                {
                    if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                        updateRowOrders();
                }
            }
            else
            {
                if (refreshRecordIndicator(dbType.Quote) > 0)
                {
                    if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                        updateRowQuotes();
                }
            }
        }

        // reports for orders only at this time
        private void futureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormReport fr = new FormReport(this, getDbasePathName(), true);

            fr.ShowDialog(this);
        }

        // goto for orders
        private void gotoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormGoto go = new FormGoto(this, getDbasePathName(), true);
            go.ShowDialog(this);

            if (go.IsSelected && go._Row >= 0)
            {
                if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                    updateRowOrders();

                List<SortableRow> sorted = buildSortedRows();
                int found = 1;
                foreach (SortableRow sr in sorted)
                {
                    if (sr.PO == go._POName)
                    {
                        break;
                    }
                    found++;
                }
                this._currentRowOrders = found;

                loadGUIforViewing(dbType.Order);
            }
        }

        // goto for quotes
        private void gotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormGoto go = new FormGoto(this, getDbasePathName(), false);
            go.ShowDialog(this);

            if (go.IsSelected && go._Row >= 0)
            {
                if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                    updateRowQuotes();

                this._currentRowQuotes = go._Row;

                loadGUIforViewing(dbType.Quote);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void purchaseOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPurchaseOrder();
        }

        private void priceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPriceSheet();
        }

        private void deliveryTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printDeliveryTicket();
        }

        private void printCurrentQuoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printCurrentQuote();
        }

        private string toCurrency(Decimal value)
        {
            return value.ToString("$ #,##0.00");
        }

        // new sales orders
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                updateRowOrders();

            string nextPO = generateNextOrderPO();

            helperCreateNewOrderRow(nextPO, false);
        }

        // new stock order T
        private void newStockOrderTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                updateRowOrders();

            string nextPO = generateNextOrderPO() + "T";

            helperCreateNewOrderRow(nextPO, false);
        }

        // new warranty order W
        private void newWarrantyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                updateRowOrders();

            string nextPO = generateNextOrderPO() + "W";

            helperCreateNewOrderRow(nextPO, false);
        }

        // new quotes
        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                updateRowQuotes();

            string nextQPO = generateNextQuotePO();

            helperCreateNewQuoteRow(nextQPO);
        }

        private void helperCreateNewOrderRow(string nextPO, bool IsCopyElements)
        {
            #region Default GUI settings

            // update the GUI with above read params
            isDataLoadingOrders = true;

            this.textBoxPO.Text = nextPO;
            this.textBoxDate.Text = todaysDate();
            if (!IsCopyElements)
                this.textBoxEndUser.Text = "";
            this.textBoxEquipment.Text = "";
            this.textBoxVendorName.Text = "";
            if (!IsCopyElements)
                this.textBoxJobName.Text = "";
            if (!IsCopyElements)
                this.textBoxCustomerPO.Text = "";
            this.textBoxVendorNumber.Text = "";
            this.comboBoxSalesAss.Text = "";

            if (!IsCopyElements)
            {
                this.textBoxSoldTo.Text = "";
                this.textBoxSoldToReadOnly.Text = ""; // mirror

                this.textBoxStreet1.Text = "";
                this.textBoxStreet2.Text = "";
                this.textBoxCity.Text = "";
                this.comboBoxSoldToState.Text = "";
                this.textBoxZip.Text = "";
            }

            this.textBoxShipTo.Text = "";
            this.textBoxShipToStreet1.Text = "";
            this.textBoxShipToStreet2.Text = "";
            this.textBoxShipToCity.Text = "";
            this.comboBoxShipToState.Text = "";
            this.textBoxShipToZip.Text = "";

            this.comboBoxCarrier.Text = "";
            this.textBoxShipDate.Text = "";

            this.checkBoxOK.Checked = false; // actually when it is ok, default ok
            this.checkBoxPONeeded.Checked = true;
            this.checkBoxComOrder.Checked = false;
            this.checkBoxComPaid.Checked = false;

            this.textBoxGrinder.Text = "";
            this.textBoxSerialNumber.Text = "";
            this.textBoxPumpStk.Text = "";

            this.textBoxReqDate.Text = "";
            this.textBoxSchedShip.Text = "";
            this.textBoxPODate.Text = "";
            this.textBoxPOShipVia.Text = "";

            this.textBoxTrkDate1.Text = ""; this.comboBoxTrkBy1.Text = ""; this.comboBoxTrkSource1.Text = ""; this.textBoxTrkNotes1.Text = "";
            this.textBoxTrkDate2.Text = ""; this.comboBoxTrkBy2.Text = ""; this.comboBoxTrkSource2.Text = ""; this.textBoxTrkNotes2.Text = "";
            this.textBoxTrkDate3.Text = ""; this.comboBoxTrkBy3.Text = ""; this.comboBoxTrkSource3.Text = ""; this.textBoxTrkNotes3.Text = "";
            this.textBoxTrkDate4.Text = ""; this.comboBoxTrkBy4.Text = ""; this.comboBoxTrkSource4.Text = ""; this.textBoxTrkNotes4.Text = "";
            this.textBoxTrkDate5.Text = ""; this.comboBoxTrkBy5.Text = ""; this.comboBoxTrkSource5.Text = ""; this.textBoxTrkNotes5.Text = "";
            this.textBoxTrkDate6.Text = ""; this.comboBoxTrkBy6.Text = ""; this.comboBoxTrkSource6.Text = ""; this.textBoxTrkNotes6.Text = "";
            this.textBoxTrkDate7.Text = ""; this.comboBoxTrkBy7.Text = ""; this.comboBoxTrkSource7.Text = ""; this.textBoxTrkNotes7.Text = "";
            this.textBoxTrkDate8.Text = ""; this.comboBoxTrkBy8.Text = ""; this.comboBoxTrkSource8.Text = ""; this.textBoxTrkNotes8.Text = "";
            this.textBoxTrkDate9.Text = ""; this.comboBoxTrkBy9.Text = ""; this.comboBoxTrkSource9.Text = ""; this.textBoxTrkNotes9.Text = "";
            this.textBoxTrkDate10.Text = ""; this.comboBoxTrkBy10.Text = ""; this.comboBoxTrkSource10.Text = ""; this.textBoxTrkNotes10.Text = "";
            this.textBoxTrkDate11.Text = ""; this.comboBoxTrkBy11.Text = ""; this.comboBoxTrkSource11.Text = ""; this.textBoxTrkNotes11.Text = "";
            this.textBoxTrkDate12.Text = ""; this.comboBoxTrkBy12.Text = ""; this.comboBoxTrkSource12.Text = ""; this.textBoxTrkNotes12.Text = "";
            this.textBoxTrkDate13.Text = ""; this.comboBoxTrkBy13.Text = ""; this.comboBoxTrkSource13.Text = ""; this.textBoxTrkNotes13.Text = "";
            this.textBoxTrkDate14.Text = ""; this.comboBoxTrkBy14.Text = ""; this.comboBoxTrkSource14.Text = ""; this.textBoxTrkNotes14.Text = "";
            this.textBoxTrkDate15.Text = ""; this.comboBoxTrkBy15.Text = ""; this.comboBoxTrkSource15.Text = ""; this.textBoxTrkNotes15.Text = "";
            this.textBoxTrkDate16.Text = ""; this.comboBoxTrkBy16.Text = ""; this.comboBoxTrkSource16.Text = ""; this.textBoxTrkNotes16.Text = "";
            this.textBoxTrkDate17.Text = ""; this.comboBoxTrkBy17.Text = ""; this.comboBoxTrkSource17.Text = ""; this.textBoxTrkNotes17.Text = "";
            this.textBoxTrkDate18.Text = ""; this.comboBoxTrkBy18.Text = ""; this.comboBoxTrkSource18.Text = ""; this.textBoxTrkNotes18.Text = "";

            this.numericUpDownQuotePrice.Text = "0.00";
            this.numericUpDownCredit.Text = "0.00";
            this.numericUpDownFreight.Text = "0.00";
            this.numericUpDownShopTime.Text = "0.00";
            this.numericUpDownTotalCost.Text = "0.00";
            this.numericUpDownGrossProfit.Text = "0.00";
            this.numericUpDownProfit.Text = "0.00";

            this.textBoxDescription.Text = "";
            this.numericUpDownOrderCount1.Text = "0.00"; this.textBoxOrderDescr1.Text = ""; this.numericUpDownOrderCost1.Text = "0.00"; this.numericUpDownOrderECost1.Text = "0.00";
            this.numericUpDownOrderCount2.Text = "0.00"; this.textBoxOrderDescr2.Text = ""; this.numericUpDownOrderCost2.Text = "0.00"; this.numericUpDownOrderECost2.Text = "0.00";
            this.numericUpDownOrderCount3.Text = "0.00"; this.textBoxOrderDescr3.Text = ""; this.numericUpDownOrderCost3.Text = "0.00"; this.numericUpDownOrderECost3.Text = "0.00";
            this.numericUpDownOrderCount4.Text = "0.00"; this.textBoxOrderDescr4.Text = ""; this.numericUpDownOrderCost4.Text = "0.00"; this.numericUpDownOrderECost4.Text = "0.00";
            this.numericUpDownOrderCount5.Text = "0.00"; this.textBoxOrderDescr5.Text = ""; this.numericUpDownOrderCost5.Text = "0.00"; this.numericUpDownOrderECost5.Text = "0.00";
            this.numericUpDownOrderCount6.Text = "0.00"; this.textBoxOrderDescr6.Text = ""; this.numericUpDownOrderCost6.Text = "0.00"; this.numericUpDownOrderECost6.Text = "0.00";
            this.numericUpDownOrderCount7.Text = "0.00"; this.textBoxOrderDescr7.Text = ""; this.numericUpDownOrderCost7.Text = "0.00"; this.numericUpDownOrderECost7.Text = "0.00";
            this.numericUpDownOrderCount8.Text = "0.00"; this.textBoxOrderDescr8.Text = ""; this.numericUpDownOrderCost8.Text = "0.00"; this.numericUpDownOrderECost8.Text = "0.00";
            this.numericUpDownOrderCount9.Text = "0.00"; this.textBoxOrderDescr9.Text = ""; this.numericUpDownOrderCost9.Text = "0.00"; this.numericUpDownOrderECost9.Text = "0.00";
            this.numericUpDownOrderCount10.Text = "0.00"; this.textBoxOrderDescr10.Text = ""; this.numericUpDownOrderCost10.Text = "0.00"; this.numericUpDownOrderECost10.Text = "0.00";
            this.numericUpDownOrderCount11.Text = "0.00"; this.textBoxOrderDescr11.Text = ""; this.numericUpDownOrderCost11.Text = "0.00"; this.numericUpDownOrderECost11.Text = "0.00";
            this.numericUpDownOrderCount12.Text = "0.00"; this.textBoxOrderDescr12.Text = ""; this.numericUpDownOrderCost12.Text = "0.00"; this.numericUpDownOrderECost12.Text = "0.00";
            this.numericUpDownOrderCount13.Text = "0.00"; this.textBoxOrderDescr13.Text = ""; this.numericUpDownOrderCost13.Text = "0.00"; this.numericUpDownOrderECost13.Text = "0.00";
            this.numericUpDownOrderCount14.Text = "0.00"; this.textBoxOrderDescr14.Text = ""; this.numericUpDownOrderCost14.Text = "0.00"; this.numericUpDownOrderECost14.Text = "0.00";
            this.numericUpDownOrderCount15.Text = "0.00"; this.textBoxOrderDescr15.Text = ""; this.numericUpDownOrderCost15.Text = "0.00"; this.numericUpDownOrderECost15.Text = "0.00";
            this.numericUpDownOrderCount16.Text = "0.00"; this.textBoxOrderDescr16.Text = ""; this.numericUpDownOrderCost16.Text = "0.00"; this.numericUpDownOrderECost16.Text = "0.00";
            this.numericUpDownOrderCount17.Text = "0.00"; this.textBoxOrderDescr17.Text = ""; this.numericUpDownOrderCost17.Text = "0.00"; this.numericUpDownOrderECost17.Text = "0.00";
            this.numericUpDownOrderCount18.Text = "0.00"; this.textBoxOrderDescr18.Text = ""; this.numericUpDownOrderCost18.Text = "0.00"; this.numericUpDownOrderECost18.Text = "0.00";
            this.numericUpDownOrderCount19.Text = "0.00"; this.textBoxOrderDescr19.Text = ""; this.numericUpDownOrderCost19.Text = "0.00"; this.numericUpDownOrderECost19.Text = "0.00";
            this.numericUpDownOrderCount20.Text = "0.00"; this.textBoxOrderDescr20.Text = ""; this.numericUpDownOrderCost20.Text = "0.00"; this.numericUpDownOrderECost20.Text = "0.00";
            this.numericUpDownOrderCount21.Text = "0.00"; this.textBoxOrderDescr21.Text = ""; this.numericUpDownOrderCost21.Text = "0.00"; this.numericUpDownOrderECost21.Text = "0.00";
            this.numericUpDownOrderCount22.Text = "0.00"; this.textBoxOrderDescr22.Text = ""; this.numericUpDownOrderCost22.Text = "0.00"; this.numericUpDownOrderECost22.Text = "0.00";
            this.numericUpDownOrderCount23.Text = "0.00"; this.textBoxOrderDescr23.Text = ""; this.numericUpDownOrderCost23.Text = "0.00"; this.numericUpDownOrderECost23.Text = "0.00";

            this.richTextBoxInvoiceInstructions.Text = "";
            this.richTextBoxInvoiceNotes.Text = "";
            this.richTextBoxVendorNotes.Text = "";
            this.richTextBoxAccNotes.Text = "";
            this.textBoxCrMemo.Text = "";
            this.textBoxInvoiceNumber.Text = "";
            this.textBoxInvoiceDate.Text = "";
            this.comboBoxStatus.Text = "";
            this.textBoxCheckNumbers.Text = "";
            this.textBoxCheckDates.Text = "";

            this.textBoxComDate1.Text = ""; this.textBoxCheckNumber1.Text = ""; this.numericUpDownPaid1.Text = "0.00";
            this.textBoxComDate2.Text = ""; this.textBoxCheckNumber2.Text = ""; this.numericUpDownPaid2.Text = "0.00";
            this.textBoxComDate3.Text = ""; this.textBoxCheckNumber3.Text = ""; this.numericUpDownPaid3.Text = "0.00";
            this.textBoxComDate4.Text = ""; this.textBoxCheckNumber4.Text = ""; this.numericUpDownPaid4.Text = "0.00";
            this.textBoxComDate5.Text = ""; this.textBoxCheckNumber5.Text = ""; this.numericUpDownPaid5.Text = "0.00";

            this.numericUpDownComAmount.Text = "0.00";
            this.numericUpDownComBalance.Text = "0.00";
            this.richTextBoxDeliveryNotes.Text = "";
            this.richTextBoxPONotes.Text = "";

            this.comboBoxBillTo.Text = "Customer";
            this.comboBoxBillStatus.Text = "Created";

            isDataLoadingOrders = false;

            #endregion

            insertNewDataRowOrders(nextPO);

            readOrderAndUpdateGUI(nextPO, 0);

            List<SortableRow> sorted = buildSortedRows();
            int found = 1;
            foreach (SortableRow sr in sorted)
            {
                if (sr.PO == nextPO)
                {
                    break;
                }
                found++;
            }
            this._currentRowOrders = found;
                
            refreshRecordIndicator(dbType.Order);
        }

        private void helperCreateNewQuoteRow(string nextQPO)
        {
            #region Default GUI settings

            // update the GUI with above read params
            isDataLoadingQuotes = true;

            this.textBoxQPO.Text = nextQPO;
            this.textBoxQDate.Text = todaysDate();
            this.comboBoxQStatus.Text = "";
            
            this.textBoxQCompany.Text = "";
            this.textBoxQCompanyReadOnly.Text = ""; // mirror

            this.textBoxVendorName.Text = "";
            this.textBoxQJobName.Text = "";
            this.textBoxQCustomerPO.Text = "";
            this.textBoxQVendorNumber.Text = "";
            this.comboBoxQSalesAss.Text = "";

            this.textBoxQTo.Text = "";
            this.textBoxQStreet1.Text = "";
            this.textBoxQStreet2.Text = "";
            this.textBoxQCity.Text = "";
            this.comboBoxQState.Text = "";
            this.textBoxQZip.Text = "";

            this.comboBoxQDelivery.Text = "";
            this.comboBoxQTerms.Text = "";
            this.comboBoxFreightSelect.Text = "Net price, freight included";
            this.checkBoxQManual.Checked = false;
            this.checkBoxPManual.Checked = true;
            this.textBoxQLocation.Text = "";
            this.textBoxQEquipment.Text = "";
            this.comboBoxQEquipCategory.Text = "";

            this.numericUpDownQQuotePrice.Text = "";
            this.textBoxQQuotedPriceReadOnly.Text = ""; // mirror

            this.textBoxShipTo.Text = "";
            this.textBoxShipToStreet1.Text = "";
            this.textBoxShipToStreet2.Text = "";
            this.textBoxShipToCity.Text = "";
            this.comboBoxShipToState.Text = "";
            this.textBoxShipToZip.Text = "";

            this.numericUpDownQQuotePrice.Text = "0.00";
            this.numericUpDownQCredit.Text = "0.00";
            this.numericUpDownQFreight.Text = "0.00";
            this.numericUpDownQShopTime.Text = "0.00";
            this.numericUpDownQTotalCost.Text = "0.00";
            this.numericUpDownQGrossProfit.Text = "0.00";
            this.numericUpDownQProfit.Text = "0.00";
            this.numericUpDownQMarkUp.Text = "40.00";

            this.textBoxQHeader.Text = "We are pleased to quote the following:";
            this.numericUpDownQQuan1.Text = "0.00"; this.textBoxQDescription1.Text = ""; this.numericUpDownQCost1.Text = "0.00"; this.numericUpDownQECost1.Text = "0.00"; this.numericUpDownQMCost1.Text = "0.00";
            this.numericUpDownQQuan2.Text = "0.00"; this.textBoxQDescription2.Text = ""; this.numericUpDownQCost2.Text = "0.00"; this.numericUpDownQECost2.Text = "0.00"; this.numericUpDownQMCost2.Text = "0.00";
            this.numericUpDownQQuan3.Text = "0.00"; this.textBoxQDescription3.Text = ""; this.numericUpDownQCost3.Text = "0.00"; this.numericUpDownQECost3.Text = "0.00"; this.numericUpDownQMCost3.Text = "0.00";
            this.numericUpDownQQuan4.Text = "0.00"; this.textBoxQDescription4.Text = ""; this.numericUpDownQCost4.Text = "0.00"; this.numericUpDownQECost4.Text = "0.00"; this.numericUpDownQMCost4.Text = "0.00";
            this.numericUpDownQQuan5.Text = "0.00"; this.textBoxQDescription5.Text = ""; this.numericUpDownQCost5.Text = "0.00"; this.numericUpDownQECost5.Text = "0.00"; this.numericUpDownQMCost5.Text = "0.00";
            this.numericUpDownQQuan6.Text = "0.00"; this.textBoxQDescription6.Text = ""; this.numericUpDownQCost6.Text = "0.00"; this.numericUpDownQECost6.Text = "0.00"; this.numericUpDownQMCost6.Text = "0.00";
            this.numericUpDownQQuan7.Text = "0.00"; this.textBoxQDescription7.Text = ""; this.numericUpDownQCost7.Text = "0.00"; this.numericUpDownQECost7.Text = "0.00"; this.numericUpDownQMCost7.Text = "0.00";
            this.numericUpDownQQuan8.Text = "0.00"; this.textBoxQDescription8.Text = ""; this.numericUpDownQCost8.Text = "0.00"; this.numericUpDownQECost8.Text = "0.00"; this.numericUpDownQMCost8.Text = "0.00";
            this.numericUpDownQQuan9.Text = "0.00"; this.textBoxQDescription9.Text = ""; this.numericUpDownQCost9.Text = "0.00"; this.numericUpDownQECost9.Text = "0.00"; this.numericUpDownQMCost9.Text = "0.00";
            this.numericUpDownQQuan10.Text = "0.00"; this.textBoxQDescription10.Text = ""; this.numericUpDownQCost10.Text = "0.00"; this.numericUpDownQECost10.Text = "0.00"; this.numericUpDownQMCost10.Text = "0.00";
            this.numericUpDownQQuan11.Text = "0.00"; this.textBoxQDescription11.Text = ""; this.numericUpDownQCost11.Text = "0.00"; this.numericUpDownQECost11.Text = "0.00"; this.numericUpDownQMCost11.Text = "0.00";
            this.numericUpDownQQuan12.Text = "0.00"; this.textBoxQDescription12.Text = ""; this.numericUpDownQCost12.Text = "0.00"; this.numericUpDownQECost12.Text = "0.00"; this.numericUpDownQMCost12.Text = "0.00";
            this.numericUpDownQQuan13.Text = "0.00"; this.textBoxQDescription13.Text = ""; this.numericUpDownQCost13.Text = "0.00"; this.numericUpDownQECost13.Text = "0.00"; this.numericUpDownQMCost13.Text = "0.00";
            this.numericUpDownQQuan14.Text = "0.00"; this.textBoxQDescription14.Text = ""; this.numericUpDownQCost14.Text = "0.00"; this.numericUpDownQECost14.Text = "0.00"; this.numericUpDownQMCost14.Text = "0.00";
            this.numericUpDownQQuan15.Text = "0.00"; this.textBoxQDescription15.Text = ""; this.numericUpDownQCost15.Text = "0.00"; this.numericUpDownQECost15.Text = "0.00"; this.numericUpDownQMCost15.Text = "0.00";
            this.numericUpDownQQuan16.Text = "0.00"; this.textBoxQDescription16.Text = ""; this.numericUpDownQCost16.Text = "0.00"; this.numericUpDownQECost16.Text = "0.00"; this.numericUpDownQMCost16.Text = "0.00";
            this.numericUpDownQQuan17.Text = "0.00"; this.textBoxQDescription17.Text = ""; this.numericUpDownQCost17.Text = "0.00"; this.numericUpDownQECost17.Text = "0.00"; this.numericUpDownQMCost17.Text = "0.00";
            this.numericUpDownQQuan18.Text = "0.00"; this.textBoxQDescription18.Text = ""; this.numericUpDownQCost18.Text = "0.00"; this.numericUpDownQECost18.Text = "0.00"; this.numericUpDownQMCost18.Text = "0.00";
            this.numericUpDownQQuan19.Text = "0.00"; this.textBoxQDescription19.Text = ""; this.numericUpDownQCost19.Text = "0.00"; this.numericUpDownQECost19.Text = "0.00"; this.numericUpDownQMCost19.Text = "0.00";
            this.numericUpDownQQuan20.Text = "0.00"; this.textBoxQDescription20.Text = ""; this.numericUpDownQCost20.Text = "0.00"; this.numericUpDownQECost20.Text = "0.00"; this.numericUpDownQMCost20.Text = "0.00";
            this.numericUpDownQQuan21.Text = "0.00"; this.textBoxQDescription21.Text = ""; this.numericUpDownQCost21.Text = "0.00"; this.numericUpDownQECost21.Text = "0.00"; this.numericUpDownQMCost21.Text = "0.00";
            this.numericUpDownQQuan22.Text = "0.00"; this.textBoxQDescription22.Text = ""; this.numericUpDownQCost22.Text = "0.00"; this.numericUpDownQECost22.Text = "0.00"; this.numericUpDownQMCost22.Text = "0.00";
            this.numericUpDownQQuan23.Text = "0.00"; this.textBoxQDescription23.Text = ""; this.numericUpDownQCost23.Text = "0.00"; this.numericUpDownQECost23.Text = "0.00"; this.numericUpDownQMCost23.Text = "0.00";

            this.richTextBoxQInvoicingNotes.Text = "";
            this.richTextBoxQDeliveryNotes.Text = "";
            this.richTextBoxQQuoteNotes.Text = "";
            
            isDataLoadingQuotes = false;

            #endregion

            insertNewDataRowQuotes(nextQPO);

            readQuoteAndUpdateGUI(nextQPO, 0);

            int count = Sql.GetRowCounts(dbType.Quote);
            this._currentRowQuotes = count;

            refreshRecordIndicator(dbType.Quote);
        }

        private void nextCurrentLetterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string currentPO = this.textBoxPO.Text;
            List<string> POs = Sql.GetPOs(dbType.Order);

            if (POs.Count == 0)
                return;

            if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                updateRowOrders();

            POs.Sort();

            string[] no = currentPO.Split('-');

            string digs = "0123456789";
            List<char> subPOs = new List<char>();
            foreach (string PO in POs)
                if (PO.StartsWith(no[0]))
                    if (!digs.Contains(PO[PO.Length - 1]))
                        subPOs.Add(PO[PO.Length - 1]);

            subPOs.Sort();

            // ignore delv tickets & warrs
            if (subPOs.Contains('T'))
                subPOs.Remove('T');
            if (subPOs.Contains('W'))
                subPOs.Remove('W');

            char letter = 'A';
            if (subPOs.Count() == 0)
            {
                // continue
            }
            else
            {
                letter = subPOs[subPOs.Count() - 1];
                letter++;
            }

            if (letter >= 'T')
            {
                MessageBox.Show(this, "You have exceeded the usable letters.", "Aborting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // create it!
            helperCreateNewOrderRow(no[0] + "-" + generatePOYearEnd("" + letter), true);
        }

        private void createNewDbaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (askImportantQuestion("DANGEROUS! This will delete the entire database. Are you sure?") == System.Windows.Forms.DialogResult.No)
                return;

            createNewDbase();
        }

        private void removeAllRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isOrdersSelected)
            {
                if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                    updateRowOrders();

                int count = Sql.GetRowCounts(dbType.Order);

                do
                {
                    if (count == 0)
                        return;

                    Sql.DeletePO(dbType.Order, this.textBoxPO.Text);

                    count--;
                    this._currentRowOrders--;

                    loadGUIforViewing(dbType.Order);
                } while (count > 0);
            }
            else
            {
                if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                    updateRowQuotes();

                int count = Sql.GetRowCounts(dbType.Quote);

                do
                {
                    if (count == 0)
                        return;

                    Sql.DeletePO(dbType.Quote, this.textBoxQPO.Text);

                    count--;
                    this._currentRowQuotes--;

                    loadGUIforViewing(dbType.Quote);
                } while (count > 0);
            }
        }

        private void removeRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isOrdersSelected)
            {
                if (this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                    updateRowOrders();

                int count = Sql.GetRowCounts(dbType.Order);

                if (count == 0)
                    return;

                if (askImportantQuestion("This will delete record " + this.textBoxPO.Text + " from the database. Are you sure?") == System.Windows.Forms.DialogResult.No)
                    return;

                Sql.DeletePO(dbType.Order, this.textBoxPO.Text);

                count--;

                if (this._currentRowOrders > 1)
                    this._currentRowOrders--;

                loadGUIforViewing(dbType.Order);
            }
            else
            {
                if (this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                    updateRowQuotes();

                int count = Sql.GetRowCounts(dbType.Quote);

                if (count == 0)
                    return;

                if (askImportantQuestion("This will delete record " + this.textBoxQPO.Text + " from the database. Are you sure?") == System.Windows.Forms.DialogResult.No)
                    return;

                Sql.DeletePO(dbType.Quote, this.textBoxQPO.Text);

                count--;

                if (this._currentRowQuotes > 1)
                    this._currentRowQuotes--;

                loadGUIforViewing(dbType.Quote);
            }
        }

        private void cleanDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Sql.VacuumDatabase();
            MessageBox.Show(this, "Clean Done!");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, 
                "This program is the propery of Roy Tugwell -- all rights reserved." + Environment.NewLine +
                "Programmed by: John Coe III 2015" + Environment.NewLine + Environment.NewLine +
                "Current Version: " + this.Text, 
                "RMT Tugwell About", MessageBoxButtons.OK);
        }

        private void removeRecordLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormPassword fPswd = new FormPassword();
            fPswd.ShowDialog(this);

            if (_isOrdersSelected)
            {
                if (!this.isDataDirtyOrders && this.groupBoxOrders.Enabled)
                {
                    if (fPswd.password == "2017")
                        if (removeLockOrders(this.textBoxPO.Text))
                            MessageBox.Show(this, "This order record is now unlocked.");
                }
                else if (this.groupBoxOrders.Enabled)
                {
                    MessageBox.Show(this, "This order record is under edit by you!  Just save and it will unlock.");
                }
            }
            else
            {
                if (!this.isDataDirtyQuotes && this.groupBoxQuotes.Enabled)
                {
                    if (fPswd.password == "2017")
                        if (removeLockQuotes(this.textBoxQPO.Text))
                            MessageBox.Show(this, "This quote record is now unlocked.");
                }
                else if (this.groupBoxQuotes.Enabled)
                {
                    MessageBox.Show(this, "This quote record is under edit by you!  Just save and it will unlock.");
                }
            }
        }

        private void importCompaniesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void importPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void displayTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string table = "OrderTable";

            List<string> list = Sql.GetTableRowNames(table);
            StringBuilder sb = new StringBuilder();
            sb.Append(table + ":\r\n");
            foreach (string item in list)
                sb.Append(item + Environment.NewLine);

            MessageBox.Show(this, sb.ToString());
        }

        private void addTableRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // add version table test
            //Sql.AppendNewTableWithDefaultRow();
        }

        #endregion

        #region Lower-Level helper functions for orders and quotes

        private bool loadGUIAndLock(dbType t)
        {
            bool was_loaded = false;
            if (t == dbType.Order)
            {
                do
                {
                    was_loaded = !loadGUIforViewing(dbType.Order);

                    if (!was_loaded)
                        return false; // abort

                    if (placeLockOrder() == true)
                    {
                        break;
                    }

                    if (MessageBox.Show(this, "Could not create the lock.  If you don't do this data loss could occur.  Do you want to retry?", "Lock Error", MessageBoxButtons.RetryCancel) != System.Windows.Forms.DialogResult.Retry)
                        break;

                } while (true);

                this.isDataDirtyOrders = true; // safety

                return was_loaded;
            }
            else
            {
                do
                {
                    was_loaded = !loadGUIforViewing(dbType.Quote);

                    if (!was_loaded)
                        return false; // abort

                    if (placeLockQuote() == true)
                    {
                        break;
                    }

                    if (MessageBox.Show(this, "Could not create the lock.  If you don't do this data loss could occur.  Do you want to retry?", "Lock Error", MessageBoxButtons.RetryCancel) != System.Windows.Forms.DialogResult.Retry)
                        break;

                } while (true);

                this.isDataDirtyQuotes = true; // safety

                return was_loaded;
            }
        }

        private bool loadGUIforViewing(dbType t)
        {
            if (t == dbType.Order)
            {
                if (refreshRecordIndicator(t) > 0)
                {
                    List<SortableRow> sorted = buildSortedRows();
                    return readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                }
                else
                    return false; // empty, force unloacked and loaded
            }
            else
            {
                if (refreshRecordIndicator(t) > 0)
                {
                    return readQuoteAndUpdateGUI("", this._currentRowQuotes);
                }
                else
                    return false; // empty, force unloacked and loaded
            }
        }

        private void lockGUIOrders(bool isLocked)
        {
            this.groupBoxOrders.Enabled = !isLocked;
            this.tabControlOrdersSub.Enabled = !isLocked;
        }

        private void lockGUIQuotes(bool isLocked)
        {
            this.groupBoxQuotes.Enabled = !isLocked;
            this.tabControlQuotesSub.Enabled = !isLocked;
        }

        private void updateGUIStatusBar()
        {
            if (this.groupBoxOrders.Enabled == false)
                this.toolStripStatusLabel.Text = "Status Orders: LOCKED";
            else if (this.isDataDirtyOrders)
                this.toolStripStatusLabel.Text = "Status Orders: Dirty";
            else
                this.toolStripStatusLabel.Text = "Status Orders: Clean";

            if (this.groupBoxQuotes.Enabled == false)
                this.toolStripStatusLabel2.Text = "Status Quotes: LOCKED";
            else if (this.isDataDirtyQuotes)
                this.toolStripStatusLabel2.Text = "Status Quotes: Dirty";
            else
                this.toolStripStatusLabel2.Text = "Status Quotes: Clean";
        }

        private bool LowLevelLockChecking(dbType t)
        {
            bool locked = false;

            if (t == dbType.Order)
            {
                List<string> theListOfLocks = getLockListOrders();

                if (theListOfLocks == null)
                { // hmmmm, IO error do nothing abnormal for now
                }
                else if (theListOfLocks.Contains(this.textBoxPO.Text))
                {
                    // if order is not locked, lock it
                    if (this.groupBoxOrders.Enabled == true)
                        lockGUIOrders(true);
                    locked = true;
                }
                else
                {
                    // if order is locked, unlock it
                    if (this.groupBoxOrders.Enabled == false)
                        lockGUIOrders(false);
                    locked = false;
                }
            }
            else
            {
                List<string> theListOfLocks = getLockListQuotes();

                if (theListOfLocks == null)
                { // hmmmm, IO error do nothing abnormal for now
                }
                else if (theListOfLocks.Contains(this.textBoxQPO.Text))
                {
                    // if quote is not locked, lock it
                    if (this.groupBoxQuotes.Enabled == true)
                        lockGUIQuotes(true);
                    locked = true;
                }
                else
                {
                    // if quote is locked, unlock it
                    if (this.groupBoxQuotes.Enabled == false)
                        lockGUIQuotes(false);
                    locked = false;
                }
            }

            updateGUIStatusBar();

            return locked;
        }



        #endregion

        #region Company Switch Event Clicks

        private void oldRMTugwellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _company = companyType.RMT;

            this.oldRMTugwellToolStripMenuItem.Checked = true;
            this.newTPSToolStripMenuItem.Checked = false;

            this.pictureBoxLogo.BackgroundImage = global::Tugwell.Properties.Resources.tug;

            save(dbType.Order);
            save(dbType.Quote);
            Sql.Killconnection();

            _DBASENAME = "tugMain.db3";
            _POOrderFirst = 30100;
            _QuoteNoFirst = 7000;

            //((Control)this.tabPageMarley).Enabled = true;

            this.groupBox2.Text = "RMT Delivery to Customer";
            this.comboBoxCarrier.Items.Clear(); 
            this.comboBoxCarrier.Items.AddRange(new object[] {
            "",
            "UPS Ground",
            "UPS Next Day Air",
            "Motor Freight",
            "Customer P/U",
            "RMT Truck",
            "Fedex Freight",
            "FedEx Ground",
            "Vendor Vehicle"});
            this.comboBoxSalesAss.Items.Clear();
            this.comboBoxSalesAss.Items.AddRange(new object[] {
            "",
            "Justin Tugwell",
            "Roy Tugwell",
            "Warehouse",
            "Marsha Outlaw"});
            this.comboBoxQSalesAss.Items.Clear();
            this.comboBoxQSalesAss.Items.AddRange(new object[] {
            "",
            "Justin Tugwell",
            "Roy Tugwell",
            "Warehouse",
            "Marsha Outlaw"});
            //this.toolStripComboBoxSignature.Items.Clear();
            //this.toolStripComboBoxSignature.Items.AddRange(new object[] {
            //"No Signature",
            //"Justin\'s",
            //"Roy\'s",
            //"Warehouse\'s",
            //"Marsha\'s"});

            loadStartup();
        }

        private void newTPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _company = companyType.TPS;

            this.oldRMTugwellToolStripMenuItem.Checked = false;
            this.newTPSToolStripMenuItem.Checked = true;

            this.pictureBoxLogo.BackgroundImage = global::Tugwell.Properties.Resources.tps;

            save(dbType.Order);
            save(dbType.Quote);
            Sql.Killconnection();

            _DBASENAME = "tpsMain.db3";
            _POOrderFirst = 60000;
            _QuoteNoFirst = 12000;

            //((Control)this.tabPageMarley).Enabled = false;

            this.groupBox2.Text = "TPS Delivery to Customer";
            this.comboBoxCarrier.Items.Clear();
            this.comboBoxCarrier.Items.AddRange(new object[] {
            "",
            "UPS Ground",
            "UPS Next Day Air",
            "Motor Freight",
            "Customer P/U",
            "TPS Truck",
            "Fedex Freight",
            "FedEx Ground",
            "Vendor Vehicle"});
            this.comboBoxSalesAss.Items.Clear();
            this.comboBoxSalesAss.Items.AddRange(new object[] {
            "",
            "Justin Tugwell",
            "Roy Tugwell",
            "Randy Swindle",
            "Marsha Outlaw",
            "Warehouse",
            "Bernice Williams",
            "Mark Gilmore",
            "Carl Hilgenberg"});
            this.comboBoxQSalesAss.Items.Clear();
            this.comboBoxQSalesAss.Items.AddRange(new object[] {
            "",
            "Justin Tugwell",
            "Roy Tugwell",
            "Randy Swindle",
            "Marsha Outlaw",
            "Warehouse",
            "Bernice Williams",
            "Mark Gilmore",
            "Carl Hilgenberg"});
            //this.toolStripComboBoxSignature.Items.Clear();
            //this.toolStripComboBoxSignature.Items.AddRange(new object[] {
            //"No Signature",
            //"Justin\'s",
            //"Roy\'s",
            //"Randy\'s",
            //"Marsha\'s",
            //"Warehouse\'s",
            //"Bernice Williams\'",
            //"Mark Gilmore's",
            //"Carl Hilgenberg's"});

            loadStartup();
        }

        #endregion

        #region Quote -> Order Copy

        private void duplicateCurrentQuoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult sel = MessageBox.Show(this,
                "The current Quote is: " + textBoxQPO.Text + Environment.NewLine +
                "This will copy all Quote information into this Order and this action cannot be undone." + Environment.NewLine +
                Environment.NewLine + "Continue?", "Confirm", MessageBoxButtons.YesNoCancel);

            if (sel == System.Windows.Forms.DialogResult.Yes)
            {
                //textBoxDescription.Text = textBoxQHeader.Text;

                numericUpDownOrderCount1.Text = numericUpDownQQuan1.Text;
                numericUpDownOrderCount1.Text = numericUpDownQQuan1.Text;// is this a bug? side-effect
                textBoxOrderDescr1.Text = textBoxQDescription1.Text;
                numericUpDownOrderCost1.Text = numericUpDownQCost1.Text;

                numericUpDownOrderCount2.Text = numericUpDownQQuan2.Text;
                textBoxOrderDescr2.Text = textBoxQDescription2.Text;
                numericUpDownOrderCost2.Text = numericUpDownQCost2.Text;

                numericUpDownOrderCount3.Text = numericUpDownQQuan3.Text;
                textBoxOrderDescr3.Text = textBoxQDescription3.Text;
                numericUpDownOrderCost3.Text = numericUpDownQCost3.Text;

                numericUpDownOrderCount4.Text = numericUpDownQQuan4.Text;
                textBoxOrderDescr4.Text = textBoxQDescription4.Text;
                numericUpDownOrderCost4.Text = numericUpDownQCost4.Text;

                numericUpDownOrderCount5.Text = numericUpDownQQuan5.Text;
                textBoxOrderDescr5.Text = textBoxQDescription5.Text;
                numericUpDownOrderCost5.Text = numericUpDownQCost5.Text;

                numericUpDownOrderCount6.Text = numericUpDownQQuan6.Text;
                textBoxOrderDescr6.Text = textBoxQDescription6.Text;
                numericUpDownOrderCost6.Text = numericUpDownQCost6.Text;

                numericUpDownOrderCount7.Text = numericUpDownQQuan7.Text;
                textBoxOrderDescr7.Text = textBoxQDescription7.Text;
                numericUpDownOrderCost7.Text = numericUpDownQCost7.Text;

                numericUpDownOrderCount8.Text = numericUpDownQQuan8.Text;
                textBoxOrderDescr8.Text = textBoxQDescription8.Text;
                numericUpDownOrderCost8.Text = numericUpDownQCost8.Text;

                numericUpDownOrderCount9.Text = numericUpDownQQuan9.Text;
                textBoxOrderDescr9.Text = textBoxQDescription9.Text;
                numericUpDownOrderCost9.Text = numericUpDownQCost9.Text;

                numericUpDownOrderCount10.Text = numericUpDownQQuan10.Text;
                textBoxOrderDescr10.Text = textBoxQDescription10.Text;
                numericUpDownOrderCost10.Text = numericUpDownQCost10.Text;

                numericUpDownOrderCount11.Text = numericUpDownQQuan11.Text;
                textBoxOrderDescr11.Text = textBoxQDescription11.Text;
                numericUpDownOrderCost11.Text = numericUpDownQCost11.Text;

                numericUpDownOrderCount12.Text = numericUpDownQQuan12.Text;
                textBoxOrderDescr12.Text = textBoxQDescription12.Text;
                numericUpDownOrderCost12.Text = numericUpDownQCost12.Text;

                numericUpDownOrderCount13.Text = numericUpDownQQuan13.Text;
                textBoxOrderDescr13.Text = textBoxQDescription13.Text;
                numericUpDownOrderCost13.Text = numericUpDownQCost13.Text;

                numericUpDownOrderCount14.Text = numericUpDownQQuan14.Text;
                textBoxOrderDescr14.Text = textBoxQDescription14.Text;
                numericUpDownOrderCost14.Text = numericUpDownQCost14.Text;

                numericUpDownOrderCount15.Text = numericUpDownQQuan15.Text;
                textBoxOrderDescr15.Text = textBoxQDescription15.Text;
                numericUpDownOrderCost15.Text = numericUpDownQCost15.Text;

                numericUpDownOrderCount16.Text = numericUpDownQQuan16.Text;
                textBoxOrderDescr16.Text = textBoxQDescription16.Text;
                numericUpDownOrderCost16.Text = numericUpDownQCost16.Text;

                numericUpDownOrderCount17.Text = numericUpDownQQuan17.Text;
                textBoxOrderDescr17.Text = textBoxQDescription17.Text;
                numericUpDownOrderCost17.Text = numericUpDownQCost17.Text;

                numericUpDownOrderCount18.Text = numericUpDownQQuan18.Text;
                textBoxOrderDescr18.Text = textBoxQDescription18.Text;
                numericUpDownOrderCost18.Text = numericUpDownQCost18.Text;

                numericUpDownOrderCount19.Text = numericUpDownQQuan19.Text;
                textBoxOrderDescr19.Text = textBoxQDescription19.Text;
                numericUpDownOrderCost19.Text = numericUpDownQCost19.Text;

                numericUpDownOrderCount20.Text = numericUpDownQQuan20.Text;
                textBoxOrderDescr20.Text = textBoxQDescription20.Text;
                numericUpDownOrderCost20.Text = numericUpDownQCost20.Text;

                numericUpDownOrderCount21.Text = numericUpDownQQuan21.Text;
                textBoxOrderDescr21.Text = textBoxQDescription21.Text;
                numericUpDownOrderCost21.Text = numericUpDownQCost21.Text;

                numericUpDownOrderCount22.Text = numericUpDownQQuan22.Text;
                textBoxOrderDescr22.Text = textBoxQDescription22.Text;
                numericUpDownOrderCost22.Text = numericUpDownQCost22.Text;

                numericUpDownOrderCount23.Text = numericUpDownQQuan23.Text;
                textBoxOrderDescr23.Text = textBoxQDescription23.Text;
                numericUpDownOrderCost23.Text = numericUpDownQCost23.Text;

                MessageBox.Show(this, "Copy Completed.");
            }
        }

        #endregion
    }
}