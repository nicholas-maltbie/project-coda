using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ProjectCoda
{
    public class DisableIfNotOwned : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            NetworkBehaviour nm = GetComponentInParent<NetworkBehaviour>();
            if (!nm.IsOwner)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
