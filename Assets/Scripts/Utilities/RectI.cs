using System;


public struct RectI
{
	/// <summary>
	/// The position of the Min corner of the rectangle.
	/// </summary>
	public int MinX, MinY;

	/// <summary>
	/// The size of the rectangle.
	/// </summary>
	public int SizeX, SizeY;


	/// <summary>
	/// The X position of the *inclusive* max edge of the rectangle.
	/// </summary>
	public int MaxX { get { return MinX + SizeX - 1; } }
	/// <summary>
	/// The Y position of the *inclusive* max edge of the rectangle.
	/// </summary>
	public int MaxY { get { return MinY + SizeY - 1; } }

	public Vector2i MinXY { get { return new Vector2i(MinX, MinY); } }
	public Vector2i MaxXY { get { return new Vector2i(MaxX, MaxY); } }
	public Vector2i MinXMaxY { get { return new Vector2i(MinX, MaxY); } }
	public Vector2i MaxXMinY { get { return new Vector2i(MaxX, MinY); } }

	public Vector2i Center { get { return new Vector2i(MinX + (SizeX / 2), MinY + (SizeY / 2)); } }


	public RectI(int minX, int minY, int sizeX, int sizeY)
	{
		MinX = minX;
		MinY = minY;
		SizeX = sizeX;
		SizeY = sizeY;
	}
	/// <summary>
	/// Note that the max corner is exclusive.
	/// </summary>
	public RectI(Vector2i minCorner, Vector2i maxCorner)
	{
		MinX = minCorner.x;
		MinY = minCorner.y;
		SizeX = (maxCorner.x - minCorner.x);
		SizeY = (maxCorner.y - minCorner.y);
	}


	public RectI Union(RectI other)
	{
		int minX = (MinX < other.MinX) ? MinX : other.MinX,
			minY = (MinY < other.MinY) ? MinY : other.MinY,
			maxX = (MaxX < other.MaxX) ? MaxX : other.MaxX,
			maxY = (MaxY < other.MaxY) ? MaxY : other.MaxY;

		return new RectI(minX, minY, maxX - minX + 1, maxY - minY + 1);
	}

	public bool ContainsX(int xPos)
	{
		return xPos >= MinX && xPos <= MaxX;
	}
	public bool ContainsY(int yPos)
	{
		return yPos >= MinY && yPos <= MaxY;
	}
	public bool Contains(Vector2i point) { return ContainsX(point.x) && ContainsY(point.y); }

	public bool Touches(RectI r)
	{
		return (r.ContainsX(MinX) || ContainsX(r.MinX)) &&
			   (r.ContainsY(MinY) || ContainsY(r.MinY));
	}

	public Vector2i.Iterator GetEnumerator()
	{
		return new Vector2i.Iterator(MinXY, MaxXY + 1);
	}
}