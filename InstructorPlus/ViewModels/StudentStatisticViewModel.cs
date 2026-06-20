using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using InstructorPlus.Repositories;
using InstructorPlus.Services;

namespace InstructorPlus.ViewModels;

public partial class StudentStatisticViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StudentRepository _studentRepository;
    private readonly LessonRepository _lessonRepository;
    private readonly StatsService _statsService;
    private readonly NavigationService _navigation;
    
    private Action _closeAction;

    [ObservableProperty] private Student _selectedStudent;
    
    
    public StudentStatisticViewModel(IServiceProvider serviceProvider,
        StudentRepository studentRepository,
        LessonRepository lessonRepository,
        StatsService statsService,
        Student selectedStudent,
        NavigationService navigation)
    {
        _serviceProvider = serviceProvider;
        _studentRepository = studentRepository;
        _selectedStudent = selectedStudent;
        _lessonRepository = lessonRepository;
        _statsService = statsService;
        _navigation = navigation;
        
        GetStudentStatistic();
    }

    public void SetCloseAction(Action closeAction)
    {
        _closeAction = closeAction;
    }
    
    [RelayCommand]
    public void CancelCommand()
    {
        _navigation.CloseWindow();
    }

    //-------------------------------------------------------------------------------------
    [ObservableProperty] private string _mostFrequentRoute;
    [ObservableProperty] private string _mostFrequentCar;
    [ObservableProperty] private int _curMonthCount;
    
    [ObservableProperty] private string _isTheoryInternalPassed;
    [ObservableProperty] private string _isPracticeInternalPassed;
    [ObservableProperty] private string _isTheoryGaiPassed;
    [ObservableProperty] private string _isPracticeGaiPassed;
    
    public void GetStudentStatistic()
    {
        MostFrequentRoute = _lessonRepository.GetAll()
            .Where(l => l.StudentId == SelectedStudent.Id && l.RouteName != null)
            .GroupBy(l => l.RouteName)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key} - {g.Count()} раз")
            .FirstOrDefault() ?? "Нет данных";
        
        MostFrequentCar = _lessonRepository.GetAll()
            .Where(l => l.StudentId == SelectedStudent.Id && l.CarModel != null)
            .GroupBy(l => new { l.CarModel, l.CarPlate})
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key.CarModel} ({g.Key.CarPlate}) - {g.Count()} раз")
            .FirstOrDefault() ?? "Нет данных";
        
        CurMonthCount = _lessonRepository.GetAll()
                .Count(l => l.StudentId == SelectedStudent.Id
                    && l.LessonDate.Year == DateTime.Now.Year
                    && l.LessonDate.Month == DateTime.Now.Month
                    && l.Status == LessonStatus.completed);

        IsTheoryInternalPassed = SelectedStudent.ExamTheoryInternal ? "✅ Сдана" : "❌ Не сдана";
        IsPracticeInternalPassed = SelectedStudent.ExamPracticeInternal ? "✅ Сдана" : "❌ Не сдана";
        IsTheoryGaiPassed = _statsService.GetTheoryGaiStatus(SelectedStudent.Id);
        IsPracticeGaiPassed = _statsService.GetPracticeGaiStatus(SelectedStudent.Id);

    }
    
}