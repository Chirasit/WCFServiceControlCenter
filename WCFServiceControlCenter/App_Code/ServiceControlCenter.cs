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
    #region "JigControl"
    public ItemCheckingResult JigToolChecking(ItemCheckingEventArgs e)
    {
        //JigControl jigControl = new JigControlWB();
        //string str = jigControl.Test();
        //ItemCheckingResult result = new ItemCheckingResult { HasError = false, WarningMessage = str };
        //return result;
        return null;
        string jigType = ""; //Get type from store procedure
        JigControl jigControl = null;
        switch (jigType)
        {
            case "Kanagata":
                jigControl = new JigControlKanagata();
                break;
            case "Preform":
                break;
            case "Frame":
                break;
            case "HP":
                break;
            case "PP":
                break;
            case "Cappillary":
                break;
            default:
                break;
        }
        jigControl.JigControlChecking(e);

    }
    #endregion
    #region "SpecialFlow"
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
            switch (e.LotJudge)
            {
                case "FT_BIN19_AUTO3":
                    if (!string.IsNullOrEmpty(e.LotNo) && !string.IsNullOrEmpty(e.McNo))
                    {
                        int? lotId = null;
                        int? currentFlowId = null;
                        string currentFlowName = "";
                        string currentAssyDevice = "";
                        DataTable currentDataTable = GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId, out currentAssyDevice);
                        if (IsGDICDevice(currentAssyDevice))
                        {
                            //3:Hold
                            //afterLotEndResult = SetQualityState(3, e.LotNo);
                            // Flow pattern = 1500
                            DataRow row = currentDataTable.Rows[0];
                            int currentStepNo = (int)row["StepNo"];
                            afterLotEndResult = AddFtInspSpecialFlow(e.McNo, e.LotNo, e.LotJudge, currentFlowId, lotId, currentStepNo);
                        }
                        else
                        {
                            SaveLogFile(e.McNo, e.LotNo, e.LotJudge, "Fail", "Device : " + currentAssyDevice + " not GDIC");
                        }
                    }
                    break;
                case "LOW YIELD": //1501
                case "IC BURN": //1502
                    if (!string.IsNullOrEmpty(e.LotNo) && !string.IsNullOrEmpty(e.McNo))
                    {
                        int? lotId = null;
                        int? currentFlowId = null;
                        string currentFlowName = "";
                        string currentAssyDevice = "";
                        DataTable currentDataTable = GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId, out currentAssyDevice);
                        //3:Hold
                        //afterLotEndResult = SetQualityState(3, e.LotNo);
                        // Flow pattern = 1500
                        DataRow row = currentDataTable.Rows[0];
                        int currentStepNo = (int)row["StepNo"];
                        afterLotEndResult = AddFtInspSpecialFlow(e.McNo, e.LotNo, e.LotJudge, currentFlowId, lotId, currentStepNo);
                    }
                    break;
                case "ASIMode": //1666
                    if (!string.IsNullOrEmpty(e.LotNo) && !string.IsNullOrEmpty(e.McNo))
                    {
                        int? lotId = null;
                        int? currentFlowId = null;
                        string currentFlowName = "";
                        string currentAssyDevice = "";
                        DataTable currentDataTable = GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId, out currentAssyDevice);
                        //3:Hold
                        //afterLotEndResult = SetQualityState(3, e.LotNo);
                        // Flow pattern = 1500
                        DataRow row = currentDataTable.Rows[0];
                        if (currentFlowName.Trim() == "AUTO(2)ASISAMPLE" || currentFlowName.Trim() == "AUTO(3)ASISAMPLE")
                        {
                            SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "Fail", "This lot already in job[" + currentFlowId.ToString() + "] : " + currentFlowName);
                        }
                        else
                        {
                            int currentStepNo = (int)row["StepNo"];
                            afterLotEndResult = AddFtInspSpecialFlow(e.McNo, e.LotNo, e.LotJudge, currentFlowId, lotId, currentStepNo);
                        }
                    }
                    break;
                case "INSP_ICBURN":
                case "INSP_LowYield":
                    if (!string.IsNullOrEmpty(e.LotNo) && !string.IsNullOrEmpty(e.McNo))
                    {
                        int? lotId = null;
                        int? currentFlowId = null;
                        string currentFlowName = "";
                        string currentAssyDevice = "";
                        DataTable currentDataTable = GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId, out currentAssyDevice);

                        DataRow row = currentDataTable.Rows[0];
                        int currentStepNo = (int)row["StepNo"];
                        afterLotEndResult = AddFtInspSpecialFlow(e.McNo, e.LotNo, e.LotJudge, currentFlowId, lotId, currentStepNo);
                    }
                    break;
                default:
                    if (e.JobSpecialFlowId != null && !string.IsNullOrEmpty(e.LotNo) && !string.IsNullOrEmpty(e.McNo))
                    {
                        if (e.McNo.Substring(0, 2) == "FT")
                        {
                            int? lotId = null;
                            int? currentFlowId = null;
                            string currentFlowName = "";
                            string currentAssyDevice = "";
                            DataTable currentDataTable = GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId, out currentAssyDevice);

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
                                    SaveLogFile(e.McNo, e.LotNo, "IsGDICDevice", "Fail", "Device : " + currentAssyDevice + " not GDIC");
                                }
                            }
                            else
                            {
                                SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "Fail", "Notfound current flow");
                            }
                        }
                    }
                    break;
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
    #endregion
    #region "Store procedure"
    private AfterLotEndResult SetQualityState(int stateNo, string lotNo)
    {
        AfterLotEndResult afterLotEndResult = new AfterLotEndResult();
        afterLotEndResult.HasError = false;
        try
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                //cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "[atom].[sp_set_quality_state]";
                //cmd.Parameters.Add("@lot_no", SqlDbType.VarChar).Value = lotNo;
                //cmd.Parameters.Add("@quality_state", SqlDbType.Int).Value = stateNo;
                //cmd.Connection.Open();
                //cmd.ExecuteNonQuery();
                //cmd.Connection.Close();
                //SaveLogFile("", lotNo, "SetQualityState", "Pass", "");
            }  
        }
        catch (Exception ex)
        {
            afterLotEndResult.HasError = true;
            afterLotEndResult.ErrorMessage = ex.Message;
            SaveLogFile("", lotNo, "SetQualityState", "Fail", ex.Message);

        }
        return afterLotEndResult;
    }
    private AfterLotEndResult AddFtInspSpecialFlow(string mcNo, string lotNo, string lotJudgement, int? currentFlowId, int? lotId, int nextStepNo = 0)
    {
        AfterLotEndResult afterLotEndResult = new AfterLotEndResult();
        afterLotEndResult.HasError = false;
        int flowPattern = 0;
        switch (lotJudgement)
        {
            case "INSPECTION":
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
                return afterLotEndResult;                
            case "INSPECTION_ICMiss":
                flowPattern = 1267;
                break;
            case "FT_BIN19_AUTO3":
                flowPattern = 1500;
                break;
            case "LOW YIELD":
                flowPattern = 1501;
                break;
            case "IC BURN":
                flowPattern = 1502;
                break;
            case "ASIMode":
                switch (currentFlowId)
                {
                    case 108: //Auto2
                        flowPattern = 1194; //Add retest Auto1
                        break;
                    case 110: //Auto3
                        flowPattern = 1195; //Add retest Auto2
                        break;
                    case 119: //Auto4
                        flowPattern = 1196; //Add retest Auto3
                        break;
                    default:
                        flowPattern = 1666; //Add flow??                        
                        break;
                }
                break;
            case "INSP_LowYield":
                if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
                {
                    flowPattern = 1501;
                }
                else
                {
                    flowPattern = 1677;
                }                
                break;
            case "INSP_ICBURN":
                if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
                {
                    flowPattern = 1502;
                }
                else
                {
                    flowPattern = 1676;
                }
                break;

        }
        if (currentFlowId == null || lotId == null)
        {
            SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "NOTHING", "FlowId and LotId == null");
            return afterLotEndResult;
        }
        if (flowPattern == 0)
        {
            SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "NOTHING", "FlowPattern == null");
            return afterLotEndResult;
        }
        
        int currentStepNo = 0;
        bool isNow = true;
        if (flowPattern == 1267)
        {
            try
            {
                DataTable dtTransLot = GetTransLotFlow(lotId);
                if (dtTransLot.Rows.Count > 0)
                {
                    string lastFtAuto = "AUTO(4)";
                    //Get last AUTO
                    foreach (DataRow row1 in dtTransLot.Rows)
                    {
                        if (row1["job_name"].ToString() == "AUTO(5)")
                        {
                            lastFtAuto = "AUTO(5)";
                        }
                    }

                    for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
                    {
                        DataRow row = dtTransLot.Rows[i];
                        if (row["job_name"].ToString() == lastFtAuto && Convert.ToInt32(row["is_skipped"]) == 0 && currentStepNo == 0)
                        {
                            currentStepNo = Convert.ToInt32(row["step_no"]);
                            continue;
                        }
                        if (currentStepNo != 0 && Convert.ToInt32(row["is_skipped"]) == 0)
                        {
                            nextStepNo = Convert.ToInt32(row["step_no"]);
                            break;
                        }
                    }
                    if (currentStepNo == 0 || nextStepNo == 0)
                    {
                        SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "FAIL", "CANNOT GET STEP_NO");
                        return afterLotEndResult;
                    }
                    
                    if (flowPattern == 1267)
                    {
                        DataTable dtCurrentLot = GetCurrentTranLot(lotNo);
                        DataRow row2 = dtCurrentLot.Rows[0];
                        if (row2["ProcessName"].ToString() != "QA")
                        {
                            isNow = false;
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "FAIL", ex.Message);
                afterLotEndResult.HasError = true;
                afterLotEndResult.ErrorMessage = ex.Message;
            }
        }
        else if (flowPattern == 1501 || flowPattern == 1502)
        {
            if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
            {

            }
            else
            {
                DataTable dtTransLot = GetTransLotFlow(lotId);
                if (dtTransLot.Rows.Count > 0)
                {
                    bool isNextInsp = false;
                    for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
                    {
                        DataRow row = dtTransLot.Rows[i];

                        if (Convert.ToInt32(row["step_no"]) == nextStepNo)
                        {
                            if (row["job_name"].ToString() == "100% INSP.")
                            {
                                isNextInsp = true;
                                currentStepNo = Convert.ToInt32(row["step_no"]);
                            }
                            else
                            {
                                break;
                            }                            
                        }
                        else if (Convert.ToInt32(row["is_skipped"]) == 0)
                        {
                            if (isNextInsp)
                            {
                                nextStepNo = Convert.ToInt32(row["step_no"]);
                                break;
                            }
                            else
                            {
                                currentStepNo = Convert.ToInt32(row["step_no"]);
                            }                           
                        }


                        //if (row["job_name"].ToString() == lastFtAuto && Convert.ToInt32(row["is_skipped"]) == 0 && currentStepNo == 0)
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
            }
        }
        else if (flowPattern == 1666)
        {
            DataTable dtTransLot = GetTransLotFlow(lotId);
            if (dtTransLot.Rows.Count > 0)
            {
                for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
                {
                    DataRow row = dtTransLot.Rows[i];
                    if (Convert.ToInt32(row["step_no"]) == nextStepNo)
                    {
                        break;
                    }
                    else if (Convert.ToInt32(row["is_skipped"]) == 0)
                    {
                        currentStepNo = Convert.ToInt32(row["step_no"]);
                        switch (row["job_name"].ToString())
                        {
                            case "AUTO(1)":
                                flowPattern = 1194;
                                break;
                            case "AUTO(2)":
                                flowPattern = 1195;
                                break;
                            case "AUTO(3)":
                                flowPattern = 1196;
                                break;
                            case "AUTO(4)":
                                flowPattern = 1197;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
        else
        {
            DataTable dtTransLot = GetTransLotFlow(lotId);
            if (dtTransLot.Rows.Count > 0)
            {                
                for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
                {
                    DataRow row = dtTransLot.Rows[i];

                    if (Convert.ToInt32(row["step_no"]) == nextStepNo)
                    {
                        break;
                    }
                    else if (Convert.ToInt32(row["is_skipped"]) == 0)
                    {
                        currentStepNo = Convert.ToInt32(row["step_no"]);
                    }
                   

                    //if (row["job_name"].ToString() == lastFtAuto && Convert.ToInt32(row["is_skipped"]) == 0 && currentStepNo == 0)
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
        }
        AddSpecialFlow_By_FlowPattern(mcNo, lotNo, lotId, currentStepNo, nextStepNo, flowPattern, isNow);
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
    private DataTable GetCurrentTranLot(string lotNo, out int? lotId, out string currentFlowName,out int? currentFlowId,out string currentAssyDevice)
    {
        currentFlowName = "";
        currentAssyDevice = "";
        currentFlowId = null;
        lotId = null;
        DataTable ret = new DataTable();
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
                            ret.Load(rd);
                        }
                    }
                    cmd.Connection.Close();
                }
                foreach (DataRow row in ret.Rows)
                {
                    currentFlowName = row["FlowName"].ToString();
                    lotId = Convert.ToInt32(row["LotId"].ToString()) ;
                    currentFlowId = Convert.ToInt32(row["FlowId"].ToString());
                    currentAssyDevice = row["Assy_Name"].ToString();
                }
            }
            catch (Exception ex)
            {
                SaveLogFile("", lotNo, "GetCurrentTranLot", "FAIL", ex.Message);
            }
        }
        return ret;
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
    private void AddSpecialFlow_By_FlowPattern(string mcNo, string lotNo, int? lotId, int currentStepNo, int nextStepNo, int flow_pattern_id, bool is_Now)
    {
        int addSpecialFlowNow = 2;
        if (is_Now)
        {
            addSpecialFlowNow = 1;
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
            cmd.Parameters.Add("@flow_pattern_id", System.Data.SqlDbType.Int).Value = flow_pattern_id; //100%INSP
            cmd.Parameters.Add("@is_special_flow", System.Data.SqlDbType.Int).Value = addSpecialFlowNow;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow", "PASS", "STORE >> [atom].[sp_set_trans_special_flow] >> [flow_pattern_id : " + flow_pattern_id.ToString() + "]|" 
                + currentStepNo.ToString() + "|" + nextStepNo.ToString() + "|");
            
        }
    }
    private DataTable Get_wb_hp_pp_checkframetype(string mcNo, string lotNo)
    {
        DataTable ret = new DataTable();
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[jig].[sp_get_wb_hp_pp_checkframetype]";
            cmd.Parameters.Add("@LotNo", SqlDbType.VarChar).Value = lotNo;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
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
        return ret;
    }
    private DataTable Get_jig_onmachine(string mcNo)
    {
        DataTable ret = new DataTable();
        try
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[jig].[sp_get_jig_onmachine]";
                cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
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
            SaveLogFile(mcNo, "", "Get_jig_onmachine", "Fail", "Error : " + ex.Message);
        }
        return ret;
    }
    private DataTable Check_jig_qrCode(string mcNo, string opNo, string lotNo, string qrCode)
    {
        DataTable ret = new DataTable();
        try
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[StoredProcedureDB].[jig].[sp_get_hp_pp_setup]";
                cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
                cmd.Parameters.Add("@LOTNo", SqlDbType.VarChar).Value = lotNo;
                cmd.Parameters.Add("@HPPP_QR", SqlDbType.VarChar).Value = qrCode;
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
            SaveLogFile(mcNo, "", "Get_jig_onmachine", "Fail", "Error : " + ex.Message);
        }
        return ret;
    }
    #endregion

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
    private List<JigDataInfo> JigCheckData_WbHp(string mcNo, string lotNo)
    {
        List<JigDataInfo> result = new List<JigDataInfo>();
        DataTable getResultDataTable = Get_wb_hp_pp_checkframetype(mcNo, lotNo);
        if (getResultDataTable.Rows.Count > 0)
        {
            int rowCount = 0;
            foreach (DataRow row in getResultDataTable.Rows)
            {
                JigDataInfo jigGetData = new JigDataInfo();
                string tmpIsPass = row["Is_Pass"].ToString();
                bool isPass = false;
                if (tmpIsPass == "TRUE") isPass = true; else isPass = false;
                jigGetData.IsPass = true;
                jigGetData.Message_Thai = row["Error_Message_THA"].ToString();
                jigGetData.Message_Eng = row["Error_Message_ENG"].ToString();
                if (isPass)
                {
                    jigGetData.Type = row["FrameType"].ToString();
                    switch (rowCount)
                    {
                        case 0:
                            jigGetData.Name = "HP";
                            jigGetData.ShortName = "HP";
                            break;
                        case 1:
                            jigGetData.Name = "PP";
                            jigGetData.ShortName = "PP";
                            break;
                        default:
                            break;
                    }                     
                }
                else
                {
                    jigGetData.IsWarning = true;
                    jigGetData.Warning = row["Handling"].ToString();
                    if (getResultDataTable.Columns.Count == 6)
                    {
                        jigGetData.Message_Thai = jigGetData.Message_Thai + "|LotFrame:" + row[4].ToString() + "|JigFrame:" + row[5].ToString() + "|";
                        jigGetData.Message_Eng = jigGetData.Message_Eng + "|LotFrame:" + row[4].ToString() + "|JigFrame:" + row[5].ToString() + "|";
                    }
                    jigGetData.Handling = row["Handling"].ToString();                          
                }
                result.Add(jigGetData);
                rowCount += 1;
            }
        }        
        return result;
    }
    private JigDataInfo[] JigGetApplyDataInfo(string mcNo)
    {
        JigDataInfo[] ret = null;
        DataTable getJigDataByMachine = Get_jig_onmachine(mcNo);
        if (getJigDataByMachine.Rows.Count > 0)
        {
            ret = new JigDataInfo[getJigDataByMachine.Rows.Count];
            int i = 0;
            foreach (DataRow row in getJigDataByMachine.Rows)
            {
                ret[i].Id = Convert.ToInt32(row["jig_id"]);
                ret[i].BarCode = row["barcode"].ToString();
                ret[i].SmallCode = row["smallcode"].ToString();
                ret[i].QrCodeByUser = row["qrcodebyuser"].ToString();
                ret[i].Status = row["status"].ToString();
                ret[i].SubType = row["SubType"].ToString();
                ret[i].Value = Convert.ToInt32(row["LifeTime"]);
                
            }
        }
        return ret;
    }
    private JigDataInfo JigCheckData(string mcNo, string opNo, string lotNo, string qrCode)
    {
        JigDataInfo ret = new JigDataInfo();
        DataTable checkResult = Check_jig_qrCode(mcNo, opNo, lotNo, qrCode);
        if (checkResult.Rows.Count > 0)
        {
            DataRow row = checkResult.Rows[0];
            if (row["Is_Pass"].ToString() == "FALSE")
            {
                ret.IsPass = false;
                ret.Message_Eng = row["Error_Message_ENG"].ToString();
            }
            else
            {
                ret.Id = Convert.ToInt32(row["HPPP_ID"]);
                ret.IsPass = true;
                ret.SmallCode = row["HPPP_SmallCode"].ToString();
            }
            //foreach (DataRow row in getJigDataByMachine.Rows)
            //{
            //    JigDataInfo jigDataInfo = new JigDataInfo();
            //    jigDataInfo.Id = Convert.ToInt32(row["jig_id"]);
            //    jigDataInfo.BarCode = row["barcode"].ToString();
            //    jigDataInfo.SmallCode = row["smallcode"].ToString();
            //    jigDataInfo.QrCodeByUser = row["qrcodebyuser"].ToString();
            //    jigDataInfo.Status = row["status"].ToString();
            //    jigDataInfo.SubType = row["SubType"].ToString();
            //    jigDataInfo.Value = Convert.ToInt32(row["LifeTime"]);
            //    ret.Add(jigDataInfo);
            //}
        }
        return ret;
    }

    //public List<JigDataInfo> JigToolCheckData(string mcNo, string lotNo, string jigQrCode)
    //{
    //    JigDataInfo[] result = null;
    //    SaveLogFile(mcNo, lotNo, "JigToolCheckData", "", "jigQrCode : " + jigQrCode);
    //    bool isUseHp = IsUseJigControl(mcNo);
    //    if (isUseHp)
    //    {
    //        switch (mcNo.Substring(0, 2))
    //        {
    //            case "WB":
    //                if (isUseHp)
    //                {
    //                    result = JigGetApplyDataInfo(mcNo);
    //                }
    //                else
    //                {

    //                }

    //                break;
    //            case "TC":
    //            case "FL":
    //                //JigDataInfo tmpJig = new JigDataInfo { Type = "Kanagata" };
    //                break;
    //            default:
    //                break;
    //        }
    //    }
       
    //    return result;
    //}

    public List<JigDataInfo> JigToolUpdateData(string mcNo, string lotNo, string jigQrCode)
    {
        List<JigDataInfo> result = new List<JigDataInfo>();

        return result;
    }

    public void OnLotSetupChecking()
    {

    }
    private bool IsUseJigControl(string mcNo)
    {
        bool ret = false;
        switch (mcNo.Substring(0, 2))
        {
            case "WB":
                string strPath = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\TempJigControl.txt");
                using (StreamReader rd = new StreamReader(strPath))
                {
                    while (!rd.EndOfStream)
                    {
                        string tmpStr = rd.ReadLine();
                        string[] tmpStrList = tmpStr.Split('|');
                        if (tmpStrList[0] == mcNo)
                        {
                            string[] tmpHp = tmpStrList[1].Split(',');
                            if (tmpHp[1] == "1")
                            {
                                ret = true;
                            }
                        }
                    }
                }
                break;
            case "TC":
            case "FL":
                //JigDataInfo tmpJig = new JigDataInfo { Type = "Kanagata" };
                break;
            default:
                break;
        }
        return ret;
    }
    
    public JigDataInfo[] JigGetData(string mcno, string lotno)
    {
        //throw new NotImplementedException();
        JigDataInfo[] result = null;
        SaveLogFile(mcno, lotno, "JigToolGetData", "", "");
        //// Get jig data here
        //// If LotNo != ""
        bool isUseHp = IsUseJigControl(mcno);
        if (isUseHp)
        {
            switch (mcno.Substring(0, 2))
            {
                case "WB":
                    if (isUseHp)
                    {
                        result = JigGetApplyDataInfo(mcno);
                    }
                    else
                    {

                    }

                    break;
                case "TC":
                case "FL":
                    //JigDataInfo tmpJig = new JigDataInfo { Type = "Kanagata" };
                    break;
                default:
                    break;
            }
        }
        return result;
    }
}
