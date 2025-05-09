﻿using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateUserRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [EmailAddress]
        public string? Email { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Identifier { get; set; }

        public string? NewPassword { get; set; }
    }
}