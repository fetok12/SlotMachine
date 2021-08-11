using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	public class AudioUtils : MonoBehaviour
	{

		[SerializeField]
		private AudioSource m_2DAudioSource = null;

		public void Play2D(AudioClip clip, float volume)
		{
			if ((bool)m_2DAudioSource)
			{
				m_2DAudioSource.PlayOneShot(clip, volume);
			}
		}

	}
