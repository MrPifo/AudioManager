using UnityEditor;
using UnityEngine;

namespace Sperlich.Audio.Editor {
	[CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            AudioManager manager = (AudioManager)target;

            if (GUILayout.Button("Regenerate Library")) {
                LibraryGenerator.GenerateLibrary();
            }
        }
    }
}