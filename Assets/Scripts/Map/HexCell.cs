using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum HexEdgeType
{
    Flat, Slope, Cliff
}

public class HexCell : MonoBehaviour {

    public Society.Mapping.HexCell Data = new Society.Mapping.HexCell();

    public HexCoordinates coordinates;

	public RectTransform uiRect;

	public HexGridChunk chunk;

	public int Index { get { return Data.Index; } set { Data.Index = value; } }

	public int ColumnIndex { get { return Data.ColumnIndex; } set { Data.ColumnIndex = value; } }

    public int Elevation {
		get {
			return Data.Elevation;
		}
		set {
			if (Data.Elevation == value) {
				return;
			}
			int originalViewElevation = ViewElevation;
            Data.Elevation = value;
			if (ViewElevation != originalViewElevation) {
				ShaderData.ViewElevationChanged();
			}
			RefreshPosition();
			ValidateRivers();

			for (int i = 0; i < Data.Roads.Length; i++) {
				if (Data.Roads[i] && GetElevationDifference((HexDirection)i) > 1) {
					SetRoad(i, false);
				}
			}

			Refresh();
		}
	}

	public int WaterLevel {
		get {
			return Data.WaterLevel;
		}
		set {
			if (Data.WaterLevel == value) {
				return;
			}
			int originalViewElevation = ViewElevation;
            Data.WaterLevel = value;
			if (ViewElevation != originalViewElevation) {
				ShaderData.ViewElevationChanged();
			}
			ValidateRivers();
			Refresh();
		}
	}

	public int ViewElevation {
		get {
			return Data.ViewElevation;
		}
	}

	public bool IsUnderwater {
		get {
			return Data.IsUnderwater;
		}
	}

	public bool HasIncomingRiver {
		get {
			return Data.HasIncomingRiver;
		}
	}

	public bool HasOutgoingRiver {
		get {
			return Data.HasOutgoingRiver;
		}
	}

	public bool HasRiver {
		get {
			return Data.HasRiver;
		}
	}

	public bool HasRiverBeginOrEnd {
		get {
			return Data.HasRiverBeginOrEnd;
		}
	}

	public HexDirection RiverBeginOrEndDirection {
		get {
			return Data.RiverBeginOrEndDirection;
		}
	}

	public bool HasRoads {
		get {
			return Data.HasRoads;
		}
	}

	public HexDirection IncomingRiver {
		get {
			return Data.IncomingRiver;
		}
	}

	public HexDirection OutgoingRiver {
		get {
			return Data.OutgoingRiver;
		}
	}

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}


	public float StreamBedY {
		get {
			return
				(Data.Elevation + HexMetrics.streamBedElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float RiverSurfaceY {
		get {
			return
				(Data.Elevation + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float WaterSurfaceY {
		get {
			return
				(Data.WaterLevel + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public int UrbanLevel {
		get {
			return Data.UrbanLevel;
		}
		set {
			if (Data.UrbanLevel != value) {
                Data.UrbanLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int FarmLevel {
		get {
			return Data.FarmLevel;
		}
		set {
			if (Data.FarmLevel != value) {
                Data.FarmLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int PlantLevel {
		get {
			return Data.PlantLevel;
		}
		set {
			if (Data.PlantLevel != value) {
                Data.PlantLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int SpecialIndex {
		get {
			return Data.SpecialIndex;
		}
		set {
			if (Data.SpecialIndex != value && !HasRiver) {
                Data.SpecialIndex = value;
				RemoveRoads();
				RefreshSelfOnly();
			}
		}
	}

	public bool IsSpecial {
		get {
			return Data.SpecialIndex > 0;
		}
	}

	public bool Walled {
		get {
			return Data.Walled;
		}
		set {
			if (Data.Walled != value) {
                Data.Walled = value;
				Refresh();
			}
		}
	}

	public int TerrainTypeIndex {
		get {
			return Data.TerrainTypeIndex;
		}
		set {
			if (Data.TerrainTypeIndex != value) {
                Data.TerrainTypeIndex = value;
				ShaderData.RefreshTerrain(this);
			}
		}
	}

	public bool IsVisible {
		get {
			return visibility > 0 && Explorable;
		}
	}

	public bool IsExplored {
		get {
			return explored && Explorable;
		}
		private set {
			explored = value;
		}
	}

	public bool Explorable { get; set; }

	public int Distance {
		get {
			return Data.Distance;
		}
		set {
            Data.Distance = value;
		}
	}

	public HexUnit Unit { get; set; }

	public HexCell PathFrom { get; set; }

	public int SearchHeuristic { get; set; }

	public int SearchPriority {
		get {
			return Data.Distance + SearchHeuristic;
		}
	}

	public int SearchPhase { get; set; }

	public HexCell NextWithSamePriority { get; set; }

	public HexCellShaderData ShaderData { get; set; }

	int visibility;

	bool explored;

	[SerializeField]
	HexCell[] neighbors;

    public void IncreaseVisibility () {
		visibility += 1;
		if (visibility == 1) {
			IsExplored = true;
			ShaderData.RefreshVisibility(this);
		}
	}

	public void DecreaseVisibility () {
		visibility -= 1;
		if (visibility == 0) {
			ShaderData.RefreshVisibility(this);
		}
	}

	public void ResetVisibility () {
		if (visibility > 0) {
			visibility = 0;
			ShaderData.RefreshVisibility(this);
		}
	}

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			Elevation, neighbors[(int)direction].Elevation
        );
	}

	public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
            Elevation, otherCell.Elevation
		);
	}

	public bool HasRiverThroughEdge (HexDirection direction) {
        return Data.HasRiverThroughEdge(direction);
	}

	public void RemoveIncomingRiver () {
		if (!Data.HasIncomingRiver) {
			return;
		}
        Data.HasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(Data.IncomingRiver);
		neighbor.Data.HasOutgoingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveOutgoingRiver () {
		if (!HasOutgoingRiver) {
			return;
		}
        Data.HasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(Data.OutgoingRiver);
		neighbor.Data.HasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveRiver () {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}

	public void SetOutgoingRiver (HexDirection direction) {
		if (HasOutgoingRiver && OutgoingRiver == direction) {
			return;
		}

		HexCell neighbor = GetNeighbor(direction);
		if (!IsValidRiverDestination(neighbor)) {
			return;
		}

		RemoveOutgoingRiver();
		if (HasIncomingRiver && IncomingRiver == direction) {
			RemoveIncomingRiver();
		}
        Data.HasOutgoingRiver = true;
		Data.OutgoingRiver = direction;
        Data.SpecialIndex = 0;

		neighbor.RemoveIncomingRiver();
		neighbor.Data.HasIncomingRiver = true;
		neighbor.Data.IncomingRiver = direction.Opposite();
		neighbor.Data.SpecialIndex = 0;

		SetRoad((int)direction, false);
	}

	public bool HasRoadThroughEdge (HexDirection direction) {
		return Data.HasRoadThroughEdge(direction);
	}

	public void AddRoad (HexDirection direction) {
		if (
			!Data.Roads[(int)direction] && !HasRiverThroughEdge(direction) &&
			!IsSpecial && !GetNeighbor(direction).IsSpecial &&
			GetElevationDifference(direction) <= 1
		) {
			SetRoad((int)direction, true);
		}
	}

	public void RemoveRoads () {
		for (int i = 0; i < neighbors.Length; i++) {
			if (Data.Roads[i]) {
				SetRoad(i, false);
			}
		}
	}

	public int GetElevationDifference (HexDirection direction) {
		return Data.GetElevationDifference(direction);
	}

	bool IsValidRiverDestination (HexCell neighbor) {
		return neighbor && (
			Data.Elevation >= neighbor.Data.Elevation || Data.WaterLevel == neighbor.Data.Elevation
        );
	}

	void ValidateRivers () {
		if (
			Data.HasOutgoingRiver &&
			!IsValidRiverDestination(GetNeighbor(Data.OutgoingRiver))
		) {
			RemoveOutgoingRiver();
		}
		if (
            Data.HasIncomingRiver &&
			!GetNeighbor(Data.IncomingRiver).IsValidRiverDestination(this)
		) {
			RemoveIncomingRiver();
		}
	}

	void SetRoad (int index, bool state) {
		Data.Roads[index] = state;
		neighbors[index].Data.Roads[(int)((HexDirection)index).Opposite()] = state;
		neighbors[index].RefreshSelfOnly();
		RefreshSelfOnly();
	}

	void RefreshPosition () {
		Vector3 position = transform.localPosition;
		position.y = Data.Elevation * HexMetrics.elevationStep;
		position.y +=
			(HexMetrics.SampleNoise(position).y * 2f - 1f) *
			HexMetrics.elevationPerturbStrength;
		transform.localPosition = position;

		Vector3 uiPosition = uiRect.localPosition;
		uiPosition.z = -position.y;
		uiRect.localPosition = uiPosition;
	}

	void Refresh () {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) {
					neighbor.chunk.Refresh();
				}
			}
			if (Unit) {
				Unit.ValidateLocation();
			}
		}
	}

	void RefreshSelfOnly () {
		chunk.Refresh();
		if (Unit) {
			Unit.ValidateLocation();
		}
	}

	public void Save (BinaryWriter writer) {
        Data.Save(writer);
		//writer.Write(IsExplored);
	}

	public void Load (BinaryReader reader, int header) {
        Data.Load(reader, header);
		ShaderData.RefreshTerrain(this);
		RefreshPosition();

		//IsExplored = header >= 3 ? reader.ReadBoolean() : false;
		ShaderData.RefreshVisibility(this);
	}

	public void SetLabel (string text) {
		UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
		label.text = text;
	}

	public void DisableHighlight () {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.enabled = false;
	}

	public void EnableHighlight (Color color) {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.color = color;
		highlight.enabled = true;
	}

	public void SetMapData (float data) {
		ShaderData.SetMapData(this, data);
	}
}