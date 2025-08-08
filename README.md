# DataMigration

The Migration System build with Bulk insert. The migration solution is generic and easy to adapt.
Run create basic tables (ModuleRun, DoneTable, Customers, DataMigrationLogger) with SQL.txt
Run the Scaffold-DbContext and add the tables designes what need to be migrated.

Example: 
Scaffold-DbContext "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer -Tables ModuleRun, DoneTable, Customers, DataMigrationLogger, StatusOnline_Author, StatusOnline_Chapter, StatusOnline_Story -OutputDir Models -ContextDir Data -Context DataSyncCoreContext -Force

In this database design each table have a TenantId & InstanceId in the name. But they all have the same layout.
Add only the database model layout the the solution.

Example:
SimpleAuthors, SimpleBooks, SimpleChapters is only use for getting the layout. But all the data is in
StatusOnlineAuthor_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7
StatusOnlineChapter_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7
StatusOnlineStory_a7047609-d23a-4f3a-8f99-bac187a2872e_9801456c-f65d-43a2-9bc2-19ab4fdb60b7

For each transfere add a Module, Request, respont and Schema.
For each row you need a way to identify unique rows. It is possible to use a combine key from multi tables. Examples:
To identify a Story: story.AuthorNameId.ToString() + "_" + chapter.StoryId.ToString(),
To identify a Chapter: story.StoryId.ToString() + "_" + chapter.ChapterId.ToString()
Save the info in Key1, Key2 and Key3 or in TraceId

Each module have 3 steps
- Query:     Get the data from the database.
- CreateDTO: Prepare the data for transfere.
- SendData:  Send the data to the webservice.

Each module has a PriorityLevel, that the order they will run. 
Example: You cannot transfere a Story with out first have created a Author and you cannot create a Chapter with first create a Story.
Author -> Story -> Chapter
That why PriorityLevel is 1 for Author, 2 for Story, 3 for Chapter.
Remember to setup your PriorityLevel correct!

Query
Create your join for the data you want to transfere. Then join the data with the DoneTable to remove all ready transferede rows.
You need some unique information to identify a row, in this database, i can use the 1-3 Primary keys to identify a row.
Each module will have it own DoneTable to keep track of what have been transfered.

CreateDTO
Collect the information you need to transfere, you can get the traceId from other models. Example:
When transfere a Story i need the new PublicId of the Author. I can get that with the Author DoneTable, by privoiding the Author ModuleId and what was the originalId (AuthorNameId)
If a row have not been transfered from Author model has not been transferred the Story from that Author will not be transferred.
Prepair the colletion of DTO to be transferred

SendData
Send data the data to the API and get the traceId and publicId. This will be save in the DoneTable for this module.
If you really want to verify that all data have been transferred correct, you can return the inserted data and run VerifyData


Rows with problems...
If there is data problem, and 1 or more rows cannot be transfre the program will scale down until it find the problem row and the increase the amount of rows again.
Fail to transfere 50.000 rows, next try 25.000, then 12.500.
When 1 row failed to transfred it will mark as failed and ship it for now. When start to scale up again.

