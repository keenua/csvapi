# .NET Core web API for CSV processing

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

Now you should be able to see the swagger UI if you go to [htts://localhost:5001].
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
