﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Common.Interfaces;
using Mahjong.Logic;
using GamePlay.Client.Model;
using GamePlay.Client.View.SubManagers;
using GamePlay.Client.Controller;
using static GamePlay.Client.View.SubManagers.PlayerPointTransferManager;
using GamePlay.Server.Model;

namespace GamePlay.Client.View
{
    public class PointTransferManager : MonoBehaviour, IObserver<ClientRoundStatus>
    {
        public PlayerPointTransferManager[] SubManagers;
        public Button ConfirmButton;
        public CountDownController ConfirmCountDownController;

        public void SetTransfer(ClientRoundStatus CurrentRoundStatus, IList<PointTransfer> transfers, UnityAction callback)
        {
            gameObject.SetActive(true);
            var points = CurrentRoundStatus.Points;
            for (int placeIndex = 0; placeIndex < SubManagers.Length; placeIndex++)
            {
                int playerIndex = CurrentRoundStatus.GetPlayerIndex(placeIndex);
                if (!IsValidPlayer(playerIndex, CurrentRoundStatus.TotalPlayers)) continue;
                // get corresponding transfers
                var localTransfers = new List<Transfer>();
                foreach (var transfer in transfers)
                {
                    if (transfer.From == playerIndex)
                    {
                        localTransfers.Add(new Transfer
                        {
                            Type = GetTransferType(transfer.From, transfer.To),
                            Amount = -transfer.Amount
                        });
                    }
                    if (transfer.To == playerIndex)
                    {
                        localTransfers.Add(new Transfer
                        {
                            Type = Type.None,
                            Amount = transfer.Amount
                        });
                    }
                }
                SubManagers[placeIndex].SetTransfers(points[placeIndex], localTransfers);
            }
            // todo -- places animation
            // count down
            ConfirmButton.onClick.RemoveAllListeners();
            ConfirmButton.onClick.AddListener(() =>
            {
                callback();
                ConfirmCountDownController.StopCountDown();
            });
            ConfirmCountDownController.StartCountDown(MahjongConstants.SummaryPanelWaitingTime, callback);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private static Type GetTransferType(int from, int to)
        {
            if (from < 0) return Type.None;
            int diff = to - from;
            if (diff < 0) diff += 4;
            switch (diff)
            {
                case 1:
                    return Type.Right;
                case 2:
                    return Type.Straight;
                case 3:
                    return Type.Left;
                default:
                    return Type.None;
            }
        }

        private void OnDisable()
        {
            foreach (var manager in SubManagers)
            {
                manager.gameObject.SetActive(false);
            }
        }

        private bool IsValidPlayer(int index, int totalPlayers)
        {
            return index >= 0 && index < totalPlayers;
        }

        public void UpdateStatus(ClientRoundStatus subject)
        {
            if (subject == null) return;
            for (int placeIndex = 0; placeIndex < SubManagers.Length; placeIndex++)
            {
                int playerIndex = subject.GetPlayerIndex(placeIndex);
                if (IsValidPlayer(playerIndex, subject.TotalPlayers))
                {
                    SubManagers[placeIndex].gameObject.SetActive(true);
                    SubManagers[placeIndex].PlayerName = subject.GetPlayerName(placeIndex);
                }
                else
                    SubManagers[placeIndex].gameObject.SetActive(false);
            }
        }
    }
}