SELECT
*
FROM [Person].[Person] as person
LEFT JOIN [Person].[BusinessEntityContact] [contact]
ON [person].BusinessEntityID = [contact].PersonID
LEFT JOIN [Person].[EmailAddress] [email]
ON [person].BusinessEntityID = [email].BusinessEntityID
LEFT JOIN [Person].[Password] [password]
ON [person].BusinessEntityID = [password].BusinessEntityID
LEFT JOIN [Person].[PersonPhone] [phone]
ON [person].BusinessEntityID = [phone].BusinessEntityID
LEFT JOIN [HumanResources].[Employee] [employee]
ON [person].BusinessEntityID = [employee].BusinessEntityID
LEFT JOIN [Sales].[Customer] [customer]
ON [person].BusinessEntityID = [customer].PersonID
LEFT JOIN [Sales].[PersonCreditCard] [card]
ON [person].BusinessEntityID = [card].BusinessEntityID