using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SetupOptimaToolkit.Models
{
    public class SqlHelper
    {
        SqlConnection cn;
        public SqlHelper(string connectionString)
        {
            cn = new SqlConnection(connectionString);
        }

        public bool IsConnection
        {
            get
            {
                if (cn.State == System.Data.ConnectionState.Closed)
                { cn.Open();

                    cn.Close();
                    cn.Dispose();
                }
                return true;
            }
        }

        public List<DatabaseModel> GetDatabases(string connectionString)
        {
            List<DatabaseModel> results = new List<DatabaseModel>();
            SqlConnection sqlcon = null;
            try
            {
               
                sqlcon = new SqlConnection(connectionString);
                sqlcon.Open();
                string query = "Select Baz_BazID, Baz_Nazwa, Baz_NazwaBazy from CDN.Bazy; ";
               
                SqlCommand sqlComm = new SqlCommand(query, sqlcon);
                sqlComm.CommandType = CommandType.Text;
                sqlComm.CommandTimeout = 30000;

                using (SqlDataReader reader = sqlComm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new DatabaseModel()
                        {
                            Id = int.Parse(reader["Baz_BazID"].ToString()),
                            Name = reader["Baz_Nazwa"].ToString(),
                            DbName = reader["Baz_NazwaBazy"].ToString()

                        });
                      
                    }
                }
                sqlcon.Close();
                sqlcon.Dispose();
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
            return results;
        }
        
    }
    public class DatabaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DbName { get; set; }
    }
}
