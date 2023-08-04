SELECT TOP(10)
	[Person].[BusinessEntityID] [id],
	[Person].[FirstName] [firstName],
	ISNULL(CONVERT(nvarchar(max), [Person].[MiddleName]), '') [middleName],
	[Person].[LastName] [lastName]
FROM [AdventureWorks2022].[Person].[Person]