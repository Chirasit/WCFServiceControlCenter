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

    
}