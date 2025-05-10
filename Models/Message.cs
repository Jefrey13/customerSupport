using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public int SenderId { get; set; }

    public string? Content { get; set; }

    public string MessageType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
