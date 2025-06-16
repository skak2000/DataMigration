using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class StatusOnlineAuthor
{
    public string? AuthorName { get; set; }

    public int AuthorNameId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<StatusOnlineStory> StatusOnlineStories { get; set; } = new List<StatusOnlineStory>();
}
