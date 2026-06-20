using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using InstructorPlus.Repositories;
using InstructorPlus.Services;
using MsBox.Avalonia.Enums;

namespace InstructorPlus.ViewModels;

public partial class BranchEditViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationService _navigation;
    private readonly BranchRepository _branchRepository;

    [ObservableProperty] private Branch _branch;
    [ObservableProperty] private string _selectedName;
    [ObservableProperty] private string _selectedAddress;
    [ObservableProperty] private string _selectedPhone;
    [ObservableProperty] private bool _isOldBranch;

    public BranchEditViewModel(
        IServiceProvider serviceProvider,
        NavigationService navigation,
        BranchRepository branchRepository,
        Branch branch)
    {
        _serviceProvider = serviceProvider;
        _navigation = navigation;
        _branchRepository = branchRepository;
        Branch = branch;

        SelectedName = Branch.Name;
        SelectedAddress = Branch.Address;
        SelectedPhone = Branch.Phone;
        
        if (Branch.Id == 0)
            IsOldBranch = false;
        else
            IsOldBranch = true;
    }

    [RelayCommand]
    public void CancelCommand()
    {
        _navigation.CloseWindow();
    }

    [RelayCommand]
    public async void DeleteCommand()
    {
        var result = await _navigation.ShowConfirm("Подтверждение", "Вы точно хотите удалить филиал?");
        
        if (result == ButtonResult.Yes)
        {
            _branchRepository.Delete(Branch.Id);
            _navigation.CloseWindow();
        }
    }

    [RelayCommand]
    public async void SaveCommand()
    {
        if (SelectedName is null)
        {
            await _navigation.ShowError("Ошибка", "Не указано название!");
            return;
        }

        if (SelectedAddress is null)
        {
            await _navigation.ShowError("Ошибка", "Не указан адрес!");
            return;
        }

        var newBranch = new Branch
        {
            Id = Branch.Id,
            Name = SelectedName,
            Address = SelectedAddress,
            Phone = SelectedPhone
        };
        
        if(Branch.Id == 0)
            _branchRepository.Add(newBranch);
        else
            _branchRepository.Update(newBranch);
        _navigation.CloseWindow();
    }

}