using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InstructorPlus.Models;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class BranchRepository : BaseRepository<Branch>, IDisposable
{
    private static ObservableCollection<Branch> _branches = new();

    public BranchRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadBranches();
    }

    private void LoadBranches()
    {
        _branches.Clear();

        using var cmd = new MySqlCommand(
            "SELECT id, name, address, phone FROM branches ORDER BY name", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _branches.Add(new Branch
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                Address = reader.GetString("address"),
                Phone = reader.GetString("phone"),
            });
        }
    }

    public Branch? GetById(int id) =>
        _branches.FirstOrDefault(b => b.Id == id);

    public ObservableCollection<Branch> GetAll() => _branches;

    public void Add(Branch branch)
    {
        using var cmd = new MySqlCommand(
            "INSERT INTO branches (name, address, phone) VALUES (@name, @address, @phone)", connection);

        cmd.Parameters.AddWithValue("@name", branch.Name);
        cmd.Parameters.AddWithValue("@address", branch.Address);
        cmd.Parameters.AddWithValue("@phone", branch.Phone ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();

        LoadBranches();
    }

    public void Update(Branch branch)
    {
        using var cmd = new MySqlCommand(
            "UPDATE branches SET name = @name, address = @address, phone = @phone WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", branch.Id);
        cmd.Parameters.AddWithValue("@name", branch.Name);
        cmd.Parameters.AddWithValue("@address", branch.Address);
        cmd.Parameters.AddWithValue("@phone", branch.Phone ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();

        LoadBranches();
    }

    public void Delete(int id)
    {
        using var cmd = new MySqlCommand("DELETE FROM branches WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        LoadBranches();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}