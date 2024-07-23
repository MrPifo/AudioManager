using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Sperlich.PrefabManager;
#if UNITY_EDITOR
using UnityEditor;

namespace Sperlich.Audio {
    public partial class AudioManager : SingleMonoPrefab<AudioManager> {

        public static string AudioFolder = "Audio";

        public static void GenerateLibrary() {
            string folderPath = Path.Combine(Application.dataPath.Replace("/", "\\"), AudioFolder);
            string audioFileTypePath = Path.Combine(folderPath, "Sounds.cs");
            string resourcesPath = Path.Combine(folderPath, "Resources");
            string libraryPath = Path.Combine("Assets/" + AudioFolder, "Resources", "AudioLibrary.asset");
            string soundObjectsPath = Path.Combine("Assets/" + AudioFolder, "Resources", "Sounds");

            if (File.Exists(audioFileTypePath)) {
                File.Delete(audioFileTypePath);
            }
            if (Directory.Exists(resourcesPath) == false) {
                Directory.CreateDirectory(resourcesPath);
            }
            if (Directory.Exists(soundObjectsPath) == false) {
                Directory.CreateDirectory(soundObjectsPath);
            }

            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).Where(f => {
                string ext = Path.GetExtension(f);
                if (ext == ".ogg" || ext == ".wav" || ext == ".mp3") {
                    return true;
                }
                return false;
            }).OrderBy(f => AudioManager.GetFileName(f)).ToArray();

            #region Generate SoundObjects
            List<SoundObject> soundObjects = Resources.LoadAll<SoundObject>(soundObjectsPath).ToList();

            for (int i = 0; i < files.Length; i++) {
                string fileName = AudioManager.GetFileName(files[i]);
                string relPath = MakeRelative(files[i], "Assets");
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relPath);

                // Check if there is already a SoundObject with the clip
                if (soundObjects.Any(s => s.clip == clip) == false) {
                    SoundObject sObject = ScriptableObject.CreateInstance<SoundObject>();
                    sObject.clip = clip;
                    sObject.name = fileName;

                    // Fill in any ID that has not been assigned yet
                    if (soundObjects.Count > 0) {
                        for (int n = 0; n <= soundObjects.Max(s => s.id) + 1; n++) {
                            if (soundObjects.Any(s => s.id == n) == false) {
                                sObject.id = n;
                                break;
                            }
                        }
                    } else {
                        sObject.id = 0;
                    }
                    soundObjects.Add(sObject);
                    AssetDatabase.CreateAsset(sObject, Path.Combine(soundObjectsPath, $"{fileName}.asset"));
                }
            }
            #endregion

            #region Enum-Generation
            // Generate Enum-Sound File
            string content = "public enum Sounds {\r\n";

            AudioLibrary library = ScriptableObject.CreateInstance<AudioLibrary>();
            foreach (SoundObject s in soundObjects) {
                content += $"\t{s.name} = {s.id},\r\n";
            }

            content += "}";

            using (FileStream fs = File.Create(audioFileTypePath)) {
                char[] value = content.ToCharArray();
                fs.Write(System.Text.Encoding.UTF8.GetBytes(value), 0, value.Length);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            #endregion

            #region Generate Library
            foreach (SoundObject s in soundObjects.OrderBy(s => s.name)) {
                library.files.Add(new AudioLibrary.AudioFile(s, (Sounds)s.id));
            }
            AssetDatabase.CreateAsset(library, libraryPath);
            AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log($"Audio Library recompiled. \n Registered Sounds {System.Enum.GetValues(typeof(Sounds)).Length}");
            #endregion
        }
        public static string MakeRelative(string filePath, string referencePath) {
            int index = filePath.IndexOf(referencePath);
            return filePath.Substring(index, filePath.Length - index);
        }


		[MenuItem("Tools/Recompile AudioManager")]
        public static void RecompileLibrary() {
            GenerateLibrary();
        }
	}
}
#endif