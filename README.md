# XRegional Library

XRegional is a simple library demonstrating a specific approach to the data consistency across multiple Microsoft Azure regions (datacenters).

Currently XRegional supports both Azure Table Storage and Azure Document Db and uses Azure Queues to propagate data from a primary to a secondary storage. XRegional uses heavily ETags and optimistic concurrency features of Table Storage and Document Db.

XRegional approach is aimed primarily for *one writer / multiple readers* scenario where writes occur strictly in one region but reads occur in all regions. Nevertheless this approach allows for failover to another write location in case the primary write region experiences an outage. Also XRegional performs full updates of an Azure Table entity or a Document Db document, partial updates are not supported due to certain constraints. On the other hand you might redesign your data structure such that the final entity structure will consist of multiple data parts – each independent of the other and updated solely by a region it represents.

XRegional introduces a simple concept of *Gateway* that is used in the primary region to send data over to other Azure regions. There is one interface IGatewayWriter and two implementations: GatewayQueueWriter and GatewayMultiQueueWriter to send data respectively to one or multiple secondary regions through the queues. GatewayQueueWriter stores a message to the Azure blob storage if it is exceeds 64KB limit passing a message-pointer instead. A message is zipped and passed as an array of bytes for the maximum efficiency. On the contrary, GatewayQueueReader is to be used in the secondary region and it simply dequeues the next message from the gateway queue calling either a callback for subsequent processing of the message or an “on-error” callback to give you a chance to process the poisonous message. Instead of GatewayQueueReader you should use TableGatewayQueueProcessor for Azure Tables and DocdbGatewayQueueProcessor for Document Db as those two classes implement the write operation to the secondary region’s data store for you. Either of the two QueueProcessor's can be hosted as a Worker Role or an Azure Web job.

To go and start executing writes to either an Azure Table or a Document Db collection, use SourceTable or SourceCollection respectively. Both implement a “brutal” writer approach. Their counterparts: TargetTable and TargetCollection implement the write operation following the optimistic concurrenty and ETag rules in the secondary region.

## How to get started

Follow instructions in XRegional.Tests/Readme.txt to run the tests.