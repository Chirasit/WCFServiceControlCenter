﻿using System;
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
        if (e.LotDataQuantity != null)
        {
            try
            {
                ZeroControlAdjust(e);
            }
            catch (Exception ex)
            {
                SaveLogFile(e.McNo, e.LotNo, "ZeroControlAdjust", "Error > ", ex.Message, LogType.zero_control);
            }
            
        }
        
        AfterLotEndResult afterLotEndResult = new AfterLotEndResult();
        afterLotEndResult.HasError = false;
        afterLotEndResult.WarningMessage = "";

        SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "NULL", "LotJudge := " + e.LotJudge, LogType.special_flow);
        if (e != null)
        {
            if (e.LotJudge == "")
            {
                return afterLotEndResult;
            }
            int? lotId = null;
            int? currentFlowId = null;
            string currentFlowName = "";
            string currentAssyDevice = "";
            DataTable currentDataTable = GetCurrentTranLot(e.LotNo, out lotId, out currentFlowName, out currentFlowId, out currentAssyDevice);
            foreach (DataRow row in currentDataTable.Rows)
            {
                if (row["QualityState"].ToString() == "Special Flow" || row["IsSpecialFlow"].ToString() == "1")
                {
                    SaveLogFile(e.McNo, e.LotNo, e.LotJudge, "Fail", "SpecialFlow:=" + Convert.ToInt32(row["FlowId"]).ToString() + "|QualityState:=" + row["QualityState"].ToString(),LogType.special_flow);
                    return afterLotEndResult;
                }
                SaveLogFile(e.McNo, e.LotNo, "GetCurrentTranLot", "QualityState:=" + row["QualityState"].ToString(), "IsSpecialFlow:=" + row["IsSpecialFlow"].ToString(), LogType.special_flow);
                currentFlowName = row["FlowName"].ToString();
                lotId = Convert.ToInt32(row["LotId"].ToString());
                currentFlowId = Convert.ToInt32(row["FlowId"].ToString());
                currentAssyDevice = row["Assy_Name"].ToString();
            }
            if (!string.IsNullOrEmpty(e.LotNo) && !string.IsNullOrEmpty(e.McNo))
            {
                DataRow row = currentDataTable.Rows[0];
                int currentStepNo = (int)row["StepNo"];
                int flowPattern = 0;
                switch (e.LotJudge)
                {
                    case "ROHM JUDGE": //1791
                        flowPattern = 1791;
                        break;
                    case "QC JUDGE": //1792
                        AddSpecialFlowBin27Judge(lotId, e.McNo, e.LotNo, 1792);
                        return afterLotEndResult;                        
                    case "ALARM BIN27": //1790
                        flowPattern = 1790;
                        break;
                    case "ABNORMAL_QC": //88888
                        AddSpecialFlowBin27Judge(lotId,e.McNo,e.LotNo,1792);
                        if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
                        {
                            SaveLogFile(e.McNo, e.LotNo, e.LotJudge, "Fail", "Current flow:=Insp.|QualityState:=" + row["QualityState"].ToString(), LogType.special_flow);
                            return afterLotEndResult;
                        }
                        flowPattern = 1198;
                        break;
                    case "AB_LOW YIELD_QC": //88888
                        AddSpecialFlowBin27Judge(lotId, e.McNo, e.LotNo, 1792);
                        flowPattern = 1719;
                        break;
                    case "LOW YIELD_QC":
                        AddSpecialFlowBin27Judge(lotId, e.McNo, e.LotNo, 1792);
                        flowPattern = 1501;
                        break;
                    case "LOW YIELD": //1501
                        flowPattern = 1501;
                        break;
                    case "IC BURN": //1502     
                        flowPattern = 1502;
                        break;
                    case "IC BURN_LOW YIELD": //1725
                        flowPattern = 1725;
                        break;
                    case "ABNORMAL": //1198
                        if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
                        {
                            SaveLogFile(e.McNo, e.LotNo, e.LotJudge, "Fail", "Current flow:=Insp.|QualityState:=" + row["QualityState"].ToString(), LogType.special_flow);
                            return afterLotEndResult;
                        }
                        flowPattern = 1198;
                        break;
                    case "AB_LOW YIELD": //1719
                        flowPattern = 1719;
                        break;
                    case "AB_IC BURN": //1720
                        flowPattern = 1720;
                        break;
                    case "AB_IC BURN_LOW YIELD": //1721
                        flowPattern = 1721;
                        break;
                    case "BIN27": //1500
                        flowPattern = 1500;
                        break;
                    case "BIN27_LOW YIELD": //1724
                        flowPattern = 1724;
                        break;
                    case "BIN27_ABNORMAL": //1722
                        flowPattern = 1722;
                        break;
                    case "BIN27_AB_LOW YIELD": //1723
                        flowPattern = 1723;
                        break;
                    case "ASI": //99999
                        if (currentFlowName.Trim() == "AUTO(2)ASISAMPLE" || currentFlowName.Trim() == "AUTO(3)ASISAMPLE")
                        {
                            SaveLogFile(e.McNo, e.LotNo, "AfterLotEnd", "Fail", "This lot already in job[" + currentFlowId.ToString() + "] : " + currentFlowName, LogType.special_flow);
                            return afterLotEndResult;
                        }
                        flowPattern = 99999;
                        break;
                        
                    case "ASI_LOW YIELD":
                    case "ASI_IC BURN":
                    case "ASI_ICBURN_LOW YIELD":

                        break;
                    case "ALARM BIN27-CF":
                        flowPattern = 1807;
                        break;
                    case "NG TEST": //1808
                        AddSpecialFlowBin27Judge(lotId, e.McNo, e.LotNo, 1808);
                        return afterLotEndResult;
                    case "LOW YIELD_NG":
                        AddSpecialFlowBin27Judge(lotId, e.McNo, e.LotNo, 1808);
                        flowPattern = 1501;
                        break;
                    case "AB_LOW YIELD_NG":
                        AddSpecialFlowBin27Judge(lotId, e.McNo, e.LotNo, 1808);
                        flowPattern = 1719;
                        break;
                    case "ABNORMAL_NG":
                        AddSpecialFlowBin27Judge(lotId, e.McNo, e.LotNo, 1808);
                        if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
                        {
                            SaveLogFile(e.McNo, e.LotNo, e.LotJudge, "Fail", "Current flow:=Insp.|QualityState:=" + row["QualityState"].ToString(), LogType.special_flow);
                            return afterLotEndResult;
                        }
                        flowPattern = 1198;
                        break;
                    default:                       
                        break;
                }
                afterLotEndResult = AddSpecialFlowByLotJudgement(e.McNo, e.LotNo, e.LotJudge, currentFlowId, lotId, flowPattern, currentStepNo);
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
            SaveLogFile("", e.LotNo, "AddSpecialFlow", "Warning", "ไม่พบข้อมูลในระบบ", LogType.special_flow);
            return result;
        }
        if (currentState != "Wait")
        {
            result.HasError = true;
            result.ErrorMessage = "สถานะของ " + e.LotNo + " ต้องเป็น WIP เท่านั้น กรุณาตรวจสอบใน ATOM";
            SaveLogFile("", e.LotNo, "AddSpecialFlow", "Failed", "สถานะของ " + e.LotNo + " ต้องเป็น WIP เท่านั้น กรุณาตรวจสอบใน ATOM", LogType.special_flow);
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
                        SaveLogFile("", e.LotNo, "AddSpecialFlow", "Failed", "This lot : " + e.LotNo + " already in " + row["job_name"].ToString(), LogType.special_flow);
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
            SaveLogFile("", e.LotNo, "AddSpecialFlow", "Failed", "ไม่พบรายละเอียดของ LotNo : " + e.LotNo + " ในระบบ กรุณาตรวจสอบ", LogType.special_flow);
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
            SaveLogFile("", lotNo, "SetQualityState", "Fail", ex.Message, LogType.special_flow);

        }
        return afterLotEndResult;
    }
    private AfterLotEndResult AddSpecialFlowByLotJudgement(string mcNo, string lotNo, string lotJudgement, int? currentFlowId, int? lotId, int? flowPattern, int nextStepNo = 0)
    {
        AfterLotEndResult afterLotEndResult = new AfterLotEndResult();
        afterLotEndResult.HasError = false;
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
                                    SaveLogFile(mcNo, lotNo, "Add SpecialFlow : 142", "PASS", "is_special_flow = " + row[0].ToString(), LogType.special_flow);
                                }
                            }
                        }
                        cmd.Connection.Close();
                        SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow", "PASS", "STORE >> [atom].[sp_set_trans_special_flow_temp]", LogType.special_flow);
                    }
                }
                catch (Exception ex)
                {
                    SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(INSPECTION)", "FAIL", ex.Message, LogType.special_flow);
                    afterLotEndResult.HasError = true;
                    afterLotEndResult.ErrorMessage = ex.Message;
                    //throw;
                }
                return afterLotEndResult;          
            case "ASI":
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
                        string ftFlow = GetLatestFtFlow(lotId, nextStepNo);
                        switch (ftFlow)
                        {
                            case "AUTO(1)":
                                flowPattern = 1194; //Add retest Auto1
                                break;
                            case "AUTO(2)":
                                flowPattern = 1195; //Add retest Auto2
                                break;
                            case "AUTO(3)":
                                flowPattern = 1196; //Add retest Auto3
                                break;
                            case "AUTO(4)":
                                flowPattern = 1197; //Add retest Auto4
                                break;
                            default:
                                flowPattern = 99999; //Add retest Auto4
                                break;
                        }         
                        break;
                }
                break;
            case "ASI_LOW YIELD":
                switch (currentFlowId)
                {
                    case 108: //Auto2
                        flowPattern = 1652; //Add retest Auto1 > Low Yield
                        break;
                    case 110: //Auto3
                        flowPattern = 1653; //Add retest Auto2 > Low Yield
                        break;
                    case 119: //Auto4
                        flowPattern = 1654; //Add retest Auto3 > Low Yield
                        break;
                    case 342: //asi 2 sample
                    case 370: //asi 3 sample
                        flowPattern = 1501; //Add Low Yield
                        break;
                    default:
                        string ftLatestFlow = GetLatestFtFlow(lotId, nextStepNo);
                        switch (ftLatestFlow)
                        {
                            case "AUTO(1)":
                                flowPattern = 1652; //Add retest Auto1 > Low Yield
                                break;
                            case "AUTO(2)":
                                flowPattern = 1653; //Add retest Auto2 > Low Yield
                                break;
                            case "AUTO(3)":
                                flowPattern = 1654; //Add retest Auto3 > Low Yield
                                break;
                            case "AUTO(4)":
                                flowPattern = 1655; //Add retest Auto4 > Low Yield
                                break;
                            default:
                                flowPattern = 99999; //Add retest Auto4
                                break;
                        }
                        break;
                }
                break;
            case "ASI_IC BURN":
                switch (currentFlowId)
                {
                    case 108: //Auto2
                        flowPattern = 1677; //Add retest Auto1 > IC Burn
                        break;
                    case 110: //Auto3
                        flowPattern = 1676; //Add retest Auto2 > IC Burn
                        break;
                    case 119: //Auto4
                        flowPattern = 1769; //Add retest Auto3 > IC Burn
                        break;
                    case 342: //asi 2 sample
                    case 370: //asi 3 sample
                        flowPattern = 1502; //Add IC Burn
                        break;
                    default:
                        string ftLatestFlow = GetLatestFtFlow(lotId, nextStepNo);
                        switch (ftLatestFlow)
                        {
                            case "AUTO(1)":
                                flowPattern = 1677; //Add retest Auto1 > IC Burn
                                break;
                            case "AUTO(2)":
                                flowPattern = 1676; //Add retest Auto2 > IC Burn
                                break;
                            case "AUTO(3)":
                                flowPattern = 1769; //Add retest Auto3 > IC Burn
                                break;
                            case "AUTO(4)":
                                flowPattern = 1770; //Add retest Auto4 > IC Burn
                                break;
                            default:
                                flowPattern = 99999; //Add retest Auto4
                                break;
                        }
                        break;
                }
                break;
            case "AB_LOW YIELD":
                if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
                {
                    flowPattern = 1501;
                }               
                break;
            case "AB_IC BURN":
                if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
                {
                    flowPattern = 1502;
                }
                break;

        }
        if (currentFlowId == null || lotId == null)
        {
            SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "NOTHING", "FlowId and LotId == null", LogType.special_flow);
            return afterLotEndResult;
        }
        if (flowPattern == 0)
        {
            SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "NOTHING", "FlowPattern == null", LogType.special_flow);
            return afterLotEndResult;
        }
        
        int currentStepNo = 0;
        bool isNow = true;
        if (flowPattern == 1198)
        {
            try
            {
                DataTable dtTransLot = GetTransLotFlow(lotId);
                if (dtTransLot.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
                    {
                        DataRow row = dtTransLot.Rows[i];
                        if ((row["job_name"].ToString() == "AUTO(2)ASISAMPLE" && Convert.ToInt32(row["is_skipped"]) == 0 && currentStepNo == 0) || (row["job_name"].ToString() == "AUTO(3)ASISAMPLE" && Convert.ToInt32(row["is_skipped"]) == 0 && currentStepNo == 0))
                        {
                            if (nextStepNo == Convert.ToInt32(row["step_no"]))
                            {
                                isNow = false;
                            }                            
                            currentStepNo = Convert.ToInt32(row["step_no"]);                            
                            continue;
                        }
                        else if (currentStepNo != 0 && Convert.ToInt32(row["is_skipped"]) == 0)
                        {
                            nextStepNo = Convert.ToInt32(row["step_no"]);
                            break;
                        }
                    }
                    //if (currentStepNo == 0 || nextStepNo == 0)
                    //{
                    //    SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "FAIL", "CANNOT GET STEP_NO", LogType.special_flow);
                    //    return afterLotEndResult;
                    //}
                    //if (flowPattern == 1267)
                    //{
                    //    DataTable dtCurrentLot = GetCurrentTranLot(lotNo);
                    //    DataRow row2 = dtCurrentLot.Rows[0];
                    //    if (row2["ProcessName"].ToString() != "QA")
                    //    {
                    //        isNow = false;
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "FAIL", ex.Message, LogType.special_flow);
                afterLotEndResult.HasError = true;
                afterLotEndResult.ErrorMessage = ex.Message;
            }
        }
        else if (flowPattern == 1501 || flowPattern == 1502 || flowPattern == 1719 || flowPattern == 1720 || flowPattern == 1721)
        {
            if (currentFlowId == 142 || currentFlowId == 11 || currentFlowId == 266)
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
                            if ((row["job_name"].ToString() == "100% INSP.") || (row["job_name"].ToString() == "SAMPLING INSP") ||  (row["job_name"].ToString() == "AUTO(2)ASISAMPLE") || (row["job_name"].ToString() == "AUTO(3)ASISAMPLE"))
                            {
                                isNextInsp = true;
                                currentStepNo = Convert.ToInt32(row["step_no"]);
                                isNow = false;
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
                    }
                }
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
                            if ((row["job_name"].ToString() == "100% INSP.") || (row["job_name"].ToString() == "SAMPLING INSP") || (row["job_name"].ToString() == "AUTO(2)ASISAMPLE") || (row["job_name"].ToString() == "AUTO(3)ASISAMPLE"))
                            {
                                isNextInsp = true;
                                currentStepNo = Convert.ToInt32(row["step_no"]);
                                isNow = false;
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
                    }
                }
            }
            
        }
        else if (flowPattern == 99999) //Future flow
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
        if (currentStepNo == 0)
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
                }
                if (currentStepNo == 0 || nextStepNo == 0)
                {
                    SaveLogFile(mcNo, lotNo, "AddFtInspSpecialFlow(" + lotJudgement + ")", "FAIL", "CANNOT GET STEP_NO", LogType.special_flow);
                    return afterLotEndResult;
                }
                //if (flowPattern == 1267)
                //{
                //    DataTable dtCurrentLot = GetCurrentTranLot(lotNo);
                //    DataRow row2 = dtCurrentLot.Rows[0];
                //    if (row2["ProcessName"].ToString() != "QA")
                //    {
                //        isNow = false;
                //    }
                //}
            }
        }
        AddSpecialFlow_By_FlowPattern(mcNo, lotNo, lotId, currentStepNo, nextStepNo, flowPattern.Value, isNow);
        return afterLotEndResult;
    }
    private string GetLatestFtFlow(int? lotId, int nextStepNo)
    {
        string ret = "";
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
                    ret = row["job_name"].ToString();
                }
            }
        }
        return ret;
    }
    private void AddSpecialFlowBin27Judge(int? lotId, string mcNo, string lotNo,int flowPattern)
    {
        int currentStepNo = 0;
        int nextStepNo = 0;
        DataTable dtTransLot = GetTransLotFlow(lotId);
        bool isNext = false;
        if (dtTransLot.Rows.Count > 0)
        {
            for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
            {
                DataRow row = dtTransLot.Rows[i];
                if (Convert.ToInt32(row["is_skipped"]) == 0 && (row["job_name"].ToString().Length > 4) && row["job_name"].ToString().Substring(0, 4) == "AUTO")
                {
                    currentStepNo = Convert.ToInt32(row["step_no"]);
                }
            }
            if (currentStepNo != 0)
            {
                for (int i = 0; i < dtTransLot.Rows.Count - 1; i++)
                {
                    DataRow row = dtTransLot.Rows[i];
                    if (isNext && Convert.ToInt32(row["is_skipped"]) == 0)
                    {
                        nextStepNo = Convert.ToInt32(row["step_no"]);
                        break;
                    }
                    else if (!isNext && currentStepNo == Convert.ToInt32(row["step_no"]))
                    {
                        isNext = true;
                    }
                }
            }
        }

        AddSpecialFlow_By_FlowPattern(mcNo, lotNo, lotId, currentStepNo, nextStepNo, flowPattern, false);
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
            SaveLogFile("", lotId.ToString(), "GetTransLotFlow", "FAIL", ex.Message, LogType.special_flow);
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
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[StoredProcedureDB].[cellcon].[sp_get_package_groups]";
                cmd.Parameters.Add("@value", System.Data.SqlDbType.VarChar).Value = assyDevice;
                cmd.Parameters.Add("@searchBy", System.Data.SqlDbType.VarChar).Value = "assy_name";
                cmd.Connection.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(rd);
                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];
                            if (Convert.ToInt32(row["pkg_id"]) == 33)
                            {
                                ret = true;
                            }
                        }
                    }
                }
                cmd.Connection.Close();
            }
        }
        catch (Exception ex)
        {
            SaveLogFile("", assyDevice, "IsGDICDevice", "FAIL", ex.Message, LogType.special_flow);
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
                SaveLogFile("", lotNo, "GetCurrentTranLot", "FAIL", ex.Message, LogType.special_flow);
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
                SaveLogFile("", lotNo, "GetCurrentTranLot(lotNo)", "PASS", "", LogType.special_flow);
            }
            catch (Exception ex)
            {
                SaveLogFile("", lotNo, "GetCurrentTranLot(lotNo)", "FAIL", ex.Message, LogType.special_flow);
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
            SaveLogFile("", lotId.ToString(), "IsLotApcsPro", "Error", ex.Message, LogType.special_flow);
            result = false;
        }
        SaveLogFile("", lotId.ToString(), "IsLotApcsPro", result.ToString(),"", LogType.special_flow);
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
                + currentStepNo.ToString() + "|" + nextStepNo.ToString() + "|", LogType.special_flow);
            
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
            SaveLogFile(mcNo, "", "Get_jig_onmachine", "Fail", "Error : " + ex.Message, LogType.special_flow);
        }
        return ret;
    }
    private DataTable Check_HpPp_qrCode(string mcNo, string opNo, string lotNo, string qrCode)
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
            SaveLogFile(mcNo, "", "Get_jig_onmachine", "Fail", "Error : " + ex.Message, LogType.jig_material);
        }
        return ret;
    }
    #endregion

    private void SaveLogFile(string mcNo, string lotNo, string title, string result, string message, LogType logType)
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
            string strPath = "";
            string strPathBackup;
            if (logType == LogType.special_flow)
            {
                strPath = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\SpecialflowLog.csv");
                strPathBackup = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup\SpecialflowLog" + DateTime.Now.ToString("yyyyMMddHHmm") + @".csv");
                DeleteLogFile(System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup\"), "SpecialflowLog");
            }
            else if (logType == LogType.zero_control)
            {
                strPath = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\ZeroControlLog.csv");
                strPathBackup = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup\ZeroControlLog" + DateTime.Now.ToString("yyyyMMddHHmm") + @".csv");
                DeleteLogFile(System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup\"), "ZeroControlLog");
            }
            else
            {
                strPath = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\JigMaterialLog.csv");
                strPathBackup = System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup\JigMaterialLog" + DateTime.Now.ToString("yyyyMMddHHmm") + @".csv");
                DeleteLogFile(System.Web.HttpContext.Current.Server.MapPath(@"~\\Log\Backup\"), "JigMaterialLog");
            }
             
            FileInfo fi = new FileInfo(strPath);
            if (fi.Exists && fi.Length > 2097152)
            {
                File.Move(strPath, strPathBackup);
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
    private void DeleteLogFile(string path,string type)
    {
        DateTime date = DateTime.Now;
        if (date.Hour == 08 && date.Minute <= 10)
        {
            try
            {
                DirectoryInfo diInfo = new DirectoryInfo(path);
                FileInfo[] fileInfos = diInfo.GetFiles(type + "*");
                if (fileInfos.Length > 50)
                {
                    for (int i = 0; i < fileInfos.Length - 50; i++)
                    {
                        File.Delete(fileInfos[i].FullName);
                    }
                }
            }
            catch (Exception)
            {
            }

        }
    }
    enum LogType
    {
        special_flow = 1,
        jig_material = 2,
        zero_control = 3
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
    private List<JigDataInfo> JigGetApplyDataInfo(string mcNo)
    {
        List<JigDataInfo> ret = new List<JigDataInfo>();        
        DataTable getJigDataByMachine = Get_jig_onmachine(mcNo);
        if (getJigDataByMachine.Rows.Count > 0)
        {
            foreach (DataRow row in getJigDataByMachine.Rows)
            {
               JigDataInfo tmp = new JigDataInfo();
                try
                {
                    tmp.Id = Convert.ToInt32(row["jig_id"]);
                    tmp.BarCode = row["barcode"].ToString();
                    tmp.SmallCode = row["smallcode"].ToString();
                    if (!row.IsNull("qrcodebyuser"))
                    {
                        tmp.QrCodeByUser = row["qrcodebyuser"].ToString();
                    }
                    else
                    {
                        tmp.QrCodeByUser = "";
                    }
                    tmp.Type = row["Type"].ToString();
                    tmp.ShortName = row["Type"].ToString();
                    tmp.Status = row["status"].ToString();
                    tmp.SubType = row["SubType"].ToString();
                    tmp.Value = Convert.ToInt32(row["LifeTime"]);
                    ret.Add(tmp);
                }
                catch (Exception ex)
                {
                    SaveLogFile(mcNo, "", "JigGetApplyDataInfo", row["jig_id"].ToString(), ex.Message, LogType.special_flow);
                }
            }
        }
        return ret;
    }
    public ResultInfo JigUpdateData(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string[] parameter)
    {
        ResultInfo resultInfo = new ResultInfo();
        SaveLogFile(mcNo, lotNo, "JigUpdateData[" + jigType + "]", "", "", LogType.jig_material);
        if (string.IsNullOrEmpty(jigInfo.QrCodeByUser))
        {
            SaveLogFile(mcNo, lotNo, "JigUpdateData[" + jigType + "]", "Fail", "not found QrCode", LogType.jig_material);
            resultInfo.HasError = true;
            resultInfo.ErrorMessage = "qrcode is null";
            resultInfo.ErrorMessage_Tha = "ไม่พบข้อมูลของ qrcode";
            return resultInfo;
        }
        string[] strSplit = jigType.Split('-');
        switch (strSplit[0])
        {
            case "Capillary":                
                resultInfo = CapillaryUpdate(mcNo, opNo, lotNo, jigInfo, strSplit[0]);
                break;
            case "HP":
            case "PP":
                resultInfo = HpPpUpdate(mcNo, opNo, lotNo, jigInfo);
                break;
            case "Wire":
                if (parameter == null)
                {
                    SaveLogFile(mcNo, lotNo, "JigSetupData[" + strSplit[0] + "]", "", "parameter is null", LogType.jig_material);
                    resultInfo.HasError = true;
                    resultInfo.ErrorMessage = "parameter is null";
                    resultInfo.ErrorMessage_Tha = "ไม่พบข้อมูลของ Parameter";
                    break;
                }
                resultInfo = WireUpdate(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            case "Kanagata":
                //resultInfo = KanagataUpdate(mcNo, opNo, lotNo, jigInfo, strSplit[0]);
                break;

            default:
                break;
        }
        return resultInfo;
    }
    public ResultInfo JigSetupData(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string[] parameter)
    {
        ResultInfo resultInfo = new ResultInfo();
        if (string.IsNullOrEmpty(jigInfo.QrCodeByUser))
        {
            SaveLogFile(mcNo, lotNo, "JigSetupData[" + jigType + "]", "Fail", "not found QrCode", LogType.jig_material);
            resultInfo.HasError = true;
            resultInfo.ErrorMessage = "qrcode is null";
            resultInfo.ErrorMessage_Tha = "ไม่พบข้อมูลของ qrcode";
            return resultInfo;
        }
        string[] strSplit = jigType.Split('-');
        switch (strSplit[0])
        {
            case "Capillary":
                if (parameter == null)
                {
                    SaveLogFile(mcNo, lotNo, "JigSetupData[" + strSplit[0] + "]", "", "parameter is null", LogType.jig_material);
                    resultInfo.HasError = true;
                    resultInfo.ErrorMessage = "parameter is null";
                    resultInfo.ErrorMessage_Tha = "ไม่พบข้อมูลของ Parameter";
                    break;
                }
                SaveLogFile(mcNo, lotNo, "CapillarySetup[" + strSplit[0] + "]", "", "QRCode := " + jigInfo.QrCodeByUser + " | TPCode := " + parameter[0], LogType.jig_material);
                resultInfo = CapillarySetup(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            case "HP":
            case "PP":
                if (parameter == null)
                {
                    resultInfo = HpPpSetup(mcNo, opNo, lotNo, jigInfo, strSplit[0], "");
                }
                else
                {
                    resultInfo = HpPpSetup(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                }
                
                break;
            case "Wire":
                if (parameter == null)
                {
                    SaveLogFile(mcNo, lotNo, "JigSetupData[" + strSplit[0] + "]", "", "parameter is null", LogType.jig_material);
                    resultInfo.HasError = true;
                    resultInfo.ErrorMessage = "parameter is null";
                    resultInfo.ErrorMessage_Tha = "ไม่พบข้อมูลของ Parameter";
                    break;
                }
                resultInfo = WireSetup(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            case "Kanagata":
                //if (parameter == null)
                //{
                //    SaveLogFile(mcNo, lotNo, "JigSetupData[" + jigType + "]", "", "parameter is null", LogType.jig_material);
                //    resultInfo.HasError = true;
                //    resultInfo.ErrorMessage = "parameter is null";
                //    resultInfo.ErrorMessage_Tha = "ไม่พบข้อมูลของ Parameter";
                //    break;
                //}
                resultInfo = KanagataSetup(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            case "Socket":
                resultInfo = SocketSetup(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            default:
                break;
        }
        return resultInfo;
    }    
    public JigDataInfo JigCheckData(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType,string[] parameter)
    {
        //JigDataInfo ret = new JigDataInfo();
        if (string.IsNullOrEmpty(jigInfo.QrCodeByUser))
        {
            SaveLogFile(mcNo, lotNo, "JigCheckData[" + jigType + "]", "Fail", "not found QrCode", LogType.jig_material);
            jigInfo.IsPass = false;
            jigInfo.Message_Eng = "qrcode is null";
            jigInfo.Message_Thai = "ไม่พบข้อมูลของ qrcode";
            return jigInfo;
        } 
        jigInfo.IsPass = false;
        string[] strSplit = jigType.Split('-');
        switch (strSplit[0])
        {
            case "Capillary":
                if (parameter == null)
                {
                    SaveLogFile(mcNo, lotNo, "JigCheckData[" + strSplit[0] + "]", "", "parameter is null", LogType.jig_material);
                    jigInfo.Message_Eng = "parameter is null";
                    jigInfo.Message_Thai = "ไม่พบข้อมูลของ Parameter";
                    break;
                }
                jigInfo = CapillaryCheck(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                SaveLogFile(mcNo, lotNo, "CapillaryCheck[" + strSplit[0] + "]", "", "", LogType.jig_material);
                break;
            case "HP":
            case "PP":
                jigInfo = HpPpCheck(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            case "Wire":
                if (parameter == null)
                {
                    SaveLogFile(mcNo, lotNo, "JigCheckData[" + strSplit[0] + "]", "", "parameter is null", LogType.jig_material);
                    jigInfo.Message_Eng = "parameter is null";
                    jigInfo.Message_Thai = "ไม่พบข้อมูลของ Parameter";
                    break;
                }
                jigInfo = WireCheck(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            case "Kanagata":
                if (parameter == null)
                {
                    SaveLogFile(mcNo, lotNo, "JigCheckData[" + strSplit[0] + "]", "", "parameter is null", LogType.jig_material);
                    jigInfo.Message_Eng = "parameter is null";
                    jigInfo.Message_Thai = "ไม่พบข้อมูลของ Parameter";
                    break;
                }
                jigInfo = KanagataCheck(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                break;
            case "Socket":
                if (parameter == null)
                {
                    SaveLogFile(mcNo, lotNo, "JigCheckData[" + strSplit[0] + "]", "", "parameter is null", LogType.jig_material);
                    jigInfo.Message_Eng = "parameter is null";
                    jigInfo.Message_Thai = "ไม่พบข้อมูลของ Parameter";
                    break;
                }
                if (parameter.Length == 2)
                {
                    int inputQty = 0;
                    int.TryParse(parameter[1], out inputQty);
                    jigInfo = SocketCheck(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0], inputQty);
                }
                else
                {
                    jigInfo = SocketCheck(mcNo, opNo, lotNo, jigInfo, strSplit[0], parameter[0]);
                }                
                break;
            default:
                break;
        }
        return jigInfo;
    }
    public ResultInfo JigRemoveData(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType)
    {
        ResultInfo ret = new ResultInfo();
        ret.HasError = false;
        SaveLogFile(mcNo, lotNo, "JigRemoveData[" + jigType + "]", "", "", LogType.jig_material);
        if (string.IsNullOrEmpty(jigInfo.QrCodeByUser))
        {
            ret.HasError = true;
            ret.ErrorMessage = "Can not remove";
            ret.ErrorMessage_Tha = "ไม่สามารถลบได้";
            ret.Handling = "Qr = NULL";
            return ret;
        }
        string[] strSplit = jigType.Split('-');
        switch (strSplit[0])
        {
            case "Capillary":
                ret.HasError = true;
                ret.ErrorMessage = "This function cannot use";
                ret.ErrorMessage_Tha = "ฟังก์ชันนี้ยังไม่เปิดให้ใช้งาน";
                //ret.Handling = "Qr = NULL";
                break;
            case "HP":
            case "PP":
                ret.HasError = true;
                ret.ErrorMessage = "This function cannot use";
                ret.ErrorMessage_Tha = "ฟังก์ชันนี้ยังไม่เปิดให้ใช้งาน";
                //ret.Handling = "Qr = NULL";
                break;
            case "Wire":
                ret.HasError = true;
                ret.ErrorMessage = "This function cannot use";
                ret.ErrorMessage_Tha = "ฟังก์ชันนี้ยังไม่เปิดให้ใช้งาน";
                //ret.Handling = "Qr = NULL";
                break;
            case "Kanagata":
                ret = KanagataRemove(mcNo, opNo, lotNo, jigInfo, strSplit[0]);
                break;
            case "Socket":
                ret = SocketRemove(mcNo, opNo, lotNo, jigInfo, strSplit[0]);
                break;
            default:
                break;
        }
        return ret;
    }
    #region JIG
    #region SOCKET
    public ResultInfo SocketSetup(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string package, int? inputQty = null)
    {
        ResultInfo resultInfo = new ResultInfo();
        if (package != "")
        {
            JigDataInfo resultCheck = SocketCheck(mcNo, opNo, lotNo, jigInfo, jigInfo.Type, package, inputQty);
            if (!resultCheck.IsPass)
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = resultCheck.Message_Eng;
                resultInfo.ErrorMessage_Tha = resultCheck.Message_Thai;
                resultInfo.Handling = resultCheck.Handling;
                goto End;
            }
        }
        resultInfo.HasError = true;
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "sp_set_socket_setup", "", jigInfo.QrCodeByUser + "|" + opNo, LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_socket_setup]";
            cmd.Parameters.Add("@QRCodeIn", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            }
            else
            {
                resultInfo.JigDataInfo = new JigDataInfo();
                resultInfo.JigDataInfo.Id = Convert.ToInt32(dataRow["id"]);
                resultInfo.JigDataInfo.SubType = dataRow["subtype"].ToString();
                
                
            }
        }
        End:
        return resultInfo;
    }
    public JigDataInfo SocketCheck(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string package, int? inputQty = null)
    {
        jigInfo.IsPass = true;
        if (package == "" || lotNo == "")
        {
            return jigInfo;
        }
        SaveLogFile(mcNo, lotNo, "sp_get_socket_setup", "", jigInfo.QrCodeByUser + "|" + lotNo, LogType.jig_material);
        DataTable dt = new DataTable();
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_get_socket_setup]";
            cmd.Parameters.Add("@QRCodeIn", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
            if (string.IsNullOrEmpty(lotNo)) cmd.Parameters.Add("@LotNo", SqlDbType.VarChar).Value = lotNo;
            if (string.IsNullOrEmpty(package)) cmd.Parameters.Add("@Package", SqlDbType.VarChar).Value = package;
            if (inputQty.HasValue) cmd.Parameters.Add("@DataInput", SqlDbType.Int).Value = inputQty.Value;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            SaveLogFile(mcNo, lotNo, "Ispass := " + dataRow["Is_Pass"].ToString(), "", "", LogType.jig_material);
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                jigInfo.IsPass = false;
                jigInfo.Message_Eng = dataRow["Error_Message_ENG"].ToString();
                jigInfo.Message_Thai = dataRow["Error_Message_THA"].ToString();
            }
            else
            {
                jigInfo.IsPass = true;
            }
        }

        return jigInfo;
    }
    public ResultInfo SocketRemove(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType)
    {
        ResultInfo resultInfo = new ResultInfo();
        resultInfo.HasError = true;
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "[sp_set_socket_outoffmachine]", "", jigInfo.QrCodeByUser + "|" + opNo, LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_socket_outoffmachine]";
            cmd.Parameters.Add("@QRCode", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            }
            else
            {
                resultInfo.HasError = false;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            }
        }
        return resultInfo;
    }
    #endregion
    #region KANAGATA

    public ResultInfo KanagataUpdate(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType,string parameter)
    {
        ResultInfo resultInfo = new ResultInfo();
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "QrCodeByUser := " + jigInfo.QrCodeByUser, "", "", LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_capillary_endlot]";
            cmd.Parameters.Add("@CAPID", SqlDbType.Int).Value = jigInfo.Id;
            cmd.Parameters.Add("@LOTNo", SqlDbType.VarChar).Value = lotNo;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
            cmd.Parameters.Add("@LTValue", SqlDbType.Int).Value = jigInfo.ValuePerLot;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE") resultInfo.HasError = true;
            resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
            resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            resultInfo.Handling = dataRow["Handling"].ToString();
        }
        return resultInfo;
    }
    public ResultInfo KanagataSetup(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string package)
    {
        ResultInfo resultInfo = new ResultInfo();
        JigDataInfo resultCheck = KanagataCheck(mcNo, opNo, lotNo, jigInfo, jigInfo.Type, package);
        if (!resultCheck.IsPass)
        {
            resultInfo.HasError = true;
            resultInfo.ErrorMessage = resultCheck.Message_Eng;
            resultInfo.ErrorMessage_Tha = resultCheck.Message_Thai;
            resultInfo.Handling = resultCheck.Handling;
            goto End;
        }
        resultInfo.HasError = true;
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "sp_set_kanagata_setup_machine_V3", "", jigInfo.QrCodeByUser + "|" + opNo, LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_kanagata_setup_machine_v3]";
            cmd.Parameters.Add("@KanagataName", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@UserID", SqlDbType.VarChar).Value = opNo;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
                resultInfo.Handling = dataRow["Handling"].ToString();
                //resultInfo.JigDataInfo.Id = Convert.ToInt32(dataRow["id"]);
                //resultInfo.JigDataInfo.SmallCode = dataRow["smallCode"].ToString();
            }
            else
            {
                resultInfo.JigDataInfo = new JigDataInfo();
                resultInfo.JigDataInfo.Id = Convert.ToInt32(dataRow["id"]);
                resultInfo.JigDataInfo.SubType = dataRow["basetype"].ToString();
            }
        }
        End:
        return resultInfo;
    }
    public JigDataInfo KanagataCheck(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string package)
    {
        jigInfo.IsPass = true;
        if (package == "" || lotNo == "")
        {
            return jigInfo;
        }
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "sp_get_kanagata_checkpackage_v2", "", jigInfo.QrCodeByUser + "|" + lotNo, LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_get_kanagata_checkpackage_v2]";
            cmd.Parameters.Add("@kanagataNo", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@jigset", SqlDbType.VarChar).Value = package;
            cmd.Parameters.Add("@LotNo", SqlDbType.VarChar).Value = lotNo;
            cmd.Parameters.Add("@Package", SqlDbType.VarChar).Value = package;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            SaveLogFile(mcNo, lotNo, "Ispass := " + dataRow["Is_Pass"].ToString(), "", "", LogType.jig_material);
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                jigInfo.IsPass = false;
                jigInfo.Message_Eng = dataRow["Error_Message_ENG"].ToString();
                jigInfo.Message_Thai = dataRow["Error_Message_THA"].ToString();
                jigInfo.Handling = dataRow["Handling"].ToString();
            }
            else
            {
                //jigInfo.Id = Convert.ToInt32(dataRow["id"]);
                jigInfo.IsPass = true;
                jigInfo.SubType = dataRow["basetype"].ToString();
                //jigInfo.Name = dataRow["name"].ToString();
            }
        }
        return jigInfo;
    }
    public ResultInfo KanagataRemove(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType)
    {
        ResultInfo resultInfo = new ResultInfo();       
        resultInfo.HasError = true;
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "[sp_set_kanagata_outoffmachine]", "", jigInfo.QrCodeByUser + "|" + opNo, LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_kanagata_outoffmachine]";
            cmd.Parameters.Add("@KanagataNo", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            }
            else
            {
                resultInfo.HasError = false;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            }
        }
        return resultInfo;
    }
    #endregion
    #region COLLET
    #endregion
    #region TSUKIAGE
    #endregion
    #region HP_PP
    private ResultInfo HpPpUpdate(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo)
    {

        ResultInfo resultInfo = new ResultInfo();
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "[sp_set_hp_pp_endlot]", "", jigInfo.QrCodeByUser + "|" + opNo, LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_hp_pp_endlot]";
            cmd.Parameters.Add("@HPPP_ID", SqlDbType.Int).Value = jigInfo.Id;
            cmd.Parameters.Add("@LOTNo", SqlDbType.VarChar).Value = lotNo;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE") resultInfo.HasError = true;
            resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
            resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            resultInfo.Handling = dataRow["Handling"].ToString();
        }
        return resultInfo;
    }
    private ResultInfo HpPpSetup(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string parameter)
    {
        ResultInfo resultInfo = new ResultInfo();
        if (!string.IsNullOrEmpty(parameter))
        {
            JigDataInfo resultCheck = HpPpCheck(mcNo, opNo, lotNo, jigInfo, jigType, parameter);
            if (!resultCheck.IsPass)
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = resultCheck.Message_Eng;
                resultInfo.ErrorMessage_Tha = resultCheck.Message_Thai;
                resultInfo.Handling = resultCheck.Handling;
                return resultInfo;
            }
        }
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "sp_set_hp_pp_setup", "", jigInfo.QrCodeByUser + "|" + opNo, LogType.jig_material);

        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_hp_pp_setup]";
            cmd.Parameters.Add("@HPPP", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];

            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
                resultInfo.Handling = dataRow["Handling"].ToString();
            }
            else
            {
                resultInfo.HasError = false;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
                resultInfo.Handling = dataRow["Handling"].ToString();
                resultInfo.JigDataInfo = new JigDataInfo();
                resultInfo.JigDataInfo.Id = Convert.ToInt32(dataRow["HPPP_ID"]);
                resultInfo.JigDataInfo.SmallCode = dataRow["HPPP_SmallCode"].ToString();
            }


        }        
        return resultInfo;
    }
    private JigDataInfo HpPpCheck(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType,string parameter)
    {
        jigInfo.IsPass = true;
        SaveLogFile(mcNo, lotNo, "HpPpCheck[" + jigType + "]|Parameter := " + parameter + "|QrCode := " + jigInfo.QrCodeByUser, "", "", LogType.jig_material);
        if (parameter == "")
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[StoredProcedureDB].[jig].[sp_get_hp_pp_setup]";
                cmd.Parameters.Add("@HPPP", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
                cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
                cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
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
            if (dt.Rows.Count > 0)
            {
                DataRow dataRow = dt.Rows[0];
                SaveLogFile(mcNo, lotNo, "Ispass := " + dataRow["Is_Pass"].ToString(), "", "", LogType.jig_material);
                if (dataRow["Is_Pass"].ToString() == "FALSE")
                {
                    jigInfo.IsPass = false;
                    jigInfo.Message_Eng = dataRow["Error_Message_ENG"].ToString();
                    jigInfo.Message_Thai = dataRow["Error_Message_THA"].ToString();
                    jigInfo.Handling = dataRow["Handling"].ToString();
                }
                else
                {
                    jigInfo.IsPass = true;
                    jigInfo.Id = 0;
                    
                    jigInfo.Message_Eng = dataRow["Error_Message_ENG"].ToString();
                    jigInfo.Message_Thai = dataRow["Error_Message_THA"].ToString();
                    jigInfo.Handling = dataRow["Handling"].ToString();
                }
            }
        }
        else
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[StoredProcedureDB].[jig].[sp_get_hp_pp_checkframetype]";
                cmd.Parameters.Add("@LotNo", SqlDbType.VarChar).Value = lotNo;
                cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
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
            if (dt.Rows.Count > 0)
            {
                DataRow dataRow = dt.Rows[0];
                SaveLogFile(mcNo, lotNo, "Ispass := " + dataRow["Is_Pass"].ToString(), "", "", LogType.jig_material);
                if (dataRow["Is_Pass"].ToString() == "FALSE")
                {
                    jigInfo.IsPass = false;
                    jigInfo.Message_Eng = dataRow["Error_Message_ENG"].ToString();
                    jigInfo.Message_Thai = dataRow["Error_Message_THA"].ToString();
                    jigInfo.Handling = dataRow["Handling"].ToString();
                }
                else
                {
                    //jigInfo.Id = Convert.ToInt32(dataRow["id"]);
                    jigInfo.IsPass = true;
                    jigInfo.Message_Eng = dataRow["Error_Message_ENG"].ToString();
                    jigInfo.Message_Thai = dataRow["Error_Message_THA"].ToString();
                    jigInfo.Handling = dataRow["Handling"].ToString();
                    //jigInfo.SmallCode = dataRow["smallcode"].ToString();
                    //jigInfo.SubType = dataRow["SubType"].ToString();
                }
            }
        }        
        return jigInfo;
    }
    #endregion
    #region Cappillary
    private ResultInfo CapillaryUpdate(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType)    {
        
        ResultInfo resultInfo = new ResultInfo();
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "QrCodeByUser := " + jigInfo.QrCodeByUser, "", "", LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_capillary_endlot]";
            cmd.Parameters.Add("@CAPID", SqlDbType.Int).Value = jigInfo.Id;
            cmd.Parameters.Add("@LOTNo", SqlDbType.VarChar).Value = lotNo;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
            cmd.Parameters.Add("@LTValue", SqlDbType.Int).Value = jigInfo.ValuePerLot;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE") resultInfo.HasError = true;
            resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
            resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            resultInfo.Handling = dataRow["Handling"].ToString();
        }
        return resultInfo;
    }
    private ResultInfo CapillarySetup(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string tpCode)
    {
        ResultInfo resultInfo = new ResultInfo();
        JigDataInfo resultCheck = CapillaryCheck(mcNo, opNo, lotNo, jigInfo, jigInfo.Type, tpCode);
        if (!resultCheck.IsPass)
        {
            resultInfo.HasError = true;
            resultInfo.ErrorMessage = resultCheck.Message_Eng;
            resultInfo.ErrorMessage_Tha = resultCheck.Message_Thai;
            resultInfo.Handling = resultCheck.Handling;
            goto End;
        }
        
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "sp_set_capillary_setup", "", jigInfo.QrCodeByUser + "|" + opNo, LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_set_capillary_setup]";
            cmd.Parameters.Add("@CAPQR", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@MCNo", SqlDbType.VarChar).Value = mcNo;
            cmd.Parameters.Add("@OPNo", SqlDbType.VarChar).Value = opNo;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            resultInfo.HasError = false;
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                resultInfo.HasError = true;
                resultInfo.ErrorMessage = dataRow["Error_Message_ENG"].ToString();
                resultInfo.ErrorMessage_Tha = dataRow["Error_Message_THA"].ToString();
            }
            else
            {
                resultInfo.JigDataInfo = new JigDataInfo();
                resultInfo.Handling = dataRow["Handling"].ToString();
                resultInfo.JigDataInfo.Id = Convert.ToInt32(dataRow["id"]);
                resultInfo.JigDataInfo.SmallCode = dataRow["smallCode"].ToString();
            }   
        }
        End:
        return resultInfo;
    }
    private JigDataInfo CapillaryCheck(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string tpCode)
    {
        jigInfo.IsPass = true;
        if (tpCode == "." || tpCode == "")
        {
            return jigInfo;
        }
        DataTable dt = new DataTable();
        SaveLogFile(mcNo, lotNo, "QrCodeByUser := " + jigInfo.QrCodeByUser + " | tpCode := " + tpCode, "", "", LogType.jig_material);
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[jig].[sp_get_capillary_check]";
            cmd.Parameters.Add("@QRCode", SqlDbType.VarChar).Value = jigInfo.QrCodeByUser;
            cmd.Parameters.Add("@WBCode", SqlDbType.VarChar).Value = tpCode;
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
        if (dt.Rows.Count > 0)
        {
            DataRow dataRow = dt.Rows[0];
            SaveLogFile(mcNo, lotNo, "Ispass := " + dataRow["Is_Pass"].ToString(), "", "", LogType.jig_material);
            if (dataRow["Is_Pass"].ToString() == "FALSE")
            {
                jigInfo.IsPass = false;
                jigInfo.Message_Eng = dataRow["Error_Message_ENG"].ToString();
                jigInfo.Message_Thai = dataRow["Error_Message_THA"].ToString();
                jigInfo.Handling = dataRow["Handling"].ToString();
            }
            else
            {
                jigInfo.Id = Convert.ToInt32(dataRow["id"]);
                jigInfo.IsPass = true;
                jigInfo.SmallCode = dataRow["smallcode"].ToString();
                jigInfo.SubType = dataRow["SubType"].ToString();
            }
        }
        return jigInfo;
    }
    #endregion
    #endregion
    #region Material
    #region Wire
    private ResultInfo WireUpdate(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string flow)
    {
        ResultInfo resultInfo = new ResultInfo();
        resultInfo.HasError = false;
        return resultInfo;
    }
    private ResultInfo WireSetup(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string flow)
    {
        ResultInfo resultInfo = new ResultInfo();
        JigDataInfo resultCheck = WireCheck(mcNo, opNo, lotNo, jigInfo, jigInfo.Type, flow);
        if (!resultCheck.IsPass)
        {
            resultInfo.HasError = true;
            resultInfo.ErrorMessage = resultCheck.Message_Eng;
            resultInfo.ErrorMessage_Tha = resultCheck.Message_Thai;
            resultInfo.Handling = resultCheck.Handling;
            goto End;
        }
        else
        {
            resultInfo.HasError = false;            
        }
        End:
        return resultInfo;
    }
    private JigDataInfo WireCheck(string mcNo, string opNo, string lotNo, JigDataInfo jigInfo, string jigType, string flow)
    {
        JigDataInfo result = new JigDataInfo();
        result.Type = "Wire";
        result.IsPass = true;
        DatabaseSqlcommand sql = new DatabaseSqlcommand();        
        string wireTypeFull_LotNo = sql.GetWireType_LotNo(lotNo);
        if (wireTypeFull_LotNo == "")
        {
            result.IsPass = false;
            result.Message_Eng = "This lot was not found wiretype";
            result.Message_Thai = "ไม่พบข้อมูล Wire ของ Lot นี้";
            result.Handling = "กรุณาติดต่อ System ครับ";
            goto End;
        }
        string[] wireLotSplit = wireTypeFull_LotNo.Split('/');
        string wireTypeLotNo = wireTypeFull_LotNo;
        if (wireLotSplit.Length>1)
        {
            if (flow == "WB1") { wireTypeLotNo = wireLotSplit[0]; } else { wireTypeLotNo = wireLotSplit[1]; }
        }
        string[] strSplit = jigInfo.QrCodeByUser.Split(',');
        string wireName = "";
        if (strSplit[0] != "MAT")
        {
        }
        if (strSplit.Length == 4)
        {
            wireName = strSplit[4].Replace(".", "");
        }
        else
        {
            wireName = strSplit[1].Replace(".", "");
        }
        int wireIdResult = 0;
        int.TryParse(wireName, out wireIdResult);
        string wireFullType = sql.GetWireType_WireID(wireIdResult);
        if (wireFullType == "")
        {
            result.IsPass = false;
            result.Message_Eng = "WireId := " + wireName + " was not found";
            result.Message_Thai = "ไม่พบข้อมูลของ Wire นี้";
            result.Handling = "กรุณาตรวจสอบข้อมุล Material ครับ";
            goto End;
        }
        string[] wireType = wireFullType.Split(' ');
        if (wireTypeLotNo.Substring(0,2).ToUpper() != wireType[0].ToUpper())
        {
            result.IsPass = false;
            result.Message_Eng = "wiretype not match";
            result.Message_Thai = "ชนิดของ wire ที่ใช้ไม่ตรงกับ Lot";
            result.Handling = "กรุณาตรวจสอบข้อมุล Material ครับ";
            goto End;
        }
        if (wireTypeLotNo.Substring(2, 2).ToUpper() != wireType[1].ToUpper())
        {
            result.IsPass = false;
            result.Message_Eng = "wiresize not match";
            result.Message_Thai = "ขนาดของ wire ที่ใช้ไม่ตรงกับ Lot";
            result.Handling = "กรุณาตรวจสอบข้อมุล Material ครับ";
            goto End;
        }
        End:
        return result;
    }

    #endregion
    #region PREFORM
    #endregion
    #region FRAME
    #endregion
    #endregion
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
    private string[] GetConfigFile(string mcNo)
    {
        string[] ret = null;
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
                            ret = tmpStrList;
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
        if (lotno == "")
        {
            SaveLogFile(mcno, "", "JigToolGetData", "", "", LogType.jig_material);
        }
        else
        {
            SaveLogFile(mcno, lotno, "JigToolGetData", "", "", LogType.jig_material);
        }
        
        List<JigDataInfo> jigAllData = JigGetApplyDataInfo(mcno);
        List<JigDataInfo> tmpResult = new List<JigDataInfo>();
        //// Get jig data here
        //// If LotNo != ""
        string[] configList = GetConfigFile(mcno);
        if (configList != null)
        {
            switch (mcno.Substring(0, 2))
            {
                case "WB":
                    string[] hp_pp = configList[1].Split(','); //HP
                    if (hp_pp[1] == "1")
                    {
                        foreach (JigDataInfo item in jigAllData)
                        {
                            if (item.Type == "HP" || item.Type == "PP")
                            {
                                tmpResult.Add(item);
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
        }
        if (tmpResult.Count>0)
        {
            result = new JigDataInfo[tmpResult.Count];
            int i = 0;
            foreach (JigDataInfo item in tmpResult)
            {
                result[i] = item;
                i += 1;
            }
        }
        return result;
        
    }
    private void ZeroControlAdjust(AfterLotEndEventArgs e)
    {
        //if (!e.LotData.PNashi.HasValue && !e.LotData.PNashi_Scrap.HasValue && !e.LotData.FrontNg.HasValue && !e.LotData.FrontNg_Scrap.HasValue
        //    && !e.LotData.Marker.HasValue && !e.LotData.Marker_Scrap.HasValue && !e.LotData.OS_Scrap.HasValue && !e.LotData.Qty_Scrap.HasValue)
        //{
        //return;
        //}
        //set default parameter
        if (!e.LotDataQuantity.PNashi.HasValue) e.LotDataQuantity.PNashi = 0;
        if (!e.LotDataQuantity.PNashi_Scrap.HasValue) e.LotDataQuantity.PNashi_Scrap = 0;
        if (!e.LotDataQuantity.FrontNg.HasValue) e.LotDataQuantity.FrontNg = 0;
        if (!e.LotDataQuantity.FrontNg_Scrap.HasValue) e.LotDataQuantity.FrontNg_Scrap = 0;
        if (!e.LotDataQuantity.Marker.HasValue) e.LotDataQuantity.Marker = 0;
        if (!e.LotDataQuantity.Marker_Scrap.HasValue) e.LotDataQuantity.Marker_Scrap = 0;
        if (!e.LotDataQuantity.OS_Scrap.HasValue) e.LotDataQuantity.OS_Scrap = 0;
        if (!e.LotDataQuantity.Qty_Scrap.HasValue) e.LotDataQuantity.Qty_Scrap = 0;

        //Get Data from current translot
        DatabaseSqlcommand sql = new DatabaseSqlcommand();
        DataTable currentLotDataTable = sql.GetCurrentTransLot(e.LotNo);
        int ngAdjust = 0; int goodAdjust = 0; int pNashi = 0;int frontNg = 0; int markerNg = 0; int qtyScrap = 0;
        int frameInput = 0; int framePass = 0; int frameFail = 0; int pcsPerFrame = 0; int frameScrap = 0;
        string[] mcnoSplit = e.McNo.Split('-');
        if (currentLotDataTable.Rows.Count > 0)
        {
            DataRow row = currentLotDataTable.Rows[0];
            string[] strSplit = row["Package"].ToString().Split('-');
            string pkg = row["Package"].ToString().Replace(" ", "");
            //if (strSplit[0] != "TO263")
            //{
            //    SaveLogFile(e.McNo, e.LotNo, "ZeroControlAdjust", "Fail", "Package:=" + row["Package"].ToString(), LogType.zero_control);
            //    return;
            //}

            //if (strSplit[0] == "TO263" || pkg == "HRP5" || pkg == "HRP7" || pkg == "TO252-5")
            //{

            //}
            //else
            //{
            //    SaveLogFile(mcnoSplit[0] + "," + e.McNo, e.LotNo, "ZeroControlAdjust", "Fail", "Package:=" + row["Package"].ToString(), LogType.zero_control);
            //    return;
            //}
            

            if (e.LotId == null)
            {
                int lotId = 0;
                int.TryParse(row["LotId"].ToString(), out lotId);
                e.LotId = lotId;
            }

            int.TryParse(row["Good"].ToString(), out goodAdjust);
            int.TryParse(row["NG"].ToString(), out ngAdjust);
            int.TryParse(row["PNashi"].ToString(), out pNashi);
            int.TryParse(row["FrontNg"].ToString(), out frontNg);
            int.TryParse(row["MarkerNg"].ToString(), out markerNg);
            int.TryParse(row["CutFrame"].ToString(), out qtyScrap);
            int.TryParse(row["PcsPerFrame"].ToString(), out pcsPerFrame);
            int.TryParse(row["FrameInput"].ToString(), out frameInput);
            int.TryParse(row["FramePass"].ToString(), out framePass);
            int.TryParse(row["FrameFail"].ToString(), out frameFail);
            if (framePass != 0)
            {
                frameScrap = (frameInput - (framePass + frameFail)) * pcsPerFrame;
            }
                        
            if (goodAdjust == 0)
            {
                SaveLogFile(e.McNo, e.LotNo, "ZeroControlAdjust", "Warning", "GoodBeforeProcess:=0", LogType.zero_control);
                int.TryParse(row["GoodStepSum"].ToString(), out goodAdjust);
                int.TryParse(row["NgStepSum"].ToString(), out ngAdjust);
                if (goodAdjust == 0)
                {
                    SaveLogFile(e.McNo, e.LotNo, "ZeroControlAdjust", "Warning", "GoodStepSum:=0", LogType.zero_control);
                    int.TryParse(row["Good"].ToString(), out goodAdjust);
                    int.TryParse(row["NG"].ToString(), out ngAdjust);
                    if (goodAdjust == 0)
                    {
                        SaveLogFile(mcnoSplit[0] + "," + e.McNo, e.LotNo, "ZeroControlAdjust", "Error", "Good:=0", LogType.zero_control);
                        return;
                    }
                }
            }
            
            SaveLogFile(mcnoSplit[0] + "," + e.McNo, e.LotNo, "ZeroControlAdjust", "-", "Good:=" + e.LotDataQuantity.GoodAdjust.ToString() + "[" + goodAdjust.ToString() + "]"
                + "|Ng:=" + e.LotDataQuantity.NgAdjust.ToString() + "[" + ngAdjust.ToString() + "]"
                + "|PNashi:=" + e.LotDataQuantity.PNashi.Value.ToString() + "[" + pNashi.ToString() + "]"
                + "|PNashi_Scrap:=" + e.LotDataQuantity.PNashi_Scrap.Value.ToString()
                + "|FrontNg:=" + e.LotDataQuantity.FrontNg.Value.ToString() + "[" + frontNg.ToString() + "]"
                + "|FrontNg_Scrap:=" + e.LotDataQuantity.FrontNg_Scrap.Value.ToString()
                + "|Marker:=" + e.LotDataQuantity.Marker.Value.ToString() + "[" + markerNg.ToString() + "]"
                + "|Marker_Scrap:=" + e.LotDataQuantity.Marker_Scrap.Value.ToString()
                + "|OS_Scrap:=" + e.LotDataQuantity.OS_Scrap.Value.ToString()
                + "|Qty_Scrap:=" + e.LotDataQuantity.Qty_Scrap.Value.ToString() + "[" + qtyScrap.ToString() + "]", LogType.zero_control);
        }
        else
        {
            SaveLogFile(mcnoSplit[0] + "," + e.McNo, e.LotNo, "ZeroControlAdjust", "Error", "Lot Not Found!", LogType.zero_control);
            return;
        }

        //update to lotProcessRecord
        //int currentNgAdjust = ngAdjust - (frontNg + markerNg + e.LotDataQuantity.FrontNg.Value + e.LotDataQuantity.Marker.Value + qtyScrap + e.LotDataQuantity.Qty_Scrap.Value);
        //if (currentNgAdjust < 0)
        //{
        //    SaveLogFile(e.McNo, e.LotNo, "ZeroControlAdjust", "Fail", "currentNgAdjust < 0", LogType.zero_control);
        //    currentNgAdjust = 0;
        //    //return;
        //}
        //e.LotDataQuantity.NgAdjust = ngAdjust - (frontNg + e.LotDataQuantity.FrontNg.Value + markerNg + e.LotDataQuantity.Marker.Value);
        //ngAdjust - (frontNg + e.LotDataQuantity.FrontNg.Value + markerNg + e.LotDataQuantity.Marker.Value);
        //if (e.LotDataQuantity.GoodAdjust == 0)
        //{
        //    DataRow row = currentLotDataTable.Rows[0];
        //    int tmpGood = 0;
        //    int.TryParse(row["GoodStepSum"].ToString(), out tmpGood);
        //    e.LotDataQuantity.GoodAdjust = tmpGood;
        //}
        //else
        //{
        int transGood = goodAdjust; // + e.LotDataQuantity.FrontNg_Scrap.Value + e.LotDataQuantity.OS_Scrap.Value + e.LotDataQuantity.Marker_Scrap.Value;
        int transNg = ngAdjust; // - e.LotDataQuantity.FrontNg_Scrap.Value - e.LotDataQuantity.OS_Scrap.Value - e.LotDataQuantity.Marker_Scrap.Value; 

        e.LotDataQuantity.NgAdjust = e.LotDataQuantity.NgAdjust - (e.LotDataQuantity.FrontNg.Value + e.LotDataQuantity.Marker.Value 
            + e.LotDataQuantity.FrontNg_Scrap.Value + e.LotDataQuantity.Marker_Scrap.Value + e.LotDataQuantity.OS_Scrap.Value);
        if (e.LotDataQuantity.NgAdjust < 0)
        {
            SaveLogFile(mcnoSplit[0] + "," + e.McNo, e.LotNo, "ZeroControlAdjust", "Error", "NgAdjust:=" + e.LotDataQuantity.NgAdjust.ToString(),LogType.zero_control);
            e.LotDataQuantity.NgAdjust = 0;
        }

        e.LotDataQuantity.GoodAdjust = goodAdjust + (frontNg + e.LotDataQuantity.FrontNg.Value)
            + (markerNg + e.LotDataQuantity.Marker.Value);  // (frontNg + markerNg + e.LotDataQuantity.FrontNg.Value + e.LotDataQuantity.Marker.Value); //goodAdjust + (frontNg + markerNg + e.LotDataQuantity.FrontNg.Value + e.LotDataQuantity.Marker.Value);
            //}
        
        e.LotDataQuantity.FrontNg = frontNg + e.LotDataQuantity.FrontNg.Value;// - e.LotDataQuantity.FrontNg_Scrap.Value;
        e.LotDataQuantity.Marker = markerNg + e.LotDataQuantity.Marker.Value;// - e.LotDataQuantity.Marker_Scrap.Value;
        e.LotDataQuantity.PNashi = pNashi + e.LotDataQuantity.PNashi.Value;// - e.LotDataQuantity.PNashi_Scrap.Value;
        e.LotDataQuantity.Qty_Scrap = qtyScrap + e.LotDataQuantity.Qty_Scrap.Value;
                
        int transFrontNg = e.LotDataQuantity.FrontNg.Value - e.LotDataQuantity.FrontNg_Scrap.Value;
        int transMarkerNg = e.LotDataQuantity.Marker.Value - e.LotDataQuantity.Marker_Scrap.Value;
        int transPNashi = e.LotDataQuantity.PNashi.Value - e.LotDataQuantity.PNashi_Scrap.Value;
        if (e.LotDataQuantity.OS_Scrap.Value > 0)
        {
            int os_scrap = e.LotDataQuantity.OS_Scrap.Value;
            e.LotDataQuantity.GoodAdjust = e.LotDataQuantity.GoodAdjust + pNashi;
            if ((e.LotDataQuantity.OS_Scrap.Value - e.LotDataQuantity.PNashi.Value) > 0)
            {
                e.LotDataQuantity.OS_Scrap = e.LotDataQuantity.OS_Scrap.Value - e.LotDataQuantity.PNashi.Value;
                transGood = transGood + e.LotDataQuantity.PNashi.Value;
                transNg = transNg - e.LotDataQuantity.PNashi.Value;
                if ((e.LotDataQuantity.FrontNg.Value - e.LotDataQuantity.OS_Scrap.Value) > 0)
                {
                    transFrontNg = e.LotDataQuantity.FrontNg.Value - e.LotDataQuantity.OS_Scrap.Value;
                    transGood = transGood + e.LotDataQuantity.OS_Scrap.Value;
                    transNg = transNg - e.LotDataQuantity.OS_Scrap.Value;
                }
                else
                {
                    transGood = transGood + e.LotDataQuantity.FrontNg.Value;
                    transNg = transNg - e.LotDataQuantity.FrontNg.Value;
                    transFrontNg = 0;
                }
                transPNashi = 0;
                e.LotDataQuantity.FrontNg = e.LotDataQuantity.OS_Scrap;
            }
            else
            {
                transGood = transGood + e.LotDataQuantity.OS_Scrap.Value;
                transNg = transNg - e.LotDataQuantity.OS_Scrap.Value;
                transPNashi = e.LotDataQuantity.PNashi.Value - e.LotDataQuantity.OS_Scrap.Value;
                e.LotDataQuantity.PNashi = e.LotDataQuantity.OS_Scrap;
            }
        }
        if (e.LotDataQuantity.Marker_Scrap.Value > 0)
        {
            if ((e.LotDataQuantity.Marker_Scrap.Value - e.LotDataQuantity.Marker.Value) > 0)
            {
                //e.LotDataQuantity.Marker = 0;
                transMarkerNg = 0;
                transGood = transGood + e.LotDataQuantity.Marker.Value;
                transNg = transNg - e.LotDataQuantity.Marker.Value;
            }
            else
            {
                transMarkerNg = e.LotDataQuantity.Marker.Value - e.LotDataQuantity.Marker_Scrap.Value;
                transGood = transGood + e.LotDataQuantity.Marker_Scrap.Value;
                transNg = transNg - e.LotDataQuantity.Marker_Scrap.Value;
            }
        }
        if (e.LotDataQuantity.FrontNg_Scrap.Value > 0)
        {
            if ((e.LotDataQuantity.FrontNg_Scrap.Value - e.LotDataQuantity.FrontNg.Value) > 0)
            {
                transFrontNg = 0;
                transGood = transGood + e.LotDataQuantity.FrontNg.Value;
                transNg = transNg - e.LotDataQuantity.FrontNg.Value;
            }
            else
            {
                transFrontNg = e.LotDataQuantity.FrontNg.Value - e.LotDataQuantity.FrontNg_Scrap.Value;
                transGood = transGood + e.LotDataQuantity.FrontNg_Scrap.Value;
                transNg = transNg - e.LotDataQuantity.FrontNg_Scrap.Value;
            }
        }
        DataTable result = sql.SetLotProcessRecord(e, transFrontNg, transMarkerNg, transPNashi, transGood, transNg);
        if (result.Rows.Count > 0)
        {
            DataRow row = result.Rows[0];
            if (row["Is_Pass"].ToString() == "FALSE")
            {
                SaveLogFile(mcnoSplit[0] + "," + e.McNo, e.LotNo, "SetLotProcessRecord", "Fail", "Error:=" + row["Error_Message_THA"].ToString(), LogType.zero_control);
                return;
            }
        }


    }
}
