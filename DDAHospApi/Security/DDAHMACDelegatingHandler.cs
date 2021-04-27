using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace DDAApi.Security
{
    public class DDAHMACDelegatingHandler: DelegatingHandler
    {
        private string AppId;
        private string SecretKey;

        public DDAHMACDelegatingHandler(string AppId, string SecretKey)
        {
            this.AppId = AppId;
            this.SecretKey = SecretKey;

        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            List<string> list = new List<string>();

            //1. Get the Request HTTP Method type
            string requestHttpMethod = request.Method.Method;
            list.Add(requestHttpMethod);

            //2. Get the Request URI
            string requestPath = request.RequestUri.AbsolutePath.ToLower();
            list.Add(requestPath);

            //3. Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
            list.Add(requestTimeStamp);

            //4. Access Key string APPId = "c657d5ab7b324f17a3fe3181aa549a36";
            list.Add(AppId);
            //5. Secret Key string APIKey = "3a3bf81a0dca437fa1e6fcce5aa8e71c";
            list.Add(SecretKey);

            //6. Create the random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");
            list.Add(nonce);

            list.Sort(StringComparer.Ordinal);
            var joinedStr = string.Join("&", list);
            //var signStr = joinedStr.Substring(1);

            string signature = CalculateMD5Hash(joinedStr);

            HttpResponseMessage response = null;
            string requestContentBase64String = string.Empty;


            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("AppId", AppId);
            request.Headers.Add("Nonce", nonce);
            request.Headers.Add("Timestamp", requestTimeStamp);
            request.Headers.Add("Sign", signature);

            //this._logger.LogError($"AppId: {AppId};\n Nonce: {nonce};\n Timestamp: {requestTimeStamp};\n Sign: {signature};");

            response = await base.SendAsync(request, cancellationToken);
            return response;
        }

        private string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
