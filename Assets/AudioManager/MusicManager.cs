using BattleTanks;
using Sperlich.Audio;
using Sperlich.PrefabManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Audio {
	[SingletonPrefab(true, true)]
	public class MusicManager : SingleMonoPrefab<MusicManager> {

		public Sounds playingAmbient;
		public Sounds playingMusicTheme;
		public static SoundPlayer ActiveMusicPlayer;
		public static SoundPlayer AmbientMusicPlayer;

		public static void PlayAmbient(WorldTheme world) {
			if(AmbientMusicPlayer != null && AmbientMusicPlayer.isPlaying) {
				AmbientMusicPlayer.Stop();
			}

			switch (world) {
				case WorldTheme.Woody:
					AmbientMusicPlayer = AudioManager.PlayAmbient(Sounds.WoodyAmbient, true);
					break;
				case WorldTheme.Fir:
					AmbientMusicPlayer = AudioManager.PlayAmbient(Sounds.WoodyAmbient, true);
					break;
				case WorldTheme.Snowy:
					AmbientMusicPlayer = AudioManager.PlayAmbient(Sounds.WinterAmbient, true);
					break;
				case WorldTheme.Garden:
					AmbientMusicPlayer = AudioManager.PlayAmbient(Sounds.GardenAmbient, true);
					break;
				case WorldTheme.Rain:
					AmbientMusicPlayer = AudioManager.PlayAmbient(Sounds.AmbientRain, true);
					break;
			}
		}

		public static void PlayMusic(WorldTheme world) {
			if(ActiveMusicPlayer != null && ActiveMusicPlayer.isPlaying) {
				ActiveMusicPlayer.Stop();
			}

			switch (world) {
				case WorldTheme.Woody:
					ActiveMusicPlayer = AudioManager.PlayMusic(Sounds.WoodyTheme, 1f, true);
					break;
				case WorldTheme.Fir:
                    ActiveMusicPlayer = AudioManager.PlayMusic(Sounds.WoodyTheme, 1f, true);
                    break;
				case WorldTheme.Snowy:
                    ActiveMusicPlayer = AudioManager.PlayMusic(Sounds.WoodyTheme, 1f, true);
                    break;
				case WorldTheme.Garden:
                    ActiveMusicPlayer = AudioManager.PlayMusic(Sounds.WoodyTheme, 1f, true);
                    break;
			}
		}

		public static void StopMusic(float fadeTime = 0.5f) {
			if (ActiveMusicPlayer != null) {
				if (fadeTime > 0) {
					ActiveMusicPlayer.Stop(fadeTime);
				} else {
					ActiveMusicPlayer.Stop();
				}
			}
		}

		public static void StopAmbient(float fadeTime = 0.5f) {
			if (AmbientMusicPlayer != null) {
				if (fadeTime > 0) {
					AmbientMusicPlayer.Stop(fadeTime);
				} else {
					AmbientMusicPlayer.Stop();
				}
			}
		}

		public static void StopAll(float fadeTime = 0.5f) {
			StopMusic(fadeTime);
			StopAmbient(fadeTime);
		}
	}
}