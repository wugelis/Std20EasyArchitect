using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace StdEasyArchitect.Web.WebApiHostBase
{
    /// <summary>
    /// EasyArchitect 的 Api Controller WebHost.
    /// </summary>
    [Route("api/[controller]/{fileName}/{nameSpace}/{className}/{methodName}")]
    [ApiController]
    public class ApiHostBase: ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<object> Get(string filename, string nameSpace, string className, string methodName)
        {
            object result = null;

            Assembly execAssm = Assembly.Load(string.Format("{0}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", filename));//Assembly.GetExecutingAssembly();
            Type boType = execAssm.GetType(string.Format("{0}.{1}", nameSpace, className));

            if (boType != null)
            {
                var invokeObj = Activator.CreateInstance(boType);
                Type boMethodType = invokeObj.GetType();
                MethodInfo invokeMethod = boMethodType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(c => c.Name.ToLower() == methodName.ToLower())
                    .FirstOrDefault();

                if (invokeMethod != null)
                {
                    result = invokeMethod.Invoke(invokeObj, null);
                }
                else
                {
                    result = new string[] { "value1", "value2" };
                }
            }

            return result;
        }
    }
}
