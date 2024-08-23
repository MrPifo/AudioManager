using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sperlich.Audio {
	public partial class AudioManager : MonoBehaviour {

		[SerializeField]
		[Range(0f, 1f)]
		internal float globalVolume;

		[SerializeField]
		internal List<AudioTypeValue> volumes = new();

		public static bool IsInitialized { get; private set; }
		public static float GlobalVolume {
			get => _globalVolume;
			set {
				SetVolume(value);
			}
		}
		public static Dictionary<PlayerPreset, SoundPlayer> AudioPresets { get; private set; }
		public static Dictionary<VolumeType, float> AudioVolumes { get; private set; }
		public static UnityEvent<VolumeType, float, bool> OnVolumeChanged { get; private set; } = new();

		private static float _globalVolume;
		private static AudioLibrary _lib;
		public static AudioLibrary Library {
			get {
				if (_lib == null) {
					_lib = Resources.Load<AudioLibrary>("AudioManager/AudioLibrary");
				}
				return _lib;
			}
		}
		private static Transform presetContainer;
		private static Transform soundPlayersContainer;
		private static List<SoundPlayer> pooledPlayers;

		private static AudioManager _instance;
		public static AudioManager Instance {
			get {
				if(_instance == null) {
					Initialize();
				}

				return _instance;
			}
			set {
				_instance = value;
			}
		}

		public static void Initialize() {
			if (IsInitialized == false) {
				var prefab = Resources.Load<AudioManager>("AudioManager/AudioManager");
				bool instanceIsNew = false;

				if (prefab != null) {
					_instance = Instantiate(prefab.gameObject).GetComponent<AudioManager>();
					Resources.UnloadAsset(prefab);
				} else {
					instanceIsNew = true;
					_instance = new GameObject("AudioManager").AddComponent<AudioManager>();
				}

				AudioPresets = new();
				pooledPlayers = new();
				AudioVolumes = new();
				presetContainer = new GameObject("Presets").transform;
				soundPlayersContainer = new GameObject("SoundPlayers").transform;
				presetContainer.SetParent(_instance.transform);
				soundPlayersContainer.SetParent(_instance.transform);
				DontDestroyOnLoad(_instance.gameObject);

				foreach (var i in Enum.GetValues(typeof(VolumeType))) {
					VolumeType type = (VolumeType)i;
					if(_instance.volumes.Exists(v => v.type == type) == false) {
						_instance.volumes.Add(new AudioTypeValue() {
							volume = 0.5f,
							type = type
						});
					}

					AudioTypeValue value = _instance.volumes.Find(a => a.type == type);

					if (value == null) {
						AudioVolumes.Add(type, 0.5f);
					} else {
						AudioVolumes.Add(value.type, value.volume);
					}
				}
				
				foreach (var i in Enum.GetValues(typeof(PlayerPreset))) {
					PlayerPreset preset = (PlayerPreset)i;
					var loadedPreset = Resources.Load<SoundPlayer>($"AudioManager/Presets/${preset}");
					SoundPlayer player;

					if(loadedPreset != null) {
						var instance = Instantiate(loadedPreset.gameObject);

						if(instance.TryGetComponent(out player) == false) {
							player = player.gameObject.AddComponent<SoundPlayer>();
						}
					} else {
						player = new GameObject(preset.ToString()).AddComponent<SoundPlayer>();
					}
					
					if(player.TryGetComponent(out AudioSource src) == false) {
						src = player.gameObject.AddComponent<AudioSource>();
					}

					player.Source = src;
					player.preset = preset;
					player.transform.SetParent(presetContainer);
					AudioPresets.Add(preset, player);

					Resources.UnloadAsset(loadedPreset);
				}

				if (instanceIsNew) {
					GlobalVolume = 1f;

					foreach (VolumeType value in Enum.GetValues(typeof(VolumeType))) {
						SetVolume(value, 0.5f);
					}
				}

				IsInitialized = true;
				Debug.Log("AudioManager initialized.");
			}
		}
		public static void SetListenerPosition(Vector3 pos) {
			Instance.transform.position = pos;
		}

		#region Sound API
		public static void SetVolume(float volume, bool dontNotify = false) {
			SetVolume(default, volume, true, dontNotify);
		}
		public static void SetVolume(VolumeType type, float volume, bool dontNotifiy = false) {
			SetVolume(type, volume, false, dontNotifiy);
		}
		private static void SetVolume(VolumeType type, float volume, bool isGlobal, bool dontNotify = false) {
			if (isGlobal == false && Enum.IsDefined(typeof(VolumeType), type)) {
				_instance.volumes.Find(v => v.type == type).volume = volume;
				AudioVolumes[type] = Mathf.Clamp01(volume);
			} else {
				_instance.globalVolume = volume;
				_globalVolume = volume;
			}

			// Update all SoundPlayers here
			foreach (SoundPlayer player in pooledPlayers) {
				if (player.isPlaying) {
					if (isGlobal) {
						player.SetVolume(volume);
					} else {
						if (player.Options.Type == type) {
							player.SetVolume(volume);
						}
					}
				}
			}

			if (dontNotify == false) {
				OnVolumeChanged.Invoke(type, volume, isGlobal);
			}
		}

		public static SoundPlayer Play3DSound(Sounds sound, VolumeType type, float spatial, Vector3 pos, float volume, float pitch) => Play3DSound(sound, type, spatial, pos, volume, pitch, pitch);
		public static SoundPlayer Play3DSound(Sounds sound, VolumeType type, float spatial, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, 9999, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, VolumeType type, float spatial, float maxDistance, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, 0f, maxDistance, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, VolumeType type, float spatial, float minDistance, float maxDistance, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, minDistance, maxDistance, 0f, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, VolumeType type, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, PlayerPreset.Default, spatial, minDistance, maxDistance, spread, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, VolumeType type, PlayerPreset preset, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, preset, spatial, minDistance, maxDistance, spread, pos, volume, minPitch, maxPitch, false);
		public static SoundPlayer Play3DSound(Sounds sound, VolumeType type, PlayerPreset preset, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float minPitch, float maxPitch, bool loop) {
			return PlaySound(new PlayOptions() {
				Sound = sound,
				Type = type,
				Preset = preset,
				Volume = volume,
				MinPitch = minPitch,
				MaxPitch = maxPitch,
				Spatial = spatial,
				MinDistance = minDistance,
				MaxDistance = maxDistance,
				Spread = spread,
				Loop = loop,
				WorldPos = pos,
				Is3D = true
			});
		}
		public static SoundPlayer Play3DSound(PlayOptions options) {
			return PlaySound(options);
		}
		
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type) => PlaySound(sound, type, 1f);
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type, float volume) => PlaySound(sound, type, volume, 1f);
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type, float volume, float pitch) => PlaySound(sound, type, pitch, pitch, volume);
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type, float minPitch, float maxPitch, float volume) => PlaySound(sound, type, PlayerPreset.Default, minPitch, maxPitch, volume, false);
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type, float minPitch, float maxPitch, float volume, bool loop) => PlaySound(sound, type, PlayerPreset.Default, minPitch, maxPitch, volume, loop);
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type, PlayerPreset preset, float pitch, float volume) => PlaySound(sound, type, preset, pitch, pitch, volume, false);
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type, PlayerPreset preset, float pitch, float volume, bool loop) => PlaySound(sound, type, preset, pitch, pitch, volume, loop);
		public static SoundPlayer PlaySound(Sounds sound, VolumeType type, PlayerPreset preset, float minPitch, float maxPitch, float volume, bool loop) {
			return PlaySound(new PlayOptions() {
				Sound = sound,
				Type = type,
				Preset = preset,
				Volume = volume,
				MinPitch = minPitch,
				MaxPitch = maxPitch,
				MaxDistance = float.MaxValue,
				Loop = loop,
			});
		}
		public static SoundPlayer PlaySound(PlayOptions options) {
			if(IsInitialized == false) {
				Initialize();
			}

			SoundPlayer pooledItem = GetFreeSoundPlayer(options.Preset);
			pooledItem.Initialize(options);
			return pooledItem;
		}
		#endregion
		public static SoundPlayer GetFreeSoundPlayer(PlayerPreset preset = PlayerPreset.Default) {
			SoundPlayer pooledItem = pooledPlayers.Where(p => p.preset == preset && p.IsFree).FirstOrDefault();
			if (pooledItem == null) {
				pooledItem = Instantiate(AudioPresets[preset].gameObject, soundPlayersContainer).GetComponent<SoundPlayer>();
				pooledPlayers.Add(pooledItem);
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

		[Serializable]
		public class AudioTypeValue {

			public VolumeType type;
			[Range(0f, 1f)]
			public float volume;

		}
	}
}