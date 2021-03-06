﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using MyData;
using System.Collections;

namespace GameLogic.Units.Player_Char
{
	public class Job_Mine : Job
	{
		public IEnumerable<Vector2i> TilesToMine {  get { return tilesToMine; } }

		private HashSet<Vector2i> tilesToMine;

		private int turnsLeft;
		private Vector2i miningCenter;

		private bool isMiningYet = false;


		public Job_Mine(HashSet<Vector2i> _tilesToMine, bool isEmergency, Map theMap)
			: base(isEmergency, theMap)
		{
			tilesToMine = new HashSet<Vector2i>(_tilesToMine);

			CalculateMiningCenter();

			//When somebody takes or gives up this job, reset it.
			Owner.OnChanged += (thisJob, oldVal, newVal) =>
			{
				isMiningYet = false;
			};
		}


		public override IEnumerable TakeTurn()
		{
			//TODO: Sometimes PlayerChars get stuck and never finish mining.

			//Remove any tiles that are no longer minable.
			tilesToMine.RemoveWhere(tile => !TheMap.Value.Tiles[tile].IsMinable());

			//If nothing is left to mine, stop the job.
			if (tilesToMine.Count == 0)
			{
				EndJob(true);
				yield break;
			}

			//If we're not currently mining a tile, look around for a nearby one.
			if (!isMiningYet)
			{
				//Move towards a spot to mine from.
				TryMoveToPos_Status moveStatus = new TryMoveToPos_Status();
				foreach (object o in TryMoveToPos(new Pathfinding.Goal<Vector2i>(miningCenter,
																				 IsAMiningSpot),
												  moveStatus))
				{
					yield return o;
				}

				switch (moveStatus.CurrentState)
				{
					case TryMoveToPos_States.Finished:
						//Start mining.
						isMiningYet = true;
						turnsLeft = Consts.TurnsToMine(Owner.Value.Strength, Owner.Value.AdultMultiplier,
                                                       tilesToMine.Count);
						break;

					case TryMoveToPos_States.EnRoute:
						//End the turn.
						yield break;

					case TryMoveToPos_States.NoPath:
						//Cancel the job.
						EndJob(false, Localization.Get("NO_PATH_JOB"));
						yield break;

					default: throw new NotImplementedException(moveStatus.CurrentState.ToString());
				}
			}
			else
			{
				UnityEngine.Assertions.Assert.IsTrue(IsAMiningSpot(Owner.Value.Pos),
													 "Not adjacent to a mining tile");
			}

			//Mine.
			if (turnsLeft > 0)
			{
				turnsLeft -= 1;
				yield break;
			}

			//Finish up.
			//Clear out the tiles.
			foreach (Vector2i pos in tilesToMine)
				TheMap.Value.Tiles[pos] = TileTypes.Empty;
            Owner.Value.Strength.Value += Consts.StrengthIncreaseFromMining(tilesToMine.Count);

			//TODO: Possibly spawn monsters.

			EndJob(true);
		}

		private bool IsAMiningSpot(Vector2i pos)
		{
			return tilesToMine.Contains(pos.LessX) ||
				   tilesToMine.Contains(pos.LessY) ||
				   tilesToMine.Contains(pos.MoreX) ||
				   tilesToMine.Contains(pos.MoreY);
		}
		private void CalculateMiningCenter()
		{
			//Calculate the new "center" of the tiles to mine by finding the average of all positions.

			if (tilesToMine.Count == 0)
				return;


			Vector2 newCenter = Vector2.zero;
			foreach (Vector2i pos in tilesToMine)
				newCenter += new Vector2(pos.x, pos.y);

			float invCount = 1.0f / tilesToMine.Count;
			newCenter.x *= invCount;
			newCenter.y *= invCount;

			miningCenter = new Vector2i((int)newCenter.x, (int)newCenter.y);

			//If it turns out that the center isn't actually in the set of tiles to be mined,
			//    search around nearby for a tile to be mined and use that as the center.
			if (!tilesToMine.Contains(miningCenter))
			{
				//Get the closest open spot adjacent to a minable tile.
				int closestDist = int.MaxValue;
				Vector2i closestTile = Vector2i.Zero;
				foreach (Vector2i tilePos in tilesToMine)
				{
					for (int i = 0; i < 4; ++i)
					{
						Vector2i pos;
						switch (i)
						{
							case 0: pos = tilePos.LessX; break;
							case 1: pos = tilePos.MoreX; break;
							case 2: pos = tilePos.LessY; break;
							case 3: pos = tilePos.MoreY; break;
							default: throw new NotImplementedException(i.ToString());
						}

						//If the position is on the map and open, it's a candidate.
						if (TheMap.Value.Tiles.IsValid(pos))
						{
							var tile = TheMap.Value.Tiles[pos];
							if (!tile.BlocksMovement())
							{
								int tempDist = pos.ManhattanDistance(miningCenter);
								if (tempDist < closestDist)
								{
									closestDist = tempDist;
									closestTile = pos;

									//If we found an adjacent tile, stop early.
									if (closestDist == 1)
										break;
								}
							}
						}
					}
				}

				miningCenter = closestTile;
			}
		}


		//Serialization:
		public override Types ThisType { get { return Types.Mine; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);

			writer.Collection<Vector2i, HashSet<Vector2i>>(
				tilesToMine, "tilesToMine",
				(w, outVal, name) => w.Vec2i(outVal, name));

			writer.Vec2i(miningCenter, "miningCenter");
			writer.Int(turnsLeft, "turnsLeft");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);

			tilesToMine.Clear();
			reader.Collection(
				"tilesToMine",
				(Reader r, ref Vector2i outVal, string name) =>
					{ outVal = r.Vec2i(name); },
				i => tilesToMine);

			miningCenter = reader.Vec2i("miningCenter");
			turnsLeft= reader.Int("turnsLeft");

			isMiningYet = false;
		}

		//Give each type of job a unique hash code.
		public override int GetHashCode()
		{
			return 42344232;
		}
	}
}
