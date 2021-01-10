using UnityEngine;

namespace Game.View
{
    /// <summary>
    /// Control script to destroy explosion VFX when the frame animation finishes.
    /// </summary>
    public class PieceVFX : MonoBehaviour
    {
        public void AnimFinished()
        {
            Destroy(gameObject);
        }
    }
}
