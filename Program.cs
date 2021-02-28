using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace UserDelegationSaS
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("User delegation Sas...");

         //Login with credential
         Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", "ae0a98cb-1600-4901-9aca-0088ce249581");
         Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", "PHWQ7C3j~ZpqeC1uKmfY9nDacbU_bOxhVc");
         Environment.SetEnvironmentVariable("AZURE_TENANT_ID", "4e6f57dc-a3d9-4a0c-818b-a7c1bb2b79f6");

         var cred = new DefaultAzureCredential();
         var blobSvcClient = new BlobServiceClient(new Uri($"https://chyastorage.blob.core.windows.net"), cred);

         //Get user delegation key, make sure RBAC is assigned
         var userDelegationKey = GetUserDelegationKey(blobSvcClient);
         userDelegationKey.Wait();

         var sasBuilder = new BlobSasBuilder(BlobSasPermissions.All, userDelegationKey.Result.SignedExpiresOn);
         var sas = sasBuilder.ToSasQueryParameters(userDelegationKey.Result, "chyastorage");

         Console.WriteLine("User delegation SaS:");
         Console.WriteLine(sas.ToString());

         var blobContainer = blobSvcClient.GetBlobContainerClient("user");
         var blob = blobContainer.GetBlobClient("UserDelegationSaS" + DateTime.Now.DayOfYear + DateTime.Now.TimeOfDay);

         using var stream = new MemoryStream();
         stream.Write(new byte[]{1,2,3,4,5,6,7,8,9,0});
         stream.Position = 0;

         blob.Upload(stream);

         Console.WriteLine($"{blob.Name} uploaded to container user by using User Delegation SaS...");

         Console.ReadKey();

      }

      static async Task<UserDelegationKey> GetUserDelegationKey(BlobServiceClient client)
      {
         var userDelegationKey = await client.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
         Console.WriteLine("User delegation key:");
         Console.WriteLine(userDelegationKey.Value);

         return userDelegationKey;
      }

   }
}
