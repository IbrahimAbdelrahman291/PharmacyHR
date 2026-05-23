using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class CreateComplaintDto
    {
        public string Content { get; set; } = string.Empty;
        public string RecipientRole { get; set; } = string.Empty; // HR, AreaManager, CEO
    }
}
