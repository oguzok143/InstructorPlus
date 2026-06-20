using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class LessonRepository: BaseRepository<Lesson>, IDisposable
{
    private static List<Lesson> _lessons = new();

    public LessonRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadLessons();
    }

    private void LoadLessons()
    {
        _lessons.Clear();
        
        using var cmd = new MySqlCommand(
            @"SELECT l.id, l.student_id, l.instructor_id, l.car_id, l.route_id,
                     l.branch_id, l.lesson_date, l.duration_minutes, l.meeting_place,
                     l.status, s.full_name AS student_name, i.full_name AS instructor_name,
                     c.model AS car_model, c.plate_number AS car_plate,
                     r.name AS route_name, b.name AS branch_name
              FROM lessons l
              LEFT JOIN students s ON l.student_id = s.id
              JOIN instructors i ON l.instructor_id = i.id
              JOIN cars c ON l.car_id = c.id
              LEFT JOIN routes r ON l.route_id = r.id
              JOIN branches b ON l.branch_id = b.id
              ORDER BY l.lesson_date DESC", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _lessons.Add(new Lesson
            {
                Id = reader.GetInt32("id"),
                StudentId = reader.IsDBNull("student_id") ? null : reader.GetInt32("student_id"),
                InstructorId = reader.GetInt32("instructor_id"),
                CarId = reader.GetInt32("car_id"),
                RouteId = reader.IsDBNull("route_id") ? null : reader.GetInt32("route_id"),
                BranchId = reader.GetInt32("branch_id"),
                LessonDate = reader.GetDateTime("lesson_date"),
                DurationMinutes = reader.GetInt32("duration_minutes"),
                MeetingPlace = reader.IsDBNull("meeting_place") ? null : reader.GetString("meeting_place"),
                Status = Enum.Parse<LessonStatus>(reader.GetString("status")),
                StudentName = reader.IsDBNull("student_name") ? null : reader.GetString("student_name"),
                InstructorName = reader.GetString("instructor_name"),
                CarModel = reader.GetString("car_model"),
                CarPlate = reader.GetString("car_plate"),
                RouteName = reader.IsDBNull("route_name") ? null : reader.GetString("route_name"),
                BranchName = reader.GetString("branch_name"),
            });
        }
    }
    
    public List<Lesson> GetAll() => _lessons;
    
    public List<Lesson> GetByStudentId(int studentId) =>
        _lessons.FindAll(l => l.StudentId == studentId);
    
    public List<Lesson> GetByInstructorId(int instructorId) =>
        _lessons.FindAll(l => l.InstructorId == instructorId);
    
    public List<Lesson> GetByBranchId(int branchId) =>
        _lessons.FindAll(l => l.BranchId == branchId);

    public List<Lesson> GetAvailableSlots(int branchId) =>
        _lessons.FindAll(l =>
            l.StudentId == null &&
            l.LessonDate > DateTime.Now &&
            l.Status == LessonStatus.scheduled &&
            l.BranchId == branchId
        );

    public int GetCountByInstructor(int instructorId) =>
        _lessons.Count(l => l.InstructorId == instructorId);

    public List<Lesson> GetPageByInstructor(int instructorId, int page, int pageSize) =>
        _lessons
            .Where(l => l.InstructorId == instructorId)
            .OrderByDescending(l => l.LessonDate)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();
    
    public int GetCountByBranch(int branchId) =>
        _lessons.Count(l => l.BranchId == branchId);

    public List<Lesson> GetPageByBranch(int branchId, int page, int pageSize) =>
        _lessons
            .Where(l => l.BranchId == branchId)
            .OrderByDescending(l => l.LessonDate)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

    public void Add(Lesson lesson)
    {
        using var cmd = new MySqlCommand(@"INSERT INTO lessons (student_id, instructor_id, car_id, route_id, branch_id,
                                   lesson_date, duration_minutes, meeting_place, status)
              VALUES (@sid, @iid, @cid, @rid, @bid, @date, @dur, @place, @status)", connection);
        
        cmd.Parameters.AddWithValue("@sid", lesson.StudentId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@iid", lesson.InstructorId);
        cmd.Parameters.AddWithValue("@cid", lesson.CarId);
        cmd.Parameters.AddWithValue("@rid", lesson.RouteId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@bid", lesson.BranchId);
        cmd.Parameters.AddWithValue("@date", lesson.LessonDate);
        cmd.Parameters.AddWithValue("@dur", lesson.DurationMinutes);
        cmd.Parameters.AddWithValue("@place", lesson.MeetingPlace ??  (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@status", lesson.Status.ToString());
        cmd.ExecuteNonQuery();
        
        LoadLessons();
    }
    
    public void Update(Lesson lesson)
    {
        using var cmd = new MySqlCommand(
            @"UPDATE lessons SET student_id = @sid, instructor_id = @iid, car_id = @cid,
                             route_id = @rid, lesson_date = @date,
                             duration_minutes = @dur, meeting_place = @place,
                             status = @status
          WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", lesson.Id);
        cmd.Parameters.AddWithValue("@sid", lesson.StudentId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@iid", lesson.InstructorId);
        cmd.Parameters.AddWithValue("@cid", lesson.CarId);
        cmd.Parameters.AddWithValue("@rid", lesson.RouteId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@date", lesson.LessonDate);
        cmd.Parameters.AddWithValue("@dur", lesson.DurationMinutes);
        cmd.Parameters.AddWithValue("@place", lesson.MeetingPlace ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@status", lesson.Status.ToString());
        cmd.ExecuteNonQuery();

        LoadLessons();
    }

    public void Cancel(int lessonId)
    {
        using var cmd = new MySqlCommand("UPDATE lessons SET status = 'cancelled' WHERE id = @id",  connection);
        cmd.Parameters.AddWithValue("@id", lessonId);
        cmd.ExecuteNonQuery();

        LoadLessons();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}