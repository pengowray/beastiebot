using System;
using MySql.Data.MySqlClient; 

public class MySQLExample
{
	
	static void Main() 
	{
		string cs = @"server=localhost;userid=user12;
            password=34klq*;database=mydb";
		
		MySqlConnection conn = null;
		
		try 
		{
			conn = new MySqlConnection(cs);
			conn.Open();
			Console.WriteLine("MySQL version : {0}", conn.ServerVersion);
			
		} catch (MySqlException ex) 
		{
			Console.WriteLine("Error: {0}",  ex.ToString());
			
		} finally 
		{          
			if (conn != null) 
			{
				conn.Close();
			}
		}
	}


	static void Main2() 
	{
		string cs = @"server=localhost;userid=user12;
            password=34klq*;database=mydb";
		
		MySqlConnection conn = null;
		
		try 
		{
			conn = new MySqlConnection(cs);
			conn.Open();
			
			string stm = "SELECT VERSION()";   
			MySqlCommand cmd = new MySqlCommand(stm, conn);
			string version = Convert.ToString(cmd.ExecuteScalar());
			Console.WriteLine("MySQL version : {0}", version);
			
		} catch (MySqlException ex) 
		{
			Console.WriteLine("Error: {0}",  ex.ToString());
			
		} finally 
		{
			
			if (conn != null) 
			{
				conn.Close();
			}
			
		}
	}

}

