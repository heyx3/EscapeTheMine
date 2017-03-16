using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GameLogic
{
	public enum TileTypes
	{
		Empty = 0,
		Wall, Deposit, Bedrock,
	}

	public static class TileTypesExtensions
	{
		public static bool BlocksMovement(this TileTypes t) { return blocksMovement[t]; }
        public static bool IsMinable(this TileTypes t) { return isMinable[t]; }
		public static bool IsBuildableOn(this TileTypes t) { return canBuildStructureOn[t]; }
		

		#region Lookup dictionaries
		private static readonly Dictionary<TileTypes, bool> blocksMovement = new Dictionary<TileTypes, bool>()
		{
			{ TileTypes.Empty, false },
			{ TileTypes.Wall, true },
			{ TileTypes.Deposit, true },
			{ TileTypes.Bedrock, true },
		};
        private static readonly Dictionary<TileTypes, bool> isMinable = new Dictionary<TileTypes, bool>()
        {
            { TileTypes.Empty, false },
            { TileTypes.Wall, true },
            { TileTypes.Deposit, true },
            { TileTypes.Bedrock, false },
        };
		private static readonly Dictionary<TileTypes, bool> canBuildStructureOn = new Dictionary<TileTypes, bool>()
		{
			{ TileTypes.Empty, true },
			{ TileTypes.Wall, false },
			{ TileTypes.Deposit, false },
			{ TileTypes.Bedrock, false },
		};
		#endregion
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
		/// Raised when an instance gets its grid reset to a completely new set of values.
		/// The parameters are the grid, size of the old grid, and size of the new grid, respectively.
		/// </summary>
		public event Action<TileGrid, Vector2i, Vector2i> OnTileGridReset;


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
				grid[tilePos.x, tilePos.y] = value;

				if (OnTileChanged != null)
					OnTileChanged(this, tilePos, oldVal, value);
			}
		}
		public TileTypes this[int x, int y] { get { return this[new Vector2i(x, y)]; } set { this[new Vector2i(x, y)] = value; } }


		public TileGrid(TileTypes[,] _grid)
		{
			//Make a copy so that modifying the parameter doesn't secretly change this grid.
			grid = new TileTypes[_grid.GetLength(0), _grid.GetLength(1)];
            for (int y = 0; y < grid.GetLength(1); ++y)
                for (int x = 0; x < grid.GetLength(0); ++x)
                    grid[x, y] = _grid[x, y];
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
		public void Reset(TileTypes[,] newGrid)
		{
			//Resize the array to match.
			Vector2i oldSize = new Vector2i(grid.GetLength(0), grid.GetLength(1)),
					 newSize = new Vector2i(newGrid.GetLength(0), newGrid.GetLength(1));
			if (oldSize != newSize)
				grid = new TileTypes[newSize.x, newSize.y];

			//Copy the data over.
			for (int y = 0; y < newSize.y; ++y)
				for (int x = 0; x < newSize.x; ++x)
					grid[x, y] = newGrid[x, y];

			//Raise the event.
			if (OnTileGridReset != null)
				OnTileGridReset(this, oldSize, newSize);
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
					data.Append((int)this[new Vector2i(x, y)]);
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