using VerifoneLibrary;
using VerifoneLibrary.DataAccess;
using RapidVerifone;
using myCompany1111;
using RapidVerifoneNAXML;

using VerifoneLibrary.DataObject;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Configuration;
using System.Xml.Serialization;
using System.Data;
using System.Xml;
using System.Collections;
using RapidVerifoneNAXML;
using System.Xml.Linq;
using vsmsNAXML;
using System.Text.RegularExpressions;
using RapidVerifoneFuelconfig;

namespace VerifoneServices
{
    public class VerifoneInsert
    {
        public string FilePath = "";
        public bool FileExists = false;
        public string lblCookieValue = "";
        public string filename = "";
        public string period = "";
        public string PeriodFileName = "";
        public bool RequestUrl = true;
        string PeriodForXML = "";
        string FileNameForXML = "";
        long OrderNo = 0;

        Comman objComman = new Comman();

        #region Masters

        public void Tax()
        {
            VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
            try
            {
                #region Read Payload, Save Tax data from Verifone
                objComman.GetPayloadXML("Tax", "POST", "vtaxratecfg");
                #endregion

                #region insert log
                objCVerifone.InsertActiveLog("BoF", "Start", "Tax()", "Initialize to Insert Verifone Data into BoF", "Tax", "");
                #endregion

                var FilePathTax = AppDomain.CurrentDomain.BaseDirectory + "xml\\Tax.xml";

                var FileExistsTax = System.IO.File.Exists(FilePathTax);
                //this.WriteToFile("FileExistsTax : " + FileExistsTax);
                if (FileExistsTax == true)
                {
                    #region Insert Verifone Tax Data
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Tax.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.taxRateConfig));

                    RapidVerifone.taxRateConfig resultingMessage = (RapidVerifone.taxRateConfig)serializer.Deserialize(new XmlTextReader(path));
                    String strItemTaxXMLData = string.Empty;
                    int taxcount = resultingMessage.taxRates.Count();
                    RapidVerifone.taxRate[] objtaxRate = new RapidVerifone.taxRate[taxcount];

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[6] {
                    new DataColumn("sysid", typeof(int)), new DataColumn("name", typeof(string)),new DataColumn("indicator", typeof(string)),
                    new DataColumn("isPriceIncsTax", typeof(bool)),new DataColumn("isPromptExemption", typeof(bool)),
                    new DataColumn("rate", typeof(decimal))
                    });

                    for (int i = 0; i < objtaxRate.Length; i++)
                    {
                        dt.Rows.Add
                        (
                                resultingMessage.taxRates[i].sysid,
                                resultingMessage.taxRates[i].name,
                                resultingMessage.taxRates[i].indicator,
                                resultingMessage.taxRates[i].isPriceIncsTax,
                                resultingMessage.taxRates[i].isPromptExemption,
                                resultingMessage.taxRates[i].taxProperties.rate
                         );
                    }

                    long result = objCVerifone.InsertTax(dt);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "Tax()", "Verifone Data not inserted in BoF", "Tax", "");
                        //this.WriteToFile("Fail");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "Tax()", "Verifone Data inserted Successfully in BoF", "Tax", "");
                        //this.WriteToFile("Success");
                    }
                    #endregion
                }
                else
                {
                    //this.WriteToFile("TAX FILE PATH : " + FilePathTax);
                    objCVerifone.InsertActiveLog("BoF", "Fail", "Tax()", "Tax.xml not found", "Tax", "");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Tax()", "Tax Exception : " + ex, "Tax", "");
                //this.WriteToFile("Tax : " + ex);
            }
        }

        public void Payment()
        {
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                #region Read Payload, Save Payment data from Verifone
                objComman.GetPayloadXML("Payment", "POST", "vpaymentcfg");
                #endregion

                var FilePathPayment = AppDomain.CurrentDomain.BaseDirectory + "xml\\Payment.xml";
                var FileExistsPayment = File.Exists(FilePathPayment);
                if (FileExistsPayment == true)
                {
                    #region Insert Verifone Payment Data
                    objCVerifone.InsertActiveLog("BoF", "Start", "Payment()", "Initialize to Insert Verifone Data into BoF", "Payment", "");

                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Payment.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.paymentConfig));

                    RapidVerifone.paymentConfig resultingMessage = (RapidVerifone.paymentConfig)serializer.Deserialize(new XmlTextReader(path));
                    String strItemPaymentXMLData = string.Empty;

                    int paymentmopCodes = resultingMessage.mopCodes.Count();
                    RapidVerifone.paymentConfigMopCode[] objPaymentmopCodes = new RapidVerifone.paymentConfigMopCode[paymentmopCodes];
                    objPaymentmopCodes = resultingMessage.mopCodes;

                    int paymentmops = resultingMessage.mops.Count();
                    RapidVerifone.paymentConfigMop[] objPaymentmops = new RapidVerifone.paymentConfigMop[paymentmops];
                    objPaymentmops = resultingMessage.mops;

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[3] {
                    new DataColumn("sysid", typeof(int)), new DataColumn("PaymentName", typeof(string)),new DataColumn("Paycode", typeof(string))
                    });


                    DataTable dtCardTypeName = new DataTable();
                    dtCardTypeName.Columns.AddRange(new DataColumn[2]
                    {     new DataColumn("CardTypeId", typeof(int)),
                          new DataColumn("CardTypeName", typeof(string))
                    });

                    for (int j = 0; j < objPaymentmopCodes.Length; j++)
                    {
                        dtCardTypeName.Rows.Add
                       (
                               objPaymentmopCodes[j].sysid,
                               objPaymentmopCodes[j].name

                       );

                    }


                    var paycode = "";
                    for (int i = 0; i < objPaymentmops.Length; i++)
                    {
                        for (int j = 0; j < objPaymentmopCodes.Length; j++)
                        {
                            if (objPaymentmopCodes[j].sysid == objPaymentmops[i].code)
                            {
                                paycode = objPaymentmopCodes[j].name;
                                break;
                            }
                        }
                        dt.Rows.Add
                        (
                                resultingMessage.mops[i].sysid,
                                resultingMessage.mops[i].name,
                                paycode
                        );
                    }


                    long result = objCVerifone.InsertPayment(dt, dtCardTypeName);
                    //long result = objCVerifone.InsertPayment(dt);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "Payment()", "Verifone Data not inserted in BoF", "Payment", "");
                        //this.WriteToFile("Fail");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "Payment()", "Verifone Data inserted Successfully in BoF", "Payment", "");
                        //this.WriteToFile("Success");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "Fail", "Payment()", "Payment.xml not found", "Payment", "");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Payment()", "Payment Exception : " + ex, "Payment", "");
                //this.WriteToFile("Payment : " + ex);
            }
        }

        public void Category()
        {
            VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
            try
            {
                #region Read Payload, Save Tax data from Verifone
                objComman.GetPayloadXML("Category", "POST", "vposcfg");
                #endregion

                var FilePathCategory = AppDomain.CurrentDomain.BaseDirectory + "xml\\Category.xml";
                var FileExistsCategory = File.Exists(FilePathCategory);
                if (FileExistsCategory == true)
                {
                    #region Insert Verifone Category Data
                    objCVerifone.InsertActiveLog("BoF", "Start", "Category()", "Initialize to Insert Verifone Data into BoF", "Category", "");
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Category.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.posConfig));
                    RapidVerifone.posConfig resultingMessage = (RapidVerifone.posConfig)serializer.Deserialize(new XmlTextReader(path));

                    int CategortCount = resultingMessage.categories.Count();
                    RapidVerifone.posConfig[] objCategory = new RapidVerifone.posConfig[CategortCount];

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[2]{
                    new DataColumn("sysid", typeof(int)), new DataColumn("name", typeof(string))
                    });

                    for (int i = 0; i < objCategory.Length; i++)
                    {
                        dt.Rows.Add(
                            resultingMessage.categories[i].sysid,
                            resultingMessage.categories[i].name
                       );
                    }

                    long result = objCVerifone.InsertCategory(dt);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "Category()", "Verifone Data not inserted in BoF", "Category", "");
                        //this.WriteToFile("Fail");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "Category()", "Verifone Data inserted Successfully in BoF", "Category", "");
                        //this.WriteToFile("Success");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "Fail", "Category()", "Category.xml not found", "Category", "");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Category()", "Category Exception : " + ex, "Category", "");
                //this.WriteToFile("Category : " + ex);
            }
        }

        public void ProductCode()
        {
            VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
            try
            {
                #region Read Payload, Save Tax data from Verifone
                objComman.GetPayloadXML("prodCodes", "POST", "vposcfg");
                #endregion

                var FilePathCategory = AppDomain.CurrentDomain.BaseDirectory + "xml\\prodCodes.xml";
                var FileExistsCategory = File.Exists(FilePathCategory);
                if (FileExistsCategory == true)
                {
                    #region Insert Verifone Category Data
                    objCVerifone.InsertActiveLog("BoF", "Start", "prodCodes()", "Initialize to Insert Verifone Data into BoF", "prodCodes", "");
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\prodCodes.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.posConfig));
                    RapidVerifone.posConfig resultingMessage = (RapidVerifone.posConfig)serializer.Deserialize(new XmlTextReader(path));

                    int prodCodesCount = resultingMessage.prodCodes.Count();
                    RapidVerifone.posConfig[] objProdcode = new RapidVerifone.posConfig[prodCodesCount];

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[2]{
                    new DataColumn("sysid", typeof(int)), new DataColumn("name", typeof(string))
                    });

                    for (int i = 0; i < objProdcode.Length; i++)
                    {
                        dt.Rows.Add(
                            resultingMessage.prodCodes[i].sysid,
                            resultingMessage.prodCodes[i].name
                       );
                    }

                    long result = objCVerifone.InsertprodCodes(dt);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "prodCodes()", "Verifone Data not inserted in BoF", "prodCodes", "");
                        //this.WriteToFile("Fail");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "prodCodes()", "Verifone Data inserted Successfully in BoF", "prodCodes", "");
                        //this.WriteToFile("Success");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "Fail", "prodCodes()", "Category.xml not found", "prodCodes", "");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "prodCodes()", "Category Exception : " + ex, "prodCodes", "");
                //this.WriteToFile("Category : " + ex);
            }
        }

        public void Department()
        {
            VerifoneLibrary.DataAccess._CVerifone objPOSdetail = new VerifoneLibrary.DataAccess._CVerifone();
            try
            {
                #region Read Payload, Save Department data from Verifone
                objComman.GetPayloadXML("Department", "POST", "vposcfg");
                #endregion

                var FilePathDepartment = AppDomain.CurrentDomain.BaseDirectory + "xml\\Department.xml";
                var FileExistsDepartment = File.Exists(FilePathDepartment);
                if (FileExistsDepartment == true)
                {
                    #region Insert Verifone Department Data
                    objPOSdetail.InsertActiveLog("BoF", "Start", "Department()", "Initialize to Insert Verifone Data into BoF", "Department", "");
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Department.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.posConfig));

                    RapidVerifone.posConfig resultingMessage = (RapidVerifone.posConfig)serializer.Deserialize(new XmlTextReader(path));
                    String strItemTaxXMLData = string.Empty;

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[10] {
                    new DataColumn("DeptId", typeof(int)), new DataColumn("DeptCode", typeof(string)), new DataColumn("DeptName", typeof(string)),
                    new DataColumn("TaxFlg", typeof(int)), new DataColumn("IsGASPump", typeof(bool)), new DataColumn("isMoneyOrder", typeof(bool)),
                    new DataColumn("TaxId", typeof(int)), new DataColumn("ItemCode", typeof(int)), new DataColumn("ProdCode", typeof(string)),
                    new DataColumn("CatId", typeof(int))
                    });

                    DataTable dt1 = new DataTable();
                    dt1.Columns.AddRange(new DataColumn[2] {
                    new DataColumn("DeptId", typeof(int)), new DataColumn("TaxId", typeof(string))});

                    int DepartmentCount = resultingMessage.departments.Count();
                    RapidVerifone.department[] objDepartment = new RapidVerifone.department[DepartmentCount];
                    objDepartment = resultingMessage.departments;

                    int ProbCodeCount = resultingMessage.prodCodes.Count();
                    RapidVerifone.prodCode[] objProdCode = new RapidVerifone.prodCode[ProbCodeCount];
                    objProdCode = resultingMessage.prodCodes;

                    var deptCode = "";
                    int TaxFlag = 0;
                    int TaxId = 0;
                    int itemCode = 0;
                    var ProdCode = "";

                    DataSet dsreg = objComman.GetRegsiterinv();
                    if (dsreg.Tables[0] != null && dsreg.Tables[0].Rows.Count > 0)
                    {
                        itemCode = Convert.ToInt32(dsreg.Tables[0].Rows[0]["ItemCode"]);
                    }

                    for (int i = 0; i < objDepartment.Length; i++)
                    {
                        for (int j = 0; j < objProdCode.Length; j++)
                        {
                            if (objProdCode[j].sysid == objDepartment[i].prodCode.sysid)
                            {
                                deptCode = objProdCode[j].name;
                                ProdCode = objProdCode[j].sysid;
                                break;
                            }
                        }

                        int DepartmentTaxCount = objDepartment[i].taxes.Count();
                        RapidVerifone.departmentTax[] objDepartmentTax = new RapidVerifone.departmentTax[DepartmentTaxCount];
                        if (DepartmentTaxCount != 0)
                        {
                            objDepartmentTax = objDepartment[i].taxes;
                            for (int k = 0; k < objDepartmentTax.Length; k++)
                            {
                                if (objDepartmentTax[k].sysid != "")
                                {
                                    TaxFlag = 1;
                                    TaxId = Convert.ToInt16(objDepartmentTax[k].sysid);

                                    dt1.Rows.Add(
                                        resultingMessage.departments[i].sysid,
                                        Convert.ToInt16(objDepartmentTax[k].sysid)
                                        );
                                }
                                else
                                {
                                    TaxFlag = 0;
                                    TaxId = 0;
                                }
                            }
                        }
                        else
                        {
                            TaxFlag = 0;
                            TaxId = 0;
                        }

                        dt.Rows.Add
                        (
                            resultingMessage.departments[i].sysid,
                            deptCode,
                            resultingMessage.departments[i].name,
                            TaxFlag,
                            resultingMessage.departments[i].isFuel,
                            resultingMessage.departments[i].isMoneyOrder,
                            TaxId,
                            itemCode = itemCode + 1,
                            ProdCode,
                            resultingMessage.departments[i].category.sysid
                        );
                    }

                    long result = objPOSdetail.InsertDepartment(dt, dt1);

                    if (result == 0)
                    {
                        objPOSdetail.InsertActiveLog("BoF", "Fail", "Department()", "Verifone Data not inserted in BoF", "Department", "");
                        //this.WriteToFile("Fail");
                    }
                    else
                    {
                        objPOSdetail.InsertActiveLog("BoF", "End", "Department()", "Verifone Data inserted Successfully in BoF", "Department", "");
                        // this.WriteToFile("Success");
                    }
                    #endregion
                }
                else
                {
                    objPOSdetail.InsertActiveLog("BoF", "Fail", "Department()", "Department.xml not found", "Department", "");
                }
            }
            catch (Exception ex)
            {
                objPOSdetail.InsertActiveLog("BoF", "Error", "Department()", "Department Exception : " + ex, "Department", "");
                //this.WriteToFile("Department : " + ex);
            }
        }

        public void Fee()
        {
            VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
            try
            {
                #region Read Payload, Save Tax data from Verifone
                objComman.GetPayloadXML("Fee", "POST", "vfeecfg");
                #endregion

                var FilePathFee = AppDomain.CurrentDomain.BaseDirectory + "xml\\Fee.xml";
                var FileExistsFee = File.Exists(FilePathFee);
                if (FileExistsFee == true)
                {
                    #region Insert Verifone Fee Data
                    objCVerifone.InsertActiveLog("BoF", "Start", "Fee()", "Initialize to Insert Verifone Data into BoF", "Fee", "");

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.feeConfig));

                    RapidVerifone.feeConfig resultingMessage = (RapidVerifone.feeConfig)serializer.Deserialize(new XmlTextReader(FilePathFee));

                    int FeeCount = resultingMessage.fees.Count();
                    RapidVerifone.fee[] objFee = new RapidVerifone.fee[FeeCount];
                    objFee = resultingMessage.fees;

                    decimal rangeFee = 0;
                    decimal rangeEnd = 0;
                    int ChargeId = 0;

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[3]{
                    new DataColumn("sysid", typeof(int)), new DataColumn("name", typeof(string)), new DataColumn("dept", typeof(int))
                    });

                    DataTable dt1 = new DataTable();
                    dt1.Columns.AddRange(new DataColumn[4]{
                    new DataColumn("ChargeId", typeof(int)), new DataColumn("sysid", typeof(int)), new DataColumn("rangeFee", typeof(decimal)),
                    new DataColumn("rangeEnd", typeof(decimal))
                    });

                    for (int i = 0; i < objFee.Length; i++)
                    {
                        if (objFee[i].Items != null)
                        {
                            int RangeCount = objFee[i].Items.Count();

                            //RapidVerifone.rangeAmountFeeType[] objRange = new RapidVerifone.rangeAmountFeeType[RangeCount];

                            for (int j = 0; j < RangeCount; j++)
                            {
                                try
                                {
                                    rangeFee = ((rangeAmountFeeType)(objFee[i].Items[j])).rangeFee;
                                    rangeEnd = ((rangeAmountFeeType)(objFee[i].Items[j])).rangeEnd;

                                    dt1.Rows.Add(
                                         ChargeId = ChargeId + 1,
                                         objFee[i].sysid,
                                         rangeFee,
                                         rangeEnd
                                    );

                                }
                                catch (Exception ex)
                                {
                                    rangeFee = 0;
                                    rangeEnd = 0;

                                    dt1.Rows.Add(
                                         ChargeId = ChargeId + 1,
                                         objFee[i].sysid,
                                         rangeFee,
                                         rangeEnd
                                    );
                                }
                            }
                        }
                        else
                        {
                            rangeFee = 0;
                            rangeEnd = 0;

                            dt1.Rows.Add(
                                 ChargeId = ChargeId + 1,
                                 objFee[i].sysid,
                                 rangeFee,
                                 rangeEnd
                            );
                        }

                        dt.Rows.Add(
                            objFee[i].sysid,
                            objFee[i].name,
                            objFee[i].dept
                            );
                    }

                    long result = objCVerifone.InsertFeeDeposit(dt, dt1);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "Fee()", "Verifone Data not inserted in BoF", "Fee", "");
                        //this.WriteToFile("Fail");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "Fee()", "Verifone Data inserted Successfully in BoF", "Fee", "");
                        // this.WriteToFile("Success");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "Fail", "Fee()", "Fee.xml not found", "Fee", "");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Fee()", "Fee Exception :" + ex, "Fee", "");
                //this.WriteToFile("Fee :" + ex);
            }
        }

        public void Item_New()
        {
            _CVerifone objVerifone = new _CVerifone();
            int TaxRate = 0;
            int ItemCode = 0;
            bool isExists = false;
            try
            {
                #region Read Payload, Save Item data from Verifone
                objComman.GetPayloadXML("Item", "POST", "vPLUs");
                #endregion

                var FilePathItem = AppDomain.CurrentDomain.BaseDirectory + "xml\\Item.xml";
                var FileExistsItem = File.Exists(FilePathItem);
                if (FileExistsItem == true)
                {
                    #region Insert Verifone Item Data
                    objVerifone.InsertActiveLog("BoF", "Start", "Item()", "Initialize to Insert Verifone Data into BoF", "Item", "");
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Item.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVarifone.PLUs));

                    RapidVarifone.PLUs resultingMessage = (RapidVarifone.PLUs)serializer.Deserialize(new XmlTextReader(path));

                    int ItemCount = resultingMessage.PLU.Count();
                    RapidVarifone.PLUCType[] objItems = new RapidVarifone.PLUCType[ItemCount];
                    objItems = resultingMessage.PLU;

                    DataTable dtItem = new DataTable();
                    dtItem.Columns.AddRange(new DataColumn[13]{
                    new DataColumn("upc", typeof(string)), new DataColumn("description", typeof(string)), new DataColumn("department", typeof(int)),
                    new DataColumn("fees", typeof(string)), new DataColumn("pcode", typeof(string)), new DataColumn("price", typeof(decimal)),
                    new DataColumn("taxRates", typeof(int)), new DataColumn("SellUnit", typeof(decimal)), new DataColumn("taxableRebate_amount", typeof(decimal)),
                    new DataColumn("ItemCode", typeof(int)), new DataColumn("ItemType", typeof(int)), new DataColumn("upcModifier", typeof(string)),
                    new DataColumn("isExists", typeof(bool))});

                    DataTable dtItemTax = new DataTable();
                    dtItemTax.Columns.AddRange(new DataColumn[4]{
                    new DataColumn("ItemCode", typeof(int)), new DataColumn("UPC", typeof(string)), new DataColumn("upcModifier", typeof(string)),
                    new DataColumn("TaxId", typeof(int))});

                    DataTable dtItemFee = new DataTable();
                    dtItemFee.Columns.AddRange(new DataColumn[4]{
                    new DataColumn("ItemCode", typeof(int)), new DataColumn("UPC", typeof(string)), new DataColumn("upcModifier", typeof(string)),
                    new DataColumn("FeeId", typeof(int))});

                    //DataSet dsreg = objComman.GetRegsiterinv();
                    //if (dsreg.Tables[0] != null && dsreg.Tables[0].Rows.Count > 0)
                    //{
                    //    ItemCode = Convert.ToInt32(dsreg.Tables[0].Rows[0]["ItemCode"]);
                    //}

                    DataSet dsItem = objVerifone.GetItemData();
                    if (dsItem.Tables[1] != null && dsItem.Tables[1].Rows.Count > 0)
                    {
                        ItemCode = Convert.ToInt32(dsItem.Tables[1].Rows[0]["ItemCode"]);
                    }

                    for (int i = 0; i < objItems.Length; i++)
                    {
                        DataView dvItem = dsItem.Tables[0].DefaultView;
                        dvItem.RowFilter = ("[" + dsItem.Tables[0].Columns["UPC"].ColumnName + "] ='" + objItems[i].upc.Value + "'AND [" + dsItem.Tables[0].Columns["upcModifier"].ColumnName + "] ='" + objItems[i].upcModifier + "'");
                        if (dvItem.Count > 0)
                        {
                            isExists = true;
                        }
                        else
                        {
                            ItemCode += 1;
                            isExists = false;
                        }

                        TaxRate = 0;

                        if (objItems[i].taxRates != null)
                        {
                            int TaxRatesCount = objItems[i].taxRates.Count();
                            RapidVarifone.taxRate[] objTaxRates = new RapidVarifone.taxRate[TaxRatesCount];
                            objTaxRates = objItems[i].taxRates;

                            for (int j = 0; j < objTaxRates.Length; j++)
                            {
                                dtItemTax.Rows.Add(
                                    isExists == true ? dvItem[0]["ItemCode"] : ItemCode,
                                    objItems[i].upc.Value,
                                    objItems[i].upcModifier,
                                    Convert.ToInt32(objTaxRates[j].sysid)
                                );
                            }
                            TaxRate = 1;
                        }
                        else
                        {
                            TaxRate = 0;
                        }

                        if (((RapidVarifone.PLUCTypeFees)(objItems[i].Item)).fee != null)
                        {
                            for (int k = 0; k < ((RapidVarifone.PLUCTypeFees)(objItems[i].Item)).fee.Length; k++)
                            {
                                dtItemFee.Rows.Add(
                                    isExists == true ? dvItem[0]["ItemCode"] : ItemCode,
                                    objItems[i].upc.Value,
                                    objItems[i].upcModifier,
                                    ((RapidVarifone.PLUCTypeFees)(objItems[i].Item)).fee[k]
                                );
                            }
                        }

                        dtItem.Rows.Add(
                            objItems[i].upc.Value,
                            objItems[i].description,
                            objItems[i].department,
                            ((RapidVarifone.PLUCTypeFees)(objItems[i].Item)).fee[0],
                            objItems[i].pcode,
                            objItems[i].price.Value,
                            TaxRate,
                            objItems[i].SellUnit,
                            objItems[i].taxableRebate.amount.Value,
                            isExists == true ? dvItem[0]["ItemCode"] : ItemCode,
                            0,
                            objItems[i].upcModifier,
                            isExists
                        );

                        // ******* new case
                        dtItem.Rows.Add(
                            objItems[i].upc.Value,//"",
                            "",
                            0,
                            "",
                            0,
                            0,
                            0,
                            0,
                            0,
                            isExists == true ? dvItem[0]["ItemCode"] : ItemCode,
                            1,
                            objItems[i].upcModifier,
                            isExists
                        );

                        // ******* new pack
                        dtItem.Rows.Add(
                            objItems[i].upc.Value,// "",
                            "",
                            0,
                            "",
                            0,
                            0,
                            0,
                            0,
                            0,
                            isExists == true ? dvItem[0]["ItemCode"] : ItemCode,
                            2,
                            objItems[i].upcModifier,
                            isExists
                        );
                    }

                    long result = objVerifone.InsertItems(dtItem, dtItemTax, dtItemFee);

                    if (result == 0)
                    {
                        objVerifone.InsertActiveLog("BoF", "Fail", "Item()", "Verifone Data not inserted in BoF", "Item", "");
                    }
                    else
                    {
                        objVerifone.InsertActiveLog("BoF", "End", "Item()", "Verifone Data inserted Successfully in BoF", "Item", "");
                    }
                    #endregion
                }
                else
                {
                    objVerifone.InsertActiveLog("BoF", "Fail", "Item()", "Item.xml not found", "Item", "");
                }
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Item()", "Item Exception : " + ex, "Item", "");
            }
        }

        public void Fuel()
        {
            VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
            try
            {
                #region Read Payload, Save Fuel data from Verifone
                objComman.GetPayloadXML("Fuel", "POST", "vfuelrtcfg");
                #endregion

                var FilePathFuel = AppDomain.CurrentDomain.BaseDirectory + "xml\\Fuel.xml";
                var FileExistsFuel = File.Exists(FilePathFuel);
                if (FileExistsFuel == true)
                {
                    #region Insert Verifone Fuel Data
                    objCVerifone.InsertActiveLog("BoF", "Start", "Fuel()", "Initialize to Insert Verifone Data into BoF", "Fuel", "");
                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Fuel.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifoneFuelconfig.fuelConfigType));

                    RapidVerifoneFuelconfig.fuelConfigType resultingMessage = (RapidVerifoneFuelconfig.fuelConfigType)serializer.Deserialize(new XmlTextReader(path));


                    RapidVerifoneFuelconfig.fuelConfigTypeFuelSvcModes objFuelSvcModes = new RapidVerifoneFuelconfig.fuelConfigTypeFuelSvcModes();
                    objFuelSvcModes = resultingMessage.fuelSvcModes;


                    DataTable dtFuelServices = new DataTable();
                    dtFuelServices.Columns.AddRange(new DataColumn[2]{
                    new DataColumn("FuelServicesId", typeof(int)), new DataColumn("FuelServices", typeof(string))});


                    for (int j = 0; j < objFuelSvcModes.fuelSvcMode.Length; j++)
                    {
                        dtFuelServices.Rows.Add(
                              objFuelSvcModes.fuelSvcMode[j].sysid,
                              objFuelSvcModes.fuelSvcMode[j].name
                          );

                    }




                    int FuelCount = resultingMessage.fuelProducts.fuelProduct.Count();
                    RapidVerifoneFuelconfig.fuelConfigTypeFuelProductsFuelProduct[] objFuel = new RapidVerifoneFuelconfig.fuelConfigTypeFuelProductsFuelProduct[FuelCount];
                    objFuel = resultingMessage.fuelProducts.fuelProduct;






                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[4]{
                    new DataColumn("FuelId", typeof(int)), new DataColumn("FuelName", typeof(string))
                    , new DataColumn("NAXMLFuelGradeID", typeof(string))
                    , new DataColumn("deptid", typeof(string))
                    });

                    DataTable dt1 = new DataTable();
                    dt1.Columns.AddRange(new DataColumn[5]{
                    new DataColumn("FuelId", typeof(int)), new DataColumn("PayId", typeof(int)), 
                    new DataColumn("ServiceType", typeof(int)), 
                    new DataColumn("Price", typeof(decimal)),
                    new DataColumn("tier", typeof(string))
                    
                    });

                    for (int i = 0; i < objFuel.Length; i++)
                    {
                        int FuelPriceCount = objFuel[i].prices.Count();
                        RapidVerifoneFuelconfig.fuelConfigTypeFuelProductsFuelProductPrice[] objFuelPrice = new RapidVerifoneFuelconfig.fuelConfigTypeFuelProductsFuelProductPrice[FuelPriceCount];
                        objFuelPrice = objFuel[i].prices;

                        fuelConfigTypeFuelProductsFuelProductDepartment objdepartment = new fuelConfigTypeFuelProductsFuelProductDepartment();
                        objdepartment = objFuel[i].department;

                        for (int j = 0; j < objFuelPrice.Length; j++)
                        {
                            //if (objFuelPrice[j].tier == "1")
                            //{
                            dt1.Rows.Add(
                                objFuel[i].sysid,
                                objFuelPrice[j].mop,
                                objFuelPrice[j].servLevel,
                                objFuelPrice[j].Value,
                                objFuelPrice[j].tier
                                );
                            //}
                        }

                        dt.Rows.Add(
                                objFuel[i].sysid,
                                objFuel[i].name,
                                objFuel[i].NAXMLFuelGradeID,
                                objdepartment.sysid

                            );
                    }

                    long result = objCVerifone.InsertFuel(dtFuelServices, dt, dt1);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "Fuel()", "Verifone Data not inserted in BoF", "Fuel", "");
                        // this.WriteToFile("Fuel");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "Fuel()", "Verifone Data inserted Successfully in BoF", "Fuel", "");
                        // this.WriteToFile("Success");
                    }
                    #endregion
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "Fail", "Fuel()", "Fuel.xml not found", "Fuel", "");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Fuel()", "Fuel Exception :" + ex, "Fuel", "");
                // this.WriteToFile("Fuel : " + ex);
            }
        }

        public void Employee()
        {
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                #region Save CashierUser data from Verifone
                objComman.GetXMLResult("", "Employee", "POST", "vpossecurity");
                #endregion

                var FilePathCashierUser = AppDomain.CurrentDomain.BaseDirectory + "xml\\Employee.xml";

                var FileExistsCashierUser = System.IO.File.Exists(FilePathCashierUser);
                if (FileExistsCashierUser == true)
                {
                    #region insert log
                    objCVerifone.InsertActiveLog("BoF", "Start", "Employee()", "Initialize to Insert Verifone Data into BoF", "Employee", "");
                    #endregion

                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Employee.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.posSecurity));

                    RapidVerifone.posSecurity resultingMessage = (RapidVerifone.posSecurity)serializer.Deserialize(new XmlTextReader(path));

                    int EmployeeCount = resultingMessage.employees.Count();
                    RapidVerifone.employeeType[] objEmployee = new RapidVerifone.employeeType[EmployeeCount];
                    objEmployee = resultingMessage.employees;

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[6]
                    {
                        new DataColumn("sysid", typeof(Int64)), new DataColumn("EmployeeName", typeof(string)), new DataColumn("Number", typeof(string))
                            , new DataColumn("securityLevel", typeof(string)) , new DataColumn("isCashier", typeof(bool)), new DataColumn("gemcomPasswd", typeof(string))
                    });

                    for (int i = 0; i < objEmployee.Length; i++)
                    {
                        dt.Rows.Add(
                            Convert.ToInt64(objEmployee[i].sysid),
                            objEmployee[i].name,
                            objEmployee[i].number,
                            objEmployee[i].securityLevel,
                            objEmployee[i].isCashier,
                            objEmployee[i].gemcomPasswd
                            );
                    }
                    long result = objCVerifone.InsertEmployee(dt);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "Employee()", "Verifone Data not inserted in BoF", "Employee", "");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "Employee()", "Verifone Data inserted Successfully in BoF", "Employee", "");
                    }
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Employee()", "CashierUser Exception : " + ex, "Employee", "");
            }
        }

        public void UserWithRole()
        {
            _CVerifone objCVerifone = new _CVerifone();
            bool isSecureUserAdmin = false;
            string secureUserID = "";
            try
            {
                #region Save data from Verifone
                objComman.GetXMLResult("", "UserRole", "POST", "vuseradmin");
                #endregion

                var FilePathCashierUser = AppDomain.CurrentDomain.BaseDirectory + "xml\\UserRole.xml";

                var FileExistsCashierUser = System.IO.File.Exists(FilePathCashierUser);
                if (FileExistsCashierUser == true)
                {
                    #region insert log
                    objCVerifone.InsertActiveLog("BoF", "Start", "UserWithRole()", "Initialize to Insert Verifone Data into BoF", "UserWithRole", "");
                    #endregion

                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\UserRole.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.userConfig));
                    RapidVerifone.userConfig resultingMessage = (RapidVerifone.userConfig)serializer.Deserialize(new XmlTextReader(path));

                    #region Function
                    RapidVerifone.functionsType objfunctionsType = new RapidVerifone.functionsType();
                    objfunctionsType = resultingMessage.Items[0] as functionsType;

                    DataTable dtFunction = new DataTable();
                    dtFunction.Columns.AddRange(new DataColumn[2]
                    {
                        new DataColumn("FunctionName", typeof(string)), new DataColumn("FunctionCmd", typeof(string))
                    });

                    for (int i = 0; i < objfunctionsType.function.Length; i++)
                    {
                        dtFunction.Rows.Add
                        (
                            Convert.ToString(objfunctionsType.function[i].name),
                            objfunctionsType.function[i].cmd
                        );
                    }
                    #endregion

                    #region Role and Role Function
                    RapidVerifone.rolesType objrolesType = new RapidVerifone.rolesType();
                    objrolesType = resultingMessage.Items[1] as rolesType;

                    DataTable dtRole = new DataTable();
                    dtRole.Columns.AddRange(new DataColumn[2]
                    {
                        new DataColumn("RoleName", typeof(string)),new DataColumn("isSecureRole", typeof(bool))
                    });

                    DataTable dtRoleFunction = new DataTable();
                    dtRoleFunction.Columns.AddRange(new DataColumn[3]
                    {
                        new DataColumn("RoleName", typeof(string)),new DataColumn("FunctionName", typeof(string)), new DataColumn("FunctionCmd", typeof(string))
                    });

                    for (int i = 0; i < objrolesType.role.Length; i++)
                    {
                        #region Role
                        dtRole.Rows.Add
                        (
                            objrolesType.role[i].name,
                            objrolesType.role[i].isSecureRole
                        );
                        #endregion

                        RapidVerifone.functionType[] objfunctionType_validFns = new RapidVerifone.functionType[objrolesType.role[i].validFns.Count()];
                        objfunctionType_validFns = objrolesType.role[i].validFns;

                        #region Role Function
                        for (int j = 0; j < objfunctionType_validFns.Length; j++)
                        {
                            dtRoleFunction.Rows.Add
                            (
                                objrolesType.role[i].name,
                                objfunctionType_validFns[j].name,
                                objfunctionType_validFns[j].cmd
                            );
                        }
                        #endregion
                    }
                    #endregion

                    #region User and User Role
                    RapidVerifone.usersType objusersType = new RapidVerifone.usersType();
                    objusersType = resultingMessage.Items[2] as usersType;

                    DataTable dtUser = new DataTable();
                    dtUser.Columns.AddRange(new DataColumn[9]
                    {
                        new DataColumn("Name", typeof(string)), new DataColumn("isDisallowLogin", typeof(bool)), new DataColumn("Expire", typeof(bool)),
                        new DataColumn("Freq", typeof(string)) , new DataColumn("MinLen", typeof(string)), new DataColumn("MaxLen", typeof(string)),
                        new DataColumn("Employee", typeof(string)), new DataColumn("isSecureUserAdmin", typeof(bool)), new DataColumn("secureUserID", typeof(string))
                    });

                    DataTable dtUserRole = new DataTable();
                    dtUserRole.Columns.AddRange(new DataColumn[2]
                    {
                        new DataColumn("Name", typeof(string)), new DataColumn("RoleName", typeof(string))
                    });

                    for (int i = 0; i < objusersType.user.Length; i++)
                    {
                        userPasswd objuserPasswd = new userPasswd();
                        objuserPasswd = objusersType.user[i].passwd;
                        //userSecureUserID objuserSecureUserID = new userSecureUserID();
                        //objuserSecureUserID.isSecureUserAdmin = objusersType.user[i].secureUserID.isSecureUserAdmin;

                        isSecureUserAdmin = objusersType.user[i].secureUserID != null ? objusersType.user[i].secureUserID.isSecureUserAdmin : false;
                        secureUserID = objusersType.user[i].secureUserID != null ? objusersType.user[i].secureUserID.Value : "";

                        #region User
                        dtUser.Rows.Add
                        (
                            objusersType.user[i].name,
                            objusersType.user[i].isDisallowLogin,
                            Convert.ToInt32(objuserPasswd.expire), // bool
                            Convert.ToInt32(objuserPasswd.freq), //string
                            Convert.ToInt32(objuserPasswd.minLen), //string
                            Convert.ToInt32(objuserPasswd.maxLen), //string
                            objusersType.user[i].employee,
                            isSecureUserAdmin,
                            secureUserID
                        );
                        #endregion

                        int usersroleCount = objusersType.user[i].validRoles.Count();
                        RapidVerifone.roleType[] objroleTypeUser = new RapidVerifone.roleType[usersroleCount];
                        objroleTypeUser = objusersType.user[i].validRoles;

                        for (int j = 0; j < objroleTypeUser.Length; j++)
                        {
                            #region User Role
                            dtUserRole.Rows.Add
                            (
                                Convert.ToString(objusersType.user[i].name),
                                objroleTypeUser[j].name
                            );
                            #endregion
                        }
                    }
                    #endregion

                    long result = objCVerifone.InsertUserFunctionRole(dtFunction, dtRole, dtRoleFunction, dtUser, dtUserRole);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "UserWithRole()", "Verifone Data not inserted in BoF", "UserWithRole", "");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "UserWithRole()", "Verifone Data inserted Successfully in BoF", "UserWithRole", "");
                    }
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "UserWithRole()", "UserWithRole Exception : " + ex, "UserWithRole ", "");
            }
        }

        public void CashierUser()
        {
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                #region Save CashierUser data from Verifone
                objComman.GetXMLResult("", "CashierUser", "POST", "vpossecurity");
                #endregion

                var FilePathCashierUser = AppDomain.CurrentDomain.BaseDirectory + "xml\\CashierUser.xml";

                var FileExistsCashierUser = System.IO.File.Exists(FilePathCashierUser);
                if (FileExistsCashierUser == true)
                {
                    #region insert log
                    objCVerifone.InsertActiveLog("BoF", "Start", "CashierUser()", "Initialize to Insert Verifone Data into BoF", "CashierUser", "");
                    #endregion

                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\CashierUser.xml";

                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifone.posSecurity));

                    RapidVerifone.posSecurity resultingMessage = (RapidVerifone.posSecurity)serializer.Deserialize(new XmlTextReader(path));

                    int EmployeeCount = resultingMessage.employees.Count();
                    RapidVerifone.employeeType[] objEmployee = new RapidVerifone.employeeType[EmployeeCount];
                    objEmployee = resultingMessage.employees;

                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[3]{
                        new DataColumn("sysid", typeof(Int64)), new DataColumn("name", typeof(string)), new DataColumn("number", typeof(string))
                    });

                    for (int i = 0; i < objEmployee.Length; i++)
                    {
                        dt.Rows.Add(
                            Convert.ToInt64(objEmployee[i].sysid),
                            objEmployee[i].name,
                            objEmployee[i].number
                            );
                    }
                    long result = objCVerifone.InsertCashierUser(dt);

                    if (result == 0)
                    {
                        objCVerifone.InsertActiveLog("BoF", "Fail", "CashierUser()", "Verifone Data not inserted in BoF", "CashierUser", "");
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "CashierUser()", "Verifone Data inserted Successfully in BoF", "CashierUser", "");
                    }
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "CashierUser()", "CashierUser Exception : " + ex, "CashierUser", "");
            }
        }

        public void PromotionNew()
        {
            #region declare variables
            _CVerifone objCVerifone = new _CVerifone();
            DataSet ds_MixMatch = new DataSet();
            DataSet ds_ItemList = new DataSet();
            string FilePath_MixMatch = "", FilePath_ItemList = "", PromotionID = "";
            int WeekdayAvailability = 0;
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();
            var finalString = "";
            int countMixMatchMaintenance = 0, countPromotion = 0;
            //int DiscountId = 0;
            int FreeType = 0;
            string ItemChoiceType = "";
            decimal Price = 0, TaxAmount = 0;
            bool isSun = false, isMon = false, isTue = false, isWed = false, isThurs = false, isFri = false, isSat = false;
            DateTime Sun_StartTime = DateTime.Now, Sun_EndTime = DateTime.Now, Mon_StartTime = DateTime.Now, Mon_EndTime = DateTime.Now,
                     Tue_StartTime = DateTime.Now, Tue_EndTime = DateTime.Now, Wed_StartTime = DateTime.Now, Wed_EndTime = DateTime.Now, Thurs_StartTime = DateTime.Now,
                     Thurs_EndTime = DateTime.Now, Fri_StartTime = DateTime.Now, Fri_EndTime = DateTime.Now, Sat_StartTime = DateTime.Now, Sat_EndTime = DateTime.Now;
            #endregion

            #region declare tables
            DataTable dt_MixMatch = new DataTable();
            dt_MixMatch.Columns.AddRange(new DataColumn[35]{
                    new DataColumn("PromotionID", typeof(string)), new DataColumn("MixMatchDescription", typeof(string)), new DataColumn("StartDate", typeof(DateTime)),
                    new DataColumn("StopDate", typeof(DateTime)), new DataColumn("StartTime", typeof(DateTime)), new DataColumn("StopTime", typeof(DateTime)),
                    new DataColumn("WeekdayAvailability", typeof(int)), new DataColumn("MixMatchUnits", typeof(float)), new DataColumn("MixMatchPrice", typeof(decimal)),
                    new DataColumn("Code", typeof(string)), new DataColumn("FreeType", typeof(int)), new DataColumn("ItemChoiceType", typeof(string)),
                    new DataColumn("ItemListID", typeof(Int64)), new DataColumn("TaxAmount", typeof(decimal)),
                    new DataColumn("Sun_StartTime", typeof(DateTime)),new DataColumn("Sun_EndTime", typeof(DateTime)),new DataColumn("Mon_StartTime", typeof(DateTime)),
                    new DataColumn("Mon_EndTime", typeof(DateTime)),new DataColumn("Tue_StartTime", typeof(DateTime)),new DataColumn("Tue_EndTime", typeof(DateTime)),
                    new DataColumn("Wed_StartTime", typeof(DateTime)),new DataColumn("Wed_EndTime", typeof(DateTime)),new DataColumn("Thurs_StartTime", typeof(DateTime)),
                    new DataColumn("Thurs_EndTime", typeof(DateTime)),new DataColumn("Fri_StartTime", typeof(DateTime)),new DataColumn("Fri_EndTime", typeof(DateTime)),
                    new DataColumn("Sat_StartTime", typeof(DateTime)), new DataColumn("Sat_EndTime", typeof(DateTime)), new DataColumn("isSun", typeof(bool)),
                    new DataColumn("isMon", typeof(bool)), new DataColumn("isTue", typeof(bool)), new DataColumn("isWed", typeof(bool)),
                    new DataColumn("isThurs", typeof(bool)), new DataColumn("isFri", typeof(bool)), new DataColumn("isSat", typeof(bool))});

            DataTable dt_ItemList = new DataTable();
            dt_ItemList.Columns.AddRange(new DataColumn[5]{
                    new DataColumn("PromotionID", typeof(string)), new DataColumn("POSCode", typeof(string)), new DataColumn("FreeType", typeof(int)),
                    new DataColumn("Free", typeof(decimal)), new DataColumn("POSCodeModifier", typeof(string))});

            DataTable dt_Tax = new DataTable();
            dt_Tax.Columns.AddRange(new DataColumn[4]{
                    new DataColumn("PromotionID", typeof(string)), new DataColumn("TaxID", typeof(Int64)), new DataColumn("FreeType", typeof(int)),
                    new DataColumn("Free", typeof(int))});
            #endregion

            try
            {
                #region Save MixMatch data from Verifone
                objComman.GetXMLResult("", "MixMatch", "POST", "vMaintenance");
                #endregion

                #region Save MixMatch_ItemList data from Verifone
                objComman.GetXMLResult("", "MixMatch_ItemList", "POST", "vMaintenance");
                #endregion

                #region xml to dataset : MixMatch and ItemList
                FilePath_MixMatch = AppDomain.CurrentDomain.BaseDirectory + "xml/MixMatch.xml";
                FilePath_ItemList = AppDomain.CurrentDomain.BaseDirectory + "xml/MixMatch_ItemList.xml";

                if (File.Exists(FilePath_MixMatch) == true && File.Exists(FilePath_ItemList) == true)
                {
                    #region insert log
                    objCVerifone.InsertActiveLog("BoF", "Start", "Promotion()", "Initialize to Insert Verifone Data into BoF", "Promotion", "");
                    #endregion

                    var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\MixMatch.xml";
                    //var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\MixMatch - Copy.xml";
                    XmlSerializer serializer = new XmlSerializer(typeof(RapidVerifoneNAXML.NAXMLMaintenanceRequest));
                    RapidVerifoneNAXML.NAXMLMaintenanceRequest resultingMessage = (RapidVerifoneNAXML.NAXMLMaintenanceRequest)serializer.Deserialize(new XmlTextReader(path));

                    var pathItem = AppDomain.CurrentDomain.BaseDirectory + "xml\\MixMatch_ItemList.xml";
                    //var pathItem = AppDomain.CurrentDomain.BaseDirectory + "xml\\MixMatch_ItemList - Copy.xml";
                    XmlSerializer serializerItem = new XmlSerializer(typeof(RapidVerifoneNAXML.NAXMLMaintenanceRequest));
                    RapidVerifoneNAXML.NAXMLMaintenanceRequest resultingMessageItem = (RapidVerifoneNAXML.NAXMLMaintenanceRequest)serializer.Deserialize(new XmlTextReader(pathItem));

                    RapidVerifoneNAXML.ItemListMaintenance[] objItemListMaintenance = new RapidVerifoneNAXML.ItemListMaintenance[resultingMessageItem.ItemListMaintenance.Count()];
                    objItemListMaintenance = resultingMessageItem.ItemListMaintenance;

                    #region MixMatchMaintenance
                    int countman = resultingMessage.MixMatchMaintenance.Count();
                    RapidVerifoneNAXML.MixMatchMaintenance[] objMixMatchMaintenance = new RapidVerifoneNAXML.MixMatchMaintenance[countman];

                    objMixMatchMaintenance = resultingMessage.MixMatchMaintenance;

                    if (objMixMatchMaintenance != null)
                    {
                        for (int i = 0; i < objMixMatchMaintenance.Length; i++)
                        {
                            if (objMixMatchMaintenance[i].MMTDetail != null && objMixMatchMaintenance[i].MMTDetail.Length > 0)
                            {
                                #region MMTDetail
                                countMixMatchMaintenance = objMixMatchMaintenance[i].MMTDetail.Count();
                                RapidVerifoneNAXML.MMTDetailType[] objMMTDetailType = new RapidVerifoneNAXML.MMTDetailType[countMixMatchMaintenance];
                                objMMTDetailType = objMixMatchMaintenance[i].MMTDetail;

                                if (objMMTDetailType != null)
                                {
                                    for (int j = 0; j < objMMTDetailType.Length; j++)
                                    {
                                        WeekdayAvailability = 0;
                                        #region Promotion
                                        PromotionID = "";
                                        countPromotion = objMMTDetailType[j].Promotion.Count();
                                        RapidVerifoneNAXML.Promotion[] objPromotion = new RapidVerifoneNAXML.Promotion[countPromotion];
                                        objPromotion = objMMTDetailType[j].Promotion;

                                        if (objPromotion != null)
                                        {
                                            for (int k = 0; k < objPromotion.Length; k++)
                                            {
                                                if (PromotionID == "")
                                                {
                                                    PromotionID = objPromotion[k].PromotionID.Value;
                                                }
                                                else
                                                {
                                                    PromotionID = PromotionID + "," + objPromotion[k].PromotionID.Value;
                                                }
                                            }
                                        }
                                        #endregion

                                        #region ItemListID
                                        string[] itemlist = objMMTDetailType[j].ItemListID;
                                        #endregion

                                        #region weekdays
                                        //WeekdayAvailability = Weeklycount(objMMTDetailType[j]);
                                        RapidVerifoneNAXML.yesNo objyesNo = new RapidVerifoneNAXML.yesNo();
                                        for (int n = 0; n < objMMTDetailType[j].WeekdayAvailability.Length; n++)
                                        {
                                            if (objMMTDetailType[j].WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Sunday)
                                            {
                                                objyesNo = objMMTDetailType[j].WeekdayAvailability[n].available;

                                                isSun = objyesNo.ToString().ToLower() == "yes" ? true : false;
                                                Sun_StartTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].startTime).ToString("HH:mm:ss"));
                                                Sun_EndTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].stopTime).ToString("HH:mm:ss"));

                                                if (isSun == true)
                                                {
                                                    WeekdayAvailability += Convert.ToInt16(WeekDays.Sunday);    // valid days
                                                }
                                            }
                                            else if (objMMTDetailType[j].WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Monday)
                                            {
                                                objyesNo = objMMTDetailType[j].WeekdayAvailability[n].available;

                                                isMon = objyesNo.ToString().ToLower() == "yes" ? true : false;
                                                Mon_StartTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].startTime).ToString("HH:mm:ss"));
                                                Mon_EndTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].stopTime).ToString("HH:mm:ss"));
                                            }
                                            else if (objMMTDetailType[j].WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Tuesday)
                                            {
                                                objyesNo = objMMTDetailType[j].WeekdayAvailability[n].available;

                                                isTue = objyesNo.ToString().ToLower() == "yes" ? true : false;
                                                Tue_StartTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].startTime).ToString("HH:mm:ss"));
                                                Tue_EndTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].stopTime).ToString("HH:mm:ss"));

                                                if (isTue == true)
                                                {
                                                    WeekdayAvailability += Convert.ToInt16(WeekDays.Tuesday);    // valid days
                                                }
                                            }
                                            else if (objMMTDetailType[j].WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Wednesday)
                                            {
                                                objyesNo = objMMTDetailType[j].WeekdayAvailability[n].available;

                                                isWed = objyesNo.ToString().ToLower() == "yes" ? true : false;
                                                Wed_StartTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].startTime).ToString("HH:mm:ss"));
                                                Wed_EndTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].stopTime).ToString("HH:mm:ss"));

                                                if (isWed == true)
                                                {
                                                    WeekdayAvailability += Convert.ToInt16(WeekDays.Wednesday);    // valid days
                                                }
                                            }
                                            else if (objMMTDetailType[j].WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Thursday)
                                            {
                                                objyesNo = objMMTDetailType[j].WeekdayAvailability[n].available;

                                                isThurs = objyesNo.ToString().ToLower() == "yes" ? true : false;
                                                Thurs_StartTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].startTime).ToString("HH:mm:ss"));
                                                Thurs_EndTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].stopTime).ToString("HH:mm:ss"));

                                                if (isThurs == true)
                                                {
                                                    WeekdayAvailability += Convert.ToInt16(WeekDays.Thursday);    // valid days
                                                }
                                            }
                                            else if (objMMTDetailType[j].WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Friday)
                                            {
                                                objyesNo = objMMTDetailType[j].WeekdayAvailability[n].available;

                                                isFri = objyesNo.ToString().ToLower() == "yes" ? true : false;
                                                Fri_StartTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].startTime).ToString("HH:mm:ss"));
                                                Fri_EndTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].stopTime).ToString("HH:mm:ss"));

                                                if (isFri == true)
                                                {
                                                    WeekdayAvailability += Convert.ToInt16(WeekDays.Friday);    // valid days
                                                }
                                            }
                                            else if (objMMTDetailType[j].WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Saturday)
                                            {
                                                objyesNo = objMMTDetailType[j].WeekdayAvailability[n].available;

                                                isSat = objyesNo.ToString().ToLower() == "yes" ? true : false;
                                                Sat_StartTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].startTime).ToString("HH:mm:ss"));
                                                Sat_EndTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].WeekdayAvailability[n].stopTime).ToString("HH:mm:ss"));

                                                if (isSat == true)
                                                {
                                                    WeekdayAvailability += Convert.ToInt16(WeekDays.Saturday);    // valid days
                                                }
                                            }
                                        }
                                        #endregion

                                        #region MixMatchEntry
                                        RapidVerifoneNAXML.MixMatchEntry[] objMixMatchEntry = new RapidVerifoneNAXML.MixMatchEntry[objMMTDetailType[j].MixMatchEntry.Count()];
                                        objMixMatchEntry = objMMTDetailType[j].MixMatchEntry;
                                        if (objMixMatchEntry != null)
                                        {
                                            for (int m = 0; m < objMixMatchEntry.Length; m++)
                                            {
                                                #region FreeType
                                                string itemvalue = "";
                                                if (objMixMatchEntry[m].ItemElementName == RapidVerifoneNAXML.ItemChoiceType.MixMatchDiscountAmount)
                                                {
                                                    itemvalue = Convert.ToString(objMixMatchEntry[m].MixMatchUnits.Value);
                                                    RapidVerifoneNAXML.amount12 obja = (RapidVerifoneNAXML.amount12)objMixMatchEntry[m].Item;
                                                    Price = obja.Value;
                                                    FreeType = 1;
                                                    ItemChoiceType = "Amount Off Package Price";
                                                }
                                                if (objMixMatchEntry[m].ItemElementName == RapidVerifoneNAXML.ItemChoiceType.MixMatchDiscountPercent)
                                                {
                                                    itemvalue = Convert.ToString(objMixMatchEntry[m].MixMatchUnits.Value);
                                                    Price = Convert.ToDecimal(objMixMatchEntry[m].Item);
                                                    FreeType = 2;
                                                    ItemChoiceType = "Percent Off Package Price";
                                                }
                                                if (objMixMatchEntry[m].ItemElementName == RapidVerifoneNAXML.ItemChoiceType.MixMatchPrice)
                                                {
                                                    itemvalue = Convert.ToString(objMixMatchEntry[m].MixMatchUnits.Value);
                                                    RapidVerifoneNAXML.amount12 obja = (RapidVerifoneNAXML.amount12)objMixMatchEntry[m].Item;
                                                    Price = obja.Value;
                                                    FreeType = 4;
                                                    ItemChoiceType = "Total Package Price";
                                                }
                                                #endregion

                                                if (PromotionID != "")
                                                {
                                                    string[] PromotionIDCount = PromotionID.Split(',');
                                                    if (PromotionIDCount != null)
                                                    {
                                                        for (int L = 0; L < PromotionIDCount.Length; L++)
                                                        {
                                                            //DiscountId = DiscountId + 1;
                                                            objComman = new Comman();

                                                            #region code (auto generated)
                                                            for (int a = 0; a < stringChars.Length; a++)
                                                            {
                                                                stringChars[a] = chars[random.Next(chars.Length)];
                                                            }
                                                            finalString = new String(stringChars);
                                                            #endregion

                                                            #region Tax
                                                            transmissionHeaderExtension obj = new transmissionHeaderExtension();
                                                            obj = objMMTDetailType[j].Items[0] as transmissionHeaderExtension;

                                                            XmlRootAttribute xRoot = new XmlRootAttribute();
                                                            xRoot.ElementName = "TaxableRebate";
                                                            xRoot.Namespace = "urn:vfi-sapphire:np.naxmlext.2005-06-24";
                                                            xRoot.IsNullable = true;
                                                            var dataElement = DeserializeXMLClass<TaxableRebate>(obj.Any[0].OuterXml, xRoot);

                                                            TaxableRebate taxableRebate = dataElement;
                                                            TaxAmount = taxableRebate.Amount.Value;

                                                            if (taxableRebate.Tax != null)
                                                            {
                                                                for (int t = 0; t < taxableRebate.Tax.Length; t++)
                                                                {
                                                                    dt_Tax.Rows.Add(
                                                                    PromotionID,
                                                                    taxableRebate.Tax[t].sysid,
                                                                    FreeType,
                                                                    Price
                                                                    );
                                                                }
                                                            }
                                                            #endregion

                                                            #region dt_MixMatch
                                                            dt_MixMatch.Rows.Add(
                                                            PromotionIDCount[L],
                                                            objMMTDetailType[j].MixMatchDescription,
                                                            objComman.SplitDate(Convert.ToString(objMMTDetailType[j].StartDate)),
                                                            objComman.SplitDate(Convert.ToString(objMMTDetailType[j].StopDate)),
                                                            Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].StartTime).ToString("HH:mm:ss")),
                                                            Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + (objMMTDetailType[j].StopTime).ToString("HH:mm:ss")),
                                                            WeekdayAvailability,
                                                            itemvalue,
                                                            Price,
                                                            finalString,
                                                            FreeType,
                                                            ItemChoiceType,
                                                            itemlist[0],
                                                            TaxAmount,
                                                            Sun_StartTime,
                                                            Sun_EndTime,
                                                            Mon_StartTime,
                                                            Mon_EndTime,
                                                            Tue_StartTime,
                                                            Tue_EndTime,
                                                            Wed_StartTime,
                                                            Wed_EndTime,
                                                            Thurs_StartTime,
                                                            Thurs_EndTime,
                                                            Fri_StartTime,
                                                            Fri_EndTime,
                                                            Sat_StartTime,
                                                            Sat_EndTime,
                                                            isSun,
                                                            isMon,
                                                            isTue,
                                                            isWed,
                                                            isThurs,
                                                            isFri,
                                                            isSat
                                                            );
                                                            #endregion

                                                            #region ItemList
                                                            if (itemlist != null)
                                                            {
                                                                //CallPromotionItem(objItemListMaintenance, itemlist[0], DiscountId, dt_ItemList, PromotionIDCount[L], FreeType);
                                                                CallPromotionItem(objItemListMaintenance, itemlist[0], dt_ItemList, PromotionIDCount[L], FreeType, Price);
                                                            }
                                                            #endregion
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        #endregion


                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                }

                long result = objCVerifone.InsertPromotion(dt_MixMatch, dt_ItemList, dt_Tax);
                if (result == 0)
                {
                    objCVerifone.InsertActiveLog("BoF", "Fail", "Promotion()", "Verifone Data not inserted in BoF", "Promotion", "");
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "End", "Promotion()", "Verifone Data inserted Successfully in BoF", "Promotion", "");
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Promotion()", "Promotion Exception : " + ex, "Promotion", "");
            }
        }

        public T DeserializeXMLClass<T>(string input, XmlRootAttribute xRoot) where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(T), xRoot);
            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        public int Weeklycount(RapidVerifoneNAXML.MMTDetailType objMMTDetailType)
        {
            int WeekalyCount = 0;
            try
            {
                #region Total WeekDays
                for (int n = 0; n < objMMTDetailType.WeekdayAvailability.Length; n++)
                {

                    if (objMMTDetailType.WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Sunday)
                    {
                        WeekalyCount += Convert.ToInt16(WeekDays.Sunday);
                    }
                    if (objMMTDetailType.WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Monday)
                    {
                        WeekalyCount += Convert.ToInt16(WeekDays.Monday);
                    }
                    if (objMMTDetailType.WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Tuesday)
                    {
                        WeekalyCount += Convert.ToInt16(WeekDays.Tuesday);
                    }
                    if (objMMTDetailType.WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Wednesday)
                    {
                        WeekalyCount += Convert.ToInt16(WeekDays.Wednesday);
                    }
                    if (objMMTDetailType.WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Thursday)
                    {
                        WeekalyCount += Convert.ToInt16(WeekDays.Thursday);
                    }
                    if (objMMTDetailType.WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Friday)
                    {
                        WeekalyCount += Convert.ToInt16(WeekDays.Friday);
                    }
                    if (objMMTDetailType.WeekdayAvailability[n].weekday == RapidVerifoneNAXML.dayOfWeek.Saturday)
                    {
                        WeekalyCount += Convert.ToInt16(WeekDays.Saturday);
                    }
                }
                #endregion

                return WeekalyCount;
            }
            catch (Exception)
            {
                return 0;
            }
        }


        public void CallPromotionItem(RapidVerifoneNAXML.ItemListMaintenance[] objItemListMaintenance, string item, DataTable dt_ItemList, string PromotionID,
                                      int FreeType, decimal Price)
        {
            try
            {
                if (objItemListMaintenance != null)
                {
                    for (int E = 0; E < objItemListMaintenance.Length; E++)
                    {
                        RapidVerifoneNAXML.ILTDetailType[] objILTDetailType = new RapidVerifoneNAXML.ILTDetailType[objItemListMaintenance[E].ILTDetail.Count()];
                        objILTDetailType = objItemListMaintenance[E].ILTDetail;
                        if (objILTDetailType != null)
                        {
                            for (int F = 0; F < objILTDetailType.Length; F++)
                            {
                                //var results = Array.FindAll(objILTDetailType[F].ItemListID, s => s.Equals(item));

                                if (objILTDetailType[F].ItemListID == item)
                                {
                                    RapidVerifoneNAXML.ItemListEntry[] objItemListEntry = new RapidVerifoneNAXML.ItemListEntry[objILTDetailType[F].ItemListID.Count()];
                                    objItemListEntry = objILTDetailType[F].ItemListEntry;

                                    RapidVerifoneNAXML.ItemCode objItemCode = new RapidVerifoneNAXML.ItemCode();

                                    for (int G = 0; G < objItemListEntry.Length; G++)
                                    {
                                        objItemCode = new RapidVerifoneNAXML.ItemCode();
                                        objItemCode = objItemListEntry[G].ItemCode;
                                        dt_ItemList.Rows.Add(
                                            PromotionID,
                                            objItemCode.POSCode,
                                            FreeType,
                                            Price,
                                            objItemCode.POSCodeModifier.Value
                                           );

                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Promotion()
        {
            #region declare variables
            _CVerifone objCVerifone = new _CVerifone();
            DataSet ds_MixMatch = new DataSet();
            DataSet ds_ItemList = new DataSet();
            string FilePath_MixMatch = "";
            string FilePath_ItemList = "";
            int WeekdayAvailability = 0;
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();
            var finalString = "";

            DataTable dt_MixMatch = new DataTable();
            dt_MixMatch.Columns.AddRange(new DataColumn[10]{
                    new DataColumn("PromotionID", typeof(Int64)), new DataColumn("MixMatchDescription", typeof(string)), new DataColumn("StartDate", typeof(string)),
                    new DataColumn("StopDate", typeof(string)), new DataColumn("StartTime", typeof(string)), new DataColumn("StopTime", typeof(string)),
                    new DataColumn("WeekdayAvailability", typeof(int)), new DataColumn("MixMatchUnits", typeof(float)), new DataColumn("MixMatchPrice", typeof(decimal)),
                    new DataColumn("Code", typeof(string))
                    });

            DataTable dt_ItemList = new DataTable();
            dt_ItemList.Columns.AddRange(new DataColumn[2]{
                    new DataColumn("PromotionID", typeof(Int64)), new DataColumn("POSCode", typeof(string))
                    });
            #endregion

            try
            {
                #region Save MixMatch data from Verifone
                objComman.GetXMLResult("", "MixMatch", "POST", "vMaintenance");
                #endregion

                #region Save MixMatch_ItemList data from Verifone
                objComman.GetXMLResult("", "MixMatch_ItemList", "POST", "vMaintenance");
                #endregion

                #region xml to dataset : MixMatch and ItemList
                FilePath_MixMatch = AppDomain.CurrentDomain.BaseDirectory + "xml/MixMatch.xml";
                FilePath_ItemList = AppDomain.CurrentDomain.BaseDirectory + "xml/MixMatch_ItemList.xml";

                if (File.Exists(FilePath_MixMatch) == true && File.Exists(FilePath_ItemList) == true)
                {
                    #region insert log
                    objCVerifone.InsertActiveLog("BoF", "Start", "Promotion()", "Initialize to Insert Verifone Data into BoF", "Promotion", "");
                    #endregion

                    ds_MixMatch.ReadXml(FilePath_MixMatch, XmlReadMode.InferSchema);
                    ds_ItemList.ReadXml(FilePath_ItemList, XmlReadMode.InferSchema);

                    for (int i = 0; i < ds_MixMatch.Tables[5].Rows.Count; i++)
                    {
                        WeekdayAvailability = 0;
                        finalString = "";
                        DataView dv_PromotionId = ds_MixMatch.Tables[6].DefaultView;
                        dv_PromotionId.RowFilter = ("[" + ds_MixMatch.Tables[6].Columns[1].ColumnName + "] ='" + ds_MixMatch.Tables[5].Rows[i][0] + "'");

                        DataView dv_WeekDays = ds_MixMatch.Tables[9].DefaultView;
                        dv_WeekDays.RowFilter = ("[" + ds_MixMatch.Tables[9].Columns[4].ColumnName + "] ='" + ds_MixMatch.Tables[5].Rows[i][0] + "'");

                        DataView dv_MixMatchEntry = ds_MixMatch.Tables[10].DefaultView;
                        dv_MixMatchEntry.RowFilter = ("[" + ds_MixMatch.Tables[10].Columns[4].ColumnName + "] ='" + ds_MixMatch.Tables[5].Rows[i][0] + "'");

                        #region Total WeekDays
                        for (int j = 0; j < dv_WeekDays.Count; j++)
                        {
                            if ((string)dv_WeekDays[j][1] == "Sunday")
                            {
                                WeekdayAvailability += Convert.ToInt16(WeekDays.Sunday);
                            }
                            if ((string)dv_WeekDays[j][1] == "Monday")
                            {
                                WeekdayAvailability += Convert.ToInt16(WeekDays.Monday);
                            }
                            if ((string)dv_WeekDays[j][1] == "Tuesday")
                            {
                                WeekdayAvailability += Convert.ToInt16(WeekDays.Tuesday);
                            }
                            if ((string)dv_WeekDays[j][1] == "Wednesday")
                            {
                                WeekdayAvailability += Convert.ToInt16(WeekDays.Wednesday);
                            }
                            if ((string)dv_WeekDays[j][1] == "Thursday")
                            {
                                WeekdayAvailability += Convert.ToInt16(WeekDays.Thursday);
                            }
                            if ((string)dv_WeekDays[j][1] == "Friday")
                            {
                                WeekdayAvailability += Convert.ToInt16(WeekDays.Friday);
                            }
                            if ((string)dv_WeekDays[j][1] == "Saturday")
                            {
                                WeekdayAvailability += Convert.ToInt16(WeekDays.Saturday);
                            }
                        }
                        #endregion

                        #region auto generated code
                        for (int a = 0; a < stringChars.Length; a++)
                        {
                            stringChars[a] = chars[random.Next(chars.Length)];
                        }
                        finalString = new String(stringChars);
                        #endregion

                        dt_MixMatch.Rows.Add(
                            Convert.ToInt64(dv_PromotionId[0]["PromotionID"]),
                            ds_MixMatch.Tables[5].Rows[i][1],
                            ds_MixMatch.Tables[5].Rows[i][3],
                            ds_MixMatch.Tables[5].Rows[i][5],
                            ds_MixMatch.Tables[5].Rows[i][4],
                            ds_MixMatch.Tables[5].Rows[i][6],
                            WeekdayAvailability,
                            dv_MixMatchEntry[0]["MixMatchUnits"],
                            dv_MixMatchEntry[0]["MixMatchPrice"],
                            finalString
                            );

                        DataView dv_ILTDetailId = ds_ItemList.Tables[5].DefaultView;
                        dv_ILTDetailId.RowFilter = ("[" + ds_ItemList.Tables[5].Columns[0].ColumnName + "] ='" + ds_MixMatch.Tables[5].Rows[i][2] + "'");

                        DataView dv_ItemListEntryId = ds_ItemList.Tables[6].DefaultView;
                        dv_ItemListEntryId.RowFilter = ("[" + ds_ItemList.Tables[6].Columns[1].ColumnName + "] ='" + dv_ILTDetailId[0]["ILTDetail_Id"] + "'");

                        for (int k = 0; k < dv_ItemListEntryId.Count; k++)
                        {
                            DataView dv_POSCode = ds_ItemList.Tables[7].DefaultView;
                            dv_POSCode.RowFilter = ("[" + ds_ItemList.Tables[7].Columns[3].ColumnName + "] ='" + dv_ItemListEntryId[k]["ItemListEntry_Id"] + "'");

                            dt_ItemList.Rows.Add(
                            Convert.ToInt64(dv_PromotionId[0]["PromotionID"]),
                            dv_POSCode[0]["POSCode"]
                            );
                        }
                    }
                }

                long result = 0;//objCVerifone.InsertPromotion(dt_MixMatch, dt_ItemList);
                if (result == 0)
                {
                    objCVerifone.InsertActiveLog("BoF", "Fail", "Promotion()", "Verifone Data not inserted in BoF", "Promotion", "");
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "End", "Promotion()", "Verifone Data inserted Successfully in BoF", "Promotion", "");
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Promotion()", "Promotion Exception : " + ex, "Promotion", "");
            }
        }

        public void Register()
        {
            _CVerifone objCVerifone = new _CVerifone();
            string FilePath = "";
            DataSet ds_Register = new DataSet();
            DataTable dt_Register = new DataTable();
            dt_Register.Columns.AddRange(new DataColumn[2]{
                    new DataColumn("sysid", typeof(Int64)), new DataColumn("dataSubsetName", typeof(string))
                    });
            try
            {
                #region Read Payload, Save Register data from Verifone
                objComman.GetXMLResult("", "Register", "POST", "vappinfo");
                #endregion

                #region Insert Log
                objCVerifone.InsertActiveLog("BoF", "Start", "Register()", "Initialize to Insert Verifone Data into BoF", "Register", "");
                #endregion

                #region xml to Dataset
                FilePath = AppDomain.CurrentDomain.BaseDirectory + "xml/Register.xml";

                if (File.Exists(FilePath) == true)
                {
                    ds_Register.ReadXml(FilePath, XmlReadMode.InferSchema);
                    if (ds_Register.Tables.Count >= 4)
                    {
                        for (int i = 0; i < ds_Register.Tables[4].Rows.Count; i++)
                        {
                            DataView dv_Register = ds_Register.Tables[2].DefaultView;
                            dv_Register.RowFilter = ("[" + ds_Register.Tables[2].Columns[9].ColumnName + "] ='" + ds_Register.Tables[4].Rows[i][0] + "'");

                            dt_Register.Rows.Add(
                                ds_Register.Tables[4].Rows[i][1],
                                dv_Register[0]["dataSubsetName"]
                                );
                        }
                        long result = objCVerifone.InsertRegister(dt_Register);
                        if (result == 0)
                        {
                            objCVerifone.InsertActiveLog("BoF", "Fail", "Register()", "Verifone Data not inserted in BoF", "Register", "");
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("BoF", "End", "Register()", "Verifone Data inserted Successfully in BoF", "Register", "");
                        }
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "Register()", "Verifone Register data not found", "Register", "");
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Register()", "Register Exception : " + ex, "Register", "");
            }
        }

        public void UpdateUserPassword()
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                long Result = objVerifone.UpdateUserPassword(VerifoneServices.VerifoneLink, VerifoneServices.VerifoneUserName, VerifoneServices.VerifonePassword.Encript());
                if (Result == 0)
                {
                    objVerifone.InsertActiveLog("BoF", "Fail", "UpdateUserPassword()", "Verifone Data not updated in BoF", "UpdateUserPassword", "");
                }
                else
                {
                    objVerifone.InsertActiveLog("BoF", "End", "UpdateUserPassword()", "Verifone Data updated Successfully in BoF", "UpdateUserPassword", "");
                }
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("Verifone", "Error", "UpdateUserPassword()", "UpdateUserPassword  : " + ex.Message, "UpdateUserPassword", "");
            }
        }
        #endregion

        #region Invoice
        public void Invoice_New_WithChanges()
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                #region Get Verifone Period data for invoice
                objComman.GetPayloadXML("LogPeriod", "POST", "vtlogpdlist");
                #endregion

                #region Get Verifone Invoice data using Period
                objComman.GetPayloadXML("Invoice", "POST", "vtransset");
                #endregion

                #region Insert Verifone Invoice Data
                string checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice";
                string[] filePaths = Directory.GetFiles(checkpath);
                string PeriodFilePath = AppDomain.CurrentDomain.BaseDirectory + "Period_FileName.xml";

                if (filePaths.Length > 0)
                {
                    for (int a = 0; a < filePaths.Length; a++)
                    {
                        if (InvoiceFileWise_DifferentType(filePaths[a]))
                        {
                            #region Create new xml
                            if (PeriodForXML != "" && FileNameForXML != "")
                            {
                                if (File.Exists(PeriodFilePath) == false)
                                {
                                    objComman.CreateXML(PeriodForXML, FileNameForXML, PeriodFilePath, "New", "Invoice");
                                }
                                else
                                {
                                    DataSet Xds = new DataSet();
                                    Xds.ReadXml(PeriodFilePath, XmlReadMode.InferSchema);

                                    DataView dvxml = Xds.Tables[0].DefaultView;
                                    dvxml.RowFilter = ("[" + Xds.Tables[0].Columns[0].ColumnName + "] ='" + PeriodForXML + "'AND [" + Xds.Tables[0].Columns[1].ColumnName + "] ='" + FileNameForXML + "'");

                                    if (dvxml.Count == 0 && FileNameForXML != "CURRENT")
                                    {
                                        objComman.CreateXML(PeriodForXML, FileNameForXML, PeriodFilePath, "Exists", "Invoice");
                                    }
                                }
                                PeriodForXML = "";
                                FileNameForXML = "";
                            }
                            #endregion
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice_New_WithChanges()", "Invoice_New_WithChanges Exception : " + ex, "Invoice", "");
            }
        }

        public bool InvoiceFileWise_DifferentType(string FilePath)
        {
            _CVerifone objVerifone = new _CVerifone();
            int Comman_InvoiceNo = 0;
            int DropId = 0;
            try
            {
                #region declare variables
                bool suspended = false, recalled = false, IsLotteryItem = false, IsLottery = false, IsAnyError = false;
                string register = "";
                long registerno = 0;
                int SaleSuspended_OrderNo = 0, trLinesCount = 0, PayLineCount = 0;
                DateTime NoSale_Date = DateTime.Now;
                DateTime TVoid_Date = DateTime.Now;
                #endregion

                DataSet dsreg = objComman.GetRegsiterinv();
                if (dsreg.Tables[0] != null && dsreg.Tables[0].Rows.Count > 0)
                {
                    register = Convert.ToString(dsreg.Tables[0].Rows[0]["reg"]);
                    registerno = Convert.ToInt32(dsreg.Tables[0].Rows[0]["regnum"]);
                    OrderNo = Convert.ToInt32(dsreg.Tables[0].Rows[0]["OrderNo"]);
                    SaleSuspended_OrderNo = Convert.ToInt32(dsreg.Tables[0].Rows[0]["HoldOrderNo"]);
                }

                DataSet dsInvNo = objComman.GetInvoiceNo();
                if (dsInvNo.Tables[0].Rows.Count > 0)
                {
                    Comman_InvoiceNo = Convert.ToInt32(dsInvNo.Tables[0].Rows[0]["Id"]);
                }
                else
                {
                    Comman_InvoiceNo = 0;
                }


                DataSet dsdropamount = objComman.Getdropamountid();
                if (dsdropamount.Tables[0].Rows.Count > 0)
                {
                    DropId = Convert.ToInt32(dsdropamount.Tables[0].Rows[0]["DropId"]);
                }
                else
                {
                    DropId = 0;
                }

                #region declare different datatable to insert

                #region sales invoice

                DataTable dtInvoice = new DataTable();
                dtInvoice.Columns.AddRange(new DataColumn[25]{
                new DataColumn("Id", typeof(int)), new DataColumn("InvoiceDate", typeof(DateTime)), new DataColumn("SubTotal", typeof(decimal)),
                new DataColumn("TaxAmount", typeof(decimal)), new DataColumn("BillAmount", typeof(decimal)), new DataColumn("PayId", typeof(int)),
                new DataColumn("ReturnAmount", typeof(decimal)), new DataColumn("ReturnFlag", typeof(bool)), new DataColumn("RegisterInvNo", typeof(string)),
                new DataColumn("OrderNo", typeof(Int64)), new DataColumn("OrgRegisterInvNo", typeof(string)), new DataColumn("IsVoid", typeof(bool)),
                new DataColumn("UserId", typeof(Int64)),  new DataColumn("TenderAmount", typeof(decimal)),  new DataColumn("PayMode", typeof(string)),
                new DataColumn("CardType", typeof(string)), new DataColumn("AuthCode", typeof(string)), new DataColumn("AccNo", typeof(string)),
                new DataColumn("TransactionNo", typeof(string)), new DataColumn("BatchNo", typeof(string)), new DataColumn("ExtData", typeof(string)),
                new DataColumn("HostCode", typeof(string)), new DataColumn("AuthDateTime", typeof(DateTime)), new DataColumn("TransactionId", typeof(string)),
                new DataColumn("RegisterId", typeof(Int64))
                });





                // change column
                DataTable dtInvoiceItem = new DataTable();
                dtInvoiceItem.Columns.AddRange(new DataColumn[24]{
                new DataColumn("Id", typeof(int)), new DataColumn("ItemNo", typeof(string)), new DataColumn("ItemDesc", typeof(string)),
                new DataColumn("UPC", typeof(string)), new DataColumn("ItemQty", typeof(int)), new DataColumn("ItemAmount", typeof(decimal)),
                new DataColumn("ItemTaxAmount", typeof(decimal)), new DataColumn("TotalItemAmount", typeof(decimal)), new DataColumn("DepartmentId", typeof(int)),
                new DataColumn("RetailAmount", typeof(decimal)), new DataColumn("ReturnFlag", typeof(bool)), new DataColumn("TaxId", typeof(Int64)),
                new DataColumn("TaxPercentage", typeof(decimal)), new DataColumn("RowPosition", typeof(int)), new DataColumn("UserId", typeof(Int64)),
                new DataColumn("ItemType", typeof(string)), new DataColumn("OldPrice", typeof(decimal)), new DataColumn("InvoiceDate", typeof(DateTime)),
                new DataColumn("PromotionAmount", typeof(decimal)), new DataColumn("Modifier", typeof(string)), new DataColumn("ItemBasicRate", typeof(decimal)),
                new DataColumn("RegisterId", typeof(Int64)), new DataColumn("ExtraCharge", typeof(decimal)), new DataColumn("IsMoneyOrder", typeof(bool))
                });



                DataTable dtInvoicePayment = new DataTable();
                dtInvoicePayment.Columns.AddRange(new DataColumn[17]
                {
                new DataColumn("Id", typeof(Int64)), new DataColumn("InvoiceDate", typeof(DateTime)), new DataColumn("TenderAmount", typeof(decimal)),
                new DataColumn("ReturnAmount", typeof(decimal)),new DataColumn("PayId", typeof(int)) ,new DataColumn("PayMode", typeof(string)),
                new DataColumn("UserId", typeof(Int64)), new DataColumn("RegisterId", typeof(Int64)), new DataColumn("CardType", typeof(string)),
                 new DataColumn("AuthCode", typeof(string)),new DataColumn("AccNo", typeof(string)), new DataColumn("TransactionNo", typeof(string)),
                 new DataColumn("BatchNo", typeof(string)),new DataColumn("TransactionId", typeof(string)),new DataColumn("ExtData", typeof(string))
                 ,new DataColumn("HostCode", typeof(string)), new DataColumn("AuthDateTime", typeof(DateTime))
                });


                DataTable dtInvoiceDiscount = new DataTable();
                dtInvoiceDiscount.Columns.AddRange(new DataColumn[8]{
                new DataColumn("InvoiceNo", typeof(int)), new DataColumn("RowPosition", typeof(int)), new DataColumn("UPC", typeof(string)),
                new DataColumn("Amount", typeof(decimal)), new DataColumn("ItemNo", typeof(string)), new DataColumn("ItemDesc", typeof(string)),
                new DataColumn("DiscountId", typeof(Int64)), new DataColumn("ItemType", typeof(string))
                });


                DataTable dtTaxExempt = new DataTable();
                dtTaxExempt.Columns.AddRange(new DataColumn[7]{
                new DataColumn("InvoiceNo", typeof(Int64)), new DataColumn("UPC", typeof(string)), new DataColumn("Date", typeof(DateTime)),
                new DataColumn("RowPosition", typeof(Int64)), new DataColumn("OldValue", typeof(string)), new DataColumn("UserId", typeof(Int64)),
                new DataColumn("RegisterId", typeof(Int64))
                });

                #endregion

                #region NoSales

                DataTable dtNoSale = new DataTable();
                dtNoSale.Columns.AddRange(new DataColumn[4]{
                new DataColumn("InvoiceNo", typeof(Int64)), new DataColumn("Date", typeof(DateTime)), new DataColumn("UserId", typeof(Int64)),
                new DataColumn("Register", typeof(Int64))
                });

                #endregion


                #region dropamount

                DataTable dtDropamount = new DataTable();
                dtDropamount.Columns.AddRange(new DataColumn[6]{
                new DataColumn("DropId", typeof(Int64)), new DataColumn("Amount", typeof(Int64)), new DataColumn("DropDate", typeof(DateTime)), new DataColumn("UserId", typeof(Int64)),
                new DataColumn("Register", typeof(Int64)),new DataColumn("PayId", typeof(Int64))
                });

                #endregion

                #region Void

                DataTable dtVoid = new DataTable();
                dtVoid.Columns.AddRange(new DataColumn[13]{
                new DataColumn("ItemType", typeof(Int64)), new DataColumn("InvoiceNo", typeof(Int64)), new DataColumn("ItemName", typeof(string)),new DataColumn("UPC", typeof(string)),
                new DataColumn("Modifier", typeof(string)), new DataColumn("Date", typeof(DateTime)),
                new DataColumn("RowPosition", typeof(Int64)), new DataColumn("UserId", typeof(Int64)),
                new DataColumn("NewAmount", typeof(decimal)), new DataColumn("OldInvoiceNo", typeof(Int64)), new DataColumn("RegisterId", typeof(Int64)),
                new DataColumn("OldPrice", typeof(decimal)), new DataColumn("ItemQty", typeof(decimal))
                });



                #endregion


                #region SaleSuspened
                DataTable dtSaleSuspened = new DataTable();
                dtSaleSuspened.Columns.AddRange(new DataColumn[9]{
                new DataColumn("InvoiceNo", typeof(Int64)), new DataColumn("InvoiceDate", typeof(DateTime)), new DataColumn("SubTotal", typeof(decimal)),
                new DataColumn("TaxAmount", typeof(decimal)), new DataColumn("BillAmount", typeof(decimal)), new DataColumn("OrderNo", typeof(Int64)),
                new DataColumn("OldInvoiceNo", typeof(Int64)), new DataColumn("UserId", typeof(Int64)), new DataColumn("Register", typeof(Int64))

                });

                // change column
                DataTable dtSalesSuspenedItem = new DataTable();
                dtSalesSuspenedItem.Columns.AddRange(new DataColumn[21]{
                new DataColumn("RowPosition", typeof(int)), new DataColumn("InvoiceNo", typeof(Int64)), new DataColumn("Barcode", typeof(string)),
                new DataColumn("ItemNo", typeof(string)), new DataColumn("ItemDesc", typeof(string)), new DataColumn("ItemQty", typeof(int)),
                new DataColumn("ItemAmount", typeof(decimal)), new DataColumn("ItemTaxAmount", typeof(decimal)), new DataColumn("TotalItemAmount", typeof(decimal)),
                new DataColumn("DepartmentId", typeof(int)), new DataColumn("RetailAmount", typeof(decimal)), new DataColumn("TaxId", typeof(int)),
                new DataColumn("TaxPercentage", typeof(decimal)), new DataColumn("InvoiceDate", typeof(DateTime)), new DataColumn("UserId", typeof(Int64)),
                new DataColumn("Modifier", typeof(string)), new DataColumn("ItemBasicRate", typeof(decimal)), new DataColumn("Register", typeof(Int64)),
                new DataColumn("OldPrice", typeof(decimal)), new DataColumn("ExtraCharge", typeof(decimal)), new DataColumn("IsMoneyOrder", typeof(bool))
                });


                #endregion


                #region SalesRecall

                DataTable dtSaleRecall = new DataTable();
                dtSaleRecall.Columns.AddRange(new DataColumn[14]{
                new DataColumn("OldInvoiceNo", typeof(int)), new DataColumn("InvoiceNo", typeof(int)), new DataColumn("InvoiceDate", typeof(DateTime)),
                new DataColumn("SubTotal", typeof(decimal)), new DataColumn("TaxAmount", typeof(decimal)), new DataColumn("BillAmount", typeof(decimal)),
                new DataColumn("PayId", typeof(int)), new DataColumn("ReturnAmount", typeof(decimal)), new DataColumn("RegisterInvNo", typeof(string)),
                new DataColumn("OrderNo", typeof(Int64)), new DataColumn("UserId", typeof(Int64)), new DataColumn("TenderAmount", typeof(decimal)),
                new DataColumn("PayMode", typeof(string)), new DataColumn("RegisterId", typeof(Int64))
                });

                // change column
                DataTable dtSaleRecallItem = new DataTable();
                dtSaleRecallItem.Columns.AddRange(new DataColumn[21]{
                new DataColumn("InvoiceNo", typeof(int)), new DataColumn("ItemNo", typeof(string)), new DataColumn("ItemDesc", typeof(string)),
                new DataColumn("UPC", typeof(string)), new DataColumn("ItemQty", typeof(int)), new DataColumn("ItemAmount", typeof(decimal)),
                new DataColumn("ItemTaxAmount", typeof(decimal)), new DataColumn("TotalItemAmount", typeof(decimal)), new DataColumn("DepartmentId", typeof(int)),
                new DataColumn("RetailAmount", typeof(decimal)), new DataColumn("TaxId", typeof(Int64)), new DataColumn("TaxPercentage", typeof(decimal)),
                new DataColumn("RowPosition", typeof(int)), new DataColumn("UserId", typeof(Int64)), new DataColumn("ItemType", typeof(string)),
                new DataColumn("PromotionAmount", typeof(decimal)), new DataColumn("Modifier", typeof(string)), new DataColumn("ItemBasicRate", typeof(decimal)),
                new DataColumn("OldPrice", typeof(decimal)), new DataColumn("ExtraCharge", typeof(decimal)), new DataColumn("IsMoneyOrder", typeof(bool))
                });

                DataTable dtSaleRecallPayment = new DataTable();
                dtSaleRecallPayment.Columns.AddRange(new DataColumn[17]
                {
                new DataColumn("Id", typeof(Int64)), new DataColumn("InvoiceDate", typeof(DateTime)), new DataColumn("TenderAmount", typeof(decimal)),
                new DataColumn("ReturnAmount", typeof(decimal)),new DataColumn("PayId", typeof(int)) ,new DataColumn("PayMode", typeof(string)),
                new DataColumn("UserId", typeof(Int64)), new DataColumn("RegisterId", typeof(Int64)), new DataColumn("CardType", typeof(string)),
                 new DataColumn("AuthCode", typeof(string)),new DataColumn("AccNo", typeof(string)), new DataColumn("TransactionNo", typeof(string)),
                 new DataColumn("BatchNo", typeof(string)),new DataColumn("TransactionId", typeof(string)),new DataColumn("ExtData", typeof(string))
                 ,new DataColumn("HostCode", typeof(string)), new DataColumn("AuthDateTime", typeof(DateTime))
                });


                DataTable dtSaleRecall_InvoiceDiscount = new DataTable();
                dtSaleRecall_InvoiceDiscount.Columns.AddRange(new DataColumn[8]{
                new DataColumn("InvoiceNo", typeof(int)), new DataColumn("RowPosition", typeof(int)), new DataColumn("UPC", typeof(string)),
                new DataColumn("Amount", typeof(decimal)), new DataColumn("ItemNo", typeof(string)), new DataColumn("ItemDesc", typeof(string)),
                new DataColumn("DiscountId", typeof(Int64)), new DataColumn("ItemType", typeof(string))
                });

                #endregion

                #region Journal Payout

                DataTable dtJournal = new DataTable();
                dtJournal.Columns.AddRange(new DataColumn[8]{
                    new DataColumn("Date", typeof(DateTime)), new DataColumn("ItemDesc", typeof(string)),
                    new DataColumn("Modifier", typeof(string)),
                    new DataColumn("UPC", typeof(string)), new DataColumn("RowPosition", typeof(Int64)),
                    new DataColumn("UserId", typeof(Int64)), new DataColumn("RegisterId", typeof(Int64)),
                    new DataColumn("NewValue", typeof(decimal))
                });

                // change column
                DataTable dtPayout = new DataTable();
                dtPayout.Columns.AddRange(new DataColumn[18]{
                new DataColumn("Id", typeof(int)), new DataColumn("InvoiceDate", typeof(DateTime)), new DataColumn("SubTotal", typeof(decimal)),
                new DataColumn("TaxAmount", typeof(decimal)), new DataColumn("BillAmount", typeof(decimal)), new DataColumn("PayId", typeof(int)),
                new DataColumn("ReturnAmount", typeof(decimal)), new DataColumn("ReturnFlag", typeof(bool)), new DataColumn("RegisterInvNo", typeof(string)),
                new DataColumn("OrderNo", typeof(Int64)), new DataColumn("OrgRegisterInvNo", typeof(string)), new DataColumn("IsVoid", typeof(bool)),
                new DataColumn("UserId", typeof(Int64)),  new DataColumn("TenderAmount", typeof(decimal)),  new DataColumn("PayMode", typeof(string)),
                new DataColumn("RegisterId", typeof(Int64)), new DataColumn("ExtraCharge", typeof(decimal)), new DataColumn("IsMoneyOrder", typeof(bool))
                });

                #endregion

                #region Sales Lottery

                DataTable dtLotteryInvoice = new DataTable();
                dtLotteryInvoice.Columns.AddRange(new DataColumn[26]{
                new DataColumn("Id", typeof(int)), new DataColumn("InvoiceDate", typeof(DateTime)), new DataColumn("SubTotal", typeof(decimal)),
                new DataColumn("TaxAmount", typeof(decimal)), new DataColumn("BillAmount", typeof(decimal)), new DataColumn("PayId", typeof(int)),
                new DataColumn("ReturnAmount", typeof(decimal)), new DataColumn("ReturnFlag", typeof(bool)), new DataColumn("RegisterInvNo", typeof(string)),
                new DataColumn("OrderNo", typeof(Int64)), new DataColumn("OrgRegisterInvNo", typeof(string)), new DataColumn("IsVoid", typeof(bool)),
                new DataColumn("UserId", typeof(Int64)),  new DataColumn("TenderAmount", typeof(decimal)),  new DataColumn("PayMode", typeof(string)),
                new DataColumn("CardType", typeof(string)), new DataColumn("AuthCode", typeof(string)), new DataColumn("AccNo", typeof(string)),
                new DataColumn("TransactionNo", typeof(string)), new DataColumn("BatchNo", typeof(string)), new DataColumn("ExtData", typeof(string)),
                new DataColumn("HostCode", typeof(string)), new DataColumn("AuthDateTime", typeof(DateTime)), new DataColumn("TransactionId", typeof(string)),
                new DataColumn("RegisterId", typeof(Int64)), new DataColumn("IsLotteryItem", typeof(bool))
                });

                // change column
                DataTable dtLotteryInvoiceItem = new DataTable();
                dtLotteryInvoiceItem.Columns.AddRange(new DataColumn[25]
                {
                new DataColumn("Id", typeof(int)), new DataColumn("ItemNo", typeof(string)), new DataColumn("ItemDesc", typeof(string)),
                new DataColumn("UPC", typeof(string)), new DataColumn("ItemQty", typeof(int)), new DataColumn("ItemAmount", typeof(decimal)),
                new DataColumn("ItemTaxAmount", typeof(decimal)), new DataColumn("TotalItemAmount", typeof(decimal)), new DataColumn("DepartmentId", typeof(int)),
                new DataColumn("RetailAmount", typeof(decimal)), new DataColumn("ReturnFlag", typeof(bool)), new DataColumn("TaxId", typeof(Int64)),
                new DataColumn("TaxPercentage", typeof(decimal)), new DataColumn("RowPosition", typeof(int)), new DataColumn("UserId", typeof(Int64)),
                new DataColumn("ItemType", typeof(string)), new DataColumn("OldPrice", typeof(decimal)), new DataColumn("InvoiceDate", typeof(DateTime)),
                new DataColumn("PromotionAmount", typeof(decimal)), new DataColumn("Modifier", typeof(string)), new DataColumn("ItemBasicRate", typeof(decimal)),
                new DataColumn("RegisterId", typeof(Int64)), new DataColumn("IsLotteryPayout", typeof(bool))
                , new DataColumn(" ", typeof(decimal)), new DataColumn("IsMoneyOrder", typeof(bool))
                });

                #endregion

                #region InvoicePump

                DataTable dtInvoicePump = new DataTable();
                dtInvoicePump.Columns.AddRange(new DataColumn[13]{
                new DataColumn("RowPosition", typeof(int)), new DataColumn("InvoiceNo", typeof(Int64)), new DataColumn("CartId", typeof(string)),
                new DataColumn("PumpId", typeof(Int64)), new DataColumn("FuelId", typeof(Int64)), new DataColumn("ServiceType", typeof(int)),
                new DataColumn("PricePerGallon", typeof(decimal)), new DataColumn("Amount", typeof(decimal)), new DataColumn("Volume", typeof(decimal)),
                new DataColumn("TransactionType", typeof(string)), new DataColumn("SequenceNumber", typeof(string)), new DataColumn("PumpCartStatus", typeof(Int64)),
                new DataColumn("DepartmentId", typeof(int))
                });

                DataTable dtPumpCart = new DataTable();
                dtPumpCart.Columns.AddRange(new DataColumn[26]{
                    new DataColumn("CartId", typeof(string)),  new DataColumn("PumpId", typeof(Int64)), new DataColumn("FuelId", typeof(Int64)),
                    new DataColumn("UserId", typeof(Int64)), new DataColumn("TimeStamp", typeof(DateTime)), new DataColumn("RegInvNum", typeof(string)),
                    new DataColumn("RegisterNumber", typeof(string)), new DataColumn("ServiceType", typeof(Int64)), new DataColumn("PayId", typeof(Int64)),
                    new DataColumn("PricePerGallon", typeof(decimal)), new DataColumn("Amount", typeof(decimal)), new DataColumn("Volume", typeof(decimal)),
                    new DataColumn("PayType", typeof(Int64)), new DataColumn("TransactionType", typeof(string)), new DataColumn("InvoiceNo", typeof(Int64)),
                    new DataColumn("TransactionNo", typeof(string)), new DataColumn("RowPosition", typeof(int)),  new DataColumn("SequenceNumber", typeof(string)),
                    new DataColumn("PumpCartStatus", typeof(Int64)), new DataColumn("parentSequence", typeof(string)), new DataColumn("reason", typeof(string)),
                    new DataColumn("ReleaseToken", typeof(string)), new DataColumn("TransactionSeqNo" , typeof(Int64)), new DataColumn("Odometer", typeof(string)),
                    new DataColumn("Vehicle", typeof(string)), new DataColumn("Driver", typeof(string))
                });

                #endregion

                #endregion

                objVerifone.InsertActiveLog("BoF", "Start", "Invoice()", "Initialize to Insert Verifone Data into BoF : " + Path.GetFileName(FilePath), "Invoice", "");

                XmlSerializer serializer = new XmlSerializer(typeof(myCompany1111.transSet));

                myCompany1111.transSet resultingMessage = (myCompany1111.transSet)serializer.Deserialize(new XmlTextReader(FilePath));


                try
                {
                    PeriodForXML = resultingMessage.periodID;
                    FileNameForXML = resultingMessage.longId + "." + resultingMessage.shortId;
                    if (FileNameForXML == "CURRENT.")
                    {
                        FileNameForXML = "CURRENT";
                    }

                }
                catch (Exception ex)
                {
                    objVerifone.InsertActiveLog("BoF", "Error", "InvoiceFileWise_DifferentType()", "CURRENT Exception : " + ex.Message, "Invoice", "");
                }

                #region File Wise Item Loop
                string dd = "";
                for (int i = 0; i < resultingMessage.Items.Length; i++)
                {
                    var GetType = resultingMessage.Items[i].GetType().Name;
                    if (GetType == "trans")
                    {
                        transactionType objTransactionType = new transactionType();
                        objTransactionType = ((trans)(resultingMessage.Items[i])).type;
                        if (Convert.ToInt16(objTransactionType) != 1)
                        {
                            var transType = ((trans)(resultingMessage.Items[i])).trHeader.trTickNum;

                            if (Convert.ToInt32(((trans)(resultingMessage.Items[i])).type) == 2)
                            {
                                dd = dd + "," + Convert.ToString(((trans)(resultingMessage.Items[i])).trHeader.duration);
                            }

                            trLinesCount = ((trans)(resultingMessage.Items[i])).trLines == null ? 0 : ((trans)(resultingMessage.Items[i])).trLines.Count();
                            myCompany1111.trLine[] objtrLine = new trLine[trLinesCount];
                            objtrLine = ((trans)(resultingMessage.Items[i])).trLines;

                            PayLineCount = ((trans)(resultingMessage.Items[i])).trPaylines == null ? 0 : ((trans)(resultingMessage.Items[i])).trPaylines.Count();
                            trPayline[] objtrPayline = new trPayline[PayLineCount];
                            objtrPayline = ((trans)(resultingMessage.Items[i])).trPaylines;

                            bool PrefuelInvoice = false;

                            #region objtrLine != null
                            if (objtrLine != null)
                            {

                                IsLottery = false;
                                IsLotteryItem = false;

                                #region Type = sale, suspended = false, recall = false (sale = 0, refundsale = 20, refundvoid = 24)
                                if (transType != null && ((objTransactionType == 0 || (Convert.ToInt16(objTransactionType) == 24) ||
                                   (Convert.ToInt16(objTransactionType) == 20) || (Convert.ToInt16(objTransactionType) == 2)))
                                    && ((trans)(resultingMessage.Items[i])).suspended == false && ((trans)(resultingMessage.Items[i])).recalled == false)
                                {
                                    myCompany1111.trLine[] objtrLineCheckType = new trLine[trLinesCount];
                                    objtrLineCheckType = ((trans)(resultingMessage.Items[i])).trLines;

                                    for (int Prefuel = 0; Prefuel < objtrLineCheckType.Length; Prefuel++)
                                    {
                                        if (Convert.ToInt16(objtrLineCheckType[Prefuel].type) == 4)
                                        {
                                            PrefuelInvoice = true;
                                            break;
                                        }
                                    }

                                    if (PrefuelInvoice == false)
                                    {
                                        Comman_InvoiceNo += 1;
                                        //OrderNo += 1;
                                        Sale((trans)(resultingMessage.Items[i]), dtInvoice, dtInvoiceItem, dtInvoiceDiscount, dtLotteryInvoice, dtLotteryInvoiceItem, dtTaxExempt,
                                              Comman_InvoiceNo, dtInvoicePump, dtPumpCart, dtInvoicePayment);
                                    }
                                }
                                #endregion

                                #region Type = void
                                else if (transType != null && Convert.ToInt16(objTransactionType) == 12)
                                {
                                    Void((trans)(resultingMessage.Items[i]), dtVoid);
                                }
                                #endregion

                                #region Type = sale, suspended = true, recall  = false or recall = true (onhold)
                                else if (transType != null && objTransactionType == 0 && ((trans)(resultingMessage.Items[i])).suspended == true && (((trans)(resultingMessage.Items[i])).recalled == false || ((trans)(resultingMessage.Items[i])).recalled == true))
                                {
                                    SaleSuspended_OrderNo += 1;
                                    Suspended((trans)(resultingMessage.Items[i]), dtSaleSuspened, dtSalesSuspenedItem, SaleSuspended_OrderNo, dtTaxExempt);
                                }
                                #endregion

                                #region Type = sale, suspened = false, recall = true
                                else if (transType != null && (objTransactionType == 0 || (Convert.ToInt16(objTransactionType) == 2)) && ((trans)(resultingMessage.Items[i])).suspended == false && ((trans)(resultingMessage.Items[i])).recalled == true)
                                {
                                    Comman_InvoiceNo += 1;
                                    //Recall((trans)(resultingMessage.Items[i]), dtSaleRecall, dtSaleRecallItem, dtSaleRecall_InvoiceDiscount, Comman_InvoiceNo, dtSaleRecallPayment);
                                    Recall((trans)(resultingMessage.Items[i]), dtSaleRecall, dtSaleRecallItem, dtSaleRecall_InvoiceDiscount, dtLotteryInvoice,
                                    dtLotteryInvoiceItem, dtTaxExempt, Comman_InvoiceNo, dtInvoicePump, dtPumpCart, dtSaleRecallPayment);
                                }
                                #endregion

                            }
                            #endregion

                            #region Type = nosale
                            else if (transType != null && Convert.ToInt16(objTransactionType) == 11)
                            {
                                NoSale((trans)(resultingMessage.Items[i]), dtNoSale);
                            }
                            #endregion

                            #region Type = payout
                            else if (transType != null && Convert.ToInt16(objTransactionType) == 9)
                            {
                                Comman_InvoiceNo += 1;
                                Payout((trans)(resultingMessage.Items[i]), dtPayout, Comman_InvoiceNo);
                            }
                            #endregion

                            #region Lottery without item
                            else if (transType != null && Convert.ToInt16(objTransactionType) == 0 && PrefuelInvoice == false)
                            {
                                if (objtrPayline != null)
                                {
                                    for (int k = 0; k < objtrPayline.Length; k++)
                                    {
                                        if (objtrPayline[k].trpPaycode.Value == "LOTTERY")
                                        {
                                            IsLotteryItem = false;
                                            Comman_InvoiceNo += 1;
                                            LotteryWithoutItem((trans)(resultingMessage.Items[i]), dtLotteryInvoice, Comman_InvoiceNo, IsLotteryItem, OrderNo, dtInvoicePayment);
                                        }
                                    }
                                }
                            }
                            #endregion


                            #region Type = payout
                            else if (transType != null && Convert.ToInt16(objTransactionType) == 10)
                            {
                                DropId = DropId + 1;
                                drpoAmountData((trans)(resultingMessage.Items[i]), dtDropamount, DropId);
                            }
                            #endregion
                        }

                        #region Type = journal - trline - plu
                        else if (Convert.ToInt16(objTransactionType) == 1)
                        {
                            Journal((trans)(resultingMessage.Items[i]), dtJournal);
                        }
                        else
                        {
                            // not require 
                        }
                        #endregion
                    }
                }
                #endregion

                #region insert into database
                #region sale, refund sale, refund void, lottery with item
                if ((dtInvoice.Rows.Count > 0 && dtInvoiceItem.Rows.Count > 0) || (dtLotteryInvoice.Rows.Count > 0 && dtLotteryInvoiceItem.Rows.Count > 0))
                {
                    long Result_Sale = objVerifone.InsertInvoice_Sale(dtInvoice, dtInvoiceItem, dtInvoiceDiscount, dtTaxExempt, dtInvoicePayment);
                    if (Result_Sale == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region suspended = true, recall = false or true (onhold)
                if (dtSaleSuspened.Rows.Count > 0 && dtSalesSuspenedItem.Rows.Count > 0)
                {
                    long Result_Suspended = objVerifone.InsertInvoice_Suspended(dtSaleSuspened, dtSalesSuspenedItem);
                    if (Result_Suspended == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region suspended = false, recall - true (recall)
                if (dtSaleRecall.Rows.Count > 0 && dtSaleRecallItem.Rows.Count > 0)
                {
                    long Result_Recall = objVerifone.InsertInvoice_Recall(dtSaleRecall, dtSaleRecallItem, dtSaleRecall_InvoiceDiscount, dtSaleRecallPayment);
                    if (Result_Recall == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region void
                if (dtVoid.Rows.Count > 0)
                {
                    long Result_Void = objVerifone.InsertInvoice_Void(dtVoid);
                    if (Result_Void == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region journal (error data)
                if (dtJournal.Rows.Count > 0)
                {
                    long Result_Journal = objVerifone.InsertInvoice_Journal(dtJournal);
                    if (Result_Journal == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region nosale
                if (dtNoSale.Rows.Count > 0)
                {
                    long Result_Nosale = objVerifone.InsertInvoice_Nosale(dtNoSale);
                    if (Result_Nosale == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region payout
                if (dtPayout.Rows.Count > 0)
                {
                    long Result_Payout = objVerifone.InsertInvoice_Payout(dtPayout);
                    if (Result_Payout == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region Lottery without item
                if (dtLotteryInvoice.Rows.Count > 0)
                {
                    long Result_Lottery = objVerifone.InsertInvoice_Lottery(dtLotteryInvoice, dtLotteryInvoiceItem, dtInvoicePayment);
                    if (Result_Lottery == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region Fuel Invoice
                if (dtInvoicePump.Rows.Count > 0 && dtPumpCart.Rows.Count > 0)
                {
                    long Result_Fuel = objVerifone.InsertInvoice_Fuel(dtInvoicePump, dtPumpCart);
                    if (Result_Fuel == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion

                #region drop amount
                if (dtDropamount.Rows.Count > 0 && dtDropamount.Rows.Count > 0)
                {
                    long Resultdrop = objVerifone.InsertDropamount(dtDropamount);
                    if (Resultdrop == 0)
                    {
                        IsAnyError = true;
                    }
                }
                #endregion
                #endregion

                if (IsAnyError == true)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "InvoiceFileWise_DifferentType()", "InvoiceFileWise_DifferentType Exception : " + ex.Message, "Invoice", "");
                return false;
            }
        }
        #endregion

        #region Different Types of Invoices
        public void NoSale(myCompany1111.trans objtrans, DataTable dtNoSale)
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                Int64 NoSale_InvoiceNo = 0, NoSale_RegisterId = 0, UserId = 0;
                DateTime NoSale_Date = DateTime.Now;

                NoSale_InvoiceNo = Convert.ToInt64(objtrans.trHeader.trTickNum.trSeq);
                NoSale_Date = objComman.SplitDate(objtrans.trHeader.date);
                NoSale_RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);
                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);

                dtNoSale.Rows.Add(
                           NoSale_InvoiceNo,
                           NoSale_Date,
                           UserId,
                           NoSale_RegisterId
                        );
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "NoSale Exception : " + ex.Message, "NoSale", "");
            }
        }

        public void drpoAmountData(myCompany1111.trans objtrans, DataTable dtDropamount, int DropId)
        {
            _CVerifone objVerifone = new _CVerifone();
            decimal drop_amount;
            try
            {
                Int64 drpoAmount_RegisterId = 0, UserId = 0;
                int PayId = 0;
                DateTime drpoAmount_Date = DateTime.Now;

                trValue objtrvalue = new trValue();
                objtrvalue = objtrans.trValue;
                drop_amount = objtrvalue.trCurrTot.Value;
                drpoAmount_RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);

                trHeaderType objtrHeaderType = new trHeaderType();
                drpoAmount_Date = (objComman.SplitDate(objtrans.trHeader.date));
                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);
                DropId = Convert.ToInt32(objtrans.trHeader.trTickNum.trSeq);

                Int64 PayLineCount = 0;
                PayLineCount = objtrans.trPaylines == null ? 0 : objtrans.trPaylines.Count();
                trPayline[] objtrPayline = new trPayline[PayLineCount];
                objtrPayline = objtrans.trPaylines;

                if (objtrPayline != null)
                {
                    for (int k = 0; k < objtrPayline.Length; k++)
                    {
                        PayId = Convert.ToInt32(objtrPayline[k].trpPaycode.mop);

                    }
                }

                dtDropamount.Rows.Add(
                            DropId,
                            drop_amount,
                            drpoAmount_Date,
                            UserId,
                            drpoAmount_RegisterId,
                            PayId
                        );


            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "NoSale Exception : " + ex.Message, "NoSale", "");
            }
        }

        public void Sale(myCompany1111.trans objtrans, DataTable dtInvoice, DataTable dtInvoiceItem, DataTable dtInvoiceDiscount, DataTable dtLotteryInvoice,
                         DataTable dtLotteryInvoiceItem, DataTable dtTaxExempt, int Comman_InvoiceNo, DataTable dtInvoicePump, DataTable dtPumpCart, DataTable dtInvoicePayment)
        {
            _CVerifone objVerifone = new _CVerifone();

            #region declare variables
            string ItemType = "", SequenceNumber = "";
            int Id = 0, RegisterId = 0, RegisterInvoiceNo = 0, TaxId = 0, PayId = 0, trLinesCount = 0, PayLineCount = 0, RowPosition = 0, Fuel_RowPosition = 0, PumpCart_RowPosition = 0;
            DateTime InvoiceDate = DateTime.Now;
            string Modifier = "", ItemNo = "", ItemDesc = "", PayMode = "", UPC = "", CardType = "", AuthCode = "", AccNo = "", TransactionNo = "", TransactionId = "", BatchNo = "", ExtData = "", HostCode = "", OrgRegisterInvNo = "";
            decimal SubTotal = 0, TaxAmount = 0, BillAmount = 0, ItemQty = 0, ItemAmount = 0, Amount = 0, ItemTaxAmount = 0, OldPrice = 0, ItemBasicRate = 0, Tax = 0, Rate = 0, TotalItemAmount = 0, Discount = 0, PromotionAmount = 0, RetailAmount = 0, TaxPercentage = 0, ReturnAmount = 0, TenderAmount = 0, LotteryTenderAmount = 0, ExtraCharge = 0;

            long DepartmentId = 0;
            DateTime AuthDateTime = DateTime.Now;
            bool IsReverse = false, IsLottery = false, IsLotteryItem = false, IsMoneyOrder = false;
            Int64 UserId = 0, RetrunPayid = 0;
            string prepostpay = "", DepartmentType = "";
            bool returnamoutflag = false;
            #endregion

            try
            {
                PayLineCount = objtrans.trPaylines == null ? 0 : objtrans.trPaylines.Count();
                trPayline[] objtrPayline = new trPayline[PayLineCount];
                objtrPayline = objtrans.trPaylines;

                transactionType objTransactionType = new transactionType();
                objTransactionType = objtrans.type;

                trLinesCount = objtrans.trLines == null ? 0 : objtrans.trLines.Count();
                myCompany1111.trLine[] objtrLine = new trLine[trLinesCount];
                objtrLine = objtrans.trLines;

                Id = Comman_InvoiceNo;
                RegisterInvoiceNo = Convert.ToInt32(objtrans.trHeader.trTickNum.trSeq);
                if (RegisterInvoiceNo == 1028446)
                {
                    int id = RegisterInvoiceNo;
                }
                RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);
                if (objtrans.trHeader.trOriginalTickNum != null)
                {
                    //fRegisterInvoiceNo = Convert.ToInt32(objtrans.trHeader.trOriginalTickNum.trSeq);
                    RegisterId = Convert.ToInt32(objtrans.trHeader.trOriginalTickNum.posNum);
                    RegisterInvoiceNo = Convert.ToInt32(objtrans.trHeader.trOriginalTickNum.trSeq);
                }


                #region check invoice
                InvoiceDate = objComman.SplitDate(objtrans.trHeader.date);

                SubTotal = objtrans.trValue.trTotNoTax;
                TaxAmount = objtrans.trValue.trTotTax;
                BillAmount = objtrans.trValue.trTotWTax;

                try
                {
                    if (objtrPayline != null)
                    {
                        for (int k = 0; k < objtrPayline.Length; k++)
                        {
                            PayId = Convert.ToInt32(objtrPayline[k].trpPaycode.mop);

                            #region commented
                            // RowPosition = RowPosition + 1;


                            //if (objtrPayline[k].trpPaycode.Value == "CASH")
                            //{
                            //    PayMode = objtrPayline[k].trpPaycode.Value;
                            //    TenderAmount = objtrPayline[k].trpAmt;


                            //    dtInvoicePayment.Rows.Add(
                            //        Id,
                            //        InvoiceDate,
                            //        objtrPayline[k].trpAmt, // TenderAmount
                            //        0,
                            //        PayId,
                            //        PayMode,
                            //        UserId,
                            //        RegisterId,
                            //        "", "", "", "", "", ""
                            //        , "", "", null
                            //    );


                            //}
                            //else if (objtrPayline[k].trpPaycode.Value == "MAN CREDIT")
                            //{
                            //    PayMode = objtrPayline[k].trpPaycode.Value;
                            //    TenderAmount = objtrPayline[k].trpAmt;

                            //    dtInvoicePayment.Rows.Add(
                            //        Id,
                            //        InvoiceDate,
                            //        objtrPayline[k].trpAmt, // TenderAmount
                            //        0,
                            //        PayId,
                            //        PayMode,
                            //        UserId,
                            //        RegisterId,
                            //        "", "", "", "", "", ""
                            //        , "", "", null
                            //    );

                            //}
                            #endregion

                            if (objtrPayline[k].trpPaycode.Value == "Change")
                            {
                                ReturnAmount = (objtrPayline[k].trpAmt * -1);
                                RetrunPayid = PayId;
                            }
                            else  //if (objtrPayline[k].trpPaycode.Value == "CREDIT" || objtrPayline[k].trpPaycode.Value == "DEBIT")
                            {

                                PayMode = objtrPayline[k].trpPaycode.Value;

                                if (Convert.ToInt16(objTransactionType) == 20)
                                {
                                    TenderAmount = objtrPayline[k].trpAmt * -1;
                                }
                                else
                                {
                                    TenderAmount = objtrPayline[k].trpAmt;
                                }

                                if (objtrPayline[k].trpPaycode.Value == "LOTTERY")
                                {
                                    LotteryTenderAmount = objtrPayline[k].trpAmt;
                                    IsLottery = true;
                                    IsLotteryItem = true;
                                }


                                if (((trpCardInfo)(objtrPayline[k].Item)) != null)
                                {
                                    CardType = ((trpCardInfo)(objtrPayline[k].Item)).trpcCCName.Value;
                                    AuthCode = ((trpCardInfo)(objtrPayline[k].Item)).trpcAuthCode;
                                    AccNo = ((trpCardInfo)(objtrPayline[k].Item)).trpcAccount.Value;
                                    TransactionNo = ((trpCardInfo)(objtrPayline[k].Item)).trpcSeqNr;
                                    TransactionId = ((trpCardInfo)(objtrPayline[k].Item)).trpcRefNum;
                                    BatchNo = ((trpCardInfo)(objtrPayline[k].Item)).trpcBatchNr;
                                    ExtData = "InvNum=" + Id + ",CardType=" + CardType;
                                    HostCode = ((trpCardInfo)(objtrPayline[k].Item)).trpcHostID.Value;
                                    AuthDateTime = objComman.SplitDate(Convert.ToString(((trpCardInfo)(objtrPayline[k].Item)).trpcAuthDateTime));
                                }
                                else
                                {
                                    CardType = "";
                                    AuthCode = "";
                                    AccNo = "";
                                    TransactionNo = "";
                                    TransactionId = "";
                                    BatchNo = "";
                                    ExtData = "";
                                    HostCode = "";
                                    AuthDateTime = DateTime.Now;
                                }

                                dtInvoicePayment.Rows.Add(
                                    Id,
                                    InvoiceDate,
                                    TenderAmount, //objtrPayline[k].trpAmt,
                                    0,
                                    PayId,
                                    PayMode,
                                    UserId,
                                    RegisterId,
                                    CardType, AuthCode, AccNo, TransactionNo, BatchNo, TransactionId
                                    , ExtData, HostCode, AuthDateTime
                                );
                            }

                            #region commented
                            //else     //if (objtrPayline[k].trpPaycode.Value == "LOTTERY")
                            //{
                            //    PayMode = objtrPayline[k].trpPaycode.Value;
                            //    LotteryTenderAmount = objtrPayline[k].trpAmt;

                            //    if (objtrPayline[k].trpPaycode.Value == "LOTTERY")
                            //    {
                            //        IsLottery = true;
                            //        IsLotteryItem = true;
                            //    }

                            //    dtInvoicePayment.Rows.Add(
                            //        Id,
                            //        InvoiceDate,
                            //        objtrPayline[k].trpAmt, // TenderAmount
                            //        0,
                            //        PayId,
                            //        PayMode,
                            //        UserId,
                            //        RegisterId,
                            //        "", "", "", "", "", ""
                            //        , "", "", null
                            //    );
                            //}
                            #endregion

                        }
                        if (RetrunPayid > 0 && ReturnAmount > 0)
                        {
                            foreach (DataRow dr in dtInvoicePayment.Rows) // search whole table
                            {
                                if (Convert.ToInt32(dr["Id"]) == Id) // if id==2
                                {
                                    if (Convert.ToInt32(dr["PayId"]) == RetrunPayid && returnamoutflag == false)
                                    {
                                        dr["ReturnAmount"] = ReturnAmount; //change the name
                                        returnamoutflag = true;
                                    }
                                }
                            }
                            if (returnamoutflag == false)
                            {
                                foreach (DataRow dr in dtInvoicePayment.Rows) // search whole table
                                {
                                    if (Convert.ToInt32(dr["Id"]) == Id && returnamoutflag == false) // if id==2
                                    {
                                        dr["ReturnAmount"] = ReturnAmount; //change the name
                                        returnamoutflag = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (Convert.ToInt16(objtrans.type) == 24)
                    {
                        CardType = "";
                        AuthCode = "";
                        AccNo = "";
                        TransactionNo = "";
                        TransactionId = "";
                        BatchNo = "";
                        ExtData = "";
                        HostCode = "";
                        AuthDateTime = DateTime.Now;

                        dtInvoicePayment.Rows.Add(
                                    Id,
                                    InvoiceDate,
                                    BillAmount,
                                    0,
                                    0,
                                    "",
                                    UserId,
                                    RegisterId,
                                    CardType, AuthCode, AccNo, TransactionNo, BatchNo, TransactionId
                                    , ExtData, HostCode, AuthDateTime
                                );
                    }
                    else
                    { }
                }
                catch (Exception ex)
                {
                    trPayline[] objtrPayline1 = new trPayline[0];
                    objtrPayline1 = objtrans.trPaylines;
                    PayId = 0;
                    PayMode = "";
                    ReturnAmount = 0;
                    TenderAmount = 0;
                }

                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);
                if (UserId == 0)
                {
                    if (objtrans.trHeader.originalCashier != null)
                    {
                        if (Convert.ToString(objtrans.trHeader.originalCashier.sysid) != "" && objtrans.trHeader.originalCashier.sysid != "0")
                        {
                            UserId = Convert.ToInt64(objtrans.trHeader.originalCashier.sysid);
                        }
                    }
                }

                #region lottery invoice
                if (IsLottery == true)
                {
                    dtLotteryInvoice.Rows.Add(
                             Id,
                             InvoiceDate,
                             SubTotal,
                             TaxAmount,
                             BillAmount,
                             PayId,
                             ReturnAmount,
                             false,
                             RegisterInvoiceNo,
                             OrderNo += 1,
                             OrgRegisterInvNo = "",
                             false,
                             UserId,
                             TenderAmount,
                             PayMode,
                             CardType,
                             AuthCode,
                             AccNo,
                             TransactionNo,
                             BatchNo,
                             ExtData,
                             HostCode,
                             AuthDateTime,
                             TransactionId,
                             RegisterId,
                             IsLotteryItem
                         );
                }
                else
                {
                    dtInvoice.Rows.Add(
                    Id,
                    InvoiceDate,
                    SubTotal,
                    TaxAmount,
                    BillAmount,
                    PayId,
                    ReturnAmount,
                    false,
                    RegisterInvoiceNo,
                    OrderNo += 1,
                    OrgRegisterInvNo = "",
                    false,
                    UserId,
                    TenderAmount,
                    PayMode,
                    CardType,
                    AuthCode,
                    AccNo,
                    TransactionNo,
                    BatchNo,
                    ExtData,
                    HostCode,
                    AuthDateTime,
                    TransactionId,
                    RegisterId
                );
                }
                #endregion

                for (int j = 0; j < objtrLine.Length; j++)
                {
                    ExtraCharge = 0;
                    RowPosition = RowPosition + 1;
                    IsReverse = false;
                    PromotionAmount = 0;
                    ItemType = Convert.ToString(objtrLine[j].type);
                    DepartmentId = Convert.ToInt64(objtrLine[j].trlDept.number);
                    DepartmentType = Convert.ToString(objtrLine[j].trlDept.type);

                    if (objtrLine[j].trlDept.Value == "MONEY ORDER")
                    {
                        if (objtrLine[j].trlFee != null)
                        {
                            trlFee[] objtrlFee = new trlFee[objtrLine[j].trlFee.Count()];
                            objtrlFee = objtrLine[j].trlFee;
                            for (int jj = 0; jj < objtrlFee.Length; jj++)
                            {
                                ExtraCharge += objtrlFee[jj].trlFeeAmount;

                            }
                            IsMoneyOrder = true;
                        }
                        else
                        {
                            ExtraCharge = 0;
                            IsMoneyOrder = false;
                        }
                    }
                    else
                    {
                        ExtraCharge = 0;
                        IsMoneyOrder = false;
                    }

                    if (ItemType == "postFuel")
                    {


                        DataSet dsdep = objComman.GetDataDepthorwItem(DepartmentId);
                        if (dsdep.Tables[0] != null && dsdep.Tables[0].Rows.Count > 0)
                        {
                            ItemNo = Convert.ToString(dsdep.Tables[0].Rows[0]["ITEM_No"]);
                            ItemDesc = Convert.ToString(dsdep.Tables[0].Rows[0]["ITEM_Desc"]);
                            UPC = Convert.ToString(dsdep.Tables[0].Rows[0]["Barcode"]);

                        }
                        else
                        {
                            ItemNo = "0";
                            ItemDesc = "";
                            UPC = "";
                        }
                        ItemQty = 1;
                    }
                    else
                    {
                        ItemNo = objtrLine[j].trlNetwCode;
                        ItemDesc = objtrLine[j].trlDesc;
                        UPC = objtrLine[j].trlUPC;
                        ItemQty = objtrLine[j].trlQty != null ? objtrLine[j].trlQty : 0;
                    }

                    Modifier = objtrLine[j].trlModifier;

                    if (objtrLine[j].trlTaxes != null && objtrLine[j].trlTaxes.Length > 0)
                    {
                        ItemTaxAmount = 0;

                        Tax = ((trlTax)(objtrLine[j].trlTaxes[0])).Value;
                        Rate = ((trlRate)(objtrLine[j].trlTaxes[1])).Value;
                        TaxId = Convert.ToInt32(((trlRate)(objtrLine[j].trlTaxes[1])).sysid);
                        IsReverse = ((trlTax)(objtrLine[j].trlTaxes[0])).reverse;
                        if (IsReverse)
                        {
                            #region for remove tax
                            dtTaxExempt.Rows.Add(
                                RegisterInvoiceNo,
                                UPC,
                                InvoiceDate,
                                RowPosition,
                                Rate,
                                UserId,
                                RegisterId
                                );
                            #endregion
                        }

                    }
                    else
                    {
                        Tax = 0;
                        Rate = 0;
                        TaxId = 0;
                        ItemTaxAmount = 0;
                    }

                    ItemTaxAmount = ((Tax * Rate) / 100);
                    TaxPercentage = Rate;



                    if (objtrLine[j].trlMixMatches != null)
                    {
                        for (int d = 0; d < objtrLine[j].trlMixMatches.Length; d++)
                        {
                            dtInvoiceDiscount.Rows.Add(
                                Id,
                                RowPosition,
                                UPC,
                                objtrLine[j].trlMixMatches[d].trlPromoAmount,
                                ItemNo,
                                ItemDesc,
                                objtrLine[j].trlMixMatches[d].trlPromotionID.Value,
                                ItemType
                                );

                            PromotionAmount += objtrLine[j].trlMixMatches[d].trlPromoAmount;
                        }
                    }
                    if (objtrLine[j].trlDisc != null)
                    {
                        trlDisc[] objtrlDisc = new trlDisc[objtrLine[j].trlDisc.Count()];
                        objtrlDisc = objtrLine[j].trlDisc;
                        for (int kk = 0; kk < objtrlDisc.Length; kk++)
                        {
                            dtInvoiceDiscount.Rows.Add(
                              Id,
                              RowPosition,
                              UPC,
                              objtrlDisc[kk].Value,
                              ItemNo,
                              ItemDesc,
                              0,
                              ItemType
                              );


                            PromotionAmount += objtrlDisc[kk].Value;
                        }
                    }

                    Discount = PromotionAmount;

                    OldPrice = objtrLine[j].trlPrcOvrd;
                    Amount = objtrLine[j].trlUnitPrice != null ? objtrLine[j].trlUnitPrice : 0;

                    #region ItemAmount & ItemBasicRate
                    if (ItemType != "postFuel")
                    {
                        if (objTransactionType == 0 || (Convert.ToInt16(objTransactionType) == 2))
                        {
                            if (ItemType == "voidplu" || ItemType == "voiddept" || DepartmentType == "neg")
                            {
                                if (ItemQty != 0)
                                {
                                    if (DepartmentType == "neg")
                                    {
                                        ItemType = "dept_neg";
                                    }
                                    ItemAmount = (((Amount * ItemQty) - Discount) / ItemQty) * (-1);
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                else
                                {
                                    ItemAmount = 0;
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                RetailAmount = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * -1) : 0;

                                if (OldPrice == 0)
                                {
                                    ItemBasicRate = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * -1) : 0;
                                }
                                else
                                {
                                    if (OldPrice < 0)
                                        ItemBasicRate = (((OldPrice * (-1)) / ItemQty) * (-1));
                                    else
                                        ItemBasicRate = ((OldPrice / ItemQty) * (-1));
                                }
                            }
                            else
                            {
                                if (ItemQty != 0)
                                {
                                    ItemAmount = ((Amount * ItemQty) - Discount) / ItemQty;
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                else
                                {
                                    ItemAmount = 0;
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                RetailAmount = objtrLine[j].trlUnitPrice != null ? objtrLine[j].trlUnitPrice : 0;
                                if (OldPrice == 0)
                                {
                                    ItemBasicRate = objtrLine[j].trlUnitPrice != null ? objtrLine[j].trlUnitPrice : 0;
                                }
                                else
                                {
                                    ItemBasicRate = OldPrice / ItemQty;
                                }
                            }
                        }
                        else
                        {
                            if (ItemQty != 0)
                            {
                                ItemAmount = (((Amount * ItemQty) - Discount) / ItemQty) * (-1);
                                // TotalLotteryItemAmount += ItemAmount;
                            }
                            else
                            {
                                ItemAmount = 0;
                                // TotalLotteryItemAmount += ItemAmount;
                            }
                            RetailAmount = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * (-1)) : 0;
                            if (OldPrice == 0)
                            {
                                ItemBasicRate = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * (-1)) : 0;
                            }
                            else
                            {
                                if (OldPrice < 0)
                                    ItemBasicRate = (((OldPrice * (-1)) / ItemQty) * (-1));
                                else
                                    ItemBasicRate = ((OldPrice / ItemQty) * (-1));
                            }
                        }
                    #endregion

                        TotalItemAmount = (ItemAmount * ItemQty) + ItemTaxAmount;
                    }
                    else
                    {
                        ItemAmount = objtrLine[j].trlLineTot;
                        ItemBasicRate = objtrLine[j].trlLineTot;
                        TotalItemAmount = objtrLine[j].trlLineTot;
                    }



                    //if (IsReverse == false)
                    //{
                    if (IsLotteryItem == true)
                    {
                        #region Lottery item
                        dtLotteryInvoiceItem.Rows.Add(
                            Id,
                            ItemNo,
                            ItemDesc,
                            UPC,
                            ItemQty,
                            ItemAmount,
                            ItemTaxAmount,
                            TotalItemAmount,
                            DepartmentId,
                            RetailAmount,
                            false,
                            TaxId,
                            TaxPercentage,
                            RowPosition,
                            UserId,
                            ItemType,
                            OldPrice,
                            InvoiceDate,
                            PromotionAmount,
                            Modifier,
                            ItemBasicRate,
                            RegisterId,
                            false,
                            ExtraCharge,
                            IsMoneyOrder
                        );
                        #endregion
                    }
                    else
                    {
                        #region Invoice item
                        dtInvoiceItem.Rows.Add(
                            Id,
                            ItemNo,
                            ItemDesc,
                            UPC,
                            ItemQty,
                            ItemAmount,
                            ItemTaxAmount,
                            TotalItemAmount,
                            DepartmentId,
                            RetailAmount,
                            false,
                            TaxId,
                            TaxPercentage,
                           RowPosition,
                            UserId,
                            ItemType,
                            OldPrice,
                            InvoiceDate,
                            PromotionAmount,
                            Modifier,
                            ItemBasicRate,
                            RegisterId,
                            ExtraCharge,
                            IsMoneyOrder
                        );
                        #endregion
                    }

                    #region Fuel invoices
                    if (objtrLine[j].trlFuel != null)
                    {
                        if (UserId != 0)
                        {

                            if (objtrans.fuelPrepayCompletion)
                            {
                                prepostpay = "PRE-PAY";
                            }
                            else
                            {
                                prepostpay = "POST-PAY";
                            }
                        }
                        else
                        {
                            prepostpay = "OUTSIDE-PAY";
                        }

                        var chars = "0123456789";
                        var stringChars = new char[19];
                        var random = new Random();

                        #region auto generated code
                        for (int a = 0; a < stringChars.Length; a++)
                        {
                            stringChars[a] = chars[random.Next(chars.Length)];
                        }
                        SequenceNumber = new String(stringChars);
                        #endregion

                        string t = "";
                        dtInvoicePump.Rows.Add(
                           RowPosition,
                            Id,
                            "/Cart-" + InvoiceDate + "," + RegisterInvoiceNo,   //CartId
                            objtrLine[j].trlFuel.fuelPosition,  //PumpId
                            objtrLine[j].trlFuel.fuelProd.sysid,    //FuelId
                            objtrLine[j].trlFuel.fuelSvcMode.sysid, //ServiceType
                            objtrLine[j].trlFuel.basePrice, //PricePerGallon
                             objtrLine[j].trlLineTot,   //Amount
                            objtrLine[j].trlFuel.fuelVolume,    //Volume
                           prepostpay,  ///TransactionType
                            SequenceNumber,  //SequenceNumber
                            6,   //PumpCartStatus
                            DepartmentId
                            );


                        dtPumpCart.Rows.Add(
                             "/Cart-" + InvoiceDate + "," + RegisterInvoiceNo,  //CartId
                             objtrLine[j].trlFuel.fuelPosition, //PumpId
                             objtrLine[j].trlFuel.fuelProd.sysid,   //FuelId
                             UserId, //UserId
                             InvoiceDate,   //TimeStamp
                             RegisterInvoiceNo, //RegInvNum
                             RegisterId,    //RegisterNumber
                             objtrLine[j].trlFuel.fuelSvcMode.sysid,
                             0, //PayId
                             objtrLine[j].trlFuel.basePrice, //PricePerGallon
                             objtrLine[j].trlLineTot,   //Amount
                             objtrLine[j].trlFuel.fuelVolume, //Volume
                             1, // PayType
                             prepostpay, //TransactionType
                             Id,    //InvoiceNo
                             objtrLine[j].trlFuel.trlFuelSeq,   //TransactionNo
                             PumpCart_RowPosition += 1,  //RowPosition
                             SequenceNumber,    //SequenceNumber
                             6, //PumpCartStatus
                             "", //parentSequence
                             "", //reason
                             "",    //ReleaseToken
                             0, //TransactionSeqNo
                             null,  //Odometer
                             null, //Vehicle
                             null   //Driver
                            );
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Sale Exception  : Invoice No " + RegisterInvoiceNo + " - Exception " + ex.Message, "Sale", "");
                //objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Sale Exception : " + ex.Message, "Sale", "");
            }
        }

        public void Void(myCompany1111.trans objtrans, DataTable dtVoid)
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                #region declare variables
                int trLinesCount = 0;
                Int64 TVoid_InvoiceNo = 0, TVoid_RowPosition = 0, TVoid_OldInvoiceNo = 0, TVoid_RegisterId = 0, UserId = 0, ItemType = 0;
                DateTime TVoid_Date = DateTime.Now;
                decimal OldPrice = 0, Discount = 0, Amount = 0, ItemQty = 0, ItemAmount = 0, LineAmount = 0;
                string itemname = "", Modifier = "", UPC = "", LineType = "";
                #endregion

                trLinesCount = objtrans.trLines == null ? 0 : objtrans.trLines.Count();
                myCompany1111.trLine[] objtrLine = new trLine[trLinesCount];
                objtrLine = objtrans.trLines;

                TVoid_InvoiceNo = Convert.ToInt32(objtrans.trHeader.trTickNum.trSeq);
                TVoid_RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);

                if (objtrans.trHeader.trRecall != null)
                {
                    TVoid_OldInvoiceNo = Convert.ToInt64(objtrans.trHeader.trRecall[0].trSeq);
                }
                else
                {
                    TVoid_OldInvoiceNo = 0;
                }

                TVoid_Date = objComman.SplitDate(objtrans.trHeader.date);

                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);

                for (int j = 0; j < objtrLine.Length; j++)
                {
                    Discount = objtrLine[j].trlMixMatches != null ? objtrLine[j].trlMixMatches[0].trlPromoAmount : 0;
                    Amount = objtrLine[j].trlUnitPrice != null ? objtrLine[j].trlUnitPrice : 0;
                    ItemQty = objtrLine[j].trlQty != null ? objtrLine[j].trlQty : 0;
                    OldPrice = objtrLine[j].trlPrcOvrd;
                    LineAmount = objtrLine[j].trlLineTot != null ? objtrLine[j].trlLineTot : 0;

                    LineType = Convert.ToString(objtrLine[j].type);

                    if (LineType == "preFuel")
                    {
                        ItemAmount = LineAmount;
                    }
                    else
                    {
                        if (ItemQty != 0)
                        {
                            ItemAmount = (((Amount * ItemQty) - Discount));
                        }
                        else
                        {
                            ItemAmount = 0;
                        }
                    }

                    if (objtrLine[j].type == 0)
                    {
                        Modifier = objtrLine[j].trlModifier != null ? objtrLine[j].trlModifier : "";
                        UPC = objtrLine[j].trlUPC;
                        //ItemType = 0;
                    }
                    else
                    {
                        Modifier = "";
                        UPC = "";
                        //ItemType = 1;
                    }

                    itemname = objtrLine[j].trlDesc != null ? objtrLine[j].trlDesc : "";

                    dtVoid.Rows.Add(
                         objtrLine[j].type, //ItemType = 0 (plu), 8 (dept), 4 (preFuel),
                         TVoid_InvoiceNo,
                         itemname,
                         UPC,
                         Modifier,
                         TVoid_Date,
                         TVoid_RowPosition += 1,
                         UserId,
                         ItemAmount,//objtrLine[j].trlLineTot,
                         TVoid_OldInvoiceNo,
                         TVoid_RegisterId,
                         OldPrice,
                         ItemQty
                      );
                }
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Void Exception : " + ex.Message, "Void", "");
            }
        }

        public void Suspended(myCompany1111.trans objtrans, DataTable dtSaleSuspened, DataTable dtSalesSuspenedItem, int SaleSuspended_OrderNo, DataTable dtTaxExempt)
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                #region declare variables
                int trLinesCount = 0, SaleSuspendedItem_RowPosition = 0, SaleSuspenedItem_DepartmentId = 0, SaleSuspenedItem_TaxId = 0;
                Int64 UserId = 0, SaleSuspened_OldInvoiceNo = 0, SaleSuspended_InvoiceNo = 0, SaleSuspended_RegisterId = 0;
                string SaleSuspenedItem_Barcode = "", SaleSuspenedItem_ItemNo = "", SaleSuspenedItem_ItemDesc = "", SaleSuspendedItem_ItemType = "", SaleSuspenedItem_Modifier = "", SaleSuspenedItem_DepartmentType = "";
                decimal SaleSuspenedItem_ItemAmount = 0, SaleSuspenedItem_ItemTaxAmount = 0, SaleSuspenedItem_TotalItemAmount = 0, SaleSuspenedItem_RetailAmount = 0, SaleSuspended_SubTotal = 0, SaleSuspened_TaxAmount = 0, SaleSuspended_BillAmount = 0, SaleSupenedItem_ItemQty = 0, SaleSuspenedItem_Tax = 0, SaleSuspenedItem_Rate = 0, SaleSuspendedItem_OldPrice = 0, SaleSuspenedItem_ItemBasicRate = 0, SaleSupenedItem_Amount = 0, SaleSupenedItem_Discount = 0, ExtraCharge = 0;
                DateTime SaleSuspended_InvoiceDate = DateTime.Now;
                bool IsMoneyOrder = false, SaleSuspenedItem_IsReverse = false;
                #endregion

                trLinesCount = objtrans.trLines == null ? 0 : objtrans.trLines.Count();
                myCompany1111.trLine[] objtrLine = new trLine[trLinesCount];
                objtrLine = objtrans.trLines;

                SaleSuspendedItem_RowPosition = 0;
                SaleSuspended_InvoiceNo = Convert.ToInt32(objtrans.trHeader.trTickNum.trSeq);
                SaleSuspended_RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);

                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);

                if (objtrans.trHeader.trRecall != null)
                {
                    SaleSuspened_OldInvoiceNo = Convert.ToInt64(objtrans.trHeader.trRecall[0].trSeq);
                }
                else
                {
                    SaleSuspened_OldInvoiceNo = 0;
                }

                SaleSuspended_InvoiceDate = objComman.SplitDate(objtrans.trHeader.date);
                SaleSuspended_SubTotal = objtrans.trValue.trTotNoTax;
                SaleSuspened_TaxAmount = objtrans.trValue.trTotTax;
                SaleSuspended_BillAmount = objtrans.trValue.trTotWTax;

                dtSaleSuspened.Rows.Add(
                    SaleSuspended_InvoiceNo,
                    SaleSuspended_InvoiceDate,
                    SaleSuspended_SubTotal,
                    SaleSuspened_TaxAmount,
                    SaleSuspended_BillAmount,
                    SaleSuspended_OrderNo,
                    SaleSuspened_OldInvoiceNo,
                    UserId,
                    SaleSuspended_RegisterId
                    );

                for (int n = 0; n < objtrLine.Length; n++)
                {

                    SaleSuspendedItem_RowPosition = n + 1;
                    SaleSuspenedItem_Barcode = objtrLine[n].trlUPC;
                    SaleSuspenedItem_ItemNo = objtrLine[n].trlNetwCode;
                    SaleSuspenedItem_ItemDesc = objtrLine[n].trlDesc;
                    SaleSupenedItem_ItemQty = objtrLine[n].trlQty;
                    SaleSuspenedItem_Modifier = objtrLine[n].trlModifier;

                    SaleSuspenedItem_DepartmentId = Convert.ToInt16(objtrLine[n].trlDept.number);
                    SaleSuspenedItem_RetailAmount = objtrLine[n].trlUnitPrice;
                    SaleSuspendedItem_OldPrice = objtrLine[n].trlPrcOvrd;
                    SaleSuspendedItem_ItemType = Convert.ToString(objtrLine[n].type);
                    SaleSupenedItem_Discount = objtrLine[n].trlMixMatches != null ? objtrLine[n].trlMixMatches[0].trlPromoAmount : 0;
                    SaleSuspenedItem_DepartmentType = Convert.ToString(objtrLine[n].trlDept.type);

                    if (objtrLine[n].trlTaxes != null && objtrLine[n].trlTaxes.Length > 0)
                    {
                        SaleSuspenedItem_Tax = ((trlTax)(objtrLine[n].trlTaxes[0])).Value;
                        SaleSuspenedItem_Rate = ((trlRate)(objtrLine[n].trlTaxes[1])).Value;
                        SaleSuspenedItem_TaxId = Convert.ToInt32(((trlRate)(objtrLine[n].trlTaxes[1])).sysid);
                        SaleSuspenedItem_IsReverse = ((trlTax)(objtrLine[n].trlTaxes[0])).reverse;

                        if (SaleSuspenedItem_IsReverse)
                        {

                            //dtTaxExempt.Rows.Add(
                            //       SaleSuspended_InvoiceNo,
                            //       SaleSuspendedItem_RowPosition,
                            //       SaleSuspenedItem_ItemDesc,
                            //       SaleSuspendedItem_ItemType,
                            //       SaleSuspenedItem_Barcode,
                            //       SaleSuspenedItem_Modifier,
                            //       SaleSuspenedItem_TaxId,
                            //       SaleSuspenedItem_Rate,
                            //       0,
                            //       ((SaleSuspenedItem_Tax * SaleSuspenedItem_Rate) / 100)
                            //   );

                            dtTaxExempt.Rows.Add(
                           SaleSuspended_InvoiceNo,
                            SaleSuspenedItem_Barcode,
                            SaleSuspended_InvoiceDate,
                           SaleSuspendedItem_RowPosition,
                            SaleSuspenedItem_Rate,
                           UserId,
                           SaleSuspended_RegisterId
                           );
                        }

                    }
                    else
                    {
                        SaleSuspenedItem_Tax = 0;
                        SaleSuspenedItem_Rate = 0;
                        SaleSuspenedItem_TaxId = 0;
                        SaleSuspenedItem_ItemTaxAmount = 0;
                    }

                    SaleSuspenedItem_ItemTaxAmount += ((SaleSuspenedItem_Tax * SaleSuspenedItem_Rate) / 100);

                    if (objtrLine[n].trlDept.Value == "MONEY ORDER")
                    {
                        if (objtrLine[n].trlFee != null)
                        {
                            trlFee[] objtrlFee = new trlFee[objtrLine[n].trlFee.Count()];
                            objtrlFee = objtrLine[n].trlFee;
                            for (int jj = 0; jj < objtrlFee.Length; jj++)
                            {
                                ExtraCharge += objtrlFee[jj].trlFeeAmount;

                            }
                            IsMoneyOrder = true;
                        }
                        else
                        {
                            ExtraCharge = 0;
                            IsMoneyOrder = false;
                        }
                    }
                    else
                    {
                        ExtraCharge = 0;
                        IsMoneyOrder = false;
                    }


                    #region ItemAmount & ItemBasicRate
                    SaleSupenedItem_Amount = objtrLine[n].trlUnitPrice != null ? objtrLine[n].trlUnitPrice : 0;
                    if (SaleSuspendedItem_ItemType == "voidplu" || SaleSuspendedItem_ItemType == "voiddept" || SaleSuspenedItem_DepartmentType == "neg")
                    {
                        if (SaleSupenedItem_ItemQty != 0)
                        {
                            if (SaleSuspenedItem_DepartmentType == "neg")
                            {
                                SaleSuspendedItem_ItemType = "dept_neg";
                            }
                            SaleSuspenedItem_ItemAmount = (((SaleSupenedItem_Amount * SaleSupenedItem_ItemQty) - SaleSupenedItem_Discount) / SaleSupenedItem_ItemQty) * (-1);
                        }
                        else
                        {
                            SaleSuspenedItem_ItemAmount = 0;
                        }
                        if (SaleSuspendedItem_OldPrice == 0)
                        {
                            SaleSuspenedItem_ItemBasicRate = objtrLine[n].trlUnitPrice != null ? (objtrLine[n].trlUnitPrice * (-1)) : 0;
                        }
                        else
                        {
                            if (SaleSuspendedItem_OldPrice < 0)
                                SaleSuspenedItem_ItemBasicRate = (((SaleSuspendedItem_OldPrice * (-1)) / SaleSupenedItem_ItemQty) * (-1));
                            else
                                SaleSuspenedItem_ItemBasicRate = ((SaleSuspendedItem_OldPrice / SaleSupenedItem_ItemQty) * (-1));
                        }
                    }
                    else
                    {
                        if (SaleSupenedItem_ItemQty != 0)
                        {
                            SaleSuspenedItem_ItemAmount = ((SaleSupenedItem_Amount * SaleSupenedItem_ItemQty) - SaleSupenedItem_Discount) / SaleSupenedItem_ItemQty;
                        }
                        else
                        {
                            SaleSuspenedItem_ItemAmount = 0;
                        }
                        if (SaleSuspendedItem_OldPrice == 0)
                        {
                            SaleSuspenedItem_ItemBasicRate = objtrLine[n].trlUnitPrice != null ? objtrLine[n].trlUnitPrice : 0;
                        }
                        else
                        {
                            SaleSuspenedItem_ItemBasicRate = SaleSuspendedItem_OldPrice / SaleSupenedItem_ItemQty;
                        }
                    }
                    #endregion

                    SaleSuspenedItem_TotalItemAmount = (SaleSuspenedItem_ItemAmount * SaleSupenedItem_ItemQty) + SaleSuspenedItem_ItemTaxAmount;

                    dtSalesSuspenedItem.Rows.Add(
                    SaleSuspendedItem_RowPosition,
                    SaleSuspended_InvoiceNo,
                    SaleSuspenedItem_Barcode,
                    SaleSuspenedItem_ItemNo,
                    SaleSuspenedItem_ItemDesc,
                    SaleSupenedItem_ItemQty,
                    SaleSuspenedItem_ItemAmount,
                    SaleSuspenedItem_ItemTaxAmount,
                    SaleSuspenedItem_TotalItemAmount,
                    SaleSuspenedItem_DepartmentId,
                    SaleSuspenedItem_RetailAmount,
                    SaleSuspenedItem_TaxId,
                    SaleSuspenedItem_Rate,
                    SaleSuspended_InvoiceDate,
                    UserId,
                    SaleSuspenedItem_Modifier,
                    SaleSuspenedItem_ItemBasicRate,
                    SaleSuspended_RegisterId,
                    SaleSuspendedItem_OldPrice,
                    ExtraCharge,
                    IsMoneyOrder
                    );
                }
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Suspended Exception : " + ex.Message, "Suspended", "");
            }
        }

        public void Recall(myCompany1111.trans objtrans, DataTable dtSaleRecall, DataTable dtSaleRecallItem, DataTable dtSaleRecall_InvoiceDiscount, DataTable dtLotteryInvoice,
                    DataTable dtLotteryInvoiceItem, DataTable dtTaxExempt, int Comman_InvoiceNo, DataTable dtInvoicePump, DataTable dtPumpCart, DataTable dtSaleRecallPayment)
        {
            _CVerifone objVerifone = new _CVerifone();

            #region declare variables
            Int64 SaleRecall_OldInvoiceNo = 0;

            string ItemType = "", SequenceNumber = "";
            int Id = 0, RegisterId = 0, RegisterInvoiceNo = 0, TaxId = 0, PayId = 0, trLinesCount = 0, PayLineCount = 0, RowPosition = 0, Fuel_RowPosition = 0, PumpCart_RowPosition = 0;
            DateTime InvoiceDate = DateTime.Now;
            string Modifier = "", ItemNo = "", ItemDesc = "", PayMode = "", UPC = "", CardType = "", AuthCode = "", AccNo = "", TransactionNo = "", TransactionId = "", BatchNo = "", ExtData = "", HostCode = "", OrgRegisterInvNo = "", DepartmentType = "";
            decimal SubTotal = 0, TaxAmount = 0, BillAmount = 0, ItemQty = 0, ItemAmount = 0, Amount = 0, ItemTaxAmount = 0, OldPrice = 0, ItemBasicRate = 0, Tax = 0, Rate = 0, TotalItemAmount = 0, Discount = 0, PromotionAmount = 0, RetailAmount = 0, TaxPercentage = 0, ReturnAmount = 0, TenderAmount = 0, LotteryTenderAmount = 0, ExtraCharge = 0;


            long DepartmentId = 0;
            DateTime AuthDateTime = DateTime.Now;
            bool IsReverse = false, IsLottery = false, IsLotteryItem = false, IsMoneyOrder = false;
            Int64 UserId = 0, RetrunPayid = 0;
            string prepostpay = "";
            bool returnamoutflag = false;
            #endregion
            try
            {


                PayLineCount = objtrans.trPaylines == null ? 0 : objtrans.trPaylines.Count();
                trPayline[] objtrPayline = new trPayline[PayLineCount];
                objtrPayline = objtrans.trPaylines;

                transactionType objTransactionType = new transactionType();
                objTransactionType = objtrans.type;

                trLinesCount = objtrans.trLines == null ? 0 : objtrans.trLines.Count();
                myCompany1111.trLine[] objtrLine = new trLine[trLinesCount];
                objtrLine = objtrans.trLines;

                Id = Comman_InvoiceNo;
                RegisterInvoiceNo = Convert.ToInt32(objtrans.trHeader.trTickNum.trSeq);
                SaleRecall_OldInvoiceNo = Convert.ToInt64(objtrans.trHeader.trRecall[0].trSeq);

                #region check invoice

                RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);

                InvoiceDate = objComman.SplitDate(objtrans.trHeader.date);


                SubTotal = objtrans.trValue.trTotNoTax;
                TaxAmount = objtrans.trValue.trTotTax;
                BillAmount = objtrans.trValue.trTotWTax;

                try
                {
                    if (objtrPayline != null)
                    {
                        for (int k = 0; k < objtrPayline.Length; k++)
                        {
                            //RowPosition = RowPosition + 1;
                            PayId = Convert.ToInt32(objtrPayline[k].trpPaycode.mop);
                            if (objtrPayline[k].trpPaycode.Value == "Change")
                            {
                                ReturnAmount = (objtrPayline[k].trpAmt * -1);
                                RetrunPayid = PayId;
                            }
                            //if (objtrPayline[k].trpPaycode.Value == "CASH")
                            //{
                            //    PayMode = objtrPayline[k].trpPaycode.Value;
                            //    TenderAmount = objtrPayline[k].trpAmt;


                            //    dtSaleRecallPayment.Rows.Add(
                            //        Id,
                            //        InvoiceDate,
                            //        objtrPayline[k].trpAmt, // TenderAmount
                            //        0,
                            //        PayId,
                            //        PayMode,
                            //        UserId,
                            //        RegisterId,
                            //        "", "", "", "", "", ""
                            //        , "", "", null
                            //    );
                            //}
                            //else if (objtrPayline[k].trpPaycode.Value == "MAN CREDIT")
                            //{
                            //    PayMode = objtrPayline[k].trpPaycode.Value;
                            //    TenderAmount = objtrPayline[k].trpAmt;

                            //    dtSaleRecallPayment.Rows.Add(
                            //        Id,
                            //        InvoiceDate,
                            //        objtrPayline[k].trpAmt, // TenderAmount
                            //        0,
                            //        PayId,
                            //        PayMode,
                            //        UserId,
                            //        RegisterId,
                            //        "", "", "", "", "", ""
                            //        , "", "", null
                            //    );

                            //}
                            else //if (objtrPayline[k].trpPaycode.Value == "CREDIT" || objtrPayline[k].trpPaycode.Value == "DEBIT")
                            {

                                if (objtrPayline[k].trpPaycode.Value == "LOTTERY")
                                {
                                    LotteryTenderAmount = objtrPayline[k].trpAmt;
                                    IsLottery = true;
                                    IsLotteryItem = true;
                                }

                                if (((trpCardInfo)(objtrPayline[k].Item)) != null)
                                {

                                    CardType = ((trpCardInfo)(objtrPayline[k].Item)).trpcCCName.Value;
                                    AuthCode = ((trpCardInfo)(objtrPayline[k].Item)).trpcAuthCode;
                                    AccNo = ((trpCardInfo)(objtrPayline[k].Item)).trpcAccount.Value;
                                    TransactionNo = ((trpCardInfo)(objtrPayline[k].Item)).trpcSeqNr;
                                    TransactionId = ((trpCardInfo)(objtrPayline[k].Item)).trpcRefNum;
                                    BatchNo = ((trpCardInfo)(objtrPayline[k].Item)).trpcBatchNr;
                                    ExtData = "InvNum=" + Id + ",CardType=" + CardType;
                                    HostCode = ((trpCardInfo)(objtrPayline[k].Item)).trpcHostID.Value;
                                    AuthDateTime = objComman.SplitDate(Convert.ToString(((trpCardInfo)(objtrPayline[k].Item)).trpcAuthDateTime));
                                }
                                else
                                {
                                    CardType = "";
                                    AuthCode = "";
                                    AccNo = "";
                                    TransactionNo = "";
                                    TransactionId = "";
                                    BatchNo = "";
                                    ExtData = "";
                                    HostCode = "";
                                    AuthDateTime = DateTime.Now;
                                }

                                dtSaleRecallPayment.Rows.Add(
                                    Id,
                                    InvoiceDate,
                                    objtrPayline[k].trpAmt, // TenderAmount
                                    0,
                                    PayId,
                                    PayMode,
                                    UserId,
                                    RegisterId,
                                    CardType, AuthCode, AccNo, TransactionNo, BatchNo, TransactionId
                                    , ExtData, HostCode, AuthDateTime
                                );
                            }
                            //else if (objtrPayline[k].trpPaycode.Value == "LOTTERY")
                            //{
                            //    PayMode = objtrPayline[k].trpPaycode.Value;
                            //    LotteryTenderAmount = objtrPayline[k].trpAmt;

                            //    IsLottery = true;
                            //    IsLotteryItem = true;

                            //    dtSaleRecallPayment.Rows.Add(
                            //        Id,
                            //        InvoiceDate,
                            //        objtrPayline[k].trpAmt, // TenderAmount
                            //        0,
                            //        PayId,
                            //        PayMode,
                            //        UserId,
                            //        RegisterId,
                            //        "", "", "", "", "", ""
                            //        , "", "", null
                            //    );


                            //}
                        }
                        if (RetrunPayid > 0 && ReturnAmount > 0)
                        {
                            foreach (DataRow dr in dtSaleRecallPayment.Rows) // search whole table
                            {
                                if (Convert.ToInt32(dr["Id"]) == Id) // if id==2
                                {
                                    if (Convert.ToInt32(dr["PayId"]) == RetrunPayid && returnamoutflag == false)
                                    {
                                        dr["ReturnAmount"] = ReturnAmount; //change the name
                                        returnamoutflag = true;
                                    }
                                }
                            }
                            if (returnamoutflag == false)
                            {
                                foreach (DataRow dr in dtSaleRecallPayment.Rows) // search whole table
                                {
                                    if (Convert.ToInt32(dr["Id"]) == Id && returnamoutflag == false) // if id==2
                                    {
                                        dr["ReturnAmount"] = ReturnAmount; //change the name
                                        returnamoutflag = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    trPayline[] objtrPayline1 = new trPayline[0];
                    objtrPayline1 = objtrans.trPaylines;
                    PayId = 0;
                    PayMode = "";
                    ReturnAmount = 0;
                    TenderAmount = 0;
                }

                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);
                if (UserId == 0)
                {
                    if (objtrans.trHeader.originalCashier != null)
                    {
                        if (Convert.ToString(objtrans.trHeader.originalCashier.sysid) != "" && objtrans.trHeader.originalCashier.sysid != "0")
                        {
                            UserId = Convert.ToInt64(objtrans.trHeader.originalCashier.sysid);
                        }
                    }
                }

                #region lottery invoice
                if (IsLottery == true)
                {
                    dtLotteryInvoice.Rows.Add(
                             Id,
                             InvoiceDate,
                             SubTotal,
                             TaxAmount,
                             BillAmount,
                             PayId,
                             ReturnAmount,
                             false,
                             RegisterInvoiceNo,
                             OrderNo += 1,
                             OrgRegisterInvNo = "",
                             false,
                             UserId,
                             TenderAmount,
                             PayMode,
                             CardType,
                             AuthCode,
                             AccNo,
                             TransactionNo,
                             BatchNo,
                             ExtData,
                             HostCode,
                             AuthDateTime,
                             TransactionId,
                             RegisterId,
                             IsLotteryItem
                         );
                }
                else
                {

                    dtSaleRecall.Rows.Add(
                    SaleRecall_OldInvoiceNo,
                    Id,
                    InvoiceDate,
                    SubTotal,
                    TaxAmount,
                    BillAmount,
                    PayId,
                    ReturnAmount,
                    RegisterInvoiceNo,
                    OrderNo += 1,
                    UserId,
                    TenderAmount,
                    PayMode,
                    RegisterId
                );
                }
                #endregion

                for (int j = 0; j < objtrLine.Length; j++)
                {

                    RowPosition = RowPosition + 1;
                    IsReverse = false;
                    PromotionAmount = 0;
                    ItemType = Convert.ToString(objtrLine[j].type);
                    DepartmentId = Convert.ToInt64(objtrLine[j].trlDept.number);
                    DepartmentType = Convert.ToString(objtrLine[j].trlDept.type);

                    if (objtrLine[j].trlDept.Value == "MONEY ORDER")
                    {
                        if (objtrLine[j].trlFee != null)
                        {
                            trlFee[] objtrlFee = new trlFee[objtrLine[j].trlFee.Count()];
                            objtrlFee = objtrLine[j].trlFee;
                            for (int jj = 0; jj < objtrlFee.Length; jj++)
                            {
                                ExtraCharge += objtrlFee[jj].trlFeeAmount;

                            }
                            IsMoneyOrder = true;
                        }
                        else
                        {
                            ExtraCharge = 0;
                            IsMoneyOrder = false;
                        }
                    }
                    else
                    {
                        ExtraCharge = 0;
                        IsMoneyOrder = false;
                    }

                    if (ItemType == "postFuel")
                    {
                        DataSet dsdep = objComman.GetDataDepthorwItem(DepartmentId);
                        if (dsdep.Tables[0] != null && dsdep.Tables[0].Rows.Count > 0)
                        {
                            ItemNo = Convert.ToString(dsdep.Tables[0].Rows[0]["ITEM_No"]);
                            ItemDesc = Convert.ToString(dsdep.Tables[0].Rows[0]["ITEM_Desc"]);
                            UPC = Convert.ToString(dsdep.Tables[0].Rows[0]["Barcode"]);
                        }
                        else
                        {
                            ItemNo = "0";
                            ItemDesc = "";
                            UPC = "";
                        }
                        ItemQty = 1;
                    }
                    else
                    {
                        ItemNo = objtrLine[j].trlNetwCode;
                        ItemDesc = objtrLine[j].trlDesc;
                        UPC = objtrLine[j].trlUPC;
                        ItemQty = objtrLine[j].trlQty != null ? objtrLine[j].trlQty : 0;
                    }

                    Modifier = objtrLine[j].trlModifier;

                    if (objtrLine[j].trlTaxes != null && objtrLine[j].trlTaxes.Length > 0)
                    {

                        Tax = ((trlTax)(objtrLine[j].trlTaxes[0])).Value;
                        Rate = ((trlRate)(objtrLine[j].trlTaxes[1])).Value;
                        TaxId = Convert.ToInt32(((trlRate)(objtrLine[j].trlTaxes[1])).sysid);
                        IsReverse = ((trlTax)(objtrLine[j].trlTaxes[0])).reverse;
                        if (IsReverse)
                        {
                            #region for remove tax
                            dtTaxExempt.Rows.Add(
                                Id,
                                UPC,
                                InvoiceDate,
                                RowPosition,
                                Rate,
                                UserId,
                                RegisterId
                                );
                            #endregion
                        }
                    }
                    else
                    {
                        Tax = 0;
                        Rate = 0;
                        TaxId = 0;
                        ItemTaxAmount = 0;
                    }

                    ItemTaxAmount = ((Tax * Rate) / 100);
                    TaxPercentage = Rate;


                    if (objtrLine[j].trlMixMatches != null)
                    {
                        for (int d = 0; d < objtrLine[j].trlMixMatches.Length; d++)
                        {
                            dtSaleRecall_InvoiceDiscount.Rows.Add(
                                Id,
                                RowPosition,
                                UPC,
                                objtrLine[j].trlMixMatches[d].trlPromoAmount,
                                ItemNo,
                                ItemDesc,
                                objtrLine[j].trlMixMatches[d].trlPromotionID.Value,
                                ItemType
                                );

                            PromotionAmount += objtrLine[j].trlMixMatches[d].trlPromoAmount;
                        }
                    }
                    if (objtrLine[j].trlDisc != null)
                    {
                        trlDisc[] objtrlDisc = new trlDisc[objtrLine[j].trlDisc.Count()];
                        objtrlDisc = objtrLine[j].trlDisc;
                        for (int kk = 0; kk < objtrlDisc.Length; kk++)
                        {
                            dtSaleRecall_InvoiceDiscount.Rows.Add(
                              Id,
                              RowPosition,
                              UPC,
                              objtrlDisc[kk].Value,
                              ItemNo,
                              ItemDesc,
                              0,
                              ItemType
                              );
                            PromotionAmount += objtrlDisc[kk].Value;
                        }
                    }

                    Discount = PromotionAmount;
                    TaxPercentage = Rate;
                    OldPrice = objtrLine[j].trlPrcOvrd;
                    Amount = objtrLine[j].trlUnitPrice != null ? objtrLine[j].trlUnitPrice : 0;

                    #region ItemAmount & ItemBasicRate
                    if (ItemType != "postFuel")
                    {
                        if (objTransactionType == 0 || (Convert.ToInt16(objTransactionType) == 2))
                        {
                            if (ItemType == "voidplu" || ItemType == "voiddept" || DepartmentType == "neg")
                            {
                                if (ItemQty != 0)
                                {
                                    if (DepartmentType == "neg")
                                    {
                                        ItemType = "dept_neg";
                                    }
                                    ItemAmount = (((Amount * ItemQty) - Discount) / ItemQty) * (-1);
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                else
                                {
                                    ItemAmount = 0;
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                RetailAmount = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * -1) : 0;

                                if (OldPrice == 0)
                                {
                                    ItemBasicRate = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * -1) : 0;
                                }
                                else
                                {
                                    if (OldPrice < 0)
                                        ItemBasicRate = (((OldPrice * (-1)) / ItemQty) * (-1));
                                    else
                                        ItemBasicRate = ((OldPrice / ItemQty) * (-1));
                                }
                            }
                            else
                            {
                                if (ItemQty != 0)
                                {
                                    ItemAmount = ((Amount * ItemQty) - Discount) / ItemQty;
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                else
                                {
                                    ItemAmount = 0;
                                    //TotalLotteryItemAmount += ItemAmount;
                                }
                                RetailAmount = objtrLine[j].trlUnitPrice != null ? objtrLine[j].trlUnitPrice : 0;
                                if (OldPrice == 0)
                                {
                                    ItemBasicRate = objtrLine[j].trlUnitPrice != null ? objtrLine[j].trlUnitPrice : 0;
                                }
                                else
                                {
                                    ItemBasicRate = OldPrice / ItemQty;
                                }
                            }
                        }
                        else
                        {
                            if (ItemQty != 0)
                            {
                                ItemAmount = (((Amount * ItemQty) - Discount) / ItemQty) * (-1);
                                // TotalLotteryItemAmount += ItemAmount;
                            }
                            else
                            {
                                ItemAmount = 0;
                                // TotalLotteryItemAmount += ItemAmount;
                            }
                            RetailAmount = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * (-1)) : 0;
                            if (OldPrice == 0)
                            {
                                ItemBasicRate = objtrLine[j].trlUnitPrice != null ? (objtrLine[j].trlUnitPrice * (-1)) : 0;
                            }
                            else
                            {
                                if (OldPrice < 0)
                                    ItemBasicRate = (((OldPrice * (-1)) / ItemQty) * (-1));
                                else
                                    ItemBasicRate = ((OldPrice / ItemQty) * (-1));
                            }
                        }
                    #endregion

                        TotalItemAmount = (ItemAmount * ItemQty) + ItemTaxAmount;
                    }
                    else
                    {
                        ItemAmount = objtrLine[j].trlLineTot;
                        ItemBasicRate = objtrLine[j].trlLineTot;
                        TotalItemAmount = objtrLine[j].trlLineTot;
                    }

                    if (IsLotteryItem == true)
                    {
                        #region Lottery item
                        dtLotteryInvoiceItem.Rows.Add(
                            Id,
                            ItemNo,
                            ItemDesc,
                            UPC,
                            ItemQty,
                            ItemAmount,
                            ItemTaxAmount,
                            TotalItemAmount,
                            DepartmentId,
                            RetailAmount,
                            false,
                            TaxId,
                            TaxPercentage,
                            RowPosition,
                            UserId,
                            ItemType,
                            OldPrice,
                            InvoiceDate,
                            PromotionAmount,
                            Modifier,
                            ItemBasicRate,
                            RegisterId,
                            false,
                            ExtraCharge,
                            IsMoneyOrder
                        );
                        #endregion
                    }
                    else
                    {
                        #region Invoice item
                        dtSaleRecallItem.Rows.Add(
                            Id, ItemNo, ItemDesc,
                            UPC, ItemQty, ItemAmount,
                            ItemTaxAmount, TotalItemAmount, DepartmentId,
                            RetailAmount, TaxId, TaxPercentage,
                           RowPosition, UserId, ItemType,
                           PromotionAmount, Modifier, ItemBasicRate,
                           OldPrice, ExtraCharge, IsMoneyOrder

                        );
                        #endregion
                    }

                    #region Fuel invoices
                    if (objtrLine[j].trlFuel != null)
                    {
                        if (objtrans.fuelPrepayCompletion)
                        {
                            prepostpay = "PRE-PAY";
                        }
                        else
                        {
                            prepostpay = "POST-PAY";
                        }

                        var chars = "0123456789";
                        var stringChars = new char[19];
                        var random = new Random();

                        #region auto generated code
                        for (int a = 0; a < stringChars.Length; a++)
                        {
                            stringChars[a] = chars[random.Next(chars.Length)];
                        }
                        SequenceNumber = new String(stringChars);
                        #endregion

                        string t = "";
                        dtInvoicePump.Rows.Add(
                           RowPosition,
                            Id,
                            "/Cart-" + InvoiceDate + "," + RegisterInvoiceNo,   //CartId
                            objtrLine[j].trlFuel.fuelPosition,  //PumpId
                            objtrLine[j].trlFuel.fuelProd.sysid,    //FuelId
                            objtrLine[j].trlFuel.fuelSvcMode.sysid, //ServiceType
                            objtrLine[j].trlFuel.basePrice, //PricePerGallon
                             objtrLine[j].trlLineTot,   //Amount
                            objtrLine[j].trlFuel.fuelVolume,    //Volume
                           prepostpay,  ///TransactionType
                            SequenceNumber,  //SequenceNumber
                            6,   //PumpCartStatus
                            DepartmentId
                            );


                        dtPumpCart.Rows.Add(
                             "/Cart-" + InvoiceDate + "," + RegisterInvoiceNo,  //CartId
                             objtrLine[j].trlFuel.fuelPosition, //PumpId
                             objtrLine[j].trlFuel.fuelProd.sysid,   //FuelId
                             UserId, //UserId
                             InvoiceDate,   //TimeStamp
                             RegisterInvoiceNo, //RegInvNum
                             RegisterId,    //RegisterNumber
                             objtrLine[j].trlFuel.fuelSvcMode.sysid,
                             0, //PayId
                             objtrLine[j].trlFuel.basePrice, //PricePerGallon
                             objtrLine[j].trlLineTot,   //Amount
                             objtrLine[j].trlFuel.fuelVolume, //Volume
                             1, // PayType
                             prepostpay, //TransactionType
                             Id,    //InvoiceNo
                             objtrLine[j].trlFuel.trlFuelSeq,   //TransactionNo
                             PumpCart_RowPosition += 1,  //RowPosition
                             SequenceNumber,    //SequenceNumber
                             6, //PumpCartStatus
                             "", //parentSequence
                             "", //reason
                             "",    //ReleaseToken
                             0, //TransactionSeqNo
                             null,  //Odometer
                             null, //Vehicle
                             null   //Driver
                            );
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                // objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Sale Exception : " + ex.Message, "Sale", "");
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Sale Exception  : Invoice No " + RegisterInvoiceNo + " - Exception " + ex.Message, "Sale", "");
                //objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Sale Exception : " + ex.Message, "Sale", "");
            }
        }

        public void Journal(myCompany1111.trans objtrans, DataTable dtJournal)
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                #region declare variable
                DateTime Journal_Date = DateTime.Now;
                string Journal_UPC = "";
                Int64 Journal_RowPosition = 0, Journal_RegisterId = 0, UserId = 0;
                decimal NewValue = 0;
                long ItemType = 0;
                string Modifier = "", ItemDesc = "";
                int Journal_trLineCount = 0;
                #endregion

                Journal_Date = objComman.SplitDate(objtrans.trHeader.date);

                Journal_trLineCount = objtrans.trJournal.trLine == null ? 0 : objtrans.trJournal.trLine.Count();
                myCompany1111.trLine[] objJournal_trLine = new trLine[Journal_trLineCount];
                objJournal_trLine = objtrans.trJournal.trLine;

                if (objJournal_trLine != null)
                {
                    for (int j = 0; j < objJournal_trLine.Length; j++)
                    {
                        if ((Convert.ToInt32(objJournal_trLine[j].type) == 0) || (Convert.ToInt32(objJournal_trLine[j].type) == 8) || (Convert.ToInt32(objJournal_trLine[j].type) == 4))
                        {
                            Journal_UPC = objJournal_trLine[j].trlUPC;
                            UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);
                            Journal_RegisterId = Convert.ToInt64(objtrans.trHeader.posNum);
                            NewValue = Convert.ToDecimal(objJournal_trLine[j].trlLineTot);

                            if ((Convert.ToInt32(objJournal_trLine[j].type) == 8))
                            {

                                string deptname = (objJournal_trLine[j].trlDept.Value);
                                DataSet ds = objComman.GetDeptItemData(Convert.ToInt32(objJournal_trLine[j].trlDept.number));
                                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                                {
                                    Journal_UPC = Convert.ToString(ds.Tables[0].Rows[0]["Barcode"]);
                                    Modifier = Convert.ToString(ds.Tables[0].Rows[0]["ITEM_ShortName"]);
                                }
                                else
                                {
                                    Journal_UPC = "";
                                    Modifier = "";
                                }
                                //ItemType = 1;
                                ItemDesc = (objJournal_trLine[j].trlDesc);

                            }
                            else if ((Convert.ToInt32(objJournal_trLine[j].type) == 0))
                            {
                                //ItemType = 1;
                                ItemDesc = Convert.ToString(objJournal_trLine[j].trlDesc);
                                Modifier = Convert.ToString(objJournal_trLine[j].trlModifier);
                                Journal_UPC = Convert.ToString(objJournal_trLine[j].trlUPC);
                            }
                            else if ((Convert.ToInt32(objJournal_trLine[j].type) == 4))
                            {
                                //ItemType = 0;
                                DataSet ds = objComman.GetGasDATA();
                                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                                {
                                    ItemDesc = Convert.ToString(ds.Tables[0].Rows[0]["ITEM_Desc"]);
                                    Modifier = Convert.ToString(ds.Tables[0].Rows[0]["ITEM_ShortName"]);
                                    Journal_UPC = Convert.ToString(ds.Tables[0].Rows[0]["Barcode"]);
                                }
                                else
                                {
                                    ItemDesc = "";
                                    Modifier = "";
                                    Journal_UPC = "";
                                }
                            }


                            dtJournal.Rows.Add(
                                Journal_Date,
                                ItemDesc,
                                Modifier,
                                Journal_UPC,
                                Journal_RowPosition += 1,
                                UserId,
                                Journal_RegisterId,
                                NewValue
                             );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Journal Exception : " + ex.Message, "Journal", "");
            }
        }

        public void Payout(myCompany1111.trans objtrans, DataTable dtPayout, int Comman_InvoiceNo)
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                #region declare variables
                int Id = 0, RegisterInvoiceNo = 0, PayId = 0, PayLineCount = 0, Payout_RegisterId = 0;
                DateTime InvoiceDate = DateTime.Now;
                string PayMode = "", OrgRegisterInvNo = "";
                decimal SubTotal = 0, TaxAmount = 0, BillAmount = 0, ReturnAmount = 0, TenderAmount = 0;
                //long OrderNo = 0;
                Int64 UserId = 0;
                #endregion

                PayLineCount = objtrans.trPaylines == null ? 0 : objtrans.trPaylines.Count();
                trPayline[] objtrPayline = new trPayline[PayLineCount];
                objtrPayline = objtrans.trPaylines;

                Id = Comman_InvoiceNo;
                RegisterInvoiceNo = Convert.ToInt32(objtrans.trHeader.trTickNum.trSeq);
                InvoiceDate = objComman.SplitDate(objtrans.trHeader.date);
                SubTotal = objtrans.trValue.trCurrTot.Value != 0 ? (objtrans.trValue.trCurrTot.Value * (-1)) : 0;
                TaxAmount = 0;
                BillAmount = objtrans.trValue.trCurrTot.Value != 0 ? (objtrans.trValue.trCurrTot.Value * (-1)) : 0;
                Payout_RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);

                try
                {
                    for (int k = 0; k < objtrPayline.Length; k++)
                    {
                        PayId = Convert.ToInt32(objtrPayline[k].trpPaycode.mop);
                        if (objtrPayline[k].trpPaycode.Value == "Change")
                        {
                            ReturnAmount = (objtrPayline[k].trpAmt * -1);
                        }
                        else //if (objtrPayline[k].trpPaycode.Value == "CASH")
                        {
                            PayMode = objtrPayline[k].trpPaycode.Value;
                            TenderAmount = objtrPayline[k].trpAmt != 0 ? (objtrPayline[k].trpAmt * (-1)) : 0;
                        }
                        //else if (objtrPayline[k].trpPaycode.Value == "MAN CREDIT")
                        //{
                        //    PayMode = "Credit";
                        //    TenderAmount = objtrPayline[k].trpAmt != 0 ? (objtrPayline[k].trpAmt * (-1)) : 0;
                        //}
                        //else
                        //{
                        //    PayMode = "";
                        //    ReturnAmount = 0;
                        //    TenderAmount = 0;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    trPayline[] objtrPayline1 = new trPayline[0];
                    objtrPayline1 = objtrans.trPaylines;
                    PayId = 0;
                    PayMode = "";
                    ReturnAmount = 0;
                    TenderAmount = 0;
                }

                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);

                dtPayout.Rows.Add(
                   Id,
                   InvoiceDate,
                   SubTotal,
                   TaxAmount,
                   BillAmount,
                   PayId,
                   ReturnAmount,
                   false,
                   RegisterInvoiceNo,
                   OrderNo += 1,
                   OrgRegisterInvNo = "",
                   false,
                   UserId,
                   TenderAmount,
                   PayMode,
                   Payout_RegisterId,
                   0, false
                 );
            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Payout Exception : " + ex.Message, "Payout", "");
            }
        }

        public void LotteryWithoutItem(myCompany1111.trans objtrans, DataTable dtLotteryInvoice, int Comman_InvoiceNo, bool IsLotteryItem, long OrderNo, DataTable dtInvoicePayment)
        {
            _CVerifone objVerifone = new _CVerifone();
            try
            {
                #region declare variables
                int Id = 0, RegisterInvoiceNo = 0, PayId = 0, PayLineCount = 0, RegisterId = 0, RetrunPayid = 0;
                DateTime InvoiceDate = DateTime.Now;
                string PayMode = "", OrgRegisterInvNo = "", CardType = "", AuthCode = "", AccNo = "", TransactionNo = "", TransactionId = "", BatchNo = "", ExtData = "", HostCode = "";
                decimal SubTotal = 0, TaxAmount = 0, BillAmount = 0, ReturnAmount = 0, TenderAmount = 0, ExtraCharge = 0;
                bool IsMoneyOrder = false;
                bool returnamoutflag = false;

                //long OrderNo = 0;
                Int64 UserId = 0;
                DateTime AuthDateTime = DateTime.Now;
                #endregion

                PayLineCount = objtrans.trPaylines == null ? 0 : objtrans.trPaylines.Count();
                trPayline[] objtrPayline = new trPayline[PayLineCount];
                objtrPayline = objtrans.trPaylines;

                Id = Comman_InvoiceNo;
                RegisterInvoiceNo = Convert.ToInt32(objtrans.trHeader.trTickNum.trSeq);
                RegisterId = Convert.ToInt32(objtrans.trHeader.trTickNum.posNum);

                InvoiceDate = objComman.SplitDate(objtrans.trHeader.date);

                SubTotal = objtrans.trValue.trCurrTot.Value != 0 ? (objtrans.trValue.trCurrTot.Value * (-1)) : 0;
                TaxAmount = objtrans.trValue.trTotTax;
                BillAmount = objtrans.trValue.trCurrTot.Value != 0 ? (objtrans.trValue.trCurrTot.Value * (-1)) : 0;

                try
                {
                    if (objtrPayline != null)
                    {
                        for (int k = 0; k < objtrPayline.Length; k++)
                        {
                            PayId = Convert.ToInt32(objtrPayline[k].trpPaycode.mop);
                            if (objtrPayline[k].trpPaycode.Value == "Change")
                            {
                                ReturnAmount = (objtrPayline[k].trpAmt * -1);
                                RetrunPayid = PayId;
                            }

                            else if (objtrPayline[k].trpPaycode.Value == "LOTTERY")
                            {
                                PayId = Convert.ToInt32(objtrPayline[k].trpPaycode.mop);
                                PayMode = objtrPayline[k].trpPaycode.Value;
                                TenderAmount = (objtrPayline[k].trpAmt * (-1));

                                dtInvoicePayment.Rows.Add(
                                       Id,
                                       InvoiceDate,
                                       TenderAmount, // TenderAmount
                                       0,
                                       PayId,
                                       PayMode,
                                       UserId,
                                       RegisterId,
                                       "", "", "", "", "", ""
                                   );

                            }
                            else
                            {
                                PayMode = "";
                                ReturnAmount = 0;
                                TenderAmount = 0;
                                break;
                            }
                        }
                    }
                    if (RetrunPayid > 0 && ReturnAmount > 0)
                    {
                        foreach (DataRow dr in dtInvoicePayment.Rows) // search whole table
                        {
                            if (Convert.ToInt32(dr["Id"]) == Id) // if id==2
                            {
                                if (Convert.ToInt32(dr["PayId"]) == RetrunPayid && returnamoutflag == false)
                                {
                                    dr["ReturnAmount"] = ReturnAmount; //change the name
                                    returnamoutflag = true;
                                }
                            }
                        }
                        if (returnamoutflag == false)
                        {
                            foreach (DataRow dr in dtInvoicePayment.Rows) // search whole table
                            {
                                if (Convert.ToInt32(dr["Id"]) == Id && returnamoutflag == false) // if id==2
                                {
                                    dr["ReturnAmount"] = ReturnAmount; //change the name
                                    returnamoutflag = true;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    trPayline[] objtrPayline1 = new trPayline[0];
                    objtrPayline1 = objtrans.trPaylines;
                    PayId = 0;
                    PayMode = "";
                    ReturnAmount = 0;
                    TenderAmount = 0;
                }

                UserId = Convert.ToInt64(objtrans.trHeader.cashier.sysid);

                dtLotteryInvoice.Rows.Add(
                            Id, InvoiceDate, TenderAmount,
                            TaxAmount, TenderAmount, PayId,
                            ReturnAmount, false, RegisterInvoiceNo,
                            OrderNo += 1, OrgRegisterInvNo = "", false,
                            UserId, TenderAmount, PayMode,
                            CardType, AuthCode, AccNo,
                            TransactionNo, BatchNo, ExtData,
                            HostCode, AuthDateTime, TransactionId,
                            RegisterId, IsLotteryItem
                        );


            }
            catch (Exception ex)
            {
                objVerifone.InsertActiveLog("BoF", "Error", "Invoice()", "Lottery Exception : " + ex.Message, "Lottery", "");
            }
        }
        #endregion

        #region Reports
        public void Ruby_Report()
        {
            VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();

            objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize Ruby_Report", "RubyReport", "");
            #region Declare Comman Variable
            DataSet dataSet = new DataSet();
            DataSet ds = new DataSet();
            DataSet dsForZ = new DataSet();
            string RubyXMLFilepath = AppDomain.CurrentDomain.BaseDirectory + "RubyReport_Period_FileName.xml";
            //objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize Ruby_Report", "RubyReport 2", "");
            long result = 0;

            string registerID = "";
            string InvoiceBegin = "", InvoiceEnd = "";
            #endregion

            #region Declare Daily Variable
            DateTime OpenDate = DateTime.Now, CloseDate = DateTime.Now;
            //Int64 BrnZId = 0;

            //decimal OpenAmount = 0, CloseAmount = 0;
            //Int64 InvoiceBegin = 0, InvoiceEnd = 0, RegisterId = 0;
            //DataTable dtInsert_Daily = new DataTable();
            #endregion

            #region Declare Shift Variable
            Int64 BrnCashInOutId = 0;
            decimal CashInAmt, CashOutAmt = 0;
            DateTime OpnDate, ClsDate = DateTime.Now;
            #endregion

            #region Declare Month Variable
            Int64 ZZId = 0, BatchNo = 0, Month_RegisterId = 0;
            DateTime ZZDate = DateTime.Now;
            #endregion

            try
            {
                DataTable dtZReport = new DataTable();
                dtZReport.Columns.AddRange(new DataColumn[6]{
                new DataColumn("SrNo", typeof(Int64)),
                new DataColumn("RegisterId", typeof(Int64)),
                new DataColumn("InvoiceBegin", typeof(Int64)), new DataColumn("InvoiceEnd", typeof(Int64)),
                new DataColumn("OpenAmount", typeof(decimal)) , new DataColumn("CloseAmount", typeof(decimal))
                });

                #region Step 1: Report Payload using command "vreportpdlist"
                objComman.GetPayloadXML("Report", "POST", "vreportpdlist");
                #endregion

                #region Achieve files
                objComman.CommanArchiveFiles("RubyReport_Daily");
                objComman.CommanArchiveFiles("RubyReport_Shift");
                objComman.CommanArchiveFiles("RubyReport_Month");
                #endregion

                #region Step 2: Check PeriodSeqNum (Id) already exists or not
                //objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize Ruby_Report", "RubyReport 3", "");
                string checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Report";
                string[] filePaths = Directory.GetFiles(checkpath);
                if (filePaths.Length > 0)
                {
                    #region Step 3: xml to dataset of "vreportpdlist" xml
                    var path = filePaths[0];
                    dataSet.ReadXml(path, XmlReadMode.InferSchema);
                    #endregion

                    #region xml to dataset of RubyReport_Period_FileName.xml
                    if (File.Exists(RubyXMLFilepath) == true)
                    {
                        ds.ReadXml(RubyXMLFilepath, XmlReadMode.InferSchema);
                    }
                    #endregion

                    #region Step 4: get shift, daily, month data from db
                    //objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize Ruby_Report", "RubyReport 4", "");

                    #region Step 5: Daily data
                    string DailySysId = "";
                    if (ds.Tables.Count > 0)
                    {
                        DataView dvdaily = ds.Tables[0].DefaultView;
                        //dvdaily.RowFilter = ("[" + ds.Tables[0].Columns[4].ColumnName + "] = 'Daily'");   // nidhi
                        dvdaily.RowFilter = ("[" + ds.Tables[0].Columns["Name"] + "] = 'Daily'");
                        for (int i = 0; i < dvdaily.Count; i++)
                        {
                            DailySysId += (string)dvdaily[i]["Sysid"] + ",";
                        }
                        if (DailySysId == "")
                        {
                            DailySysId = "0";
                        }
                    }
                    else
                    {
                        DailySysId = "0";
                    }


                    DataView dv = dataSet.Tables["period"].DefaultView;

                    // dv.RowFilter = ("([" + Convert.ToString(dataSet.Tables["period"].Columns[4].ColumnName).ToUpper() + "] = 'DAILY' OR [" + Convert.ToString(dataSet.Tables[1].Columns["period"].ColumnName).ToUpper() + "] = 'DAY')   AND [" + dataSet.Tables["period"].Columns[2].ColumnName + "] NOT IN (" + DailySysId + ") AND [" + dataSet.Tables["period"].Columns[1].ColumnName + "] <> 'CURRENT'");

                    //---nidhi //dv.RowFilter = ("([" + Convert.ToString(dataSet.Tables["period"].Columns[4].ColumnName).ToUpper() + "] = 'DAILY' OR [" + Convert.ToString(dataSet.Tables["period"].Columns[4].ColumnName).ToUpper() + "] = 'DAY') AND [" + dataSet.Tables["period"].Columns[2].ColumnName + "] NOT IN (" + DailySysId + ") AND [" + dataSet.Tables["period"].Columns[1].ColumnName + "] <> 'CURRENT'");
                    dv.RowFilter = ("([" + Convert.ToString(dataSet.Tables["period"].Columns["name"]).ToUpper() + "] = 'DAILY' OR [" + Convert.ToString(dataSet.Tables["period"].Columns["name"]).ToUpper() + "] = 'DAY') AND [" + dataSet.Tables["period"].Columns["periodBeginDate"] + "] NOT IN (" + DailySysId + ") AND [" + dataSet.Tables["period"].Columns["periodSeqNum"] + "] <> 'CURRENT'");

                    dv.Sort = "periodSeqNum asc";
                    DataTable dtDaily = dv.ToTable();

                    //objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize Ruby_Report", "RubyReport 5", "");
                    for (int i = 0; i < dtDaily.Rows.Count; i++)
                    {
                        DataView dv1 = dataSet.Tables["reportParameters"].DefaultView;
                        //dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns[1].ColumnName + "] = " + dtDaily.Rows[i][dtDaily.Columns.Count - 1] + "");
                        //---nidhi //dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns[1].ColumnName + "] = " + dtDaily.Rows[i]["periodInfo_Id"] + "");

                        dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns["periodInfo_Id"] + "] = " + dtDaily.Rows[i]["periodInfo_Id"] + "");

                        DataView dv2 = dataSet.Tables["reportParameter"].DefaultView;
                        //---nidhi //dv2.RowFilter = ("[" + dataSet.Tables["reportParameter"].Columns[2].ColumnName + "] = " + (Int32)dv1[0]["reportParameters_Id"] + "");
                        dv2.RowFilter = ("[" + dataSet.Tables["reportParameter"].Columns["reportParameters_Id"] + "] = " + (Int32)dv1[0]["reportParameters_Id"] + "");

                        objComman.period = (string)dv2[0]["reportParameter_Text"];
                        objComman.PeriodFileName = (string)dv2[1]["reportParameter_Text"];

                        #region Step 6: generate daily xml file
                        objComman.GetXMLResult("", "RubyReport_Daily", "POST", "vrubyrept/summary");
                        #endregion

                        #region Step 7: insert z (daily data)
                        objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize to Insert Z Data into BoF", "RubyReport_Daily", "");
                        //objComman.RubyPeriodFileName = "RubyReport_Daily" + "_" + objComman.period + "_" + objComman.PeriodFileName;
                        var pathForZ = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\" + objComman.RubyPeriodFileName + ".xml";

                        XmlSerializer serializer = new XmlSerializer(typeof(periodTargs.summaryPdType));

                        periodTargs.summaryPdType resultingMessageDaily = (periodTargs.summaryPdType)serializer.Deserialize(new XmlTextReader(pathForZ));

                        //BrnZId = Convert.ToInt64(resultingMessageDaily.period.periodSeqNum);

                        OpenDate = objComman.SplitDate(resultingMessageDaily.period.periodBeginDate);
                        CloseDate = objComman.SplitDate(resultingMessageDaily.period.periodEndDate);

                        //OpenAmount = resultingMessageDaily.totals.totalizers.start.insideGrand.Value;
                        //CloseAmount = resultingMessageDaily.totals.totalizers.end.insideGrand.Value;

                        if (resultingMessageDaily.byRegister != null)
                        {
                            dtZReport.Clear();
                            //long invoicestart = 0; 
                            //long invoiceend =0; 
                            long zid = 0;
                            for (int ii = 0; ii < resultingMessageDaily.byRegister.Length; ii++)
                            {

                                //dtZReport = new DataTable();InsertInvoice_Sale
                                if (resultingMessageDaily.byRegister[ii].ticketRange != null)
                                {
                                    for (int j = 0; j < resultingMessageDaily.byRegister[ii].ticketRange.Length; j++)
                                    {
                                        zid = zid + 1;
                                        dtZReport.Rows.Add(
                                        Convert.ToInt64(zid),
                                        Convert.ToInt64(resultingMessageDaily.byRegister[ii].register.sysid),
                                        Convert.ToInt64(resultingMessageDaily.byRegister[ii].ticketRange[j].begin),
                                        Convert.ToInt64(resultingMessageDaily.byRegister[ii].ticketRange[j].end),
                                        Convert.ToDecimal(resultingMessageDaily.byRegister[ii].totalizers.start.overallSales.Value),
                                        Convert.ToDecimal(resultingMessageDaily.byRegister[ii].totalizers.end.overallSales.Value)
                                        );
                                    }
                                }

                            }
                        }

                        //if (InvoiceBegin != 0 && InvoiceEnd != 0)
                        if (dtZReport.Rows.Count > 0)
                        {
                            //result = objCVerifone.InsertZ(OpenDate, CloseDate, OpenAmount, CloseAmount, InvoiceBegin, InvoiceEnd, BrnZId, RegisterId);
                            //result = objCVerifone.InsertZ(OpenDate, CloseDate, OpenAmount, CloseAmount, dtZReport, BrnZId, RegisterId);
                            result = objCVerifone.InsertZ(OpenDate, CloseDate, dtZReport);
                            if (result != 0)
                            {
                                if (File.Exists(RubyXMLFilepath) == false)
                                {
                                    objComman.CreateXML(objComman.period, objComman.PeriodFileName, "", "New", "RubyReport_Daily");
                                }
                                else
                                {
                                    objComman.CreateXML(objComman.period, objComman.PeriodFileName, RubyXMLFilepath, "Exists", "RubyReport_Daily");
                                }
                                objCVerifone.InsertActiveLog("BoF", "End", "Ruby_Report()", "Z Data Inserted Successfully", "RubyReport_Daily", "");
                                result = 0;
                            }
                            else
                            {
                                objCVerifone.InsertActiveLog("BoF", "Fail", "Ruby_Report()", "Z Data not Inserted", "RubyReport_Daily", "");
                                result = 0;
                            }
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("BoF", "End", "Ruby_Report()", "Invoice not found in Z Report", "RubyReport_Daily", "");
                        }
                        #endregion
                    }
                    #endregion

                    #region Step 8: Shift data
                    string ShiftSysId = "";
                    if (ds.Tables.Count > 0)
                    {
                        DataView dvdaily = ds.Tables[0].DefaultView;
                        //---nidhi //dvdaily.RowFilter = ("[" + ds.Tables[0].Columns[4].ColumnName + "] = 'Shift'");

                        dvdaily.RowFilter = ("[" + ds.Tables[0].Columns["Name"] + "] = 'Shift'");

                        for (int i = 0; i < dvdaily.Count; i++)
                        {
                            ShiftSysId += (string)dvdaily[i]["Sysid"] + ",";
                        }
                        if (ShiftSysId == "")
                        {
                            ShiftSysId = "0";
                        }
                    }
                    else
                    {
                        ShiftSysId = "0";
                    }


                    DataView dvShift = dataSet.Tables["period"].DefaultView;

                    //---nidhi //dvShift.RowFilter = ("[" + dataSet.Tables["period"].Columns[4].ColumnName + "] = 'Shift' AND [" + dataSet.Tables["period"].Columns[2].ColumnName + "] NOT IN (" + ShiftSysId + ") AND [" + dataSet.Tables["period"].Columns[1].ColumnName + "] <> 'CURRENT'");
                    dvShift.RowFilter = ("[" + dataSet.Tables["period"].Columns["name"] + "] = 'Shift' AND [" + dataSet.Tables["period"].Columns["periodBeginDate"] + "] NOT IN (" + ShiftSysId + ") AND [" + dataSet.Tables["period"].Columns["periodSeqNum"] + "] <> 'CURRENT'");
                    dvShift.Sort = "periodSeqNum asc";
                    DataTable dtShift = dvShift.ToTable();

                    DataSet dsShifNo = objComman.GetShiftID();
                    if (dsShifNo.Tables[0].Rows.Count > 0)
                    {
                        BrnCashInOutId = Convert.ToInt32(dsShifNo.Tables[0].Rows[0]["BrnCashInOutId"]);
                    }
                    else
                    {
                        BrnCashInOutId = 0;
                    }

                    for (int j = 0; j < dtShift.Rows.Count; j++)
                    {
                        DataView dv1 = dataSet.Tables["reportParameters"].DefaultView;
                        //dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns[1].ColumnName + "] = " + dtShift.Rows[j][5] + "");
                        //---nidhi //dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns[1].ColumnName + "] = " + dtShift.Rows[j]["periodInfo_Id"] + "");
                        dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns["periodInfo_Id"] + "] = " + dtShift.Rows[j]["periodInfo_Id"] + "");

                        DataView dv2 = dataSet.Tables["reportParameter"].DefaultView;
                        //---nidhi //dv2.RowFilter = ("[" + dataSet.Tables["reportParameter"].Columns[2].ColumnName + "] = " + (Int32)dv1[0]["reportParameters_Id"] + "");
                        dv2.RowFilter = ("[" + dataSet.Tables["reportParameter"].Columns["reportParameters_Id"] + "] = " + (Int32)dv1[0]["reportParameters_Id"] + "");

                        objComman.period = (string)dv2[0]["reportParameter_Text"];
                        objComman.PeriodFileName = (string)dv2[1]["reportParameter_Text"];

                        #region Step 9: generate shift xml file
                        objComman.GetXMLResult("", "RubyReport_Shift", "POST", "vrubyrept/summary");
                        #endregion

                        #region Step 10: Insert Shift Data
                        objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize to Insert Shift Data into BoF", "RubyReport_Shift", "");
                        //objComman.RubyPeriodFileName = "RubyReport_Shift" + "_" + objComman.period + "_" + objComman.PeriodFileName;
                        var pathForShift = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\" + objComman.RubyPeriodFileName + ".xml";

                        XmlSerializer serializer = new XmlSerializer(typeof(periodTargs.summaryPdType));

                        periodTargs.summaryPdType resultingMessageShift = (periodTargs.summaryPdType)serializer.Deserialize(new XmlTextReader(pathForShift));

                        //BrnCashInOutId = Convert.ToInt64(resultingMessageShift.period.periodSeqNum);

                        //CashInAmt = Convert.ToDecimal(resultingMessageShift.totals.totalizers.start.insideGrand.Value);
                        //CashOutAmt = Convert.ToDecimal(resultingMessageShift.totals.totalizers.end.insideGrand.Value);
                        OpnDate = objComman.SplitDate(resultingMessageShift.period.periodBeginDate);
                        ClsDate = objComman.SplitDate(resultingMessageShift.period.periodEndDate);

                        // code bimal change for array wise
                        if (resultingMessageShift.byRegister != null)
                        {
                            periodTargs.summaryPdTypeByRegister[] objsummaryPdTypeByRegister = new periodTargs.summaryPdTypeByRegister[resultingMessageShift.byRegister.Count()];
                            objsummaryPdTypeByRegister = resultingMessageShift.byRegister;

                            for (int jj = 0; jj < objsummaryPdTypeByRegister.Length; jj++)
                            {
                                registerID = Convert.ToString(resultingMessageShift.byRegister[jj].register.sysid);
                                if (resultingMessageShift.byRegister[jj].ticketRange != null)
                                {
                                    periodTargs.ticketRange[] objticketRange = new periodTargs.ticketRange[resultingMessageShift.byRegister[jj].ticketRange.Count()];

                                    objticketRange = resultingMessageShift.byRegister[jj].ticketRange;

                                    if (objticketRange != null)
                                    {
                                        for (int k = 0; k < objticketRange.Length; k++)
                                        {
                                            InvoiceBegin = objticketRange[k].begin;
                                            InvoiceEnd = objticketRange[k].end;

                                            CashInAmt = Convert.ToDecimal(resultingMessageShift.byRegister[jj].totalizers.start.overallSales.Value);
                                            CashOutAmt = Convert.ToDecimal(resultingMessageShift.byRegister[jj].totalizers.end.overallSales.Value);
                                            BrnCashInOutId = BrnCashInOutId + 1;
                                            try
                                            {
                                                result = objCVerifone.InsertShift(BrnCashInOutId, CashInAmt, CashOutAmt, OpnDate, ClsDate, InvoiceBegin, InvoiceEnd);

                                                if (result != 0)
                                                {
                                                    if (File.Exists(RubyXMLFilepath) == false)
                                                    {
                                                        objComman.CreateXML(objComman.period, objComman.PeriodFileName, "", "New", "RubyReport_Shift");
                                                    }
                                                    else
                                                    {
                                                        objComman.CreateXML(objComman.period, objComman.PeriodFileName, RubyXMLFilepath, "Exists", "RubyReport_Shift");
                                                    }
                                                    objCVerifone.InsertActiveLog("BoF", "End", "Ruby_Report()", "Shift Data inserted Successfully", "RubyReport_Shift", "");
                                                    result = 0;
                                                }
                                                else
                                                {
                                                    objCVerifone.InsertActiveLog("BoF", "Fail", "Ruby_Report()", "Shift Data not inserted", "RubyReport_Shift", "");
                                                    result = 0;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                objCVerifone.InsertActiveLog("BoF", "Fail", "Ruby_Report()", ex.Message.ToString(), "RubyReport_Shift", "");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Step 11: Month data
                    string MonthSysId = "";
                    if (ds.Tables.Count > 0)
                    {
                        DataView dvdaily = ds.Tables[0].DefaultView;
                        //---nidhi //dvdaily.RowFilter = ("[" + ds.Tables[0].Columns[4].ColumnName + "] = 'Month'");
                        dvdaily.RowFilter = ("[" + ds.Tables[0].Columns["Name"] + "] = 'Month'");
                        for (int i = 0; i < dvdaily.Count; i++)
                        {
                            MonthSysId += (string)dvdaily[i]["Sysid"] + ",";
                        }
                        if (MonthSysId == "")
                        {
                            MonthSysId = "0";
                        }
                    }
                    else
                    {
                        MonthSysId = "0";
                    }

                    DataView dvMonth = dataSet.Tables["period"].DefaultView;
                    //---nidhi //dvMonth.RowFilter = ("([" + Convert.ToString(dataSet.Tables["period"].Columns[4].ColumnName).ToUpper() + "] = 'MONTHLY' OR [" + Convert.ToString(dataSet.Tables["period"].Columns[4].ColumnName).ToUpper() + "] = 'MONTH')  AND  [" + dataSet.Tables["period"].Columns[2].ColumnName + "] NOT IN (" + MonthSysId + ") AND [" + dataSet.Tables["period"].Columns[1].ColumnName + "] <> 'CURRENT'");
                    dvMonth.RowFilter = ("([" + Convert.ToString(dataSet.Tables["period"].Columns["name"]).ToUpper() + "] = 'MONTHLY' OR [" + Convert.ToString(dataSet.Tables["period"].Columns["name"]).ToUpper() + "] = 'MONTH')  AND  [" + dataSet.Tables["period"].Columns["periodBeginDate"] + "] NOT IN (" + MonthSysId + ") AND [" + dataSet.Tables["period"].Columns["periodSeqNum"] + "] <> 'CURRENT'");

                    dvMonth.Sort = "periodSeqNum asc";
                    DataTable dtMonth = dvMonth.ToTable();
                    bool MonthlyZZStatus = false;
                    for (int j = 0; j < dtMonth.Rows.Count; j++)
                    {
                        DataView dv1 = dataSet.Tables["reportParameters"].DefaultView;
                        /// dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns[1].ColumnName + "] = " + dtMonth.Rows[j][5] + "");
                        //---nidhi //dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns[1].ColumnName + "] = " + dtMonth.Rows[j]["periodInfo_Id"] + "");
                        dv1.RowFilter = ("[" + dataSet.Tables["reportParameters"].Columns["periodInfo_Id"] + "] = " + dtMonth.Rows[j]["periodInfo_Id"] + "");

                        DataView dv2 = dataSet.Tables["reportParameter"].DefaultView;
                        //---nidhi //dv2.RowFilter = ("[" + dataSet.Tables["reportParameter"].Columns[2].ColumnName + "] = " + (Int32)dv1[0]["reportParameters_Id"] + "");
                        dv2.RowFilter = ("[" + dataSet.Tables["reportParameter"].Columns["reportParameters_Id"] + "] = " + (Int32)dv1[0]["reportParameters_Id"] + "");

                        objComman.period = (string)dv2[0]["reportParameter_Text"];
                        objComman.PeriodFileName = (string)dv2[1]["reportParameter_Text"];

                        #region Step 12: generate month data file
                        objComman.GetXMLResult("", "RubyReport_Month", "POST", "vrubyrept/summary");
                        #endregion

                        #region Step 13: Insert Month Data
                        objCVerifone.InsertActiveLog("BoF", "Start", "Ruby_Report()", "Initialize to Insert ZZ Data into BoF", "RubyReport_Month", "");
                        //objComman.RubyPeriodFileName = "RubyReport_Month" + "_" + objComman.period + "_" + objComman.PeriodFileName;
                        var pathForMonth = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\" + objComman.RubyPeriodFileName + ".xml";

                        XmlSerializer serializer = new XmlSerializer(typeof(periodTargs.summaryPdType));

                        periodTargs.summaryPdType resultingMessageMonth = (periodTargs.summaryPdType)serializer.Deserialize(new XmlTextReader(pathForMonth));

                        OpnDate = objComman.SplitDate(resultingMessageMonth.period.periodBeginDate);
                        ZZDate = objComman.SplitDate(resultingMessageMonth.period.periodEndDate);
                        BatchNo = Convert.ToInt64(resultingMessageMonth.period.periodSeqNum);

                        long monthlyInvoiceBegin = 0;
                        long monthlyInvoiceEnd = 0;
                        long regsiterid = 0;
                        if (resultingMessageMonth.byRegister != null)
                        {
                            for (int ii = 0; ii < resultingMessageMonth.byRegister.Length; ii++)
                            {
                                if (resultingMessageMonth.byRegister[ii].ticketRange != null)
                                {
                                    for (int jj = 0; jj < resultingMessageMonth.byRegister[ii].ticketRange.Length; jj++)
                                    {
                                        monthlyInvoiceBegin = Convert.ToInt64(resultingMessageMonth.byRegister[ii].ticketRange[jj].begin);
                                        monthlyInvoiceEnd = Convert.ToInt64(resultingMessageMonth.byRegister[ii].ticketRange[jj].end);
                                        Month_RegisterId = Convert.ToInt64(resultingMessageMonth.byRegister[ii].register.sysid);

                                        DataSet dsreg = objComman.GetRegsiteridZZReport(monthlyInvoiceBegin, monthlyInvoiceEnd, Month_RegisterId, OpnDate, ZZDate);
                                        if (dsreg != null && dsreg.Tables[0] != null && dsreg.Tables[0].Rows.Count > 0)
                                        {
                                            regsiterid = Convert.ToInt32(dsreg.Tables[0].Rows[0]["RegisterId"]);

                                            result = objCVerifone.Insert_ZZ(ZZId, OpnDate, ZZDate, BatchNo, regsiterid, monthlyInvoiceBegin, monthlyInvoiceEnd);
                                            if (result == 0)
                                            {
                                                MonthlyZZStatus = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (MonthlyZZStatus)
                        {
                            if (File.Exists(RubyXMLFilepath) == false)
                            {
                                objComman.CreateXML(objComman.period, objComman.PeriodFileName, "", "New", "RubyReport_Month");
                            }
                            else
                            {
                                objComman.CreateXML(objComman.period, objComman.PeriodFileName, RubyXMLFilepath, "Exists", "RubyReport_Month");
                            }
                            objCVerifone.InsertActiveLog("BoF", "End", "Ruby_Report()", "ZZ Data inserted Successfully", "RubyReport_Month", "");
                            result = 0;
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("BoF", "Fail", "Ruby_Report()", "ZZ Data not inserted", "RubyReport_Month", "");
                            result = 0;
                        }
                        #endregion
                    }
                    #endregion
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "Ruby_Report()", "Ruby_Report Exeption : " + objComman.RubyPeriodFileName + " -- " + ex, "RubyReport", "");
            }
        }
        #endregion
    }
}
