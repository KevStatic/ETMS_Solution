USE ETMSsol_DB;
GO

-- Insert New Domestic Locations (Country: India)
IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = 'Hazira')
    INSERT INTO Locations (City, State, Country) VALUES ('Hazira', 'Gujarat', 'India');

-- Insert New International Locations
IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = 'Riyadh')
    INSERT INTO Locations (City, State, Country) VALUES ('Riyadh', 'Riyadh Province', 'Saudi Arabia');
IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = 'Dubai')
    INSERT INTO Locations (City, State, Country) VALUES ('Dubai', 'Dubai', 'UAE');
IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = 'Singapore City')
    INSERT INTO Locations (City, State, Country) VALUES ('Singapore City', 'Singapore', 'Singapore');
IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = 'Shanghai')
    INSERT INTO Locations (City, State, Country) VALUES ('Shanghai', 'Shanghai', 'China');
GO