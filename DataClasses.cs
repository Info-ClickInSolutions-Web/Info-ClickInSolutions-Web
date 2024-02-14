using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace WebAPI
{
    public static class SqlParameterCollectionHelper
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        public static void AddNewParm<T>(this List<SqlParameter> obj, string Name, T Value)
        {
            if (Value != null)
            {
                obj.Add(new SqlParameter("@" + Name, Value));
            }
            else
            {
                obj.Add(new SqlParameter("@" + Name, DBNull.Value));
            }
        }

        public static void AddNewParm<T>(this List<SqlParameter> obj, string Name, T Value, ParameterDirection parameterDirection)
        {
            if (Value != null)
            {
                SqlParameter p = new SqlParameter("@" + Name, Value);
                p.Direction = parameterDirection;
                obj.Add(p);
            }
            else
            {
                SqlParameter p = new SqlParameter("@" + Name, DBNull.Value);
                p.Direction = parameterDirection;
                obj.Add(p);
            }
        }
    }

    public class DataClasses
    {
        public DataClasses() { }

        public static JArray DataTableToJson(DataTable source)
        {
            JArray result = new JArray();
            JObject row;
            foreach (System.Data.DataRow dr in source.Rows)
            {
                row = new JObject();
                foreach (System.Data.DataColumn col in source.Columns)
                {
                    row.Add(col.ColumnName.Trim(), JToken.FromObject(dr[col]));
                }
                result.Add(row);
            }
            return result;
        }

        public DataTable GetDataTable(string SQL, CommandType commandType, string TableName, List<SqlParameter> sqlParameterCollection, string ConnectionString)
        {
            SqlConnection con = new SqlConnection();

            con.ConnectionString = ConnectionString;


            DataTable dt = new DataTable();
            dt.TableName = TableName;
            try
            {
                SqlCommand cmd = new SqlCommand();

                cmd.Connection = con;
                cmd.CommandText = SQL;

                cmd.CommandType = commandType;
                cmd.Parameters.Clear();
                cmd.CommandTimeout = 30000;

                if (sqlParameterCollection != null)
                {
                    foreach (SqlParameter sqlPara in sqlParameterCollection)
                    {
                        sqlPara.Value = sqlPara.Value.ToString().Replace("'", "''");
                    }
                    cmd.Parameters.AddRange(sqlParameterCollection.ToArray());
                }

                con.Open();


                dt.Load(cmd.ExecuteReader());

                con.Close();
                cmd.Dispose();
                cmd = null;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (con.State != ConnectionState.Closed)
                {
                    con.Close();
                }
                con.Dispose();
                con = null;
            }
            return dt;
        }

        public int JustExecute(string SQL, CommandType commandType, List<SqlParameter> sqlParameterCollection, string ConnectionString)
        {
            int i = 0;
            SqlConnection con = new SqlConnection();

            con.ConnectionString = ConnectionString;
            try
            {
                SqlCommand cmd = new SqlCommand();

                cmd.Connection = con;
                cmd.CommandText = SQL;

                cmd.CommandType = commandType;
                cmd.Parameters.Clear();

                if (sqlParameterCollection != null)
                {
                    cmd.Parameters.AddRange(sqlParameterCollection.ToArray());
                }

                con.Open();


                i = cmd.ExecuteNonQuery();

                con.Close();
                cmd.Dispose();
                cmd = null;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (con.State != ConnectionState.Closed)
                {
                    con.Close();
                }
                con.Dispose();
                con = null;
            }

            return i;
        }
        public DataSet GetDataSet(string strSQL, List<SqlParameter> lstSQLParameterCollection, string strConnectionString)
        {
            SqlConnection objSqlConnection = new SqlConnection();
            objSqlConnection.ConnectionString = strConnectionString;

            DataSet objDataSet = new DataSet("TempDataSet");
            using (SqlConnection objConnection = new SqlConnection(strConnectionString))
            {
                SqlCommand objSqlCommand = new SqlCommand(strSQL, objConnection);

                if (lstSQLParameterCollection != null)
                    objSqlCommand.Parameters.AddRange(lstSQLParameterCollection.ToArray());

                objSqlCommand.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter objSqlDataAdapter = new SqlDataAdapter();
                objSqlDataAdapter.SelectCommand = objSqlCommand;
                objSqlDataAdapter.Fill(objDataSet);
            }
            return objDataSet;
        }

        public static string GetMultiResultJSON(DataSet objDataSet)
        {
            string strJSON = "[";
            try
            {
                foreach (DataTable objTable in objDataSet.Tables)
                {
                    strJSON += JsonConvert.SerializeObject(objTable);
                    strJSON += ",";
                }
                strJSON = strJSON.TrimEnd(',');
                strJSON += "]";
            }
            catch (Exception objException)
            {
                throw objException;
            }
            return strJSON;
        }
    }


}
