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
                    string currentAssyDevice = "";
                    GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId, out currentAssyDevice);

                    if (currentFlowId != null)                        
                    {
                        if (IsGDICDevice(currentAssyDevice))
                        {
                            if (currentFlowId != 142 && currentFlowId != 11 && currentFlowId != 266)
                            {
                                afterLotEndResult = AddFtInspSpecialFlow(e.McNo, e.LotNo, e.LotJudge, currentFlowId, lotId);
                            }
                            else
                            {
                                SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "Fail", "This lot already in job[" + currentFlowId.ToString() + "] : " + currentFlowName);
                            }
                        }
                        else
                        {
                            SaveLogFile(e.McNo, e.LotNo, "IsGDICDevice", "Fail", "Device : "+ currentAssyDevice + " not GDIC");
                        }                                          
                    }
                    else
                    {
                        SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "Fail", "Notfound current flow");
                    }                    
                }              
            }
        }
        return afterLotEndResult;
    }


    public AddSpecialFlowResult AddSpecialFlow(AddSpecialFlowEventArgs e)
    {
        AddSpecialFlowResult result = new AddSpecialFlowResult();
        result.HasError = false;
        int? lotId, currentFlowId;
        int transCurrentStepNo;
        string currentFlowName,currentState;
        DataTable currentTable = GetCurrentTranLot(e.LotNo);
        if (currentTable.Rows.Count > 0)
        {
            DataRow row = currentTable.Rows[0];
            currentState = row["ProcessState"].ToString().Trim();
            lotId = Convert.ToInt32(row["LotId"]);
            transCurrentStepNo = Convert.ToInt32(row["StepNo"]);
        }
        else
        {
            result.WarningMessage = "ไม่พบ LotNo : " + e.LotNo + " ในระบบ กรุณาตรวจสอบ";
            SaveLogFile("", e.LotNo, "AddSpecialFlow", "Warning", "ไม่พบข้อมูลในระบบ");
            return result;
        }
        if (currentState != "Wait")
        {
            result.HasError = true;
            result.ErrorMessage = "สถานะของ " + e.LotNo + " ต้องเป็น WIP เท่านั้น กรุณาตรวจสอบใน ATOM";
            SaveLogFile("", e.LotNo, "AddSpecialFlow", "Failed", "สถานะของ " + e.LotNo + " ต้องเป็น WIP เท่านั้น กรุณาตรวจสอบใน ATOM");
            return result;
        }
        DataTable dtTransLot = GetTransLotFlow(lotId);
        if (dtTransLot.Rows.Count > 0)
        {
            string beforeStepNo = "";
            int nextStepNo = 0;
            for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
            {
                DataRow row = dtTransLot.Rows[i];

                if (transCurrentStepNo == Convert.ToInt32(row["step_no"]))
                {
                    if (row["job_name"].ToString() == "100% INSP." || row["job_name"].ToString() == "SAMPLING INSP")
                    {
                        result.HasError = false;
                        result.WarningMessage = "สถานะของ " + e.LotNo + " อยู่ " + row["job_name"].ToString();
                        SaveLogFile("", e.LotNo, "AddSpecialFlow", "Failed", "This lot : " + e.LotNo + " already in " + row["job_name"].ToString());
                        return result;
                    }

                }

                //if (row["job_name"].ToString() == "AUTO(4)" && Convert.ToInt32(row["is_skipped"]) == 0 && currentStepNo == 0)
                //{
                //    currentStepNo = Convert.ToInt32(row["step_no"]);
                //    continue;
                //}
                //if (currentStepNo != 0 && Convert.ToInt32(row["is_skipped"]) == 0)
                //{
                //    nextStepNo = Convert.ToInt32(row["step_no"]);
                //    break;
                //}
            }
        }
        else
        {
            result.HasError = true;
            result.ErrorMessage = "ไม่พบรายละเอียดของ LotNo : " + e.LotNo + " ในระบบ กรุณาตรวจสอบ";
            SaveLogFile("", e.LotNo, "AddSpecialFlow", "Failed", "ไม่พบรายละเอียดของ LotNo : " + e.LotNo + " ในระบบ กรุณาตรวจสอบ");
            return result;
        }
        return result;
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
    
    private AfterLotEndResult AddFtInspSpecialFlow(string mcNo, string lotNo, string lotJudgement, int? currentFlowId, int? lotId)
    {
        AfterLotEndResult afterLotEndResult = new AfterLotEndResult();
        afterLotEndResult.HasError = false;
        if (lotJudgement == "INSPECTION")
        {
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
                SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(INSPECTION)", "FAIL", ex.Message);
                afterLotEndResult.HasError = true;
                afterLotEndResult.ErrorMessage = ex.Message;            
                //throw;
            }
        }
        else if (lotJudgement == "INSPECTION_ICMiss")
        {
            if (currentFlowId == null || lotId == null)
            {
                SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(INSPECTION_ICMiss)", "NOTHING", "FlowId and LotId == null");
                return afterLotEndResult;
            }
            ////108 = AUTO(2),110 = AUTO(3),119 = AUTO(4)
            //if (currentFlowId != 108 || currentFlowId != 110 || currentFlowId != 119)
            //{
            //    SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(INSPECTION_ICMiss)", "NOTHING","Job in : " + currentFlowId.ToString());
            //    return afterLotEndResult;
            //}
            try
            {
                DataTable dtTransLot = GetTransLotFlow(lotId);
                if (dtTransLot.Rows.Count > 0)
                {
                    int currentStepNo = 0;
                    int nextStepNo = 0;
                    for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
                    {
                        DataRow row = dtTransLot.Rows[i];
                        if (row["job_name"].ToString() == "AUTO(4)" && Convert.ToInt32(row["is_skipped"]) == 0 && currentStepNo == 0)
                        {
                            currentStepNo = Convert.ToInt32(row["step_no"]);
                            continue;
                        }
                        if (currentStepNo != 0 &&  Convert.ToInt32(row["is_skipped"]) == 0)
                        {
                            nextStepNo = Convert.ToInt32(row["step_no"]);
                            break;
                        }
                    }
                    if (currentStepNo == 0 || nextStepNo == 0)
                    {
                        SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(INSPECTION_ICMiss)", "FAIL", "CANNOT GET STEP_NO");
                        return afterLotEndResult;
                    }
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "[atom].[sp_set_trans_special_flow]";
                        cmd.Parameters.Add("@lot_id", System.Data.SqlDbType.Int).Value = lotId;
                        cmd.Parameters.Add("@step_no", System.Data.SqlDbType.Int).Value = currentStepNo;
                        cmd.Parameters.Add("@back_step_no", System.Data.SqlDbType.Int).Value = nextStepNo;
                        cmd.Parameters.Add("@user_id", System.Data.SqlDbType.Int).Value = 1; //Admin
                        cmd.Parameters.Add("@flow_pattern_id", System.Data.SqlDbType.Int).Value = 1267; //100%INSP
                        cmd.Parameters.Add("@is_special_flow", System.Data.SqlDbType.Int).Value = 2;
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                        SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow", "PASS", "STORE >> [atom].[sp_set_trans_special_flow]");
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(INSPECTION_ICMiss)", "FAIL", ex.Message);
                afterLotEndResult.HasError = true;
                afterLotEndResult.ErrorMessage = ex.Message;
                //throw;
            }
        }
        return afterLotEndResult;
    }
    private DataTable GetTransLotFlow(int? lotId)
    {
        DataTable ret = new DataTable();
        try
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[atom].[sp_get_trans_lot_flows]";
                cmd.Parameters.Add("@lot_id", System.Data.SqlDbType.Int).Value = lotId;
                cmd.Connection.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.HasRows)
                    {
                        ret.Load(rd);
                    }
                }
                cmd.Connection.Close();
            }
        }
        catch (Exception ex)
        {
            SaveLogFile("", lotId.ToString(), "GetTransLotFlow", "FAIL", ex.Message);
        }
        return ret;
    }
    private bool IsGDICDevice(string assyDevice)
    {
        bool ret = false;
        try
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "SELECT [device_name] FROM [DBxDW].[CAC].[DeviceGdic] WHERE [device_name] = @AssyDeviceName";
                cmd.Parameters.Add("@AssyDeviceName", System.Data.SqlDbType.VarChar).Value = assyDevice;
                cmd.Connection.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(rd);
                        if (dt.Rows.Count > 0)
                        {
                            ret = true;
                        }
                    }
                }
                cmd.Connection.Close();
            }
        }
        catch (Exception ex)
        {
            SaveLogFile("", assyDevice, "IsGDICDevice", "FAIL", ex.Message);
        }
        return ret;
    }
    private void GetCurrentTranLot(string lotNo, out int? lotId, out string currentFlowName,out int? currentFlowId,out string currentAssyDevice)
    {
        currentFlowName = "";
        currentAssyDevice = "";
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
                    currentAssyDevice = row["Assy_Name"].ToString();
                }
            }
            catch (Exception ex)
            {
                SaveLogFile("", lotNo, "GetCurrentTranLot", "FAIL", ex.Message);
            }
        }
        
    }
    private DataTable GetCurrentTranLot(string lotNo)
    {
        DataTable result = new DataTable();
        if (!string.IsNullOrEmpty(lotNo))
        {            
            try
            {
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
                            result.Load(rd);
                        }
                    }
                    cmd.Connection.Close();
                }
                SaveLogFile("", lotNo, "GetCurrentTranLot(lotNo)", "PASS", "");
            }
            catch (Exception ex)
            {
                SaveLogFile("", lotNo, "GetCurrentTranLot(lotNo)", "FAIL", ex.Message);
            }

        }
        return result;
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

    #region "SQL Server"
    private bool IsLotApcsPro(int? lotId)
    {
        bool result = false;
        try
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT [APCSProDB].[trans].[lots].[id],[lot_no],[APCSProDB].[method].[packages].[id] as packageId,[APCSProDB].[method].[packages].[name],[act_device_name_id],[device_slip_id] FROM [APCSProDB].[trans].[lots] inner join [APCSProDB].[method].[packages] on [APCSProDB].[trans].[lots].[act_package_id] = [APCSProDB].[method].[packages].[id] WHERE [APCSProDB].[trans].[lots].[id] = @lotId and [APCSProDB].[method].[packages].is_enabled = 1";
                cmd.Parameters.Add("@lotId", SqlDbType.Int).Value = lotId;
                cmd.Connection.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(rd);
                        if (dt.Rows.Count > 0)
                        {
                            result = true;
                        }
                    }
                }
                cmd.Connection.Close();
            }
        }
        catch (Exception ex)
        {
            SaveLogFile("", lotId.ToString(), "IsLotApcsPro", "Error", ex.Message);
            result = false;
        }
        SaveLogFile("", lotId.ToString(), "IsLotApcsPro", result.ToString(),"");
        return result;
    }
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
