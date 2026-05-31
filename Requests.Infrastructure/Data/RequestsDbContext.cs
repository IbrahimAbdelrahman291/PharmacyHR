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
        public DbSet<HolidayRequest> HolidayRequests { get; set; }
        public DbSet<BorrowRequest> BorrowRequests { get; set; }
        public DbSet<InstallmentBorrow> InstallmentBorrows { get; set; }
        public DbSet<OvertimeRequest> OvertimeRequests { get; set; }
    }
}
