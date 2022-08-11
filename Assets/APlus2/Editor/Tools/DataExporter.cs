//  Copyright (c) 2016-present amlovey
//  
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;

namespace APlus2
{
    public class DataExporter
    {
        enum DataType
        {
            CSV,
        }

        [MenuItem("Tools/A+ Assets Explorer 2/Data Exporter/Export as CSV...", false, 33)]
        public static void ExportToCSV()
        {
            string title = "Export as CSV";
            SaveDataWithDialog(title, DataType.CSV);
        }

        private static void SaveDataWithDialog(string title, DataType type)
        {
            string folderPath = EditorUtility.OpenFolderPanel(title, Application.dataPath, "");
            Utilities.DebugLog(folderPath);
            if (!string.IsNullOrEmpty(folderPath))
            {
                switch (type)
                {
                    case DataType.CSV:
                        SaveCSV(folderPath);
                        break;
                }
            }
        }

        private static void SaveCSV(string folderPath)
        {
            SaveToLocal(Path.Combine(folderPath, "textures.csv"), GetTextureCSV());
            SaveToLocal(Path.Combine(folderPath, "models.csv"), GetModlesCSV());
            SaveToLocal(Path.Combine(folderPath, "animationClips.csv"), GetAnimationClipCSV());
            SaveToLocal(Path.Combine(folderPath, "material.csv"), GetMaterialsCSV());
            SaveToLocal(Path.Combine(folderPath, "prefabs.csv"), GetPrefabsCSV());
            SaveToLocal(Path.Combine(folderPath, "shaders.csv"), GetShadersCSV());
            SaveToLocal(Path.Combine(folderPath, "audios.csv"), GetAudiosCSV());
            SaveToLocal(Path.Combine(folderPath, "movies.csv"), GetMoviesCSV());
            SaveToLocal(Path.Combine(folderPath, "streamingAssets.csv"), GetStreamingAssetsCSV());
            SaveToLocal(Path.Combine(folderPath, "others.csv"), GetOtherFilesCSV());

            string message = string.Format("Saved to folder {0}", folderPath);
            if(EditorUtility.DisplayDialog("Done!", message, "OK"))
            {
                EditorUtility.RevealInFinder(folderPath);
            }
        }

        private static void SaveToLocal(string filePath, string data)
        {
            File.WriteAllText(filePath, data);
        }

        private static string GetOtherFilesCSV()
        {
            string header = "Name,FileSize,Used,Path,MD5";
            return GenerateCSV<APFile>(header, AssetType.OTHERS, file => 
                string.Format("{0},{1},{2},{3},{4}", file.Name, file.FileSize, file.Used, file.Path, file.Hash)
            );
        }

        private static string GetStreamingAssetsCSV()
        {
            string header = "Name,FileSize,Used,Path,MD5";
            return GenerateCSV<APFile>(header, AssetType.STREAMING_ASSETS, file => 
                string.Format("{0},{1},{2},{3},{4}", file.Name, file.FileSize, file.Used, file.Path, file.Hash)
            );
        }

        private static string GetMoviesCSV()
        {
            string header = "Name,FileSize,TextureSize,Used,Path,MD5";
            return GenerateCSV<APVideo>(header, AssetType.VIDEOS, movie => 
                string.Format("{0},{1},{2},{3},{4},{5}", 
                    movie.Name, 
                    movie.FileSize,
                    movie.Size,
                    movie.Used,
                    movie.Path,
                    movie.Hash)
            );
        }

        private static string GetAudiosCSV()
        {
            string header = "Name,FileSize,ImportedSize,Duration,Compress,Frequency,Ratio,Quality,Background,Used,Path,MD5";
            return GenerateCSV<APAudio>(header, AssetType.AUDIOS, audio => 
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", 
                    audio.Name, 
                    audio.FileSize, 
                    audio.ImportedSize,
                    audio.Duration, 
                    audio.Compress, 
                    audio.Frequency,
                    audio.Ratio,
                    audio.Quality,
                    audio.Background,
                    audio.Used,
                    audio.Path,
                    audio.Hash)
            );
        }

        private static string GetShadersCSV()
        {
            string header = "Name,FileSize,FileName,RenderQueue,RenderQueueText,LOD,CastShadows,DisableBatching,IgnoreProjector,SurfaceShader,VariantsTotal,VariantsIncluded,Used,Path,MD5";
            return GenerateCSV<APShader>(header, AssetType.SHADERS, shader => 
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", 
                    shader.Name,
                    shader.FileSize,
                    shader.FileName,
                    shader.RenderQueue,
                    shader.RenderQueueText,
                    shader.LOD,
                    shader.CastShadows,
                    shader.DisableBatching,
                    shader.IgnoreProjector,
                    shader.SurfaceShader,
                    shader.VariantsTotal,
                    shader.VariantsIncluded,
                    shader.Used,
                    shader.Path,
                    shader.Hash)
            );
        }

        private static string GetCodeCSV()
        {
            string header = "Name,FileSize,FileType,Used,Path,MD5";
            return GenerateCSV<APCodeFile>(header, AssetType.CODE, code => 
                string.Format("{0},{1},{2},{3},{4},{5}", code.Name, code.FileSize, code.FileType, code.Used, code.Path, code.Hash)
            );
        }

        private static string GetPrefabsCSV()
        {
            string header = "Name,FileSize,Used,Path,MD5";
            return GenerateCSV<APPrefab>(header, AssetType.PREFABS, prefab => 
                string.Format("{0},{1},{2},{3},{4}", prefab.Name, prefab.FileSize, prefab.Used, prefab.Path, prefab.Hash)
            );
        }

        private static string GetMaterialsCSV()
        {
            string header = "Name,FileSize,Shader,Type,Used,Path,MD5";
            return GenerateCSV<APMaterial>(header, AssetType.MATERIALS, material => 
                string.Format("{0},{1},{2},{3},{4},{5},{6}", 
                    material.Name,
                    material.FileSize,
                    material.Shader,
                    material.Type,
                    material.Used,
                    material.Path,
                    material.Hash)
            );
        }

        private static string GetAnimationClipCSV()
        {
            string header = "Name,InFile,FileSize,Length,FPS,CycleOffset,LoopTime,LoopPose,Used,Path,MD5";
            return GenerateCSV<APAnimation>(header, AssetType.ANIMATIONS, clip =>
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        clip.Name,
                        clip.InFile,
                        clip.FileSize,
                        clip.Length,
                        clip.FPS,
                        clip.CycleOffset,
                        clip.LoopTime,
                        clip.LoopPose,
                        clip.Used,
                        clip.Path,
                        clip.Hash)
            );
        }

        private static string GetModlesCSV()
        {
            string header = "Name,FileSize,Vertexes,Tris,ScaleFactor,MeshCompression,ReadWrite,ImportBlendShapes,GenerateColliders,LightmapToUV2,OptimizeMesh,SwapUVs,Used,Path,MD5";
            return GenerateCSV<APModel>(header, AssetType.MODELS, model =>
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                            model.Name,
                            model.FileSize,
                            model.Vertexes,
                            model.Tris,
                            model.ScaleFactor,
                            model.MeshCompression,
                            model.ReadWrite,
                            model.ImportBlendShapes,
                            model.GenerateColliders,
                            model.LightmapToUV2,
                            model.OptimizeMesh,
                            model.SwapUVs,
                            model.Used,
                            model.Path,
                            model.Hash));
        }

        private static string GetTextureCSV()
        {
            string header = "Name,FileSize,StorageSize,MaxSize,MipMap,ReadWrite,TextureFormat,TextureType,Compression,CrunchedCompression,CompressionQuality,Width,Height,WidthInPixel,HeightInPixel,Used,Path,MD5";
            return GenerateCSV<APTexture>(header, AssetType.TEXTURES, texture =>
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
                            texture.Name,
                            texture.FileSize,
                            texture.StorageSize,
                            texture.MaxSize,
                            texture.MipMap,
                            texture.ReadWrite,
                            texture.TextureFormat,
                            texture.TextureType,
                            texture.Compress,
                            texture.CrunchedCompression,
                            texture.CompressionQuality,
                            texture.Width,
                            texture.Height,
                            texture.WidthInPixel,
                            texture.HeightInPixel,
                            texture.Used,
                            texture.Path,
                            texture.Hash)
            );

        }

        private static string GenerateCSV<T>(string header, string assetType, Func<T, string> rowDataGenerator) where T : APAsset
        {
            var dataSet = CacheManager.cache.GetAssetsListByType(assetType);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (var item in dataSet)
            {
                sb.AppendLine(rowDataGenerator(item as T));
            }

            return sb.ToString();
        }
    }
}
#endif
