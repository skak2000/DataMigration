using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class StatusOnlineStory
{
    public int StoryId { get; set; }

    public string? StoryTitle { get; set; }

    public short? StoryCompleted { get; set; }

    public string? StoryUrlSource { get; set; }

    public string? StoryTags { get; set; }

    public int? AuthorNameId { get; set; }

    public bool IsDeleted { get; set; }

    public bool Comic { get; set; }

    public virtual StatusOnlineAuthor? AuthorName { get; set; }

    public virtual ICollection<StatusOnlineChapter> StatusOnlineChapters { get; set; } = new List<StatusOnlineChapter>();
}
