//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

namespace DataBlobStorageSample
{
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Auth;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    ////// <summary>
    /// Azure Blob存储的示例 - 演示如何使用Blob存储服务。
    /// Blob存储主要是用来存储一些非结构化的数据，例如：文本、二进制数据、文档、媒体文件。
    /// Blobs能够通过HTTP或者HTTPS的方式被世界各地访问。
    ///
    /// 注意：这个示例使用.NET 4.5异步编程模型来演示如何使用storage client libraries异步API调用存储服务。 在实际的应用中这种方式
    /// 可以提高程序的响应速度。调用存储服务只要添加关键字await为前缀即可。
    ///
    /// 文档引用: 
    /// - 什么是存储账号- https://www.azure.cn/documentation/articles/storage-create-storage-account/
    /// - Blobs起步 - http://www.azure.cn/documentation/articles/storage-dotnet-how-to-use-blobs/
    /// - Blob服务概念 - https://msdn.microsoft.com/zh-cn/library/dd179376.aspx 
    /// - Blob服务REST API - http://msdn.microsoft.com/zh-cn/library/dd135733.aspx
    /// - Blob服务C# API - http://go.microsoft.com/fwlink/?LinkID=398944
    /// - 使用共享访问签名(SAS)委托访问- http://www.azure.cn/documentation/articles/storage-dotnet-shared-access-signature-part-1/
    /// - 存储模拟器 - https://www.azure.cn/documentation/articles/storage-use-emulator/
    /// - 使用 Async 和 Await异步编程  - http://msdn.microsoft.com/zh-cn/library/hh191443.aspx
    /// </summary>
    /// 

    public class Program
    {
        // *************************************************************************************************************************
        // 使用说明: 这个示例可以在Azure存储模拟器（存储模拟器是Azure SDK安装的一部分）上运行，或者通过修改App.Config文档中的存储账号和存储密匙
        // 的方式针对存储服务来使用。      
        // 
        // 使用Azure存储模拟器来运行这个示例  (默认选项)
        //      1. 点击开始按钮或者是键盘的Windows键，然后输入“Azure Storage Emulator”来寻找Azure存储模拟器，然后点击运行。       
        //      2. 设置断点，然后使用F10按钮运行这个示例. 
        // 
        // 使用Azure存储服务来运行这个示例
        //      1. 打来AppConfig文件然后使用第二个连接字符串。
        //      2. 在Azure门户网站上创建存储账号，然后修改App.Config的存储账号和存储密钥。更多详细内容请阅读：https://www.azure.cn/documentation/articles/storage-dotnet-how-to-use-blobs/
        //      3. 设置断点，然后使用F10按钮运行这个示例. 
        // 
        // *************************************************************************************************************************
        static void Main(string[] args)
        {
            Console.WriteLine("Azure Blob存储示例\n ");

            // 块blob基础
            Console.WriteLine("块 Blob 示例");
            BasicStorageBlockBlobOperationsAsync().Wait();

            // 共享访问签名（SAS）使用块Blobs基础
            BasicStorageBlockBlobOperationsWithAccountSASAsync().Wait();

            // 页blob基础
            Console.WriteLine("\n页 Blob 示例");
            BasicStoragePageBlobOperationsAsync().Wait();

            Console.WriteLine("按任意键退出");
            Console.ReadLine();
        }

        /// <summary>
        /// 块blobs的基本操作
        /// </summary>
        /// <returns>Task<returns>
        private static async Task BasicStorageBlockBlobOperationsAsync()
        {
            const string imageToUpload = "HelloWorld.png";
            string blockBlobContainerName = "demoblockblobcontainer-" + Guid.NewGuid();

            // 通过连接字符串找到存储账号的信息
            // 如何配置 Azure 存储空间连接字符串 - https://www.azure.cn/documentation/articles/storage-configure-connection-string/
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // 创建一个客户端的blob来和blob服务交互。
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // 创建一个容器来组织存储账号下的blobs。
            Console.WriteLine("1. 创建容器...");
            CloudBlobContainer container = blobClient.GetContainerReference(blockBlobContainerName);
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch (StorageException)
            {
                Console.WriteLine("如果使用默认配置文件，请确保Azure模拟器已经启动。点击Windows键然后输入\"Azure Storage\"，找到Azure模拟器然后点击运行，之后请重新启动该示例.");
                Console.ReadLine();
                throw;
            }

            // 我们有两个选择去查看已经上传的blob
            // 1) 使用共享访问签名(SAS)来委托访问资源。更多详细内容请阅读上面提供的关于SAS的文档。
            // 2) 公开容器下blobs的访问权限。取消下面代码的注释可以达到此目的。通过这样的设置我们可以使用下面的链接访问之前上传的图片
            // https://[InsertYourStorageAccountNameHere].blob.core.chinacloudapi.cn/democontainer/HelloWorld.png

            // await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // 上传BlockBlob到最新创建的容器中
            Console.WriteLine("2. 上传 BlockBlob");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageToUpload);
            await blockBlob.UploadFromFileAsync(imageToUpload, FileMode.Open);

            // 列出容器内所有的blobs 
            Console.WriteLine("3. 列出容器内所有的blobs");
            foreach (IListBlobItem blob in container.ListBlobs())
            {
                // Blob 的类型可能是 CloudBlockBlob, CloudPageBlob 或者 CloudBlobDirectory
                // 使用 blob.GetType() 然后转换成适当的类型以获得访问每个类型下特别的属性 
                Console.WriteLine("- {0} (类型: {1})", blob.Uri, blob.GetType());
            }

            // 下载blob到你的文件系统
            Console.WriteLine("4. 下载Blob，下载地址：{0}", blockBlob.Uri.AbsoluteUri);
            await blockBlob.DownloadToFileAsync(string.Format("./CopyOf{0}", imageToUpload), FileMode.Create);

            // 创建只读的blob快照
            Console.WriteLine("5. 创建只读的blob快照");            
            CloudBlockBlob blockBlobSnapshot =  await blockBlob.CreateSnapshotAsync(null, null, null, null);
            
            // 示例后的一些清除工作
            Console.WriteLine("6. 删除块Blob以及所有的快照");
            await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots,null,null,null);

            Console.WriteLine("7. 删除容器");
            await container.DeleteIfExistsAsync();
        }
        /// <summary>
        /// 块blobs的基本操作
        /// </summary>
        /// <returns>Task<returns>
        private static async Task BasicStorageBlockBlobOperationsWithAccountSASAsync()
        {
            const string imageToUpload = "HelloWorld.png";
            string blockBlobContainerName = "demoblockblobcontainer-" + Guid.NewGuid();
            string accountName = "sas";
         
            // 调用GetAccountSASToken 来获得基于存储账号、存储密匙的sasToken 
            string sasToken = GetAccountSASToken();

            // 通过SASToken创建AccountSAS
            StorageCredentials accountSAS = new StorageCredentials(sasToken);

            //信息: 打印账号SAS的签名和令牌
            Console.WriteLine();
            Console.WriteLine("账户SAS的签名: " + accountSAS.SASSignature);
            Console.WriteLine("账户SAS的令牌: " + accountSAS.SASToken);
            Console.WriteLine();

            // 创建一个容器来组织存储账号下的blobs
            Console.WriteLine("1. 使用账户SAS创建容器");

            // 通过传递存储账号和容器名来获得容器的Uri
            Uri ContainerUri = GetContainerSASUri(blockBlobContainerName);

            // 通过使用Uri和sasToken来创建CloudBlobContainer
            CloudStorageAccount accountWithSAS = new CloudStorageAccount(accountSAS, accountName, "core.chinacloudapi.cn", true);
            CloudBlobClient blobClient = accountWithSAS.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(blockBlobContainerName);
          
           // CloudBlobContainer container = new CloudBlobContainer(ContainerUri, accountSAS);
            try
            {
                await container.CreateIfNotExistsAsync();
            }            
            catch (StorageException)
            {
                Console.WriteLine("如果使用默认配置文件，请确保Azure模拟器已经启动。点击Windows键然后输入\"Azure Storage\"，找到Azure模拟器然后点击运行，之后请重新启动该示例.");
                Console.ReadLine();
                throw;
            }
           

            // 我们有两个选择去查看已经上传的blob
            // 1) 使用共享访问签名(SAS)来委托访问资源。更多详细内容请阅读上面提供的关于SAS的文档。
            // 2) 公开容器下blobs的访问权限。取消下面代码的注释可以达到此目的。通过这样的设置我们可以使用下面的链接访问之前上传的图片
            // https://[InsertYourStorageAccountNameHere].blob.core.chinacloudapi.cn/democontainer/HelloWorld.png 
              
            // await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // 上传BlockBlob到最新创建的容器中
            Console.WriteLine("2. 上传 BlockBlob");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageToUpload);
            await blockBlob.UploadFromFileAsync(imageToUpload, FileMode.Open);

            // 列出容器内所有的blobs
            Console.WriteLine("3. 列出容器内所有的blobs");
            foreach (IListBlobItem blob in container.ListBlobs())
            {                
                // Blob 的类型可能是 CloudBlockBlob, CloudPageBlob 或者 CloudBlobDirectory
                // 使用 blob.GetType() 然后转换成适当的类型以获得访问每个类型下特别的属性 
                Console.WriteLine("- {0} (类型: {1})", blob.Uri, blob.GetType());
            }

            // 下载blob到你的文件系统
            Console.WriteLine("4. 下载Blob，下载地址：{0}", blockBlob.Uri.AbsoluteUri);
            await blockBlob.DownloadToFileAsync(string.Format("./CopyOf{0}", imageToUpload), FileMode.Create);

            // 创建只读的blob快照
            Console.WriteLine("5. 创建只读的blob快照");
            CloudBlockBlob blockBlobSnapshot = await blockBlob.CreateSnapshotAsync(null, null, null, null);

            // 示例后的一些清除工作 
            Console.WriteLine("6. 删除块Blob以及所有的快照");
            await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);

            Console.WriteLine("7. 删除容器");
            await container.DeleteIfExistsAsync();

        }
        /// <summary>
        /// 页blobs的基本操作
        /// </summary>
        /// <returns>Task</returns>
        private static async Task BasicStoragePageBlobOperationsAsync()
        {
            const string PageBlobName = "samplepageblob";
            string pageBlobContainerName = "demopageblobcontainer-" + Guid.NewGuid();

            // 通过连接字符串找到存储账号的信息
            // 如何配置 Azure 存储空间连接字符串 - https://www.azure.cn/documentation/articles/storage-configure-connection-string/           
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // 创建一个客户端的blob来和blob服务交互
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // 创建一个容器来组织存储账号下的blobs。
            Console.WriteLine("1. 创建容器...");
            CloudBlobContainer container = blobClient.GetContainerReference(pageBlobContainerName);
            await container.CreateIfNotExistsAsync();

            // 创建一个页blob到新创建的容器中 
            Console.WriteLine("2. 创建页Blob");
            CloudPageBlob pageBlob = container.GetPageBlobReference(PageBlobName);
            await pageBlob.CreateAsync(512 * 2 /*size*/); // 大小必须是512 bytes的倍数

            // 写页blob 
            Console.WriteLine("2. 写页blob");
            byte[] samplePagedata = new byte[512];
            Random random = new Random();
            random.NextBytes(samplePagedata);
            await pageBlob.UploadFromByteArrayAsync(samplePagedata, 0, samplePagedata.Length);

            // 列出容器内的所有的blobs。因为一个容器包含很多的blobs，页blob的返回结果可能会包含多个段，每个段最大可能有5000个blobs。
            // 你可以通过ListBlobsSegmentedAsync方法的参数maxResults定义一个更小的尺寸         
            Console.WriteLine("3. 列出容器内的Blobs");
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(token);
                token = resultSegment.ContinuationToken;
                foreach (IListBlobItem blob in resultSegment.Results)
                {
                    // Blob 的类型可能是 CloudBlockBlob, CloudPageBlob 或者 CloudBlobDirectory
                    Console.WriteLine("{0} (类型: {1}", blob.Uri, blob.GetType());
                }
            } while (token != null);

            // 从页blob中读取数据
            // Console.WriteLine("4. Read from a Page Blob");
            int bytesRead = await pageBlob.DownloadRangeToByteArrayAsync(samplePagedata, 0, 0, samplePagedata.Count());

            // 示例后的一些清除工作 
            Console.WriteLine("6. 删除页Blob");
            await pageBlob.DeleteIfExistsAsync();

            Console.WriteLine("7. 删除容器");
            await container.DeleteIfExistsAsync();
        }

        /// <summary>
        /// 创建一个容器的SAS Uri
        /// </summary>
        /// <param name="storageAccount">存储账号对象</param>
        /// <param name="containerName"> BlockBlob 的容器名</param>
        /// <returns>Uri</returns>
        private static Uri GetContainerSASUri(string containerName)
        {
            // 通过连接字符串找到存储账号的信息
            // 如何配置 Azure 存储空间连接字符串 - https://www.azure.cn/documentation/articles/storage-configure-connection-string/ 
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            return new Uri(storageAccount.BlobStorageUri.PrimaryUri.OriginalString + "/" + containerName);
        }

        /// <summary>
        /// 创建存储账号的SAS 令牌
        /// </summary>
        /// <param name="storageAccount">存储账号对象</param>
        /// <returns>sasToken</returns>
        private static string GetAccountSASToken()
        {
            // 通过连接字符串找到存储账号的信息
            // 如何配置 Azure 存储空间连接字符串 - https://www.azure.cn/documentation/articles/storage-configure-connection-string/ 
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // 使用下属属性为存储账号创建一个新的访问策略:
            // Permissions: Read, Write, List, Create, Delete
            // ResourceType: Container
            // Expires in 24 hours
            // Protocols: Https or Http (模拟器还不支持Https)
            SharedAccessAccountPolicy policy = new SharedAccessAccountPolicy()
            {
                Permissions = SharedAccessAccountPermissions.Read | SharedAccessAccountPermissions.Write | SharedAccessAccountPermissions.List | SharedAccessAccountPermissions.Create | SharedAccessAccountPermissions.Delete,
                Services = SharedAccessAccountServices.Blob,
                ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Protocols = SharedAccessProtocol.HttpsOrHttp
            };

            // 使用SAS令牌创建一个新的存储凭证
            string sasToken = storageAccount.GetSharedAccessSignature(policy);

            // 返回SASToken
            return sasToken;
        }

        /// <summary>
        /// 验证App.Config文件中的连接字符串，当使用者没有更新有效的值时抛出错误提示
        /// </summary>
        /// <param name="storageConnectionString">连接字符串</param>
        /// <returns>CloudStorageAccount object</returns>
        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("提供的存储信息无效，请确认App.Config文件中的AccountName和AccountKey有效后重新启动该示例");                  
                Console.ReadLine();
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("提供的存储信息无效，请确认App.Config文件中的AccountName和AccountKey有效后重新启动该示例");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }
    }
}
