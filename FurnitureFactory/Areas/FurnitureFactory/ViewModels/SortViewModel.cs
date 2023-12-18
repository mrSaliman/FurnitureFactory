namespace FurnitureFactory.Areas.FurnitureFactory.ViewModels;

public class SortViewModel
{
    public string CurrentSortField { get; set; }
    public bool IsAscending { get; set; }

    public SortViewModel(string currentSortField, bool isAscending)
    {
        CurrentSortField = currentSortField;
        IsAscending = isAscending;
    }

    public string GetOrientedSortOrder(string sortField)
    {
        if (string.IsNullOrEmpty(sortField)) return CurrentSortField == "" ? "" :  $"{CurrentSortField} {(IsAscending ? "asc" : "desc")}";
        if (sortField == CurrentSortField)
        {
            IsAscending = !IsAscending;
            return $"{CurrentSortField} {(IsAscending ? "asc" : "desc")}";
        }

        CurrentSortField = sortField;
        IsAscending = true;
        return $"{CurrentSortField} asc";
    }
}