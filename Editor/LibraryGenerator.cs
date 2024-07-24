using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Sperlich.Audio.Editor {
	public class LibraryGenerator {

		public const string EnumContextName = "Sounds";
		public static string AudioSrcFolder => Path.Combine(ProjectRootFolder, "Assets/Audio");
		public static string LibraryAssetPath => Path.Combine(ResourceFolderPath, "AudioLibrary.asset");
		public static string SoundObjectFolderPath => Path.Combine(ResourceFolderPath, "SoundObjects");
		public static string SoundsFilePath => Path.Combine(RuntimeFolder, "Sounds.cs");
		public static string ResourceFolderPath => Path.Combine(RuntimeFolder, "Resources");
		public static string RuntimeFolder => Path.Combine(PackageRootFolder, "Runtime");
		public static string PackageRootFolder => Path.GetFullPath("../", GetRelativeFolderPath<AudioManager>());
		public static string ProjectRootFolder => Path.GetFullPath("../", Application.dataPath);
		public static readonly string[] ValidExtensions = {
			".ogg",
			".wav",
			".mp3",
			".aif",
			".aiff",
			".aac",
			".flac",
			".m4a"
		};

		public static void GenerateLibrary() {
			CreateDirectoryIfNotExists(ResourceFolderPath);
			CreateDirectoryIfNotExists(AudioSrcFolder);
			CreateDirectoryIfNotExists(SoundObjectFolderPath, true);

			#region Generate SoundObjects
			var soundObjects = new List<SoundObject>();

			foreach (string file in GetSortedAudioFiles(AudioSrcFolder)) {
				string fileName = SanitizeEnumName(AudioManager.GetFileName(file), true);
				string relPath = MakeRelative(file);
				AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relPath);

				// Create new SoundObject
				SoundObject sObject = ScriptableObject.CreateInstance<SoundObject>();
				sObject.clip = clip;
				sObject.name = SanitizeEnumName(AudioManager.GetFileName(file), false);
				sObject.uniqueId = GenerateUniqueId(fileName, false); // Assign the next available ID

				// Add to list and track existing clips
				soundObjects.Add(sObject);

				// Save the asset
				string assetPath = Path.Combine(SoundObjectFolderPath, $"{fileName}.asset");
				assetPath = MakeRelative(assetPath);
				AssetDatabase.CreateAsset(sObject, assetPath);
			}
			#endregion

			#region Enum-Generation
			// Generate Enum-Sound File
			GenerateAndSaveEnumFile(SoundsFilePath, EnumContextName, soundObjects);
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			#endregion

			#region Generate Library
			AudioLibrary library = ScriptableObject.CreateInstance<AudioLibrary>();
			foreach (SoundObject s in soundObjects.OrderBy(s => s.name)) {
				library._internalAddFile(new AudioLibrary.AudioFile(s, (Sounds)s.uniqueId));
			}
			AssetDatabase.CreateAsset(library, MakeRelative(LibraryAssetPath));
			AssetDatabase.SaveAssets();

			UnityEngine.Debug.Log($"Audio Library recompiled. \n Registered Sounds {System.Enum.GetValues(typeof(Sounds)).Length}");
			#endregion
		}

		#region Generation
		public static string SanitizeEnumName(string fileName, bool toLower = true) {
			// Convert to snake_case
			fileName = ToSnakeCase(fileName, toLower);

			// Remove invalid characters
			fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_]", "");

			// Ensure it does not start with a digit
			if (char.IsDigit(fileName[0])) {
				fileName = "_" + fileName;
			}

			// Ensure the first character is uppercase
			fileName = CapitalizeFirstLetter(fileName);

			// Ensure the name is not a C# reserved keyword (optional)
			string[] reservedKeywords = {
				"abstract", "as", "base", "bool", "break", "byte", "case", "catch",
				"char", "checked", "class", "const", "continue", "decimal", "default",
				"delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
				"false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
				"implicit", "in", "int", "interface", "internal", "is", "lock", "long",
				"namespace", "new", "null", "object", "operator", "out", "override",
				"params", "private", "protected", "public", "readonly", "ref", "return",
				"sbyte", "sealed", "short", "sizeof", "static", "string", "struct",
				"switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
				"unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile",
				"while"
			};

			if (Array.Exists(reservedKeywords, keyword => keyword.Equals(fileName, StringComparison.OrdinalIgnoreCase))) {
				fileName = "_" + fileName;
			}

			return fileName;
		}
		private static string CapitalizeFirstLetter(string input) {
			if (string.IsNullOrEmpty(input))
				return input;

			// Capitalize the first letter and concatenate with the rest of the string
			return char.ToUpper(input[0]) + input.Substring(1);
		}
		private static string ToSnakeCase(string input, bool toLower) {
			// Replace spaces or hyphens with underscores and make all lowercase
			string result = Regex.Replace(input, @"[\s\-]", "_");

			if (toLower) {
				result = result.ToLower();
			}
			return result;
		}
		public static string[] GetSortedAudioFiles(string audioSrcFolder) {
			return Directory.GetFiles(audioSrcFolder, "*.*", SearchOption.AllDirectories)
				.Where(file => ValidExtensions.Contains(Path.GetExtension(file).ToLower()))
				.OrderBy(file => AudioManager.GetFileName(file))
				.ToArray();
		}
		static void GenerateAndSaveEnumFile(string filePath, string enumName, IList<SoundObject> soundObjects) {
			string content = GenerateEnumContent("Sperlich.Audio", enumName, soundObjects);

			DeleteFileIfExists(filePath);
			File.WriteAllText(filePath, content, Encoding.UTF8);
			AssetDatabase.Refresh();
		}
		static string GenerateEnumContent(string namespaceName, string enumName, IList<SoundObject> soundObjects) {
			var template = new StringBuilder();

			// Add namespace if provided
			if (!string.IsNullOrWhiteSpace(namespaceName)) {
				template.AppendLine($"namespace {namespaceName}");
				template.AppendLine("{");
			}

			// Add enum declaration
			template.AppendLine($"\tpublic enum {enumName}");
			template.AppendLine("\t{");

			foreach (var soundObject in soundObjects) {
				template.AppendLine($"\t\t{soundObject.name} = {soundObject.uniqueId},");
			}

			template.AppendLine("\t}");

			// Close namespace block if namespace was provided
			if (!string.IsNullOrWhiteSpace(namespaceName)) {
				template.AppendLine("}");
			}

			return template.ToString();
		}
		#endregion

		#region Helpers
		public static int GenerateUniqueId(string input, bool sanitize = false) {
			if(sanitize) {
				input = SanitizeEnumName(input);
			}

			using (var md5 = MD5.Create()) {
				// Compute the hash of the input string
				byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

				// Convert the first 4 bytes of the hash to an integer
				int id = BitConverter.ToInt32(hashBytes, 0);

				return id;
			}
		}
		public static string MakeRelative(string filePath) {
			return Path.GetRelativePath(ProjectRootFolder, filePath);
		}
		public static string GetRelativeFolderPath<T>() where T : UnityEngine.Object {
			string filePath = "";

			foreach(string guid in AssetDatabase.FindAssets($"{typeof(T).Name}")) {
				string absolutePath = AssetDatabase.GUIDToAssetPath(guid);
				string fileName = Path.GetFileName(absolutePath);
				
				if(fileName == $"{typeof(T).Name}.cs") {
					filePath = absolutePath;
				}
			}

			string dirPath = Path.Combine(ProjectRootFolder, Path.GetDirectoryName(filePath));
			return dirPath;
		}
		public static void DeleteFileIfExists(string filePath) {
			if (File.Exists(filePath)) {
				File.Delete(filePath);
			}
		}
		public static void CreateDirectoryIfNotExists(string directoryPath, bool deleteContentIfExists = false) {
			if (!Directory.Exists(directoryPath)) {
				Directory.CreateDirectory(directoryPath);
			} else if(deleteContentIfExists) {
				Directory.Delete(directoryPath, true);
				Directory.CreateDirectory(directoryPath);
			}
		}
#endregion

		[MenuItem("Tools/Recompile AudioManager")]
		public static void RecompileLibrary() {
			GenerateLibrary();
		}
	}
}