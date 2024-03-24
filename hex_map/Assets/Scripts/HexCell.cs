using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;

    public HexGridChunk chunk;

    private int elevation = int.MinValue;
    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value) return;

            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2 - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            // river handling
            if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation) RemoveOutgoingRiver();
            if (hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation) RemoveIncomingRiver();

            Refresh();
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    [SerializeField]
    public HexCell[] neighbors;

    private Color color;
    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value) return;

            color = value;
            Refresh();
        }
    }

    public RectTransform uiRect;

    // river
    private bool hasIncomingRiver = false;
    private bool hasOutgoingRiver = false;
    private HexDirection incomingRiver;
    private HexDirection outgoingRiver;

    public float StreamBedY
    {
        get => (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
    }

    public float RiverSurfaceY
    {
        get => (elevation + HexMetrics.riverSurfaceElevationOffset) * HexMetrics.elevationStep;
    }

    public bool HasIncomingRiver
    {
        get => hasIncomingRiver;
    }
    public bool HasOutgoingRiver
    {
        get => hasOutgoingRiver;
    }
    public HexDirection IncomingRiver
    {
        get => incomingRiver;
    }
    public HexDirection OutgoingRiver
    {
        get => outgoingRiver;
    }
    public bool HasRiver => hasIncomingRiver || hasOutgoingRiver;
    public bool HasRiverBeginOrEnd => hasIncomingRiver != hasOutgoingRiver;

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return hasIncomingRiver && IncomingRiver == direction
            || hasOutgoingRiver && OutgoingRiver == direction;
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction) return;

        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation) return;

        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction) RemoveIncomingRiver();

        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }

    public void RemoveOutgoingRiver()
    {
        if (!HasOutgoingRiver) return;
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(OutgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveIncomingRiver()
    {
        if (!HasIncomingRiver) return;
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    public void RefreshSelfOnly()
    {
        chunk.Refresh();
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }
}
