SELECT TOP (1000) *
FROM [DotnetDatabase].[dbo].[ErrorLog]

DELETE FROM [DotnetDatabase].[dbo].[ErrorLog];


SELECT *
FROM [DotnetDatabase].[dbo].[ErrorLog]
WHERE TraceId LIKE '%69d0f69d3e3720a9c19a40fd867eca71%';