using System;
using InstructorPlus.Models.Enums;

namespace InstructorPlus.Models;

public class Lesson
{
    public int Id { get; set; }
    public int? StudentId { get; set; }
    public int InstructorId { get; set; }
    public int CarId { get; set; }
    public int? RouteId { get; set; }
    public int BranchId { get; set; }
    public DateTime LessonDate { get; set; }
    
    public string LessonDateDisplay => LessonDate.ToString("dd.MM.yyyy");
    public string LessonTimeDisplay => LessonDate.ToString("HH:mm");
    
    public int DurationMinutes { get; set; } = 60;
    public string? MeetingPlace { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.scheduled;

    public string? StudentName { get; set; }
    public string? InstructorName { get; set; }
    public string? CarModel { get; set; }
    public string? CarPlate { get; set; }
    public string? RouteName { get; set; }
    public string? BranchName { get; set; }
}