﻿namespace FurnitureFactory.Areas.FurnitureFactory.ViewModels;

public class PageViewModel
{
    public PageViewModel(int count, int pageNumber = 1, int pageSize = 20)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    public int PageNumber { get; }
    public int TotalPages { get; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}