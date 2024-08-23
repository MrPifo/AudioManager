using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.Audio {
	public class SoundPlayer : MonoBehaviour {

		public PlayerPreset preset;
		public bool isPlaying;
		public bool isPaused;
		public bool isLooping;
		public AudioClip Clip => Options.Clip;
		public AudioSource Source {
			get {
				if(_source == null) {
					_source = GetComponent<AudioSource>();
					_source.rolloffMode = AudioRolloffMode.Linear;
				}
				return _source;
			}
			set => _source = value;
		}
		private AudioSource _source;
		private AudioReverbFilter _reverbFilter;
		private Transform bindParent;
		public AudioReverbFilter ReverbFilter {
			get {
				if(_reverbFilter == null) {
					_reverbFilter = GetComponent<AudioReverbFilter>();
				}
				return _reverbFilter;
			}
			set => _reverbFilter = value;
		}
		public float Pitch { get => _source.pitch; set => _source.pitch = value; }
		public float Volume { get => _source.volume; }
		public float Spatial { get => _source.spatialBlend; set => _source.spatialBlend = value; }
		public UnityEvent OnPlayComplete { get; private set; } = new UnityEvent();
		public PlayOptions Options { get; set; }

		internal bool _isFree;
		/// <summary>
		/// Set this to true to avoid that this Player is automaticially freed.
		/// </summary>
		public bool Reserve { get; set; }
		public bool IsFree => _isFree && Reserve == false && Source.isPlaying == false;

		public void Play(float volume, bool loop) {
			Source.volume = Mathf.Clamp01(volume);
			Options.Loop = loop;
			Initialize(Options);
		}
		public void Initialize(PlayOptions options) {
			_isFree = false;
			Options = options;
			bindParent = null;
			name = $"{Clip.name}_Playing";
			Source.playOnAwake = false;
			isLooping = Options.Loop;
			isPlaying = true;
			Options.Apply(_source);
			_source.Play();
			SetVolume(options.Volume);

			if (Options.Loop == false) {
				StartCoroutine(IDelay());
				IEnumerator IDelay() {
					float time = 0;
					while (time < Clip.length) {
						yield return null;
						if (isPaused == false) {
							time += Time.deltaTime;
						}
					}
					OnPlayComplete.Invoke();
					OnPlayComplete.RemoveAllListeners();
					isPlaying = false;

					name = $"{Clip.name}_Finished";
					Free();
				}
			}
		}
		public void Stop() {
			Source.Stop();
			OnPlayComplete.RemoveAllListeners();

			name = $"{Clip.name}_Stopped";
			bindParent = null;
			_isFree = true;
		}
		public void Stop(float fadeTime) {
			StartCoroutine(Fade());
			IEnumerator Fade() {
				float time = fadeTime;
				float startVolume = Source.volume;
				while (time > 0 && _source.isPlaying) {
					_source.volume = time.Remap(0f, fadeTime, 0f, startVolume);
					yield return null;
					time -= Time.deltaTime;
				}
				_source.Stop();
				bindParent = null;
				name = $"{Clip.name}_Stopped";
				_isFree = true;
			}
        }
		public void Pause() {
			Source.Pause();

			name = $"{Clip.name}_Paused";
		}
		public void Resume() {
			Source.UnPause();

			name = $"{Clip.name}_Playing";
		}
		public void SetClip(AudioClip clip) {
			_source.clip = clip;
		}
		public void SetPos(Vector3 pos) {
			transform.position = pos;
		}
		public void BindPos(Transform parent) {
			bindParent = parent;
			StartCoroutine(ICopyPos());

			IEnumerator ICopyPos() {
				while(bindParent != null) {
					transform.position = parent.position;
					yield return null;
				}
			}
		}
		public void Free() {
			_isFree = true;
		}
		public void SetVolume(float volume) {
			Options.Volume = Mathf.Clamp01(volume);
			float targetVolume;

			if(Options.IsGlobalVolumeType) {
				targetVolume = volume * AudioManager.GlobalVolume;
			} else {
				targetVolume = AudioManager.AudioVolumes[Options.Type] * volume * AudioManager.GlobalVolume;
			}

			Options.Volume = volume;
			_source.volume = targetVolume;
		}

		public SoundPlayer FadeIn(float fadeTime) {
			StartCoroutine(Fade());
			IEnumerator Fade() {
				float time = 0;
				float targetVolume = Source.volume;
				while (time < fadeTime && _source.isPlaying) {
					_source.volume = time.Remap(0f, fadeTime, 0f, targetVolume);
					yield return null;
					time += Time.deltaTime;
				}
			}
			return this;
        }
	}
}