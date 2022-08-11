//  Copyright (c) 2020-present amlovey
//  
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Video;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;

namespace APlus2
{
    public class AssetsHelper
    {
        public static List<APAsset> GetTextures()
        {
            return GetAssetsListByType<APAsset>("texture", GetAPTextureFromAssetGuid);
        }

        public static List<APAsset> GetModels()
        {
            return GetAssetsListByType<APAsset>("model", GetAPModelFromAssetGuid);
        }

        public static List<APAsset> GetPrefabs()
        {
            return GetAssetsListByType<APAsset>("prefab", GetAPPrefabFromAssetGuid);
        }

        public static List<APAsset> GetMaterials()
        {
            List<APAsset> materials = new List<APAsset>();
            materials.AddRange(GetAssetsListByType<APMaterial>("material", GetAPMaterialFromAssetGuid));
            materials.AddRange(GetAssetsListByType<APMaterial>("PhysicMaterial", GetAPMaterialFromAssetGuid));
            materials.AddRange(GetAssetsListByType<APMaterial>("PhysicsMaterial2D", GetAPMaterialFromAssetGuid));
            return materials;
        }

        public static string[] GetAssetGuidsByType(string type)
        {
            return AssetDatabase.FindAssets(string.Format("t:{0}", type));
        }

        public static List<T> GetAssetsListByType<T>(string type, Func<string, T> parseFunction) where T : APAsset
        {
            var guids = GetAssetGuidsByType(type);
            List<T> list = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                T obj = parseFunction(guids[i]);
                if (obj != null)
                {
                    list.Add(obj);
                }
            }

            return list;
        }

        private static T GetAssetImporterFromAssetGuid<T>(string guid) where T : class
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetImporter.GetAtPath(path) as T;
        }

        public static List<APAsset> GetCodeFiles()
        {
            string[] filesExtensions = Utilities.GetCodeFileExtensions().Select(ext => "*" + ext).ToArray();
            List<APAsset> codeFiles = new List<APAsset>();
            foreach (var extension in filesExtensions)
            {
                var files = Utilities.GetFilesInRelativePath(Application.dataPath, extension);
                if (files == null)
                {
                    continue;
                }

                foreach (var file in files)
                {
                    string filePath = "Assets" + file;
                    codeFiles.Add(GetCodeFile(filePath));
                }
            }

            return codeFiles;
        }

        public static APCodeFile GetCodeFile(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);

            APCodeFile codeFile = new APCodeFile();
            codeFile.Path = AssetDatabase.GUIDToAssetPath(guid);
            codeFile.Hash = Utilities.GetFileMd5(path);
            codeFile.Name = Utilities.GetFileName(path);
            codeFile.FileSize = Utilities.GetFileSize(path);
            codeFile.FileType = Utilities.GetFileExtension(path);
            codeFile.Id = guid;

            if (codeFile.FileType.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            {
                var tags = GetTagsInCode(File.ReadAllText(path)).ToArray();
                codeFile.ContainTags = string.Join(",", tags);
            }

            return codeFile;
        }

        private static HashSet<string> GetTagsInCode(string code)
        {
            HashSet<string> set = new HashSet<string>();
            string pattern = "(CompareTag|FindGameObjectsWithTag|FindGameObjectWithTag)\\(\"(?<TAG>\\S+?)\"\\)|.tag\\s*(==|!=)\\s*\"(?<TAG>\\S+?)\"";
            var matches = Regex.Matches(code, pattern);

            if (matches.Count <= 0)
            {
                return set;
            }

            foreach (Match match in matches)
            {
                var tag = match.Groups["TAG"].Value;
                if (!string.IsNullOrEmpty(tag))
                {
                    set.Add(tag);
                }
            }

            return set;
        }

        public static List<APAsset> GetShaders()
        {
            return GetAssetsListByType<APAsset>("shader", GetAPShaderFromAssetGuid);
        }

        public static APShader GetAPShaderFromAssetGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Shader shader = AssetDatabase.LoadAssetAtPath(path, typeof(Shader)) as Shader;
            if (shader == null)
            {
                return null;
            }

            APShader apShader = new APShader();
            apShader.Path = path;
            apShader.Hash = Utilities.GetFileMd5(path);
            apShader.FileSize = Utilities.GetFileSize(path);
            apShader.FileName = Utilities.GetFileName(path);
            apShader.CastShadows = ShaderUtils.HasShadowCasterPass(shader);
            apShader.DisableBatching = ShaderUtils.GetDisableBatching(shader);
            apShader.SurfaceShader = ShaderUtils.HasSurfaceShaders(shader);
            apShader.IgnoreProjector = ShaderUtils.DoesIgnoreProjector(shader);
            apShader.LOD = ShaderUtils.GetLOD(shader);
            apShader.RenderQueue = ShaderUtils.GetRenderQueue(shader);
            apShader.RenderQueueText = Utilities.GetShaderRenderQueueText(apShader.RenderQueue);
            apShader.VariantsIncluded = ShaderUtils.GetVariantsCount(shader, true);
            apShader.Name = shader.name;
            apShader.VariantsTotal = ShaderUtils.GetVariantsCount(shader, false);
            apShader.Id = guid;
            apShader.FileType = Utilities.GetFileExtension(path);

            UnloadAsset(shader);
            return apShader;
        }

        public static List<APAsset> GetVideos()
        {
            return GetAssetsListByType<APAsset>("VideoClip", GetAPVideoFromAssetGuid);
        }

        public static APVideo GetAPVideoFromAssetGuid(string guid)
        {
            VideoClipImporter movieImporter = GetAssetImporterFromAssetGuid<VideoClipImporter>(guid);
            
            // if texture is render texture or others, tImporter will to set to null.
            //
            if (movieImporter == null)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var texture = AssetDatabase.LoadAssetAtPath(path, typeof(VideoClip)) as VideoClip;
            if (texture == null)
            {
                return null;
            }

            APVideo video = new APVideo();
            video.Size = (long)movieImporter.outputFileSize;
            video.Name = Utilities.GetFileName(path);
            video.Path = path;
            video.Hash = Utilities.GetFileMd5(path);
            video.FileSize = Utilities.GetFileSize(path);
            video.Duration = texture.length;
            video.Id = guid;
            video.FileType = Utilities.GetFileExtension(path);

            UnloadAsset(texture);
            return video;
        }

        public static List<APAsset> GetAudios()
        {
            return GetAssetsListByType<APAsset>("AudioClip", GetAPAudioFromAssetGuid);
        }

        public static APAudio GetAPAudioFromAssetGuid(string guid)
        {
            AudioImporter audioImporter = GetAssetImporterFromAssetGuid<AudioImporter>(guid);
            if (audioImporter == null)
            {
                return null;
            }

            APAudio audio = new APAudio();
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;

            if (audioClip == null)
            {
                return null;
            }

            audio.Name = Utilities.GetFileName(path);
            audio.FileSize = Utilities.GetFileSize(path);
            audio.FileType = Utilities.GetFileExtension(path);
            audio.Background = audioImporter.loadInBackground;

            string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;

            if (audioImporter.ContainsSampleSettingsOverride(platform))
            {
                settings = audioImporter.GetOverrideSampleSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
            }

            Type t = typeof(AudioImporter);
            var comSizeProperty = t.GetProperty("compSize", BindingFlags.NonPublic | BindingFlags.Instance);
            if (comSizeProperty != null)
            {
                audio.ImportedSize = long.Parse(comSizeProperty.GetValue(audioImporter, null).ToString());
            }

            audio.Path = path;
            audio.Hash = Utilities.GetFileMd5(path);
            audio.Ratio = audio.ImportedSize * 100f / audio.FileSize;
            audio.Quality = settings.quality;
            audio.Compress = settings.compressionFormat;
            audio.Duration = audioClip.length;
            audio.Frequency = AudioUtil.GetFrequency(audioClip);
            audio.Id = guid;

            UnloadAsset(audioClip);
            return audio;
        }

        public static List<APAsset> GetStreamingAssets()
        {
            var files = Utilities.GetFilesInRelativePath(Application.streamingAssetsPath);
            List<APAsset> apfiles = new List<APAsset>();

            if (files == null)
            {
                return apfiles;
            }

            foreach (var item in files)
            {
                string folder = System.IO.Path.Combine("Assets", "StreamingAssets");
                string itemPath = folder + item;
                apfiles.Add(GetAPFile(itemPath));
            }

            return apfiles;
        }

        public static List<APAsset> GetOthers()
        {
            List<APAsset> files = new List<APAsset>();
            var allfiles = Utilities.GetFilesInRelativePath(Application.dataPath);

            foreach (var file in allfiles)
            {
                string filePath = "Assets" + file;
                var guid = AssetDatabase.AssetPathToGUID(filePath);
                if (CacheManager.cache.HasAsset(guid))
                {
                    continue;
                }

                files.Add(GetAPFile(filePath));
            }

            return files;
        }

        public static APFile GetAPFile(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);

            APFile file = new APFile();
            file.Path = AssetDatabase.GUIDToAssetPath(guid);
            file.Hash = Utilities.GetFileMd5(path);
            file.Name = Utilities.GetFileName(path);
            file.FileSize = Utilities.GetFileSize(path);
            file.FileType = Utilities.GetFileExtension(path);
            file.Id = guid;

            return file;
        }

        public static List<APAsset> GetAnimations()
        {
            string[] guids = GetAssetGuidsByType("AnimationClip");

            HashSet<string> uniqueGuids = new HashSet<string>();
            foreach (var guid in guids)
            {
                uniqueGuids.Add(guid);
            }

            List<APAsset> list = new List<APAsset>();
            foreach (var guid in uniqueGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Utilities.IsUntyNewAnimation(path))
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    APAnimation apClip = GetAPAnimationFromClip(clip);
                    apClip.Path = path;
                    apClip.FileType = Utilities.GetFileExtension(path);
                    apClip.Hash = Utilities.GetFileMd5(path);
                    apClip.FileSize = Utilities.GetFileSize(path);
                    apClip.InFile = Utilities.GetFileName(path);
                    apClip.Id = guid;

                    UnloadAsset(clip);
                    list.Add(apClip);
                }
                else
                {
                    foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
                    {
                        if (obj is AnimationClip)
                        {
                            if (IsPreviewAnimation(obj as AnimationClip))
                            {
                                UnloadAsset(obj);
                                continue;
                            }

                            APAnimation apClip = GetAPAnimationFromClip(obj as AnimationClip);
                            apClip.Path = path;
                            apClip.FileType = Utilities.GetFileExtension(path);
                            apClip.Hash = Utilities.GetFileMd5(path);
                            apClip.InFile = Utilities.GetFileName(path);
                            apClip.FileSize = 0;
                            apClip.Id = Utilities.GetAssetId(guid, Utilities.GetLocalIndentifierOfObject(obj).ToString());

                            UnloadAsset(obj);
                            list.Add(apClip);
                        }
                    }
                }
            }

            return list;
        }

        public static List<string> GetAnimationClipAssetIdInModel(string modelAssetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(modelAssetPath);

            List<string> ids = new List<string>();
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(modelAssetPath))
            {
                if (obj is AnimationClip)
                {
                    if (IsPreviewAnimation(obj as AnimationClip))
                    {
                        UnloadAsset(obj);
                        continue;
                    }

                    ids.Add(Utilities.GetAssetId(guid, Utilities.GetLocalIndentifierOfObject(obj).ToString()));
                }

                UnloadAsset(obj);
            }

            return ids;
        }

        private static bool IsPreviewAnimation(AnimationClip clip)
        {
            return clip.name.IndexOf("__preview__") != -1;
        }

        public static APAnimation GetAPAnimationFromAssetPath(string guid, string fileId)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            if (Utilities.IsUntyNewAnimation(path))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                APAnimation apClip = GetAPAnimationFromClip(clip);
                apClip.Path = path;
                apClip.FileType = Utilities.GetFileExtension(path);
                apClip.Hash = Utilities.GetFileMd5(path);
                apClip.InFile = Utilities.GetFileName(path);
                apClip.Id = guid;

                UnloadAsset(clip);
                return apClip;
            }
            else
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return null;
                }


                var objects = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var obj in objects)
                {
                    if ((obj is AnimationClip) && Utilities.GetLocalIndentifierOfObject(obj).ToString() == fileId)
                    {
                        APAnimation apClip = GetAPAnimationFromClip(obj as AnimationClip);
                        apClip.Path = path;
                        apClip.FileType = Utilities.GetFileExtension(path);
                        apClip.Hash = Utilities.GetFileMd5(path);
                        apClip.InFile = Utilities.GetFileName(path);
                        apClip.Id = Utilities.GetAssetId(guid, fileId);

                        foreach (var item in objects)
                        {
                            UnloadAsset(item);
                        }

                        return apClip;
                    }
                }

                foreach (var item in objects)
                {
                    UnloadAsset(item);
                }
            }

            return null;
        }

        public static APAnimation GetAPAnimationFromClip(AnimationClip clip)
        {
            APAnimation animation = new APAnimation();
            AnimationClipSettings setting = AnimationUtility.GetAnimationClipSettings(clip);
            animation.Name = clip.name;
            animation.CycleOffset = setting.cycleOffset;
            animation.LoopPose = setting.loopBlend;
            animation.LoopTime = setting.loopTime;
            animation.FPS = Mathf.RoundToInt(clip.frameRate);
            animation.Length = clip.length;

            return animation;
        }

        public static APModel GetAPModelFromAssetGuid(string guid)
        {
            ModelImporter modelImpoter = GetAssetImporterFromAssetGuid<ModelImporter>(guid);
            if (modelImpoter == null)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            APModel model = new APModel();
            model.Name = Utilities.GetFileName(path);
            model.FileSize = Utilities.GetFileSize(path);
            model.FileType = Utilities.GetFileExtension(path);
            model.MeshCompression = modelImpoter.meshCompression;
            model.OptimizeMesh = modelImpoter.optimizeMeshVertices || modelImpoter.optimizeMeshPolygons;
            model.ScaleFactor = modelImpoter.globalScale;
            model.ReadWrite = modelImpoter.isReadable;
            model.ImportBlendShapes = modelImpoter.importBlendShapes;
            model.GenerateColliders = modelImpoter.addCollider;
            model.SwapUVs = modelImpoter.swapUVChannels;
            model.LightmapToUV2 = modelImpoter.generateSecondaryUV;
            model.Path = path;
            model.Hash = Utilities.GetFileMd5(path);
            model.Id = guid;

            var modelObject = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in modelObject)
            {
                if (obj is MeshFilter)
                {
                    Mesh mesh = (obj as MeshFilter).sharedMesh;
                    model.Tris += mesh.triangles.Length / 3;
                    model.Vertexes += mesh.vertexCount;
                }

                if (obj is SkinnedMeshRenderer)
                {
                    Mesh mesh = (obj as SkinnedMeshRenderer).sharedMesh;
                    model.Tris += mesh.triangles.Length / 3;
                    model.Vertexes += mesh.vertexCount;
                }

                UnloadAsset(obj);
            }

            return model;
        }

        public static APTexture GetAPTextureFromAssetGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var texture = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            APTexture apTexture = new APTexture();
            if (texture is RenderTexture)
            {
                var renderTexture = texture as RenderTexture;
                apTexture.StorageSize = TextureUtillity.GetStorageMemorySize(renderTexture);
                apTexture.Width = renderTexture.width;
                apTexture.Height = renderTexture.height;
                apTexture.TextureType = "Render";
                apTexture.Path = path;
                apTexture.FileType = Utilities.GetFileExtension(path);
                apTexture.Hash = Utilities.GetFileMd5(path);
                apTexture.Name = Utilities.GetFileName(path);
                apTexture.FileSize = Utilities.GetFileSize(path);
                apTexture.Id = guid;
                apTexture.Compress = TextureImporterCompression.Uncompressed;
                UnloadAsset(texture);
                return apTexture;
            }

            TextureImporter tImporter = GetAssetImporterFromAssetGuid<TextureImporter>(guid);
            if (tImporter == null)
            {
                return null;
            }

            var tex = texture as Texture;
            TextureImporterCompression importerCompression = tImporter.textureCompression;
            var platformSettings = tImporter.GetPlatformTextureSettings(Utilities.BuildTargetToPlatform(EditorUserBuildSettings.activeBuildTarget));
            apTexture.CompressionQuality = platformSettings.compressionQuality;
            apTexture.CrunchedCompression = platformSettings.crunchedCompression;
            apTexture.MaxSize = platformSettings.maxTextureSize;
            apTexture.Compress = platformSettings.textureCompression;

            // Get texture settings for different platform
            //
            apTexture.StorageSize = TextureUtillity.GetStorageMemorySize(tex);
            apTexture.Name = Utilities.GetFileName(path);
            apTexture.ReadWrite = tImporter.isReadable;

            if ((int)platformSettings.format > 0)
            {
                apTexture.TextureFormat = platformSettings.format.ToString();
            }
            else
            {
                apTexture.TextureFormat = "Auto";
            }

            apTexture.TextureType = tImporter.textureType.ToString();
            apTexture.Path = path;
            apTexture.FileType = Utilities.GetFileExtension(path);
            apTexture.Hash = Utilities.GetFileMd5(path);
            apTexture.MipMap = tImporter.mipmapEnabled;
            apTexture.Width = tex.width;
            apTexture.Height = tex.height;
            apTexture.FileSize = Utilities.GetFileSize(path);

            int widthInPixel = 0;
            int heightInPixel = 0;
            TextureUtillity.GetImageSize(texture as Texture2D, out widthInPixel, out heightInPixel);
            apTexture.WidthInPixel = widthInPixel;
            apTexture.HeightInPixel = heightInPixel;

            if (tImporter.textureType == TextureImporterType.Sprite)
            {
                apTexture.PackingTag = tImporter.spritePackingTag;
            }

            apTexture.Id = guid;

            UnloadAsset(texture);
            return apTexture;
        }

        public static APPrefab GetAPPrefabFromAssetGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            APPrefab prefab = new APPrefab();
            prefab.Path = path;
            prefab.FileType = Utilities.GetFileExtension(path);
            prefab.Hash = Utilities.GetFileMd5(path);
            prefab.FileSize = Utilities.GetFileSize(path);
            prefab.Name = Utilities.GetFileName(path);
            prefab.Id = guid;

            HashSet<string> Tags = new HashSet<string>();
            HashSet<string> Layers = new HashSet<string>();

            var objects = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in objects)
            {
                var go = obj as GameObject;
                if (go == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(go.tag)
                    && !go.tag.Equals("untagged", StringComparison.OrdinalIgnoreCase))
                {
                    Tags.Add(go.tag);
                }

                var layerName = LayerMask.LayerToName(go.layer);
                if (!string.IsNullOrEmpty(layerName) &&
                    !layerName.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    Layers.Add(layerName);
                }
            }

            prefab.InLayers = string.Join(",", Layers.ToArray());
            prefab.ContainTags = string.Join(",", Tags.ToArray());

            objects = null;
            return prefab;
        }

        public static APMaterial GetAPMaterialFromAssetGuid(string guid)
        {
            APMaterial material = new APMaterial();
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (Utilities.IsMaterial(path))
            {
                if (path.EndsWith(".mat"))
                {
                    material.Type = MaterialType.Material;
                }
                else
                {
                    if (path.EndsWith(".physicmaterial"))
                    {
                        material.Type = MaterialType.PhysicMaterial;
                    }
                    else
                    {
                        material.Type = MaterialType.PhysicsMaterial2D;
                    }
                }

                material.Path = path;
                material.FileType = Utilities.GetFileExtension(path);
                material.Hash = Utilities.GetFileMd5(path);
                material.FileSize = Utilities.GetFileSize(path);
                material.Name = Utilities.GetFileName(path);

                Material m = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                if (m != null && m.shader != null)
                {
                    material.Shader = m.shader.name;
                }
                else
                {
                    material.Shader = string.Empty;
                }

                material.Id = guid;
                UnloadAsset(m);
                return material;
            }

            return null;
        }

        private static bool InUseOrSelection(UnityEngine.Object obj)
        {
            // In selection
            //
            foreach (var selected in Selection.objects)
            {
                if (selected == obj)
                {
                    return true;
                }
            }

            return false;
        }

        public static void UnloadAsset(UnityEngine.Object obj)
        {
            if (InUseOrSelection(obj) || obj is GameObject || obj is Component || obj is AssetBundle)
            {
                obj = null;
                return;
            }

            Resources.UnloadAsset(obj);
            obj = null;
        }

        public static string GetAssetTypeByObject(UnityEngine.Object obj)
        {
            foreach (var cat in ModuleHelper.Categories)
            {
                if (cat.IsMatch(obj))
                {
                    return cat.CreateAssetType();
                }
            }

            return AssetType.OTHERS;
        }

        public static List<APAsset> GetHierarchyAssets()
        {
            List<APAsset> assets = new List<APAsset>();
            var assetObjects = EditorUtility.CollectDependencies(GameObject.FindObjectsOfType(typeof(GameObject)));
            HashSet<string> filter = new HashSet<string>();
            foreach (var item in assetObjects)
            {
                var assetPath = AssetDatabase.GetAssetPath(item);

                if (filter.Contains(assetPath))
                {
                    continue;
                }

                filter.Add(assetPath);

                if (string.IsNullOrEmpty(assetPath)
                    || assetPath.Contains("unity_builtin_extra")
                    || assetPath.Contains("unity default resources")
                    || assetPath.Contains("unity editor resources"))
                {
                    continue;
                }

                var asset = GetAPFile(assetPath);
                assets.Add(asset);
            }

            return assets;
        }

        public static Dictionary<string, string> GetMonoScriptsMap()
        {
            var guids = GetAssetGuidsByType("Script");
            Dictionary<string, string> map = new Dictionary<string, string>();

            foreach (var item in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript ms = AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)) as MonoScript;
                if (ms != null && !map.ContainsKey(ms.name))
                {
                    var cls = ms.GetClass();
                    if (cls != null)
                    {
                        map.Add(ms.name, path);
                    }
                }
            }

            return map;
        }
    }
}