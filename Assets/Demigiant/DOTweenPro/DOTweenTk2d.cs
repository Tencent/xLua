// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/10/27 15:59
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php

#if false // MODULE_MARKER
using UnityEngine;

namespace DG.Tweening
{
    /// <summary>
    /// Methods that extend 2D Toolkit objects and allow to directly create and control tweens from their instances.
    /// </summary>
    public static class ShortcutExtensionsTk2d
    {
        #region Sprite

        /// <summary>Tweens a 2D Toolkit Sprite's dimensions to the given value.
        /// Also stores the Sprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScale(this tk2dBaseSprite target, Vector3 endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, endValue, duration)
                .SetTarget(target);
        }
        /// <summary>Tweens a Sprite's dimensions to the given value.
        /// Also stores the Sprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleX(this tk2dBaseSprite target, float endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, new Vector3(endValue, 0, 0), duration)
                .SetOptions(AxisConstraint.X)
                .SetTarget(target);
        }
        /// <summary>Tweens a Sprite's dimensions to the given value.
        /// Also stores the Sprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleY(this tk2dBaseSprite target, float endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, new Vector3(0, endValue, 0), duration)
                .SetOptions(AxisConstraint.Y)
                .SetTarget(target);
        }
        /// <summary>Tweens a Sprite's dimensions to the given value.
        /// Also stores the Sprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleZ(this tk2dBaseSprite target, float endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, new Vector3(0, 0, endValue), duration)
                .SetOptions(AxisConstraint.Z)
                .SetTarget(target);
        }

        /// <summary>Tweens a 2D Toolkit Sprite's color to the given value.
        /// Also stores the Sprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOColor(this tk2dBaseSprite target, Color endValue, float duration)
        {
            return DOTween.To(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a 2D Toolkit Sprite's alpha color to the given value.
        /// Also stores the Sprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOFade(this tk2dBaseSprite target, float endValue, float duration)
        {
            return DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a 2D Toolkit Sprite's color using the given gradient
        /// (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="gradient">The gradient to use</param><param name="duration">The duration of the tween</param>
        public static Sequence DOGradientColor(this tk2dBaseSprite target, Gradient gradient, float duration)
        {
            Sequence s = DOTween.Sequence();
            GradientColorKey[] colors = gradient.colorKeys;
            int len = colors.Length;
            for (int i = 0; i < len; ++i) {
                GradientColorKey c = colors[i];
                if (i == 0 && c.time <= 0) {
                    target.color = c.color;
                    continue;
                }
                float colorDuration = i == len - 1
                    ? duration - s.Duration(false) // Verifies that total duration is correct
                    : duration * (i == 0 ? c.time : c.time - colors[i - 1].time);
                s.Append(target.DOColor(c.color, colorDuration).SetEase(Ease.Linear));
            }
            return s;
        }

        #endregion

        #region tk2dSlicedSprite

        /// <summary>Tweens a 2D Toolkit SlicedSprite's dimensions to the given value.
        /// Also stores the SlicedSprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleDimensions(this tk2dSlicedSprite target, Vector2 endValue, float duration)
        {
            return DOTween.To(() => target.dimensions, x => target.dimensions = x, endValue, duration)
                .SetTarget(target);
        }
        /// <summary>Tweens a SlicedSprite's dimensions to the given value.
        /// Also stores the SlicedSprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleDimensionsX(this tk2dSlicedSprite target, float endValue, float duration)
        {
            return DOTween.To(() => target.dimensions, x => target.dimensions = x, new Vector2(endValue, 0), duration)
                .SetOptions(AxisConstraint.X)
                .SetTarget(target);
        }
        /// <summary>Tweens a SlicedSprite's dimensions to the given value.
        /// Also stores the SlicedSprite as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleDimensionsY(this tk2dSlicedSprite target, float endValue, float duration)
        {
            return DOTween.To(() => target.dimensions, x => target.dimensions = x, new Vector2(0, endValue), duration)
                .SetOptions(AxisConstraint.Y)
                .SetTarget(target);
        }

        #endregion

        #region TextMesh

        /// <summary>Tweens a 2D Toolkit TextMesh's dimensions to the given value.
        /// Also stores the TextMesh as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScale(this tk2dTextMesh target, Vector3 endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, endValue, duration)
                .SetTarget(target);
        }
        /// <summary>Tweens a 2D Toolkit TextMesh's dimensions to the given value.
        /// Also stores the TextMesh as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleX(this tk2dTextMesh target, float endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, new Vector3(endValue, 0, 0), duration)
                .SetOptions(AxisConstraint.X)
                .SetTarget(target);
        }
        /// <summary>Tweens a 2D Toolkit TextMesh's dimensions to the given value.
        /// Also stores the TextMesh as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleY(this tk2dTextMesh target, float endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, new Vector3(0, endValue, 0), duration)
                .SetOptions(AxisConstraint.Y)
                .SetTarget(target);
        }
        /// <summary>Tweens a 2D Toolkit TextMesh's dimensions to the given value.
        /// Also stores the TextMesh as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScaleZ(this tk2dTextMesh target, float endValue, float duration)
        {
            return DOTween.To(() => target.scale, x => target.scale = x, new Vector3(0, 0, endValue), duration)
                .SetOptions(AxisConstraint.Z)
                .SetTarget(target);
        }

        /// <summary>Tweens a 2D Toolkit TextMesh's color to the given value.
        /// Also stores the TextMesh as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOColor(this tk2dTextMesh target, Color endValue, float duration)
        {
            return DOTween.To(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a 2D Toolkit TextMesh's alpha color to the given value.
        /// Also stores the TextMesh as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOFade(this tk2dTextMesh target, float endValue, float duration)
        {
            return DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a 2D Toolkit TextMesh's color using the given gradient
        /// (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="gradient">The gradient to use</param><param name="duration">The duration of the tween</param>
        public static Sequence DOGradientColor(this tk2dTextMesh target, Gradient gradient, float duration)
        {
            Sequence s = DOTween.Sequence();
            GradientColorKey[] colors = gradient.colorKeys;
            int len = colors.Length;
            for (int i = 0; i < len; ++i) {
                GradientColorKey c = colors[i];
                if (i == 0 && c.time <= 0) {
                    target.color = c.color;
                    continue;
                }
                float colorDuration = i == len - 1
                    ? duration - s.Duration(false) // Verifies that total duration is correct
                    : duration * (i == 0 ? c.time : c.time - colors[i - 1].time);
                s.Append(target.DOColor(c.color, colorDuration).SetEase(Ease.Linear));
            }
            return s;
        }

        /// <summary>Tweens a tk2dTextMesh's text to the given value.
        /// Also stores the tk2dTextMesh as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end string to tween to</param><param name="duration">The duration of the tween</param>
        /// <param name="richTextEnabled">If TRUE (default), rich text will be interpreted correctly while animated,
        /// otherwise all tags will be considered as normal text</param>
        /// <param name="scrambleMode">The type of scramble mode to use, if any</param>
        /// <param name="scrambleChars">A string containing the characters to use for scrambling.
        /// Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters.
        /// Leave it to NULL (default) to use default ones</param>
        public static Tweener DOText(this tk2dTextMesh target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
        {
            return DOTween.To(() => target.text, x => target.text = x, endValue, duration)
                .SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                .SetTarget(target);
        }

        #endregion
    }
}
#endif
