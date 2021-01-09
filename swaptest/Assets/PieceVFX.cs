using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceVFX : MonoBehaviour
{
    public void AnimFinished()
    {
        Destroy(gameObject);
    }
}
