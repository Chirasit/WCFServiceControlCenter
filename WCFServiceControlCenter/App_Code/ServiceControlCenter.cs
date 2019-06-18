using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Configuration;
using System.Data;
using System.IO;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ServiceControlCenter" in code, svc and config file together.
public class ServiceControlCenter : IServiceControlCenter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public AfterLotEndResult AfterLotEnd(AfterLotEndEventArgs e)
    {
        AfterLotEndResult afterLotEndResult = new AfterLotEndResult();
        afterLotEndResult.HasError = false;
        afterLotEndResult.WarningMessage = "";
        SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "NULL", "NULL");
        if (e != null)
        {
            if (e.JobSpecialFlowId != null && !string.IsNullOrEmpty(e.LotNo) && !string.IsNullOrEmpty(e.McNo))
            {
                if (e.McNo.Substring(0,2) == "FT")
                {
                    int? lotId = null;
                    int? currentFlowId = null;
                    string currentFlowName = "";
                    GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId);
                    if (currentFlowId != null && currentFlowId != 142)
                    {

                        afterLotEndResult = AddFtInspSpecialFlow(e.McNo, e.LotNo);
                    }
                    else
                    {
                        SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "Fail", "This lot already in job : " + currentFlowName);
                    }                    
                }              
            }
        }
        return afterLotEndResult;
    }
    
    //private void LotJudgementInspection(string mcNo, string lotNo)
    //{
    //    //string currentFlowName = GetCurrentFlowName(lotNo);
    //    int? lotId = null;
    //    string currentFlowName = "";
    //    if (GetCurrentFlowName(lotNo,out lotId,out currentFlowName))
    //    {
    //        GetPreviousFlow()
    //        switch (flowName)
    //        {

    //            default:
    //                break;
    //        }
    //    }

    //    AddFtInspSpecialFlow(lotNo);
    //}
    #region "Store procedure"
    private AfterLotEndResult AddFtInspSpecialFlow(string mcNo, string lotNo)
    {
        AfterLotEndResult afterLotEndResult = new AfterLotEndResult();
        afterLotEndResult.HasError = false;
        try
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[atom].[sp_set_trans_special_flow_temp]";
                cmd.Parameters.Add("@lot_no", System.Data.SqlDbType.VarChar).Value = lotNo;
                cmd.Connection.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(rd);
                        foreach (DataRow row in dt.Rows)
                        {
                            SaveLogFile(mcNo, lotNo, "Add SpecialFlow : 142", "PASS", "is_special_flow = " + row[0].ToString()); 
                        }
                    }
                } 
                cmd.Connection.Close();
                SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow", "PASS", "STORE >> [atom].[sp_set_trans_special_flow_temp]");
            }
        }
        catch (Exception ex)
        {
            SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow", "FAIL", ex.Message);
            afterLotEndResult.HasError = true;
            afterLotEndResult.ErrorMessage = ex.Message;            
            //throw;
        }
        return afterLotEndResult;
    }
    private void GetCurrentTranLot(string lotNo, out int? lotId, out string currentFlowName,out int? currentFlowId)
    {
        currentFlowName = "";
        currentFlowId = null;
        lotId = null;
        if (!string.IsNullOrEmpty(lotNo))
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[cellcon].[sp_get_current_trans_lots]";
                    cmd.Parameters.Add("@lot_no", System.Data.SqlDbType.VarChar).Value = lotNo;
                    cmd.Connection.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.HasRows)
                        {
                            dt.Load(rd);
                        }
                    }
                    cmd.Connection.Close();
                }
                foreach (DataRow row in dt.Rows)
                {
                    currentFlowName = row["FlowName"].ToString();
                    lotId = Convert.ToInt32(row["LotId"]);
                    currentFlowId = Convert.ToInt32(row["FlowId"].ToString());
                }
            }
            catch (Exception ex)
            {
                SaveLogFile("", lotNo, "GetCurrentTranLot", "FAIL", ex.Message);
            }
        }
        
    }
    //private bool GetPreviousFlow(int lotId, string currentFlow, out string preViousFlow)
    //{
    //    bool ret = false;
    //    try
    //    {
    //        DataTable dt = new DataTable();
    //        using (SqlCommand cmd = new SqlCommand())
    //        {
    //            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
    //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
    //            cmd.CommandText = "[atom].[sp_get_trans_lot_flows]";
    //            cmd.Parameters.Add("@lot_id", System.Data.SqlDbType.Int).Value = lotId;
    //            cmd.Connection.Open();
    //            using (SqlDataReader rd = cmd.ExecuteReader())
    //            {
    //                if (rd.HasRows)
    //                {
    //                    dt.Load(rd);
    //                }
    //            }
    //            cmd.Connection.Close();
    //        }
    //        //string job_name = "";
    //        foreach (DataRow row in dt.Rows)
    //        {
    //            if (Convert.ToInt32( row["is_skipped"]) == 0)
    //            {

    //            }
    //            //ret = row["FlowName"].ToString();
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        return ret;
    //        //throw;
    //    }
    //    return ret;
    //}
    #endregion

    //public void AddSpecialFlow(int lotId)
    //{
    //    throw new NotImplementedException();
    //}
    private void SaveLogFile(string mcNo, string lotNo, string title, string result, string message)
    {
        try
        {
            string strFolderPath = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log");
            string strFolderPathBackup = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup");
            if (!System.IO.Directory.Exists(strFolderPath))
            {
                System.IO.Directory.CreateDirectory(strFolderPath);
                System.IO.Directory.CreateDirectory(strFolderPathBackup);
            }
            string strPath = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\dataLog.csv");
            //string backupPath = @"~\\Log\Backup\dataLog" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + @".log";
            string strPathBackup = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup\dataLog" + DateTime.Now.ToString("yyyyMMddHHmm") + @".csv");
            FileInfo fi = new FileInfo(strPath);
            if (fi.Exists && fi.Length > 2097152)
            {
                //File.Move(strPath, strPathBackup);
                File.Delete(strPath);
            }
            using (StreamWriter sw = new StreamWriter(strPath, true))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + mcNo + "," + lotNo + "," + title + "," + message + "," + result);
            }
        }
        catch (Exception ex)
        {

            //throw;
        }
       
    }
    public void DoWork()
    {
    }

}
