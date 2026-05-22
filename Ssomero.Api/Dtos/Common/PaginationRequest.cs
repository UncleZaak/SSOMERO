namespace Ssomero.Api.DTOs.Common;

public class PaginationRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int Page { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    
    public string? Search { get; set; }
    
    public Dictionary<string, string>? Filters { get; set; }
}
