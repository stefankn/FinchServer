namespace FinchServer.Controllers;

public class Pager<T>(T[] items, int page, int total, int limit) {
    
    // - Properties
    
    public PagerMetadata Metadata { get; set; } = new() {
        Page = page,
        Total = total,
        Per = limit
    };

    public T[] Items { get; set; } = items;
}

public class PagerMetadata {
    
    // - Properties
    
    public int Page { get; set; }
    public int Total { get; set; }
    public int Per { get; set; }
}