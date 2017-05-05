using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tugwell
{
    public class Order
    {
        static List<string> gui_objects = new List<string>(new string[]
        {
            #region GUI Names
            /*PO*/ "textBoxPO",
            /*Date*/ "textBoxDate",
            /*EndUser*/ "textBoxEndUser",
            /*Equipment*/ "textBoxEquipment",
            /*VendorName*/ "textBoxVendorName",
            /*JobName*/ "textBoxJobName",
            /*CustomerPO*/ "textBoxCustomerPO",
            /*VendorNumber*/ "textBoxVendorNumber",
            /*SalesAss*/ "comboBoxSalesAss",
            /*SoldTo*/ "textBoxSoldTo",

            /*Street1*/ "textBoxStreet1",
            /*Street2*/ "textBoxStreet2",
            /*City*/ "textBoxCity",
            /*State*/ "comboBoxSoldToState",
            /*Zip*/ "textBoxZip",

            /*ShipTo*/ "textBoxShipTo",
            /*ShipStreet1*/ "textBoxShipToStreet1",
            /*ShipStreet2*/ "textBoxShipToStreet2",
            /*ShipCity*/ "textBoxShipToCity",
            /*ShipState*/ "comboBoxShipToState",
            /*ShipZip*/ "textBoxShipToZip",

            /*Carrier*/ "comboBoxCarrier",
            /*ShipDate*/ "textBoxShipDate",

            /*IsOk*/ "checkBoxOK",
            /*IsComOrder*/ "checkBoxComOrder",
            /*IsComPaid*/ "checkBoxComPaid",

            /*Grinder*/ "textBoxGrinder",
            /*SerialNo*/ "textBoxSerialNumber",
            /*PumpStk*/ "textBoxPumpStk",

            /*ReqDate*/ "textBoxReqDate",
            /*SchedShip*/ "textBoxSchedShip",
            /*PODate*/ "textBoxPODate",
            /*POShipVia*/ "textBoxPOShipVia",

            /*TrackDate1*/ "textBoxTrkDate1", /*TrackBy1*/ "comboBoxTrkBy1", /*TrackSource1*/ "comboBoxTrkSource1", /*TrackNote1*/ "textBoxTrkNotes1",
            /*TrackDate2*/ "textBoxTrkDate2", /*TrackBy2*/ "comboBoxTrkBy2", /*TrackSource2*/ "comboBoxTrkSource2", /*TrackNote2*/ "textBoxTrkNotes2",
            /*TrackDate3*/ "textBoxTrkDate3", /*TrackBy3*/ "comboBoxTrkBy3", /*TrackSource3*/ "comboBoxTrkSource3", /*TrackNote3*/ "textBoxTrkNotes3",
            /*TrackDate4*/ "textBoxTrkDate4", /*TrackBy4*/ "comboBoxTrkBy4", /*TrackSource4*/ "comboBoxTrkSource4", /*TrackNote4*/ "textBoxTrkNotes4",
            /*TrackDate5*/ "textBoxTrkDate5", /*TrackBy5*/ "comboBoxTrkBy5", /*TrackSource5*/ "comboBoxTrkSource5", /*TrackNote5*/ "textBoxTrkNotes5",
            /*TrackDate6*/ "textBoxTrkDate6", /*TrackBy6*/ "comboBoxTrkBy6", /*TrackSource6*/ "comboBoxTrkSource6", /*TrackNote6*/ "textBoxTrkNotes6",
            /*TrackDate7*/ "textBoxTrkDate7", /*TrackBy7*/ "comboBoxTrkBy7", /*TrackSource7*/ "comboBoxTrkSource7", /*TrackNote7*/ "textBoxTrkNotes7",
            /*TrackDate8*/ "textBoxTrkDate8", /*TrackBy8*/ "comboBoxTrkBy8", /*TrackSource8*/ "comboBoxTrkSource8", /*TrackNote8*/ "textBoxTrkNotes8",
            /*TrackDate9*/ "textBoxTrkDate9", /*TrackBy9*/ "comboBoxTrkBy9", /*TrackSource9*/ "comboBoxTrkSource9", /*TrackNote9*/ "textBoxTrkNotes9",
            /*TrackDate10*/ "textBoxTrkDate10", /*TrackBy10*/ "comboBoxTrkBy10", /*TrackSource10*/ "comboBoxTrkSource10", /*TrackNote10*/ "textBoxTrkNotes10",
            /*TrackDate11*/ "textBoxTrkDate11", /*TrackBy11*/ "comboBoxTrkBy11", /*TrackSource11*/ "comboBoxTrkSource11", /*TrackNote11*/ "textBoxTrkNotes11",
            /*TrackDate12*/ "textBoxTrkDate12", /*TrackBy12*/ "comboBoxTrkBy12", /*TrackSource12*/ "comboBoxTrkSource12", /*TrackNote12*/ "textBoxTrkNotes12",
            /*TrackDate13*/ "textBoxTrkDate13", /*TrackBy13*/ "comboBoxTrkBy13", /*TrackSource13*/ "comboBoxTrkSource13", /*TrackNote13*/ "textBoxTrkNotes13",
            /*TrackDate14*/ "textBoxTrkDate14", /*TrackBy14*/ "comboBoxTrkBy14", /*TrackSource14*/ "comboBoxTrkSource14", /*TrackNote14*/ "textBoxTrkNotes14",
            /*TrackDate15*/ "textBoxTrkDate15", /*TrackBy15*/ "comboBoxTrkBy15", /*TrackSource15*/ "comboBoxTrkSource15", /*TrackNote15*/ "textBoxTrkNotes15",
            /*TrackDate16*/ "textBoxTrkDate16", /*TrackBy16*/ "comboBoxTrkBy16", /*TrackSource16*/ "comboBoxTrkSource16", /*TrackNote16*/ "textBoxTrkNotes16",
            /*TrackDate17*/ "textBoxTrkDate17", /*TrackBy17*/ "comboBoxTrkBy17", /*TrackSource17*/ "comboBoxTrkSource17", /*TrackNote17*/ "textBoxTrkNotes17",
            /*TrackDate18*/ "textBoxTrkDate18", /*TrackBy18*/ "comboBoxTrkBy18", /*TrackSource18*/ "comboBoxTrkSource18", /*TrackNote18*/ "textBoxTrkNotes18",

            /*QuotePrice*/ "numericUpDownQuotePrice",
            /*Credit*/ "numericUpDownCredit",
            /*Freight*/ "numericUpDownFreight",
            /*ShopTime*/ "numericUpDownShopTime",
            /*TotalCost*/ "numericUpDownTotalCost",
            /*GrossProfit*/ "numericUpDownGrossProfit",
            /*Profit*/ "numericUpDownProfit",

            /*Description*/ "textBoxDescription",
            /*Quant1*/ "numericUpDownOrderCount1", /*Descr1*/ "textBoxOrderDescr1", /*Costs1*/ "numericUpDownOrderCost1", /*ECost1*/ "numericUpDownOrderECost1",
            /*Quant2*/ "numericUpDownOrderCount2", /*Descr2*/ "textBoxOrderDescr2", /*Costs2*/ "numericUpDownOrderCost2", /*ECost2*/ "numericUpDownOrderECost2",
            /*Quant3*/ "numericUpDownOrderCount3", /*Descr3*/ "textBoxOrderDescr3", /*Costs3*/ "numericUpDownOrderCost3", /*ECost3*/ "numericUpDownOrderECost3",
            /*Quant4*/ "numericUpDownOrderCount4", /*Descr4*/ "textBoxOrderDescr4", /*Costs4*/ "numericUpDownOrderCost4", /*ECost4*/ "numericUpDownOrderECost4",
            /*Quant5*/ "numericUpDownOrderCount5", /*Descr5*/ "textBoxOrderDescr5", /*Costs5*/ "numericUpDownOrderCost5", /*ECost5*/ "numericUpDownOrderECost5",
            /*Quant6*/ "numericUpDownOrderCount6", /*Descr6*/ "textBoxOrderDescr6", /*Costs6*/ "numericUpDownOrderCost6", /*ECost6*/ "numericUpDownOrderECost6",
            /*Quant7*/ "numericUpDownOrderCount7", /*Descr7*/ "textBoxOrderDescr7", /*Costs7*/ "numericUpDownOrderCost7", /*ECost7*/ "numericUpDownOrderECost7",
            /*Quant8*/ "numericUpDownOrderCount8", /*Descr8*/ "textBoxOrderDescr8", /*Costs8*/ "numericUpDownOrderCost8", /*ECost8*/ "numericUpDownOrderECost8",
            /*Quant9*/ "numericUpDownOrderCount9", /*Descr9*/ "textBoxOrderDescr9", /*Costs9*/ "numericUpDownOrderCost9", /*ECost9*/ "numericUpDownOrderECost9",
            /*Quant10*/ "numericUpDownOrderCount10", /*Descr10*/ "textBoxOrderDescr10", /*Costs10*/ "numericUpDownOrderCost10", /*ECost10*/ "numericUpDownOrderECost10",
            /*Quant11*/ "numericUpDownOrderCount11", /*Descr11*/ "textBoxOrderDescr11", /*Costs11*/ "numericUpDownOrderCost11", /*ECost11*/ "numericUpDownOrderECost11",
            /*Quant12*/ "numericUpDownOrderCount12", /*Descr12*/ "textBoxOrderDescr12", /*Costs12*/ "numericUpDownOrderCost12", /*ECost12*/ "numericUpDownOrderECost12",
            /*Quant13*/ "numericUpDownOrderCount13", /*Descr13*/ "textBoxOrderDescr13", /*Costs13*/ "numericUpDownOrderCost13", /*ECost13*/ "numericUpDownOrderECost13",
            /*Quant14*/ "numericUpDownOrderCount14", /*Descr14*/ "textBoxOrderDescr14", /*Costs14*/ "numericUpDownOrderCost14", /*ECost14*/ "numericUpDownOrderECost14",
            /*Quant15*/ "numericUpDownOrderCount15", /*Descr15*/ "textBoxOrderDescr15", /*Costs15*/ "numericUpDownOrderCost15", /*ECost15*/ "numericUpDownOrderECost15",
            /*Quant16*/ "numericUpDownOrderCount16", /*Descr16*/ "textBoxOrderDescr16", /*Costs16*/ "numericUpDownOrderCost16", /*ECost16*/ "numericUpDownOrderECost16",
            /*Quant17*/ "numericUpDownOrderCount17", /*Descr17*/ "textBoxOrderDescr17", /*Costs17*/ "numericUpDownOrderCost17", /*ECost17*/ "numericUpDownOrderECost17",
            /*Quant18*/ "numericUpDownOrderCount18", /*Descr18*/ "textBoxOrderDescr18", /*Costs18*/ "numericUpDownOrderCost18", /*ECost18*/ "numericUpDownOrderECost18",
            /*Quant19*/ "numericUpDownOrderCount19", /*Descr19*/ "textBoxOrderDescr19", /*Costs19*/ "numericUpDownOrderCost19", /*ECost19*/ "numericUpDownOrderECost19",
            /*Quant20*/ "numericUpDownOrderCount20", /*Descr20*/ "textBoxOrderDescr20", /*Costs20*/ "numericUpDownOrderCost20", /*ECost20*/ "numericUpDownOrderECost20",
            /*Quant21*/ "numericUpDownOrderCount21", /*Descr21*/ "textBoxOrderDescr21", /*Costs21*/ "numericUpDownOrderCost21", /*ECost21*/ "numericUpDownOrderECost21",
            /*Quant22*/ "numericUpDownOrderCount22", /*Descr22*/ "textBoxOrderDescr22", /*Costs22*/ "numericUpDownOrderCost22", /*ECost22*/ "numericUpDownOrderECost22",
            /*Quant23*/ "numericUpDownOrderCount23", /*Descr23*/ "textBoxOrderDescr23", /*Costs23*/ "numericUpDownOrderCost23", /*ECost23*/ "numericUpDownOrderECost23",

            /*InvInstructions*/ "richTextBoxInvoiceInstructions",
            /*InvNotes*/ "richTextBoxInvoiceNotes",
            /*VendorNotes*/ "richTextBoxVendorNotes",
            /*AccNotes*/ "richTextBoxAccNotes",
            /*CrMemo*/ "textBoxCrMemo",
            /*InvNumber*/ "textBoxInvoiceNumber",
            /*InvDate*/ "textBoxInvoiceDate",
            /*Status*/ "comboBoxStatus",
            /*CheckNumbers*/ "textBoxCheckNumbers",
            /*CheckDates*/ "textBoxCheckDates",

            /*ComDate1*/ "textBoxComDate1", /*ComCheckNumber1*/ "textBoxCheckNumber1", /*ComPaid1*/ "numericUpDownPaid1",
            /*ComDate2*/ "textBoxComDate2", /*ComCheckNumber2*/ "textBoxCheckNumber2", /*ComPaid2*/ "numericUpDownPaid2",
            /*ComDate3*/ "textBoxComDate3", /*ComCheckNumber3*/ "textBoxCheckNumber3", /*ComPaid3*/ "numericUpDownPaid3",
            /*ComDate4*/ "textBoxComDate4", /*ComCheckNumber4*/ "textBoxCheckNumber4", /*ComPaid4*/ "numericUpDownPaid4",
            /*ComDate5*/ "textBoxComDate5", /*ComCheckNumber5*/ "textBoxCheckNumber5", /*ComPaid5*/ "numericUpDownPaid5",

            /*ComAmount*/ "numericUpDownComAmount",
            /*ComBalance*/ "numericUpDownComBalance",
            /*DeliveryNotes*/ "richTextBoxDeliveryNotes",
            /*PONotes*/ "richTextBoxPONotes",

            /*BillTo*/ "comboBoxBillTo",
            /*BillStatus*/ "comboBoxBillStatus"
            #endregion
        });

        static List<string> gui_defaults = new List<string>(new string[]
        {
            #region GUI Defaults
            /*PO*/ "",
            /*Date*/ "",
            /*EndUser*/ "",
            /*Equipment*/ "",
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

            /*ShipTo*/ "",
            /*ShipStreet1*/ "",
            /*ShipStreet2*/ "",
            /*ShipCity*/ "",
            /*ShipState*/ "",
            /*ShipZip*/ "",

            /*Carrier*/ "",
            /*ShipDate*/ "",

            /*IsOk*/ "",
            /*IsComOrder*/ "",
            /*IsComPaid*/ "",

            /*Grinder*/ "",
            /*SerialNo*/ "",
            /*PumpStk*/ "",

            /*ReqDate*/ "",
            /*SchedShip*/ "",
            /*PODate*/ "",
            /*POShipVia*/ "",

            /*TrackDate1*/ "", /*TrackBy1*/ "", /*TrackSource1*/ "", /*TrackNote1*/ "",
            /*TrackDate2*/ "", /*TrackBy2*/ "", /*TrackSource2*/ "", /*TrackNote2*/ "",
            /*TrackDate3*/ "", /*TrackBy3*/ "", /*TrackSource3*/ "", /*TrackNote3*/ "",
            /*TrackDate4*/ "", /*TrackBy4*/ "", /*TrackSource4*/ "", /*TrackNote4*/ "",
            /*TrackDate5*/ "", /*TrackBy5*/ "", /*TrackSource5*/ "", /*TrackNote5*/ "",
            /*TrackDate6*/ "", /*TrackBy6*/ "", /*TrackSource6*/ "", /*TrackNote6*/ "",
            /*TrackDate7*/ "", /*TrackBy7*/ "", /*TrackSource7*/ "", /*TrackNote7*/ "",
            /*TrackDate8*/ "", /*TrackBy8*/ "", /*TrackSource8*/ "", /*TrackNote8*/ "",
            /*TrackDate9*/ "", /*TrackBy9*/ "", /*TrackSource9*/ "", /*TrackNote9*/ "",
            /*TrackDate10*/ "", /*TrackBy10*/ "", /*TrackSource10*/ "", /*TrackNote10*/ "",
            /*TrackDate11*/ "", /*TrackBy11*/ "", /*TrackSource11*/ "", /*TrackNote11*/ "",
            /*TrackDate12*/ "", /*TrackBy12*/ "", /*TrackSource12*/ "", /*TrackNote12*/ "",
            /*TrackDate13*/ "", /*TrackBy13*/ "", /*TrackSource13*/ "", /*TrackNote13*/ "",
            /*TrackDate14*/ "", /*TrackBy14*/ "", /*TrackSource14*/ "", /*TrackNote14*/ "",
            /*TrackDate15*/ "", /*TrackBy15*/ "", /*TrackSource15*/ "", /*TrackNote15*/ "",
            /*TrackDate16*/ "", /*TrackBy16*/ "", /*TrackSource16*/ "", /*TrackNote16*/ "",
            /*TrackDate17*/ "", /*TrackBy17*/ "", /*TrackSource17*/ "", /*TrackNote17*/ "",
            /*TrackDate18*/ "", /*TrackBy18*/ "", /*TrackSource18*/ "", /*TrackNote18*/ "",

            /*QuotePrice*/ "0.00",
            /*Credit*/ "0.00",
            /*Freight*/ "0.00",
            /*ShopTime*/ "0.00",
            /*TotalCost*/ "0.00",
            /*GrossProfit*/ "0.00",
            /*Profit*/ "0.00",

            /*Description*/ "",
            /*Quant1*/ "0.00", /*Descr1*/ "", /*Costs1*/ "0.00", /*ECost1*/ "0.00",
            /*Quant2*/ "0.00", /*Descr2*/ "", /*Costs2*/ "0.00", /*ECost2*/ "0.00",
            /*Quant3*/ "0.00", /*Descr3*/ "", /*Costs3*/ "0.00", /*ECost3*/ "0.00",
            /*Quant4*/ "0.00", /*Descr4*/ "", /*Costs4*/ "0.00", /*ECost4*/ "0.00",
            /*Quant5*/ "0.00", /*Descr5*/ "", /*Costs5*/ "0.00", /*ECost5*/ "0.00",
            /*Quant6*/ "0.00", /*Descr6*/ "", /*Costs6*/ "0.00", /*ECost6*/ "0.00",
            /*Quant7*/ "0.00", /*Descr7*/ "", /*Costs7*/ "0.00", /*ECost7*/ "0.00",
            /*Quant8*/ "0.00", /*Descr8*/ "", /*Costs8*/ "0.00", /*ECost8*/ "0.00",
            /*Quant9*/ "0.00", /*Descr9*/ "", /*Costs9*/ "0.00", /*ECost9*/ "0.00",
            /*Quant10*/ "0.00", /*Descr10*/ "", /*Costs10*/ "0.00", /*ECost10*/ "0.00",
            /*Quant11*/ "0.00", /*Descr11*/ "", /*Costs11*/ "0.00", /*ECost11*/ "0.00",
            /*Quant12*/ "0.00", /*Descr12*/ "", /*Costs12*/ "0.00", /*ECost12*/ "0.00",
            /*Quant13*/ "0.00", /*Descr13*/ "", /*Costs13*/ "0.00", /*ECost13*/ "0.00",
            /*Quant14*/ "0.00", /*Descr14*/ "", /*Costs14*/ "0.00", /*ECost14*/ "0.00",
            /*Quant15*/ "0.00", /*Descr15*/ "", /*Costs15*/ "0.00", /*ECost15*/ "0.00",
            /*Quant16*/ "0.00", /*Descr16*/ "", /*Costs16*/ "0.00", /*ECost16*/ "0.00",
            /*Quant17*/ "0.00", /*Descr17*/ "", /*Costs17*/ "0.00", /*ECost17*/ "0.00",
            /*Quant18*/ "0.00", /*Descr18*/ "", /*Costs18*/ "0.00", /*ECost18*/ "0.00",
            /*Quant19*/ "0.00", /*Descr19*/ "", /*Costs19*/ "0.00", /*ECost19*/ "0.00",
            /*Quant20*/ "0.00", /*Descr20*/ "", /*Costs20*/ "0.00", /*ECost20*/ "0.00",
            /*Quant21*/ "0.00", /*Descr21*/ "", /*Costs21*/ "0.00", /*ECost21*/ "0.00",
            /*Quant22*/ "0.00", /*Descr22*/ "", /*Costs22*/ "0.00", /*ECost22*/ "0.00",
            /*Quant23*/ "0.00", /*Descr23*/ "", /*Costs23*/ "0.00", /*ECost23*/ "0.00",

            /*InvInstructions*/ "",
            /*InvNotes*/ "",
            /*VendorNotes*/ "",
            /*AccNotes*/ "",
            /*CrMemo*/ "",
            /*InvNumber*/ "",
            /*InvDate*/ "",
            /*Status*/ "",
            /*CheckNumbers*/ "",
            /*CheckDates*/ "",

            /*ComDate1*/ "", /*ComCheckNumber1*/ "", /*ComPaid1*/ "0.00",
            /*ComDate2*/ "", /*ComCheckNumber2*/ "", /*ComPaid2*/ "0.00",
            /*ComDate3*/ "", /*ComCheckNumber3*/ "", /*ComPaid3*/ "0.00",
            /*ComDate4*/ "", /*ComCheckNumber4*/ "", /*ComPaid4*/ "0.00",
            /*ComDate5*/ "", /*ComCheckNumber5*/ "", /*ComPaid5*/ "0.00",

            /*ComAmount*/ "0.00",
            /*ComBalance*/ "0.00",
            /*DeliveryNotes*/ "",
            /*PONotes*/ "",

            /*BillTo*/ "Customer",
            /*BillStatus*/ "Created"
            #endregion
        });

        static List<string> table_names = new List<string>(new string[]
        {
            #region Table Names
            /*thePO*/ "PO",

            /*Date*/ "Date",
            /*EndUser*/ "EndUser",

            /*Equipment*/ "Equipment",
            /*VendorName*/ "VendorName",
            /*JobNumber*/ "JobNumber",
            /*CustomerPO*/ "CustomerPO",
            /*VendorNumber*/ "VendorNumber",
            /*SalesAss*/ "SalesAss",
            /*SoldTo*/ "SoldTo",
            /*Street1*/ "Street1",
            /*Street2*/ "Street2",
            /*City*/ "City",
            /*State*/ "State",
            /*Zip*/ "Zip",
            /*ShipTo*/ "ShipTo",
            /*ShipStreet1*/ "ShipStreet1",
            /*ShipStreet2*/ "ShipStreet2",
            /*ShipCity*/ "ShipCity",
            /*ShipState*/ "ShipState",
            /*ShipZip*/ "ShipZip",
            /*Carrier*/ "Carrier",
            /*ShipDate*/ "ShipDate",

            /*IsOk*/ "Spare2",
            /*IsComOrder*/ "IsComOrder",
            /*IsComPaid*/ "IsComPaid",

            /*Grinder*/ "Grinder",
            /*SerialNo*/ "SerialNo",
            /*PumpStk*/ "PumpStk",
            /*ReqDate*/ "ReqDate",
            /*SchedShip*/ "SchedShip",
            /*PODate*/ "PODate",
            /*POShipVia*/ "POShipVia",

            /*TrackDate1*/ "TrackDate1",
            /*TrackBy1*/ "TrackBy1",
            /*TrackSource1*/ "TrackSource1",
            /*TrackNote1*/ "TrackNote1",
            /*TrackDate2*/ "TrackDate2",
            /*TrackBy2*/ "TrackBy2",
            /*TrackSource2*/ "TrackSource2",
            /*TrackNote2*/ "TrackNote2",
            /*TrackDate3*/ "TrackDate3",
            /*TrackBy3*/ "TrackBy3",
            /*TrackSource3*/ "TrackSource3",
            /*TrackNote3*/ "TrackNote3",
            /*TrackDate4*/ "TrackDate4",
            /*TrackBy4*/ "TrackBy4",
            /*TrackSource4*/ "TrackSource4",
            /*TrackNote4*/ "TrackNote4",
            /*TrackDate5*/ "TrackDate5",
            /*TrackBy5*/ "TrackBy5",
            /*TrackSource5*/ "TrackSource5",
            /*TrackNote5*/ "TrackNote5",
            /*TrackDate6*/ "TrackDate6",
            /*TrackBy6*/ "TrackBy6",
            /*TrackSource6*/ "TrackSource6",
            /*TrackNote6*/ "TrackNote6",
            /*TrackDate7*/ "TrackDate7",
            /*TrackBy7*/ "TrackBy7",
            /*TrackSource7*/ "TrackSource7",
            /*TrackNote7*/ "TrackNote7",
            /*TrackDate8*/ "TrackDate8",
            /*TrackBy8*/ "TrackBy8",
            /*TrackSource8*/ "TrackSource8",
            /*TrackNote8*/ "TrackNote8",
            /*TrackDate9*/ "TrackDate9",
            /*TrackBy9*/ "TrackBy9",
            /*TrackSource9*/ "TrackSource9",
            /*TrackNote9*/ "TrackNote9",
            /*TrackDate10*/ "TrackDate10",
            /*TrackBy10*/ "TrackBy10",
            /*TrackSource10*/ "TrackSource10",
            /*TrackNote10*/ "TrackNote10",
            /*TrackDate11*/ "TrackDate11",
            /*TrackBy11*/ "TrackBy11",
            /*TrackSource11*/ "TrackSource11",
            /*TrackNote11*/ "TrackNote11",
            /*TrackDate12*/ "TrackDate12",
            /*TrackBy12*/ "TrackBy12",
            /*TrackSource12*/ "TrackSource12",
            /*TrackNote12*/ "TrackNote12",
            /*TrackDate13*/ "TrackDate13",
            /*TrackBy13*/ "TrackBy13",
            /*TrackSource13*/ "TrackSource13",
            /*TrackNote13*/ "TrackNote13",
            /*TrackDate14*/ "TrackDate14",
            /*TrackBy14*/ "TrackBy14",
            /*TrackSource14*/ "TrackSource14",
            /*TrackNote14*/ "TrackNote14",
            /*TrackDate15*/ "TrackDate15",
            /*TrackBy15*/ "TrackBy15",
            /*TrackSource15*/ "TrackSource15",
            /*TrackNote15*/ "TrackNote15",
            /*TrackDate16*/ "TrackDate16",
            /*TrackBy16*/ "TrackBy16",
            /*TrackSource16*/ "TrackSource16",
            /*TrackNote16*/ "TrackNote16",
            /*TrackDate17*/ "TrackDate17",
            /*TrackBy17*/ "TrackBy17",
            /*TrackSource17*/ "TrackSource17",
            /*TrackNote17*/ "TrackNote17",
            /*TrackDate18*/ "TrackDate18",
            /*TrackBy18*/ "TrackBy18",
            /*TrackSource18*/ "TrackSource18",
            /*TrackNote18*/ "TrackNote18",
            /*QuotePrice*/ "QuotePrice",
            /*Credit*/ "Credit",
            /*Freight*/ "Freight",
            /*ShopTime*/ "ShopTime",
            /*TotalCost*/ "TotalCost",
            /*GrossProfit*/ "GrossProfit",
            /*Profit*/ "Profit",
            /*Description*/ "Description",

            /*Quant1*/ "Quant1",
            /*Descr1*/ "Descr1",
            /*Costs1*/ "Costs1",
            /*ECost1*/ "ECost1",
            /*Quant2*/ "Quant2",
            /*Descr2*/ "Descr2",
            /*Costs2*/ "Costs2",
            /*ECost2*/ "ECost2",
            /*Quant3*/ "Quant3",
            /*Descr3*/ "Descr3",
            /*Costs3*/ "Costs3",
            /*ECost3*/ "ECost3",
            /*Quant4*/ "Quant4",
            /*Descr4*/ "Descr4",
            /*Costs4*/ "Costs4",
            /*ECost4*/ "ECost4",
            /*Quant5*/ "Quant5",
            /*Descr5*/ "Descr5",
            /*Costs5*/ "Costs5",
            /*ECost5*/ "ECost5",
            /*Quant6*/ "Quant6",
            /*Descr6*/ "Descr6",
            /*Costs6*/ "Costs6",
            /*ECost6*/ "ECost6",
            /*Quant7*/ "Quant7",
            /*Descr7*/ "Descr7",
            /*Costs7*/ "Costs7",
            /*ECost7*/ "ECost7",
            /*Quant8*/ "Quant8",
            /*Descr8*/ "Descr8",
            /*Costs8*/ "Costs8",
            /*ECost8*/ "ECost8",
            /*Quant9*/ "Quant9",
            /*Descr9*/ "Descr9",
            /*Costs9*/ "Costs9",
            /*ECost9*/ "ECost9",
            /*Quant10*/ "Quant10",
            /*Descr10*/ "Descr10",
            /*Costs10*/ "Costs10",
            /*ECost10*/ "ECost10",
            /*Quant11*/ "Quant11",
            /*Descr11*/ "Descr11",
            /*Costs11*/ "Costs11",
            /*ECost11*/ "ECost11",
            /*Quant12*/ "Quant12",
            /*Descr12*/ "Descr12",
            /*Costs12*/ "Costs12",
            /*ECost12*/ "ECost12",
            /*Quant13*/ "Quant13",
            /*Descr13*/ "Descr13",
            /*Costs13*/ "Costs13",
            /*ECost13*/ "ECost13",
            /*Quant14*/ "Quant14",
            /*Descr14*/ "Descr14",
            /*Costs14*/ "Costs14",
            /*ECost14*/ "ECost14",
            /*Quant15*/ "Quant15",
            /*Descr15*/ "Descr15",
            /*Costs15*/ "Costs15",
            /*ECost15*/ "ECost15",
            /*Quant16*/ "Quant16",
            /*Descr16*/ "Descr16",
            /*Costs16*/ "Costs16",
            /*ECost16*/ "ECost16",
            /*Quant17*/ "Quant17",
            /*Descr17*/ "Descr17",
            /*Costs17*/ "Costs17",
            /*ECost17*/ "ECost17",
            /*Quant18*/ "Quant18",
            /*Descr18*/ "Descr18",
            /*Costs18*/ "Costs18",
            /*ECost18*/ "ECost18",
            /*Quant19*/ "Quant19",
            /*Descr19*/ "Descr19",
            /*Costs19*/ "Costs19",
            /*ECost19*/ "ECost19",
            /*Quant20*/ "Quant20",
            /*Descr20*/ "Descr20",
            /*Costs20*/ "Costs20",
            /*ECost20*/ "ECost20",
            /*Quant21*/ "Quant21",
            /*Descr21*/ "Descr21",
            /*Costs21*/ "Costs21",
            /*ECost21*/ "ECost21",
            /*Quant22*/ "Quant22",
            /*Descr22*/ "Descr22",
            /*Costs22*/ "Costs22",
            /*ECost22*/ "ECost22",
            /*Quant23*/ "Quant23",
            /*Descr23*/ "Descr23",
            /*Costs23*/ "Costs23",
            /*ECost23*/ "ECost23",

            /*InvInstructions*/ "InvInstructions",
            /*InvNotes*/ "InvNotes",
            /*VendorNotes*/ "VendorNotes",
            /*AccNotes*/ "Spare1",
            /*CrMemo*/ "CrMemo",
            /*InvNumber*/ "InvNumber",
            /*InvDate*/ "InvDate",
            /*Status*/ "Status",
            /*CheckNumbers*/ "CheckNumbers",
            /*CheckDates*/ "CheckDates",

            /*ComDate1*/ "ComDate1",
            /*ComCheckNumber1*/ "ComCheckNumber1",
            /*ComPaid1*/ "ComPaid1",
            /*ComDate2*/ "ComDate2",
            /*ComCheckNumber2*/ "ComCheckNumber2",
            /*ComPaid2*/ "ComPaid2",
            /*ComDate3*/ "ComDate3",
            /*ComCheckNumber3*/ "ComCheckNumber3",
            /*ComPaid3*/ "ComPaid3",
            /*ComDate4*/ "ComDate4",
            /*ComCheckNumber4*/ "ComCheckNumber4",
            /*ComPaid4*/ "ComPaid4",
            /*ComDate5*/ "ComDate5",
            /*ComCheckNumber5*/ "ComCheckNumber5",
            /*ComPaid5*/ "ComPaid5",

            /*ComAmount*/ "ComAmount",
            /*ComBalance*/ "ComBalance",
            /*DeliveryNotes*/ "DeliveryNotes",
            /*PONotes*/ "PONotes",
            
            /*BillTo*/ "Spare3",
            /*BillStatus*/ "Spare4"
            #endregion
        });


        static public List<string> Defaults
        {
            get { return gui_defaults; }
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
            get { return table_names.Count; }
        }

        #region Sql Command Text Generators

        static public string GetRowCountCommandText
        {
            get { return "Select COUNT(PO) From OrderTable"; }
        }

        static public string DeletePOCommandText(string PO)
        {
            return "DELETE From OrderTable Where PO = '" + PO + "'";
        }

        static public string GetPOsCommandText
        {
            get { return "Select PO FROM OrderTable"; }
        }

        static public string ReadRowCommandText(string PO)
        {
            if (PO == "")
                return "Select * FROM OrderTable";
            else
                return "Select * FROM OrderTable Where PO = '" + PO + "'";
        }

        static public string UpdateRowCommandText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("UPDATE OrderTable SET ");
                for (int i = 0; i < NameCount; i++)
                {
                    sb.Append(GetTableName(i) + " = @" + GetTableName(i));
                    if ( i < NameCount-1)
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

                sb.Append("INSERT INTO OrderTable (");
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

        #endregion
    }
}
