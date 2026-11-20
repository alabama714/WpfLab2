using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WpfLab2.Models;
using WpfLab2.Infrastructure;
using WpfLab2.Services;

namespace WpfLab2.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    private int _nextId = 10;
    private WorkItem? _clipboardItem;
    private string _selectedTheme = "Светлая";
    private string _selectedLanguage = "Русский";
    private string _selectedFontFamily = "Segoe UI";
    private double _fontSize = 14;
    private WorkItem? _selectedSourceRow;
    private WorkItem? _selectedDestinationRow;
    private string _statusMessage = "Готово. Данные можно редактировать в таблицах.";
    private string _clipboardStatus = "Буфер обмена пуст.";

    private string _oneWayText = "Исходная строка для режима OneWay";
    private string _twoWayText = "Измените этот текст — значение обновится в модели";
    private string _oneWayToSourceInput = "";
    private string _oneTimeValue = "Исходное значение OneTime";
    private string _propertyChangedText = "Изменяйте текст — источник обновляется сразу";
    private string _lostFocusText = "Источник обновится после ухода фокуса";
    private string _defaultTriggerText = "Для TextBox значение по умолчанию обновляется при потере фокуса";
    private string _explicitText = "Нажмите в поле и измените меня, затем подтвердите";

    public MainViewModel(ThemeService themeService)
    {
        _themeService = themeService;

        AvailableThemes = new ObservableCollection<string>
        {
            "Светлая", "Тёмная", "Высокий контраст"
        };
        AvailableLanguages = new ObservableCollection<string>
        {
            "Русский", "English", "Deutsch"
        };
        AvailableFonts = new ObservableCollection<string>
        {
            "Segoe UI", "Arial", "Calibri", "Consolas", "Times New Roman"
        };

        SourceRows = new ObservableCollection<WorkItem>
        {
            new(1, "Кнопка подтверждения", "Button", "Готово", "Обычная кнопка интерфейса"),
            new(2, "Выбор темы", "ComboBox", "Активно", "Раскрывающийся список тем"),
            new(3, "Табличные данные", "DataGrid", "На проверке", "Таблица с редактируемыми строками"),
            new(4, "Индикатор", "ProgressBar", "Готово", "Отображение прогресса")
        };

        DestinationRows = new ObservableCollection<WorkItem>
        {
            new(5, "Демонстрация дерева", "TreeView", "Готово", "Иерархический список"),
            new(6, "Контейнер", "Grid", "Черновик", "Размещение по строкам и столбцам")
        };

        _nextId = SourceRows.Concat(DestinationRows).Max(item => item.Id) + 1;

        AddSourceRowCommand = new RelayCommand(_ => AddSourceRow());
        DeleteSourceRowCommand = new RelayCommand(_ => DeleteSourceRow(), _ => SelectedSourceRow is not null);
        AddDestinationRowCommand = new RelayCommand(_ => AddDestinationRow());
        DeleteDestinationRowCommand = new RelayCommand(_ => DeleteDestinationRow(), _ => SelectedDestinationRow is not null);
        CopySourceCommand = new RelayCommand(_ => CopySource(), _ => SelectedSourceRow is not null);
        PasteToDestinationCommand = new RelayCommand(_ => PasteToDestination(), _ => _clipboardItem is not null);
        CopyDestinationCommand = new RelayCommand(_ => CopyDestination(), _ => SelectedDestinationRow is not null);
        PasteToSourceCommand = new RelayCommand(_ => PasteToSource(), _ => _clipboardItem is not null);

        ChangeOneWaySourceCommand = new RelayCommand(_ =>
            OneWayText = $"Значение источника изменено: {DateTime.Now:T}");
        ChangeOneTimeValueCommand = new RelayCommand(_ =>
            OneTimeValue = $"Новое значение источника: {DateTime.Now:T}");
        CommitExplicitBindingCommand = new RelayCommand(CommitExplicitBinding, _ => true);
        ResetBindingDemoCommand = new RelayCommand(_ => ResetBindingDemo());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        SetLightThemeCommand = new RelayCommand(_ => SelectedTheme = "Светлая");
        SetDarkThemeCommand = new RelayCommand(_ => SelectedTheme = "Тёмная");
    }

    public ObservableCollection<string> AvailableThemes { get; }
    public ObservableCollection<string> AvailableLanguages { get; }
    public ObservableCollection<string> AvailableFonts { get; }
    public ObservableCollection<WorkItem> SourceRows { get; }
    public ObservableCollection<WorkItem> DestinationRows { get; }

    public ICommand AddSourceRowCommand { get; }
    public ICommand DeleteSourceRowCommand { get; }
    public ICommand AddDestinationRowCommand { get; }
    public ICommand DeleteDestinationRowCommand { get; }
    public ICommand CopySourceCommand { get; }
    public ICommand PasteToDestinationCommand { get; }
    public ICommand CopyDestinationCommand { get; }
    public ICommand PasteToSourceCommand { get; }
    public ICommand ChangeOneWaySourceCommand { get; }
    public ICommand ChangeOneTimeValueCommand { get; }
    public ICommand CommitExplicitBindingCommand { get; }
    public ICommand ResetBindingDemoCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand SetLightThemeCommand { get; }
    public ICommand SetDarkThemeCommand { get; }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                _themeService.ApplyTheme(value);
                StatusMessage = $"Выбрана тема: {value}";
            }
        }
    }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public string SelectedFontFamily
    {
        get => _selectedFontFamily;
        set => SetProperty(ref _selectedFontFamily, value);
    }

    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    public WorkItem? SelectedSourceRow
    {
        get => _selectedSourceRow;
        set
        {
            if (SetProperty(ref _selectedSourceRow, value))
            {
                RaiseCommandCanExecuteChanged();
            }
        }
    }

    public WorkItem? SelectedDestinationRow
    {
        get => _selectedDestinationRow;
        set
        {
            if (SetProperty(ref _selectedDestinationRow, value))
            {
                RaiseCommandCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ClipboardStatus
    {
        get => _clipboardStatus;
        set => SetProperty(ref _clipboardStatus, value);
    }

    public string OneWayText
    {
        get => _oneWayText;
        set => SetProperty(ref _oneWayText, value);
    }

    public string TwoWayText
    {
        get => _twoWayText;
        set => SetProperty(ref _twoWayText, value);
    }

    public string OneWayToSourceInput
    {
        get => _oneWayToSourceInput;
        set => SetProperty(ref _oneWayToSourceInput, value);
    }

    public string OneTimeValue
    {
        get => _oneTimeValue;
        set => SetProperty(ref _oneTimeValue, value);
    }

    public string PropertyChangedText
    {
        get => _propertyChangedText;
        set => SetProperty(ref _propertyChangedText, value);
    }

    public string LostFocusText
    {
        get => _lostFocusText;
        set => SetProperty(ref _lostFocusText, value);
    }

    public string DefaultTriggerText
    {
        get => _defaultTriggerText;
        set => SetProperty(ref _defaultTriggerText, value);
    }

    public string ExplicitText
    {
        get => _explicitText;
        set => SetProperty(ref _explicitText, value);
    }

    private void AddSourceRow()
    {
        var item = new WorkItem(_nextId++, "Новая запись", "Не указано", "Черновик", "Добавлено пользователем");
        SourceRows.Add(item);
        SelectedSourceRow = item;
        StatusMessage = "Добавлена строка в таблицу 1.";
    }

    private void DeleteSourceRow()
    {
        if (SelectedSourceRow is null) return;
        SourceRows.Remove(SelectedSourceRow);
        SelectedSourceRow = null;
        StatusMessage = "Выбранная строка удалена из таблицы 1.";
    }

    private void AddDestinationRow()
    {
        var item = new WorkItem(_nextId++, "Новая запись", "Не указано", "Черновик", "Добавлено пользователем");
        DestinationRows.Add(item);
        SelectedDestinationRow = item;
        StatusMessage = "Добавлена строка в таблицу 2.";
    }

    private void DeleteDestinationRow()
    {
        if (SelectedDestinationRow is null) return;
        DestinationRows.Remove(SelectedDestinationRow);
        SelectedDestinationRow = null;
        StatusMessage = "Выбранная строка удалена из таблицы 2.";
    }

    private void CopySource()
    {
        if (SelectedSourceRow is null) return;
        _clipboardItem = SelectedSourceRow.CopyWithId(SelectedSourceRow.Id);
        ClipboardStatus = $"Скопировано из таблицы 1: «{_clipboardItem.Name}».";
        StatusMessage = ClipboardStatus;
        RaiseCommandCanExecuteChanged();
    }

    private void PasteToDestination()
    {
        if (_clipboardItem is null) return;
        var copy = _clipboardItem.CopyWithId(_nextId++);
        DestinationRows.Add(copy);
        SelectedDestinationRow = copy;
        StatusMessage = $"Запись «{copy.Name}» вставлена в таблицу 2.";
    }

    private void CopyDestination()
    {
        if (SelectedDestinationRow is null) return;
        _clipboardItem = SelectedDestinationRow.CopyWithId(SelectedDestinationRow.Id);
        ClipboardStatus = $"Скопировано из таблицы 2: «{_clipboardItem.Name}».";
        StatusMessage = ClipboardStatus;
        RaiseCommandCanExecuteChanged();
    }

    private void PasteToSource()
    {
        if (_clipboardItem is null) return;
        var copy = _clipboardItem.CopyWithId(_nextId++);
        SourceRows.Add(copy);
        SelectedSourceRow = copy;
        StatusMessage = $"Запись «{copy.Name}» вставлена в таблицу 1.";
    }

    private void CommitExplicitBinding(object? parameter)
    {
        if (parameter is not TextBox textBox) return;

        BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty)?.UpdateSource();
        StatusMessage = "Источник обновлён вручную: UpdateSource() для режима Explicit.";
    }

    private void ResetBindingDemo()
    {
        OneWayText = "Исходная строка для режима OneWay";
        TwoWayText = "Измените этот текст — значение обновится в модели";
        OneWayToSourceInput = string.Empty;
        PropertyChangedText = "Изменяйте текст — источник обновляется сразу";
        LostFocusText = "Источник обновится после ухода фокуса";
        DefaultTriggerText = "Для TextBox значение по умолчанию обновляется при потере фокуса";
        ExplicitText = "Нажмите в поле и измените меня, затем подтвердите";
        StatusMessage = "Примеры привязки сброшены. OneTime обновляется только при создании привязки.";
    }

    private void RaiseCommandCanExecuteChanged()
    {
        (DeleteSourceRowCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CopySourceCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteDestinationRowCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CopyDestinationCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (PasteToDestinationCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (PasteToSourceCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
}
