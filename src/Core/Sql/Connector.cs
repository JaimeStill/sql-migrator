using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Core.Sql;
public class Connector
{
    readonly string server;
    readonly string db;

    public Connector(string server, string db)
    {
        this.server = server;
        this.db = db;
    }

    public Connector(string key)
    {
        IConfiguration config = new ConfigurationBuilder()
          .AddJsonFile("connections.json")
          .AddEnvironmentVariables()
          .Build();

        ConnectorConfig result = config
          .GetRequiredSection(key)
          .Get<ConnectorConfig>()
        ?? throw new Exception($"No connector configuration was found for {key}");

        server = result.Server;
        db = result.Database;
    }

    public async Task<List<T>> Query<T>(string query)
    {
        using SqlConnection connection = BuildConnection(server, db);
        await connection.OpenAsync();
        IEnumerable<T> result = await connection.QueryAsync<T>(query);

        return result.ToList();
    }

    public async Task<T?> QueryFirstOrDefault<T>(string query)
    {
        using SqlConnection connection = BuildConnection(server, db);
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<T>(query);
    }

    public async Task<bool> Verify<T>(string query)
    {
        T? result = await QueryFirstOrDefault<T>(query);
        return result is null;
    }

    public async Task<T> Insert<T>(string query, T data)
    {
        using SqlConnection connection = BuildConnection(server, db);
        await connection.OpenAsync();
        return await connection.QuerySingleAsync<T>(query, data);
    }

    protected static SqlConnection BuildConnection(string server, string db) =>
      new(
        new SqlConnectionStringBuilder()
        {
            DataSource = server,
            InitialCatalog = db,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectRetryCount = 3,
            ConnectRetryInterval = 10
        }.ConnectionString
      );
}