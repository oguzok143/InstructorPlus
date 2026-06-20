using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class ExamRepository : BaseRepository<Exam>, IDisposable
{
    private static List<Exam> _exams = new();

    public ExamRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadExams();
    }

    private void LoadExams()
    {
        _exams.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT e.id, e.student_id, e.exam_type, e.attempt_number,
                     e.is_passed, e.errors, e.exam_date, s.full_name AS student_name
              FROM exams e
              JOIN students s ON e.student_id = s.id
              ORDER BY e.exam_date DESC", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _exams.Add(new Exam
            {
                Id = reader.GetInt32("id"),
                StudentId = reader.GetInt32("student_id"),
                ExamType = Enum.Parse<ExamType>(reader.GetString("exam_type")),
                AttemptNumber = reader.GetInt32("attempt_number"),
                IsPassed = reader.GetBoolean("is_passed"),
                Errors = reader.IsDBNull("errors") ? null : reader.GetString("errors"),
                ExamDate = reader.GetDateTime("exam_date"),
                StudentName = reader.GetString("student_name"),
            });
        }
    }

    public Exam? GetById(int id) =>
        _exams.FirstOrDefault(e => e.Id == id);

    public List<Exam> GetAll() => _exams;

    public List<Exam> GetByStudent(int studentId) =>
        _exams.FindAll(e => e.StudentId == studentId);

    public Exam? GetLastAttempt(int studentId, ExamType examType) =>
        _exams
            .Where(e => e.StudentId == studentId && e.ExamType == examType)
            .OrderByDescending(e => e.AttemptNumber)
            .FirstOrDefault();

    public void Add(Exam exam)
    {
        using var cmd = new MySqlCommand(
            @"INSERT INTO exams (student_id, exam_type, attempt_number, is_passed, errors, exam_date)
              VALUES (@sid, @type, @attempt, @passed, @errors, @date)", connection);

        cmd.Parameters.AddWithValue("@sid", exam.StudentId);
        cmd.Parameters.AddWithValue("@type", exam.ExamType.ToString());
        cmd.Parameters.AddWithValue("@attempt", exam.AttemptNumber);
        cmd.Parameters.AddWithValue("@passed", exam.IsPassed);
        cmd.Parameters.AddWithValue("@errors", exam.Errors);
        cmd.Parameters.AddWithValue("@date", exam.ExamDate);
        cmd.ExecuteNonQuery();

        LoadExams();
    }

    public void Update(Exam exam)
    {
        using var cmd = new MySqlCommand(
            @"UPDATE exams SET exam_type = @type, attempt_number = @attempt,
                               is_passed = @passed, errors = @errors, exam_date = @date
              WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", exam.Id);
        cmd.Parameters.AddWithValue("@type", exam.ExamType.ToString());
        cmd.Parameters.AddWithValue("@attempt", exam.AttemptNumber);
        cmd.Parameters.AddWithValue("@passed", exam.IsPassed);
        cmd.Parameters.AddWithValue("@errors", exam.Errors);
        cmd.Parameters.AddWithValue("@date", exam.ExamDate);
        cmd.ExecuteNonQuery();

        LoadExams();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}