using System.Collections.Generic;
using UnityEngine;
using TextFileManager = UtilScript.TextFileManager;

public class NeuralNetwork : MonoBehaviour
{
    public NeuralNetworkLayer InputLayer;
    public NeuralNetworkLayer[] HiddenLayer;
    public NeuralNetworkLayer OutputLayer;

    private TextFileManager tfm;
    private readonly string testTxt = "debug.txt";

    /*
    [SerializeField]
    private float learningRate = 0.0001f;

    [SerializeField]
    private LearningDataObject learningDataObject;
    private List<LearningData> learningDatas;
    private LearningDataManager learningDataManager;

    [SerializeField]
    private float[] testInput = new float[3];
    */

    /*
    private void Start()
    {
        // データ管理
        learningDataManager = new LearningDataManager();
        learningDataManager.SetDataObj(learningDataObject);
        learningDatas = learningDataManager.GetLearningData();

        // 初期化等
        Initialize(3, new int[]{3,3}, 1);
        SetInput(0, 1);
        SetInput(1, 2);
        SetDesiredOutput(0, 10);
        SetLerningRate(learningRate);
        SetMomentum(true, 0.01f);

        tfm = new TextFileManager(Application.dataPath + "/Log");
        tfm.Create(testTxt);
    }
    */

    /*
    int i = 0, j = 0;
    bool excecute = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            excecute = !excecute;
        }

        if (excecute)
        {
            for (int j = 0; j < InputLayer.NOFnodes; j++)
            {
                SetInput(j, learningDatas[i].input[j]);
            }
            for (int j = 0; j < OutputLayer.NOFnodes; j++)
            {
                SetDesiredOutput(j, learningDatas[i].output[j]);
            }
            i++;
            i %= learningDatas.Count;

            FeedForward();

            if (j % 1 == 0)
            {
                tfm.Write(testTxt, j + "回目", true);
                PrintData(testTxt);
            }
            j++;

            Debug.Log("Output");
            for (int i = 0; i < OutputLayer.NOFnodes; i++)
            {
                Debug.Log(i + " : " + GetOutput(i));
            }

            if(CulcurateError() < 10)
            Debug.Log(CulcurateError());

            j++;
            if (j % 100 == 0)
            {
                Debug.Log(j + "回目");
            }
            if (j < 10000)
            {
                tfm.Write(testTxt, CulcurateError().ToString(), true);
            }

            BackPropagate();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            for (int k = 0; k < InputLayer.NOFnodes; k++)
            {
                SetInput(k, testInput[k]);
            }
            FeedForward();

            Debug.Log(GetOutput(0));
        }
    }
    */

    /// <summary>
    /// ネットワークを初期化する。
    /// </summary>
    /// <param name="NumberOfInputNode">入力層のニューロンの数</param>
    /// <param name="NumberOfHiddenNode">中間層のニューロンの数を入力層側から格納した配列</param>
    /// <param name="NumberOfOutputNode">出力層のニューロンの数</param>
    public void Initialize(int NumberOfInputNode, int[] NumberOfHiddenNode, int NumberOfOutputNode)
    {
        // インスタンスの作成
        InputLayer = gameObject.AddComponent<NeuralNetworkLayer>();
        HiddenLayer = new NeuralNetworkLayer[NumberOfHiddenNode.Length];
        for (int i = 0; i < HiddenLayer.Length; i++)
        {
            HiddenLayer[i] = gameObject.AddComponent<NeuralNetworkLayer>();
        }
        OutputLayer = gameObject.AddComponent<NeuralNetworkLayer>();

        // 初期化
        // 入力層
        InputLayer.NOFparentNodes = 0;
        InputLayer.NOFchildNodes = NumberOfHiddenNode[0];
        InputLayer.Initialize(NumberOfInputNode, null, HiddenLayer[0]);
        InputLayer.RandomizeWeights();

        // 中間層
        for (int i = 0; i < HiddenLayer.Length; i++)
        {
            NeuralNetworkLayer parent, child;
            int tmpNOFchildNodes;
            if (i == 0)
            {
                parent = InputLayer;
                if (HiddenLayer.Length == 1)
                {
                    child = OutputLayer;
                    tmpNOFchildNodes = NumberOfOutputNode;
                }
                else
                {
                    child = HiddenLayer[1];
                    tmpNOFchildNodes = NumberOfHiddenNode[1];
                }
            }
            else if(i == NumberOfHiddenNode.Length - 1)
            {
                parent = HiddenLayer[i - 1];
                child = OutputLayer;
                tmpNOFchildNodes = NumberOfOutputNode;
            }
            else
            {
                parent = HiddenLayer[i - 1];
                child = HiddenLayer[i + 1];
                tmpNOFchildNodes = NumberOfHiddenNode[i + 1];
            }
            HiddenLayer[i].NOFparentNodes = parent.NOFnodes;
            HiddenLayer[i].NOFchildNodes = tmpNOFchildNodes;
            HiddenLayer[i].Initialize(NumberOfHiddenNode[i], parent, child);
            HiddenLayer[i].RandomizeWeights();
        }

        // 出力層
        OutputLayer.NOFparentNodes = NumberOfHiddenNode[NumberOfHiddenNode.Length - 1];
        OutputLayer.NOFchildNodes = 0;
        OutputLayer.Initialize(NumberOfOutputNode, HiddenLayer[HiddenLayer.Length - 1], null);
        OutputLayer.RandomizeWeights();
    }

    /// <summary>
    /// レイヤーのインスタンスを破棄する。
    /// </summary>
    public void CleanUp()
    {
        InputLayer = null;
        HiddenLayer = null;
        OutputLayer = null;
    }

    /// <summary>
    /// 入力値を設定する。
    /// </summary>
    /// <param name="i">入力するニューロンのインデックス</param>
    /// <param name="input">入力値</param>
    public void SetInput(int i, float input)
    {
        if (i >= 0 && i < InputLayer.NOFnodes)
        {
            InputLayer.neuronValues[i] = input;
        }
        else
        {
            Debug.LogError("Index is wrong.");
        }
    }

    /// <summary>
    /// 出力値を得る。
    /// </summary>
    /// <param name="i">出力ニューロンのインデックス</param>
    /// <returns>出力値</returns>
    public float GetOutput(int i)
    {
        if (i >= 0 && i < OutputLayer.NOFnodes)
        {
            return OutputLayer.neuronValues[i];
        }

        Debug.LogError("Index is wrong.");
        return 0;
    }

    /// <summary>
    /// 理想値を設定する。
    /// </summary>
    /// <param name="i">出力ニューロンのインデックス</param>
    /// <param name="value">理想値</param>
    public void SetDesiredOutput(int i, float value)
    {
        if (i >= 0 && i < OutputLayer.NOFnodes)
        {
            OutputLayer.desiredValues[i] = value;
        }
        else
        {
            Debug.LogError("Index is wrong.");
        }
    }

    /// <summary>
    /// ニューロンの値を出力層から順に計算する。
    /// </summary>
    public void FeedForward()
    {
        foreach (NeuralNetworkLayer hidden in HiddenLayer)
        {
            hidden.CalculateNeuronValues();
        }
        OutputLayer.CalculateNeuronValues();
    }

    /// <summary>
    /// 重みを更新する。
    /// </summary>
    public void BackPropagate()
    {
        // 勾配計算
        OutputLayer.CalculateGradients();
        for (int i = HiddenLayer.Length-1; i >= 0; i--)
        {
            HiddenLayer[i].CalculateGradients();
        }

        // 重み調整
        OutputLayer.AdjustWeights();
        foreach (NeuralNetworkLayer hidden in HiddenLayer)
        {
            hidden.AdjustWeights();
        }
    }

    /// <summary>
    /// 出力値が最も高い出力層のニューロンのインデックスを取得する。
    /// </summary>
    /// <returns>ニューロンのインデックス</returns>
    public int GetMaxOutputID()
    {
        float max = float.MinValue;
        int maxIndex = 0;
        for (int i= 0; i < OutputLayer.NOFnodes; i++)
        {
            if (max < OutputLayer.neuronValues[i])
            {
                max = OutputLayer.neuronValues[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    /// <summary>
    /// 出力層の平均二乗誤差を計算する。
    /// </summary>
    /// <returns>平均二乗誤差</returns>
    public float CulcurateError()
    {
        float error = 0;
        for (int i= 0; i < OutputLayer.NOFnodes; i++)
        {
            error += Mathf.Pow(OutputLayer.desiredValues[i] - OutputLayer.neuronValues[i], 2);
        }
        error /= OutputLayer.NOFnodes;
        return error;
    }

    public void NormalizeOutput()
    {
        float[] neuronValuesNormalized = UtilScript.Normalize(OutputLayer.neuronValues);
        for (int i = 0; i < OutputLayer.NOFnodes; i++)
        {
            OutputLayer.neuronValues[i] = neuronValuesNormalized[i];
        }
    }

    /// <summary>
    /// 学習率を設定する。
    /// </summary>
    /// <param name="learningRate">学習率</param>
    public void SetLerningRate(float learningRate)
    {
        foreach (NeuralNetworkLayer hidden in HiddenLayer)
        {
            hidden.learningRate = learningRate;
        }
        OutputLayer.learningRate = learningRate;
    }

    /// <summary>
    /// モーメンタムの設定をする。
    /// </summary>
    /// <param name="useMomentum">モーメンタム使用のためのフラグ</param>
    /// <param name="factor">モーメンタム因子</param>
    public void SetMomentum(bool useMomentum, float factor)
    {
        foreach (NeuralNetworkLayer hidden in HiddenLayer)
        {
            hidden.useMomentum = useMomentum;
            hidden.momentumFactor = factor;
        }
        OutputLayer.useMomentum = useMomentum;
        OutputLayer.momentumFactor = factor;
    }

    /// <summary>
    /// データをファイルに出力する。
    /// </summary>
    public void PrintData(TextFileManager tfm, string testTxt)
    {
        tfm.Write(testTxt, InputLayer.GetData(), true);
        for (int i = 0; i < HiddenLayer.Length; i++)
        {
            tfm.Write(testTxt, i + "番目", true);
            tfm.Write(testTxt, HiddenLayer[i].GetData(), true);
        }
        tfm.Write(testTxt, OutputLayer.GetData(), true);
        tfm.Write(testTxt, "--------------------------------", true);
    }
}
