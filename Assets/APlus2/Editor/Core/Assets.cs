//  Copyright (c) 2020-present amlovey
//  
using System;
using UnityEditor;
using UnityEngine;

namespace APlus2
{
    public enum NullableBoolean
    {
        Unkonwn,
        True,
        False,
    }

    public class AssetType 
    {
        public const string TEXTURES = "textures";
        public const string ANIMATIONS = "animations";
        public const string MODELS = "models";
        public const string AUDIOS = "audios";
        public const string VIDEOS = "videos";
        public const string MATERIALS = "materials";
        public const string SHADERS = "shaders";
        public const string FONTS = "fonts";
        public const string PREFABS = "prefabs";
        public const string STREAMING_ASSETS = "streamingassets";
        public const string CODE = "code";
        public const string BLACKLIST = "blacklist";
        public const string SCENE = "Scene";
        public const string OTHERS = "others";
    }

    [Serializable]
    public class APAsset
    {
        public string Id;
        public string Name;
        public string Path;
        public long FileSize;
        public NullableBoolean Used;
        public string Hash;
        public string AssetType;
        public string FileType;
        public bool InAssetBundle;
#if ADDRESSABLE_ON
        public bool InAddressables;
#endif
    }

    [Serializable]
    public class APTexture: APAsset
    {
        public int StorageSize;
        public string TextureFormat;
        public string TextureType;
        public bool ReadWrite;
        public bool MipMap;
        public int MaxSize;
        public int Width;
        public int Height;
        public int WidthInPixel;
        public int HeightInPixel;
        public TextureImporterCompression Compress;
        public string PackingTag;
        public int CompressionQuality;
        public bool CrunchedCompression;
    }

    [Serializable]
    public class APModel : APAsset
    {
        public int Vertexes;
        public int Tris;
        public float ScaleFactor;
        public bool OptimizeMesh;
        public ModelImporterMeshCompression MeshCompression;
        public bool ReadWrite;
        public bool ImportBlendShapes;
        public bool GenerateColliders;
        public bool SwapUVs;
        public bool LightmapToUV2;
    }

    [Serializable]
    public class APPrefab : APAsset
    {
        public string ContainTags;
        public string InLayers;
    }

    [Serializable]
    public enum MaterialType
	{
		Material,
		PhysicMaterial,
        PhysicsMaterial2D
	}

    [Serializable]
    public class APMaterial : APAsset
    {
        public MaterialType Type;
        public string Shader;
    }

    [Serializable]
    public class APAnimation: APAsset 
	{
		public string InFile;
		public float Length;
		public int FPS;
		public bool LoopTime;
		public bool LoopPose;
		public float CycleOffset;

		public static string GetModelAnimationPath(string modelPath, long localId)
		{
			return string.Format("{0}?{1}", modelPath, localId);
		}
	}

    [Serializable]
    public class APFile : APAsset
    {
        
    }

    [Serializable]
    public class APCodeFile : APFile
    {
        public string ContainTags;
    }

    [Serializable]
    public class APShader : APAsset
    {
        public string FileName;
        public int LOD;
        public bool IgnoreProjector;
        public bool CastShadows;
        public bool SurfaceShader;
        public ulong VariantsIncluded;
        public ulong VariantsTotal;
        public int RenderQueue;
        public string RenderQueueText;
        public string DisableBatching;
    }

    [Serializable]
    public class APAudio : APAsset
    {
        public float Duration;
        public long ImportedSize;
        public float Ratio;
        public float Quality;
        public AudioCompressionFormat Compress;
        public int Frequency;
        public bool Background;
    }

    [Serializable]
    public class APVideo : APAsset 
    {
        public double Duration;
        public long Size;
    }

    public class APOverviewItem : APAsset
    {
        public string Assets;
        public int Number;
        public long StorageSize;
        public long AppUseSize;
    }
}