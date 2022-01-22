using System;
using System.IO;
using Duality;
using SardineFish.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GenerateTiles : EditorWindow
    {
        [MenuItem("GameEditor/GenerateTiles")]
        private static void ShowWindow()
        {
            var window = GetWindow<GenerateTiles>();
            window.titleContent = new GUIContent("Generate Tiles");
            window.Show();
        }

        private string outputFolder;
        private TileType _tileType;

        private void OnGUI()
        {
            outputFolder = EditorGUILayout.TextField("Output", outputFolder);
            
            if (GUILayout.Button("Browse"))
            {
                outputFolder = EditorUtility.OpenFolderPanel("Output", outputFolder, "");
            }

            _tileType = (TileType)EditorGUILayout.EnumFlagsField("TileType", _tileType);

            if (GUILayout.Button("Generate"))
            {
                foreach (var obj in Selection.GetFiltered(typeof (Sprite), SelectionMode.Assets))
                {
                    var sprite = obj as Sprite;

                    var tile = CreateInstance<GameTile>();
                    tile.sprite = sprite;
                    tile.Type = _tileType;
                    var filename = Path.Join(outputFolder, $"Tile_{_tileType}_{sprite.name}");
                    AssetDatabase.CreateAsset(tile, filename);
                    Debug.Log($"Saved to {filename}");
                }
            }
        }
    }
}