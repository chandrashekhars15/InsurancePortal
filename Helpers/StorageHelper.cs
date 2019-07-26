using InsuranceClientPortal.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceClientPortal.Helpers
{
    /// <summary>
    /// Contains the method for adding customer data to Table and image to blob storage.
    /// </summary>
    public class StorageHelper
    {
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;
        private CloudQueueClient queueClient;

        public string StorageConnectionString
        {
            set
            {
                this.storageAccount = CloudStorageAccount.Parse(value);
                this.blobClient = storageAccount.CreateCloudBlobClient();
                this.tableClient = storageAccount.CreateCloudTableClient();
                this.queueClient = storageAccount.CreateCloudQueueClient();
            }
        }

        public string TableConnectionString
        {
            set
            {
                var sa = CloudStorageAccount.Parse(value);
                this.tableClient = storageAccount.CreateCloudTableClient();
            }
        }

        public async Task<CloudBlobContainer> CreateContainerIfNotExistsAsync(string containerName)
        {
            var container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        public async Task<string> UploadFileAsync(string imagePath, string containerName)
        {
            var fileName = Path.GetFileName(imagePath);
            var container = await CreateContainerIfNotExistsAsync(containerName);
            var blob = container.GetBlockBlobReference(fileName);
            await blob.UploadFromFileAsync(imagePath);

            return blob.Uri.AbsoluteUri;
        }

        public async Task<CloudTable> CreateTableIsNotExistsAsync(string tableName)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<Customer> SaveInsuranceDetailAsync(Customer customer, string tableName)
        {
            TableOperation tableOperation = TableOperation.InsertOrMerge(customer);
            var table = await CreateTableIsNotExistsAsync(tableName);
            TableResult entity = await table.ExecuteAsync(tableOperation);
            var result = entity.Result as Customer;
            return result;
        }

        public async Task<CloudQueue> CreateQueueIsNotExistsAsync(string queueName)
        {
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            return queue;
        }

        public async Task<bool> SendMessageAsync(string messageText, string queueName)
        {
            var queue = await CreateQueueIsNotExistsAsync(queueName);

            CloudQueueMessage message = new CloudQueueMessage(messageText);

            await queue.AddMessageAsync(message, TimeSpan.FromMinutes(30), TimeSpan.Zero, null, null);

            //TableOperation tableOperation = TableOperation.InsertOrMerge(customer);
            //var table = await CreateTableIsNotExistsAsync(tableName);
            //TableResult entity = await table.ExecuteAsync(tableOperation);
            //var result = entity.Result as Customer;
            return true;
        }
    }
}
