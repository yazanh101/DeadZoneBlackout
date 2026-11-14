
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class GenerateSoldierDeath
{
    private const string BasePath =
        "Assets/Art/Characters/Soldier/";

    private static readonly string[] Directions =
    {
        "S", "SE", "E", "NE",
        "N", "NW", "W", "SW"
    };

    [MenuItem("Tools/Dead Zone/Generate Death Animations")]
    public static void Generate()
    {
        string imagePath = BasePath + "death.png";

        Sprite[] sprites = AssetDatabase
            .LoadAllAssetsAtPath(imagePath)
            .OfType<Sprite>()
            .OrderByDescending(s => s.rect.y)
            .ThenBy(s => s.rect.x)
            .ToArray();

        if (sprites.Length != 96)
        {
            Debug.LogError(
                "Expected 96 sprites, found " +
                sprites.Length
            );
            return;
        }

        string controllerPath =
            BasePath + "Walk_S/SoldierVisual.controller";

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(
                controllerPath
            );

        if (controller == null)
        {
            Debug.LogError(
                "Soldier Animator Controller not found!"
            );
            return;
        }

        string outputFolder =
            BasePath + "Death_Generated";

        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            AssetDatabase.CreateFolder(
                BasePath.TrimEnd('/'),
                "Death_Generated"
            );
        }

        var stateMachine =
            controller.layers[0].stateMachine;

        for (int row = 0; row < 8; row++)
        {
            string stateName =
                "Soldier_Death_" + Directions[row];

            string clipPath =
                outputFolder + "/" + stateName + ".anim";

            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    clipPath
                );

            if (clip == null)
            {
                clip = new AnimationClip();

                AssetDatabase.CreateAsset(
                    clip,
                    clipPath
                );
            }

            // 12 frames at 12 FPS
            clip.frameRate = 12f;

            ObjectReferenceKeyframe[] frames =
                new ObjectReferenceKeyframe[12];

            for (int col = 0; col < 12; col++)
            {
                frames[col] = new ObjectReferenceKeyframe
                {
                    time = col / 12f,
                    value = sprites[row * 12 + col]
                };
            }

            var binding = EditorCurveBinding.PPtrCurve(
                "",
                typeof(SpriteRenderer),
                "m_Sprite"
            );

            AnimationUtility.SetObjectReferenceCurve(
                clip,
                binding,
                frames
            );

            // Death animation must not repeat
            var settings =
                AnimationUtility.GetAnimationClipSettings(clip);

            settings.loopTime = false;

            AnimationUtility.SetAnimationClipSettings(
                clip,
                settings
            );

            AnimatorState state = null;

            foreach (var child in stateMachine.states)
            {
                if (child.state.name == stateName)
                {
                    state = child.state;
                    break;
                }
            }

            if (state == null)
            {
                state = stateMachine.AddState(stateName);
            }

            state.motion = clip;

            EditorUtility.SetDirty(clip);
            EditorUtility.SetDirty(state);
        }

        EditorUtility.SetDirty(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "SUCCESS: 8 Soldier Death animations generated!"
        );
    }
}
