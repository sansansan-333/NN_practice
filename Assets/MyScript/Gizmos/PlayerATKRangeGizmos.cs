using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerATKRangeGizmos : MonoBehaviour
{
    [SerializeField]
    private PlayerController player;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, player.atk_range);
    }
}
