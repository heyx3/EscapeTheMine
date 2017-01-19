using System;
using System.Collections.Generic;
using System.Linq;



namespace GameLogic.Units.Player_Char
{
    //TODO: A tab in the UI window for this info.

	/// <summary>
	/// A PlayerChar's personal data.
	/// </summary>
	public class Personality : MyData.IReadWritable
	{
		public enum Genders
		{
			Male,
			Female,
		}


		public static string GenerateName(Genders gender, int seed)
		{
            //Basic idea: Take a bunch of syllables and smash 'em together.
            //Every syllable has a beginning and ending letter.
            //These beginnings and endings can be split into three groups:
            //    1. Vowels -- can follow any consonant, but should usually not follow a vowel
            //    2. Ending consonant -- shouldn't follow an ending consonant, MIGHT follow a continuing consonant, can follow any vowel
            //    3. Continuing consonant -- can follow any continuing consonants or vowels, but not ending consonants.
            //These syllables are hard-coded and stored in the below "syllables" collection.

            PRNG rng = new PRNG(seed);

			const int minSyllables = 2,
					  maxSyllables = 4;
			const float chance_VowelToVowel = 0.05f,
						chance_ContinuingConsonantToEndingConsonant = 0.5f;

			int nSyllables = rng.NextInt(minSyllables, maxSyllables + 1);

			//Start with a completely random syllable.
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			Syllable firstSyllable = syllables.ElementAt(rng.NextInt(0, syllables.Count));
			sb.Append(firstSyllable.Value);

			//Add successive syllables making sure they don't conflict with what came before.
			HashSet<Syllable> acceptableSyllables = new HashSet<Syllable>();
			Syllable lastSyllable = firstSyllable;
			for (int i = 1; i < nSyllables; ++i)
			{
				//Get all acceptable syllables.
				acceptableSyllables.Clear();
				foreach (Syllable syllable in syllables)
				{
					float chanceOfAccepting;

					switch (syllable.StartType)
					{
						case Syllable.Types.Vowel:
							switch (lastSyllable.EndType)
							{
								case Syllable.Types.Vowel:
									chanceOfAccepting = chance_VowelToVowel;
									break;
								case Syllable.Types.ContinuingConsonant:
								case Syllable.Types.EndingConsonant:
									chanceOfAccepting = 1.0f;
									break;
								default: throw new NotImplementedException(lastSyllable.EndType.ToString());
							}
							break;

						case Syllable.Types.EndingConsonant:
							switch (lastSyllable.EndType)
							{
								case Syllable.Types.Vowel:
									chanceOfAccepting = 1.0f;
									break;
								case Syllable.Types.ContinuingConsonant:
									chanceOfAccepting = chance_ContinuingConsonantToEndingConsonant;
									break;
								case Syllable.Types.EndingConsonant:
									chanceOfAccepting = 0.0f;
									break;
								default: throw new NotImplementedException(lastSyllable.EndType.ToString());
							}
							break;

						case Syllable.Types.ContinuingConsonant:
							switch (lastSyllable.EndType)
							{
								case Syllable.Types.Vowel:
								case Syllable.Types.ContinuingConsonant:
									chanceOfAccepting = 1.0f;
									break;
								case Syllable.Types.EndingConsonant:
									chanceOfAccepting = 0.0f;
									break;
								default: throw new NotImplementedException(lastSyllable.EndType.ToString());
							}
							break;

						default: throw new NotImplementedException(syllable.StartType.ToString());
					}

					if (chanceOfAccepting <= 0.0f || syllable == lastSyllable)
						continue;
					if (chanceOfAccepting >= 1.0f || rng.NextFloat() < chanceOfAccepting)
						acceptableSyllables.Add(syllable);
				}

				//Pick one randomly.
				var nextSyllable = acceptableSyllables.ElementAt(rng.NextInt(0, acceptableSyllables.Count));
				lastSyllable = nextSyllable;
				sb.Append(nextSyllable.Value);
			}

            sb[0] = char.ToUpper(sb[0]);
			return sb.ToString();
		}
		#region Name generation helpers
		private struct Syllable
		{
			public enum Types
			{
				Vowel,
				EndingConsonant,
				ContinuingConsonant,
			}

			public string Value;
			public Types StartType, EndType;
			public Syllable(string value, Types startType, Types endType)
			{
				Value = value;
				StartType = startType;
				EndType = endType;
			}
			public override int GetHashCode()
			{
				return Value.GetHashCode();
			}
			public override bool Equals(object obj)
			{
				return obj is Syllable && ((Syllable)obj) == this;
			}
			public static bool operator ==(Syllable a, Syllable b)
			{
				return a.Value == b.Value && a.StartType == b.StartType && a.EndType == b.EndType;
			}
			public static bool operator !=(Syllable a, Syllable b) { return !(a == b); }
		}
		private static readonly HashSet<Syllable> syllables = new HashSet<Syllable>()
		{
			new Syllable("ab", Syllable.Types.Vowel, Syllable.Types.ContinuingConsonant),
			new Syllable("ge", Syllable.Types.EndingConsonant, Syllable.Types.Vowel),
			new Syllable("ber", Syllable.Types.ContinuingConsonant, Syllable.Types.EndingConsonant),
			new Syllable("du", Syllable.Types.ContinuingConsonant, Syllable.Types.Vowel),
			new Syllable("y", Syllable.Types.Vowel, Syllable.Types.ContinuingConsonant),
			new Syllable("ar", Syllable.Types.Vowel, Syllable.Types.EndingConsonant),
			new Syllable("in", Syllable.Types.Vowel, Syllable.Types.ContinuingConsonant),
			new Syllable("tr", Syllable.Types.EndingConsonant, Syllable.Types.EndingConsonant),
			new Syllable("yo", Syllable.Types.Vowel, Syllable.Types.Vowel),
			new Syllable("vil", Syllable.Types.EndingConsonant, Syllable.Types.ContinuingConsonant),
		};
		#endregion


		public PlayerChar Owner { get; private set; }

		public Stat<string, Personality> Name { get; private set; }
		public Stat<Genders, Personality> Gender { get; private set; }

		/// <summary>
		/// Used to decide how to render this personality.
		/// </summary>
		public Stat<int, Personality> AppearanceIndex { get; private set; }


		public Personality(PlayerChar owner, string name, Genders gender, int appearanceIndex)
		{
			Owner = owner;
			Name = new Stat<string, Personality>(this, name);
			Gender = new Stat<Genders, Personality>(this, gender);
			AppearanceIndex = new Stat<int, Personality>(this, appearanceIndex);
		}


		public void WriteData(MyData.Writer writer)
		{
			writer.String(Name.Value, "name");
			writer.Int((int)Gender.Value, "gender");
			writer.Int(AppearanceIndex.Value, "appearanceIndex");
		}
		public void ReadData(MyData.Reader reader)
		{
			Name.Value = reader.String("name");
			Gender.Value = (Genders)reader.Int("gender");
			AppearanceIndex.Value = reader.Int("appearanceIndex");
		}
	}
}
