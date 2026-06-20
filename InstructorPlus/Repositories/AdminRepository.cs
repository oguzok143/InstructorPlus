using System;
using System.Collections.Generic;
using System.Linq;
using InstructorPlus.Models;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class AdminRepository : BaseRepository<Admin>, IDisposable
{
    private static List<Admin> _admins = new();

    public AdminRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadAdmins();
    }

    private void LoadAdmins()
    {
        _admins.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT a.id, a.login, a.password, a.full_name, a.phone,
                     a.branch_id, a.hire_date, b.name AS branch_name
              FROM admins a
              LEFT JOIN branches b ON a.branch_id = b.id
              ORDER BY a.full_name", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _admins.Add(new Admin
            {
                Id = reader.GetInt32("id"),
                Login = reader.GetString("login"),
                Password = reader.GetString("password"),
                FullName = reader.GetString("full_name"),
                Phone = reader.GetString("phone"),
                BranchId = reader.GetInt32("branch_id"),
                HireDate = reader.GetDateTime("hire_date"),
                BranchName = reader.GetString("branch_name"),
            });
        }
    }

    public Admin? GetByLogin(string login) =>
        _admins.FirstOrDefault(a => a.Login == login);

    public List<Admin> GetAll() => _admins;

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}