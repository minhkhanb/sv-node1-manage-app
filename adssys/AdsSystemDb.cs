using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adssys.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;
using adssys.Models;

using User = adssys.Models.User;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using Microsoft.Azure;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace adssys
{
    public class AdsSystemDb
    {

#if DEBUG
        private static string EndpointUrl = "https://localhost:8081";
        private static string AuthorizationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static string DbName = "AdsSystemDb";
#else 
        private static string EndpointUrl = "https://topebox-test.documents.azure.com:443/";
        private static string AuthorizationKey = "xKvMJTxVYYdwAFu8mQFTi2l1EuOz48KPKRIMi0oC8McriitMNnkJbEzinZXYFjCD2QZv5alV2wL9lg4jLoKkjw==";
                                                  
        private static string DbName = "adsystem";
#endif

        private static Database database;
        private static DocumentCollection collection;
        //private static DocumentClient client;
        private static string guid;
        private static async Task RegisterHandler(User u)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'users'").AsEnumerable().First();
                guid = "" + Helpers.GetUID();
                u.Id = guid;
                u.Password = Helpers.Encrypt(u.Password, "username:" + u.Username + ".");
                await client.CreateDocumentAsync(collection.SelfLink, u);
            }

        }

        public static async Task UpdatePasswordAdmin(User user, string field)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'users'").AsEnumerable().First();
                var sql = "SELECT * FROM c WHERE c.username = '" + user.Username.ToLower() + "'";
                var documents = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                string uId = documents[0].id; 
                Document doc = client.CreateDocumentQuery<Document>(collection.SelfLink)
                            .Where(r => r.Id == uId)
                            .AsEnumerable()
                            .SingleOrDefault();

                doc.SetPropertyValue("password-shadow", Helpers.Encrypt(field, "username:" + user.Username.ToLower() + "."));
                Helpers.SecretKey = null;
                await client.ReplaceDocumentAsync(doc);
            }
        }

        public static Object FindAds(Ads ads)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'ads'").AsEnumerable().First();
                if(ads != null && ads.Id != null)
                {
                    var sql = "SELECT * FROM c WHERE c.id = '" + ads.Id + "'";
                    var documents = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                    if (documents.Count() > 0)
                    {
                        return documents;
                    }
                }
                
                return null;
            }
        }

        public static Object GetStatus()
        {
            var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
            var obj = new
            {
                client = client,
                endpoint = EndpointUrl,
                dbName = DbName
            };
            return obj;
        }
        public static async Task Register(User u)
        {
            await RegisterHandler(u);
        }

        public static async Task AddAds(Ads ads)
        {
            using(var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'ads'").AsEnumerable().First();
                await client.CreateDocumentAsync(collection.SelfLink, ads);
            }
        }

        public static string UploadIcon(string fileUrl, string fileName)
        {
            try
            {
                string blobName = fileName;
                string containerName = "icons".ToString().ToLower();
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString")
                    );

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "iconblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                using (var fileStream = System.IO.File.OpenRead(fileUrl))
                {
                    blockBlob.UploadFromStream(fileStream);
                    return blockBlob.Uri.ToString();
                }
            }
            catch(Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public static async Task AddProvider(Provider provider)
        {
            using(var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'providers'").AsEnumerable().First();
                provider.SecretKey = Helpers.GenerateSHA256String("username:" + Helpers.Username + ".title:"+provider.Title +"." +Helpers.SecretKey);
                await client.CreateDocumentAsync(collection.SelfLink, provider);
            }
        }

        public static async Task UpdateProvider(Provider provider)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'providers'").AsEnumerable().First();
                var sql = "SELECT * FROM c WHERE c.title = '" + provider.Title.ToLower() + "'";
                var documents = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                string uId = documents[0].id;
                Document doc = client.CreateDocumentQuery<Document>(collection.SelfLink)
                            .Where(r => r.Id == uId)
                            .AsEnumerable()
                            .SingleOrDefault();

                doc.SetPropertyValue("ads-id", provider.AdsId);
                await client.ReplaceDocumentAsync(doc);
            }
        }

        public static Array LoadAds()
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'ads'").AsEnumerable().First();

                var sql = "SELECT c FROM c";
                var documentLinks = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToArray();
                return documentLinks;
            }
            
        }

        public static Array LoadProvider()
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'providers'").AsEnumerable().First();

                var sql = "SELECT c FROM c";
                var documentLinks = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToArray();
                return documentLinks;
            }

        }

        public static Array LoadUser ()
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'users'").AsEnumerable().First();

                var sql = "SELECT c FROM c";
                var documentLinks = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToArray();
                return documentLinks;
            }
        }

        public static string getSecretKey(string providerId)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'providers'").AsEnumerable().First();
                var sql = "SELECT * FROM c WHERE c.id = '" + providerId + "'";
                var documents = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                string uId = documents[0].id;
                Document doc = client.CreateDocumentQuery<Document>(collection.SelfLink)
                            .Where(r => r.Id == uId)
                            .AsEnumerable()
                            .SingleOrDefault();
                string secretKey = doc.GetPropertyValue<string>("secret-key");
                return secretKey;
            }
        }

        public static string getAdsId(string providerId)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'providers'").AsEnumerable().First();
                var sql = "SELECT * FROM c WHERE c.id = '" + providerId + "'";
                var documents = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                string uId = documents[0].id;
                Document doc = client.CreateDocumentQuery<Document>(collection.SelfLink)
                            .Where(r => r.Id == uId)
                            .AsEnumerable()
                            .SingleOrDefault();
                string adsId = doc.GetPropertyValue<string>("ads-id");
                return adsId;
            }
        }

        public static string getAds(string adsId)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'ads'").AsEnumerable().First();
                var sql = "SELECT * FROM c WHERE c.id = '" + adsId + "'";
                var documents = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                if (documents.Count() > 0)
                {
                    string aId = documents[0].id;
                    Document doc = client.CreateDocumentQuery<Document>(collection.SelfLink)
                                .Where(r => r.Id == aId)
                                .AsEnumerable()
                                .SingleOrDefault();
                    string adsTitle = doc.GetPropertyValue<string>("title");
                    return adsTitle;
                }
                return null;
                
            }
        }

        public static Object getObjAds(string adsId)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = '" + DbName + "'").AsEnumerable().First();
                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink, "SELECT * FROM c WHERE c.id = 'ads'").AsEnumerable().First();
                var sql = "SELECT * FROM c WHERE c.id = '" + adsId + "'";
                var documents = client.CreateDocumentQuery(collection.SelfLink, sql, new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                if (documents.Count() > 0)
                {
                    string aId = documents[0].id;
                    Document doc = client.CreateDocumentQuery<Document>(collection.SelfLink)
                                .Where(r => r.Id == aId)
                                .AsEnumerable()
                                .SingleOrDefault();
                    return doc;
                }
                return null;

            }
        }



    }
}
