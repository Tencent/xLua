// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/27 19:02
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php

#if false // MODULE_MARKER
using UnityEngine;
using TMPro;

namespace DG.Tweening
{
    /// <summary>
    /// Methods that extend TMP_Text objects and allow to directly create and control tweens from their instances.
    /// </summary>
    public static class ShortcutExtensionsTMPText
    {
        #region Colors

        /// <summary>Tweens a TextMeshPro's color to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOColor(this TMP_Text target, Color endValue, float duration)
        {
            return DOTween.To(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro's faceColor to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOFaceColor(this TMP_Text target, Color32 endValue, float duration)
        {
            return DOTween.To(() => target.faceColor, x => target.faceColor = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro's outlineColor to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOOutlineColor(this TMP_Text target, Color32 endValue, float duration)
        {
            return DOTween.To(() => target.outlineColor, x => target.outlineColor = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro's glow color to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="useSharedMaterial">If TRUE will use the fontSharedMaterial instead than the fontMaterial</param>
        public static Tweener DOGlowColor(this TMP_Text target, Color endValue, float duration, bool useSharedMaterial = false)
        {
            return useSharedMaterial
                ? target.fontSharedMaterial.DOColor(endValue, "_GlowColor", duration).SetTarget(target)
                : target.fontMaterial.DOColor(endValue, "_GlowColor", duration).SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro's alpha color to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOFade(this TMP_Text target, float endValue, float duration)
        {
            return DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro faceColor's alpha to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOFaceFade(this TMP_Text target, float endValue, float duration)
        {
            return DOTween.ToAlpha(() => target.faceColor, x => target.faceColor = x, endValue, duration)
                .SetTarget(target);
        }

        #endregion

        #region Other

        /// <summary>Tweens a TextMeshPro's scale to the given value (using correct uniform scale as TMP requires).
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOScale(this TMP_Text target, float endValue, float duration)
        {
            Transform t = target.transform;
            Vector3 endValueV3 = new Vector3(endValue, endValue, endValue);
            return DOTween.To(() => t.localScale, x => t.localScale = x, endValueV3, duration).SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro's fontSize to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOFontSize(this TMP_Text target, float endValue, float duration)
        {
            return DOTween.To(() => target.fontSize, x => target.fontSize = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro's maxVisibleCharacters to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static Tweener DOMaxVisibleCharacters(this TMP_Text target, int endValue, float duration)
        {
            return DOTween.To(() => target.maxVisibleCharacters, x => target.maxVisibleCharacters = x, endValue, duration)
                .SetTarget(target);
        }

        /// <summary>Tweens a TextMeshPro's text to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end string to tween to</param><param name="duration">The duration of the tween</param>
        /// <param name="richTextEnabled">If TRUE (default), rich text will be interpreted correctly while animated,
        /// otherwise all tags will be considered as normal text</param>
        /// <param name="scrambleMode">The type of scramble mode to use, if any</param>
        /// <param name="scrambleChars">A string containing the characters to use for scrambling.
        /// Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters.
        /// Leave it to NULL (default) to use default ones</param>
        public static Tweener DOText(this TMP_Text target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
        {
            return DOTween.To(() => target.text, x => target.text = x, endValue, duration)
                .SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                .SetTarget(target);
        }

        #endregion
    }

//    /// <summary>
//    /// Methods that extend TextMeshPro objects and allow to directly create and control tweens from their instances.
//    /// </summary>
//    public static class ShortcutExtensionsTextMeshPro
//    {
//        #region Colors
//
//        /// <summary>Tweens a TextMeshPro's color to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOColor(this TextMeshPro target, Color endValue, float duration)
//        {
//            return DOTween.To(() => target.color, x => target.color = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro's faceColor to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFaceColor(this TextMeshPro target, Color32 endValue, float duration)
//        {
//            return DOTween.To(() => target.faceColor, x => target.faceColor = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro's outlineColor to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOOutlineColor(this TextMeshPro target, Color32 endValue, float duration)
//        {
//            return DOTween.To(() => target.outlineColor, x => target.outlineColor = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro's glow color to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        /// <param name="useSharedMaterial">If TRUE will use the fontSharedMaterial instead than the fontMaterial</param>
//        public static Tweener DOGlowColor(this TextMeshPro target, Color endValue, float duration, bool useSharedMaterial = false)
//        {
//            return useSharedMaterial
//                ? target.fontSharedMaterial.DOColor(endValue, "_GlowColor", duration).SetTarget(target)
//                : target.fontMaterial.DOColor(endValue, "_GlowColor", duration).SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro's alpha color to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFade(this TextMeshPro target, float endValue, float duration)
//        {
//            return DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro faceColor's alpha to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFaceFade(this TextMeshPro target, float endValue, float duration)
//        {
//            return DOTween.ToAlpha(() => target.faceColor, x => target.faceColor = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        #endregion
//
//        #region Other
//
//        /// <summary>Tweens a TextMeshPro's scale to the given value (using correct uniform scale as TMP requires).
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOScale(this TextMeshPro target, float endValue, float duration)
//        {
//            Transform t = target.transform;
//            Vector3 endValueV3 = new Vector3(endValue, endValue, endValue);
//            return DOTween.To(() => t.localScale, x => t.localScale = x, endValueV3, duration).SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro's fontSize to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFontSize(this TextMeshPro target, float endValue, float duration)
//        {
//            return DOTween.To(() => target.fontSize, x => target.fontSize = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro's maxVisibleCharacters to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOMaxVisibleCharacters(this TextMeshPro target, int endValue, float duration)
//        {
//            return DOTween.To(() => target.maxVisibleCharacters, x => target.maxVisibleCharacters = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshPro's text to the given value.
//        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end string to tween to</param><param name="duration">The duration of the tween</param>
//        /// <param name="richTextEnabled">If TRUE (default), rich text will be interpreted correctly while animated,
//        /// otherwise all tags will be considered as normal text</param>
//        /// <param name="scrambleMode">The type of scramble mode to use, if any</param>
//        /// <param name="scrambleChars">A string containing the characters to use for scrambling.
//        /// Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters.
//        /// Leave it to NULL (default) to use default ones</param>
//        public static Tweener DOText(this TextMeshPro target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
//        {
//            return DOTween.To(() => target.text, x => target.text = x, endValue, duration)
//                .SetOptions(richTextEnabled, scrambleMode, scrambleChars)
//                .SetTarget(target);
//        }
//
//        #endregion
//    }
//
//    /// <summary>
//    /// Methods that extend TextMeshProUGUI objects and allow to directly create and control tweens from their instances.
//    /// </summary>
//    public static class ShortcutExtensionsTextMeshProUGUI
//    {
//        #region Colors
//
//        /// <summary>Tweens a TextMeshProUGUI's color to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOColor(this TextMeshProUGUI target, Color endValue, float duration)
//        {
//            return DOTween.To(() => target.color, x => target.color = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI's faceColor to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFaceColor(this TextMeshProUGUI target, Color32 endValue, float duration)
//        {
//            return DOTween.To(() => target.faceColor, x => target.faceColor = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI's outlineColor to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOOutlineColor(this TextMeshProUGUI target, Color32 endValue, float duration)
//        {
//            return DOTween.To(() => target.outlineColor, x => target.outlineColor = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI's glow color to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        /// <param name="useSharedMaterial">If TRUE will use the fontSharedMaterial instead than the fontMaterial</param>
//        public static Tweener DOGlowColor(this TextMeshProUGUI target, Color endValue, float duration, bool useSharedMaterial = false)
//        {
//            return useSharedMaterial
//                ? target.fontSharedMaterial.DOColor(endValue, "_GlowColor", duration).SetTarget(target)
//                : target.fontMaterial.DOColor(endValue, "_GlowColor", duration).SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI's alpha color to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFade(this TextMeshProUGUI target, float endValue, float duration)
//        {
//            return DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI faceColor's alpha to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFaceFade(this TextMeshProUGUI target, float endValue, float duration)
//        {
//            return DOTween.ToAlpha(() => target.faceColor, x => target.faceColor = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        #endregion
//
//        #region Other
//
//        /// <summary>Tweens a TextMeshProUGUI's scale to the given value (using correct uniform scale as TMP requires).
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOScale(this TextMeshProUGUI target, float endValue, float duration)
//        {
//            Transform t = target.transform;
//            Vector3 endValueV3 = new Vector3(endValue, endValue, endValue);
//            return DOTween.To(() => t.localScale, x => t.localScale = x, endValueV3, duration).SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI's fontSize to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOFontSize(this TextMeshProUGUI target, float endValue, float duration)
//        {
//            return DOTween.To(() => target.fontSize, x => target.fontSize = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI's maxVisibleCharacters to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
//        public static Tweener DOMaxVisibleCharacters(this TextMeshProUGUI target, int endValue, float duration)
//        {
//            return DOTween.To(() => target.maxVisibleCharacters, x => target.maxVisibleCharacters = x, endValue, duration)
//                .SetTarget(target);
//        }
//
//        /// <summary>Tweens a TextMeshProUGUI's text to the given value.
//        /// Also stores the TextMeshProUGUI as the tween's target so it can be used for filtered operations</summary>
//        /// <param name="endValue">The end string to tween to</param><param name="duration">The duration of the tween</param>
//        /// <param name="richTextEnabled">If TRUE (default), rich text will be interpreted correctly while animated,
//        /// otherwise all tags will be considered as normal text</param>
//        /// <param name="scrambleMode">The type of scramble mode to use, if any</param>
//        /// <param name="scrambleChars">A string containing the characters to use for scrambling.
//        /// Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters.
//        /// Leave it to NULL (default) to use default ones</param>
//        public static Tweener DOText(this TextMeshProUGUI target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
//        {
//            return DOTween.To(() => target.text, x => target.text = x, endValue, duration)
//                .SetOptions(richTextEnabled, scrambleMode, scrambleChars)
//                .SetTarget(target);
//        }
//
//        #endregion
//    }
}
#endif
