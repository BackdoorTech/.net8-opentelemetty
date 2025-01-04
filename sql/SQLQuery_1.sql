SELECT TOP (1000) *
FROM [DotnetDatabase].[dbo].[ErrorLog]

DELETE FROM [DotnetDatabase].[dbo].[ErrorLog];


SELECT *
FROM [DotnetDatabase].[dbo].[ErrorLog]
WHERE TraceId LIKE '%c4065e2fccf7c0ff59011c4a253eb7b0%';