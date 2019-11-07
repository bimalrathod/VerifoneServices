using VerifoneLibrary;
using VerifoneLibrary.DataAccess;

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
using System.Net;
using System.Xml.Linq;

using System.Diagnostics;

using System.Net.Mail;
using System.Threading;

namespace VerifoneServices
{
    public class Comman
    {
        public sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public string lblCookieValue = "";
        public string filename = "";
        public string period = "";
        public string PeriodFileName = "";
        public bool RequestUrl = true;
        public string InvoiceFileName = "";
        public string RubyPeriodFileName = "";
        //VerifoneInsert objInsert = new VerifoneInsert();
        //VerifoneUpdate objUpdate = new VerifoneUpdate();

        #region Transfer files
        //public int PeriodFileCount = 0;
        public int InvoiceFileCount = 0;
        #endregion

        public void GetPayloadXML(string PayloadName, string Method, string RequestCommand)
        {
            string checkpath = "";
            string PayloadXml = "";
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                #region Read Payload
                if (PayloadName == "Category" || PayloadName == "prodCodes")
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "Payload\\Department.xml";
                    PayloadXml = File.ReadAllText(checkpath);
                }
                else if (PayloadName == "LogPeriod" || PayloadName == "Invoice" || PayloadName == "Report" || PayloadName == "CahierUser")
                {
                    PayloadXml = "";
                }
                else
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "Payload\\" + PayloadName + ".xml";
                    PayloadXml = File.ReadAllText(checkpath);
                }
                #endregion

                #region Pass Payload to generate Verifone data xml
                GetXMLResult(PayloadXml, PayloadName, Method, RequestCommand);
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "GetPayloadXML()", "GetPayloadXML Exception : " + Convert.ToString(ex), PayloadName, RequestCommand);
                //this.WriteToFile("GetPayloadXML : " + ex);
            }
        }

        public string GetXMLResult(string Payload, string Command, string Method, string RequestCommand)
        {
            string url = "";
            string body = "";
            string ResponseBody = "";
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                if (GetCookie(Command))
                {
                    #region ********** Insert Action Log **********
                    //objCVerifone.InsertActiveLog("Verifone", "Start", "GetXMLResult()", "Initializing XML Result", Command, RequestCommand);
                    #endregion

                    switch (Command)
                    {
                        case "Department":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vposcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "Tax":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vtaxratecfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "Payment":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vpaymentcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "Category":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vposcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "prodCodes":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vposcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "Fee":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vfeecfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "Item":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vPLUs&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "Fuel":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=vfuelrtcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "LogPeriod":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vtlogpdlist&cookie=" + lblCookieValue + "";
                            break;

                        case "Invoice":
                            GetPeriod(Command, Method, body, RequestCommand);
                            RequestUrl = false;
                            #region ********** Insert Action Log **********
                            objCVerifone.InsertActiveLog("Verifone", "End", "GetXMLResult()", "Verifone XML Result Saved Successfully", Command, RequestCommand);
                            #endregion
                            break;

                        case "UpdateTax":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=utaxratecfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "UpdatePayment":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=upaymentcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "UpdateCategory":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=uposcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "UpdateDepartment":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=uposcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            objCVerifone.InsertActiveLog("Verifone", "Error", "GetPayloadXML()", url + body, Payload, RequestCommand);

                            break;

                        case "UpdateFees":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=ufeecfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "UpdateItems":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=uPLUs&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "UpdateFuel":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=ufuelcfg&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "UpdateCashierUser":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=upossecurity&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "Report":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vreportpdlist&cookie=" + lblCookieValue + "";
                            break;

                        case "RubyReport_Daily":
                        case "RubyReport_Shift":
                        case "RubyReport_Month":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vrubyrept&reptname=summary&period=" + period + "&filename=" + PeriodFileName + "&cookie=" + lblCookieValue;
                            RubyPeriodFileName = Command + "_" + period + "_" + PeriodFileName;
                            break;

                        case "CashierUser":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vpossecurity&cookie=" + lblCookieValue + "";
                            // objCVerifone.InsertActiveLog("url", "End", "CashierUser()", url, url, url);
                            break;

                        case "MixMatch":
                            // {cookie}
                            url = VerifoneServices.VerifoneLink + "cgi-bin/NAXML?cmd=vMaintenance&dataset=MixMatch&cookie=" + lblCookieValue + "";
                            break;

                        case "MixMatch_ItemList":
                            //https://site-controller_IP/cgi-bin/NAXML?cmd=vMaintenance&dataset=ItemList&cookie={cookie}
                            url = VerifoneServices.VerifoneLink + "cgi-bin/NAXML?cmd=vMaintenance&dataset=ItemList&cookie=" + lblCookieValue + "";
                            break;

                        case "Register":
                            url = VerifoneServices.VerifoneLink + "/cgi-bin/CGILink?cmd=vAppInfo";
                            break;

                        case "UpdateMixMatch_ItemList":
                            //https://{site-controller_IP}/cgi-bin/NAXML?cmd=uMaintenance&dataset=ItemList&cookie={cookie}
                            //url = VerifoneServices.VerifoneLink + "cgi-bin/NAXML?cmd=uMaintenance&dataset=ItemList&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            url = VerifoneServices.VerifoneLink + "cgi-bin/NAXML?";
                            body = "cmd=uMaintenance&dataset=ItemList&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            objCVerifone.InsertActiveLog("Verifone", "UpdateMixMatch_ItemList URL", "GetXMLResult()", "GetXMLResult url : " + url, Command, RequestCommand);
                            break;

                        case "UpdateMixMatch":
                            //https://{site-controller_IP}/cgi-bin/NAXML?cmd=uMaintenance&dataset=MixMatch&cookie={cookie}
                            //url = VerifoneServices.VerifoneLink + "cgi-bin/NAXML?cmd=uMaintenance&dataset=MixMatch&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            url = VerifoneServices.VerifoneLink + "cgi-bin/NAXML?";
                            body = "cmd=uMaintenance&dataset=MixMatch&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            objCVerifone.InsertActiveLog("Verifone", "UpdateMixMatch_ItemList URL", "GetXMLResult()", "GetXMLResult url : " + url, Command, RequestCommand);

                            break;

                        case "Employee":
                            // {cookie}
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vpossecurity&cookie=" + lblCookieValue + "";
                            break;
                        case "UserRole":
                            // {cookie}
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vuseradmin&cookie=" + lblCookieValue + "";
                            break;

                        case "UpdateUser":
                            //Change the password for the user associated with a credential
                            //https://{site-controller_IP}/cgi-bin/CGIUplink?cmd=changepasswd&cookie={cookie}
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGIUplink?";
                            body = "cmd=changepasswd&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;

                        case "InsertUserRole":
                            url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?";
                            body = "cmd=uuseradmin&cookie=" + lblCookieValue + "\r\n\r\n" + Payload;
                            break;
                    }
                    if (RequestUrl == true)
                    {
                        ResponseBody = GetRequestUrl(url, Method, body, Command, RequestCommand);

                        #region ********** Insert Action Log **********
                        //objCVerifone.InsertActiveLog("Verifone", "End", "GetXMLResult()", "Verifone XML Result Saved Successfully", Command, RequestCommand);
                        #endregion
                    }
                }
                ReleaseGetCookie(Command);
                RequestUrl = true;
                return ResponseBody;
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "GetXMLResult()", "GetXMLResult Exception : " + ex, Command, RequestCommand);
                //this.WriteToFile("GetXMLResult : " + ex);
                return ResponseBody;
            }
        }

        public bool GetCookie(string cmd)
        {
            VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
            try
            {
                #region ********** Insert Action Log **********
                //objCVerifone.InsertActiveLog("Verifone", "Start", "GetCookie()", "Initializing Verifone Connection to Server", cmd, "validate");
                #endregion




                string rsp = "";
                string getcokkieurl = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=validate&user=" + VerifoneServices.VerifoneUserName + "&passwd=" + VerifoneServices.VerifonePassword;
                //objCVerifone.InsertActiveLog("Verifone", "Error", "GetCookie()", "GetCookie msg : " + getcokkieurl, cmd, "validate");
                rsp = GetRequestUrl(getcokkieurl, "GET", "", "", "validate");
                if (rsp != null && Convert.ToString(rsp) != "")
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(new XmlTextReader(new StringReader(rsp)));
                    if (ds.Tables["credential"] != null)
                    {
                        lblCookieValue = ds.Tables["credential"].Rows[0]["cookie"].ToString();
                    }

                    #region ********** Insert Action Log **********
                    if (lblCookieValue != "" && lblCookieValue != null)
                    {
                        return true;
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("Verifone", "Fail", "GetCookie()", "Verifone not Connected to Server", cmd, "validate");
                        return false;
                    }
                }
                else
                {
                    objCVerifone.InsertActiveLog("Verifone", "Fail", "GetCookie()", "GetRequestUrl Response Null or blank", cmd, "validate");
                    return false;
                }
                    #endregion
            }
            catch (Exception ex)
            {

                objCVerifone.InsertActiveLog("Verifone", "Error", "GetCookie()", "GetCookie Exception : " + Convert.ToString(ex), cmd, "validate");
                //this.WriteToFile("GetCookie : " + ex);
                return false;
            }
        }

        public string GetRequestUrl(string url, string Method, string body, string methodname, string RequestCommand)
        {
            string responseBody = null;
            _CVerifone objCVerifone = new _CVerifone();
            string datetime = DateTime.Now.ToString("ddMMyyyyHHmmss");
            try
            {
                //objCVerifone.InsertActiveLog("Verifone", "check", "GetRequestUrl()", url + " - " + Method + " - " + body + " - " +methodname + " - " +RequestCommand , methodname, RequestCommand);
                #region ********** Insert Action Log **********
                if (RequestCommand != "validate" && RequestCommand != "releaseCredential")
                {
                    objCVerifone.InsertActiveLog("Verifone", "Start", "GetRequestUrl()", "Initializing Url Request", methodname, RequestCommand);
                }
                #endregion

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpWebResponse resp;
                req.Method = Method;
                req.Timeout = 90000;

                if (body != null && body != "")
                {
                    byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
                    req.GetRequestStream().Write(bodyBytes, 0, bodyBytes.Length);
                    req.GetRequestStream().Close();
                }

                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                    Stream respStream = resp.GetResponseStream();
                    if (respStream != null)
                    {
                        responseBody = new StreamReader(respStream).ReadToEnd();
                    }
                    else
                    {
                        Console.WriteLine("HttpWebResponse.GetResponseStream returned null");
                    }
                    if (methodname != "" && methodname != "LogPeriod" && methodname != "Invoice" && methodname != "Report" && methodname != "RubyReport_Daily" &&
                        methodname != "RubyReport_Shift" && methodname != "RubyReport_Month")
                    {
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\" + methodname + ".xml");
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(responseBody);
                        xdoc.Save(AppDomain.CurrentDomain.BaseDirectory + "xml\\" + methodname + ".xml");
                        if (RequestCommand != "validate" && RequestCommand != "releaseCredential")
                        {
                            #region ********** Insert Action Log **********
                            objCVerifone.InsertActiveLog("Verifone", "End", "GetRequestUrl()", "Verifone Command Response Saved", methodname, RequestCommand);
                            #endregion
                        }
                    }
                    else if (methodname == "LogPeriod")
                    {
                        #region Archive old period file
                        CommanArchiveFiles(methodname);
                        #endregion
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(responseBody);
                        xdoc.Save(AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\" + methodname + "_" + datetime + ".xml");
                        PeriodFileName = methodname + "_" + datetime + ".xml";

                        #region ********** Insert Action Log **********
                        objCVerifone.InsertActiveLog("Verifone", "End", "GetRequestUrl()", "Verifone Command Response Saved", methodname, RequestCommand);
                        #endregion
                    }
                    else if (methodname == "Invoice")
                    {
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(responseBody);
                        xdoc.Save(AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\" + InvoiceFileName + ".xml");

                        #region ********** Insert Action Log **********
                        objCVerifone.InsertActiveLog("Verifone", "End", "GetRequestUrl()", "Verifone Command Response Saved", methodname, RequestCommand);
                        #endregion
                    }
                    else if (methodname == "Report")
                    {
                        #region Archive old file
                        CommanArchiveFiles(methodname);
                        #endregion

                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(responseBody);
                        xdoc.Save(AppDomain.CurrentDomain.BaseDirectory + "xml\\Report\\" + methodname + "_" + datetime + ".xml");

                        #region ********** Insert Action Log **********
                        objCVerifone.InsertActiveLog("Verifone", "End", "GetRequestUrl()", "Verifone Command Response Saved", methodname, RequestCommand);
                        #endregion
                    }
                    else if (methodname == "RubyReport_Daily")
                    {
                        #region Archive old period file
                        //CommanArchiveFiles(methodname);
                        #endregion

                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(responseBody);
                        xdoc.Save(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\" + RubyPeriodFileName + ".xml");

                        #region ********** Insert Action Log **********
                        objCVerifone.InsertActiveLog("Verifone", "End", "GetRequestUrl()", "Verifone Command Response Saved", methodname, RequestCommand);
                        #endregion
                    }
                    else if (methodname == "RubyReport_Shift")
                    {
                        #region Archive old period file
                        //CommanArchiveFiles(methodname);
                        #endregion

                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(responseBody);
                        xdoc.Save(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\" + RubyPeriodFileName + ".xml");

                        #region ********** Insert Action Log **********
                        objCVerifone.InsertActiveLog("Verifone", "End", "GetRequestUrl()", "Verifone Command Response Saved", methodname, RequestCommand);
                        #endregion
                    }
                    else if (methodname == "RubyReport_Month")
                    {
                        #region Archive old period file
                        //CommanArchiveFiles(methodname);
                        #endregion

                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(responseBody);
                        xdoc.Save(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\" + RubyPeriodFileName + ".xml");

                        #region ********** Insert Action Log **********
                        objCVerifone.InsertActiveLog("Verifone", "End", "GetRequestUrl()", "Verifone Command Response Saved", methodname, RequestCommand);
                        #endregion
                    }
                }
                catch (WebException e)
                {
                    resp = (HttpWebResponse)e.Response;
                    objCVerifone.InsertActiveLog("Verifone", "Error", "GetRequestUrl()", "GetRequestUrl Response Exception : " + e.ToString(), methodname, RequestCommand);
                }
                return responseBody;
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "GetRequestUrl()", "GetRequestUrl Exception : " + ex, methodname, RequestCommand);
                return responseBody;
            }
        }

        public void ReleaseGetCookie(string cmd)
        {
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                string rsp = "";
                string getcokkieurl = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=releaseCredential&cookie=" + lblCookieValue;
                rsp = GetRequestUrl(getcokkieurl, "GET", "", "releaseCredential", "releaseCredential");
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "ReleaseGetCookie()", "ReleaseGetCookie Exception : " + ex, cmd, "releaseCredential");

            }
        }

        public void GetPeriod(string Command, string Method, string body, string RequestCommand)
        {
            //PeriodFileName = "LogPeriod.xml";
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                var Period = "";
                var FileName = "";
                DataSet ds = new DataSet();
                int reportParameterCount = 0;
                var PeriodFilename_Path = AppDomain.CurrentDomain.BaseDirectory + "Period_FileName.xml";

                if (File.Exists(PeriodFilename_Path) == true)
                {
                    ds.ReadXml(PeriodFilename_Path, XmlReadMode.InferSchema);
                }

                #region get period to get invoice file
                var path = AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\" + PeriodFileName;

                XmlSerializer serializer = new XmlSerializer(typeof(Credential.periodList));

                Credential.periodList resultingMessage = (Credential.periodList)serializer.Deserialize(new XmlTextReader(path));

                int periodInfoCount = resultingMessage.periodInfo.Count();
                Credential.periodInfo[] objPeriodInfo = new Credential.periodInfo[periodInfoCount];
                objPeriodInfo = resultingMessage.periodInfo;

                CommanArchiveFiles("Invoice");   //Archived all invoice files
                #endregion

                for (int i = 0; i < objPeriodInfo.Length; i++)
                {
                    //if (((Credential.periodCType)(objPeriodInfo[i].Items[0])).sysid == "2" && objPeriodInfo[i].name != "current")
                    if (((Credential.periodCType)(objPeriodInfo[i].Items[0])).sysid == "2")
                    {
                        reportParameterCount = objPeriodInfo[i].reportParameters.reportParameter.Count();
                        Credential.periodInfoReportParametersReportParameter[] objReportParameter = new Credential.periodInfoReportParametersReportParameter[reportParameterCount];
                        objReportParameter = objPeriodInfo[i].reportParameters.reportParameter;

                        for (int j = 0; j < objReportParameter.Length; j++)
                        {
                            if (objReportParameter[j].name == "period")
                            {
                                Period = objReportParameter[j].Value;
                            }
                            else if (objReportParameter[j].name == "filename")
                            {
                                FileName = objReportParameter[j].Value;
                            }
                        }

                        #region Archived File
                        if (ds.Tables.Count > 0)
                        {
                            DataView dv = ds.Tables[0].DefaultView;
                            dv.RowFilter = ("[" + ds.Tables[0].Columns[0].ColumnName + "] ='" + Period + "'AND [" + ds.Tables[0].Columns[1].ColumnName + "] ='" + FileName + "'");

                            if (dv.Count == 0)
                            {
                                #region generate invoice file
                                try
                                {
                                    string url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vtransset&period=" + Period + "&filename=" + FileName + "&cookie=" + lblCookieValue + "";
                                    //http://192.168.31.11/cgi-bin/CGILink?cmd=vtransset&period=1&filename=2019-06-30.622&cookie=;
                                    InvoiceFileName = Command + "_" + Period + "_" + FileName;
                                    GetRequestUrl(url, Method, body, Command, RequestCommand);
                                }
                                catch (Exception ex)
                                {
                                    objCVerifone.InsertActiveLog("BoF", "Error", "GetPeriod()", "GetPeriod ex:" + ex, "", "");
                                }
                                #endregion

                                //objCVerifone.InsertActiveLog("BoF", "End", "GetPeriod()", "generate new period file", "", "");
                            }
                            else
                            {
                                //objCVerifone.InsertActiveLog("BoF", "End", "GetPeriod()", "period filename already exists", "", "");
                            }
                        }
                        else
                        {
                            #region generate invoice file
                            try
                            {
                                string url = VerifoneServices.VerifoneLink + "cgi-bin/CGILink?cmd=vtransset&period=" + Period + "&filename=" + FileName + "&cookie=" + lblCookieValue + "";
                                InvoiceFileName = Command + "_" + Period + "_" + FileName;
                                GetRequestUrl(url, Method, body, Command, RequestCommand);
                            }
                            catch (Exception ex)
                            {
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "GetPeriod()", "GetPeriod Exception :  " + Convert.ToString(PeriodFileName) + "->" + ex, "", "");
            }
        }

        public DataSet GetRegsiterinv()
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objVerifone.GetRegisterno();
                return ds;
            }
            catch (Exception ex)
            {
                return ds;
            }
        }


        public DataSet GetRegsiteridZZReport(long monthlyInvoiceBegin, long monthlyInvoiceEnd, long Month_RegisterId, DateTime OpnDate, DateTime CloseDate)
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objVerifone.GetRegsiteridZZReport(monthlyInvoiceBegin, monthlyInvoiceEnd, Month_RegisterId, OpnDate, CloseDate);
                return ds;
            }
            catch (Exception ex)
            {
                return ds;
            }
        }


        public DataSet GetInvoiceNo()
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objCVerifone.GetInvoiceNo();
                return ds;
            }
            catch (Exception ex)
            {
                //this.WriteToFile("GetInvoiceNo : " + ex);
                return ds;
            }
        }

        public DataSet Getdropamountid()
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objCVerifone.Getdropamountid();
                return ds;
            }
            catch (Exception ex)
            {
                //this.WriteToFile("GetInvoiceNo : " + ex);
                return ds;
            }
        }


        public DataSet GetDiscountID()
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objCVerifone.GetDiscountID();
                return ds;
            }
            catch (Exception ex)
            {
                //this.WriteToFile("GetInvoiceNo : " + ex);
                return ds;
            }
        }


        public DataSet GetShiftID()
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objCVerifone.GetShiftID();
                return ds;
            }
            catch (Exception ex)
            {
                //this.WriteToFile("GetInvoiceNo : " + ex);
                return ds;
            }
        }



        public DataSet GetDataDepthorwItem(long DepID)
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objVerifone.GetDataDepthorwItem(DepID);
                return ds;
            }
            catch (Exception ex)
            {
                return ds;
            }
        }


        public string GetXMLFromObject(object o)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = null;
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                //objCVerifone.InsertActiveLog("Verifone", "Start", "GetXMLFromObject()", "Initialize XML Serialize", "", "");

                XmlSerializer s = new XmlSerializer(o.GetType());
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                tw = new XmlTextWriter(sw);

                s.Serialize(tw, o, ns);
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "GetXMLFromObject()", "GetXMLFromObject Exception : " + Convert.ToString(ex), "", "");
            }
            finally
            {
                sw.Close();
                if (tw != null)
                {
                    tw.Close();
                }
                //objCVerifone.InsertActiveLog("Verifone", "End", "GetXMLFromObject()", "XML Serialize successfully", "", "");
            }
            return sw.ToString();
        }

        #region commented
        //public void ArchiveOldFiles(string FileName, string MethodName)
        //{
        //    _CVerifone objCVerifone = new _CVerifone();
        //    try
        //    {
        //        var SourcePath = "";
        //        var DestinationPath = "";
        //        //var myFile1 = File.Create(FileName);
        //        //myFile1.Close();
        //        objCVerifone.InsertActiveLog("BoF", "Start", "ArchiveOldFiles()", "Initializing Archived File", MethodName, "");
        //        if (MethodName == "LogPeriod")
        //        {
        //            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\" + FileName;
        //            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\Archived_Files\\" + FileName;
        //            File.Move(SourcePath, DestinationPath);
        //            objCVerifone.InsertActiveLog("BoF", "End", "ArchiveOldFiles()", "LogPeriod File Archived Successfully", MethodName, "");
        //        }
        //        else if (MethodName == "Invoice")
        //        {
        //            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\" + FileName;
        //            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\Archived_Files\\" + FileName;
        //            File.Move(SourcePath, DestinationPath);
        //            objCVerifone.InsertActiveLog("BoF", "End", "ArchiveOldFiles()", "Invoice File Archived Successfully", MethodName, "");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objCVerifone.InsertActiveLog("BoF", "Fail", "ArchiveOldFiles()", "ArchiveOldFiles Exception : " + ex, MethodName, "");
        //    }
        //}
        #endregion

        public void CreateXML(string Period, string FileName, string PeriodFilePath, string XMLStatus, string Type)
        {
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                //objCVerifone.InsertActiveLog("BoF", "Start", "CreateXML()", "Initializing To Create XML", "Invoice", "");
                if (Type == "Invoice")
                {
                    if (XMLStatus == "New")
                    {
                        #region New code to create an xml
                        // Create a root node
                        XElement XPeriodInfo = new XElement("PeriodInfo");
                        // Add child nodes
                        //XAttribute Period_FileName = new XAttribute("Period_FileName", "");
                        XElement XPeriod = new XElement("Period", Period);
                        XElement XFileName = new XElement("FileName", FileName);
                        //XElement publisher = new XElement("Publisher", "Addison-Wesley");
                        XElement Period_FileName = new XElement("Period_FileName");
                        //Period_FileName.Add(Period_FileName);
                        Period_FileName.Add(XPeriod);
                        Period_FileName.Add(XFileName);
                        //author.Add(publisher);
                        XPeriodInfo.Add(Period_FileName);
                        XPeriodInfo.Save(AppDomain.CurrentDomain.BaseDirectory + "Period_FileName.xml");
                        #endregion

                        //objCVerifone.InsertActiveLog("BoF", "End", "CreateXML()", "New XML Saved Successfully", "Invoice", "");
                    }
                    else if (XMLStatus == "Exists")
                    {
                        XDocument doc = XDocument.Load(PeriodFilePath);
                        XElement period = doc.Element("PeriodInfo");
                        period.Add(new XElement("Period_FileName",
                                   new XElement("Period", Period),
                                   new XElement("FileName", FileName)));
                        doc.Save(PeriodFilePath);
                        //objCVerifone.InsertActiveLog("BoF", "End", "CreateXML()", "XML Updated Successfully", "Invoice", "");
                    }
                }
                else if (Type == "RubyReport_Daily" || Type == "RubyReport_Shift" || Type == "RubyReport_Month")
                {
                    string[] date = FileName.Split('.');
                    string periodDate = date[0];
                    int Sysid = Convert.ToInt16(date[1]);
                    string Name = Type == "RubyReport_Daily" ? "Daily" : Type == "RubyReport_Shift" ? "Shift" : Type == "RubyReport_Month" ? "Month" : "";
                    if (XMLStatus == "New")
                    {
                        // Create a root node
                        XElement XRubyReport = new XElement("RubyReport");
                        // Add child nodes
                        XAttribute XSysid = new XAttribute("Sysid", Sysid);
                        XAttribute XDate = new XAttribute("Date", periodDate);
                        XAttribute XName = new XAttribute("Name", Name);
                        XElement XPeriod = new XElement("Period", Period);
                        XElement XFileName = new XElement("FileName", FileName);
                        XElement Report = new XElement("Report", XSysid, XDate, XName);
                        Report.Add(XPeriod);
                        Report.Add(XFileName);
                        XRubyReport.Add(Report);
                        XRubyReport.Save(AppDomain.CurrentDomain.BaseDirectory + "RubyReport_Period_FileName.xml");
                    }
                    else if (XMLStatus == "Exists")
                    {
                        XDocument doc = XDocument.Load(PeriodFilePath);
                        XElement period = doc.Element("RubyReport");
                        XAttribute XSysid = new XAttribute("Sysid", Sysid);
                        XAttribute XDate = new XAttribute("Date", periodDate);
                        XAttribute XName = new XAttribute("Name", Name);
                        period.Add(new XElement("Report", XSysid, XDate, XName,
                                   new XElement("Period", Period),
                                   new XElement("FileName", FileName)));
                        doc.Save(PeriodFilePath);

                        //objCVerifone.InsertActiveLog("BoF", "End", "CreateXML()", "XML Updated Successfully", "Invoice", "");
                    }
                }

            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "CreateXML()", "CreateXML Exception : " + ex, "Invoice", "");
            }
        }

        public void CommanArchiveFiles(string MethodName)
        {
            string checkpath = "";
            var SourcePath = "";
            var DestinationPath = "";
            bool FileExists = false;
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                //objCVerifone.InsertActiveLog("BoF", "Start", "CommanArchiveFiles()", "Initializing to Archive the File/s", MethodName, "");
                if (MethodName == "LogPeriod")
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Period";
                    string[] filePaths = Directory.GetFiles(checkpath);
                    if (filePaths.Length > 0)
                    {
                        for (int i = 0; i < filePaths.Length; i++)
                        {
                            //ArchiveOldFiles(Path.GetFileName(filePaths[i]), MethodName);
                            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\" + Path.GetFileName(filePaths[i]);
                            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\Archived_Files\\" + Path.GetFileName(filePaths[i]);
                            FileExists = File.Exists(DestinationPath);
                            if (FileExists == false)
                            {
                                File.Move(SourcePath, DestinationPath);
                                //objCVerifone.InsertActiveLog("BoF", "End", "CommanArchiveFiles()", "LogPeriod File/s Archived Successfully", MethodName, "");
                            }

                        }
                        //objCVerifone.InsertActiveLog("BoF", "End", "CommanArchiveFiles()", "LogPeriod File/s Archived Successfully", MethodName, "");
                    }
                }
                else if (MethodName == "Invoice")
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice";
                    string[] filePaths = Directory.GetFiles(checkpath);
                    if (filePaths.Length > 0)
                    {
                        for (int i = 0; i < filePaths.Length; i++)
                        {
                            //ArchiveOldFiles(Path.GetFileName(filePaths[i]), methodname);
                            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\" + Path.GetFileName(filePaths[i]);
                            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\Archived_Files\\" + Path.GetFileName(filePaths[i]);
                            FileExists = File.Exists(DestinationPath);
                            if (FileExists == true)
                            {
                                try
                                {
                                    File.Delete(DestinationPath);
                                }
                                catch (Exception)
                                {
                                }

                            }
                            File.Move(SourcePath, DestinationPath);

                        }
                        //objCVerifone.InsertActiveLog("BoF", "End", "CommanArchiveFiles()", "Invoice File/s Archived Successfully", MethodName, "");
                    }
                }
                else if (MethodName == "Report")
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Report";
                    string[] filePaths = Directory.GetFiles(checkpath);
                    if (filePaths.Length > 0)
                    {
                        for (int i = 0; i < filePaths.Length; i++)
                        {
                            //ArchiveOldFiles(Path.GetFileName(filePaths[i]), methodname);
                            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Report\\" + Path.GetFileName(filePaths[i]);
                            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\Report\\Archived_Files\\" + Path.GetFileName(filePaths[i]);
                            FileExists = File.Exists(DestinationPath);
                            if (FileExists == false)
                            {
                                File.Move(SourcePath, DestinationPath);
                                // objCVerifone.InsertActiveLog("BoF", "End", "CommanArchiveFiles()", "Report File/s Archived Successfully", MethodName, "");
                            }

                        }
                    }
                }
                else if (MethodName == "RubyReport_Daily")
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily";
                    string[] filePaths = Directory.GetFiles(checkpath);
                    if (filePaths.Length > 0)
                    {
                        for (int i = 0; i < filePaths.Length; i++)
                        {
                            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\" + Path.GetFileName(filePaths[i]);
                            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\Archived_Files\\" + Path.GetFileName(filePaths[i]);
                            FileExists = File.Exists(DestinationPath);
                            if (FileExists == true)
                            {
                                try
                                {
                                    File.Delete(DestinationPath);
                                }
                                catch (Exception)
                                {
                                }

                            }
                            File.Move(SourcePath, DestinationPath);
                        }
                    }
                }
                else if (MethodName == "RubyReport_Shift")
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift";
                    string[] filePaths = Directory.GetFiles(checkpath);
                    if (filePaths.Length > 0)
                    {
                        for (int i = 0; i < filePaths.Length; i++)
                        {
                            //ArchiveOldFiles(Path.GetFileName(filePaths[i]), methodname);
                            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\" + Path.GetFileName(filePaths[i]);
                            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\Archived_Files\\" + Path.GetFileName(filePaths[i]);
                            FileExists = File.Exists(DestinationPath);
                            if (FileExists == true)
                            {
                                try
                                {
                                    File.Delete(DestinationPath);
                                }
                                catch (Exception)
                                {
                                }

                            }
                            File.Move(SourcePath, DestinationPath);
                        }
                    }
                }
                else if (MethodName == "RubyReport_Month")
                {
                    checkpath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month";
                    string[] filePaths = Directory.GetFiles(checkpath);
                    if (filePaths.Length > 0)
                    {
                        for (int i = 0; i < filePaths.Length; i++)
                        {
                            //ArchiveOldFiles(Path.GetFileName(filePaths[i]), methodname);
                            SourcePath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\" + Path.GetFileName(filePaths[i]);
                            DestinationPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\Archived_Files\\" + Path.GetFileName(filePaths[i]);
                            FileExists = File.Exists(DestinationPath);
                            if (FileExists == true)
                            {
                                try
                                {
                                    File.Delete(DestinationPath);
                                }
                                catch (Exception)
                                {
                                }

                            }
                            File.Move(SourcePath, DestinationPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "CommanArchiveFiles()", "CommanArchiveFiles Exception  " + SourcePath + " : " + ex, MethodName, "");
            }
        }

        public DateTime SplitDate(string Date)
        {
            DateTime FinalDate = DateTime.Now;
            string[] beginDate = Date.Split('T');
            try
            {
                if (beginDate != null && beginDate.Length > 0)
                {
                    string[] timestring = beginDate[1].Split('-');
                    if (timestring != null && timestring.Length > 0)
                    {
                        FinalDate = Convert.ToDateTime(beginDate[0] + " " + timestring[0]);
                    }
                    else
                    {
                        FinalDate = Convert.ToDateTime(beginDate[0] + " 00:00:00");
                    }
                }
                else
                {
                    FinalDate = Convert.ToDateTime(Date.Replace("T", " ").Replace("-05:00", ""));
                }
                return FinalDate;
            }
            catch (Exception ex)
            {
                try
                {
                    FinalDate = Convert.ToDateTime(Date.Replace("T", " ").Replace("-05:00", ""));
                }
                catch (Exception)
                {
                    FinalDate = Convert.ToDateTime(beginDate);
                }
                return FinalDate;
            }
        }


        public DataSet GetDeptItemData(long DeptId)
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objCVerifone.GetDeptItemData(DeptId);
                return ds;
            }
            catch (Exception ex)
            {
                //this.WriteToFile("GetInvoiceNo : " + ex);
                return ds;
            }
        }

        public DataSet GetGasDATA()
        {
            DataSet ds = new DataSet();
            try
            {
                VerifoneLibrary.DataAccess._CVerifone objCVerifone = new VerifoneLibrary.DataAccess._CVerifone();
                ds = objCVerifone.GetGasDATA();
                return ds;
            }
            catch (Exception ex)
            {
                //this.WriteToFile("GetInvoiceNo : " + ex);
                return ds;
            }
        }

        public string GetXMLFromObject_standalone(object o)
        {
            #region utc-16 & standalone - commented
            //XmlSerializer xsSubmit = new XmlSerializer(typeof(RapidVerifoneNAXML.NAXMLMaintenanceRequest));

            //string xml = "";

            //using (var sww = new StringWriter())
            //{
            //    using (XmlWriter writer = XmlWriter.Create(sww))
            //    {
            //        writer.WriteStartDocument(true);
            //        xsSubmit.Serialize(writer, o);
            //        xml = sww.ToString(); // Your XML
            //    }
            //}

            //return xml.ToString();
            #endregion

            #region another way     (utc - 8 & standalone)
            string xml = "";
            using (var sw = new Utf8StringWriter())
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
            {
                xw.WriteStartDocument(true); // that bool parameter is called "standalone"

                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                var xmlSerializer = new XmlSerializer(typeof(RapidVerifoneNAXML.NAXMLMaintenanceRequest));
                xmlSerializer.Serialize(xw, o);

                xml = sw.ToString();
            }
            return xml.ToString();
            #endregion
        }

        #region Update Database Server file
        public bool UpdateDatabaseServerFile(string New_VP, string New_VU = "")
        {
            //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataSource\\DatabaseServers.xml");
            var path = AppDomain.CurrentDomain.BaseDirectory + "DataSource\\DatabaseServers.xml";
            _CVerifone objCVerifone = new _CVerifone();
            try
            {
                #region old code
                //XmlDocument doc = new XmlDocument();
                //doc.Load(path);

                //XmlNodeList aDateNodes = doc.SelectNodes("/VerifoneServers/VerifoneServer");

                //objCVerifone.InsertActiveLog("BoF", "Begin", "3()", aDateNodes.ToString(), "", "");

                //foreach (XmlNode aDateNode in aDateNodes)
                //{
                //    objCVerifone.InsertActiveLog("BoF", "Begin", "4()", aDateNode.ToString(), "", "");
                //    XmlAttribute DateAttribute = aDateNode.Attributes["VP"];
                //    aDateNode.InnerText = New_VP;
                //    objCVerifone.InsertActiveLog("BoF", "Begin", "5()", aDateNode.InnerText.ToString(), "", "");
                //}

                //doc.Save(path);
                #endregion

                XDocument xmlFile = XDocument.Load(path);
                var query = from c in xmlFile.Elements("VerifoneServers").Elements("VerifoneServer")
                            select c;

                foreach (XElement book in query)
                {
                    book.Attribute("VP").Value = New_VP;
                    if (New_VU != "")
                    {
                        book.Attribute("VU").Value = New_VU;
                    }
                }
                xmlFile.Save(path);

                #region check either pwd updated or not in DS file
                XDocument xdoc = XDocument.Load(path);
                XElement dbServers = xdoc.Element("VerifoneServers");
                var dbServerDetailResult = (from dbServer in dbServers.Elements("VerifoneServer")
                                            select new
                                            {
                                                VP = dbServer.Attribute("VP").Value,
                                                VU = dbServer.Attribute("VU").Value,
                                            }).ToList()[0];

                //string NewChanged_VP = dbServerDetailResult.VP.Decript();
                //string NewChanged_VU = dbServerDetailResult.VU.Decript(); 

                string NewChanged_VP = dbServerDetailResult.VP;
                string NewChanged_VU = dbServerDetailResult.VU;

                objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", NewChanged_VP, "", "");
                objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", New_VP, "", "");
                if (NewChanged_VP == New_VP)
                {
                    objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "1", "", "");

                    if (New_VU != "")
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "2", "", "");
                        if (NewChanged_VU == New_VU)
                        {
                            objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "3", "", "");
                            VerifoneServices.VerifoneUserName = New_VU;
                            objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "DatabaseServers.xml file updated", "", "");
                            return true;
                        }
                        else
                        {
                            objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "4", "", "");
                            objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "DatabaseServers.xml file not updated", "", "");
                            return false;
                        }
                    }
                    else
                    {
                        objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "5", "", "");
                        VerifoneServices.VerifonePassword = New_VP;
                        objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "DatabaseServers.xml file updated", "", "");
                        return true;
                    }
                }
                else
                {
                    objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "6", "", "");
                    objCVerifone.InsertActiveLog("BoF", "End", "UpdateDatabaseServerFile()", "DatabaseServers.xml file not updated", "", "");
                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "UpdateDatabaseServerFile()", "UpdateDatabaseServerFile Exception : " + ex, "User", "");
                return false;
            }
        }
        #endregion

        private static Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #region Email
        public void SendEmail(string Subject, string Message)
        {
            try
            {
                MailMessage message = new MailMessage();
                string SMTPFROM = ConfigurationManager.AppSettings["SMTPFROM"];
                string MailTo = ConfigurationManager.AppSettings["MailTo"];
                string Body = "";
                message.From = new MailAddress(SMTPFROM);
                message.To.Add(MailTo);
                message.Subject = Subject;
                message.IsBodyHtml = true;
                Body = "<b>DatabaseName</b> : " + VerifoneLibrary.DataAccess._CVerifone.DTN;
                Body += "<br/><b>ErrorMessage</b> : " + Message;
                message.Body = Body;


                SendMailAnync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SendMailAnync(MailMessage message)
        {
            try
            {
                string SMTPFROM = ConfigurationManager.AppSettings["SMTPFROM"];
                string SMTPFROMPWD = ConfigurationManager.AppSettings["SMTPFROMPWD"];
                string SMTPIP = ConfigurationManager.AppSettings["SMTPIP"];
                int SMTPPORT = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPORT"]);

                SmtpClient smtp = new SmtpClient(SMTPIP, SMTPPORT);
                smtp.UseDefaultCredentials = false;
                var creds = new System.Net.NetworkCredential(SMTPFROM, SMTPFROMPWD);
                smtp.Credentials = creds;

                bool IsCheckCreadReq = true;
                new Thread(() =>
                {
                    try
                    {
                        if (IsCheckCreadReq)
                        {
                            smtp.EnableSsl = true;
                            smtp.Send(message);
                        }
                        else
                        {
                            smtp.Send(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
    }

    public enum WeekDays
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }

    #region email msgs
    public class EmailMessages
    {
        #region user
        public string UserSubject = "Verifone - User Password Blank";
        public string UserBody = "Rapid User Password found blank";
        #endregion
    }
    #endregion
}
