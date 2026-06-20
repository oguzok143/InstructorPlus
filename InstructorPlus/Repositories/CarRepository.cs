using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class CarRepository : BaseRepository<Car>, IDisposable
{
    private static List<Car> _cars = new();

    public CarRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadCars();
    }

    private void LoadCars()
    {
        _cars.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT c.id, c.model, c.plate_number, c.transmission,
                     c.branch_id, c.is_active, c.commissioning_date,
                     b.name AS branch_name
              FROM cars c
              JOIN branches b ON c.branch_id = b.id
              ORDER BY c.model", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _cars.Add(new Car
            {
                Id = reader.GetInt32("id"),
                Model = reader.GetString("model"),
                PlateNumber = reader.GetString("plate_number"),
                Transmission = Enum.Parse<Transmission>(reader.GetString("transmission")),
                BranchId = reader.GetInt32("branch_id"),
                IsActive = reader.GetBoolean("is_active"),
                CommissioningDate = reader.GetDateTime("commissioning_date"),
                BranchName = reader.GetString("branch_name"),
            });
        }
    }

    public Car? GetById(int? id) =>
        _cars.FirstOrDefault(c => c.Id == id);

    public List<Car> GetAll() => _cars;

    public List<Car> GetByBranch(int branchId) =>
        _cars.FindAll(c => c.BranchId == branchId);

    public List<Car> GetActive() =>
        _cars.FindAll(c => c.IsActive);

    public void Add(Car car)
    {
        using var cmd = new MySqlCommand(
            @"INSERT INTO cars (model, plate_number, transmission, branch_id, is_active, commissioning_date)
              VALUES (@model, @plate, @transmission, @bid, @active, @date)", connection);

        cmd.Parameters.AddWithValue("@model", car.Model);
        cmd.Parameters.AddWithValue("@plate", car.PlateNumber);
        cmd.Parameters.AddWithValue("@transmission", car.Transmission.ToString());
        cmd.Parameters.AddWithValue("@bid", car.BranchId);
        cmd.Parameters.AddWithValue("@active", car.IsActive);
        cmd.Parameters.AddWithValue("@date", car.CommissioningDate);
        cmd.ExecuteNonQuery();

        LoadCars();
    }

    public void Update(Car car)
    {
        using var cmd = new MySqlCommand(
            @"UPDATE cars SET model = @model, plate_number = @plate, transmission = @transmission,
                              branch_id = @bid, is_active = @active, commissioning_date = @date
              WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", car.Id);
        cmd.Parameters.AddWithValue("@model", car.Model);
        cmd.Parameters.AddWithValue("@plate", car.PlateNumber);
        cmd.Parameters.AddWithValue("@transmission", car.Transmission.ToString());
        cmd.Parameters.AddWithValue("@bid", car.BranchId);
        cmd.Parameters.AddWithValue("@active", car.IsActive);
        cmd.Parameters.AddWithValue("@date", car.CommissioningDate);
        cmd.ExecuteNonQuery();

        LoadCars();
    }

    public void Delete(int id)
    {
        using var cmd = new MySqlCommand("DELETE FROM cars WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        LoadCars();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}