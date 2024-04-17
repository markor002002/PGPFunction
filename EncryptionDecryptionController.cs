// using System;
// using System.IO;
// using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using PgpCore;
using System.Text;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace PGPFunction
{
    public class EncryptionDecryptionController
    {
        public EncryptionDecryptionRequest Request { get; set; }

        public EncryptionDecryptionController(EncryptionDecryptionRequest _request)
        {
            this.Request = _request;
        }

        public async Task EncryptAsync()
        {
            var inputStream = new MemoryStream();
            await DownloadFileFromBlobAsync(Request.InputFile, inputStream);

            var outputStream = new MemoryStream();

            var encryptionKeyStream = new MemoryStream();
            //await DownloadFileFromBlobAsync(Request.EncryptionDecryptionKeyFile, encryptionKeyStream);
            
            string publicKeyBase64 = Environment.GetEnvironmentVariable("pgp-"+Request.Keyname+"-public");

            if (string.IsNullOrEmpty(publicKeyBase64))
            {
              //  return new BadRequestObjectResult($"Please add a base64 encoded public key to an environment variable called pgp-public-key");
            }

            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            string publicKey = Encoding.UTF8.GetString(publicKeyBytes);
           

           // await new PGP().EncryptStreamAsync(inputStream, outputStream, encryptionKeyStream, Request.Armor);
//           	EncryptionKeys encryptionKeys;
//            encryptionKeys = new EncryptionKeys(encryptionKeyStream);
//            PGP pgp = new PGP(encryptionKeys);

           	EncryptionKeys encryptionKeys;
            encryptionKeys = new EncryptionKeys(publicKey);
            PGP pgp = new PGP(encryptionKeys);

            
            await pgp.EncryptAsync(inputStream, outputStream);
 
            await UploadStreamToBlobAsync(Request.OutputFile, outputStream);
     
        }

        public async Task DecryptAsync()
        {
            var inputStream = new MemoryStream();
            await DownloadFileFromBlobAsync(Request.InputFile, inputStream);

            var outputStream = new MemoryStream();

        //    var encryptionKeyStream = new MemoryStream();
        //    await DownloadFileFromBlobAsync(Request.EncryptionDecryptionKeyFile, encryptionKeyStream);

         //   await new PGP().DecryptStreamAsync(inputStream, outputStream, encryptionKeyStream, Request.passPhrase);
            string privateKeyBase64 = Environment.GetEnvironmentVariable("pgp-"+Request.Keyname+"-private");
            string passPhrasekey = Environment.GetEnvironmentVariable("pgp-"+Request.Keyname+"-passcode");
            if (string.IsNullOrEmpty(privateKeyBase64))
            {
              //  return new BadRequestObjectResult($"Please add a base64 encoded public key to an environment variable called pgp-public-key");
            }

            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            string privateKey = Encoding.UTF8.GetString(privateKeyBytes);

           	//EncryptionKeys encryptionKeys;
            //encryptionKeys = new EncryptionKeys(encryptionKeyStream, Request.passPhrase);
            //PGP pgp = new PGP(encryptionKeys);
           // string passPhrasekey = "12345";
            EncryptionKeys encryptionKeys;
            encryptionKeys = new EncryptionKeys(privateKey, passPhrasekey);
            PGP pgp = new PGP(encryptionKeys);

            await pgp.DecryptAsync(inputStream, outputStream);

            await UploadStreamToBlobAsync(Request.OutputFile, outputStream); 
        }

        public async Task DownloadFileFromBlobAsync(AzureFileInfo _azureFileInfo, MemoryStream _toStream)
        {
            try
            {
                //var blobClient = new BlockBlobClient(_azureFileInfo.ConnectionString, _azureFileInfo.BlobContainerName, _azureFileInfo.BlobName);
                //await blobClient.DownloadToAsync(_toStream);
                //_toStream.Position = 0;
                
                var blobServiceClient = new BlockBlobClient(new Uri(_azureFileInfo.ConnectionString + "/"+  _azureFileInfo.BlobContainerName + "/" + _azureFileInfo.BlobName), new DefaultAzureCredential());
                //var containerClient = blobServiceClient.GetBlobContainerClient(azureFileInfo.BlobContainerName);
                //var blobClient = containerClient.GetBlobClient(_azureFileInfo.BlobName);
                await blobServiceClient.DownloadToAsync(_toStream);
                _toStream.Position = 0;
            }
            catch (Exception ex)
            {
                throw new Azure.RequestFailedException(
                    $"Unable to download file."
                //    + $" ConnectionString: {_azureFileInfo.ConnectionString},"
                    + $" BlobContainerName: {_azureFileInfo.BlobContainerName},"
                    + $" BlobName: {_azureFileInfo.BlobName}."
                    + $" Exception message: {ex.Message}");
            }
        }

        public async Task UploadStreamToBlobAsync(AzureFileInfo _azureFileInfo, MemoryStream _stream)
        {
            try
            {
/*                 _stream.Position = 0;
                var blobClient = new BlockBlobClient(_azureFileInfo.ConnectionString, _azureFileInfo.BlobContainerName, _azureFileInfo.BlobName);
                await blobClient.UploadAsync(_stream); */

                _stream.Position = 0;
                var blobServiceClient = new BlockBlobClient(new Uri(_azureFileInfo.ConnectionString + "/"+  _azureFileInfo.BlobContainerName + "/" + _azureFileInfo.BlobName), new DefaultAzureCredential());
                //var containerClient = blobServiceClient.GetBlobContainerClient(azureFileInfo.BlobContainerName);
                //var blobClient = containerClient.GetBlobClient(_azureFileInfo.BlobName);
                await blobServiceClient.UploadAsync(_stream);

            }
            catch (Exception ex)
            {
                throw new Azure.RequestFailedException(
                    $"Unable to upload file."
               //     + $" ConnectionString: {_azureFileInfo.ConnectionString},"
                    + $" BlobContainerName: {_azureFileInfo.BlobContainerName},"
                    + $" BlobName: {_azureFileInfo.BlobName}."
                    + $" Exception message: {ex.Message}");
            }
        }


    }
}