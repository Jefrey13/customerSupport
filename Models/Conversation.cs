﻿using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public int? CompanyId { get; set; }

    public int? ClientUserId { get; set; }

    public int? AssignedAgent { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? AssignedAt { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? AssignedAgentNavigation { get; set; }

    public virtual User? AssignedByNavigation { get; set; }

    public virtual User? ClientUser { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
