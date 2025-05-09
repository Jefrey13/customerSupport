using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class Attachment
{
    public int AttachmentId { get; set; }

    public int MessageId { get; set; }

    public string? MimeType { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public string MediaId { get; set; } = null!;

    public string? FileName { get; set; }

    public string? MediaUrl { get; set; }

    public virtual Message Message { get; set; } = null!;
}
