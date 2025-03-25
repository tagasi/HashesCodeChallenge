using MySqlConnector;
using System.Data;
using System.Text.Json;

public class DbHelper:IDbHelper
{
    private readonly string? _connectionString;
    private readonly ILogger<DbHelper> _logger;

    public DbHelper(ILogger<DbHelper> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("HashesConnection");
    }

    public async Task<string> ExecuteStoredProcedureAsync(string procedureName, Dictionary<string, object>? parameters = null)
    {
        using (var connection = new MySqlConnection(_connectionString))
        using (var command = new MySqlCommand(procedureName, connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            await connection.OpenAsync();
            using (var reader = await command.ExecuteReaderAsync())
            {
                var result = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    result.Add(row);
                }

                // Serialize the result to JSON string
                return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
}
