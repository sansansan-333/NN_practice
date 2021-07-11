using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject camera_main;
    [SerializeField]
    private Slider hp_bar;
    [SerializeField]
    private GameController gameController;

    private float hp;
    [SerializeField]
    private float hp_MAX;
    [SerializeField]
    private float hp_recover;
    [SerializeField, Range(0, 0.1f)]
    private float speed;

    [field: SerializeField]
    public float atk { get; private set; }
    [field: SerializeField, Range(1f, 10f)]
    public float atk_range { get; private set; }
    private float atk_span=0.1f;
    private float time=0;

    public void ManagedStart()
    {
        hp = hp_MAX;
        hp_bar.maxValue = hp_MAX;
        hp_bar.value = hp_MAX;
    }

    void Update()
    {
        // 移動
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = System.Math.Abs(this.transform.position.z - camera_main.transform.position.z);
        if (Input.GetMouseButton(0)) {
            GoTo(Camera.main.ScreenToWorldPoint(mousePos), speed);
        }

        // 攻撃
        time += Time.deltaTime;
        if(time >= atk_span)
        {
            time = atk_span;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                time = 0;
                gameController.PlayerAttack();
            }
        }
    }

    /// <summary>
    /// 目的地に向かう
    /// </summary>
    /// <param name="position">目的地</param>
    /// <param name="speed">スピード</param>
    private void GoTo(Vector2 position, float speed)
    {
        // 回転
        Vector2 dir = position - (Vector2)this.transform.position;
        dir = new Vector2(dir.y, -dir.x);
        dir.Normalize();
        float rotZ = (float)(System.Math.Acos(dir.x) * 180f / System.Math.PI);
        rotZ *= dir.y < 0 ? -1 : 1;
        Vector3 rot = this.transform.localEulerAngles;
        this.transform.localEulerAngles = new Vector3(rot.x, rot.y, rotZ);

        // 移動
        this.transform.position = Vector2.Lerp(this.transform.position, position ,speed);
    }

    public void Damaged(float damage)
    {
        hp -= damage;
        hp_bar.value = hp;
    }

    public void Recover()
    {
        hp += hp_recover;
        if (hp > hp_MAX)
        {
            hp = hp_MAX;
        }
    }
}
