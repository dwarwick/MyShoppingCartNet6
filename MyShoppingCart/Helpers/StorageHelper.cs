using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;

using Azure;

using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Helpers
{
    public static class StorageHelper
    {

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(IFormFile iFormFile, string sFilePath, string ConnectionString, string ContainerName, string userId)
        {
            try
            {
                // Create the blob client.            
                BlobContainerClient blobClient = new BlobContainerClient(ConnectionString, ContainerName);

                string sNewFileName = iFormFile.FileName.Trim().Replace(' ', '_');

                // Upload the file
                BlobClient blob = blobClient.GetBlobClient($"{userId}/{sNewFileName}");

                // Upload local file
                await blob.UploadAsync(sFilePath, true); 
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        //public static async Task<bool> UploadFileToStorageFolders(IFormFile iFormFile, string sFilePath, string ConnectionString, string ContainerName)
        //{
        //    try
        //    {
        //        ShareClient share = new ShareClient(ConnectionString, "myshoppingcartfileshare");

        //        ShareDirectoryClient directory = share.GetDirectoryClient("9e935bab-714f-4c30-8d23-5efa39efc06e");

        //        //Upload destination(azure side)Create if there is no folder in.
        //        directory.CreateIfNotExists();

        //        //Upload destination(azure side)Create a file instance in.
        //        ShareFileClient file = directory.GetFileClient(iFormFile.FileName);

        //        //Delete any file with the same name
        //        file.DeleteIfExists();

        //        //Open the Local file to be uploaded. It is easy to get binary information by opening it with FileStream type.
        //        FileStream stream = File.OpenRead(sFilePath);

        //        //Upload destination(azure side)Inject binary information into a file instance
        //        file.Create(stream.Length);
        //        file.UploadRange(new Azure.HttpRange(0, stream.Length), stream);

        //        //Free local files
        //        stream.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        return await Task.FromResult(false);
        //    }
        //    return await Task.FromResult(true);
        //}




        public static async Task<bool> DeleteFileToStorage(string sUrl, string ConnectionString, string ContainerName)
        {

            string sFilename = Path.GetFileName(sUrl);
            try
            {
                // Create the blob client.            
                BlobContainerClient blobClient = new BlobContainerClient(ConnectionString, ContainerName);

                // Upload the file
                BlobClient blob = blobClient.GetBlobClient(sFilename);

                // Upload local file
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }
    }
}