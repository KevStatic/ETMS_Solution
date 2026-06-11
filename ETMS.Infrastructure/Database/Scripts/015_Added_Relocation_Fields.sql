USE ETMSsol_DB;
GO

-- Relocation & Reimbursement fields
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'TravelMode')
    ALTER TABLE TransferRequests ADD TravelMode          NVARCHAR(50)  NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'TravelClass')
    ALTER TABLE TransferRequests ADD TravelClass         NVARCHAR(50)  NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'RelocationAllowance')
    ALTER TABLE TransferRequests ADD RelocationAllowance NVARCHAR(3)   NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'AccommodationRequired')
    ALTER TABLE TransferRequests ADD AccommodationRequired NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'DependentsCount')
    ALTER TABLE TransferRequests ADD DependentsCount     INT           NULL;

-- Handover & Transition fields
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'NoticePeriodWeeks')
    ALTER TABLE TransferRequests ADD NoticePeriodWeeks   INT           NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'CurrentTaskStatus')
    ALTER TABLE TransferRequests ADD CurrentTaskStatus   NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'HandoverPlan')
    ALTER TABLE TransferRequests ADD HandoverPlan        NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TransferRequests') AND name = 'KnowledgeTransferReqd')
    ALTER TABLE TransferRequests ADD KnowledgeTransferReqd NVARCHAR(3) NULL;
GO
