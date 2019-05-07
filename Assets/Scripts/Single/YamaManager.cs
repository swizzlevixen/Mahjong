﻿using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single
{
    public class YamaManager : MonoBehaviour
    {
        public static readonly IDictionary<int, int> IndexToYama = new Dictionary<int, int> {
            {0, 0}, {1, 3}, {2, 2}, {3, 1}
        };
        [SerializeField] private Transform[] Walls;
        [HideInInspector] public int[] Places;
        [HideInInspector] public int OyaPlayerIndex;
        [HideInInspector] public int Dice;
        [HideInInspector] public int LingshangTilesCount;
        private MahjongSetData MahjongSetData;

        public void SetMahjongData(MahjongSetData data)
        {
            MahjongSetData = data;
            if (Dice == 0 || OyaPlayerIndex < 0) return;
            UpdateTiles();
        }

        private void UpdateTiles()
        {
            var yamaIndex = GetYamaIndex(Dice, OyaPlayerIndex, Places);
            UpdateYama(yamaIndex);
        }

        private static int GetYamaIndex(int dice, int oya, int[] places)
        {
            if (dice <= 0 || oya < 0 || oya >= 4 || places == null || places.Length != 4)
                throw new System.ArgumentException();
            var openSideOffset = (dice - 1) % 4;
            var openIndex = (oya + openSideOffset) % 4;
            var index = System.Array.FindIndex(places, i => i == openIndex);
            return IndexToYama[index];
        }

        private void UpdateYama(int openYamaIndex)
        {
            // drawn tiles
            for (int i = 0; i < MahjongSetData.TilesDrawn; i++)
            {
                var t = GetTileAt(openYamaIndex, Dice * 2 + i);
                DrawTile(t);
            }
            // lingshang tiles
            for (int i = 0; i < MahjongSetData.LingShangDrawn; i++)
            {
                var s = Dice - i / 2;
                var t = GetTileAt(openYamaIndex, (s - 1) * 2 + i % 2);
                DrawTile(t);
            }
            // dora tiles
            for (int i = 0; i < MahjongSetData.DoraIndicators.Length; i++)
            {
                var s = Dice - LingshangTilesCount / 2 - i;
                var t = GetTileAt(openYamaIndex, (s - 1) * 2);
                TurnTileFaceUp(t, MahjongSetData.DoraIndicators[i]);
            }
        }

        private Transform GetTileAt(int openYamaIndex, int index)
        {
            if (index < 0) index += MahjongSetData.TotalTiles;
            int yamaIndex = openYamaIndex;
            while (index >= Walls[yamaIndex].childCount)
            {
                index -= Walls[yamaIndex].childCount;
                yamaIndex++;
                if (yamaIndex >= 4) yamaIndex -= 4;
            }
            return Walls[yamaIndex].GetChild(index);
        }

        public void ResetAllTiles()
        {
            foreach (var wall in Walls)
            {
                wall.TraversalChildren(t =>
                {
                    t.gameObject.SetActive(true);
                    TurnTileFaceDown(t);
                });
            }
        }

        private static void DrawTile(Transform t)
        {
            t.gameObject.SetActive(false);
        }

        private static void TurnTileFaceUp(Transform t, Tile tile)
        {
            t.localRotation = MahjongConstants.FaceUpOnWall;
            var tileInstance = t.GetComponent<TileInstance>();
            tileInstance.SetTile(tile);
        }

        private static void TurnTileFaceDown(Transform t)
        {
            t.localRotation = MahjongConstants.FaceDownOnWall;
        }
    }
}
