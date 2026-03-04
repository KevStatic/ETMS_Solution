USE ETMSsol_DB;
GO

CREATE TABLE OpenPositions (
    PositionId INT IDENTITY(1,1) PRIMARY KEY,
    LocationName NVARCHAR(100),
    DepartmentName NVARCHAR(100),
    VacancyCount INT
);

-- Insert some starting data for the dashboard to read
INSERT INTO OpenPositions (LocationName, DepartmentName, VacancyCount) VALUES 
('Mumbai', 'IT', 12),
('Hazira', 'Civil Engineering', 8),
('Bangalore', 'Metro Projects', 5),
('Dubai', 'Corporate', 3);
GO