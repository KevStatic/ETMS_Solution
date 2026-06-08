using System;
using System.Collections.Generic;
using System.Text;

namespace ETMS.Application.DTOs.Approval
{
    public class ApprovalTrailDto
    {
        public string ApproverRole { get; set; }
        public string ApproverName { get; set; }
        public string ApprovalStatus { get; set; }
        public string Comments { get; set; }
        public DateTime? ActionDate { get; set; }
    }
}
