using Requests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Interfaces
{
    public interface IBorrowRepository
    {
        Task<bool> AddBorrowRequestAsync(BorrowRequest request);
        Task<IList<BorrowRequest>> GetAllBorrowRequestsAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize);
        Task<int> GetTotalBorrowRequestsCountAsync(int? employeeId, bool? isSeenByHR);
        Task<BorrowRequest?> GetBorrowRequestByIdAsync(int id);
        Task<bool> UpdateBorrowRequestAsync(BorrowRequest request);
        Task<int> GetUnseenBorrowCountAsync(string role);
        Task<bool> AddInstallmentBorrowAsync(InstallmentBorrow borrow);
        Task<IList<InstallmentBorrow>> GetActiveInstallmentBorrowsAsync();
        Task<IList<InstallmentBorrow>> GetInstallmentBorrowsByEmployeeAsync(int employeeId);
        Task<bool> UpdateInstallmentBorrowAsync(InstallmentBorrow borrow);
    }
}
