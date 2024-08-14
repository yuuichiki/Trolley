using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RCabinet.Helpers
{
public class SqlHelper
{
	private static readonly int COMMAND_TIMEOUT = 0;


        public static string quotedStr(string value)
        {
            return "'" + value + "'";
        }
        public static string getSQLConnection()
		{
            //read connectionString from AppConfig
			string config = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            // ETS_CONFIG eTSConfig = funcs.getETSConfig();
            //SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder("Server=" + eTSConfig.serverName + ";DataBase=" + eTSConfig.DataBase + ";uid=" + eTSConfig.userName + ";pwd=" + Des.DecryStrHex(eTSConfig.passWord, "I_LoVe_YJG") + ";");
            //return sqlConnectionStringBuilder.ToString();
            return config;
        }

        public static string getSHAConnection()
        {
            string config = ConfigurationManager.ConnectionStrings["SHAConnectionString"].ConnectionString;
            return config;
        }

		public static DataSet getDataSet(string asql,string type)
		{ 
		DataSet dataSet = new DataSet();
			if(type=="ets")
            {
                using (SqlConnection sqlConnection = new SqlConnection(getSQLConnection()))
                {
                    sqlConnection.Open();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(asql, sqlConnection);
                    sqlDataAdapter.SelectCommand.CommandTimeout = COMMAND_TIMEOUT;
                    sqlDataAdapter.Fill(dataSet);
                }
            }
            else
            {
                using (SqlConnection sqlConnection = new SqlConnection(getSHAConnection()))
                {
                    sqlConnection.Open();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(asql, sqlConnection);
                    sqlDataAdapter.SelectCommand.CommandTimeout = COMMAND_TIMEOUT;
                    sqlDataAdapter.Fill(dataSet);
                }
            }
		return dataSet;
	}

	public static DataTable getTable(string asql)
	{
		DataTable dataTable = new DataTable();
		using (SqlConnection sqlConnection = new SqlConnection(getSQLConnection()))
		{
			sqlConnection.Open();
			SqlCommand sqlCommand = new SqlCommand(asql, sqlConnection);
			sqlCommand.CommandTimeout = COMMAND_TIMEOUT;
			SqlDataReader reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
			dataTable.Load(reader);
		}
		return dataTable;
	}

	public static void execSQL(string ASQL)
	{
		SqlConnection sqlConnection = new SqlConnection(getSQLConnection());
		try
		{
			sqlConnection.Open();
			SqlCommand sqlCommand = new SqlCommand(ASQL, sqlConnection);
			sqlCommand.CommandTimeout = COMMAND_TIMEOUT;
			sqlCommand.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			sqlConnection.Close();
			throw ex;
		}
	}

	public static string SQLResult(string sql)
	{
		string result = string.Empty;
		DataTable table = getTable(sql);
		if (table.Rows.Count > 0)
		{
			result = table.Rows[0][0].ToString();
		}
		return result;
	}

	public static SqlDataReader spResult(string spName, SqlParameter[] spParams)
	{
		SqlConnection sqlConnection = new SqlConnection(getSQLConnection());
		try
		{
			sqlConnection.Open();
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.Connection = sqlConnection;
			sqlCommand.CommandType = CommandType.StoredProcedure;
			sqlCommand.CommandText = spName;
			sqlCommand.CommandTimeout = COMMAND_TIMEOUT;
			sqlCommand.Parameters.AddRange(spParams);
			return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
		}
		catch (Exception ex)
		{
			sqlConnection.Close();
			throw ex;
		}
	}

	public static DataSet getStoreProcedure(string spName, SqlParameter[] spParapms)
	{
		DataSet dataSet = new DataSet();
		using (SqlConnection sqlConnection = new SqlConnection(getSQLConnection()))
		{
			try
			{
				sqlConnection.Open();
				SqlCommand sqlCommand = new SqlCommand(spName, sqlConnection);
				sqlCommand.CommandType = CommandType.StoredProcedure;
				sqlCommand.CommandTimeout = COMMAND_TIMEOUT;
				sqlCommand.Parameters.AddRange(spParapms);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
				sqlDataAdapter.Fill(dataSet);
			}
			catch (Exception ex)
			{
				sqlConnection.Close();
				throw ex;
			}
		}
		return dataSet;
	}

	public static SqlDataReader ExecuteReader(string ASQL, CommandType cmdtype)
	{
		SqlConnection sqlConnection = new SqlConnection(getSQLConnection());
		try
		{
			sqlConnection.Open();
			SqlCommand sqlCommand = new SqlCommand(ASQL, sqlConnection);
			sqlCommand.CommandType = cmdtype;
			sqlCommand.CommandTimeout = COMMAND_TIMEOUT;
			return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
		}
		catch (Exception ex)
		{
			sqlConnection.Close();
			throw ex;
		}
	}

	public static IList<string> getValueList(string ASQL)
	{
		IList<string> list = new List<string>();
		DataTable table = getTable(ASQL);
		foreach (DataRow row in table.Rows)
		{
			list.Add(row[0].ToString());
		}
		return list;
	}
}

}