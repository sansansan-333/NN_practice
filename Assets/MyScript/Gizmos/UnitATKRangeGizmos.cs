using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitATKRangeGizmos : MonoBehaviour
{
    [SerializeField]
    private UnitController unit;
    [SerializeField]
    private bool use;

    private void OnDrawGizmos()
    {
        if (use) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, unit.atk_range);
        }
    }
}
