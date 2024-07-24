using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Audio {
    [System.Serializable]
    [CreateAssetMenu(fileName = "Asset", menuName = "Audio/Sound", order = 1)]
    public class SoundObject : ScriptableObject {

        public new string name;
		public int uniqueId;
		public AudioClip clip;

    }
}