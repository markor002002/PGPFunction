using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using System.Text;
//using PgpCore;
using Newtonsoft.Json;
using System.Text;

namespace PGPFunction
{

 public class PGPEncrypt
    {

        private readonly ILogger<PGPEncrypt> _logger;

        public PGPEncrypt(ILogger<PGPEncrypt> logger)
        {
            _logger = logger;
        }

        public EncryptionDecryptionRequest Request { get; set; }

        public PGPEncrypt(EncryptionDecryptionRequest _request)
        {
            this.Request = _request;
        }

        [Function(nameof(PGPEncrypt))]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)       
        {
            _logger.LogInformation($"C# HTTP trigger PGPEncrypt function processed a request at: {DateTime.Now}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			
			var request = JsonConvert.DeserializeObject<EncryptionDecryptionRequest>(requestBody);

			try
            {

                
                if(request == null)
                {
                    throw new ArgumentException("Request parameters are missing or not in the correct format.");
                }
                request.validate();

                string publicKeyBase64 = Environment.GetEnvironmentVariable("pgp-" + Request.pgpKeyName + "-public");
                string passPhrase = Environment.GetEnvironmentVariable("pgp-" + Request.pgpKeyName + "-passPhrase");
                
                if((string.IsNullOrEmpty(publicKeyBase64)))
                {
                    throw new ArgumentException("Request parameters are missing or not in the correct format.");
                }

                
                byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
                string publicKey = Encoding.UTF8.GetString(publicKeyBytes);

           //     await new EncryptionDecryptionController(request).EncryptAsync(publicKey, passPhrase);

            var inputStream = new MemoryStream();
            await DownloadFileFromBlobAsync(Request.InputFile, inputStream);

            var outputStream = new MemoryStream();

           // var encryptionKeyStream = new MemoryStream();
           // await DownloadFileFromBlobAsync(Request.EncryptionDecryptionKeyFile, encryptionKeyStream);
            
            
           

           // await new PGP().EncryptStreamAsync(inputStream, outputStream, encryptionKeyStream, Request.Armor);
           	EncryptionKeys encryptionKeys;
            encryptionKeys = new EncryptionKeys(encryptionKeys);
            PGP pgp = new PGP(encryptionKeys);
            await pgp.EncryptAsync(inputStream, outputStream);

 
            await UploadStreamToBlobAsync(Request.OutputFile, outputStream);
     

            }
            catch (Exception ex)
            {
                _logger.LogError($"The following exception has occurred: {ex.ToString()}");
                return new BadRequestObjectResult(ex.Message);
            }

            _logger.LogInformation($"Encryption function executed successfully.");
            return new OkObjectResult("Encryption function executed successfully!");
           // return new OkResult();
        }

        
    }

 public class PGPDecrypt
    {
        private readonly ILogger<PGPDecrypt> _logger;

        public PGPDecrypt(ILogger<PGPDecrypt> logger)
        {
            _logger = logger;
        }
        [Function(nameof(PGPDecrypt))]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
            
        {
           _logger.LogInformation($"C# HTTP trigger PGPDecrypt function processed a request at: {DateTime.Now}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			
			var request = JsonConvert.DeserializeObject<EncryptionDecryptionRequest>(requestBody);

			try
            {
                if(request == null)
                {
                    throw new ArgumentException("Request parameters are missing or not in the correct format.");
                }

                request.validate();

                await new EncryptionDecryptionController(request).DecryptAsync();
            }
            catch (Exception ex)
            {
            //    logger.LogError($"The following exception has occurred: {ex.ToString()}");
                return new BadRequestObjectResult(ex.Message);
            }

            _logger.LogInformation($"Decrypt function executed successfully.");
            return new OkObjectResult("Decrypt function executed successfully!");
            //return new OkResult();
        }

        
    }
}


