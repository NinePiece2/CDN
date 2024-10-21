using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CDN.Data.Tables;

public class CDNUser : IdentityUser
{
    [MaxLength(255)]
    public string? Name { get; set; }
    public bool AccountApproved { get; set; }
}

