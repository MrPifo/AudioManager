using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.Audio {
	public class SoundPlayer : MonoBehaviour {

		public AudioManager.AudioPreset preset;
		public bool isPlaying;
		public bool isPaused;
		public bool isLooping;
		public AudioClip Clip => _source.clip;
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
		public float Volume { get => _source.volume; set => _source.volume = value; }
		public float Spatial { get => _source.spatialBlend; set => _source.spatialBlend = value; }
		public UnityEvent OnPlayComplete { get; private set; } = new UnityEvent();

		public void Play(float volume, bool loop) {
			Source.volume = Mathf.Clamp01(volume);
			Initialize(_source.clip, _source.pitch, volume, transform.position, _source.spatialBlend, _source.minDistance, _source.maxDistance, _source.spread, loop);
		}
		public void Initialize(AudioClip clip, float pitch, float volume, Vector3 pos, float spatial, float minDistance, float maxDistance, float spread, bool loop) {
			bindParent = null;
			name = $"{clip.name}_Playing";
			Source.playOnAwake = false;
			isLooping = loop;
			transform.position = pos;
			isPlaying = true;
			_source.pitch = pitch;
			_source.clip = clip;
			_source.loop = loop;
			_source.spatialBlend = spatial;
			_source.minDistance = minDistance;
			_source.maxDistance = maxDistance;
			_source.spread = spread;
			_source.volume = volume;
			_source.Play();

			if (loop == false) {
				StartCoroutine(IDelay());
				IEnumerator IDelay() {
					float time = 0;
					while (time < clip.length) {
						yield return null;
						if (isPaused == false) {
							time += Time.deltaTime;
						}
					}
					OnPlayComplete.Invoke();
					OnPlayComplete.RemoveAllListeners();
					isPlaying = false;

					name = $"{clip.name}_Finished";
				}
			}
		}
		public void Stop() {
			Source.Stop();
			OnPlayComplete.RemoveAllListeners();

			name = $"{Clip.name}_Stopped";
			bindParent = null;
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