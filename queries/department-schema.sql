SELECT DISTINCT
    CAST([department].[DepartmentID] as nvarchar(MAX)) [OriginKey],
    [department].[Name] [Name],
    [department].[GroupName] [Section]
FROM [HumanResources].[Department] [department]
ORDER BY [department].[Name]