using System;
using UnityEngine;


public static class MyMath
{
	public static bool IsNearZero(this float f, float error) { return Math.Abs(f) <= error; }
	public static bool IsNearEqual(this float f, float f2, float error) { return Math.Abs(f - f2) <= error; }
	
	public static float Sign(this float f) { return (f > 0.0f ? 1.0f : (f < 0.0f ? -1.0f : 0.0f)); }
	public static int SignI(this float f) { return (f > 0.0f ? 1 : (f < 0.0f ? -1 : 0)); }


	public static Vector2 Mult(this Vector2 a, Vector2 b) { return new Vector2(a.x * b.x, a.y * b.y); }


	public static Rect Union(this Rect r, Rect r2) { return Rect.MinMaxRect(Math.Min(r.xMin, r2.xMin),
																			Math.Min(r.yMin, r2.yMin),
																			Math.Max(r.xMax, r2.xMax),
																			Math.Max(r.yMax, r2.yMax)); }

	public static string ToString(this Vector2 v, int nDecimals)
	{
		return "{" + Math.Round(v.x, nDecimals) + ", " + Math.Round(v.y, nDecimals) + "}";
	}
}