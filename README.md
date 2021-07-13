# Audio Recognition Uniti3D SDK

## Overview
  [ACRCloud](https://www.acrcloud.com/) provides services such as **[Music Recognition](https://www.acrcloud.com/music-recognition)**, **[Broadcast Monitoring](https://www.acrcloud.com/broadcast-monitoring/)**, **[Custom Audio Recognition](https://www.acrcloud.com/second-screen-synchronization%e2%80%8b/)**, **[Copyright Compliance & Data Deduplication](https://www.acrcloud.com/copyright-compliance-data-deduplication/)**, **[Live Channel Detection](https://www.acrcloud.com/live-channel-detection/)**, and **[Offline Recognition](https://www.acrcloud.com/offline-recognition/)** etc.<br>
  
  This **audio recognition Unity3D SDK** enable apps recognize audio by recording sound through microphone. 

## Requirements
Follow one of the tutorials to create a project and get your host, access_key and access_secret.

 * [Recognize Music](https://docs.acrcloud.com/tutorials/recognize-music)
 * [Recognize Custom Content](https://docs.acrcloud.com/tutorials/recognize-custom-content)
 * [Broadcast Monitoring for Music](https://docs.acrcloud.com/tutorials/broadcast-monitoring-for-music)
 * [Broadcast Monitoring for Custom Content](https://docs.acrcloud.com/tutorials/broadcast-monitoring-for-custom-content)
 * [Detect Live & Timeshift TV Channels](https://docs.acrcloud.com/tutorials/detect-live-and-timeshift-tv-channels)
 * [Recognize Custom Content Offline](https://docs.acrcloud.com/tutorials/recognize-custom-content-offline)
 * [Recognize Live Channels and Custom Content](https://docs.acrcloud.com/tutorials/recognize-tv-channels-and-custom-content)


## Others
**Recognizer**: It is the code implementation of ACRCloud WebApi and create fingerprint functions，https://docs.acrcloud.com/docs/acrcloud/audio-fingerprinting-api/audio-identification-api/protocol-1/
**Recorder**: Used to record audio from a microphone.
**Worker**: is a thread, used to asynchronously obtain recordings for identification. Default, one session can send up to 4 WebApi HTTP requests at 3s, 6s, 9s, and 12s respectively, the time interval is 3s(RecognizeInterval).
Once the correct results are available at these time points, the SDK will stop recognizing and return the results.
**Some Parameters**:
mMaxRecordTime: Maximum recording time.
mRecordSample: The format of the recorded audio, the sampling rate is 8000Hz
RecognizeInterval: One session can send up to 4 WebApi HTTP requests at 3s, 6s, 9s, and 12s respectively, the time interval is 3s(RecognizeInterval).
mMaxRecognizeAudioTime: The maximum audio supported by webapi is 12s
mHttpErrorRetryNum: The HTTP request fails due to network and other reasons, and will be retried 3 times (mHttpErrorRetryNum).

## Functions
Introduction all API.
### ACRCloudRecognizer.cs
```c
public class ACRCloudRecognizer {
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
     public string Recognize(byte[] pcmAudioBuffer, int pcmAudioBufferLen);
}

public class ACRCloudFingerprintTool {
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
    public static byte[] CreateFingerprint(byte[] pcmBuffer, int pcmBufferLen);
}
```
### ACRCloudRecorder.cs
```c
public class ACRCloudRecorder {
    public static ACRCloudRecorder getInstance();

    /**
      *
      *  start microphone and record
      * 
      **/
    public bool StartRecord();

    /**
      *
      *  stop microphone
      * 
      **/
    public void StopRecord();
    
    /**
      *
      *  If Microphone is ok, return current volume.
      *  
      *  @return int volume
      *
      **/
    public float getVolume();

    /**
      *
      *  init acrcloud microphone by global GameObject
      *
      *  @param GameObject gObj : global GameObject gameObject
      *  
      *  @return bool
      *
      **/
    public bool init(GameObject gObj);

    /**
      *
      *  get the length of current audio
      *
      *  @return int
      *
      **/
    public int GetAudioDataLen();

    /**
      *
      *  get current audio data
      * 
      *  @return byte[], if microphone is not ok, return null.
      *
      **/
    public byte[] GetAudioData();

    /**
      *
      *  You must call this function in a component,
      *     1. every (this.mVolumeInterval) seconds calculate current volume.
      *     2. get float[] buffer from microphone clip to this.mAudioBuffer and tranform Float[] to Short[].
      * 
      **/
    public void Update();
}
```

### ACRCloudWorker.cs
```c
	public interface IACRCloudWorkerListener {
	    /**
              *
              *  callback function of ACRCloudWorker, you can implement in a component script.
              *
              **/
		void OnResult(string result);
	}

	public class ACRCloudWorker {
	    public ACRCloudWorker(IACRCloudWorkerListener lins, ACRCloudRecorder recorder, IDictionary<string, object> config) {
	    	this.mListener = lins;
	    	this.mRecorder = recorder;
	    	this.mRecognizer = new ACRCloudRecognizer(config);
	    }
	}

   	/**
          *
          *  Cancel this recognition Session.
          * 
          *  Note:  ACRCloudWorker do not callback OnResult.
          * 
          **/
	public void Cancel();

    	/**
          *
          *  Make ACRCloudWorker know Recorder is stopped.
          * 
          *  Note: If you do not click this button(Stop Record and recognize), 
          *        ACRCloudWorker can callback OnResult when it has result.
          * 
          **/
	public void StopRecordToRecognize();

	/**
          *
          *  Start a Thread to recognize.
          * 
          **/
	public void Start();
```

## Example
You must replace "XXXXXXXX" below with your project's host, access_key and access_secret in ACRCloudSDKDemo.cs, 
and add ACRCloudSDKDemo.cs as a component to Main Camera, test the package.
```c
   var recognizerConfig = new Dictionary<string, object>();
   // Replace "XXXXXXXX" below with your project's host, access_key and access_secret
   recognizerConfig.Add("host", "XXXXXXXX");
   recognizerConfig.Add("access_key", "XXXXXXXX");
   recognizerConfig.Add("access_secret", "XXXXXXXX"); 
```
