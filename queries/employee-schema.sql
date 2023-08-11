SELECT
    CAST([person].[BusinessEntityID] as nvarchar(MAX)) [OriginKey],
    CAST([history].[DepartmentID] as nvarchar(MAX)) [OriginDepartmentKey],
    [employee].[NationalIdNumber] [NationalId],
    [person].[LastName] [LastName],
    [person].[FirstName] [FirstName],
    [person].[MiddleName] [MiddleName],
    [employee].[LoginId] [Login],
    [employee].[JobTitle] [JobTitle]
FROM [Person].[Person] [person]
LEFT JOIN [HumanResources].[Employee] [employee]
ON [person].[BusinessEntityID] = [employee].[BusinessEntityID]
LEFT JOIN [HumanResources].[EmployeeDepartmentHistory] [history]
ON [employee].[BusinessEntityID] = [history].[BusinessEntityID]
WHERE [person].[PersonType] = 'EM'
AND [history].[EndDate] IS NULL
ORDER BY [person].[LastName]