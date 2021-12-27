/**
 * Hangzhou Westsoft Information Technology Co.,Ltd.
 * Copyright (c) 1993-2016 All Rights Reserved.
 * 
 * 用户： zhouhj
 * 日期: 2016/5/22
 * 
 */
using System;
using SimpleJson;

namespace com.foxhis.xop.sample
{
	class Program
	{
		public static void Main(string[] args)
		{
			// 平台提供的地址
			const string url = "http://xop.test.foxhis.com/xmsopen-web/rest";
			// 平台appkey
			const string appKey = "XOPTEST";
			// 平台授权码
			const string secret = "fcKL3Ib4lESYaz8cMPf";
			// 使用哪个酒店的身份接入 
			const string hotelId = "G000001";
			
			try {
				var client = new XopClient(url,appKey,secret,hotelId);
				// 开始登录平台
				var rspLogin = client.login();
				Console.WriteLine("login-response:");
				Console.WriteLine(SimpleJson.SimpleJson.SerializeObject(rspLogin));
				if (!XopClient.isResponseSuccess(rspLogin))
				{
					Console.WriteLine("ERROR login");
					return;
				}
				
				// 进行业务处理
				// 使用 JsonObject按apidoc中的  规格组织对象
				var request = new JsonObject();
				request["method"] = "xmsopen.xoptest.xopechotest";
				request["ver"] = "1.0.0";
				var jary = new JsonArray();
				request["params"] = jary;
				var param = new JsonObject();
				jary.Add(param);
				param["rq"] = new DateTime(2006,  1,  7,  10,  10,  10,  0);//DateTime.Now;
				param["hotelid"] = "H000001";
				param["ok"] = true;
				JsonObject rsp = client.execute(request);
				Console.WriteLine("echoTest-response:");
				Console.WriteLine(SimpleJson.SimpleJson.SerializeObject(rsp));
					
				//登出平台
				var rspLogout = client.logout();
				Console.WriteLine("logout-response:");
				Console.WriteLine(SimpleJson.SimpleJson.SerializeObject(rspLogout));
	
			}
			catch (Exception e)
			{
				Console.WriteLine("exception:");
				Console.WriteLine(e.ToString());
			}


			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}

 
}