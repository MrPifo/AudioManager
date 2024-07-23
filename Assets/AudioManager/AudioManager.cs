using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sperlich.Audio {
	[SingletonPrefab(true, true)]
	public partial class AudioManager : SingleMonoPrefab<AudioManager> {

		public enum AudioPreset { Default, Filter, Ambient }

		[Range(0f, 1f)]
		public float globalVolume;
		[Range(0f, 1f)]
		public float musicVolume;
		[Range(0f, 1f)]
		public float ambientVolume;
		[Range(0f, 1f)]
		public float soundEffectVolume;
		[Range(0f, 1f)]
		public float uiVolume;
		public string audioFolder;
		public bool useOwnAudioListener;
		private AudioLibrary library;
		public AudioLibrary Library {
			get {
				if (library == null) {
					library = Resources.Load<AudioLibrary>("AudioLibrary");
				}
				return library;
			}
		}
		private bool initialized;
		private AudioListener audioListener;
		private Transform presetContainer;
		private Transform soundPlayersContainer;
		private SoundPlayer currentMusicPlayer;
		private SoundPlayer currentAmbientPlayer;
		private List<SoundPlayer> audioPresets;
		private List<SoundPlayer> pooledPlayers;

		protected override void Awake() {
			base.Awake();
			Initialize(useOwnAudioListener);
		}

		void FixedUpdate() {
			if(useOwnAudioListener) {
				CheckAudioListeners();
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void GameInit() {
			Game.OnGameAfterInitialization.AddListener(() => {
				Instance.Awake();
			});
		}
		public void Initialize(bool useOwnAudioListener) {
			if (initialized == false) {
				initialized = true;
				Instance.useOwnAudioListener = useOwnAudioListener;
				audioPresets = new List<SoundPlayer>();
				pooledPlayers = new List<SoundPlayer>();
				presetContainer = new GameObject("Presets").transform;
				soundPlayersContainer = new GameObject("SoundPlayers").transform;
				presetContainer.SetParent(transform);
				soundPlayersContainer.SetParent(transform);
				DontDestroyOnLoad(Instance.gameObject);

				if(useOwnAudioListener) {
					CheckAudioListeners();
                }

				foreach (var typ in System.Enum.GetValues(typeof(AudioPreset))) {
					AudioPreset preset = (AudioPreset)typ;
					SoundPlayer player = new GameObject(preset.ToString() + "_preset").AddComponent<SoundPlayer>();
					AudioSource source = player.gameObject.AddComponent<AudioSource>();
					player.transform.SetParent(presetContainer);
					player.preset = preset;

					switch (preset) {
						case AudioPreset.Default:
							break;
						case AudioPreset.Filter:
							player.gameObject.AddComponent<AudioReverbFilter>();
							break;
					}
					audioPresets.Add(player);
				}
			}
		}

		public void CheckAudioListeners() {
            AudioListener[] listeners = FindObjectsOfType<AudioListener>();
            if (listeners.Length > 0) {
                foreach (var al in listeners) {
                    if (al != audioListener) {
                        Destroy(al);
                    }
                }
            }

            if (audioListener != null) return;
            if (audioListener == null) {
                audioListener = new GameObject("AudioListener").AddComponent<AudioListener>();
                audioListener.transform.SetParent(transform);
            }
        }

		public static void SetListenerPosition(Vector3 pos) {
			Instance.transform.position = pos;
		}

		#region Sound API
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, Vector3 pos, float volume, float pitch) => Play3DSound(sound, type, spatial, pos, volume, pitch, pitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, 9999, pos, volume, maxPitch, minPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, float maxDistance, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, 0f, maxDistance, pos, volume, maxPitch, minPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, float minDistance, float maxDistance, Vector3 pos, float volume, float maxPitch, float minPitch) => Play3DSound(sound, type, spatial, minDistance, maxDistance, 0f, pos, volume, maxPitch, minPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float maxPitch, float minPitch) => Play3DSound(sound, type, AudioPreset.Default, spatial, minDistance, maxDistance, spread, pos, volume, maxPitch, minPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, AudioPreset preset, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float maxPitch, float minPitch) => PlaySound(sound, type, preset, minPitch, maxPitch, volume, pos, spatial, minDistance, maxDistance, spread, false);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, AudioPreset preset, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float maxPitch, float minPitch, bool loop) => PlaySound(sound, type, preset, minPitch, maxPitch, volume, pos, spatial, minDistance, maxDistance, spread, loop);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type) => PlaySound(sound, type, 1f);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float volume) => PlaySound(sound, type, volume, 1f);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float volume, float pitch) => PlaySound(sound, type, pitch, pitch, volume);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float minPitch, float maxPitch, float volume) => PlaySound(sound, type, AudioPreset.Default, minPitch, maxPitch, volume, Vector3.zero, 0, 0, 0, 0, false);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float minPitch, float maxPitch, float volume, bool loop) => PlaySound(sound, type, AudioPreset.Default, minPitch, maxPitch, volume, Vector3.zero, 0, 0, 0, 0, loop);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, AudioPreset preset, float pitch, float volume) => PlaySound(sound, type, preset, pitch, pitch, volume, Vector3.zero, 0, 0, 0, 0, false);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, AudioPreset preset, float pitch, float volume, bool loop) => PlaySound(sound, type, preset, pitch, pitch, volume, Vector3.zero, 0, 0, 0, 0, loop);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, AudioPreset preset, float minPitch, float maxPitch, float volume, Vector3 pos, float spatial, float minDistance, float maxDistance, float spread, bool loop) {
			float finalVolume = volume * Instance.globalVolume;
			switch (type) {
				case SoundType.Effect:
					finalVolume *= Instance.soundEffectVolume;
					break;
				case SoundType.Music:
					finalVolume *= Instance.musicVolume;
					break;
				case SoundType.UI:
					finalVolume *= Instance.uiVolume;
					break;
			}

			AudioClip clip = Instance.Library.GetClip(sound);
			SoundPlayer pooledItem = GetFreeSoundPlayer(preset);
			pooledItem.Initialize(clip, UnityEngine.Random.Range(minPitch, maxPitch), finalVolume, pos, spatial, minDistance, maxDistance, spread, loop);
			return pooledItem;
		}
		#endregion
		#region Music API
		public static SoundPlayer PlayMusic(Sounds sound, bool volume, bool loop = false) => PlayMusic(sound, 1f, loop);
		public static SoundPlayer PlayMusic(Sounds sound, float volume, bool loop = false) => PlayMusic(sound, AudioPreset.Default, volume, loop);
		public static SoundPlayer PlayMusic(Sounds sound, AudioPreset preset, float volume, bool loop = false) {
			if(Instance.currentMusicPlayer != null && Instance.currentMusicPlayer.Source.isPlaying) {
				Instance.currentMusicPlayer.Stop();
			}
			float finalVolume = volume * Instance.globalVolume * Instance.musicVolume;
			AudioClip clip = Instance.Library.GetClip(sound);
			SoundPlayer pooledItem = GetFreeSoundPlayer(preset);
			pooledItem.Initialize(clip, 1f, finalVolume, Vector3.zero, 0f, 0f, 0f, 0f, loop);
			pooledItem.Source.loop = true;
			Instance.currentMusicPlayer = pooledItem;

			return pooledItem;
		}
		#endregion
		#region Ambient API
		public static SoundPlayer PlayAmbient(Sounds sound, bool loop = false) => PlayAmbient(sound, 1f, loop);
		public static SoundPlayer PlayAmbient(Sounds sound, float volume, bool loop = false) => PlayAmbient(sound, AudioPreset.Default, volume, loop);
		public static SoundPlayer PlayAmbient(Sounds sound, AudioPreset preset, float volume, bool loop = false) {
			if (Instance.currentAmbientPlayer != null && Instance.currentAmbientPlayer.Source.isPlaying) {
				Instance.currentAmbientPlayer.Stop();
			}
			float finalVolume = volume * Instance.globalVolume * Instance.ambientVolume;
			AudioClip clip = Instance.Library.GetClip(sound);
			SoundPlayer pooledItem = GetFreeSoundPlayer(preset);
			pooledItem.Initialize(clip, 1f, finalVolume, Vector3.zero, 0f, 0f, 0f, 0f, loop);
			pooledItem.Source.loop = true;
			Instance.currentAmbientPlayer = pooledItem;

			return pooledItem;
		}
		#endregion
		public static SoundPlayer GetFreeSoundPlayer(AudioPreset preset = AudioPreset.Default) {
			SoundPlayer pooledItem = Instance.pooledPlayers.Where(p => p.preset == preset && p.isPlaying == false).FirstOrDefault();
			if (pooledItem == null) {
				pooledItem = Instantiate(Instance.audioPresets.Find(a => a.preset == preset).gameObject, Instance.soundPlayersContainer).GetComponent<SoundPlayer>();
				Instance.pooledPlayers.Add(pooledItem);
			}
			return pooledItem;
		}
		public static ulong GetUInt64Hash(System.Security.Cryptography.HashAlgorithm hasher, string text) {
			using (hasher) {
				var bytes = hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(text));
				Array.Resize(ref bytes, bytes.Length + bytes.Length % 8); //make multiple of 8 if hash is not, for exampel SHA1 creates 20 bytes. 
				return Enumerable.Range(0, bytes.Length / 8) // create a counter for de number of 8 bytes in the bytearray
					.Select(i => BitConverter.ToUInt64(bytes, i * 8)) // combine 8 bytes at a time into a integer
					.Aggregate((x, y) => x ^ y); //xor the bytes together so you end up with a ulong (64-bit int)
			}
		}
		public static string GetFileName(string filePath) => Path.GetFileNameWithoutExtension(filePath).Replace(" ", "_").Replace(")", "").Replace("(", "");
	}
}