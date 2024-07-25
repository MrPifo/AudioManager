using Sperlich.Audio;
using System.Collections;
using UnityEngine;

public class AudioTester : MonoBehaviour {

	public float speed = 0.2f;

	void Awake() {
		var player = AudioManager.GetFreeSoundPlayer(AudioPreset.Default);
		StartCoroutine(ILoop());

		IEnumerator ILoop() {
			while(true) {
				AudioManager.Play3DSound(Sounds.BulletRicochet, SoundType.Default, 1f, new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)), 0.3f, 0.6f, 2f);
				yield return new WaitForSeconds(speed);
			}
		}
	}

}
