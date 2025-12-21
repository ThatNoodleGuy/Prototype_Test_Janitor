#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

public class ProBuilderLevelGenerator_TJunction : EditorWindow
{
    [Header("Sizes (Unity units / meters)")]
    private Vector3 hubSize = new Vector3(16f, 3f, 10f);

    // Main corridor runs forward (+Z)
    private Vector3 mainCorridorSize = new Vector3(4f, 3f, 18f);

    // Side connector: we interpret as:
    // - size.z = connector length along X (how far it sticks out from corridor)
    // - size.x = connector width along Z (how wide the stub is)
    // - size.y = height
    private Vector3 sideConnectorSize = new Vector3(4f, 3f, 6f); // (widthZ, height, lengthX)

    private Vector3 sideRoomSize = new Vector3(10f, 3f, 10f);

    [Header("Construction")]
    private float wallThickness = 0.2f;

    [Header("Doors")]
    private float doorWidth = 1.8f;
    private float doorHeight = 2.2f;

    [Header("T-Junction Placement")]
    [Tooltip("Where along the main corridor the side doors/connectors are placed (0=start near hub, 1=end).")]
    private float sideDoorT = 0.90f;

    [MenuItem("Tools/Level Gen/Generate ProBuilder T-Junction Layout (Doors)")]
    public static void Open()
    {
        GetWindow<ProBuilderLevelGenerator_TJunction>("PB Level Gen (T)");
    }

    private void OnGUI()
    {
        GUILayout.Label("ProBuilder T-Junction Generator (with aligned doors)", EditorStyles.boldLabel);

        hubSize = EditorGUILayout.Vector3Field("Hub Size", hubSize);
        mainCorridorSize = EditorGUILayout.Vector3Field("Main Corridor Size", mainCorridorSize);
        sideConnectorSize = EditorGUILayout.Vector3Field("Side Connector Size (widthZ, height, lengthX)", sideConnectorSize);
        sideRoomSize = EditorGUILayout.Vector3Field("Side Room Size", sideRoomSize);

        wallThickness = EditorGUILayout.FloatField("Wall Thickness", wallThickness);

        GUILayout.Space(6);
        doorWidth = EditorGUILayout.FloatField("Door Width", doorWidth);
        doorHeight = EditorGUILayout.FloatField("Door Height", doorHeight);

        GUILayout.Space(6);
        sideDoorT = EditorGUILayout.Slider("Side Door Position (T)", sideDoorT, 0.2f, 0.98f);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Level"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        var root = new GameObject("GeneratedLevel_ProBuilder_T");

        // --- Placement plan (top-down):
        // Hub centered at (0,0,0)
        // Main corridor extends forward (+Z) from hub front
        // At some point near corridor end, create left/right connector stubs extending along X
        // Rooms sit at the outer ends of the stubs.

        Vector3 hubCenter = Vector3.zero;

        // Main corridor starts at hub front
        float hubFrontZ = hubCenter.z + hubSize.z * 0.5f;
        Vector3 mainCorridorCenter = new Vector3(
            hubCenter.x,
            hubCenter.y,
            hubFrontZ + mainCorridorSize.z * 0.5f
        );

        float corridorHalfX = mainCorridorSize.x * 0.5f;
        float corridorHalfZ = mainCorridorSize.z * 0.5f;

        // Side door position along corridor (local Z)
        float sideDoorLocalZ = Mathf.Lerp(-corridorHalfZ, corridorHalfZ, sideDoorT);
        float sideDoorWorldZ = mainCorridorCenter.z + sideDoorLocalZ;

        // Interpret connector params
        float connectorWidthZ = Mathf.Max(1f, sideConnectorSize.x);  // width along Z
        float connectorHeight = Mathf.Max(1f, sideConnectorSize.y);  // height
        float connectorLenX = Mathf.Max(1f, sideConnectorSize.z);    // length along X (sticks out)

        // Corridor wall X positions
        float leftWallX = mainCorridorCenter.x - corridorHalfX;
        float rightWallX = mainCorridorCenter.x + corridorHalfX;

        // Place connector centers so their INNER face is flush with corridor walls:
        // Left connector inner face is its +X face at x = center.x + len/2 -> equals leftWallX
        Vector3 leftConnectorCenter = new Vector3(
            leftWallX - connectorLenX * 0.5f,
            hubCenter.y,
            sideDoorWorldZ
        );

        // Right connector inner face is its -X face at x = center.x - len/2 -> equals rightWallX
        Vector3 rightConnectorCenter = new Vector3(
            rightWallX + connectorLenX * 0.5f,
            hubCenter.y,
            sideDoorWorldZ
        );

        // Outer faces of the connectors (where rooms connect)
        float leftConnectorOuterX = leftConnectorCenter.x - connectorLenX * 0.5f;
        float rightConnectorOuterX = rightConnectorCenter.x + connectorLenX * 0.5f;

        // Place rooms flush to connector outer faces
        Vector3 leftRoomCenter = new Vector3(
            leftConnectorOuterX - sideRoomSize.x * 0.5f,
            hubCenter.y,
            sideDoorWorldZ
        );

        Vector3 rightRoomCenter = new Vector3(
            rightConnectorOuterX + sideRoomSize.x * 0.5f,
            hubCenter.y,
            sideDoorWorldZ
        );

        // --- Build geometry

        // Hub: front door to main corridor
        CreateHollowRoomWithDoors(
            name: "Hub",
            center: hubCenter,
            size: hubSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: true, backDoor: false, leftDoor: false, rightDoor: false,
            sideDoorOffsetAlongSpan: 0f
        );

        // Main corridor: back door to hub, plus left/right doors at the chosen Z offset
        CreateHollowRoomWithDoors(
            name: "MainCorridor",
            center: mainCorridorCenter,
            size: mainCorridorSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false, backDoor: true, leftDoor: true, rightDoor: true,
            sideDoorOffsetAlongSpan: sideDoorLocalZ
        );

        // Left connector: doors on both ends along X (left wall + right wall)
        CreateHollowRoomWithDoors(
            name: "LeftConnector",
            center: leftConnectorCenter,
            size: new Vector3(connectorLenX, connectorHeight, connectorWidthZ),
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false, backDoor: false, leftDoor: true, rightDoor: true,
            sideDoorOffsetAlongSpan: 0f
        );

        // Right connector: doors on both ends along X as well
        CreateHollowRoomWithDoors(
            name: "RightConnector",
            center: rightConnectorCenter,
            size: new Vector3(connectorLenX, connectorHeight, connectorWidthZ),
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false, backDoor: false, leftDoor: true, rightDoor: true,
            sideDoorOffsetAlongSpan: 0f
        );

        // Left room: door on right wall (faces connector)
        CreateHollowRoomWithDoors(
            name: "Room_Left",
            center: leftRoomCenter,
            size: sideRoomSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false, backDoor: false, leftDoor: false, rightDoor: true,
            sideDoorOffsetAlongSpan: 0f
        );

        // Right room: door on left wall (faces connector)
        CreateHollowRoomWithDoors(
            name: "Room_Right",
            center: rightRoomCenter,
            size: sideRoomSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false, backDoor: false, leftDoor: true, rightDoor: false,
            sideDoorOffsetAlongSpan: 0f
        );

        Selection.activeGameObject = root;
        SceneView.lastActiveSceneView?.FrameSelected();
        Debug.Log("Generated T-junction layout with aligned connectors and doors.");
    }

    // ---------- Room building (hollow box with optional door cutouts in walls) ----------

    private enum WallAxis { X, Z }

    /// <summary>
    /// sideDoorOffsetAlongSpan is used for LEFT/RIGHT walls to position the door along the Z span (local space).
    /// For front/back doors we always center them (offset 0).
    /// </summary>
    private void CreateHollowRoomWithDoors(
        string name,
        Vector3 center,
        Vector3 size,
        float thickness,
        Transform parent,
        bool frontDoor,
        bool backDoor,
        bool leftDoor,
        bool rightDoor,
        float sideDoorOffsetAlongSpan)
    {
        var roomRoot = new GameObject(name);
        roomRoot.transform.SetParent(parent, true);
        roomRoot.transform.position = center;

        float halfX = size.x * 0.5f;
        float halfY = size.y * 0.5f;
        float halfZ = size.z * 0.5f;

        // Floor & ceiling
        CreatePBBox($"{name}_Floor", new Vector3(0f, -halfY + thickness * 0.5f, 0f), new Vector3(size.x, thickness, size.z), roomRoot.transform);
        CreatePBBox($"{name}_Ceiling", new Vector3(0f, halfY - thickness * 0.5f, 0f), new Vector3(size.x, thickness, size.z), roomRoot.transform);

        // Front wall (+Z) spans X
        if (frontDoor)
        {
            CreateWallLeavingDoorGap(
                wallName: $"{name}_Wall_Front",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(0f, 0f, +halfZ - thickness * 0.5f),
                span: size.x,
                height: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorCenterAlongSpan: 0f,
                wallAxis: WallAxis.X
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Front", new Vector3(0f, 0f, +halfZ - thickness * 0.5f), new Vector3(size.x, size.y, thickness), roomRoot.transform);
        }

        // Back wall (-Z) spans X
        if (backDoor)
        {
            CreateWallLeavingDoorGap(
                wallName: $"{name}_Wall_Back",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(0f, 0f, -halfZ + thickness * 0.5f),
                span: size.x,
                height: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorCenterAlongSpan: 0f,
                wallAxis: WallAxis.X
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Back", new Vector3(0f, 0f, -halfZ + thickness * 0.5f), new Vector3(size.x, size.y, thickness), roomRoot.transform);
        }

        // Left wall (-X) spans Z
        if (leftDoor)
        {
            CreateWallLeavingDoorGap(
                wallName: $"{name}_Wall_Left",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(-halfX + thickness * 0.5f, 0f, 0f),
                span: size.z,
                height: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorCenterAlongSpan: sideDoorOffsetAlongSpan,
                wallAxis: WallAxis.Z
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Left", new Vector3(-halfX + thickness * 0.5f, 0f, 0f), new Vector3(thickness, size.y, size.z), roomRoot.transform);
        }

        // Right wall (+X) spans Z
        if (rightDoor)
        {
            CreateWallLeavingDoorGap(
                wallName: $"{name}_Wall_Right",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(+halfX - thickness * 0.5f, 0f, 0f),
                span: size.z,
                height: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorCenterAlongSpan: sideDoorOffsetAlongSpan,
                wallAxis: WallAxis.Z
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Right", new Vector3(+halfX - thickness * 0.5f, 0f, 0f), new Vector3(thickness, size.y, size.z), roomRoot.transform);
        }
    }

    /// <summary>
    /// Builds a wall with a rectangular doorway cutout by composing 3 ProBuilder boxes:
    /// left segment, right segment, and a lintel (top segment). Door reaches the floor.
    /// wallAxis determines the span axis (X or Z in local space).
    /// </summary>
    private static void CreateWallLeavingDoorGap(
        string wallName,
        Transform parent,
        Vector3 wallCenterLocal,
        float span,
        float height,
        float thickness,
        float doorWidth,
        float doorHeight,
        float doorCenterAlongSpan,
        WallAxis wallAxis)
    {
        doorWidth = Mathf.Clamp(doorWidth, 0.5f, span - 0.5f);
        doorHeight = Mathf.Clamp(doorHeight, 1.0f, height - 0.2f);

        float halfSpan = span * 0.5f;

        float doorLeft = doorCenterAlongSpan - doorWidth * 0.5f;
        float doorRight = doorCenterAlongSpan + doorWidth * 0.5f;

        float leftSeg = Mathf.Max(0f, (doorLeft + halfSpan));
        float rightSeg = Mathf.Max(0f, (halfSpan - doorRight));

        float lintelH = Mathf.Max(0f, height - doorHeight);
        float doorTopY = -height * 0.5f + doorHeight;
        float lintelCenterY = doorTopY + lintelH * 0.5f;

        Vector3 SpanOffset(float spanOffset, float yOffset)
        {
            return wallAxis == WallAxis.X
                ? new Vector3(spanOffset, yOffset, 0f)
                : new Vector3(0f, yOffset, spanOffset);
        }

        // Left segment
        if (leftSeg > 0.001f)
        {
            float c = -halfSpan + leftSeg * 0.5f;
            Vector3 sz = wallAxis == WallAxis.X
                ? new Vector3(leftSeg, height, thickness)
                : new Vector3(thickness, height, leftSeg);

            CreatePBBox($"{wallName}_Left", wallCenterLocal + SpanOffset(c, 0f), sz, parent);
        }

        // Right segment
        if (rightSeg > 0.001f)
        {
            float c = halfSpan - rightSeg * 0.5f;
            Vector3 sz = wallAxis == WallAxis.X
                ? new Vector3(rightSeg, height, thickness)
                : new Vector3(thickness, height, rightSeg);

            CreatePBBox($"{wallName}_Right", wallCenterLocal + SpanOffset(c, 0f), sz, parent);
        }

        // Lintel (top segment)
        if (lintelH > 0.001f)
        {
            Vector3 sz = wallAxis == WallAxis.X
                ? new Vector3(doorWidth, lintelH, thickness)
                : new Vector3(thickness, lintelH, doorWidth);

            CreatePBBox($"{wallName}_Top", wallCenterLocal + SpanOffset(doorCenterAlongSpan, lintelCenterY), sz, parent);
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
#endif
