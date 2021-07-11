2020/8/5 LearningData, LearningDataObject, LearningDataManagerの説明

・ニューラルネットワークの入力値と理想値のデータ作成から、スクリプト上でそれらのデータを扱うためのクラスたちです。

・使用法
    1 データ作成
        Assets > Create > Learning Data Object
    2 データ入力
        inputに入力値を、outputに理想値を入力してください。値+改行で入力してください。データ末の改行は任意です。誤ったフォーマットで入力するとおそらくエラーになります。
        sizeはあくまで確認用であり、入力は任意です。
    3 データ使用
        ManagerにデータをセットしGetLearningData()することで、変換後のデータを取得できます。

・使用例
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleClass : MonoBehaviour
{
    private LearningDataManager learningDataManager;
    [SerializeField]
    private LearningDataObject learningDataObject; // inspectorからアウトレット接続
    private List<LearningData> learningData;

    // Start is called before the first frame update
    void Start()
    {
        learningDataManager = new LearningDataManager();
        learningData = new List<LearningData>();

        learningDataManager.SetData(learningDataObject);
        learningData = learningDataManager.GetLearningData();

        foreach (LearningData ldata in learningData)
        {
            Debug.Log("input");
            foreach (float f in ldata.input)
            {
                Debug.Log(f);
            }
            Debug.Log("output");
            foreach (float f in ldata.output)
            {
                Debug.Log(f);
            }
        }
    }
}
