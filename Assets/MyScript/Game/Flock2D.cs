using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// TODO

/// <summary>
/// 2Dでの群衆アルゴリズムクラス
/// </summary>
/// <remarks>中の関数も全て2D用</remarks>
public class Flock2D : MonoBehaviour
{
	private GameController gameController;
	readonly private string name_gameController = "GameController";

	List<GameObject> adj_obj = new List<GameObject>();
	GameObject[] obj_tmp;

	private float MAX_force_rotate = 300;
	[SerializeField, Range(-100, 100)]
	float force_rotate = 0;
	[SerializeField, Range(0, 1000)]
	private float rotate_coe = 100;

	[SerializeField, Range(0, 1)]
	float speed_forward;

	// 視界の為のパラメータ
	[SerializeField, Range(0, 180)]
	float view_angle = 90;
	[SerializeField, Range(0, 100)]
	float view_radius = 0;
	private float scale_monster;

	// 速度ベクトル計算のための変数
	private Vector3 prepos;
	private Vector3 curpos;
	public Vector3 velocity { get; set; }

	// チーム分け
	public string team = null;

	[SerializeField]
	private bool deb = true;

	public void Initialize()
    {
		gameController = GameObject.Find(name_gameController).GetComponent<GameController>();

		scale_monster = Mathf.Sqrt(Mathf.Pow(this.transform.localScale.x, 2) + Mathf.Pow(this.transform.localScale.y, 2)) / Mathf.Sqrt(2);

		prepos = this.transform.position;
		velocity = Vector3.zero;
	}

	public void DoFlock()
	{
		obj_tmp = gameController.unitObjects.ToArray();

		//　それぞれ自分じゃないか,視野内にいるか(それに加えて同じチームか)を判定してadj_objに入れる
		foreach (GameObject element in obj_tmp)
		{
			if (element != this.gameObject && IsInView(element, view_angle, view_radius))
			{
				if (this.team == element.GetComponent<Flock2D>().team)
				{
					adj_obj.Add(element);
				}
			}
		}
		// 操舵力を計算
		if (adj_obj.Count > 0)
		{
			force_rotate += Cohension(adj_obj);
			force_rotate += Alignment(adj_obj);
			force_rotate += Separation(adj_obj);
			// 動かす
			RigidRotate(force_rotate);
		}
		else
		// 視界にunitがいなければleader
		{
			RotateAsLeader();
		}
		RigidRotate(LockInSqr(gameController.field.p1, gameController.field.p2));
		MoveForward();

		adj_obj.Clear();
		force_rotate = 0;

		// Alignnmentのためのvelocity計算
		curpos = this.transform.position;
		velocity = (curpos - prepos) / Time.deltaTime;
		prepos = curpos;
	}

	void RigidRotate(float f)
	{
		float rot;
		float c1, c2, c3;
		float t = Time.deltaTime;

		Vector3 curAngle = this.transform.localEulerAngles;

		c1 = 3 / 2 * f / 27f;
		c2 = 0;
		c3 = 0;

		rot = c1 * Mathf.Pow(t, 2) + c2 * t + c3;
		rot *= rotate_coe;

		//Debug.Log(rot);

		this.transform.localEulerAngles = new Vector3(curAngle.x, curAngle.y, curAngle.z - rot);
	}

	void MoveForward()
	{
		this.transform.position = (Vector2)this.transform.position + this.forward2D() * speed_forward;
	}

	void RotateAsLeader()
	{
		if (this.team == "right")
		{
			RigidRotate(MAX_force_rotate * 0.1f);
		}
		if (this.team == "left")
		{
			RigidRotate(-MAX_force_rotate * 0.1f);
		}
	}

	//　自分の視野内にobjがいるならtrue
	bool IsInView(GameObject obj, float angle, float radius)
	{
		float theta;
		Vector2 u, v;
		u = this.forward2D();
		v = obj.transform.position - this.transform.position;
		// 半径より外にいた場合
		if (mag(v) > radius * scale_monster)
		{
			return false;
		}

		u.Normalize();
		v.Normalize();

		theta = Mathf.Acos(Vector2.Dot(u, v));
		//Debug.Log(theta);
		if (theta < angle * Mathf.PI / 180)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	// 結合:視野内のunitの平均位置に向かわせる
	float Cohension(List<GameObject> objects)
	{
		float force = 0;

		Vector2 avgPos = Vector2.zero;
		Vector2 u, v;
		float theta, d;
		int m = 1; // 方向係数 -1 or 1

		// 平均位置計算
		foreach (GameObject obj in objects)
		{
			avgPos += (Vector2)obj.transform.position;
		}
		avgPos /= objects.Count;

		v = this.forward2D();
		u = avgPos - (Vector2)this.transform.position;

		//Debug.Log(v);
		//Debug.Log(Vector3.Cross(v, u));
		// 平均位置が自分の左側だったら
		if (Vector3.Cross(v, u).z > 0)
		{
			m = -1;
		}

		v.Normalize();
		u.Normalize();

		d = Mathf.Clamp(Vector2.Dot(u, v), -1, 1);
		theta = Mathf.Acos(d);
		force = m * MAX_force_rotate * (theta / Mathf.PI);

		return force;
	}

	// 整列:視野内のunitの平均的な方向に向かわせる
	float Alignment(List<GameObject> objects)
	{
		float force;

		Vector2 u, v;
		Vector2 avgVelo = Vector2.zero;

		float theta, d;
		int m = 1; // 方向係数 -1 or 1

		// 平均速度ベクトル計算
		foreach (GameObject element in objects)
		{
			avgVelo += (Vector2)element.GetComponent<Flock2D>().velocity;
		}
		avgVelo /= objects.Count;

		u = avgVelo.normalized;
		v = this.forward2D();

		// 平均位置が自分の左側だったら
		if (Vector3.Cross(v, u).z > 0)
		{
			m = -1;
		}

		// Mathf.Acos(Vector3.Dot(u, v))だと|u|=|v|=1なのにNaNを返すことがあったので修正した
		d = Mathf.Clamp(Vector3.Dot(u, v), -1, 1);
		theta = Mathf.Acos(d);
		force = m * MAX_force_rotate * (theta / Mathf.PI);

		return force;
	}

	// 分離:近づきすぎたunitから離れる
	float Separation(List<GameObject> objects)
	{
		float force = 0;

		Vector2 u, v;
		int m = 0;
		float dist, miniview_radius = view_radius / 20;

		v = this.forward2D();

		foreach (GameObject element in objects)
		{
			if (IsInView(element, view_angle, miniview_radius))
			{
				u = element.transform.position - this.transform.position;
				dist = mag(u);

				if (Vector3.Cross(v, u).z < 0)
				{
					m = -1;
				}
				else if (Vector3.Cross(v, u).z > 0)
				{
					m = 1;
				}
				else
				{
					m = 0;
				}

				force += m * MAX_force_rotate * (miniview_radius / dist);
			}
		}
		return force;
	}

	/// <summary>
	/// このオブジェクトが向いている方向の単位ベクトルを返す
	/// </summary>
	/// <returns>向いている方向の単位ベクトル</returns>
	private Vector2 forward2D()
    {
		float x, y, theta;
		Vector2 forward; // 直進方向の単位ベクトル

		theta = this.transform.localEulerAngles.z * Mathf.PI / 180f;
		x = -Mathf.Sin(theta);
		y = Mathf.Cos(theta);
		forward = new Vector2(x, y).normalized;

		return forward;
	}

	/// <summary>
    /// 四角の箱に閉じ込めるための操舵力を計算する
    /// </summary>
    /// <param name="p1">箱の右上の座標</param>
    /// <param name="p2">箱の左下の座標</param>
    /// <returns>操舵力</returns>
	private float LockInSqr(Vector2 p1, Vector2 p2)
    {
		float force, m;
		Vector2 u, v=this.forward2D();
		Vector2 curpos = this.transform.position;

		Vector2[] a = new Vector2[4];

		int wall;
        if (v.y > 0)
        {

			a[0] = p1 - curpos;
			a[3] = new Vector2(p2.x, p1.y) - curpos;
			a[0] /= mag(a[0]);
			a[3] /= mag(a[3]);
			if (a[0].x <= v.x && v.x <= a[3].x)
            {
				wall = 0;
            }
            else if(a[3].x <= v.x)
            {
				wall = 3;
            }
            else
            {
				wall = 1;
            }
        }
		else
		{
			a[1] = new Vector2(p1.x, p2.y) - curpos;
			a[2] = p2 - curpos;
			a[1] /= mag(a[1]);
			a[2] /= mag(a[2]);
			if (a[1].x <= v.x && v.x <= a[2].x)
            {
				wall = 2;
            }
			else if (a[2].x <= v.x)
            {
				wall = 3;
            }
            else
            {
				wall = 1;
            }
        }

		float dist;
		Vector2[] d = new Vector2[4];
		d[1] = new Vector2(p1.x - curpos.x, 0);
		d[3] = new Vector2(p2.x - curpos.x, 0);
		d[0] = new Vector2(0, p1.y - curpos.y);
		d[2] = new Vector2(0, p2.y - curpos.y);

		dist = mag(d[wall]);
		u = d[wall];
		if (Vector3.Cross(v, u).z < 0) m = -1;
		else if (Vector3.Cross(v, u).z > 0) m = -1;
		else m = -1;

		force = m * MAX_force_rotate / dist * 1.5f;

		return force;
    }

	/// <summary>
	/// ベクトルの長さを返す
	/// </summary>
	/// <param name="v">ベクトル</param>
	/// <returns>ベクトルの長さ</returns>
    /// <remarks>Vector2.magnitudeの負荷軽減用</remarks>
	private float mag(Vector2 v)
    {
		return (float)System.Math.Sqrt(v.x * v.x + v.y * v.y);
    }

	/// <summary>
	/// ベクトルの長さの二乗を返す
	/// </summary>
	/// <param name="v">ベクトル</param>
	/// <returns>クトルの長さの二乗</returns>
	/// <remarks>Vector2.sqrMagnitudeの負荷軽減用</remarks>
	private float sqrMag(Vector2 v)
    {
		return v.x * v.x + v.y * v.y;
	}
}
