namespace Willovate.Store.Api.Models;

public class Theme
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsLive { get; set; }
    public DateTime LastEdited { get; set; }

    // Navigation properties
    public Website? Website { get; set; }
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}
