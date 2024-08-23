using UnityEngine;

namespace Sperlich.Audio {
	public class PlayOptions {

		public Sounds Sound { get; set; }
		public VolumeType Type { get; set; } = default;
		public PlayerPreset Preset { get; set; } = default;
		public float Volume { get; set; } = 1f;
		public float MinPitch { get; set; } = 1f;
		public float MaxPitch { get; set; } = 1f;
		public bool Loop { get; set; } = false;
		/// <summary>
		/// Overrides Spatial if true.
		/// </summary>
		public bool Is3D { get; set; } = false;
		public bool IsGlobalVolumeType { get; set; }

		// 3D-Options
		public float MinDistance { get; set; }
		public float MaxDistance { get; set; } = float.MaxValue;
		public float Spread { get; set; } = 0f;

		private Vector3 _worldPos;
		private float _spatial;
		public Vector3 WorldPos {
			get => Is3D ? _worldPos : Vector3.zero;
			set => _worldPos = value;
		}
		public float Pitch {
			get {
				return Mathf.Clamp(MinPitch * Mathf.Exp(Random.Range(0f, 1f) * Mathf.Log(MaxPitch / MinPitch)), MinPitch, MaxPitch);
			}
		}
		public float Spatial {
			get => Is3D ? _spatial : 0f;
			set => _spatial = value;
		}

		public AudioClip Clip => AudioManager.Library.GetClip(Sound);

		public void Apply(AudioSource source) {
			source.pitch = Pitch;
			source.clip = Clip;
			source.loop = Loop;
			source.spatialBlend = Spatial;
			source.minDistance = MinDistance;
			source.maxDistance = MaxDistance;
			source.spread = Spread;
			source.volume = Volume;
			source.transform.position = Is3D ? WorldPos : Vector3.zero;
		}
	}
}