using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.Interfaces
{
    public interface INewMonthJob
    {
        Task ExecuteAsync();
    }
}
