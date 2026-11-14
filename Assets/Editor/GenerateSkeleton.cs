
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Animations;

public static class GenerateSkeleton
{
    private const string Root =
        "Assets/Art/Characters/Skeleton/";

    private const string Sheets = Root + "Sheets/";
    private const string Output = Root + "Generated/";

    private static readonly string[] Directions =
    {
        "S", "SE", "E", "NE",
        "N", "NW", "W", "SW"
    };

    // Filename, number of columns, frames per second, loop
    private struct AnimationData
    {
        public string file;
        public string action;
        public int columns;
        public float fps;
        public bool loop;

        public AnimationData(
            string f, string a, int c, float speed, bool l)
        {
            file = f;
            action = a;
            columns = c;
            fps = speed;
            loop = l;
        }
    }

    [MenuItem("Tools/Dead Zone/Generate Skeleton")]
    public static void Generate()
    {
        var animations = new AnimationData[]
        {
            new AnimationData(
                "idle_smooth", "Idle", 12, 12f, true),

            new AnimationData(
                "walk_smooth", "Walk", 12, 12f, true),

            new AnimationData(
                "attack_smooth", "Attack", 12, 12f, false),

            new AnimationData(
                "hurt_smooth", "Hurt", 12, 18f, false),

            new AnimationData(
                "death", "Death", 6, 10f, false)
        };

        if (!AssetDatabase.IsValidFolder(Output))
        {
            AssetDatabase.CreateFolder(
                Root.TrimEnd('/'), "Generated");
        }

        string controllerPath =
            Output + "Skeleton.controller";

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(
                controllerPath);

        if (controller == null)
        {
            controller =
                AnimatorController.CreateAnimatorControllerAtPath(
                    controllerPath);
        }

        var machine = controller.layers[0].stateMachine;

        int total = 0;

        foreach (var anim in animations)
        {
            string path = Sheets + anim.file + ".png";

            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError("Missing texture: " + path);
                return;
            }

            importer.textureType =
                TextureImporterType.Sprite;

            importer.spriteImportMode =
                SpriteImportMode.Multiple;

            importer.spritePixelsPerUnit = 100;
            importer.maxTextureSize = 4096;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;

            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;

            importer.SaveAndReimport();

            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (texture == null)
            {
                Debug.LogError("Cannot load: " + path);
                return;
            }

            // Automatically slice the 8-direction sheet
            var factory = new SpriteDataProviderFactories();
            factory.Init();

            var provider =
                factory.GetSpriteEditorDataProviderFromObject(
                    importer);

            provider.InitSpriteEditorDataProvider();

            var existing = provider.GetSpriteRects()
                .ToDictionary(r => r.name, r => r.spriteID);

            int rows = 8;
            float cellWidth =
                (float)texture.width / anim.columns;

            float cellHeight =
                (float)texture.height / rows;

            List<SpriteRect> rectangles =
                new List<SpriteRect>();

            List<SpriteNameFileIdPair> pairs =
                new List<SpriteNameFileIdPair>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < anim.columns; col++)
                {
                    string name = anim.file + "_" +
                        (row * anim.columns + col);

                    GUID id = existing.ContainsKey(name)
                        ? existing[name]
                        : GUID.Generate();

                    SpriteRect rect = new SpriteRect
                    {
                        name = name,
                        spriteID = id,
                        rect = new Rect(
                            col * cellWidth,
                            texture.height - (row + 1) * cellHeight,
                            cellWidth,
                            cellHeight),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };

                    rectangles.Add(rect);

                    pairs.Add(
                        new SpriteNameFileIdPair(name, id));
                }
            }

            provider.SetSpriteRects(rectangles.ToArray());

            var idProvider =
                provider.GetDataProvider<
                    ISpriteNameFileIdDataProvider>();

            if (idProvider != null)
                idProvider.SetNameFileIdPairs(pairs);

            provider.Apply();
            importer.SaveAndReimport();

            // Load sprites in top-to-bottom row order
            Sprite[] sprites =
                AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Sprite>()
                    .OrderByDescending(s => s.rect.y)
                    .ThenBy(s => s.rect.x)
                    .ToArray();

            int expected = 8 * anim.columns;

            if (sprites.Length != expected)
            {
                Debug.LogError(
                    anim.file + ": expected " + expected +
                    " sprites, found " + sprites.Length);
                return;
            }

            // Generate one animation clip per direction
            for (int row = 0; row < 8; row++)
            {
                string stateName =
                    "Skeleton_" + anim.action + "_" +
                    Directions[row];

                string clipPath =
                    Output + stateName + ".anim";

                AnimationClip clip =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(
                        clipPath);

                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, clipPath);
                }

                clip.frameRate = anim.fps;

                var frames =
                    new ObjectReferenceKeyframe[anim.columns];

                for (int col = 0; col < anim.columns; col++)
                {
                    frames[col] = new ObjectReferenceKeyframe
                    {
                        time = col / anim.fps,
                        value = sprites[row * anim.columns + col]
                    };
                }

                var binding = EditorCurveBinding.PPtrCurve(
                    "",
                    typeof(SpriteRenderer),
                    "m_Sprite");

                AnimationUtility.SetObjectReferenceCurve(
                    clip, binding, frames);

                var settings =
                    AnimationUtility.GetAnimationClipSettings(clip);

                settings.loopTime = anim.loop;

                AnimationUtility.SetAnimationClipSettings(
                    clip, settings);

                AnimatorState state = null;

                foreach (var child in machine.states)
                {
                    if (child.state.name == stateName)
                    {
                        state = child.state;
                        break;
                    }
                }

                if (state == null)
                    state = machine.AddState(stateName);

                state.motion = clip;

                if (stateName == "Skeleton_Idle_S")
                    machine.defaultState = state;

                EditorUtility.SetDirty(clip);
                total++;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "SUCCESS: Generated " + total +
            " Skeleton animations!");
    }
}
