using Sperlich.Audio;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AudioTester : MonoBehaviour {

	public Slider globalVolumeSlider;
	public Slider musicVolumeSlider;
	public Slider effectVolumeSlider;
	public Slider uiVolumeSlider;
	public Button musicBtn;

	private bool isMusicOn;
	private SoundPlayer musicPlayer;

	void Awake() {
		globalVolumeSlider.onValueChanged.AddListener((float value) => {
			globalVolumeSlider.GetComponentInChildren<Text>().text = "Global Volume: " + Mathf.Round(value * 100f) / 100f;
			AudioManager.SetVolume(value);
		});
		musicVolumeSlider.onValueChanged.AddListener((float value) => {
			musicVolumeSlider.GetComponentInChildren<Text>().text = "Music Volume: " + Mathf.Round(value * 100f) / 100f;
			UpdateVolume("Music", value);
		});
		effectVolumeSlider.onValueChanged.AddListener((float value) => {
			effectVolumeSlider.GetComponentInChildren<Text>().text = "Effect Volume: " + Mathf.Round(value * 100f) / 100f;
			UpdateVolume("Effect", value);
		});
		uiVolumeSlider.onValueChanged.AddListener((float value) => {
			uiVolumeSlider.GetComponentInChildren<Text>().text = "UI Volume: " + Mathf.Round(value * 100f) / 100f;
			UpdateVolume("UI", value);
		});

		AudioManager.OnVolumeChanged.AddListener((VolumeType type, float volume, bool isGlobal) => {
			if (isGlobal == false) {
				switch (type) {
					case VolumeType.Effect:
						effectVolumeSlider.value = volume;
						break;
					case VolumeType.Music:
						musicVolumeSlider.value = volume;
						break;
					case VolumeType.UI:
						uiVolumeSlider.value = volume;
						break;
				}
			} else {
				globalVolumeSlider.value = volume;
			}
		});
		AudioManager.Initialize();
	}

	void UpdateVolume(string name, float value) {
		if (Enum.TryParse(name, true, out VolumeType type)) {
			AudioManager.SetVolume(type, value, true);
		}
	}

	public void ToggleMusic() {
		if(isMusicOn == false) {
			if(Enum.TryParse("Music", true, out VolumeType type)) {
				if(Enum.TryParse("Demo_Music_1", out Sounds sound)) {
					musicPlayer = AudioManager.PlaySound(sound, type, 1f, 1f);
				}
			}

			isMusicOn = true;
		} else {
			if (musicPlayer != null && isMusicOn) {
				musicPlayer.Stop(0.5f);
			}

			isMusicOn = false;
		}

		musicBtn.gameObject.GetComponentInChildren<Text>().text = isMusicOn ? "Music OFF" : "Music ON";
	}

	public void Play2DSound() {
		if(Enum.TryParse("Demo_2D_Sound", true, out Sounds sound)) {
			AudioManager.PlaySound(sound, default, 1f);
		}
	}
	public void Play3DSound() {
		if (Enum.TryParse("Demo_3D_Sound", true, out Sounds sound)) {
			Vector3 randPos = new Vector3(UnityEngine.Random.Range(-16, 16), 0, 0) + Camera.main.transform.position;
			AudioManager.Play3DSound(sound, default, 1f, randPos, 1f, 1f);
		}
	}
	public void PlayUISound() {
		if (Enum.TryParse("UI", true, out VolumeType type)) {
			if (Enum.TryParse("Demo_UI_Sound", true, out Sounds sound)) {
				AudioManager.PlaySound(sound, type, 1f, 0.7f, 1.5f);
			}
		}
	}
}
