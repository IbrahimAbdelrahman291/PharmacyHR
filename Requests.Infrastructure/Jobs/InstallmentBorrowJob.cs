using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;

namespace Requests.Infrastructure.Jobs
{
    public class InstallmentBorrowJob : IInstallmentBorrowJob
    {
        private readonly IBorrowRepository _repository;
        private readonly IMonthlyDataRepository _monthlyDataRepository;

        public InstallmentBorrowJob(
            IBorrowRepository repository,
            IMonthlyDataRepository monthlyDataRepository)
        {
            _repository = repository;
            _monthlyDataRepository = monthlyDataRepository;
        }

        public async Task ProcessAsync()
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var activeBorrows = await _repository.GetActiveInstallmentBorrowsAsync();

            foreach (var borrow in activeBorrows)
            {
                var startDate = new DateTime(borrow.StartYear, borrow.StartMonth, 1);
                if (egyptNow < startDate) continue;

                await _monthlyDataRepository.UpdateInstallmentBorrow(borrow.EmployeeId, borrow.MonthlyAmount);

                borrow.RemainingMonths--;
                if (borrow.RemainingMonths <= 0)
                    borrow.IsActive = false;

                await _repository.UpdateInstallmentBorrowAsync(borrow);
            }
        }
    }
}