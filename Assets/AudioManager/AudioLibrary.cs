using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Audio {
	[System.Serializable]
	[CreateAssetMenu(fileName = "Asset", menuName = "Audio/Library", order = 1)]
	public class AudioLibrary : ScriptableObject {

		public List<AudioFile> files = new List<AudioFile>();

		public AudioFile GetFile(Sounds name) {
			return files.Find(s => s.sound == name);
		}
		public AudioClip GetClip(Sounds name) {
			return files.Find(s => s.sound == name).Clip;
		}

		[System.Serializable]
		public class AudioFile {
			public SoundObject sObject;
			public Sounds sound;

			public AudioFile(SoundObject sObject, Sounds sound) {
				this.sObject = sObject;
				this.sound = sound;
			}

			public AudioClip Clip => sObject.clip;
		}
	}
}