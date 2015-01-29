using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using XRegional.Common;

namespace XRegional.Wrappers
{
    /// <summary>
    /// Light-weight wrapper for Azure Queue
    /// </summary>
    public class QueueWrapper
    {
        private readonly CloudQueue _queue;

        private static readonly IRetryPolicy DefaultRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 5);

        public string QueueName
        {
            get { return _queue.Name; }
        }

        public static QueueWrapper FromConnectionString(string queueName, string storageAccountConnectionString)
        {
            return FromConnectionString(queueName, storageAccountConnectionString, DefaultRetryPolicy);
        }

        public static QueueWrapper FromConnectionString(string queueName, string storageAccountConnectionString, IRetryPolicy defaultRetry)
        {
            Guard.NotNullOrEmpty(queueName, "queueName");
            Guard.NotNullOrEmpty(storageAccountConnectionString, "storageAccountConnectionString");

            return new QueueWrapper(queueName, CloudStorageAccount.Parse(storageAccountConnectionString), defaultRetry);
        }

        public QueueWrapper(string queueName, CloudStorageAccount storageAccount)
            : this(queueName, storageAccount, DefaultRetryPolicy)
        {
        }

        public QueueWrapper(string queueName, CloudStorageAccount storageAccount, IRetryPolicy defaultRetry)
        {
            Guard.NotNullOrEmpty(queueName, "queueName");
            Guard.NotNull(storageAccount, "storageAccount");

            CloudQueueClient queueClient = new CloudQueueClient(storageAccount.QueueEndpoint, storageAccount.Credentials);
            if (defaultRetry == null)
                defaultRetry = DefaultRetryPolicy;
            queueClient.DefaultRequestOptions.RetryPolicy = defaultRetry;

            // ensure that the queue is created
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();

            _queue = queue;
        }

        /// <summary>
        /// Delete this Azure queue
        /// </summary>
        public void Delete()
        {
            _queue.Delete();
        }

        /// <summary>
        /// Enqueues a message that is represented as an array of bytes.
        /// </summary>
        /// <param name="content"></param>
        public void AddMessage(byte[] content)
        {
            Guard.NotNull(content, "content");

            CloudQueueMessage msg = new CloudQueueMessage(content);
            _queue.AddMessage(msg);
        }   

        /// <summary>
        /// Gets message from the queue
        /// </summary>
        /// <param name="visibilityTimeout"></param>
        /// <returns></returns>
        public CloudQueueMessage GetMessage(TimeSpan? visibilityTimeout =null)
        {
            return _queue.GetMessage(visibilityTimeout);
        }

        /// <summary>
        /// Maximum messageCount is 32
        /// </summary>
        public IEnumerable<CloudQueueMessage> GetMessages(int messageCount =32, TimeSpan? visibilityTimeout =null)
        {
            IEnumerable<CloudQueueMessage> msgs = _queue.GetMessages(messageCount, visibilityTimeout);
            return msgs ?? Enumerable.Empty<CloudQueueMessage>();
        }

        public CloudQueueMessage PeekMessage()
        {
            return _queue.PeekMessage();
        }

        public void DeleteMessage(CloudQueueMessage message)
        {
            Guard.NotNull(message, "message");

            try
            {
                _queue.DeleteMessage(message);
            }
            catch (StorageException e)
            {
                // Addressing the properties should not throw an exception
                if (e.RequestInformation.ExtendedErrorInformation.ErrorCode == "MessageNotFound")
                {
                    // pop receipt in invalid
                    // ignore or log or we could tune the invisibility time
                    // TODO
                }
                else
                {
                    // not the error we expected
                    throw;
                }
            }
        }

        public int ApproximateMessageCount()
        {
            // Fetch the queue attributes.
            _queue.FetchAttributes();

            // Retrieve the cached approximate message count.
            int? messageCount = _queue.ApproximateMessageCount;

            return messageCount.HasValue
                ? messageCount.Value : 0;
        }
    }
}
