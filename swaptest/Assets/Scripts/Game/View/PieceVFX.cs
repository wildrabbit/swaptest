using UnityEngine;

namespace Game.View
{
    public class PieceVFX : MonoBehaviour
    {
        public void AnimFinished()
        {
            Destroy(gameObject);
        }
    }
}
