using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class S_UI_Options : MonoBehaviour
{
	[Header("External")]
	public S_UI_IngameInterface _MainUI;
	public Animator _Animator;
	S_HedgeCamera Cam;

	[Header ("Assets")]
	public AudioMixer VoiceMixer;
	public AudioMixer MusicMixer;
	public AudioMixer SFXMixer;

	[Header ("Internal")]
	public GameObject _OptionsPanel;
	public Slider MusicSlider;
	public Slider VoiceSlider;
	public Slider SfxSlider;

	public Slider SensX;
	public Slider SensY;
	public UnityEngine.UI.Toggle FullScreen;

	[NonSerialized] public bool _isOptionsOpen;


	void Start () {
		Cam = _MainUI.Cam;
		InitialSetSliders();
		_OptionsPanel.SetActive(false);

	}

	public static float TrySetFloatByPlayerPref ( string pref, float defaultFloat ) {
		float tryFloat = PlayerPrefs.GetFloat("X_SENS");
		return tryFloat == 0 ? defaultFloat : tryFloat;
	}

	void InitialSetSliders () {
		MusicSlider.value = PlayerPrefs.GetFloat("MUSIC_VOL"); //MUSIC volume is not saved, always set to 1
		VoiceSlider.value = PlayerPrefs.GetFloat("VOICE_VOL"); //MUSIC volume is not saved, always set to 1
		SfxSlider.value = PlayerPrefs.GetFloat("SFX_VOL"); //MUSIC volume is not saved, always set to 1

		SensX.value = TrySetFloatByPlayerPref("X_SENS", 1);
		SensY.value = TrySetFloatByPlayerPref("Y_SENS", 1);
		Cam._sensiX = SensX.value;
		Cam._sensiY = SensY.value;

		FullScreen.isOn = PlayerPrefs.GetInt("FullScreen") == 1;
	}

	public void EventToggleOptionsMenu () {
		if (!_isOptionsOpen)
		{
			_isOptionsOpen = true;
			_Animator.SetTrigger("OptionsEnter");
			_Animator.SetBool("OptionsOpen", true);
			_OptionsPanel.SetActive(true);
		}
		else
		{
			_isOptionsOpen = false;
			_Animator.SetBool("OptionsOpen", false);
			_Animator.SetTrigger("OptionsExit");
			PlayerPrefs.Save();
		}
	}

	public void OnPauseOpen () {
		_isOptionsOpen = false;
		_Animator.SetBool("OptionsOpen", false);
		_OptionsPanel.SetActive(false);
	}

	public void AnEventOptionsClosed () {
		_OptionsPanel.SetActive(false);
	}


	public void EventSetMusicVolume ( float value ) {
		PlayerPrefs.SetFloat("MUSIC_VOL", value);
		MusicMixer.SetFloat("Volume", PlayerPrefs.GetFloat("MUSIC_VOL"));
	}

	public void EventSetSFXVolume ( float value ) {
		PlayerPrefs.SetFloat("SFX_VOL", value);
		SFXMixer.SetFloat("Volume", PlayerPrefs.GetFloat("SFX_VOL"));
	}

	public void EventSetVoiceVolume ( float value ) {
		PlayerPrefs.SetFloat("VOICE_VOL", value);
		VoiceMixer.SetFloat("Volume", PlayerPrefs.GetFloat("VOICE_VOL"));
	}

	public void EventSetVertiSensitivity ( float value ) {
		PlayerPrefs.SetFloat("Y_SENS", value);
		Cam._sensiY = value;
	}

	public void EventSetHorizSensitivity ( float value ) {
		PlayerPrefs.SetFloat("X_SENS", value);
		Cam._sensiX = value;
	}

	public void EventSetFullScreen ( bool value ) {
		PlayerPrefs.SetInt("FullScreen", value ? 1 : 0);
		Screen.fullScreen = value;
	}
}
