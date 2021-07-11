using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextFileManager = UtilScript.TextFileManager;

public class Brain : MonoBehaviour
{
    [SerializeField]
    private GameController gameController;

    private NeuralNetwork NN;
    [SerializeField]
    private float learningRate;
    [SerializeField]
    private float momentum;

    private TextFileManager tfm;
    readonly private string log_fileName = "debug1.txt";
    private int updateCount=0;

    public enum Actions
    {
        Flock,
        Chase,
        Evade
    }

    private void Start()
    {
        tfm = new TextFileManager(Application.dataPath + "/Log");
        tfm.Create(log_fileName);
        NN.PrintData(tfm, log_fileName);
        Debug.Log("Output");
    }

    private void Update()
    {
        if (updateCount % 100 == 0) {
            updateCount = 0;
        }
        updateCount++;
    }

    public class DataForBrainInput
    {
        public int NOFunit;
        public float hp;
        public float hp_MAX;
        public float distance;

        public void SetData(int NOFunit, float hp, float hp_MAX, float distance)
        {
            this.NOFunit = NOFunit;
            this.hp = hp;
            this.hp_MAX = hp_MAX;
            this.distance = distance;
        }
    }

    public void Initialize()
    {
        NN = GetComponent<NeuralNetwork>();
        NN.Initialize(3, new int[] {3}, 3);
        NN.SetLerningRate(learningRate);
        NN.SetMomentum(true, momentum);
    }

    /// <summary>
    /// ニューラルネットワークを使って行動を決定する
    /// </summary>
    /// <param name="NOFunit">今のunitの数</param>
    /// <param name="hp">自身のhp</param>
    /// <param name="hp_MAX">自身の最大hp</param>
    /// <param name="distance">プレイヤーとの距離</param>
    /// <returns>行動</returns>
    public Actions GetAction(DataForBrainInput dataForBrainInput)
    {
        int scaledNOFunit = dataForBrainInput.NOFunit / gameController.unit_max_num;
        float scaledHP = dataForBrainInput.hp / dataForBrainInput.hp_MAX;
        float scaledDistance = dataForBrainInput.distance / gameController.field.diagonal;
        NN.SetInput(0, scaledNOFunit);
        NN.SetInput(1, scaledHP);
        NN.SetInput(2, scaledDistance);

        NN.FeedForward();
        
        switch (NN.GetMaxOutputID())
        {
            case 0:
                return Actions.Flock;
            case 1:
                return Actions.Chase;
            case 2:
                return Actions.Evade;
            default:
                Debug.LogError("Max output id doesn't match any of Actions.");
                return Actions.Flock;
        }
    }

    public void ReTrain(DataForBrainInput dataForBrainInput, float[] desiredValues)
    {
        /// setinput
        /// setdesired
        /// feedforward
        /// backpropagate

        if (desiredValues.Length != 3)
        {
            Debug.LogError("The length of desiredValue must be 3");
            return;
        }

        int scaledNOFunit = dataForBrainInput.NOFunit / gameController.unit_max_num;
        float scaledHP = dataForBrainInput.hp / dataForBrainInput.hp_MAX;
        float scaledDistance = dataForBrainInput.distance / gameController.field.diagonal;
        NN.SetInput(0, scaledNOFunit);
        NN.SetInput(1, scaledHP);
        NN.SetInput(2, scaledDistance);

        for (int i = 0; i < 3; i++) {
            NN.SetDesiredOutput(i, desiredValues[i]);
        }

        NN.FeedForward();

        NN.NormalizeOutput();

        NN.BackPropagate();

        NN.PrintData(tfm, log_fileName);
        Debug.Log("Output");
    }
}
