﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;


public class NodeUi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler {

	public delegate void OnClickHandler();
	public event OnClickHandler OnClick;

	private Node node;
	private bool drag = false;


	public void Init(GameObject container, Node node) {
		this.node = node;

		name = node.name;
		transform.SetParent(container.transform);
		transform.localPosition = new Vector3(node.X, node.Y, 0);

		Transform t = transform.Find("Text");
		if (t != null) {
			t.GetComponent<MeshRenderer>().sortingOrder = 10;
			TextMesh text = t.GetComponent<TextMesh>();
			//text.color = node.color;
			text.text = node.name;
		}

		GenerateConnections();
	}


	public void OnDestroy () {
		// clear events
		OnClick = null;
	}
	

	// Drag Node

	public void OnBeginDrag (PointerEventData eventData) {
		MapCamera.enabled = false;
	}


	public void OnDrag (PointerEventData eventData) {
		Vector3 pos = MapCamera.currentCamera.ScreenToWorldPoint(Input.mousePosition);

		node.X = Mathf.RoundToInt(pos.x);
		node.Y =  Mathf.RoundToInt(pos.y);
		transform.localPosition = new Vector3(node.X, node.Y, 0); 

		NodeUi[] arr = transform.root.GetComponentsInChildren<NodeUi>();
		foreach (NodeUi ui in arr) {
			ui.GenerateConnections();
		}
	}


	public void OnEndDrag (PointerEventData eventData) {
		MapCamera.enabled = true;
	}


	// Click on node

	public void OnPointerClick (PointerEventData eventData) {
		// if camera is disabled is because we were dragging, so escape
		if (!MapCamera.enabled) { return; }

		// emit click event
		if (OnClick != null) {
			OnClick.Invoke();
		}
	}


	// Update Connections

	public void GenerateConnections () {
		// draw lines towards friends
		Transform tr = transform.Find("Paths");
		if (tr != null) { 
			GameObject.Destroy(tr.gameObject); 
		}

		GameObject container = new GameObject();
		container.transform.SetParent(transform, false);
		container.name = "Paths";

		for (int i = 0; i < node.Links.Count; i++) {
			Node link = node.Links[i];
			GenerateConnection(container, link);
		}
	}


	private void GenerateConnection (GameObject rootContainer, Node link) {
		// create path container inside the node
		GameObject container = new GameObject();
		container.transform.SetParent(rootContainer.transform, false);
		container.name = "Path";

		// set dot line parameters
		int dotRadius = 25;
		int distanceBetweenDots = 25;

		// get line vector
		Vector2 p1 = new Vector3(node.X, node.Y, 0); 
		Vector2 p2 = new Vector3(link.X, link.Y, 0);
		Vector2 vec = (p2 - p1);
		vec -= (vec.normalized * (dotRadius * 2));

		// get number of points to render, and at which step
		float length = vec.magnitude;
		int maxPoints = (int)Mathf.Round(length / distanceBetweenDots);
		float step = (length) / (maxPoints + 1);

		// create and locate line points
		for (int i = 1; i <= maxPoints; i++) {
			GameObject point = (GameObject)GameObject.Instantiate<GameObject>(Scene.instance.pathPrefab);
			point.name = "Point" + i;
			point.transform.SetParent(container.transform);
			point.transform.localPosition = Vector3.zero;
			point.transform.Translate(vec.normalized * step * i);
			point.transform.Translate(vec.normalized * dotRadius);
			point.gameObject.SetActive(true);
		}
	}

}
