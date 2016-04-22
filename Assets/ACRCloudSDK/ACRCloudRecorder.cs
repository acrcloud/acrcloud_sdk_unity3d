using UnityEngine;
using System.Collections;

using System;  
using System.IO; 
using System.Collections.Generic;   

namespace ACRCloud {
	
	public class ACRCloudRecorder {
		
		private AudioSource mRecorderAudioSource; 
		private string mMicrophoneName = null;
		private int mMaxRecordTime = 15;
		private int mRecordSample = 8000;

		private byte[] mAudioBuffer = null;
		private int mAudioBufferLen = 0;

		private float mVolume = 0;
		private int mVolumeInterval = 50; // ms

		private bool mIsRunning = false;

		private static readonly ACRCloudRecorder mInstance = new ACRCloudRecorder();

		private ACRCloudRecorder()
		{
		}

		public static ACRCloudRecorder getInstance()
		{
			return mInstance;
		}

		/**
          *
          *  If Microphone is ok, return current volume.
          *  
          *  @return int volume
          *
          **/
		public float getVolume()
		{
			return this.mVolume;
		}

		/**
          *
          *  init acrcloud microphone by global GameObject
          *
          *  @param GameObject gObj : global GameObject gameObject
          *  
          *  @return bool
          *
          **/
		public bool init(GameObject gObj)
		{
			if (Microphone.devices == null || Microphone.devices.Length < 1) {
				Debug.LogError ("Microphone init error");
				return false;
			}
			this.mMicrophoneName = Microphone.devices [0];
			this.mRecorderAudioSource = gObj.AddComponent<AudioSource>();
			if (this.mRecorderAudioSource == null) {
				return false;
			}
			this.mAudioBuffer = new byte[this.mMaxRecordTime * 3 * mRecordSample];
			return true;
		}

		/**
          *
          *  get the length of current audio
          *
          *  @return int
          *
          **/
		public int GetAudioDataLen()
		{
			return this.mAudioBufferLen;
		}

		/**
          *
          *  get current audio data
          * 
          *  @return byte[], if microphone is not ok, return null.
          *
          **/
		public byte[] GetAudioData()
		{
			if (this.mAudioBufferLen <= 0)
				return null;
			byte[] tmp = new byte[this.mAudioBufferLen];
			Array.Copy (this.mAudioBuffer, 0, tmp, 0, tmp.Length);
			return tmp;
		}

		/**
          *
          *  You must call this function in a component,
          *     1. every (this.mVolumeInterval) seconds calculate current volume.
          *     2. get float[] buffer from microphone clip to this.mAudioBuffer and tranform Float[] to Short[].
          * 
          **/
		public void Update()
		{
			if (this.mIsRunning && this.mRecorderAudioSource != null && Microphone.IsRecording (this.mMicrophoneName)) {
				int currentMicPosition = Microphone.GetPosition (this.mMicrophoneName);
				int nowBufferSamples = this.mAudioBufferLen / 2;
				int nextReadSamples = currentMicPosition - nowBufferSamples;
				if (nextReadSamples > this.mRecordSample/1000*this.mVolumeInterval) {
					float[] samples = new float[nextReadSamples];  
					this.mRecorderAudioSource.clip.GetData(samples, nowBufferSamples);

					float tmpVol = 0;
					for (int i = 0; i < nextReadSamples; i++) {
						tmpVol += Math.Abs(samples [i]);
					}
					this.mVolume = tmpVol / samples.Length;

					byte[] tmpBuffer = FloatToShort (samples);
					if (tmpBuffer != null)
						Array.Copy (tmpBuffer, 0, this.mAudioBuffer, this.mAudioBufferLen, nextReadSamples*2);
					this.mAudioBufferLen += nextReadSamples*2;
				}
			}
		}

		/**
          *
          *  start microphone and record
          * 
          **/
		public bool StartRecord()
		{
			StopRecord ();
			this.mAudioBufferLen = 0;
			this.mIsRunning = true;
			this.mRecorderAudioSource.clip = Microphone.Start (this.mMicrophoneName, false, this.mMaxRecordTime, this.mRecordSample);
			return Microphone.IsRecording (this.mMicrophoneName);
		}

		/**
          *
          *  stop microphone
          * 
          **/
		public void StopRecord()
		{
			if (Microphone.IsRecording (this.mMicrophoneName)) {
				Microphone.End (this.mMicrophoneName);
			}
			this.mIsRunning = false;
		}

		private byte[] FloatToShort(float[] pcmFloatBuffer)  
		{  
			if (pcmFloatBuffer == null || pcmFloatBuffer.Length == 0) {
				return null;
			}
			byte[] outData = new byte[2 * pcmFloatBuffer.Length];
			int rescaleFactor = 32767;
			for (int i = 0; i < pcmFloatBuffer.Length; i++)  
			{  
				short temshort = (short)(pcmFloatBuffer[i] * rescaleFactor);  
				byte[] temdata = System.BitConverter.GetBytes(temshort);  
				outData[i*2]=temdata[0];  
				outData[i*2+1]=temdata[1];  
			}    
			return outData;  
		}  
	}
}
