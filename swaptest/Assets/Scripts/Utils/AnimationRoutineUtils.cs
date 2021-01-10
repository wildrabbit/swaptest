﻿using System;
using System.Collections;
using UnityEngine;

namespace Game.Utils
{
    public static class AnimationRoutineUtils
    {
        public static IEnumerator AnimateFloatWithEaseCurve(float duration, AnimationCurve easeCurve, Action<float> update)
        {
            float t = 0.0f;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                t = easeCurve.Evaluate(elapsed / duration);
                update?.Invoke(t);
                yield return null;
                elapsed += Time.deltaTime;
            }
        }

        public static IEnumerator LerpVectorWithEaseCurve(Vector3 startVec, Vector3 endVec, float duration, AnimationCurve easeCurve, Action<Vector3> updateFunction)
        {
            float t = 0.0f;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                t = easeCurve.Evaluate(elapsed / duration);
                updateFunction?.Invoke(Vector3.Lerp(startVec, endVec, t));
                yield return null;
                elapsed += Time.deltaTime;
            }
        }
    }
}