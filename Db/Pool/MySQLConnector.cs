using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;



namespace Imprint.Db.Pool
{
    /// <summary>
    /// mysql连接池连接器
    /// </summary>
    public class MySQLConnector : IConnector<MySqlConnection>
    {
        public MySqlConnection Connect(object param)
        {
            var conn = new MySqlConnection(param as string);
            do
            {
                try
                {
                    conn.Open();
                }
                catch (Exception e)
                {

                }
            } while (conn.State == ConnectionState.Closed);
            return conn;
        }

        public void Disconnect(MySqlConnection conn)
        {
            conn.Close();
            conn.Dispose();
        }

        public bool IsConnected(MySqlConnection connection)
        {
            return connection.State >= ConnectionState.Open
                &&
                connection.State != ConnectionState.Broken;
        }

        public void Reset(MySqlConnection connection)
        {
            // do nothing
        }
    }
}