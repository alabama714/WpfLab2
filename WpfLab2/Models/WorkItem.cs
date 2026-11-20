using WpfLab2.Infrastructure;

namespace WpfLab2.Models;

public sealed class WorkItem : ObservableObject
{
    private string _name;
    private string _category;
    private string _state;
    private string _description;

    public WorkItem(int id, string name, string category, string state, string description)
    {
        Id = id;
        _name = name;
        _category = category;
        _state = state;
        _description = description;
    }

    public int Id { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public WorkItem CopyWithId(int newId)
        => new(newId, Name, Category, State, Description);
}
