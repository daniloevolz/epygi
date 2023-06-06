using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallDataMiCC_Danilo
{
	class SqlClient : Log
	{
		private static SqlClient _instance;
		private static readonly object _lock = new object();

		private string _usuario;
		private string _senha;
		private string _db;
		private string _serverIp;
		private static SqlConnection _conn;
		public bool _enabled;
		//Log Log = new Log();
		//definição do comando sql
		private string sql = "";

		private SqlClient()
		{
			_usuario = ConfigurationManager.AppSettings["UsuarioSQL"];
			_senha = ConfigurationManager.AppSettings["SenhaSQL"];
			_db = ConfigurationManager.AppSettings["DBSQL"];
			_serverIp = ConfigurationManager.AppSettings["IPSQL"];
			_enabled = Convert.ToBoolean(ConfigurationManager.AppSettings["SQLEnabled"]);
			Send("SqlClient.", _db + _serverIp + _usuario + _senha, "INFO");
		}

		public static SqlClient GetInstance()
		{
			if (_instance == null)
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = new SqlClient();
						_conn = new SqlConnection(@"Data Source=" + _instance._serverIp + ";Initial Catalog=" + _instance._db + ";User ID=" + _instance._usuario + ";Password=" + _instance._senha + ";");
					}
				}
			}
			return _instance;
		}
		internal void CallStart(string callId, string recId, string agentName, DateTime gmtNow, string phone, string card, string type, string mode)
		{
            if (_enabled)
            {
				try
				{
					//definição do comando sql
					Send("SqlClient.CallStart", callId + card + phone, "INFO");
					sql = "INSERT INTO chamadas(CallID, Card, Phone, PhoneType, RecID, AgentName ,CallStart, Mode)VALUES(@callId, @card, @phone, @phoneType, @recId, @agentName, @gmtNow, @mode);";
					using (SqlCommand comando = new SqlCommand(sql, _conn))
					{
						comando.Parameters.Add(new SqlParameter("@callId", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@card", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@phone", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@phoneType", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@recId", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@agentName", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@gmtNow", SqlDbType.DateTime));
						comando.Parameters.Add(new SqlParameter("@mode", SqlDbType.VarChar));

						_conn.Open();

						//output the data || NOTE: **NSERT into database table**
						comando.Parameters["@callId"].Value = Convert.ToString(callId);
						comando.Parameters["@card"].Value = Convert.ToString(card);
						comando.Parameters["@phone"].Value = Convert.ToString(phone);
						comando.Parameters["@phoneType"].Value = Convert.ToString(type);
						comando.Parameters["@recId"].Value = Convert.ToString(recId);
						comando.Parameters["@agentName"].Value = Convert.ToString(agentName);
						comando.Parameters["@gmtNow"].Value = Convert.ToDateTime(gmtNow);
						comando.Parameters["@mode"].Value = Convert.ToString(mode);
						//executa o comando com os parametros que foram adicionados acima

						comando.ExecuteNonQuery();
					}
				}
				catch (Exception e)
				{
					//Log arquivo texto para conferência
					//Console.WriteLine(e.Message);
					Send("SqlClient.CallStart", e.Message, "ERRO");
					//

				}
				finally
				{
					_conn.Close();
					//_connected = false;
				}
			}
		}
		internal void CallRing(string v, int intData, DateTime gmtNow)
		{
            if (_enabled)
            {
				try
				{
					Send("SqlClient.CallRing", v + Convert.ToString(gmtNow), "INFO");
					//definição do comando sql
					sql = "UPDATE chamadas SET CallRing = @gmtNow WHERE CallID = @callId";
					using (SqlCommand comando = new SqlCommand(sql, _conn))
					{
						comando.Parameters.Add(new SqlParameter("@callId", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@gmtNow", SqlDbType.DateTime));

						_conn.Open();
						//output the data || NOTE: **NSERT into database table**

						comando.Parameters["@callId"].Value = Convert.ToString(v);
						comando.Parameters["@gmtNow"].Value = Convert.ToDateTime(gmtNow);
						//executa o comando com os parametros que foram adicionados acima
						comando.ExecuteNonQuery();
					}
				}
				catch (Exception e)
				{
					//Log arquivo texto para conferência
					//Console.WriteLine(e.Message);
					Send("SqlClient.CallRing", e.Message, "ERRO");
					//

				}
				finally
				{
					_conn.Close();
					//_connected = false;
				}
			}
		}
		internal void CallAnswered(string v, int intData, DateTime gmtNow)
		{
            if (_enabled)
            {
				try
				{
					Send("SqlClient.CallAnswered", v + Convert.ToString(gmtNow), "INFO");
					//definição do comando sql
					sql = "UPDATE chamadas SET CallAnswered = @gmtNow WHERE CallID = @callId";
					using (SqlCommand comando = new SqlCommand(sql, _conn))
					{
						comando.Parameters.Add(new SqlParameter("@callId", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@gmtNow", SqlDbType.DateTime));

						_conn.Open();
						//output the data || NOTE: **NSERT into database table**

						comando.Parameters["@callId"].Value = Convert.ToString(v);
						comando.Parameters["@gmtNow"].Value = Convert.ToDateTime(gmtNow);
						//executa o comando com os parametros que foram adicionados acima
						comando.ExecuteNonQuery();
					}
				}
				catch (Exception e)
				{
					//Log arquivo texto para conferência
					Send("SqlClient.CallAnswered", e.Message, "ERRO");
					//

				}
				finally
				{
					_conn.Close();
					//_connected = false;
				}
			}
		}
		internal void CallEnd(string v, int intData, DateTime gmtNow, TimeSpan holdTime)
		{
            if (_enabled)
            {
				try
				{
					Send("SqlClient.CallEnd", v + Convert.ToString(gmtNow), "INFO");
					//definição do comando sql
					sql = "UPDATE chamadas SET CallEnd = @gmtNow, HoldTime = @holdTime WHERE CallID = @callId";
					using (SqlCommand comando = new SqlCommand(sql, _conn))
					{
						comando.Parameters.Add(new SqlParameter("@callId", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@gmtNow", SqlDbType.DateTime));
						comando.Parameters.Add(new SqlParameter("@holdTime", SqlDbType.Time));
						_conn.Open();
						//output the data || NOTE: **NSERT into database table**

						comando.Parameters["@callId"].Value = Convert.ToString(v);
						comando.Parameters["@gmtNow"].Value = Convert.ToDateTime(gmtNow);
						comando.Parameters["@holdTime"].Value = TimeSpan.Parse(Convert.ToString(holdTime));
						//executa o comando com os parametros que foram adicionados acima
						comando.ExecuteNonQuery();
					}
				}
				catch (Exception e)
				{
					//Log arquivo texto para conferência
					//Console.WriteLine(e.Message);
					Send("SqlClient.CallEnd", e.Message, "ERRO");
					//

				}
				finally
				{
					_conn.Close();
					//_connected = false;
				}
			}
		}
		internal void ClericalEnd(string v, int intData, DateTime gmtNow)
		{
            if (_enabled)
            {
				try
				{
					Send("SqlClient.ClericalEnd", v + Convert.ToString(gmtNow), "INFO");
					//definição do comando sql
					sql = "UPDATE chamadas SET ClericalEnd = @gmtNow WHERE CallID = @callId";
					using (SqlCommand comando = new SqlCommand(sql, _conn))
					{
						comando.Parameters.Add(new SqlParameter("@callId", SqlDbType.VarChar));
						comando.Parameters.Add(new SqlParameter("@gmtNow", SqlDbType.DateTime));

						_conn.Open();
						//output the data || NOTE: **NSERT into database table**

						comando.Parameters["@callId"].Value = Convert.ToString(v);
						comando.Parameters["@gmtNow"].Value = Convert.ToDateTime(gmtNow);
						//executa o comando com os parametros que foram adicionados acima
						comando.ExecuteNonQuery();
					}
				}
				catch (Exception e)
				{
					//Log arquivo texto para conferência
					Send("SqlClient.ClericalEnd", e.Message, "ERRO");
					//

				}
				finally
				{
					_conn.Close();
					//_connected = false;
				}
			}
		}
		public void Dispose()
		{
			//fecha a conexao
			_conn.Close();
		}
	}
}

/*
		public void Open()
		{
			try
			{
				//abre a conexao
				conn.Open();
				_connected = true;
			}
			catch (Exception e)
			{
				//Log arquivo texto para conferência
				Log.Send("SqlClient.Open", e.Message, "ERRO");
				//
			}


		}
		
		public void Clear()
		{
			//definição do comando sql
			Log.Send("SqlClient.Clear", "", "INFO");
			sql = "DELETE FROM agentintegration.dbo.agentes;";
			try
			{
				using (SqlCommand comando = new SqlCommand(sql, conn))
				{
					//executa o comando com os parametros que foram adicionados acima
					comando.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				//Log arquivo texto para conferência
				Log.Send("SqlClient.Clear", e.Message, "ERRO");
				//

			}
			finally
			{
				//conn.Close();
				//_connected = false;
			}

		}
		public void InsertAgent(int recId, string name, string reason)
		{
			//definição do comando sql
			Log.Send("SqlClient.InsertAgent", recId + name + reason, "INFO");
			sql = "INSERT INTO agentes(RecID, AgentName, Reason)VALUES(@recId, @name, @reason)";
			try
			{

				using (SqlCommand comando = new SqlCommand(sql, conn))
				{
					comando.Parameters.Add(new SqlParameter("@recId", SqlDbType.Int));
					comando.Parameters.Add(new SqlParameter("@name", SqlDbType.VarChar));
					comando.Parameters.Add(new SqlParameter("@reason", SqlDbType.VarChar));

					//conn.Open();

					//output the data || NOTE: **NSERT into database table**

					comando.Parameters["@recId"].Value = Convert.ToInt32(recId);
					comando.Parameters["@name"].Value = Convert.ToString(name);
					comando.Parameters["@reason"].Value = Convert.ToString(reason);
					//executa o comando com os parametros que foram adicionados acima
					comando.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				//Log arquivo texto para conferência
				Log.Send("SqlClient.InsertAgent", recId + e.Message, "ERRO");
				//
				_connected = false;

			}
			finally
			{
				//conn.Close();
				//_connected = false;
			}

		}
		public void UpdateAgent(int recId, string reason)
		{
			Log.Send("SqlClient.UpdateAgent", recId + reason, "INFO");
			//definição do comando sql
			sql = "UPDATE agentes SET Reason = @reason WHERE RecID = @recId";
			try
			{

				using (SqlCommand comando = new SqlCommand(sql, conn))
				{
					comando.Parameters.Add(new SqlParameter("@recId", SqlDbType.Int));
					comando.Parameters.Add(new SqlParameter("@reason", SqlDbType.VarChar));

					//conn.Open();
					//output the data || NOTE: **NSERT into database table**

					comando.Parameters["@recId"].Value = Convert.ToInt32(recId);
					comando.Parameters["@reason"].Value = Convert.ToString(reason);
					//executa o comando com os parametros que foram adicionados acima
					comando.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				//Log arquivo texto para conferência
				Log.Send("SqlClient.UpdateAgent", recId + e.Message, "ERRO");
				//
				_connected = false;

			}
			finally
			{
				//conn.Close();
				//_connected = false;
			}
		}
		public void DeleteAgent(int recId)
		{
			Log.Send("SqlClient.DeleteAgent", Convert.ToString(recId), "INFO");
			//definição do comando sql
			sql = "DELETE FROM agentes WHERE RecID = @recId";
			try
			{

				using (SqlCommand comando = new SqlCommand(sql, conn))
				{
					comando.Parameters.Add(new SqlParameter("@recId", SqlDbType.Int));

					//conn.Open();
					//output the data || NOTE: **NSERT into database table**
					comando.Parameters["@recId"].Value = Convert.ToInt32(recId);
					//executa o comando com os parametros que foram adicionados acima
					comando.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				//Log arquivo texto para conferência
				Log.Send("SqlClient.DeleteAgent", recId + e.Message, "ERRO");
				//
				_connected = false;

			}
			finally
			{
				//conn.Close();
				//_connected = false;
			}
		}
		public string AgentReason(int recId)
		{
			Log.Send("SqlClient.AgentReason", Convert.ToString(recId), "INFO");
			string result = "";
			try
			{

				//definição do comando sql
				SqlCommand command = new SqlCommand("SELECT Reason FROM agentes WHERE RecID = @recId", conn);
				command.Parameters.AddWithValue("@recId", recId);

				//conn.Open();
				// int result = command.ExecuteNonQuery();
				using (SqlDataReader reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						result = Convert.ToString(reader["Reason"]);
						Console.WriteLine(String.Format("{0}", result));
					}
				}
			}
			catch (Exception e)
			{
				//Log arquivo texto para conferência
				Log.Send("SqlClient.AgentReason", recId + e.Message, "ERRO");
				//
				_connected = false;
			}
			finally
			{
				//conn.Close();
				//_connected = false;
			}
			return result;
		}
		*/


