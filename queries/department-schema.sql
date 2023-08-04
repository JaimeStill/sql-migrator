SELECT DISTINCT
    CAST([department].[DepartmentID] as nvarchar(MAX)) [SourceId],
    [department].[Name] [Name],
    [department].[GroupName] [Section]
FROM [HumanResources].[Department] [department]
ORDER BY [department].[Name]