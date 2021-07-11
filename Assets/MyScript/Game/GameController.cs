using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    /// <summary>
    /// ゲームのフィールド
    /// </summary>
    [System.Serializable]
    public class Field
    {
        public Field(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        /// <summary>
        /// 左上の点
        /// </summary>
        public Vector2 p1;
        /// <summary>
        /// 右下の点
        /// </summary>
        public Vector2 p2;
        /// <summary>
        /// 対角線の長さの二乗
        /// </summary>
        public float diagonal { get { return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y); } }
    }
    [field: SerializeField]
    public Field field { get; private set; }

    private PlayerController player;
    [SerializeField]
    private GameObject unit_folderObject;
    [SerializeField]
    private GameObject unit_prefab;
    private List<UnitController> units;
    public List<GameObject> unitObjects { get; private set; }
    public int NOFunit { get { return units.Count; } }

    [SerializeField]
    private Brain brain1;

    [field: SerializeField]
    public int unit_max_num { get; private set; }

    readonly private string tag_player = "Fish_Player";
    readonly private string tag_unit = "Fish_Enemy";

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(tag_player).GetComponent<PlayerController>();
        unitObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag_unit));
        units = new List<UnitController>();
        foreach (GameObject unitObj in unitObjects)
        {
            units.Add(unitObj.GetComponent<UnitController>());
        }

        // Init
        player.ManagedStart();
        foreach (UnitController unit in units)
        {
            unit.ManagedStart();
        }
        brain1.Initialize();
    }

    void Update()
    {
        if (units.Count < unit_max_num)
        {
            float unitPosX = Random.Range(field.p1.x, field.p2.x);
            float unitPosY = Random.Range(field.p2.y, field.p1.y);
            GenerateUnit(new Vector2(unitPosX, unitPosY), brain1);
        }
    }

    /// <summary>
    /// Playerが範囲内のunitを攻撃する
    /// </summary>
    /// <remark>PlayerControllerで呼ぶ</remark>
    public void PlayerAttack()
    {
        List<UnitController> unitsInRange = new List<UnitController>();
        foreach (UnitController unit in units)
        {
            float dist = (player.transform.position - unit.transform.position).sqrMagnitude;
            if (player.atk_range*player.atk_range > dist)
            {
                unitsInRange.Add(unit);
            }
        }
        foreach (UnitController unit in unitsInRange)
        {
            unit.Damaged(player.atk);
            player.Recover();
        }
    }

    /// <summary>
    /// 範囲内にPlayerがいればPlayerを攻撃する
    /// </summary>
    /// <param name="unit_attacker">攻撃を仕掛けるUnit</param>
    /// <returns>攻撃成功時true、失敗時false</returns>
    /// <remark>UnitControllerで呼ぶ</remark>
    public bool UnitAttack(UnitController unit_attacker)
    {
        float dist = (player.transform.position - unit_attacker.transform.position).sqrMagnitude;
        if (unit_attacker.atk_range*unit_attacker.atk_range > dist)
        {
            player.Damaged(unit_attacker.atk);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 指定されたUnitを削除する
    /// </summary>
    /// <param name="unit_destroyed">削除するUnit</param>
    public void DestroyUnit(UnitController unit_destroyed)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == unit_destroyed)
            {
                units.Remove(unit_destroyed);
                unitObjects.Remove(unit_destroyed.gameObject);
                Destroy(unit_destroyed.gameObject);
                return;
            }
        }
    }

    /// <summary>
    /// Unitを生成する
    /// </summary>
    /// <param name="position">生成するグローバル座標</param>
    /// <param name="brain">セットするBrain</param>
    private void GenerateUnit(Vector2 position, Brain brain)
    {
        GameObject unit_object_new;
        UnitController unit_new;
        unit_object_new =  Instantiate(unit_prefab, position, Quaternion.identity, unit_folderObject.transform);
        unit_new = unit_object_new.GetComponent<UnitController>();
        unit_new.ManagedStart();
        unit_new.SetBrain(brain);
        units.Add(unit_object_new.GetComponent<UnitController>());
    }
}
