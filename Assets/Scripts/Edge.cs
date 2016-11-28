﻿using UnityEngine;
using System.Collections;

public class Edge : MonoBehaviour {

	public BezierSpline curve;
	private EdgeDecorator decorator;

	private Node[] _edgeVertices;
	public int Strength = 1; 

	void Awake () {
		curve = GetComponent<BezierSpline> ();
		decorator = GetComponent<EdgeDecorator> ();
	}

	public Node[] EndingVertices(){
		return _edgeVertices;
	}

	public bool GetEdgeDirection(Node n){
		return (n == _edgeVertices [0]);
	}

	public Node GetVert1(){
		return _edgeVertices[0];
	}
	public Node GetVert2(){
		return _edgeVertices[1];
	}

	public bool ConnectedTo(Node n){
		if (n == _edgeVertices [0] || n == _edgeVertices [1]) return true;

		return false;
	}

	public Vector3 GetInitVelocity(Node n){

		if (n == _edgeVertices[0]) {
			return curve.GetVelocity (0);
		} else {
			return -curve.GetVelocity(1);
		}
	}

	public void Reinforce(){
		
		decorator.Decorate ();
	}

	public Vector3 GetReversedInitVelocity(Node n){

		if (n == _edgeVertices[0]) {
			return -curve.GetVelocity(0);
		} else {
			return curve.GetVelocity(1);
		}
	}

	public float GetAngleAtNode(Vector3 direction, Node n, bool reversed = false){

		if (reversed) {
			return Vector3.Angle (direction, GetReversedInitVelocity (n));
		} else {
			return Vector3.Angle (direction, GetInitVelocity (n));
		}
	}

	public void SetVerts(Node from, Node to){

		_edgeVertices = new Node[]{from, to};
		Edge e;

		Vector3 v1 = Vector3.zero;
		Vector3 v2 = Vector3.zero;
		Vector3 cursorPos = GameObject.Find ("Player").GetComponent<PlayerTraversal> ().cursor.transform.position;

		float distance = Vector3.Distance (to.transform.position, from.transform.position);

		if(from.HasEdges()){
			e = from.GetClosestEdgeDirection((cursorPos - from.transform.position).normalized, true);
			v1 = e.GetReversedInitVelocity (from).normalized * distance/2f; //could times by distance
			v1 = Vector3.Lerp (v1, (cursorPos - from.transform.position).normalized, 0.05f);
		}
		if(to.HasEdges()){
			e = to.GetClosestEdgeDirection((cursorPos - to.transform.position).normalized, true);
			v2 = e.GetReversedInitVelocity (to).normalized * distance/2f;
		}

		Debug.Log (distance);
		curve.CreateCurve (_edgeVertices[0].transform, _edgeVertices[1].transform, distance, v1, v2);

		decorator.Decorate ();
	}

	public BezierSpline Getcurve(){
		return curve;
	}

}
