//using VerifoneLibrary;
using VerifoneLibrary.DataAccess;
using RapidVerifone;
//using myCompany1111;
using RapidVarifone;
//using RapidVerifoneFuelconfig;
using RapidVerifoneNAXML;

using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Xml;

using System.Xml.Serialization;
using System.IO;
//using System.Xml.Linq;

using System.Globalization;
using System.Configuration;

namespace VerifoneServices
{
    public class VerifoneUpdate
    {
        Comman objComman = new Comman();
        public string MixMatch_ItemListID = "";
        public bool ItemListID_isError = false;
        public string New_VP = "";

        public void UpdateVerifoneTax(DataTable dt)
        {
            _CVerifone objCVerifone = new _CVerifone();
            DataTable dtUpdate = new DataTable();
            string arraysysid = "";
            string sysidxml = "";
            try
            {

                taxRateConfig objtaxRateConfig = new taxRateConfig();

                if (dt != null && dt.Rows.Count > 0)
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Tax.xml";
                    XmlDocument docxmlPath = new XmlDocument();
                    docxmlPath.Load(path);
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneTax()", "Initialize to Update", "Tax", "utaxratecfg");

                    string TaxPayloadXML = "";
                    StringBuilder sb = new StringBuilder();
                    RapidVerifone.taxRate[] objtaxRate = new RapidVerifone.taxRate[dt.Rows.Count];

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string dr1 = dt.Rows[i][1].ToString();
                        objtaxRate[i] = new RapidVerifone.taxRate();
                        XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                        if (nodesite != null)
                        {
                            objtaxRateConfig.site = nodesite.InnerText;
                        }
                        else
                        {
                            objtaxRateConfig.site = "";

                        }

                        objtaxRate[i].sysid = Convert.ToString(dt.Rows[i][1]);
                        objtaxRate[i].name = Convert.ToString(dt.Rows[i][3]);

                        #region taxRateTaxProperties
                        taxRateTaxProperties objtaxRateTaxProperties = new taxRateTaxProperties();

                        objtaxRateTaxProperties.rate = Convert.ToDecimal(dt.Rows[i][4]);

                        #region insert update optional values
                        if (Convert.ToString(dt.Rows[i]["CommandType"]) == "Update")
                        {
                            sysidxml = Convert.ToString(dt.Rows[i][1]);
                            XmlNode node = docxmlPath.DocumentElement.SelectSingleNode("//taxRates//taxRate[@sysid='" + sysidxml + "']");
                            if (node != null)
                            {
                                try
                                {
                                    var serializer = new XmlSerializer(typeof(RapidVerifone.taxRate));
                                    RapidVerifone.taxRate objtaxRate1 = new RapidVerifone.taxRate();

                                    using (TextReader reader = new StringReader(node.OuterXml))
                                    {
                                        objtaxRate1 = (RapidVerifone.taxRate)serializer.Deserialize(reader);
                                    }
                                    if (objtaxRate1 != null)
                                    {
                                        //objtaxRateConfig.site = Convert.ToString(dt.Rows[j][9]);
                                        objtaxRate[i].indicator = Convert.ToString(objtaxRate1.indicator);  //2
                                        objtaxRate[i].isPriceIncsTax = Convert.ToBoolean(objtaxRate1.isPriceIncsTax);  //3
                                        objtaxRate[i].isPromptExemption = Convert.ToBoolean(objtaxRate1.isPromptExemption);  //4
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
                                    objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneTax()", "UpdateVerifoneTax Exception : " + ex, "Tax", "utaxratecfg");
                                }
                            }
                        }
                        else
                        {
                            // insert
                            try
                            {
                                objtaxRate[i].indicator = "T";
                                objtaxRate[i].isPriceIncsTax = Convert.ToBoolean(0);
                                objtaxRate[i].isPromptExemption = Convert.ToBoolean(0);
                                objtaxRateTaxProperties.pctStartAmt = Convert.ToDecimal(0.00);
                                objtaxRateTaxProperties.pctStartAmtSpecified = true;
                                objtaxRateTaxProperties.rateSpecified = true;

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
                                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneTax()", "UpdateVerifoneTax Exception 2 : " + ex, "Tax", "utaxratecfg");
                            }
                        }
                        #endregion
                        objtaxRate[i].taxProperties = objtaxRateTaxProperties;
                        #endregion

                        objtaxRateConfig.taxRates = objtaxRate;
                    }

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
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneTax()", "Verifone Data not updated ", "Tax", "utaxratecfg");
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneTax()", "Verifone Data updated successfully", "Tax", "utaxratecfg");
                        }
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneTax()", "No Response", "Tax", "utaxratecfg");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneTax()", "Not any Tax data is pending to update", "Tax", "utaxratecfg");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneTax()", "UpdateVerifoneTax Exception 3 : " + ex, "Tax", "utaxratecfg");
            }
        }

        public void UpdateVerifonePayment(DataTable dtpayment)
        {
            _CVerifone objCVerifone = new _CVerifone();
            string sysidxml = "";
            string arraysysid = "";
            try
            {

                paymentConfig objpaymentConfig = new paymentConfig();


                if (dtpayment != null && dtpayment.Rows.Count > 0)
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Payment.xml";
                    XmlDocument docxmlPath = new XmlDocument();
                    docxmlPath.Load(path);
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifonePayment()", "Initialize to update", "Payment", "upaymentcfg");

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

                    paymentConfigMop[] objpaymentConfigMop = new paymentConfigMop[dtpayment.Rows.Count];

                    for (int i = 0; i < dtpayment.Rows.Count; i++)
                    {
                        if (Convert.ToString(dtpayment.Rows[i]["CommandType"]) == "Update")
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
                                        objpaymentConfigMop[i] = new paymentConfigMop();
                                        objpaymentConfigMop[i].sysid = Convert.ToString(dtpayment.Rows[i][1]);
                                        objpaymentConfigMop[i].name = Convert.ToString(dtpayment.Rows[i][3]);
                                        objpaymentConfigMop[i].code = objpaymentConfigMopGet.code;
                                        objpaymentConfigMop[i].min = objpaymentConfigMopGet.min;
                                        objpaymentConfigMop[i].max = objpaymentConfigMopGet.max;
                                        objpaymentConfigMop[i].limit = objpaymentConfigMopGet.limit;
                                        objpaymentConfigMop[i].isForceSafeDrop = objpaymentConfigMopGet.isForceSafeDrop;
                                        objpaymentConfigMop[i].isOpenDrwOnSale = objpaymentConfigMopGet.isOpenDrwOnSale;
                                        objpaymentConfigMop[i].isTenderAmtReqd = objpaymentConfigMopGet.isTenderAmtReqd;
                                        objpaymentConfigMop[i].isCashrRptPrompt = objpaymentConfigMopGet.isCashrRptPrompt;
                                        objpaymentConfigMop[i].isAllowZeroEntry = objpaymentConfigMopGet.isAllowZeroEntry;
                                        objpaymentConfigMop[i].isAllowWithoutSale = objpaymentConfigMopGet.isAllowWithoutSale;
                                        objpaymentConfigMop[i].isAllowRefund = objpaymentConfigMopGet.isAllowRefund;
                                        objpaymentConfigMop[i].isAllowChange = objpaymentConfigMopGet.isAllowChange;
                                        objpaymentConfigMop[i].isAllowSafeDrop = objpaymentConfigMopGet.isAllowSafeDrop;
                                        objpaymentConfigMop[i].isAllowMOPurch = objpaymentConfigMopGet.isAllowMOPurch;
                                        objpaymentConfigMop[i].isForceTicketPrint = objpaymentConfigMopGet.isForceTicketPrint;
                                        objpaymentConfigMop[i].numReceiptCopies = objpaymentConfigMopGet.numReceiptCopies;
                                        objpaymentConfigMop[i].nacstendercode = objpaymentConfigMopGet.nacstendercode;
                                        objpaymentConfigMop[i].nacstendersubcode = objpaymentConfigMopGet.nacstendersubcode;

                                        objpaymentConfig.mops = objpaymentConfigMop;

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
                                    objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifonePayment()", "UpdateVerifonePayment Exception : " + Convert.ToString(ex), "Payment", "upaymentcfg");
                                }
                            }
                        }
                        else
                        {
                            objpaymentConfigMop[i] = new paymentConfigMop();
                            objpaymentConfigMop[i].sysid = Convert.ToString(dtpayment.Rows[i][1]);
                            objpaymentConfigMop[i].name = Convert.ToString(dtpayment.Rows[i][3]);
                            objpaymentConfigMop[i].code = "";
                            objpaymentConfigMop[i].min = 0;
                            objpaymentConfigMop[i].max = 0;
                            objpaymentConfigMop[i].limit = 0;
                            objpaymentConfigMop[i].isForceSafeDrop = false;
                            objpaymentConfigMop[i].isOpenDrwOnSale = false;
                            objpaymentConfigMop[i].isTenderAmtReqd = false;
                            objpaymentConfigMop[i].isCashrRptPrompt = false;
                            objpaymentConfigMop[i].isAllowZeroEntry = false;
                            objpaymentConfigMop[i].isAllowWithoutSale = false;
                            objpaymentConfigMop[i].isAllowRefund = false;
                            objpaymentConfigMop[i].isAllowChange = false;
                            objpaymentConfigMop[i].isAllowSafeDrop = false;
                            objpaymentConfigMop[i].isAllowMOPurch = false;
                            objpaymentConfigMop[i].isForceTicketPrint = false;
                            objpaymentConfigMop[i].numReceiptCopies = 1;
                            objpaymentConfigMop[i].nacstendercode = "";
                            objpaymentConfigMop[i].nacstendersubcode = "";

                            objpaymentConfig.mops = objpaymentConfigMop;

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
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifonePayment()", "Verifone Data not updated", "Payment", "upaymentcfg");
                        }
                        else
                        {
                            // ******** msg of success
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifonePayment()", "Verifone Data updated successfully", "Payment", "upaymentcfg");
                        }
                    }
                    else
                    {
                        // ******** msg of no response
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifonePayment()", "No Response", "Payment", "upaymentcfg");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifonePayment()", "Not any Payment data is pending to update", "Payment", "upaymentcfg");
                }

            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifonePayment()", "UpdateVerifonePayment Exception 2 : " + Convert.ToString(ex), "Payment", "upaymentcfg");
            }
        }

        public void UpdateVerifoneCategory(DataTable dtCat)
        {
            string arraysysid = "";
            string sysidxml = "";
            _CVerifone objCVerifone = new _CVerifone();
            try
            {

                posConfig objposConfig = new posConfig();

                if (dtCat != null && dtCat.Rows.Count > 0)
                {
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneCategory()", "Initialize to update", "Category", "uposcfg");

                    string CategoryPayloadXML = "";
                    StringBuilder sb = new StringBuilder();

                    objposConfig.site = "";

                    category[] objCategory = new category[dtCat.Rows.Count];

                    for (int i = 0; i < dtCat.Rows.Count; i++)
                    {
                        try
                        {
                            objCategory[i] = new category();
                            sysidxml = Convert.ToString(dtCat.Rows[i][0]);
                            objCategory[i].sysid = Convert.ToString(dtCat.Rows[i][0]);
                            objCategory[i].name = Convert.ToString(dtCat.Rows[i][1]);
                            objposConfig.categories = objCategory;

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
                            objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneCategory()", "UpdateVerifoneCategory Exception : " + Convert.ToString(ex), "Category", "uposcfg");
                        }
                    }

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
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneCategory()", "Verifone data not updated", "Category", "uposcfg");
                        }
                        else
                        {
                            // ******** msg of success
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneCategory()", "Verifone data updated successfully", "Category", "uposcfg");
                        }
                    }
                    else
                    {
                        // ******** msg of no response
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneCategory()", "No Response", "Category", "uposcfg");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneCategory()", "Not any Category data is pending to update", "Category", "uposcfg");
                }
            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneCategory()", "UpdateVerifoneCategory Exception 2 : " + Convert.ToString(ex), "Category", "uposcfg");
            }
        }

        public void UpdateVerifoneProductCode(DataTable dt)
        {
            string arraysysid = "";
            string sysidxml = "";
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                //DataSet ds = new DataSet();
                posConfig objposConfig = new posConfig();
                //ds = objCVerifone.GetVerifoneCategoryData();

                if (dt.Rows.Count > 0)
                {
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneProductCode()", "Initialize to update", "ProductCode", "uposcfg");

                    string ProductCodePayloadXML = "";
                    StringBuilder sb = new StringBuilder();

                    objposConfig.site = "";

                    prodCode[] objprodCode = new prodCode[dt.Rows.Count];

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            objprodCode[i] = new prodCode();
                            sysidxml = Convert.ToString(dt.Rows[i][1]);
                            objprodCode[i].sysid = Convert.ToString(dt.Rows[i][1]);
                            objprodCode[i].name = Convert.ToString(dt.Rows[i][2]);
                            objposConfig.prodCodes = objprodCode;

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
                        }
                    }

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
                    ProductCodePayloadXML = sb.ToString();

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(ProductCodePayloadXML, "UpdateProdCode", "POST", "uposcfg");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long result = objCVerifone.DeleteVerifoneHistory("ProdCode", arraysysid);
                        if (result == 0)
                        {
                            // ******** msg of fail
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneProductCode()", "Verifone data not updated", "ProdCode", "uposcfg");
                        }
                        else
                        {
                            //lblResult.Text = "UpdateCategory : Success";
                            // ******** msg of success
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneProductCode()", "Verifone data updated successfully", "ProdCode", "uposcfg");
                        }
                    }
                    else
                    {
                        //lblResult.Text = "UpdateCategory : No Reponse";
                        // ******** msg of no response
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneProductCode()", "No Response", "ProdCode", "uposcfg");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneProductCode()", "Not any ProdCode data is pending to update", "ProdCode", "uposcfg");
                }
            }
            catch (Exception ex)
            {
                //lblResult.Text = "UpdateCategory : " + ex;
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneProductCode()", "UpdateVerifoneCategory Exception : " + Convert.ToString(ex), "ProdCode", "uposcfg");
            }
        }

        public void UpdateVerifoneDepartment(DataTable dt, DataTable dttax)
        {
            _CVerifone objCVerifone = new _CVerifone();
            string sysidxml = "";
            string arraysysid = "";
            try
            {
                objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneDepartment()", "Initialize to update", "Department", "uposcfg");

                posConfig objposConfig = new posConfig();

                if (dt != null && dt.Rows.Count > 0)
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Department.xml";
                    XmlDocument docxmlPath = new XmlDocument();
                    docxmlPath.Load(path);

                    string DepartmentPayloadXML = "";
                    StringBuilder sb = new StringBuilder();

                    XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                    if (nodesite != null)
                    {
                        objposConfig.site = nodesite.InnerText;
                    }
                    else
                    {
                        objposConfig.site = "";
                    }

                    department[] objdepartment = new department[dt.Rows.Count];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        departmentCategory objdeptCat = new departmentCategory();
                        departmentProdCode objdepartmentProdCode = new departmentProdCode();

                        if (Convert.ToString(dt.Rows[i]["CommandType"]) == "Update")
                        {
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
                                        objdepartment[i] = new department();
                                        objdepartment[i].sysid = Convert.ToString(dt.Rows[i][0]);
                                        objdepartment[i].name = Convert.ToString(dt.Rows[i][1]);
                                        objdepartment[i].minAmt = objDepartmentGet.minAmt;
                                        objdepartment[i].maxAmt = objDepartmentGet.maxAmt;
                                        objdepartment[i].isAllowFS = objDepartmentGet.isAllowFS;
                                        objdepartment[i].isNegative = objDepartmentGet.isNegative;
                                        objdepartment[i].isFuel = Convert.ToBoolean(dt.Rows[i][2]);
                                        objdepartment[i].isAllowFQ = objDepartmentGet.isAllowFQ;
                                        objdepartment[i].isAllowSD = objDepartmentGet.isAllowSD;
                                        objdepartment[i].prohibitDisc = objDepartmentGet.prohibitDisc;  //
                                        objdepartment[i].prohibitDiscSpecified = objDepartmentGet.prohibitDiscSpecified;  //
                                        objdepartment[i].isBL1 = objDepartmentGet.isBL1; //
                                        objdepartment[i].isBL1Specified = objDepartmentGet.isBL1Specified; //
                                        objdepartment[i].isBL2 = objDepartmentGet.isBL2; //
                                        objdepartment[i].isBL2Specified = objDepartmentGet.isBL2Specified; //
                                        objdepartment[i].isMoneyOrder = Convert.ToBoolean(dt.Rows[i][3]);
                                        objdepartment[i].isSNPromptReqd = objDepartmentGet.isSNPromptReqd; //
                                        objdepartment[i].isSNPromptReqdSpecified = objDepartmentGet.isSNPromptReqdSpecified; //
                                        objdeptCat.sysid = Convert.ToString(dt.Rows[i][4]);
                                        objdepartment[i].category = objdeptCat;

                                        objdepartmentProdCode.sysid = Convert.ToString(dt.Rows[i][5]) == "" ? "0" : Convert.ToString(dt.Rows[i][5]);
                                        objdepartment[i].prodCode = objdepartmentProdCode;

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

                                                objdepartment[i].taxes = objdepartmentTax;
                                            
                                        }

                                        objposConfig.departments = objdepartment;
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
                                    objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneDepartment()", "UpdateVerifoneDepartment Exception : " + Convert.ToString(ex), "Department", "uposcfg");
                                }
                            }
                        }
                        else
                        {
                            objdepartment[i] = new department();
                            objdepartment[i].sysid = Convert.ToString(dt.Rows[i][0]);
                            objdepartment[i].name = Convert.ToString(dt.Rows[i][1]);
                            objdepartment[i].minAmt = 0;
                            objdepartment[i].maxAmt = 0;
                            objdepartment[i].isAllowFS = false;
                            objdepartment[i].isNegative = false;
                            objdepartment[i].isFuel = false;
                            objdepartment[i].isAllowFQ = false;
                            objdepartment[i].isAllowSD = false;
                            objdepartment[i].prohibitDisc = false;
                            objdepartment[i].prohibitDiscSpecified = false;
                            objdepartment[i].isBL1 = false;
                            objdepartment[i].isBL1Specified = false;
                            objdepartment[i].isBL2 = false;
                            objdepartment[i].isBL2Specified = false;
                            objdepartment[i].isMoneyOrder = false;
                            objdepartment[i].isSNPromptReqd = false;
                            objdepartment[i].isSNPromptReqdSpecified = false;

                            objdeptCat.sysid = Convert.ToString(dt.Rows[i][4]);
                            objdepartment[i].category = objdeptCat;

                            objdepartmentProdCode.sysid = Convert.ToString(dt.Rows[i][5]) == "" ? "0" : Convert.ToString(dt.Rows[i][5]);
                            objdepartment[i].prodCode = objdepartmentProdCode;

                            DataView dv = dttax.DefaultView;
                            DataTable dttaxdept = new DataTable();
                            dv.RowFilter = ("[" + dttax.Columns[0].ColumnName + "] = " + Convert.ToString(dt.Rows[i][0]) + "");
                            dttaxdept = dv.ToTable();


                          
                                if (dttaxdept != null && dttaxdept.Rows.Count > 0)
                                {
                                    departmentTax[] objdepartmentTax = new departmentTax[dttaxdept.Rows.Count];

                                    for (int a = 0; a < dttaxdept.Rows.Count; a++)
                                    {
                                        objdepartmentTax[a] = new departmentTax();
                                        objdepartmentTax[a].sysid = Convert.ToString(dttaxdept.Rows[a][1]);
                                    }
                                    objdepartment[i].taxes = objdepartmentTax;
                                }
                            

                            objposConfig.departments = objdepartment;
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
                                objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneDepartment()", "Verifone data not updated", "Department", "uposcfg");
                            }
                            else
                            {
                                // ******** msg of success
                                objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneDepartment()", "Verifone data updated successfully", "Department", "uposcfg");
                            }
                        }
                        else
                        {
                            // ******** msg of no response
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneDepartment()", "No Response", "Department", "uposcfg");
                        }
                        #endregion
                    }
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneDepartment()", "Not any Department data is pending to update", "Department", "uposcfg");
                }
            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneDepartment()", "UpdateVerifoneDepartment Exception 2 : " + Convert.ToString(ex), "Department", "uposcfg");
            }
        }

        public void UpdateVerifoneFees(DataTable dt, DataTable dt1)
        {
            _CVerifone objCVerifone = new _CVerifone();
            string sysidxml = "";
            string arraysysid = "";
            try
            {

                DataTable dtNew = new DataTable();
                feeConfig objfeeConfig = new feeConfig();

                if (dt != null && dt.Rows.Count > 0)
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Fee.xml";
                    XmlDocument docxmlPath = new XmlDocument();
                    docxmlPath.Load(path);
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneFees()", "Initialize to update", "Fee", "ufeecfg");

                    string FeePayloadXML = "";
                    StringBuilder sb = new StringBuilder();

                    XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                    if (nodesite != null)
                    {
                        objfeeConfig.site = nodesite.InnerText;
                    }
                    else
                    {
                        objfeeConfig.site = "";
                    }

                    fee[] objfee = new fee[dt.Rows.Count];

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Convert.ToString(dt.Rows[i]["CommandType"]) == "Update")
                        {
                            sysidxml = Convert.ToString(dt.Rows[i][0]);
                            XmlNode node = docxmlPath.DocumentElement.SelectSingleNode("//fees//fee[@sysid='" + sysidxml + "']");
                            if (node != null)
                            {
                                try
                                {
                                    var serializer = new XmlSerializer(typeof(RapidVerifone.fee));
                                    RapidVerifone.fee objFeeGet = new RapidVerifone.fee();

                                    using (TextReader reader = new StringReader(node.OuterXml))
                                    {
                                        objFeeGet = (RapidVerifone.fee)serializer.Deserialize(reader);
                                    }
                                    if (objFeeGet != null)
                                    {
                                        objfee[i] = new fee();
                                        objfee[i].sysid = Convert.ToString(dt.Rows[i][0]);
                                        objfee[i].name = Convert.ToString(dt.Rows[i][2]);
                                        objfee[i].dept = objFeeGet.dept;
                                        objfee[i].isRefundable = objFeeGet.isRefundable;
                                        objfee[i].feeIdentifier = objFeeGet.feeIdentifier;

                                        DataView dv = dt1.DefaultView;
                                        dv.RowFilter = ("[" + dt1.Columns[1].ColumnName + "] = " + objfee[i].sysid + "");
                                        dtNew = dv.ToTable();

                                        if (dtNew != null && dtNew.Rows.Count > 0)
                                        {
                                            rangeAmountFeeType[] objItems = new rangeAmountFeeType[dtNew.Rows.Count];

                                            for (int a = 0; a < dtNew.Rows.Count; a++)
                                            {
                                                objItems[a] = new rangeAmountFeeType();
                                                objItems[a].rangeFee = Convert.ToDecimal(dtNew.Rows[a][3]);
                                                objItems[a].rangeEnd = Convert.ToDecimal(dtNew.Rows[a][4]);
                                            }
                                            objfee[i].Items = objItems;
                                        }


                                        objfeeConfig.fees = objfee;

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
                                }
                            }
                        }
                        else
                        {
                            objfee[i] = new fee();
                            objfee[i].sysid = Convert.ToString(dt.Rows[i][0]);
                            objfee[i].name = Convert.ToString(dt.Rows[i][2]);
                            objfee[i].dept = "0";
                            objfee[i].isRefundable = false;
                            objfee[i].feeIdentifier = "";

                            DataView dv = dt1.DefaultView;
                            dv.RowFilter = ("[" + dt1.Columns[1].ColumnName + "] = " + objfee[i].sysid + "");
                            dtNew = dv.ToTable();


                            if (dtNew != null && dtNew.Rows.Count > 0)
                            {
                                rangeAmountFeeType[] objItems = new rangeAmountFeeType[dtNew.Rows.Count];

                                for (int a = 0; a < dtNew.Rows.Count; a++)
                                {
                                    objItems[a] = new rangeAmountFeeType();
                                    objItems[a].rangeFee = Convert.ToDecimal(dtNew.Rows[a][3]);
                                    objItems[a].rangeEnd = Convert.ToDecimal(dtNew.Rows[a][4]);
                                }
                                objfee[i].Items = objItems;
                            }

                            objfeeConfig.fees = objfee;

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

                    string xml = objComman.GetXMLFromObject(objfeeConfig);
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
                    FeePayloadXML = sb.ToString();

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(FeePayloadXML, "UpdateFees", "POST", "ufeecfg");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long result = objCVerifone.DeleteVerifoneHistory("Fee", arraysysid);
                        if (result == 0)
                        {
                            // ******** msg of fail
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneFees()", "Verifone data not updated", "Fee", "ufeecfg");
                        }
                        else
                        {
                            // ******** msg of success
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneFees()", "Verifone data updated successfully", "Fee", "ufeecfg");
                        }
                    }
                    else
                    {
                        // ******** msg of no response
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneFees()", "No Response", "Fee", "ufeecfg");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneFees()", "Not any Fees data is pending to update", "Fees", "ufeecfg");
                }
            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneFees()", "UpdateVerifoneFees Exception 2 : " + Convert.ToString(ex), "Fee", "ufeecfg");
            }
        }

        public void UpdateVerifoneItems(DataTable dtItem, DataTable dtitemTax, DataTable dtitemFee)
        {
            _CVerifone objCVerifone = new _CVerifone();
            try
            {

                PLUs objPLUs = new PLUs();

                if (dtItem != null && dtItem.Rows.Count > 0)
                {
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneItems()", "Initialize to update", "Items", "uPLUs");

                    string ItemPayloadXML = "";
                    StringBuilder sb = new StringBuilder();

                    PLUCType[] objPLUCType = new PLUCType[dtItem.Rows.Count];
                    taxRebateType objtaxRebateType = new taxRebateType();
                    RapidVarifone.currencyAmt objcurrencyAmt = new RapidVarifone.currencyAmt();

                    for (int i = 0; i < dtItem.Rows.Count; i++)
                    {
                        if (Convert.ToString(dtItem.Rows[i]["ItemCode"]) == "66")
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneItems()", "Initialize to update", "Items", "uPLUs");
                        }

                        objPLUCType[i] = new PLUCType();

                        PLUCTypeUpc objPLUCTypeUpc = new PLUCTypeUpc();

                        objPLUCTypeUpc.Value = Convert.ToString(dtItem.Rows[i]["Barcode"]);
                        objPLUCType[i].upc = objPLUCTypeUpc;

                        //objPLUCType[i].upcModifier = Convert.ToString(dt.Rows[i][12]) == "1" ? "000" : Convert.ToString(dt.Rows[i][12]) == "2" ? "001" : Convert.ToString(dt.Rows[i][12]) == "3" ? "002" : "000";
                        objPLUCType[i].upcModifier = Convert.ToString(dtItem.Rows[i]["ITEM_ShortName"]);
                        objPLUCType[i].description = Convert.ToString(dtItem.Rows[i]["ITEM_Desc"]);
                        objPLUCType[i].department = Convert.ToString(dtItem.Rows[i]["DeptId"]);

                        objPLUCType[i].pcode = Convert.ToString(dtItem.Rows[i]["ITEM_No"]);

                        PLUCTypePrice objPLUCTypePrice = new PLUCTypePrice();
                        objPLUCTypePrice.Value = Convert.ToDecimal(dtItem.Rows[i]["SalesPrice"]);
                        objPLUCType[i].price = objPLUCTypePrice;

                        //*******************************
                        DataView dvTax = dtitemTax.DefaultView;
                        if (dvTax != null && dvTax.Count > 0)
                        {
                            //dvTax.RowFilter = ("[" + dtitemTax.Columns["Barcode"].ColumnName + "] ='" + dtItem.Rows[i]["Barcode"] + "' and  [" + dtitemTax.Columns["ITEM_ShortName"].ColumnName + "] ='" + dtItem.Rows[i]["ITEM_ShortName"] + "' ");
                            dvTax.RowFilter = ("[" + dtitemTax.Columns["ItemCode"].ColumnName + "] ='" + dtItem.Rows[i]["ItemCode"] + "'");

                            DataTable dtNew = dvTax.ToTable();

                            if (dtNew != null && dtNew.Rows.Count > 0)
                            {
                                RapidVarifone.taxRate[] objtaxRate = new RapidVarifone.taxRate[dtNew.Rows.Count];

                                for (int j = 0; j < dtNew.Rows.Count; j++)
                                {
                                    objtaxRate[j] = new RapidVarifone.taxRate();
                                    objtaxRate[j].sysid = Convert.ToString(dtNew.Rows[j]["TAXId"]);
                                }
                                objPLUCType[i].taxRates = objtaxRate;
                            }

                            dvTax.RowFilter = string.Empty;
                        }

                        DataView dvFee = dtitemFee.DefaultView;
                        if (dvFee != null && dvFee.Count > 0)
                        {
                            //dvFee.RowFilter = ("[" + dtitemFee.Columns["Barcode"].ColumnName + "] ='" + dtItem.Rows[i]["Barcode"] + "' and [" + dtitemFee.Columns["ITEM_ShortName"].ColumnName + "] ='" + dtItem.Rows[i]["ITEM_ShortName"] + "' ");
                            dvFee.RowFilter = ("[" + dtitemFee.Columns["ItemCode"].ColumnName + "] ='" + dtItem.Rows[i]["ItemCode"] + "'");

                            DataTable dtNew = dvFee.ToTable();

                            if (dtNew != null && dtNew.Rows.Count > 0)
                            {
                                PLUCTypeFees objPLUCTypeFees = new PLUCTypeFees();

                                string[] arrFee = new string[dtNew.Rows.Count];

                                for (int j = 0; j < dtNew.Rows.Count; j++)
                                {
                                    arrFee[j] = Convert.ToString(dtNew.Rows[j]["FeeOrDepositId"]);
                                }
                                objPLUCTypeFees.fee = arrFee;
                                objPLUCType[i].Item = objPLUCTypeFees;
                            }

                            dvFee.RowFilter = string.Empty;
                        }

                        objPLUCType[i].SellUnit = Convert.ToString(dtItem.Rows[i]["Qty"]);

                        // objcurrencyAmt.Value = Convert.ToDecimal(dtItem.Rows[i][8]);
                        // reason only taxableRebate 0 this reason 
                        objcurrencyAmt.Value = 0;
                        objtaxRebateType.amount = objcurrencyAmt;
                        objPLUCType[i].taxableRebate = objtaxRebateType;

                        objPLUs.PLU = objPLUCType;
                        // }
                    }

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

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(ItemPayloadXML, "UpdateItems", "POST", "uPLUs");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long result = objCVerifone.DeleteVerifoneHistory("Items", "");
                        if (result == 0)
                        {
                            // ******** msg of fail
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneItems()", "Verifone data not updated", "Items", "uPLUs");
                        }
                        else
                        {
                            // ******** msg of success
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneItems()", "Verifone data updated successfully", "Items", "uPLUs");
                        }
                    }
                    else
                    {
                        // ******** msg of no response
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneItems()", "No Response", "Items", "uPLUs");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneItems()", "Not any Item data is pending to update", "Items", "uPLUs");
                }
            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneItems()", "UpdateVerifoneItems Exception 2 : " + Convert.ToString(ex), "Items", "uPLUs");
            }
        }

        public void UpdateVerifoneFuel(DataTable dtfuel, DataTable dtfuelPrice)
        {
            _CVerifone objCVerifone = new _CVerifone();
            string arraysysid = "";
            try
            {

                var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Fuel.xml";
                XmlDocument docxmlPath = new XmlDocument();
                docxmlPath.Load(path);

                #region Fuel Price  1
                fuelPrices objfuelPrices = new fuelPrices();
                #endregion


                if (dtfuel != null && dtfuel.Rows.Count > 0)
                {
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneFuel()", "Initialize to update", "Fuel", "ufuelprices");

                    string FuelPayloadXML = "";
                    StringBuilder sb = new StringBuilder();
                    RapidVerifone.site objsite = new RapidVerifone.site();

                    if (docxmlPath.DocumentElement.ChildNodes[0].LocalName == "site")
                    {
                        objsite.Value = docxmlPath.DocumentElement.ChildNodes[0].InnerText;
                        objfuelPrices.site = objsite;
                    }
                    else
                    {
                        objsite.Value = "";
                        objfuelPrices.site = objsite;
                    }

                    #region Fuel Price products  2
                    fuelPricesFuelProducts objfuelPricesFuelProducts = new fuelPricesFuelProducts();
                    objfuelPricesFuelProducts.maxSize = Convert.ToString(dtfuel.Rows.Count);
                    #endregion

                    fuelPricesFuelProductsFuelProduct[] objfuelPricesFuelProductsFuelProduct = new fuelPricesFuelProductsFuelProduct[dtfuel.Rows.Count];

                    for (int i = 0; i < dtfuel.Rows.Count; i++)
                    {
                        fuelPricesFuelProductsFuelProduct FuelProduct = new fuelPricesFuelProductsFuelProduct();

                        FuelProduct.sysid = Convert.ToString(dtfuel.Rows[i]["FuelTypeId"]);
                        FuelProduct.name = Convert.ToString(dtfuel.Rows[i]["FuelTypeLabelTitle"]);
                        FuelProduct.NAXMLFuelGradeID = Convert.ToString(dtfuel.Rows[i]["NAXMLFuelGradeID"]);

                        DataView dv = dtfuelPrice.DefaultView;
                        dv.RowFilter = ("[" + dtfuelPrice.Columns[0].ColumnName + "] =" + dtfuel.Rows[i]["FuelTypeId"] + "");
                        DataTable dtNew = dv.ToTable();


                        if (dtNew != null && dtNew.Rows.Count > 0)
                        {
                            fuelPricesFuelProductsFuelProductPrice[] objFuelPrice = new fuelPricesFuelProductsFuelProductPrice[dtNew.Rows.Count];
                            for (int j = 0; j < dtNew.Rows.Count; j++)
                            {
                                fuelPricesFuelProductsFuelProductPrice objfuelPricesFuelProductsFuelProductPrice = new fuelPricesFuelProductsFuelProductPrice();

                                objfuelPricesFuelProductsFuelProductPrice.tier = Convert.ToString(dtNew.Rows[j]["tier"]);
                                objfuelPricesFuelProductsFuelProductPrice.servLevel = Convert.ToString(dtNew.Rows[j]["ServiceType"]);
                                objfuelPricesFuelProductsFuelProductPrice.mop = Convert.ToString(dtNew.Rows[j]["PayId"]);
                                objfuelPricesFuelProductsFuelProductPrice.Value = Convert.ToDecimal(dtNew.Rows[j]["Price"]);
                                objFuelPrice[j] = objfuelPricesFuelProductsFuelProductPrice;
                            }
                            FuelProduct.prices = objFuelPrice;
                        }


                        objfuelPricesFuelProductsFuelProduct[i] = FuelProduct;

                        dv.RowFilter = string.Empty;
                    }

                    objfuelPricesFuelProducts.fuelProduct = objfuelPricesFuelProductsFuelProduct;

                    objfuelPrices.fuelProducts = objfuelPricesFuelProducts;

                    string xml = objComman.GetXMLFromObject(objfuelPrices);
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
                    FuelPayloadXML = sb.ToString();

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(FuelPayloadXML, "UpdateFuel", "POST", "ufuelprices");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long result = objCVerifone.DeleteVerifoneHistory("Fuel", arraysysid);
                        if (result == 0)
                        {
                            // ******** msg of fail
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneFuel()", "Verifone data not updated", "Fuel", "ufuelprices");
                        }
                        else
                        {
                            // ******** msg of success
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneFuel()", "Verifone data updated successfully", "Fuel", "ufuelprices");
                        }
                    }
                    else
                    {
                        // ******** msg of no response
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneFuel()", "No Response", "Fuel", "ufuelprices");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneFuel()", "Not any Fuel data is pending to update", "Fuel", "ufuelcfg");
                }
            }
            catch (Exception ex)
            {
                // ******** msg of exception
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneFuel()", "UpdateVerifoneFuel Exception : " + Convert.ToString(ex), "Fuel", "ufuelcfg");
            }
        }

        public void UpdateVerifoneUser(DataTable dtUser, DataTable dtUserRole)
        {
            _CVerifone objCVerifone = new _CVerifone();
            DataTable dt = new DataTable();
            DataTable dtUpdate = new DataTable();
            string arraysysid = "";
            string sysidxml = "";
            string UserRolePayloadXML = "";
            StringBuilder sb = new StringBuilder();
            string NewPassword = "";
            try
            {

                RapidVerifone.userConfig objuserConfig = new RapidVerifone.userConfig();

                if (dtUser != null && dtUser.Rows.Count > 0)
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\UserRole.xml";
                    XmlDocument docxmlPath = new XmlDocument();
                    docxmlPath.Load(path);

                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneUser()", "Initialize to Insert", "UserWithRole", "uuseradmin");

                    RapidVerifone.usersType objusersType = new RapidVerifone.usersType();

                    user[] objuser = new user[dtUser.Rows.Count];

                    for (int i = 0; i < dtUser.Rows.Count; i++)
                    {
                        sysidxml = Convert.ToString(dtUser.Rows[i]["CommandID"]);
                        objuser[i] = new user();
                        XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                        if (nodesite != null)
                        {
                            objuserConfig.site = nodesite.InnerText;
                        }
                        else
                        {
                            objuserConfig.site = "";
                        }

                        objuser[i].name = Convert.ToString(dtUser.Rows[i]["UserName"]);
                        objuser[i].isDisallowLogin = false;

                        userPasswd objuserPasswd = new userPasswd();
                        objuserPasswd.expire = Convert.ToBoolean(dtUser.Rows[i]["Expire"]);
                        objuserPasswd.freq = Convert.ToString(dtUser.Rows[i]["Freq"]);
                        objuserPasswd.minLen = Convert.ToString(dtUser.Rows[i]["MinLen"]);
                        objuserPasswd.maxLen = Convert.ToString(dtUser.Rows[i]["MaxLen"]);

                        if (Convert.ToBoolean(dtUser.Rows[i]["isPasswordUpdated"]) == true)
                        {
                            objuserPasswd.value = Convert.ToString(dtUser.Rows[i]["CommandPassword"]).Decript();
                            NewPassword = Convert.ToString(dtUser.Rows[i]["CommandPassword"]).Decript();
                        }
                        objuser[i].passwd = objuserPasswd;
                        objuser[i].employee = Convert.ToString(dtUser.Rows[i]["Employee"]);

                        DataView dvrole = dtUserRole.DefaultView;
                        dvrole.RowFilter = ("[" + dtUserRole.Columns["Name"].ColumnName + "] = '" + Convert.ToString(dtUser.Rows[i]["UserName"]) + "'");
                        if (dvrole != null && dvrole.Count > 0)
                        {
                            RapidVerifone.roleType[] objroleTypeUser = new RapidVerifone.roleType[dvrole.Count];

                            for (int j = 0; j < dvrole.Count; j++)
                            {
                                objroleTypeUser[j] = new RapidVerifone.roleType();
                                objroleTypeUser[j].name = Convert.ToString(dvrole[j]["RoleName"]);
                            }
                            objuser[i].validRoles = objroleTypeUser;
                        }

                        if (arraysysid == "")
                        {
                            arraysysid = sysidxml;
                        }
                        else
                        {
                            arraysysid = arraysysid + "," + sysidxml;
                        }

                        dvrole.RowFilter = string.Empty;
                    }
                    objusersType.user = objuser;
                    object[] item = new object[3];
                    item[2] = objusersType;

                    objuserConfig.Items = item;

                    string xml = objComman.GetXMLFromObject(objuserConfig);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(sb, settings);
                    doc.WriteTo(writer);
                    writer.Close();
                    string t = "xmlns=\"" + "" + "\"";
                    sb = sb.Replace(t, "");
                    UserRolePayloadXML = sb.ToString();

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(UserRolePayloadXML, "InsertUserRole", "POST", "uuseradmin");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long result = objCVerifone.DeleteVerifoneHistory("User", arraysysid);
                        if (result == 0)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneUser()", "Verifone Data not Insert ", "User", "uuseradmin");
                        }
                        else
                        {
                            //bool Result = objComman.UpdateDatabaseServerFile(NewPassword); // update pwd in DS file
                            //if (Result == true)
                            //{
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneUser()", "Verifone Data Insert successfully", "User", "uuseradmin");
                            //}
                        }
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneUser()", "No Response", "User", "uuseradmin");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneUser()", "Not any Tax data is pending to update", "User", "uuseradmin");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneUser()", "UpdateVerifoneUser Exception : " + ex, "User", "uuseradmin");
            }
        }

        public void Update_UpdateVerifoneCashierUser(DataTable dtUser)
        {
            _CVerifone objCVerifone = new _CVerifone();
            DataTable dt = new DataTable();
            DataTable dtUpdate = new DataTable();
            string arraysysid = "";
            string sysidxml = "";
            try
            {

                posSecurity objPOS = new posSecurity();

                if (dtUser.Rows.Count > 0)
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\CashierUser.xml";
                    XmlDocument docxmlPath = new XmlDocument();
                    docxmlPath.Load(path);

                    objCVerifone.InsertActiveLog("Verifone", "Start", "Update_UpdateVerifoneCashierUser()", "Initialize to Update", "CashierUser", "upossecurity");

                    string CashierUserPayloadXML = "";
                    StringBuilder sb = new StringBuilder();
                    RapidVerifone.employeeType[] objEmployeeType = new RapidVerifone.employeeType[dtUser.Rows.Count];

                    for (int i = 0; i < dtUser.Rows.Count; i++)
                    {
                        //string dr1 = Datads.Tables[10].Rows[i][1].ToString();
                        XmlNode nodesite = docxmlPath.DocumentElement.SelectSingleNode("//site");
                        if (nodesite != null)
                        {
                            objPOS.site = nodesite.InnerText;
                        }
                        else
                        {
                            objPOS.site = "";

                        }

                        //objEmployeeType[i].sysid = Convert.ToString(Datads.Tables[10].Rows[i][0]);
                        //objEmployeeType[i].name = Convert.ToString(Datads.Tables[10].Rows[i][1]);

                        #region taxRateTaxProperties
                        //taxRateTaxProperties objtaxRateTaxProperties = new taxRateTaxProperties();

                        //objtaxRateTaxProperties.rate = Convert.ToDecimal(Datads.Tables[0].Rows[i][4]);

                        #region insert update optional values
                        #region update
                        if (Convert.ToString(dtUser.Rows[i]["CommandType"]) == "Update")
                        {
                            sysidxml = Convert.ToString(dtUser.Rows[i][0]);
                            XmlNode node = docxmlPath.DocumentElement.SelectSingleNode("//employees//employee[@sysid='" + sysidxml + "']");
                            if (node != null)
                            {
                                try
                                {
                                    XmlRootAttribute xRoot = new XmlRootAttribute();
                                    xRoot.ElementName = "employee";
                                    xRoot.IsNullable = true;

                                    var serializer = new XmlSerializer(typeof(RapidVerifone.employeeType), xRoot);
                                    RapidVerifone.employeeType objEmployeeType1 = new RapidVerifone.employeeType();

                                    using (TextReader reader = new StringReader(node.OuterXml))
                                    {
                                        objEmployeeType1 = (RapidVerifone.employeeType)serializer.Deserialize(reader);
                                    }
                                    if (objEmployeeType1 != null)
                                    {
                                        objEmployeeType[i] = new RapidVerifone.employeeType();

                                        //objEmployeeType.site = Convert.ToString(dt.Rows[j][9]);
                                        objEmployeeType[i].sysid = Convert.ToString(dtUser.Rows[i][0]);
                                        objEmployeeType[i].name = Convert.ToString(dtUser.Rows[i][1]);
                                        //objEmployeeType[i].name = Convert.ToString(objEmployeeType1.name);  //2
                                        objEmployeeType[i].number = Convert.ToString(dtUser.Rows[i][2]);
                                        objEmployeeType[i].securityLevel = objEmployeeType1.securityLevel;  //4
                                        objEmployeeType[i].isCashier = objEmployeeType1.isCashier;  //4.isCashier
                                        objEmployeeType[i].gemcomPasswd = objEmployeeType1.gemcomPasswd;  //4.isCashier
                                        //objtaxRateTaxProperties.pctStartAmt = Convert.ToDecimal(objtaxRate1.taxProperties.pctStartAmt); //6
                                        //objtaxRateTaxProperties.pctStartAmtSpecified = Convert.ToBoolean(objtaxRate1.taxProperties.pctStartAmtSpecified); //6
                                        //objtaxRateTaxProperties.rateSpecified = Convert.ToBoolean(objtaxRate1.taxProperties.rateSpecified); //6


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
                                catch (Exception)
                                {

                                }
                            }
                        }
                        else if (Convert.ToString(dtUser.Rows[i]["CommandType"]) == "Insert")
                        {
                            objEmployeeType[i] = new RapidVerifone.employeeType();
                            objEmployeeType[i].sysid = Convert.ToString(dtUser.Rows[i][0]);
                            objEmployeeType[i].name = Convert.ToString(dtUser.Rows[i][1]);
                            objEmployeeType[i].number = Convert.ToString(dtUser.Rows[i][2]);
                            objEmployeeType[i].securityLevel = "0";  //4
                            objEmployeeType[i].isCashier = false;  //4.isCashier
                            // objEmployeeType[i].gemcomPasswd = objEmployeeType1.gemcomPasswd;  //4.isCashier

                            if (arraysysid == "")
                            {
                                arraysysid = sysidxml;
                            }
                            else
                            {
                                arraysysid = arraysysid + "," + sysidxml;
                            }

                        }
                        #endregion
                        #endregion
                        // objtaxRate[i].taxProperties = objtaxRateTaxProperties;
                        #endregion

                        objPOS.employees = objEmployeeType;
                    }

                    string xml = objComman.GetXMLFromObject(objPOS);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(sb, settings);
                    doc.WriteTo(writer);
                    writer.Close();
                    string t = "xmlns=\"" + "" + "\"";
                    sb = sb.Replace(t, "");
                    CashierUserPayloadXML = sb.ToString();

                    #region Pass Payload to generate Verifone data xml
                    string Response = objComman.GetXMLResult(CashierUserPayloadXML, "UpdateCashierUser", "POST", "upossecurity");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long result = objCVerifone.DeleteVerifoneHistory("CashierUser", arraysysid);
                        if (result == 0)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneTax()", "Verifone Data not updated ", "Tax", "utaxratecfg");
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneTax()", "Verifone Data updated successfully", "Tax", "utaxratecfg");
                        }
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneTax()", "No Response", "Tax", "utaxratecfg");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneTax()", "Not any Tax data is pending to update", "Tax", "utaxratecfg");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneTax()", "UpdateVerifoneTax Exception : " + ex, "Tax", "utaxratecfg");
            }
        }

        #region MixMatch
        public void UpdateMixMatchItem(DataTable dtItem)
        {
            _CVerifone objCVerifone = new _CVerifone();
            StringBuilder sb = new StringBuilder();
            string ItemXML = "";
            string ItemListID = "";
            try
            {
                objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateMixMatchItem()", "Initialize UpdateMixMatchItem", "MixMatch_ItemList", "uMaintenance");

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
                                    objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateMixMatchItem()", "UpdateMixMatchItem Exception : " + ex, "MixMatch_ItemList", "uMaintenance");
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
                    string Response = objComman.GetXMLResult(ItemXML, "UpdateMixMatch_ItemList", "POST", "uMaintenance");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatchItem()", "Verifone MixMatch_Item updated successfully", "MixMatch_ItemList", "uMaintenance");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatchItem()", "No Response", "MixMatch_ItemList", "uMaintenance");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatchItem()", "Not any MixMatch_Item is pending to update", "MixMatch_ItemList", "uMaintenance");
                }

                #region entry of error item data
                if (MixMatch_ItemListID != "")
                {
                    objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateMixMatchItem()", "Error MixMatch_ItemList : " + MixMatch_ItemListID, "MixMatch_ItemList", "uMaintenance");
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateMixMatchItem()", "UpdateMixMatchItem Exception 2 : " + ex, "MixMatch_ItemList", "uMaintenance");
            }
        }

        public void UpdateMixMatch(DataTable dt, DataTable dtMixMatchEntry)
        {
            _CVerifone objCVerifone = new _CVerifone();
            StringBuilder sb = new StringBuilder();
            string MixMatchXML = "";
            string arraysysid = "";
            try
            {
                objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateMixMatch()", "Initialize UpdateMixMatch", "MixMatch", "uMaintenance");

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
                    string Response = objComman.GetXMLResult(MixMatchXML, "UpdateMixMatch", "POST", "uMaintenance");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        long resultt = objCVerifone.DeleteVerifoneHistory("Promotion", arraysysid);
                        if (resultt == 0)
                        {
                            objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateMixMatch()", "Verifone MixMatch Data not updated ", "MixMatch", "uMaintenance");
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatch()", "Verifone MixMatch Data updated successfully", "MixMatch", "uMaintenance");
                        }
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatch()", "No Response", "MixMatch", "uMaintenance");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatch()", "Not any MixMatch data is pending to update", "MixMatch", "uMaintenance");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateMixMatch()", "UpdateMixMatch Exception : " + ex, "Update MixMatch", "uMaintenance");
            }
        }

        //public void UpdateUser(DataTable dt)
        //{
        //    _CVerifone objCVerifone = new _CVerifone();
        //    StringBuilder sb = new StringBuilder();
        //    string MixMatchXML = "";
        //    string arraysysid = "";
        //    try
        //    {
        //        #region filter of error MixMatch_Item
        //        if (MixMatch_ItemListID != "")
        //        {
        //            MixMatch_ItemListID = MixMatch_ItemListID.TrimEnd(',');
        //            DataView dvNonErrorItems = dt.DefaultView;
        //            dvNonErrorItems.RowFilter = ("[" + dt.Columns["ItemListID"].ColumnName + "] not in (" + MixMatch_ItemListID + ")");
        //            dt = dvNonErrorItems.ToTable();
        //        }
        //        #endregion

        //        if (dt.Rows.Count > 0)
        //        {
        //            RapidVerifoneNAXML.NAXMLMaintenanceRequest objNAXMLConfig_Item = new RapidVerifoneNAXML.NAXMLMaintenanceRequest();

        //            #region TransmissionHeader
        //            RapidVerifoneNAXML.transmissionHeader objtransmissionHeader = new RapidVerifoneNAXML.transmissionHeader();
        //            objtransmissionHeader.StoreLocationID = ConfigurationManager.AppSettings["StoreLocationID"];
        //            objtransmissionHeader.VendorName = ConfigurationManager.AppSettings["VendorName"];
        //            objtransmissionHeader.VendorModelVersion = ConfigurationManager.AppSettings["VendorModelVersion"];
        //            objNAXMLConfig_Item.TransmissionHeader = objtransmissionHeader;
        //            #endregion

        //            RapidVerifoneNAXML.MixMatchMaintenance[] objMixMatch = new RapidVerifoneNAXML.MixMatchMaintenance[1];
        //            objMixMatch[0] = new RapidVerifoneNAXML.MixMatchMaintenance();

        //            RapidVerifoneNAXML.tableActionType result = new RapidVerifoneNAXML.tableActionType();
        //            result = RapidVerifoneNAXML.tableActionType.update;

        //            RapidVerifoneNAXML.tableAction objtableAction = new RapidVerifoneNAXML.tableAction();
        //            objtableAction.type = result;
        //            objMixMatch[0].TableAction = objtableAction;

        //            RapidVerifoneNAXML.recordActionType objRecordType = new RapidVerifoneNAXML.recordActionType();
        //            objRecordType = RapidVerifoneNAXML.recordActionType.addchange;

        //            RapidVerifoneNAXML.recordAction objrecordAction = new RapidVerifoneNAXML.recordAction();
        //            objrecordAction.type = objRecordType;

        //            objMixMatch[0].RecordAction = objrecordAction;

        //            RapidVerifoneNAXML.MMTDetailType[] objMMTDetail = new RapidVerifoneNAXML.MMTDetailType[dt.Rows.Count];

        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                objMMTDetail[i] = new RapidVerifoneNAXML.MMTDetailType();

        //                #region PromotionID
        //                RapidVerifoneNAXML.Promotion[] objPromotion = new RapidVerifoneNAXML.Promotion[1];
        //                objPromotion[0] = new Promotion();
        //                RapidVerifoneNAXML.PromotionID objPromotionID = new RapidVerifoneNAXML.PromotionID();
        //                objPromotionID.Value = Convert.ToString(dt.Rows[i]["Name"]);
        //                objPromotion[0].PromotionID = objPromotionID;
        //                objMMTDetail[i].Promotion = objPromotion;
        //                arraysysid += Convert.ToString(dt.Rows[i]["DiscountId"]) + ",";
        //                #endregion

        //                #region MixMatchDescription
        //                objMMTDetail[i].MixMatchDescription = Convert.ToString(dt.Rows[i]["Description"]);
        //                #endregion

        //                #region MixMatchStrictHighFlag & MixMatchStrictLowFlag
        //                flagDefYes objflagDefYes = new flagDefYes();
        //                yesNo objyesNo = new yesNo();
        //                objyesNo = yesNo.yes;
        //                objflagDefYes.value = objyesNo;
        //                objMMTDetail[i].MixMatchStrictHighFlag = objflagDefYes;
        //                objMMTDetail[i].MixMatchStrictLowFlag = objflagDefYes;
        //                #endregion

        //                #region ItemListID
        //                string[] arrItemListID = new string[1];
        //                arrItemListID[0] = Convert.ToString(dt.Rows[i]["ItemListID"]);
        //                objMMTDetail[i].ItemListID = arrItemListID;
        //                #endregion

        //                #region Start Date & Time - Stop Date & Time
        //                objMMTDetail[i].StartDate = Convert.ToDateTime(dt.Rows[i]["StartDate"]);
        //                objMMTDetail[i].StopDate = Convert.ToDateTime(dt.Rows[i]["EndDate"]);

        //                DateTime StartTime = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                objMMTDetail[i].StartTime = StartTime;

        //                DateTime StopTime = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                objMMTDetail[i].StopTime = StopTime;
        //                #endregion

        //                #region WeekdayAvailability declaration
        //                RapidVerifoneNAXML.WeekdayAvailability[] objWeekdayAvailability = new RapidVerifoneNAXML.WeekdayAvailability[7];
        //                for (int k = 0; k < 7; k++)
        //                {
        //                    objWeekdayAvailability[k] = new RapidVerifoneNAXML.WeekdayAvailability();
        //                    if (k == 0) // sunday
        //                    {
        //                        objyesNo = Convert.ToBoolean(dt.Rows[i]["isSun"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
        //                        objWeekdayAvailability[k].available = objyesNo;  // available

        //                        RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
        //                        objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Sunday;
        //                        objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

        //                        DateTime StartTime_Sun = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sun_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].startTime = StartTime_Sun; // startTime

        //                        DateTime StopTime_Sun = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sun_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].stopTime = StopTime_Sun; // stopTime
        //                    }

        //                    if (k == 1) // monday
        //                    {
        //                        objyesNo = Convert.ToBoolean(dt.Rows[i]["isMon"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
        //                        objWeekdayAvailability[k].available = objyesNo;  // available

        //                        RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
        //                        objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Monday;
        //                        objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

        //                        DateTime StartTime_Mon = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Mon_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].startTime = StartTime_Mon; // startTime

        //                        DateTime StopTime_Mon = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Mon_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].stopTime = StopTime_Mon; // stopTime
        //                    }

        //                    if (k == 2) // tuesday
        //                    {
        //                        objyesNo = Convert.ToBoolean(dt.Rows[i]["isTue"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
        //                        objWeekdayAvailability[k].available = objyesNo;  // available

        //                        RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
        //                        objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Tuesday;
        //                        objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

        //                        DateTime StartTime_Tue = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Tue_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].startTime = StartTime_Tue; // startTime

        //                        DateTime StopTime_Tue = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Tue_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].stopTime = StopTime_Tue; // stopTime
        //                    }

        //                    if (k == 3) // wed
        //                    {
        //                        objWeekdayAvailability[k] = new RapidVerifoneNAXML.WeekdayAvailability();
        //                        objyesNo = Convert.ToBoolean(dt.Rows[i]["isWed"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
        //                        objWeekdayAvailability[k].available = objyesNo;  // available

        //                        RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
        //                        objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Wednesday;
        //                        objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

        //                        DateTime StartTime_Wed = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Wed_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].startTime = StartTime_Wed; // startTime

        //                        DateTime StopTime_Wed = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Wed_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].stopTime = StopTime_Wed; // stopTime
        //                    }

        //                    if (k == 4) // thurs
        //                    {
        //                        objyesNo = Convert.ToBoolean(dt.Rows[i]["isThurs"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
        //                        objWeekdayAvailability[k].available = objyesNo;  // available

        //                        RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
        //                        objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Thursday;
        //                        objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

        //                        DateTime StartTime_Thurs = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Thurs_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].startTime = StartTime_Thurs; // startTime

        //                        DateTime StopTime_Thurs = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Thurs_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].stopTime = StopTime_Thurs; // stopTime
        //                    }

        //                    if (k == 5) // friday
        //                    {
        //                        objyesNo = Convert.ToBoolean(dt.Rows[i]["isFri"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
        //                        objWeekdayAvailability[k].available = objyesNo;  // available

        //                        RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
        //                        objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Friday;
        //                        objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

        //                        DateTime StartTime_Fri = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Fri_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].startTime = StartTime_Fri; // startTime

        //                        DateTime StopTime_Fri = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Fri_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].stopTime = StopTime_Fri; // stopTime
        //                    }

        //                    if (k == 6) // sat
        //                    {
        //                        objyesNo = Convert.ToBoolean(dt.Rows[i]["isSat"]) == true ? RapidVerifoneNAXML.yesNo.yes : RapidVerifoneNAXML.yesNo.no;
        //                        objWeekdayAvailability[k].available = objyesNo;  // available

        //                        RapidVerifoneNAXML.dayOfWeek objdayOfWeek = new RapidVerifoneNAXML.dayOfWeek();
        //                        objdayOfWeek = RapidVerifoneNAXML.dayOfWeek.Saturday;
        //                        objWeekdayAvailability[k].weekday = objdayOfWeek;  // weekday

        //                        DateTime StartTime_Sat = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sat_StartTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].startTime = StartTime_Sat; // startTime

        //                        DateTime StopTime_Sat = DateTime.ParseExact(Convert.ToString(dt.Rows[i]["Sat_EndTime"]), "HH:mm:ss", CultureInfo.InvariantCulture);
        //                        objWeekdayAvailability[k].stopTime = StopTime_Sat; // stopTime
        //                    }
        //                }
        //                objMMTDetail[i].WeekdayAvailability = objWeekdayAvailability;
        //                #endregion

        //                #region MixMatchEntry
        //                DataTable dtMMEntry = new DataTable();
        //                DataView dv = new DataView();
        //                dv = dtMixMatchEntry.DefaultView;
        //                dv.RowFilter = ("[" + dtMixMatchEntry.Columns["Name"].ColumnName + "] = " + Convert.ToString(dt.Rows[i]["Name"]) + "");
        //                dtMMEntry = dv.ToTable();
        //                RapidVerifoneNAXML.MixMatchEntry[] objMixMatchEntry = new RapidVerifoneNAXML.MixMatchEntry[dtMMEntry.Rows.Count];
        //                for (int n = 0; n < dtMMEntry.Rows.Count; n++)
        //                {
        //                    objMixMatchEntry[n] = new RapidVerifoneNAXML.MixMatchEntry();
        //                    sellUnit objsellUnit = new sellUnit();
        //                    //objsellUnit.Value = Convert.ToDecimal(dt.Rows[i]["PrimaryItemQty"]);
        //                    objsellUnit.Value = Convert.ToDecimal(dtMMEntry.Rows[n]["PrimaryItemQty"]);
        //                    objMixMatchEntry[n].MixMatchUnits = objsellUnit;

        //                    RapidVerifoneNAXML.ItemChoiceType objItemChoiceType = new RapidVerifoneNAXML.ItemChoiceType();
        //                    objItemChoiceType = Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "1" ? RapidVerifoneNAXML.ItemChoiceType.MixMatchDiscountAmount : Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "2" ? RapidVerifoneNAXML.ItemChoiceType.MixMatchDiscountPercent : RapidVerifoneNAXML.ItemChoiceType.MixMatchPrice;
        //                    objMixMatchEntry[n].ItemElementName = objItemChoiceType;

        //                    if (Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "1" || Convert.ToString(dtMMEntry.Rows[n]["FreeType"]) == "4")
        //                    {
        //                        amount12 objamount = new amount12();
        //                        objamount.Value = Convert.ToDecimal(dtMMEntry.Rows[n]["Free"]);
        //                        objMixMatchEntry[n].Item = objamount;
        //                    }
        //                    else
        //                    {
        //                        objMixMatchEntry[n].Item = Convert.ToDecimal(dtMMEntry.Rows[n]["Free"]);
        //                    }
        //                }

        //                objMMTDetail[i].MixMatchEntry = objMixMatchEntry;
        //                #endregion

        //                #region Priority
        //                objMMTDetail[i].Priority = "";
        //                #endregion

        //                #region Extension
        //                vsmsNAXML.TaxableRebate objTaxableRebate = new vsmsNAXML.TaxableRebate();
        //                vsmsNAXML.currencyAmt objcurrencyAmt = new vsmsNAXML.currencyAmt();
        //                objcurrencyAmt.Value = Convert.ToDecimal(dt.Rows[i]["TaxAmount"]);
        //                objTaxableRebate.Amount = objcurrencyAmt;

        //                if (Convert.ToString(dt.Rows[i]["TaxId"]) != "")
        //                {
        //                    string TaxId = Convert.ToString(dt.Rows[i]["TaxId"]);
        //                    TaxId = TaxId.Replace(" ", "");
        //                    string[] TaxIdSplit = TaxId.Split(',');

        //                    vsmsNAXML.TaxableRebateTax[] objTaxableRebateTax = new vsmsNAXML.TaxableRebateTax[TaxIdSplit.Length];

        //                    for (int m = 0; m < TaxIdSplit.Length; m++)
        //                    {
        //                        objTaxableRebateTax[m] = new vsmsNAXML.TaxableRebateTax();
        //                        objTaxableRebateTax[m].sysid = TaxIdSplit[m];
        //                    }

        //                    objTaxableRebate.Tax = objTaxableRebateTax;
        //                }

        //                transmissionHeaderExtension objtransmissionHeaderExtension = new transmissionHeaderExtension();
        //                objtransmissionHeaderExtension.Any = new[] { objTaxableRebate.AsXmlElement() };

        //                objMMTDetail[i].Items = new object[] { objtransmissionHeaderExtension };
        //                #endregion
        //            }

        //            objMixMatch[0].MMTDetail = objMMTDetail;
        //            objNAXMLConfig_Item.MixMatchMaintenance = objMixMatch;
        //            objNAXMLConfig_Item.version = ConfigurationManager.AppSettings["MixMatchVersion"];

        //            #region object to xml
        //            string xml = objComman.GetXMLFromObject_standalone(objNAXMLConfig_Item);
        //            XmlDocument doc = new XmlDocument();
        //            doc.LoadXml(xml);
        //            XmlWriterSettings settings = new XmlWriterSettings();
        //            settings.Indent = true;
        //            XmlWriter writer = XmlWriter.Create(sb, settings);
        //            doc.WriteTo(writer);
        //            writer.Close();
        //            string t = "xmlns=\"" + "" + "\"";
        //            sb = sb.Replace(t, "");
        //            MixMatchXML = sb.ToString();
        //            #endregion

        //            #region Pass Payload to generate Verifone data xml
        //            string Response = objComman.GetXMLResult(MixMatchXML, "UpdateMixMatch", "POST", "uMaintenance");
        //            #endregion

        //            #region Delete updated data from BoF
        //            if (Response != null && Response != "")
        //            {
        //                long resultt = objCVerifone.DeleteVerifoneHistory("Promotion", arraysysid);
        //                if (resultt == 0)
        //                {
        //                    objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateMixMatch()", "Verifone MixMatch Data not updated ", "MixMatch", "uMaintenance");
        //                }
        //                else
        //                {
        //                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatch()", "Verifone MixMatch Data updated successfully", "MixMatch", "uMaintenance");
        //                }
        //            }
        //            else
        //            {
        //                objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatch()", "No Response", "MixMatch", "uMaintenance");
        //            }
        //            #endregion
        //        }
        //        else
        //        {
        //            objCVerifone.InsertActiveLog("Verifone", "End", "UpdateMixMatch()", "Not any MixMatch data is pending to update", "MixMatch", "uMaintenance");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateMixMatch()", "UpdateMixMatch Exception : " + ex, "Update MixMatch", "uMaintenance");
        //    }
        //}
        #endregion

        #region User
        public string UpdateVerifoneUserPassword(DataTable dt)
        {
            _CVerifone objCVerifone = new _CVerifone();
            DataTable dtUpdate = new DataTable();
            string arraysysid = "";
            string Response = "";
            Comman com = new Comman();
            try
            {

                passwdConfig objpasswdConfig = new passwdConfig();

                if (dt != null && dt.Rows.Count > 0)
                {
                    objCVerifone.InsertActiveLog("Verifone", "Start", "UpdateVerifoneUserPassword()", "Initialize to Update User Password", "User", "changepasswd");

                    string UserPayloadXML = "";
                    StringBuilder sb = new StringBuilder();

                    // remove loop bcoz at a time only one pwd will be updated using DataSource file
                    passwdConfigUser objpasswdConfigUser = new passwdConfigUser();
                    objpasswdConfigUser.name = VerifoneServices.VerifoneUserName; //Convert.ToString(dt.Rows[0]["UserName"]);

                    passwdConfigUserPasswd objpasswdConfigUserPasswd = new passwdConfigUserPasswd();
                    objpasswdConfigUserPasswd.oldValue = VerifoneServices.VerifonePassword.Decript();

                    //New_VP = Convert.ToString(dt.Rows[0]["CommandPassword"]);
                    New_VP = com.RandomString(7);

                    //objpasswdConfigUserPasswd.newValue = New_VP.Decript();
                    objpasswdConfigUserPasswd.newValue = New_VP;

                    objpasswdConfigUser.Item = objpasswdConfigUserPasswd;

                    objpasswdConfig.user = objpasswdConfigUser;

                    arraysysid = Convert.ToString(dt.Rows[0]["SrNo"]);

                    string xml = objComman.GetXMLFromObject(objpasswdConfig);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(sb, settings);
                    doc.WriteTo(writer);
                    writer.Close();
                    string t = "xmlns=\"" + "" + "\"";
                    sb = sb.Replace(t, "");
                    UserPayloadXML = sb.ToString();

                    #region Pass Payload to generate Verifone data xml
                    Response = objComman.GetXMLResult(UserPayloadXML, "UpdateUser", "POST", "changepasswd");
                    #endregion

                    #region Delete updated data from BoF
                    if (Response != null && Response != "")
                    {
                        //objComman.UpdateDatabaseServerFile(New_VP);

                        //long result = objCVerifone.DeleteVerifoneHistory("isPasswordUpdated", arraysysid);
                        //if (result == 0)
                        //{
                        //    objCVerifone.InsertActiveLog("Verifone", "Fail", "UpdateVerifoneUser()", "Verifone Data not updated ", "User", "changepasswd");
                        //}
                        //else
                        //{
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneUserPassword()", "Verifone Data Updated Successfully", "User", "changepasswd");
                        //}
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneUserPassword()", "No Response", "User", "changepasswd");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "End", "UpdateVerifoneUserPassword()", "Not any User data is pending to update", "User", "changepasswd");
                }
                return Response;
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateVerifoneUserPassword()", "UpdateVerifoneUserPassword Exception : " + ex, "User", "changepasswd");
                return Response;
            }
        }
        #endregion
    }
}











