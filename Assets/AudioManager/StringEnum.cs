using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Audio {
	[System.Serializable]
	public class StringEnum<T> : IStringEnum where T : struct, System.Enum {

		[SerializeField]
		private string name;
		private T? value;

		public T Value {
			get {
				if(value.HasValue == false) {
					if(Enum.TryParse(name, true, out T result)) {
						value = result;
					} else {
						value = default;
					}
				}

				return value.GetValueOrDefault();
			}
		}

		void IStringEnum.SetValue(Enum value) {
			this.name = value.ToString();
			this.value = (T)value;
		}
		Enum IStringEnum.GetValue() {
			value = null;
			return Value;
		}

		public StringEnum(T value) {
			this.value = value;
			this.name = value.ToString();
		}

		public static implicit operator T(StringEnum<T> value) {
			return value.Value;
		}
		public static implicit operator StringEnum<T>(T value) {
			return new StringEnum<T>(value);
		}
	}

	public interface IStringEnum {

		public void SetValue(Enum val);
		public Enum GetValue();
	}
}