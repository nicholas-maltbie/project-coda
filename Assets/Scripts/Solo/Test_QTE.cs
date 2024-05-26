using System.Collections;
using System.Collections.Generic;
using ProjectCoda.Solo;
using Unity.Netcode;
using UnityEngine;

namespace ProjectCoda
{
    public class Test_QTE : MonoBehaviour
    {
        public void StartTest()
        {
            QTESolo.StartSolo(NetworkManager.Singleton.LocalClientId);
        }
    }
}
