using System;
using System.Runtime.Serialization;

namespace StdEasyArchitect.Web.WebApiHostBase
{
    /// <summary>
    /// 提供可序列化至前端的錯誤類別.
    /// </summary>
    [DataContract]
    public class RPCServiceException
    {
        /// <summary>
        /// 來源錯誤實體物件
        /// </summary>
        [DataMember]
        public string SourceException;
        /// <summary>
        /// 錯誤發生時間
        /// </summary>
        [DataMember]
        public DateTime Datetime { get; set; }
        /// <summary>
        /// Current Identity username for Web Handler.
        /// </summary>
        [DataMember]
        public string UserName { get; set; }
        /// <summary>
        /// 用戶端要求的 Request Url.
        /// </summary>
        [DataMember]
        public string HostUrl { get; set; }
        /// <summary>
        /// 錯誤訊息.
        /// </summary>
        [DataMember]
        public string Message { get; set; }
        /// <summary>
        /// 錯誤發生位置.
        /// </summary>
        [DataMember]
        public string ExceptionLocation { get; set; }
        /// <summary>
        /// 發錯誤的方法名稱.
        /// </summary>
        [DataMember]
        public string MethodInfo { get; set; }
        /// <summary>
        /// 詳細的堆疊錯誤資訊.
        /// </summary>
        [DataMember]
        public string StackTrace { get; set; }
    }
}