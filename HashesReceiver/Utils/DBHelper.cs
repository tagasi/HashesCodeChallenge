using MySqlConnector;


public class DBHelper: IDBHelper
{
    private readonly string? _connectionString;
    private readonly ILogger<DBHelper> _logger;

    public DBHelper(ILogger<DBHelper> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("HashesConnection");
    }

    public async Task InsertHashAsync(string sha1)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "INSERT INTO hashes (sha1) VALUES (@sha1)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@sha1", sha1);

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation($"Inserted SHA1: {sha1}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting hash into MariaDB");
        }
    }
}
