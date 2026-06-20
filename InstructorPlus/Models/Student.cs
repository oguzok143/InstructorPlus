using System;

namespace InstructorPlus.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public long? TelegramId { get; set; }
    public int BranchId { get; set; }
    public int TotalHoursRequired { get; set; }
    public int HoursCompleted { get; set; }
    public bool ExamTheoryInternal { get; set; }
    public bool ExamPracticeInternal { get; set; }
    public DateTime EnrollmentDate { get; set; }
    
    public string? BranchName { get; set; }
}