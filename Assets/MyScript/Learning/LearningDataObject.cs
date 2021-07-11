using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 学習用データのScriptableObject用クラス
/// </summary>
[CreateAssetMenu]
public class LearningDataObject : ScriptableObject
{
    [SerializeField, Range(0, 100)]
    private int inputSize;
    [SerializeField, Range(0, 100)]
    private int outputSize;

    [SerializeField, Multiline(10)]
    private string description; // inspector上で説明を記入する変数

    [SerializeField, Tooltip("使用例:\n     Input:    1\n                   2\n                   3\n     Output: 0.9\n                    0.1")]
    private bool PutCursorToSeeUsage; // inspectorに使用法を表示するための変数（意味はない）
    [SerializeField]
    public List<StringData> stringDatas = new List<StringData>();

    /// <summary>
    /// 学習用データを文字列で保持するクラス
    /// </summary>
    [System.Serializable]
    public class StringData
    {
        [Multiline(5)]
        public string input;
        [Multiline(5)]
        public string output;
    }
}

