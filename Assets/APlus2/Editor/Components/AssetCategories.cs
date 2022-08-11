//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.Video;
using Type = System.Type;
using Object = UnityEngine.Object;

namespace APlus2
{
    public class TextureAssetCategory : AssetCategory
    {
        public TextureAssetCategory() : base(11) { }
        public override void AddAsset(string assetPath)
        {
            AddAsset<APTexture>(assetPath, AssetsHelper.GetAPTextureFromAssetGuid);
        }

        public override string CreateAssetType()
        {
            return AssetType.TEXTURES;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                // Texture Column Actions
                { "aptexture_storagesize", new LabelColumnAction<APTexture>(tex => Utilities.GetSizeDescription(tex.StorageSize), tex => tex.StorageSize)},
                { "aptexture_maxsize", new LabelColumnAction<APTexture>(tex => tex.MaxSize.ToString(), tex => tex.MaxSize)},
                { "aptexture_format", new LabelColumnAction<APTexture>(tex => tex.TextureFormat, tex => tex.TextureFormat)},
                { "aptexture_type", new LabelColumnAction<APTexture>(tex => tex.TextureType, tex => tex.TextureType)},
                { "aptexture_width", new LabelColumnAction<APTexture>(tex => tex.Width.ToString(), tex => tex.Width)},
                { "aptexture_height", new LabelColumnAction<APTexture>(tex => tex.Height.ToString(), tex => tex.Height)},
                { "aptexture_widthinpixel", new LabelColumnAction<APTexture>(tex => tex.WidthInPixel.ToString(), tex => tex.WidthInPixel)},
                { "aptexture_heightinpixel", new LabelColumnAction<APTexture>(tex => tex.HeightInPixel.ToString(), tex => tex.HeightInPixel)},
                { "aptexture_compress", new LabelColumnAction<APTexture>(tex => tex.Compress.ToString(), tex => tex.Compress)},
                { "aptexture_packingtag", new LabelColumnAction<APTexture>(tex => tex.PackingTag, tex => tex.PackingTag)},
                { "aptexture_compressionquality", new LabelColumnAction<APTexture>(tex => tex.CompressionQuality.ToString(), tex => tex.CompressionQuality)},
                { "aptexture_crunchedcompression", new FontIconColumnAction<APTexture>(tex => tex.CrunchedCompression)},
                { "aptexture_readwrite", new FontIconColumnAction<APTexture>(tex => tex.ReadWrite)},
                { "aptexture_mipmap", new FontIconColumnAction<APTexture>(tex => tex.MipMap)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize", false),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APTexture_StorageSize", "StorageSize", true),
                Column.CreateInstance("APTexture_MaxSize", "MaxSize"),
                Column.CreateInstance("APTexture_Format", "Format", false),
                Column.CreateInstance("APTexture_Type", "Type"),
                Column.CreateInstance("APTexture_Width", "Width", 80, false),
                Column.CreateInstance("APTexture_Height", "Height", 80, false),
                Column.CreateInstance("APTexture_WidthInPixel", "WidthInPixel", false),
                Column.CreateInstance("APTexture_HeightInPixel", "HeightInPixel", false),
                Column.CreateInstance("APTexture_Compress", "Compress", false),
                Column.CreateInstance("APTexture_PackingTag", "PackingTag", false),
                Column.CreateInstance("APTexture_CompressionQuality", "Quality", 120, false, "Compression Quality"),
                Column.CreateInstance("APTexture_CrunchedCompression", "Crunched", 120, false, "Crunched Compression"),
                Column.CreateInstance("APTexture_ReadWrite", "ReadWrite", false),
                Column.CreateInstance("APTexture_Mipmap", "Mipmap", false),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Texutes, this.CreateAssetType(), "Textures", 8);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var textures = state.GetAssetCacheItem(this.CreateAssetType()).assets;
            return CreateItem(
                textures.Count,
                GetTotalFileSize(textures),
                GetUsedStorageSize<APTexture>(textures, tex => tex.StorageSize),
                GetStorageSize<APTexture>(textures, tex => tex.StorageSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetTextures();
        }

        public override bool IsMatch(UnityEngine.Object obj)
        {
            return obj is Texture;
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APTexture);
        }

        public override void UpdateAsset(string assetPath)
        {
            UpdateAsset<APTexture>(assetPath, AssetsHelper.GetAPTextureFromAssetGuid);
        }
    }

    public class MaterialAssetCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            AddAsset<APMaterial>(assetPath, AssetsHelper.GetAPMaterialFromAssetGuid);
        }

        public override string CreateAssetType()
        {
            return AssetType.MATERIALS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                { "apmaterial_type", new LabelColumnAction<APMaterial>(m => m.Type.ToString(), m => m.Type)},
                { "apmaterial_shader", new LabelColumnAction<APMaterial>(m => m.Shader, m => m.Shader)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize", false),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APMaterial_Type", "Type", 140),
                Column.CreateInstance("APMaterial_Shader", "Shader", 240),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Materials, this.CreateAssetType(), "Materials", 9);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var materials = state.GetAssetCacheItem(this.CreateAssetType()).assets;
            return CreateItem(
                materials.Count,
                GetTotalFileSize(materials),
                GetUsedStorageSize<APMaterial>(materials, m => m.FileSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetMaterials();
        }

        public override bool IsMatch(UnityEngine.Object obj)
        {
            return obj is Material || obj is PhysicMaterial || obj is PhysicsMaterial2D;
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APMaterial);
        }

        public override void UpdateAsset(string assetPath)
        {
            UpdateAsset<APMaterial>(assetPath, AssetsHelper.GetAPMaterialFromAssetGuid);
        }
    }

    public class ModelAssetCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var asset = AssetsHelper.GetAPModelFromAssetGuid(guid);
            CacheManager.cache.AddAsset(asset);

            var animationClipAssetids = AssetsHelper.GetAnimationClipAssetIdInModel(assetPath);
            foreach (var id in animationClipAssetids)
            {
                var clipGUID = Utilities.GetGuidFromAssetId(id);
                var fileId = Utilities.GetFileIdFromAssetId(id);
                var clip = AssetsHelper.GetAPAnimationFromAssetPath(clipGUID, fileId);
                CacheManager.cache.AddAsset(clip);
            }
        }

        public override string CreateAssetType()
        {
            return AssetType.MODELS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                // Model Column Actions
                { "apmodel_vertexes", new LabelColumnAction<APModel>(model => model.Vertexes.ToString(), model => model.Vertexes)},
                { "apmodel_tris", new LabelColumnAction<APModel>(model => model.Tris.ToString(), model => model.Tris)},
                { "apmodel_scalefactor", new LabelColumnAction<APModel>(model => model.ScaleFactor.ToString("f2"), model => model.ScaleFactor)},
                { "apmodel_meshcompression", new LabelColumnAction<APModel>(model => model.MeshCompression.ToString(), model => model.MeshCompression)},
                { "apmodel_optimizemesh", new FontIconColumnAction<APModel>(model => model.OptimizeMesh)},
                { "apmodel_readwrite", new FontIconColumnAction<APModel>(model => model.ReadWrite)},
                { "apmodel_importblendshapes", new FontIconColumnAction<APModel>(model => model.ImportBlendShapes)},
                { "apmodel_generatecolliders", new FontIconColumnAction<APModel>(model => model.GenerateColliders)},
                { "apmodel_swapuvs", new FontIconColumnAction<APModel>(model => model.SwapUVs)},
                { "apmodel_lightmaptouv2", new FontIconColumnAction<APModel>(model => model.LightmapToUV2)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize"),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APModel_Vertexes", "Vertexes"),
                Column.CreateInstance("APModel_Tris", "Tris"),
                Column.CreateInstance("APModel_ScaleFactor", "ScaleFactor", false),
                Column.CreateInstance("APModel_MeshCompression", "MeshComp", false, "Mesh Compression"),
                Column.CreateInstance("APModel_OptimizeMesh", "OptMesh", false, "OptimizeMesh"),
                Column.CreateInstance("APModel_ReadWrite", "ReadWrite", false),
                Column.CreateInstance("APModel_ImportBlendShapes", "ImportBlendShapes", 180, false),
                Column.CreateInstance("APModel_GenerateColliders", "GenerateColliders", 180, false),
                Column.CreateInstance("APModel_SwapUVs", "SwapUVs", false),
                Column.CreateInstance("APModel_LightmapToUV2", "LightmapToUV2", 140, false),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Models, this.CreateAssetType(), "Models", 10);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var models = state.GetAssetCacheItem(AssetType.MODELS).assets;
            return CreateItem(
                models.Count,
                GetTotalFileSize(models),
                GetUsedStorageSize<APModel>(models, m => m.FileSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetModels();
        }

        public override bool IsMatch(Object obj)
        {
            return !(obj is AnimationClip) && Utilities.IsModelPath(AssetDatabase.GetAssetPath(obj));
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APModel);
        }

        public override void UpdateAsset(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var model = AssetsHelper.GetAPModelFromAssetGuid(guid);
            CacheManager.cache.UpdateAsset(model);

            var animationIds = CacheManager.cache.GetAssetsListByType(AssetType.ANIMATIONS).Select(ani => ani.Id).ToArray();
            HashSet<string> animationsAssetIdInCache = new HashSet<string>();

            foreach (var id in animationIds)
            {
                if (id.ToLower().Contains(guid.ToLower()))
                {
                    animationsAssetIdInCache.Add(id);
                }
            }

            var clipIds = AssetsHelper.GetAnimationClipAssetIdInModel(assetPath);
            foreach (var id in clipIds)
            {
                var clipGuid = Utilities.GetGuidFromAssetId(id);
                var fileId = Utilities.GetFileIdFromAssetId(id);
                var clip = AssetsHelper.GetAPAnimationFromAssetPath(clipGuid, fileId);
                if (animationsAssetIdInCache.Contains(id))
                {
                    CacheManager.cache.UpdateAsset(clip);
                }
                else
                {
                    CacheManager.cache.AddAsset(clip);
                }
            }

            foreach (var id in animationsAssetIdInCache)
            {
                if (!clipIds.Contains(id))
                {
                    Utilities.DebugLog(string.Format("delete in model animation {0}", id));
                    CacheManager.cache.RemoveAsset(id, AssetType.ANIMATIONS);
                }
            }
        }
    }

    public class AnimationCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var asset = AssetsHelper.GetAPAnimationFromAssetPath(guid, "");
            CacheManager.cache.AddAsset(asset);
        }

        public override string CreateAssetType()
        {
            return AssetType.ANIMATIONS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                // Animations Column Actions
                { "apanimation_infile", new LabelColumnAction<APAnimation>(ani => ani.InFile, ani => ani.InFile)},
                { "apanimation_length", new LabelColumnAction<APAnimation>(ani => ani.Length.ToString("f2"), ani => ani.Length)},
                { "apanimation_fps", new LabelColumnAction<APAnimation>(ani => ani.FPS.ToString(), ani => ani.FPS)},
                { "apanimation_looptime", new FontIconColumnAction<APAnimation>(ani => ani.LoopTime)},
                { "apanimation_looppose", new FontIconColumnAction<APAnimation>(ani => ani.LoopPose)},
                { "apanimation_cycleoffset", new LabelColumnAction<APAnimation>(ani => ani.CycleOffset.ToString("f2"), ani => ani.CycleOffset)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize", false),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APAnimation_InFile", "InFile", 240, false),
                Column.CreateInstance("APAnimation_Length", "Length", 100, true, "Length of animations in seconds"),
                Column.CreateInstance("APAnimation_FPS", "FPS", 80, true, "Frame per secnods"),
                Column.CreateInstance("APAnimation_LoopTime", "LoopTime"),
                Column.CreateInstance("APAnimation_LoopPose", "LoopPose"),
                Column.CreateInstance("APAnimation_CycleOffset", "CycleOffset", false),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Animations, this.CreateAssetType(), "Animations", 11);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var animations = state.GetAssetCacheItem(AssetType.ANIMATIONS).assets;
            return CreateItem(
                animations.Count,
                GetTotalFileSize(animations),
                GetUsedStorageSize<APAnimation>(animations, ani => ani.FileSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetAnimations();
        }

        public override bool IsMatch(Object obj)
        {
            return obj is AnimationClip || Utilities.IsUntyNewAnimation(AssetDatabase.GetAssetPath(obj));
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APAnimation);
        }

        public override void UpdateAsset(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var asset = AssetsHelper.GetAPAnimationFromAssetPath(guid, "");
            CacheManager.cache.UpdateAsset(asset);
        }
    }

    public class PrefabAssetCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            AddAsset<APPrefab>(assetPath, AssetsHelper.GetAPPrefabFromAssetGuid);
        }

        public override string CreateAssetType()
        {
            return AssetType.PREFABS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                // Prefab Column Actions
                { "apprefab_containtags", new LabelColumnAction<APPrefab>(prefab => prefab.ContainTags, prefab => prefab.ContainTags)},
                { "apprefab_inlayers", new LabelColumnAction<APPrefab>(prefab => prefab.InLayers, prefab => prefab.InLayers)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize"),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APPrefab_ContainTags", "ContainTags", 140),
                Column.CreateInstance("APPrefab_InLayers", "InLayers"),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Prefabs, this.CreateAssetType(), "Prefabs", 12);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var prefabs = state.GetAssetCacheItem(AssetType.PREFABS).assets;
            return CreateItem(
                prefabs.Count, 
                GetTotalFileSize(prefabs),
                GetUsedStorageSize<APPrefab>(prefabs, prefab => prefab.FileSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetPrefabs();
        }

        public override bool IsMatch(Object obj)
        {
            return Utilities.IsPrefab(AssetDatabase.GetAssetPath(obj));
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APPrefab);
        }

        public override void UpdateAsset(string assetPath)
        {
            UpdateAsset<APPrefab>(assetPath, AssetsHelper.GetAPPrefabFromAssetGuid);
        }
    }

    public class ScriptsAssetCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var asset = AssetsHelper.GetCodeFile(assetPath);
            CacheManager.cache.AddAsset(asset);
        }

        public override string CreateAssetType()
        {
            return AssetType.CODE;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                { "apcodefile_containtags", new LabelColumnAction<APCodeFile>(file => file.ContainTags, file => file.ContainTags)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize"),
                Column.CreateInstance("APAsset_FileType", "FileType"),
                Column.CreateInstance("APCodeFile_ContainTags", "ContainTags", false),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Codes, this.CreateAssetType(), "Codes", 13);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var scripts = state.GetAssetCacheItem(AssetType.CODE).assets;
            return CreateItem(
                scripts.Count, 
                GetTotalFileSize(scripts),
                GetUsedStorageSize<APCodeFile>(scripts, script => script.FileSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetCodeFiles();
        }

        public override bool IsMatch(Object obj)
        {
            return Utilities.IsCodeFile(AssetDatabase.GetAssetPath(obj));
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APCodeFile);
        }

        public override void UpdateAsset(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var asset = AssetsHelper.GetCodeFile(assetPath);
            CacheManager.cache.UpdateAsset(asset);
        }
    }

    public class ShaderAssetCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            AddAsset<APShader>(assetPath, AssetsHelper.GetAPShaderFromAssetGuid);
        }

        public override string CreateAssetType()
        {
            return AssetType.SHADERS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                // Shader Column Actions
                { "apshader_filename", new LabelColumnAction<APShader>(shader => shader.FileName, shader => shader.FileName)},
                { "apshader_lod", new LabelColumnAction<APShader>(shader => shader.LOD.ToString(), shader => shader.LOD)},
                { "apshader_ignoreprojector", new FontIconColumnAction<APShader>(shader => shader.IgnoreProjector)},
                { "apshader_castshadows", new FontIconColumnAction<APShader>(shader => shader.CastShadows)},
                { "apshader_surfaceshader", new FontIconColumnAction<APShader>(shader => shader.SurfaceShader)},
                { "apshader_variantsincluded", new LabelColumnAction<APShader>(shader => shader.VariantsIncluded.ToString(), shader => shader.VariantsIncluded)},
                { "apshader_variantstotal", new LabelColumnAction<APShader>(shader => shader.VariantsTotal.ToString(), shader => shader.VariantsTotal)},
                { "apshader_renderqueue", new LabelColumnAction<APShader>(shader => shader.RenderQueue.ToString(), shader => shader.RenderQueue)},
                { "apshader_renderqueuetext", new LabelColumnAction<APShader>(shader => shader.RenderQueueText, shader => shader.RenderQueueText)},
                { "apshader_disablebatching", new LabelColumnAction<APShader>(shader => shader.DisableBatching, shader => shader.DisableBatching)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize", false),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APShader_FileName", "FileName", 240),
                Column.CreateInstance("APShader_LOD", "LOD", 80, false),
                Column.CreateInstance("APShader_IgnoreProjector", "IgnoreProjector", 160, false),
                Column.CreateInstance("APShader_CastShadows", "CastShadows", false),
                Column.CreateInstance("APShader_SurfaceShader", "SurfaceShader", 140, false),
                Column.CreateInstance("APShader_VariantsIncluded", "VariantsIncluded", 180, false),
                Column.CreateInstance("APShader_VariantsTotal", "VariantsTotal", 140, false),
                Column.CreateInstance("APShader_RenderQueue", "RenderQueue", 130),
                Column.CreateInstance("APShader_RenderQueueText", "RenderQueueText", 170, false),
                Column.CreateInstance("APShader_DisableBatching", "DisableBatching", 180, false),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Shaders, this.CreateAssetType(), "Shaders", 14);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var shaders = state.GetAssetCacheItem(AssetType.SHADERS).assets;
            return CreateItem(
                shaders.Count, 
                GetTotalFileSize(shaders),
                GetUsedStorageSize<APShader>(shaders, shader => shader.FileSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetShaders();
        }

        public override bool IsMatch(Object obj)
        {
            return obj is Shader;
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APShader);
        }

        public override void UpdateAsset(string assetPath)
        {
            UpdateAsset<APShader>(assetPath, AssetsHelper.GetAPShaderFromAssetGuid);
        }
    }

    public class AudioAssetCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            AddAsset<APAudio>(assetPath, AssetsHelper.GetAPAudioFromAssetGuid);
        }

        public override string CreateAssetType()
        {
            return AssetType.AUDIOS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                // Audio Column Actions
                { "apaudio_duration", new LabelColumnAction<APAudio>(audio => Utilities.GetTimeDurationString(audio.Duration), audio => audio.Duration)},
                { "apaudio_importedsize", new LabelColumnAction<APAudio>(audio => Utilities.GetSizeDescription(audio.ImportedSize), audio => audio.ImportedSize)},
                { "apaudio_ratio", new LabelColumnAction<APAudio>(audio => audio.Ratio.ToString("f2"), audio => audio.Ratio)},
                { "apaudio_quality", new LabelColumnAction<APAudio>(audio => audio.Quality.ToString("f2"), audio => audio.Quality)},
                { "apaudio_compress", new LabelColumnAction<APAudio>(audio => audio.Compress.ToString(), audio => audio.Compress)},
                { "apaudio_frequency", new LabelColumnAction<APAudio>(audio => audio.Frequency.ToString(), audio => audio.Frequency)},
                { "apaudio_background", new FontIconColumnAction<APAudio>(audio => audio.Background)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize", false),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APAudio_Duration", "Duration", 100),
                Column.CreateInstance("APAudio_ImportedSize", "ImportedSize", 140),
                Column.CreateInstance("APAudio_Ratio", "Ratio", 80),
                Column.CreateInstance("APAudio_Quality", "Quality", 90),
                Column.CreateInstance("APAudio_Compress", "Compress", false),
                Column.CreateInstance("APAudio_Frequency", "Frequency", 130, false),
                Column.CreateInstance("APAudio_Background", "Background", 140, false, "Load in background"),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Audios, this.CreateAssetType(), "Audios", 15);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var audios = state.GetAssetCacheItem(this.CreateAssetType()).assets;
            return CreateItem(
                audios.Count, 
                GetTotalFileSize(audios),
                GetUsedStorageSize<APAudio>(audios, audio => audio.ImportedSize),
                GetStorageSize<APAudio>(audios, audio => audio.ImportedSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetAudios();
        }

        public override bool IsMatch(Object obj)
        {
            return obj is AudioClip;
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APAudio);
        }

        public override void UpdateAsset(string assetPath)
        {
            UpdateAsset<APAudio>(assetPath, AssetsHelper.GetAPAudioFromAssetGuid);
        }
    }

    public class VideoAssetCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            AddAsset<APVideo>(assetPath, AssetsHelper.GetAPVideoFromAssetGuid);
        }

        public override string CreateAssetType()
        {
            return AssetType.VIDEOS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return new Dictionary<string, ColumnAction>()
            {
                // Video Column Actions
                { "apvideo_duration", new LabelColumnAction<APVideo>(video => Utilities.GetTimeDurationString(video.Duration), video => video.Duration)},
                { "apvideo_size", new LabelColumnAction<APVideo>(video => Utilities.GetSizeDescription(video.Size), video => video.Size)},
            };
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize", false),
                Column.CreateInstance("APAsset_FileType", "FileType", false),
                Column.CreateInstance("APVideo_Duration", "Duration", 100),
                Column.CreateInstance("APVideo_Size", "Size", 90),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Videos, this.CreateAssetType(), "Videos", 16);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var videos = state.GetAssetCacheItem(this.CreateAssetType()).assets;
            return CreateItem(
                videos.Count, 
                GetTotalFileSize(videos),
                GetUsedStorageSize<APVideo>(videos, video => video.Size),
                GetStorageSize<APVideo>(videos, video => video.Size)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetVideos();
        }

        public override bool IsMatch(Object obj)
        {
            return obj is VideoClip;
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APVideo);
        }

        public override void UpdateAsset(string assetPath)
        {
            UpdateAsset<APVideo>(assetPath, AssetsHelper.GetAPVideoFromAssetGuid);
        }
    }

    public class StreamingAssetsCategory : AssetCategory
    {
        public override void AddAsset(string assetPath)
        {
            var file = AssetsHelper.GetAPFile(assetPath);
            CacheManager.cache.AddAsset(file);
        }

        public override string CreateAssetType()
        {
            return AssetType.STREAMING_ASSETS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return null;
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize"),
                Column.CreateInstance("APAsset_FileType", "FileType"),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.StreamingAssets, AssetType.STREAMING_ASSETS, "StreamingAssets", 17);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var streamingAssets = state.GetAssetCacheItem(this.CreateAssetType()).assets;
            return CreateItem(
                streamingAssets.Count, 
                GetTotalFileSize(streamingAssets),
                GetUsedStorageSize<APFile>(streamingAssets, file => file.FileSize)
            );
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetStreamingAssets();
        }

        public override bool IsMatch(Object obj)
        {
            return Utilities.IsStreamingAssetsFile(AssetDatabase.GetAssetPath(obj));
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APFile);
        }

        public override void UpdateAsset(string assetPath)
        {
            var file = AssetsHelper.GetAPFile(assetPath);
            CacheManager.cache.UpdateAsset(file);
        }
    }

    public class OthersAssetCategory : AssetCategory
    {
        public OthersAssetCategory() : base(65535)
        {

        }

        public override void AddAsset(string assetPath)
        {
            var asset = AssetsHelper.GetAPFile(assetPath);
            CacheManager.cache.AddAsset(asset);
        }

        public override string CreateAssetType()
        {
            return AssetType.OTHERS;
        }

        public override Dictionary<string, ColumnAction> CreateColumnActions()
        {
            return null;
        }

        public override List<Column> CreateColumns()
        {
            return new List<Column>()
            {
                Column.CreateInstance("APAsset_Name", "Name", 240),
                Column.CreateInstance("APAsset_FileSize", "FileSize"),
                Column.CreateInstance("APAsset_FileType", "FileType"),
                Column.CreateInstance("APAsset_Path", "Path", 360, false),
                Column.CreateInstance("APAsset_Used", "Used"),
                Column.CreateInstance("APAsset_Id", "Id", 280, false),
                Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
            };
        }

        public override NaviMenuItem CreateMenu()
        {
            return new NaviMenuItem(Icons.Others, this.CreateAssetType(), "Others", 65535);
        }

        public override APOverviewItem CreateOverviewItem(AppState state)
        {
            var others = state.GetAssetCacheItem(this.CreateAssetType()).assets;
            var othersCount = others.Count();
            var othersSize = GetTotalFileSize(others);
            var usedOtherSize = GetUsedStorageSize<APFile>(others, asset => asset.FileSize); 
            return CreateItem(othersCount, othersSize, usedOtherSize);
        }

        public override List<APAsset> GetAssets()
        {
            return AssetsHelper.GetOthers();
        }

        public override bool IsMatch(Object obj)
        {
            return AssetDatabase.GetAssetPath(obj).ToLower().StartsWith("assets");
        }

        public override Type RegisterAPAssetClass()
        {
            return typeof(APFile);
        }

        public override void UpdateAsset(string assetPath)
        {
            var asset = AssetsHelper.GetAPFile(assetPath);
            CacheManager.cache.AddAsset(asset);
        }
    }
}
