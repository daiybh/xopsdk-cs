/**
 * Hangzhou Westsoft Information Technology Co.,Ltd.
 * Copyright (c) 1993-2016 All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using SimpleJson;
namespace com.foxhis.xop
{
	/**
	 * XMS OPEN PLATFORM CLIENT  <br>
	 * 用于XOP客户端处理，集成了sign的校验功能，POST发送功能等等 <br>
	 * 
	 * @author zhouhj
	 * 
	 */
	public class XopClient
	{
		/**
		 * 服务URL
		 */
		protected string url;
		/**
		 * 客户AppKey
		 */
		protected string appKey;
		/**
		 * 授权密码
		 */
		protected string secret;
		
		/**
		 * 连接session
		 */
		protected string sessionKey;
		/**
		 * 酒店Id
		 */
		protected string hotelId;
		
		/**
		 * 返回信息语种
		 */
		protected string locale;
		
		public XopClient(string url, string appKey, string secret, string hotelId)
			: this(url, appKey, secret, hotelId, "zh_CN")
		{
		}
		
		public XopClient(string url, string appKey, string secret, string hotelId, string locale)
		{
			this.url = url;
			this.appKey = appKey;
			this.secret = secret;
			this.sessionKey = null;
			this.hotelId = hotelId;
			this.locale = locale;
		}
		/**
		 * 判断是否已经登录
		 * @return
		 */
		public bool isLogin()
		{
			return !string.IsNullOrEmpty(sessionKey);
		}
		
		/**
		 * 登录平台
		 * @return
		 * @throws Exception
		 */
		public JsonObject login()
		{
			JsonObject request = new JsonObject();
			request["method"] = "xmsopen.public.login";
			request["ver"] = "1.0.0";
			request["secret"] = secret;
			JsonObject response = execute(request);
			if (isResponseSuccess(response)) {
				this.sessionKey = response["session"].ToString();
			}
			return response;
		}
		
		/**
		 * 登出平台
		 * @return
		 * @throws Exception
		 */
		public JsonObject logout()
		{
			JsonObject request = new JsonObject();
			request["method"] = "xmsopen.public.logout";
			request["ver"] = "1.0.0";
			JsonObject response = execute(request);
			this.sessionKey = null;
			return response;
		}
		
		/**
		 * 具体业务处理调用
		 * @param request  请求参数
		 * @return
		 * @throws Exception
		 */
		public JsonObject execute(JsonObject request)
		{
			/**
			 * 平台参数加载
			 */
			request["appkey"] = appKey;
			request["session"] = sessionKey;
			request["hotelid"] = hotelId;
			request["loc"] = locale;
			request["ts"] = 1477217353;// Convert.ToInt64((DateTime.UtcNow  -  new DateTime(1970,  1,  1,  0,  0,  0,  0)).TotalSeconds);
			request["sign"] = "";
			/**
			 * 创建数字校验
			 */
			var values = new List<string>();
			if(request.ContainsKey("params")){
				walkRequest(request["params"], ref values);
				values.Sort();
			}
			string signStr = "";
			signStr += appKey + hotelId;
			signStr += request["loc"] + request["ts"].ToString();
			for (int i = 0; i < values.Count; i++) {
				signStr += values[i];
			}
			signStr += secret;
			Trace.TraceInformation("signStr:{0}", signStr);
			var md5 = new MD5CryptoServiceProvider() as MD5CryptoServiceProvider;
			byte[] dataHash = md5.ComputeHash(Encoding.UTF8.GetBytes(signStr));
			var sb = new StringBuilder();
			foreach (var b in dataHash) {
				sb.Append(b.ToString("x2").ToUpper());
			}
			request["sign"] = sb.ToString();
			/**
			 * POST调用
			 */
			string jsonReq = SimpleJson.SimpleJson.SerializeObject(request);
			Trace.TraceInformation("send request:{0}", jsonReq);
			string jsonRsp = doPost(url, jsonReq);
			Trace.TraceInformation("recv response:{0}", jsonRsp);
			JsonObject response = SimpleJson.SimpleJson.DeserializeObject(jsonRsp) as SimpleJson.JsonObject;
			return response;
		}
		
		/**
		 * 响应对象是否是成功
		 * @param response
		 */
		public static bool isResponseSuccess(JsonObject response)
		{
			if (response.ContainsKey("success")) {
				return Convert.ToBoolean(response["success"]);
			}
			return false;
		}
		
		/**
		 * HTTP POST 调用
		 * @param url
		 * @param jsonReq
		 * @return
		 * @throws Exception
		 */
		protected static string doPost(string url, string jsonReq)
		{
			string jsonRsp = null;
			var address = new Uri(url);
			var webRequest = WebRequest.Create(address) as HttpWebRequest;
			webRequest.Method = "POST";
			webRequest.Proxy = null;
			webRequest.ContentType = "application/json";
			byte[] byteData = Encoding.UTF8.GetBytes(jsonReq);
			webRequest.ContentLength = byteData.Length;
			using (var postStream = webRequest.GetRequestStream()) {
				postStream.Write(byteData, 0, byteData.Length);
			}
			using (var webResponse = webRequest.GetResponse() as HttpWebResponse) {
				var reader = new StreamReader(webResponse.GetResponseStream());
				jsonRsp = reader.ReadToEnd();
			}
			return jsonRsp;
		}
		
		/**
		 * 获取JSONObject中所有不为空的值
		 * 
		 * @param obj
		 * @param values
		 */
		protected static void walkRequest(object obj, ref List<string> values)
		{
			if (obj is JsonObject) {
				var jso = obj as JsonObject;
				for (var i = jso.Keys.GetEnumerator(); i.MoveNext();) {
					String key = i.Current;
					walkRequest(jso[key], ref values);
				}
			} else if (obj is JsonArray) {
				var ary = obj as JsonArray;
				for (int i = 0; i < ary.Count; i++) {
					walkRequest(ary[i], ref values);
				}
			} else if (obj != null) {
				if(obj is DateTime){
					var dt = (DateTime)obj;
					long tm = Convert.ToInt64((dt  -  new DateTime(1970,  1,  1,  0,  0,  0,  0)).TotalSeconds);
					values.Add(tm.ToString().ToUpper());
				}else{
					values.Add(obj.ToString().ToUpper());
				}
			}

		}
	}
}
