
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class GenerateSoldierReload
{
    private const string BasePath =
        "Assets/Art/Characters/Soldier/";

    private static readonly string[] Directions =
    {
        "S", "SE", "E", "NE",
        "N", "NW", "W", "SW"
    };

    [MenuItem("Tools/Dead Zone/Generate Reload Animations")]
    public static void Generate()
    {
        string controllerPath =
            BasePath + "Walk_S/SoldierVisual.controller";

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(
                controllerPath
            );

        if (controller == null)
        {
            Debug.LogError("Soldier Animator Controller not found!");
            return;
        }

        string reloadFolder = BasePath + "Reload_Generated";

        if (!AssetDatabase.IsValidFolder(reloadFolder))
        {
            AssetDatabase.CreateFolder(
                BasePath.TrimEnd('/'),
                "Reload_Generated"
            );
        }

        var stateMachine = controller.layers[0].stateMachine;

        foreach (string dir in Directions)
        {
            Sprite shoot = LoadSprite("Shoot_" + dir, "000");
            Sprite idle0 = LoadSprite("Idle_" + dir, "000");
            Sprite idle4 = LoadSprite("Idle_" + dir, "004");
            Sprite idle8 = LoadSprite("Idle_" + dir, "008");

            if (shoot == null || idle0 == null ||
                idle4 == null || idle8 == null)
            {
                Debug.LogWarning("Missing sprites for " + dir);
                continue;
            }

            string stateName = "Soldier_Reload_" + dir;
            string clipPath = reloadFolder + "/" +
                              stateName + ".anim";

            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    clipPath
                );

            if (clip == null)
            {
                clip = new AnimationClip();
                clip.frameRate = 12;
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            ObjectReferenceKeyframe[] frames =
            {
                Frame(0.00f, shoot),
                Frame(0.18f, idle0),
                Frame(0.40f, idle4),
                Frame(0.85f, idle8),
                Frame(1.15f, idle0),
                Frame(1.35f, shoot),
                Frame(1.50f, shoot)
            };

            var binding = EditorCurveBinding.PPtrCurve(
                "",
                typeof(SpriteRenderer),
                "m_Sprite"
            );

            AnimationUtility.SetObjectReferenceCurve(
                clip, binding, frames
            );

            var settings =
                AnimationUtility.GetAnimationClipSettings(clip);

            settings.loopTime = false;

            AnimationUtility.SetAnimationClipSettings(
                clip, settings
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

        Debug.Log("8 Reload animations generated!");
    }

    private static Sprite LoadSprite(
        string folder, string frame)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(
            BasePath + folder + "/" + frame + ".png"
        );
    }

    private static ObjectReferenceKeyframe Frame(
        float time, Sprite sprite)
    {
        return new ObjectReferenceKeyframe
        {
            time = time,
            value = sprite
        };
    }
}
