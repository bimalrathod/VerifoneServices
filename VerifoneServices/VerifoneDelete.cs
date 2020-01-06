using VerifoneLibrary.DataAccess;
using RapidVerifone;
using RapidVarifone;
using RapidVerifoneNAXML;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Configuration;
using System.Collections.Generic;

namespace VerifoneServices
{
    public class VerifoneDelete
    {
        Comman objComman = new Comman();
        public string MixMatch_ItemListID = "";
        public bool ItemListID_isError = false;
        public string New_VP = "";

        public void DeleteVerifoneTax(DataTable dtdeleteTax)
        {
            _CVerifone objCVerifone = new _CVerifone();
            DataTable dtUpdate = new DataTable();
            string arraysysid = "";
            string sysidxml = "";
            taxRateConfig objtaxRateConfig = new taxRateConfig();
            XmlDocument docxmlPath = new XmlDocument();
            string TaxPayloadXML = "";
            StringBuilder sb = new StringBuilder();
            List<RapidVerifone.taxRate> lsttaxRate = new List<RapidVerifone.taxRate>();
            taxRateConfigOperation[] operationClass = new taxRateConfigOperation[1];
            taxRateConfigOperation opr = new taxRateConfigOperation();

            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Tax.xml";
                docxmlPath.Load(path);
                objCVerifone.InsertActiveLog("Verifone", "Start", "DeleteVerifoneTax()", "Initialize to Delete", "Tax", "utaxratecfg");

                XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                if (nodesite != null)
                {
                    objtaxRateConfig.site = nodesite.InnerText;
                }
                else
                {
                    objtaxRateConfig.site = "";
                }
                opr.type = "delete";

                for (int i = 0; i < dtdeleteTax.Rows.Count; i++)
                {

                    taxRateTaxProperties objtaxRateTaxProperties = new taxRateTaxProperties();
                    sysidxml = Convert.ToString(dtdeleteTax.Rows[i][1]);
                    XmlNode node = docxmlPath.DocumentElement.SelectSingleNode("//taxRates//taxRate[@sysid='" + sysidxml + "']");
                    RapidVerifone.taxRate objtaxRate1 = new RapidVerifone.taxRate();


                    objtaxRate1.sysid = Convert.ToString(dtdeleteTax.Rows[i][1]);
                    objtaxRate1.name = Convert.ToString(dtdeleteTax.Rows[i][3]);
                    objtaxRateTaxProperties.rate = Convert.ToDecimal(dtdeleteTax.Rows[i][4]);


                    if (node != null)
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof(RapidVerifone.taxRate));
                            using (TextReader reader = new StringReader(node.OuterXml))
                            {
                                objtaxRate1 = (RapidVerifone.taxRate)serializer.Deserialize(reader);
                            }
                            if (objtaxRate1 != null)
                            {
                                objtaxRate1.indicator = Convert.ToString(objtaxRate1.indicator);  //2
                                objtaxRate1.isPriceIncsTax = Convert.ToBoolean(objtaxRate1.isPriceIncsTax);  //3
                                objtaxRate1.isPromptExemption = Convert.ToBoolean(objtaxRate1.isPromptExemption);  //4
                                objtaxRateTaxProperties.pctStartAmt = Convert.ToDecimal(objtaxRate1.taxProperties.pctStartAmt); //6
                                objtaxRateTaxProperties.pctStartAmtSpecified = Convert.ToBoolean(objtaxRate1.taxProperties.pctStartAmtSpecified); //6
                                objtaxRateTaxProperties.rateSpecified = Convert.ToBoolean(objtaxRate1.taxProperties.rateSpecified); //6
                                if (arraysysid == "")
                                {
                                    arraysysid = sysidxml;
                                }
                                else
                                {
                                    arraysysid = arraysysid + "," + sysidxml;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteVerifoneTax()", "DeleteVerifoneTax Exception : " + ex, "Tax", "utaxratecfg");
                        }
                    }

                    objtaxRate1.taxProperties = objtaxRateTaxProperties;
                    lsttaxRate.Add(objtaxRate1);
                }

                opr.taxRates = lsttaxRate.ToArray();
                operationClass[0] = opr;
                objtaxRateConfig.operation = operationClass;

                string xml = objComman.GetXMLFromObject(objtaxRateConfig);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(sb, settings);
                doc.WriteTo(writer);
                writer.Close();
                string t = "xmlns=\"" + "" + "\"";
                sb = sb.Replace(t, "");
                TaxPayloadXML = sb.ToString();

                #region Pass Payload to generate Verifone data xml
                string Response = objComman.GetXMLResult(TaxPayloadXML, "UpdateTax", "POST", "utaxratecfg");
                #endregion

                #region Delete updated data from BoF
                if (Response != null && Response != "")
                {
                    long result = objCVerifone.DeleteVerifoneHistory("Tax", arraysysid);
                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("Verifone", "Fail", "DeleteVerifoneTax()", "Verifone Data not Deleted ", "Tax", "utaxratecfg");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifoneTax()", "Verifone Data Deleted successfully", "Tax", "utaxratecfg");
                    }
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifoneTax()", "No Response", "Tax", "utaxratecfg");
                }
                #endregion

            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteVerifoneTax()", "DeleteVerifoneTax Exception 3 : " + ex, "Tax", "utaxratecfg");
            }
        }

        public void DeleteVerifoneDepartment(DataTable dt, DataTable dttax)
        {
            _CVerifone objCVerifone = new _CVerifone();
            string sysidxml = "";
            string arraysysid = "";
            posConfig objposConfig = new posConfig();
            List<department> lstdepartment = new List<department>();
            XmlDocument docxmlPath = new XmlDocument();
            string DepartmentPayloadXML = "";
            StringBuilder sb = new StringBuilder();

            posConfigOperation[] operationClass = new posConfigOperation[1];
            posConfigOperation opr = new posConfigOperation();
            try
            {
                objCVerifone.InsertActiveLog("Verifone", "Start", "DeleteVerifoneDepartment()", "Initialize to update", "Department", "uposcfg");


                var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Department.xml";
                docxmlPath.Load(path);

                XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                if (nodesite != null)
                {
                    objposConfig.site = nodesite.InnerText;
                }
                else
                {
                    objposConfig.site = "";
                }

                opr.type = "delete";
                operationClass[0] = opr;
                objposConfig.operation = operationClass;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    department objdepartment = new department();
                    departmentCategory objdeptCat = new departmentCategory();
                    departmentProdCode objdepartmentProdCode = new departmentProdCode();

                    sysidxml = Convert.ToString(dt.Rows[i][0]);
                    XmlNode node = docxmlPath.DocumentElement.SelectSingleNode("//departments//department[@sysid='" + sysidxml + "']");
                    if (node != null)
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof(RapidVerifone.department));
                            RapidVerifone.department objDepartmentGet = new RapidVerifone.department();

                            using (TextReader reader = new StringReader(node.OuterXml))
                            {
                                objDepartmentGet = (RapidVerifone.department)serializer.Deserialize(reader);
                            }
                            if (objDepartmentGet != null)
                            {

                                objdepartment.sysid = Convert.ToString(dt.Rows[i][0]);
                                objdepartment.name = Convert.ToString(dt.Rows[i][1]);
                                objdepartment.minAmt = objDepartmentGet.minAmt;
                                objdepartment.maxAmt = objDepartmentGet.maxAmt;
                                objdepartment.isAllowFS = objDepartmentGet.isAllowFS;
                                objdepartment.isNegative = objDepartmentGet.isNegative;
                                objdepartment.isFuel = Convert.ToBoolean(dt.Rows[i][2]);
                                objdepartment.isAllowFQ = objDepartmentGet.isAllowFQ;
                                objdepartment.isAllowSD = objDepartmentGet.isAllowSD;
                                objdepartment.prohibitDisc = objDepartmentGet.prohibitDisc;  //
                                objdepartment.prohibitDiscSpecified = objDepartmentGet.prohibitDiscSpecified;  //
                                objdepartment.isBL1 = objDepartmentGet.isBL1; //
                                objdepartment.isBL1Specified = objDepartmentGet.isBL1Specified; //
                                objdepartment.isBL2 = objDepartmentGet.isBL2; //
                                objdepartment.isBL2Specified = objDepartmentGet.isBL2Specified; //
                                objdepartment.isMoneyOrder = Convert.ToBoolean(dt.Rows[i][3]);
                                objdepartment.isSNPromptReqd = objDepartmentGet.isSNPromptReqd; //
                                objdepartment.isSNPromptReqdSpecified = objDepartmentGet.isSNPromptReqdSpecified; //

                                objdeptCat.sysid = Convert.ToString(dt.Rows[i][4]);
                                objdepartment.category = objdeptCat;

                                objdepartmentProdCode.sysid = Convert.ToString(dt.Rows[i][5]) == "" ? "0" : Convert.ToString(dt.Rows[i][5]);
                                objdepartment.prodCode = objdepartmentProdCode;

                                DataView dv = dttax.DefaultView;
                                DataTable dttaxdept = new DataTable();
                                dv.RowFilter = ("[" + dttax.Columns[0].ColumnName + "] = " + Convert.ToString(dt.Rows[i][0]) + "");
                                dttaxdept = dv.ToTable();
                                dv.RowFilter = string.Empty;



                                if (dttaxdept != null && dttaxdept.Rows.Count > 0)
                                {
                                    departmentTax[] objdepartmentTax = new departmentTax[dttaxdept.Rows.Count];

                                    for (int a = 0; a < dttaxdept.Rows.Count; a++)
                                    {
                                        objdepartmentTax[a] = new departmentTax();
                                        objdepartmentTax[a].sysid = Convert.ToString(dttaxdept.Rows[a][1]);
                                    }

                                    objdepartment.taxes = objdepartmentTax;

                                }
                                lstdepartment.Add(objdepartment);
                                //objposConfig.departments = objdepartment;
                                if (arraysysid == "")
                                {
                                    arraysysid = sysidxml;
                                }
                                else
                                {
                                    arraysysid = arraysysid + "," + sysidxml;
                                }

                                dv.RowFilter = string.Empty;
                            }
                        }
                        catch (Exception ex)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteVerifoneDepartment()", "DeleteVerifoneDepartment Exception : " + Convert.ToString(ex), "Department", "uposcfg");
                        }
                    }
                }

                opr.departments = lstdepartment.ToArray();
                operationClass[0] = opr;
                objposConfig.operation = operationClass;

                string xml = objComman.GetXMLFromObject(objposConfig);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(sb, settings);
                doc.WriteTo(writer);
                writer.Close();
                string t = "xmlns=\"" + "" + "\"";
                sb = sb.Replace(t, "");
                //this.txtPayloadXml.Text = sb.ToString();
                DepartmentPayloadXML = sb.ToString();

                #region Pass Payload to generate Verifone data xml
                string Response = objComman.GetXMLResult(DepartmentPayloadXML, "UpdateDepartment", "POST", "uposcfg");
                #endregion

                #region Delete updated data from BoF
                if (Response != null && Response != "")
                {
                    long result = objCVerifone.DeleteVerifoneHistory("Department", arraysysid);
                    if (result == 0)
                    {
                        // ******** msg of fail
                        objCVerifone.InsertActiveLog("Verifone", "Fail", "DeleteVerifoneDepartment()", "Verifone data not updated", "Department", "uposcfg");
                    }
                    else
                    {
                        // ******** msg of success
                        objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifoneDepartment()", "Verifone data updated successfully", "Department", "uposcfg");
                    }
                }
                else
                {
                    // ******** msg of no response
                    objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifoneDepartment()", "No Response", "Department", "uposcfg");
                }
                #endregion

            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteVerifoneDepartment()", "DeleteVerifoneDepartment Exception 2 : " + Convert.ToString(ex), "Department", "uposcfg");
            }
        }

        public void DeleteVerifonePayment(DataTable dtpayment)
        {
            _CVerifone objCVerifone = new _CVerifone();
            string sysidxml = "";
            string arraysysid = "";
            try
            {

                paymentConfig objpaymentConfig = new paymentConfig();

                var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Payment.xml";
                XmlDocument docxmlPath = new XmlDocument();
                docxmlPath.Load(path);
                objCVerifone.InsertActiveLog("Verifone", "Start", "DeleteVerifonePayment()", "Initialize to update", "Payment", "upaymentcfg");

                string PaymentPayloadXML = "";
                StringBuilder sb = new StringBuilder();

                XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                if (nodesite != null)
                {
                    objpaymentConfig.site = nodesite.InnerText;
                }
                else
                {
                    objpaymentConfig.site = "";
                }

                paymentConfigOperation[] operationClass = new paymentConfigOperation[1];
                paymentConfigOperation opr = new paymentConfigOperation();
                opr.type = "delete";

                List<mop> lst = new List<mop>();
                for (int i = 0; i < dtpayment.Rows.Count; i++)
                {

                    sysidxml = Convert.ToString(dtpayment.Rows[i][1]);
                    XmlNode node = docxmlPath.DocumentElement.SelectSingleNode("//mops//mop[@sysid='" + sysidxml + "']");
                    if (node != null)
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof(RapidVerifone.mop));
                            RapidVerifone.mop objpaymentConfigMopGet = new RapidVerifone.mop();

                            using (TextReader reader = new StringReader(node.OuterXml))
                            {
                                objpaymentConfigMopGet = (RapidVerifone.mop)serializer.Deserialize(reader);
                            }
                            if (objpaymentConfigMopGet != null)
                            {
                                mop objmop = new mop();
                                objmop.sysid = Convert.ToString(dtpayment.Rows[i][1]);
                                objmop.name = Convert.ToString(dtpayment.Rows[i][3]);
                                objmop.code = objpaymentConfigMopGet.code;
                                objmop.min = objpaymentConfigMopGet.min;
                                objmop.max = objpaymentConfigMopGet.max;
                                objmop.limit = objpaymentConfigMopGet.limit;
                                objmop.isForceSafeDrop = objpaymentConfigMopGet.isForceSafeDrop;
                                objmop.isOpenDrwOnSale = objpaymentConfigMopGet.isOpenDrwOnSale;
                                objmop.isTenderAmtReqd = objpaymentConfigMopGet.isTenderAmtReqd;
                                objmop.isCashrRptPrompt = objpaymentConfigMopGet.isCashrRptPrompt;
                                objmop.isAllowZeroEntry = objpaymentConfigMopGet.isAllowZeroEntry;
                                objmop.isAllowWithoutSale = objpaymentConfigMopGet.isAllowWithoutSale;
                                objmop.isAllowRefund = objpaymentConfigMopGet.isAllowRefund;
                                objmop.isAllowChange = objpaymentConfigMopGet.isAllowChange;
                                objmop.isAllowSafeDrop = objpaymentConfigMopGet.isAllowSafeDrop;
                                objmop.isAllowMOPurch = objpaymentConfigMopGet.isAllowMOPurch;
                                objmop.isForceTicketPrint = objpaymentConfigMopGet.isForceTicketPrint;
                                objmop.numReceiptCopies = objpaymentConfigMopGet.numReceiptCopies;
                                objmop.nacstendercode = objpaymentConfigMopGet.nacstendercode;
                                objmop.nacstendersubcode = objpaymentConfigMopGet.nacstendersubcode;

                                lst.Add(objmop);

                                if (arraysysid == "")
                                {
                                    arraysysid = sysidxml;
                                }
                                else
                                {
                                    arraysysid = arraysysid + "," + sysidxml;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteVerifonePayment()", "DeleteVerifonePayment Exception : " + Convert.ToString(ex), "Payment", "upaymentcfg");
                        }
                    }

                }

                opr.mops = lst.ToArray();
                operationClass[0] = opr;
                objpaymentConfig.operation = operationClass;


                string xml = objComman.GetXMLFromObject(objpaymentConfig);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(sb, settings);
                doc.WriteTo(writer);
                writer.Close();
                string t = "xmlns=\"" + "" + "\"";
                sb = sb.Replace(t, "");
                //this.txtPayloadXml.Text = sb.ToString();
                PaymentPayloadXML = sb.ToString();

                #region Pass Payload to generate Verifone data xml
                string Response = objComman.GetXMLResult(PaymentPayloadXML, "UpdatePayment", "POST", "upaymentcfg");
                #endregion

                #region Delete updated data from BoF
                if (Response != null && Response != "")
                {
                    long result = objCVerifone.DeleteVerifoneHistory("Payment", arraysysid);
                    if (result == 0)
                    {
                        // ******** msg of fail
                        objCVerifone.InsertActiveLog("Verifone", "Fail", "DeleteVerifonePayment()", "Verifone Data not updated", "Payment", "upaymentcfg");
                    }
                    else
                    {
                        // ******** msg of success
                        objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifonePayment()", "Verifone Data updated successfully", "Payment", "upaymentcfg");
                    }
                }
                else
                {
                    // ******** msg of no response
                    objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifonePayment()", "No Response", "Payment", "upaymentcfg");
                }
                #endregion


            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteVerifonePayment()", "DeleteVerifonePayment Exception 2 : " + Convert.ToString(ex), "Payment", "upaymentcfg");
            }
        }

        public void DeleteVerifoneCategory(DataTable dtCat)
        {
            string arraysysid = "";
            string sysidxml = "";
            _CVerifone objCVerifone = new _CVerifone();
            try
            {

                posConfig objposConfig = new posConfig();

                if (dtCat != null && dtCat.Rows.Count > 0)
                {
                    objCVerifone.InsertActiveLog("Verifone", "Start", "Update_UpdateVerifoneCategory()", "Initialize to update", "Category", "uposcfg");

                    string CategoryPayloadXML = "";
                    StringBuilder sb = new StringBuilder();

                    objposConfig.site = "";



                    posConfigOperation[] operationClass = new posConfigOperation[1];

                    posConfigOperation opr = new posConfigOperation();
                    opr.type = "delete";
                    operationClass[0] = opr;

                    objposConfig.operation = operationClass;

                    List<category> lstcategory = new List<category>();
                   // category[] objCategory = new category[dtCat.Rows.Count];

                    for (int i = 0; i < dtCat.Rows.Count; i++)
                    {
                        try
                        {
                            category objcategory = new category();
                            sysidxml = Convert.ToString(dtCat.Rows[i][0]);
                            objcategory.sysid = Convert.ToString(dtCat.Rows[i][0]);
                            objcategory.name = Convert.ToString(dtCat.Rows[i][1]);

                            lstcategory.Add(objcategory);
                            

                            if (arraysysid == "")
                            {
                                arraysysid = sysidxml;
                            }
                            else
                            {
                                arraysysid = arraysysid + "," + sysidxml;
                            }
                        }
                        catch (Exception ex)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Error", "Update_UpdateVerifoneCategory()", "Update_UpdateVerifoneCategory Exception : " + Convert.ToString(ex), "Category", "uposcfg");
                        }
                    }

                    opr.categories = lstcategory.ToArray();
                    operationClass[0] = opr;
                    objposConfig.operation = operationClass;

                    string xml = objComman.GetXMLFromObject(objposConfig);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(sb, settings);
                    doc.WriteTo(writer);
                    writer.Close();
                    string t = "xmlns=\"" + "" + "\"";
                    sb = sb.Replace(t, "");
                    //this.txtPayloadXml.Text = sb.ToString();
                    CategoryPayloadXML = sb.ToString();

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(CategoryPayloadXML, "UpdateCategory", "POST", "uposcfg");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long result = objCVerifone.DeleteVerifoneHistory("Category", arraysysid);
                        if (result == 0)
                        {
                            // ******** msg of fail
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "Update_UpdateVerifoneCategory()", "Verifone data not updated", "Category", "uposcfg");
                        }
                        else
                        {
                            // ******** msg of success
                            objCVerifone.InsertActiveLog("Verifone", "End", "Update_UpdateVerifoneCategory()", "Verifone data updated successfully", "Category", "uposcfg");
                        }
                    }
                    else
                    {
                        // ******** msg of no response
                        objCVerifone.InsertActiveLog("Verifone", "End", "Update_UpdateVerifoneCategory()", "No Response", "Category", "uposcfg");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "Update_UpdateVerifoneCategory()", "Not any Category data is pending to update", "Category", "uposcfg");
                }
            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "Update_UpdateVerifoneCategory()", "Update_UpdateVerifoneCategory Exception 2 : " + Convert.ToString(ex), "Category", "uposcfg");
            }
        }

        public void DeleteVerifoneItems(DataTable dtItem)
        {
            _CVerifone objCVerifone = new _CVerifone();
            try
            {

                PLUsDeletePLU[] operationClass = new PLUsDeletePLU[1];
                PLUs objPLUs = new PLUs();


                List<PLUsDeletePLU> lstPLUsDeletePLU = new List<PLUsDeletePLU>();
                if (dtItem != null && dtItem.Rows.Count > 0)
                {


                    for (int i = 0; i < dtItem.Rows.Count; i++)
                    {
                        PLUsDeletePLU opr = new PLUsDeletePLU();
                        PLUCTypeUpc objPLUCTypeUpcdel = new PLUCTypeUpc();
                        objPLUCTypeUpcdel.Value = Convert.ToString(dtItem.Rows[i]["Barcode"]);
                        opr.upc = objPLUCTypeUpcdel;
                        opr.upcModifier = Convert.ToString(dtItem.Rows[i]["ITEM_ShortName"]);
                       // operationClass[0] = opr;
                        lstPLUsDeletePLU.Add(opr);
                    }

                }
                operationClass = lstPLUsDeletePLU.ToArray();

                string ItemPayloadXML = "";
                StringBuilder sb = new StringBuilder();

                //PLUCType[] objPLUCType = new PLUCType[dtItem.Rows.Count];
                //taxRebateType objtaxRebateType = new taxRebateType();
                //RapidVarifone.currencyAmt objcurrencyAmt = new RapidVarifone.currencyAmt();

                objPLUs.deletePLU = operationClass;


                string xml = objComman.GetXMLFromObject(objPLUs);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(sb, settings);
                doc.WriteTo(writer);
                writer.Close();
                string t = "xmlns=\"" + "" + "\"";
                sb = sb.Replace(t, "");
                //this.txtPayloadXml.Text = sb.ToString();
                ItemPayloadXML = sb.ToString();

                string Response = objComman.GetXMLResult(ItemPayloadXML, "UpdateItems", "POST", "uPLUs");
               
                if (Response != null && Response != "")
                {
                    long result = objCVerifone.DeleteVerifoneHistory("Items", "");
                    if (result == 0)
                    {
                        // ******** msg of fail
                        objCVerifone.InsertActiveLog("Verifone", "Fail", "DeleteVerifoneItems()", "Verifone data not updated", "Items", "uPLUs");
                    }
                    else
                    {
                        // ******** msg of success
                        objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifoneItems()", "Verifone data updated successfully", "Items", "uPLUs");
                    }
                }
                else
                {
                    // ******** msg of no response
                    objCVerifone.InsertActiveLog("Verifone", "End", "DeleteVerifoneItems()", "No Response", "Items", "uPLUs");
                }
            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteVerifoneItems()", "DeleteVerifoneItems Exception 2 : " + Convert.ToString(ex), "Items", "uPLUs");
            }
        }
        
        #region MixMatch
        public void DeleteMixMatchItem(DataTable dtItem)
        {
            _CVerifone objCVerifone = new _CVerifone();
            StringBuilder sb = new StringBuilder();
            string ItemXML = "";
            string ItemListID = "";
            try
            {
                objCVerifone.InsertActiveLog("Verifone", "Start", "DeleteMixMatchItem()", "Initialize DeleteMixMatchItem", "MixMatch_ItemList", "uMaintenance");

                if (dtItem != null && dtItem.Rows.Count > 0)
                {
                    RapidVerifoneNAXML.NAXMLMaintenanceRequest objNAXMLConfig_Item = new RapidVerifoneNAXML.NAXMLMaintenanceRequest();

                    DataTable uniqueCols = dtItem.DefaultView.ToTable(true, "ItemListID", "CommandType", "ItemName");

                    #region TransmissionHeader
                    RapidVerifoneNAXML.transmissionHeader objtransmissionHeader = new RapidVerifoneNAXML.transmissionHeader();
                    objtransmissionHeader.StoreLocationID = ConfigurationManager.AppSettings["StoreLocationID"];
                    objtransmissionHeader.VendorName = ConfigurationManager.AppSettings["VendorName"];
                    objtransmissionHeader.VendorModelVersion = ConfigurationManager.AppSettings["VendorModelVersion"];
                    objNAXMLConfig_Item.TransmissionHeader = objtransmissionHeader;
                    #endregion

                    RapidVerifoneNAXML.ItemListMaintenance[] objItem = new RapidVerifoneNAXML.ItemListMaintenance[1];

                    objItem[0] = new RapidVerifoneNAXML.ItemListMaintenance();

                    RapidVerifoneNAXML.tableActionType result = new RapidVerifoneNAXML.tableActionType();
                    result = RapidVerifoneNAXML.tableActionType.update;

                    RapidVerifoneNAXML.tableAction objtableAction = new RapidVerifoneNAXML.tableAction();
                    objtableAction.type = result;
                    objItem[0].TableAction = objtableAction;

                    RapidVerifoneNAXML.recordActionType objRecordType = new RapidVerifoneNAXML.recordActionType();
                    objRecordType = RapidVerifoneNAXML.recordActionType.addchange;

                    RapidVerifoneNAXML.recordAction objrecordAction = new RapidVerifoneNAXML.recordAction();
                    objrecordAction.type = objRecordType;

                    objItem[0].RecordAction = objrecordAction;



                    if (uniqueCols != null && uniqueCols.Rows.Count > 0)
                    {
                        RapidVerifoneNAXML.ILTDetailType[] objILTDetail = new RapidVerifoneNAXML.ILTDetailType[uniqueCols.Rows.Count];

                        for (int i = 0; i < uniqueCols.Rows.Count; i++)
                        {
                            try
                            {
                                ItemListID = Convert.ToString(uniqueCols.Rows[i]["ItemListID"]);
                                objILTDetail[i] = new RapidVerifoneNAXML.ILTDetailType();


                                RapidVerifoneNAXML.recordActionType objRecordTypechild = new RapidVerifoneNAXML.recordActionType();
                                objRecordType = RapidVerifoneNAXML.recordActionType.delete;

                                recordAction objrecordActionChild = new recordAction();
                                objrecordActionChild.type = objRecordType;

                                objILTDetail[i].RecordAction = objrecordActionChild;

                                objILTDetail[i].RecordAction = objrecordActionChild;

                                #region ItemListID
                                objILTDetail[i].ItemListID = Convert.ToString(uniqueCols.Rows[i]["ItemListID"]);
                                #endregion

                                #region ItemListDescription
                                objILTDetail[i].ItemListDescription = Convert.ToString(uniqueCols.Rows[i]["ItemName"]);
                                #endregion

                                #region ItemListEntry
                                DataView dvItem = dtItem.DefaultView;
                                dvItem.RowFilter = ("[" + dtItem.Columns["ItemListID"].ColumnName + "] = " + Convert.ToString(uniqueCols.Rows[i]["ItemListID"]) + "");
                                RapidVerifoneNAXML.ItemListEntry[] objItemList = new RapidVerifoneNAXML.ItemListEntry[dvItem.Count];

                                if (dvItem != null)
                                {
                                    if (dvItem.Count > 0)
                                    {
                                        for (int j = 0; j < dvItem.Count; j++)
                                        {
                                            objItemList[j] = new RapidVerifoneNAXML.ItemListEntry();

                                            RapidVerifoneNAXML.ItemCode objItemCode = new RapidVerifoneNAXML.ItemCode();
                                            RapidVerifoneNAXML.POSCodeFormat objPOS = new RapidVerifoneNAXML.POSCodeFormat();
                                            objPOS.format = "plu";
                                            objItemCode.POSCodeFormat = objPOS;
                                            objItemCode.POSCode = Convert.ToString((string)dvItem[j]["Barcode"]);

                                            RapidVerifoneNAXML.POSCodeModifier objModifier = new RapidVerifoneNAXML.POSCodeModifier();
                                            objModifier.Value = Convert.ToString((string)dvItem[j]["ITEM_ShortName"]);

                                            objItemCode.POSCodeModifier = objModifier;

                                            objItemList[j].ItemCode = objItemCode;
                                        }
                                        objILTDetail[i].ItemListEntry = objItemList;
                                    }
                                }

                                dvItem.RowFilter = string.Empty;
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteMixMatchItem()", "DeleteMixMatchItem Exception : " + ex, "MixMatch_ItemList", "uMaintenance");
                                MixMatch_ItemListID += ItemListID + ",";
                            }
                        }

                        objItem[0].ILTDetail = objILTDetail;
                    }


                    objNAXMLConfig_Item.ItemListMaintenance = objItem;
                    objNAXMLConfig_Item.version = ConfigurationManager.AppSettings["MixMatchVersion"];

                    #region object to xml
                    string xml = objComman.GetXMLFromObject_standalone(objNAXMLConfig_Item);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(sb, settings);
                    doc.WriteTo(writer);
                    writer.Close();
                    string t = "xmlns=\"" + "" + "\"";
                    sb = sb.Replace(t, "");
                    ItemXML = sb.ToString();
                    #endregion
                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(ItemXML, "DeleteMixMatch_ItemList", "POST", "uMaintenance");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "DeleteMixMatchItem()", "Verifone MixMatch_Item updated successfully", "MixMatch_ItemList", "uMaintenance");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "DeleteMixMatchItem()", "No Response", "MixMatch_ItemList", "uMaintenance");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "DeleteMixMatchItem()", "Not any MixMatch_Item is pending to update", "MixMatch_ItemList", "uMaintenance");
                }

                #region entry of error item data
                if (MixMatch_ItemListID != "")
                {
                    objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteMixMatchItem()", "Error MixMatch_ItemList : " + MixMatch_ItemListID, "MixMatch_ItemList", "uMaintenance");
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteMixMatchItem()", "DeleteMixMatchItem Exception 2 : " + ex, "MixMatch_ItemList", "uMaintenance");
            }
        }

        public void DeleteMixMatch(DataTable dt, DataTable dtMixMatchEntry)
        {
            _CVerifone objCVerifone = new _CVerifone();
            StringBuilder sb = new StringBuilder();
            string MixMatchXML = "";
            string arraysysid = "";
            try
            {
                objCVerifone.InsertActiveLog("Verifone", "Start", "DeleteMixMatch()", "Initialize DeleteMixMatch", "MixMatch", "uMaintenance");

                #region filter of error MixMatch_Item
                if (MixMatch_ItemListID != "")
                {
                    MixMatch_ItemListID = MixMatch_ItemListID.TrimEnd(',');
                    DataView dvNonErrorItems = dt.DefaultView;
                    dvNonErrorItems.RowFilter = ("[" + dt.Columns["ItemListID"].ColumnName + "] not in (" + MixMatch_ItemListID + ")");
                    dt = dvNonErrorItems.ToTable();

                    dvNonErrorItems.RowFilter = string.Empty;
                }
                #endregion


                if (dt != null && dt.Rows.Count > 0)
                {
                    RapidVerifoneNAXML.NAXMLMaintenanceRequest objNAXMLConfig_Item = new RapidVerifoneNAXML.NAXMLMaintenanceRequest();

                    #region TransmissionHeader
                    RapidVerifoneNAXML.transmissionHeader objtransmissionHeader = new RapidVerifoneNAXML.transmissionHeader();
                    objtransmissionHeader.StoreLocationID = ConfigurationManager.AppSettings["StoreLocationID"];
                    objtransmissionHeader.VendorName = ConfigurationManager.AppSettings["VendorName"];
                    objtransmissionHeader.VendorModelVersion = ConfigurationManager.AppSettings["VendorModelVersion"];
                    objNAXMLConfig_Item.TransmissionHeader = objtransmissionHeader;
                    #endregion

                    RapidVerifoneNAXML.MixMatchMaintenance[] objMixMatch = new RapidVerifoneNAXML.MixMatchMaintenance[1];
                    objMixMatch[0] = new RapidVerifoneNAXML.MixMatchMaintenance();

                    RapidVerifoneNAXML.tableActionType result = new RapidVerifoneNAXML.tableActionType();
                    result = RapidVerifoneNAXML.tableActionType.update;

                    RapidVerifoneNAXML.tableAction objtableAction = new RapidVerifoneNAXML.tableAction();
                    objtableAction.type = result;
                    objMixMatch[0].TableAction = objtableAction;

                    RapidVerifoneNAXML.recordActionType objRecordType = new RapidVerifoneNAXML.recordActionType();
                    objRecordType = RapidVerifoneNAXML.recordActionType.addchange;

                    RapidVerifoneNAXML.recordAction objrecordAction = new RapidVerifoneNAXML.recordAction();
                    objrecordAction.type = objRecordType;

                    objMixMatch[0].RecordAction = objrecordAction;

                    RapidVerifoneNAXML.MMTDetailType[] objMMTDetail = new RapidVerifoneNAXML.MMTDetailType[dt.Rows.Count];

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        objMMTDetail[i] = new RapidVerifoneNAXML.MMTDetailType();

                        RapidVerifoneNAXML.recordActionType objRecordTypechild = new RapidVerifoneNAXML.recordActionType();
                        objRecordType = RapidVerifoneNAXML.recordActionType.delete;

                        recordAction objrecordActionChild = new recordAction();
                        objrecordActionChild.type = objRecordType;

                        objMMTDetail[i].RecordAction = objrecordActionChild;
                        #region PromotionID
                        RapidVerifoneNAXML.Promotion[] objPromotion = new RapidVerifoneNAXML.Promotion[1];
                        objPromotion[0] = new Promotion();
                        RapidVerifoneNAXML.PromotionID objPromotionID = new RapidVerifoneNAXML.PromotionID();
                        objPromotionID.Value = Convert.ToString(dt.Rows[i]["Name"]);
                        objPromotion[0].PromotionID = objPromotionID;
                        objMMTDetail[i].Promotion = objPromotion;
                        arraysysid += Convert.ToString(dt.Rows[i]["DiscountId"]) + ",";
                        #endregion

                        #region MixMatchDescription
                        objMMTDetail[i].MixMatchDescription = Convert.ToString(dt.Rows[i]["Description"]);
                        #endregion

                        #region MixMatchStrictHighFlag & MixMatchStrictLowFlag
                        flagDefYes objflagDefYes = new flagDefYes();
                        yesNo objyesNo = new yesNo();
                        objyesNo = yesNo.yes;
                        objflagDefYes.value = objyesNo;
                        objMMTDetail[i].MixMatchStrictHighFlag = objflagDefYes;
                        objMMTDetail[i].MixMatchStrictLowFlag = objflagDefYes;
                        #endregion

                        #region ItemListID
                        string[] arrItemListID = new string[1];
                        arrItemListID[0] = Convert.ToString(dt.Rows[i]["ItemListID"]);
                        objMMTDetail[i].ItemListID = arrItemListID;
                        #endregion

                        #region Start Date & Time - Stop Date & Time
                        objMMTDetail[i].StartDate = Convert.ToDateTime(dt.Rows[i]["StartDate"]);
                        objMMTDetail[i].StopDate = Convert.ToDateTime(dt.Rows[i]["EndDate"]);

                        DateTime StartTime = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                        objMMTDetail[i].StartTime = StartTime;

                        DateTime StopTime = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                        objMMTDetail[i].StopTime = StopTime;
                        #endregion

                        #region WeekdayAvailability declaration
                        RapidVerifoneNAXML.WeekdayAvailability[] objWeekdayAvailability = new RapidVerifoneNAXML.WeekdayAvailability[7];
                        for (int k = 0; k < 7; k++)
                        {
                            objWeekdayAvailability[k] = new RapidVerifoneNAXML.WeekdayAvailability();
                            if (k == 0) // sunday
                            {
                                objyesNo = Convert.ToBoolean(dt.Rows[i]["isSun"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
                                objWeekdayAvailability[k].available = objyesNo;  // available

                                RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
                                objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Sunday;
                                objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

                                DateTime StartTime_Sun = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sun_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].startTime = StartTime_Sun; // startTime

                                DateTime StopTime_Sun = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sun_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].stopTime = StopTime_Sun; // stopTime
                            }

                            if (k == 1) // monday
                            {
                                objyesNo = Convert.ToBoolean(dt.Rows[i]["isMon"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
                                objWeekdayAvailability[k].available = objyesNo;  // available

                                RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
                                objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Monday;
                                objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

                                DateTime StartTime_Mon = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Mon_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].startTime = StartTime_Mon; // startTime

                                DateTime StopTime_Mon = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Mon_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].stopTime = StopTime_Mon; // stopTime
                            }

                            if (k == 2) // tuesday
                            {
                                objyesNo = Convert.ToBoolean(dt.Rows[i]["isTue"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
                                objWeekdayAvailability[k].available = objyesNo;  // available

                                RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
                                objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Tuesday;
                                objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

                                DateTime StartTime_Tue = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Tue_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].startTime = StartTime_Tue; // startTime

                                DateTime StopTime_Tue = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Tue_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].stopTime = StopTime_Tue; // stopTime
                            }

                            if (k == 3) // wed
                            {
                                objWeekdayAvailability[k] = new RapidVerifoneNAXML.WeekdayAvailability();
                                objyesNo = Convert.ToBoolean(dt.Rows[i]["isWed"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
                                objWeekdayAvailability[k].available = objyesNo;  // available

                                RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
                                objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Wednesday;
                                objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

                                DateTime StartTime_Wed = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Wed_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].startTime = StartTime_Wed; // startTime

                                DateTime StopTime_Wed = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Wed_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].stopTime = StopTime_Wed; // stopTime
                            }

                            if (k == 4) // thurs
                            {
                                objyesNo = Convert.ToBoolean(dt.Rows[i]["isThurs"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
                                objWeekdayAvailability[k].available = objyesNo;  // available

                                RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
                                objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Thursday;
                                objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

                                DateTime StartTime_Thurs = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Thurs_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].startTime = StartTime_Thurs; // startTime

                                DateTime StopTime_Thurs = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Thurs_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].stopTime = StopTime_Thurs; // stopTime
                            }

                            if (k == 5) // friday
                            {
                                objyesNo = Convert.ToBoolean(dt.Rows[i]["isFri"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
                                objWeekdayAvailability[k].available = objyesNo;  // available

                                RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
                                objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Friday;
                                objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

                                DateTime StartTime_Fri = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Fri_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].startTime = StartTime_Fri; // startTime

                                DateTime StopTime_Fri = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Fri_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].stopTime = StopTime_Fri; // stopTime
                            }

                            if (k == 6) // sat
                            {
                                objyesNo = Convert.ToBoolean(dt.Rows[i]["isSat"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
                                objWeekdayAvailability[k].available = objyesNo;  // available

                                RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
                                objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Saturday;
                                objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

                                DateTime StartTime_Sat = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sat_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].startTime = StartTime_Sat; // startTime

                                DateTime StopTime_Sat = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sat_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
                                objWeekdayAvailability[k].stopTime = StopTime_Sat; // stopTime
                            }
                        }
                        objMMTDetail[i].WeekdayAvailability = objWeekdayAvailability;
                        #endregion

                        #region MixMatchEntry
                        DataTable dtMMEntry = new DataTable();
                        DataView dv = new DataView();
                        dv = dtMixMatchEntry.DefaultView;
                        dv.RowFilter = ("[" + dtMixMatchEntry.Columns["Name"].ColumnName + "] = " + Convert.ToString(dt.Rows[i]["Name"]) + "");
                        dtMMEntry = dv.ToTable();
                        dv.RowFilter = string.Empty;


                        if (dtMMEntry != null && dtMMEntry.Rows.Count > 0)
                        {
                            RapidVerifoneNAXML.MixMatchEntry[] objMixMatchEntry = new RapidVerifoneNAXML.MixMatchEntry[dtMMEntry.Rows.Count];
                            for (int n = 0; n < dtMMEntry.Rows.Count; n++)
                            {
                                objMixMatchEntry[n] = new RapidVerifoneNAXML.MixMatchEntry();
                                sellUnit objsellUnit = new sellUnit();
                                //objsellUnit.Value = Convert.ToDecimal(dt.Rows[i]["PrimaryItemQty"]);
                                objsellUnit.Value = Convert.ToDecimal(dtMMEntry.Rows[n]["PrimaryItemQty"]);
                                objMixMatchEntry[n].MixMatchUnits = objsellUnit;

                                RapidVerifoneNAXML.ItemChoiceType objItemChoiceType = new RapidVerifoneNAXML.ItemChoiceType();
                                objItemChoiceType = Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "1" ? RapidVerifoneNAXML.ItemChoiceType.MixMatchDiscountAmount : Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "2" ? RapidVerifoneNAXML.ItemChoiceType.MixMatchDiscountPercent : RapidVerifoneNAXML.ItemChoiceType.MixMatchPrice;
                                objMixMatchEntry[n].ItemElementName = objItemChoiceType;

                                if (Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "1" || Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "4")
                                {
                                    amount12 objamount = new amount12();
                                    objamount.Value = Convert.ToDecimal(dtMMEntry.Rows[n]["Free"]);
                                    objMixMatchEntry[n].Item = objamount;
                                }
                                else
                                {
                                    objMixMatchEntry[n].Item = Convert.ToDecimal(dtMMEntry.Rows[n]["Free"]);
                                }
                            }

                            objMMTDetail[i].MixMatchEntry = objMixMatchEntry;
                        }

                        #endregion

                        #region Priority
                        objMMTDetail[i].Priority = "";
                        #endregion

                        #region Extension
                        vsmsNAXML.TaxableRebate objTaxableRebate = new vsmsNAXML.TaxableRebate();
                        vsmsNAXML.currencyAmt objcurrencyAmt = new vsmsNAXML.currencyAmt();
                        objcurrencyAmt.Value = Convert.ToDecimal(dt.Rows[i]["TaxAmount"]);
                        objTaxableRebate.Amount = objcurrencyAmt;

                        if (Convert.ToString(dt.Rows[i]["TaxId"]) != "")
                        {
                            string TaxId = Convert.ToString(dt.Rows[i]["TaxId"]);
                            TaxId = TaxId.Replace(" ", "");
                            string[] TaxIdSplit = TaxId.Split(',');


                            if (TaxIdSplit != null && TaxIdSplit.Length > 0)
                            {
                                vsmsNAXML.TaxableRebateTax[] objTaxableRebateTax = new vsmsNAXML.TaxableRebateTax[TaxIdSplit.Length];

                                for (int m = 0; m < TaxIdSplit.Length; m++)
                                {
                                    objTaxableRebateTax[m] = new vsmsNAXML.TaxableRebateTax();
                                    objTaxableRebateTax[m].sysid = TaxIdSplit[m];
                                }

                                objTaxableRebate.Tax = objTaxableRebateTax;
                            }

                        }

                        transmissionHeaderExtension objtransmissionHeaderExtension = new transmissionHeaderExtension();
                        objtransmissionHeaderExtension.Any = new[] { objTaxableRebate.AsXmlElement() };

                        objMMTDetail[i].Items = new object[] { objtransmissionHeaderExtension };
                        #endregion
                    }

                    objMixMatch[0].MMTDetail = objMMTDetail;
                    objNAXMLConfig_Item.MixMatchMaintenance = objMixMatch;
                    objNAXMLConfig_Item.version = ConfigurationManager.AppSettings["MixMatchVersion"];

                    #region object to xml
                    string xml = objComman.GetXMLFromObject_standalone(objNAXMLConfig_Item);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(sb, settings);
                    doc.WriteTo(writer);
                    writer.Close();
                    string t = "xmlns=\"" + "" + "\"";
                    sb = sb.Replace(t, "");
                    MixMatchXML = sb.ToString();
                    #endregion

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(MixMatchXML, "DeleteMixMatch", "POST", "uMaintenance");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long resultt = objCVerifone.DeleteVerifoneHistory("Promotion", arraysysid);
                        if (resultt == 0)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "DeleteMixMatch()", "Verifone MixMatch Data not updated ", "MixMatch", "uMaintenance");
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("Verifone", "End", "DeleteMixMatch()", "Verifone MixMatch Data updated successfully", "MixMatch", "uMaintenance");
                        }
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "DeleteMixMatch()", "No Response", "MixMatch", "uMaintenance");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "DeleteMixMatch()", "Not any MixMatch data is pending to update", "MixMatch", "uMaintenance");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteMixMatch()", "DeleteMixMatch Exception : " + ex, "Update MixMatch", "uMaintenance");
            }
        }

        #endregion

    }
}
