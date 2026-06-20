using System;
using InstructorPlus.Models.Enums;

namespace InstructorPlus.Models;

public class Exam
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public ExamType ExamType { get; set; }
    public int AttemptNumber { get; set; }
    public bool IsPassed { get; set; }
    public string Errors { get; set; }
    public DateTime ExamDate { get; set; }
    
    public string? StudentName { get; set; }
}