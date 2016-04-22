# Overview
  [ACRCloud](https://www.acrcloud.com/) provides cloud ACR services to help excellent companies and developers build Audio Fingerprinting based applications such as **Audio Recognition** (supports music, video, ads for both online and offline), **Broadcast Monitoring**, **Second Screen Interaction**, **Copyright Detection** and etc.<br>
  This Unity SDK can recognize ACRCloud by most of audio/video file. create "ACRCloud Fingerprint" by Audio/Video file, and use "ACRCloud Fingerprint" to recognize metainfos by "ACRCloud webapi".<br>
>>>>Audio: mp3, wav, m4a, flac, aac, amr, ape, ogg ...<br>
>>>>Video: mp4, mkv, wmv, flv, ts, avi ...

# ACRCloud
Docs: [https://docs.acrcloud.com/](https://docs.acrcloud.com/)<br>
Console: [https://console.acrcloud.com/](https://console.acrcloud.com/)

# Functions
Introduction all API.
## recognizer.py
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

# Example
You must replace "XXXXXXXX" below with your project's host, access_key and access_secret, and add ACRCloudSDKDemo as a component to Main Camera, test the package.
```c
    /**
      *
      *  Recognize ACRCloud Server by audio buffer.
      *  You must replace "XXXXXXXX" below with your project's host, access_key and access_secret
      * 
      **/
	private string DoRecognize(byte[] pcmb) 
	{
		byte[] pcmBuffer = (byte[])pcmb;
		int pcmBufferLen = pcmBuffer.Length;
		var config = new Dictionary<string, object>();

		// Replace "XXXXXXXX" below with your project's host, access_key and access_secret
		config.Add("host", "XXXXXXXX");
		config.Add("access_key", "XXXXXXXX");
		config.Add("access_secret", "XXXXXXXX");
		ACRCloudRecognizer re = new ACRCloudRecognizer(config);

		string res = re.Recognize (pcmBuffer, pcmBufferLen);
		return res;
	}
```
