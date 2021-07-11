using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainTest : MonoBehaviour
{
    private NeuralNetwork NN;
    [SerializeField]
    private float learningRate;
    [SerializeField]
    private float momentum;

    private int count=0;
    private int span=100;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        NN = GetComponent<NeuralNetwork>();
        NN.Initialize(3, new int[] { 3 }, 3);
        NN.SetLerningRate(learningRate);
        NN.SetMomentum(true, momentum);
    }

    private void Update()
    {
        
        count++;
        if (count > span) {
            count = 0;
            NN.SetInput(0, 1);
            NN.SetInput(1, 2);
            NN.SetInput(2, 3);

            NN.SetDesiredOutput(0, 2);
            NN.SetDesiredOutput(1, 2);
            NN.SetDesiredOutput(2, 3);

            NN.FeedForward();
            NN.BackPropagate();

            //Debug.Log(NN.GetOutput(0));
            //Debug.Log(NN.GetOutput(1));
            //Debug.Log(NN.GetOutput(2));

            Debug.Log(NN.GetMaxOutputID());
            Debug.Log("s");
        }
    }
}
