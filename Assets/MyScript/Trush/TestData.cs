using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu]
public class TestData : ScriptableObject
{
    [SerializeField]
    public List<Data> datas = new List<Data>();
}

[System.Serializable]
public class Data
{
    public float[] input;
    public float[] desiredValue;
}
