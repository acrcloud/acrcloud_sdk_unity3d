using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using System.Runtime.InteropServices;

using System.Threading;

using LitJson;

using ACRCloud;


public class ACRCloudSDKDemo : MonoBehaviour, IACRCloudWorkerListener {

	private string mRecResult = "";
	private ACRCloudRecorder mRecorder = null;
	private ACRCloudWorker mWorker = null;

	private bool mIsProcessing = false;

	// Use this for initialization
	void Start () {  
		this.mRecorder = ACRCloudRecorder.getInstance ();
		if (!this.mRecorder.init (gameObject)) {
			this.mRecResult = "Microphone init error";
			this.mRecorder = null;
		}
		//yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
	}

	// Update is called once per frame
	void Update () {
		if (mIsProcessing && mRecorder != null)
			mRecorder.Update (); // update recorder buffer
	}

	void OnGUI() 
	{      
		//if(Application.HasUserAuthorization(UserAuthorization.Microphone))
		DrawAllView();

	}

	private void DrawAllView()
	{
		GUIStyle tmpBtnStyle = new GUIStyle(GUI.skin.button);
		tmpBtnStyle.fontSize = 20;

		GUIStyle tmpLabelStyle = new GUIStyle(GUI.skin.label);
		tmpLabelStyle.fontSize = 20;

		if (GUI.Button (new Rect (0, 0, Screen.width, 70), "Start Record and recognize", tmpBtnStyle)) {
			if (!this.mIsProcessing && this.mRecorder != null) {
				this.mRecResult = "";
				if (!this.mRecorder.StartRecord ()) {
					this.mRecResult = "start Microphone record error";
				}
				this.mWorker = new ACRCloudWorker (this, this.mRecorder);
				this.mWorker.Start ();
				this.mIsProcessing = true;
			}
		}
		// Note: If you do not click this button(Stop Record and recognize), 
		//       ACRCloudWorker can callback OnResult when it has result.
		if (GUI.Button (new Rect (0, 80, Screen.width, 70), "Stop Record to recognize", tmpBtnStyle)) {
			if (this.mIsProcessing && this.mRecorder != null) {
				this.mRecorder.StopRecord ();
				this.mWorker.StopRecordToRecognize ();
			}
		}
		if (GUI.Button (new Rect (0, 160, Screen.width, 70), "Cancel", tmpBtnStyle)) {
			if (this.mIsProcessing && this.mRecorder != null && this.mWorker != null) {
				this.mIsProcessing = false;
				this.mRecorder.StopRecord ();
				this.mWorker.Cancel ();
			}
		}
		if (this.mIsProcessing && this.mRecorder != null) {
			GUI.Label (new Rect(0, 240, Screen.width, 50), "volume: " + this.mRecorder.getVolume(), tmpLabelStyle);
		}

		if (this.mRecResult != "") {
			this.mRecorder.StopRecord ();
			this.mIsProcessing = false;
			GUI.Label (new Rect(0, 400, Screen.width, 600), this.mRecResult, tmpLabelStyle);
		}
	}
		
	public void OnResult(string result) {
		this.mRecResult = result;
	}
}
