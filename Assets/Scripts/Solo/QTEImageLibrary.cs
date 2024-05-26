using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCoda.Solo
{
    [CreateAssetMenu(fileName="QTEImageLibrary", menuName="ScriptableObjects/QTEImageLibrary")]
    public class QTEImageLibrary : ScriptableObject
    {
        public Sprite up, down, left, right;

        public Sprite GetSprite(QTEDirection direction)
        {
            switch( direction )
            {
                case QTEDirection.UP: return up;
                case QTEDirection.DOWN: return down;
                case QTEDirection.LEFT: return left;
                case QTEDirection.RIGHT: return right;
                default: return default;
            }
        }
    }
}
