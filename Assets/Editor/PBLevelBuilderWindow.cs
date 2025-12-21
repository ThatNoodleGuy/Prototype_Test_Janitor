#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

public class PBLevelBuilderWindow : EditorWindow
{
    [Header("Build Settings")]
    private float wallThickness = 0.35f;

    [Header("Doors")]
    private float doorWidth = 1.4f;
    private float doorHeight = 2.2f;

    [Header("Windows")]
    private bool buildWindows = true;
    private float windowWidth = 1.8f;
    private float windowHeight = 1.0f;
    private float windowBottom = 1.1f; // window starts at this height above floor (local)

    [Header("Grid/Snapping")]
    private bool snapToGrid = true;
    private float gridSize = 1.0f;

    [Header("Add-Room Helper")]
    private Vector3 defaultRoomSize = new Vector3(10f, 3f, 10f);
    private float gapBetweenRooms = 0.0f; // 0 means rooms “touch” for door alignment

    [MenuItem("Tools/Level Gen/PB Level Builder (Modular)")]
    public static void Open()
    {
        GetWindow<PBLevelBuilderWindow>("PB Level Builder");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Modular ProBuilder Level Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        wallThickness = EditorGUILayout.FloatField("Wall Thickness", wallThickness);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Doors", EditorStyles.boldLabel);
        doorWidth = EditorGUILayout.FloatField("Door Width", doorWidth);
        doorHeight = EditorGUILayout.FloatField("Door Height", doorHeight);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Windows", EditorStyles.boldLabel);
        buildWindows = EditorGUILayout.Toggle("Build Windows", buildWindows);
        windowWidth = EditorGUILayout.FloatField("Window Width", windowWidth);
        windowHeight = EditorGUILayout.FloatField("Window Height", windowHeight);
        windowBottom = EditorGUILayout.FloatField("Window Bottom (above floor)", windowBottom);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        snapToGrid = EditorGUILayout.Toggle("Snap To Grid", snapToGrid);
        gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Add Room Helper", EditorStyles.boldLabel);
        defaultRoomSize = EditorGUILayout.Vector3Field("Default Room Size", defaultRoomSize);
        gapBetweenRooms = EditorGUILayout.FloatField("Gap Between Rooms", gapBetweenRooms);

        EditorGUILayout.Space(10);

        var layout = FindAnyLayout();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create Layout Object"))
            {
                CreateLayoutObject();
            }

            GUI.enabled = layout != null;
            if (GUILayout.Button("Build From Layout"))
            {
                Build(layout);
            }
            if (GUILayout.Button("Clear Built Geometry"))
            {
                ClearBuilt(layout);
            }
            GUI.enabled = true;
        }

        EditorGUILayout.Space(10);

        GUI.enabled = layout != null;
        if (GUILayout.Button("Add Room (Attach To Selected Room, East)"))
        {
            AddRoomAttached(layout, PBLevelLayout.Dir.East);
        }
        if (GUILayout.Button("Add Room (Attach To Selected Room, West)"))
        {
            AddRoomAttached(layout, PBLevelLayout.Dir.West);
        }
        if (GUILayout.Button("Add Room (Attach To Selected Room, North)"))
        {
            AddRoomAttached(layout, PBLevelLayout.Dir.North);
        }
        if (GUILayout.Button("Add Room (Attach To Selected Room, South)"))
        {
            AddRoomAttached(layout, PBLevelLayout.Dir.South);
        }
        GUI.enabled = true;

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Workflow:\n" +
            "1) Create Layout Object\n" +
            "2) Add rooms + edit their centers/sizes in Inspector\n" +
            "3) Add connections (doors) by editing Layout.connections in Inspector\n" +
            "4) Build From Layout\n\n" +
            "Tip: Select the Layout object to edit rooms and connections.",
            MessageType.Info);
    }

    // -------------------- Layout Helpers --------------------

    private static PBLevelLayout FindAnyLayout()
    {
        return FindFirstObjectByType<PBLevelLayout>();
    }

    private void CreateLayoutObject()
    {
        var go = new GameObject("PB_LevelLayout");
        var layout = go.AddComponent<PBLevelLayout>();

        // Seed with one room
        var r = new PBLevelLayout.Room
        {
            name = "Hub",
            center = Vector3.zero,
            size = new Vector3(14f, 3f, 10f),
            windowsNorth = true
        };
        layout.rooms.Add(r);

        Selection.activeGameObject = go;
    }

    private void AddRoomAttached(PBLevelLayout layout, PBLevelLayout.Dir dir)
    {
        if (layout == null) return;

        // We attach to the currently selected room root if possible.
        // Selection should be a generated room root OR the layout object, but we’ll prefer a room root.
        string selectedRoomId = TryGetSelectedBuiltRoomId();
        if (string.IsNullOrEmpty(selectedRoomId))
        {
            if (layout.rooms.Count == 0) return;
            selectedRoomId = layout.rooms[0].id;
        }

        var baseRoom = layout.GetRoom(selectedRoomId);
        if (baseRoom == null) return;

        var newRoom = new PBLevelLayout.Room
        {
            name = $"Room_{layout.rooms.Count}",
            size = defaultRoomSize
        };

        // Compute center so rooms touch (plus optional gap) and doors align on shared wall.
        Vector3 offset = ComputeAttachOffset(baseRoom.size, newRoom.size, dir, gapBetweenRooms);
        newRoom.center = Snap(baseRoom.center + offset);

        layout.rooms.Add(newRoom);

        // Add a connection between them (door on shared walls)
        var conn = new PBLevelLayout.Connection
        {
            aRoomId = baseRoom.id,
            bRoomId = newRoom.id,
            aDoorDir = dir,
            bDoorDir = Opposite(dir)
        };
        layout.connections.Add(conn);

        EditorUtility.SetDirty(layout);
        Selection.activeGameObject = layout.gameObject;
        Debug.Log($"Added {newRoom.name} attached to {baseRoom.name} ({dir}). Now click Build From Layout.");
    }

    private Vector3 ComputeAttachOffset(Vector3 aSize, Vector3 bSize, PBLevelLayout.Dir dir, float gap)
    {
        // rooms assumed axis-aligned
        float ax = aSize.x * 0.5f;
        float az = aSize.z * 0.5f;
        float bx = bSize.x * 0.5f;
        float bz = bSize.z * 0.5f;

        return dir switch
        {
            PBLevelLayout.Dir.East  => new Vector3(ax + bx + gap, 0f, 0f),
            PBLevelLayout.Dir.West  => new Vector3(-(ax + bx + gap), 0f, 0f),
            PBLevelLayout.Dir.North => new Vector3(0f, 0f, az + bz + gap),
            PBLevelLayout.Dir.South => new Vector3(0f, 0f, -(az + bz + gap)),
            _ => Vector3.zero
        };
    }

    private PBLevelLayout.Dir Opposite(PBLevelLayout.Dir d)
    {
        return d switch
        {
            PBLevelLayout.Dir.North => PBLevelLayout.Dir.South,
            PBLevelLayout.Dir.South => PBLevelLayout.Dir.North,
            PBLevelLayout.Dir.East => PBLevelLayout.Dir.West,
            PBLevelLayout.Dir.West => PBLevelLayout.Dir.East,
            _ => PBLevelLayout.Dir.North
        };
    }

    private Vector3 Snap(Vector3 v)
    {
        if (!snapToGrid || gridSize <= 0.0001f) return v;
        v.x = Mathf.Round(v.x / gridSize) * gridSize;
        v.y = Mathf.Round(v.y / gridSize) * gridSize;
        v.z = Mathf.Round(v.z / gridSize) * gridSize;
        return v;
    }

    private string TryGetSelectedBuiltRoomId()
    {
        var go = Selection.activeGameObject;
        if (go == null) return null;
        var marker = go.GetComponent<PBBuiltRoomMarker>();
        if (marker != null) return marker.roomId;

        // If user selected a child piece, walk up
        var parentMarker = go.GetComponentInParent<PBBuiltRoomMarker>();
        return parentMarker != null ? parentMarker.roomId : null;
    }

    // -------------------- Build/Clear --------------------

    private void ClearBuilt(PBLevelLayout layout)
    {
        if (layout == null) return;

        var existing = layout.transform.Find("PB_BuiltGeometry");
        if (existing != null)
        {
            DestroyImmediate(existing.gameObject);
        }
    }

    private void Build(PBLevelLayout layout)
    {
        if (layout == null) return;

        ClearBuilt(layout);

        var builtRoot = new GameObject("PB_BuiltGeometry");
        builtRoot.transform.SetParent(layout.transform, false);

        // For quick lookup
        System.Collections.Generic.Dictionary<string, PBLevelLayout.Room> roomById = new();
        foreach (var r in layout.rooms)
            roomById[r.id] = r;

        // Build each room with door flags derived from connections
        foreach (var r in layout.rooms)
        {
            bool doorN = false, doorS = false, doorE = false, doorW = false;

            foreach (var c in layout.connections)
            {
                if (c.aRoomId == r.id)
                    SetDoorFlag(c.aDoorDir, ref doorN, ref doorS, ref doorE, ref doorW);
                else if (c.bRoomId == r.id)
                    SetDoorFlag(c.bDoorDir, ref doorN, ref doorS, ref doorE, ref doorW);
            }

            // build room
            var roomGO = CreateHollowRoom(
                name: r.name,
                center: Snap(r.center),
                size: r.size,
                thickness: wallThickness,
                parent: builtRoot.transform,
                doorNorth: doorN,
                doorSouth: doorS,
                doorEast: doorE,
                doorWest: doorW,
                windows: buildWindows,
                winN: r.windowsNorth,
                winS: r.windowsSouth,
                winE: r.windowsEast,
                winW: r.windowsWest
            );

            // marker for later “add room attached”
            var marker = roomGO.AddComponent<PBBuiltRoomMarker>();
            marker.roomId = r.id;
        }

        Selection.activeGameObject = builtRoot;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    private void SetDoorFlag(PBLevelLayout.Dir dir, ref bool n, ref bool s, ref bool e, ref bool w)
    {
        switch (dir)
        {
            case PBLevelLayout.Dir.North: n = true; break;
            case PBLevelLayout.Dir.South: s = true; break;
            case PBLevelLayout.Dir.East:  e = true; break;
            case PBLevelLayout.Dir.West:  w = true; break;
        }
    }

    // -------------------- ProBuilder Geometry --------------------

    private enum WallAxis { X, Z } // span axis in local space

    private GameObject CreateHollowRoom(
        string name,
        Vector3 center,
        Vector3 size,
        float thickness,
        Transform parent,
        bool doorNorth,
        bool doorSouth,
        bool doorEast,
        bool doorWest,
        bool windows,
        bool winN,
        bool winS,
        bool winE,
        bool winW)
    {
        var roomRoot = new GameObject(name);
        roomRoot.transform.SetParent(parent, false);
        roomRoot.transform.position = center;

        float halfX = size.x * 0.5f;
        float halfY = size.y * 0.5f;
        float halfZ = size.z * 0.5f;

        // Floor & ceiling
        CreatePBBox($"{name}_Floor", new Vector3(0f, -halfY + thickness * 0.5f, 0f), new Vector3(size.x, thickness, size.z), roomRoot.transform);
        CreatePBBox($"{name}_Ceiling", new Vector3(0f, halfY - thickness * 0.5f, 0f), new Vector3(size.x, thickness, size.z), roomRoot.transform);

        // North wall (+Z) spans X
        BuildWall(
            $"{name}_Wall_North",
            roomRoot.transform,
            new Vector3(0f, 0f, +halfZ - thickness * 0.5f),
            span: size.x,
            height: size.y,
            thickness: thickness,
            axis: WallAxis.X,
            door: doorNorth,
            window: windows && winN
        );

        // South wall (-Z)
        BuildWall(
            $"{name}_Wall_South",
            roomRoot.transform,
            new Vector3(0f, 0f, -halfZ + thickness * 0.5f),
            span: size.x,
            height: size.y,
            thickness: thickness,
            axis: WallAxis.X,
            door: doorSouth,
            window: windows && winS
        );

        // West wall (-X) spans Z
        BuildWall(
            $"{name}_Wall_West",
            roomRoot.transform,
            new Vector3(-halfX + thickness * 0.5f, 0f, 0f),
            span: size.z,
            height: size.y,
            thickness: thickness,
            axis: WallAxis.Z,
            door: doorWest,
            window: windows && winW
        );

        // East wall (+X)
        BuildWall(
            $"{name}_Wall_East",
            roomRoot.transform,
            new Vector3(+halfX - thickness * 0.5f, 0f, 0f),
            span: size.z,
            height: size.y,
            thickness: thickness,
            axis: WallAxis.Z,
            door: doorEast,
            window: windows && winE
        );

        return roomRoot;
    }

    private void BuildWall(
        string wallName,
        Transform parent,
        Vector3 wallCenterLocal,
        float span,
        float height,
        float thickness,
        WallAxis axis,
        bool door,
        bool window)
    {
        // If door exists, build a door cutout at center
        if (door)
        {
            CreateRectCutoutWall(
                wallName,
                parent,
                wallCenterLocal,
                span,
                height,
                thickness,
                axis,
                holeWidth: doorWidth,
                holeHeight: doorHeight,
                holeBottomFromFloor: 0f // door starts at floor
            );
            return;
        }

        // Else if window exists, build a window cutout at center (elevated)
        if (window)
        {
            CreateRectCutoutWall(
                wallName,
                parent,
                wallCenterLocal,
                span,
                height,
                thickness,
                axis,
                holeWidth: windowWidth,
                holeHeight: windowHeight,
                holeBottomFromFloor: windowBottom
            );
            return;
        }

        // Else plain wall
        Vector3 wallSize = axis == WallAxis.X
            ? new Vector3(span, height, thickness)
            : new Vector3(thickness, height, span);

        CreatePBBox(wallName, wallCenterLocal, wallSize, parent);
    }

    /// <summary>
    /// Creates a wall composed of up to 4 segments around a rectangular hole (door/window).
    /// Hole is centered along span axis; bottom of hole is holeBottomFromFloor (measured from floor).
    /// </summary>
    private void CreateRectCutoutWall(
        string wallName,
        Transform parent,
        Vector3 wallCenterLocal,
        float span,
        float height,
        float thickness,
        WallAxis axis,
        float holeWidth,
        float holeHeight,
        float holeBottomFromFloor)
    {
        holeWidth = Mathf.Clamp(holeWidth, 0.5f, span - 0.5f);
        holeHeight = Mathf.Clamp(holeHeight, 0.5f, height - 0.2f);

        float halfSpan = span * 0.5f;
        float halfH = height * 0.5f;

        // Local Y: floor is at -halfH
        float holeBottomY = -halfH + holeBottomFromFloor;
        float holeTopY = holeBottomY + holeHeight;

        // Clamp vertically inside wall
        holeBottomY = Mathf.Clamp(holeBottomY, -halfH + 0.05f, halfH - 0.55f);
        holeTopY = Mathf.Clamp(holeTopY, holeBottomY + 0.3f, halfH - 0.05f);

        float leftSeg = ( -holeWidth * 0.5f + halfSpan );
        float rightSeg = ( halfSpan - holeWidth * 0.5f );

        // Actually compute left/right segment widths around a centered hole
        float holeLeft = -holeWidth * 0.5f;
        float holeRight = +holeWidth * 0.5f;

        float leftWidth = holeLeft + halfSpan;
        float rightWidth = halfSpan - holeRight;

        float bottomHeight = holeBottomY - (-halfH);
        float topHeight = halfH - holeTopY;

        Vector3 SpanOffset(float s, float y) => axis == WallAxis.X ? new Vector3(s, y, 0f) : new Vector3(0f, y, s);

        // Left segment (full height)
        if (leftWidth > 0.001f)
        {
            float centerS = -halfSpan + leftWidth * 0.5f;
            Vector3 segSize = axis == WallAxis.X ? new Vector3(leftWidth, height, thickness) : new Vector3(thickness, height, leftWidth);
            CreatePBBox($"{wallName}_Left", wallCenterLocal + SpanOffset(centerS, 0f), segSize, parent);
        }

        // Right segment (full height)
        if (rightWidth > 0.001f)
        {
            float centerS = halfSpan - rightWidth * 0.5f;
            Vector3 segSize = axis == WallAxis.X ? new Vector3(rightWidth, height, thickness) : new Vector3(thickness, height, rightWidth);
            CreatePBBox($"{wallName}_Right", wallCenterLocal + SpanOffset(centerS, 0f), segSize, parent);
        }

        // Bottom segment (sill) across hole width
        if (bottomHeight > 0.001f)
        {
            float centerY = (-halfH) + bottomHeight * 0.5f;
            Vector3 segSize = axis == WallAxis.X ? new Vector3(holeWidth, bottomHeight, thickness) : new Vector3(thickness, bottomHeight, holeWidth);
            CreatePBBox($"{wallName}_Bottom", wallCenterLocal + SpanOffset(0f, centerY), segSize, parent);
        }

        // Top segment (lintel) across hole width
        if (topHeight > 0.001f)
        {
            float centerY = holeTopY + topHeight * 0.5f;
            Vector3 segSize = axis == WallAxis.X ? new Vector3(holeWidth, topHeight, thickness) : new Vector3(thickness, topHeight, holeWidth);
            CreatePBBox($"{wallName}_Top", wallCenterLocal + SpanOffset(0f, centerY), segSize, parent);
        }
    }

    private static GameObject CreatePBBox(string name, Vector3 localPos, Vector3 localSize, Transform parent)
    {
        ProBuilderMesh pb = ShapeGenerator.GenerateCube(PivotLocation.Center, localSize);
        pb.gameObject.name = name;
        pb.transform.SetParent(parent, false);
        pb.transform.localPosition = localPos;

        if (!pb.gameObject.TryGetComponent<MeshCollider>(out _))
            pb.gameObject.AddComponent<MeshCollider>();

        pb.ToMesh();
        pb.Refresh();

        return pb.gameObject;
    }
}

/// <summary>
/// Marker component added to each built room root so we can attach new rooms relative to selection.
/// </summary>
public class PBBuiltRoomMarker : MonoBehaviour
{
    public string roomId;
}
#endif
