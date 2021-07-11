using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
//〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓★
//  　┏┓
//  　┃┃　つ┃ぎ┃に┃や┃る┃こ┃と┃！┃！┃！┃
//  　┃┃　━┛━┛━┛━┛━┛━┛━┛━┛━┛━┛
//  　┃┣┳┳┓
// 　┏┫┃┃┃┃　
//  ┃　┗┻┻┫　~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  ┃　　　┃　
//  ┗┓　 ┏┛　~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//出力層の活性化関数には恒等関数かシグモイド関数、中間層の活性化関数にはReLUしか実装してない
// z = weightedSum + biasValue*biasWeight
// 多値分類問題には使えないと思う。交差エントロピー誤差とソフトマックス関数を実装しないとね
//〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓★

/// <summary>
/// ニューラルネットワークの層クラス
/// </summary>
public class NeuralNetworkLayer : MonoBehaviour
{
    public int NOFnodes; // 自身のニューロンの数
    public int NOFchildNodes; // 子層のニューロンの数
    public int NOFparentNodes; // 親層のニューロンの数
    public float[,] weights; // 親層からの重み
    public float[,] gradients; // 各重みに対する損失関数の勾配
    public float[,] weightChanges; // 前回のエポックで得られた重みの調節値(Δw')
    public float[] neuronValues; // 活性化されたニューロンの値
    public float[] weightedSums; // 活性化される前の重み付き和(バイアスを足す前の値)
    public float[] desiredValues; // 理想(target)値(出力層のみ)
    public float[] delta; // 損失関数の各ニューロンの値に対する傾き
    public float[] biasWeights; // バイアスの重み(中間層のみ)
    public float[] biasValues; // バイアスの値(中間層のみ)
    public float[] biasGradients; // バイアスの重みに対する損失関数の勾配(中間層のみ)
    public float learningRate; // 学習率ε

    public bool linerOutputActivation; // 出力層の活性化関数に恒等関数を使うためのフラグ
    public bool sigmoidOutputActivation; // 出力層の活性化関数にシグモイド関数を使うためのフラグ
    public bool softmaxOutoutActivation; // 出力層の活性化関数にソフトマックス関数を使うためのフラグ
    public bool ReLUActivation; // 中間層の活性化関数にReLUを使うためのフラグ

    public bool msError; // 平均二乗誤差(mean squared error)を使うためのフラグ
    public bool ceError; // クロスエントロピー誤差を使うためのフラグ
    public bool useMomentum; // モーメンタムを使用するためのフラグ
    public float momentumFactor; // モーメンタム因子α

    //-- 親層 > この層 > 子層 --//
    public NeuralNetworkLayer parentLayer; // 親層
    public NeuralNetworkLayer childLayer; // 子層
    public bool isInputLayer; // 入力層であるか
    public bool isOutputLayer; // 出力層であるか

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public NeuralNetworkLayer()
    {
        parentLayer = null;
        childLayer = null;
        linerOutputActivation = true;
        ReLUActivation = true;
        msError = true;
        useMomentum = false;
        momentumFactor = 0.9f;
    }

    /// <summary>
    /// 層を初期化する。
    /// </summary>
    /// <param name="NumberOfNodes">ニューロンの数</param>
    /// <param name="parent">親層。なければnull</param>
    /// <param name="child">子層。なければnull</param>
    public void Initialize(int NumberOfNodes, NeuralNetworkLayer parent, NeuralNetworkLayer child)
    {
        if(NumberOfNodes <= 0)
        {
            Debug.LogError("The number of nodes must be more than 0.");
            return;
        }
        NOFnodes = NumberOfNodes;

        if (parent == null)
        {
            isInputLayer = true;
        }
        else
        {
            isInputLayer = false;
        }
        if (child == null)
        {
            isOutputLayer = true;
        }
        else
        {
            isOutputLayer = false;
        }

        // レイヤーの代入、必要な配列の初期化
        neuronValues = new float[NOFnodes];
        if (this.isInputLayer)
        {
            childLayer = child;
        }
        else if (this.isOutputLayer)
        {
            parentLayer = parent;

            weightedSums = new float[NOFnodes];
            desiredValues = new float[NOFnodes];
            weights = new float[NOFparentNodes, NOFnodes];
            gradients = new float[NOFparentNodes, NOFnodes];
            weightChanges = new float[NOFparentNodes, NOFnodes];
            delta = new float[NOFnodes];
        }
        else
        {
            parentLayer = parent;
            childLayer = child;

            weightedSums = new float[NOFnodes];
            weights = new float[NOFparentNodes, NOFnodes];
            gradients = new float[NOFparentNodes, NOFnodes];
            weightChanges = new float[NOFparentNodes, NOFnodes];
            biasWeights = new float[NOFnodes];
            biasValues = new float[NOFnodes];
            biasGradients = new float[NOFnodes];
            delta = new float[NOFnodes];

            for (int i = 0; i < NOFnodes; i++)
            {
                biasValues[i] = 1;
            }
        }

    }

    /// <summary>
    /// 重みをランダムで初期化する。
    /// </summary>
    public void RandomizeWeights()
    {
        // 0 ~ +1の範囲で初期化する

        if (!this.isInputLayer) {
            for (int i = 0; i < NOFparentNodes; i++)
            {
                for (int j = 0; j < NOFnodes; j++)
                {
                    weights[i,j] = Random.Range(0f, 1f);
                }
            }
        }
        // バイアスの重み
        if (!(this.isInputLayer || this.isOutputLayer))
        {
            for (int i = 0; i < NOFnodes; i++)
            {
                biasWeights[i] = Random.Range(0f, 1f);
            }
        }

    }

    /// <summary>
    /// 損失関数の重みに対する傾きを計算する。
    /// </summary>
    /// <remarks>
    /// deltaも計算する。
    /// </remarks>
    public void CalculateGradients()
    {
        // 出力層
        if (this.isOutputLayer)
        {
            for (int i = 0; i < parentLayer.NOFnodes; i++)
            {
                for (int j = 0; j < NOFnodes; j++)
                {
                    delta[j] = neuronValues[j] - desiredValues[j];
                    gradients[i, j] = delta[j]
                        * AFprimeOutput(weightedSums[j])
                        * parentLayer.neuronValues[i];
                }
            }
            return;
        }
        // 中間層
        else if(!this.isInputLayer)
        {
            for (int i = 0; i < parentLayer.NOFnodes; i++)
            {
                for (int j = 0; j < NOFnodes; j++)
                {
                    delta[j] = 0;
                    float hPrimeZ = AFprimeHidden(weightedSums[j] + biasValues[j] * biasWeights[j]);
                    for (int k = 0; k < childLayer.NOFnodes; k++)
                    {
                        delta[j] += childLayer.delta[k] * childLayer.weights[j, k] * hPrimeZ; // δ
                    }
                    gradients[i, j] = delta[j] * parentLayer.neuronValues[i]; // ニューロン間の重みに対する勾配
                }
            }

            for (int j = 0; j < NOFnodes; j++)
            {
                biasGradients[j] = biasValues[j] * delta[j]; // バイアスの重みに対する勾配
            }

            return;
        }

        Debug.LogError("Error");
    }

    /// <summary>
    /// 重みを調整する。
    /// </summary>
    public void AdjustWeights()
    {
        if (!this.isInputLayer)
        {
            float dw;
            for (int j = 0; j < NOFnodes; j++)
            {
                for (int i = 0; i < NOFparentNodes; i++)
                {
                    dw = learningRate * gradients[i,j]; 
                    if (useMomentum)
                    {
                        weights[i, j] -= dw + momentumFactor * weightChanges[i, j];
                        weightChanges[i, j] = dw;
                    }
                    else
                    {
                        weights[i, j] -= dw;
                    }
                }
                if (!this.isOutputLayer) {
                    biasWeights[j] -= learningRate * biasGradients[j]; // バイアスの重み
                }
            }
        }
        else
        {
            Debug.LogError("Error");
        }
    }

    /// <summary>
    /// ニューロンの値を計算する。
    /// </summary>
    /// <remarks>
    /// weightedSumsとneuronValuesを計算する。
    /// </remarks>
    public void CalculateNeuronValues()
    {
        if (!this.isInputLayer) {
            for (int j = 0; j < NOFnodes; j++)
            {
                weightedSums[j] = 0;
                for (int i = 0; i < parentLayer.NOFnodes; i++)
                {
                    weightedSums[j] += parentLayer.neuronValues[i] * weights[i, j];
                }

                if (this.isOutputLayer)
                {
                    neuronValues[j] = AFoutput(weightedSums[j]);
                }
                else
                {
                    neuronValues[j] = AFhidden(weightedSums[j] + biasValues[j] * biasWeights[j]);
                }
            }
        }
        else
        {
            Debug.LogError("Error");
        }

    }

    /// <summary>
    /// 層のデータを文字列で返す
    /// </summary>
    /// <returns>データの文字列</returns>
    /// <remarks>
    /// デバッグ用
    /// </remarks>
    public string GetData()
    {
        //string result = "";
        StringBuilder result = new StringBuilder();

        if (this.isInputLayer)
        {
            result.Append("<< 入力層 >>" + "\n");
            result.Append("ニューロンの数 : " + NOFnodes + "\n");
            result.Append("子層ニューロンの数 : " + NOFchildNodes + "\n");

            result.Append("ニューロンの値\n");
            for (int i = 0; i < NOFnodes; i++)
            {
                result.Append("[" + i + "] : " + neuronValues[i] + "\n");
            }
        }
        else if (this.isOutputLayer)
        {
            result.Append("<< 出力層 >>" + "\n");
            result.Append("親層ニューロンの数 : " + NOFparentNodes + "\n");
            result.Append("ニューロンの数 : " + NOFnodes + "\n");

            result.Append("ニューロンの値\n");
            for (int i = 0; i < NOFnodes; i++)
            {
                result.Append("[" + i + "] : " + neuronValues[i] + "\n");
            }
            result.Append("理想値\n");
            for (int i = 0; i < NOFnodes; i++)
            {
                result.Append("[" + i + "] : " + desiredValues[i] + "\n");
            }
            result.Append("重み、勾配\n");
            for (int j = 0; j < NOFnodes; j++)
            {
                for (int i = 0; i < NOFparentNodes; i++)
                {
                    result.Append("[" + i + "-" + j + "] : ");
                    result.Append((weights[i, j] > 0) ? " " : "");
                    result.Append(weights[i,j] + ", "+ gradients[i, j] + "\n");
                }
            }
        }
        else
        {
            result.Append("<< 中間層 >>" + "\n");
            result.Append("親層ニューロンの数 : " + NOFparentNodes + "\n");
            result.Append("ニューロンの数 : " + NOFnodes + "\n");
            result.Append("子層ニューロンの数 : " + NOFchildNodes + "\n");

            result.Append("ニューロンの値\n");
            for (int i = 0; i < NOFnodes; i++)
            {
                result.Append("[" + i + "] : " + neuronValues[i] + "\n");
            }
            result.Append("重み、勾配\n");
            for (int j = 0; j < NOFnodes; j++)
            {
                for (int i = 0; i < NOFparentNodes; i++)
                {
                    result.Append("[" + i + "-" + j + "] : ");
                    result.Append((weights[i, j] > 0) ? " " : "");
                    result.Append(weights[i, j] + ", " + gradients[i, j] + "\n");
                }
            }
            result.Append("バイアスの値\n");
            for (int i = 0; i < NOFnodes; i++)
            {
                result.Append("[" + i + "] : " + biasValues[i] + "\n");
            }
            result.Append("バイアスの重み、勾配\n");
            for (int j = 0; j < NOFnodes; j++)
            {
                result.Append("[" + j + "] : ");
                result.Append((biasWeights[j] > 0) ? " " : "");
                result.Append(biasWeights[j] + ", " + biasGradients[j] + "\n");
            }
        }

        return result.ToString();
    }

    // ---- 以下private関数 ---- //


    /// <summary>
    /// 中間層の活性化関数
    /// </summary>
    /// <param name="z">活性化する値</param>
    /// <returns>活性化された値</returns>
    private float AFhidden(float z)
    {
        float a;
        // ReLU
        if (ReLUActivation)
        {
            if (z > 0)
            {
                a = z;
            }
            else
            {
                a = 0;
            }
            return a;
        }

        Debug.LogError("No activation function is used in hidden layer.");
        return 0;
    }

    /// <summary>
    /// 中間層の活性化関数の導関数
    /// </summary>
    /// <param name="z"></param>
    /// <returns>活性化関数の傾き</returns>
    private float AFprimeHidden(float z)
    {
        float d;
        // ReLU
        if (ReLUActivation)
        {
            if (z > 0)
            {
                d = 1;
            }
            else
            {
                d = 0;
            }
            return d;
        }

        Debug.LogError("No activation function is used in hidden layer.");
        return 0;
    }

    /// <summary>
    /// 出力層の活性化関数
    /// </summary>
    /// <param name="z">活性化する値</param>
    /// <returns>活性化された値</returns>
    private float AFoutput(float z)
    {
        float a;
        // 恒等関数
        if (linerOutputActivation)
        {
            a = z;
            return a;
        }
        // シグモイド関数
        if (sigmoidOutputActivation)
        {
            a = 1 / (1 + Mathf.Exp(-z));
            return a;
        }

        Debug.LogError("No activation function is used in output layer.");
        return 0;
    }

    /// <summary>
    /// 出力層の活性化関数の導関数
    /// </summary>
    /// <param name="z"></param>
    /// <returns>活性化関数の傾き</returns>
    private float AFprimeOutput(float z)
    {
        float d;
        // 恒等関数
        if (linerOutputActivation)
        {
            d = 1;
            return d;
        }
        // シグモイド関数
        if (sigmoidOutputActivation)
        {
            d = AFoutput(z) * (1 - AFoutput(z));
            return d;
        }

        Debug.LogError("No activation function is used in output layer.");
        return 0;
    }


}
