using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 学習用データ管理クラス
/// </summary>
public class LearningDataManager
{
    private LearningDataObject learningDataObj;
    private List<LearningDataObject.StringData> stringDatas;

    public LearningDataManager()
    {

    }

    /// <summary>
    /// 変換前のデータをセットする。
    /// </summary>
    /// <param name="learningDataObj">学習用データのScriptableObject</param>
    public void SetDataObj(LearningDataObject learningDataObj)
    { 
        this.learningDataObj = learningDataObj;
        stringDatas = learningDataObj.stringDatas;
    }

    /// <summary>
    /// StringData型をLearningData型に変換する。
    /// </summary>
    /// <param name="stringData">string型の学習用データ</param>
    /// <returns>学習用データ</returns>
    private LearningData TransformData(LearningDataObject.StringData stringData)
    {
        LearningData resultData = new LearningData();
        List<float> input = new List<float>(), output = new List<float>();
        string tmp = "";
        // inputの変換
        for (int i = 0; i < stringData.input.Length; i++)
        {
            if (stringData.input[i] != '\n')
            {
                tmp += stringData.input[i];
                if (i == stringData.input.Length-1)
                {
                    input.Add(float.Parse(tmp)); // ここのエラーはScriptableObjectのstringに間違ったデータの入力をしている時に起こる
                    break;
                }
            }
            else
            {
                input.Add(float.Parse(tmp)); // ここのエラーはScriptableObjectのstringに間違ったデータの入力をしている時に起こる
                tmp = "";
            }
        }
        tmp = "";
        // outputの変換
        for (int i = 0; i < stringData.output.Length; i++)
        {
            if (stringData.output[i] != '\n')
            {
                tmp += stringData.output[i];
                if (i == stringData.output.Length - 1)
                {
                    output.Add(float.Parse(tmp)); // ここのエラーはScriptableObjectのstringに間違ったデータの入力をしている時に起こる
                    break;
                }
            }
            else
            {
                output.Add(float.Parse(tmp)); // ここのエラーはScriptableObjectのstringに間違ったデータの入力をしている時に起こる
                tmp = "";
            }
        }
        resultData.input = input.ToArray();
        resultData.output = output.ToArray();
        resultData.inputSize = input.Count;
        resultData.outputSize = output.Count;

        return resultData;
    }

    public List<LearningData> GetLearningData()
    {
        List<LearningData> learningDatas = new List<LearningData>();
        for (int i = 0; i < learningDataObj.stringDatas.Count; i++)
        {
            learningDatas.Add(TransformData(stringDatas[i]));
        }
        return learningDatas;
    }
}
