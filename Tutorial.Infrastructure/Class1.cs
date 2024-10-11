/*
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Project.Modules.Accounts.Entities;
using Project.Modules.Areas.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project.App.Helpers
{
    public static class GeneralHelper
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<(string responseData, int? responseStatusCode)> SendRequestAsync(this HttpRequestMessage httpRequestMessage, string endpointURL, IDictionary<string, object> headers)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, object> header in headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value.ToString());
                }
            }
            try
            {
                using HttpResponseMessage httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                int responseStatusCode = (int)httpResponseMessage.StatusCode;
                if (httpResponseMessage.Content != null)
                {
                    return (await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false), responseStatusCode);
                }
                else if (httpResponseMessage.Content is null && httpResponseMessage.IsSuccessStatusCode)
                {
                    return (null, responseStatusCode);
                }
                else
                {
                    return ($"Request to {endpointURL} error! StatusCode = {httpResponseMessage.StatusCode}", responseStatusCode);
                }
            }
            catch (Exception ex)
            {
                return (ex.Message, (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<(string responseData, int? responseStatusCode)> SendRequestWithStringContentAsync(this HttpMethod method, string endpointURL, string encodingData = null, IDictionary<string, object> headers = null, string mediaType = "application/json")
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, endpointURL);
            if (!(encodingData is null))
            {
                httpRequestMessage.Content = new StringContent(encodingData, Encoding.UTF8, mediaType);
            }
            return await httpRequestMessage.SendRequestAsync(endpointURL, headers);
        }

        public static async Task<(string responseData, int? responseStatusCode)> SendRequestWithFormDataContentAsync(this HttpMethod method, string endpointURL, MultipartFormDataContent multipartFormData = null, IDictionary<string, object> headers = null)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, endpointURL);
            if (!(multipartFormData is null))
            {
                httpRequestMessage.Content = multipartFormData;
            }
            return await httpRequestMessage.SendRequestAsync(endpointURL, headers);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext">
        /// Getting the current url of the request depends on whether you use Https or not?
        /// </param>
        /// <param name="useHttps">
        /// Option to use Https (The parameter is taken from the UseHttps property in appsetting.json):
        ///     There used: true
        ///     Do not use: false
        /// </param>
        /// <returns></returns>
        public static string GetBaseUrl(this HttpContext httpContext, bool useHttps)
        {
            if (useHttps)
            {
                return httpContext.Request.Scheme + "s://" + httpContext.Request.Host.Value;
            }
            else
            {
                return httpContext.Request.Scheme + "://" + httpContext.Request.Host.Value;
            }
        }

        public static T MergeData<T>(this object newData, T originData)
        {
            foreach (PropertyInfo propertyInfo in newData.GetType().GetProperties())
            {
                if (propertyInfo.GetValue(newData, null) != null && originData.GetType().GetProperties().Any(p => p.Name.Equals(propertyInfo.Name)))
                {
                    originData.GetType().GetProperty(propertyInfo.Name).SetValue(originData, propertyInfo.GetValue(newData, null));
                }
            }
            return originData;
        }

        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0);
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.StartOfMonth().AddMonths(1).AddSeconds(-1);
        }

        public static async Task<string> UploadFile(this IFormFile requestFile, string folderPath)
        {
            string fileFullName = new Random().Next() + "_" + Regex.Replace(requestFile.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "");
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            Directory.CreateDirectory(Path.Combine(webRootPath, folderPath)); // Tự động tạo dường dẫn thư mục nếu chưa có
            using (var stream = new FileStream(Path.Combine(webRootPath, folderPath, fileFullName), FileMode.Create))
            {
                await requestFile.CopyToAsync(stream);
                stream.Close();
            }
            return fileFullName;
        }

        public static (bool, string) DeleteFile(this string FileToDelete)
        {
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            try
            {
                string fullPathToDelete = Path.Combine(webRootPath, FileToDelete);
                if (File.Exists(fullPathToDelete))
                {
                    File.Delete(fullPathToDelete);
                    return (true, "File deleted successfully!");
                }
                else
                {
                    return (false, "File no longer exists on the server!");
                }
            }
            catch (IOException ioExp)
            {
                return (false, ioExp.Message);
            }
        }

        public static string MD5Hash(this string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.ASCII.GetBytes(text)); // Compute hash from the bytes of text
            byte[] result = md5.Hash; // Get hash result after compute it  
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2")); // Change it into 2 hexadecimal digits
            }
            return strBuilder.ToString();
        }

        public static bool PingIPDevice(this string targetHost, string data = "PingForTest")
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions
            {
                DontFragment = true
            };
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            PingReply reply = pingSender.Send(targetHost, 120, buffer, options);
            if (reply.Status == IPStatus.Success)
                return true;
            return false;
        }

        public static bool ValidateIPv4(this string DeviceIP)
        {
            if (string.IsNullOrWhiteSpace(DeviceIP))
                return false;
            string[] splitValues = DeviceIP.Split('.');
            if (splitValues.Length != 4)
                return false;
            return splitValues.All(r => byte.TryParse(r, out byte tempForParsing));
        }

        public static string GetMimeType(this string fileName)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);
            return contentType ?? "application/octet-stream";
        }
        public static string RandomString(this int count)
        {
            string b = "abcdefghijklmnopqrstuvwxyz1234567890ASDFGHJKLMNBVCXZQWETYUIOP";
            Random ran = new Random();
            string random = "";
            for (int i = 0; i < count; i++)
            {
                int a = ran.Next(26);
                random += b.ElementAt(a);
            }
            return random;
        }
        public static string HashPassword(this string password)
        {
            using SHA512 sha = SHA512.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha.ComputeHash(inputBytes);
            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
        public static string HashPasswordSHA256(this string password)
        {
            using SHA256 sha = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha.ComputeHash(inputBytes);
            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
        public static (bool ExecuteCommandStatus, string ExecuteCommandErrorMessage) ExecuteCommand(this string applicationName, string commandString)
        {
            try
            {
                Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = applicationName,
                        Arguments = commandString,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }

                };
                process.Start();
                process.WaitForExitAsync();
                //string standardOutput = process.StandardOutput.ReadToEnd();
                //Console.WriteLine("ExecuteCommand:StandardOutput: " + standardOutput);
                string standardError = process.StandardError.ReadToEnd();
                Console.WriteLine("ExecuteCommand:StandardError: " + standardError);
                //if (!string.IsNullOrEmpty(standardError))
                //{
                //    return (false, standardError);
                //}
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.GetBaseException().ToString());
            }
        }
        public static (string output, string ExecuteCommandErrorMessage) ExecuteCommandWithOutput(this string applicationName, string commandString)
        {
            try
            {
                Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = applicationName,
                        Arguments = commandString,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }

                };
                process.Start();
                process.WaitForExitAsync();
                string standardOutput = process.StandardOutput.ReadToEnd();
                Console.WriteLine("ExecuteCommand:StandardOutput: " + standardOutput);
                string standardError = process.StandardError.ReadToEnd();
                Console.WriteLine("ExecuteCommand:StandardError: " + standardError);
                if (!string.IsNullOrEmpty(standardError))
                {
                    return (standardOutput, standardError);
                }
                return (standardOutput, null);
            }
            catch (Exception ex)
            {
                return (null, ex.GetBaseException().ToString());
            }
        }
        public static ContextUser GetContextUser(this HttpContext httpContext, bool getPermissions = true)
        {
            try
            {
                Claim userAreaClaim = httpContext.User.FindFirst(ExtendClaimTypes.AreaId);
                if (userAreaClaim is null)
                {
                    throw new HttpResponseException() { Status = 401, Value = "UnAuthorize" };
                }
                Claim accountTypeClaim = httpContext.User.FindFirst(ExtendClaimTypes.AccountType);
                if (accountTypeClaim is null)
                {
                    throw new HttpResponseException() { Status = 401, Value = "UnAuthorize" };
                }
                Claim userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userAreaClaim is null)
                {
                    throw new HttpResponseException() { Status = 401, Value = "UnAuthorize" };
                }
                List<string> permissions = null;
                if (getPermissions)
                {
                    permissions = new List<string>();
                    permissions = httpContext.User.FindAll(ClaimTypes.Role).Select(x => x.Value.ToString()).ToList();
                }
                return new ContextUser(userAreaClaim.Value, userIdClaim.Value, (AccountRegion)Enum.Parse(typeof(AccountRegion), accountTypeClaim.Value, true), permissions);
            }
            catch
            {
                throw new HttpResponseException() { Status = 401, Value = "UnAuthorize" };
            }
        }
        public static bool CheckContextRole(this HttpContext httpContext, string roles)
        {
            if (!string.IsNullOrEmpty(roles))
                return true;
            string[] roleArr = roles.Split(",");
            foreach (string role in roleArr)
            {
                if (!string.IsNullOrEmpty(role))
                    if (!httpContext.User.IsInRole(role))
                        return false;
            }
            return true;
        }
        public static Int64 BitwiseDayArray(this List<int> source, bool isWeek)
        {
            Int64 result = 0;
            source.ForEach(x => result += 1 << Math.Max(0, isWeek ? (x + 6) % 7 : (x - 1)));
            return result;
        }
        public static bool IsBase64(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }
    }
}
*/