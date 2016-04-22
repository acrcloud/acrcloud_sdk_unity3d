using UnityEngine;
using System.Collections;

using System;
using System.IO;
using System.Collections.Generic;

using System.Threading;
using System.Runtime.InteropServices;

using LitJson;

namespace ACRCloud {
	
	public interface IACRCloudWorkerListener {
		/**
          *
          *  callback function of ACRCloudWorker, you can implement in a component script.
          *
          **/
		void OnResult(string result);
	}

	public class ACRCloudWorker {
		
		private IACRCloudWorkerListener mListener = null;
		private ACRCloudRecorder mRecorder = null;
		private ACRCloudRecognizer mRecognizer = null;

		private int mRecognizeInterval = 3; // seconds
		private int mMaxRecognizeAudioTime = 12;
		private int mHttpErrorRetryNum = 3;

		private bool mIsRunning = false;
		private bool mIsStopRecord = false;

		public ACRCloudWorker(IACRCloudWorkerListener lins, ACRCloudRecorder recorder, IDictionary<string, object> config) {
			this.mListener = lins;
			this.mRecorder = recorder;
			this.mRecognizer = new ACRCloudRecognizer(config);
		}

		/**
          *
          *  Cancel this recognition Session.
          * 
          *  Note:  ACRCloudWorker do not callback OnResult.
          * 
          **/
		public void Cancel() {
			this.mIsRunning = false;
		}

		/**
          *
          *  Make ACRCloudWorker know Recorder is stopped.
          * 
          *  Note: If you do not click this button(Stop Record and recognize), 
          *        ACRCloudWorker can callback OnResult when it has result.
          * 
          **/
		public void StopRecordToRecognize() {
			this.mIsStopRecord = true;
		}

		/**
          *
          *  Start a Thread to recognize.
          * 
          **/
		public void Start() {
			Thread newThrd = new Thread(new ThreadStart(Run));
			newThrd.Start();
		}

		/**
          *
          *  ACRCloud Worker main function.
          * 
          *    Every (mRecognizeInterval) seconds recognize ACRCloud Server by audio buffer. 
          *  If has result and callback OnResult.
          *
          **/
		private void Run()
		{
			if (this.mRecorder == null || this.mListener == null) {
				return;
			}

			this.mIsRunning = true;

			string result = "";
			int retryNum = this.mHttpErrorRetryNum;
			int nextRecognizeTime = 3; // seconds
			int preRecorderAudioLen = 0;
			int recordRetryNum = 10;
			while (this.mIsRunning) {
				Thread.Sleep(100);

				byte[] pcmData = null;
				int nowRecordAudioLen = this.mRecorder.GetAudioDataLen ();
				if ((!this.mIsStopRecord) && nowRecordAudioLen == preRecorderAudioLen) { // check microphone is OK
					recordRetryNum--;
					if (recordRetryNum <= 0) {
						if (result == "") { 
							result = ACRCloudErrorInfo.getJsonInfo (ACRCloudErrorInfo.RECORD_ERROR, "record error");
						}
						if (result == null) { // mute audio and create fingerprint null
							result = ACRCloudErrorInfo.getJsonInfo (ACRCloudErrorInfo.NO_RESULT, "No Result");
						}
						break;
					}
					continue;
				}
				preRecorderAudioLen = nowRecordAudioLen;
				recordRetryNum = 10;

				if (this.mIsStopRecord || this.mRecorder.GetAudioDataLen () >= nextRecognizeTime * 2 * 8000) {
					pcmData = this.mRecorder.GetAudioData ();
					if (pcmData == null) {
						result = ACRCloudErrorInfo.getJsonInfo (ACRCloudErrorInfo.RECORD_ERROR, "record error");
						break;
					}
					result = this.DoRecognize (pcmData);
					Debug.LogError (result);

					if (result == "") {
						retryNum--;

						result = ACRCloudErrorInfo.getJsonInfo (ACRCloudErrorInfo.HTTP_ERROR, "http error");
						if (retryNum <= 0) {
							break;
						}
						continue;
					}
					retryNum = this.mHttpErrorRetryNum;

					if (result != null) {
						try {
							JsonData rTmp = JsonMapper.ToObject (result);
							if ((int)rTmp ["status"] ["code"] == 1001) { // no result
								if (nextRecognizeTime >= this.mMaxRecognizeAudioTime) {
									break;
								}
							} else {
								break;
							}
						} catch (Exception e) {
							result = ACRCloudErrorInfo.getJsonInfo (ACRCloudErrorInfo.JSON_ERROR, "json error (" + result +")");
							break;
						}
					}
						
					if (this.mIsStopRecord) {
						if (result == null) {
							result = ACRCloudErrorInfo.getJsonInfo (ACRCloudErrorInfo.NO_RESULT, "No Result");
						}
						break;
					}
					nextRecognizeTime = pcmData.Length/(2*8000) + this.mRecognizeInterval;
					if (nextRecognizeTime > this.mMaxRecognizeAudioTime) {
						nextRecognizeTime = this.mMaxRecognizeAudioTime;
					}
				}
			}
			if (this.mIsRunning) {
				this.mListener.OnResult (result);
			}
			this.mIsRunning = false;
		}

		/**
          *
          *  Recognize ACRCloud Server by audio buffer.
          * 
          **/
		private string DoRecognize(byte[] pcmBuffer) 
		{
			int pcmBufferLen = pcmBuffer.Length;

			return this.mRecognizer.Recognize (pcmBuffer, pcmBufferLen);
		}
	}

	class ACRCloudErrorInfo {
		public static int NO_RESULT = 1001;
		public static int JSON_ERROR = 2002;
		public static int HTTP_ERROR = 3000;
		public static int GEN_FP_ERROR = 2004;
		public static int RECORD_ERROR = 2000;

		public static string getJsonInfo(int errorCode, string errorMsg) {
			JsonData root = new JsonData ();
			JsonData status = new JsonData ();
			root ["status"] = status;
			status ["msg"] = errorMsg;
			status ["code"] = errorCode;
			status["version"] = "1.0";
			return root.ToJson ();
		}
	}
}
