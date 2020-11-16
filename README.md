# .NET Core Web API for CSV processing

A .NET Core Web API project designed to handle CSV file uploads of a predefined format. 
It processes the data, performs simple validation and then stores it in JSON file and `MongoDB`.
A sample CSV file can be found [here][sample csv]

## Prerequisites

- [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core)
- [MongoDB](https://docs.mongodb.com/manual/tutorial/install-mongodb-on-windows/)

## Running locally

The project assumes that your `MongoDB` instance is available at `mongodb://127.0.0.1:27017`, but you can change the connection string in [appsettings.json file][app settings].

Just run these few commands:

- `git clone https://github.com/keenua/csvapi`
- `cd csvapi\src\Ireckonu.Api`
- `dotnet run`

Now you should be able to see the swagger UI if you go to https://localhost:5001.
You can try uploading a sample file through the `/upload` endpoint. 
All parameters have reasonable defaults, so just attach the file itself.

## Configuration

### appsettings.json

[appsettings.json][app settings] contains a few configurable properties.

- `MongoDb` section allows you to change Mongo connection settings
- `JsonDB` section configures the paths to json files. More on the meaning of these in json section
- `BufferSize` value controls the number of records kept in the memory before dumping into database

### Query parameters 

File upload itself can be configured by specifying query parameter values (as seen on swagger page)

- `ReportSuccessForRecords` (**false** by default) determines whether successfully processed records should be included in the response, i.e.:
```javascript
 {
    "success": true,
    "line": 514
 }
```

- `ContainsHeader` (**true** by default) determines whether processor should expect the CSV file to contain a header

- `MaxRecordsInResponse` (**1000** by default) determines how many records can response contain. If processor reaches this limit, it would eagerly stop and return current results. If `ReportSuccessForRecords` is **true**, all records would be counted, which means a maximum of `{MaxRecordsInResponse}` would be processed. Otherwise, the records would be processed until `{MaxRecordsInResponse}` invalid records are encountered.

### Code

Maximum file size, accepted by API is configured through code. You can find the variable in [UploadController class][max file size]

[sample csv]: ./src/Ireckonu.Tests/TestData/small.csv
[app settings]: ./src/Ireckonu.Api/appsettings.json
[max file size]: ./src/Ireckonu.Api/Controllers/UploadController.cs#17

## Design decisions

### Contract

One of the possible approaches to implementing CSV upload would be to have the user upload the whole file and then process it on the server. 
That would require some kind of notification or polling (long polling) mechanism. I.e. user uploads the file -> gets task ID -> polls for results by task ID -> receives a result (or gets notified through some system). However, for this particular assignment it looked like an overkill to me. Especially if the end user would be a system, and not a human. 

That's why I went with a simple "process as we go" approach:
- Request stream is sequentially processed into objects
- Those objects get validated and put into a buffer
- When buffer is full, the objects are dumped into db (both JSON and Mongo)

This way we only keep a certain amount of objects in memory, which allows us to handle larger files.

### Error reporting

All processing errors are reported in the response. That allows user to fix the broken records and only reupload the CSV lines that were affected.
Current solution also allows to warn user about potential issues, while still processing the record (i.e. "are you sure this value is valid?").

### Storing in JSON file

Since we're working with large files, keeping the whole JSON file in memory doesn't sound feasible. 
And given that we already have a buffer with current objects in memory, it makes sense to only update the file when we dump into db.
I would be nice to just update current records (or add them), however it's quite complicated to "guess" where exactly the object needs to be in the file.
This means that we would need to rewrite the whole file each time we dump the data.
In order to be on the safe side, I came up with the following system:
- Sequentially read objects from `main` json file
- Write these objects into the `secondary` json file, but skip the ones with the keys present in the buffer
- Now write objects from the buffer into the `secondary` json file
- Backup the `main` into `backup` json file
- Move `secondary` into `main` (essentialy making it the main file)
- Remove the `backup` file

If anything goes wrong during the process, we can either "forget" about the changes we tried to make, or restore `backup` into `main`.
This does come with a price of double storage space. 

This is all done with a lock (semaphore in this case), so that data isn't corrupted in the process by a competing request.
It doesn't save us from competing **processes**, but if that becomes a problem, I would advise not to use a file as a storage.

We also **don't** guarantee that the MongoDB and JSON file are in a consistent state at all times.
It can be implemented with transactions and rollbacks in one storage in case of failure in the other one.
But once again, it sounds like an overkill.
For now, we rely on the fact that if we fail to store in one of the databases, we would report the error back to user, which would in turn fix the broken records and reupload.
This can be considered a conditional eventual consistency :)

## Performance

Even though performance wasn't my primary concern, the solution processes and stores a reasonable amount of 1_000_000 records in ~70 seconds on my machine (~14_000 records/second).
There are lots of improvements that can be made to speed it up, including reusing objects, optimizing code etc.
For now, the performance can be tweaked by changing the size of the buffer in settings.

## Leftovers

- I haven't split the `Article` model into multiple models, since there was no real reason to. If the use cases dictate that, it could easily be changed. This does mean that we can potentially get a sigificant amount of duplicate values (i.e. colors, article codes) in the db. However, I would choose simplicity over optimization until a clear use case is known here.
- Given the limited time, not all cases are covered by tests (or even the solution itself). I fully skipped MongoDbContext, converters and other tests.
- Some parts of the solution are overengineered either by design (to show the approach I would take in a larger project) or by the pure fact that I spent more time on one thing than the other.
- I don't have Resharper installed on this machine, so sorry for any obvious formatting/redundancy mistakes in the project