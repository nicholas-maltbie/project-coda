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
using UnityEngine;

namespace ProjectCoda.Solo
{
    public enum QTEDirection
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
    public class QTENote : NetworkBehaviour
    {
        public NetworkVariable<QTEDirection> direction;
        [SerializeField] private QTEImageLibrary imageLibrary;

        public void Start()
        {
            direction.OnValueChanged += OnDirectionChange;
        }

        public void Initialize()
        {
            QTEDirection dir;
            switch ((int)(Random.value * 4))
            {
                case 0: dir = QTEDirection.LEFT; break;
                case 1: dir = QTEDirection.RIGHT; break;
                case 2: dir = QTEDirection.UP; break;
                case 3: dir = QTEDirection.DOWN; break;
                default: dir = QTEDirection.LEFT; break;
            }

            Initialize(dir);
        }

        public void Initialize(QTEDirection dir)
        {
            direction.Value = dir;
            GetComponent<SpriteRenderer>().sprite = imageLibrary.GetSprite(dir);
        }

        public void OnDirectionChange(QTEDirection oldVal, QTEDirection newVal)
        {
            GetComponent<SpriteRenderer>().sprite = imageLibrary.GetSprite(newVal);
        }
    }
}
