<<<<<<< HEAD
﻿using System;

namespace ETMS.Domain.Entities
=======
﻿namespace ETMS.Domain.Entities
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
{
    public class TransferRequest
    {
        public int TransferRequestId { get; set; }
        public int EmployeeId { get; set; }
<<<<<<< HEAD
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
=======
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
        public int FromDepartmentId { get; set; }
        public int ToDepartmentId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
<<<<<<< HEAD
        public bool IsActive { get; set; }
=======
        public int? OldManagerId { get; set; }
        public int? NewManagerId { get; set; }
        public string TransferType { get; set; } = "Permanent";
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ExpectedRelievingDate { get; set; }
        public DateTime? ExpectedJoiningDate { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsActive { get; set; } = true;
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
    }
}
