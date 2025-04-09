﻿using Microsoft.AspNetCore.Identity;

namespace DocumentWebsite.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
