USE ETMSsol_DB;
GO

ALTER TABLE Locations
ADD LocationName AS (City + ', ' + State);

ALTER TABLE TransferApprovals
ADD Comments AS Remarks;