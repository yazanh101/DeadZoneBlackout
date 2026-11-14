
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class GenerateSoldierHurt
{
    private const string BasePath =
        "Assets/Art/Characters/Soldier/";

    private static readonly string[] Directions =
    {
        "S", "SE", "E", "NE",
        "N", "NW", "W", "SW"
    };

    [MenuItem("Tools/Dead Zone/Generate Hurt Animations")]
    public static void Generate()
    {
        string imagePath = BasePath + "hurt_smooth.png";

        // Load all sprites from the sprite sheet
        Sprite[] sprites = AssetDatabase
            .LoadAllAssetsAtPath(imagePath)
            .OfType<Sprite>()
            .OrderByDescending(s => s.rect.y)
            .ThenBy(s => s.rect.x)
            .ToArray();

        if (sprites.Length != 176)
        {
            Debug.LogError(
                "Expected 176 sprites, found " +
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

        string outputFolder = BasePath + "Hurt_Generated";

        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            AssetDatabase.CreateFolder(
                BasePath.TrimEnd('/'),
                "Hurt_Generated"
            );
        }

        var stateMachine =
            controller.layers[0].stateMachine;

        // Generate one animation for each direction
        for (int row = 0; row < 8; row++)
        {
            string stateName =
                "Soldier_Hurt_" + Directions[row];

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

            // Approximately 0.6 seconds per hit
            clip.frameRate = 36f;

            ObjectReferenceKeyframe[] frames =
                new ObjectReferenceKeyframe[22];

            for (int col = 0; col < 22; col++)
            {
                int spriteIndex = row * 22 + col;

                frames[col] = new ObjectReferenceKeyframe
                {
                    time = col / 36f,
                    value = sprites[spriteIndex]
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

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "SUCCESS: 8 Soldier Hurt animations generated!"
        );
    }
}
