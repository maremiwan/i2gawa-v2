using Dapper;
using i2gawa.Core.DataContract.Response;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace i2gawa.Core.Util.DbAccess
{
    public class Db
    {
        static DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        public string connectionString { get; set; }
        private readonly IConfiguration configuration;
        public Db(string conn = null)
        {
            if (conn == null)
                //connectionString = "Server=202.157.184.27;Database=i2gawa;User Id=sa;Password=P@ssw0rd;";
                //connectionString = "Server=DESKTOP-TS6OBGD\\SQLSERVER2019;Database=i2gawa;User Id=sa;Password=123Aa;";
                connectionString = "Server=HARIS\\SQL2019;Database=i2gawa;User Id=sa;Password=fid123!!;";
            else
                connectionString = conn;
        }
        //public Db(IConfiguration configuration)
        //{
        //    this.configuration = configuration;
        //}


        // fast read and instantiate (i.e. make) a collection of objects

        public async IAsyncEnumerable<T> ReadAsyn<T>(string sql, CommandType commandType, Func<IDataReader, T> make, params object[] parms)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, commandType, parms))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            yield return make(reader);
                        }
                    }
                }
            }
        }
        public IEnumerable<T> ReadSync<T>(string sql, CommandType commandType, Func<IDataReader, T> make, params object[] parms)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, commandType, parms))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return make(reader);
                        }
                    }
                }
            }
        }    
        // return a scalar object

        public object Scalar(string sql, CommandType commandType, params object[] parms)
        {

            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, commandType, parms))
                {
                    return command.ExecuteScalar();
                }
            }

        }

        #region INSERT

        public async Task<int> Insert(string sql, CommandType commandType, params object[] parms)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(sql + ";SELECT SCOPE_IDENTITY();", connection, commandType, parms))
                    {
                        return await Task.FromResult(int.Parse(command.ExecuteScalar().ToString()));
                    }
                }
            }
            catch (Exception ex) { throw new DbException(sql, parms, ex); }
        }

        public async Task<int> InsertWithNoIdReturn(string sql, CommandType commandType, params object[] parms)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(sql, connection, commandType, parms))
                    {
                        return await Task.FromResult(command.ExecuteNonQuery());
                    }
                }
            }
            catch (Exception ex) { throw new DbException(sql, parms, ex); }

        }

        public async Task<i2gawaExecuteResult> InsertWithEntity(string sql, params object[] entity)
        {
            i2gawaExecuteResult res = new i2gawaExecuteResult();
            try
            {                
                using (var connection = CreateConnection())
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    var result = await connection.ExecuteAsync(sql, entity);
                    if(result > 0)
                    {
                        res.Success = true;
                        res.Message = "Success";
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Message = ex.Message;
                return res;
            }
        }

        public async Task<int> Insert(string sql, params object[] entity)
        {
            int result = 0;
            try
            {
                using (var connection = CreateConnection())
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    result = await connection.ExecuteAsync(sql, entity);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        // update an existing record

        public async Task<int> Update(string sql, CommandType commandType, params object[] parms)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(sql, connection, commandType, parms))
                    {
                        return await Task.FromResult(command.ExecuteNonQuery());
                    }
                }
            }
            catch (Exception ex) { throw new DbException(sql, parms, ex); }
        }

        public async Task<i2gawaExecuteResult> UpdateWithEntity(string sql, params object[] entity)
        {
            i2gawaExecuteResult res = new i2gawaExecuteResult();
            try
            {
                using (var connection = CreateConnection())
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    var result = await connection.ExecuteAsync(sql, entity);
                    if (result > 0)
                    {
                        res.Success = true;
                        res.Message = "Success";                        
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Message = ex.Message;
                return res;
            }
        }
        public async Task<int> Update(string sql, params object[] entity)
        {
            int result = 0;
            try
            {
                using (var connection = CreateConnection())
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    result = await connection.ExecuteAsync(sql, entity);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Task<int> ExecuteCommand(string spQuery, CommandType commandType, params object[] parms)
        {

            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(spQuery, connection, commandType, parms))
                {
                    return command.ExecuteNonQueryAsync();
                }
            }
        }
        // delete a record

        public Task<int> Delete(string sql, CommandType commandType, params object[] parms)
        {
            return Update(sql, commandType, parms);
        }


        public string ExecuteQuerySingle(string sql, CommandType commandType, params object[] parms)
        {
            string result = "";
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(sql, connection, commandType, parms))
                    {

                        CreateAdapter(command);
                    }
                }

            }
            catch { }
            return result;
        }
        // creates a connection object
        public DbConnection CreateConnection()
        {
            // ** Factory pattern in action

            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            //connection.ConnectionString = configuration.GetConnectionString("SqlConnection");
            connection.Open();
            return connection;
        }

        // creates a command object

        public DbCommand CreateCommand(string sql, DbConnection conn, CommandType commandType, params object[] parms)
        {
            // ** Factory pattern in action

            var command = factory.CreateCommand();
            command.Connection = conn;
            command.CommandType = commandType;
            command.CommandText = sql;
            command.AddParameters(parms);
            return command;
        }

        // creates an adapter object

        DbDataAdapter CreateAdapter(DbCommand command)
        {
            // ** Factory pattern in action

            var adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = command;
            return adapter;
        }
        //public DbTransaction BeginTransaction()
        //{
        //    _db = DatabaseFactory.CreateDatabase();
        //    _connection = _db.CreateConnection();
        //    _connection.Open();
        //    _transaction = _connection.BeginTransaction();
        //    return _transaction;
        //}
    }
    public static class DbExtentions
    {
        // adds parameters to a command object

        public static void AddParameters(this DbCommand command, object[] parms)
        {
            if (parms != null && parms.Length > 0)
            {

                // ** Iterator pattern
                // NOTE: processes a name/value pair at each iteration

                for (int i = 0; i < parms.Length; i += 2)
                {
                    string name = parms[i].ToString();

                    // no empty strings to the database

                    if (parms[i + 1] is string && (string)parms[i + 1] == "")
                        parms[i + 1] = null;

                    // if null, set to DbNull

                    object value = parms[i + 1] ?? DBNull.Value;

                    // ** Factory pattern

                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = name;
                    dbParameter.Value = value;

                    command.Parameters.Add(dbParameter);
                }
            }
        }
    }
    public class DbException : Exception
    {
        public DbException()
        {
        }
        public DbException(string message)
            : base("In Db: " + message)
        {
        }

        public DbException(string message, Exception innerException)
            : base("In Db: " + message, innerException)
        {
        }

        public DbException(string sql, object[] parms, Exception innerException)
            : base("In Db: " + string.Format("Sql: {0}  Parms: {1}", (sql ?? "--"),
                    (parms != null ? string.Join(",", Array.ConvertAll<object, string>(parms, o => (o ?? "null").ToString())) : "--")),
            innerException)
        {
        }
    }
}
