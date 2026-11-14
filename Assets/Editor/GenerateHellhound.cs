
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Animations;

public static class GenerateHellhound
{
    private const string Root =
        "Assets/Art/Characters/Hellhound/";

    private const string Sheets = Root + "Sheets/";
    private const string Output = Root + "Generated/";

    private static readonly string[] Directions =
    {
        "S", "SE", "E", "NE",
        "N", "NW", "W", "SW"
    };

    private struct AnimationData
    {
        public string file;
        public string action;
        public float fps;
        public bool loop;

        public AnimationData(
            string file,
            string action,
            float fps,
            bool loop)
        {
            this.file = file;
            this.action = action;
            this.fps = fps;
            this.loop = loop;
        }
    }

    [MenuItem("Tools/Dead Zone/Generate Hellhound")]
    public static void Generate()
    {
        AnimationData[] animations =
        {
            new AnimationData(
                "run_smooth", "Run", 16f, true),

            new AnimationData(
                "attack2_smooth", "Attack", 16f, false),

            new AnimationData(
                "hurt", "Hurt", 12f, false),

            new AnimationData(
                "death", "Death", 12f, false)
        };

        if (!AssetDatabase.IsValidFolder(Output))
        {
            AssetDatabase.CreateFolder(
                Root.TrimEnd('/'),
                "Generated"
            );
        }

        string controllerPath =
            Output + "Hellhound.controller";

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(
                controllerPath
            );

        if (controller == null)
        {
            controller =
                AnimatorController.CreateAnimatorControllerAtPath(
                    controllerPath
                );
        }

        var machine =
            controller.layers[0].stateMachine;

        int total = 0;

        foreach (AnimationData anim in animations)
        {
            string path =
                Sheets + anim.file + ".png";

            TextureImporter importer =
                AssetImporter.GetAtPath(path)
                as TextureImporter;

            if (importer == null)
            {
                Debug.LogError(
                    "Missing Hellhound texture: " + path
                );
                return;
            }

            importer.textureType =
                TextureImporterType.Sprite;

            importer.spriteImportMode =
                SpriteImportMode.Multiple;

            importer.spritePixelsPerUnit = 100;
            importer.maxTextureSize = 8192;

            importer.textureCompression =
                TextureImporterCompression.Uncompressed;

            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;

            importer.SaveAndReimport();

            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    path
                );

            if (texture == null)
            {
                Debug.LogError(
                    "Cannot load texture: " + path
                );
                return;
            }

            // Native HD frames are 244 x 220.
            // Support the 128 x 128 variant too.
            int cellWidth;
            int cellHeight;

            if (texture.height == 8 * 220)
            {
                cellWidth = 244;
                cellHeight = 220;
            }
            else if (texture.height == 8 * 128)
            {
                cellWidth = 128;
                cellHeight = 128;
            }
            else
            {
                Debug.LogError(
                    anim.file +
                    ": Unexpected image size " +
                    texture.width + "x" +
                    texture.height +
                    ". Check the original sheet."
                );
                return;
            }

            if (texture.width % cellWidth != 0)
            {
                Debug.LogError(
                    anim.file +
                    ": Width is not divisible by " +
                    cellWidth
                );
                return;
            }

            int columns =
                texture.width / cellWidth;

            // Set sprite rectangles automatically
            var factory =
                new SpriteDataProviderFactories();

            factory.Init();

            var provider =
                factory.GetSpriteEditorDataProviderFromObject(
                    importer
                );

            provider.InitSpriteEditorDataProvider();

            var existing =
                provider.GetSpriteRects()
                .ToDictionary(
                    r => r.name,
                    r => r.spriteID
                );

            List<SpriteRect> rectangles =
                new List<SpriteRect>();

            List<SpriteNameFileIdPair> pairs =
                new List<SpriteNameFileIdPair>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    string name =
                        anim.file + "_" +
                        (row * columns + col);

                    GUID id =
                        existing.ContainsKey(name)
                        ? existing[name]
                        : GUID.Generate();

                    SpriteRect rect =
                        new SpriteRect
                        {
                            name = name,
                            spriteID = id,

                            rect = new Rect(
                                col * cellWidth,
                                texture.height -
                                (row + 1) * cellHeight,
                                cellWidth,
                                cellHeight
                            ),

                            alignment =
                                SpriteAlignment.Center,

                            pivot =
                                new Vector2(0.5f, 0.5f)
                        };

                    rectangles.Add(rect);

                    pairs.Add(
                        new SpriteNameFileIdPair(
                            name, id
                        )
                    );
                }
            }

            provider.SetSpriteRects(
                rectangles.ToArray()
            );

            var idProvider =
                provider.GetDataProvider<
                    ISpriteNameFileIdDataProvider>();

            if (idProvider != null)
            {
                idProvider.SetNameFileIdPairs(pairs);
            }

            provider.Apply();
            importer.SaveAndReimport();

            Sprite[] sprites =
                AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();

            if (sprites.Length != 8 * columns)
            {
                Debug.LogError(
                    "Invalid sprite count: " +
                    anim.file
                );
                return;
            }

            // Generate animation for every direction
            for (int row = 0; row < 8; row++)
            {
                string stateName =
                    "Hellhound_" + anim.action +
                    "_" + Directions[row];

                string clipPath =
                    Output + stateName + ".anim";

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

                clip.frameRate = anim.fps;

                ObjectReferenceKeyframe[] frames =
                    new ObjectReferenceKeyframe[columns];

                for (int col = 0; col < columns; col++)
                {
                    frames[col] =
                        new ObjectReferenceKeyframe
                        {
                            time = col / anim.fps,

                            value = sprites[
                                row * columns + col
                            ]
                        };
                }

                var binding =
                    EditorCurveBinding.PPtrCurve(
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
                    AnimationUtility.GetAnimationClipSettings(
                        clip
                    );

                settings.loopTime = anim.loop;

                AnimationUtility.SetAnimationClipSettings(
                    clip,
                    settings
                );

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
                {
                    state = machine.AddState(stateName);
                }

                state.motion = clip;

                if (stateName == "Hellhound_Run_S")
                {
                    machine.defaultState = state;
                }

                EditorUtility.SetDirty(clip);

                total++;
            }
        }

        EditorUtility.SetDirty(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "SUCCESS: Generated " + total +
            " Hellhound animations!"
        );
    }
}
