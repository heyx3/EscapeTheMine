using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
    /// <summary>
    /// Singleton that allows anyone to create and destroy graphical objects
    ///     that highlight a specific tile.
    /// </summary>
    public class TileHighlighter : Singleton<TileHighlighter>
    {
        private class Tile
        {
            public Vector2i Pos
            {
                get
                {
                    Vector3 p = tr.position;
                    return new Vector2i((int)p.x, (int)p.y);
                }
                set
                {
                    tr.position = new Vector3(value.x + 0.5f, value.y + 0.5f, tr.position.z);
                }
            }
            public Color Col
            {
                get
                {
                    if (spr != null)
                        return spr.color;
                    else
                        throw new NotImplementedException();
                }
                set
                {
                    if (spr != null)
                        spr.color = value;
                    else
                        throw new NotImplementedException();
                }
            }
			public Sprite Spr
			{
				get
				{
					if (spr != null)
						return spr.sprite;
					else
						throw new NotImplementedException();
				}
				set
				{
					if (spr != null)
						spr.sprite = value;
					else
						throw new NotImplementedException();
				}
			}

            public GameObject Obj { get { return tr.gameObject; } }
            
            private Transform tr;
            private SpriteRenderer spr;


            public Tile(GameObject go)
            {
				tr = go.transform;
				UpdateGameObject();
            }

			public void UpdateGameObject()
			{
				spr = Obj.GetComponent<SpriteRenderer>();
			}
            public override int GetHashCode()
            {
                return tr.GetInstanceID();
            }
        }


        public static readonly ulong INVALID_ID = ulong.MaxValue;

        
        public int MaxUnusedHighlights = 50;
        public Sprite HighlightSprite;
        public int SpriteLayer = 50;

        private Dictionary<ulong, Tile> highlightsByID = new Dictionary<ulong, Tile>();
        private List<Tile> unusedHighlights = new List<Tile>();
        private ulong nextID = 0;

        
        public ulong CreateHighlight(Vector2i tilePos, Color highlightColor)
        {
            ulong id = nextID;
            nextID += 1;

            if (unusedHighlights.Count > 0)
            {
                Tile t = unusedHighlights[unusedHighlights.Count - 1];
                unusedHighlights.RemoveAt(unusedHighlights.Count - 1);
                highlightsByID.Add(id, t);
                t.Obj.SetActive(true);

                t.Pos = tilePos;
                t.Col = highlightColor;
                t.Spr = HighlightSprite;
            }
            else
            {
                Tile t = MakeTile();
                highlightsByID.Add(id, t);

                t.Pos = tilePos;
                t.Col = highlightColor;
            }

            return id;
        }
        public void DestroyHighlight(ulong id)
        {
            if (unusedHighlights.Count >= MaxUnusedHighlights)
                Destroy(highlightsByID[id].Obj);
            else
            {
                highlightsByID[id].Obj.SetActive(false);
                unusedHighlights.Add(highlightsByID[id]);
                highlightsByID.Remove(id);
            }
        }

        public Vector2i GetPos(ulong highlightID) { return highlightsByID[highlightID].Pos; }
        public Color GetColor(ulong highlightID) { return highlightsByID[highlightID].Col; }
		public Sprite GetSprite(ulong highlightID) { return highlightsByID[highlightID].Spr; }

        public void SetPos(ulong highlightID, Vector2i pos)
        {
            Tile t = highlightsByID[highlightID];
            t.Pos = pos;
        }
        public void SetColor(ulong highlightID, Color col)
        {
            Tile t = highlightsByID[highlightID];
            t.Col = col;
        }
		public void SetSprite(ulong highlightID, Sprite spr)
		{
			Tile t = highlightsByID[highlightID];
			t.Spr = spr;
		}

        private void Start()
        {
            UnityLogic.Options.OnChanged_ViewMode += Callback_ViewModeChanged;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnityLogic.Options.OnChanged_ViewMode -= Callback_ViewModeChanged;
        }
        private void Callback_ViewModeChanged(UnityLogic.ViewModes oldMode, UnityLogic.ViewModes newMode)
        {
			var tiles = unusedHighlights.Concat(highlightsByID.Values).ToList();

			//Remove old resources.
            switch (oldMode)
            {
                case UnityLogic.ViewModes.TwoD:
					foreach (Tile t in tiles)
						Destroy(t.Obj.GetComponent<SpriteRenderer>());
                    break;

                case UnityLogic.ViewModes.ThreeD:
                    throw new NotImplementedException();
                    //break;

                default: throw new NotImplementedException(oldMode.ToString());
            }

			//Set up new resources.
            switch (newMode)
            {
                case UnityLogic.ViewModes.TwoD:
					foreach (Tile t in tiles)
					{
						var spr = t.Obj.AddComponent<SpriteRenderer>();
						spr.sortingOrder = SpriteLayer;
						spr.sprite = HighlightSprite;
					}
                    break;

                case UnityLogic.ViewModes.ThreeD:
                    throw new NotImplementedException();
                    //break;

                default: throw new NotImplementedException(newMode.ToString());
            }
			
			//Update Tile data structures.
			foreach (Tile t in tiles)
				t.UpdateGameObject();
        }

        private Tile MakeTile()
        {
            GameObject go = new GameObject("Tile");
            Transform tr = go.transform;
            SpriteRenderer spr = null;

            switch (UnityLogic.Options.ViewMode)
            {
                case UnityLogic.ViewModes.TwoD:
                    spr = go.AddComponent<SpriteRenderer>();
                    spr.sprite = HighlightSprite;
                    spr.sortingOrder = SpriteLayer;
                    break;

                case UnityLogic.ViewModes.ThreeD:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException(UnityLogic.Options.ViewMode.ToString());
            }

            return new Tile(go);
        }
    }
}