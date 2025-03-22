using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// 从旧版2D扩展中提取，因为它显然已被从中移除
/// 用于为Koala2D和Grasslands场景中的地面瓦片提供动力
namespace MoreMountains.TopDownEngine 
{
    /// <summary>
    /// 一个带有用于随机化的权重值的精灵
    /// </summary>
    [Serializable]
    public struct WeightedSprite 
    {
        /// <summary>
        /// Sprite.
        /// </summary>
        public Sprite Sprite;
        /// <summary>
        /// 精灵的权重
        /// </summary>
        public int Weight;
    }

    /// <summary>
    /// 权重随机瓦片是从给定的精灵列表和目标位置中随机选择一个精灵并显示该精灵的瓦片。
    /// 可以通过设置一个值来改变精灵出现的概率，从而对精灵进行加权。基于其位置随机确定显示在瓦片上的精灵，并且该精灵在特定位置是固定的
    /// </summary>
    [Serializable]
    public class WeightedRandomTile : Tile 
    {
        /// <summary>
        /// 用于随机生成输出的精灵
        /// </summary>
        [SerializeField] public WeightedSprite[] Sprites;

        /// <summary>
        /// 从脚本化的瓦片中检索任何瓦片渲染数据
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) 
        {
            base.GetTileData(position, tilemap, ref tileData);
            
            if (Sprites == null || Sprites.Length <= 0) return;
            
            var oldState = Random.state;
            long hash = position.x;
            hash = hash + 0xabcd1234 + (hash << 15);
            hash = hash + 0x0987efab ^ (hash >> 11);
            hash ^= position.y;
            hash = hash + 0x46ac12fd + (hash << 7);
            hash = hash + 0xbe9730af ^ (hash << 11);
            Random.InitState((int) hash);

            //获取精灵的累积权重
            var cumulativeWeight = 0;
            foreach (var spriteInfo in Sprites) cumulativeWeight += spriteInfo.Weight;

            // 选择一个随机权重，并根据该权重选择一个精灵
            var randomWeight = Random.Range(0, cumulativeWeight);
            foreach (var spriteInfo in Sprites) 
            {
                randomWeight -= spriteInfo.Weight;
                if (randomWeight < 0) 
                {
                    tileData.sprite = spriteInfo.Sprite;    
                    break;
                }
            }
            Random.state = oldState;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WeightedRandomTile))]
    public class WeightedRandomTileEditor : Editor 
    {
        private SerializedProperty m_Color;
        private SerializedProperty m_ColliderType;

        private WeightedRandomTile Tile {
            get { return target as WeightedRandomTile; }
        }

        /// <summary>
        /// 为加权随机瓦片启用时
        /// </summary>
        public void OnEnable()
        {
            m_Color = serializedObject.FindProperty("m_Color");
            m_ColliderType = serializedObject.FindProperty("m_ColliderType");
        }

        /// <summary>
        /// 为加权随机瓦片绘制一个检查器
        /// </summary>
        public override void OnInspectorGUI() 
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            int count = EditorGUILayout.DelayedIntField("Number of Sprites", Tile.Sprites != null ? Tile.Sprites.Length : 0);
            if (count < 0) 
                count = 0;

            if (Tile.Sprites == null || Tile.Sprites.Length != count) 
            {
                Array.Resize(ref Tile.Sprites, count);
            }

            if (count == 0) 
                return;

            EditorGUILayout.LabelField("Place random sprites.");
            EditorGUILayout.Space();

            for (int i = 0; i < count; i++) 
            {
                Tile.Sprites[i].Sprite = (Sprite) EditorGUILayout.ObjectField("Sprite " + (i + 1), Tile.Sprites[i].Sprite, typeof(Sprite), false, null);
                Tile.Sprites[i].Weight = EditorGUILayout.IntField("Weight " + (i + 1), Tile.Sprites[i].Weight);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Color);
            EditorGUILayout.PropertyField(m_ColliderType);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Tile);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
