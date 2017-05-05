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

namespace Tugwell
{
    public partial class FormMain : Form
    {
        #region Create New Dbase

        private void createNewDbase()
        {
            Sql.CreateNewDbase();
        }

        #endregion


        #region Orders read & Update GUI

        private bool readOrderAndUpdateGUI(string PO, int row)
        {
            _log.append("** " + PO + " row " + row);
            _log.append("readOrderAndUpdateGUI start");

            List<string> guis;
            Sql.ReadRow(dbType.Order, PO, row, out guis);

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
            Sql.UpdateRow(dbType.Order, guis);

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
            Sql.InsertRow(dbType.Order, guis);

            isDataDirtyOrders = false;
        }

        #endregion


        #region Quote read & Update GUI

        private bool readQuoteAndUpdateGUI(string QPO, int row)
        {
            List<string> guis;
            Sql.ReadRow(dbType.Quote, QPO, row, out guis);

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
            Sql.UpdateRow(dbType.Quote, guis);

            this.isDataDirtyQuotes = false;
            this.toolStripStatusLabel2.Text = "Status Quotes: Clean";

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
            Sql.InsertRow(dbType.Quote, guis);

            isDataDirtyQuotes = false;
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

        #region General Helpers

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
            List<string> posUnsorted = Sql.GetPOs(dbType.Order);
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
    }
}
