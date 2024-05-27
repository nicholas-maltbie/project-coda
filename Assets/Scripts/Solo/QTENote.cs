using System.Collections;
using System.Collections.Generic;
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

        private void Start()
        {
            direction.OnValueChanged += OnDirectionChange;
        }

        public void Initialize()
        {
            QTEDirection dir;
            switch( (int)(Random.value * 4) )
            {
                case 0: dir = QTEDirection.LEFT; break;
                case 1: dir = QTEDirection.RIGHT; break;
                case 2: dir = QTEDirection.UP; break;
                case 3: dir = QTEDirection.DOWN; break;
                default: dir = QTEDirection.LEFT; break;
            }

            Initialize(dir);
        }

        public void Initialize( QTEDirection dir )
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
