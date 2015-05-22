//
// SQLinfo - (C) 2015 Patrick Lambert - http://dendory.net
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Management;
using System.Data.SqlClient;
using System.Data;

[assembly: AssemblyTitle("SQLinfo")]
[assembly: AssemblyCopyright("(C) 2015 Patrick Lambert")]
[assembly: AssemblyFileVersion("0.1.0.0")]

namespace SQLinfo
{
	public class Program
	{
		static void Main(string[] args)
		{
			string server = "127.0.0.1";
			string dbname = "master";
			string userid = "sa";
			string passwd = "";
			string tmp = "";
			Int32 timeout = 10;
			SqlDataReader res;
			SqlCommand sql;
			SqlConnection con;
			
			// Parse parameters
			for(int i=0; i < args.Length; i++)
			{
				switch(args[i].ToLower())
				{
					case "-server":
						server = args[++i];
						break;
					case "-dbname":
						dbname = args[++i];
						break;
					case "-userid":
						userid = args[++i];
						break;
					case "-passwd":
						passwd = args[++i];
						break;
					case "-timeout":
						timeout = Int32.Parse(args[++i]);
						break;
					default:
						Console.WriteLine("* Unknown option: " + args[i]);
						break;
				}
			}
			
			// Connect to database server
			Console.WriteLine("* Connecting to [" + server.ToUpper() + ":" + dbname + "]");
			con = new SqlConnection("Server=" + server + "; Database=" + dbname + "; User Id=" + userid + "; Password=" + passwd + "; connection timeout=" + timeout);
			try
			{
				con.Open();
			}
			catch(Exception e)
			{
				Console.WriteLine("* Error connecting to server: " + e.Message);
				Environment.Exit(1);
			}

			// server version
			sql = new SqlCommand();
			sql.Connection = con;
			sql.CommandType = CommandType.Text;
			sql.CommandText = "SELECT @@version";
			res = sql.ExecuteReader();
			Console.Write("\n* Server version:\n");
			while(res.Read())
			{
				Console.WriteLine(res[0]);
			}
			res.Close();
			
			// List databases
			sql = new SqlCommand();
			sql.Connection = con;
			sql.CommandType = CommandType.StoredProcedure;
			sql.CommandText = "sp_databases";
			res = sql.ExecuteReader();
			Console.WriteLine("\n* Server database names:");
			while(res.Read())
			{
				Console.Write(res.GetString(0) + " ");
			}
			Console.Write("\n");
			res.Close();

			// List logins
			sql = new SqlCommand();
			sql.Connection = con;
			sql.CommandType = CommandType.Text;
			sql.CommandText = "SELECT name,type,default_database_name FROM sys.server_principals";
			res = sql.ExecuteReader();
			Console.Write("\n* Server logins:\n");
			Console.WriteLine(String.Format("{0,-44} {1,-8} {2,-20}", "Login name", "Type", "Default database"));
			Console.WriteLine(String.Format("{0,-44} {1,-8} {2,-20}", "----------", "----", "----------------"));
			while(res.Read())
			{
				switch(res[1].ToString())
				{
					case "S":
						tmp = "SQL";
						break;
					case "U":
						tmp = "Windows";
						break;
					case "G":
						tmp = "Group";
						break;
					case "R":
						tmp = "Role";
						break;
					default:
						tmp = "Mapped";
						break;
				}
				Console.WriteLine(String.Format("{0,-44} {1,-8} {2,-20}", res[0], tmp, res[2]));
			}
			res.Close();

			// Log info
			sql = new SqlCommand();
			sql.Connection = con;
			sql.CommandType = CommandType.Text;
			sql.CommandText = "DBCC SQLPERF(logspace)";
			res = sql.ExecuteReader();
			Console.Write("\n* Transaction logs size:\n");
			Console.WriteLine(String.Format("{0,-20} {1,-15} {2,-16}", "Database name", "Log size (MB)", "Space used (%)"));
			Console.WriteLine(String.Format("{0,-20} {1,-15} {2,-16}", "-------------", "-------------", "--------------"));
			while(res.Read())
			{
				Console.WriteLine(String.Format("{0,-20} {1,-15} {2,-16}", res[0], res[1], res[2]));
			}
			res.Close();

			// db files
			sql = new SqlCommand();
			sql.Connection = con;
			sql.CommandType = CommandType.Text;
			sql.CommandText = "SELECT name,type_desc,physical_name FROM sys.database_files";
			res = sql.ExecuteReader();
			Console.Write("\n* Database files:\n");
			Console.WriteLine(String.Format("{0,-20} {1,-6} {2,-30}", "Name", "Type", "Physical path"));
			Console.WriteLine(String.Format("{0,-20} {1,-6} {2,-30}", "----", "----", "-------------"));
			while(res.Read())
			{
				Console.WriteLine(String.Format("{0,-20} {1,-6} {2,-30}", res[0], res[1], res[2]));
			}
			res.Close();

			// db tables
			sql = new SqlCommand();
			sql.Connection = con;
			sql.CommandType = CommandType.Text;
			sql.CommandText = "SELECT sobjects.name FROM sysobjects sobjects WHERE sobjects.xtype = 'U' ORDER BY name";
			res = sql.ExecuteReader();
			Console.Write("\n* Database tables:\n");
			while(res.Read())
			{
				Console.Write(res[0] + " ");
			}
			Console.Write("\n");
			res.Close();

			// Close connection
			con.Close();
		}
	}
}