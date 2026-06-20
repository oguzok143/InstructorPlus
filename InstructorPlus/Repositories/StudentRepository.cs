using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InstructorPlus.Models;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class StudentRepository : BaseRepository<Student>, IDisposable
{
    private static List<Student> _students = new();

    public StudentRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadStudents();
    }

    private void LoadStudents()
    {
        _students.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT s.id, s.full_name, s.phone, s.telegram_id, s.branch_id,
                     s.total_hours_required, s.hours_completed,
                     s.exam_theory_internal, s.exam_practice_internal,
                     s.enrollment_date, b.name AS branch_name
              FROM students s
              JOIN branches b ON s.branch_id = b.id
              ORDER BY s.full_name", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _students.Add(new Student
            {
                Id = reader.GetInt32("id"),
                FullName = reader.GetString("full_name"),
                Phone = reader.GetString("phone"),
                TelegramId = reader.IsDBNull("telegram_id") ? null : reader.GetInt64("telegram_id"),
                BranchId = reader.GetInt32("branch_id"),
                TotalHoursRequired = reader.GetInt32("total_hours_required"),
                HoursCompleted = reader.GetInt32("hours_completed"),
                ExamTheoryInternal = reader.GetBoolean("exam_theory_internal"),
                ExamPracticeInternal = reader.GetBoolean("exam_practice_internal"),
                EnrollmentDate = reader.GetDateTime("enrollment_date"),
                BranchName = reader.GetString("branch_name"),
            });
        }
    }

    public Student? GetById(int id) =>
        _students.FirstOrDefault(s => s.Id == id);

    public List<Student> GetAll() => _students;

    public List<Student> GetByBranch(int branchId) =>
        _students.FindAll(s => s.BranchId == branchId);

    public void Add(Student student)
    {
        using var cmd = new MySqlCommand(
            @"INSERT INTO students (full_name, phone, telegram_id, branch_id,
                                    total_hours_required, hours_completed,
                                    exam_theory_internal, exam_practice_internal,
                                    enrollment_date)
              VALUES (@name, @phone, @tid, @bid, @total, @completed, @ti, @pi, @date)", connection);

        cmd.Parameters.AddWithValue("@name", student.FullName);
        cmd.Parameters.AddWithValue("@phone", student.Phone);
        cmd.Parameters.AddWithValue("@tid", student.TelegramId);
        cmd.Parameters.AddWithValue("@bid", student.BranchId);
        cmd.Parameters.AddWithValue("@total", student.TotalHoursRequired);
        cmd.Parameters.AddWithValue("@completed", student.HoursCompleted);
        cmd.Parameters.AddWithValue("@ti", student.ExamTheoryInternal);
        cmd.Parameters.AddWithValue("@pi", student.ExamPracticeInternal);
        cmd.Parameters.AddWithValue("@date", student.EnrollmentDate);
        cmd.ExecuteNonQuery();

        LoadStudents();
    }

    public void Update(Student student)
    {
        using var cmd = new MySqlCommand(
            @"UPDATE students SET full_name = @name, phone = @phone, telegram_id = @tid,
                                  branch_id = @bid, total_hours_required = @total,
                                  hours_completed = @completed,
                                  exam_theory_internal = @ti, exam_practice_internal = @pi,
                                  enrollment_date = @date
              WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", student.Id);
        cmd.Parameters.AddWithValue("@name", student.FullName);
        cmd.Parameters.AddWithValue("@phone", student.Phone);
        cmd.Parameters.AddWithValue("@tid", student.TelegramId);
        cmd.Parameters.AddWithValue("@bid", student.BranchId);
        cmd.Parameters.AddWithValue("@total", student.TotalHoursRequired);
        cmd.Parameters.AddWithValue("@completed", student.HoursCompleted);
        cmd.Parameters.AddWithValue("@ti", student.ExamTheoryInternal);
        cmd.Parameters.AddWithValue("@pi", student.ExamPracticeInternal);
        cmd.Parameters.AddWithValue("@date", student.EnrollmentDate);
        cmd.ExecuteNonQuery();

        LoadStudents();
    }

    public void Delete(int id)
    {
        using var cmd = new MySqlCommand("DELETE FROM students WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        LoadStudents();
    }

    public int GetCountByBranch(int branchId) =>
        _students.Count(l => l.BranchId == branchId);

    public List<Student> GetPageByBranch(int branchId, int page, int pageSize) =>
        _students
            .Where(l => l.BranchId == branchId)
            .OrderByDescending(l => l.EnrollmentDate)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

    
    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}