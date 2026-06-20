using System.Collections.Generic;
using System.Linq;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using InstructorPlus.Repositories;

namespace InstructorPlus.Services;

public class StatsService
{
    private readonly StudentRepository _studentRepository;
    private readonly ExamRepository _examRepository;

    public StatsService(StudentRepository studentRepository,
        ExamRepository examRepository)
    {
        _studentRepository = studentRepository;
        _examRepository = examRepository;
    }

    public (string FullName, int Hours)? GetMaxHoursToExam(int branchId)
    {
        return _studentRepository.GetByBranch(branchId)
            .Where(s => s.ExamPracticeInternal)
            .Select(s => (s.FullName, s.HoursCompleted))
            .OrderByDescending(s => s.HoursCompleted)
            .FirstOrDefault();
    }

    public (string FullName, int Hours)? GetMinHoursToExam(int branchId)
    {
        return _studentRepository.GetByBranch(branchId)
            .Where(s => s.ExamPracticeInternal)
            .Select(s => (s.FullName, s.HoursCompleted))
            .OrderBy(s => s.HoursCompleted)
            .FirstOrDefault();
    }

    public string GetTheoryGaiStatus(int studentId)
    {
        var last = _examRepository.GetByStudent(studentId)
            .Where(e => e.ExamType == ExamType.theory_gai)
            .OrderByDescending(e => e.AttemptNumber)
            .FirstOrDefault();

        if (last == null) return "⏳ Не начата";
        return last.IsPassed ? $"✅ Сдана (попытка {last.AttemptNumber})" : $"❌ Не сдана (попытка {last.AttemptNumber})";
    }
    
    public string GetPracticeGaiStatus(int studentId)
    {
        var last = _examRepository.GetByStudent(studentId)
            .Where(e => e.ExamType == ExamType.practice_gai)
            .OrderByDescending(e => e.AttemptNumber)
            .FirstOrDefault();

        if (last == null) return "⏳ Не начата";
        return last.IsPassed ? $"✅ Сдана (попытка {last.AttemptNumber})" : $"❌ Не сдана (попытка {last.AttemptNumber})";
    }
}