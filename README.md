# Overview
  [ACRCloud](https://www.acrcloud.com/) provides cloud ACR services to help excellent companies and developers build Audio Fingerprinting based applications such as **Audio Recognition** (supports music, video, ads for both online and offline), **Broadcast Monitoring**, **Second Screen Interaction**, **Copyright Detection** and etc.<br>

# ACRCloud
Docs: [https://www.acrcloud.com/docs/](https://www.acrcloud.com/docs/)<br>
Console: [https://console.acrcloud.com/](https://console.acrcloud.com/)

# Functions
Introduction all API.
## ACRCloudRecognizer.cs
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
## ACRCloudRecorder.cs
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

## ACRCloudWorker.cs
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

# Example
You must replace "XXXXXXXX" below with your project's host, access_key and access_secret in ACRCloudSDKDemo.cs, 
and add ACRCloudSDKDemo.cs as a component to Main Camera, test the package.
```c
   var recognizerConfig = new Dictionary<string, object>();
   // Replace "XXXXXXXX" below with your project's host, access_key and access_secret
   recognizerConfig.Add("host", "XXXXXXXX");
   recognizerConfig.Add("access_key", "XXXXXXXX");
   recognizerConfig.Add("access_secret", "XXXXXXXX"); 
```
