using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StdEasyArchitect.Web.WebApiHostBase;

namespace WebCore21ApiApp1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private object ExecuteApiMethod(
            string dllName,
            string nameSpace,
            string className,
            string methodName,
            object parameter)
        {
            string appPath = string.Empty;

            // 標記：True正式環境，False:UnitTest環境
            bool result = true;

            string nameSapce = string.Empty;

            object returnValue = null;
            //UserAuthType user = new UserAuthType();

            try
            {
                string dllPath = "";

                if (result)
                {
                    dllPath = "bin/" + dllName + ".dll";
                }
                else
                {
                    // 此路徑為project/bin/debug
                    dllPath = "/" + dllName + ".dll";
                }

                // 判斷上傳的實體路徑下是否有此文件，沒有提示錯誤信息
                if (!System.IO.File.Exists(appPath + dllPath))
                {
                    RPCServiceException rpc = new RPCServiceException();

                    rpc.Datetime = DateTime.Now;

                    rpc.Message = "請檢查DLL是否存在!";

                    rpc.MethodInfo = string.Format("{0}/{1}/{2}/{3}", dllName, nameSpace, className, methodName);
                    //if (HttpContext.Current != null)
                    //{
                    //    rpc.UserName = user.GetUserName;
                    //    rpc.HostUrl = HttpContext.Current.Request.Url.ToString();
                    //}
                    return rpc;
                }
                else
                {
                    //載入指定路徑中的 Assembly.
                    Assembly ass = Assembly.LoadFile(appPath + dllPath);

                    if (ass != null)
                    {
                        Type magicType = ass.GetType(nameSpace + "." + className);
                        //returnValue = Common(ass, dllName, nameSpace, className, methodName, parameter);
                    }
                    else
                    {
                        RPCServiceException rpc = new RPCServiceException();
                        rpc.Datetime = DateTime.Now;
                        rpc.Message = "請檢查DLL是否存在!";
                        rpc.MethodInfo = string.Format("{0}/{1}/{2}/{3}", dllName, nameSpace, className, methodName);
                        //if (HttpContext.Current != null)
                        //{
                        //    rpc.UserName = user.GetUserName;
                        //    rpc.HostUrl = HttpContext.Current.Request.Url.ToString();
                        //}
                        return rpc;
                    }
                }
            }
            catch (Exception ex)
            {
                RPCServiceException rpc = new RPCServiceException();

                rpc.Datetime = DateTime.Now;
                rpc.Message = ex.Message;
                rpc.MethodInfo = string.Format("{0}/{1}/{2}/{3}", dllName, nameSpace, className, methodName);

                //if (HttpContext.Current != null)
                //{
                //    rpc.UserName = user.GetUserName;
                //    rpc.HostUrl = HttpContext.Current.Request.Url.ToString();
                //}
                rpc.SourceException = ex.Source;
                rpc.StackTrace = ex.StackTrace;

                return rpc;
            }

            return returnValue;
        }
    }
}
