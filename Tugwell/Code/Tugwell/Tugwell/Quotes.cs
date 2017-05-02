using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tugwell
{
    public class Quote
    {
        static List<string> gui_objects = new List<string>(new string[]
        {
            #region GUI Names
            /*QPO*/ "textBoxQPO",
            /*Date*/ "textBoxQDate",
            /*Status*/ "comboBoxQStatus",
            /*Company*/ "textBoxQCompany",
            /*VendorName*/ "textBoxQVendorName",
            /*JobName*/ "textBoxQJobName",
            /*CustomerPO*/ "textBoxQCustomerPO",
            /*VendorNumber*/ "textBoxQVendorNumber",
            /*SalesAss*/ "comboBoxQSalesAss",
            /*SoldTo*/ "textBoxQTo",

            /*Street1*/ "textBoxQStreet1",
            /*Street2*/ "textBoxQStreet2",
            /*City*/ "textBoxQCity",
            /*State*/ "comboBoxQState",
            /*Zip*/ "textBoxQZip",

            /*Delivery*/ "comboBoxQDelivery",
            /*Terms*/ "comboBoxQTerms",
            /*FreightSelect*/ "comboBoxFreightSelect",
            /*IsQManual*/ "checkBoxQManual",
            /*IsPManual*/ "checkBoxPManual",
            /*Location*/ "textBoxQLocation",
            /*Equipment*/ "textBoxQEquipment",
            /*EquipCategory*/ "comboBoxQEquipCategory",

            /*QuotePrice*/ "numericUpDownQQuotePrice",
            /*Credit*/ "numericUpDownQCredit",
            /*Freight*/ "numericUpDownQFreight",
            /*ShopTime*/ "numericUpDownQShopTime",
            /*TotalCost*/ "numericUpDownQTotalCost",
            /*GrossProfit*/ "numericUpDownQGrossProfit",
            /*Profit*/ "numericUpDownQProfit",
            /*MarkUp*/ "numericUpDownQMarkUp",

            /*Description*/ "textBoxQHeader",
            /*Quant1*/ "numericUpDownQQuan1", /*Descr1*/ "textBoxQDescription1", /*Costs1*/ "numericUpDownQCost1", /*ECost1*/ "numericUpDownQECost1", /*Price1*/ "numericUpDownQMCost1",
            /*Quant2*/ "numericUpDownQQuan2", /*Descr2*/ "textBoxQDescription2", /*Costs2*/ "numericUpDownQCost2", /*ECost2*/ "numericUpDownQECost2", /*Price2*/ "numericUpDownQMCost2",
            /*Quant3*/ "numericUpDownQQuan3", /*Descr3*/ "textBoxQDescription3", /*Costs3*/ "numericUpDownQCost3", /*ECost3*/ "numericUpDownQECost3", /*Price3*/ "numericUpDownQMCost3",
            /*Quant4*/ "numericUpDownQQuan4", /*Descr4*/ "textBoxQDescription4", /*Costs4*/ "numericUpDownQCost4", /*ECost4*/ "numericUpDownQECost4", /*Price4*/ "numericUpDownQMCost4",
            /*Quant5*/ "numericUpDownQQuan5", /*Descr5*/ "textBoxQDescription5", /*Costs5*/ "numericUpDownQCost5", /*ECost5*/ "numericUpDownQECost5", /*Price5*/ "numericUpDownQMCost5",
            /*Quant6*/ "numericUpDownQQuan6", /*Descr6*/ "textBoxQDescription6", /*Costs6*/ "numericUpDownQCost6", /*ECost6*/ "numericUpDownQECost6", /*Price6*/ "numericUpDownQMCost6",
            /*Quant7*/ "numericUpDownQQuan7", /*Descr7*/ "textBoxQDescription7", /*Costs7*/ "numericUpDownQCost7", /*ECost7*/ "numericUpDownQECost7", /*Price7*/ "numericUpDownQMCost7",
            /*Quant8*/ "numericUpDownQQuan8", /*Descr8*/ "textBoxQDescription8", /*Costs8*/ "numericUpDownQCost8", /*ECost8*/ "numericUpDownQECost8", /*Price8*/ "numericUpDownQMCost8",
            /*Quant9*/ "numericUpDownQQuan9", /*Descr9*/ "textBoxQDescription9", /*Costs9*/ "numericUpDownQCost9", /*ECost9*/ "numericUpDownQECost9", /*Price9*/ "numericUpDownQMCost9",
            /*Quant10*/ "numericUpDownQQuan10", /*Descr10*/ "textBoxQDescription10", /*Costs10*/ "numericUpDownQCost10", /*ECost10*/ "numericUpDownQECost10", /*Price10*/ "numericUpDownQMCost10",
            /*Quant11*/ "numericUpDownQQuan11", /*Descr11*/ "textBoxQDescription11", /*Costs11*/ "numericUpDownQCost11", /*ECost11*/ "numericUpDownQECost11", /*Price11*/ "numericUpDownQMCost11",
            /*Quant12*/ "numericUpDownQQuan12", /*Descr12*/ "textBoxQDescription12", /*Costs12*/ "numericUpDownQCost12", /*ECost12*/ "numericUpDownQECost12", /*Price12*/ "numericUpDownQMCost12",
            /*Quant13*/ "numericUpDownQQuan13", /*Descr13*/ "textBoxQDescription13", /*Costs13*/ "numericUpDownQCost13", /*ECost13*/ "numericUpDownQECost13", /*Price13*/ "numericUpDownQMCost13",
            /*Quant14*/ "numericUpDownQQuan14", /*Descr14*/ "textBoxQDescription14", /*Costs14*/ "numericUpDownQCost14", /*ECost14*/ "numericUpDownQECost14", /*Price14*/ "numericUpDownQMCost14",
            /*Quant15*/ "numericUpDownQQuan15", /*Descr15*/ "textBoxQDescription15", /*Costs15*/ "numericUpDownQCost15", /*ECost15*/ "numericUpDownQECost15", /*Price15*/ "numericUpDownQMCost15",
            /*Quant16*/ "numericUpDownQQuan16", /*Descr16*/ "textBoxQDescription16", /*Costs16*/ "numericUpDownQCost16", /*ECost16*/ "numericUpDownQECost16", /*Price16*/ "numericUpDownQMCost16",
            /*Quant17*/ "numericUpDownQQuan17", /*Descr17*/ "textBoxQDescription17", /*Costs17*/ "numericUpDownQCost17", /*ECost17*/ "numericUpDownQECost17", /*Price17*/ "numericUpDownQMCost17",
            /*Quant18*/ "numericUpDownQQuan18", /*Descr18*/ "textBoxQDescription18", /*Costs18*/ "numericUpDownQCost18", /*ECost18*/ "numericUpDownQECost18", /*Price18*/ "numericUpDownQMCost18",
            /*Quant19*/ "numericUpDownQQuan19", /*Descr19*/ "textBoxQDescription19", /*Costs19*/ "numericUpDownQCost19", /*ECost19*/ "numericUpDownQECost19", /*Price19*/ "numericUpDownQMCost19",
            /*Quant20*/ "numericUpDownQQuan20", /*Descr20*/ "textBoxQDescription20", /*Costs20*/ "numericUpDownQCost20", /*ECost20*/ "numericUpDownQECost20", /*Price20*/ "numericUpDownQMCost20",
            /*Quant21*/ "numericUpDownQQuan21", /*Descr21*/ "textBoxQDescription21", /*Costs21*/ "numericUpDownQCost21", /*ECost21*/ "numericUpDownQECost21", /*Price21*/ "numericUpDownQMCost21",
            /*Quant22*/ "numericUpDownQQuan22", /*Descr22*/ "textBoxQDescription22", /*Costs22*/ "numericUpDownQCost22", /*ECost22*/ "numericUpDownQECost22", /*Price22*/ "numericUpDownQMCost22",
            /*Quant23*/ "numericUpDownQQuan23", /*Descr23*/ "textBoxQDescription23", /*Costs23*/ "numericUpDownQCost23", /*ECost23*/ "numericUpDownQECost23", /*Price23*/ "numericUpDownQMCost23",

            /*InvNotes*/ "richTextBoxQInvoicingNotes",
            /*DeliveryNotes*/ "richTextBoxQDeliveryNotes",
            /*QuoteNotes*/ "richTextBoxQQuoteNotes"
            #endregion
        });

        static List<string> gui_defaults = new List<string>(new string[]
        {
            #region GUI Defaults
            /*QPO*/ "",
            /*Date*/ "",
            /*Status*/ "",
            /*Company*/ "",
            /*VendorName*/ "",
            /*JobName*/ "",
            /*CustomerPO*/ "",
            /*VendorNumber*/ "",
            /*SalesAss*/ "",
            /*SoldTo*/ "",

            /*Street1*/ "",
            /*Street2*/ "",
            /*City*/ "",
            /*State*/ "",
            /*Zip*/ "",

            /*Delivery*/ "",
            /*Terms*/ "",
            /*FreightSelect*/ "Net price, freight included",
            /*IsQManual*/ "",
            /*IsPManual*/ "1", // for prices
            /*Location*/ "",
            /*Equipment*/ "",
            /*EquipCategory*/ "",

            /*QuotePrice*/ "0.00",
            /*Credit*/ "0.00",
            /*Freight*/ "0.00",
            /*ShopTime*/ "0.00",
            /*TotalCost*/ "0.00",
            /*GrossProfit*/ "0.00",
            /*Profit*/ "0.00",
            /*MarkUp*/ "0.00",

            /*Description*/ "",
            /*Quant1*/ "0.00", /*Descr1*/ "", /*Costs1*/ "0.00", /*ECost1*/ "0.00", /*Price1*/ "0.00",
            /*Quant2*/ "0.00", /*Descr2*/ "", /*Costs2*/ "0.00", /*ECost2*/ "0.00", /*Price2*/ "0.00",
            /*Quant3*/ "0.00", /*Descr3*/ "", /*Costs3*/ "0.00", /*ECost3*/ "0.00", /*Price3*/ "0.00",
            /*Quant4*/ "0.00", /*Descr4*/ "", /*Costs4*/ "0.00", /*ECost4*/ "0.00", /*Price4*/ "0.00",
            /*Quant5*/ "0.00", /*Descr5*/ "", /*Costs5*/ "0.00", /*ECost5*/ "0.00", /*Price5*/ "0.00",
            /*Quant6*/ "0.00", /*Descr6*/ "", /*Costs6*/ "0.00", /*ECost6*/ "0.00", /*Price6*/ "0.00",
            /*Quant7*/ "0.00", /*Descr7*/ "", /*Costs7*/ "0.00", /*ECost7*/ "0.00", /*Price7*/ "0.00",
            /*Quant8*/ "0.00", /*Descr8*/ "", /*Costs8*/ "0.00", /*ECost8*/ "0.00", /*Price8*/ "0.00",
            /*Quant9*/ "0.00", /*Descr9*/ "", /*Costs9*/ "0.00", /*ECost9*/ "0.00", /*Price9*/ "0.00",
            /*Quant10*/ "0.00", /*Descr10*/ "", /*Costs10*/ "0.00", /*ECost10*/ "0.00", /*Price10*/ "0.00",
            /*Quant11*/ "0.00", /*Descr11*/ "", /*Costs11*/ "0.00", /*ECost11*/ "0.00", /*Price11*/ "0.00",
            /*Quant12*/ "0.00", /*Descr12*/ "", /*Costs12*/ "0.00", /*ECost12*/ "0.00", /*Price12*/ "0.00",
            /*Quant13*/ "0.00", /*Descr13*/ "", /*Costs13*/ "0.00", /*ECost13*/ "0.00", /*Price13*/ "0.00",
            /*Quant14*/ "0.00", /*Descr14*/ "", /*Costs14*/ "0.00", /*ECost14*/ "0.00", /*Price14*/ "0.00",
            /*Quant15*/ "0.00", /*Descr15*/ "", /*Costs15*/ "0.00", /*ECost15*/ "0.00", /*Price15*/ "0.00",
            /*Quant16*/ "0.00", /*Descr16*/ "", /*Costs16*/ "0.00", /*ECost16*/ "0.00", /*Price16*/ "0.00",
            /*Quant17*/ "0.00", /*Descr17*/ "", /*Costs17*/ "0.00", /*ECost17*/ "0.00", /*Price17*/ "0.00",
            /*Quant18*/ "0.00", /*Descr18*/ "", /*Costs18*/ "0.00", /*ECost18*/ "0.00", /*Price18*/ "0.00",
            /*Quant19*/ "0.00", /*Descr19*/ "", /*Costs19*/ "0.00", /*ECost19*/ "0.00", /*Price19*/ "0.00",
            /*Quant20*/ "0.00", /*Descr20*/ "", /*Costs20*/ "0.00", /*ECost20*/ "0.00", /*Price20*/ "0.00",
            /*Quant21*/ "0.00", /*Descr21*/ "", /*Costs21*/ "0.00", /*ECost21*/ "0.00", /*Price21*/ "0.00",
            /*Quant22*/ "0.00", /*Descr22*/ "", /*Costs22*/ "0.00", /*ECost22*/ "0.00", /*Price22*/ "0.00",
            /*Quant23*/ "0.00", /*Descr23*/ "", /*Costs23*/ "0.00", /*ECost23*/ "0.00", /*Price23*/ "0.00",

            /*InvNotes*/ "",
            /*DeliveryNotes*/ "",
            /*QuoteNotes*/ ""
            #endregion
        });

        static List<string> table_names = new List<string>(new string[]
        {
            #region Table Names
            /*theQPO*/ "PO",

            /*Date*/ "Date",
            /*Status*/ "Status",

            /*Company*/ "Company",
            /*VendorName*/ "VendorName",
            /*JobName*/ "JobName",
            /*CustomerPO*/ "CustomerPO",
            /*VendorNumber*/ "VendorNumber",
            /*SalesAss*/ "SalesAss",
            /*SoldTo*/ "SoldTo",
            /*Street1*/ "Street1",
            /*Street2*/ "Street2",
            /*City*/ "City",
            /*State*/ "State",
            /*Zip*/ "Zip",
            /*Delivery*/ "Delivery",
            /*Terms*/ "Terms",
            /*FreightSelect*/ "FreightSelect",
            /*IsQManual*/ "IsQManual",
            /*IsPManual*/ "IsPManual",
            /*Location*/ "Location",
            /*Equipment*/ "Equipment",
            /*EquipCategory*/ "EquipCategory",

            /*QuotePrice*/ "QuotePrice",
            /*Credit*/ "Credit",
            /*Freight*/ "Freight",
            /*ShopTime*/ "ShopTime",
            /*TotalCost*/ "TotalCost",
            /*GrossProfit*/ "GrossProfit",
            /*Profit*/ "Profit",
            /*MarkUp*/ "MarkUp",
            /*Description*/ "Description",

            /*Quant1*/ "Quant1",
            /*Descr1*/ "Descr1",
            /*Costs1*/ "Costs1",
            /*ECost1*/ "ECost1",
            /*Price1*/ "Price1",
            /*Quant2*/ "Quant2",
            /*Descr2*/ "Descr2",
            /*Costs2*/ "Costs2",
            /*Price2*/ "Price2",
            /*ECost2*/ "ECost2",
            /*Quant3*/ "Quant3",
            /*Descr3*/ "Descr3",
            /*Costs3*/ "Costs3",
            /*ECost3*/ "ECost3",
            /*Price3*/ "Price3",
            /*Quant4*/ "Quant4",
            /*Descr4*/ "Descr4",
            /*Costs4*/ "Costs4",
            /*ECost4*/ "ECost4",
            /*Price4*/ "Price4",
            /*Quant5*/ "Quant5",
            /*Descr5*/ "Descr5",
            /*Costs5*/ "Costs5",
            /*ECost5*/ "ECost5",
            /*Price5*/ "Price5",
            /*Quant6*/ "Quant6",
            /*Descr6*/ "Descr6",
            /*Costs6*/ "Costs6",
            /*ECost6*/ "ECost6",
            /*Price6*/ "Price6",
            /*Quant7*/ "Quant7",
            /*Descr7*/ "Descr7",
            /*Costs7*/ "Costs7",
            /*ECost7*/ "ECost7",
            /*Price7*/ "Price7",
            /*Quant8*/ "Quant8",
            /*Descr8*/ "Descr8",
            /*Costs8*/ "Costs8",
            /*ECost8*/ "ECost8",
            /*Price8*/ "Price8",
            /*Quant9*/ "Quant9",
            /*Descr9*/ "Descr9",
            /*Costs9*/ "Costs9",
            /*ECost9*/ "ECost9",
            /*Price9*/ "Price9",
            /*Quant10*/ "Quant10",
            /*Descr10*/ "Descr10",
            /*Costs10*/ "Costs10",
            /*ECost10*/ "ECost10",
            /*Price10*/ "Price10",
            /*Quant11*/ "Quant11",
            /*Descr11*/ "Descr11",
            /*Costs11*/ "Costs11",
            /*ECost11*/ "ECost11",
            /*Price11*/ "Price11",
            /*Quant12*/ "Quant12",
            /*Descr12*/ "Descr12",
            /*Costs12*/ "Costs12",
            /*ECost12*/ "ECost12",
            /*Price12*/ "Price12",
            /*Quant13*/ "Quant13",
            /*Descr13*/ "Descr13",
            /*Costs13*/ "Costs13",
            /*ECost13*/ "ECost13",
            /*Price13*/ "Price13",
            /*Quant14*/ "Quant14",
            /*Descr14*/ "Descr14",
            /*Costs14*/ "Costs14",
            /*ECost14*/ "ECost14",
            /*Price14*/ "Price14",
            /*Quant15*/ "Quant15",
            /*Descr15*/ "Descr15",
            /*Costs15*/ "Costs15",
            /*ECost15*/ "ECost15",
            /*Price15*/ "Price15",
            /*Quant16*/ "Quant16",
            /*Descr16*/ "Descr16",
            /*Costs16*/ "Costs16",
            /*ECost16*/ "ECost16",
            /*Price16*/ "Price16",
            /*Quant17*/ "Quant17",
            /*Descr17*/ "Descr17",
            /*Costs17*/ "Costs17",
            /*ECost17*/ "ECost17",
            /*Price17*/ "Price17",
            /*Quant18*/ "Quant18",
            /*Descr18*/ "Descr18",
            /*Costs18*/ "Costs18",
            /*ECost18*/ "ECost18",
            /*Price18*/ "Price18",
            /*Quant19*/ "Quant19",
            /*Descr19*/ "Descr19",
            /*Costs19*/ "Costs19",
            /*ECost19*/ "ECost19",
            /*Price19*/ "Price19",
            /*Quant20*/ "Quant20",
            /*Descr20*/ "Descr20",
            /*Costs20*/ "Costs20",
            /*ECost20*/ "ECost20",
            /*Price20*/ "Price20",
            /*Quant21*/ "Quant21",
            /*Descr21*/ "Descr21",
            /*Costs21*/ "Costs21",
            /*ECost21*/ "ECost21",
            /*Price21*/ "Price21",
            /*Quant22*/ "Quant22",
            /*Descr22*/ "Descr22",
            /*Costs22*/ "Costs22",
            /*ECost22*/ "ECost22",
            /*Price22*/ "Price22",
            /*Quant23*/ "Quant23",
            /*Descr23*/ "Descr23",
            /*Costs23*/ "Costs23",
            /*ECost23*/ "ECost23",
            /*Price23*/ "Price23",

            /*InvNotes*/ "InvNotes",
            /*DeliveryNotes*/ "DeliveryNotes",
            /*QuoteNotes*/ "QuoteNotes"
            #endregion
        });


        static public List<string> Defaults
        {
            get
            {
                return gui_defaults;
            }
        }

        static public string GetTableName(int index)
        {
            return table_names[index];
        }

        static public string GetGUIName(int index)
        {
            return gui_objects[index];
        }

        static public int NameCount
        {
            get
            {
                return table_names.Count;
            }
        }

        static public string UpdateRowCommandText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("UPDATE QuoteTable SET ");
                for (int i = 0; i < NameCount; i++)
                {
                    sb.Append(GetTableName(i) + " = @" + GetTableName(i));
                    if (i < NameCount - 1)
                        sb.Append(", ");
                }
                sb.Append(" Where PO = @PO");

                return sb.ToString();
            }
        }

        static public string InsertRowCommandText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("INSERT INTO QuoteTable (");
                for (int i = 0; i < NameCount; i++)
                {
                    sb.Append(GetTableName(i));
                    if (i < NameCount - 1)
                        sb.Append(", ");
                }
                sb.Append(") VALUES (");
                for (int i = 0; i < NameCount; i++)
                {
                    sb.Append("@" + GetTableName(i));
                    if (i < NameCount - 1)
                        sb.Append(", ");
                }
                sb.Append(")");

                return sb.ToString();
            }
        }
    }
}
