using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Random = UnityEngine.Random;

public class UtilScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// テキストファイルを扱うクラス
    /// </summary>
    public class TextFileManager
    {
        private string folderPath;

        /// <summary>
        /// コンストラクタ
        /// フォルダパス指定例:Application.dataPath + "/Log"
        /// </summary>
        /// <param name="folderPath"></param>
        public TextFileManager(string folderPath)
        {
            this.folderPath = folderPath;
        }

        /// <summary>
        /// テキストファイルを作成する
        /// </summary>
        /// <param name="name">ファイル名</param>
        public void Create(string name)
        {
            StreamWriter sw = File.CreateText(folderPath + "/" + name);
            sw.Close();
        }

        /// <summary>
        /// ファイルに文字列を書き込む
        /// </summary>
        /// <param name="name">書き込むファイルの名前</param>
        /// <param name="text">書き込むテキスト</param>
        /// <param name="postscript">追記する時true、上書きする時false</param>
        public void Write(string name, string text, bool postscript)
        {
            try
            {
                StreamWriter sw = new StreamWriter(folderPath + "/" + name, postscript); //true=追記 false=上書き
                sw.WriteLine(text);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    /// <summary>
    /// 正規化した配列を返す
    /// </summary>
    /// <param name="probArray"></param>
    /// <returns></returns>
    public static float[] Normalize(float[] probArray)
    {
        float[] result = new float[probArray.Length];
        float sum = 0;
        foreach (float prob in probArray)
        {
            sum += prob;
        }
        for (int i = 0; i < probArray.Length; i++)
        {
            result[i] = probArray[i] / sum;
        }
        return result;
    }

    /// <summary>
    /// 引数の大きさ分の長さのバーを返す
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static string Bar(float f)
    {
        string result = "";
        for (int i = 0; i < f; i++)
        {
            result += "█";
        }
        return result;
    }
}
