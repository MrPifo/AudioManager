using Sperlich.Audio;
using UnityEditor;
using UnityEngine;

namespace Sperlich.Editor {
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            AudioManager manager = (AudioManager)target;

            if (GUILayout.Button("Regenerate Library")) {
                AudioManager.GenerateLibrary();
            }
        }
    }
}