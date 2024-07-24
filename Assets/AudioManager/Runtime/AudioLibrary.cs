using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Audio {
	[System.Serializable]
	[CreateAssetMenu(fileName = "Asset", menuName = "Audio/Library", order = 1)]
	public class AudioLibrary : ScriptableObject {

		[SerializeField]
		internal List<AudioFile> files = new List<AudioFile>();

		private Dictionary<Sounds, AudioFile> _fileDic;
		public Dictionary<Sounds, AudioFile> Files {
			get {
				if(_fileDic == null || _fileDic.Count == 0) {
					_fileDic = new();

					foreach (AudioFile file in files) {
						_fileDic[file.sound] = file;
					}
				}

				return _fileDic;
			}
		}

		public void _internalAddFile(AudioFile file) => files.Add(file);

		#region API
		public int GetId(Sounds name) => Files[name].sObject.uniqueId;
		public AudioFile GetFile(Sounds name) {
			return Files[name];
		}
		public AudioClip GetClip(Sounds name) {
			return Files[name].Clip;
		}
		#endregion

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