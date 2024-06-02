// Copyright (C) 2024 Nicholas Maltbie, Sam Scherer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCoda
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        public AudioClip intro;
        public bool muffled = false;
        private float originalVolume;

        // Start is called before the first frame update
        void Start()
        {
            originalVolume = GetComponent<AudioSource>().volume;
            GetComponent<AudioSource>().PlayOneShot(intro);
            GetComponent<AudioSource>().PlayScheduled(AudioSettings.dspTime + intro.length);
            StartCoroutine(BeginLoop());
        }

        public IEnumerator BeginLoop()
        {
            while( GetComponent<AudioSource>().isPlaying )
            {
                yield return null;
            }

            GetComponent<AudioSource>().SetScheduledStartTime(0);
        }

        public void SetMuffled(bool isMuffled)
        {
            if( muffled && !isMuffled)
            {
                GetComponent<AudioSource>().volume = originalVolume;
                muffled = false;
            }
            else if( !muffled && isMuffled ) 
            {
                GetComponent<AudioSource>().volume *= .5f;
                muffled = true;
            }
        }
    }
}
