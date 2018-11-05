// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/12 16:03

using System;
using System.Collections.Generic;
using System.IO;
using DG.DemiEditor;
using DG.DOTweenEditor.Core;
using DG.DOTweenEditor.UI;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;
#if true // UI_MARKER
using UnityEngine.UI;
#endif
#if false // TEXTMESHPRO_MARKER
    using TMPro;
#endif

namespace DG.DOTweenEditor
{
    [CustomEditor(typeof(DOTweenAnimation))]
    public class DOTweenAnimationInspector : ABSAnimationInspector
    {
        enum FadeTargetType
        {
            CanvasGroup,
            Image
        }

        enum ChooseTargetMode
        {
            None,
            BetweenCanvasGroupAndImage
        }

        static readonly Dictionary<DOTweenAnimationType, Type[]> _AnimationTypeToComponent = new Dictionary<DOTweenAnimationType, Type[]>() {
            { DOTweenAnimationType.Move, new[] {
                typeof(Transform),
#if true // PHYSICS_MARKER
                typeof(Rigidbody),
#endif
#if true // PHYSICS2D_MARKER
                typeof(Rigidbody2D),
#endif
#if true // UI_MARKER
                typeof(RectTransform),
#endif
            }},
            { DOTweenAnimationType.Rotate, new[] {
                typeof(Transform),
#if true // PHYSICS_MARKER
                typeof(Rigidbody),
#endif
#if true // PHYSICS2D_MARKER
                typeof(Rigidbody2D),
#endif
            }},
            { DOTweenAnimationType.LocalMove, new[] { typeof(Transform) } },
            { DOTweenAnimationType.LocalRotate, new[] { typeof(Transform) } },
            { DOTweenAnimationType.Scale, new[] { typeof(Transform) } },
            { DOTweenAnimationType.Color, new[] {
                typeof(Renderer),
                typeof(Light),
#if true // SPRITE_MARKER
                typeof(SpriteRenderer),
#endif
#if true // UI_MARKER
                typeof(Image), typeof(Text),
#endif
            }},
            { DOTweenAnimationType.Fade, new[] {
                typeof(Renderer),
                typeof(Light),
#if true // SPRITE_MARKER
                typeof(SpriteRenderer),
#endif
#if true // UI_MARKER
                typeof(Image), typeof(Text), typeof(CanvasGroup),
#endif
            }},
#if true // UI_MARKER
            { DOTweenAnimationType.Text, new[] { typeof(Text) } },
#endif
            { DOTweenAnimationType.PunchPosition, new[] {
                typeof(Transform),
#if true // UI_MARKER
                typeof(RectTransform),
#endif
            }},
            { DOTweenAnimationType.PunchRotation, new[] { typeof(Transform) } },
            { DOTweenAnimationType.PunchScale, new[] { typeof(Transform) } },
            { DOTweenAnimationType.ShakePosition, new[] {
                typeof(Transform),
#if true // UI_MARKER
                typeof(RectTransform),
#endif
            }},
            { DOTweenAnimationType.ShakeRotation, new[] { typeof(Transform) } },
            { DOTweenAnimationType.ShakeScale, new[] { typeof(Transform) } },
            { DOTweenAnimationType.CameraAspect, new[] { typeof(Camera) } },
            { DOTweenAnimationType.CameraBackgroundColor, new[] { typeof(Camera) } },
            { DOTweenAnimationType.CameraFieldOfView, new[] { typeof(Camera) } },
            { DOTweenAnimationType.CameraOrthoSize, new[] { typeof(Camera) } },
            { DOTweenAnimationType.CameraPixelRect, new[] { typeof(Camera) } },
            { DOTweenAnimationType.CameraRect, new[] { typeof(Camera) } },
#if true // UI_MARKER
            { DOTweenAnimationType.UIWidthHeight, new[] { typeof(RectTransform) } },
#endif
        };

#if false // TK2D_MARKER
        static readonly Dictionary<DOTweenAnimationType, Type[]> _Tk2dAnimationTypeToComponent = new Dictionary<DOTweenAnimationType, Type[]>() {
            { DOTweenAnimationType.Scale, new[] { typeof(tk2dBaseSprite), typeof(tk2dTextMesh) } },
            { DOTweenAnimationType.Color, new[] { typeof(tk2dBaseSprite), typeof(tk2dTextMesh) } },
            { DOTweenAnimationType.Fade, new[] { typeof(tk2dBaseSprite), typeof(tk2dTextMesh) } },
            { DOTweenAnimationType.Text, new[] { typeof(tk2dTextMesh) } }
        };
#endif
#if false // TEXTMESHPRO_MARKER
        static readonly Dictionary<DOTweenAnimationType, Type[]> _TMPAnimationTypeToComponent = new Dictionary<DOTweenAnimationType, Type[]>() {
            { DOTweenAnimationType.Color, new[] { typeof(TextMeshPro), typeof(TextMeshProUGUI) } },
            { DOTweenAnimationType.Fade, new[] { typeof(TextMeshPro), typeof(TextMeshProUGUI) } },
            { DOTweenAnimationType.Text, new[] { typeof(TextMeshPro), typeof(TextMeshProUGUI) } }
        };
#endif

        static readonly string[] _AnimationType = new[] {
            "None",
            "Move", "LocalMove",
            "Rotate", "LocalRotate",
            "Scale",
            "Color", "Fade",
#if true // UI_MARKER
            "Text",
#endif
#if false // TK2D_MARKER
            "Text",
#endif
#if false // TEXTMESHPRO_MARKER
            "Text",
#endif
#if true // UI_MARKER
            "UIWidthHeight",
#endif
            "Punch/Position", "Punch/Rotation", "Punch/Scale",
            "Shake/Position", "Shake/Rotation", "Shake/Scale",
            "Camera/Aspect", "Camera/BackgroundColor", "Camera/FieldOfView", "Camera/OrthoSize", "Camera/PixelRect", "Camera/Rect"
        };
        static string[] _animationTypeNoSlashes; // _AnimationType list without slashes in values
        static string[] _datString; // String representation of DOTweenAnimation enum (here for caching reasons)

        DOTweenAnimation _src;
        bool _runtimeEditMode; // If TRUE allows to change and save stuff at runtime
        bool _refreshRequired; // If TRUE refreshes components data
        int _totComponentsOnSrc; // Used to determine if a Component is added or removed from the source
        bool _isLightSrc; // Used to determine if we're tweening a Light, to set the max Fade value to more than 1
#pragma warning disable 414
        ChooseTargetMode _chooseTargetMode = ChooseTargetMode.None;
#pragma warning restore 414

        static readonly GUIContent _GuiC_selfTarget_true = new GUIContent(
            "SELF", "Will animate components on this gameObject"
        );
        static readonly GUIContent _GuiC_selfTarget_false = new GUIContent(
            "OTHER", "Will animate components on the given gameObject instead than on this one"
        );
        static readonly GUIContent _GuiC_tweenTargetIsTargetGO_true = new GUIContent(
            "Use As Tween Target", "Will set the tween target (via SetTarget, used to control a tween directly from a target) to the \"OTHER\" gameObject"
        );
        static readonly GUIContent _GuiC_tweenTargetIsTargetGO_false = new GUIContent(
            "Use As Tween Target", "Will set the tween target (via SetTarget, used to control a tween directly from a target) to the gameObject containing this animation, not the \"OTHER\" one"
        );

        #region MonoBehaviour Methods

        void OnEnable()
        {
            _src = target as DOTweenAnimation;

            onStartProperty = base.serializedObject.FindProperty("onStart");
            onPlayProperty = base.serializedObject.FindProperty("onPlay");
            onUpdateProperty = base.serializedObject.FindProperty("onUpdate");
            onStepCompleteProperty = base.serializedObject.FindProperty("onStepComplete");
            onCompleteProperty = base.serializedObject.FindProperty("onComplete");
            onRewindProperty = base.serializedObject.FindProperty("onRewind");
            onTweenCreatedProperty = base.serializedObject.FindProperty("onTweenCreated");

            // Convert _AnimationType to _animationTypeNoSlashes
            int len = _AnimationType.Length;
            _animationTypeNoSlashes = new string[len];
            for (int i = 0; i < len; ++i) {
                string a = _AnimationType[i];
                a = a.Replace("/", "");
                _animationTypeNoSlashes[i] = a;
            }
        }

        override public void OnInspectorGUI()
        {
        	base.OnInspectorGUI();

            GUILayout.Space(3);
            EditorGUIUtils.SetGUIStyles();

            bool playMode = Application.isPlaying;
            _runtimeEditMode = _runtimeEditMode && playMode;

            GUILayout.BeginHorizontal();
            EditorGUIUtils.InspectorLogo();
            GUILayout.Label(_src.animationType.ToString() + (string.IsNullOrEmpty(_src.id) ? "" : " [" + _src.id + "]"), EditorGUIUtils.sideLogoIconBoldLabelStyle);
            // Up-down buttons
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("▲", DeGUI.styles.button.toolIco)) UnityEditorInternal.ComponentUtility.MoveComponentUp(_src);
            if (GUILayout.Button("▼", DeGUI.styles.button.toolIco)) UnityEditorInternal.ComponentUtility.MoveComponentDown(_src);
            GUILayout.EndHorizontal();

            if (playMode) {
                if (_runtimeEditMode) {
                    
                } else {
                    GUILayout.Space(8);
                    GUILayout.Label("Animation Editor disabled while in play mode", EditorGUIUtils.wordWrapLabelStyle);
                    if (!_src.isActive) {
                        GUILayout.Label("This animation has been toggled as inactive and won't be generated", EditorGUIUtils.wordWrapLabelStyle);
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button(new GUIContent("Activate Edit Mode", "Switches to Runtime Edit Mode, where you can change animations values and restart them"))) {
                        _runtimeEditMode = true;
                    }
                    GUILayout.Label("NOTE: when using DOPlayNext, the sequence is determined by the DOTweenAnimation Components order in the target GameObject's Inspector", EditorGUIUtils.wordWrapLabelStyle);
                    GUILayout.Space(10);
                    if (!_runtimeEditMode) return;
                }
            }

            Undo.RecordObject(_src, "DOTween Animation");

//            _src.isValid = Validate(); // Moved down

            EditorGUIUtility.labelWidth = 110;

            if (playMode) {
                GUILayout.Space(4);
                DeGUILayout.Toolbar("Edit Mode Commands");
                DeGUILayout.BeginVBox(DeGUI.styles.box.stickyTop);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("TogglePause")) _src.tween.TogglePause();
                    if (GUILayout.Button("Rewind")) _src.tween.Rewind();
                    if (GUILayout.Button("Restart")) _src.tween.Restart();
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Commit changes and restart")) {
                        _src.tween.Rewind();
                        _src.tween.Kill();
                        if (_src.isValid) {
                            _src.CreateTween();
                            _src.tween.Play();
                        }
                    }
                    GUILayout.Label("To apply your changes when exiting Play mode, use the Component's upper right menu and choose \"Copy Component\", then \"Paste Component Values\" after exiting Play mode", DeGUI.styles.label.wordwrap);
                DeGUILayout.EndVBox();
            } else {
                bool hasManager = _src.GetComponent<DOTweenVisualManager>() != null;
                if (!hasManager) {
                    if (GUILayout.Button(new GUIContent("Add Manager", "Adds a manager component which allows you to choose additional options for this gameObject"))) {
                        _src.gameObject.AddComponent<DOTweenVisualManager>();
                    }
                }
            }

            // Choose target
            GUILayout.BeginHorizontal();
                _src.isActive = EditorGUILayout.Toggle(new GUIContent("", "If unchecked, this animation will not be created"), _src.isActive, GUILayout.Width(14));
                EditorGUI.BeginChangeCheck();
                    EditorGUI.BeginChangeCheck();
                        _src.targetIsSelf = DeGUILayout.ToggleButton(
                            _src.targetIsSelf, _src.targetIsSelf ? _GuiC_selfTarget_true : _GuiC_selfTarget_false,
                            new Color(1f, 0.78f, 0f), DeGUI.colors.bg.toggleOn, new Color(0.33f, 0.14f, 0.02f), DeGUI.colors.content.toggleOn,
                            null, GUILayout.Width(47)
                        );
                    bool innerChanged = EditorGUI.EndChangeCheck();
                    if (innerChanged) {
                        _src.targetGO = null;
                        GUI.changed = true;
                    }
                    if (_src.targetIsSelf) GUILayout.Label(_GuiC_selfTarget_true.tooltip);
                    else {
                        using (new DeGUI.ColorScope(null, null, _src.targetGO == null ? Color.red : Color.white)) {
                            _src.targetGO = (GameObject)EditorGUILayout.ObjectField(_src.targetGO, typeof(GameObject), true);
                        }
                        _src.tweenTargetIsTargetGO = DeGUILayout.ToggleButton(
                            _src.tweenTargetIsTargetGO, _src.tweenTargetIsTargetGO ? _GuiC_tweenTargetIsTargetGO_true : _GuiC_tweenTargetIsTargetGO_false,
                            GUILayout.Width(131)
                        );
                    }
                bool check = EditorGUI.EndChangeCheck();
                if (check) _refreshRequired = true;
            GUILayout.EndHorizontal();

            GameObject targetGO = _src.targetIsSelf ? _src.gameObject : _src.targetGO;

            if (targetGO == null) {
                // Uses external target gameObject but it's not set
                if (_src.targetGO != null || _src.target != null) {
                    _src.targetGO = null;
                    _src.target = null;
                    GUI.changed = true;
                }
            } else {
                GUILayout.BeginHorizontal();
                DOTweenAnimationType prevAnimType = _src.animationType;
//                _src.animationType = (DOTweenAnimationType)EditorGUILayout.EnumPopup(_src.animationType, EditorGUIUtils.popupButton);
                GUI.enabled = _src.isActive;
                _src.animationType = AnimationToDOTweenAnimationType(_AnimationType[EditorGUILayout.Popup(DOTweenAnimationTypeToPopupId(_src.animationType), _AnimationType)]);
                _src.autoPlay = DeGUILayout.ToggleButton(_src.autoPlay, new GUIContent("AutoPlay", "If selected, the tween will play automatically"));
                _src.autoKill = DeGUILayout.ToggleButton(_src.autoKill, new GUIContent("AutoKill", "If selected, the tween will be killed when it completes, and won't be reusable"));
                GUILayout.EndHorizontal();
                if (prevAnimType != _src.animationType) {
                    // Set default optional values based on animation type
                    _src.endValueTransform = null;
                    _src.useTargetAsV3 = false;
                    switch (_src.animationType) {
                    case DOTweenAnimationType.Move:
                    case DOTweenAnimationType.LocalMove:
                    case DOTweenAnimationType.Rotate:
                    case DOTweenAnimationType.LocalRotate:
                    case DOTweenAnimationType.Scale:
                        _src.endValueV3 = Vector3.zero;
                        _src.endValueFloat = 0;
                        _src.optionalBool0 = _src.animationType == DOTweenAnimationType.Scale;
                        break;
                    case DOTweenAnimationType.UIWidthHeight:
                        _src.endValueV3 = Vector3.zero;
                        _src.endValueFloat = 0;
                        _src.optionalBool0 = _src.animationType == DOTweenAnimationType.UIWidthHeight;
                        break;
                    case DOTweenAnimationType.Color:
                    case DOTweenAnimationType.Fade:
                        _isLightSrc = targetGO.GetComponent<Light>() != null;
                        _src.endValueFloat = 0;
                        break;
                    case DOTweenAnimationType.Text:
                        _src.optionalBool0 = true;
                        break;
                    case DOTweenAnimationType.PunchPosition:
                    case DOTweenAnimationType.PunchRotation:
                    case DOTweenAnimationType.PunchScale:
                        _src.endValueV3 = _src.animationType == DOTweenAnimationType.PunchRotation ? new Vector3(0, 180, 0) : Vector3.one;
                        _src.optionalFloat0 = 1;
                        _src.optionalInt0 = 10;
                        _src.optionalBool0 = false;
                        break;
                    case DOTweenAnimationType.ShakePosition:
                    case DOTweenAnimationType.ShakeRotation:
                    case DOTweenAnimationType.ShakeScale:
                        _src.endValueV3 = _src.animationType == DOTweenAnimationType.ShakeRotation ? new Vector3(90, 90, 90) : Vector3.one;
                        _src.optionalInt0 = 10;
                        _src.optionalFloat0 = 90;
                        _src.optionalBool0 = false;
                        break;
                    case DOTweenAnimationType.CameraAspect:
                    case DOTweenAnimationType.CameraFieldOfView:
                    case DOTweenAnimationType.CameraOrthoSize:
                        _src.endValueFloat = 0;
                        break;
                    case DOTweenAnimationType.CameraPixelRect:
                    case DOTweenAnimationType.CameraRect:
                        _src.endValueRect = new Rect(0, 0, 0, 0);
                        break;
                    }
                }
                if (_src.animationType == DOTweenAnimationType.None) {
                    _src.isValid = false;
                    if (GUI.changed) EditorUtility.SetDirty(_src);
                    return;
                }

                if (_refreshRequired || prevAnimType != _src.animationType || ComponentsChanged()) {
                    _refreshRequired = false;
                    _src.isValid = Validate(targetGO);
                    // See if we need to choose between multiple targets
#if true // UI_MARKER
                    if (_src.animationType == DOTweenAnimationType.Fade && targetGO.GetComponent<CanvasGroup>() != null && targetGO.GetComponent<Image>() != null) {
                        _chooseTargetMode = ChooseTargetMode.BetweenCanvasGroupAndImage;
                        // Reassign target and forcedTargetType if lost
                        if (_src.forcedTargetType == TargetType.Unset) _src.forcedTargetType = _src.targetType;
                        switch (_src.forcedTargetType) {
                        case TargetType.CanvasGroup:
                            _src.target = targetGO.GetComponent<CanvasGroup>();
                            break;
                        case TargetType.Image:
                            _src.target = targetGO.GetComponent<Image>();
                            break;
                        }
                    } else {
#endif
                        _chooseTargetMode = ChooseTargetMode.None;
                        _src.forcedTargetType = TargetType.Unset;
#if true // UI_MARKER
                    }
#endif
                }

                if (!_src.isValid) {
                    GUI.color = Color.red;
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.Label("No valid Component was found for the selected animation", EditorGUIUtils.wordWrapLabelStyle);
                    GUILayout.EndVertical();
                    GUI.color = Color.white;
                    if (GUI.changed) EditorUtility.SetDirty(_src);
                    return;
                }

#if true // UI_MARKER
                // Special cases in which multiple target types could be used (set after validation)
                if (_chooseTargetMode == ChooseTargetMode.BetweenCanvasGroupAndImage && _src.forcedTargetType != TargetType.Unset) {
                    FadeTargetType fadeTargetType = (FadeTargetType)Enum.Parse(typeof(FadeTargetType), _src.forcedTargetType.ToString());
                    TargetType prevTargetType = _src.forcedTargetType;
                    _src.forcedTargetType = (TargetType)Enum.Parse(typeof(TargetType), EditorGUILayout.EnumPopup(_src.animationType + " Target", fadeTargetType).ToString());
                    if (_src.forcedTargetType != prevTargetType) {
                        // Target type change > assign correct target
                        switch (_src.forcedTargetType) {
                        case TargetType.CanvasGroup:
                            _src.target = targetGO.GetComponent<CanvasGroup>();
                            break;
                        case TargetType.Image:
                            _src.target = targetGO.GetComponent<Image>();
                            break;
                        }
                    }
                }
#endif

                GUILayout.BeginHorizontal();
                _src.duration = EditorGUILayout.FloatField("Duration", _src.duration);
                if (_src.duration < 0) _src.duration = 0;
                _src.isSpeedBased = DeGUILayout.ToggleButton(_src.isSpeedBased, new GUIContent("SpeedBased", "If selected, the duration will count as units/degree x second"), DeGUI.styles.button.tool, GUILayout.Width(75));
                GUILayout.EndHorizontal();
                _src.delay = EditorGUILayout.FloatField("Delay", _src.delay);
                if (_src.delay < 0) _src.delay = 0;
                _src.isIndependentUpdate = EditorGUILayout.Toggle("Ignore TimeScale", _src.isIndependentUpdate);
                _src.easeType = EditorGUIUtils.FilteredEasePopup(_src.easeType);
                if (_src.easeType == Ease.INTERNAL_Custom) {
                    _src.easeCurve = EditorGUILayout.CurveField("   Ease Curve", _src.easeCurve);
                }
                _src.loops = EditorGUILayout.IntField(new GUIContent("Loops", "Set to -1 for infinite loops"), _src.loops);
                if (_src.loops < -1) _src.loops = -1;
                if (_src.loops > 1 || _src.loops == -1)
                    _src.loopType = (LoopType)EditorGUILayout.EnumPopup("   Loop Type", _src.loopType);
                _src.id = EditorGUILayout.TextField("ID", _src.id);

                bool canBeRelative = true;
                // End value and eventual specific options
                switch (_src.animationType) {
                case DOTweenAnimationType.Move:
                case DOTweenAnimationType.LocalMove:
                    GUIEndValueV3(targetGO, _src.animationType == DOTweenAnimationType.Move);
                    _src.optionalBool0 = EditorGUILayout.Toggle("    Snapping", _src.optionalBool0);
                    canBeRelative = !_src.useTargetAsV3;
                    break;
                case DOTweenAnimationType.Rotate:
                case DOTweenAnimationType.LocalRotate:
                    bool isRigidbody2D = DOTweenModuleUtils.Physics.HasRigidbody2D(_src);
                    if (isRigidbody2D) GUIEndValueFloat();
                    else {
                        GUIEndValueV3(targetGO);
                        _src.optionalRotationMode = (RotateMode)EditorGUILayout.EnumPopup("    Rotation Mode", _src.optionalRotationMode);
                    }
                    break;
                case DOTweenAnimationType.Scale:
                    if (_src.optionalBool0) GUIEndValueFloat();
                    else GUIEndValueV3(targetGO);
                    _src.optionalBool0 = EditorGUILayout.Toggle("Uniform Scale", _src.optionalBool0);
                    break;
                case DOTweenAnimationType.UIWidthHeight:
                    if (_src.optionalBool0) GUIEndValueFloat();
                    else GUIEndValueV2();
                    _src.optionalBool0 = EditorGUILayout.Toggle("Uniform Scale", _src.optionalBool0);
                    break;
                case DOTweenAnimationType.Color:
                    GUIEndValueColor();
                    canBeRelative = false;
                    break;
                case DOTweenAnimationType.Fade:
                    GUIEndValueFloat();
                    if (_src.endValueFloat < 0) _src.endValueFloat = 0;
                    if (!_isLightSrc && _src.endValueFloat > 1) _src.endValueFloat = 1;
                    canBeRelative = false;
                    break;
                case DOTweenAnimationType.Text:
                    GUIEndValueString();
                    _src.optionalBool0 = EditorGUILayout.Toggle("Rich Text Enabled", _src.optionalBool0);
                    _src.optionalScrambleMode = (ScrambleMode)EditorGUILayout.EnumPopup("Scramble Mode", _src.optionalScrambleMode);
                    _src.optionalString = EditorGUILayout.TextField(new GUIContent("Custom Scramble", "Custom characters to use in case of ScrambleMode.Custom"), _src.optionalString);
                    break;
                case DOTweenAnimationType.PunchPosition:
                case DOTweenAnimationType.PunchRotation:
                case DOTweenAnimationType.PunchScale:
                    GUIEndValueV3(targetGO);
                    canBeRelative = false;
                    _src.optionalInt0 = EditorGUILayout.IntSlider(new GUIContent("    Vibrato", "How much will the punch vibrate"), _src.optionalInt0, 1, 50);
                    _src.optionalFloat0 = EditorGUILayout.Slider(new GUIContent("    Elasticity", "How much the vector will go beyond the starting position when bouncing backwards"), _src.optionalFloat0, 0, 1);
                    if (_src.animationType == DOTweenAnimationType.PunchPosition) _src.optionalBool0 = EditorGUILayout.Toggle("    Snapping", _src.optionalBool0);
                    break;
                case DOTweenAnimationType.ShakePosition:
                case DOTweenAnimationType.ShakeRotation:
                case DOTweenAnimationType.ShakeScale:
                    GUIEndValueV3(targetGO);
                    canBeRelative = false;
                    _src.optionalInt0 = EditorGUILayout.IntSlider(new GUIContent("    Vibrato", "How much will the shake vibrate"), _src.optionalInt0, 1, 50);
                    _src.optionalFloat0 = EditorGUILayout.Slider(new GUIContent("    Randomness", "The shake randomness"), _src.optionalFloat0, 0, 90);
                    if (_src.animationType == DOTweenAnimationType.ShakePosition) _src.optionalBool0 = EditorGUILayout.Toggle("    Snapping", _src.optionalBool0);
                    break;
                case DOTweenAnimationType.CameraAspect:
                case DOTweenAnimationType.CameraFieldOfView:
                case DOTweenAnimationType.CameraOrthoSize:
                    GUIEndValueFloat();
                    canBeRelative = false;
                    break;
                case DOTweenAnimationType.CameraBackgroundColor:
                    GUIEndValueColor();
                    canBeRelative = false;
                    break;
                case DOTweenAnimationType.CameraPixelRect:
                case DOTweenAnimationType.CameraRect:
                    GUIEndValueRect();
                    canBeRelative = false;
                    break;
                }

                // Final settings
                if (canBeRelative) _src.isRelative = EditorGUILayout.Toggle("    Relative", _src.isRelative);

                // Events
                AnimationInspectorGUI.AnimationEvents(this, _src);
            }

            if (GUI.changed) EditorUtility.SetDirty(_src);
        }

        #endregion

        #region Methods

        // Returns TRUE if the Component layout on the src gameObject changed (a Component was added or removed)
        bool ComponentsChanged()
        {
            int prevTotComponentsOnSrc = _totComponentsOnSrc;
            _totComponentsOnSrc = _src.gameObject.GetComponents<Component>().Length;
            return prevTotComponentsOnSrc != _totComponentsOnSrc;
        }

        // Checks if a Component that can be animated with the given animationType is attached to the src
        bool Validate(GameObject targetGO)
        {
            if (_src.animationType == DOTweenAnimationType.None) return false;

            Component srcTarget;
            // First check for external plugins
#if false // TK2D_MARKER
            if (_Tk2dAnimationTypeToComponent.ContainsKey(_src.animationType)) {
                foreach (Type t in _Tk2dAnimationTypeToComponent[_src.animationType]) {
                    srcTarget = targetGO.GetComponent(t);
                    if (srcTarget != null) {
                        _src.target = srcTarget;
                        _src.targetType = DOTweenAnimation.TypeToDOTargetType(t);
                        return true;
                    }
                }
            }
#endif
#if false // TEXTMESHPRO_MARKER
            if (_TMPAnimationTypeToComponent.ContainsKey(_src.animationType)) {
                foreach (Type t in _TMPAnimationTypeToComponent[_src.animationType]) {
                    srcTarget = targetGO.GetComponent(t);
                    if (srcTarget != null) {
                        _src.target = srcTarget;
                        _src.targetType = DOTweenAnimation.TypeToDOTargetType(t);
                        return true;
                    }
                }
            }
#endif
            // Then check for regular stuff
            if (_AnimationTypeToComponent.ContainsKey(_src.animationType)) {
                foreach (Type t in _AnimationTypeToComponent[_src.animationType]) {
                    srcTarget = targetGO.GetComponent(t);
                    if (srcTarget != null) {
                        _src.target = srcTarget;
                        _src.targetType = DOTweenAnimation.TypeToDOTargetType(t);
                        return true;
                    }
                }
            }
            return false;
        }

        DOTweenAnimationType AnimationToDOTweenAnimationType(string animation)
        {
            if (_datString == null) _datString = Enum.GetNames(typeof(DOTweenAnimationType));
            animation = animation.Replace("/", "");
            return (DOTweenAnimationType)(Array.IndexOf(_datString, animation));
        }
        int DOTweenAnimationTypeToPopupId(DOTweenAnimationType animation)
        {
            return Array.IndexOf(_animationTypeNoSlashes, animation.ToString());
        }

        #endregion

        #region GUI Draw Methods

        void GUIEndValueFloat()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            _src.endValueFloat = EditorGUILayout.FloatField(_src.endValueFloat);
            GUILayout.EndHorizontal();
        }

        void GUIEndValueColor()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            _src.endValueColor = EditorGUILayout.ColorField(_src.endValueColor);
            GUILayout.EndHorizontal();
        }

        void GUIEndValueV3(GameObject targetGO, bool optionalTransform = false)
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            if (_src.useTargetAsV3) {
                Transform prevT = _src.endValueTransform;
                _src.endValueTransform = EditorGUILayout.ObjectField(_src.endValueTransform, typeof(Transform), true) as Transform;
                if (_src.endValueTransform != prevT && _src.endValueTransform != null) {
#if true // UI_MARKER
                    // Check that it's a Transform for a Transform or a RectTransform for a RectTransform
                    if (targetGO.GetComponent<RectTransform>() != null) {
                        if (_src.endValueTransform.GetComponent<RectTransform>() == null) {
                            EditorUtility.DisplayDialog("DOTween Pro", "For Unity UI elements, the target must also be a UI element", "Ok");
                            _src.endValueTransform = null;
                        }
                    } else if (_src.endValueTransform.GetComponent<RectTransform>() != null) {
                        EditorUtility.DisplayDialog("DOTween Pro", "You can't use a UI target for a non UI object", "Ok");
                        _src.endValueTransform = null;
                    }
#endif
                }
            } else {
                _src.endValueV3 = EditorGUILayout.Vector3Field("", _src.endValueV3, GUILayout.Height(16));
            }
            if (optionalTransform) {
                if (GUILayout.Button(_src.useTargetAsV3 ? "target" : "value", EditorGUIUtils.sideBtStyle, GUILayout.Width(44))) _src.useTargetAsV3 = !_src.useTargetAsV3;
            }
            GUILayout.EndHorizontal();
#if true // UI_MARKER
            if (_src.useTargetAsV3 && _src.endValueTransform != null && _src.target is RectTransform) {
                EditorGUILayout.HelpBox("NOTE: when using a UI target, the tween will be created during Start instead of Awake", MessageType.Info);
            }
#endif
        }

        void GUIEndValueV2()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            _src.endValueV2 = EditorGUILayout.Vector2Field("", _src.endValueV2, GUILayout.Height(16));
            GUILayout.EndHorizontal();
        }

        void GUIEndValueString()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            _src.endValueString = EditorGUILayout.TextArea(_src.endValueString, EditorGUIUtils.wordWrapTextArea);
            GUILayout.EndHorizontal();
        }

        void GUIEndValueRect()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            _src.endValueRect = EditorGUILayout.RectField(_src.endValueRect);
            GUILayout.EndHorizontal();
        }

        void GUIToFromButton()
        {
            if (GUILayout.Button(_src.isFrom ? "FROM" : "TO", EditorGUIUtils.sideBtStyle, GUILayout.Width(90))) _src.isFrom = !_src.isFrom;
            GUILayout.Space(16);
        }

        #endregion
    }
}
