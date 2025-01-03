SELECT TOP (1000) [Id]
      ,[Method]
      ,[Endpoint]
      ,[Payload]
      ,[ErrorMessage]
      ,[Timestamp]
      ,[statusCode]
FROM [DotnetDatabase].[dbo].[ErrorLog]

DELETE FROM [DotnetDatabase].[dbo].[ErrorLog];


SELECT *
FROM [DotnetDatabase].[dbo].[ErrorLog]
WHERE ErrorMessage LIKE '%893b45080af7fec4%';