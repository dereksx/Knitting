﻿using UnityEngine;
using System;
using System.Collections;

public class BezierSpline : MonoBehaviour {

	[SerializeField]
	private Vector3[] points;

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

	[SerializeField]


	public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
			if (value == true) {
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount {
		get {
			return points.Length;
		}
	}

	public Vector3 GetControlPoint (int index) {
		return points[index];
	}

	public void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points[index];
			if (loop) {
				if (index == 0) {
					points[1] += delta;
					points[points.Length - 2] += delta;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1) {
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else {
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
			}
			else {
				if (index > 0) {
					points[index - 1] += delta;
				}
				if (index + 1 < points.Length) {
					points[index + 1] += delta;
				}
			}
		}
		points[index] = point;
		EnforceMode(index);
	}

	public BezierControlPointMode GetControlPointMode (int index) {
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop) {
			if (modeIndex == 0) {
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1) {
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) {
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = points.Length - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}

	public int CurveCount {
		get {
			return (points.Length - 1) / 3;
		}
	}

	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}
	
	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	public void AddCurve () {
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 1f;
		points[points.Length - 3] = point;
		point.x += 1f;
		points[points.Length - 2] = point;
		point.x += 1f;
		points[points.Length - 1] = point;

		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);

		if (loop) {
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}


	public void CreateCurve (Transform p1, Transform p2, float distance, Vector3 v1 = default(Vector3), Vector3 v2 = default(Vector3)) {
		Vector3 target = p2.position - p1.position;


		points = new Vector3[4];
		points [0] = Vector3.zero;

		if (v1 == Vector3.zero) {
			points [1] = Vector3.Lerp (Vector3.zero, target, 0.33f);
		} else {
			points [1] = v1 ; //multiplicant should range between 0 and 1. Should also scale with line length
		}

		if (v2 != Vector3.zero) {
			points [2] = target + v2;
	
		} else if (v1 != Vector3.zero) {
//			v2 = Quaternion.Euler(0, 0, 0) * v1; //rotate v1 180 degrees
			points [2] = target + ((v1 - target));
//			points[2] = Vector3.Reflect(v1, (target-v1));

		} else {
			points [2] = Vector3.Lerp (points[1], target, 0.5f);
		}

		points[3] = target;

		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Aligned,
			BezierControlPointMode.Aligned
		};
	}

//	public void CreateCurve (Transform p1, Transform p2) {
//		Vector3 target = p2.position - p1.position;
//		points = new Vector3[] {
//			Vector3.zero,
//			new Vector3 (Vector3.Lerp(Vector3.zero, target, 0.33f).x, 0, 0),
//			new Vector3 (Vector3.Lerp(Vector3.zero, target, 0.66f).x, target.y, 0),
//			Vector3.Lerp(Vector3.zero, target, 0.33f),
//			Vector3.Lerp(Vector3.zero, target, 0.66f),
//			target
//		};
//		modes = new BezierControlPointMode[] {
//			BezierControlPointMode.Free,
//			BezierControlPointMode.Free
//		};
//	}

//	public void Reset () {
//		points = new Vector3[] {
//			new Vector3(1f, 0f, 0f),
//			new Vector3(2f, 0f, 0f),
//			new Vector3(3f, 0f, 0f),
//			new Vector3(4f, 0f, 0f)
//		};
//		modes = new BezierControlPointMode[] {
//			BezierControlPointMode.Free,
//			BezierControlPointMode.Free
//		};
//	}

	public void SetSpline(Vector3[] newPoints, BezierControlPointMode[] newModes){
		points = newPoints;
		modes = newModes;
	}

}