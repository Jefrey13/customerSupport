using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public Guid ContactId { get; set; }

    public Guid? AssignedAgent { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual User? AssignedAgentNavigation { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
