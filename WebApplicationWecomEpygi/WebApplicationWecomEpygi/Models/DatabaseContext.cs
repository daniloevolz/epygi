using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace WebApplicationWecomEpygi.Models
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = config["DefaultConnection"];

        }

        public dynamic ExecuteQuery(string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    List<dynamic> results = new List<dynamic>();

                    while (reader.Read())
                    {
                        dynamic result = new System.Dynamic.ExpandoObject();
                        var dict = result as IDictionary<string, object>;

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dict[reader.GetName(i)] = reader[i];
                        }

                        results.Add(result);
                    }
                    connection.Close();

                    return results;
                }
            }
        }

        public void ExecuteNonQuery(string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        public void InsertUser(string query, User user)
        {
            // Executar a query com parâmetros
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Adicionar parâmetros
                    command.Parameters.AddWithValue("@Name", user.name);
                    command.Parameters.AddWithValue("@Sip", user.sip);
                    command.Parameters.AddWithValue("@Number", user.num);
                    command.Parameters.AddWithValue("@Email", user.email);
                    command.Parameters.AddWithValue("@Image", user.img);
                    command.Parameters.AddWithValue("@Department", user.department);
                    command.Parameters.AddWithValue("@Location", user.location);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        public void DeleteUser(string query, string sip)
        {
            // Executar a query com parâmetros
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Adicionar parâmetros
                    command.Parameters.AddWithValue("@Sip", sip);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        public void Department(string query, string department)
        {
            // Executar a query com parâmetros
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Adicionar parâmetros
                    command.Parameters.AddWithValue("@Department", department);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        public void Location(string query, string location)
        {
            // Executar a query com parâmetros
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Adicionar parâmetros
                    command.Parameters.AddWithValue("@Location", location);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}

