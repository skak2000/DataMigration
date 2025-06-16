using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class StatusOnlineChapter
{
    public int ChapterId { get; set; }

    public string? ChapterTitle { get; set; }

    public string? ChapterText { get; set; }

    public string? ChapterUrl { get; set; }

    public string? ChapterTags { get; set; }

    public int? ChapterNextChapter { get; set; }

    public int? ChapterPreChapter { get; set; }

    public int? StoryId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual StatusOnlineStory? Story { get; set; }
}
