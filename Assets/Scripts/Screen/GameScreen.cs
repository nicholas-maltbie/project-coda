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

using Eflatun.SceneReference;
using ProjectCoda.State;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCoda.Screen
{
    public class GameScreen : MonoBehaviour
    {
        [SerializeField]
        private SceneReference lobbyScene;

        private Button leaveButton;

        private Label timer;

        public void OnEnable()
        {
            // The UXML is already instantiated by the UIDocument component
            UIDocument uiDocument = GetComponent<UIDocument>();

            leaveButton = uiDocument.rootVisualElement.Q("leave-game") as Button;
            timer = uiDocument.rootVisualElement.Q("game-timer") as Label;
            leaveButton.RegisterCallback<ClickEvent>(LeaveGame);
            leaveButton.RegisterCallback<NavigationSubmitEvent>(LeaveGame);

            leaveButton.visible = NetworkManager.Singleton.IsServer;
        }

        public void OnDisable()
        {
            leaveButton.UnregisterCallback<ClickEvent>(LeaveGame);
            leaveButton.UnregisterCallback<NavigationSubmitEvent>(LeaveGame);
        }

        private void LeaveGame(ClickEvent evt)
        {
            DoLeave();
        }

        private void LeaveGame(NavigationSubmitEvent evt)
        {
            DoLeave();
        }

        public void Update()
        {
            if (GameState.Instance == null)
            {
                return;
            }

            switch (GameState.Instance.Phase)
            {
                case GameState.GamePhase.Starting:
                    timer.visible = true;
                    timer.text = GameState.Instance.TimeToStart.ToString("{0.00}");
                    break;
                case GameState.GamePhase.Fighting:
                    timer.visible = true;
                    timer.text = GameState.Instance.GameTimeRemaining.ToString("{0.00}");
                    break;
                case GameState.GamePhase.Ending:
                    timer.visible = true;
                    timer.text = GameState.Instance.TimeToClose.ToString("{0.00}");
                    break;
                default:
                case GameState.GamePhase.Unknown:
                    timer.visible = false;
                    break;
            }
        }

        private void DoLeave()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(lobbyScene.Name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
