using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InstructorPlus.Models;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class InstructorRepository : BaseRepository<Instructor>, IDisposable
{
    private static ObservableCollection<Instructor> _instructors = new();

    public InstructorRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadInstructors();
    }

    private void LoadInstructors()
    {
        _instructors.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT i.id, i.login, i.password, i.full_name, i.phone,
                     i.telegram_id, i.branch_id, i.car_id, i.hire_date,
                     b.name AS branch_name,
                     c.model AS car_model, c.plate_number AS car_plate
              FROM instructors i
              JOIN branches b ON i.branch_id = b.id
              LEFT JOIN cars c ON i.car_id = c.id
              ORDER BY i.full_name", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _instructors.Add(new Instructor
            {
                Id = reader.GetInt32("id"),
                Login = reader.GetString("login"),
                Password = reader.GetString("password"),
                FullName = reader.GetString("full_name"),
                Phone = reader.GetString("phone"),
                TelegramId = reader.GetInt64("telegram_id"),
                BranchId = reader.GetInt32("branch_id"),
                CarId = reader.GetInt32("car_id"),
                HireDate = reader.GetDateTime("hire_date"),
                BranchName = reader.GetString("branch_name"),
                CarModel = reader.GetString("car_model"),
                CarPlate = reader.GetString("car_plate"),
            });
        }
    }

    public Instructor? GetByLogin(string login) =>
        _instructors.FirstOrDefault(i => i.Login == login);

    public ObservableCollection<Instructor> GetAll() => _instructors;

    // public ObservableCollection<Instructor> GetByBranch(int branchId) =>
    //     _instructors.FindAll(i => i.BranchId == branchId);

    public void Add(Instructor instructor)
    {
        using var cmd = new MySqlCommand(
            @"INSERT INTO instructors (login, password, full_name, phone, telegram_id,
                                       branch_id, car_id, hire_date)
              VALUES (@login, @pass, @name, @phone, @tid, @bid, @cid, @date)", connection);

        cmd.Parameters.AddWithValue("@login", instructor.Login);
        cmd.Parameters.AddWithValue("@pass", instructor.Password);
        cmd.Parameters.AddWithValue("@name", instructor.FullName);
        cmd.Parameters.AddWithValue("@phone", instructor.Phone);
        cmd.Parameters.AddWithValue("@tid", instructor.TelegramId);
        cmd.Parameters.AddWithValue("@bid", instructor.BranchId);
        cmd.Parameters.AddWithValue("@cid", instructor.CarId);
        cmd.Parameters.AddWithValue("@date", instructor.HireDate);
        cmd.ExecuteNonQuery();

        LoadInstructors();
    }

    public void Update(Instructor instructor)
    {
        using var cmd = new MySqlCommand(
            @"UPDATE instructors SET login = @login, password = @pass, full_name = @name,
                                     phone = @phone, telegram_id = @tid,
                                     branch_id = @bid, car_id = @cid, hire_date = @date
              WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", instructor.Id);
        cmd.Parameters.AddWithValue("@login", instructor.Login);
        cmd.Parameters.AddWithValue("@pass", instructor.Password);
        cmd.Parameters.AddWithValue("@name", instructor.FullName);
        cmd.Parameters.AddWithValue("@phone", instructor.Phone);
        cmd.Parameters.AddWithValue("@tid", instructor.TelegramId);
        cmd.Parameters.AddWithValue("@bid", instructor.BranchId);
        cmd.Parameters.AddWithValue("@cid", instructor.CarId);
        cmd.Parameters.AddWithValue("@date", instructor.HireDate);
        cmd.ExecuteNonQuery();

        LoadInstructors();
    }

    public void Delete(int id)
    {
        using var cmd = new MySqlCommand("DELETE FROM instructors WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        LoadInstructors();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}