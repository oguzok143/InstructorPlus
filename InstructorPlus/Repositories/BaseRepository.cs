using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public abstract class BaseRepository<T> : IDisposable 
{
    private readonly string _connectionString;
    protected MySqlConnection? connection;

    protected BaseRepository(IOptions<DatabaseSettings> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    protected bool OpenConnection()
    {
        try
        {
            connection = new MySqlConnection(_connectionString);
            connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[DB ERROR] {ex.Message}");
            return false;
        }
    }

    protected bool CloseConnection()
    {
        try
        {
            connection?.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[DB ERROR] {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
