//using VerifoneLibrary;
using VerifoneLibrary.DataAccess;
//using RapidVerifone;
//using myCompany1111;
using System;
//using System.Collections.Generic;
//using System.ComponentModel;
using System.Configuration;
using System.Data;
//using System.Data.OleDb;
//using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Net;
using System.ServiceProcess;
//using System.Text;
using System.Threading;
//using System.Threading.Tasks;
//using System.Xml;

//using System.Xml.Serialization;
//using System.Collections;
using System.Xml.Linq;
//using System.Text.RegularExpressions;

namespace VerifoneServices
{
    public partial class VerifoneServices : ServiceBase
    {
        #region declare variables
        public string FilePath = "";
        public bool FileExists = false;
        public string lblCookieValue = "";
        public string filename = "";
        public string period = "";
        public string PeriodFileName = "";
        public bool RequestUrl = true;
        public static DataSet dataset = null;

        #region Varibles for Active Log
        public string ReturnVal = "";
        public string Action = "";
        public string Status = "";
        public string Description = "";
        public string Command = "";
        #endregion


        public static string VerifoneLink = "";
        public static string VerifoneUserName = "";
        public static string VerifonePassword = "";
        public static string ApplicationKey = "";

        public static string ServerDetail = "";
        public static string userName = "";
        public static string password = "";
        public static string ServiceIntervalTime = "";
        #endregion

        VerifoneInsert objInsert = new VerifoneInsert();
        VerifoneUpdate objUpdate = new VerifoneUpdate();
        VerifoneDelete objDelete = new VerifoneDelete();
        Comman objComman = new Comman();
        _CVerifone objCVerifone = new _CVerifone();
        private Timer Schedular;
        EmailMessages objEmail = new EmailMessages();

        public VerifoneServices()
        {
            InitializeComponent();
            SetVarible();
           // objInsert.Invoice_New_WithChanges();
            VerifoneDelete objVerifoneDelete = new VerifoneDelete();
             DeleteMasterData();
           // objInsert.Promotion();

            
        }

        protected override void OnStart(string[] args)
        {
            this.ScheduleService();
        }

        protected override void OnStop()
        {
            this.Schedular.Dispose();
        }

        public void ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));
                string mode = ConfigurationManager.AppSettings["Mode"].ToUpper();
                // this.WriteToFile("Simple Service Mode: " + mode + " {0}");

                //Set the Default Time.
                DateTime scheduledTime = DateTime.MinValue;

                if (mode == "DAILY")
                {
                    //Get the Scheduled Time from AppSettings.
                    scheduledTime = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["ScheduledTime"]);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next day.
                        scheduledTime = scheduledTime.AddDays(1);
                    }
                }

                if (mode.ToUpper() == "INTERVAL")
                {
                    //Get the Interval in Minutes from AppSettings.
                    int intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);
                    //int intervalMinutes = Convert.ToInt32(ServiceIntervalTime);

                    //Set the Scheduled Time by adding the Interval to Current Time.
                    scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next Interval.
                        scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                    }
                }

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                // this.WriteToFile("Simple Service scheduled to run after: " + schedule + " {0}");

                //Get the difference in Minutes between the Scheduled and Current Time.
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                //WriteToFile("Simple Service Error on: {0} " + ex.Message + ex.StackTrace);
                _CVerifone objVerifone = new _CVerifone();
                objVerifone.InsertActiveLog("Verifone", "Error", "ScheduleService()", "ScheduleService  : " + ex.Message, "ScheduleService", "");

                //Stop the Windows Service.
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private void SchedularCallback(object e)
        {
            CallVerifone();
            this.ScheduleService();
        }

        public void CallVerifone1()
        {
            try
            {
                WriteToFile("1");
                WriteToFile("2");
                WriteToFile("3");
                WriteToFile("4");
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "CallVerifone()", "CallVerifone Exception : " + ex, "CallVerifone", "");
            }
        }


        public void CallVerifone()
        {
            try
            {
                DeleteDummyfile();
                SetVarible();
              
                #region change Rapid pwd before expire on 86th day
                if (VerifoneUserName == "Rapid")
                {
                    DataSet DS = objCVerifone.CheckUserExpireOrNot(VerifoneUserName);

                    if (DS.Tables.Count > 0)  // nidhi - changes while code review
                    {
                        if (DS.Tables[0].Rows.Count > 0)
                        {
                            if (Convert.ToInt64(DS.Tables[0].Rows[0]["DayRemain"]) <= 5) // now need to update Rapid pwd in verifone bcoz it's near to expire
                            {
                                string Response = objUpdate.UpdateVerifoneUserPassword(DS.Tables[0]); // update pwd in verifone
                                if (Response != null && Response != "")
                                {
                                    bool Result = objComman.UpdateDatabaseServerFile(objUpdate.New_VP.Encript()); // update pwd in DS file
                                    if (Result == true)
                                    {
                                        // update pwd in BoF
                                        long Return = objCVerifone.UpdateUserPassword(VerifoneServices.VerifoneLink, VerifoneServices.VerifoneUserName, objUpdate.New_VP.Encript());
                                        UpdateMasterData();
                                        DeleteMasterData();
                                        InsertMasterData();
                                        objInsert.Invoice_New_WithChanges();
                                        objInsert.Ruby_Report();
                                    }
                                }
                            }
                            else // dont need to update Rapid pwd in verifone bcoz it's not near to expire
                            {
                                UpdateMasterData();
                                DeleteMasterData();
                                InsertMasterData();
                                objInsert.Invoice_New_WithChanges();
                                objInsert.Ruby_Report();
                            }
                        }
                    }
                }
                else // if manager user
                {
                    UpdateMasterData();
                    DeleteMasterData();
                    InsertMasterData();
                    objInsert.Invoice_New_WithChanges();
                    objInsert.Ruby_Report();

                    DataSet DS = objCVerifone.CheckUserExpireOrNot("Rapid"); // check Rapid user exists or not in BoF (pwd should not be blanked)
                    if (DS.Tables.Count > 0)
                    {
                        if (DS.Tables[0].Rows.Count > 0)
                        {
                            if (Convert.ToString(DS.Tables[0].Rows[0]["CommandPassword"]) != "")
                            {
                                bool Result = objComman.UpdateDatabaseServerFile(Convert.ToString(DS.Tables[0].Rows[0]["CommandPassword"]),
                                                                                 Convert.ToString(DS.Tables[0].Rows[0]["UserName"]).Encript()); // update pwd in DS file
                            }
                            else
                            {
                                objComman.SendEmail(objEmail.UserSubject, objEmail.UserBody); // send mail if found blank pwd of Rapid user
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("BoF", "Error", "CallVerifone()", "CallVerifone Exception : " + ex, "CallVerifone", "");
            }
        }

        public void DeleteDummyfile()
        {
            try
            {
              
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\Invoice.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\Invoice.txt");
                }
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\Archived_Files\\InvoiceArchived_Files.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\Invoice\\Archived_Files\\InvoiceArchived_Files.txt");
                }


                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\Period.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\Period.txt");
                }
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\Archived_Files\\PeriodArchived_Files.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\Period\\Archived_Files\\PeriodArchived_Files.txt");
                }



                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\Report\\Report.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\Report\\Report.txt");
                }
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\Report\\Archived_Files\\ReportArchived_Files.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\Report\\Archived_Files\\ReportArchived_Files.txt");
                }


                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\Daily.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\Daily.txt");
                }
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\Archived_Files\\DailyArchived_Files.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Daily\\\\Archived_Files\\DailyArchived_Files.txt");
                }

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\Month.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\Month.txt");
                }
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\Archived_Files\\MonthArchived_Files.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Month\\\\Archived_Files\\MonthArchived_Files.txt");
                }

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\shift.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\shift.txt");
                }
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\Archived_Files\\shiftArchived_Files.txt"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "xml\\RubyReport\\Shift\\\\Archived_Files\\shiftArchived_Files.txt");
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "DeleteDummyfile()", "DeleteDummyfile  : " + ex.Message, "DeleteDummyfile", "");
            }
        }

        public void SetVarible()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataSource\\DatabaseServers.xml");
                XDocument xdoc = XDocument.Load(path);

                XElement dbServers = xdoc.Element("VerifoneServers");

                var dbServerDetailResult = (from dbServer in dbServers.Elements("VerifoneServer")
                                            select new
                                            {
                                                ServerId = Convert.ToInt32(dbServer.Attribute("id").Value),
                                                VL = dbServer.Attribute("VL").Value,
                                                VU = dbServer.Attribute("VU").Value,
                                                VP = dbServer.Attribute("VP").Value,
                                                ApplicationKey = dbServer.Attribute("ApplicationKey").Value
                                            }).ToList()[0];

                var dbDetailResult = (from dbDetail in dbServers.Elements("RapidrmsServers")
                                      from x in dbDetail.Elements("RapidrmsServer")
                                      select new
                                      {
                                          DbId = Convert.ToInt32(x.Attribute("id").Value),
                                          DS = x.Attribute("DS").Value,
                                          FN = x.Attribute("FN").Value,
                                          PN = x.Attribute("PN").Value
                                      }).ToList()[0];


                VerifoneLink = dbServerDetailResult.VL.Decript();
                VerifoneUserName = dbServerDetailResult.VU.Decript();
                VerifonePassword = dbServerDetailResult.VP.Decript();

                //VerifoneLibrary.DataAccess._CVerifone.DTN = dbServerDetailResult.ApplicationKey.Decript();
                VerifoneLibrary.DataAccess._CVerifone.DS = dbDetailResult.DS.Decript();
                VerifoneLibrary.DataAccess._CVerifone.FN = userName = dbDetailResult.FN.Decript();
                VerifoneLibrary.DataAccess._CVerifone.PN = password = dbDetailResult.PN.Decript();

                VerifoneLibrary.DataAccess._CVerifone.DTN = objCVerifone.GetDatabaseNameFromApplicationKey(dbServerDetailResult.ApplicationKey);
                objCVerifone.InsertActiveLog("Verifone", "End", "SetVarible()", "SetVarible Done", "SetVarible", "");
             }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "SetVarible()", "SetVarible  : " + ex.Message, "SetVarible", "");
            }
        }

        public void UpdateMasterData()
        {
            DataSet ds = new DataSet();
            try
            {


                ds = objCVerifone.GetVerifoneUpdatePendingData();

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    // Tax
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        objUpdate.UpdateVerifoneTax(ds.Tables[0]);
                    }
                    // Payment
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        objUpdate.UpdateVerifonePayment(ds.Tables[1]);
                    }
                    // Category
                    if (ds.Tables[2].Rows.Count > 0)
                    {
                        objUpdate.UpdateVerifoneCategory(ds.Tables[2]);
                    }
                    // ProdCode
                    //if (ds.Tables[3].Rows.Count > 0)
                    //{
                    //    objUpdate.Update_UpdateVerifoneProductCode(ds.Tables[3]);
                    //}

                    //department
                    if (ds.Tables[3].Rows.Count > 0)
                    {
                        objUpdate.UpdateVerifoneDepartment(ds.Tables[3], ds.Tables[4]);
                    }
                    //Fee
                    if (ds.Tables[5].Rows.Count > 0)
                    {
                        objUpdate.UpdateVerifoneFees(ds.Tables[5], ds.Tables[6]);
                    }
                    // Item
                    if (ds.Tables[7].Rows.Count > 0)
                    {
                        objUpdate.UpdateVerifoneItems(ds.Tables[7], ds.Tables[8], ds.Tables[9]);
                    }
                    if (ds.Tables[10].Rows.Count > 0)
                    {
                        // objUpdate.UpdateVerifoneFuel();
                        objUpdate.UpdateVerifoneFuel(ds.Tables[10], ds.Tables[11]);
                    }
                    // User
                    if (ds.Tables[12].Rows.Count > 0)
                    {
                        objUpdate.UpdateVerifoneUser(ds.Tables[12], ds.Tables[13]);
                    }
                    //Promotion Item
                    if (ds.Tables[14].Rows.Count > 0)
                    {
                        objUpdate.UpdateMixMatchItem(ds.Tables[14]);
                    }
                    // Promotion
                    if (ds.Tables[15].Rows.Count > 0)
                    {
                        objUpdate.UpdateMixMatch(ds.Tables[15], ds.Tables[16]);
                    }
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateMasterData()", "UpdateMasterData Exception : " + ex.Message, "UpdateMasterData", "");
            }
        }

        public void DeleteMasterData()
        {
            DataSet ds = new DataSet();
            try
            {
                ds = objCVerifone.GetVerifoneDeletePendingData();

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    // Tax
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        objDelete.DeleteVerifoneTax(ds.Tables[0]);
                    }
                    // Payment
                    if (ds.Tables[1] != null &&  ds.Tables[1].Rows.Count > 0)
                    {
                        objDelete.DeleteVerifonePayment(ds.Tables[1]);
                    }
                    // Category
                    if (ds.Tables[2] != null &&  ds.Tables[2].Rows.Count > 0)
                    {
                        objDelete.DeleteVerifoneCategory(ds.Tables[2]);
                    }
                    //department
                    if (ds.Tables[3] != null &&  ds.Tables[3].Rows.Count > 0)
                    {
                        objDelete.DeleteVerifoneDepartment(ds.Tables[3], ds.Tables[4]);
                    }
                    //Fee
                    //if (ds.Tables[5] != null &&  ds.Tables[5].Rows.Count > 0)
                    //{
                    //    objDelete.DeleteVerifoneFees(ds.Tables[5], ds.Tables[6]);
                    //}
                    // Item
                    if (ds.Tables[7] != null && ds.Tables[7].Rows.Count > 0)
                    {
                        objDelete.DeleteVerifoneItems(ds.Tables[7]);
                    }
                    //if (ds.Tables[10] != null &&  ds.Tables[10].Rows.Count > 0)
                    //{
                    //    // objUpdate.UpdateVerifoneFuel();
                    //    objDelete.DeleteVerifoneFuel(ds.Tables[10], ds.Tables[11]);
                    //}
                    //// User
                    //if (ds.Tables[12] != null &&  ds.Tables[12].Rows.Count > 0)
                    //{
                    //    objDelete.DeleteVerifoneUser(ds.Tables[12], ds.Tables[13]);
                    //}
                    //Promotion Item
                    if (ds.Tables[8] != null &&  ds.Tables[8].Rows.Count > 0)
                    {
                        objDelete.DeleteMixMatchItem(ds.Tables[8]);
                    }
                    // Promotion
                    if (ds.Tables[9] != null && ds.Tables[9].Rows.Count > 0)
                    {
                        objDelete.DeleteMixMatch(ds.Tables[9], ds.Tables[10]);
                    }
                }
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "UpdateMasterData()", "UpdateMasterData Exception : " + ex.Message, "UpdateMasterData", "");
            }
        }

        public void InsertMasterData()
        {
            try
            {
                objInsert.Employee();
                objInsert.UserWithRole();
                //if (UserPasswordExistsOrNot() == 0)
                //{
                //    objInsert.UpdateUserPassword();
                //}

                //objInsert.CashierUser();
                objInsert.Tax();
                objInsert.Payment();
                objInsert.Category();
                objInsert.ProductCode();
                objInsert.Department();
                objInsert.Fee();
                objInsert.Item();
                objInsert.Fuel();
                objInsert.Promotion();
                objInsert.Register();
            }
            catch (Exception ex)
            {
                objCVerifone.InsertActiveLog("Verifone", "Error", "InsertMasterData()", "InsertMasterData  : " + ex.Message, "InsertMasterData", "");
            }
        }

        private void WriteToFile(string text)
        {
            string path = ConfigurationManager.AppSettings["Apppath"];

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(text);
                writer.Close();
            }
        }

        public long UserPasswordExistsOrNot()
        {
            //_CVerifone objCVerifone = new _CVerifone();
            try
            {
                long Result = objCVerifone.UserPasswordExistsOrNot(VerifoneServices.VerifoneUserName);
                return Result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //public DataSet CheckUserExpireOrNot()
        //{
        //    _CVerifone objCVerifone = new _CVerifone();
        //    try
        //    {
        //        DataSet Result = objCVerifone.CheckUserExpireOrNot(VerifoneServices.VerifoneUserName);
        //        return Result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return 0;
        //    }
        //}
    }
}
