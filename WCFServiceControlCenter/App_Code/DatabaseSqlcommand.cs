using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DatabaseSqlcommand
/// </summary>
public class DatabaseSqlcommand
{
    public DatabaseSqlcommand()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public string GetWireType_LotNo(string lotNo)
    {
        string ret = "";
        DataTable dt = new DataTable();
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT TOP 1 [LOT_NO_1] ,[MANU_COND_GOLD_LINE]  FROM [APCSDB].[dbo].[LCQW_UNION_WORK_DENPYO_PRINT]  WHERE LOT_NO_1 = @LotNo";
            cmd.Parameters.Add("@LotNo", SqlDbType.VarChar).Value = lotNo;
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
        if (dt.Rows.Count>0)
        {
            DataRow row = dt.Rows[0];
            ret = row["MANU_COND_GOLD_LINE"].ToString();
        }
        return ret;
    }
    public string GetWireType_WireID(int id)
    {
        string ret = "";
        DataTable dt = new DataTable();
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT ID, MaterialItem, MaterialModel, CreateDate, QRCode, MakerLotNo, PONo, StockTaking, SEQ_NO FROM dbx.MAT.StockItem WHERE (ID = @ID)";
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            cmd.Connection.Open();
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                if (rd.HasRows)
                {
                    dt.Load(rd);

                }
            }
            cmd.Connection.Close();
            if (dt.Rows.Count>0)
            {
                DataRow row = dt.Rows[0];
                ret = row["MaterialModel"].ToString();
            }
        }
        return ret;
    }
    public DataTable GetCurrentTransLot(string lotNo)
    {
        DataTable dataTable = new DataTable();
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[cellcon].[sp_get_current_trans_lots]";
            cmd.Parameters.Add("@lot_no", System.Data.SqlDbType.VarChar).Value = lotNo;
            cmd.Connection.Open();
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                if (rd.HasRows)
                {
                    dataTable.Load(rd);

                }
            }
            cmd.Connection.Close();

        }
        return dataTable;
    }
    public DataTable SetLotProcessRecord(AfterLotEndEventArgs e,int transFrontNg,int transMarkerNg,int transPNashi,int transGood,int transNg)
    {
        DataTable dataTable = new DataTable();
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBxConnectionString"].ToString());
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[StoredProcedureDB].[cellcon].[sp_set_lot_process_records]";
            cmd.Parameters.Add("@lot_id", System.Data.SqlDbType.Int).Value = e.LotId;
            cmd.Parameters.Add("@lot_no", System.Data.SqlDbType.VarChar).Value = e.LotNo;
            cmd.Parameters.Add("@p_nashi", System.Data.SqlDbType.Int).Value = e.LotDataQuantity.PNashi.Value;
            cmd.Parameters.Add("@frontng", System.Data.SqlDbType.Int).Value = e.LotDataQuantity.FrontNg.Value;
            cmd.Parameters.Add("@marker", System.Data.SqlDbType.Int).Value = e.LotDataQuantity.Marker.Value;
            cmd.Parameters.Add("@ngadjust", System.Data.SqlDbType.Int).Value = e.LotDataQuantity.NgAdjust;
            cmd.Parameters.Add("@job_id", System.Data.SqlDbType.Int).Value = e.JobId;
            cmd.Parameters.Add("@good", System.Data.SqlDbType.Int).Value = e.LotDataQuantity.GoodAdjust;
            cmd.Parameters.Add("@cut_frame", System.Data.SqlDbType.Int).Value = e.LotDataQuantity.Qty_Scrap;
            cmd.Parameters.Add("@t_frontng", System.Data.SqlDbType.Int).Value = transFrontNg;
            cmd.Parameters.Add("@t_marker", System.Data.SqlDbType.Int).Value = transMarkerNg;
            cmd.Parameters.Add("@t_p_nashi", System.Data.SqlDbType.Int).Value = transPNashi;
            cmd.Parameters.Add("@t_good", System.Data.SqlDbType.Int).Value = transGood;
            cmd.Parameters.Add("@t_ng", System.Data.SqlDbType.Int).Value = transNg;
            cmd.Connection.Open();
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                if (rd.HasRows)
                {
                    dataTable.Load(rd);
                }
            }
            cmd.Connection.Close();
        }
        return dataTable;
    }
}