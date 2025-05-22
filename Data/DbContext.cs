using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeAttendance.Data
{
    public class DbContext
    {
        private readonly string _connectionString;

        public DbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, IDictionary<string, object>? parameters = null)
        {
            using SqlConnection connection = CreateConnection();
            using SqlCommand command = new(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            var dataTable = new DataTable();
            await connection.OpenAsync();
            using SqlDataAdapter adapter = new(command);
            adapter.Fill(dataTable);
            return dataTable;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, IDictionary<string, object>? parameters = null)
        {
            using SqlConnection connection = CreateConnection();
            using SqlCommand command = new(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<object?> ExecuteScalarAsync(string query, IDictionary<string, object>? parameters = null)
        {
            using SqlConnection connection = CreateConnection();
            using SqlCommand command = new(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            await connection.OpenAsync();
            return await command.ExecuteScalarAsync();
        }
    }
} 