using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GameLogic
{
	public enum TileTypes
	{
		Empty = 0,
		Wall,
		Entrance, Exit,
	}


	public class TileGrid : MyData.IReadWritable
	{
		/// <summary>
		/// Raised when a tile gets changed.
		/// The parameters are the grid, position, old value, and new value, respectively.
		/// Note that any information about the subscribers to this event does not get serialized.
		/// </summary>
		public event Action<TileGrid, Vector2i, TileTypes, TileTypes> OnTileChanged;
		/// <summary>
		/// Raised when an instance gets its grid resized.
		/// The parameters are the grid, old size, and new size, respectively.
		/// </summary>
		public event Action<TileGrid, Vector2i, Vector2i> OnTileGridResized;


		private TileTypes[,] grid;
		
		
		public int Width { get { return grid.GetLength(0); } }
		public int Height { get { return grid.GetLength(1); } }

		public TileTypes this[Vector2i tilePos]
		{
			get
			{
				UnityEngine.Assertions.Assert.IsTrue(IsValid(tilePos), tilePos.ToString());
				return grid[tilePos.x, tilePos.y];
			}
			set
			{
				TileTypes oldVal = this[tilePos];
				this[tilePos] = value;

				if (OnTileChanged != null)
					OnTileChanged(this, tilePos, oldVal, value);
			}
		}


		public TileGrid(TileTypes[,] _grid)
		{
			//Make a copy so that modifying the parameter doesn't secretly change this grid.
			grid = new TileTypes[_grid.GetLength(0), _grid.GetLength(1)];
			_grid.CopyTo(grid, 0);
		}
		public TileGrid(int width, int height) : this(new Vector2i(width, height)) { }
		public TileGrid(Vector2i size)
		{
			grid = new TileTypes[size.x, size.y];
			for (int y = 0; y < Height; ++y)
				for (int x = 0; x < Width; ++x)
					grid[x, y] = TileTypes.Empty;
		}
		public TileGrid() { grid = null; }


		/// <summary>
		/// Resizes the grid, inserting the given value if any new spaces are created.
		/// </summary>
		public void Resize(Vector2i newSize, TileTypes defaultVal)
		{
			//Create the new grid.
			TileTypes[,] oldGrid = grid;
			grid = new TileTypes[newSize.x, newSize.y];

			//Copy the data over into the new grid.
			for (int y = 0; y < Height; ++y)
				for (int x = 0; x < Width; ++x)
					if (x < oldGrid.GetLength(0) && y < oldGrid.GetLength(1))
						grid[x, y] = oldGrid[x, y];
					else
						grid[x, y] = defaultVal;

			//Raise the corresponding event.
			if (OnTileGridResized != null)
			{
				OnTileGridResized(this,
								  new Vector2i(oldGrid.GetLength(0), oldGrid.GetLength(1)),
								  new Vector2i(Width, Height));
			}
		}

		public bool IsValid(Vector2i tilePos)
		{
			return tilePos.x >= 0 && tilePos.x < Width &&
				   tilePos.y >= 0 && tilePos.y < Height;
		}
		
		/// <summary>
		/// Gets the position of all tiles of the given type.
		/// </summary>
		public IEnumerable<Vector2i> GetTiles(TileTypes type)
		{
			for (Vector2i p = Vector2i.Zero; p.y < Height; ++p.y)
				for (p.x = 0; p.x < Width; ++p.x)
					if (grid[p.x, p.y] == type)
						yield return p;
		}
		/// <summary>
		/// Gets the position of all tiles that satisfy the given criteria.
		/// </summary>
		public IEnumerable<Vector2i> GetTiles(Func<Vector2i, TileTypes, bool> predicate)
		{
			for (Vector2i p = Vector2i.Zero; p.y < Height; ++p.y)
				for (p.x = 0; p.x < Width; ++p.x)
					if (predicate(p, grid[p.x, p.y]))
						yield return p;
		}

		public void WriteData(MyData.Writer writer)
		{
			writer.Int(Width, "width");
			writer.Int(Height, "height");

			//Write one string for each row.
			//Each string is a collection of tiles, represented by their int values and separated by '|'.
			StringBuilder data = new StringBuilder();
			for (int y = 0; y < Height; ++y)
			{
				for (int x = 0; x < Width; ++x)
				{
					if (x > 0)
						data.Append('|');
					data.Append(x);
				}

				writer.String(data.ToString(), "row" + y);
				data.Remove(0, data.Length);
			}
		}
		public void ReadData(MyData.Reader reader)
		{
			grid = new TileTypes[reader.Int("width"), reader.Int("height")];
			for (int y = 0; y < Height; ++y)
			{
				//Split the row into individual elements.
				string[] elements = reader.String("row" + y).Split('|');
				if (elements.Length != Width)
				{
					throw new MyData.Reader.ReadException("Expected " + Width +
														      ", got " + elements.Length);
				}

				//Parse each element in the row.
				for (int x = 0; x < Width; ++x)
					grid[x, y] = (TileTypes)int.Parse(elements[x]);
			}
		}
	}
}