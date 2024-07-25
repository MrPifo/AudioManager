using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using System.Collections;
using static UnityEditor.PlayerSettings;
using System.Threading;
using UnityEditor.Presets;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sperlich.Audio {
	public partial class AudioManager : MonoBehaviour {

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
		private AudioLibrary library;
		public AudioLibrary Library {
			get {
				if (library == null) {
					library = Resources.Load<AudioLibrary>("AudioManager/AudioLibrary");
				}
				return library;
			}
		}
		private bool initialized;
		private Transform presetContainer;
		private Transform soundPlayersContainer;
		private List<SoundPlayer> audioPresets;
		private List<SoundPlayer> pooledPlayers;

		private static AudioListener _audioListener;
		private static AudioManager _instance;
		public static AudioManager Instance {
			get {
				if(_instance == null) {
					_instance = GetInstance();
					_instance.Initialize();
				}

				return _instance;
			}
			set {
				_instance = value;
			}
		}

		public void Initialize() {
			if (initialized == false) {
				audioPresets = new List<SoundPlayer>();
				pooledPlayers = new List<SoundPlayer>();
				presetContainer = new GameObject("Presets").transform;
				soundPlayersContainer = new GameObject("SoundPlayers").transform;
				presetContainer.SetParent(transform);
				soundPlayersContainer.SetParent(transform);
				DontDestroyOnLoad(Instance.gameObject);
				CheckAudioListeners();

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

				StartCoroutine(ICheckForListeners());
				initialized = true;

				IEnumerator ICheckForListeners() {
					var waiter = new WaitForSeconds(1f);

					while(true) {
						CheckAudioListeners();
						yield return waiter;
					}
				}
			}
		}

		void FixedUpdate() {
			CheckAudioListeners();
		}
		public void CheckAudioListeners() {
			AudioListener[] listeners = FindObjectsOfType<AudioListener>();

			foreach (var al in listeners) {
				if (al != _audioListener) {
					Destroy(al);
				}
			}
		}
		static internal AudioManager GetInstance() {
			var prefab = Resources.Load<AudioManager>("AudioManager/AudioManager");
			AudioManager inst = null;

			if(prefab != null) {
				inst = Instantiate(prefab.gameObject).GetComponent<AudioManager>();
			}
			if (inst == null) {
				inst = new GameObject("AudioManager").AddComponent<AudioManager>();
				inst.globalVolume = 100;
				inst.ambientVolume = 100;
				inst.musicVolume = 100;
				inst.soundEffectVolume = 100;
				inst.uiVolume = 100;
			}
			if(inst.TryGetComponent(out _audioListener) == false) {
				_audioListener = inst.gameObject.AddComponent<AudioListener>();
			}

			return inst;
		}
		public static void SetListenerPosition(Vector3 pos) {
			Instance.transform.position = pos;
		}

		#region Sound API
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, Vector3 pos, float volume, float pitch) => Play3DSound(sound, type, spatial, pos, volume, pitch, pitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, 9999, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, float maxDistance, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, 0f, maxDistance, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, float minDistance, float maxDistance, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, spatial, minDistance, maxDistance, 0f, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, AudioPreset.Default, spatial, minDistance, maxDistance, spread, pos, volume, minPitch, maxPitch);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, AudioPreset preset, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float minPitch, float maxPitch) => Play3DSound(sound, type, preset, spatial, minDistance, maxDistance, spread, pos, volume, minPitch, maxPitch, false);
		public static SoundPlayer Play3DSound(Sounds sound, SoundType type, AudioPreset preset, float spatial, float minDistance, float maxDistance, float spread, Vector3 pos, float volume, float minPitch, float maxPitch, bool loop) {
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
		
		public static SoundPlayer PlaySound(Sounds sound, SoundType type) => PlaySound(sound, type, 1f);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float volume) => PlaySound(sound, type, volume, 1f);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float volume, float pitch) => PlaySound(sound, type, pitch, pitch, volume);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float minPitch, float maxPitch, float volume) => PlaySound(sound, type, AudioPreset.Default, minPitch, maxPitch, volume, false);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, float minPitch, float maxPitch, float volume, bool loop) => PlaySound(sound, type, AudioPreset.Default, minPitch, maxPitch, volume, loop);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, AudioPreset preset, float pitch, float volume) => PlaySound(sound, type, preset, pitch, pitch, volume, false);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, AudioPreset preset, float pitch, float volume, bool loop) => PlaySound(sound, type, preset, pitch, pitch, volume, loop);
		public static SoundPlayer PlaySound(Sounds sound, SoundType type, AudioPreset preset, float minPitch, float maxPitch, float volume, bool loop) {
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
			float finalVolume = options.Volume * Instance.globalVolume;
			switch (options.Type) {
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

			options.Volume = finalVolume;
			SoundPlayer pooledItem = GetFreeSoundPlayer(options.Preset);
			pooledItem.Initialize(options);
			return pooledItem;
		}
		#endregion
		public static SoundPlayer GetFreeSoundPlayer(AudioPreset preset = AudioPreset.Default) {
			SoundPlayer pooledItem = Instance.pooledPlayers.Where(p => p.preset == preset && p.IsFree).FirstOrDefault();
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