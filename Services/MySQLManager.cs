using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using System.Collections.Generic;

namespace VisionProSortPlatform.Services
{
    /// <summary>
    /// MySQL 数据库通用访问层：封装连接字符串管理与两类基础操作（增删改 / 查询）。
    /// 全部方法为异步，调用方通过 await 避免 UI 线程阻塞。
    /// 使用 MySqlConnector 官方驱动，参数化查询防止 SQL 注入。
    /// </summary>
    public static class MySQLManager
    {
        // ---------- 连接参数----------
        private static  string Server   = "127.0.0.1";
        private static  string Port     = "3306";
        private static  string Database = "test";  
        private static  string Uid      = "root";
        private static  string Pwd      = "admin";
        private static  string Charset  = "utf8";


        // 用界面输入的参数覆盖默认连接参数
        public static void SetConnection(string server, string port, string database, string uid, string pwd, string charset)
        {
            Server = server;
            Port = port;
            Database = database;
            Uid = uid;
            Pwd = pwd;
            Charset = charset;
        }

        /// <summary>连接字符串：由上述参数动态拼接，每次访问重新生成。</summary>
        private static string ConnectionString =>
            $"Server={Server};Port={Port};Database={Database};Uid={Uid};Pwd={Pwd};Charset={Charset};";

        // 读取当前数据库里的所有表名
        public static async Task<List<string>> GetTablesAsync()
        {
            var tables = new List<string>();
            DataTable dt = await ExecuteQueryAsync("SHOW TABLES");
            foreach (DataRow row in dt.Rows)
            {
                tables.Add(row[0].ToString());
            }
            return tables;
        }

        // 读取指定表的字段名列表
        public static async Task<List<string>> GetColumnsAsync(string tableName)
        {
            var columns = new List<string>();
            DataTable dt = await ExecuteQueryAsync($"SELECT * FROM `{tableName}` LIMIT 0");
            foreach (DataColumn col in dt.Columns)
            {
                columns.Add(col.ColumnName);
            }
            return columns;
        }


        /// <summary>
        /// 用指定参数尝试建立连接，成功返回 true，失败返回 false。
        /// </summary>
        public static async Task<bool> TestConnectionAsync(string server, string port, string database, string uid, string pwd, string charset)
        {
            string connStr = $"Server={server};Port={port};Database={database};Uid={uid};Pwd={pwd};Charset={charset};";
            try
            {
                using (var conn = new MySqlConnection(connStr))
                { 
                    await conn.OpenAsync();
                    return true;                
                }

            }
            catch{ return false; }
        }



        /// <summary>
        /// 异步执行非查询类SQL命令 (INSERT / UPDATE / DELETE)
        /// using 块保证连接与命令对象在方法结束时自动释放，无需手动 Close。
        /// </summary>
        /// <param name="sql">带参数占位符的 SQL 语句</param>
        /// <param name="parameters">SQL 参数数组，与占位符一一对应</param>
        /// <returns>受影响的行数</returns>
        public static async Task<int> ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// 通用查询方法（SELECT），结果以 DataTable 形式返回。
        /// 使用 MySqlDataAdapter 自动填充，无需手动遍历 DataReader。
        /// </summary>
        /// <param name="sql">带参数占位符的 SELECT 语句</param>
        /// <param name="parameters">SQL 参数数组</param>
        /// <returns>填充后的查询结果表</returns>
        public static async Task<DataTable> ExecuteQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    await conn.OpenAsync();
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }
}