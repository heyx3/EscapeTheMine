using System;


public struct Vector2i : IEquatable<Vector2i>
{
	public static readonly Vector2i Zero = new Vector2i(0, 0);


	public int x, y;


	public Vector2i(int _x, int _y) { x = _x; y = _y; }
	public Vector2i(UnityEngine.Vector2 v) { x = (int)v.x; y = (int)v.y; }


	public static bool operator==(Vector2i lhs, Vector2i rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}
	public static bool operator!=(Vector2i lhs, Vector2i rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}
	public bool Equals(Vector2i other) { return this == other; }
	
	public static Vector2i operator+(Vector2i lhs, Vector2i rhs)
	{
		return new Vector2i(lhs.x + rhs.x, lhs.y + rhs.y);
	}
	public static Vector2i operator-(Vector2i lhs, Vector2i rhs)
	{
		return new Vector2i(lhs.x - rhs.x, lhs.y - rhs.y);
	}
	public static Vector2i operator*(Vector2i lhs, int rhs)
	{
		return new Vector2i(lhs.x * rhs, lhs.y * rhs);
	}
	public static Vector2i operator/(Vector2i lhs, int rhs)
	{
		return new Vector2i(lhs.x / rhs, lhs.y / rhs);
	}

	
	public Vector2i LessX { get { return new Vector2i(x - 1, y); } }
	public Vector2i LessY { get { return new Vector2i(x, y - 1); } }
	public Vector2i MoreX { get { return new Vector2i(x + 1, y); } }
	public Vector2i MoreY { get { return new Vector2i(x, y + 1); } }


	public bool IsWithin(Vector2i minInclusive, Vector2i maxInclusive)
	{
		return x >= minInclusive.x && y >= minInclusive.y &&
			   x <= maxInclusive.x && y <= maxInclusive.y;
	}

	public float Distance(Vector2i other)
	{
		return UnityEngine.Mathf.Sqrt((float)DistanceSqr(other));
	}
	public int DistanceSqr(Vector2i other)
	{
		int x2 = x - other.x,
			y2 = y - other.y;
		return (x2 * x2) + (y2 * y2);
	}
	public int ManhattanDistance(Vector2i other)
	{
		return Math.Abs(x - other.x) + Math.Abs(y - other.y);
	}

	public float Distance(UnityEngine.Vector2 other)
	{
		return UnityEngine.Vector2.Distance(other, new UnityEngine.Vector2(x, y));
	}
	public float DistanceSqr(UnityEngine.Vector2 other)
	{
		return (other - new UnityEngine.Vector2(x, y)).sqrMagnitude;
	}


	public override string ToString()
	{
		return "{" + x + ", " + y + "}";
	}
	public override int GetHashCode()
	{
		return unchecked(x * 73856093) ^ unchecked(y * 19349663);
	}
	public int GetHashCode(int z)
	{
		return (this * z).GetHashCode();
	}
	public override bool Equals(object obj)
	{
		return (obj is Vector2i) && ((Vector2i)obj) == this;
	}
}