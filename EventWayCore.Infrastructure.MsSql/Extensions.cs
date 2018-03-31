using System.Data.SqlClient;

namespace EventWayCore.Infrastructure.MsSql
{
    public static class Extensions
    {
        public static SqlConnection AsOpen(this SqlConnection conn)
        {
            conn.Open();
            return conn;
        }
    }
}
