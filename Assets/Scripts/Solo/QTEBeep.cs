using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ProjectCoda
{
    public class QTEBeep : MonoBehaviour
    {
        public void Awake()
        {
            GetComponent<AudioSource>().pitch = Random.value * 2f + 1;
        }

        private void Start()
        {
            if( NetworkManager.Singleton.IsServer )
            {
                StartCoroutine(Fade());
            }
        }

        public IEnumerator Fade()
        {
            yield return new WaitForSeconds(1);
            Destroy(gameObject);
        }
    }
}
