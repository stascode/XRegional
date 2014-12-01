A sample library demonstrating the approach to have Azure Table Storage or Azure DocumentDb data consistent across multiple Azure regions.

Uses Azure Queues to propagate data from a primary to a secondary storage.

The QueueProcessor can be hosted as a Worker Role or an Azure Web job.

Follow instructions in XRegional.Tests/Readme.txt to run the tests.