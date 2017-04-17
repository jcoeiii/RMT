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
        private void printPurchaseOrder()
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

        private void printPriceSheet()
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

        private void printDeliveryTicket()
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

            p.AddText(tiNotes, "I have examined all equipment including pump electrical cables and all is in good condition.\n\n", 40, 590, p.Width, 18);
            
            // added: 1/23/2017
            Print2Pdf.TextInfo tiNotes2 = p.CreateTextInfo();
            tiNotes2.Size = 12.0;
            tiNotes2.Textstyle = Print2Pdf.TextStyle.Regular;
            p.AddText(tiNotes2, "R. M. Tugwell & Associates, Inc. assumes no liability whatsoever for delays or damages caused by\ndefects or any other equipment failure.", 40, 555, p.Width, 18);

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
            p.AddText(tiFooter, todaysDateTime(), 60, pos + 88, p.Width, 18);
            p.AddText(tiFooter, "\nPage 1 of 1");

            string pdf = generatePDFileName("Delv", this.textBoxPO.Text);
            p.Save(pdf);
            p.ShowPDF(pdf);

            #endregion
        }

        private void printCurrentQuote()
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

            Print2Pdf.TextInfo table1 = p.AddTable(pos + 100, rowCount, 12.0, (showPrices) ? new List<double> { 10, 80, 10 } : new List<double> { 10, 90 }, 40);
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
    }
}
