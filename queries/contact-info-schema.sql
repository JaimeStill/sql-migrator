(
    SELECT
        CAST([person].[BusinessEntityID] as nvarchar(MAX)) [OriginEmployeeKey],
        CAST([phone].[PhoneNumber] as nvarchar(MAX)) [Value],
        CAST([phoneType].[Name] as nvarchar(MAX)) [ContactType]
    FROM [Person].[Person] [person]
    LEFT JOIN [Person].[PersonPhone] [phone]
    ON [person].[BusinessEntityID] = [phone].[BusinessEntityID]
    LEFT JOIN [Person].[PhoneNumberType] [phoneType]
    ON [phone].[PhoneNumberTypeID] = [phoneType].[PhoneNumberTypeID]
)
UNION
(
    SELECT
        CAST([person].[BusinessEntityID] as nvarchar(MAX)) [OriginEmployeeKey],
        CAST([email].[EmailAddress] as nvarchar(MAX)) [Value],
        CAST('Email' as nvarchar(MAX)) [ContactType]
    FROM [Person].[Person] [person]
    LEFT JOIN [Person].[EmailAddress] [email]
    ON [person].[BusinessEntityID] = [email].[BusinessEntityID]
)
ORDER BY [Value]