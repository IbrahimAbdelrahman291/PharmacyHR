using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Infrastructure.Data
{
    public class RequestsDbContext : DbContext
    {
        public RequestsDbContext(DbContextOptions<RequestsDbContext> options) : base(options) { }

        public DbSet<ComplaintRequest> ComplaintRequests { get; set; }
        public DbSet<ForgetedHoursRequest> ForgetedHoursRequests { get; set; }

    }
}
