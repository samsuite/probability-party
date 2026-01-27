using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class PrintManager : MonoBehaviour {

    [System.Serializable]
    private struct ReceiptData {
        public string title;
        public string playerNum;
        public string tags;
        public string body;
        public bool hasQR;
        public bool hasImage;
        public string qrLink;
    }

    public static void PrintActivityReceipt (ActivityProfile activity) {
        UpdateImage(activity);
        UpdateJSON(activity);

        Process printProcess = new Process();
        printProcess.StartInfo.FileName = Application.streamingAssetsPath + "/ReceiptPrint/ReceiptPrint.exe";
        printProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        printProcess.Start();
    }

    private static void UpdateJSON (ActivityProfile activity) {
        string path = Application.streamingAssetsPath + "/ReceiptPrint/_internal/ReceiptResources/receiptContents.json";
        string playerNum = "for "+GameLogic.GetPlayerCountSummary(activity);
        string tags = GetTagsString(activity);

        ReceiptData data = new ReceiptData() {
            title = activity.name,
            playerNum = playerNum,
            tags = tags,
            body = activity.description,
            hasImage = activity.hasImage,
            hasQR = activity.hasQR,
            qrLink = activity.qrLink
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    private static void UpdateImage (ActivityProfile activity) {
        if (!activity.hasImage) {
            return;
        }

        string path = Application.streamingAssetsPath + "/ReceiptPrint/_internal/ReceiptResources/image.png";
        byte[] bytes = activity.image.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }

    private static string GetTagsString (ActivityProfile activity) {
        string completeString = string.Empty;
        bool isFirst = true;

        foreach (ActivityTag tag in Enum.GetValues(typeof(ActivityTag))) {
            bool hasTag = ((int)activity.tags & (int)tag) == (int)tag;
            if (hasTag) {
                if (!isFirst) {
                    completeString += ", ";
                }
                completeString += GameLogic.GetTagName(tag);
                isFirst = false;
            }
        }

        return completeString;
    }

}
