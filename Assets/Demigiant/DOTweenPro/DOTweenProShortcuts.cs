// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins;
using UnityEngine;

#pragma warning disable 1591
namespace DG.Tweening
{
	public static class DOTweenProShortcuts
    {
        static DOTweenProShortcuts()
        {
            // Create stub instances of custom plugins, in order to allow IL2CPP to understand they must be included in the build
#pragma warning disable 219
            SpiralPlugin stub = new SpiralPlugin();
#pragma warning restore 219
        }

        #region Shortcuts

        #region Transform

        /// <summary>Tweens a Transform's localPosition in a spiral shape.
        /// Also stores the transform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="axis">The axis around which the spiral will rotate</param>
        /// <param name="mode">The type of spiral movement</param>
        /// <param name="speed">Speed of the rotations</param>
        /// <param name="frequency">Frequency of the rotation. Lower values lead to wider spirals</param>
        /// <param name="depth">Indicates how much the tween should move along the spiral's axis</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Tweener DOSpiral(
            this Transform target, float duration, Vector3? axis = null, SpiralMode mode = SpiralMode.Expand,
            float speed = 1, float frequency = 10, float depth = 0, bool snapping = false
        ) {
            if (Mathf.Approximately(speed, 0)) speed = 1;
            if (axis == null || axis == Vector3.zero) axis = Vector3.forward;

            TweenerCore<Vector3, Vector3, SpiralOptions> t = DOTween.To(SpiralPlugin.Get(), () => target.localPosition, x => target.localPosition = x, (Vector3)axis, duration)
                .SetTarget(target);

            t.plugOptions.mode = mode;
            t.plugOptions.speed = speed;
            t.plugOptions.frequency = frequency;
            t.plugOptions.depth = depth;
            t.plugOptions.snapping = snapping;
            return t;
        }

        #endregion

#if true // PHYSICS_MARKER
        #region Rigidbody

        /// <summary>Tweens a Rigidbody's position in a spiral shape.
        /// Also stores the transform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="axis">The axis around which the spiral will rotate</param>
        /// <param name="mode">The type of spiral movement</param>
        /// <param name="speed">Speed of the rotations</param>
        /// <param name="frequency">Frequency of the rotation. Lower values lead to wider spirals</param>
        /// <param name="depth">Indicates how much the tween should move along the spiral's axis</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Tweener DOSpiral(
            this Rigidbody target, float duration, Vector3? axis = null, SpiralMode mode = SpiralMode.Expand,
            float speed = 1, float frequency = 10, float depth = 0, bool snapping = false
        ) {
            if (Mathf.Approximately(speed, 0)) speed = 1;
            if (axis == null || axis == Vector3.zero) axis = Vector3.forward;

            TweenerCore<Vector3, Vector3, SpiralOptions> t = DOTween.To(SpiralPlugin.Get(), () => target.position, target.MovePosition, (Vector3)axis, duration)
                .SetTarget(target);

            t.plugOptions.mode = mode;
            t.plugOptions.speed = speed;
            t.plugOptions.frequency = frequency;
            t.plugOptions.depth = depth;
            t.plugOptions.snapping = snapping;
            return t;
        }

        #endregion
#endif

        #endregion
	}
}
