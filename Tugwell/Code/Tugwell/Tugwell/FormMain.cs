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
        public FormMain()
        {
            InitializeComponent();

            generateLockName();

            this.Text = "Tugwell V5.5 2016_10_03";
            
            // make sure dbase file is the only one in this folder
            this.toolStripTextBoxDbasePath.Text = @"Z:\Tugwell\DB\";
            this.comboBoxYearControl.Text = "2016";

            this.toolStripComboBoxSignature.SelectedIndex = 0; // no signature

            bool isAdminVersion = false;
            this.importCompaniesToolStripMenuItem.Enabled = isAdminVersion;
            this.importPartsToolStripMenuItem.Enabled = isAdminVersion;
            this.createNewDbaseToolStripMenuItem.Enabled = isAdminVersion;

            loadStartup();
        }

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

        public static logFile _log = new logFile("log.txt");


        #region Vars...

        private readonly string _DBASENAME = "tugMain.db3";
        private readonly int _POOrderFirst = 30100;
        private readonly int _QuoteNoFirst = 7000;

        private string _lockName;

        private bool _isOrdersSelected = true;
        
        private bool isDataLoadingOrders = false;
        private bool isDataDirtyOrders = false;
        private int _currentRowOrders = 0;

        private bool isDataLoadingQuotes = false;
        private bool isDataDirtyQuotes = false;
        private int _currentRowQuotes = 0;

        private bool _isLetterControlEnabled = true;
        private List<int> _letterRows = new List<int>();

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
                
                refreshRecordIndicatorOrders();
            }
            else
            {
                this.pONumberToolStripMenuItem.Enabled = false;
                this.quoteNumberToolStripMenuItem.Enabled = true;
                this.printCurrentOrderToolStripMenuItem.Enabled = false;
                this.printCurrentQuoteToolStripMenuItem.Enabled = true;

                refreshRecordIndicatorQuotes();
            }

            autoSelectSignature();
        }

        #endregion

        //Random _MyRandom = new Random(Guid.NewGuid().GetHashCode());
        //private int getRandom()
        //{
        //    return _MyRandom.Next(100);
        //}


        #region Loading Startup...

        private void loadStartup()
        {
            this._currentRowQuotes = getRowCountsFromQuotes();
            this._currentRowOrders = getRowCountsFromOrders();

            if (refreshRecordIndicatorQuotes() > 0)
            {
                // load the first record & load GUI for Quotes
                readQuoteAndUpdateGUI("", this._currentRowQuotes);
            }

            if (refreshRecordIndicatorOrders() > 0)
            {
                List<SortableRow> sorted = buildSortedRows();

                // load the first record & load GUI for Orders
                readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);//"", this._currentRowOrders);
            }

            refreshLetterControl(getRowCountsFromOrders());
        }

        #endregion

        #region Form Closing Event

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (refreshRecordIndicatorOrders() > 0)
            {
                if (this.isDataDirtyOrders)
                    updateRowOrders();
            }
            if (refreshRecordIndicatorQuotes() > 0)
            {
                if (this.isDataDirtyQuotes)
                    updateRowQuotes();
            }

            // do this 'vacuum' only 10% of the time
            //Random r = new Random(Guid.NewGuid().GetHashCode());
            //int num = r.Next(100);
            //if (num > 90)
            //    vacuumDatabase();

            Killconnection();
        }

        #endregion


        #region Refresh Record Indicators for Orders

        private int refreshRecordIndicatorOrders()
        {
            _log.append("refreshRecordIndicatorOrders start");

            int count = getRowCountsFromOrders();

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

        #endregion

        #region Refresh Record Indicators for Quotes

        private int refreshRecordIndicatorQuotes()
        {
            int count = getRowCountsFromQuotes();

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

                            int count = getRowCountsFromOrders();

                            if (count == 0 || value == 0)
                                return;
                            else if (value > count)
                                return;

                            if (this.isDataDirtyOrders)
                                updateRowOrders();

                            this._currentRowOrders = value;

                            //readOrderAndUpdateGUI("", this._currentRowOrders);
                            List<SortableRow> sorted = buildSortedRows();
                            readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);

                            refreshRecordIndicatorOrders();
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

            List<string> POs = getPOsFromOrders();

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
            List<string> QPOs = getPOsFromQuotes();

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
                int count = getRowCountsFromOrders();

                if (count == 0)
                    return;

                if (this.isDataDirtyOrders)
                    updateRowOrders();

                this._currentRowOrders = 1;

                List<SortableRow> sorted = buildSortedRows();
                readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                //readOrderAndUpdateGUI("", 1);

                refreshRecordIndicatorOrders();
            }
            else
            {
                int count = getRowCountsFromQuotes();

                if (count == 0)
                    return;

                if (this.isDataDirtyQuotes)
                    updateRowQuotes();

                this._currentRowQuotes = 1;

                readQuoteAndUpdateGUI("", 1);

                refreshRecordIndicatorQuotes();
            }
        }

        private void buttonRecordGo1Less_Click(object sender, EventArgs e)
        {
            if (this._isOrdersSelected)
            {
                int count = getRowCountsFromOrders();

                if (count == 0)
                    return;
                else if (this._currentRowOrders <= 1)
                    return;

                if (this.isDataDirtyOrders)
                    updateRowOrders();

                this._currentRowOrders--;

                List<SortableRow> sorted = buildSortedRows();
                readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                //readOrderAndUpdateGUI("", this._currentRowOrders);

                refreshRecordIndicatorOrders();
            }
            else
            {
                int count = getRowCountsFromQuotes();

                if (count == 0)
                    return;
                else if (this._currentRowQuotes <= 1)
                    return;

                if (this.isDataDirtyQuotes)
                    updateRowQuotes();

                this._currentRowQuotes--;

                readQuoteAndUpdateGUI("", this._currentRowQuotes);

                refreshRecordIndicatorQuotes();
            }
        }

        private void buttonRecordGo1More_Click(object sender, EventArgs e)
        {
            if (this._isOrdersSelected)
            {
                int count = getRowCountsFromOrders();

                if (count == 0)
                    return;
                else if (this._currentRowOrders >= count)
                    return;

                if (this.isDataDirtyOrders)
                    updateRowOrders();

                this._currentRowOrders++;

                List<SortableRow> sorted = buildSortedRows();
                readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                //readOrderAndUpdateGUI("", this._currentRowOrders);

                refreshRecordIndicatorOrders();
            }
            else
            {
                int count = getRowCountsFromQuotes();

                if (count == 0)
                    return;
                else if (this._currentRowQuotes >= count)
                    return;

                if (this.isDataDirtyQuotes)
                    updateRowQuotes();

                this._currentRowQuotes++;

                readQuoteAndUpdateGUI("", this._currentRowQuotes);

                refreshRecordIndicatorQuotes();
            }
        }

        private void buttonRecordBottom_Click(object sender, EventArgs e)
        {
            if (this._isOrdersSelected)
            {
                int count = getRowCountsFromOrders();

                if (count == 0)
                    return;

                if (this.isDataDirtyOrders)
                    updateRowOrders();

                this._currentRowOrders = count;

                List<SortableRow> sorted = buildSortedRows();
                readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                //readOrderAndUpdateGUI("", count);

                refreshRecordIndicatorOrders();
            }
            else
            {
                int count = getRowCountsFromQuotes();

                if (count == 0)
                    return;

                if (this.isDataDirtyQuotes)
                    updateRowQuotes();

                this._currentRowQuotes = count;

                readQuoteAndUpdateGUI("", count);

                refreshRecordIndicatorQuotes();
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

        #region Signature Auto Selection

        private void autoSelectSignature()
        {
            this.toolStripComboBoxSignature.SelectedIndex = (this._isOrdersSelected) ?
                this.comboBoxSalesAss.SelectedIndex :
                this.comboBoxQSalesAss.SelectedIndex;
        }

        private void comboBoxSalesAss_SelectionChangeCommitted(object sender, EventArgs e)
        {
            autoSelectSignature();
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
                    if (refreshRecordIndicatorOrders() > 0)
                    {
                        List<SortableRow> sorted = buildSortedRows();

                        // load the first record & load GUI for Orders
                        readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                    }

                    placeLockOrder();
                    this.isDataDirtyOrders = true; // safety
                }
            }
            else
            {
                // if quote was clean, reload to be safe
                if (!this.isDataDirtyQuotes)
                {
                    if (refreshRecordIndicatorQuotes() > 0)
                    {
                        // load the first record & load GUI for Quotes
                        readQuoteAndUpdateGUI("", this._currentRowQuotes);
                    }

                    placeLockQuote();
                    this.isDataDirtyQuotes = true;
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
                    if (refreshRecordIndicatorOrders() > 0)
                    {
                        List<SortableRow> sorted = buildSortedRows();

                        // load the first record & load GUI for Orders
                        readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                        was_loaded = true;
                    }

                    placeLockOrder();
                    this.isDataDirtyOrders = true; // safety
                }
            }
            else
            {
                // if quote was clean, reload to be safe
                if (!this.isDataDirtyQuotes)
                {
                    if (refreshRecordIndicatorQuotes() > 0)
                    {
                        // load the first record & load GUI for Quotes
                        readQuoteAndUpdateGUI("", this._currentRowQuotes);
                        was_loaded = true;
                    }

                    placeLockQuote();
                    this.isDataDirtyQuotes = true;
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
            dbaseControlChangedEvent();
        }

        private bool killOrderComboEvent = false;
        private void comboBoxSalesAss_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (killOrderComboEvent)
                return;

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
            dbaseControlChangedEvent();
        }

        private bool killOrderCheckEvent = false;
        private void checkBoxComOrder_CheckedChanged(object sender, EventArgs e)
        {
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
            //_log.append("dbaseControlChangedEvent start");

            // any control actually runs here when changed
            if (!isDataLoadingOrders)
            {
                // user must be making changes to the order controls!
                List<string> theListOfLocks = getLockListOrders();
                if (theListOfLocks.Contains(this.textBoxPO.Text))
                {
                    lockGUIOrders(true);
                    return;
                }
                else
                {
                    lockGUIOrders(false);
                }

                // just in case!
                this.textBoxSoldToReadOnly.Text = this.textBoxSoldTo.Text;

                if (!this.isDataDirtyOrders)
                {
                    if (refreshRecordIndicatorOrders() > 0)
                    {
                        List<SortableRow> sorted = buildSortedRows();

                        // load the first record & load GUI for Orders
                        readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                    }

                    // create the lock file one time!
                    placeLockOrder();
                }

                this.isDataDirtyOrders = true;
            }

            if (this.isDataDirtyOrders)
                this.toolStripStatusLabel.Text = "Status Orders: Dirty";
            else
                this.toolStripStatusLabel.Text = "Status Orders: Clean";

            doTheMathOrders();

            //_log.append("dbaseControlChangedEvent end");
        }

        #endregion

        #region Change Events Quotes

        private void textBoxQuote_TextChanged(object sender, EventArgs e)
        {
            dbaseControlChangedEvent2();
        }

        private bool killQuoteComboEvent = false;
        private void comboBoxQuote_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (killQuoteComboEvent)
                return;

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
            dbaseControlChangedEvent2();
        }

        private bool killQuoterCheckEvent = false;
        private void checkBoxQuote_CheckedChanged(object sender, EventArgs e)
        {
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
            // any control actually runs here when changed
            if (!isDataLoadingQuotes)
            {
                // user must be making changes to the order controls!
                List<string> theListOfLocks = getLockListQuotes();
                if (theListOfLocks.Contains(this.textBoxQPO.Text))
                {
                    lockGUIQuotes(true);
                    return;
                }
                else
                {
                    lockGUIQuotes(false);
                }

                // just in case!
                this.textBoxQCompanyReadOnly.Text = this.textBoxQCompany.Text;
                this.textBoxQQuotedPriceReadOnly.Text = this.numericUpDownQQuotePrice.Text;

                if (!this.isDataDirtyQuotes)
                {
                    if (refreshRecordIndicatorQuotes() > 0)
                    {
                        // load the first record & load GUI for Quotes
                        readQuoteAndUpdateGUI("", this._currentRowQuotes);
                    }

                    // create the lock file one time!
                    placeLockQuote();
                }

                this.isDataDirtyQuotes = true;
            }

            if (this.isDataDirtyQuotes)
                this.toolStripStatusLabel2.Text = "Status Quotes: Dirty";
            else
                this.toolStripStatusLabel2.Text = "Status Quotes: Clean";

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

                        int count = getRowCountsFromOrders();

                        if (count == 0 || value == 0)
                            return;
                        else if (value > count)
                            return;

                        if (this.isDataDirtyOrders)
                            updateRowOrders();

                        this._currentRowOrders = value;

                        List<SortableRow> sorted = buildSortedRows();
                        readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                        //readOrderAndUpdateGUI("", this._currentRowOrders);

                        refreshRecordIndicatorOrders();
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

                        int count = getRowCountsFromQuotes();

                        if (count == 0 || value == 0)
                            return;
                        else if (value > count)
                            return;

                        if (this.isDataDirtyQuotes)
                            updateRowQuotes();

                        this._currentRowQuotes = value;

                        readQuoteAndUpdateGUI("", this._currentRowQuotes);

                        refreshRecordIndicatorQuotes();
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


        #region Menu Strip Events - saving, printing, new orders, new quotes, etc.

        // saving
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            save();
        }

        private void save()
        {
            if (_isOrdersSelected)
            {
                if (refreshRecordIndicatorOrders() > 0)
                {
                    if (this.isDataDirtyOrders)
                        updateRowOrders();
                }
            }
            else
            {
                if (refreshRecordIndicatorQuotes() > 0)
                {
                    if (this.isDataDirtyQuotes)
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
                if (this.isDataDirtyOrders)
                    updateRowOrders();

                //this._currentRowOrders = go._Row;

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
                readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                
                
                //readOrderAndUpdateGUI("", this._currentRowOrders);

                refreshRecordIndicatorOrders();
            }
        }

        // goto for quotes
        private void gotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormGoto go = new FormGoto(this, getDbasePathName(), false);
            go.ShowDialog(this);

            if (go.IsSelected && go._Row >= 0)
            {
                if (this.isDataDirtyQuotes)
                    updateRowQuotes();

                this._currentRowQuotes = go._Row;

                readQuoteAndUpdateGUI("", this._currentRowQuotes);

                refreshRecordIndicatorQuotes();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void purchaseOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Print Purchase Order

            Print2Pdf p = new Print2Pdf();

            double pos = addHeader(p, "Purchase Order") + 6.0;

            // Date: (left side)
            Print2Pdf.TextInfo tiTopLeft = p.CreateTextInfo();
            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopLeft.Size = 12.0;
            p.AddText(tiTopLeft, "Date:", 40, pos, p.Width, 18);
            p.AddText(tiTopLeft, "Vendor:");
            p.AddText(tiTopLeft, "Req. Ship:");
            p.AddText(tiTopLeft, "Ship Via:");
            p.AddText(tiTopLeft, "Salesman:");
            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopLeft, this.textBoxPODate.Text/*textBoxDate.Text*/, 40 + 80, pos, p.Width, 18);
            p.AddText(tiTopLeft, this.textBoxVendorName.Text);
            p.AddText(tiTopLeft, this.textBoxReqDate.Text);
            p.AddText(tiTopLeft, this.textBoxPOShipVia.Text);
            p.AddText(tiTopLeft, this.comboBoxSalesAss.Text);

            // POs Marks (right)
            Print2Pdf.TextInfo tiTopRight = p.CreateTextInfo();
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopRight.Size = 12.0;
            p.AddText(tiTopRight, "PO #:", p.Width / 2, pos, p.Width, 18);
            p.AddText(tiTopRight, "Marks:");
            p.AddText(tiTopRight, "Ship To:");
            p.AddText(tiTopRight, "Address:");
            p.AddText(tiTopRight, this.textBoxPO.Text, p.Width / 2 + 56, pos, p.Width, 18);
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopRight, this.textBoxCustomerPO.Text);
            p.AddText(tiTopRight, this.textBoxShipTo.Text);
            string streets = this.textBoxShipToStreet1.Text + "\n" + this.textBoxShipToStreet2.Text;
            p.AddText(tiTopRight, streets.Trim() + "\n" + this.textBoxShipToCity.Text + ", " + this.comboBoxShipToState.Text + " " + this.textBoxShipToZip.Text);

            // Quoted Equip : Terms : Delivery
            p.DrawHorizontalLineDouble(1.0, pos + 75, 40.0);

            Print2Pdf.TextInfo tiDescr = p.CreateTextInfo();
            tiDescr.Textstyle = Print2Pdf.TextStyle.Bold;
            tiDescr.Size = 9.0;
            p.AddText(tiDescr, "Description:", 40, pos + 85, p.Width, 18);
            tiDescr.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiDescr, this.textBoxDescription.Text, 40 + 80, pos + 85, p.Width, 18);

            int rowCount = 13;

            #region how many rows used? min is 12 + one for header
            if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr23.Text))
            {
                rowCount = 24;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr22.Text))
            {
                rowCount = 23;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr21.Text))
            {
                rowCount = 22;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr20.Text))
            {
                rowCount = 21;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr19.Text))
            {
                rowCount = 20;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr18.Text))
            {
                rowCount = 19;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr17.Text))
            {
                rowCount = 18;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr16.Text))
            {
                rowCount = 17;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr15.Text))
            {
                rowCount = 16;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr14.Text))
            {
                rowCount = 15;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr13.Text))
            {
                rowCount = 14;
            }
            else
            {
                rowCount = 13;
            }
            #endregion

            Print2Pdf.TextInfo table1 = p.AddTable(pos + 105, rowCount, 12.0, new List<double> { 9, 71, 10, 10 }, 40);
            table1.Size = 11.0;
            table1.TextMarginLR = 5.0;

            p.AddTableText(table1, "Quantity", 0, 0);
            p.AddTableText(table1, "Description", 0, 1);
            p.AddTableText(table1, "Unit Price", 0, 2);
            p.AddTableText(table1, "Ext. Price", 0, 3);
            table1.Size = 8.0;
            table1.TextMarginTB = 2.0;

            #region insert table text 1 to 23 rows

            if (this.numericUpDownOrderCount1.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount1.Text, 1, 0);
            p.AddTableText(table1, this.textBoxOrderDescr1.Text, 1, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost1.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost1.Value), 1, 2);
            if (this.numericUpDownOrderECost1.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost1.Value), 1, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount2.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount2.Text, 2, 0);
            p.AddTableText(table1, this.textBoxOrderDescr2.Text, 2, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost2.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost2.Value), 2, 2);
            if (this.numericUpDownOrderECost2.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost2.Value), 2, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount3.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount3.Text, 3, 0);
            p.AddTableText(table1, this.textBoxOrderDescr3.Text, 3, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost3.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost3.Value), 3, 2);
            if (this.numericUpDownOrderECost3.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost3.Value), 3, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount4.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount4.Text, 4, 0);
            p.AddTableText(table1, this.textBoxOrderDescr4.Text, 4, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost4.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost4.Value), 4, 2);
            if (this.numericUpDownOrderECost4.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost4.Value), 4, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount5.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount5.Text, 5, 0);
            p.AddTableText(table1, this.textBoxOrderDescr5.Text, 5, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost5.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost5.Value), 5, 2);
            if (this.numericUpDownOrderECost5.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost5.Value), 5, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount6.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount6.Text, 6, 0);
            p.AddTableText(table1, this.textBoxOrderDescr6.Text, 6, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost6.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost6.Value), 6, 2);
            if (this.numericUpDownOrderECost6.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost6.Value), 6, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount7.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount7.Text, 7, 0);
            p.AddTableText(table1, this.textBoxOrderDescr7.Text, 7, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost7.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost7.Value), 7, 2);
            if (this.numericUpDownOrderECost7.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost7.Value), 7, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount8.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount8.Text, 8, 0);
            p.AddTableText(table1, this.textBoxOrderDescr8.Text, 8, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost8.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost8.Value), 8, 2);
            if (this.numericUpDownOrderECost8.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost8.Value), 8, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount9.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount9.Text, 9, 0);
            p.AddTableText(table1, this.textBoxOrderDescr9.Text, 9, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost9.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost9.Value), 9, 2);
            if (this.numericUpDownOrderECost9.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost9.Value), 9, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount10.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount10.Text, 10, 0);
            p.AddTableText(table1, this.textBoxOrderDescr10.Text, 10, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost10.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost10.Value), 10, 2);
            if (this.numericUpDownOrderECost10.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost10.Value), 10, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount11.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount11.Text, 11, 0);
            p.AddTableText(table1, this.textBoxOrderDescr11.Text, 11, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost11.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost11.Value), 11, 2);
            if (this.numericUpDownOrderECost11.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost11.Value), 11, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount12.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount12.Text, 12, 0);
            p.AddTableText(table1, this.textBoxOrderDescr12.Text, 12, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost12.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost12.Value), 12, 2);
            if (this.numericUpDownOrderECost12.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost12.Value), 12, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (rowCount > 13)
            {
                if (this.numericUpDownOrderCount13.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount13.Text, 13, 0);
                p.AddTableText(table1, this.textBoxOrderDescr13.Text, 13, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost13.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost13.Value), 13, 2);
                if (this.numericUpDownOrderECost13.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost13.Value), 13, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 14)
            {
                if (this.numericUpDownOrderCount14.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount14.Text, 14, 0);
                p.AddTableText(table1, this.textBoxOrderDescr14.Text, 14, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost14.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost14.Value), 14, 2);
                if (this.numericUpDownOrderECost14.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost14.Value), 14, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 15)
            {
                if (this.numericUpDownOrderCount15.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount15.Text, 15, 0);
                p.AddTableText(table1, this.textBoxOrderDescr15.Text, 15, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost15.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost15.Value), 15, 2);
                if (this.numericUpDownOrderECost15.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost15.Value), 15, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 16)
            {
                if (this.numericUpDownOrderCount16.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount16.Text, 16, 0);
                p.AddTableText(table1, this.textBoxOrderDescr16.Text, 16, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost16.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost16.Value), 16, 2);
                if (this.numericUpDownOrderECost16.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost16.Value), 16, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 17)
            {
                if (this.numericUpDownOrderCount17.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount17.Text, 17, 0);
                p.AddTableText(table1, this.textBoxOrderDescr17.Text, 17, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost17.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost17.Value), 17, 2);
                if (this.numericUpDownOrderECost17.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost17.Value), 17, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 18)
            {
                if (this.numericUpDownOrderCount18.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount18.Text, 18, 0);
                p.AddTableText(table1, this.textBoxOrderDescr18.Text, 18, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost18.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost18.Value), 18, 2);
                if (this.numericUpDownOrderECost18.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost18.Value), 18, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 19)
            {
                if (this.numericUpDownOrderCount19.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount19.Text, 19, 0);
                p.AddTableText(table1, this.textBoxOrderDescr19.Text, 19, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost19.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost19.Value), 19, 2);
                if (this.numericUpDownOrderECost19.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost19.Value), 19, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 20)
            {
                if (this.numericUpDownOrderCount20.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount20.Text, 20, 0);
                p.AddTableText(table1, this.textBoxOrderDescr20.Text, 20, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost20.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost20.Value), 20, 2);
                if (this.numericUpDownOrderECost20.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost20.Value), 20, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 21)
            {
                if (this.numericUpDownOrderCount21.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount21.Text, 21, 0);
                p.AddTableText(table1, this.textBoxOrderDescr21.Text, 21, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost21.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost21.Value), 21, 2);
                if (this.numericUpDownOrderECost21.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost21.Value), 21, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 22)
            {
                if (this.numericUpDownOrderCount22.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount22.Text, 22, 0);
                p.AddTableText(table1, this.textBoxOrderDescr22.Text, 22, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost22.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost22.Value), 22, 2);
                if (this.numericUpDownOrderECost22.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost22.Value), 22, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 23)
            {
                if (this.numericUpDownOrderCount23.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount23.Text, 23, 0);
                p.AddTableText(table1, this.textBoxOrderDescr23.Text, 23, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost23.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost23.Value), 23, 2);
                if (this.numericUpDownOrderECost23.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost23.Value), 23, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }

            #endregion

            pos += 5 + 105 + rowCount * 12.0;

            // PO notes
            Print2Pdf.TextInfo tiNotes = p.CreateTextInfo();
            tiNotes.Textstyle = Print2Pdf.TextStyle.Bold;
            tiNotes.Size = 12.0;
            p.AddText(tiNotes, "Please confirm receipt of this purchase order.", 40 + 110, pos, p.Width, 18);
            tiNotes.Size = 10.0;
            p.AddText(tiNotes, "NOTES:\n", 40, pos + 12, p.Width, 18);
            tiNotes.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiNotes, this.richTextBoxPONotes.Text);

            Print2Pdf.TextInfo tiBottomRight = p.CreateTextInfo();
            tiBottomRight.Textstyle = Print2Pdf.TextStyle.Bold;// | Print2Pdf.TextStyle.Underline;
            tiBottomRight.Size = 12.0;
            tiBottomRight.Textpos = Print2Pdf.TextPos.TopRight;
            p.AddText(tiBottomRight, "Total:    " + toCurrency(this.numericUpDownTotalCost.Value), p.Width / 4 * 3, pos, p.Width - 40, 18);

            //pos = tiNotes.Y1;

            pos = 645;

            // above footer
            Print2Pdf.TextInfo tiBottomLeft = p.CreateTextInfo();
            tiBottomLeft.Textstyle = Print2Pdf.TextStyle.Bold;
            tiBottomLeft.Size = 11.0;
            p.AddText(tiBottomLeft, "Unless indicated otherwise above all shipments are FOB SP, Freight PPY & Add, and standard vendor terms.", 40, pos + 12, p.Width, 18);

            // footer

            // signature?
            if (this.toolStripComboBoxSignature.SelectedIndex != 0)
            {
                Image sig = Properties.Resources.justin;
                double h = 0.0;

                if (this.toolStripComboBoxSignature.SelectedIndex == 1)
                    sig = Properties.Resources.justin;
                else if (this.toolStripComboBoxSignature.SelectedIndex == 2)
                    sig = Properties.Resources.roy;
                else if (this.toolStripComboBoxSignature.SelectedIndex == 3)
                    sig = Properties.Resources.scott;
                else
                {
                    sig = Properties.Resources.marsha2;
                    h = -15.0;
                }

                h += 200.0 * ((double)sig.Height / (double)sig.Width);
                p.DrawImage(sig, p.Width / 2 + 50.0, pos + 90 - h / 2, 200.0, h);
            }

            Print2Pdf.TextInfo tiFooter = p.CreateTextInfo();
            tiFooter.Textstyle = Print2Pdf.TextStyle.Bold;// | Print2Pdf.TextStyle.Underline;
            tiFooter.Size = 11.0;
            p.AddText(tiFooter, "Signature:___________________________________", p.Width / 2 + 4.0, pos + 88, p.Width, 18);
            tiFooter.Textstyle = Print2Pdf.TextStyle.Italic;
            tiFooter.Size = 8.0;
            p.AddText(tiFooter, todaysDate(), 60, pos + 88, p.Width, 18);
            p.AddText(tiFooter, "\nPage 1 of 1");

            string pdf = generatePDFileName("PO", this.textBoxPO.Text);
            p.Save(pdf);
            p.ShowPDF(pdf);

            #endregion
        }

        private void priceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Print Price Sheet

            Print2Pdf p = new Print2Pdf();

            double pos = addHeader(p, "Price Sheet") + 6.0;

            // Date: (left side)
            Print2Pdf.TextInfo tiTopLeft = p.CreateTextInfo();
            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopLeft.Size = 12.0;
            p.AddText(tiTopLeft, "Date:", 40, pos, p.Width, 18);
            p.AddText(tiTopLeft, "Customer:");
            p.AddText(tiTopLeft, "PO#:");
            p.AddText(tiTopLeft, "Project:");
            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopLeft, this.textBoxDate.Text, 40 + 55, pos, p.Width, 18);
            p.AddText(tiTopLeft, this.textBoxSoldTo.Text);
            p.AddText(tiTopLeft, this.textBoxCustomerPO.Text);
            p.AddText(tiTopLeft, this.textBoxJobName.Text);

            // POs Marks (right)
            Print2Pdf.TextInfo tiTopRight = p.CreateTextInfo();
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopRight.Size = 12.0;
            p.AddText(tiTopRight, "RMT #:", p.Width / 2 + 10, pos, p.Width, 18);
            p.AddText(tiTopRight, "Salesman:");
            p.AddText(tiTopRight, "Vendor Name:");
            p.AddText(tiTopRight, "Selling Price:");
            //p.AddText(tiTopRight, "Address:");
            p.AddText(tiTopRight, this.textBoxPO.Text, p.Width / 2 + 10 + 80, pos, p.Width, 18);
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopRight, this.comboBoxSalesAss.Text);
            p.AddText(tiTopRight, this.textBoxVendorName.Text);
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Bold | Print2Pdf.TextStyle.Underline;
            p.AddText(tiTopRight, toCurrency(this.numericUpDownQuotePrice.Value));
            //p.AddText(tiTopRight, this.textBoxShipTo.Text);
            //string streets = this.textBoxShipToStreet1.Text + "\n" + this.textBoxShipToStreet2.Text;
            //p.AddText(tiTopRight, streets.Trim() + "\n" + this.textBoxShipToCity.Text + ", " + this.comboBoxShipToState.Text + " " + this.textBoxShipToZip.Text);

            // Quoted Equip : Terms : Delivery
            p.DrawHorizontalLineDouble(1.0, pos + 70, 40.0);

            Print2Pdf.TextInfo tiDescr = p.CreateTextInfo();
            tiDescr.Textstyle = Print2Pdf.TextStyle.Bold;
            tiDescr.Size = 9.0;
            p.AddText(tiDescr, "Description:", 40, pos + 90, p.Width, 18);
            tiDescr.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiDescr, this.textBoxDescription.Text, 40 + 80, pos + 90, p.Width, 18);

            int rowCount = 13;

            #region how many rows used? min is 12 + one for header
            if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr23.Text))
            {
                rowCount = 24;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr22.Text))
            {
                rowCount = 23;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr21.Text))
            {
                rowCount = 22;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr20.Text))
            {
                rowCount = 21;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr19.Text))
            {
                rowCount = 20;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr18.Text))
            {
                rowCount = 19;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr17.Text))
            {
                rowCount = 18;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr16.Text))
            {
                rowCount = 17;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr15.Text))
            {
                rowCount = 16;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr14.Text))
            {
                rowCount = 15;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxOrderDescr13.Text))
            {
                rowCount = 14;
            }
            else
            {
                rowCount = 13;
            }
            #endregion

            Print2Pdf.TextInfo table1 = p.AddTable(pos + 110, rowCount, 12.0, new List<double> { 9, 71, 10, 10 }, 40);
            table1.Size = 11.0;
            table1.TextMarginLR = 5.0;

            p.AddTableText(table1, "Quantity", 0, 0);
            p.AddTableText(table1, "Description", 0, 1);
            p.AddTableText(table1, "Unit Price", 0, 2);
            p.AddTableText(table1, "Ext. Price", 0, 3);
            table1.Size = 8.0;
            table1.TextMarginTB = 2.0;

            #region insert table text 1 to 23 rows

            if (this.numericUpDownOrderCount1.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount1.Text, 1, 0);
            p.AddTableText(table1, this.textBoxOrderDescr1.Text, 1, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost1.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost1.Value), 1, 2);
            if (this.numericUpDownOrderECost1.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost1.Value), 1, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount2.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount2.Text, 2, 0);
            p.AddTableText(table1, this.textBoxOrderDescr2.Text, 2, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost2.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost2.Value), 2, 2);
            if (this.numericUpDownOrderECost2.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost2.Value), 2, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount3.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount3.Text, 3, 0);
            p.AddTableText(table1, this.textBoxOrderDescr3.Text, 3, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost3.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost3.Value), 3, 2);
            if (this.numericUpDownOrderECost3.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost3.Value), 3, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount4.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount4.Text, 4, 0);
            p.AddTableText(table1, this.textBoxOrderDescr4.Text, 4, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost4.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost4.Value), 4, 2);
            if (this.numericUpDownOrderECost4.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost4.Value), 4, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount5.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount5.Text, 5, 0);
            p.AddTableText(table1, this.textBoxOrderDescr5.Text, 5, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost5.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost5.Value), 5, 2);
            if (this.numericUpDownOrderECost5.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost5.Value), 5, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount6.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount6.Text, 6, 0);
            p.AddTableText(table1, this.textBoxOrderDescr6.Text, 6, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost6.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost6.Value), 6, 2);
            if (this.numericUpDownOrderECost6.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost6.Value), 6, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount7.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount7.Text, 7, 0);
            p.AddTableText(table1, this.textBoxOrderDescr7.Text, 7, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost7.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost7.Value), 7, 2);
            if (this.numericUpDownOrderECost7.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost7.Value), 7, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount8.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount8.Text, 8, 0);
            p.AddTableText(table1, this.textBoxOrderDescr8.Text, 8, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost8.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost8.Value), 8, 2);
            if (this.numericUpDownOrderECost8.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost8.Value), 8, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount9.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount9.Text, 9, 0);
            p.AddTableText(table1, this.textBoxOrderDescr9.Text, 9, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost9.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost9.Value), 9, 2);
            if (this.numericUpDownOrderECost9.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost9.Value), 9, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount10.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount10.Text, 10, 0);
            p.AddTableText(table1, this.textBoxOrderDescr10.Text, 10, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost10.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost10.Value), 10, 2);
            if (this.numericUpDownOrderECost10.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost10.Value), 10, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount11.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount11.Text, 11, 0);
            p.AddTableText(table1, this.textBoxOrderDescr11.Text, 11, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost11.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost11.Value), 11, 2);
            if (this.numericUpDownOrderECost11.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost11.Value), 11, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownOrderCount12.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount12.Text, 12, 0);
            p.AddTableText(table1, this.textBoxOrderDescr12.Text, 12, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownOrderCost12.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost12.Value), 12, 2);
            if (this.numericUpDownOrderECost12.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost12.Value), 12, 3);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (rowCount > 13)
            {
                if (this.numericUpDownOrderCount13.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount13.Text, 13, 0);
                p.AddTableText(table1, this.textBoxOrderDescr13.Text, 13, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost13.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost13.Value), 13, 2);
                if (this.numericUpDownOrderECost13.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost13.Value), 13, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 14)
            {
                if (this.numericUpDownOrderCount14.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount14.Text, 14, 0);
                p.AddTableText(table1, this.textBoxOrderDescr14.Text, 14, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost14.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost14.Value), 14, 2);
                if (this.numericUpDownOrderECost14.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost14.Value), 14, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 15)
            {
                if (this.numericUpDownOrderCount15.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount15.Text, 15, 0);
                p.AddTableText(table1, this.textBoxOrderDescr15.Text, 15, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost15.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost15.Value), 15, 2);
                if (this.numericUpDownOrderECost15.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost15.Value), 15, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 16)
            {
                if (this.numericUpDownOrderCount16.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount16.Text, 16, 0);
                p.AddTableText(table1, this.textBoxOrderDescr16.Text, 16, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost16.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost16.Value), 16, 2);
                if (this.numericUpDownOrderECost16.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost16.Value), 16, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 17)
            {
                if (this.numericUpDownOrderCount17.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount17.Text, 17, 0);
                p.AddTableText(table1, this.textBoxOrderDescr17.Text, 17, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost17.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost17.Value), 17, 2);
                if (this.numericUpDownOrderECost17.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost17.Value), 17, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 18)
            {
                if (this.numericUpDownOrderCount18.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount18.Text, 18, 0);
                p.AddTableText(table1, this.textBoxOrderDescr18.Text, 18, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost18.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost18.Value), 18, 2);
                if (this.numericUpDownOrderECost18.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost18.Value), 18, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 19)
            {
                if (this.numericUpDownOrderCount19.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount19.Text, 19, 0);
                p.AddTableText(table1, this.textBoxOrderDescr19.Text, 19, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost19.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost19.Value), 19, 2);
                if (this.numericUpDownOrderECost19.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost19.Value), 19, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 20)
            {
                if (this.numericUpDownOrderCount20.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount20.Text, 20, 0);
                p.AddTableText(table1, this.textBoxOrderDescr20.Text, 20, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost20.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost20.Value), 20, 2);
                if (this.numericUpDownOrderECost20.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost20.Value), 20, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 21)
            {
                if (this.numericUpDownOrderCount21.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount21.Text, 21, 0);
                p.AddTableText(table1, this.textBoxOrderDescr21.Text, 21, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost21.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost21.Value), 21, 2);
                if (this.numericUpDownOrderECost21.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost21.Value), 21, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 22)
            {
                if (this.numericUpDownOrderCount22.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount22.Text, 22, 0);
                p.AddTableText(table1, this.textBoxOrderDescr22.Text, 22, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost22.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost22.Value), 22, 2);
                if (this.numericUpDownOrderECost22.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost22.Value), 22, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 23)
            {
                if (this.numericUpDownOrderCount23.Value != 0) p.AddTableText(table1, "    " + this.numericUpDownOrderCount23.Text, 23, 0);
                p.AddTableText(table1, this.textBoxOrderDescr23.Text, 23, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownOrderCost23.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost23.Value), 23, 2);
                if (this.numericUpDownOrderECost23.Value != 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost23.Value), 23, 3);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }

            #endregion

            pos += 5 + 110 + rowCount * 12.0;

            // PO notes
            Print2Pdf.TextInfo tiNotes = p.CreateTextInfo();
            tiNotes.Textstyle = Print2Pdf.TextStyle.BoldItalic;
            tiNotes.Size = 10.0;
            p.AddText(tiNotes, "NOTES:\n", 40, pos, p.Width, 18);
            tiNotes.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiNotes, this.richTextBoxInvoiceInstructions.Text + "\n\n");
            p.AddText(tiNotes, this.richTextBoxInvoiceNotes.Text + "\n\n");
            p.AddText(tiNotes, this.richTextBoxVendorNotes.Text);


            Print2Pdf.TextInfo tiBottomRight = p.CreateTextInfo();
            tiBottomRight.Textstyle = Print2Pdf.TextStyle.Bold;// | Print2Pdf.TextStyle.Underline;
            tiBottomRight.Size = 12.0;
            p.AddText(tiBottomRight, "Total Cost:\n", p.Width / 4 * 3 - 20.0, pos, p.Width, 18);
            p.AddText(tiBottomRight, "Shop Time:\n");
            p.AddText(tiBottomRight, "Freight:\n");
            p.AddText(tiBottomRight, "PROFIT:");

            tiBottomRight.Textpos = Print2Pdf.TextPos.TopRight;
            p.AddText(tiBottomRight, toCurrency(this.numericUpDownTotalCost.Value) + "\n", p.Width / 4 * 3, pos, p.Width - 40.0, 18);
            p.AddText(tiBottomRight, toCurrency(this.numericUpDownShopTime.Value) + "\n");
            p.AddText(tiBottomRight, toCurrency(this.numericUpDownFreight.Value) + "\n");
            p.AddText(tiBottomRight, toCurrency(this.numericUpDownProfit.Value));


            //pos = tiNotes.Y1;

            pos = 645;

            // footer
            Print2Pdf.TextInfo tiFooter = p.CreateTextInfo();
            //tiFooter.Textstyle = Print2Pdf.TextStyle.Bold | Print2Pdf.TextStyle.Underline;
            //tiFooter.Size = 11.0;
            //p.AddText(tiFooter, "Signature:                                   _", p.Width / 3 * 2, pos + 88, p.Width, 18);
            tiFooter.Textstyle = Print2Pdf.TextStyle.Italic;
            tiFooter.Size = 8.0;
            p.AddText(tiFooter, todaysDate(), 60, pos + 88, p.Width, 18);
            p.AddText(tiFooter, "\nPage 1 of 1");

            string pdf = generatePDFileName("Price", this.textBoxPO.Text);
            p.Save(pdf);
            p.ShowPDF(pdf);

            #endregion
        }

        private void deliveryTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Print Delivery Ticket

            Print2Pdf p = new Print2Pdf();

            double pos = addHeader(p, "Delivery Receipt") + 6.0;

            // Date: (left side)
            Print2Pdf.TextInfo tiTopLeft = p.CreateTextInfo();
            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopLeft.Size = 12.0;
            p.AddText(tiTopLeft, "TO:", 40, pos, p.Width, 18);
            if (this.textBoxShipToStreet2.Text != "")
                p.AddText(tiTopLeft, "\n\n\n\nProject:");
            else
                p.AddText(tiTopLeft, "\n\n\nProject:");
            p.AddText(tiTopLeft, "PO:");
            p.AddText(tiTopLeft, "Date:");
            p.AddText(tiTopLeft, "Carrier:");


            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopLeft, this.textBoxShipTo.Text, 40 + 50, pos, p.Width, 18);
            string streets = this.textBoxShipToStreet1.Text + "\n" + this.textBoxShipToStreet2.Text;
            p.AddText(tiTopLeft, streets.Trim() + "\n" + this.textBoxShipToCity.Text + ", " + this.comboBoxShipToState.Text + " " + this.textBoxShipToZip.Text + "\n");

            //p.AddText(tiTopLeft, this.textBoxDate.Text, 20 + 80, 105, p.Width, 18);
            //p.AddText(tiTopLeft, this.textBoxVendorName.Text);
            p.AddText(tiTopLeft, this.textBoxJobName.Text);
            p.AddText(tiTopLeft, this.textBoxCustomerPO.Text);
            p.AddText(tiTopLeft, this.textBoxShipDate.Text);
            p.AddText(tiTopLeft, this.comboBoxCarrier.Text);

            // POs Marks (right)
            Print2Pdf.TextInfo tiTopRight = p.CreateTextInfo();
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopRight.Size = 12.0;
            p.AddText(tiTopRight, "RMT #:", p.Width / 2, pos, p.Width, 18);
            p.AddText(tiTopRight, "Salesman:");
            //p.AddText(tiTopRight, "Vendor Name:");
            //p.AddText(tiTopRight, "Selling Price:");
            //p.AddText(tiTopRight, "Address:");
            p.AddText(tiTopRight, this.textBoxPO.Text, p.Width / 2 + 80, pos, p.Width, 18);
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopRight, this.comboBoxSalesAss.Text);
            //p.AddText(tiTopRight, this.textBoxVendorName.Text);
            //tiTopRight.Textstyle = Print2Pdf.TextStyle.Bold | Print2Pdf.TextStyle.Underline;
            //p.AddText(tiTopRight, toCurrency(this.numericUpDownQuotePrice.Text);
            //p.AddText(tiTopRight, this.textBoxShipTo.Text);
            //string streets = this.textBoxShipToStreet1.Text + "\n" + this.textBoxShipToStreet2.Text;
            //p.AddText(tiTopRight, streets.Trim() + "\n" + this.textBoxShipToCity.Text + ", " + this.comboBoxShipToState.Text + " " + this.textBoxShipToZip.Text);

            // Quoted Equip : Terms : Delivery
            //p.DrawHorizontalLineDouble(1.0, 184, 20.0);

            Print2Pdf.TextInfo tiDescr = p.CreateTextInfo();
            tiDescr.Textstyle = Print2Pdf.TextStyle.Bold;
            tiDescr.Size = 9.0;
            p.AddText(tiDescr, "Description:", 40, pos + 110, p.Width, 18);
            tiDescr.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiDescr, this.textBoxDescription.Text, 40 + 50, pos + 110, p.Width, 18);

            int rowCount = 13;

            #region how many rows used? min is 12 + one for header
            if (this.numericUpDownOrderCount23.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr23.Text))
            {
                rowCount = 24;
            }
            else if (this.numericUpDownOrderCount22.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr22.Text))
            {
                rowCount = 23;
            }
            else if (this.numericUpDownOrderCount21.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr21.Text))
            {
                rowCount = 22;
            }
            else if (this.numericUpDownOrderCount20.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr20.Text))
            {
                rowCount = 21;
            }
            else if (this.numericUpDownOrderCount19.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr19.Text))
            {
                rowCount = 20;
            }
            else if (this.numericUpDownOrderCount18.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr18.Text))
            {
                rowCount = 19;
            }
            else if (this.numericUpDownOrderCount17.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr17.Text))
            {
                rowCount = 18;
            }
            else if (this.numericUpDownOrderCount16.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr16.Text))
            {
                rowCount = 17;
            }
            else if (this.numericUpDownOrderCount15.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr15.Text))
            {
                rowCount = 16;
            }
            else if (this.numericUpDownOrderCount14.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr14.Text))
            {
                rowCount = 15;
            }
            else if (this.numericUpDownOrderCount13.Value > 0 && !String.IsNullOrWhiteSpace(this.textBoxOrderDescr13.Text))
            {
                rowCount = 14;
            }
            else
            {
                rowCount = 13;
            }
            #endregion

            Print2Pdf.TextInfo table1 = p.AddTable(pos + 130, rowCount, 12.0, new List<double> { 10, 90 }, 40);
            table1.Size = 11.0;
            table1.TextMarginLR = 5.0;

            //Print2Pdf.TextInfo table1 = p.AddTable(pos + 110, rowCount, 12.0, new List<double> { 9, 71, 10, 10 }, 40);
            //table1.Size = 11.0;
            //table1.TextMarginLR = 5.0;

            p.AddTableText(table1, "Quantity", 0, 0);
            p.AddTableText(table1, "Description", 0, 1);
            //p.AddTableText(table1, "Unit Price", 0, 2);
            //p.AddTableText(table1, "Ext. Price", 0, 3);
            table1.Size = 8.0;
            table1.TextMarginTB = 2.0;

            #region insert table text 1 to 23 rows

            if (this.numericUpDownOrderCount1.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount1.Text, 1, 0);
                p.AddTableText(table1, this.textBoxOrderDescr1.Text, 1, 1);
            }
            //if (this.numericUpDownOrderCost1.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost1.Text, 1, 2);
            //if (this.numericUpDownOrderECost1.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost1.Text, 1, 3);

            if (this.numericUpDownOrderCount2.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount2.Text, 2, 0);
                p.AddTableText(table1, this.textBoxOrderDescr2.Text, 2, 1);
            }
            //if (this.numericUpDownOrderCost2.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost2.Text, 2, 2);
            //if (this.numericUpDownOrderECost2.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost2.Text, 2, 3);

            if (this.numericUpDownOrderCount3.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount3.Text, 3, 0);
                p.AddTableText(table1, this.textBoxOrderDescr3.Text, 3, 1);
            }
            //if (this.numericUpDownOrderCost3.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost3.Text, 3, 2);
            //if (this.numericUpDownOrderECost3.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost3.Text, 3, 3);

            if (this.numericUpDownOrderCount4.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount4.Text, 4, 0);
                p.AddTableText(table1, this.textBoxOrderDescr4.Text, 4, 1);
            }
            //if (this.numericUpDownOrderCost4.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost4.Text, 4, 2);
            //if (this.numericUpDownOrderECost4.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost4.Text, 4, 3);

            if (this.numericUpDownOrderCount5.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount5.Text, 5, 0);
                p.AddTableText(table1, this.textBoxOrderDescr5.Text, 5, 1);
            }
            //if (this.numericUpDownOrderCost5.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost5.Text, 5, 2);
            //if (this.numericUpDownOrderECost5.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost5.Text, 5, 3);

            if (this.numericUpDownOrderCount6.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount6.Text, 6, 0);
                p.AddTableText(table1, this.textBoxOrderDescr6.Text, 6, 1);
            }
            //if (this.numericUpDownOrderCost6.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost6.Text, 6, 2);
            //if (this.numericUpDownOrderECost6.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost6.Text, 6, 3);

            if (this.numericUpDownOrderCount7.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount7.Text, 7, 0);
                p.AddTableText(table1, this.textBoxOrderDescr7.Text, 7, 1);
            }
            //if (this.numericUpDownOrderCost7.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost7.Text, 7, 2);
            //if (this.numericUpDownOrderECost7.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost7.Text, 7, 3);

            if (this.numericUpDownOrderCount8.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount8.Text, 8, 0);
                p.AddTableText(table1, this.textBoxOrderDescr8.Text, 8, 1);
            }
            //if (this.numericUpDownOrderCost8.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost8.Text, 8, 2);
            //if (this.numericUpDownOrderECost8.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost8.Text, 8, 3);

            if (this.numericUpDownOrderCount9.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount9.Text, 9, 0);
                p.AddTableText(table1, this.textBoxOrderDescr9.Text, 9, 1);
            }
            //if (this.numericUpDownOrderCost9.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost9.Text, 9, 2);
            //if (this.numericUpDownOrderECost9.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost9.Text, 9, 3);

            if (this.numericUpDownOrderCount10.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount10.Text, 10, 0);
                p.AddTableText(table1, this.textBoxOrderDescr10.Text, 10, 1);
            }
            //if (this.numericUpDownOrderCost10.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost10.Text, 10, 2);
            //if (this.numericUpDownOrderECost10.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost10.Text, 10, 3);

            if (this.numericUpDownOrderCount11.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount11.Text, 11, 0);
                p.AddTableText(table1, this.textBoxOrderDescr11.Text, 11, 1);
            }
            //if (this.numericUpDownOrderCost11.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost11.Text, 11, 2);
            //if (this.numericUpDownOrderECost11.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost11.Text, 11, 3);

            if (this.numericUpDownOrderCount12.Value != 0)
            {
                p.AddTableText(table1, "    " + this.numericUpDownOrderCount12.Text, 12, 0);
                p.AddTableText(table1, this.textBoxOrderDescr12.Text, 12, 1);
            }
            //if (this.numericUpDownOrderCost12.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost12.Text, 12, 2);
            //if (this.numericUpDownOrderECost12.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost12.Text, 12, 3);

            if (rowCount > 13)
            {
                if (this.numericUpDownOrderCount13.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount13.Text, 13, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr13.Text, 13, 1);
                }
                //if (this.numericUpDownOrderCost13.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost13.Text, 13, 2);
                //if (this.numericUpDownOrderECost13.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost13.Text, 13, 3);
            }
            if (rowCount > 14)
            {
                if (this.numericUpDownOrderCount14.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount14.Text, 14, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr14.Text, 14, 1);
                }
                //if (this.numericUpDownOrderCost14.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost14.Text, 14, 2);
                //if (this.numericUpDownOrderECost14.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost14.Text, 14, 3);
            }
            if (rowCount > 15)
            {
                if (this.numericUpDownOrderCount15.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount15.Text, 15, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr15.Text, 15, 1);
                }
                //if (this.numericUpDownOrderCost15.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost15.Text, 15, 2);
                //if (this.numericUpDownOrderECost15.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost15.Text, 15, 3);
            }
            if (rowCount > 16)
            {
                if (this.numericUpDownOrderCount16.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount16.Text, 16, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr16.Text, 16, 1);
                }
                //if (this.numericUpDownOrderCost16.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost16.Text, 16, 2);
                //if (this.numericUpDownOrderECost16.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost16.Text, 16, 3);
            }
            if (rowCount > 17)
            {
                if (this.numericUpDownOrderCount17.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount17.Text, 17, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr17.Text, 17, 1);
                }
                //if (this.numericUpDownOrderCost17.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost17.Text, 17, 2);
                //if (this.numericUpDownOrderECost17.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost17.Text, 17, 3);
            }
            if (rowCount > 18)
            {
                if (this.numericUpDownOrderCount18.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount18.Text, 18, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr18.Text, 18, 1);
                }
                //if (this.numericUpDownOrderCost18.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost18.Text, 18, 2);
                //if (this.numericUpDownOrderECost18.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost18.Text, 18, 3);
            }
            if (rowCount > 19)
            {
                if (this.numericUpDownOrderCount19.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount19.Text, 19, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr19.Text, 19, 1);
                }
                //if (this.numericUpDownOrderCost19.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost19.Text, 19, 2);
                //if (this.numericUpDownOrderECost19.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost19.Text, 19, 3);
            }
            if (rowCount > 20)
            {
                if (this.numericUpDownOrderCount20.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount20.Text, 20, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr20.Text, 20, 1);
                }
                //if (this.numericUpDownOrderCost20.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost20.Text, 20, 2);
                //if (this.numericUpDownOrderECost20.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost20.Text, 20, 3);
            }
            if (rowCount > 21)
            {
                if (this.numericUpDownOrderCount21.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount21.Text, 21, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr21.Text, 21, 1);
                }
                //if (this.numericUpDownOrderCost21.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost21.Text, 21, 2);
                //if (this.numericUpDownOrderECost21.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost21.Text, 21, 3);
            }
            if (rowCount > 22)
            {
                if (this.numericUpDownOrderCount22.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount22.Text, 22, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr22.Text, 22, 1);
                }
                //if (this.numericUpDownOrderCost22.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost22.Text, 22, 2);
                //if (this.numericUpDownOrderECost22.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost22.Text, 22, 3);
            }
            if (rowCount > 23)
            {
                if (this.numericUpDownOrderCount23.Value != 0)
                {
                    p.AddTableText(table1, "    " + this.numericUpDownOrderCount23.Text, 23, 0);
                    p.AddTableText(table1, this.textBoxOrderDescr23.Text, 23, 1);
                }
                //if (this.numericUpDownOrderCost23.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderCost23.Text, 23, 2);
                //if (this.numericUpDownOrderECost23.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownOrderECost23.Text, 23, 3);
            }

            #endregion

            pos += 130 + 5 + rowCount * 12.0;

            // PO notes
            Print2Pdf.TextInfo tiNotes = p.CreateTextInfo();
            tiNotes.Textstyle = Print2Pdf.TextStyle.BoldItalic;
            tiNotes.Size = 10.0;
            p.AddText(tiNotes, "NOTES:\n", 40, pos, p.Width, 18);
            tiNotes.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiNotes, this.richTextBoxDeliveryNotes.Text);
            //p.AddText(tiNotes, this.richTextBoxInvoiceInstructions.Text);

            p.AddText(tiNotes, "(I have examined all equipment including pump electrical cables and all is in good condition.)\n\n", 40, 590, p.Width, 18);

            tiNotes.Textstyle = Print2Pdf.TextStyle.Bold;// | Print2Pdf.TextStyle.Underline;

            string grinder = this.textBoxGrinder.Text;
            string pumpNo = this.textBoxSerialNumber.Text;
            string stockNo = this.textBoxPumpStk.Text;

            //grinder = grinder.PadRight(140, ' ');
            //pumpNo = pumpNo.PadRight(140, ' ');
            //stockNo = stockNo.PadRight(140, ' ');

            //p.DrawHorizontalLine(1.0, tiNotes.Y1 + 10.0, 40.0);
            p.AddText(tiNotes, "Signature:_______________________       Printed Name:________________________        Date:____________\n\n");
            //p.DrawHorizontalLine(1.0, tiNotes.Y1 + 10.0, 40.0);
            //p.AddText(tiNotes, "Printed Name:\n");
            p.DrawHorizontalLine(0.8, tiNotes.Y1 + 10.0, 70 + 40.0);
            p.AddText(tiNotes, "Model #:             " + grinder + "\n");
            p.DrawHorizontalLine(0.8, tiNotes.Y1 + 10.0, 70 + 40.0);
            p.AddText(tiNotes, "Pump SN#:         " + pumpNo + "\n");
            p.DrawHorizontalLine(0.8, tiNotes.Y1 + 10.0, 70 + 40.0);
            p.AddText(tiNotes, "Stock Pump #:    " + stockNo + "\n");
            //p.DrawHorizontalLine(1.0, tiNotes.Y1 + 10.0, 40.0);
            //p.AddText(tiNotes, "Panel #:                                                                             _\n");
            //p.AddText(tiNotes, "Basin #:                                                                             _\n");

            //p.DrawHorizontalLine(1.0, 630.0, 40.0);

            pos = 645;

            // footer
            Print2Pdf.TextInfo tiFooter = p.CreateTextInfo();
            //tiFooter.Textstyle = Print2Pdf.TextStyle.Bold | Print2Pdf.TextStyle.Underline;
            //tiFooter.Size = 11.0;
            //p.AddText(tiFooter, "Signature:                                   _", p.Width / 3 * 2, pos + 88, p.Width, 18);
            tiFooter.Textstyle = Print2Pdf.TextStyle.Italic;
            tiFooter.Size = 8.0;
            p.AddText(tiFooter, todaysDate(), 60, pos + 88, p.Width, 18);
            p.AddText(tiFooter, "\nPage 1 of 1");

            string pdf = generatePDFileName("Delv", this.textBoxPO.Text);
            p.Save(pdf);
            p.ShowPDF(pdf);

            #endregion
        }

        private void printCurrentQuoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Print Quote

            Print2Pdf p = new Print2Pdf();

            double pos = addHeader(p, "Quotation") + 6.0;

            // TO:
            Print2Pdf.TextInfo tiTopLeft = p.CreateTextInfo();
            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopLeft.Size = 12.0;
            p.AddText(tiTopLeft, "To:", 40, pos, p.Width, 18);
            tiTopLeft.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopLeft, this.textBoxQTo.Text, 20 + 45, pos, p.Width, 18);
            p.AddText(tiTopLeft, this.textBoxQCompany.Text);
            string streets = this.textBoxQStreet1.Text + "\n" + this.textBoxQStreet2.Text;
            p.AddText(tiTopLeft, streets.Trim() + "\n" + this.textBoxQCity.Text + ", " + this.comboBoxQState.Text + " " + this.textBoxQZip.Text);

            // Date: Project: Location:
            Print2Pdf.TextInfo tiTopRight = p.CreateTextInfo();
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Bold;
            tiTopRight.Size = 12.0;
            p.AddText(tiTopRight, "Date:", p.Width / 2 - 56.0, pos, p.Width, 18);
            p.AddText(tiTopRight, "Project:");
            p.AddText(tiTopRight, "Location:");
            tiTopRight.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiTopRight, this.textBoxQDate.Text, p.Width / 2, pos, p.Width, 18);
            p.AddText(tiTopRight, this.textBoxQJobName.Text);
            p.AddText(tiTopRight, this.textBoxQLocation.Text);

            // Quoted Equip : Terms : Delivery
            p.DrawHorizontalLineDouble(1.0, pos + 64, 40.0);

            Print2Pdf.TextInfo tiQuoteEq = p.CreateTextInfo();
            tiQuoteEq.Textstyle = Print2Pdf.TextStyle.Bold;
            tiQuoteEq.Size = 9.0;
            p.AddText(tiQuoteEq, "Quoted Equipment:", 40, pos + 68, p.Width, 18);
            tiQuoteEq.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiQuoteEq, this.textBoxQEquipment.Text, 40 + 80, pos + 68, p.Width, 18);

            tiQuoteEq.Textstyle = Print2Pdf.TextStyle.Bold;
            p.AddText(tiQuoteEq, "Terms:", p.Width / 2 - 10, pos + 68, p.Width, 18);
            tiQuoteEq.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiQuoteEq, this.comboBoxQTerms.Text, p.Width / 2 + 20, pos + 68, p.Width, 18);

            tiQuoteEq.Textstyle = Print2Pdf.TextStyle.Bold;
            p.AddText(tiQuoteEq, "Delivery:", p.Width - 120 - 6, pos + 68, p.Width, 18);
            tiQuoteEq.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiQuoteEq, this.comboBoxQDelivery.Text, p.Width - 120 + 40 - 6, pos + 68, p.Width, 18);

            p.DrawHorizontalLineDouble(1.0, pos + 80, 40.0);

            // header description
            Print2Pdf.TextInfo tiHeader = p.CreateTextInfo();
            tiHeader.Textstyle = Print2Pdf.TextStyle.Bold;
            tiHeader.Size = 12.0;
            p.AddText(tiTopLeft, this.textBoxQHeader.Text, 40, pos + 85, p.Width, 18);

            int rowCount = 13;

            #region how many rows used? min is 12 + one for header
            if (!String.IsNullOrWhiteSpace(this.textBoxQDescription23.Text))
            {
                rowCount = 24;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription22.Text))
            {
                rowCount = 23;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription21.Text))
            {
                rowCount = 22;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription20.Text))
            {
                rowCount = 21;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription19.Text))
            {
                rowCount = 20;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription18.Text))
            {
                rowCount = 19;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription17.Text))
            {
                rowCount = 18;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription16.Text))
            {
                rowCount = 17;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription15.Text))
            {
                rowCount = 16;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription14.Text))
            {
                rowCount = 15;
            }
            else if (!String.IsNullOrWhiteSpace(this.textBoxQDescription13.Text))
            {
                rowCount = 14;
            }
            else
            {
                rowCount = 13;
            }
            #endregion

            bool showPrices = false;
            if (this.numericUpDownQMCost1.Value > 0 || this.numericUpDownQMCost13.Value > 0 ||
                this.numericUpDownQMCost2.Value > 0 || this.numericUpDownQMCost14.Value > 0 ||
                this.numericUpDownQMCost3.Value > 0 || this.numericUpDownQMCost15.Value > 0 ||
                this.numericUpDownQMCost4.Value > 0 || this.numericUpDownQMCost16.Value > 0 ||
                this.numericUpDownQMCost5.Value > 0 || this.numericUpDownQMCost17.Value > 0 ||
                this.numericUpDownQMCost6.Value > 0 || this.numericUpDownQMCost18.Value > 0 ||
                this.numericUpDownQMCost7.Value > 0 || this.numericUpDownQMCost19.Value > 0 ||
                this.numericUpDownQMCost8.Value > 0 || this.numericUpDownQMCost20.Value > 0 ||
                this.numericUpDownQMCost9.Value > 0 || this.numericUpDownQMCost21.Value > 0 ||
                this.numericUpDownQMCost10.Value > 0 || this.numericUpDownQMCost22.Value > 0 ||
                this.numericUpDownQMCost11.Value > 0 || this.numericUpDownQMCost23.Value > 0 ||
                this.numericUpDownQMCost12.Value > 0)
                showPrices = true;

            Print2Pdf.TextInfo table1 = p.AddTable(pos + 100, rowCount, 12.0, (showPrices) ? new List<double> { 10, 80, 10 } : new List<double> { 10, 90}, 40);
            table1.Size = 11.0;
            table1.TextMarginLR = 5.0;

            p.AddTableText(table1, "Quantity", 0, 0);
            p.AddTableText(table1, "Description", 0, 1);
            if (showPrices)
                p.AddTableText(table1, "Price", 0, 2);
            table1.Size = 8.0;
            table1.TextMarginTB = 2.0;

            #region insert table text 1 to 23 rows

            if (this.numericUpDownQQuan1.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan1.Text, 1, 0);
            p.AddTableText(table1, this.textBoxQDescription1.Text, 1, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost1.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost1.Value), 1, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan2.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan2.Text, 2, 0);
            p.AddTableText(table1, this.textBoxQDescription2.Text, 2, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost2.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost2.Value), 2, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan3.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan3.Text, 3, 0);
            p.AddTableText(table1, this.textBoxQDescription3.Text, 3, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost3.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost3.Value), 3, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan4.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan4.Text, 4, 0);
            p.AddTableText(table1, this.textBoxQDescription4.Text, 4, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost4.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost4.Value), 4, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan5.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan5.Text, 5, 0);
            p.AddTableText(table1, this.textBoxQDescription5.Text, 5, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost5.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost5.Value), 5, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan6.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan6.Text, 6, 0);
            p.AddTableText(table1, this.textBoxQDescription6.Text, 6, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost6.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost6.Value), 6, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan7.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan7.Text, 7, 0);
            p.AddTableText(table1, this.textBoxQDescription7.Text, 7, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost7.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost7.Value), 7, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan8.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan8.Text, 8, 0);
            p.AddTableText(table1, this.textBoxQDescription8.Text, 8, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost8.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost8.Value), 8, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan9.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan9.Text, 9, 0);
            p.AddTableText(table1, this.textBoxQDescription9.Text, 9, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost9.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost9.Value), 9, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan10.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan10.Text, 10, 0);
            p.AddTableText(table1, this.textBoxQDescription10.Text, 10, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost10.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost10.Value), 10, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan11.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan11.Text, 11, 0);
            p.AddTableText(table1, this.textBoxQDescription11.Text, 11, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost11.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost11.Value), 11, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (this.numericUpDownQQuan12.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan12.Text, 12, 0);
            p.AddTableText(table1, this.textBoxQDescription12.Text, 12, 1);
            table1.Textpos = Print2Pdf.TextPos.TopRight;
            if (this.numericUpDownQMCost12.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost12.Value), 12, 2);
            table1.Textpos = Print2Pdf.TextPos.TopLeft;

            if (rowCount > 13)
            {
                if (this.numericUpDownQQuan13.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan13.Text, 13, 0);
                p.AddTableText(table1, this.textBoxQDescription13.Text, 13, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost13.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost13.Value), 13, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 14)
            {
                if (this.numericUpDownQQuan14.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan14.Text, 14, 0);
                p.AddTableText(table1, this.textBoxQDescription14.Text, 14, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost14.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost14.Value), 14, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 15)
            {
                if (this.numericUpDownQQuan15.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan15.Text, 15, 0);
                p.AddTableText(table1, this.textBoxQDescription15.Text, 15, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost15.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost15.Value), 15, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 16)
            {
                if (this.numericUpDownQQuan16.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan16.Text, 16, 0);
                p.AddTableText(table1, this.textBoxQDescription16.Text, 16, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost16.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost16.Value), 16, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 17)
            {
                if (this.numericUpDownQQuan17.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan17.Text, 17, 0);
                p.AddTableText(table1, this.textBoxQDescription17.Text, 17, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost17.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost17.Value), 17, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 18)
            {
                if (this.numericUpDownQQuan18.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan18.Text, 18, 0);
                p.AddTableText(table1, this.textBoxQDescription18.Text, 18, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost18.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost18.Value), 18, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 19)
            {
                if (this.numericUpDownQQuan19.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan19.Text, 19, 0);
                p.AddTableText(table1, this.textBoxQDescription19.Text, 19, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost19.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost19.Value), 19, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 20)
            {
                if (this.numericUpDownQQuan20.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan20.Text, 20, 0);
                p.AddTableText(table1, this.textBoxQDescription20.Text, 20, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost20.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost20.Value), 20, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 21)
            {
                if (this.numericUpDownQQuan21.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan21.Text, 21, 0);
                p.AddTableText(table1, this.textBoxQDescription21.Text, 21, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost21.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost21.Value), 21, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 22)
            {
                if (this.numericUpDownQQuan22.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan22.Text, 22, 0);
                p.AddTableText(table1, this.textBoxQDescription22.Text, 22, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost22.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost22.Value), 22, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }
            if (rowCount > 23)
            {
                if (this.numericUpDownQQuan23.Value > 0) p.AddTableText(table1, "    " + this.numericUpDownQQuan23.Text, 23, 0);
                p.AddTableText(table1, this.textBoxQDescription23.Text, 23, 1);
                table1.Textpos = Print2Pdf.TextPos.TopRight;
                if (this.numericUpDownQMCost23.Value > 0) p.AddTableText(table1, toCurrency(this.numericUpDownQMCost23.Value), 23, 2);
                table1.Textpos = Print2Pdf.TextPos.TopLeft;
            }

            #endregion

            pos = table1.Y2 + 35;
            //5 + 220 + rowCount * 12.0;

            // quote notes
            Print2Pdf.TextInfo tiNotes = p.CreateTextInfo();
            tiNotes.Textstyle = Print2Pdf.TextStyle.Bold;
            tiNotes.Size = 8.0;
            p.AddText(tiNotes, "NOTES:\n\n", 40, pos, p.Width, 18);
            //p.AddText(tiNotes, "1. If Ordered, please sign this quotation form and FAX or mail back to R. M. Tugwell.\n");
            //tiNotes.Textstyle = Print2Pdf.TextStyle.Regular;
            //p.AddText(tiNotes, "2. Applicable sales tax not included.\n");
            if (this.richTextBoxQQuoteNotes.Text != "")
                p.AddText(tiNotes, this.richTextBoxQQuoteNotes.Text + "\n");

            pos = table1.Y2 + 5;// tiNotes.Y1;
            

            // NET & price
            if (this.comboBoxFreightSelect.Text != "")
            {
                Print2Pdf.TextInfo tiNetPrice = p.CreateTextInfo();
                tiNetPrice.Textstyle = Print2Pdf.TextStyle.Bold;
                tiNetPrice.Size = 13.0;

                //int totalWidth = 96;// -this.comboBoxFreightSelect.Text.Length;
                //string net = this.comboBoxFreightSelect.Text.PadRight(totalWidth, '.');

                p.AddText(tiNetPrice, this.comboBoxFreightSelect.Text, 40, pos, p.Width, 18);
                p.AddText(tiNetPrice, toCurrency(this.numericUpDownQQuotePrice.Value), p.Width / 3 * 2 + 100 + 8, pos, p.Width, 18);
            }

            pos = 590;

            //p.DrawHorizontalLine(0.5, pos + 8, 20.0);

            // salesman
            Print2Pdf.TextInfo tiBottomLeft = p.CreateTextInfo();
            tiBottomLeft.Textstyle = Print2Pdf.TextStyle.Bold;
            tiBottomLeft.Size = 9.0;
            p.AddText(tiBottomLeft, "This Quotation Prepared By:", 40, pos, p.Width, 18);
            //tiBottomLeft.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiBottomLeft, this.comboBoxQSalesAss.Text, 40 + 15 + 130, pos, p.Width, 18);

            // terms bottom right
            Print2Pdf.TextInfo tiBottomRight = p.CreateTextInfo();
            tiBottomRight.Textstyle = Print2Pdf.TextStyle.Bold;
            tiBottomRight.Size = 9.0;
            p.AddText(tiBottomRight, "The undersigned agrees to and has the authority to bind the\npurchaser to the terms and conditions and equipment stated\nabove.",
                p.Width / 2 + 15, pos, p.Width, 18);

            // signature lines
            //p.DrawHorizontalLine(0.5, pos + 55, 20.0);

            // signature?
            if (this.toolStripComboBoxSignature.SelectedIndex != 0)
            {
                Image sig = Properties.Resources.justin;
                double h = 0.0;

                if (this.toolStripComboBoxSignature.SelectedIndex == 1)
                    sig = Properties.Resources.justin;
                else if (this.toolStripComboBoxSignature.SelectedIndex == 2)
                    sig = Properties.Resources.roy;
                else if (this.toolStripComboBoxSignature.SelectedIndex == 3)
                    sig = Properties.Resources.scott;
                else
                {
                    sig = Properties.Resources.marsha2;
                    h = -25.0;
                }

                h += 200.0 * ((double)sig.Height / (double)sig.Width);
                p.DrawImage(sig, 25.0, pos + 60 - h / 2, 200.0, h);
            }

            Print2Pdf.TextInfo tiSig = p.CreateTextInfo();
            tiSig.Size = 8.0;
            p.AddText(tiSig, "__________________________________________                                                        ________________________________________________________", 40, pos + 48 + 10, p.Width, 18);

            tiSig.Textstyle = Print2Pdf.TextStyle.Bold;
            
            p.AddText(tiSig, "For R.M. Tugwell & Associates, Inc.", 40, pos + 58 + 10, p.Width, 18);
            p.AddText(tiSig, "For: " + this.textBoxQCompany.Text, p.Width / 2 + 15, pos + 58 + 10, p.Width, 18);
            p.AddText(tiSig, "Date:", p.Width / 2 + 15 + 145, pos + 58 + 10, p.Width, 18);

            // footer
            Print2Pdf.TextInfo tiFooter = p.CreateTextInfo();
            tiFooter.Textstyle = Print2Pdf.TextStyle.Italic;
            tiFooter.Size = 8.0;
            p.AddText(tiFooter, "Quotation good for 30 days.  Prices do not include any applicable taxes.  Payment terms are NET 30 days from date of shipment.  Past due accounts\nwill be charged interest at 1.5% per month.  Should the services of an attorney, collection agency or other legal service become necessary for\ncollection, purchaser will assume responsibilty for all expenses accrued in the collection process including fees, court cost, serving charges, lien filing,\netc.  Manufacturer's warranty applies.  R. M. Tugwell & Assoc., Inc. assumes no liability whatsoever for delays or damages caused by defects or any\nother equipment failure.",
                60, pos + 88 + 30, p.Width, 18);

            string pdf = generatePDFileName("Quote", this.textBoxQPO.Text);
            p.Save(pdf);
            p.ShowPDF(pdf);

            #endregion
        }

        private double addHeader(Print2Pdf p, string title)
        {
            #region Header for all

            Print2Pdf.TextInfo tiBigCentered = p.CreateTextInfo();
            tiBigCentered.Size = 22.0;
            tiBigCentered.Textpos = Print2Pdf.TextPos.Center;
            //tiBigCentered.Textstyle = Print2Pdf.TextStyle.Bold | Print2Pdf.TextStyle.Underline;

            //p.AddText(tiBigCentered, title, 0, 15 + 5, p.Width, 28);

            tiBigCentered.Textstyle = Print2Pdf.TextStyle.Bold;
            p.AddText(tiBigCentered, title, 0, 32, p.Width, 28);

            tiBigCentered.Size = 15.0;
            //tiBigCentered.Textstyle = Print2Pdf.TextStyle.Bold;
            p.AddText(tiBigCentered, "R. M. Tugwell & Associates, Inc.", 0, 58 + 10, p.Width, 28);
            tiBigCentered.Size = 9.0;
            tiBigCentered.Textstyle = Print2Pdf.TextStyle.Regular;

            p.AddText(tiBigCentered, "1014 Creighton Road", 0, 82 + 10, p.Width, 10);
            tiBigCentered.Y1 -= 3.0;
            p.AddText(tiBigCentered, "Pensacola, FL 32504");
            tiBigCentered.Y1 -= 3.0;
            p.AddText(tiBigCentered, "Voice: (850) 477-1200, Fax: (850) 477-4366");
            //p.DrawHorizontalLine(1.0, 10 + 33, 40.0);
            p.DrawHorizontalLineDouble(1.0, 124, 40.0);

            return 124 + 2;

            #endregion
        }

        private string toCurrency(Decimal value)
        {
            return value.ToString("$ #,##0.00");
        }

        // new sales orders
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyOrders)
                updateRowOrders();

            string nextPO = generateNextOrderPO();

            helperCreateNewOrderRow(nextPO, false);
            _OrderTotal++;
        }

        // new stock order T
        private void newStockOrderTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyOrders)
                updateRowOrders();

            string nextPO = generateNextOrderPO() + "T";

            helperCreateNewOrderRow(nextPO, false);
            _OrderTotal++;
        }

        // new warranty order W
        private void newWarrantyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyOrders)
                updateRowOrders();

            string nextPO = generateNextOrderPO() + "W";

            helperCreateNewOrderRow(nextPO, false);
            _OrderTotal++;
        }

        // new quotes
        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.isDataDirtyQuotes)
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

            this.checkBoxOK.Checked = true; // actually when it is ok, default ok
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

            isDataLoadingOrders = false;

            #endregion

            insertNewDataRowOrders(nextPO);

            readOrderAndUpdateGUI(nextPO, 0);
            _OrderTotal++;

            //int count = getRowCountsFromOrders();
            //this._currentRowOrders = count;

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
                
            refreshRecordIndicatorOrders();
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

            int count = getRowCountsFromQuotes();
            this._currentRowQuotes = count;

            refreshRecordIndicatorQuotes();
        }

        private void nextCurrentLetterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string currentPO = this.textBoxPO.Text;
            List<string> POs = getPOsFromOrders();

            if (POs.Count == 0)
                return;

            if (this.isDataDirtyOrders)
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

            _OrderTotal = 0;

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

        private void removeRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isOrdersSelected)
            {
                if (this.isDataDirtyOrders)
                    updateRowOrders();

                int count = getRowCountsFromOrders();

                if (count == 0)
                    return;

                if (askImportantQuestion("This will delete record " + this.textBoxPO.Text + " from the database. Are you sure?") == System.Windows.Forms.DialogResult.No)
                    return;

                deletePOFromOrders(this.textBoxPO.Text);
                _OrderTotal--;

                count--;

                if (this._currentRowOrders > 1)
                    this._currentRowOrders--;

                List<SortableRow> sorted = buildSortedRows();
                readOrderAndUpdateGUI(sorted[this._currentRowOrders - 1].PO, 0);
                //readOrderAndUpdateGUI("", this._currentRowOrders);

                refreshRecordIndicatorOrders();
            }
            else
            {
                if (this.isDataDirtyQuotes)
                    updateRowQuotes();

                int count = getRowCountsFromQuotes();

                if (count == 0)
                    return;

                if (askImportantQuestion("This will delete record " + this.textBoxQPO.Text + " from the database. Are you sure?") == System.Windows.Forms.DialogResult.No)
                    return;

                deletePOFromQuotes(this.textBoxQPO.Text);

                count--;

                if (this._currentRowQuotes > 1)
                    this._currentRowQuotes--;

                readQuoteAndUpdateGUI("", this._currentRowQuotes);

                refreshRecordIndicatorQuotes();
            }
        }

        private void cleanDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vacuumDatabase();
            _OrderTotal = 0;

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

        private bool readRowOrders(string PO, int rowCount, out string thePO,
            out string Date, out string EndUser, out string Equipment, out string VendorName, out string JobNumber, out string CustomerPO,
            out string VendorNumber, out string SalesAss, out string SoldTo, out string Street1, out string Street2, out string City, out string State, out string Zip,
            out string ShipTo, out string ShipStreet1, out string ShipStreet2, out string ShipCity, out string ShipState, out string ShipZip,
            out string Carrier, out string ShipDate, out string IsComOrder, out string IsComPaid, out string Grinder, out string SerialNo, out string PumpStk,
            out string ReqDate, out string SchedShip, out string PODate, out string POShipVia,
            out string TrackDate1, out string TrackBy1, out string TrackSource1, out string TrackNote1,
            out string TrackDate2, out string TrackBy2, out string TrackSource2, out string TrackNote2,
            out string TrackDate3, out string TrackBy3, out string TrackSource3, out string TrackNote3,
            out string TrackDate4, out string TrackBy4, out string TrackSource4, out string TrackNote4,
            out string TrackDate5, out string TrackBy5, out string TrackSource5, out string TrackNote5,
            out string TrackDate6, out string TrackBy6, out string TrackSource6, out string TrackNote6,
            out string TrackDate7, out string TrackBy7, out string TrackSource7, out string TrackNote7,
            out string TrackDate8, out string TrackBy8, out string TrackSource8, out string TrackNote8,
            out string TrackDate9, out string TrackBy9, out string TrackSource9, out string TrackNote9,
            out string TrackDate10, out string TrackBy10, out string TrackSource10, out string TrackNote10,
            out string TrackDate11, out string TrackBy11, out string TrackSource11, out string TrackNote11,
            out string TrackDate12, out string TrackBy12, out string TrackSource12, out string TrackNote12,
            out string TrackDate13, out string TrackBy13, out string TrackSource13, out string TrackNote13,
            out string TrackDate14, out string TrackBy14, out string TrackSource14, out string TrackNote14,
            out string TrackDate15, out string TrackBy15, out string TrackSource15, out string TrackNote15,
            out string TrackDate16, out string TrackBy16, out string TrackSource16, out string TrackNote16,
            out string TrackDate17, out string TrackBy17, out string TrackSource17, out string TrackNote17,
            out string TrackDate18, out string TrackBy18, out string TrackSource18, out string TrackNote18,
            out string QuotePrice, out string Credit, out string Freight, out string ShopTime, out string TotalCost, out string GrossProfit, out string Profit,
            out string Description,
            out string Quant1, out string Descr1, out string Costs1, out string ECost1,
            out string Quant2, out string Descr2, out string Costs2, out string ECost2,
            out string Quant3, out string Descr3, out string Costs3, out string ECost3,
            out string Quant4, out string Descr4, out string Costs4, out string ECost4,
            out string Quant5, out string Descr5, out string Costs5, out string ECost5,
            out string Quant6, out string Descr6, out string Costs6, out string ECost6,
            out string Quant7, out string Descr7, out string Costs7, out string ECost7,
            out string Quant8, out string Descr8, out string Costs8, out string ECost8,
            out string Quant9, out string Descr9, out string Costs9, out string ECost9,
            out string Quant10, out string Descr10, out string Costs10, out string ECost10,
            out string Quant11, out string Descr11, out string Costs11, out string ECost11,
            out string Quant12, out string Descr12, out string Costs12, out string ECost12,
            out string Quant13, out string Descr13, out string Costs13, out string ECost13,
            out string Quant14, out string Descr14, out string Costs14, out string ECost14,
            out string Quant15, out string Descr15, out string Costs15, out string ECost15,
            out string Quant16, out string Descr16, out string Costs16, out string ECost16,
            out string Quant17, out string Descr17, out string Costs17, out string ECost17,
            out string Quant18, out string Descr18, out string Costs18, out string ECost18,
            out string Quant19, out string Descr19, out string Costs19, out string ECost19,
            out string Quant20, out string Descr20, out string Costs20, out string ECost20,
            out string Quant21, out string Descr21, out string Costs21, out string ECost21,
            out string Quant22, out string Descr22, out string Costs22, out string ECost22,
            out string Quant23, out string Descr23, out string Costs23, out string ECost23,
            out string InvInstructions, out string InvNotes, out string VendorNotes, out string AccNotes, out string CrMemo, out string InvNumber, out string InvDate, out string Status,
            out string CheckNumbers, out string CheckDates,
            out string ComDate1, out string ComCheckNumber1, out string ComPaid1,
            out string ComDate2, out string ComCheckNumber2, out string ComPaid2,
            out string ComDate3, out string ComCheckNumber3, out string ComPaid3,
            out string ComDate4, out string ComCheckNumber4, out string ComPaid4,
            out string ComDate5, out string ComCheckNumber5, out string ComPaid5,
            out string ComAmount, out string ComBalance, out string DeliveryNotes, out string PONotes, out string IsOk)
        {
            _log.append("readRowOrders start");

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


                                //_log.append("readRowOrders reads start");
                                #region reads as strings

                                thePO = rrr["PO"] as string;

                                Date = rrr["Date"] as string;
                                EndUser = rrr["EndUser"] as string;

                                Equipment = rrr["Equipment"] as string;
                                VendorName = rrr["VendorName"] as string;
                                JobNumber = rrr["JobNumber"] as string;
                                CustomerPO = rrr["CustomerPO"] as string;
                                VendorNumber = rrr["VendorNumber"] as string;
                                SalesAss = rrr["SalesAss"] as string;
                                SoldTo = rrr["SoldTo"] as string;
                                Street1 = rrr["Street1"] as string;
                                Street2 = rrr["Street2"] as string;
                                City = rrr["City"] as string;
                                State = rrr["State"] as string;
                                Zip = rrr["Zip"] as string;
                                ShipTo = rrr["ShipTo"] as string;
                                ShipStreet1 = rrr["ShipStreet1"] as string;
                                ShipStreet2 = rrr["ShipStreet2"] as string;
                                ShipCity = rrr["ShipCity"] as string;
                                ShipState = rrr["ShipState"] as string;
                                ShipZip = rrr["ShipZip"] as string;
                                Carrier = rrr["Carrier"] as string;
                                ShipDate = rrr["ShipDate"] as string;
                                IsComOrder = rrr["IsComOrder"] as string;
                                IsComPaid = rrr["IsComPaid"] as string;
                                Grinder = rrr["Grinder"] as string;
                                SerialNo = rrr["SerialNo"] as string;
                                PumpStk = rrr["PumpStk"] as string;
                                ReqDate = rrr["ReqDate"] as string;
                                SchedShip = rrr["SchedShip"] as string;
                                PODate = rrr["PODate"] as string;
                                POShipVia = rrr["POShipVia"] as string;

                                TrackDate1 = rrr["TrackDate1"] as string;
                                TrackBy1 = rrr["TrackBy1"] as string;
                                TrackSource1 = rrr["TrackSource1"] as string;
                                TrackNote1 = rrr["TrackNote1"] as string;
                                TrackDate2 = rrr["TrackDate2"] as string;
                                TrackBy2 = rrr["TrackBy2"] as string;
                                TrackSource2 = rrr["TrackSource2"] as string;
                                TrackNote2 = rrr["TrackNote2"] as string;
                                TrackDate3 = rrr["TrackDate3"] as string;
                                TrackBy3 = rrr["TrackBy3"] as string;
                                TrackSource3 = rrr["TrackSource3"] as string;
                                TrackNote3 = rrr["TrackNote3"] as string;
                                TrackDate4 = rrr["TrackDate4"] as string;
                                TrackBy4 = rrr["TrackBy4"] as string;
                                TrackSource4 = rrr["TrackSource4"] as string;
                                TrackNote4 = rrr["TrackNote4"] as string;
                                TrackDate5 = rrr["TrackDate5"] as string;
                                TrackBy5 = rrr["TrackBy5"] as string;
                                TrackSource5 = rrr["TrackSource5"] as string;
                                TrackNote5 = rrr["TrackNote5"] as string;
                                TrackDate6 = rrr["TrackDate6"] as string;
                                TrackBy6 = rrr["TrackBy6"] as string;
                                TrackSource6 = rrr["TrackSource6"] as string;
                                TrackNote6 = rrr["TrackNote6"] as string;
                                TrackDate7 = rrr["TrackDate7"] as string;
                                TrackBy7 = rrr["TrackBy7"] as string;
                                TrackSource7 = rrr["TrackSource7"] as string;
                                TrackNote7 = rrr["TrackNote7"] as string;
                                TrackDate8 = rrr["TrackDate8"] as string;
                                TrackBy8 = rrr["TrackBy8"] as string;
                                TrackSource8 = rrr["TrackSource8"] as string;
                                TrackNote8 = rrr["TrackNote8"] as string;
                                TrackDate9 = rrr["TrackDate9"] as string;
                                TrackBy9 = rrr["TrackBy9"] as string;
                                TrackSource9 = rrr["TrackSource9"] as string;
                                TrackNote9 = rrr["TrackNote9"] as string;
                                TrackDate10 = rrr["TrackDate10"] as string;
                                TrackBy10 = rrr["TrackBy10"] as string;
                                TrackSource10 = rrr["TrackSource10"] as string;
                                TrackNote10 = rrr["TrackNote10"] as string;
                                TrackDate11 = rrr["TrackDate11"] as string;
                                TrackBy11 = rrr["TrackBy11"] as string;
                                TrackSource11 = rrr["TrackSource11"] as string;
                                TrackNote11 = rrr["TrackNote11"] as string;
                                TrackDate12 = rrr["TrackDate12"] as string;
                                TrackBy12 = rrr["TrackBy12"] as string;
                                TrackSource12 = rrr["TrackSource12"] as string;
                                TrackNote12 = rrr["TrackNote12"] as string;
                                TrackDate13 = rrr["TrackDate13"] as string;
                                TrackBy13 = rrr["TrackBy13"] as string;
                                TrackSource13 = rrr["TrackSource13"] as string;
                                TrackNote13 = rrr["TrackNote13"] as string;
                                TrackDate14 = rrr["TrackDate14"] as string;
                                TrackBy14 = rrr["TrackBy14"] as string;
                                TrackSource14 = rrr["TrackSource14"] as string;
                                TrackNote14 = rrr["TrackNote14"] as string;
                                TrackDate15 = rrr["TrackDate15"] as string;
                                TrackBy15 = rrr["TrackBy15"] as string;
                                TrackSource15 = rrr["TrackSource15"] as string;
                                TrackNote15 = rrr["TrackNote15"] as string;
                                TrackDate16 = rrr["TrackDate16"] as string;
                                TrackBy16 = rrr["TrackBy16"] as string;
                                TrackSource16 = rrr["TrackSource16"] as string;
                                TrackNote16 = rrr["TrackNote16"] as string;
                                TrackDate17 = rrr["TrackDate17"] as string;
                                TrackBy17 = rrr["TrackBy17"] as string;
                                TrackSource17 = rrr["TrackSource17"] as string;
                                TrackNote17 = rrr["TrackNote17"] as string;
                                TrackDate18 = rrr["TrackDate18"] as string;
                                TrackBy18 = rrr["TrackBy18"] as string;
                                TrackSource18 = rrr["TrackSource18"] as string;
                                TrackNote18 = rrr["TrackNote18"] as string;
                                QuotePrice = rrr["QuotePrice"] as string;
                                Credit = rrr["Credit"] as string;
                                Freight = rrr["Freight"] as string;
                                ShopTime = rrr["ShopTime"] as string;
                                TotalCost = rrr["TotalCost"] as string;
                                GrossProfit = rrr["GrossProfit"] as string;
                                Profit = rrr["Profit"] as string;
                                Description = rrr["Description"] as string;

                                Quant1 = rrr["Quant1"] as string;
                                Descr1 = rrr["Descr1"] as string;
                                Costs1 = rrr["Costs1"] as string;
                                ECost1 = rrr["ECost1"] as string;
                                Quant2 = rrr["Quant2"] as string;
                                Descr2 = rrr["Descr2"] as string;
                                Costs2 = rrr["Costs2"] as string;
                                ECost2 = rrr["ECost2"] as string;
                                Quant3 = rrr["Quant3"] as string;
                                Descr3 = rrr["Descr3"] as string;
                                Costs3 = rrr["Costs3"] as string;
                                ECost3 = rrr["ECost3"] as string;
                                Quant4 = rrr["Quant4"] as string;
                                Descr4 = rrr["Descr4"] as string;
                                Costs4 = rrr["Costs4"] as string;
                                ECost4 = rrr["ECost4"] as string;
                                Quant5 = rrr["Quant5"] as string;
                                Descr5 = rrr["Descr5"] as string;
                                Costs5 = rrr["Costs5"] as string;
                                ECost5 = rrr["ECost5"] as string;
                                Quant6 = rrr["Quant6"] as string;
                                Descr6 = rrr["Descr6"] as string;
                                Costs6 = rrr["Costs6"] as string;
                                ECost6 = rrr["ECost6"] as string;
                                Quant7 = rrr["Quant7"] as string;
                                Descr7 = rrr["Descr7"] as string;
                                Costs7 = rrr["Costs7"] as string;
                                ECost7 = rrr["ECost7"] as string;
                                Quant8 = rrr["Quant8"] as string;
                                Descr8 = rrr["Descr8"] as string;
                                Costs8 = rrr["Costs8"] as string;
                                ECost8 = rrr["ECost8"] as string;
                                Quant9 = rrr["Quant9"] as string;
                                Descr9 = rrr["Descr9"] as string;
                                Costs9 = rrr["Costs9"] as string;
                                ECost9 = rrr["ECost9"] as string;
                                Quant10 = rrr["Quant10"] as string;
                                Descr10 = rrr["Descr10"] as string;
                                Costs10 = rrr["Costs10"] as string;
                                ECost10 = rrr["ECost10"] as string;
                                Quant11 = rrr["Quant11"] as string;
                                Descr11 = rrr["Descr11"] as string;
                                Costs11 = rrr["Costs11"] as string;
                                ECost11 = rrr["ECost11"] as string;
                                Quant12 = rrr["Quant12"] as string;
                                Descr12 = rrr["Descr12"] as string;
                                Costs12 = rrr["Costs12"] as string;
                                ECost12 = rrr["ECost12"] as string;
                                Quant13 = rrr["Quant13"] as string;
                                Descr13 = rrr["Descr13"] as string;
                                Costs13 = rrr["Costs13"] as string;
                                ECost13 = rrr["ECost13"] as string;
                                Quant14 = rrr["Quant14"] as string;
                                Descr14 = rrr["Descr14"] as string;
                                Costs14 = rrr["Costs14"] as string;
                                ECost14 = rrr["ECost14"] as string;
                                Quant15 = rrr["Quant15"] as string;
                                Descr15 = rrr["Descr15"] as string;
                                Costs15 = rrr["Costs15"] as string;
                                ECost15 = rrr["ECost15"] as string;
                                Quant16 = rrr["Quant16"] as string;
                                Descr16 = rrr["Descr16"] as string;
                                Costs16 = rrr["Costs16"] as string;
                                ECost16 = rrr["ECost16"] as string;
                                Quant17 = rrr["Quant17"] as string;
                                Descr17 = rrr["Descr17"] as string;
                                Costs17 = rrr["Costs17"] as string;
                                ECost17 = rrr["ECost17"] as string;
                                Quant18 = rrr["Quant18"] as string;
                                Descr18 = rrr["Descr18"] as string;
                                Costs18 = rrr["Costs18"] as string;
                                ECost18 = rrr["ECost18"] as string;
                                Quant19 = rrr["Quant19"] as string;
                                Descr19 = rrr["Descr19"] as string;
                                Costs19 = rrr["Costs19"] as string;
                                ECost19 = rrr["ECost19"] as string;
                                Quant20 = rrr["Quant20"] as string;
                                Descr20 = rrr["Descr20"] as string;
                                Costs20 = rrr["Costs20"] as string;
                                ECost20 = rrr["ECost20"] as string;
                                Quant21 = rrr["Quant21"] as string;
                                Descr21 = rrr["Descr21"] as string;
                                Costs21 = rrr["Costs21"] as string;
                                ECost21 = rrr["ECost21"] as string;
                                Quant22 = rrr["Quant22"] as string;
                                Descr22 = rrr["Descr22"] as string;
                                Costs22 = rrr["Costs22"] as string;
                                ECost22 = rrr["ECost22"] as string;
                                Quant23 = rrr["Quant23"] as string;
                                Descr23 = rrr["Descr23"] as string;
                                Costs23 = rrr["Costs23"] as string;
                                ECost23 = rrr["ECost23"] as string;

                                InvInstructions = rrr["InvInstructions"] as string;
                                InvNotes = rrr["InvNotes"] as string;
                                VendorNotes = rrr["VendorNotes"] as string;
                                AccNotes = rrr["Spare1"] as string;
                                CrMemo = rrr["CrMemo"] as string;
                                InvNumber = rrr["InvNumber"] as string;
                                InvDate = rrr["InvDate"] as string;
                                Status = rrr["Status"] as string;
                                CheckNumbers = rrr["CheckNumbers"] as string;
                                CheckDates = rrr["CheckDates"] as string;

                                ComDate1 = rrr["ComDate1"] as string;
                                ComCheckNumber1 = rrr["ComCheckNumber1"] as string;
                                ComPaid1 = rrr["ComPaid1"] as string;
                                ComDate2 = rrr["ComDate2"] as string;
                                ComCheckNumber2 = rrr["ComCheckNumber2"] as string;
                                ComPaid2 = rrr["ComPaid2"] as string;
                                ComDate3 = rrr["ComDate3"] as string;
                                ComCheckNumber3 = rrr["ComCheckNumber3"] as string;
                                ComPaid3 = rrr["ComPaid3"] as string;
                                ComDate4 = rrr["ComDate4"] as string;
                                ComCheckNumber4 = rrr["ComCheckNumber4"] as string;
                                ComPaid4 = rrr["ComPaid4"] as string;
                                ComDate5 = rrr["ComDate5"] as string;
                                ComCheckNumber5 = rrr["ComCheckNumber5"] as string;
                                ComPaid5 = rrr["ComPaid5"] as string;

                                ComAmount = rrr["ComAmount"] as string;
                                ComBalance = rrr["ComBalance"] as string;
                                DeliveryNotes = rrr["DeliveryNotes"] as string;
                                PONotes = rrr["PONotes"] as string;
                                IsOk = rrr["Spare2"] as string;

                                #endregion
                                //_log.append("readRowOrders reads end");

                                //con.Close();

                                _log.append("readRowOrders end1");

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

            #region default sets if no record -- goes into GUI

            thePO = generateNextOrderPO();

            Date = todaysDate();
            EndUser = "";

            Equipment = "";
            VendorName = "";
            JobNumber = "";
            CustomerPO = "";
            VendorNumber = "";
            SalesAss = "";
            SoldTo = "";
            Street1 = "";
            Street2 = "";
            City = "";
            State = "";
            Zip = "";
            ShipTo = "";
            ShipStreet1 = "";
            ShipStreet2 = "";
            ShipCity = "";
            ShipState = "";
            ShipZip = "";
            Carrier = "";
            ShipDate = "";
            IsComOrder = "";
            IsComPaid = "";
            Grinder = "";
            SerialNo = "";
            PumpStk = "";
            ReqDate = "";
            SchedShip = "";
            PODate = "";
            POShipVia = "";

            TrackDate1 = "";
            TrackBy1 = "";
            TrackSource1 = "";
            TrackNote1 = "";
            TrackDate2 = "";
            TrackBy2 = "";
            TrackSource2 = "";
            TrackNote2 = "";
            TrackDate3 = "";
            TrackBy3 = "";
            TrackSource3 = "";
            TrackNote3 = "";
            TrackDate4 = "";
            TrackBy4 = "";
            TrackSource4 = "";
            TrackNote4 = "";
            TrackDate5 = "";
            TrackBy5 = "";
            TrackSource5 = "";
            TrackNote5 = "";
            TrackDate6 = "";
            TrackBy6 = "";
            TrackSource6 = "";
            TrackNote6 = "";
            TrackDate7 = "";
            TrackBy7 = "";
            TrackSource7 = "";
            TrackNote7 = "";
            TrackDate8 = "";
            TrackBy8 = "";
            TrackSource8 = "";
            TrackNote8 = "";
            TrackDate9 = "";
            TrackBy9 = "";
            TrackSource9 = "";
            TrackNote9 = "";
            TrackDate10 = "";
            TrackBy10 = "";
            TrackSource10 = "";
            TrackNote10 = "";
            TrackDate11 = "";
            TrackBy11 = "";
            TrackSource11 = "";
            TrackNote11 = "";
            TrackDate12 = "";
            TrackBy12 = "";
            TrackSource12 = "";
            TrackNote12 = "";
            TrackDate13 = "";
            TrackBy13 = "";
            TrackSource13 = "";
            TrackNote13 = "";
            TrackDate14 = "";
            TrackBy14 = "";
            TrackSource14 = "";
            TrackNote14 = "";
            TrackDate15 = "";
            TrackBy15 = "";
            TrackSource15 = "";
            TrackNote15 = "";
            TrackDate16 = "";
            TrackBy16 = "";
            TrackSource16 = "";
            TrackNote16 = "";
            TrackDate17 = "";
            TrackBy17 = "";
            TrackSource17 = "";
            TrackNote17 = "";
            TrackDate18 = "";
            TrackBy18 = "";
            TrackSource18 = "";
            TrackNote18 = "";

            QuotePrice = "0.00";
            Credit = "0.00";
            Freight = "0.00";
            ShopTime = "0.00";
            TotalCost = "0.00";
            GrossProfit = "0.00";
            Profit = "0.00";
            Description = "";

            Quant1 = "0.00";
            Descr1 = "";
            Costs1 = "0.00";
            ECost1 = "0.00";
            Quant2 = "0.00";
            Descr2 = "";
            Costs2 = "0.00";
            ECost2 = "0.00";
            Quant3 = "0.00";
            Descr3 = "";
            Costs3 = "0.00";
            ECost3 = "0.00";
            Quant4 = "0.00";
            Descr4 = "";
            Costs4 = "0.00";
            ECost4 = "0.00";
            Quant5 = "0.00";
            Descr5 = "";
            Costs5 = "0.00";
            ECost5 = "0.00";
            Quant6 = "0.00";
            Descr6 = "";
            Costs6 = "0.00";
            ECost6 = "0.00";
            Quant7 = "0.00";
            Descr7 = "";
            Costs7 = "0.00";
            ECost7 = "0.00";
            Quant8 = "0.00";
            Descr8 = "";
            Costs8 = "0.00";
            ECost8 = "0.00";
            Quant9 = "0.00";
            Descr9 = "";
            Costs9 = "0.00";
            ECost9 = "0.00";
            Quant10 = "0.00";
            Descr10 = "";
            Costs10 = "0.00";
            ECost10 = "0.00";
            Quant11 = "0.00";
            Descr11 = "";
            Costs11 = "0.00";
            ECost11 = "0.00";
            Quant12 = "0.00";
            Descr12 = "";
            Costs12 = "0.00";
            ECost12 = "0.00";
            Quant13 = "0.00";
            Descr13 = "";
            Costs13 = "0.00";
            ECost13 = "0.00";
            Quant14 = "0.00";
            Descr14 = "";
            Costs14 = "0.00";
            ECost14 = "0.00";
            Quant15 = "0.00";
            Descr15 = "";
            Costs15 = "0.00";
            ECost15 = "0.00";
            Quant16 = "0.00";
            Descr16 = "";
            Costs16 = "0.00";
            ECost16 = "0.00";
            Quant17 = "0.00";
            Descr17 = "";
            Costs17 = "0.00";
            ECost17 = "0.00";
            Quant18 = "0.00";
            Descr18 = "";
            Costs18 = "0.00";
            ECost18 = "0.00";
            Quant19 = "0.00";
            Descr19 = "";
            Costs19 = "0.00";
            ECost19 = "0.00";
            Quant20 = "0.00";
            Descr20 = "";
            Costs20 = "0.00";
            ECost20 = "0.00";
            Quant21 = "0.00";
            Descr21 = "";
            Costs21 = "0.00";
            ECost21 = "0.00";
            Quant22 = "0.00";
            Descr22 = "";
            Costs22 = "0.00";
            ECost22 = "0.00";
            Quant23 = "0.00";
            Descr23 = "";
            Costs23 = "0.00";
            ECost23 = "0.00";

            InvInstructions = "";
            InvNotes = "";
            VendorNotes = "";
            AccNotes = "";
            CrMemo = "";
            InvNumber = "";
            InvDate = "";
            Status = "";
            CheckNumbers = "";
            CheckDates = "";

            ComDate1 = "";
            ComCheckNumber1 = "";
            ComPaid1 = "0.00";
            ComDate2 = "";
            ComCheckNumber2 = "";
            ComPaid2 = "0.00";
            ComDate3 = "";
            ComCheckNumber3 = "";
            ComPaid3 = "0.00";
            ComDate4 = "";
            ComCheckNumber4 = "";
            ComPaid4 = "0.00";
            ComDate5 = "";
            ComCheckNumber5 = "";
            ComPaid5 = "0.00";

            ComAmount = "";
            ComBalance = "";
            DeliveryNotes = "";
            PONotes = "";
            IsOk = "";

            #endregion

            _log.append("readRowOrders end2");

            return false;
        }


        private int updateRowOrders(string PO, string Date, string EndUser, string Equipment, string VendorName, string JobNumber, string CustomerPO,
            string VendorNumber, string SalesAss, string SoldTo, string Street1, string Street2, string City, string State, string Zip,
            string ShipTo, string ShipStreet1, string ShipStreet2, string ShipCity, string ShipState, string ShipZip,
            string Carrier, string ShipDate, string IsComOrder, string IsComPaid, string Grinder, string SerialNo, string PumpStk,
            string ReqDate, string SchedShip, string PODate, string POShipVia,
            string TrackDate1, string TrackBy1, string TrackSource1, string TrackNote1,
            string TrackDate2, string TrackBy2, string TrackSource2, string TrackNote2,
            string TrackDate3, string TrackBy3, string TrackSource3, string TrackNote3,
            string TrackDate4, string TrackBy4, string TrackSource4, string TrackNote4,
            string TrackDate5, string TrackBy5, string TrackSource5, string TrackNote5,
            string TrackDate6, string TrackBy6, string TrackSource6, string TrackNote6,
            string TrackDate7, string TrackBy7, string TrackSource7, string TrackNote7,
            string TrackDate8, string TrackBy8, string TrackSource8, string TrackNote8,
            string TrackDate9, string TrackBy9, string TrackSource9, string TrackNote9,
            string TrackDate10, string TrackBy10, string TrackSource10, string TrackNote10,
            string TrackDate11, string TrackBy11, string TrackSource11, string TrackNote11,
            string TrackDate12, string TrackBy12, string TrackSource12, string TrackNote12,
            string TrackDate13, string TrackBy13, string TrackSource13, string TrackNote13,
            string TrackDate14, string TrackBy14, string TrackSource14, string TrackNote14,
            string TrackDate15, string TrackBy15, string TrackSource15, string TrackNote15,
            string TrackDate16, string TrackBy16, string TrackSource16, string TrackNote16,
            string TrackDate17, string TrackBy17, string TrackSource17, string TrackNote17,
            string TrackDate18, string TrackBy18, string TrackSource18, string TrackNote18,
            string QuotePrice, string Credit, string Freight, string ShopTime, string TotalCost, string GrossProfit, string Profit,
            string Description,
            string Quant1, string Descr1, string Costs1, string ECost1,
            string Quant2, string Descr2, string Costs2, string ECost2,
            string Quant3, string Descr3, string Costs3, string ECost3,
            string Quant4, string Descr4, string Costs4, string ECost4,
            string Quant5, string Descr5, string Costs5, string ECost5,
            string Quant6, string Descr6, string Costs6, string ECost6,
            string Quant7, string Descr7, string Costs7, string ECost7,
            string Quant8, string Descr8, string Costs8, string ECost8,
            string Quant9, string Descr9, string Costs9, string ECost9,
            string Quant10, string Descr10, string Costs10, string ECost10,
            string Quant11, string Descr11, string Costs11, string ECost11,
            string Quant12, string Descr12, string Costs12, string ECost12,
            string Quant13, string Descr13, string Costs13, string ECost13,
            string Quant14, string Descr14, string Costs14, string ECost14,
            string Quant15, string Descr15, string Costs15, string ECost15,
            string Quant16, string Descr16, string Costs16, string ECost16,
            string Quant17, string Descr17, string Costs17, string ECost17,
            string Quant18, string Descr18, string Costs18, string ECost18,
            string Quant19, string Descr19, string Costs19, string ECost19,
            string Quant20, string Descr20, string Costs20, string ECost20,
            string Quant21, string Descr21, string Costs21, string ECost21,
            string Quant22, string Descr22, string Costs22, string ECost22,
            string Quant23, string Descr23, string Costs23, string ECost23,
            string InvInstructions, string InvNotes, string VendorNotes, string AccNotes, string CrMemo, string InvNumber, string InvDate, string Status,
            string CheckNumbers, string CheckDates,
            string ComDate1, string ComCheckNumber1, string ComPaid1,
            string ComDate2, string ComCheckNumber2, string ComPaid2,
            string ComDate3, string ComCheckNumber3, string ComPaid3,
            string ComDate4, string ComCheckNumber4, string ComPaid4,
            string ComDate5, string ComCheckNumber5, string ComPaid5,
            string ComAmount, string ComBalance, string DeliveryNotes, string PONotes, string IsOk)
        {
            _log.append("updateRowOrders start");
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region update string for OrderTable

                    com.CommandText =
                    "UPDATE OrderTable SET PO = @PO, Date = @Date, EndUser = @EndUser, Equipment = @Equipment, VendorName = @VendorName, JobNumber = @JobNumber, CustomerPO = @CustomerPO, " +
                    "VendorNumber = @VendorNumber,  SalesAss = @SalesAss,  SoldTo = @SoldTo,  Street1 = @Street1,  Street2 = @Street2,  City = @City,  State = @State,  Zip = @Zip, " +
                    "ShipTo = @ShipTo, ShipStreet1 = @ShipStreet1,  ShipStreet2 = @ShipStreet2,  ShipCity = @ShipCity,  ShipState = @ShipState,  ShipZip = @ShipZip, " +
                    "Carrier = @Carrier, ShipDate = @ShipDate, IsComOrder = @IsComOrder,  IsComPaid = @IsComPaid,  Grinder = @Grinder,  SerialNo = @SerialNo,  PumpStk = @PumpStk, " +
                    "ReqDate = @ReqDate, SchedShip = @SchedShip,  PODate = @PODate,  POShipVia = @POShipVia, " +
                    "TrackDate1 = @TrackDate1,  TrackBy1 = @TrackBy1,  TrackSource1 = @TrackSource1,  TrackNote1 = @TrackNote1, " +
                    "TrackDate2 = @TrackDate2,  TrackBy2 = @TrackBy2,  TrackSource2 = @TrackSource2,  TrackNote2 = @TrackNote2, " +
                    "TrackDate3 = @TrackDate3,  TrackBy3 = @TrackBy3,  TrackSource3 = @TrackSource3,  TrackNote3 = @TrackNote3, " +
                    "TrackDate4 = @TrackDate4,  TrackBy4 = @TrackBy4,  TrackSource4 = @TrackSource4,  TrackNote4 = @TrackNote4, " +
                    "TrackDate5 = @TrackDate5,  TrackBy5 = @TrackBy5,  TrackSource5 = @TrackSource5,  TrackNote5 = @TrackNote5, " +
                    "TrackDate6 = @TrackDate6,  TrackBy6 = @TrackBy6,  TrackSource6 = @TrackSource6,  TrackNote6 = @TrackNote6, " +
                    "TrackDate7 = @TrackDate7,  TrackBy7 = @TrackBy7,  TrackSource7 = @TrackSource7,  TrackNote7 = @TrackNote7, " +
                    "TrackDate8 = @TrackDate8,  TrackBy8 = @TrackBy8,  TrackSource8 = @TrackSource8,  TrackNote8 = @TrackNote8, " +
                    "TrackDate9 = @TrackDate9,  TrackBy9 = @TrackBy9,  TrackSource9 = @TrackSource9,  TrackNote9 = @TrackNote9, " +
                    "TrackDate10 = @TrackDate10,  TrackBy10 = @TrackBy10,  TrackSource10 = @TrackSource10,  TrackNote10 = @TrackNote10, " +
                    "TrackDate11 = @TrackDate11,  TrackBy11 = @TrackBy11,  TrackSource11 = @TrackSource11,  TrackNote11 = @TrackNote11, " +
                    "TrackDate12 = @TrackDate12,  TrackBy12 = @TrackBy12,  TrackSource12 = @TrackSource12,  TrackNote12 = @TrackNote12, " +
                    "TrackDate13 = @TrackDate13,  TrackBy13 = @TrackBy13,  TrackSource13 = @TrackSource13,  TrackNote13 = @TrackNote13, " +
                    "TrackDate14 = @TrackDate14,  TrackBy14 = @TrackBy14,  TrackSource14 = @TrackSource14,  TrackNote14 = @TrackNote14, " +
                    "TrackDate15 = @TrackDate15,  TrackBy15 = @TrackBy15,  TrackSource15 = @TrackSource15,  TrackNote15 = @TrackNote15, " +
                    "TrackDate16 = @TrackDate16,  TrackBy16 = @TrackBy16,  TrackSource16 = @TrackSource16,  TrackNote16 = @TrackNote16, " +
                    "TrackDate17 = @TrackDate17,  TrackBy17 = @TrackBy17,  TrackSource17 = @TrackSource17,  TrackNote17 = @TrackNote17, " +
                    "TrackDate18 = @TrackDate18,  TrackBy18 = @TrackBy18,  TrackSource18 = @TrackSource18,  TrackNote18 = @TrackNote18, " +
                    "QuotePrice = @QuotePrice,  Credit = @Credit,  Freight = @Freight,  ShopTime = @ShopTime,  TotalCost = @TotalCost,  GrossProfit = @GrossProfit,  Profit = @Profit, " +
                    "Description = @Description, " +
                    "Quant1 = @Quant1,  Descr1 = @Descr1,  Costs1 = @Costs1,  ECost1 = @ECost1, " +
                    "Quant2 = @Quant2,  Descr2 = @Descr2,  Costs2 = @Costs2,  ECost2 = @ECost2, " +
                    "Quant3 = @Quant3,  Descr3 = @Descr3,  Costs3 = @Costs3,  ECost3 = @ECost3, " +
                    "Quant4 = @Quant4,  Descr4 = @Descr4,  Costs4 = @Costs4,  ECost4 = @ECost4, " +
                    "Quant5 = @Quant5,  Descr5 = @Descr5,  Costs5 = @Costs5,  ECost5 = @ECost5, " +
                    "Quant6 = @Quant6,  Descr6 = @Descr6,  Costs6 = @Costs6,  ECost6 = @ECost6, " +
                    "Quant7 = @Quant7,  Descr7 = @Descr7,  Costs7 = @Costs7,  ECost7 = @ECost7, " +
                    "Quant8 = @Quant8,  Descr8 = @Descr8,  Costs8 = @Costs8,  ECost8 = @ECost8, " +
                    "Quant9 = @Quant9,  Descr9 = @Descr9,  Costs9 = @Costs9,  ECost9 = @ECost9, " +
                    "Quant10 = @Quant10,  Descr10 = @Descr10,  Costs10 = @Costs10,  ECost10 = @ECost10, " +
                    "Quant11 = @Quant11,  Descr11 = @Descr11,  Costs11 = @Costs11,  ECost11 = @ECost11, " +
                    "Quant12 = @Quant12,  Descr12 = @Descr12,  Costs12 = @Costs12,  ECost12 = @ECost12, " +
                    "Quant13 = @Quant13,  Descr13 = @Descr13,  Costs13 = @Costs13,  ECost13 = @ECost13, " +
                    "Quant14 = @Quant14,  Descr14 = @Descr14,  Costs14 = @Costs14,  ECost14 = @ECost14, " +
                    "Quant15 = @Quant15,  Descr15 = @Descr15,  Costs15 = @Costs15,  ECost15 = @ECost15, " +
                    "Quant16 = @Quant16,  Descr16 = @Descr16,  Costs16 = @Costs16,  ECost16 = @ECost16, " +
                    "Quant17 = @Quant17,  Descr17 = @Descr17,  Costs17 = @Costs17,  ECost17 = @ECost17, " +
                    "Quant18 = @Quant18,  Descr18 = @Descr18,  Costs18 = @Costs18,  ECost18 = @ECost18, " +
                    "Quant19 = @Quant19,  Descr19 = @Descr19,  Costs19 = @Costs19,  ECost19 = @ECost19, " +
                    "Quant20 = @Quant20,  Descr20 = @Descr20,  Costs20 = @Costs20,  ECost20 = @ECost20, " +
                    "Quant21 = @Quant21,  Descr21 = @Descr21,  Costs21 = @Costs21,  ECost21 = @ECost21, " +
                    "Quant22 = @Quant22,  Descr22 = @Descr22,  Costs22 = @Costs22,  ECost22 = @ECost22, " +
                    "Quant23 = @Quant23,  Descr23 = @Descr23,  Costs23 = @Costs23,  ECost23 = @ECost23, " +
                    "InvInstructions = @InvInstructions,  InvNotes = @InvNotes,  VendorNotes = @VendorNotes,  Spare1 = @Spare1, CrMemo = @CrMemo,  InvNumber = @InvNumber,  InvDate = @InvDate,  Status = @Status, " +
                    "CheckNumbers = @CheckNumbers,  CheckDates = @CheckDates, " +
                    "ComDate1 = @ComDate1,  ComCheckNumber1 = @ComCheckNumber1,  ComPaid1 = @ComPaid1, " +
                    "ComDate2 = @ComDate2,  ComCheckNumber2 = @ComCheckNumber2,  ComPaid2 = @ComPaid2, " +
                    "ComDate3 = @ComDate3,  ComCheckNumber3 = @ComCheckNumber3,  ComPaid3 = @ComPaid3, " +
                    "ComDate4 = @ComDate4,  ComCheckNumber4 = @ComCheckNumber4,  ComPaid4 = @ComPaid4, " +
                    "ComDate5 = @ComDate5,  ComCheckNumber5 = @ComCheckNumber5,  ComPaid5 = @ComPaid5, " +
                    "ComAmount = @ComAmount,  ComBalance = @ComBalance,  DeliveryNotes = @DeliveryNotes,  PONotes = @PONotes,  Spare2 = @Spare2 " +
                    "Where PO = @PO";

                    #endregion

                    #region Parameters for OrderTable

                    com.Parameters.AddWithValue("@PO", PO);
                    com.Parameters.AddWithValue("@Date", Date);
                    com.Parameters.AddWithValue("@EndUser", EndUser);
                    com.Parameters.AddWithValue("@Equipment", Equipment);
                    com.Parameters.AddWithValue("@VendorName", VendorName);
                    com.Parameters.AddWithValue("@JobNumber", JobNumber);
                    com.Parameters.AddWithValue("@CustomerPO", CustomerPO);
                    com.Parameters.AddWithValue("@VendorNumber", VendorNumber);
                    com.Parameters.AddWithValue("@SalesAss", SalesAss);
                    com.Parameters.AddWithValue("@SoldTo", SoldTo);
                    com.Parameters.AddWithValue("@Street1", Street1);
                    com.Parameters.AddWithValue("@Street2", Street2);
                    com.Parameters.AddWithValue("@City", City);
                    com.Parameters.AddWithValue("@State", State);
                    com.Parameters.AddWithValue("@Zip", Zip);
                    com.Parameters.AddWithValue("@ShipTo", ShipTo);
                    com.Parameters.AddWithValue("@ShipStreet1", ShipStreet1);
                    com.Parameters.AddWithValue("@ShipStreet2", ShipStreet2);
                    com.Parameters.AddWithValue("@ShipCity", ShipCity);
                    com.Parameters.AddWithValue("@ShipState", ShipState);
                    com.Parameters.AddWithValue("@ShipZip", ShipZip);
                    com.Parameters.AddWithValue("@Carrier", Carrier);
                    com.Parameters.AddWithValue("@ShipDate", ShipDate);
                    com.Parameters.AddWithValue("@IsComOrder", IsComOrder);
                    com.Parameters.AddWithValue("@IsComPaid", IsComPaid);
                    com.Parameters.AddWithValue("@Grinder", Grinder);
                    com.Parameters.AddWithValue("@SerialNo", SerialNo);
                    com.Parameters.AddWithValue("@PumpStk", PumpStk);
                    com.Parameters.AddWithValue("@ReqDate", ReqDate);
                    com.Parameters.AddWithValue("@SchedShip", SchedShip);
                    com.Parameters.AddWithValue("@PODate", PODate);
                    com.Parameters.AddWithValue("@POShipVia", POShipVia);

                    com.Parameters.AddWithValue("@TrackDate1", TrackDate1);
                    com.Parameters.AddWithValue("@TrackBy1", TrackBy1);
                    com.Parameters.AddWithValue("@TrackSource1", TrackSource1);
                    com.Parameters.AddWithValue("@TrackNote1", TrackNote1);

                    com.Parameters.AddWithValue("@TrackDate2", TrackDate2);
                    com.Parameters.AddWithValue("@TrackBy2", TrackBy2);
                    com.Parameters.AddWithValue("@TrackSource2", TrackSource2);
                    com.Parameters.AddWithValue("@TrackNote2", TrackNote2);

                    com.Parameters.AddWithValue("@TrackDate3", TrackDate3);
                    com.Parameters.AddWithValue("@TrackBy3", TrackBy3);
                    com.Parameters.AddWithValue("@TrackSource3", TrackSource3);
                    com.Parameters.AddWithValue("@TrackNote3", TrackNote3);

                    com.Parameters.AddWithValue("@TrackDate4", TrackDate4);
                    com.Parameters.AddWithValue("@TrackBy4", TrackBy4);
                    com.Parameters.AddWithValue("@TrackSource4", TrackSource4);
                    com.Parameters.AddWithValue("@TrackNote4", TrackNote4);

                    com.Parameters.AddWithValue("@TrackDate5", TrackDate5);
                    com.Parameters.AddWithValue("@TrackBy5", TrackBy5);
                    com.Parameters.AddWithValue("@TrackSource5", TrackSource5);
                    com.Parameters.AddWithValue("@TrackNote5", TrackNote5);

                    com.Parameters.AddWithValue("@TrackDate6", TrackDate6);
                    com.Parameters.AddWithValue("@TrackBy6", TrackBy6);
                    com.Parameters.AddWithValue("@TrackSource6", TrackSource6);
                    com.Parameters.AddWithValue("@TrackNote6", TrackNote6);

                    com.Parameters.AddWithValue("@TrackDate7", TrackDate7);
                    com.Parameters.AddWithValue("@TrackBy7", TrackBy7);
                    com.Parameters.AddWithValue("@TrackSource7", TrackSource7);
                    com.Parameters.AddWithValue("@TrackNote7", TrackNote7);

                    com.Parameters.AddWithValue("@TrackDate8", TrackDate8);
                    com.Parameters.AddWithValue("@TrackBy8", TrackBy8);
                    com.Parameters.AddWithValue("@TrackSource8", TrackSource8);
                    com.Parameters.AddWithValue("@TrackNote8", TrackNote8);

                    com.Parameters.AddWithValue("@TrackDate9", TrackDate9);
                    com.Parameters.AddWithValue("@TrackBy9", TrackBy9);
                    com.Parameters.AddWithValue("@TrackSource9", TrackSource9);
                    com.Parameters.AddWithValue("@TrackNote9", TrackNote9);

                    com.Parameters.AddWithValue("@TrackDate10", TrackDate10);
                    com.Parameters.AddWithValue("@TrackBy10", TrackBy10);
                    com.Parameters.AddWithValue("@TrackSource10", TrackSource10);
                    com.Parameters.AddWithValue("@TrackNote10", TrackNote10);

                    com.Parameters.AddWithValue("@TrackDate11", TrackDate11);
                    com.Parameters.AddWithValue("@TrackBy11", TrackBy11);
                    com.Parameters.AddWithValue("@TrackSource11", TrackSource11);
                    com.Parameters.AddWithValue("@TrackNote11", TrackNote11);

                    com.Parameters.AddWithValue("@TrackDate12", TrackDate12);
                    com.Parameters.AddWithValue("@TrackBy12", TrackBy12);
                    com.Parameters.AddWithValue("@TrackSource12", TrackSource12);
                    com.Parameters.AddWithValue("@TrackNote12", TrackNote12);

                    com.Parameters.AddWithValue("@TrackDate13", TrackDate13);
                    com.Parameters.AddWithValue("@TrackBy13", TrackBy13);
                    com.Parameters.AddWithValue("@TrackSource13", TrackSource13);
                    com.Parameters.AddWithValue("@TrackNote13", TrackNote13);

                    com.Parameters.AddWithValue("@TrackDate14", TrackDate14);
                    com.Parameters.AddWithValue("@TrackBy14", TrackBy14);
                    com.Parameters.AddWithValue("@TrackSource14", TrackSource14);
                    com.Parameters.AddWithValue("@TrackNote14", TrackNote14);

                    com.Parameters.AddWithValue("@TrackDate15", TrackDate15);
                    com.Parameters.AddWithValue("@TrackBy15", TrackBy15);
                    com.Parameters.AddWithValue("@TrackSource15", TrackSource15);
                    com.Parameters.AddWithValue("@TrackNote15", TrackNote15);

                    com.Parameters.AddWithValue("@TrackDate16", TrackDate16);
                    com.Parameters.AddWithValue("@TrackBy16", TrackBy16);
                    com.Parameters.AddWithValue("@TrackSource16", TrackSource16);
                    com.Parameters.AddWithValue("@TrackNote16", TrackNote16);

                    com.Parameters.AddWithValue("@TrackDate17", TrackDate17);
                    com.Parameters.AddWithValue("@TrackBy17", TrackBy17);
                    com.Parameters.AddWithValue("@TrackSource17", TrackSource17);
                    com.Parameters.AddWithValue("@TrackNote17", TrackNote17);

                    com.Parameters.AddWithValue("@TrackDate18", TrackDate18);
                    com.Parameters.AddWithValue("@TrackBy18", TrackBy18);
                    com.Parameters.AddWithValue("@TrackSource18", TrackSource18);
                    com.Parameters.AddWithValue("@TrackNote18", TrackNote18);
                    com.Parameters.AddWithValue("@QuotePrice", QuotePrice);
                    com.Parameters.AddWithValue("@Credit", Credit);
                    com.Parameters.AddWithValue("@Freight", Freight);
                    com.Parameters.AddWithValue("@ShopTime", ShopTime);
                    com.Parameters.AddWithValue("@TotalCost", TotalCost);
                    com.Parameters.AddWithValue("@GrossProfit", GrossProfit);
                    com.Parameters.AddWithValue("@Profit", Profit);
                    com.Parameters.AddWithValue("@Description", Description);

                    com.Parameters.AddWithValue("@Quant1", Quant1);
                    com.Parameters.AddWithValue("@Descr1", Descr1);
                    com.Parameters.AddWithValue("@Costs1", Costs1);
                    com.Parameters.AddWithValue("@ECost1", ECost1);
                    com.Parameters.AddWithValue("@Quant2", Quant2);
                    com.Parameters.AddWithValue("@Descr2", Descr2);
                    com.Parameters.AddWithValue("@Costs2", Costs2);
                    com.Parameters.AddWithValue("@ECost2", ECost2);
                    com.Parameters.AddWithValue("@Quant3", Quant3);
                    com.Parameters.AddWithValue("@Descr3", Descr3);
                    com.Parameters.AddWithValue("@Costs3", Costs3);
                    com.Parameters.AddWithValue("@ECost3", ECost3);
                    com.Parameters.AddWithValue("@Quant4", Quant4);
                    com.Parameters.AddWithValue("@Descr4", Descr4);
                    com.Parameters.AddWithValue("@Costs4", Costs4);
                    com.Parameters.AddWithValue("@ECost4", ECost4);
                    com.Parameters.AddWithValue("@Quant5", Quant5);
                    com.Parameters.AddWithValue("@Descr5", Descr5);
                    com.Parameters.AddWithValue("@Costs5", Costs5);
                    com.Parameters.AddWithValue("@ECost5", ECost5);
                    com.Parameters.AddWithValue("@Quant6", Quant6);
                    com.Parameters.AddWithValue("@Descr6", Descr6);
                    com.Parameters.AddWithValue("@Costs6", Costs6);
                    com.Parameters.AddWithValue("@ECost6", ECost6);
                    com.Parameters.AddWithValue("@Quant7", Quant7);
                    com.Parameters.AddWithValue("@Descr7", Descr7);
                    com.Parameters.AddWithValue("@Costs7", Costs7);
                    com.Parameters.AddWithValue("@ECost7", ECost7);
                    com.Parameters.AddWithValue("@Quant8", Quant8);
                    com.Parameters.AddWithValue("@Descr8", Descr8);
                    com.Parameters.AddWithValue("@Costs8", Costs8);
                    com.Parameters.AddWithValue("@ECost8", ECost8);
                    com.Parameters.AddWithValue("@Quant9", Quant9);
                    com.Parameters.AddWithValue("@Descr9", Descr9);
                    com.Parameters.AddWithValue("@Costs9", Costs9);
                    com.Parameters.AddWithValue("@ECost9", ECost9);
                    com.Parameters.AddWithValue("@Quant10", Quant10);
                    com.Parameters.AddWithValue("@Descr10", Descr10);
                    com.Parameters.AddWithValue("@Costs10", Costs10);
                    com.Parameters.AddWithValue("@ECost10", ECost10);
                    com.Parameters.AddWithValue("@Quant11", Quant11);
                    com.Parameters.AddWithValue("@Descr11", Descr11);
                    com.Parameters.AddWithValue("@Costs11", Costs11);
                    com.Parameters.AddWithValue("@ECost11", ECost11);
                    com.Parameters.AddWithValue("@Quant12", Quant12);
                    com.Parameters.AddWithValue("@Descr12", Descr12);
                    com.Parameters.AddWithValue("@Costs12", Costs12);
                    com.Parameters.AddWithValue("@ECost12", ECost12);
                    com.Parameters.AddWithValue("@Quant13", Quant13);
                    com.Parameters.AddWithValue("@Descr13", Descr13);
                    com.Parameters.AddWithValue("@Costs13", Costs13);
                    com.Parameters.AddWithValue("@ECost13", ECost13);
                    com.Parameters.AddWithValue("@Quant14", Quant14);
                    com.Parameters.AddWithValue("@Descr14", Descr14);
                    com.Parameters.AddWithValue("@Costs14", Costs14);
                    com.Parameters.AddWithValue("@ECost14", ECost14);
                    com.Parameters.AddWithValue("@Quant15", Quant15);
                    com.Parameters.AddWithValue("@Descr15", Descr15);
                    com.Parameters.AddWithValue("@Costs15", Costs15);
                    com.Parameters.AddWithValue("@ECost15", ECost15);
                    com.Parameters.AddWithValue("@Quant16", Quant16);
                    com.Parameters.AddWithValue("@Descr16", Descr16);
                    com.Parameters.AddWithValue("@Costs16", Costs16);
                    com.Parameters.AddWithValue("@ECost16", ECost16);
                    com.Parameters.AddWithValue("@Quant17", Quant17);
                    com.Parameters.AddWithValue("@Descr17", Descr17);
                    com.Parameters.AddWithValue("@Costs17", Costs17);
                    com.Parameters.AddWithValue("@ECost17", ECost17);
                    com.Parameters.AddWithValue("@Quant18", Quant18);
                    com.Parameters.AddWithValue("@Descr18", Descr18);
                    com.Parameters.AddWithValue("@Costs18", Costs18);
                    com.Parameters.AddWithValue("@ECost18", ECost18);
                    com.Parameters.AddWithValue("@Quant19", Quant19);
                    com.Parameters.AddWithValue("@Descr19", Descr19);
                    com.Parameters.AddWithValue("@Costs19", Costs19);
                    com.Parameters.AddWithValue("@ECost19", ECost19);
                    com.Parameters.AddWithValue("@Quant20", Quant20);
                    com.Parameters.AddWithValue("@Descr20", Descr20);
                    com.Parameters.AddWithValue("@Costs20", Costs20);
                    com.Parameters.AddWithValue("@ECost20", ECost20);
                    com.Parameters.AddWithValue("@Quant21", Quant21);
                    com.Parameters.AddWithValue("@Descr21", Descr21);
                    com.Parameters.AddWithValue("@Costs21", Costs21);
                    com.Parameters.AddWithValue("@ECost21", ECost21);
                    com.Parameters.AddWithValue("@Quant22", Quant22);
                    com.Parameters.AddWithValue("@Descr22", Descr22);
                    com.Parameters.AddWithValue("@Costs22", Costs22);
                    com.Parameters.AddWithValue("@ECost22", ECost22);
                    com.Parameters.AddWithValue("@Quant23", Quant23);
                    com.Parameters.AddWithValue("@Descr23", Descr23);
                    com.Parameters.AddWithValue("@Costs23", Costs23);
                    com.Parameters.AddWithValue("@ECost23", ECost23);

                    com.Parameters.AddWithValue("@InvInstructions", InvInstructions);
                    com.Parameters.AddWithValue("@InvNotes", InvNotes);
                    com.Parameters.AddWithValue("@VendorNotes", VendorNotes);
                    com.Parameters.AddWithValue("@Spare1", AccNotes);
                    com.Parameters.AddWithValue("@CrMemo", CrMemo);
                    com.Parameters.AddWithValue("@InvNumber", InvNumber);
                    com.Parameters.AddWithValue("@InvDate", InvDate);
                    com.Parameters.AddWithValue("@Status", Status);
                    com.Parameters.AddWithValue("@CheckNumbers", CheckNumbers);
                    com.Parameters.AddWithValue("@CheckDates", CheckDates);

                    com.Parameters.AddWithValue("@ComDate1", ComDate1);
                    com.Parameters.AddWithValue("@ComCheckNumber1", ComCheckNumber1);
                    com.Parameters.AddWithValue("@ComPaid1", ComPaid1);
                    com.Parameters.AddWithValue("@ComDate2", ComDate2);
                    com.Parameters.AddWithValue("@ComCheckNumber2", ComCheckNumber2);
                    com.Parameters.AddWithValue("@ComPaid2", ComPaid2);
                    com.Parameters.AddWithValue("@ComDate3", ComDate3);
                    com.Parameters.AddWithValue("@ComCheckNumber3", ComCheckNumber3);
                    com.Parameters.AddWithValue("@ComPaid3", ComPaid3);
                    com.Parameters.AddWithValue("@ComDate4", ComDate4);
                    com.Parameters.AddWithValue("@ComCheckNumber4", ComCheckNumber4);
                    com.Parameters.AddWithValue("@ComPaid4", ComPaid4);
                    com.Parameters.AddWithValue("@ComDate5", ComDate5);
                    com.Parameters.AddWithValue("@ComCheckNumber5", ComCheckNumber5);
                    com.Parameters.AddWithValue("@ComPaid5", ComPaid5);

                    com.Parameters.AddWithValue("@ComAmount", ComAmount);
                    com.Parameters.AddWithValue("@ComBalance", ComBalance);

                    com.Parameters.AddWithValue("@DeliveryNotes", DeliveryNotes);
                    com.Parameters.AddWithValue("@PONotes", PONotes);
                    com.Parameters.AddWithValue("@Spare2", IsOk);

                    #endregion

                    try
                    {
                        //con.Open();


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

        private int insertRowOrders(string PO, string Date, string EndUser, string Equipment, string VendorName, string JobNumber, string CustomerPO,
            string VendorNumber, string SalesAss, string SoldTo, string Street1, string Street2, string City, string State, string Zip,
            string ShipTo, string ShipStreet1, string ShipStreet2, string ShipCity, string ShipState, string ShipZip,
            string Carrier, string ShipDate, string IsComOrder, string IsComPaid, string Grinder, string SerialNo, string PumpStk,
            string ReqDate, string SchedShip, string PODate, string POShipVia,
            string TrackDate1, string TrackBy1, string TrackSource1, string TrackNote1,
            string TrackDate2, string TrackBy2, string TrackSource2, string TrackNote2,
            string TrackDate3, string TrackBy3, string TrackSource3, string TrackNote3,
            string TrackDate4, string TrackBy4, string TrackSource4, string TrackNote4,
            string TrackDate5, string TrackBy5, string TrackSource5, string TrackNote5,
            string TrackDate6, string TrackBy6, string TrackSource6, string TrackNote6,
            string TrackDate7, string TrackBy7, string TrackSource7, string TrackNote7,
            string TrackDate8, string TrackBy8, string TrackSource8, string TrackNote8,
            string TrackDate9, string TrackBy9, string TrackSource9, string TrackNote9,
            string TrackDate10, string TrackBy10, string TrackSource10, string TrackNote10,
            string TrackDate11, string TrackBy11, string TrackSource11, string TrackNote11,
            string TrackDate12, string TrackBy12, string TrackSource12, string TrackNote12,
            string TrackDate13, string TrackBy13, string TrackSource13, string TrackNote13,
            string TrackDate14, string TrackBy14, string TrackSource14, string TrackNote14,
            string TrackDate15, string TrackBy15, string TrackSource15, string TrackNote15,
            string TrackDate16, string TrackBy16, string TrackSource16, string TrackNote16,
            string TrackDate17, string TrackBy17, string TrackSource17, string TrackNote17,
            string TrackDate18, string TrackBy18, string TrackSource18, string TrackNote18,
            string QuotePrice, string Credit, string Freight, string ShopTime, string TotalCost, string GrossProfit, string Profit,
            string Description,
            string Quant1, string Descr1, string Costs1, string ECost1,
            string Quant2, string Descr2, string Costs2, string ECost2,
            string Quant3, string Descr3, string Costs3, string ECost3,
            string Quant4, string Descr4, string Costs4, string ECost4,
            string Quant5, string Descr5, string Costs5, string ECost5,
            string Quant6, string Descr6, string Costs6, string ECost6,
            string Quant7, string Descr7, string Costs7, string ECost7,
            string Quant8, string Descr8, string Costs8, string ECost8,
            string Quant9, string Descr9, string Costs9, string ECost9,
            string Quant10, string Descr10, string Costs10, string ECost10,
            string Quant11, string Descr11, string Costs11, string ECost11,
            string Quant12, string Descr12, string Costs12, string ECost12,
            string Quant13, string Descr13, string Costs13, string ECost13,
            string Quant14, string Descr14, string Costs14, string ECost14,
            string Quant15, string Descr15, string Costs15, string ECost15,
            string Quant16, string Descr16, string Costs16, string ECost16,
            string Quant17, string Descr17, string Costs17, string ECost17,
            string Quant18, string Descr18, string Costs18, string ECost18,
            string Quant19, string Descr19, string Costs19, string ECost19,
            string Quant20, string Descr20, string Costs20, string ECost20,
            string Quant21, string Descr21, string Costs21, string ECost21,
            string Quant22, string Descr22, string Costs22, string ECost22,
            string Quant23, string Descr23, string Costs23, string ECost23,
            string InvInstructions, string InvNotes, string VendorNotes, string AccNotes, string CrMemo, string InvNumber, string InvDate, string Status,
            string CheckNumbers, string CheckDates,
            string ComDate1, string ComCheckNumber1, string ComPaid1,
            string ComDate2, string ComCheckNumber2, string ComPaid2,
            string ComDate3, string ComCheckNumber3, string ComPaid3,
            string ComDate4, string ComCheckNumber4, string ComPaid4,
            string ComDate5, string ComCheckNumber5, string ComPaid5,
            string ComAmount, string ComBalance, string DeliveryNotes, string PONotes, string IsOk)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region insert string for OrderTable

                    com.CommandText =
                    "INSERT INTO OrderTable (PO, Date, EndUser, Equipment, VendorName, JobNumber, CustomerPO, " +
                    "VendorNumber, SalesAss, SoldTo, Street1, Street2, City, State, Zip, " +
                    "ShipTo, ShipStreet1, ShipStreet2, ShipCity, ShipState, ShipZip, " +
                    "Carrier, ShipDate, IsComOrder, IsComPaid, Grinder, SerialNo, PumpStk, " +
                    "ReqDate, SchedShip, PODate, POShipVia, " +
                    "TrackDate1, TrackBy1, TrackSource1, TrackNote1, " +
                    "TrackDate2, TrackBy2, TrackSource2, TrackNote2, " +
                    "TrackDate3, TrackBy3, TrackSource3, TrackNote3, " +
                    "TrackDate4, TrackBy4, TrackSource4, TrackNote4, " +
                    "TrackDate5, TrackBy5, TrackSource5, TrackNote5, " +
                    "TrackDate6, TrackBy6, TrackSource6, TrackNote6, " +
                    "TrackDate7, TrackBy7, TrackSource7, TrackNote7, " +
                    "TrackDate8, TrackBy8, TrackSource8, TrackNote8, " +
                    "TrackDate9, TrackBy9, TrackSource9, TrackNote9, " +
                    "TrackDate10, TrackBy10, TrackSource10, TrackNote10, " +
                    "TrackDate11, TrackBy11, TrackSource11, TrackNote11, " +
                    "TrackDate12, TrackBy12, TrackSource12, TrackNote12, " +
                    "TrackDate13, TrackBy13, TrackSource13, TrackNote13, " +
                    "TrackDate14, TrackBy14, TrackSource14, TrackNote14, " +
                    "TrackDate15, TrackBy15, TrackSource15, TrackNote15, " +
                    "TrackDate16, TrackBy16, TrackSource16, TrackNote16, " +
                    "TrackDate17, TrackBy17, TrackSource17, TrackNote17, " +
                    "TrackDate18, TrackBy18, TrackSource18, TrackNote18, " +
                    "QuotePrice, Credit, Freight, ShopTime, TotalCost, GrossProfit, Profit, " +
                    "Description, " +
                    "Quant1, Descr1, Costs1, ECost1, " +
                    "Quant2, Descr2, Costs2, ECost2, " +
                    "Quant3, Descr3, Costs3, ECost3, " +
                    "Quant4, Descr4, Costs4, ECost4, " +
                    "Quant5, Descr5, Costs5, ECost5, " +
                    "Quant6, Descr6, Costs6, ECost6, " +
                    "Quant7, Descr7, Costs7, ECost7, " +
                    "Quant8, Descr8, Costs8, ECost8, " +
                    "Quant9, Descr9, Costs9, ECost9, " +
                    "Quant10, Descr10, Costs10, ECost10, " +
                    "Quant11, Descr11, Costs11, ECost11, " +
                    "Quant12, Descr12, Costs12, ECost12, " +
                    "Quant13, Descr13, Costs13, ECost13, " +
                    "Quant14, Descr14, Costs14, ECost14, " +
                    "Quant15, Descr15, Costs15, ECost15, " +
                    "Quant16, Descr16, Costs16, ECost16, " +
                    "Quant17, Descr17, Costs17, ECost17, " +
                    "Quant18, Descr18, Costs18, ECost18, " +
                    "Quant19, Descr19, Costs19, ECost19, " +
                    "Quant20, Descr20, Costs20, ECost20, " +
                    "Quant21, Descr21, Costs21, ECost21, " +
                    "Quant22, Descr22, Costs22, ECost22, " +
                    "Quant23, Descr23, Costs23, ECost23, " +
                    "InvInstructions, InvNotes, VendorNotes, CrMemo, InvNumber, InvDate, Status, " +
                    "CheckNumbers, CheckDates, " +
                    "ComDate1, ComCheckNumber1, ComPaid1, " +
                    "ComDate2, ComCheckNumber2, ComPaid2, " +
                    "ComDate3, ComCheckNumber3, ComPaid3, " +
                    "ComDate4, ComCheckNumber4, ComPaid4, " +
                    "ComDate5, ComCheckNumber5, ComPaid5, " +
                    "ComAmount,  ComBalance, DeliveryNotes, PONotes, Spare1, Spare2) VALUES " +

                    "(@PO, @Date, @EndUser, @Equipment, @VendorName, @JobNumber, @CustomerPO, " +
                    "@VendorNumber, @SalesAss, @SoldTo, @Street1, @Street2, @City, @State, @Zip, " +
                    "@ShipTo, @ShipStreet1, @ShipStreet2, @ShipCity, @ShipState, @ShipZip, " +
                    "@Carrier, @ShipDate, @IsComOrder, @IsComPaid, @Grinder, @SerialNo, @PumpStk, " +
                    "@ReqDate, @SchedShip, @PODate, @POShipVia, " +
                    "@TrackDate1, @TrackBy1, @TrackSource1, @TrackNote1, " +
                    "@TrackDate2, @TrackBy2, @TrackSource2, @TrackNote2, " +
                    "@TrackDate3, @TrackBy3, @TrackSource3, @TrackNote3, " +
                    "@TrackDate4, @TrackBy4, @TrackSource4, @TrackNote4, " +
                    "@TrackDate5, @TrackBy5, @TrackSource5, @TrackNote5, " +
                    "@TrackDate6, @TrackBy6, @TrackSource6, @TrackNote6, " +
                    "@TrackDate7, @TrackBy7, @TrackSource7, @TrackNote7, " +
                    "@TrackDate8, @TrackBy8, @TrackSource8, @TrackNote8, " +
                    "@TrackDate9, @TrackBy9, @TrackSource9, @TrackNote9, " +
                    "@TrackDate10, @TrackBy10, @TrackSource10, @TrackNote10, " +
                    "@TrackDate11, @TrackBy11, @TrackSource11, @TrackNote11, " +
                    "@TrackDate12, @TrackBy12, @TrackSource12, @TrackNote12, " +
                    "@TrackDate13, @TrackBy13, @TrackSource13, @TrackNote13, " +
                    "@TrackDate14, @TrackBy14, @TrackSource14, @TrackNote14, " +
                    "@TrackDate15, @TrackBy15, @TrackSource15, @TrackNote15, " +
                    "@TrackDate16, @TrackBy16, @TrackSource16, @TrackNote16, " +
                    "@TrackDate17, @TrackBy17, @TrackSource17, @TrackNote17, " +
                    "@TrackDate18, @TrackBy18, @TrackSource18, @TrackNote18, " +
                    "@QuotePrice, @Credit, @Freight, @ShopTime, @TotalCost, @GrossProfit, @Profit, " +
                    "@Description, " +
                    "@Quant1, @Descr1, @Costs1, @ECost1, " +
                    "@Quant2, @Descr2, @Costs2, @ECost2, " +
                    "@Quant3, @Descr3, @Costs3, @ECost3, " +
                    "@Quant4, @Descr4, @Costs4, @ECost4, " +
                    "@Quant5, @Descr5, @Costs5, @ECost5, " +
                    "@Quant6, @Descr6, @Costs6, @ECost6, " +
                    "@Quant7, @Descr7, @Costs7, @ECost7, " +
                    "@Quant8, @Descr8, @Costs8, @ECost8, " +
                    "@Quant9, @Descr9, @Costs9, @ECost9, " +
                    "@Quant10, @Descr10, @Costs10, @ECost10, " +
                    "@Quant11, @Descr11, @Costs11, @ECost11, " +
                    "@Quant12, @Descr12, @Costs12, @ECost12, " +
                    "@Quant13, @Descr13, @Costs13, @ECost13, " +
                    "@Quant14, @Descr14, @Costs14, @ECost14, " +
                    "@Quant15, @Descr15, @Costs15, @ECost15, " +
                    "@Quant16, @Descr16, @Costs16, @ECost16, " +
                    "@Quant17, @Descr17, @Costs17, @ECost17, " +
                    "@Quant18, @Descr18, @Costs18, @ECost18, " +
                    "@Quant19, @Descr19, @Costs19, @ECost19, " +
                    "@Quant20, @Descr20, @Costs20, @ECost20, " +
                    "@Quant21, @Descr21, @Costs21, @ECost21, " +
                    "@Quant22, @Descr22, @Costs22, @ECost22, " +
                    "@Quant23, @Descr23, @Costs23, @ECost23, " +
                    "@InvInstructions, @InvNotes, @VendorNotes, @CrMemo, @InvNumber, @InvDate, @Status, " +
                    "@CheckNumbers, @CheckDates, " +
                    "@ComDate1, @ComCheckNumber1, @ComPaid1, " +
                    "@ComDate2, @ComCheckNumber2, @ComPaid2, " +
                    "@ComDate3, @ComCheckNumber3, @ComPaid3, " +
                    "@ComDate4, @ComCheckNumber4, @ComPaid4, " +
                    "@ComDate5, @ComCheckNumber5, @ComPaid5, " +
                    "@ComAmount, @ComBalance, @DeliveryNotes, @PONotes, @Spare1, @Spare2)";

                    #endregion

                    #region Parameters for OrderTable

                    com.Parameters.AddWithValue("@PO", PO);
                    com.Parameters.AddWithValue("@Date", Date);
                    com.Parameters.AddWithValue("@EndUser", EndUser);
                    com.Parameters.AddWithValue("@Equipment", Equipment);
                    com.Parameters.AddWithValue("@VendorName", VendorName);
                    com.Parameters.AddWithValue("@JobNumber", JobNumber);
                    com.Parameters.AddWithValue("@CustomerPO", CustomerPO);
                    com.Parameters.AddWithValue("@VendorNumber", VendorNumber);
                    com.Parameters.AddWithValue("@SalesAss", SalesAss);
                    com.Parameters.AddWithValue("@SoldTo", SoldTo);
                    com.Parameters.AddWithValue("@Street1", Street1);
                    com.Parameters.AddWithValue("@Street2", Street2);
                    com.Parameters.AddWithValue("@City", City);
                    com.Parameters.AddWithValue("@State", State);
                    com.Parameters.AddWithValue("@Zip", Zip);
                    com.Parameters.AddWithValue("@ShipTo", ShipTo);
                    com.Parameters.AddWithValue("@ShipStreet1", ShipStreet1);
                    com.Parameters.AddWithValue("@ShipStreet2", ShipStreet2);
                    com.Parameters.AddWithValue("@ShipCity", ShipCity);
                    com.Parameters.AddWithValue("@ShipState", ShipState);
                    com.Parameters.AddWithValue("@ShipZip", ShipZip);
                    com.Parameters.AddWithValue("@Carrier", Carrier);
                    com.Parameters.AddWithValue("@ShipDate", ShipDate);
                    com.Parameters.AddWithValue("@IsComOrder", IsComOrder);
                    com.Parameters.AddWithValue("@IsComPaid", IsComPaid);
                    com.Parameters.AddWithValue("@Grinder", Grinder);
                    com.Parameters.AddWithValue("@SerialNo", SerialNo);
                    com.Parameters.AddWithValue("@PumpStk", PumpStk);
                    com.Parameters.AddWithValue("@ReqDate", ReqDate);
                    com.Parameters.AddWithValue("@SchedShip", SchedShip);
                    com.Parameters.AddWithValue("@PODate", PODate);
                    com.Parameters.AddWithValue("@POShipVia", POShipVia);

                    com.Parameters.AddWithValue("@TrackDate1", TrackDate1);
                    com.Parameters.AddWithValue("@TrackBy1", TrackBy1);
                    com.Parameters.AddWithValue("@TrackSource1", TrackSource1);
                    com.Parameters.AddWithValue("@TrackNote1", TrackNote1);

                    com.Parameters.AddWithValue("@TrackDate2", TrackDate2);
                    com.Parameters.AddWithValue("@TrackBy2", TrackBy2);
                    com.Parameters.AddWithValue("@TrackSource2", TrackSource2);
                    com.Parameters.AddWithValue("@TrackNote2", TrackNote2);

                    com.Parameters.AddWithValue("@TrackDate3", TrackDate3);
                    com.Parameters.AddWithValue("@TrackBy3", TrackBy3);
                    com.Parameters.AddWithValue("@TrackSource3", TrackSource3);
                    com.Parameters.AddWithValue("@TrackNote3", TrackNote3);

                    com.Parameters.AddWithValue("@TrackDate4", TrackDate4);
                    com.Parameters.AddWithValue("@TrackBy4", TrackBy4);
                    com.Parameters.AddWithValue("@TrackSource4", TrackSource4);
                    com.Parameters.AddWithValue("@TrackNote4", TrackNote4);

                    com.Parameters.AddWithValue("@TrackDate5", TrackDate5);
                    com.Parameters.AddWithValue("@TrackBy5", TrackBy5);
                    com.Parameters.AddWithValue("@TrackSource5", TrackSource5);
                    com.Parameters.AddWithValue("@TrackNote5", TrackNote5);

                    com.Parameters.AddWithValue("@TrackDate6", TrackDate6);
                    com.Parameters.AddWithValue("@TrackBy6", TrackBy6);
                    com.Parameters.AddWithValue("@TrackSource6", TrackSource6);
                    com.Parameters.AddWithValue("@TrackNote6", TrackNote6);

                    com.Parameters.AddWithValue("@TrackDate7", TrackDate7);
                    com.Parameters.AddWithValue("@TrackBy7", TrackBy7);
                    com.Parameters.AddWithValue("@TrackSource7", TrackSource7);
                    com.Parameters.AddWithValue("@TrackNote7", TrackNote7);

                    com.Parameters.AddWithValue("@TrackDate8", TrackDate8);
                    com.Parameters.AddWithValue("@TrackBy8", TrackBy8);
                    com.Parameters.AddWithValue("@TrackSource8", TrackSource8);
                    com.Parameters.AddWithValue("@TrackNote8", TrackNote8);

                    com.Parameters.AddWithValue("@TrackDate9", TrackDate9);
                    com.Parameters.AddWithValue("@TrackBy9", TrackBy9);
                    com.Parameters.AddWithValue("@TrackSource9", TrackSource9);
                    com.Parameters.AddWithValue("@TrackNote9", TrackNote9);

                    com.Parameters.AddWithValue("@TrackDate10", TrackDate10);
                    com.Parameters.AddWithValue("@TrackBy10", TrackBy10);
                    com.Parameters.AddWithValue("@TrackSource10", TrackSource10);
                    com.Parameters.AddWithValue("@TrackNote10", TrackNote10);

                    com.Parameters.AddWithValue("@TrackDate11", TrackDate11);
                    com.Parameters.AddWithValue("@TrackBy11", TrackBy11);
                    com.Parameters.AddWithValue("@TrackSource11", TrackSource11);
                    com.Parameters.AddWithValue("@TrackNote11", TrackNote11);

                    com.Parameters.AddWithValue("@TrackDate12", TrackDate12);
                    com.Parameters.AddWithValue("@TrackBy12", TrackBy12);
                    com.Parameters.AddWithValue("@TrackSource12", TrackSource12);
                    com.Parameters.AddWithValue("@TrackNote12", TrackNote12);

                    com.Parameters.AddWithValue("@TrackDate13", TrackDate13);
                    com.Parameters.AddWithValue("@TrackBy13", TrackBy13);
                    com.Parameters.AddWithValue("@TrackSource13", TrackSource13);
                    com.Parameters.AddWithValue("@TrackNote13", TrackNote13);

                    com.Parameters.AddWithValue("@TrackDate14", TrackDate14);
                    com.Parameters.AddWithValue("@TrackBy14", TrackBy14);
                    com.Parameters.AddWithValue("@TrackSource14", TrackSource14);
                    com.Parameters.AddWithValue("@TrackNote14", TrackNote14);

                    com.Parameters.AddWithValue("@TrackDate15", TrackDate15);
                    com.Parameters.AddWithValue("@TrackBy15", TrackBy15);
                    com.Parameters.AddWithValue("@TrackSource15", TrackSource15);
                    com.Parameters.AddWithValue("@TrackNote15", TrackNote15);

                    com.Parameters.AddWithValue("@TrackDate16", TrackDate16);
                    com.Parameters.AddWithValue("@TrackBy16", TrackBy16);
                    com.Parameters.AddWithValue("@TrackSource16", TrackSource16);
                    com.Parameters.AddWithValue("@TrackNote16", TrackNote16);

                    com.Parameters.AddWithValue("@TrackDate17", TrackDate17);
                    com.Parameters.AddWithValue("@TrackBy17", TrackBy17);
                    com.Parameters.AddWithValue("@TrackSource17", TrackSource17);
                    com.Parameters.AddWithValue("@TrackNote17", TrackNote17);

                    com.Parameters.AddWithValue("@TrackDate18", TrackDate18);
                    com.Parameters.AddWithValue("@TrackBy18", TrackBy18);
                    com.Parameters.AddWithValue("@TrackSource18", TrackSource18);
                    com.Parameters.AddWithValue("@TrackNote18", TrackNote18);
                    com.Parameters.AddWithValue("@QuotePrice", QuotePrice);
                    com.Parameters.AddWithValue("@Credit", Credit);
                    com.Parameters.AddWithValue("@Freight", Freight);
                    com.Parameters.AddWithValue("@ShopTime", ShopTime);
                    com.Parameters.AddWithValue("@TotalCost", TotalCost);
                    com.Parameters.AddWithValue("@GrossProfit", GrossProfit);
                    com.Parameters.AddWithValue("@Profit", Profit);
                    com.Parameters.AddWithValue("@Description", Description);

                    com.Parameters.AddWithValue("@Quant1", Quant1);
                    com.Parameters.AddWithValue("@Descr1", Descr1);
                    com.Parameters.AddWithValue("@Costs1", Costs1);
                    com.Parameters.AddWithValue("@ECost1", ECost1);
                    com.Parameters.AddWithValue("@Quant2", Quant2);
                    com.Parameters.AddWithValue("@Descr2", Descr2);
                    com.Parameters.AddWithValue("@Costs2", Costs2);
                    com.Parameters.AddWithValue("@ECost2", ECost2);
                    com.Parameters.AddWithValue("@Quant3", Quant3);
                    com.Parameters.AddWithValue("@Descr3", Descr3);
                    com.Parameters.AddWithValue("@Costs3", Costs3);
                    com.Parameters.AddWithValue("@ECost3", ECost3);
                    com.Parameters.AddWithValue("@Quant4", Quant4);
                    com.Parameters.AddWithValue("@Descr4", Descr4);
                    com.Parameters.AddWithValue("@Costs4", Costs4);
                    com.Parameters.AddWithValue("@ECost4", ECost4);
                    com.Parameters.AddWithValue("@Quant5", Quant5);
                    com.Parameters.AddWithValue("@Descr5", Descr5);
                    com.Parameters.AddWithValue("@Costs5", Costs5);
                    com.Parameters.AddWithValue("@ECost5", ECost5);
                    com.Parameters.AddWithValue("@Quant6", Quant6);
                    com.Parameters.AddWithValue("@Descr6", Descr6);
                    com.Parameters.AddWithValue("@Costs6", Costs6);
                    com.Parameters.AddWithValue("@ECost6", ECost6);
                    com.Parameters.AddWithValue("@Quant7", Quant7);
                    com.Parameters.AddWithValue("@Descr7", Descr7);
                    com.Parameters.AddWithValue("@Costs7", Costs7);
                    com.Parameters.AddWithValue("@ECost7", ECost7);
                    com.Parameters.AddWithValue("@Quant8", Quant8);
                    com.Parameters.AddWithValue("@Descr8", Descr8);
                    com.Parameters.AddWithValue("@Costs8", Costs8);
                    com.Parameters.AddWithValue("@ECost8", ECost8);
                    com.Parameters.AddWithValue("@Quant9", Quant9);
                    com.Parameters.AddWithValue("@Descr9", Descr9);
                    com.Parameters.AddWithValue("@Costs9", Costs9);
                    com.Parameters.AddWithValue("@ECost9", ECost9);
                    com.Parameters.AddWithValue("@Quant10", Quant10);
                    com.Parameters.AddWithValue("@Descr10", Descr10);
                    com.Parameters.AddWithValue("@Costs10", Costs10);
                    com.Parameters.AddWithValue("@ECost10", ECost10);
                    com.Parameters.AddWithValue("@Quant11", Quant11);
                    com.Parameters.AddWithValue("@Descr11", Descr11);
                    com.Parameters.AddWithValue("@Costs11", Costs11);
                    com.Parameters.AddWithValue("@ECost11", ECost11);
                    com.Parameters.AddWithValue("@Quant12", Quant12);
                    com.Parameters.AddWithValue("@Descr12", Descr12);
                    com.Parameters.AddWithValue("@Costs12", Costs12);
                    com.Parameters.AddWithValue("@ECost12", ECost12);
                    com.Parameters.AddWithValue("@Quant13", Quant13);
                    com.Parameters.AddWithValue("@Descr13", Descr13);
                    com.Parameters.AddWithValue("@Costs13", Costs13);
                    com.Parameters.AddWithValue("@ECost13", ECost13);
                    com.Parameters.AddWithValue("@Quant14", Quant14);
                    com.Parameters.AddWithValue("@Descr14", Descr14);
                    com.Parameters.AddWithValue("@Costs14", Costs14);
                    com.Parameters.AddWithValue("@ECost14", ECost14);
                    com.Parameters.AddWithValue("@Quant15", Quant15);
                    com.Parameters.AddWithValue("@Descr15", Descr15);
                    com.Parameters.AddWithValue("@Costs15", Costs15);
                    com.Parameters.AddWithValue("@ECost15", ECost15);
                    com.Parameters.AddWithValue("@Quant16", Quant16);
                    com.Parameters.AddWithValue("@Descr16", Descr16);
                    com.Parameters.AddWithValue("@Costs16", Costs16);
                    com.Parameters.AddWithValue("@ECost16", ECost16);
                    com.Parameters.AddWithValue("@Quant17", Quant17);
                    com.Parameters.AddWithValue("@Descr17", Descr17);
                    com.Parameters.AddWithValue("@Costs17", Costs17);
                    com.Parameters.AddWithValue("@ECost17", ECost17);
                    com.Parameters.AddWithValue("@Quant18", Quant18);
                    com.Parameters.AddWithValue("@Descr18", Descr18);
                    com.Parameters.AddWithValue("@Costs18", Costs18);
                    com.Parameters.AddWithValue("@ECost18", ECost18);
                    com.Parameters.AddWithValue("@Quant19", Quant19);
                    com.Parameters.AddWithValue("@Descr19", Descr19);
                    com.Parameters.AddWithValue("@Costs19", Costs19);
                    com.Parameters.AddWithValue("@ECost19", ECost19);
                    com.Parameters.AddWithValue("@Quant20", Quant20);
                    com.Parameters.AddWithValue("@Descr20", Descr20);
                    com.Parameters.AddWithValue("@Costs20", Costs20);
                    com.Parameters.AddWithValue("@ECost20", ECost20);
                    com.Parameters.AddWithValue("@Quant21", Quant21);
                    com.Parameters.AddWithValue("@Descr21", Descr21);
                    com.Parameters.AddWithValue("@Costs21", Costs21);
                    com.Parameters.AddWithValue("@ECost21", ECost21);
                    com.Parameters.AddWithValue("@Quant22", Quant22);
                    com.Parameters.AddWithValue("@Descr22", Descr22);
                    com.Parameters.AddWithValue("@Costs22", Costs22);
                    com.Parameters.AddWithValue("@ECost22", ECost22);
                    com.Parameters.AddWithValue("@Quant23", Quant23);
                    com.Parameters.AddWithValue("@Descr23", Descr23);
                    com.Parameters.AddWithValue("@Costs23", Costs23);
                    com.Parameters.AddWithValue("@ECost23", ECost23);

                    com.Parameters.AddWithValue("@InvInstructions", InvInstructions);
                    com.Parameters.AddWithValue("@InvNotes", InvNotes);
                    com.Parameters.AddWithValue("@VendorNotes", VendorNotes);
                    com.Parameters.AddWithValue("@Spare1", AccNotes);
                    com.Parameters.AddWithValue("@CrMemo", CrMemo);
                    com.Parameters.AddWithValue("@InvNumber", InvNumber);
                    com.Parameters.AddWithValue("@InvDate", InvDate);
                    com.Parameters.AddWithValue("@Status", Status);
                    com.Parameters.AddWithValue("@CheckNumbers", CheckNumbers);
                    com.Parameters.AddWithValue("@CheckDates", CheckDates);

                    com.Parameters.AddWithValue("@ComDate1", ComDate1);
                    com.Parameters.AddWithValue("@ComCheckNumber1", ComCheckNumber1);
                    com.Parameters.AddWithValue("@ComPaid1", ComPaid1);
                    com.Parameters.AddWithValue("@ComDate2", ComDate2);
                    com.Parameters.AddWithValue("@ComCheckNumber2", ComCheckNumber2);
                    com.Parameters.AddWithValue("@ComPaid2", ComPaid2);
                    com.Parameters.AddWithValue("@ComDate3", ComDate3);
                    com.Parameters.AddWithValue("@ComCheckNumber3", ComCheckNumber3);
                    com.Parameters.AddWithValue("@ComPaid3", ComPaid3);
                    com.Parameters.AddWithValue("@ComDate4", ComDate4);
                    com.Parameters.AddWithValue("@ComCheckNumber4", ComCheckNumber4);
                    com.Parameters.AddWithValue("@ComPaid4", ComPaid4);
                    com.Parameters.AddWithValue("@ComDate5", ComDate5);
                    com.Parameters.AddWithValue("@ComCheckNumber5", ComCheckNumber5);
                    com.Parameters.AddWithValue("@ComPaid5", ComPaid5);

                    com.Parameters.AddWithValue("@ComAmount", ComAmount);
                    com.Parameters.AddWithValue("@ComBalance", ComBalance);

                    com.Parameters.AddWithValue("@DeliveryNotes", DeliveryNotes);
                    com.Parameters.AddWithValue("@PONotes", PONotes);
                    com.Parameters.AddWithValue("@Spare2", IsOk);

                    #endregion

                    try
                    {
                        //con.Open();
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

        private void readOrderAndUpdateGUI(string PO, int row)
        {
            _log.append("** " + PO + " row " + row);
            _log.append("readOrderAndUpdateGUI start");

            string thePO;
            string Date; string EndUser; string Equipment; string VendorName; string JobName; string CustomerPO;
            string VendorNumber; string SalesAss; string SoldTo; string Street1; string Street2; string City; string State; string Zip;
            string ShipTo; string ShipStreet1; string ShipStreet2; string ShipCity; string ShipState; string ShipZip;
            string Carrier; string ShipDate; string IsComOrder; string IsComPaid; string Grinder; string SerialNo; string PumpStk;
            string ReqDate; string SchedShip; string PODate; string POShipVia;
            string TrackDate1; string TrackBy1; string TrackSource1; string TrackNote1;
            string TrackDate2; string TrackBy2; string TrackSource2; string TrackNote2;
            string TrackDate3; string TrackBy3; string TrackSource3; string TrackNote3;
            string TrackDate4; string TrackBy4; string TrackSource4; string TrackNote4;
            string TrackDate5; string TrackBy5; string TrackSource5; string TrackNote5;
            string TrackDate6; string TrackBy6; string TrackSource6; string TrackNote6;
            string TrackDate7; string TrackBy7; string TrackSource7; string TrackNote7;
            string TrackDate8; string TrackBy8; string TrackSource8; string TrackNote8;
            string TrackDate9; string TrackBy9; string TrackSource9; string TrackNote9;
            string TrackDate10; string TrackBy10; string TrackSource10; string TrackNote10;
            string TrackDate11; string TrackBy11; string TrackSource11; string TrackNote11;
            string TrackDate12; string TrackBy12; string TrackSource12; string TrackNote12;
            string TrackDate13; string TrackBy13; string TrackSource13; string TrackNote13;
            string TrackDate14; string TrackBy14; string TrackSource14; string TrackNote14;
            string TrackDate15; string TrackBy15; string TrackSource15; string TrackNote15;
            string TrackDate16; string TrackBy16; string TrackSource16; string TrackNote16;
            string TrackDate17; string TrackBy17; string TrackSource17; string TrackNote17;
            string TrackDate18; string TrackBy18; string TrackSource18; string TrackNote18;
            string QuotePrice; string Credit; string Freight; string ShopTime; string TotalCost; string GrossProfit; string Profit;
            string Description;
            string Quant1; string Descr1; string Costs1; string ECost1;
            string Quant2; string Descr2; string Costs2; string ECost2;
            string Quant3; string Descr3; string Costs3; string ECost3;
            string Quant4; string Descr4; string Costs4; string ECost4;
            string Quant5; string Descr5; string Costs5; string ECost5;
            string Quant6; string Descr6; string Costs6; string ECost6;
            string Quant7; string Descr7; string Costs7; string ECost7;
            string Quant8; string Descr8; string Costs8; string ECost8;
            string Quant9; string Descr9; string Costs9; string ECost9;
            string Quant10; string Descr10; string Costs10; string ECost10;
            string Quant11; string Descr11; string Costs11; string ECost11;
            string Quant12; string Descr12; string Costs12; string ECost12;
            string Quant13; string Descr13; string Costs13; string ECost13;
            string Quant14; string Descr14; string Costs14; string ECost14;
            string Quant15; string Descr15; string Costs15; string ECost15;
            string Quant16; string Descr16; string Costs16; string ECost16;
            string Quant17; string Descr17; string Costs17; string ECost17;
            string Quant18; string Descr18; string Costs18; string ECost18;
            string Quant19; string Descr19; string Costs19; string ECost19;
            string Quant20; string Descr20; string Costs20; string ECost20;
            string Quant21; string Descr21; string Costs21; string ECost21;
            string Quant22; string Descr22; string Costs22; string ECost22;
            string Quant23; string Descr23; string Costs23; string ECost23;
            string InvInstructions; string InvNotes; string VendorNotes; string AccNotes; string CrMemo; string InvNumber; string InvDate; string Status;
            string CheckNumbers; string CheckDates;
            string ComDate1; string ComCheckNumber1; string ComPaid1;
            string ComDate2; string ComCheckNumber2; string ComPaid2;
            string ComDate3; string ComCheckNumber3; string ComPaid3;
            string ComDate4; string ComCheckNumber4; string ComPaid4;
            string ComDate5; string ComCheckNumber5; string ComPaid5;
            string ComAmount; string ComBalance; string DeliveryNotes; string PONotes; string IsOk;

            readRowOrders(PO, row, out thePO, out Date, out EndUser, out Equipment, out VendorName, out JobName, out CustomerPO,
            out VendorNumber, out SalesAss, out SoldTo, out Street1, out Street2, out City, out State, out Zip,
            out ShipTo, out ShipStreet1, out ShipStreet2, out ShipCity, out ShipState, out ShipZip,
            out Carrier, out ShipDate, out IsComOrder, out IsComPaid, out Grinder, out SerialNo, out PumpStk,
            out ReqDate, out SchedShip, out PODate, out POShipVia,
            out TrackDate1, out TrackBy1, out TrackSource1, out TrackNote1,
            out TrackDate2, out TrackBy2, out TrackSource2, out TrackNote2,
            out TrackDate3, out TrackBy3, out TrackSource3, out TrackNote3,
            out TrackDate4, out TrackBy4, out TrackSource4, out TrackNote4,
            out TrackDate5, out TrackBy5, out TrackSource5, out TrackNote5,
            out TrackDate6, out TrackBy6, out TrackSource6, out TrackNote6,
            out TrackDate7, out TrackBy7, out TrackSource7, out TrackNote7,
            out TrackDate8, out TrackBy8, out TrackSource8, out TrackNote8,
            out TrackDate9, out TrackBy9, out TrackSource9, out TrackNote9,
            out TrackDate10, out TrackBy10, out TrackSource10, out TrackNote10,
            out TrackDate11, out TrackBy11, out TrackSource11, out TrackNote11,
            out TrackDate12, out TrackBy12, out TrackSource12, out TrackNote12,
            out TrackDate13, out TrackBy13, out TrackSource13, out TrackNote13,
            out TrackDate14, out TrackBy14, out TrackSource14, out TrackNote14,
            out TrackDate15, out TrackBy15, out TrackSource15, out TrackNote15,
            out TrackDate16, out TrackBy16, out TrackSource16, out TrackNote16,
            out TrackDate17, out TrackBy17, out TrackSource17, out TrackNote17,
            out TrackDate18, out TrackBy18, out TrackSource18, out TrackNote18,
            out QuotePrice, out Credit, out Freight, out ShopTime, out TotalCost, out GrossProfit, out Profit,
            out Description,
            out Quant1, out Descr1, out Costs1, out ECost1,
            out Quant2, out Descr2, out Costs2, out ECost2,
            out Quant3, out Descr3, out Costs3, out ECost3,
            out Quant4, out Descr4, out Costs4, out ECost4,
            out Quant5, out Descr5, out Costs5, out ECost5,
            out Quant6, out Descr6, out Costs6, out ECost6,
            out Quant7, out Descr7, out Costs7, out ECost7,
            out Quant8, out Descr8, out Costs8, out ECost8,
            out Quant9, out Descr9, out Costs9, out ECost9,
            out Quant10, out Descr10, out Costs10, out ECost10,
            out Quant11, out Descr11, out Costs11, out ECost11,
            out Quant12, out Descr12, out Costs12, out ECost12,
            out Quant13, out Descr13, out Costs13, out ECost13,
            out Quant14, out Descr14, out Costs14, out ECost14,
            out Quant15, out Descr15, out Costs15, out ECost15,
            out Quant16, out Descr16, out Costs16, out ECost16,
            out Quant17, out Descr17, out Costs17, out ECost17,
            out Quant18, out Descr18, out Costs18, out ECost18,
            out Quant19, out Descr19, out Costs19, out ECost19,
            out Quant20, out Descr20, out Costs20, out ECost20,
            out Quant21, out Descr21, out Costs21, out ECost21,
            out Quant22, out Descr22, out Costs22, out ECost22,
            out Quant23, out Descr23, out Costs23, out ECost23,
            out InvInstructions, out InvNotes, out VendorNotes, out AccNotes, out CrMemo, out InvNumber, out InvDate, out Status,
            out CheckNumbers, out CheckDates,
            out ComDate1, out ComCheckNumber1, out ComPaid1,
            out ComDate2, out ComCheckNumber2, out ComPaid2,
            out ComDate3, out ComCheckNumber3, out ComPaid3,
            out ComDate4, out ComCheckNumber4, out ComPaid4,
            out ComDate5, out ComCheckNumber5, out ComPaid5,
            out ComAmount, out ComBalance, out DeliveryNotes, out PONotes, out IsOk);

            #region update the GUI with above read params
            isDataLoadingOrders = true;

            this.textBoxPO.Text = thePO;
            this.textBoxDate.Text = Date;
            this.textBoxEndUser.Text = EndUser;
            this.textBoxEquipment.Text = Equipment;
            this.textBoxVendorName.Text = VendorName;
            this.textBoxJobName.Text = JobName;
            this.textBoxCustomerPO.Text = CustomerPO;
            this.textBoxVendorNumber.Text = VendorNumber;
            this.comboBoxSalesAss.Text = SalesAss;

            this.textBoxSoldTo.Text = SoldTo;
            this.textBoxSoldToReadOnly.Text = SoldTo; // mirror

            this.textBoxStreet1.Text = Street1;
            this.textBoxStreet2.Text = Street2;
            this.textBoxCity.Text = City;
            this.comboBoxSoldToState.Text = State;
            this.textBoxZip.Text = Zip;

            this.textBoxShipTo.Text = ShipTo;
            this.textBoxShipToStreet1.Text = ShipStreet1;
            this.textBoxShipToStreet2.Text = ShipStreet2;
            this.textBoxShipToCity.Text = ShipCity;
            this.comboBoxShipToState.Text = ShipState;
            this.textBoxShipToZip.Text = ShipZip;

            this.comboBoxCarrier.Text = Carrier;
            this.textBoxShipDate.Text = ShipDate;

            this.checkBoxOK.Checked = isStringTrue(IsOk);
            this.checkBoxComOrder.Checked = isStringTrue(IsComOrder);
            this.checkBoxComPaid.Checked = isStringTrue(IsComPaid);

            this.textBoxGrinder.Text = Grinder;
            this.textBoxSerialNumber.Text = SerialNo;
            this.textBoxPumpStk.Text = PumpStk;

            this.textBoxReqDate.Text = ReqDate;
            this.textBoxSchedShip.Text = SchedShip;
            this.textBoxPODate.Text = PODate;
            this.textBoxPOShipVia.Text = POShipVia;

            this.textBoxTrkDate1.Text = TrackDate1; this.comboBoxTrkBy1.Text = TrackBy1; this.comboBoxTrkSource1.Text = TrackSource1; this.textBoxTrkNotes1.Text = TrackNote1;
            this.textBoxTrkDate2.Text = TrackDate2; this.comboBoxTrkBy2.Text = TrackBy2; this.comboBoxTrkSource2.Text = TrackSource2; this.textBoxTrkNotes2.Text = TrackNote2;
            this.textBoxTrkDate3.Text = TrackDate3; this.comboBoxTrkBy3.Text = TrackBy3; this.comboBoxTrkSource3.Text = TrackSource3; this.textBoxTrkNotes3.Text = TrackNote3;
            this.textBoxTrkDate4.Text = TrackDate4; this.comboBoxTrkBy4.Text = TrackBy4; this.comboBoxTrkSource4.Text = TrackSource4; this.textBoxTrkNotes4.Text = TrackNote4;
            this.textBoxTrkDate5.Text = TrackDate5; this.comboBoxTrkBy5.Text = TrackBy5; this.comboBoxTrkSource5.Text = TrackSource5; this.textBoxTrkNotes5.Text = TrackNote5;
            this.textBoxTrkDate6.Text = TrackDate6; this.comboBoxTrkBy6.Text = TrackBy6; this.comboBoxTrkSource6.Text = TrackSource6; this.textBoxTrkNotes6.Text = TrackNote6;
            this.textBoxTrkDate7.Text = TrackDate7; this.comboBoxTrkBy7.Text = TrackBy7; this.comboBoxTrkSource7.Text = TrackSource7; this.textBoxTrkNotes7.Text = TrackNote7;
            this.textBoxTrkDate8.Text = TrackDate8; this.comboBoxTrkBy8.Text = TrackBy8; this.comboBoxTrkSource8.Text = TrackSource8; this.textBoxTrkNotes8.Text = TrackNote8;
            this.textBoxTrkDate9.Text = TrackDate9; this.comboBoxTrkBy9.Text = TrackBy9; this.comboBoxTrkSource9.Text = TrackSource9; this.textBoxTrkNotes9.Text = TrackNote9;
            this.textBoxTrkDate10.Text = TrackDate10; this.comboBoxTrkBy10.Text = TrackBy10; this.comboBoxTrkSource10.Text = TrackSource10; this.textBoxTrkNotes10.Text = TrackNote10;
            this.textBoxTrkDate11.Text = TrackDate11; this.comboBoxTrkBy11.Text = TrackBy11; this.comboBoxTrkSource11.Text = TrackSource11; this.textBoxTrkNotes11.Text = TrackNote11;
            this.textBoxTrkDate12.Text = TrackDate12; this.comboBoxTrkBy12.Text = TrackBy12; this.comboBoxTrkSource12.Text = TrackSource12; this.textBoxTrkNotes12.Text = TrackNote12;
            this.textBoxTrkDate13.Text = TrackDate13; this.comboBoxTrkBy13.Text = TrackBy13; this.comboBoxTrkSource13.Text = TrackSource13; this.textBoxTrkNotes13.Text = TrackNote13;
            this.textBoxTrkDate14.Text = TrackDate14; this.comboBoxTrkBy14.Text = TrackBy14; this.comboBoxTrkSource14.Text = TrackSource14; this.textBoxTrkNotes14.Text = TrackNote14;
            this.textBoxTrkDate15.Text = TrackDate15; this.comboBoxTrkBy15.Text = TrackBy15; this.comboBoxTrkSource15.Text = TrackSource15; this.textBoxTrkNotes15.Text = TrackNote15;
            this.textBoxTrkDate16.Text = TrackDate16; this.comboBoxTrkBy16.Text = TrackBy16; this.comboBoxTrkSource16.Text = TrackSource16; this.textBoxTrkNotes16.Text = TrackNote16;
            this.textBoxTrkDate17.Text = TrackDate17; this.comboBoxTrkBy17.Text = TrackBy17; this.comboBoxTrkSource17.Text = TrackSource17; this.textBoxTrkNotes17.Text = TrackNote17;
            this.textBoxTrkDate18.Text = TrackDate18; this.comboBoxTrkBy18.Text = TrackBy18; this.comboBoxTrkSource18.Text = TrackSource18; this.textBoxTrkNotes18.Text = TrackNote18;

            this.numericUpDownQuotePrice.Text = QuotePrice;
            this.numericUpDownCredit.Text = Credit;
            this.numericUpDownFreight.Text = Freight;
            this.numericUpDownShopTime.Text = ShopTime;
            this.numericUpDownTotalCost.Text = TotalCost;
            this.numericUpDownGrossProfit.Text = GrossProfit;
            this.numericUpDownProfit.Text = Profit;

            this.textBoxDescription.Text = Description;
            this.numericUpDownOrderCount1.Text = Quant1; this.textBoxOrderDescr1.Text = Descr1; this.numericUpDownOrderCost1.Text = Costs1; this.numericUpDownOrderECost1.Text = ECost1;
            this.numericUpDownOrderCount2.Text = Quant2; this.textBoxOrderDescr2.Text = Descr2; this.numericUpDownOrderCost2.Text = Costs2; this.numericUpDownOrderECost2.Text = ECost2;
            this.numericUpDownOrderCount3.Text = Quant3; this.textBoxOrderDescr3.Text = Descr3; this.numericUpDownOrderCost3.Text = Costs3; this.numericUpDownOrderECost3.Text = ECost3;
            this.numericUpDownOrderCount4.Text = Quant4; this.textBoxOrderDescr4.Text = Descr4; this.numericUpDownOrderCost4.Text = Costs4; this.numericUpDownOrderECost4.Text = ECost4;
            this.numericUpDownOrderCount5.Text = Quant5; this.textBoxOrderDescr5.Text = Descr5; this.numericUpDownOrderCost5.Text = Costs5; this.numericUpDownOrderECost5.Text = ECost5;
            this.numericUpDownOrderCount6.Text = Quant6; this.textBoxOrderDescr6.Text = Descr6; this.numericUpDownOrderCost6.Text = Costs6; this.numericUpDownOrderECost6.Text = ECost6;
            this.numericUpDownOrderCount7.Text = Quant7; this.textBoxOrderDescr7.Text = Descr7; this.numericUpDownOrderCost7.Text = Costs7; this.numericUpDownOrderECost7.Text = ECost7;
            this.numericUpDownOrderCount8.Text = Quant8; this.textBoxOrderDescr8.Text = Descr8; this.numericUpDownOrderCost8.Text = Costs8; this.numericUpDownOrderECost8.Text = ECost8;
            this.numericUpDownOrderCount9.Text = Quant9; this.textBoxOrderDescr9.Text = Descr9; this.numericUpDownOrderCost9.Text = Costs9; this.numericUpDownOrderECost9.Text = ECost9;
            this.numericUpDownOrderCount10.Text = Quant10; this.textBoxOrderDescr10.Text = Descr10; this.numericUpDownOrderCost10.Text = Costs10; this.numericUpDownOrderECost10.Text = ECost10;
            this.numericUpDownOrderCount11.Text = Quant11; this.textBoxOrderDescr11.Text = Descr11; this.numericUpDownOrderCost11.Text = Costs11; this.numericUpDownOrderECost11.Text = ECost11;
            this.numericUpDownOrderCount12.Text = Quant12; this.textBoxOrderDescr12.Text = Descr12; this.numericUpDownOrderCost12.Text = Costs12; this.numericUpDownOrderECost12.Text = ECost12;
            this.numericUpDownOrderCount13.Text = Quant13; this.textBoxOrderDescr13.Text = Descr13; this.numericUpDownOrderCost13.Text = Costs13; this.numericUpDownOrderECost13.Text = ECost13;
            this.numericUpDownOrderCount14.Text = Quant14; this.textBoxOrderDescr14.Text = Descr14; this.numericUpDownOrderCost14.Text = Costs14; this.numericUpDownOrderECost14.Text = ECost14;
            this.numericUpDownOrderCount15.Text = Quant15; this.textBoxOrderDescr15.Text = Descr15; this.numericUpDownOrderCost15.Text = Costs15; this.numericUpDownOrderECost15.Text = ECost15;
            this.numericUpDownOrderCount16.Text = Quant16; this.textBoxOrderDescr16.Text = Descr16; this.numericUpDownOrderCost16.Text = Costs16; this.numericUpDownOrderECost16.Text = ECost16;
            this.numericUpDownOrderCount17.Text = Quant17; this.textBoxOrderDescr17.Text = Descr17; this.numericUpDownOrderCost17.Text = Costs17; this.numericUpDownOrderECost17.Text = ECost17;
            this.numericUpDownOrderCount18.Text = Quant18; this.textBoxOrderDescr18.Text = Descr18; this.numericUpDownOrderCost18.Text = Costs18; this.numericUpDownOrderECost18.Text = ECost18;
            this.numericUpDownOrderCount19.Text = Quant19; this.textBoxOrderDescr19.Text = Descr19; this.numericUpDownOrderCost19.Text = Costs19; this.numericUpDownOrderECost19.Text = ECost19;
            this.numericUpDownOrderCount20.Text = Quant20; this.textBoxOrderDescr20.Text = Descr20; this.numericUpDownOrderCost20.Text = Costs20; this.numericUpDownOrderECost20.Text = ECost20;
            this.numericUpDownOrderCount21.Text = Quant21; this.textBoxOrderDescr21.Text = Descr21; this.numericUpDownOrderCost21.Text = Costs21; this.numericUpDownOrderECost21.Text = ECost21;
            this.numericUpDownOrderCount22.Text = Quant22; this.textBoxOrderDescr22.Text = Descr22; this.numericUpDownOrderCost22.Text = Costs22; this.numericUpDownOrderECost22.Text = ECost22;
            this.numericUpDownOrderCount23.Text = Quant23; this.textBoxOrderDescr23.Text = Descr23; this.numericUpDownOrderCost23.Text = Costs23; this.numericUpDownOrderECost23.Text = ECost23;

            this.richTextBoxInvoiceInstructions.Text = InvInstructions;
            this.richTextBoxInvoiceNotes.Text = InvNotes;
            this.richTextBoxVendorNotes.Text = VendorNotes;
            this.richTextBoxAccNotes.Text = AccNotes;
            this.textBoxCrMemo.Text = CrMemo;
            this.textBoxInvoiceNumber.Text = InvNumber;
            this.textBoxInvoiceDate.Text = InvDate;
            this.comboBoxStatus.Text = Status;
            this.textBoxCheckNumbers.Text = CheckNumbers;
            this.textBoxCheckDates.Text = CheckDates;

            this.textBoxComDate1.Text = ComDate1; this.textBoxCheckNumber1.Text = ComCheckNumber1; this.numericUpDownPaid1.Text = ComPaid1;
            this.textBoxComDate2.Text = ComDate2; this.textBoxCheckNumber2.Text = ComCheckNumber2; this.numericUpDownPaid2.Text = ComPaid2;
            this.textBoxComDate3.Text = ComDate3; this.textBoxCheckNumber3.Text = ComCheckNumber3; this.numericUpDownPaid3.Text = ComPaid3;
            this.textBoxComDate4.Text = ComDate4; this.textBoxCheckNumber4.Text = ComCheckNumber4; this.numericUpDownPaid4.Text = ComPaid4;
            this.textBoxComDate5.Text = ComDate5; this.textBoxCheckNumber5.Text = ComCheckNumber5; this.numericUpDownPaid5.Text = ComPaid5;

            this.numericUpDownComAmount.Text = ComAmount;
            this.numericUpDownComBalance.Text = ComBalance;
            this.richTextBoxDeliveryNotes.Text = DeliveryNotes;
            this.richTextBoxPONotes.Text = PONotes;

            isDataLoadingOrders = false;

            #endregion

            _log.append("readOrderAndUpdateGUI end~");

            List<string> theListOfLocks = getLockListOrders();
            if (theListOfLocks.Contains(this.textBoxPO.Text))
            {
                lockGUIOrders(true);
            }
            else
            {
                lockGUIOrders(false);
            }

            autoSelectSignature();

            _log.append("readOrderAndUpdateGUI end");
        }

        #endregion

        #region Orders update row

        private void updateRowOrders()
        {
            string Date; string EndUser; string Equipment; string VendorName; string JobName; string CustomerPO;
            string VendorNumber; string SalesAss; string SoldTo; string Street1; string Street2; string City; string State; string Zip;
            string ShipTo; string ShipStreet1; string ShipStreet2; string ShipCity; string ShipState; string ShipZip;
            string Carrier; string ShipDate; string IsComOrder; string IsComPaid; string Grinder; string SerialNo; string PumpStk;
            string ReqDate; string SchedShip; string PODate; string POShipVia;
            string TrackDate1; string TrackBy1; string TrackSource1; string TrackNote1;
            string TrackDate2; string TrackBy2; string TrackSource2; string TrackNote2;
            string TrackDate3; string TrackBy3; string TrackSource3; string TrackNote3;
            string TrackDate4; string TrackBy4; string TrackSource4; string TrackNote4;
            string TrackDate5; string TrackBy5; string TrackSource5; string TrackNote5;
            string TrackDate6; string TrackBy6; string TrackSource6; string TrackNote6;
            string TrackDate7; string TrackBy7; string TrackSource7; string TrackNote7;
            string TrackDate8; string TrackBy8; string TrackSource8; string TrackNote8;
            string TrackDate9; string TrackBy9; string TrackSource9; string TrackNote9;
            string TrackDate10; string TrackBy10; string TrackSource10; string TrackNote10;
            string TrackDate11; string TrackBy11; string TrackSource11; string TrackNote11;
            string TrackDate12; string TrackBy12; string TrackSource12; string TrackNote12;
            string TrackDate13; string TrackBy13; string TrackSource13; string TrackNote13;
            string TrackDate14; string TrackBy14; string TrackSource14; string TrackNote14;
            string TrackDate15; string TrackBy15; string TrackSource15; string TrackNote15;
            string TrackDate16; string TrackBy16; string TrackSource16; string TrackNote16;
            string TrackDate17; string TrackBy17; string TrackSource17; string TrackNote17;
            string TrackDate18; string TrackBy18; string TrackSource18; string TrackNote18;
            string QuotePrice; string Credit; string Freight; string ShopTime; string TotalCost; string GrossProfit; string Profit;
            string Description;
            string Quant1; string Descr1; string Costs1; string ECost1;
            string Quant2; string Descr2; string Costs2; string ECost2;
            string Quant3; string Descr3; string Costs3; string ECost3;
            string Quant4; string Descr4; string Costs4; string ECost4;
            string Quant5; string Descr5; string Costs5; string ECost5;
            string Quant6; string Descr6; string Costs6; string ECost6;
            string Quant7; string Descr7; string Costs7; string ECost7;
            string Quant8; string Descr8; string Costs8; string ECost8;
            string Quant9; string Descr9; string Costs9; string ECost9;
            string Quant10; string Descr10; string Costs10; string ECost10;
            string Quant11; string Descr11; string Costs11; string ECost11;
            string Quant12; string Descr12; string Costs12; string ECost12;
            string Quant13; string Descr13; string Costs13; string ECost13;
            string Quant14; string Descr14; string Costs14; string ECost14;
            string Quant15; string Descr15; string Costs15; string ECost15;
            string Quant16; string Descr16; string Costs16; string ECost16;
            string Quant17; string Descr17; string Costs17; string ECost17;
            string Quant18; string Descr18; string Costs18; string ECost18;
            string Quant19; string Descr19; string Costs19; string ECost19;
            string Quant20; string Descr20; string Costs20; string ECost20;
            string Quant21; string Descr21; string Costs21; string ECost21;
            string Quant22; string Descr22; string Costs22; string ECost22;
            string Quant23; string Descr23; string Costs23; string ECost23;
            string InvInstructions; string InvNotes; string VendorNotes; string AccNotes; string CrMemo; string InvNumber; string InvDate; string Status;
            string CheckNumbers; string CheckDates;
            string ComDate1; string ComCheckNumber1; string ComPaid1;
            string ComDate2; string ComCheckNumber2; string ComPaid2;
            string ComDate3; string ComCheckNumber3; string ComPaid3;
            string ComDate4; string ComCheckNumber4; string ComPaid4;
            string ComDate5; string ComCheckNumber5; string ComPaid5;
            string ComAmount; string ComBalance; string DeliveryNotes; string PONotes; string IsOk;

            Date = this.textBoxDate.Text;
            EndUser = this.textBoxEndUser.Text;
            Equipment = this.textBoxEquipment.Text;
            VendorName = this.textBoxVendorName.Text;
            JobName = this.textBoxJobName.Text;
            CustomerPO = this.textBoxCustomerPO.Text;
            VendorNumber = this.textBoxVendorNumber.Text;
            SalesAss = this.comboBoxSalesAss.Text;
            SoldTo = this.textBoxSoldTo.Text;

            Street1 = this.textBoxStreet1.Text;
            Street2 = this.textBoxStreet2.Text;
            City = this.textBoxCity.Text;
            State = this.comboBoxSoldToState.Text;
            Zip = this.textBoxZip.Text;

            ShipTo = this.textBoxShipTo.Text;
            ShipStreet1 = this.textBoxShipToStreet1.Text;
            ShipStreet2 = this.textBoxShipToStreet2.Text;
            ShipCity = this.textBoxShipToCity.Text;
            ShipState = this.comboBoxShipToState.Text;
            ShipZip = this.textBoxShipToZip.Text;

            Carrier = this.comboBoxCarrier.Text;
            ShipDate = this.textBoxShipDate.Text;

            IsOk = (this.checkBoxOK.Checked) ? "1" : "0";
            IsComOrder = (this.checkBoxComOrder.Checked) ? "1" : "0";
            IsComPaid = (this.checkBoxComPaid.Checked) ? "1" : "0";

            Grinder = this.textBoxGrinder.Text;
            SerialNo = this.textBoxSerialNumber.Text;
            PumpStk = this.textBoxPumpStk.Text;

            ReqDate = this.textBoxReqDate.Text;
            SchedShip = this.textBoxSchedShip.Text;
            PODate = this.textBoxPODate.Text;
            POShipVia = this.textBoxPOShipVia.Text;

            TrackDate1 = this.textBoxTrkDate1.Text; TrackBy1 = this.comboBoxTrkBy1.Text; TrackSource1 = this.comboBoxTrkSource1.Text; TrackNote1 = this.textBoxTrkNotes1.Text;
            TrackDate2 = this.textBoxTrkDate2.Text; TrackBy2 = this.comboBoxTrkBy2.Text; TrackSource2 = this.comboBoxTrkSource2.Text; TrackNote2 = this.textBoxTrkNotes2.Text;
            TrackDate3 = this.textBoxTrkDate3.Text; TrackBy3 = this.comboBoxTrkBy3.Text; TrackSource3 = this.comboBoxTrkSource3.Text; TrackNote3 = this.textBoxTrkNotes3.Text;
            TrackDate4 = this.textBoxTrkDate4.Text; TrackBy4 = this.comboBoxTrkBy4.Text; TrackSource4 = this.comboBoxTrkSource4.Text; TrackNote4 = this.textBoxTrkNotes4.Text;
            TrackDate5 = this.textBoxTrkDate5.Text; TrackBy5 = this.comboBoxTrkBy5.Text; TrackSource5 = this.comboBoxTrkSource5.Text; TrackNote5 = this.textBoxTrkNotes5.Text;
            TrackDate6 = this.textBoxTrkDate6.Text; TrackBy6 = this.comboBoxTrkBy6.Text; TrackSource6 = this.comboBoxTrkSource6.Text; TrackNote6 = this.textBoxTrkNotes6.Text;
            TrackDate7 = this.textBoxTrkDate7.Text; TrackBy7 = this.comboBoxTrkBy7.Text; TrackSource7 = this.comboBoxTrkSource7.Text; TrackNote7 = this.textBoxTrkNotes7.Text;
            TrackDate8 = this.textBoxTrkDate8.Text; TrackBy8 = this.comboBoxTrkBy8.Text; TrackSource8 = this.comboBoxTrkSource8.Text; TrackNote8 = this.textBoxTrkNotes8.Text;
            TrackDate9 = this.textBoxTrkDate9.Text; TrackBy9 = this.comboBoxTrkBy9.Text; TrackSource9 = this.comboBoxTrkSource9.Text; TrackNote9 = this.textBoxTrkNotes9.Text;
            TrackDate10 = this.textBoxTrkDate10.Text; TrackBy10 = this.comboBoxTrkBy10.Text; TrackSource10 = this.comboBoxTrkSource10.Text; TrackNote10 = this.textBoxTrkNotes10.Text;
            TrackDate11 = this.textBoxTrkDate11.Text; TrackBy11 = this.comboBoxTrkBy11.Text; TrackSource11 = this.comboBoxTrkSource11.Text; TrackNote11 = this.textBoxTrkNotes11.Text;
            TrackDate12 = this.textBoxTrkDate12.Text; TrackBy12 = this.comboBoxTrkBy12.Text; TrackSource12 = this.comboBoxTrkSource12.Text; TrackNote12 = this.textBoxTrkNotes12.Text;
            TrackDate13 = this.textBoxTrkDate13.Text; TrackBy13 = this.comboBoxTrkBy13.Text; TrackSource13 = this.comboBoxTrkSource13.Text; TrackNote13 = this.textBoxTrkNotes13.Text;
            TrackDate14 = this.textBoxTrkDate14.Text; TrackBy14 = this.comboBoxTrkBy14.Text; TrackSource14 = this.comboBoxTrkSource14.Text; TrackNote14 = this.textBoxTrkNotes14.Text;
            TrackDate15 = this.textBoxTrkDate15.Text; TrackBy15 = this.comboBoxTrkBy15.Text; TrackSource15 = this.comboBoxTrkSource15.Text; TrackNote15 = this.textBoxTrkNotes15.Text;
            TrackDate16 = this.textBoxTrkDate16.Text; TrackBy16 = this.comboBoxTrkBy16.Text; TrackSource16 = this.comboBoxTrkSource16.Text; TrackNote16 = this.textBoxTrkNotes16.Text;
            TrackDate17 = this.textBoxTrkDate17.Text; TrackBy17 = this.comboBoxTrkBy17.Text; TrackSource17 = this.comboBoxTrkSource17.Text; TrackNote17 = this.textBoxTrkNotes17.Text;
            TrackDate18 = this.textBoxTrkDate18.Text; TrackBy18 = this.comboBoxTrkBy18.Text; TrackSource18 = this.comboBoxTrkSource18.Text; TrackNote18 = this.textBoxTrkNotes18.Text;

            QuotePrice = this.numericUpDownQuotePrice.Text;
            Credit = this.numericUpDownCredit.Text;
            Freight = this.numericUpDownFreight.Text;
            ShopTime = this.numericUpDownShopTime.Text;
            TotalCost = this.numericUpDownTotalCost.Text;
            GrossProfit = this.numericUpDownGrossProfit.Text;
            Profit = this.numericUpDownProfit.Text;

            Description = this.textBoxDescription.Text;
            Quant1 = this.numericUpDownOrderCount1.Text; Descr1 = this.textBoxOrderDescr1.Text; Costs1 = this.numericUpDownOrderCost1.Text; ECost1 = this.numericUpDownOrderECost1.Text;
            Quant2 = this.numericUpDownOrderCount2.Text; Descr2 = this.textBoxOrderDescr2.Text; Costs2 = this.numericUpDownOrderCost2.Text; ECost2 = this.numericUpDownOrderECost2.Text;
            Quant3 = this.numericUpDownOrderCount3.Text; Descr3 = this.textBoxOrderDescr3.Text; Costs3 = this.numericUpDownOrderCost3.Text; ECost3 = this.numericUpDownOrderECost3.Text;
            Quant4 = this.numericUpDownOrderCount4.Text; Descr4 = this.textBoxOrderDescr4.Text; Costs4 = this.numericUpDownOrderCost4.Text; ECost4 = this.numericUpDownOrderECost4.Text;
            Quant5 = this.numericUpDownOrderCount5.Text; Descr5 = this.textBoxOrderDescr5.Text; Costs5 = this.numericUpDownOrderCost5.Text; ECost5 = this.numericUpDownOrderECost5.Text;
            Quant6 = this.numericUpDownOrderCount6.Text; Descr6 = this.textBoxOrderDescr6.Text; Costs6 = this.numericUpDownOrderCost6.Text; ECost6 = this.numericUpDownOrderECost6.Text;
            Quant7 = this.numericUpDownOrderCount7.Text; Descr7 = this.textBoxOrderDescr7.Text; Costs7 = this.numericUpDownOrderCost7.Text; ECost7 = this.numericUpDownOrderECost7.Text;
            Quant8 = this.numericUpDownOrderCount8.Text; Descr8 = this.textBoxOrderDescr8.Text; Costs8 = this.numericUpDownOrderCost8.Text; ECost8 = this.numericUpDownOrderECost8.Text;
            Quant9 = this.numericUpDownOrderCount9.Text; Descr9 = this.textBoxOrderDescr9.Text; Costs9 = this.numericUpDownOrderCost9.Text; ECost9 = this.numericUpDownOrderECost9.Text;
            Quant10 = this.numericUpDownOrderCount10.Text; Descr10 = this.textBoxOrderDescr10.Text; Costs10 = this.numericUpDownOrderCost10.Text; ECost10 = this.numericUpDownOrderECost10.Text;
            Quant11 = this.numericUpDownOrderCount11.Text; Descr11 = this.textBoxOrderDescr11.Text; Costs11 = this.numericUpDownOrderCost11.Text; ECost11 = this.numericUpDownOrderECost11.Text;
            Quant12 = this.numericUpDownOrderCount12.Text; Descr12 = this.textBoxOrderDescr12.Text; Costs12 = this.numericUpDownOrderCost12.Text; ECost12 = this.numericUpDownOrderECost12.Text;
            Quant13 = this.numericUpDownOrderCount13.Text; Descr13 = this.textBoxOrderDescr13.Text; Costs13 = this.numericUpDownOrderCost13.Text; ECost13 = this.numericUpDownOrderECost13.Text;
            Quant14 = this.numericUpDownOrderCount14.Text; Descr14 = this.textBoxOrderDescr14.Text; Costs14 = this.numericUpDownOrderCost14.Text; ECost14 = this.numericUpDownOrderECost14.Text;
            Quant15 = this.numericUpDownOrderCount15.Text; Descr15 = this.textBoxOrderDescr15.Text; Costs15 = this.numericUpDownOrderCost15.Text; ECost15 = this.numericUpDownOrderECost15.Text;
            Quant16 = this.numericUpDownOrderCount16.Text; Descr16 = this.textBoxOrderDescr16.Text; Costs16 = this.numericUpDownOrderCost16.Text; ECost16 = this.numericUpDownOrderECost16.Text;
            Quant17 = this.numericUpDownOrderCount17.Text; Descr17 = this.textBoxOrderDescr17.Text; Costs17 = this.numericUpDownOrderCost17.Text; ECost17 = this.numericUpDownOrderECost17.Text;
            Quant18 = this.numericUpDownOrderCount18.Text; Descr18 = this.textBoxOrderDescr18.Text; Costs18 = this.numericUpDownOrderCost18.Text; ECost18 = this.numericUpDownOrderECost18.Text;
            Quant19 = this.numericUpDownOrderCount19.Text; Descr19 = this.textBoxOrderDescr19.Text; Costs19 = this.numericUpDownOrderCost19.Text; ECost19 = this.numericUpDownOrderECost19.Text;
            Quant20 = this.numericUpDownOrderCount20.Text; Descr20 = this.textBoxOrderDescr20.Text; Costs20 = this.numericUpDownOrderCost20.Text; ECost20 = this.numericUpDownOrderECost20.Text;
            Quant21 = this.numericUpDownOrderCount21.Text; Descr21 = this.textBoxOrderDescr21.Text; Costs21 = this.numericUpDownOrderCost21.Text; ECost21 = this.numericUpDownOrderECost21.Text;
            Quant22 = this.numericUpDownOrderCount22.Text; Descr22 = this.textBoxOrderDescr22.Text; Costs22 = this.numericUpDownOrderCost22.Text; ECost22 = this.numericUpDownOrderECost22.Text;
            Quant23 = this.numericUpDownOrderCount23.Text; Descr23 = this.textBoxOrderDescr23.Text; Costs23 = this.numericUpDownOrderCost23.Text; ECost23 = this.numericUpDownOrderECost23.Text;

            InvInstructions = this.richTextBoxInvoiceInstructions.Text;
            InvNotes = this.richTextBoxInvoiceNotes.Text;
            VendorNotes = this.richTextBoxVendorNotes.Text;
            AccNotes = this.richTextBoxAccNotes.Text;
            CrMemo = this.textBoxCrMemo.Text;
            InvNumber = this.textBoxInvoiceNumber.Text;
            InvDate = this.textBoxInvoiceDate.Text;
            Status = this.comboBoxStatus.Text;
            CheckNumbers = this.textBoxCheckNumbers.Text;
            CheckDates = this.textBoxCheckDates.Text;

            ComDate1 = this.textBoxComDate1.Text; ComCheckNumber1 = this.textBoxCheckNumber1.Text; ComPaid1 = this.numericUpDownPaid1.Text;
            ComDate2 = this.textBoxComDate2.Text; ComCheckNumber2 = this.textBoxCheckNumber2.Text; ComPaid2 = this.numericUpDownPaid2.Text;
            ComDate3 = this.textBoxComDate3.Text; ComCheckNumber3 = this.textBoxCheckNumber3.Text; ComPaid3 = this.numericUpDownPaid3.Text;
            ComDate4 = this.textBoxComDate4.Text; ComCheckNumber4 = this.textBoxCheckNumber4.Text; ComPaid4 = this.numericUpDownPaid4.Text;
            ComDate5 = this.textBoxComDate5.Text; ComCheckNumber5 = this.textBoxCheckNumber5.Text; ComPaid5 = this.numericUpDownPaid5.Text;

            ComAmount = this.numericUpDownComAmount.Text;
            ComBalance = this.numericUpDownComBalance.Text;
            DeliveryNotes = this.richTextBoxDeliveryNotes.Text;
            PONotes = this.richTextBoxPONotes.Text;

            int rowsWritten = updateRowOrders(this.textBoxPO.Text, Date, EndUser, Equipment, VendorName, JobName, CustomerPO,
             VendorNumber, SalesAss, SoldTo, Street1, Street2, City, State, Zip,
             ShipTo, ShipStreet1, ShipStreet2, ShipCity, ShipState, ShipZip,
             Carrier, ShipDate, IsComOrder, IsComPaid, Grinder, SerialNo, PumpStk,
             ReqDate, SchedShip, PODate, POShipVia,
             TrackDate1, TrackBy1, TrackSource1, TrackNote1,
             TrackDate2, TrackBy2, TrackSource2, TrackNote2,
             TrackDate3, TrackBy3, TrackSource3, TrackNote3,
             TrackDate4, TrackBy4, TrackSource4, TrackNote4,
             TrackDate5, TrackBy5, TrackSource5, TrackNote5,
             TrackDate6, TrackBy6, TrackSource6, TrackNote6,
             TrackDate7, TrackBy7, TrackSource7, TrackNote7,
             TrackDate8, TrackBy8, TrackSource8, TrackNote8,
             TrackDate9, TrackBy9, TrackSource9, TrackNote9,
             TrackDate10, TrackBy10, TrackSource10, TrackNote10,
             TrackDate11, TrackBy11, TrackSource11, TrackNote11,
             TrackDate12, TrackBy12, TrackSource12, TrackNote12,
             TrackDate13, TrackBy13, TrackSource13, TrackNote13,
             TrackDate14, TrackBy14, TrackSource14, TrackNote14,
             TrackDate15, TrackBy15, TrackSource15, TrackNote15,
             TrackDate16, TrackBy16, TrackSource16, TrackNote16,
             TrackDate17, TrackBy17, TrackSource17, TrackNote17,
             TrackDate18, TrackBy18, TrackSource18, TrackNote18,
             QuotePrice, Credit, Freight, ShopTime, TotalCost, GrossProfit, Profit,
             Description,
             Quant1, Descr1, Costs1, ECost1,
             Quant2, Descr2, Costs2, ECost2,
             Quant3, Descr3, Costs3, ECost3,
             Quant4, Descr4, Costs4, ECost4,
             Quant5, Descr5, Costs5, ECost5,
             Quant6, Descr6, Costs6, ECost6,
             Quant7, Descr7, Costs7, ECost7,
             Quant8, Descr8, Costs8, ECost8,
             Quant9, Descr9, Costs9, ECost9,
             Quant10, Descr10, Costs10, ECost10,
             Quant11, Descr11, Costs11, ECost11,
             Quant12, Descr12, Costs12, ECost12,
             Quant13, Descr13, Costs13, ECost13,
             Quant14, Descr14, Costs14, ECost14,
             Quant15, Descr15, Costs15, ECost15,
             Quant16, Descr16, Costs16, ECost16,
             Quant17, Descr17, Costs17, ECost17,
             Quant18, Descr18, Costs18, ECost18,
             Quant19, Descr19, Costs19, ECost19,
             Quant20, Descr20, Costs20, ECost20,
             Quant21, Descr21, Costs21, ECost21,
             Quant22, Descr22, Costs22, ECost22,
             Quant23, Descr23, Costs23, ECost23,
             InvInstructions, InvNotes, VendorNotes, AccNotes, CrMemo, InvNumber, InvDate, Status,
             CheckNumbers, CheckDates,
             ComDate1, ComCheckNumber1, ComPaid1,
             ComDate2, ComCheckNumber2, ComPaid2,
             ComDate3, ComCheckNumber3, ComPaid3,
             ComDate4, ComCheckNumber4, ComPaid4,
             ComDate5, ComCheckNumber5, ComPaid5,
             ComAmount, ComBalance, DeliveryNotes, PONotes, IsOk);

            this.isDataDirtyOrders = false;
            this.toolStripStatusLabel.Text = "Status Orders: Clean";

            // remove the lock
            removeLockOrder();

            //MessageBox.Show(rowsWritten.ToString());
        }

        #endregion

        #region Orders insert row

        private void insertNewDataRowOrders(string PO)
        {
            string Date; string EndUser; string Equipment; string VendorName; string JobName; string CustomerPO;
            string VendorNumber; string SalesAss; string SoldTo; string Street1; string Street2; string City; string State; string Zip;
            string ShipTo; string ShipStreet1; string ShipStreet2; string ShipCity; string ShipState; string ShipZip;
            string Carrier; string ShipDate; string IsComOrder; string IsComPaid; string Grinder; string SerialNo; string PumpStk;
            string ReqDate; string SchedShip; string PODate; string POShipVia;
            string TrackDate1; string TrackBy1; string TrackSource1; string TrackNote1;
            string TrackDate2; string TrackBy2; string TrackSource2; string TrackNote2;
            string TrackDate3; string TrackBy3; string TrackSource3; string TrackNote3;
            string TrackDate4; string TrackBy4; string TrackSource4; string TrackNote4;
            string TrackDate5; string TrackBy5; string TrackSource5; string TrackNote5;
            string TrackDate6; string TrackBy6; string TrackSource6; string TrackNote6;
            string TrackDate7; string TrackBy7; string TrackSource7; string TrackNote7;
            string TrackDate8; string TrackBy8; string TrackSource8; string TrackNote8;
            string TrackDate9; string TrackBy9; string TrackSource9; string TrackNote9;
            string TrackDate10; string TrackBy10; string TrackSource10; string TrackNote10;
            string TrackDate11; string TrackBy11; string TrackSource11; string TrackNote11;
            string TrackDate12; string TrackBy12; string TrackSource12; string TrackNote12;
            string TrackDate13; string TrackBy13; string TrackSource13; string TrackNote13;
            string TrackDate14; string TrackBy14; string TrackSource14; string TrackNote14;
            string TrackDate15; string TrackBy15; string TrackSource15; string TrackNote15;
            string TrackDate16; string TrackBy16; string TrackSource16; string TrackNote16;
            string TrackDate17; string TrackBy17; string TrackSource17; string TrackNote17;
            string TrackDate18; string TrackBy18; string TrackSource18; string TrackNote18;
            string QuotePrice; string Credit; string Freight; string ShopTime; string TotalCost; string GrossProfit; string Profit;
            string Description;
            string Quant1; string Descr1; string Costs1; string ECost1;
            string Quant2; string Descr2; string Costs2; string ECost2;
            string Quant3; string Descr3; string Costs3; string ECost3;
            string Quant4; string Descr4; string Costs4; string ECost4;
            string Quant5; string Descr5; string Costs5; string ECost5;
            string Quant6; string Descr6; string Costs6; string ECost6;
            string Quant7; string Descr7; string Costs7; string ECost7;
            string Quant8; string Descr8; string Costs8; string ECost8;
            string Quant9; string Descr9; string Costs9; string ECost9;
            string Quant10; string Descr10; string Costs10; string ECost10;
            string Quant11; string Descr11; string Costs11; string ECost11;
            string Quant12; string Descr12; string Costs12; string ECost12;
            string Quant13; string Descr13; string Costs13; string ECost13;
            string Quant14; string Descr14; string Costs14; string ECost14;
            string Quant15; string Descr15; string Costs15; string ECost15;
            string Quant16; string Descr16; string Costs16; string ECost16;
            string Quant17; string Descr17; string Costs17; string ECost17;
            string Quant18; string Descr18; string Costs18; string ECost18;
            string Quant19; string Descr19; string Costs19; string ECost19;
            string Quant20; string Descr20; string Costs20; string ECost20;
            string Quant21; string Descr21; string Costs21; string ECost21;
            string Quant22; string Descr22; string Costs22; string ECost22;
            string Quant23; string Descr23; string Costs23; string ECost23;
            string InvInstructions; string InvNotes; string VendorNotes; string AccNotes; string CrMemo; string InvNumber; string InvDate; string Status;
            string CheckNumbers; string CheckDates;
            string ComDate1; string ComCheckNumber1; string ComPaid1;
            string ComDate2; string ComCheckNumber2; string ComPaid2;
            string ComDate3; string ComCheckNumber3; string ComPaid3;
            string ComDate4; string ComCheckNumber4; string ComPaid4;
            string ComDate5; string ComCheckNumber5; string ComPaid5;
            string ComAmount; string ComBalance; string DeliveryNotes; string PONotes; string IsOk;

            Date = this.textBoxDate.Text;
            EndUser = this.textBoxEndUser.Text;
            Equipment = this.textBoxEquipment.Text;
            VendorName = this.textBoxVendorName.Text;
            JobName = this.textBoxJobName.Text;
            CustomerPO = this.textBoxCustomerPO.Text;
            VendorNumber = this.textBoxVendorNumber.Text;
            SalesAss = this.comboBoxSalesAss.Text;
            SoldTo = this.textBoxSoldTo.Text;

            Street1 = this.textBoxStreet1.Text;
            Street2 = this.textBoxStreet2.Text;
            City = this.textBoxCity.Text;
            State = this.comboBoxSoldToState.Text;
            Zip = this.textBoxZip.Text;

            ShipTo = this.textBoxShipTo.Text;
            ShipStreet1 = this.textBoxShipToStreet1.Text;
            ShipStreet2 = this.textBoxShipToStreet2.Text;
            ShipCity = this.textBoxShipToCity.Text;
            ShipState = this.comboBoxShipToState.Text;
            ShipZip = this.textBoxShipToZip.Text;

            Carrier = this.comboBoxCarrier.Text;
            ShipDate = this.textBoxShipDate.Text;

            IsOk = (this.checkBoxOK.Checked) ? "1" : "0";
            IsComOrder = (this.checkBoxComOrder.Checked) ? "1" : "0";
            IsComPaid = (this.checkBoxComPaid.Checked) ? "1" : "0";

            Grinder = this.textBoxGrinder.Text;
            SerialNo = this.textBoxSerialNumber.Text;
            PumpStk = this.textBoxPumpStk.Text;

            ReqDate = this.textBoxReqDate.Text;
            SchedShip = this.textBoxSchedShip.Text;
            PODate = this.textBoxPODate.Text;
            POShipVia = this.textBoxPOShipVia.Text;

            TrackDate1 = this.textBoxTrkDate1.Text; TrackBy1 = this.comboBoxTrkBy1.Text; TrackSource1 = this.comboBoxTrkSource1.Text; TrackNote1 = this.textBoxTrkNotes1.Text;
            TrackDate2 = this.textBoxTrkDate2.Text; TrackBy2 = this.comboBoxTrkBy2.Text; TrackSource2 = this.comboBoxTrkSource2.Text; TrackNote2 = this.textBoxTrkNotes2.Text;
            TrackDate3 = this.textBoxTrkDate3.Text; TrackBy3 = this.comboBoxTrkBy3.Text; TrackSource3 = this.comboBoxTrkSource3.Text; TrackNote3 = this.textBoxTrkNotes3.Text;
            TrackDate4 = this.textBoxTrkDate4.Text; TrackBy4 = this.comboBoxTrkBy4.Text; TrackSource4 = this.comboBoxTrkSource4.Text; TrackNote4 = this.textBoxTrkNotes4.Text;
            TrackDate5 = this.textBoxTrkDate5.Text; TrackBy5 = this.comboBoxTrkBy5.Text; TrackSource5 = this.comboBoxTrkSource5.Text; TrackNote5 = this.textBoxTrkNotes5.Text;
            TrackDate6 = this.textBoxTrkDate6.Text; TrackBy6 = this.comboBoxTrkBy6.Text; TrackSource6 = this.comboBoxTrkSource6.Text; TrackNote6 = this.textBoxTrkNotes6.Text;
            TrackDate7 = this.textBoxTrkDate7.Text; TrackBy7 = this.comboBoxTrkBy7.Text; TrackSource7 = this.comboBoxTrkSource7.Text; TrackNote7 = this.textBoxTrkNotes7.Text;
            TrackDate8 = this.textBoxTrkDate8.Text; TrackBy8 = this.comboBoxTrkBy8.Text; TrackSource8 = this.comboBoxTrkSource8.Text; TrackNote8 = this.textBoxTrkNotes8.Text;
            TrackDate9 = this.textBoxTrkDate9.Text; TrackBy9 = this.comboBoxTrkBy9.Text; TrackSource9 = this.comboBoxTrkSource9.Text; TrackNote9 = this.textBoxTrkNotes9.Text;
            TrackDate10 = this.textBoxTrkDate10.Text; TrackBy10 = this.comboBoxTrkBy10.Text; TrackSource10 = this.comboBoxTrkSource10.Text; TrackNote10 = this.textBoxTrkNotes10.Text;
            TrackDate11 = this.textBoxTrkDate11.Text; TrackBy11 = this.comboBoxTrkBy11.Text; TrackSource11 = this.comboBoxTrkSource11.Text; TrackNote11 = this.textBoxTrkNotes11.Text;
            TrackDate12 = this.textBoxTrkDate12.Text; TrackBy12 = this.comboBoxTrkBy12.Text; TrackSource12 = this.comboBoxTrkSource12.Text; TrackNote12 = this.textBoxTrkNotes12.Text;
            TrackDate13 = this.textBoxTrkDate13.Text; TrackBy13 = this.comboBoxTrkBy13.Text; TrackSource13 = this.comboBoxTrkSource13.Text; TrackNote13 = this.textBoxTrkNotes13.Text;
            TrackDate14 = this.textBoxTrkDate14.Text; TrackBy14 = this.comboBoxTrkBy14.Text; TrackSource14 = this.comboBoxTrkSource14.Text; TrackNote14 = this.textBoxTrkNotes14.Text;
            TrackDate15 = this.textBoxTrkDate15.Text; TrackBy15 = this.comboBoxTrkBy15.Text; TrackSource15 = this.comboBoxTrkSource15.Text; TrackNote15 = this.textBoxTrkNotes15.Text;
            TrackDate16 = this.textBoxTrkDate16.Text; TrackBy16 = this.comboBoxTrkBy16.Text; TrackSource16 = this.comboBoxTrkSource16.Text; TrackNote16 = this.textBoxTrkNotes16.Text;
            TrackDate17 = this.textBoxTrkDate17.Text; TrackBy17 = this.comboBoxTrkBy17.Text; TrackSource17 = this.comboBoxTrkSource17.Text; TrackNote17 = this.textBoxTrkNotes17.Text;
            TrackDate18 = this.textBoxTrkDate18.Text; TrackBy18 = this.comboBoxTrkBy18.Text; TrackSource18 = this.comboBoxTrkSource18.Text; TrackNote18 = this.textBoxTrkNotes18.Text;

            QuotePrice = this.numericUpDownQuotePrice.Text;
            Credit = this.numericUpDownCredit.Text;
            Freight = this.numericUpDownFreight.Text;
            ShopTime = this.numericUpDownShopTime.Text;
            TotalCost = this.numericUpDownTotalCost.Text;
            GrossProfit = this.numericUpDownGrossProfit.Text;
            Profit = this.numericUpDownProfit.Text;

            Description = this.textBoxDescription.Text;
            Quant1 = this.numericUpDownOrderCount1.Text; Descr1 = this.textBoxOrderDescr1.Text; Costs1 = this.numericUpDownOrderCost1.Text; ECost1 = this.numericUpDownOrderECost1.Text;
            Quant2 = this.numericUpDownOrderCount2.Text; Descr2 = this.textBoxOrderDescr2.Text; Costs2 = this.numericUpDownOrderCost2.Text; ECost2 = this.numericUpDownOrderECost2.Text;
            Quant3 = this.numericUpDownOrderCount3.Text; Descr3 = this.textBoxOrderDescr3.Text; Costs3 = this.numericUpDownOrderCost3.Text; ECost3 = this.numericUpDownOrderECost3.Text;
            Quant4 = this.numericUpDownOrderCount4.Text; Descr4 = this.textBoxOrderDescr4.Text; Costs4 = this.numericUpDownOrderCost4.Text; ECost4 = this.numericUpDownOrderECost4.Text;
            Quant5 = this.numericUpDownOrderCount5.Text; Descr5 = this.textBoxOrderDescr5.Text; Costs5 = this.numericUpDownOrderCost5.Text; ECost5 = this.numericUpDownOrderECost5.Text;
            Quant6 = this.numericUpDownOrderCount6.Text; Descr6 = this.textBoxOrderDescr6.Text; Costs6 = this.numericUpDownOrderCost6.Text; ECost6 = this.numericUpDownOrderECost6.Text;
            Quant7 = this.numericUpDownOrderCount7.Text; Descr7 = this.textBoxOrderDescr7.Text; Costs7 = this.numericUpDownOrderCost7.Text; ECost7 = this.numericUpDownOrderECost7.Text;
            Quant8 = this.numericUpDownOrderCount8.Text; Descr8 = this.textBoxOrderDescr8.Text; Costs8 = this.numericUpDownOrderCost8.Text; ECost8 = this.numericUpDownOrderECost8.Text;
            Quant9 = this.numericUpDownOrderCount9.Text; Descr9 = this.textBoxOrderDescr9.Text; Costs9 = this.numericUpDownOrderCost9.Text; ECost9 = this.numericUpDownOrderECost9.Text;
            Quant10 = this.numericUpDownOrderCount10.Text; Descr10 = this.textBoxOrderDescr10.Text; Costs10 = this.numericUpDownOrderCost10.Text; ECost10 = this.numericUpDownOrderECost10.Text;
            Quant11 = this.numericUpDownOrderCount11.Text; Descr11 = this.textBoxOrderDescr11.Text; Costs11 = this.numericUpDownOrderCost11.Text; ECost11 = this.numericUpDownOrderECost11.Text;
            Quant12 = this.numericUpDownOrderCount12.Text; Descr12 = this.textBoxOrderDescr12.Text; Costs12 = this.numericUpDownOrderCost12.Text; ECost12 = this.numericUpDownOrderECost12.Text;
            Quant13 = this.numericUpDownOrderCount13.Text; Descr13 = this.textBoxOrderDescr13.Text; Costs13 = this.numericUpDownOrderCost13.Text; ECost13 = this.numericUpDownOrderECost13.Text;
            Quant14 = this.numericUpDownOrderCount14.Text; Descr14 = this.textBoxOrderDescr14.Text; Costs14 = this.numericUpDownOrderCost14.Text; ECost14 = this.numericUpDownOrderECost14.Text;
            Quant15 = this.numericUpDownOrderCount15.Text; Descr15 = this.textBoxOrderDescr15.Text; Costs15 = this.numericUpDownOrderCost15.Text; ECost15 = this.numericUpDownOrderECost15.Text;
            Quant16 = this.numericUpDownOrderCount16.Text; Descr16 = this.textBoxOrderDescr16.Text; Costs16 = this.numericUpDownOrderCost16.Text; ECost16 = this.numericUpDownOrderECost16.Text;
            Quant17 = this.numericUpDownOrderCount17.Text; Descr17 = this.textBoxOrderDescr17.Text; Costs17 = this.numericUpDownOrderCost17.Text; ECost17 = this.numericUpDownOrderECost17.Text;
            Quant18 = this.numericUpDownOrderCount18.Text; Descr18 = this.textBoxOrderDescr18.Text; Costs18 = this.numericUpDownOrderCost18.Text; ECost18 = this.numericUpDownOrderECost18.Text;
            Quant19 = this.numericUpDownOrderCount19.Text; Descr19 = this.textBoxOrderDescr19.Text; Costs19 = this.numericUpDownOrderCost19.Text; ECost19 = this.numericUpDownOrderECost19.Text;
            Quant20 = this.numericUpDownOrderCount20.Text; Descr20 = this.textBoxOrderDescr20.Text; Costs20 = this.numericUpDownOrderCost20.Text; ECost20 = this.numericUpDownOrderECost20.Text;
            Quant21 = this.numericUpDownOrderCount21.Text; Descr21 = this.textBoxOrderDescr21.Text; Costs21 = this.numericUpDownOrderCost21.Text; ECost21 = this.numericUpDownOrderECost21.Text;
            Quant22 = this.numericUpDownOrderCount22.Text; Descr22 = this.textBoxOrderDescr22.Text; Costs22 = this.numericUpDownOrderCost22.Text; ECost22 = this.numericUpDownOrderECost22.Text;
            Quant23 = this.numericUpDownOrderCount23.Text; Descr23 = this.textBoxOrderDescr23.Text; Costs23 = this.numericUpDownOrderCost23.Text; ECost23 = this.numericUpDownOrderECost23.Text;

            InvInstructions = this.richTextBoxInvoiceInstructions.Text;
            InvNotes = this.richTextBoxInvoiceNotes.Text;
            VendorNotes = this.richTextBoxVendorNotes.Text;
            AccNotes = this.richTextBoxAccNotes.Text;
            CrMemo = this.textBoxCrMemo.Text;
            InvNumber = this.textBoxInvoiceNumber.Text;
            InvDate = this.textBoxInvoiceDate.Text;
            Status = this.comboBoxStatus.Text;
            CheckNumbers = this.textBoxCheckNumbers.Text;
            CheckDates = this.textBoxCheckDates.Text;

            ComDate1 = this.textBoxComDate1.Text; ComCheckNumber1 = this.textBoxCheckNumber1.Text; ComPaid1 = this.numericUpDownPaid1.Text;
            ComDate2 = this.textBoxComDate2.Text; ComCheckNumber2 = this.textBoxCheckNumber2.Text; ComPaid2 = this.numericUpDownPaid2.Text;
            ComDate3 = this.textBoxComDate3.Text; ComCheckNumber3 = this.textBoxCheckNumber3.Text; ComPaid3 = this.numericUpDownPaid3.Text;
            ComDate4 = this.textBoxComDate4.Text; ComCheckNumber4 = this.textBoxCheckNumber4.Text; ComPaid4 = this.numericUpDownPaid4.Text;
            ComDate5 = this.textBoxComDate5.Text; ComCheckNumber5 = this.textBoxCheckNumber5.Text; ComPaid5 = this.numericUpDownPaid5.Text;

            ComAmount = this.numericUpDownComAmount.Text;
            ComBalance = this.numericUpDownComBalance.Text;
            DeliveryNotes = this.richTextBoxDeliveryNotes.Text;
            PONotes = this.richTextBoxPONotes.Text;

            int rowsWritten = insertRowOrders(PO, Date, EndUser, Equipment, VendorName, JobName, CustomerPO,
             VendorNumber, SalesAss, SoldTo, Street1, Street2, City, State, Zip,
             ShipTo, ShipStreet1, ShipStreet2, ShipCity, ShipState, ShipZip,
             Carrier, ShipDate, IsComOrder, IsComPaid, Grinder, SerialNo, PumpStk,
             ReqDate, SchedShip, PODate, POShipVia,
             TrackDate1, TrackBy1, TrackSource1, TrackNote1,
             TrackDate2, TrackBy2, TrackSource2, TrackNote2,
             TrackDate3, TrackBy3, TrackSource3, TrackNote3,
             TrackDate4, TrackBy4, TrackSource4, TrackNote4,
             TrackDate5, TrackBy5, TrackSource5, TrackNote5,
             TrackDate6, TrackBy6, TrackSource6, TrackNote6,
             TrackDate7, TrackBy7, TrackSource7, TrackNote7,
             TrackDate8, TrackBy8, TrackSource8, TrackNote8,
             TrackDate9, TrackBy9, TrackSource9, TrackNote9,
             TrackDate10, TrackBy10, TrackSource10, TrackNote10,
             TrackDate11, TrackBy11, TrackSource11, TrackNote11,
             TrackDate12, TrackBy12, TrackSource12, TrackNote12,
             TrackDate13, TrackBy13, TrackSource13, TrackNote13,
             TrackDate14, TrackBy14, TrackSource14, TrackNote14,
             TrackDate15, TrackBy15, TrackSource15, TrackNote15,
             TrackDate16, TrackBy16, TrackSource16, TrackNote16,
             TrackDate17, TrackBy17, TrackSource17, TrackNote17,
             TrackDate18, TrackBy18, TrackSource18, TrackNote18,
             QuotePrice, Credit, Freight, ShopTime, TotalCost, GrossProfit, Profit,
             Description,
             Quant1, Descr1, Costs1, ECost1,
             Quant2, Descr2, Costs2, ECost2,
             Quant3, Descr3, Costs3, ECost3,
             Quant4, Descr4, Costs4, ECost4,
             Quant5, Descr5, Costs5, ECost5,
             Quant6, Descr6, Costs6, ECost6,
             Quant7, Descr7, Costs7, ECost7,
             Quant8, Descr8, Costs8, ECost8,
             Quant9, Descr9, Costs9, ECost9,
             Quant10, Descr10, Costs10, ECost10,
             Quant11, Descr11, Costs11, ECost11,
             Quant12, Descr12, Costs12, ECost12,
             Quant13, Descr13, Costs13, ECost13,
             Quant14, Descr14, Costs14, ECost14,
             Quant15, Descr15, Costs15, ECost15,
             Quant16, Descr16, Costs16, ECost16,
             Quant17, Descr17, Costs17, ECost17,
             Quant18, Descr18, Costs18, ECost18,
             Quant19, Descr19, Costs19, ECost19,
             Quant20, Descr20, Costs20, ECost20,
             Quant21, Descr21, Costs21, ECost21,
             Quant22, Descr22, Costs22, ECost22,
             Quant23, Descr23, Costs23, ECost23,
             InvInstructions, InvNotes, VendorNotes, AccNotes, CrMemo, InvNumber, InvDate, Status,
             CheckNumbers, CheckDates,
             ComDate1, ComCheckNumber1, ComPaid1,
             ComDate2, ComCheckNumber2, ComPaid2,
             ComDate3, ComCheckNumber3, ComPaid3,
             ComDate4, ComCheckNumber4, ComPaid4,
             ComDate5, ComCheckNumber5, ComPaid5,
             ComAmount, ComBalance, DeliveryNotes, PONotes, IsOk);

            isDataDirtyOrders = false;
            this.toolStripStatusLabel.Text = "Status Orders: Clean";

            //MessageBox.Show(rowsWritten.ToString());
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

        private bool readRowQuotes(string QPO, int rowCount, out string theQPO,
            out string Date, out string Status, out string Company, out string VendorName, out string JobName, out string CustomerPO,
            out string VendorNumber, out string SalesAss, out string SoldTo, out string Street1, out string Street2, out string City, out string State, out string Zip,
            out string Delivery, out string Terms, out string FreightSelect, out string IsQManual, out string IsPManual, out string Location, out string Equipment, out string EquipCategory,
            out string QuotePrice, out string Credit, out string Freight, out string ShopTime, out string TotalCost, out string GrossProfit, out string Profit, out string MarkUp,
            out string Description,
            out string Quant1, out string Descr1, out string Costs1, out string ECost1, out string Price1,
            out string Quant2, out string Descr2, out string Costs2, out string ECost2, out string Price2,
            out string Quant3, out string Descr3, out string Costs3, out string ECost3, out string Price3,
            out string Quant4, out string Descr4, out string Costs4, out string ECost4, out string Price4,
            out string Quant5, out string Descr5, out string Costs5, out string ECost5, out string Price5,
            out string Quant6, out string Descr6, out string Costs6, out string ECost6, out string Price6,
            out string Quant7, out string Descr7, out string Costs7, out string ECost7, out string Price7,
            out string Quant8, out string Descr8, out string Costs8, out string ECost8, out string Price8,
            out string Quant9, out string Descr9, out string Costs9, out string ECost9, out string Price9,
            out string Quant10, out string Descr10, out string Costs10, out string ECost10, out string Price10,
            out string Quant11, out string Descr11, out string Costs11, out string ECost11, out string Price11,
            out string Quant12, out string Descr12, out string Costs12, out string ECost12, out string Price12,
            out string Quant13, out string Descr13, out string Costs13, out string ECost13, out string Price13,
            out string Quant14, out string Descr14, out string Costs14, out string ECost14, out string Price14,
            out string Quant15, out string Descr15, out string Costs15, out string ECost15, out string Price15,
            out string Quant16, out string Descr16, out string Costs16, out string ECost16, out string Price16,
            out string Quant17, out string Descr17, out string Costs17, out string ECost17, out string Price17,
            out string Quant18, out string Descr18, out string Costs18, out string ECost18, out string Price18,
            out string Quant19, out string Descr19, out string Costs19, out string ECost19, out string Price19,
            out string Quant20, out string Descr20, out string Costs20, out string ECost20, out string Price20,
            out string Quant21, out string Descr21, out string Costs21, out string ECost21, out string Price21,
            out string Quant22, out string Descr22, out string Costs22, out string ECost22, out string Price22,
            out string Quant23, out string Descr23, out string Costs23, out string ECost23, out string Price23,
            out string InvNotes, out string DeliveryNotes, out string QuoteNotes)
        {
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


                                #region reads as strings

                                theQPO = rrr["PO"] as string;

                                Date = rrr["Date"] as string;
                                Status = rrr["Status"] as string;

                                Company = rrr["Company"] as string;
                                VendorName = rrr["VendorName"] as string;
                                JobName = rrr["JobName"] as string;
                                CustomerPO = rrr["CustomerPO"] as string;
                                VendorNumber = rrr["VendorNumber"] as string;
                                SalesAss = rrr["SalesAss"] as string;
                                SoldTo = rrr["SoldTo"] as string;
                                Street1 = rrr["Street1"] as string;
                                Street2 = rrr["Street2"] as string;
                                City = rrr["City"] as string;
                                State = rrr["State"] as string;
                                Zip = rrr["Zip"] as string;
                                Delivery = rrr["Delivery"] as string;
                                Terms = rrr["Terms"] as string;
                                FreightSelect = rrr["FreightSelect"] as string;
                                IsQManual = rrr["IsQManual"] as string;
                                IsPManual = rrr["IsPManual"] as string;
                                Location = rrr["Location"] as string;
                                Equipment = rrr["Equipment"] as string;
                                EquipCategory = rrr["EquipCategory"] as string;

                                QuotePrice = rrr["QuotePrice"] as string;
                                Credit = rrr["Credit"] as string;
                                Freight = rrr["Freight"] as string;
                                ShopTime = rrr["ShopTime"] as string;
                                TotalCost = rrr["TotalCost"] as string;
                                GrossProfit = rrr["GrossProfit"] as string;
                                Profit = rrr["Profit"] as string;
                                MarkUp = rrr["MarkUp"] as string;
                                Description = rrr["Description"] as string;

                                Quant1 = rrr["Quant1"] as string;
                                Descr1 = rrr["Descr1"] as string;
                                Costs1 = rrr["Costs1"] as string;
                                ECost1 = rrr["ECost1"] as string;
                                Price1 = rrr["Price1"] as string;
                                Quant2 = rrr["Quant2"] as string;
                                Descr2 = rrr["Descr2"] as string;
                                Costs2 = rrr["Costs2"] as string;
                                Price2 = rrr["Price2"] as string;
                                ECost2 = rrr["ECost2"] as string;
                                Quant3 = rrr["Quant3"] as string;
                                Descr3 = rrr["Descr3"] as string;
                                Costs3 = rrr["Costs3"] as string;
                                ECost3 = rrr["ECost3"] as string;
                                Price3 = rrr["Price3"] as string;
                                Quant4 = rrr["Quant4"] as string;
                                Descr4 = rrr["Descr4"] as string;
                                Costs4 = rrr["Costs4"] as string;
                                ECost4 = rrr["ECost4"] as string;
                                Price4 = rrr["Price4"] as string;
                                Quant5 = rrr["Quant5"] as string;
                                Descr5 = rrr["Descr5"] as string;
                                Costs5 = rrr["Costs5"] as string;
                                ECost5 = rrr["ECost5"] as string;
                                Price5 = rrr["Price5"] as string;
                                Quant6 = rrr["Quant6"] as string;
                                Descr6 = rrr["Descr6"] as string;
                                Costs6 = rrr["Costs6"] as string;
                                ECost6 = rrr["ECost6"] as string;
                                Price6 = rrr["Price6"] as string;
                                Quant7 = rrr["Quant7"] as string;
                                Descr7 = rrr["Descr7"] as string;
                                Costs7 = rrr["Costs7"] as string;
                                ECost7 = rrr["ECost7"] as string;
                                Price7 = rrr["Price7"] as string;
                                Quant8 = rrr["Quant8"] as string;
                                Descr8 = rrr["Descr8"] as string;
                                Costs8 = rrr["Costs8"] as string;
                                ECost8 = rrr["ECost8"] as string;
                                Price8 = rrr["Price8"] as string;
                                Quant9 = rrr["Quant9"] as string;
                                Descr9 = rrr["Descr9"] as string;
                                Costs9 = rrr["Costs9"] as string;
                                ECost9 = rrr["ECost9"] as string;
                                Price9 = rrr["Price9"] as string;
                                Quant10 = rrr["Quant10"] as string;
                                Descr10 = rrr["Descr10"] as string;
                                Costs10 = rrr["Costs10"] as string;
                                ECost10 = rrr["ECost10"] as string;
                                Price10 = rrr["Price10"] as string;
                                Quant11 = rrr["Quant11"] as string;
                                Descr11 = rrr["Descr11"] as string;
                                Costs11 = rrr["Costs11"] as string;
                                ECost11 = rrr["ECost11"] as string;
                                Price11 = rrr["Price11"] as string;
                                Quant12 = rrr["Quant12"] as string;
                                Descr12 = rrr["Descr12"] as string;
                                Costs12 = rrr["Costs12"] as string;
                                ECost12 = rrr["ECost12"] as string;
                                Price12 = rrr["Price12"] as string;
                                Quant13 = rrr["Quant13"] as string;
                                Descr13 = rrr["Descr13"] as string;
                                Costs13 = rrr["Costs13"] as string;
                                ECost13 = rrr["ECost13"] as string;
                                Price13 = rrr["Price13"] as string;
                                Quant14 = rrr["Quant14"] as string;
                                Descr14 = rrr["Descr14"] as string;
                                Costs14 = rrr["Costs14"] as string;
                                ECost14 = rrr["ECost14"] as string;
                                Price14 = rrr["Price14"] as string;
                                Quant15 = rrr["Quant15"] as string;
                                Descr15 = rrr["Descr15"] as string;
                                Costs15 = rrr["Costs15"] as string;
                                ECost15 = rrr["ECost15"] as string;
                                Price15 = rrr["Price15"] as string;
                                Quant16 = rrr["Quant16"] as string;
                                Descr16 = rrr["Descr16"] as string;
                                Costs16 = rrr["Costs16"] as string;
                                ECost16 = rrr["ECost16"] as string;
                                Price16 = rrr["Price16"] as string;
                                Quant17 = rrr["Quant17"] as string;
                                Descr17 = rrr["Descr17"] as string;
                                Costs17 = rrr["Costs17"] as string;
                                ECost17 = rrr["ECost17"] as string;
                                Price17 = rrr["Price17"] as string;
                                Quant18 = rrr["Quant18"] as string;
                                Descr18 = rrr["Descr18"] as string;
                                Costs18 = rrr["Costs18"] as string;
                                ECost18 = rrr["ECost18"] as string;
                                Price18 = rrr["Price18"] as string;
                                Quant19 = rrr["Quant19"] as string;
                                Descr19 = rrr["Descr19"] as string;
                                Costs19 = rrr["Costs19"] as string;
                                ECost19 = rrr["ECost19"] as string;
                                Price19 = rrr["Price19"] as string;
                                Quant20 = rrr["Quant20"] as string;
                                Descr20 = rrr["Descr20"] as string;
                                Costs20 = rrr["Costs20"] as string;
                                ECost20 = rrr["ECost20"] as string;
                                Price20 = rrr["Price20"] as string;
                                Quant21 = rrr["Quant21"] as string;
                                Descr21 = rrr["Descr21"] as string;
                                Costs21 = rrr["Costs21"] as string;
                                ECost21 = rrr["ECost21"] as string;
                                Price21 = rrr["Price21"] as string;
                                Quant22 = rrr["Quant22"] as string;
                                Descr22 = rrr["Descr22"] as string;
                                Costs22 = rrr["Costs22"] as string;
                                ECost22 = rrr["ECost22"] as string;
                                Price22 = rrr["Price22"] as string;
                                Quant23 = rrr["Quant23"] as string;
                                Descr23 = rrr["Descr23"] as string;
                                Costs23 = rrr["Costs23"] as string;
                                ECost23 = rrr["ECost23"] as string;
                                Price23 = rrr["Price23"] as string;

                                InvNotes = rrr["InvNotes"] as string;
                                DeliveryNotes = rrr["DeliveryNotes"] as string;
                                QuoteNotes = rrr["QuoteNotes"] as string;

                                #endregion


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

            #region default sets if no record -- goes into GUI

            theQPO = generateNextQuotePO();

            Date = todaysDate();
            Status = "";
            Company = "";
            
            VendorName = "";
            JobName = "";
            CustomerPO = "";
            VendorNumber = "";
            SalesAss = "";
            SoldTo = "";
            Street1 = "";
            Street2 = "";
            City = "";
            State = "";
            Zip = "";
            Delivery = "";
            Terms = "";
            FreightSelect = "Net price, freight included";
            IsQManual = "";
            IsPManual = "1";  // default on for prices
            Location = "";
            Equipment = "";
            EquipCategory = "";

            QuotePrice = "0.00";
            Credit = "0.00";
            Freight = "0.00";
            ShopTime = "0.00";
            TotalCost = "0.00";
            GrossProfit = "0.00";
            Profit = "0.00";
            MarkUp = "40.00";
            Description = "We are pleased to quote the following:";

            Quant1 = "0.00";
            Descr1 = "";
            Costs1 = "0.00";
            ECost1 = "0.00";
            Price1 = "0.00";
            Quant2 = "0.00";
            Descr2 = "";
            Costs2 = "0.00";
            ECost2 = "0.00";
            Price2 = "0.00";
            Quant3 = "0.00";
            Descr3 = "";
            Costs3 = "0.00";
            ECost3 = "0.00";
            Price3 = "0.00";
            Quant4 = "0.00";
            Descr4 = "";
            Costs4 = "0.00";
            ECost4 = "0.00";
            Price4 = "0.00";
            Quant5 = "0.00";
            Descr5 = "";
            Costs5 = "0.00";
            ECost5 = "0.00";
            Price5 = "0.00";
            Quant6 = "0.00";
            Descr6 = "";
            Costs6 = "0.00";
            ECost6 = "0.00";
            Price6 = "0.00";
            Quant7 = "0.00";
            Descr7 = "";
            Costs7 = "0.00";
            ECost7 = "0.00";
            Price7 = "0.00";
            Quant8 = "0.00";
            Descr8 = "";
            Costs8 = "0.00";
            ECost8 = "0.00";
            Price8 = "0.00";
            Quant9 = "0.00";
            Descr9 = "";
            Costs9 = "0.00";
            ECost9 = "0.00";
            Price9 = "0.00";
            Quant10 = "0.00";
            Descr10 = "";
            Costs10 = "0.00";
            ECost10 = "0.00";
            Price10 = "0.00";
            Quant11 = "0.00";
            Descr11 = "";
            Costs11 = "0.00";
            ECost11 = "0.00";
            Price11 = "0.00";
            Quant12 = "0.00";
            Descr12 = "";
            Costs12 = "0.00";
            ECost12 = "0.00";
            Price12 = "0.00";
            Quant13 = "0.00";
            Descr13 = "";
            Costs13 = "0.00";
            ECost13 = "0.00";
            Price13 = "0.00";
            Quant14 = "0.00";
            Descr14 = "";
            Costs14 = "0.00";
            ECost14 = "0.00";
            Price14 = "0.00";
            Quant15 = "0.00";
            Descr15 = "";
            Costs15 = "0.00";
            ECost15 = "0.00";
            Price15 = "0.00";
            Quant16 = "0.00";
            Descr16 = "";
            Costs16 = "0.00";
            ECost16 = "0.00";
            Price16 = "0.00";
            Quant17 = "0.00";
            Descr17 = "";
            Costs17 = "0.00";
            ECost17 = "0.00";
            Price17 = "0.00";
            Quant18 = "0.00";
            Descr18 = "";
            Costs18 = "0.00";
            ECost18 = "0.00";
            Price18 = "0.00";
            Quant19 = "0.00";
            Descr19 = "";
            Costs19 = "0.00";
            ECost19 = "0.00";
            Price19 = "0.00";
            Quant20 = "0.00";
            Descr20 = "";
            Costs20 = "0.00";
            ECost20 = "0.00";
            Price20 = "0.00";
            Quant21 = "0.00";
            Descr21 = "";
            Costs21 = "0.00";
            ECost21 = "0.00";
            Price21 = "0.00";
            Quant22 = "0.00";
            Descr22 = "";
            Costs22 = "0.00";
            ECost22 = "0.00";
            Price22 = "0.00";
            Quant23 = "0.00";
            Descr23 = "";
            Costs23 = "0.00";
            ECost23 = "0.00";
            Price23 = "0.00";

            InvNotes = "";
            DeliveryNotes = "";
            QuoteNotes = "";

            #endregion

            return false;
        }

        private int updateRowQuotes(string QPO, string Date, string Status, string Company, string VendorName, string JobName, string CustomerPO,
             string VendorNumber, string SalesAss, string SoldTo, string Street1, string Street2, string City, string State, string Zip,
             string Delivery, string Terms, string FreightSelect, string IsQManual, string IsPManual, string Location, string Equipment, string EquipCategory,
             string QuotePrice, string Credit, string Freight, string ShopTime, string TotalCost, string GrossProfit, string Profit, string MarkUp,
             string Description,
             string Quant1, string Descr1, string Costs1, string ECost1, string Price1,
             string Quant2, string Descr2, string Costs2, string ECost2, string Price2,
             string Quant3, string Descr3, string Costs3, string ECost3, string Price3,
             string Quant4, string Descr4, string Costs4, string ECost4, string Price4,
             string Quant5, string Descr5, string Costs5, string ECost5, string Price5,
             string Quant6, string Descr6, string Costs6, string ECost6, string Price6,
             string Quant7, string Descr7, string Costs7, string ECost7, string Price7,
             string Quant8, string Descr8, string Costs8, string ECost8, string Price8,
             string Quant9, string Descr9, string Costs9, string ECost9, string Price9,
             string Quant10, string Descr10, string Costs10, string ECost10, string Price10,
             string Quant11, string Descr11, string Costs11, string ECost11, string Price11,
             string Quant12, string Descr12, string Costs12, string ECost12, string Price12,
             string Quant13, string Descr13, string Costs13, string ECost13, string Price13,
             string Quant14, string Descr14, string Costs14, string ECost14, string Price14,
             string Quant15, string Descr15, string Costs15, string ECost15, string Price15,
             string Quant16, string Descr16, string Costs16, string ECost16, string Price16,
             string Quant17, string Descr17, string Costs17, string ECost17, string Price17,
             string Quant18, string Descr18, string Costs18, string ECost18, string Price18,
             string Quant19, string Descr19, string Costs19, string ECost19, string Price19,
             string Quant20, string Descr20, string Costs20, string ECost20, string Price20,
             string Quant21, string Descr21, string Costs21, string ECost21, string Price21,
             string Quant22, string Descr22, string Costs22, string ECost22, string Price22,
             string Quant23, string Descr23, string Costs23, string ECost23, string Price23,
             string InvNotes, string DeliveryNotes, string QuoteNotes)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region update string for QuoteTable

                    com.CommandText =
                    "UPDATE QuoteTable SET PO = @PO, Date = @Date, Status = @Status, Company = @Company, VendorName = @VendorName, JobName = @JobName, CustomerPO = @CustomerPO, " +
                    "VendorNumber = @VendorNumber,  SalesAss = @SalesAss,  SoldTo = @SoldTo,  Street1 = @Street1,  Street2 = @Street2,  City = @City,  State = @State,  Zip = @Zip, " +
                    "Delivery = @Delivery, Terms = @Terms,  FreightSelect = @FreightSelect,  IsQManual = @IsQManual,  IsPManual = @IsPManual,  Location = @Location,  Equipment = @Equipment,  EquipCategory = @EquipCategory, " +
                    "QuotePrice = @QuotePrice,  Credit = @Credit,  Freight = @Freight,  ShopTime = @ShopTime,  TotalCost = @TotalCost,  GrossProfit = @GrossProfit,  Profit = @Profit, MarkUp = @MarkUp, " +
                    "Description = @Description, " +
                    "Quant1 = @Quant1,  Descr1 = @Descr1,  Costs1 = @Costs1,  ECost1 = @ECost1,  Price1 = @Price1, " +
                    "Quant2 = @Quant2,  Descr2 = @Descr2,  Costs2 = @Costs2,  ECost2 = @ECost2,  Price2 = @Price2, " +
                    "Quant3 = @Quant3,  Descr3 = @Descr3,  Costs3 = @Costs3,  ECost3 = @ECost3,  Price3 = @Price3, " +
                    "Quant4 = @Quant4,  Descr4 = @Descr4,  Costs4 = @Costs4,  ECost4 = @ECost4,  Price4 = @Price4, " +
                    "Quant5 = @Quant5,  Descr5 = @Descr5,  Costs5 = @Costs5,  ECost5 = @ECost5,  Price5 = @Price5, " +
                    "Quant6 = @Quant6,  Descr6 = @Descr6,  Costs6 = @Costs6,  ECost6 = @ECost6,  Price6 = @Price6, " +
                    "Quant7 = @Quant7,  Descr7 = @Descr7,  Costs7 = @Costs7,  ECost7 = @ECost7,  Price7 = @Price7, " +
                    "Quant8 = @Quant8,  Descr8 = @Descr8,  Costs8 = @Costs8,  ECost8 = @ECost8,  Price8 = @Price8, " +
                    "Quant9 = @Quant9,  Descr9 = @Descr9,  Costs9 = @Costs9,  ECost9 = @ECost9,  Price9 = @Price9, " +
                    "Quant10 = @Quant10,  Descr10 = @Descr10,  Costs10 = @Costs10,  ECost10 = @ECost10,  Price10 = @Price10, " +
                    "Quant11 = @Quant11,  Descr11 = @Descr11,  Costs11 = @Costs11,  ECost11 = @ECost11,  Price11 = @Price11, " +
                    "Quant12 = @Quant12,  Descr12 = @Descr12,  Costs12 = @Costs12,  ECost12 = @ECost12,  Price12 = @Price12, " +
                    "Quant13 = @Quant13,  Descr13 = @Descr13,  Costs13 = @Costs13,  ECost13 = @ECost13,  Price13 = @Price13, " +
                    "Quant14 = @Quant14,  Descr14 = @Descr14,  Costs14 = @Costs14,  ECost14 = @ECost14,  Price14 = @Price14, " +
                    "Quant15 = @Quant15,  Descr15 = @Descr15,  Costs15 = @Costs15,  ECost15 = @ECost15,  Price15 = @Price15, " +
                    "Quant16 = @Quant16,  Descr16 = @Descr16,  Costs16 = @Costs16,  ECost16 = @ECost16,  Price16 = @Price16, " +
                    "Quant17 = @Quant17,  Descr17 = @Descr17,  Costs17 = @Costs17,  ECost17 = @ECost17,  Price17 = @Price17, " +
                    "Quant18 = @Quant18,  Descr18 = @Descr18,  Costs18 = @Costs18,  ECost18 = @ECost18,  Price18 = @Price18, " +
                    "Quant19 = @Quant19,  Descr19 = @Descr19,  Costs19 = @Costs19,  ECost19 = @ECost19,  Price19 = @Price19, " +
                    "Quant20 = @Quant20,  Descr20 = @Descr20,  Costs20 = @Costs20,  ECost20 = @ECost20,  Price20 = @Price20, " +
                    "Quant21 = @Quant21,  Descr21 = @Descr21,  Costs21 = @Costs21,  ECost21 = @ECost21,  Price21 = @Price21, " +
                    "Quant22 = @Quant22,  Descr22 = @Descr22,  Costs22 = @Costs22,  ECost22 = @ECost22,  Price22 = @Price22, " +
                    "Quant23 = @Quant23,  Descr23 = @Descr23,  Costs23 = @Costs23,  ECost23 = @ECost23,  Price23 = @Price23, " +
                    "InvNotes = @InvNotes, DeliveryNotes = @DeliveryNotes, QuoteNotes = @QuoteNotes " +
                    "Where PO = @PO";

                    #endregion

                    #region Parameters for QuoteTable

                    com.Parameters.AddWithValue("@PO", QPO);
                    com.Parameters.AddWithValue("@Date", Date);
                    com.Parameters.AddWithValue("@Status", Status);
                    com.Parameters.AddWithValue("@Company", Company);
                    com.Parameters.AddWithValue("@VendorName", VendorName);
                    com.Parameters.AddWithValue("@JobName", JobName);
                    com.Parameters.AddWithValue("@CustomerPO", CustomerPO);
                    com.Parameters.AddWithValue("@VendorNumber", VendorNumber);
                    com.Parameters.AddWithValue("@SalesAss", SalesAss);
                    com.Parameters.AddWithValue("@SoldTo", SoldTo);
                    com.Parameters.AddWithValue("@Street1", Street1);
                    com.Parameters.AddWithValue("@Street2", Street2);
                    com.Parameters.AddWithValue("@City", City);
                    com.Parameters.AddWithValue("@State", State);
                    com.Parameters.AddWithValue("@Zip", Zip);
                    com.Parameters.AddWithValue("@Delivery", Delivery);
                    com.Parameters.AddWithValue("@Terms", Terms);
                    com.Parameters.AddWithValue("@FreightSelect", FreightSelect);
                    com.Parameters.AddWithValue("@IsQManual", IsQManual);
                    com.Parameters.AddWithValue("@IsPManual", IsPManual);
                    com.Parameters.AddWithValue("@Location", Location);
                    com.Parameters.AddWithValue("@Equipment", Equipment);
                    com.Parameters.AddWithValue("@EquipCategory", EquipCategory);

                    com.Parameters.AddWithValue("@QuotePrice", QuotePrice);
                    com.Parameters.AddWithValue("@Credit", Credit);
                    com.Parameters.AddWithValue("@Freight", Freight);
                    com.Parameters.AddWithValue("@ShopTime", ShopTime);
                    com.Parameters.AddWithValue("@TotalCost", TotalCost);
                    com.Parameters.AddWithValue("@GrossProfit", GrossProfit);
                    com.Parameters.AddWithValue("@Profit", Profit);
                    com.Parameters.AddWithValue("@MarkUp", MarkUp);
                    com.Parameters.AddWithValue("@Description", Description);

                    com.Parameters.AddWithValue("@Quant1", Quant1);
                    com.Parameters.AddWithValue("@Descr1", Descr1);
                    com.Parameters.AddWithValue("@Costs1", Costs1);
                    com.Parameters.AddWithValue("@ECost1", ECost1);
                    com.Parameters.AddWithValue("@Price1", Price1);
                    com.Parameters.AddWithValue("@Quant2", Quant2);
                    com.Parameters.AddWithValue("@Descr2", Descr2);
                    com.Parameters.AddWithValue("@Costs2", Costs2);
                    com.Parameters.AddWithValue("@ECost2", ECost2);
                    com.Parameters.AddWithValue("@Price2", Price2);
                    com.Parameters.AddWithValue("@Quant3", Quant3);
                    com.Parameters.AddWithValue("@Descr3", Descr3);
                    com.Parameters.AddWithValue("@Costs3", Costs3);
                    com.Parameters.AddWithValue("@ECost3", ECost3);
                    com.Parameters.AddWithValue("@Price3", Price3);
                    com.Parameters.AddWithValue("@Quant4", Quant4);
                    com.Parameters.AddWithValue("@Descr4", Descr4);
                    com.Parameters.AddWithValue("@Costs4", Costs4);
                    com.Parameters.AddWithValue("@ECost4", ECost4);
                    com.Parameters.AddWithValue("@Price4", Price4);
                    com.Parameters.AddWithValue("@Quant5", Quant5);
                    com.Parameters.AddWithValue("@Descr5", Descr5);
                    com.Parameters.AddWithValue("@Costs5", Costs5);
                    com.Parameters.AddWithValue("@ECost5", ECost5);
                    com.Parameters.AddWithValue("@Price5", Price5);
                    com.Parameters.AddWithValue("@Quant6", Quant6);
                    com.Parameters.AddWithValue("@Descr6", Descr6);
                    com.Parameters.AddWithValue("@Costs6", Costs6);
                    com.Parameters.AddWithValue("@ECost6", ECost6);
                    com.Parameters.AddWithValue("@Price6", Price6);
                    com.Parameters.AddWithValue("@Quant7", Quant7);
                    com.Parameters.AddWithValue("@Descr7", Descr7);
                    com.Parameters.AddWithValue("@Costs7", Costs7);
                    com.Parameters.AddWithValue("@ECost7", ECost7);
                    com.Parameters.AddWithValue("@Price7", Price7);
                    com.Parameters.AddWithValue("@Quant8", Quant8);
                    com.Parameters.AddWithValue("@Descr8", Descr8);
                    com.Parameters.AddWithValue("@Costs8", Costs8);
                    com.Parameters.AddWithValue("@ECost8", ECost8);
                    com.Parameters.AddWithValue("@Price8", Price8);
                    com.Parameters.AddWithValue("@Quant9", Quant9);
                    com.Parameters.AddWithValue("@Descr9", Descr9);
                    com.Parameters.AddWithValue("@Costs9", Costs9);
                    com.Parameters.AddWithValue("@ECost9", ECost9);
                    com.Parameters.AddWithValue("@Price9", Price9);
                    com.Parameters.AddWithValue("@Quant10", Quant10);
                    com.Parameters.AddWithValue("@Descr10", Descr10);
                    com.Parameters.AddWithValue("@Costs10", Costs10);
                    com.Parameters.AddWithValue("@ECost10", ECost10);
                    com.Parameters.AddWithValue("@Price10", Price10);
                    com.Parameters.AddWithValue("@Quant11", Quant11);
                    com.Parameters.AddWithValue("@Descr11", Descr11);
                    com.Parameters.AddWithValue("@Costs11", Costs11);
                    com.Parameters.AddWithValue("@ECost11", ECost11);
                    com.Parameters.AddWithValue("@Price11", Price11);
                    com.Parameters.AddWithValue("@Quant12", Quant12);
                    com.Parameters.AddWithValue("@Descr12", Descr12);
                    com.Parameters.AddWithValue("@Costs12", Costs12);
                    com.Parameters.AddWithValue("@ECost12", ECost12);
                    com.Parameters.AddWithValue("@Price12", Price12);
                    com.Parameters.AddWithValue("@Quant13", Quant13);
                    com.Parameters.AddWithValue("@Descr13", Descr13);
                    com.Parameters.AddWithValue("@Costs13", Costs13);
                    com.Parameters.AddWithValue("@ECost13", ECost13);
                    com.Parameters.AddWithValue("@Price13", Price13);
                    com.Parameters.AddWithValue("@Quant14", Quant14);
                    com.Parameters.AddWithValue("@Descr14", Descr14);
                    com.Parameters.AddWithValue("@Costs14", Costs14);
                    com.Parameters.AddWithValue("@ECost14", ECost14);
                    com.Parameters.AddWithValue("@Price14", Price14);
                    com.Parameters.AddWithValue("@Quant15", Quant15);
                    com.Parameters.AddWithValue("@Descr15", Descr15);
                    com.Parameters.AddWithValue("@Costs15", Costs15);
                    com.Parameters.AddWithValue("@ECost15", ECost15);
                    com.Parameters.AddWithValue("@Price15", Price15);
                    com.Parameters.AddWithValue("@Quant16", Quant16);
                    com.Parameters.AddWithValue("@Descr16", Descr16);
                    com.Parameters.AddWithValue("@Costs16", Costs16);
                    com.Parameters.AddWithValue("@ECost16", ECost16);
                    com.Parameters.AddWithValue("@Price16", Price16);
                    com.Parameters.AddWithValue("@Quant17", Quant17);
                    com.Parameters.AddWithValue("@Descr17", Descr17);
                    com.Parameters.AddWithValue("@Costs17", Costs17);
                    com.Parameters.AddWithValue("@ECost17", ECost17);
                    com.Parameters.AddWithValue("@Price17", Price17);
                    com.Parameters.AddWithValue("@Quant18", Quant18);
                    com.Parameters.AddWithValue("@Descr18", Descr18);
                    com.Parameters.AddWithValue("@Costs18", Costs18);
                    com.Parameters.AddWithValue("@ECost18", ECost18);
                    com.Parameters.AddWithValue("@Price18", Price18);
                    com.Parameters.AddWithValue("@Quant19", Quant19);
                    com.Parameters.AddWithValue("@Descr19", Descr19);
                    com.Parameters.AddWithValue("@Costs19", Costs19);
                    com.Parameters.AddWithValue("@ECost19", ECost19);
                    com.Parameters.AddWithValue("@Price19", Price19);
                    com.Parameters.AddWithValue("@Quant20", Quant20);
                    com.Parameters.AddWithValue("@Descr20", Descr20);
                    com.Parameters.AddWithValue("@Costs20", Costs20);
                    com.Parameters.AddWithValue("@ECost20", ECost20);
                    com.Parameters.AddWithValue("@Price20", Price20);
                    com.Parameters.AddWithValue("@Quant21", Quant21);
                    com.Parameters.AddWithValue("@Descr21", Descr21);
                    com.Parameters.AddWithValue("@Costs21", Costs21);
                    com.Parameters.AddWithValue("@ECost21", ECost21);
                    com.Parameters.AddWithValue("@Price21", Price21);
                    com.Parameters.AddWithValue("@Quant22", Quant22);
                    com.Parameters.AddWithValue("@Descr22", Descr22);
                    com.Parameters.AddWithValue("@Costs22", Costs22);
                    com.Parameters.AddWithValue("@ECost22", ECost22);
                    com.Parameters.AddWithValue("@Price22", Price22);
                    com.Parameters.AddWithValue("@Quant23", Quant23);
                    com.Parameters.AddWithValue("@Descr23", Descr23);
                    com.Parameters.AddWithValue("@Costs23", Costs23);
                    com.Parameters.AddWithValue("@ECost23", ECost23);
                    com.Parameters.AddWithValue("@Price23", Price23);

                    com.Parameters.AddWithValue("@InvNotes", InvNotes);
                    com.Parameters.AddWithValue("@DeliveryNotes", DeliveryNotes);
                    com.Parameters.AddWithValue("@QuoteNotes", QuoteNotes);

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

        private int insertRowQuotes(string QPO, string Date, string Status, string Company, string VendorName, string JobName, string CustomerPO,
             string VendorNumber, string SalesAss, string SoldTo, string Street1, string Street2, string City, string State, string Zip,
             string Delivery, string Terms, string FreightSelect, string IsQManual, string IsPManual, string Location, string Equipment, string EquipCategory,
             string QuotePrice, string Credit, string Freight, string ShopTime, string TotalCost, string GrossProfit, string Profit, string MarkUp,
             string Description,
             string Quant1, string Descr1, string Costs1, string ECost1, string Price1,
             string Quant2, string Descr2, string Costs2, string ECost2, string Price2,
             string Quant3, string Descr3, string Costs3, string ECost3, string Price3,
             string Quant4, string Descr4, string Costs4, string ECost4, string Price4,
             string Quant5, string Descr5, string Costs5, string ECost5, string Price5,
             string Quant6, string Descr6, string Costs6, string ECost6, string Price6,
             string Quant7, string Descr7, string Costs7, string ECost7, string Price7,
             string Quant8, string Descr8, string Costs8, string ECost8, string Price8,
             string Quant9, string Descr9, string Costs9, string ECost9, string Price9,
             string Quant10, string Descr10, string Costs10, string ECost10, string Price10,
             string Quant11, string Descr11, string Costs11, string ECost11, string Price11,
             string Quant12, string Descr12, string Costs12, string ECost12, string Price12,
             string Quant13, string Descr13, string Costs13, string ECost13, string Price13,
             string Quant14, string Descr14, string Costs14, string ECost14, string Price14,
             string Quant15, string Descr15, string Costs15, string ECost15, string Price15,
             string Quant16, string Descr16, string Costs16, string ECost16, string Price16,
             string Quant17, string Descr17, string Costs17, string ECost17, string Price17,
             string Quant18, string Descr18, string Costs18, string ECost18, string Price18,
             string Quant19, string Descr19, string Costs19, string ECost19, string Price19,
             string Quant20, string Descr20, string Costs20, string ECost20, string Price20,
             string Quant21, string Descr21, string Costs21, string ECost21, string Price21,
             string Quant22, string Descr22, string Costs22, string ECost22, string Price22,
             string Quant23, string Descr23, string Costs23, string ECost23, string Price23,
             string InvNotes, string DeliveryNotes, string QuoteNotes)
        {
            SQLiteConnection con = GetConnection();
            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("data source=" + getDbasePathName()))
            {
                
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    #region insert string for QuoteTable

                    com.CommandText =
                    "INSERT INTO QuoteTable (PO, Date, Status, Company, VendorName, JobName, CustomerPO, " +
                    "VendorNumber, SalesAss, SoldTo, Street1, Street2, City, State, Zip, " +
                    "Delivery, Terms, FreightSelect, IsQManual, IsPManual, Location, Equipment, EquipCategory, " +
                    "QuotePrice, Credit, Freight, ShopTime, TotalCost, GrossProfit, Profit, MarkUp, " +
                    "Description, " +
                    "Quant1, Descr1, Costs1, ECost1, Price1, " +
                    "Quant2, Descr2, Costs2, ECost2, Price2, " +
                    "Quant3, Descr3, Costs3, ECost3, Price3, " +
                    "Quant4, Descr4, Costs4, ECost4, Price4, " +
                    "Quant5, Descr5, Costs5, ECost5, Price5, " +
                    "Quant6, Descr6, Costs6, ECost6, Price6, " +
                    "Quant7, Descr7, Costs7, ECost7, Price7, " +
                    "Quant8, Descr8, Costs8, ECost8, Price8, " +
                    "Quant9, Descr9, Costs9, ECost9, Price9, " +
                    "Quant10, Descr10, Costs10, ECost10, Price10, " +
                    "Quant11, Descr11, Costs11, ECost11, Price11, " +
                    "Quant12, Descr12, Costs12, ECost12, Price12, " +
                    "Quant13, Descr13, Costs13, ECost13, Price13, " +
                    "Quant14, Descr14, Costs14, ECost14, Price14, " +
                    "Quant15, Descr15, Costs15, ECost15, Price15, " +
                    "Quant16, Descr16, Costs16, ECost16, Price16, " +
                    "Quant17, Descr17, Costs17, ECost17, Price17, " +
                    "Quant18, Descr18, Costs18, ECost18, Price18, " +
                    "Quant19, Descr19, Costs19, ECost19, Price19, " +
                    "Quant20, Descr20, Costs20, ECost20, Price20, " +
                    "Quant21, Descr21, Costs21, ECost21, Price21, " +
                    "Quant22, Descr22, Costs22, ECost22, Price22, " +
                    "Quant23, Descr23, Costs23, ECost23, Price23, " +
                    "InvNotes, DeliveryNotes, QuoteNotes) VALUES " +

                    "(@PO, @Date, @Status, @Company, @VendorName, @JobName, @CustomerPO, " +
                    "@VendorNumber, @SalesAss, @SoldTo, @Street1, @Street2, @City, @State, @Zip, " +
                    "@Delivery, @Terms, @FreightSelect, @IsQManual, @IsPManual, @Location, @Equipment, @EquipCategory, " +
                    "@QuotePrice, @Credit, @Freight, @ShopTime, @TotalCost, @GrossProfit, @Profit, @MarkUp, " +
                    "@Description, " +
                    "@Quant1, @Descr1, @Costs1, @ECost1, @Price1, " +
                    "@Quant2, @Descr2, @Costs2, @ECost2, @Price2, " +
                    "@Quant3, @Descr3, @Costs3, @ECost3, @Price3, " +
                    "@Quant4, @Descr4, @Costs4, @ECost4, @Price4, " +
                    "@Quant5, @Descr5, @Costs5, @ECost5, @Price5, " +
                    "@Quant6, @Descr6, @Costs6, @ECost6, @Price6, " +
                    "@Quant7, @Descr7, @Costs7, @ECost7, @Price7, " +
                    "@Quant8, @Descr8, @Costs8, @ECost8, @Price8, " +
                    "@Quant9, @Descr9, @Costs9, @ECost9, @Price9, " +
                    "@Quant10, @Descr10, @Costs10, @ECost10, @Price10, " +
                    "@Quant11, @Descr11, @Costs11, @ECost11, @Price11, " +
                    "@Quant12, @Descr12, @Costs12, @ECost12, @Price12, " +
                    "@Quant13, @Descr13, @Costs13, @ECost13, @Price13, " +
                    "@Quant14, @Descr14, @Costs14, @ECost14, @Price14, " +
                    "@Quant15, @Descr15, @Costs15, @ECost15, @Price15, " +
                    "@Quant16, @Descr16, @Costs16, @ECost16, @Price16, " +
                    "@Quant17, @Descr17, @Costs17, @ECost17, @Price17, " +
                    "@Quant18, @Descr18, @Costs18, @ECost18, @Price18, " +
                    "@Quant19, @Descr19, @Costs19, @ECost19, @Price19, " +
                    "@Quant20, @Descr20, @Costs20, @ECost20, @Price20, " +
                    "@Quant21, @Descr21, @Costs21, @ECost21, @Price21, " +
                    "@Quant22, @Descr22, @Costs22, @ECost22, @Price22, " +
                    "@Quant23, @Descr23, @Costs23, @ECost23, @Price23, " +
                    "@InvNotes, @DeliveryNotes, @QuoteNotes)";

                    #endregion

                    #region Parameters for QuoteTable

                    com.Parameters.AddWithValue("@PO", QPO);
                    com.Parameters.AddWithValue("@Date", Date);
                    com.Parameters.AddWithValue("@Status", Status);
                    com.Parameters.AddWithValue("@Company", Company);
                    com.Parameters.AddWithValue("@VendorName", VendorName);
                    com.Parameters.AddWithValue("@JobName", JobName);
                    com.Parameters.AddWithValue("@CustomerPO", CustomerPO);
                    com.Parameters.AddWithValue("@VendorNumber", VendorNumber);
                    com.Parameters.AddWithValue("@SalesAss", SalesAss);
                    com.Parameters.AddWithValue("@SoldTo", SoldTo);
                    com.Parameters.AddWithValue("@Street1", Street1);
                    com.Parameters.AddWithValue("@Street2", Street2);
                    com.Parameters.AddWithValue("@City", City);
                    com.Parameters.AddWithValue("@State", State);
                    com.Parameters.AddWithValue("@Zip", Zip);
                    com.Parameters.AddWithValue("@Delivery", Delivery);
                    com.Parameters.AddWithValue("@Terms", Terms);
                    com.Parameters.AddWithValue("@FreightSelect", FreightSelect);
                    com.Parameters.AddWithValue("@IsQManual", IsQManual);
                    com.Parameters.AddWithValue("@IsPManual", IsPManual);
                    com.Parameters.AddWithValue("@Location", Location);
                    com.Parameters.AddWithValue("@Equipment", Equipment);
                    com.Parameters.AddWithValue("@EquipCategory", EquipCategory);

                    com.Parameters.AddWithValue("@QuotePrice", QuotePrice);
                    com.Parameters.AddWithValue("@Credit", Credit);
                    com.Parameters.AddWithValue("@Freight", Freight);
                    com.Parameters.AddWithValue("@ShopTime", ShopTime);
                    com.Parameters.AddWithValue("@TotalCost", TotalCost);
                    com.Parameters.AddWithValue("@GrossProfit", GrossProfit);
                    com.Parameters.AddWithValue("@Profit", Profit);
                    com.Parameters.AddWithValue("@MarkUp", MarkUp);
                    com.Parameters.AddWithValue("@Description", Description);

                    com.Parameters.AddWithValue("@Quant1", Quant1);
                    com.Parameters.AddWithValue("@Descr1", Descr1);
                    com.Parameters.AddWithValue("@Costs1", Costs1);
                    com.Parameters.AddWithValue("@ECost1", ECost1);
                    com.Parameters.AddWithValue("@Price1", Price1);
                    com.Parameters.AddWithValue("@Quant2", Quant2);
                    com.Parameters.AddWithValue("@Descr2", Descr2);
                    com.Parameters.AddWithValue("@Costs2", Costs2);
                    com.Parameters.AddWithValue("@ECost2", ECost2);
                    com.Parameters.AddWithValue("@Price2", Price2);
                    com.Parameters.AddWithValue("@Quant3", Quant3);
                    com.Parameters.AddWithValue("@Descr3", Descr3);
                    com.Parameters.AddWithValue("@Costs3", Costs3);
                    com.Parameters.AddWithValue("@ECost3", ECost3);
                    com.Parameters.AddWithValue("@Price3", Price3);
                    com.Parameters.AddWithValue("@Quant4", Quant4);
                    com.Parameters.AddWithValue("@Descr4", Descr4);
                    com.Parameters.AddWithValue("@Costs4", Costs4);
                    com.Parameters.AddWithValue("@ECost4", ECost4);
                    com.Parameters.AddWithValue("@Price4", Price4);
                    com.Parameters.AddWithValue("@Quant5", Quant5);
                    com.Parameters.AddWithValue("@Descr5", Descr5);
                    com.Parameters.AddWithValue("@Costs5", Costs5);
                    com.Parameters.AddWithValue("@ECost5", ECost5);
                    com.Parameters.AddWithValue("@Price5", Price5);
                    com.Parameters.AddWithValue("@Quant6", Quant6);
                    com.Parameters.AddWithValue("@Descr6", Descr6);
                    com.Parameters.AddWithValue("@Costs6", Costs6);
                    com.Parameters.AddWithValue("@ECost6", ECost6);
                    com.Parameters.AddWithValue("@Price6", Price6);
                    com.Parameters.AddWithValue("@Quant7", Quant7);
                    com.Parameters.AddWithValue("@Descr7", Descr7);
                    com.Parameters.AddWithValue("@Costs7", Costs7);
                    com.Parameters.AddWithValue("@ECost7", ECost7);
                    com.Parameters.AddWithValue("@Price7", Price7);
                    com.Parameters.AddWithValue("@Quant8", Quant8);
                    com.Parameters.AddWithValue("@Descr8", Descr8);
                    com.Parameters.AddWithValue("@Costs8", Costs8);
                    com.Parameters.AddWithValue("@ECost8", ECost8);
                    com.Parameters.AddWithValue("@Price8", Price8);
                    com.Parameters.AddWithValue("@Quant9", Quant9);
                    com.Parameters.AddWithValue("@Descr9", Descr9);
                    com.Parameters.AddWithValue("@Costs9", Costs9);
                    com.Parameters.AddWithValue("@ECost9", ECost9);
                    com.Parameters.AddWithValue("@Price9", Price9);
                    com.Parameters.AddWithValue("@Quant10", Quant10);
                    com.Parameters.AddWithValue("@Descr10", Descr10);
                    com.Parameters.AddWithValue("@Costs10", Costs10);
                    com.Parameters.AddWithValue("@ECost10", ECost10);
                    com.Parameters.AddWithValue("@Price10", Price10);
                    com.Parameters.AddWithValue("@Quant11", Quant11);
                    com.Parameters.AddWithValue("@Descr11", Descr11);
                    com.Parameters.AddWithValue("@Costs11", Costs11);
                    com.Parameters.AddWithValue("@ECost11", ECost11);
                    com.Parameters.AddWithValue("@Price11", Price11);
                    com.Parameters.AddWithValue("@Quant12", Quant12);
                    com.Parameters.AddWithValue("@Descr12", Descr12);
                    com.Parameters.AddWithValue("@Costs12", Costs12);
                    com.Parameters.AddWithValue("@ECost12", ECost12);
                    com.Parameters.AddWithValue("@Price12", Price12);
                    com.Parameters.AddWithValue("@Quant13", Quant13);
                    com.Parameters.AddWithValue("@Descr13", Descr13);
                    com.Parameters.AddWithValue("@Costs13", Costs13);
                    com.Parameters.AddWithValue("@ECost13", ECost13);
                    com.Parameters.AddWithValue("@Price13", Price13);
                    com.Parameters.AddWithValue("@Quant14", Quant14);
                    com.Parameters.AddWithValue("@Descr14", Descr14);
                    com.Parameters.AddWithValue("@Costs14", Costs14);
                    com.Parameters.AddWithValue("@ECost14", ECost14);
                    com.Parameters.AddWithValue("@Price14", Price14);
                    com.Parameters.AddWithValue("@Quant15", Quant15);
                    com.Parameters.AddWithValue("@Descr15", Descr15);
                    com.Parameters.AddWithValue("@Costs15", Costs15);
                    com.Parameters.AddWithValue("@ECost15", ECost15);
                    com.Parameters.AddWithValue("@Price15", Price15);
                    com.Parameters.AddWithValue("@Quant16", Quant16);
                    com.Parameters.AddWithValue("@Descr16", Descr16);
                    com.Parameters.AddWithValue("@Costs16", Costs16);
                    com.Parameters.AddWithValue("@ECost16", ECost16);
                    com.Parameters.AddWithValue("@Price16", Price16);
                    com.Parameters.AddWithValue("@Quant17", Quant17);
                    com.Parameters.AddWithValue("@Descr17", Descr17);
                    com.Parameters.AddWithValue("@Costs17", Costs17);
                    com.Parameters.AddWithValue("@ECost17", ECost17);
                    com.Parameters.AddWithValue("@Price17", Price17);
                    com.Parameters.AddWithValue("@Quant18", Quant18);
                    com.Parameters.AddWithValue("@Descr18", Descr18);
                    com.Parameters.AddWithValue("@Costs18", Costs18);
                    com.Parameters.AddWithValue("@ECost18", ECost18);
                    com.Parameters.AddWithValue("@Price18", Price18);
                    com.Parameters.AddWithValue("@Quant19", Quant19);
                    com.Parameters.AddWithValue("@Descr19", Descr19);
                    com.Parameters.AddWithValue("@Costs19", Costs19);
                    com.Parameters.AddWithValue("@ECost19", ECost19);
                    com.Parameters.AddWithValue("@Price19", Price19);
                    com.Parameters.AddWithValue("@Quant20", Quant20);
                    com.Parameters.AddWithValue("@Descr20", Descr20);
                    com.Parameters.AddWithValue("@Costs20", Costs20);
                    com.Parameters.AddWithValue("@ECost20", ECost20);
                    com.Parameters.AddWithValue("@Price20", Price20);
                    com.Parameters.AddWithValue("@Quant21", Quant21);
                    com.Parameters.AddWithValue("@Descr21", Descr21);
                    com.Parameters.AddWithValue("@Costs21", Costs21);
                    com.Parameters.AddWithValue("@ECost21", ECost21);
                    com.Parameters.AddWithValue("@Price21", Price21);
                    com.Parameters.AddWithValue("@Quant22", Quant22);
                    com.Parameters.AddWithValue("@Descr22", Descr22);
                    com.Parameters.AddWithValue("@Costs22", Costs22);
                    com.Parameters.AddWithValue("@ECost22", ECost22);
                    com.Parameters.AddWithValue("@Price22", Price22);
                    com.Parameters.AddWithValue("@Quant23", Quant23);
                    com.Parameters.AddWithValue("@Descr23", Descr23);
                    com.Parameters.AddWithValue("@Costs23", Costs23);
                    com.Parameters.AddWithValue("@ECost23", ECost23);
                    com.Parameters.AddWithValue("@Price23", Price23);

                    com.Parameters.AddWithValue("@InvNotes", InvNotes);
                    com.Parameters.AddWithValue("@DeliveryNotes", DeliveryNotes);
                    com.Parameters.AddWithValue("@QuoteNotes", QuoteNotes);

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

        #region Quote read & Update GUI

        private void readQuoteAndUpdateGUI(string QPO, int row)
        {
            string theQPO; string Date; string Status; string Company; string VendorName; string JobName; string CustomerPO;
            string VendorNumber; string SalesAss; string SoldTo; string Street1; string Street2; string City; string State; string Zip;
            string Delivery; string Terms; string FreightSelect; string IsQManual; string IsPManual; string Location; string Equipment; string EquipCategory;
            string QuotePrice; string Credit; string Freight; string ShopTime; string TotalCost; string GrossProfit; string Profit; string MarkUp;
            string Description;
            string Quant1; string Descr1; string Costs1; string ECost1; string Price1;
            string Quant2; string Descr2; string Costs2; string ECost2; string Price2;
            string Quant3; string Descr3; string Costs3; string ECost3; string Price3;
            string Quant4; string Descr4; string Costs4; string ECost4; string Price4;
            string Quant5; string Descr5; string Costs5; string ECost5; string Price5;
            string Quant6; string Descr6; string Costs6; string ECost6; string Price6;
            string Quant7; string Descr7; string Costs7; string ECost7; string Price7;
            string Quant8; string Descr8; string Costs8; string ECost8; string Price8;
            string Quant9; string Descr9; string Costs9; string ECost9; string Price9;
            string Quant10; string Descr10; string Costs10; string ECost10; string Price10;
            string Quant11; string Descr11; string Costs11; string ECost11; string Price11;
            string Quant12; string Descr12; string Costs12; string ECost12; string Price12;
            string Quant13; string Descr13; string Costs13; string ECost13; string Price13;
            string Quant14; string Descr14; string Costs14; string ECost14; string Price14;
            string Quant15; string Descr15; string Costs15; string ECost15; string Price15;
            string Quant16; string Descr16; string Costs16; string ECost16; string Price16;
            string Quant17; string Descr17; string Costs17; string ECost17; string Price17;
            string Quant18; string Descr18; string Costs18; string ECost18; string Price18;
            string Quant19; string Descr19; string Costs19; string ECost19; string Price19;
            string Quant20; string Descr20; string Costs20; string ECost20; string Price20;
            string Quant21; string Descr21; string Costs21; string ECost21; string Price21;
            string Quant22; string Descr22; string Costs22; string ECost22; string Price22;
            string Quant23; string Descr23; string Costs23; string ECost23; string Price23;
            string InvNotes; string DeliveryNotes; string QuoteNotes;

            readRowQuotes(QPO, row, out theQPO, out Date, out Status, out Company, out VendorName, out JobName, out CustomerPO,
            out VendorNumber, out SalesAss, out SoldTo, out Street1, out Street2, out City, out State, out Zip,
            out Delivery, out Terms, out FreightSelect, out IsQManual, out IsPManual, out Location, out Equipment, out EquipCategory,
            out QuotePrice, out Credit, out Freight, out ShopTime, out TotalCost, out GrossProfit, out Profit, out MarkUp,
            out Description,
            out Quant1, out Descr1, out Costs1, out ECost1, out Price1,
            out Quant2, out Descr2, out Costs2, out ECost2, out Price2,
            out Quant3, out Descr3, out Costs3, out ECost3, out Price3,
            out Quant4, out Descr4, out Costs4, out ECost4, out Price4,
            out Quant5, out Descr5, out Costs5, out ECost5, out Price5,
            out Quant6, out Descr6, out Costs6, out ECost6, out Price6,
            out Quant7, out Descr7, out Costs7, out ECost7, out Price7,
            out Quant8, out Descr8, out Costs8, out ECost8, out Price8,
            out Quant9, out Descr9, out Costs9, out ECost9, out Price9,
            out Quant10, out Descr10, out Costs10, out ECost10, out Price10,
            out Quant11, out Descr11, out Costs11, out ECost11, out Price11,
            out Quant12, out Descr12, out Costs12, out ECost12, out Price12,
            out Quant13, out Descr13, out Costs13, out ECost13, out Price13,
            out Quant14, out Descr14, out Costs14, out ECost14, out Price14,
            out Quant15, out Descr15, out Costs15, out ECost15, out Price15,
            out Quant16, out Descr16, out Costs16, out ECost16, out Price16,
            out Quant17, out Descr17, out Costs17, out ECost17, out Price17,
            out Quant18, out Descr18, out Costs18, out ECost18, out Price18,
            out Quant19, out Descr19, out Costs19, out ECost19, out Price19,
            out Quant20, out Descr20, out Costs20, out ECost20, out Price20,
            out Quant21, out Descr21, out Costs21, out ECost21, out Price21,
            out Quant22, out Descr22, out Costs22, out ECost22, out Price22,
            out Quant23, out Descr23, out Costs23, out ECost23, out Price23,
            out InvNotes, out DeliveryNotes, out QuoteNotes);

            #region update the GUI with above read params
            isDataLoadingQuotes = true;

            this.textBoxQPO.Text = theQPO;
            this.textBoxQDate.Text = Date;
            this.comboBoxQStatus.Text = Status;

            this.textBoxQCompany.Text = Company;
            this.textBoxQCompanyReadOnly.Text = Company; // mirror

            this.textBoxQVendorName.Text = VendorName;
            this.textBoxQJobName.Text = JobName;
            this.textBoxQCustomerPO.Text = CustomerPO;
            this.textBoxQVendorNumber.Text = VendorNumber;
            this.comboBoxQSalesAss.Text = SalesAss;
            this.textBoxQTo.Text = SoldTo;
            this.textBoxQStreet1.Text = Street1;
            this.textBoxQStreet2.Text = Street2;
            this.textBoxQCity.Text = City;
            this.comboBoxQState.Text = State;
            this.textBoxQZip.Text = Zip;

            this.comboBoxQDelivery.Text = Delivery;
            this.comboBoxQTerms.Text = Terms;
            this.comboBoxFreightSelect.Text = FreightSelect;
            this.checkBoxQManual.Checked = isStringTrue(IsQManual);
            this.checkBoxPManual.Checked = isStringTrue(IsPManual);
            this.textBoxQLocation.Text = Location;
            this.textBoxQEquipment.Text = Equipment;
            this.comboBoxQEquipCategory.Text = EquipCategory;
            

            this.numericUpDownQQuotePrice.Text = QuotePrice;
            this.textBoxQQuotedPriceReadOnly.Text = QuotePrice; // mirror
            
            this.numericUpDownQCredit.Text = Credit;
            this.numericUpDownQFreight.Text = Freight;
            this.numericUpDownQShopTime.Text = ShopTime;
            this.numericUpDownQTotalCost.Text = TotalCost;
            this.numericUpDownQGrossProfit.Text = GrossProfit;
            this.numericUpDownQProfit.Text = Profit;
            this.numericUpDownQMarkUp.Text = MarkUp;

            this.textBoxQHeader.Text = Description;
            this.numericUpDownQQuan1.Text = Quant1; this.textBoxQDescription1.Text = Descr1; this.numericUpDownQCost1.Text = Costs1; this.numericUpDownQECost1.Text = ECost1; this.numericUpDownQMCost1.Text = Price1;
            this.numericUpDownQQuan2.Text = Quant2; this.textBoxQDescription2.Text = Descr2; this.numericUpDownQCost2.Text = Costs2; this.numericUpDownQECost2.Text = ECost2; this.numericUpDownQMCost2.Text = Price2;
            this.numericUpDownQQuan3.Text = Quant3; this.textBoxQDescription3.Text = Descr3; this.numericUpDownQCost3.Text = Costs3; this.numericUpDownQECost3.Text = ECost3; this.numericUpDownQMCost3.Text = Price3;
            this.numericUpDownQQuan4.Text = Quant4; this.textBoxQDescription4.Text = Descr4; this.numericUpDownQCost4.Text = Costs4; this.numericUpDownQECost4.Text = ECost4; this.numericUpDownQMCost4.Text = Price4;
            this.numericUpDownQQuan5.Text = Quant5; this.textBoxQDescription5.Text = Descr5; this.numericUpDownQCost5.Text = Costs5; this.numericUpDownQECost5.Text = ECost5; this.numericUpDownQMCost5.Text = Price5;
            this.numericUpDownQQuan6.Text = Quant6; this.textBoxQDescription6.Text = Descr6; this.numericUpDownQCost6.Text = Costs6; this.numericUpDownQECost6.Text = ECost6; this.numericUpDownQMCost6.Text = Price6;
            this.numericUpDownQQuan7.Text = Quant7; this.textBoxQDescription7.Text = Descr7; this.numericUpDownQCost7.Text = Costs7; this.numericUpDownQECost7.Text = ECost7; this.numericUpDownQMCost7.Text = Price7;
            this.numericUpDownQQuan8.Text = Quant8; this.textBoxQDescription8.Text = Descr8; this.numericUpDownQCost8.Text = Costs8; this.numericUpDownQECost8.Text = ECost8; this.numericUpDownQMCost8.Text = Price8;
            this.numericUpDownQQuan9.Text = Quant9; this.textBoxQDescription9.Text = Descr9; this.numericUpDownQCost9.Text = Costs9; this.numericUpDownQECost9.Text = ECost9; this.numericUpDownQMCost9.Text = Price9;
            this.numericUpDownQQuan10.Text = Quant10; this.textBoxQDescription10.Text = Descr10; this.numericUpDownQCost10.Text = Costs10; this.numericUpDownQECost10.Text = ECost10; this.numericUpDownQMCost10.Text = Price10;
            this.numericUpDownQQuan11.Text = Quant11; this.textBoxQDescription11.Text = Descr11; this.numericUpDownQCost11.Text = Costs11; this.numericUpDownQECost11.Text = ECost11; this.numericUpDownQMCost11.Text = Price11;
            this.numericUpDownQQuan12.Text = Quant12; this.textBoxQDescription12.Text = Descr12; this.numericUpDownQCost12.Text = Costs12; this.numericUpDownQECost12.Text = ECost12; this.numericUpDownQMCost12.Text = Price12;
            this.numericUpDownQQuan13.Text = Quant13; this.textBoxQDescription13.Text = Descr13; this.numericUpDownQCost13.Text = Costs13; this.numericUpDownQECost13.Text = ECost13; this.numericUpDownQMCost13.Text = Price13;
            this.numericUpDownQQuan14.Text = Quant14; this.textBoxQDescription14.Text = Descr14; this.numericUpDownQCost14.Text = Costs14; this.numericUpDownQECost14.Text = ECost14; this.numericUpDownQMCost14.Text = Price14;
            this.numericUpDownQQuan15.Text = Quant15; this.textBoxQDescription15.Text = Descr15; this.numericUpDownQCost15.Text = Costs15; this.numericUpDownQECost15.Text = ECost15; this.numericUpDownQMCost15.Text = Price15;
            this.numericUpDownQQuan16.Text = Quant16; this.textBoxQDescription16.Text = Descr16; this.numericUpDownQCost16.Text = Costs16; this.numericUpDownQECost16.Text = ECost16; this.numericUpDownQMCost16.Text = Price16;
            this.numericUpDownQQuan17.Text = Quant17; this.textBoxQDescription17.Text = Descr17; this.numericUpDownQCost17.Text = Costs17; this.numericUpDownQECost17.Text = ECost17; this.numericUpDownQMCost17.Text = Price17;
            this.numericUpDownQQuan18.Text = Quant18; this.textBoxQDescription18.Text = Descr18; this.numericUpDownQCost18.Text = Costs18; this.numericUpDownQECost18.Text = ECost18; this.numericUpDownQMCost18.Text = Price18;
            this.numericUpDownQQuan19.Text = Quant19; this.textBoxQDescription19.Text = Descr19; this.numericUpDownQCost19.Text = Costs19; this.numericUpDownQECost19.Text = ECost19; this.numericUpDownQMCost19.Text = Price19;
            this.numericUpDownQQuan20.Text = Quant20; this.textBoxQDescription20.Text = Descr20; this.numericUpDownQCost20.Text = Costs20; this.numericUpDownQECost20.Text = ECost20; this.numericUpDownQMCost20.Text = Price20;
            this.numericUpDownQQuan21.Text = Quant21; this.textBoxQDescription21.Text = Descr21; this.numericUpDownQCost21.Text = Costs21; this.numericUpDownQECost21.Text = ECost21; this.numericUpDownQMCost21.Text = Price21;
            this.numericUpDownQQuan22.Text = Quant22; this.textBoxQDescription22.Text = Descr22; this.numericUpDownQCost22.Text = Costs22; this.numericUpDownQECost22.Text = ECost22; this.numericUpDownQMCost22.Text = Price22;
            this.numericUpDownQQuan23.Text = Quant23; this.textBoxQDescription23.Text = Descr23; this.numericUpDownQCost23.Text = Costs23; this.numericUpDownQECost23.Text = ECost23; this.numericUpDownQMCost23.Text = Price23;

            this.richTextBoxQInvoicingNotes.Text = InvNotes;
            this.richTextBoxQDeliveryNotes.Text = DeliveryNotes;
            this.richTextBoxQQuoteNotes.Text = QuoteNotes;

            isDataLoadingQuotes = false;

            #endregion

            List<string> theListOfLocks = getLockListQuotes();
            if (theListOfLocks.Contains(this.textBoxQPO.Text))
            {
                lockGUIQuotes(true);
            }
            else
            {
                lockGUIQuotes(false);
            }

            autoSelectSignature();
        }

        #endregion

        #region Quotes update row

        private void updateRowQuotes()
        {
            string Date; string Status; string Company; string VendorName; string JobName; string CustomerPO;
            string VendorNumber; string SalesAss; string SoldTo; string Street1; string Street2; string City; string State; string Zip;
            string Delivery; string Terms; string FreightSelect; string IsQManual; string IsPManual; string Location; string Equipment; string EquipCategory;
            string QuotePrice; string Credit; string Freight; string ShopTime; string TotalCost; string GrossProfit; string Profit; string MarkUp;
            string Description;
            string Quant1; string Descr1; string Costs1; string ECost1; string Price1;
            string Quant2; string Descr2; string Costs2; string ECost2; string Price2;
            string Quant3; string Descr3; string Costs3; string ECost3; string Price3;
            string Quant4; string Descr4; string Costs4; string ECost4; string Price4;
            string Quant5; string Descr5; string Costs5; string ECost5; string Price5;
            string Quant6; string Descr6; string Costs6; string ECost6; string Price6;
            string Quant7; string Descr7; string Costs7; string ECost7; string Price7;
            string Quant8; string Descr8; string Costs8; string ECost8; string Price8;
            string Quant9; string Descr9; string Costs9; string ECost9; string Price9;
            string Quant10; string Descr10; string Costs10; string ECost10; string Price10;
            string Quant11; string Descr11; string Costs11; string ECost11; string Price11;
            string Quant12; string Descr12; string Costs12; string ECost12; string Price12;
            string Quant13; string Descr13; string Costs13; string ECost13; string Price13;
            string Quant14; string Descr14; string Costs14; string ECost14; string Price14;
            string Quant15; string Descr15; string Costs15; string ECost15; string Price15;
            string Quant16; string Descr16; string Costs16; string ECost16; string Price16;
            string Quant17; string Descr17; string Costs17; string ECost17; string Price17;
            string Quant18; string Descr18; string Costs18; string ECost18; string Price18;
            string Quant19; string Descr19; string Costs19; string ECost19; string Price19;
            string Quant20; string Descr20; string Costs20; string ECost20; string Price20;
            string Quant21; string Descr21; string Costs21; string ECost21; string Price21;
            string Quant22; string Descr22; string Costs22; string ECost22; string Price22;
            string Quant23; string Descr23; string Costs23; string ECost23; string Price23;
            string InvNotes; string DeliveryNotes; string QuoteNotes;

            Date = this.textBoxQDate.Text;
            Status = this.comboBoxQStatus.Text;
            Company = this.textBoxQCompany.Text;
            VendorName = this.textBoxQVendorName.Text;
            JobName = this.textBoxQJobName.Text;
            CustomerPO = this.textBoxQCustomerPO.Text;
            VendorNumber = this.textBoxQVendorNumber.Text;
            SalesAss = this.comboBoxQSalesAss.Text;
            SoldTo = this.textBoxQTo.Text;

            Street1 = this.textBoxQStreet1.Text;
            Street2 = this.textBoxQStreet2.Text;
            City = this.textBoxQCity.Text;
            State = this.comboBoxQState.Text;
            Zip = this.textBoxQZip.Text;

            Delivery = this.comboBoxQDelivery.Text;
            Terms = this.comboBoxQTerms.Text;
            FreightSelect = this.comboBoxFreightSelect.Text;
            IsQManual = (this.checkBoxQManual.Checked) ? "1" : "0";
            IsPManual = (this.checkBoxPManual.Checked) ? "1" : "0";
            Location = this.textBoxQLocation.Text;
            Equipment = this.textBoxQEquipment.Text;
            EquipCategory = this.comboBoxQEquipCategory.Text;
            
            QuotePrice = this.numericUpDownQQuotePrice.Text;
            Credit = this.numericUpDownQCredit.Text;
            Freight = this.numericUpDownQFreight.Text;
            ShopTime = this.numericUpDownQShopTime.Text;
            TotalCost = this.numericUpDownQTotalCost.Text;
            GrossProfit = this.numericUpDownQGrossProfit.Text;
            Profit = this.numericUpDownQProfit.Text;
            MarkUp = this.numericUpDownQMarkUp.Text;

            Description = this.textBoxQHeader.Text;
            Quant1 = this.numericUpDownQQuan1.Text; Descr1 = this.textBoxQDescription1.Text; Costs1 = this.numericUpDownQCost1.Text; ECost1 = this.numericUpDownQECost1.Text; Price1 = this.numericUpDownQMCost1.Text;
            Quant2 = this.numericUpDownQQuan2.Text; Descr2 = this.textBoxQDescription2.Text; Costs2 = this.numericUpDownQCost2.Text; ECost2 = this.numericUpDownQECost2.Text; Price2 = this.numericUpDownQMCost2.Text;
            Quant3 = this.numericUpDownQQuan3.Text; Descr3 = this.textBoxQDescription3.Text; Costs3 = this.numericUpDownQCost3.Text; ECost3 = this.numericUpDownQECost3.Text; Price3 = this.numericUpDownQMCost3.Text;
            Quant4 = this.numericUpDownQQuan4.Text; Descr4 = this.textBoxQDescription4.Text; Costs4 = this.numericUpDownQCost4.Text; ECost4 = this.numericUpDownQECost4.Text; Price4 = this.numericUpDownQMCost4.Text;
            Quant5 = this.numericUpDownQQuan5.Text; Descr5 = this.textBoxQDescription5.Text; Costs5 = this.numericUpDownQCost5.Text; ECost5 = this.numericUpDownQECost5.Text; Price5 = this.numericUpDownQMCost5.Text;
            Quant6 = this.numericUpDownQQuan6.Text; Descr6 = this.textBoxQDescription6.Text; Costs6 = this.numericUpDownQCost6.Text; ECost6 = this.numericUpDownQECost6.Text; Price6 = this.numericUpDownQMCost6.Text;
            Quant7 = this.numericUpDownQQuan7.Text; Descr7 = this.textBoxQDescription7.Text; Costs7 = this.numericUpDownQCost7.Text; ECost7 = this.numericUpDownQECost7.Text; Price7 = this.numericUpDownQMCost7.Text;
            Quant8 = this.numericUpDownQQuan8.Text; Descr8 = this.textBoxQDescription8.Text; Costs8 = this.numericUpDownQCost8.Text; ECost8 = this.numericUpDownQECost8.Text; Price8 = this.numericUpDownQMCost8.Text;
            Quant9 = this.numericUpDownQQuan9.Text; Descr9 = this.textBoxQDescription9.Text; Costs9 = this.numericUpDownQCost9.Text; ECost9 = this.numericUpDownQECost9.Text; Price9 = this.numericUpDownQMCost9.Text;
            Quant10 = this.numericUpDownQQuan10.Text; Descr10 = this.textBoxQDescription10.Text; Costs10 = this.numericUpDownQCost10.Text; ECost10 = this.numericUpDownQECost10.Text; Price10 = this.numericUpDownQMCost10.Text;
            Quant11 = this.numericUpDownQQuan11.Text; Descr11 = this.textBoxQDescription11.Text; Costs11 = this.numericUpDownQCost11.Text; ECost11 = this.numericUpDownQECost11.Text; Price11 = this.numericUpDownQMCost11.Text;
            Quant12 = this.numericUpDownQQuan12.Text; Descr12 = this.textBoxQDescription12.Text; Costs12 = this.numericUpDownQCost12.Text; ECost12 = this.numericUpDownQECost12.Text; Price12 = this.numericUpDownQMCost12.Text;
            Quant13 = this.numericUpDownQQuan13.Text; Descr13 = this.textBoxQDescription13.Text; Costs13 = this.numericUpDownQCost13.Text; ECost13 = this.numericUpDownQECost13.Text; Price13 = this.numericUpDownQMCost13.Text;
            Quant14 = this.numericUpDownQQuan14.Text; Descr14 = this.textBoxQDescription14.Text; Costs14 = this.numericUpDownQCost14.Text; ECost14 = this.numericUpDownQECost14.Text; Price14 = this.numericUpDownQMCost14.Text;
            Quant15 = this.numericUpDownQQuan15.Text; Descr15 = this.textBoxQDescription15.Text; Costs15 = this.numericUpDownQCost15.Text; ECost15 = this.numericUpDownQECost15.Text; Price15 = this.numericUpDownQMCost15.Text;
            Quant16 = this.numericUpDownQQuan16.Text; Descr16 = this.textBoxQDescription16.Text; Costs16 = this.numericUpDownQCost16.Text; ECost16 = this.numericUpDownQECost16.Text; Price16 = this.numericUpDownQMCost16.Text;
            Quant17 = this.numericUpDownQQuan17.Text; Descr17 = this.textBoxQDescription17.Text; Costs17 = this.numericUpDownQCost17.Text; ECost17 = this.numericUpDownQECost17.Text; Price17 = this.numericUpDownQMCost17.Text;
            Quant18 = this.numericUpDownQQuan18.Text; Descr18 = this.textBoxQDescription18.Text; Costs18 = this.numericUpDownQCost18.Text; ECost18 = this.numericUpDownQECost18.Text; Price18 = this.numericUpDownQMCost18.Text;
            Quant19 = this.numericUpDownQQuan19.Text; Descr19 = this.textBoxQDescription19.Text; Costs19 = this.numericUpDownQCost19.Text; ECost19 = this.numericUpDownQECost19.Text; Price19 = this.numericUpDownQMCost19.Text;
            Quant20 = this.numericUpDownQQuan20.Text; Descr20 = this.textBoxQDescription20.Text; Costs20 = this.numericUpDownQCost20.Text; ECost20 = this.numericUpDownQECost20.Text; Price20 = this.numericUpDownQMCost20.Text;
            Quant21 = this.numericUpDownQQuan21.Text; Descr21 = this.textBoxQDescription21.Text; Costs21 = this.numericUpDownQCost21.Text; ECost21 = this.numericUpDownQECost21.Text; Price21 = this.numericUpDownQMCost21.Text;
            Quant22 = this.numericUpDownQQuan22.Text; Descr22 = this.textBoxQDescription22.Text; Costs22 = this.numericUpDownQCost22.Text; ECost22 = this.numericUpDownQECost22.Text; Price22 = this.numericUpDownQMCost22.Text;
            Quant23 = this.numericUpDownQQuan23.Text; Descr23 = this.textBoxQDescription23.Text; Costs23 = this.numericUpDownQCost23.Text; ECost23 = this.numericUpDownQECost23.Text; Price23 = this.numericUpDownQMCost23.Text;

            InvNotes = this.richTextBoxQInvoicingNotes.Text;
            DeliveryNotes = this.richTextBoxQDeliveryNotes.Text;
            QuoteNotes = this.richTextBoxQQuoteNotes.Text;

            int rowsWritten = updateRowQuotes(this.textBoxQPO.Text, Date, Status, Company, VendorName, JobName, CustomerPO,
             VendorNumber, SalesAss, SoldTo, Street1, Street2, City, State, Zip,
             Delivery, Terms, FreightSelect, IsQManual, IsPManual, Location, Equipment, EquipCategory,
             QuotePrice, Credit, Freight, ShopTime, TotalCost, GrossProfit, Profit, MarkUp,
             Description,
             Quant1, Descr1, Costs1, ECost1, Price1,
             Quant2, Descr2, Costs2, ECost2, Price2,
             Quant3, Descr3, Costs3, ECost3, Price3,
             Quant4, Descr4, Costs4, ECost4, Price4,
             Quant5, Descr5, Costs5, ECost5, Price5,
             Quant6, Descr6, Costs6, ECost6, Price6,
             Quant7, Descr7, Costs7, ECost7, Price7,
             Quant8, Descr8, Costs8, ECost8, Price8,
             Quant9, Descr9, Costs9, ECost9, Price9,
             Quant10, Descr10, Costs10, ECost10, Price10,
             Quant11, Descr11, Costs11, ECost11, Price11,
             Quant12, Descr12, Costs12, ECost12, Price12,
             Quant13, Descr13, Costs13, ECost13, Price13,
             Quant14, Descr14, Costs14, ECost14, Price14,
             Quant15, Descr15, Costs15, ECost15, Price15,
             Quant16, Descr16, Costs16, ECost16, Price16,
             Quant17, Descr17, Costs17, ECost17, Price17,
             Quant18, Descr18, Costs18, ECost18, Price18,
             Quant19, Descr19, Costs19, ECost19, Price19,
             Quant20, Descr20, Costs20, ECost20, Price20,
             Quant21, Descr21, Costs21, ECost21, Price21,
             Quant22, Descr22, Costs22, ECost22, Price22,
             Quant23, Descr23, Costs23, ECost23, Price23,
             InvNotes, DeliveryNotes, QuoteNotes);

            this.isDataDirtyQuotes = false;
            this.toolStripStatusLabel2.Text = "Status Quote: Clean";

            // remove the lock
            removeLockQuote();

            //MessageBox.Show(rowsWritten.ToString());
        }

        #endregion

        #region Quotes insert row

        private void insertNewDataRowQuotes(string QPO)
        {
            string Date; string Status; string Company; string VendorName; string JobName; string CustomerPO;
            string VendorNumber; string SalesAss; string SoldTo; string Street1; string Street2; string City; string State; string Zip;
            string Delivery; string Terms; string FreightSelect; string IsQManual; string IsPManual; string Location; string Equipment; string EquipCategory;
            string QuotePrice; string Credit; string Freight; string ShopTime; string TotalCost; string GrossProfit; string Profit; string MarkUp;
            string Description;
            string Quant1; string Descr1; string Costs1; string ECost1; string Price1;
            string Quant2; string Descr2; string Costs2; string ECost2; string Price2;
            string Quant3; string Descr3; string Costs3; string ECost3; string Price3;
            string Quant4; string Descr4; string Costs4; string ECost4; string Price4;
            string Quant5; string Descr5; string Costs5; string ECost5; string Price5;
            string Quant6; string Descr6; string Costs6; string ECost6; string Price6;
            string Quant7; string Descr7; string Costs7; string ECost7; string Price7;
            string Quant8; string Descr8; string Costs8; string ECost8; string Price8;
            string Quant9; string Descr9; string Costs9; string ECost9; string Price9;
            string Quant10; string Descr10; string Costs10; string ECost10; string Price10;
            string Quant11; string Descr11; string Costs11; string ECost11; string Price11;
            string Quant12; string Descr12; string Costs12; string ECost12; string Price12;
            string Quant13; string Descr13; string Costs13; string ECost13; string Price13;
            string Quant14; string Descr14; string Costs14; string ECost14; string Price14;
            string Quant15; string Descr15; string Costs15; string ECost15; string Price15;
            string Quant16; string Descr16; string Costs16; string ECost16; string Price16;
            string Quant17; string Descr17; string Costs17; string ECost17; string Price17;
            string Quant18; string Descr18; string Costs18; string ECost18; string Price18;
            string Quant19; string Descr19; string Costs19; string ECost19; string Price19;
            string Quant20; string Descr20; string Costs20; string ECost20; string Price20;
            string Quant21; string Descr21; string Costs21; string ECost21; string Price21;
            string Quant22; string Descr22; string Costs22; string ECost22; string Price22;
            string Quant23; string Descr23; string Costs23; string ECost23; string Price23;
            string InvNotes; string DeliveryNotes; string QuoteNotes;

            Date = this.textBoxQDate.Text;
            Status = this.comboBoxQStatus.Text;
            Company = this.textBoxQCompany.Text;
            VendorName = this.textBoxQVendorName.Text;
            JobName = this.textBoxQJobName.Text;
            CustomerPO = this.textBoxQCustomerPO.Text;
            VendorNumber = this.textBoxQVendorNumber.Text;
            SalesAss = this.comboBoxQSalesAss.Text;
            SoldTo = this.textBoxQTo.Text;

            Street1 = this.textBoxQStreet1.Text;
            Street2 = this.textBoxQStreet2.Text;
            City = this.textBoxQCity.Text;
            State = this.comboBoxQState.Text;
            Zip = this.textBoxQZip.Text;

            Delivery = this.comboBoxQDelivery.Text;
            Terms = this.comboBoxQTerms.Text;
            FreightSelect = this.comboBoxFreightSelect.Text;
            IsQManual = (this.checkBoxQManual.Checked) ? "1" : "0";
            IsPManual = (this.checkBoxPManual.Checked) ? "1" : "0";
            Location = this.textBoxQLocation.Text;
            Equipment = this.textBoxQEquipment.Text;
            EquipCategory = this.comboBoxQEquipCategory.Text;

            QuotePrice = this.numericUpDownQQuotePrice.Text;
            Credit = this.numericUpDownQCredit.Text;
            Freight = this.numericUpDownQFreight.Text;
            ShopTime = this.numericUpDownQShopTime.Text;
            TotalCost = this.numericUpDownQTotalCost.Text;
            GrossProfit = this.numericUpDownQGrossProfit.Text;
            Profit = this.numericUpDownQProfit.Text;
            MarkUp = this.numericUpDownQMarkUp.Text;

            Description = this.textBoxQHeader.Text;
            Quant1 = this.numericUpDownQQuan1.Text; Descr1 = this.textBoxQDescription1.Text; Costs1 = this.numericUpDownQCost1.Text; ECost1 = this.numericUpDownQECost1.Text; Price1 = this.numericUpDownQMCost1.Text;
            Quant2 = this.numericUpDownQQuan2.Text; Descr2 = this.textBoxQDescription2.Text; Costs2 = this.numericUpDownQCost2.Text; ECost2 = this.numericUpDownQECost2.Text; Price2 = this.numericUpDownQMCost2.Text;
            Quant3 = this.numericUpDownQQuan3.Text; Descr3 = this.textBoxQDescription3.Text; Costs3 = this.numericUpDownQCost3.Text; ECost3 = this.numericUpDownQECost3.Text; Price3 = this.numericUpDownQMCost3.Text;
            Quant4 = this.numericUpDownQQuan4.Text; Descr4 = this.textBoxQDescription4.Text; Costs4 = this.numericUpDownQCost4.Text; ECost4 = this.numericUpDownQECost4.Text; Price4 = this.numericUpDownQMCost4.Text;
            Quant5 = this.numericUpDownQQuan5.Text; Descr5 = this.textBoxQDescription5.Text; Costs5 = this.numericUpDownQCost5.Text; ECost5 = this.numericUpDownQECost5.Text; Price5 = this.numericUpDownQMCost5.Text;
            Quant6 = this.numericUpDownQQuan6.Text; Descr6 = this.textBoxQDescription6.Text; Costs6 = this.numericUpDownQCost6.Text; ECost6 = this.numericUpDownQECost6.Text; Price6 = this.numericUpDownQMCost6.Text;
            Quant7 = this.numericUpDownQQuan7.Text; Descr7 = this.textBoxQDescription7.Text; Costs7 = this.numericUpDownQCost7.Text; ECost7 = this.numericUpDownQECost7.Text; Price7 = this.numericUpDownQMCost7.Text;
            Quant8 = this.numericUpDownQQuan8.Text; Descr8 = this.textBoxQDescription8.Text; Costs8 = this.numericUpDownQCost8.Text; ECost8 = this.numericUpDownQECost8.Text; Price8 = this.numericUpDownQMCost8.Text;
            Quant9 = this.numericUpDownQQuan9.Text; Descr9 = this.textBoxQDescription9.Text; Costs9 = this.numericUpDownQCost9.Text; ECost9 = this.numericUpDownQECost9.Text; Price9 = this.numericUpDownQMCost9.Text;
            Quant10 = this.numericUpDownQQuan10.Text; Descr10 = this.textBoxQDescription10.Text; Costs10 = this.numericUpDownQCost10.Text; ECost10 = this.numericUpDownQECost10.Text; Price10 = this.numericUpDownQMCost10.Text;
            Quant11 = this.numericUpDownQQuan11.Text; Descr11 = this.textBoxQDescription11.Text; Costs11 = this.numericUpDownQCost11.Text; ECost11 = this.numericUpDownQECost11.Text; Price11 = this.numericUpDownQMCost11.Text;
            Quant12 = this.numericUpDownQQuan12.Text; Descr12 = this.textBoxQDescription12.Text; Costs12 = this.numericUpDownQCost12.Text; ECost12 = this.numericUpDownQECost12.Text; Price12 = this.numericUpDownQMCost12.Text;
            Quant13 = this.numericUpDownQQuan13.Text; Descr13 = this.textBoxQDescription13.Text; Costs13 = this.numericUpDownQCost13.Text; ECost13 = this.numericUpDownQECost13.Text; Price13 = this.numericUpDownQMCost13.Text;
            Quant14 = this.numericUpDownQQuan14.Text; Descr14 = this.textBoxQDescription14.Text; Costs14 = this.numericUpDownQCost14.Text; ECost14 = this.numericUpDownQECost14.Text; Price14 = this.numericUpDownQMCost14.Text;
            Quant15 = this.numericUpDownQQuan15.Text; Descr15 = this.textBoxQDescription15.Text; Costs15 = this.numericUpDownQCost15.Text; ECost15 = this.numericUpDownQECost15.Text; Price15 = this.numericUpDownQMCost15.Text;
            Quant16 = this.numericUpDownQQuan16.Text; Descr16 = this.textBoxQDescription16.Text; Costs16 = this.numericUpDownQCost16.Text; ECost16 = this.numericUpDownQECost16.Text; Price16 = this.numericUpDownQMCost16.Text;
            Quant17 = this.numericUpDownQQuan17.Text; Descr17 = this.textBoxQDescription17.Text; Costs17 = this.numericUpDownQCost17.Text; ECost17 = this.numericUpDownQECost17.Text; Price17 = this.numericUpDownQMCost17.Text;
            Quant18 = this.numericUpDownQQuan18.Text; Descr18 = this.textBoxQDescription18.Text; Costs18 = this.numericUpDownQCost18.Text; ECost18 = this.numericUpDownQECost18.Text; Price18 = this.numericUpDownQMCost18.Text;
            Quant19 = this.numericUpDownQQuan19.Text; Descr19 = this.textBoxQDescription19.Text; Costs19 = this.numericUpDownQCost19.Text; ECost19 = this.numericUpDownQECost19.Text; Price19 = this.numericUpDownQMCost19.Text;
            Quant20 = this.numericUpDownQQuan20.Text; Descr20 = this.textBoxQDescription20.Text; Costs20 = this.numericUpDownQCost20.Text; ECost20 = this.numericUpDownQECost20.Text; Price20 = this.numericUpDownQMCost20.Text;
            Quant21 = this.numericUpDownQQuan21.Text; Descr21 = this.textBoxQDescription21.Text; Costs21 = this.numericUpDownQCost21.Text; ECost21 = this.numericUpDownQECost21.Text; Price21 = this.numericUpDownQMCost21.Text;
            Quant22 = this.numericUpDownQQuan22.Text; Descr22 = this.textBoxQDescription22.Text; Costs22 = this.numericUpDownQCost22.Text; ECost22 = this.numericUpDownQECost22.Text; Price22 = this.numericUpDownQMCost22.Text;
            Quant23 = this.numericUpDownQQuan23.Text; Descr23 = this.textBoxQDescription23.Text; Costs23 = this.numericUpDownQCost23.Text; ECost23 = this.numericUpDownQECost23.Text; Price23 = this.numericUpDownQMCost23.Text;

            InvNotes = this.richTextBoxQInvoicingNotes.Text;
            DeliveryNotes = this.richTextBoxQDeliveryNotes.Text;
            QuoteNotes = this.richTextBoxQQuoteNotes.Text;

            int rowsWritten = insertRowQuotes(QPO, Date, Status, Company, VendorName, JobName, CustomerPO,
             VendorNumber, SalesAss, SoldTo, Street1, Street2, City, State, Zip,
             Delivery, Terms, FreightSelect, IsQManual, IsPManual, Location, Equipment, EquipCategory,
             QuotePrice, Credit, Freight, ShopTime, TotalCost, GrossProfit, Profit, MarkUp,
             Description,
             Quant1, Descr1, Costs1, ECost1, Price1,
             Quant2, Descr2, Costs2, ECost2, Price2,
             Quant3, Descr3, Costs3, ECost3, Price3,
             Quant4, Descr4, Costs4, ECost4, Price4,
             Quant5, Descr5, Costs5, ECost5, Price5,
             Quant6, Descr6, Costs6, ECost6, Price6,
             Quant7, Descr7, Costs7, ECost7, Price7,
             Quant8, Descr8, Costs8, ECost8, Price8,
             Quant9, Descr9, Costs9, ECost9, Price9,
             Quant10, Descr10, Costs10, ECost10, Price10,
             Quant11, Descr11, Costs11, ECost11, Price11,
             Quant12, Descr12, Costs12, ECost12, Price12,
             Quant13, Descr13, Costs13, ECost13, Price13,
             Quant14, Descr14, Costs14, ECost14, Price14,
             Quant15, Descr15, Costs15, ECost15, Price15,
             Quant16, Descr16, Costs16, ECost16, Price16,
             Quant17, Descr17, Costs17, ECost17, Price17,
             Quant18, Descr18, Costs18, ECost18, Price18,
             Quant19, Descr19, Costs19, ECost19, Price19,
             Quant20, Descr20, Costs20, ECost20, Price20,
             Quant21, Descr21, Costs21, ECost21, Price21,
             Quant22, Descr22, Costs22, ECost22, Price22,
             Quant23, Descr23, Costs23, ECost23, Price23,
             InvNotes, DeliveryNotes, QuoteNotes);

            isDataDirtyQuotes = false;
            this.toolStripStatusLabel2.Text = "Status Quote: Clean";

            //MessageBox.Show(rowsWritten.ToString());
        }

        #endregion


        #region Database Locking

        private void generateLockName()
        {
            this._lockName = RandomString(8);
        }

        #region place & remove locks

        private void placeLockOrder()
        {
            try
            {
                removeLockOrder();
                createLock(this.toolStripTextBoxDbasePath.Text + "!_O" + this._lockName + ".tmp", this.textBoxPO.Text);
            }
            catch { }
        }

        private void placeLockQuote()
        {
            try
            {
                removeLockQuote();
                createLock(this.toolStripTextBoxDbasePath.Text + "!_Q" + this._lockName + ".tmp", this.textBoxQPO.Text);
            }
            catch { }
        }

        private void removeLockOrder()
        {
            try
            {
                if (File.Exists(this.toolStripTextBoxDbasePath.Text + "!_O" + this._lockName + ".tmp"))
                    File.Delete(this.toolStripTextBoxDbasePath.Text + "!_O" + this._lockName + ".tmp");
            }
            catch { }
        }

        private void removeLockQuote()
        {
            try
            {
                if (File.Exists(this.toolStripTextBoxDbasePath.Text + "!_Q" + this._lockName + ".tmp"))
                    File.Delete(this.toolStripTextBoxDbasePath.Text + "!_Q" + this._lockName + ".tmp");
            }
            catch { }
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
            catch { }

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
            catch { }

            return list;
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
                catch { }
            }

            return __con;
        }

        private void Killconnection()
        {
            if (__con != null)
            {
                __con.Close();
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

        #endregion  
    }
}