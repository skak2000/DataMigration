Data Migration Guide
Overview
The Migration System is built using bulk insert operations to ensure fast and efficient data transfer.
It is designed as a generic, easily adaptable solution for migrating data between databases.

This guide outlines:

- Initial setup

- Data model preparation

- Module execution process

- Error handling and retry logic

1. Initial Setup
Create Basic Tables
Before running migrations, create the following tables by executing the SQL script SQL.txt:

- ModuleRun

- DoneTable

- Customers

- DataMigrationLogger

Scaffold Entity Framework Models
Use the Scaffold-DbContext command to generate Entity Framework models for the tables to be migrated.

Example:
Scaffold-DbContext "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer `
  -Tables ModuleRun, DoneTable, Customers, DataMigrationLogger, StatusOnline_Author, StatusOnline_Chapter, StatusOnline_Story `
  -OutputDir Models -ContextDir Data -Context DataSyncCoreContext -Force

CoreDatabase class -> ReplaceTableNames: Setup your naming scheme if you use a TenantId & Instance naming else remove it

2. Database Design Notes
In this system, each table name contains a TenantId and InstanceId.
Despite different names, all tables share the same structure.

For migration, only include the database model layouts in the solution.

Example:
The following layout tables are used only for structure, not for actual data:

- SimpleAuthors

- SimpleBooks

- SimpleChapters

The actual data resides in tenant-specific tables such as:
StatusOnlineAuthor_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7
StatusOnlineChapter_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7
StatusOnlineStory_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7

3. Module Structure
Each migration module represents one data type (e.g., Author, Story, Chapter) and consists of three steps:

1) Query – Retrieve data from the source database.

2) CreateDTO – Transform and prepare the data for transfer.

3) SendData – Send the data to the target API and store migration results.


4. Priority Levels
Modules have a PriorityLevel to define execution order.

Example Order:

1) Author (PriorityLevel = 1)

2) Story (PriorityLevel = 2)

3) Chapter (PriorityLevel = 3)
You cannot transfer a Story before its Author exists, and you cannot transfer a Chapter before its Story exists.

5. Identifying Unique Records
Each row in the source database must have a unique identifier.
This can be a composite key built from multiple fields.

Examples:
Story Key: story.AuthorNameId.ToString() + "_" + chapter.StoryId.ToString()
Chapter Key: story.StoryId.ToString() + "_" + chapter.ChapterId.ToString()
Store these identifiers in Key1, Key2, and Key3 or in TraceId.

6. Step-by-Step Module Process
6.1 Query
Create a join to retrieve the required data.

- Exclude already transferred rows by joining with the module’s DoneTable.

- Use 1–3 primary keys to uniquely identify each row.

- Each module has its own DoneTable for tracking migration progress.

6.2 CreateDTO
- Map the source data to a Data Transfer Object (DTO) format.

- If related entities are needed (e.g., Author ID for a Story), retrieve them from the corresponding module’s DoneTable.

- If a dependency is missing (e.g., the Author hasn’t been transferred yet), skip the dependent row.

- Prepare the final DTO collection for transfer.

6.3 SendData
- Send the DTOs to the target API.

- Receive and store TraceId and PublicId in the module’s DoneTable.

- Optionally, run VerifyData by retrieving the inserted data to confirm a successful transfer.


7. Error Handling & Retry Logic
When a batch transfer fails:

1) Reduce batch size and retry: 50,000 rows → 25,000 rows → 12,500 rows → … until the failing row is found.

2) Mark failing rows as Failed in the DoneTable.

3) Continue processing the remaining rows.

4) Gradually scale the batch size back up after resolving the issue.


8. Key Best Practices
- Keep PriorityLevel correct to avoid dependency errors.

- Always track processed rows in the DoneTable.

- Use composite keys for unique identification when necessary.

- Implement retry logic to handle partial failures gracefully.

- Use layout tables for scaffolding, but actual migration should use tenant-specific data tables.


# DataMigration

The Migration System is built with bulk insert. The migration solution is generic and easy to adapt.
Run the creation of basic tables (ModuleRun, DoneTable, Customers, DataMigrationLogger) using SQL.txt.
Run Scaffold-DbContext and add the table designs that need to be migrated.

Example: 
Scaffold-DbContext "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer -Tables ModuleRun, DoneTable, Customers, DataMigrationLogger, StatusOnline_Author, StatusOnline_Chapter, StatusOnline_Story -OutputDir Models -ContextDir Data -Context DataSyncCoreContext -Force

In this database design, each table has a TenantId and InstanceId in the name, but they all have the same layout. Add only the database model layout to the solution.

Example:
SimpleAuthors, SimpleBooks, SimpleChapters are only used for getting the layout, but all the data is in:
StatusOnlineAuthor_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7
StatusOnlineChapter_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7
StatusOnlineStory_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7

For each transfer, add a module, request, response, and schema.
For each row, you need a way to identify unique records. It is possible to use a combined key from multiple tables. 
Examples:
To identify a Story: story.AuthorNameId.ToString() + "_" + chapter.StoryId.ToString(),
To identify a Chapter: story.StoryId.ToString() + "_" + chapter.ChapterId.ToString()
Save the info in Key1, Key2, and Key3 or in TraceId.

Each module have 3 steps
- Query:     Get the data from the database.
- CreateDTO: Prepare the data for transfere.
- SendData:  Send the data to the webservice.

Each module has a PriorityLevel that defines the order in which they run.
Example: You cannot transfer a Story without first creating an Author, and you cannot create a Chapter without first creating a Story.
Order: Author -> Story -> Chapter
That’s why PriorityLevel is 1 for Author, 2 for Story, 3 for Chapter.
Remember to set your PriorityLevel correctly!

Query
Create your join for the data you want to transfer. Then join the data with the DoneTable to remove already transferred rows.
You need unique information to identify a row; in this database, you can use the 1–3 primary keys to identify it. Each module will have its own DoneTable to keep track of what has been transferred.

CreateDTO
Collect the information you need to transfer. You can get the TraceId from other models.
Example: When transferring a Story, you need the new PublicId of the Author. You can get that from the Author DoneTable by providing the Author ModuleId and the original AuthorNameId.
If a row from the Author model has not been transferred, then the Story from that Author will not be transferred.

SendData
Send the data to the API and get the TraceId and PublicId. These will be saved in the DoneTable for this module.
If you want to verify that all data has been transferred correctly, you can return the inserted data and run VerifyData.


Rows with problems...
If there is a data problem and one or more rows cannot be transferred, the program will scale down until it finds the problem row, and then increase the batch size again.

Example:
Fail to transfer 50,000 rows → next try 25,000 → then 12,500.
When a single row fails, it will be marked as failed and skipped for now, and processing will continue. Later, the batch size will scale up again.

