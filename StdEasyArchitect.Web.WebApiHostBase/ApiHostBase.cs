using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;

namespace StdEasyArchitect.Web.WebApiHostBase
{
    /// <summary>
    /// EasyArchitect 的 Api Controller 中間層 (Middleware)
    /// </summary>
    public class ApiHostBase: ControllerBase
    {
        private object GetParameter(NameValueCollection getParameters)
        {
            if (getParameters.Count <= 0)
                return null;

            if (getParameters.Count == 1)
            {
                return getParameters[0];
            }

            var DictForNameValue = NameValue2Dictionary(getParameters, false);
            string json = JsonConvert.SerializeObject(DictForNameValue);
            return json;
        }

        private Dictionary<string, object> NameValue2Dictionary(NameValueCollection nvc, bool handleMultipleValuesPerKey)
        {
            var result = new Dictionary<string, object>();
            foreach (string key in nvc.Keys)
            {
                if (handleMultipleValuesPerKey)
                {
                    string[] values = nvc.GetValues(key);
                    if (values.Length == 1)
                    {
                        result.Add(key, values[0]);
                    }
                    else
                    {
                        result.Add(key, values);
                    }
                }
                else
                {
                    result.Add(key, nvc[key]);
                }
            }

            return result;
        }
        public HttpResponseMessage Get(string fileName, string nameSpaceName, string className, string methodName)
        {


            //return Request.CreateResponse(HttpStatusCode.MethodNotAllowed, new RPCServiceException() { Message = "此服務不支援 GET 方法！請使用 POST 方法。" });
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }
    }
}
