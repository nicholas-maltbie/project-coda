using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ProjectCoda
{
    public class QTESolo : MonoBehaviour
    {
        

        public void StartSolo(ulong clientIdForSolo)
        {
            foreach( ulong clientId in NetworkManager.Singleton.ConnectedClientsIds )
            {
                if( clientId == clientIdForSolo )
                {
                    // this player does the solo!
                }
                else
                {
                    // these players watch the solo.
                }
            }
        }
    }
}
