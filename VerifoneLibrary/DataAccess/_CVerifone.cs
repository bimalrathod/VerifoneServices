using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using VerifoneLibrary.DataObject;
using System.Data.SqlClient;
using System.Data;
using Microsoft.ApplicationBlocks.Data;
//using VerifoneLibrary.BusinessObject;
using System.Configuration;
using System.IO;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer;
//using Microsoft.SqlServer.Management.Smo;
//using Microsoft.SqlServer.Management.Common;
using System.Threading;

using VerifoneLibrary.DataObject;


namespace VerifoneLibrary.DataAccess
{
    public class _CVerifone : IDataAccess, IDisposable
    {
        //private string ConnectionString = ConnString.GetConnectionString();
        public static string DS = "";
        public static string FN = "";
        public static string PN = "";
        public static string DTN = "";

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string connectionstring()
        {
            //var RConnString = ConfigurationManager.ConnectionStrings["RetailConnection"].ConnectionString;
            //RConnString = RConnString.Replace("DataSource", ConfigurationManager.AppSettings["DataSource"]);
            //RConnString = RConnString.Replace("Database", ConfigurationManager.AppSettings["Database"]);
            //RConnString = RConnString.Replace("UserId", ConfigurationManager.AppSettings["UserId"]);
            //RConnString = RConnString.Replace("SqlPassword", ConfigurationManager.AppSettings["SqlPassword"]);



            var RConnString = ConfigurationManager.ConnectionStrings["RetailConnection"].ConnectionString;
            RConnString = RConnString.Replace("DataSource", DS);
            RConnString = RConnString.Replace("Database", DTN);
            RConnString = RConnString.Replace("UserId", FN);
            RConnString = RConnString.Replace("SqlPassword", PN);

            return RConnString;
        }

        public string Rapidconnectionstring()
        {
            var RConnString = ConfigurationManager.ConnectionStrings["RetailConnection"].ConnectionString;
            RConnString = RConnString.Replace("DataSource", DS);
            RConnString = RConnString.Replace("Database", "RapidRMS");
            RConnString = RConnString.Replace("UserId", FN);
            RConnString = RConnString.Replace("SqlPassword", PN);

            return RConnString;
        }

        public long InsertTax(DataTable dt)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertTax",
                                         new SqlParameter("@VP_Tax", dt)
                                         );
                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Tax()", "InsertTax_Class Exception : " + ex, "Tax", "");
                return 0;
            }
        }

        public long InsertPLUs(DataTable dt)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "Proc_InsertPLUs",
                                         new SqlParameter("@tblPLUsData", dt),
                                         new SqlParameter("@BranchId", 1)
                                         );
                return Convert.ToInt64(returnVal);
            }
            catch (Exception) { return 0; }
        }



        public long InsertPayment(DataTable dt, DataTable dtCardTypeName)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertPayment",
                                         new SqlParameter("@VP_Payment", dt),
                                         new SqlParameter("@VP_PaymentCardTypeName", dtCardTypeName)
                                         );
                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Payment()", "InsertPayment_Class Exception : " + ex, "Payment", "");
                return 0;
            }
        }

        public long InsertCategory(DataTable dt)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertCategory",
                                    new SqlParameter("@VP_Category", dt));

                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Category()", "InsertCategory_Class Exception : " + ex, "Category", "");
                return 0;
            }
        }

        public long InsertprodCodes(DataTable dt)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertprodCodes",
                                    new SqlParameter("@VP_ProdCodes", dt));

                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Category()", "InsertCategory_Class Exception : " + ex, "Category", "");
                return 0;
            }
        }

        public long InsertDepartment(DataTable dt, DataTable dt1)
        {
            try
            {
                string conn = connectionstring();

                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertDepartment",
                                         new SqlParameter("@VP_Department", dt),
                                         new SqlParameter("@VP_DepartmentTax", dt1));
                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Department()", "InsertDepartment_Class Exception : " + ex, "Department", "");
                return 0;
            }
        }

        public long InsertFuel(DataTable dtFuelServices , DataTable dt, DataTable dt1)
        {
            try
            {
                string conn = connectionstring();

                object returnVal = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_InsertFuel",
                    new SqlParameter("@VP_FuelServices", dtFuelServices),                    
                    new SqlParameter("@VP_Fuel", dt),
                    new SqlParameter("@VP_FuelPrice", dt1));

                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Fuel()", "InsertFuel_Class Exception : " + ex, "Fuel", "");
                return 0;
            }
        }

        public long InsertFeeDeposit(DataTable dt, DataTable dt1)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertFeeDeposit",
                    new SqlParameter("@VP_FeeDeposit", dt),
                    new SqlParameter("@VP_FeeDepositCharges", dt1)
                    );

                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Fees()", "InsertFeeDeposit_Class Exception : " + ex, "Fees", "");
                return 0;
            }
        }

        public long InsertItems(DataTable dt, DataTable dtitemtax, DataTable dtitemFee)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_InsertItems",
                    new SqlParameter("@VP_Items", dt),
                    new SqlParameter("@VP_ItemsTax", dtitemtax),
                    new SqlParameter("@VP_ItemsFee", dtitemFee)
                    );

                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Items()", "InsertItems_Class Exception : " + ex, "Items", "");
                return 0;
            }
        }

        //public long InsertInvoice(DataTable dt, DataTable dt1, DataTable dtnosale, DataTable dtvoid, DataTable dtrefundvoid, DataTable dtsalesuspended,
        //                          DataTable dtsalesuspended_item, DataTable dtsalerecall, DataTable dtsalerecall_item)
        public long InsertInvoice(DataTable dtInvoice, DataTable dtInvoiceItem, DataTable dtnosale, DataTable dtvoid, DataTable dtsalesuspended,
                                  DataTable dtsalesuspended_item, DataTable dtsalerecall, DataTable dtsalerecall_item, DataTable dtjournal, DataTable dtTaxExempt,
                                  DataTable dtInvoiceDiscount, DataTable dtPayout, DataTable dtsalerecall_discount, DataTable dtLotteryInvoice, DataTable dtLotteryInvoiceItem)
        {
            try
            {
                string conn = connectionstring();
                //object returnVal
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_InsertInvoice",
                    new SqlParameter("@VP_Invoice", dtInvoice),
                    new SqlParameter("@VP_InvoiceItem", dtInvoiceItem),

                    new SqlParameter("@VP_Invoice_nosale", dtnosale),
                    new SqlParameter("@VP_Invoice_void", dtvoid),
                    new SqlParameter("@VP_Invoice_salesuspended", dtsalesuspended),
                    new SqlParameter("@VP_InvoiceItem_salesuspened", dtsalesuspended_item),
                    new SqlParameter("@VP_Invoice_salerecall", dtsalerecall),
                    new SqlParameter("@VP_InvoiceItem_salerecall", dtsalerecall_item),
                    new SqlParameter("@VP_Invoice_journal", dtjournal),
                    new SqlParameter("@VP_Invoice_TaxExempt", dtTaxExempt),
                    new SqlParameter("@VP_InvoiceDiscount", dtInvoiceDiscount),
                    new SqlParameter("@VP_InvoicePayout", dtPayout),
                    new SqlParameter("@VP_InvoiceDiscount_salerecall", dtsalerecall_discount),
                    new SqlParameter("@VP_InvoiceLottery", dtLotteryInvoice),
                    new SqlParameter("@VP_InvoiceLotteryItem", dtLotteryInvoiceItem)
                    );
                return 1;
                //return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Invoice()", "InsertInvoice Exception : " + ex, "Invoice", "");
                return 0;
            }
        }

        public DataSet GetRegisterno()
        {
            DataSet DS = null;
            try
            {

                string conn = connectionstring();
                DS = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_GetRegisterNoVerifone",
                                         new SqlParameter("@branchid", 1)
                                         );
                return DS;
            }
            catch (Exception)
            {
                return DS;
            }
        }

        public DataSet GetRegsiteridZZReport(long monthlyInvoiceBegin, long monthlyInvoiceEnd, long Month_RegisterId, DateTime OpnDate, DateTime CloseDate)
        {
            DataSet DS = null;
            try
            {

                string conn = connectionstring();
                DS = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "sp_GetRegisterIdMonthlyReport",
                                                    new SqlParameter("@RegisterId", Month_RegisterId),
                                                    new SqlParameter("@monthlyInvoiceBegin", monthlyInvoiceBegin),
                                                    new SqlParameter("@monthlyInvoiceEnd", monthlyInvoiceEnd),
                                                    new SqlParameter("@OpnDate", @OpnDate),
                                                    new SqlParameter("@CloseDate", @CloseDate));
                return DS;
            }
            catch (Exception)
            {
                return DS;
            }
        }


        public DataSet GetDataDepthorwItem(long DepID)
        {
            DataSet DS = null;
            try
            {

                string conn = connectionstring();
                DS = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_GetDeptIdthrowItem",
                                         new SqlParameter("@DeptId", DepID)
                                         );
                return DS;
            }
            catch (Exception)
            {
                return DS;
            }
        }

        public DataSet GetInvoiceNo()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_GetInvoiceNo");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet Getdropamountid()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_Getdropamountid");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public DataSet GetDiscountID()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_GetDiscountID");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetShiftID()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_GetShiftID");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void InsertActiveLog(string Action, string Status, string Method, string Description, string Command, string RequestCommand)
        {
            try
            {
                string conn = connectionstring();
                SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertActiveLog",
                    new SqlParameter("@Action", Action),
                    new SqlParameter("@Status", Status),
                    new SqlParameter("@Method", Method),
                    new SqlParameter("@Description", Description),
                    new SqlParameter("@Command", Command),
                    new SqlParameter("@RequestCommand", RequestCommand)
                    );
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetVerifoneTaxData()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetTax");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetVerifonePaymentData()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetPayment");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetVerifoneCategoryData()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetCategory");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetVerifoneDepartmentData()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetDepartment");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetVerifoneFeesData()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetFees");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetVerifoneItemData()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetItems");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetVerifoneFuelData()
        {
            try
            {
                string conn = connectionstring();
                DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetFuel");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public long DeleteVerifoneHistory(string Command, string arraysysid)
        {
            try
            {
                string conn = connectionstring();
                object result = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_DeleteHistory",
                    new SqlParameter("@Command", Command),
                    new SqlParameter("@arraysysid", arraysysid)
                    );

                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "DeleteVerifoneHistory()", "DeleteVerifoneHistory_Class Exception : " + ex, "DeleteVerifoneHistory", "");
                return 0;
            }
        }

        public DataSet GetVerifoneUpdatePendingData()
        {
            DataSet ds = new DataSet();
            string conn = connectionstring();
            try
            {
                ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetUpdatePendingData");
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public long InsertZ(DataTable dt)
        //public long InsertZ(DateTime OpenDate, DateTime CloseDate, decimal OpenAmount, decimal CloseAmount, Int64 InvoiceBegin, Int64 InvoiceEnd, Int64 BrnZId, Int64 RegisterId)
        // public long InsertZ(DateTime OpenDate, DateTime CloseDate, decimal OpenAmount, decimal CloseAmount, DataTable dtZReport, Int64 BrnZId, Int64 RegisterId)
        public long InsertZ(DateTime OpenDate, DateTime CloseDate, DataTable dtZReport)
        {
            try
            {
                string conn = connectionstring();
                object result = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertZ",
                    //new SqlParameter("@VP_Z", dt)
                    new SqlParameter("@OpenDate", OpenDate),
                    new SqlParameter("@CloseDate", CloseDate),
                    // new SqlParameter("@OpenAmount", OpenAmount),
                    //new SqlParameter("@CloseAmount", CloseAmount),
                    //new SqlParameter("@InvoiceBegin", InvoiceBegin),
                    //new SqlParameter("@InvoiceEnd", InvoiceEnd),
                    new SqlParameter("@VP_Z", dtZReport)
                    //new SqlParameter("@BrnZId", BrnZId)

                    );

                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertZ()", "InsertZ_Class Exception : " + ex, "Z", "");
                return 0;
            }
        }



        #region commented
        //public long GetVerifoneReportData(long Id, long PeriodSeqNum)
        //{
        //    try
        //    {
        //        string conn = connectionstring();
        //        object result = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_GetReportData",
        //            new SqlParameter("@Id", Id),
        //            new SqlParameter("@PeriodSeqNum", PeriodSeqNum)
        //            );

        //        return Convert.ToInt64(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return 0;
        //    }
        //}
        #endregion

        public DataSet GetShiftDailyMonthYearData()
        {
            DataSet ds = new DataSet();
            try
            {
                string conn = connectionstring();
                ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetShiftDailyMonthYearData");

                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet GetZReportDetail_28092016(Int64 BranchId, Int64 RegisterId, Int64 UserId, Decimal Amount, Int64 ZId, DateTime Datetime)
        {
            try
            {
                string conn = connectionstring();

                DataSet oDataSet = SqlHelper.ExecuteDataset(conn, System.Data.CommandType.StoredProcedure, "proc_ZReport_12122014_Tips28092016",
                  new SqlParameter("@BranchId", BranchId),
                  new SqlParameter("@RegisterId", RegisterId),
                  new SqlParameter("@UserId", UserId),
                  new SqlParameter("@ZClsAmount", Amount),
                  new SqlParameter("@ZId", ZId),
                  new SqlParameter("@Datetime", Datetime)
                  );

                return oDataSet;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public long InsertShift(Int64 BrnCashInOutId, decimal CashInAmt, decimal CashOutAmt, DateTime OpnDate, DateTime ClsDate, Int64 InvoiceId, Int64 Shift_RegisterId)
        {
            try
            {
                string conn = connectionstring();
                object result = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertShift",
                    new SqlParameter("@BrnCashInOutId", BrnCashInOutId),
                    new SqlParameter("@CashInAmt", CashInAmt),
                    new SqlParameter("@CashOutAmt", CashOutAmt),
                    new SqlParameter("@OpnDate", OpnDate),
                    new SqlParameter("@ClsDate", ClsDate),
                    new SqlParameter("@InvoiceId", InvoiceId),
                    new SqlParameter("@RegisterId", Shift_RegisterId)
                    );

                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertShift()", "InsertShift_Class Exception : " + ex, "Shift", "");
                return 0;
            }
        }


        public long InsertShift(Int64 BrnCashInOutId, decimal CashInAmt, decimal CashOutAmt, DateTime OpnDate, DateTime ClsDate, string InvoiceBegin, string InvoiceEnd)
        {
            try
            {
                string conn = connectionstring();
                object result = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertShift",
                    new SqlParameter("@BrnCashInOutId", BrnCashInOutId),
                    new SqlParameter("@CashInAmt", CashInAmt),
                    new SqlParameter("@CashOutAmt", CashOutAmt),
                    new SqlParameter("@OpnDate", OpnDate),
                    new SqlParameter("@ClsDate", ClsDate),
                    new SqlParameter("@InvoiceBegin", InvoiceBegin),
                    new SqlParameter("@InvoiceEnd", InvoiceEnd)
                    );

                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertShift()", "InsertShift_Class Exception : " + ex, "Shift", "");
                return 0;
            }
        }

        public long Insert_ZZ(Int64 ZZId, DateTime OpnDate, DateTime ZZDate, Int64 BatchNo, Int64 Month_RegisterId, long monthlyInvoiceBegin, long monthlyInvoiceEnd)
        {
            try
            {
                string conn = connectionstring();
                object result = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_Insert_ZZ",
                    new SqlParameter("@ZZId", ZZId),
                    new SqlParameter("@OpnDate", OpnDate),
                    new SqlParameter("@ZZDate", ZZDate),
                    new SqlParameter("@BatchNo", BatchNo),
                    new SqlParameter("@RegisterId", Month_RegisterId)

                    );

                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Insert_ZZ()", "Insert_ZZ_Class Exception : " + ex, "ZZ", "");
                return 0;
            }
        }


        public long InsertEmployee(DataTable dt)
        {
            try
            {
                string conn = connectionstring();

                object returnval = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertEmployee",
                    new SqlParameter("@VerifoneTblEmployee", dt)
                    );

                return Convert.ToInt64(returnval);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertEmployee()", "InsertEmployeer_Class Exception : " + ex, "Employee", "");
                return 0;
            }
        }



        public long InsertUserFunctionRole(DataTable dtFunction, DataTable dtRole, DataTable dtRoleFunction, DataTable dtUser, DataTable dtUserRole)
        {
            try
            {
                string conn = connectionstring();

                object returnval = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertUserFunctionRole",
                    new SqlParameter("@dtFunction", dtFunction),
                    new SqlParameter("@dtRole", dtRole),
                    new SqlParameter("@dtRoleFunction", dtRoleFunction),
                    new SqlParameter("@dtUser", dtUser),
                    new SqlParameter("@dtUserRole", dtUserRole)
                    );

                return Convert.ToInt64(returnval);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertUserFunctionRole()", "InsertUserFunctionRole_Class Exception : " + ex, "UserRole", "");
                return 0;
            }
        }



        public long InsertCashierUser(DataTable dt)
        {
            try
            {
                string conn = connectionstring();

                object returnval = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertCashierUser",
                    new SqlParameter("@VP_CashierUser", dt)
                    );

                return Convert.ToInt64(returnval);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertCashierUser()", "InsertCashierUser_Class Exception : " + ex, "User", "");
                return 0;
            }
        }

        public long InsertPromotion(DataTable dt_MixMatch, DataTable dt_ItemList, DataTable dt_Tax)
        {
            //DataSet result = new DataSet();
            try
            {
                string conn = connectionstring();
                object result = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertPromotion",
                    new SqlParameter("@VP_MixMatch", dt_MixMatch),
                    new SqlParameter("@VP_ItemList", dt_ItemList),
                    new SqlParameter("@VP_Tax", dt_Tax));

                //result = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_InsertPromotion",
                //  new SqlParameter("@VP_MixMatch", dt_MixMatch),
                //  new SqlParameter("@VP_ItemList", dt_ItemList),
                //  new SqlParameter("@VP_Tax", dt_Tax));
                //return result; 
                return Convert.ToInt16(result);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertPromotion()", "InsertPromotion_Class Exception : " + ex, "Promotion", "");
                //return result;
                return 0;
            }
        }

        public long InsertRegister(DataTable dt)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_InsertRegister",
                                         new SqlParameter("@VP_Register", dt)
                                         );
                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "Register()", "InsertRegister_Class Exception : " + ex, "Register", "");
                return 0;
            }
        }

        #region Different Types of Invoices
        public long InsertInvoice_Nosale(DataTable dtnosale)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Nosale",
                                   new SqlParameter("@VP_Invoice_nosale", dtnosale));

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Nosale()", "InsertInvoice_Nosale Exception : " + ex.Message, "Nosale", "");
                return 0;
            }
        }

        public long InsertInvoice_Journal(DataTable dtJournal)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Journal",
                                   new SqlParameter("@VP_Invoice_journal", dtJournal));

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Journal()", "InsertInvoice_Journal Exception : " + ex.Message, "Journal", "");
                return 0;
            }
        }

        public long InsertInvoice_Void(DataTable dtVoid)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Void",
                                   new SqlParameter("@VP_Invoice_void", dtVoid));

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Void()", "InsertInvoice_Void Exception : " + ex.Message, "Void", "");
                return 0;
            }
        }

        public long InsertInvoice_Suspended(DataTable dtSaleSuspened, DataTable dtSalesSuspenedItem)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Suspended",
                                   new SqlParameter("@VP_Invoice_salesuspended", dtSaleSuspened),
                                   new SqlParameter("@VP_InvoiceItem_salesuspened", dtSalesSuspenedItem)
                                  );

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Suspended()", "InsertInvoice_Suspended Exception : " + ex.Message, "Suspended", "");
                return 0;
            }
        }

        public long InsertInvoice_Recall(DataTable dtSaleRecall, DataTable dtSaleRecallItem, DataTable dtSaleRecall_InvoiceDiscount, DataTable dtSaleRecallPayment)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Recall",
                                   new SqlParameter("@VP_Invoice_salerecall", dtSaleRecall),
                                   new SqlParameter("@VP_InvoiceItem_salerecall", dtSaleRecallItem),
                                   new SqlParameter("@VP_InvoiceDiscount_salerecall", dtSaleRecall_InvoiceDiscount),
                                   new SqlParameter("@VP_InvoicePayment_salerecall", dtSaleRecallPayment));

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Recall()", "InsertInvoice_Recall Exception : " + ex.Message, "Recall", "");
                return 0;
            }
        }

        public long InsertInvoice_Payout(DataTable dtPayout)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Payout",
                                    new SqlParameter("@VP_InvoicePayout", dtPayout));

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Payout()", "InsertInvoice_Payout Exception : " + ex.Message, "Payout", "");
                return 0;
            }
        }


        public long InsertInvoice_Lottery(DataTable dtLotteryInvoice, DataTable dtLotteryInvoiceItem, DataTable dtInvoicePayment)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Lottery",
                                    new SqlParameter("@VP_InvoiceLottery", dtLotteryInvoice),
                                    new SqlParameter("@VP_InvoiceLotteryItem", dtLotteryInvoiceItem),
                                    new SqlParameter("@VP_InvoicePayment", dtInvoicePayment)
                                    );

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Lottery()", "InsertInvoice_Lottery Exception : " + ex.Message, "Lottery", "");
                return 0;
            }
        }

        public long InsertInvoice_Sale(DataTable dtInvoice, DataTable dtInvoiceItem, DataTable dtInvoiceDiscount, DataTable dtTaxExempt, DataTable dtInvoicePayment)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Sale",
                                    new SqlParameter("@VP_Invoice", dtInvoice),
                                    new SqlParameter("@VP_InvoiceItem", dtInvoiceItem),
                                    new SqlParameter("@VP_InvoiceDiscount", dtInvoiceDiscount),
                                    new SqlParameter("@VP_InvoicePayment", dtInvoicePayment),
                                    new SqlParameter("@VP_Invoice_TaxExempt", dtTaxExempt)
                                    );

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Sale()", "InsertInvoice_Sale Exception : " + ex.Message, "Sale", "");
                return 0;
            }
        }

        public long InsertInvoice_Fuel(DataTable dtInvoicePump, DataTable dtPumpCart)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Invoice_Fuel",
                                    new SqlParameter("@VP_InvoicePump", dtInvoicePump),
                                    new SqlParameter("@VP_PumpCart", dtPumpCart)
                                    );
                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Fuel()", "InsertInvoice_Fuel Exception : " + ex.Message, "Fuel", "");
                return 0;
            }
        }
        #endregion


        public long InsertDropamount(DataTable dtDropamount)
        {
            try
            {
                string conn = connectionstring();
                long Result = SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "VP_Insertdropamount",
                                    new SqlParameter("@VP_Dropamount", dtDropamount));

                return Result;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "InsertInvoice_Payout()", "InsertInvoice_Payout Exception : " + ex.Message, "Payout", "");
                return 0;
            }
        }



        public DataSet GetDeptItemData(long DepID)
        {
            DataSet DS = null;
            try
            {

                string conn = connectionstring();
                DS = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_GetDeptDataVerifone",
                                                    new SqlParameter("@DeptId", DepID));
                return DS;
            }
            catch (Exception)
            {
                return DS;
            }
        }

        public DataSet GetGasDATA()
        {
            DataSet DS = null;
            try
            {

                string conn = connectionstring();
                DS = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SP_GetGasItemDataVerifone");
                return DS;
            }
            catch (Exception)
            {
                return DS;
            }
        }

        public DataSet GetItemData()
        {
            DataSet ds = new DataSet();
            try
            {
                string conn = connectionstring();
                ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetItemData");
                return ds;
            }
            catch (Exception ex)
            {
                return ds;
            }
        }

        #region get databasename from Application key
        public string GetDatabaseNameFromApplicationKey(string Key)
        {
            try
            {
                string conn = Rapidconnectionstring();
                object returnvalue = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_GetDatabaseNameFromApplicationKey",
                   new SqlParameter("@Key", Key));

                return Convert.ToString(returnvalue);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        public long UpdateUserPassword(string VerifoneLink, string VerifoneUserName, string VerifonePassword)
        {
            try
            {
                string conn = connectionstring();
                object returnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_UpdateUserPassword",
                    new SqlParameter("@VerifoneLink", VerifoneLink),
                    new SqlParameter("@VerifoneUserName", VerifoneUserName),
                    new SqlParameter("@VerifonePassword", VerifonePassword)
                    );

                return Convert.ToInt64(returnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "UpdateUserPassword()", "UpdateUserPassword_Class Exception : " + ex, "UpdateUserPassword", "");
                return 0;
            }
        }

        public long UserPasswordExistsOrNot(string VerifoneUserName)
        {
            try
            {
                string conn = connectionstring();
                object ReturnVal = SqlHelper.ExecuteScalar(conn, CommandType.StoredProcedure, "VP_UserPasswordExistsOrNot",
                    new SqlParameter("@VerifoneUserName", VerifoneUserName));

                return Convert.ToInt64(ReturnVal);
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "UserPasswordExistsOrNot()", "UserPasswordExistsOrNot_Class Exception : " + ex, "UserPasswordExistsOrNot", "");
                return 0;
            }
        }

        public DataSet CheckUserExpireOrNot(string VerifoneUserName)
        {
            DataSet DS = new DataSet();
            try
            {
                string conn = connectionstring();
                DS = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_CheckUserExpireOrNot",
                    new SqlParameter("@VerifoneUserName", VerifoneUserName));

                return DS;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "CheckUserExpireOrNot()", "CheckUserExpireOrNot_Class Exception : " + ex, "CheckUserExpireOrNot", "");
                return DS;
            }
        }

        public DataSet GetVerifoneUserPwdPendingData(string VerifoneUserName)
        {
            DataSet ds = new DataSet();
            string conn = connectionstring();
            try
            {
                ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "VP_GetVerifoneUserPwdPendingData",
                    new SqlParameter("@VerifoneUserName", VerifoneUserName));
                return ds;
            }
            catch (Exception ex)
            {
                InsertActiveLog("BoF", "Error", "GetVerifoneUserPwdPendingData()", "GetVerifoneUserPwdPendingData_Class Exception : " + ex, "GetVerifoneUserPwdPendingData", "");
                return ds;
            }
        }
    }
}
