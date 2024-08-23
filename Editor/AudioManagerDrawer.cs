using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.Audio.Editor {
	[CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor {

		public AudioManager Target => (AudioManager)target;

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement();
			var globalSlider = GetSlider("Global Volume");
			var manager = (AudioManager)target;
			root.Add(globalSlider);
			root.Add(new Box() {
				style = {
					height = 12,
					backgroundColor = Color.clear
				}
			});
			globalSlider.Q<Slider>().RegisterValueChangedCallback((ChangeEvent<float> ev) => {
				AudioManager.SetVolume(ev.newValue, false);
				OnSliderValueChange(globalSlider, ev.newValue, default, true);
			});

			if (manager.volumes == null) {
				manager.volumes = new();
			}

			foreach(VolumeType type in System.Enum.GetValues(typeof(VolumeType))) {
				var volumeValue = manager.volumes.Find(v => v.type == type);
				if(volumeValue == null) {
					volumeValue = new AudioManager.AudioTypeValue() {
						type = type,
						volume = 0.5f
					};
				}

				var slider = GetSlider(type.ToString());
				slider.Q<Slider>().RegisterValueChangedCallback((ChangeEvent<float> ev) => {
					AudioManager.SetVolume(type, ev.newValue, false);
					OnSliderValueChange(slider, ev.newValue, type, false);
				});
				
				root.Add(slider);
				slider.Q<Slider>().value = volumeValue.volume;

				slider.schedule.Execute(() => {
					OnSliderValueChange(slider, Target.volumes.Find(t => t.type == type).volume, type, false);
				}).Every(50);
			}

			globalSlider.Q<Slider>().value = manager.globalVolume;

			root.schedule.Execute(() => {
				OnSliderValueChange(globalSlider, Target.globalVolume, default, true);
			}).Every(50);
			return root;
		}
		
		VisualElement GetSlider(string label) {
			var view = new VisualElement() {
				name = "View_" + label,
				style = {
					flexDirection = FlexDirection.Row,
					flexGrow = 1,
				},
			};
			var slider = new Slider(label, 0f, 1f) {
				name = "Slider",
				style = {
					width = new Length(85, LengthUnit.Percent),
				},
			};
			var percentLabel = new Label("10%") {
				name = "Percentage",
				style = {
					width = new Length(15, LengthUnit.Percent),
					unityTextAlign = TextAnchor.MiddleCenter
				}
			};

			view.hierarchy.Add(slider);
			view.hierarchy.Add(percentLabel);

			return view;
		}

		#region Events
		void OnSliderValueChange(VisualElement view, float value, VolumeType type, bool isGlobal = false) {
			view.Q<Label>("Percentage").text = Mathf.RoundToInt(value * 100f) + "%";
			view.Q<Slider>("Slider").SetValueWithoutNotify(value);

			if(value < 0.5f) {
				view.Q<Label>("Percentage").style.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), Color.yellow, value * 2f);
			} else {
				view.Q<Label>("Percentage").style.color = Color.Lerp(Color.yellow, new Color(0.3f, 1f, 0.3f), (value - 0.5f) * 2f);
			}

			if (isGlobal) {
				Target.globalVolume = value;
			} else {
				Target.volumes.Find(v => v.type == type).volume = value;
			}
		}
		#endregion
		/*VisualElement GetVolumeType(VolumeType type) {
			var listView = new MultiColumnListView();
			var slider = GetSlider(type.ToString());

			listView.Add();
			listView.Add(slider);
		}*/
	}
}