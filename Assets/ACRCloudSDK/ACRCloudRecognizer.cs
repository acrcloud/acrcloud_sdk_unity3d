using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
/*
 
   @author qinxue.pan E-mail: xue@acrcloud.com
   @version 1.0.0
   @create 2016.04.20
   
   @copyright ACRCloud 2016
   
*/

using System.Text;
using System.Runtime.InteropServices;

using System.Security.Cryptography;
using System.Net;

namespace ACRCloud {

	public class ACRCloudRecognizer {

		private string mHost = "";
		private string mAccessKey = "";
		private string mAccessSecret = "";
		private int mTimeout = 5 * 1000; // ms

		public ACRCloudRecognizer(IDictionary<string, object> config)
		{
			if (config.ContainsKey("host"))
			{
				this.mHost = (string)config["host"];
			}
			if (config.ContainsKey("access_key"))
			{
				this.mAccessKey = (string)config["access_key"];
			}
			if (config.ContainsKey("access_secret"))
			{
				this.mAccessSecret = (string)config["access_secret"];
			}
			if (config.ContainsKey("timeout_seconds"))
			{
				this.mTimeout = 1000 * (int)config["timeout_seconds"];
			}
		}

		/**
          *
          *  recognize by wav audio buffer(RIFF (little-endian) data, WAVE audio, Microsoft PCM, 16 bit, mono 8000 Hz) 
          *
          *  @param wavAudioBuffer query audio buffer
          *  @param wavAudioBufferLen the length of wavAudioBuffer
          *  
          *  @return result 
          *
          **/
		public string Recognize(byte[] pcmAudioBuffer, int pcmAudioBufferLen)
		{
			byte[] fp = null;
			try {
				fp = ACRCloudFingerprintTool.CreateFingerprint(pcmAudioBuffer, pcmAudioBufferLen);
			}catch(Exception e) {
				return ACRCloudErrorInfo.getJsonInfo(ACRCloudErrorInfo.GEN_FP_ERROR, e.ToString());
			}
			if (fp == null) {
				return null;
			}
		 	return this.DoRecognize(fp);
		}

		private string DoRecognize(byte[] queryData)
		{
			string method = "POST";
			string httpURL = "/v1/identify";
			string dataType = "fingerprint";
			string sigVersion = "1";
			string timestamp = ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

			string reqURL = "http://" + mHost + httpURL;

			string sigStr = method + "\n" + httpURL + "\n" + mAccessKey + "\n" + dataType + "\n" + sigVersion + "\n" + timestamp;
			string signature = EncryptByHMACSHA1(sigStr, this.mAccessSecret);

			var dict = new Dictionary<string, object>();
			dict.Add("access_key", this.mAccessKey);
			dict.Add("sample_bytes", queryData.Length.ToString());
			dict.Add("sample", queryData);
			dict.Add("timestamp", timestamp);
			dict.Add("signature", signature);
			dict.Add("data_type", dataType);
			dict.Add("signature_version", sigVersion);

			string res = PostHttp(reqURL, dict);

			return res;
		}
			
		private string PostHttpV1(string url, IDictionary<string, object> postParams)
		{
			string res = "";
			WWWForm postForm = new WWWForm();  
			foreach (var item in postParams)
			{
				if (item.Value is string) {
					postForm.AddField (item.Key, (string)item.Value);
				} else if (item.Value is byte[]) {
					postForm.AddBinaryData (item.Key, (byte[])item.Value);
				}
			}
			WWW httpHandler = new WWW(url, postForm);  
			while (!httpHandler.isDone) {
			}

			if(httpHandler.error != null) {  
				Debug.LogError (httpHandler.error);  
				return res;
			}
			return httpHandler.text;
		}

		private string PostHttp(string url, IDictionary<string, object> postParams)
		{
			string result = "";

			var BOUNDARYSTR = "acrcloud***copyright***" + DateTime.Now.Ticks.ToString("x");
			var BOUNDARY = "--" + BOUNDARYSTR + "\r\n";
			var ENDBOUNDARY = Encoding.ASCII.GetBytes("--" + BOUNDARYSTR + "--\r\n\r\n");

			string stringKeyHeader = BOUNDARY +
				"Content-Disposition: form-data; name=\"{0}\"" +
				"\r\n\r\n{1}\r\n";
			string filePartHeader = BOUNDARY +
				"Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
				"Content-Type: application/octet-stream\r\n\r\n";

			HttpWebRequest request = null;
			HttpWebResponse response = null;
			Stream writer = null;
			StreamReader myReader = null;
			MemoryStream memStream = null;
			try
			{
				memStream = new MemoryStream();
				foreach (var item in postParams)
				{
					if (item.Value is string)
					{
						string tmpStr = string.Format(stringKeyHeader, item.Key, item.Value);
						byte[] tmpBytes = Encoding.UTF8.GetBytes(tmpStr);
						memStream.Write(tmpBytes, 0, tmpBytes.Length);
					}
					else if (item.Value is byte[])
					{
						var header = string.Format(filePartHeader, "sample", "sample");
						var headerbytes = Encoding.UTF8.GetBytes(header);
						memStream.Write(headerbytes, 0, headerbytes.Length);
						byte[] sample = (byte[])item.Value;
						memStream.Write(sample, 0, sample.Length);
						memStream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, 2);
					}
				}
				memStream.Write(ENDBOUNDARY, 0, ENDBOUNDARY.Length);
				memStream.Position = 0;
				byte[] postBuffer = new byte[memStream.Length];
				memStream.Read(postBuffer, 0, postBuffer.Length);
				memStream.Close();

				request = (HttpWebRequest)WebRequest.Create(url);
				request.Timeout = this.mTimeout;
				request.Method = "POST";
				request.ContentType = "multipart/form-data; boundary=" + BOUNDARYSTR;

				writer = request.GetRequestStream();
				writer.Write(postBuffer, 0, postBuffer.Length);
				writer.Flush();
				writer.Close();
				writer = null;

				response = (HttpWebResponse)request.GetResponse();
				myReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				result = myReader.ReadToEnd();
			}
			catch (Exception e)
			{
				Debug.LogError("excption:" + e.ToString());
			}
			finally
			{
				if (myReader != null)
				{
					myReader.Close();
					myReader = null;
				}
				if (response != null)
				{
					response.Close();
					response = null;
				}
			}

			return result;
		}

		private string EncryptByHMACSHA1(string input, string key)
		{
			HMACSHA1 hmac = new HMACSHA1(System.Text.Encoding.UTF8.GetBytes(key));
			byte[] stringBytes = Encoding.UTF8.GetBytes(input);
			byte[] hashedValue = hmac.ComputeHash(stringBytes);
			return EncodeToBase64(hashedValue);
		}

		private string EncodeToBase64(byte[] input)
		{
			string res = Convert.ToBase64String(input, 0, input.Length);
			return res;
		}
	}

	public class ACRCloudFingerprintTool {
		#if UNITY_IPHONE 
		[DllImport ("__Internal")] 
		private static extern int native_create_fingerprint(byte[] pcm, int pcm_len, ref IntPtr out_fp);
		[DllImport ("__Internal")] 
		private static extern void native_free(IntPtr buffer);
		#else 
		[DllImport ("ACRCloudExtrTool", CallingConvention = CallingConvention.Cdecl)] 
		private static extern int native_create_fingerprint(byte[] pcm, int pcm_len, ref IntPtr out_fp);
		[DllImport ("ACRCloudExtrTool", CallingConvention = CallingConvention.Cdecl)] 
		private static extern void native_free(IntPtr buffer);
		#endif  

		/**
          *
          *  create acrcloud fingerprint from wav audio buffer(RIFF (little-endian) data, WAVE audio, Microsoft PCM, 16 bit, mono 8000 Hz) 
          *
          *  @param pcmBuffer query audio buffer
          *  @param pcmBufferLen the length of wavAudioBuffer
          *  
          *  @return byte[] fingerprint, if return null, create fingerprint error 
          *
          **/
		public static byte[] CreateFingerprint(byte[] pcmBuffer, int pcmBufferLen) {
			byte[] fpBuffer = null;
			if (pcmBuffer == null || pcmBufferLen <= 0)
			{
				return fpBuffer;
			}
			if (pcmBufferLen > pcmBuffer.Length)
			{
				pcmBufferLen = pcmBuffer.Length;
			}

			IntPtr pFpBuffer = IntPtr.Zero;
			int fpBufferLen = native_create_fingerprint(pcmBuffer, pcmBufferLen, ref pFpBuffer); 
			if (fpBufferLen <= 0)
			{
				return fpBuffer;
			}

			fpBuffer = new byte[fpBufferLen];
			Marshal.Copy(pFpBuffer, fpBuffer, 0, fpBufferLen);

			native_free(pFpBuffer);

			return fpBuffer;
		}
	}
}
