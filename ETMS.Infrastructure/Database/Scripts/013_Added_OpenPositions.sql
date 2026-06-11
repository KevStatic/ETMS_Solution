USE ETMSsol_DB;
GO

IF OBJECT_ID(N'dbo.OpenPositions', N'U') IS NULL
BEGIN
    CREATE TABLE OpenPositions (
        PositionId     INT IDENTITY(1,1) PRIMARY KEY,
        LocationName   NVARCHAR(150) NOT NULL,
        DepartmentName NVARCHAR(100) NOT NULL
    );
END;
GO

-- Seed a few sample open positions so dashboards show real data
IF NOT EXISTS (SELECT 1 FROM OpenPositions)
BEGIN
    INSERT INTO OpenPositions (LocationName, DepartmentName) VALUES
    ('Mumbai, Maharashtra',   'IT'),
    ('Mumbai, Maharashtra',   'Finance'),
    ('Bangalore, Karnataka',  'IT'),
    ('Bangalore, Karnataka',  'HR'),
    ('Mumbai, Maharashtra',   'HR'),
    ('Bangalore, Karnataka',  'Finance');
END;
GO
