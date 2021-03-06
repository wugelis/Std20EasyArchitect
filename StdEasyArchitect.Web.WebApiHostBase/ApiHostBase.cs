﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace StdEasyArchitect.Web.WebApiHostBase
{
    /// <summary>
    /// EasyArchitect 的 Api Controller WebHost.
    /// </summary>
    [Route("api/[controller]/{fileName}/{nameSpace}/{className}/{methodName}/{*pathInfo}")]
    [Route("api/[controller]/{fileName}/{nameSpace}/{methodName}/{*pathInfo}")]
    [Route("api/[controller]/{fileName}/{methodName}/{*pathInfo}")]
    [Route("api/[controller]/{fileName}/{*pathInfo}")]
    [Route("api/[controller]/{*pathInfo}")]
    [ApiController]
    public class ApiHostBase: ControllerBase
    {
        /// <summary>
        /// 取得 HTTP Body 參數內容方法
        /// </summary>
        /// <returns></returns>
        private object GetParameter()
        {
            object inputStramStr = null;
            var ContentType = HttpContext.Request.ContentType;

            if (!string.IsNullOrEmpty(ContentType)
                && (ContentType.IndexOf("application/json") >= 0 || ContentType.IndexOf("text/plain") >= 0))
            {
                MemoryStream ms = new MemoryStream();
                HttpContext.Request.Body.CopyTo(ms);

                using (StreamReader inputStream = new StreamReader(ms, Encoding.UTF8))
                {
                    inputStramStr = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            else
            {
                inputStramStr = HttpContext.Request.Body;
            }

            return inputStramStr;
        }
        /// <summary>
        /// 去除 Raw Data 內單一參數值 JSON 的雙引號.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private string ReleaseStartEndQuotes(string parameter)
        {
            parameter = parameter.StartsWith("\"") ? parameter.Substring(1, parameter.Length - 1) : parameter;
            parameter = parameter.EndsWith("\"") ? parameter.Substring(0, parameter.Length - 1) : parameter;
            return parameter;
        }

        #region The binary read method.
        /// <summary>
        /// The binary read method.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private byte[] BinaryReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
        #endregion

        /// <summary>
        /// ApiHostBase 核心所提供的共用的 Post 方法
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="nameSpace"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<object> Post(
            string fileName,
            string nameSpace,
            string className,
            string methodName)
        {
            object result = null;

            if (string.IsNullOrEmpty(fileName) ||
                string.IsNullOrEmpty(nameSpace) ||
                string.IsNullOrEmpty(className) ||
                string.IsNullOrEmpty(methodName))
            {
                return GetJSONMessage("傳入的 Url 有誤！請確認呼叫 Api 的 Url 的格式！");
            }

            object parameter = GetParameter();

            Assembly assem = Assembly.Load($"{fileName}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            if(assem != null)
            {
                Type runtimeType = assem.GetType($"{nameSpace}.{className}");
                object targetObj = Activator.CreateInstance(runtimeType);
                object invokeObj = null;
                MethodInfo[] methods = runtimeType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);

                bool found = false;
                foreach(var method in methods)
                {
                    if (method.Name.ToLower() == methodName.ToLower())
                    {
                        ParameterInfo[] parames = method.GetParameters();
                        if (parames.Length > 0)
                        {
                            string paramName = parames[0].Name;
                            Type propertyType = parames[0].GetType();
                            Type parameType = parames[0].ParameterType;

                            switch (parameType.ToString())
                            {
                                case "System.Int16":
                                    invokeObj = Int16.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.Int32":
                                    invokeObj = int.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.Int64":
                                    invokeObj = Int64.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.Single":
                                    invokeObj = Single.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.DateTime":
                                    invokeObj = DateTime.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.Double":
                                    invokeObj = double.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.Decimal":
                                    invokeObj = decimal.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.Boolean":
                                    invokeObj = bool.Parse(ReleaseStartEndQuotes(parameter.ToString()));
                                    break;
                                case "System.String":
                                    invokeObj = ReleaseStartEndQuotes(parameter.ToString());
                                    break;
                                case "System.Byte[]":
                                    if (parameter is Stream)
                                    {
                                        Stream content = parameter as Stream;
                                        invokeObj = BinaryReadToEnd(content);
                                    }
                                    else
                                    {
                                        invokeObj = JsonConvert.DeserializeObject(parameter.ToString(), parameType);
                                    }
                                    break;
                                default:    //如果都不是以上的物件，才進行 JSON DeserializeObject.
                                    invokeObj = JsonConvert.DeserializeObject(parameter.ToString(), parameType);
                                    break;
                            }

                            result = method.Invoke(targetObj, new object[] { invokeObj });
                        }
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// 實作 Get 方法.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="nameSpace"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public ActionResult<object> Get(string fileName, string nameSpace, string className, string methodName)
        {
            if (string.IsNullOrEmpty(fileName) ||
                string.IsNullOrEmpty(nameSpace) ||
                string.IsNullOrEmpty(className) ||
                string.IsNullOrEmpty(methodName))
            {
                return GetJSONMessage("使用的 Url 呼叫方式有誤，請確認！");
            }

            object result = null;
            Assembly targetAsm = null;

            try
            {
                targetAsm = Assembly.Load($"{fileName}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            }
            catch
            {
                try
                {
                    targetAsm = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), $"bin\\Debug\\netcoreapp2.1\\{fileName}.dll"));
                }
                catch (Exception ex)
                {
                    return GetJSONMessage($"讀取 DLLs 檔案：{fileName} 發生錯誤, 錯誤訊息：{ex.Message}");
                }
            }


            if (targetAsm == null)
            {
                return GetJSONMessage($"在 bin 底下找不到名稱為 {fileName} 的 DLL！");
            }

            Type targetObjType = targetAsm.GetType($"{nameSpace}.{className}");
            if (targetObjType == null)
            {
                return GetJSONMessage("使用的 Url 找不到可呼叫的 Web API！請確認！");
            }

            object targetObjIns = Activator.CreateInstance(targetObjType);
            var targetMethod = targetObjType.GetMethods(BindingFlags.Public | BindingFlags.Default | BindingFlags.Instance)
                .Where(c => c.Name.ToLower() == methodName.ToLower())
                .FirstOrDefault();

            if (targetMethod == null)
            {
                return GetJSONMessage($"在 DLL {fileName} 裡找不到名稱為 {methodName} 的方法！");
            }

            var queryString = Request.Query;
            if (queryString.Count() > 0)
            {
                ParameterInfo[] parames = targetMethod.GetParameters();
                if (parames.Length > 0)
                {
                    ParameterInfo param01 = parames[0];
                    Type param01Type = param01.ParameterType;
                    object param01Ins = Activator.CreateInstance(param01Type);

                    foreach (var q in queryString.Keys)
                    {
                        string keyName = q;
                        string keyValue = queryString[q].ToString();

                        var targetProperty = (from pm in param01Type.GetProperties(
                                                BindingFlags.Public |
                                                BindingFlags.Instance |
                                                BindingFlags.Default)
                                              where pm.Name.ToLower() == keyName.ToLower()
                                              select pm).FirstOrDefault();

                        if (targetProperty != null)
                        {
                            switch (targetProperty.PropertyType.FullName)
                            {
                                case "System.Int32":
                                    targetProperty.SetValue(param01Ins, int.Parse(keyValue));
                                    break;
                                case "System.String":
                                    targetProperty.SetValue(param01Ins, keyValue);
                                    break;
                                case "System.Decimal":
                                    targetProperty.SetValue(param01Ins, decimal.Parse(keyValue));
                                    break;
                                case "System.Int64":
                                    targetProperty.SetValue(param01Ins, long.Parse(keyValue));
                                    break;
                                case "System.Int16":
                                    targetProperty.SetValue(param01Ins, short.Parse(keyValue));
                                    break;
                                case "System.DataTime":
                                    targetProperty.SetValue(param01Ins, DateTime.Parse(keyValue));
                                    break;
                                case "System.Double":
                                    targetProperty.SetValue(param01Ins, double.Parse(keyValue));
                                    break;
                                default:
                                    if (targetProperty.PropertyType == typeof(Nullable<DateTime>))
                                    {
                                        targetProperty.SetValue(param01Ins, DateTime.Parse(keyValue));
                                    }
                                    break;
                            }

                        }
                    }
                    result = targetMethod.Invoke(targetObjIns, new object[] { param01Ins });
                }
            }
            else
            {
                result = targetMethod.Invoke(targetObjIns, null);
            }

            return result;
        }

        private static object GetJSONMessage(string message)
        {
            return new string[] { message }
                .Select(msg => new {
                    Err = msg
                }).ToList();
        }
    }
}
