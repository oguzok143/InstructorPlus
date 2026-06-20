using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InstructorPlus.Models;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class RouteRepository : BaseRepository<Route>, IDisposable
{
    private static List<Route> _routes = new();

    public RouteRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadRoutes();
    }

    private void LoadRoutes()
    {
        _routes.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT r.id, r.name, r.description, r.branch_id,
                     b.name AS branch_name
              FROM routes r
              JOIN branches b ON r.branch_id = b.id
              ORDER BY r.name", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _routes.Add(new Route
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                BranchId = reader.GetInt32("branch_id"),
                BranchName = reader.GetString("branch_name"),
            });
        }
    }

    public Route? GetById(int id) =>
        _routes.FirstOrDefault(r => r.Id == id);

    public List<Route> GetAll() => _routes;

    public List<Route> GetByBranch(int branchId) =>
        _routes.FindAll(r => r.BranchId == branchId);

    public void Add(Route route)
    {
        using var cmd = new MySqlCommand(
            @"INSERT INTO routes (name, description, branch_id)
              VALUES (@name, @desc, @bid)", connection);

        cmd.Parameters.AddWithValue("@name", route.Name);
        cmd.Parameters.AddWithValue("@desc", route.Description);
        cmd.Parameters.AddWithValue("@bid", route.BranchId);
        cmd.ExecuteNonQuery();

        LoadRoutes();
    }

    public void Update(Route route)
    {
        using var cmd = new MySqlCommand(
            @"UPDATE routes SET name = @name, description = @desc, branch_id = @bid
              WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", route.Id);
        cmd.Parameters.AddWithValue("@name", route.Name);
        cmd.Parameters.AddWithValue("@desc", route.Description);
        cmd.Parameters.AddWithValue("@bid", route.BranchId);
        cmd.ExecuteNonQuery();

        LoadRoutes();
    }

    public void Delete(int id)
    {
        using var cmd = new MySqlCommand("DELETE FROM routes WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        LoadRoutes();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}