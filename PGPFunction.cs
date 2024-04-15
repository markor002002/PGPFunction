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


namespace PGPFunction
{

 public class PGPEncrypt
    {

        private readonly ILogger<PGPEncrypt> _logger;

        public PGPEncrypt(ILogger<PGPEncrypt> logger)
        {
            _logger = logger;
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

                await new EncryptionDecryptionController(request).EncryptAsync();
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


