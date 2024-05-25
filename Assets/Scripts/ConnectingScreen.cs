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

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCoda
{
    public class ConnectingScreen : MonoBehaviour
    {
        private Button abortButton;
        private Label timerLabel;
        private float elapsed;

        public void Start()
        {
            FocusFirstElement();
        }

        public void FocusFirstElement()
        {
            GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("abort-connection").Focus();
        }

        public void OnEnable()
        {
            // The UXML is already instantiated by the UIDocument component
            UIDocument uiDocument = GetComponent<UIDocument>();

            abortButton = uiDocument.rootVisualElement.Q("abort-connection") as Button;
            timerLabel = uiDocument.rootVisualElement.Q("timer") as Label;
            abortButton.RegisterCallback<ClickEvent>(AbortConnection);
            abortButton.RegisterCallback<NavigationSubmitEvent>(AbortConnection);
            elapsed = 0;
        }

        public void OnDisable()
        {
            abortButton.UnregisterCallback<ClickEvent>(AbortConnection);
            abortButton.UnregisterCallback<NavigationSubmitEvent>(AbortConnection);
        }

        public void Update()
        {
            elapsed += Time.deltaTime;

            var networkTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
            if (networkTransport != null)
            {
                timerLabel.text = string.Format(
                    "Attempting connection to {0}:{1:D} for: {2:0.00} seconds",
                    networkTransport.ConnectionData.Address,
                    networkTransport.ConnectionData.Port,
                    elapsed);
            }
        }

        private void AbortConnection(ClickEvent evt) => AbortConnection();
        private void AbortConnection(NavigationSubmitEvent evt) => AbortConnection();

        private void AbortConnection()
        {
            Debug.Log("Leaving Game");
            NetworkManager.Singleton.Shutdown();
        }
    }
}
