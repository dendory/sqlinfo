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
					case "-s":
						server = args[++i];
						break;
					case "-d":
						dbname = args[++i];
						break;
					case "-u":
						userid = args[++i];
						break;
					case "-p":
						passwd = args[++i];
						break;
					case "-t":
						timeout = Int32.Parse(args[++i]);
						break;
					case "-h":
						Console.WriteLine("* Usage:\n\t sqlinfo -s <server> -d <database> -u <username> -p <password> -t <timeout>\n");
						Environment.Exit(0);
						break;
					default:
						Console.WriteLine("* Unknown option: " + args[i] + ". Try sqlinfo -h for help.");
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
				Console.WriteLine("Error connecting to server: " + e.Message);
				Environment.Exit(1);
			}

			// server version
			Console.Write("\n* Server version:\n");
			try
			{
				sql = new SqlCommand();
				sql.Connection = con;
				sql.CommandType = CommandType.Text;
				sql.CommandText = "SELECT @@version";
				res = sql.ExecuteReader();
				while(res.Read())
				{
					Console.WriteLine(res[0]);
				}
				res.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Error querying server: " + e.Message);
			}
			
			// List databases
			Console.WriteLine("\n* Server database names:");
			try
			{
				sql = new SqlCommand();
				sql.Connection = con;
				sql.CommandType = CommandType.StoredProcedure;
				sql.CommandText = "sp_databases";
				res = sql.ExecuteReader();
				while(res.Read())
				{
					Console.Write(res.GetString(0) + " ");
				}
				Console.Write("\n");
				res.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Error querying server: " + e.Message);
			}

			// List logins
			Console.Write("\n* Server logins:\n");
			try
			{
				sql = new SqlCommand();
				sql.Connection = con;
				sql.CommandType = CommandType.Text;
				sql.CommandText = "SELECT name,type,default_database_name FROM sys.server_principals";
				res = sql.ExecuteReader();
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
			}
			catch(Exception e)
			{
				Console.WriteLine("Error querying server: " + e.Message);
			}

			// Log info
			Console.Write("\n* Transaction logs size:\n");
			try
			{
				sql = new SqlCommand();
				sql.Connection = con;
				sql.CommandType = CommandType.Text;
				sql.CommandText = "DBCC SQLPERF(logspace)";
				res = sql.ExecuteReader();
				Console.WriteLine(String.Format("{0,-20} {1,-15} {2,-16}", "Database name", "Log size (MB)", "Space used (%)"));
				Console.WriteLine(String.Format("{0,-20} {1,-15} {2,-16}", "-------------", "-------------", "--------------"));
				while(res.Read())
				{
					Console.WriteLine(String.Format("{0,-20} {1,-15} {2,-16}", res[0], res[1], res[2]));
				}
				res.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Error querying server: " + e.Message);
			}

			// db files
			Console.Write("\n* Database files:\n");
			try
			{
				sql = new SqlCommand();
				sql.Connection = con;
				sql.CommandType = CommandType.Text;
				sql.CommandText = "SELECT name,type_desc,physical_name FROM sys.database_files";
				res = sql.ExecuteReader();
				Console.WriteLine(String.Format("{0,-20} {1,-6} {2,-30}", "Name", "Type", "Physical path"));
				Console.WriteLine(String.Format("{0,-20} {1,-6} {2,-30}", "----", "----", "-------------"));
				while(res.Read())
				{
					Console.WriteLine(String.Format("{0,-20} {1,-6} {2,-30}", res[0], res[1], res[2]));
				}
				res.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Error querying server: " + e.Message);
			}

			// db tables
			Console.Write("\n* Database tables:\n");
			try
			{
				sql = new SqlCommand();
				sql.Connection = con;
				sql.CommandType = CommandType.Text;
				sql.CommandText = "SELECT sobjects.name FROM sysobjects sobjects WHERE sobjects.xtype = 'U' ORDER BY name";
				res = sql.ExecuteReader();
				while(res.Read())
				{
					Console.Write(res[0] + " ");
				}
				Console.Write("\n");
				res.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Error querying server: " + e.Message);
			}

			// Close connection
			con.Close();
		}
	}
}