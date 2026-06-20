using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstructorPlus.Models;
using InstructorPlus.Repositories;
using InstructorPlus.Services;

namespace InstructorPlus.ViewModels;

public partial class StudentEditViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StudentRepository _studentRepository;
    private readonly BranchRepository _branchRepository;
    private readonly NavigationService _navigation;
    
    [ObservableProperty] private List<Student> _students;
    [ObservableProperty] private Student _selectedStudent;
    [ObservableProperty] private ObservableCollection<Branch> _branches;
    [ObservableProperty] private Branch _selectedBranch;
    [ObservableProperty] private string _selectedName;
    [ObservableProperty] private string _selectedPhone;
    [ObservableProperty] private long? _selectedTelegramId;
    [ObservableProperty] private int _selectedTotalHoursRequired;
    [ObservableProperty] private int _selectedHoursCompleted;
    [ObservableProperty] private bool _isExamTheoryInternal;
    [ObservableProperty] private bool _isExamPracticeInternal;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StudentDate))]
    private DateTimeOffset _selectedDate = DateTimeOffset.Now;
    
    public DateTime StudentDate =>  SelectedDate.Date;
    
    public StudentEditViewModel(
        IServiceProvider serviceProvider,
        StudentRepository studentRepository,
        BranchRepository branchRepository,
        NavigationService navigation,
        Student selectedStudent)
    {
        _serviceProvider = serviceProvider;
        _studentRepository = studentRepository;
        _branchRepository = branchRepository;
        
        _navigation = navigation;
        
        _selectedStudent = selectedStudent;
        
        SelectedName = selectedStudent.FullName;
        SelectedPhone = selectedStudent.Phone;
        SelectedTelegramId = selectedStudent.TelegramId;
        SelectedBranch = _branchRepository.GetById(selectedStudent.BranchId);
        SelectedTotalHoursRequired = selectedStudent.TotalHoursRequired;
        SelectedHoursCompleted = selectedStudent.HoursCompleted;
        IsExamTheoryInternal = selectedStudent.ExamTheoryInternal;
        IsExamPracticeInternal = _isExamTheoryInternal;
        
        LoadData();
    }

    private void LoadData()
    {
        Branches = _branchRepository.GetAll();
    }

    [RelayCommand]
    public void CancelCommand()
    {
        _navigation.CloseWindow();
    }

    [RelayCommand]
    public async void SaveCommand()
    {
        if (SelectedName is null)
        {
            await _navigation.ShowError("Ошибка", "Нет указано имя!");
            return;
        }

        if (SelectedPhone is null)
        {
            await _navigation.ShowError("Ошибка", "Не указан телефон!");
            return;
        }

        if (StudentDate > DateTime.Now)
        {
            await _navigation.ShowError("Ошибка", "Неверно выбрана дата!");
            return;
        }

        if (SelectedBranch is null)
        {
            await _navigation.ShowError("Ошибка", "Не выбран филиал!");
            return;
        }

        var newStudent = new Student
        {
            Id = SelectedStudent.Id,
            FullName = SelectedName,
            Phone = SelectedPhone,
            TelegramId = SelectedTelegramId,
            BranchId = SelectedBranch.Id,
            TotalHoursRequired = SelectedTotalHoursRequired,
            HoursCompleted = SelectedHoursCompleted,
            ExamTheoryInternal = IsExamTheoryInternal,
            ExamPracticeInternal = IsExamPracticeInternal,
            EnrollmentDate = StudentDate
        };
        
        if(SelectedStudent.Id == 0)
            _studentRepository.Add(newStudent);
        else
            _studentRepository.Update(newStudent);
        _navigation.CloseWindow();
    }
}