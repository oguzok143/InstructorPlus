using System;
using System.Collections.Generic;
using System.Linq;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class CancellationRepository : BaseRepository<Cancellation>, IDisposable
{
    private static List<Cancellation> _cancellations = new();

    public CancellationRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadCancellations();
    }

    private void LoadCancellations()
    {
        _cancellations.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT c.id, c.lesson_id, c.cancelled_by, c.reason, c.cancelled_at
              FROM cancellations c
              ORDER BY c.cancelled_at DESC", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _cancellations.Add(new Cancellation
            {
                Id = reader.GetInt32("id"),
                LessonId = reader.GetInt32("lesson_id"),
                CancelledBy = Enum.Parse<CancelledBy>(reader.GetString("cancelled_by")),
                Reason = reader.GetString("reason"),
                CancelledAt = reader.GetDateTime("cancelled_at"),
            });
        }
    }

    public List<Cancellation> GetAll() => _cancellations;

    public Cancellation? GetByLesson(int lessonId) =>
        _cancellations.FirstOrDefault(c => c.LessonId == lessonId);

    public List<Cancellation> GetByStudent(int studentId)
    {
        return _cancellations
            .Where(c => _cancellations.Any())
            .ToList();
    }
    
    public void Add(Cancellation cancellation)
    {
        using var cmd = new MySqlCommand(@"INSERT INTO InstructorPlus.cancellations (lesson_id, cancelled_by, reason, cancelled_at)
                    VALUES(@lid, @cby, @reason, @cat)", connection);

        cmd.Parameters.AddWithValue("@lid", cancellation.LessonId);
        cmd.Parameters.AddWithValue("@cby", cancellation.CancelledBy.ToString());
        cmd.Parameters.AddWithValue("@reason", cancellation.Reason);
        cmd.Parameters.AddWithValue("@cat", cancellation.CancelledAt);
        cmd.ExecuteNonQuery();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}