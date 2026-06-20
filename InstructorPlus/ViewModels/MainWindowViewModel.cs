using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstroctorPlus.Views;
// using InstroctorPlus.Views;
using InstructorPlus.Models;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using InstructorPlus.Repositories;
using InstructorPlus.Services;
using InstructorPlus.Views;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace InstructorPlus.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LessonRepository _lessonRepository;
    private readonly StudentRepository _studentRepository;
    private readonly CancellationRepository _cancellationRepository;
    private readonly BranchRepository _branchRepository;
    private readonly InstructorRepository _instructorRepository;
    private readonly StatsService _statsService;
    private readonly NavigationService _navigation;
    
    private readonly User _currentUser;
    private readonly Instructor? _instructor;
    private readonly Admin? _admin;
    
    [ObservableProperty] private Lesson _selectedLesson;
    [ObservableProperty] private Student _selectedStudent;

    [ObservableProperty] private ObservableCollection<Lesson> _lessons = new();
    [ObservableProperty] private int _currentPageSize = 20;
    [ObservableProperty] private List<int> _pageSizes = new([5, 15, 25]);
    [ObservableProperty] private string _pageInfo;
    [ObservableProperty] private bool _canGoNext;
    [ObservableProperty] private bool _canGoPrev;
    
    [ObservableProperty] private ObservableCollection<Student> _students = new();
    [ObservableProperty] private int _currentPageSizeStudent = 20;
    [ObservableProperty] private List<int> _pageSizesStudent = new([5, 15, 25]);
    [ObservableProperty] private string _pageInfoStudent;
    [ObservableProperty] private bool _canGoNextStudent;
    [ObservableProperty] private bool _canGoPrevStudent;

    [ObservableProperty] private string _studentsText;
    [ObservableProperty] private ObservableCollection<Branch> _branches;
    [ObservableProperty] private ObservableCollection<Instructor> _instructors;
    
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isInstructor;
    
    private int _currentPage;
    private int _totalPages;
    
    private int _currentPageStudent;
    private int _totalPagesStudent;

    partial void OnCurrentPageSizeChanged(int value) => LoadPage(0);
    
    public MainWindowViewModel(
        IServiceProvider serviceProvider,
        LessonRepository lessonRepository,
        StudentRepository studentRepository,
        CancellationRepository cancellationRepository,
        BranchRepository branchRepository,
        InstructorRepository instructorRepository,
        StatsService statsService,
        NavigationService navigation,
        object user)
    {
        _serviceProvider = serviceProvider;
        _lessonRepository = lessonRepository;
        _studentRepository = studentRepository;
        _cancellationRepository = cancellationRepository;
        _branchRepository = branchRepository;
        _instructorRepository = instructorRepository;
        
        _statsService = statsService;
        _navigation = navigation;

        if (user is Instructor instructor)
        {
            _instructor = instructor;
            _currentUser = instructor;
            IsInstructor = true;
            StudentsText = "Мои ученики";
            
            GetInstructorStatistic();
        }
        else if (user is Admin admin)
        {
            _admin = admin;
            _currentUser = admin;
            IsAdmin = true;
            StudentsText = "Ученики";
            RefreshBranches();
            RefreshInstructor();
        }
        LoadPage(0);
        LoadPageSudents(0);
    }
    
    private void LoadPage(int page)
    {
        _currentPage = page;

        int totalCount;
        List<Lesson> pageItems;

        if (IsInstructor)
        {
            totalCount = _lessonRepository.GetCountByInstructor(_instructor!.Id);
            pageItems = _lessonRepository.GetPageByInstructor(_instructor.Id, _currentPage, CurrentPageSize);
        }
        else
        {
            totalCount = _lessonRepository.GetCountByBranch(_admin!.BranchId);
            pageItems = _lessonRepository.GetPageByBranch(_admin.BranchId, _currentPage, CurrentPageSize);
        }

        _totalPages = (int)Math.Ceiling((double)totalCount / CurrentPageSize);

        Lessons.Clear();
        foreach (var lesson in pageItems)
            Lessons.Add(lesson);

        PageInfo = $"Страница {_currentPage + 1} из {_totalPages}";
        CanGoPrev = _currentPage > 0;
        CanGoNext = _currentPage < _totalPages - 1;
    }
    
    public void RefreshLessons() => LoadPage(_currentPage);

    [RelayCommand] private void NextPage() { if (_currentPage < _totalPages - 1) LoadPage(_currentPage + 1); }
    [RelayCommand] private void PrevPage() { if (_currentPage > 0) LoadPage(_currentPage - 1); }
    [RelayCommand] private void FirstPage() => LoadPage(0);
    [RelayCommand] private void LastPage() => LoadPage(_totalPages - 1);
   
    
    [RelayCommand]
    public void ExitProgram()
    {
        _navigation.CloseApplication();
    }
    
    //-------------------------------------------
    
    private void LoadPageSudents(int page)
    {
        _currentPageStudent = page;

        int totalCount;
        List<Student> pageItems;

        if (IsInstructor)
        {
            totalCount = _studentRepository.GetCountByBranch(_instructor.BranchId);
            pageItems = _studentRepository.GetPageByBranch(_instructor.BranchId, _currentPage, CurrentPageSize);
        }
        else
        {
            totalCount = _studentRepository.GetCountByBranch(_admin!.BranchId);
            pageItems = _studentRepository.GetPageByBranch(_admin.BranchId, _currentPage, CurrentPageSize);
        }

        _totalPagesStudent = (int)Math.Ceiling((double)totalCount / CurrentPageSizeStudent);

        Students.Clear();
        foreach (var student in pageItems)
            Students.Add(student);

        PageInfoStudent = $"Страница {_currentPageStudent + 1} из {_totalPagesStudent}";
        CanGoPrevStudent = _currentPageStudent > 0;
        CanGoNextStudent = _currentPageStudent < _totalPagesStudent - 1;
    }

    public void RefreshStudents() => LoadPageSudents(_currentPage);

    [RelayCommand] private void NextPageStudent() { if (_currentPageStudent < _totalPagesStudent - 1) LoadPageSudents(_currentPageStudent + 1); }
    [RelayCommand] private void PrevPageStudent() { if (_currentPageStudent > 0) LoadPageSudents(_currentPageStudent - 1); }
    [RelayCommand] private void FirstPageStudent() => LoadPageSudents(0);
    [RelayCommand] private void LastPageStudent() => LoadPageSudents(_totalPagesStudent - 1);

    //-------------------------------------------
    //Student
    [RelayCommand]
    public async void OpenStudentStatistic()
    {         
        if (SelectedStudent is null)
        {
            await _navigation.ShowError("Ошибка", "Не выбран ученик!");
            return;
        }

        var win = _serviceProvider.GetRequiredService<StudentStatisticWindow>();
        var vm = ActivatorUtilities.CreateInstance<StudentStatisticViewModel>(_serviceProvider, SelectedStudent);
        win.DataContext = vm;
        win.Show();
    }

    [RelayCommand]
    public void AddStudent()
    {
        var newStudent = new Student
        {
            Id = 0,
            FullName = "",
            Phone = "",
            TelegramId = 0,
            BranchId = 0,
            TotalHoursRequired = 0,
            HoursCompleted = 0,
            ExamTheoryInternal = false,
            ExamPracticeInternal = false,
            EnrollmentDate = DateTime.Now.Date,
        };
        OpenEditStudentWindow(newStudent);

    } 
    
    [RelayCommand]
    public async void DeleteStudent()
    {
        if (SelectedStudent is null)
        {
            await _navigation.ShowError("Ошибка", "Не выбран ученик!");
            return;
        }
        
        var result = await _navigation.ShowConfirm("Подтверждение", "Вы точно хотите удалить ученика?");
        
        if (result == ButtonResult.Yes)
        {
            _studentRepository.Delete(SelectedStudent.Id);
        }
        RefreshStudents();
    } 

    private void OpenEditStudentWindow(Student student)
    {
        var win = _serviceProvider.GetRequiredService<StudentEditWindow>();
        var vm = ActivatorUtilities.CreateInstance<StudentEditViewModel>(_serviceProvider, student);
        win.DataContext = vm;
        win.Closing += (_, _) => RefreshStudents();
        win.Show();
    }

    [RelayCommand]
    public async void EditStudent()
    {   
        if (SelectedStudent == null)
        {
            await _navigation.ShowError("Ошибка", "Не найден такой ученик!");
            return;
        }
        OpenEditStudentWindow(SelectedStudent);
    }

    //-------------------------------------------
    //Lesson
    [RelayCommand]
    public void OpenAddLesson()
    {
        var lesson = new Lesson
        {
            Id = 0,
            InstructorId = _instructor!.Id,
            CarId = _instructor.CarId ?? 0,
            BranchId = _instructor!.BranchId,
            LessonDate = DateTime.Now,
        };
        OpenEditLessonWindow(lesson);
    }

    private void OpenEditLessonWindow(Lesson lesson)
    {
        var win = _serviceProvider.GetRequiredService<LessonEditWindow>();
        var vm = ActivatorUtilities.CreateInstance<LessonEditViewModel>(_serviceProvider, lesson, _instructor!);
        win.DataContext = vm;
        win.Closing += (_, _) => RefreshLessons();
        win.Show();
    }

    [RelayCommand]
    public async void DeleteLesson()
    {
        if (SelectedLesson is null)
        {
            await _navigation.ShowError("Ошибка", "Не выбрано занятие!");
            return;
        }
        if (SelectedLesson.Status == LessonStatus.cancelled)
        {
            await _navigation.ShowError("Ошибка", "Занятие уже отменено!");
            return;
        }

        var result = await _navigation.ShowConfirm("Подтверждение", "Вы точно хотите отменить занятие?");
        
        if (result == ButtonResult.Yes)
        {
            _lessonRepository.Cancel(SelectedLesson.Id);
            _cancellationRepository.Add(new Cancellation
            {
                LessonId = SelectedLesson.Id,
                CancelledBy = CancelledBy.instructor,
                Reason = "Отменил инструктор",
                CancelledAt = DateTime.Now
            });
        }
        RefreshLessons();
    }

    //-------------------------------------------
    //Branch

    [RelayCommand]
    public async void EditBranch(Branch branch)
    {   
        if (branch == null)
        {
            await _navigation.ShowError("Ошибка", "Не найден такой филиал!");
            return;
        }
        OpenEditBranchWindow(branch);
    }
    
    [RelayCommand]
    public void AddBranch()
    {
        var newBrach = new Branch
        {
            Id = 0,
            Name = "",
            Address = "",
            Phone = ""
        };
        OpenEditBranchWindow(newBrach);
    }

    private void OpenEditBranchWindow(Branch branch)
    {
        var win = _serviceProvider.GetRequiredService<BranchEditWindow>();
        var vm = ActivatorUtilities.CreateInstance<BranchEditViewModel>(_serviceProvider, branch);
        win.DataContext = vm;
        win.Closing += (_, _) => RefreshBranches();;
        win.Show();
    }

    public void RefreshBranches() => Branches = _branchRepository.GetAll();

    //-------------------------------------------
    //Instructor

    [RelayCommand]
    public async void EditInstructor(Instructor instructor)
    {   
        if (instructor == null)
        {
            await _navigation.ShowError("Ошибка", "Не найден такой инструктор!");
            return;
        }
        OpenEditInstructorWindow(instructor);
    }
    
    [RelayCommand]
    public void AddInstructor()
    {
        var newInstructor = new Instructor
        {
            Id = 0,
            Login = "",
            Password = "",
            Phone = "",
            TelegramId = 0,
            BranchId = 0,
            CarId = 0,
            HireDate =  DateTime.Now
        };
        OpenEditInstructorWindow(newInstructor);
    }

    private void OpenEditInstructorWindow(Instructor instructor)
    {
        var win = _serviceProvider.GetRequiredService<InstructorEditWindow>();
        var vm = ActivatorUtilities.CreateInstance<InstructorEditViewModel>(_serviceProvider, instructor);
        win.DataContext = vm;
        win.Closing += (_, _) => RefreshInstructor();;
        win.Show();
    }

    public void RefreshInstructor() => Instructors = _instructorRepository.GetAll();


    //---------------------------------------------------------------------------
    //Statistic
    [ObservableProperty] private string _maxHoursString;
    
    [ObservableProperty] private string _minHoursString;
    
    [ObservableProperty] private string _mostFrequentCar;
    [ObservableProperty] private string _mostFrequentRoute;
    [ObservableProperty] private double _avgPerMonth;
    [ObservableProperty] private int _curMonthLessons;
    
    private void GetInstructorStatistic()
    {
        if (_instructor == null) return;
        
        var maxStats = _statsService.GetMaxHoursToExam(_instructor.BranchId);
        if (maxStats != null)
        {
            MaxHoursString = $"{maxStats.Value.FullName} ({maxStats.Value.Hours} ч.)";
        }
        
        var minStats = _statsService.GetMinHoursToExam(_instructor.BranchId);
        if (minStats != null)
        {
            MinHoursString = $"{minStats.Value.FullName} ({minStats.Value.Hours} ч.)";
        }
        
        MostFrequentCar = _lessonRepository.GetAll()
            .Where(l => l.InstructorId == _instructor.Id)
            .GroupBy(l => new { l.CarModel, l.CarPlate })
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key.CarModel} ({g.Key.CarPlate}) — {g.Count()} занятий")
            .FirstOrDefault() ?? "Нет данных";
        
        MostFrequentRoute = _lessonRepository.GetAll()
            .Where(l => l.InstructorId == _instructor.Id)
            .GroupBy(l => l.RouteName)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key} — {g.Count()} занятий")
            .FirstOrDefault() ?? "Нет данных";

        AvgPerMonth = Math.Round(_lessonRepository.GetAll()
            .GroupBy(l => new { l.LessonDate.Year, l.LessonDate.Month })
            .Select(g => g.Count())
            .DefaultIfEmpty(0)
            .Average(),1); 
        
        CurMonthLessons = _lessonRepository.GetAll()
            .Count(l => l.LessonDate.Month == DateTime.Now.Month && l.LessonDate.Year == DateTime.Now.Year);
    }
}