#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

public class ProBuilderLevelGenerator : EditorWindow
{
    // Room dimensions (Unity units)
    private Vector3 mainRoomSize = new Vector3(12f, 3f, 10f);
    private Vector3 corridorSize = new Vector3(3f, 3f, 8f);
    private Vector3 sideRoomSize = new Vector3(8f, 3f, 8f);

    private float wallThickness = 0.2f;

    // Door settings
    private float doorWidth = 1.6f;
    private float doorHeight = 2.1f;

    // Where along the corridor the side doors are (0..1, from start to end)
    private float corridorSideDoorT = 0.85f;

    [MenuItem("Tools/Level Gen/Generate ProBuilder Hub + Corridor + 2 Rooms (Doors)")]
    public static void Open()
    {
        GetWindow<ProBuilderLevelGenerator>("PB Level Gen (Doors)");
    }

    private void OnGUI()
    {
        GUILayout.Label("ProBuilder Level Generator (with doors)", EditorStyles.boldLabel);

        mainRoomSize = EditorGUILayout.Vector3Field("Main Room Size", mainRoomSize);
        corridorSize = EditorGUILayout.Vector3Field("Corridor Size", corridorSize);
        sideRoomSize = EditorGUILayout.Vector3Field("Side Room Size", sideRoomSize);

        wallThickness = EditorGUILayout.FloatField("Wall Thickness", wallThickness);

        GUILayout.Space(6);
        doorWidth = EditorGUILayout.FloatField("Door Width", doorWidth);
        doorHeight = EditorGUILayout.FloatField("Door Height", doorHeight);
        corridorSideDoorT = EditorGUILayout.Slider("Corridor Side Door Position (T)", corridorSideDoorT, 0.2f, 0.95f);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Level"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        var root = new GameObject("GeneratedLevel_ProBuilder");

        Vector3 hubCenter = Vector3.zero;

        // Corridor is in front of hub
        float hubFrontZ = hubCenter.z + mainRoomSize.z * 0.5f;
        Vector3 corridorCenter = new Vector3(hubCenter.x, hubCenter.y, hubFrontZ + corridorSize.z * 0.5f);

        // Corridor end Z (front)
        float corridorEndZ = corridorCenter.z + corridorSize.z * 0.5f;

        // Place side rooms so their "back" faces align to corridor side openings
        // Left room is left of corridor; its back faces corridor (i.e., the wall at -Z of the room points toward corridor)
        // We'll place rooms with their back wall near corridorEndZ *approx*, and centered around that.
        Vector3 leftRoomCenter = new Vector3(
            corridorCenter.x - (corridorSize.x * 0.5f) - (sideRoomSize.x * 0.5f),
            hubCenter.y,
            corridorCenter.z + Mathf.Lerp(-corridorSize.z * 0.5f, corridorSize.z * 0.5f, corridorSideDoorT)
        );

        Vector3 rightRoomCenter = new Vector3(
            corridorCenter.x + (corridorSize.x * 0.5f) + (sideRoomSize.x * 0.5f),
            hubCenter.y,
            corridorCenter.z + Mathf.Lerp(-corridorSize.z * 0.5f, corridorSize.z * 0.5f, corridorSideDoorT)
        );

        // Build hub with a FRONT door
        var hub = CreateHollowRoomWithDoors(
            name: "Hub",
            center: hubCenter,
            size: mainRoomSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: true,
            backDoor: false,
            leftDoor: false,
            rightDoor: false,
            sideDoorZOffset: 0f
        );

        // Build corridor with a BACK door (to hub) and LEFT/RIGHT doors near corridor end
        var corridor = CreateHollowRoomWithDoors(
            name: "Corridor",
            center: corridorCenter,
            size: corridorSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false,
            backDoor: true,
            leftDoor: true,
            rightDoor: true,
            sideDoorZOffset: Mathf.Lerp(-corridorSize.z * 0.5f, corridorSize.z * 0.5f, corridorSideDoorT)
        );

        // Left room: door on RIGHT wall (facing corridor)
        var roomL = CreateHollowRoomWithDoors(
            name: "Room_Left",
            center: leftRoomCenter,
            size: sideRoomSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false,
            backDoor: false,
            leftDoor: false,
            rightDoor: true,
            sideDoorZOffset: 0f // center door
        );

        // Right room: door on LEFT wall (facing corridor)
        var roomR = CreateHollowRoomWithDoors(
            name: "Room_Right",
            center: rightRoomCenter,
            size: sideRoomSize,
            thickness: wallThickness,
            parent: root.transform,
            frontDoor: false,
            backDoor: false,
            leftDoor: true,
            rightDoor: false,
            sideDoorZOffset: 0f // center door
        );

        Selection.activeGameObject = root;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    /// <summary>
    /// Hollow room built from floor, ceiling, and 4 walls.
    /// Any wall can include a doorway cut-out by building wall segments around the door.
    /// sideDoorZOffset is used for left/right walls to position door along Z (local space).
    /// </summary>
    private GameObject CreateHollowRoomWithDoors(
        string name,
        Vector3 center,
        Vector3 size,
        float thickness,
        Transform parent,
        bool frontDoor,
        bool backDoor,
        bool leftDoor,
        bool rightDoor,
        float sideDoorZOffset)
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

        // Walls
        // Front wall (local +Z)
        if (frontDoor)
        {
            CreateWallWithDoor(
                wallName: $"{name}_Wall_Front",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(0f, 0f, +halfZ - thickness * 0.5f),
                wallSpanWidth: size.x,
                wallHeight: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorOffsetAlongSpan: 0f,
                wallAxis: WallAxis.X
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Front", new Vector3(0f, 0f, +halfZ - thickness * 0.5f), new Vector3(size.x, size.y, thickness), roomRoot.transform);
        }

        // Back wall (local -Z)
        if (backDoor)
        {
            CreateWallWithDoor(
                wallName: $"{name}_Wall_Back",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(0f, 0f, -halfZ + thickness * 0.5f),
                wallSpanWidth: size.x,
                wallHeight: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorOffsetAlongSpan: 0f,
                wallAxis: WallAxis.X
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Back", new Vector3(0f, 0f, -halfZ + thickness * 0.5f), new Vector3(size.x, size.y, thickness), roomRoot.transform);
        }

        // Left wall (local -X)
        if (leftDoor)
        {
            CreateWallWithDoor(
                wallName: $"{name}_Wall_Left",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(-halfX + thickness * 0.5f, 0f, 0f),
                wallSpanWidth: size.z,
                wallHeight: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorOffsetAlongSpan: sideDoorZOffset,
                wallAxis: WallAxis.Z
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Left", new Vector3(-halfX + thickness * 0.5f, 0f, 0f), new Vector3(thickness, size.y, size.z), roomRoot.transform);
        }

        // Right wall (local +X)
        if (rightDoor)
        {
            CreateWallWithDoor(
                wallName: $"{name}_Wall_Right",
                parent: roomRoot.transform,
                wallCenterLocal: new Vector3(+halfX - thickness * 0.5f, 0f, 0f),
                wallSpanWidth: size.z,
                wallHeight: size.y,
                thickness: thickness,
                doorWidth: doorWidth,
                doorHeight: doorHeight,
                doorOffsetAlongSpan: sideDoorZOffset,
                wallAxis: WallAxis.Z
            );
        }
        else
        {
            CreatePBBox($"{name}_Wall_Right", new Vector3(+halfX - thickness * 0.5f, 0f, 0f), new Vector3(thickness, size.y, size.z), roomRoot.transform);
        }

        return roomRoot;
    }

    private enum WallAxis { X, Z }

    /// <summary>
    /// Builds a wall panel with a rectangular doorway cutout by composing 3 ProBuilder boxes:
    /// left segment, right segment, and a lintel (top segment). No bottom segment (door goes to floor).
    ///
    /// wallAxis determines which local axis is the "span":
    /// - X means wall spans along local X (front/back walls)
    /// - Z means wall spans along local Z (left/right walls)
    /// doorOffsetAlongSpan is the door center offset along the span axis (local units).
    /// </summary>
    private void CreateWallWithDoor(
        string wallName,
        Transform parent,
        Vector3 wallCenterLocal,
        float wallSpanWidth,
        float wallHeight,
        float thickness,
        float doorWidth,
        float doorHeight,
        float doorOffsetAlongSpan,
        WallAxis wallAxis)
    {
        // Clamp door sizes reasonably
        doorWidth = Mathf.Clamp(doorWidth, 0.5f, wallSpanWidth - 0.5f);
        doorHeight = Mathf.Clamp(doorHeight, 1.0f, wallHeight - 0.2f);

        float halfSpan = wallSpanWidth * 0.5f;

        float doorLeft = doorOffsetAlongSpan - doorWidth * 0.5f;
        float doorRight = doorOffsetAlongSpan + doorWidth * 0.5f;

        float leftSegWidth = Mathf.Max(0f, (doorLeft + halfSpan));
        float rightSegWidth = Mathf.Max(0f, (halfSpan - doorRight));

        float lintelHeight = Mathf.Max(0f, wallHeight - doorHeight);
        float doorTopY = -wallHeight * 0.5f + doorHeight;
        float lintelCenterY = doorTopY + lintelHeight * 0.5f;

        // Helper to convert span offsets into local position
        Vector3 SpanOffset(float spanCenterOffset, float yOffset)
        {
            if (wallAxis == WallAxis.X)
                return new Vector3(spanCenterOffset, yOffset, 0f);
            else
                return new Vector3(0f, yOffset, spanCenterOffset);
        }

        // Left segment
        if (leftSegWidth > 0.001f)
        {
            float leftCenter = -halfSpan + leftSegWidth * 0.5f;
            Vector3 segSize = (wallAxis == WallAxis.X)
                ? new Vector3(leftSegWidth, wallHeight, thickness)
                : new Vector3(thickness, wallHeight, leftSegWidth);

            CreatePBBox($"{wallName}_Left", wallCenterLocal + SpanOffset(leftCenter, 0f), segSize, parent);
        }

        // Right segment
        if (rightSegWidth > 0.001f)
        {
            float rightCenter = halfSpan - rightSegWidth * 0.5f;
            Vector3 segSize = (wallAxis == WallAxis.X)
                ? new Vector3(rightSegWidth, wallHeight, thickness)
                : new Vector3(thickness, wallHeight, rightSegWidth);

            CreatePBBox($"{wallName}_Right", wallCenterLocal + SpanOffset(rightCenter, 0f), segSize, parent);
        }

        // Lintel (top segment above door)
        if (lintelHeight > 0.001f)
        {
            Vector3 lintelSize = (wallAxis == WallAxis.X)
                ? new Vector3(doorWidth, lintelHeight, thickness)
                : new Vector3(thickness, lintelHeight, doorWidth);

            CreatePBBox($"{wallName}_Top", wallCenterLocal + SpanOffset(doorOffsetAlongSpan, lintelCenterY), lintelSize, parent);
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
