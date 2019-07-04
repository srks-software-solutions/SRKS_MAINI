using System;
using System.Data.SqlClient;

namespace SRKSDemo
{
    public  class MsqlConnection : IDisposable
    {
        //Server
        //static string ServerName = @"PLM";//@"rmlabkm12400\ranesqlexp17ifac";
        //static string username = "srkssa";
        //static string password = "srks4$maini";
        //static string DB = "unitworksccs";


        //local host
        static string ServerName = @"TCP:DESKTOP-M96NU10\SQLEXPRESS,7015";
        static string username = "sa";
        static string password = "srks4$";
        static string DB = "unitworksccs";

        public SqlConnection msqlConnection = new SqlConnection(@"Data Source = " + ServerName + ";User ID = " + username + ";Password = " + password + ";Initial Catalog = " + DB + ";Persist Security Info=True");


        public void open()
        {
            if (msqlConnection.State != System.Data.ConnectionState.Open)
                msqlConnection.Open();
        }

        public void close()
        {
            msqlConnection.Close();
        }

        public void Dispose()
        { }
    }
}