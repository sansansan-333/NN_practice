using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2Dでの追跡運動のためのクラス
/// </summary>
public class Chase2D : MonoBehaviour
{
	[SerializeField]
	private GameController gameController;
	readonly private string name_gameController = "GameController";

	private float MAX_force_rotate = 300;

	[SerializeField, Range(0, 10000f)]
	private float rotate_coe;
	[SerializeField, Range(0, 1f)]
	private float speed_forward;

	public void Initialize()
	{
		gameController = GameObject.Find(name_gameController).GetComponent<GameController>();
	}

	public void DoChase(GameObject target)
	{
		int m;
		float angle, force;
		Vector2 u, v;
		u = this.forward2D();
		v = target.transform.position - this.transform.position;
		v.Normalize();

		if (Vector3.Cross(u, v).z > 0) m = -1;
		else m = 1;

		angle = (float)System.Math.Acos(Clamp(u.x*v.x + u.y*v.y, -1, 1));
		angle *= 90 / (float)System.Math.PI;

		force = angle * m;

		RigidRotate(force);
		RigidRotate(LockInSqr(gameController.field.p1, gameController.field.p2));
		MoveForward();
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

		this.transform.localEulerAngles = new Vector3(curAngle.x, curAngle.y, curAngle.z - rot);
	}

	void MoveForward()
	{
		this.transform.position = (Vector2)this.transform.position + this.forward2D() * speed_forward;
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
		Vector2 u, v = this.forward2D();
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
			else if (a[3].x <= v.x)
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
    /// Mathf.Clampと同様
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
	private float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
			return min;
        }
		else if(value > max)
        {
			return max;
        }
        else
        {
			return value;
        }
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
}
