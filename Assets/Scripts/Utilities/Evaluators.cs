using System;
using System.Collections.Generic;
using System.Linq;
using Mathf = UnityEngine.Mathf;


/// <summary>
/// Represents a value that is calculated as ((Stat^exp) * scale) + offset, where:
/// "Stat" is some stat, like "Strength", whose value strongly influences this value.
/// "exp" is an exponent that affects how strongly the stat affects the value.
/// "scale" is a scale that adjusts how the input stat maps to the output value.
/// "offset" is the "base" value that you get when the stat is 0.
/// </summary>
[Serializable]
public class ScaledValue : MyData.IReadWritable
{
	public float Exp, Scale, Offset;

	/// <param name="exp">Affects how quickly the stat grows.</param>
	/// <param name="scale">How the input stat maps to the output value.</param>
	/// <param name="offset">The "base" output that you get when the input is 0.</param>
	public ScaledValue(float exp, float scale, float offset)
	{
		Exp = exp;
		Scale = scale;
		Offset = offset;
	}

	/// <summary>
	/// Evaluates this value.
	/// </summary>
	public float Evaluate(float stat, float? min = null, float? max = null)
	{
		float result = Offset + (Scale * Mathf.Pow(stat, Exp));

		if (min.HasValue && result < min.Value)
			result = min.Value;
		if (max.HasValue && result > max.Value)
			result = max.Value;

		return result;
	}
	/// <summary>
	/// Evaluates this value with the given modified exponent, scale, or offset.
	/// </summary>
	public float EvaluateFull(float stat, float? exp = null, float? scale = null, float? offset = null,
						      float? min = null, float? max = null)
	{
		float _exp = Exp,
			  _scale = Scale,
			  _offset = Offset;
		if (exp.HasValue)
			Exp = exp.Value;
		if (scale.HasValue)
			Scale = scale.Value;
		if (offset.HasValue)
			Offset = offset.Value;

		float result = Evaluate(stat, min, max);

		Exp = _exp;
		Scale = _scale;
		Offset = _offset;

		return result;
	}

	public void WriteData(MyData.Writer writer)
	{
		writer.Float(Exp, "exp");
		writer.Float(Scale, "scale");
		writer.Float(Offset, "offset");
	}
	public void ReadData(MyData.Reader reader)
	{
		Exp = reader.Float("exp");
		Scale = reader.Float("scale");
		Offset = reader.Float("offset");
	}
}


/// <summary>
/// Represents a value that asymptotically approaches some "boundary" value
///     from above or below as a stat (e.x. "Strength") increases.
/// The actual basic function used is (1/(x + 1)) - 1.
/// </summary>
[Serializable]
public class AsymptoteValue : MyData.IReadWritable
{
	/// <summary>
	/// "Start" is the value when the stat is 0.
	/// "End" is the value when the stat is infinity
	///     (note: if the stat is actually float.PositiveInfinity, it will NOT result in "End").
	/// </summary>
	public float Start, End;
	/// <summary>
	/// The larger the value, the more extreme the initial departure from the starting value.
	/// </summary>
	public float Slope;
	
	/// <param name="start">The value when the stat is 0.</param>
	/// <param name="end">The value when the stat is extremely large.</param>
	/// <param name="slope">
	/// The larger the value, the more extreme the initial departure from the starting value.
	/// </param>
	public AsymptoteValue(float start, float end, float slope)
	{
		Start = start;
		End = end;
		Slope = slope;
	}

	/// <summary>
	/// Evaluates this value.
	/// </summary>
	public float Evaluate(float stat)
	{
		//Figured out how to do this thanks to Wolfram Alpha.

		//Asymptotic growth from 0 towards 1 based on "stat".
		float t = -((1.0f / ((stat * Slope) + 1.0f)) - 1.0f);
		//Manual lerp to skip Unity's automatic clamp in Mathf.Lerp().
		return Start + ((End - Start) * t);
	}
	/// <summary>
	/// Evaluates this value with the given modified start, end, or slope.
	/// </summary>
	public float EvaluateFull(float stat, float? start = null, float? end = null, float? slope = null)
	{
		float _start = Start,
			  _end = End,
			  _slope = Slope;
		if (start.HasValue)
			Start = start.Value;
		if (end.HasValue)
			End = end.Value;
		if (slope.HasValue)
			Slope = slope.Value;

		float result = Evaluate(stat);

		Start = _start;
		End = _end;
		Slope = _slope;

		return result;
	}

	public void WriteData(MyData.Writer writer)
	{
		writer.Float(Start, "start");
		writer.Float(End, "end");
		writer.Float(Slope, "slope");
	}
	public void ReadData(MyData.Reader reader)
	{
		Start = reader.Float("start");
		End = reader.Float("end");
		Slope = reader.Float("slope");
	}
}