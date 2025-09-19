using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class LocationData
{
    [Header("Position (WGS84)")]
    public double latitude;
    public double longitude;
    public double altitude;

    [Header("Rotation (Degrees)")]
    public float heading;  // Yaw
    public float pitch;    // Tilt
    public float roll;

    public ArcGISPoint ToArcGISPoint()
    {
        return new ArcGISPoint(longitude, latitude, altitude, ArcGISSpatialReference.WGS84());
    }

    public ArcGISRotation ToArcGISRotation()
    {
        return new ArcGISRotation(pitch, heading, roll);
    }
}

public class ChangeLocation : MonoBehaviour
{
    [Header("Locations (Editable in Inspector)")]
    [SerializeField] private LocationData[] locations;

    private int currentIndex = 0;

    private ArcGISMapComponent mapComponent;
    private ArcGISCameraComponent cameraComponent;

    // Input Actions wrapper (auto-generated from ChangeLocationActions.inputactions)
    private ChangeLocationActions inputActions;

    private void Awake()
    {
        // Find ArcGIS Map + Camera
        mapComponent = FindFirstObjectByType<ArcGISMapComponent>();
        cameraComponent = mapComponent.GetComponentInChildren<ArcGISCameraComponent>();

        // Initialize Input Actions
        inputActions = new ChangeLocationActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Actions.CycleLocationForward.performed += OnCycleForward;
        inputActions.Actions.CycleLocationBackward.performed += OnCycleBackward;
    }

    private void OnDisable()
    {
        inputActions.Actions.CycleLocationForward.performed -= OnCycleForward;
        inputActions.Actions.CycleLocationBackward.performed -= OnCycleBackward;

        inputActions.Disable();
    }

    private void OnCycleForward(InputAction.CallbackContext ctx)
    {
        currentIndex++;
        if (currentIndex >= locations.Length)
            currentIndex = 0;

        MoveToLocation(locations[currentIndex]);
    }

    private void OnCycleBackward(InputAction.CallbackContext ctx)
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = locations.Length - 1;

        MoveToLocation(locations[currentIndex]);
    }

    private void MoveToLocation(LocationData location)
    {
        if (cameraComponent == null || mapComponent == null)
        {
            Debug.LogError("ArcGIS components not found in scene!");
            return;
        }

        var cameraLocationComp = cameraComponent.GetComponent<ArcGISLocationComponent>();

        cameraLocationComp.Position = location.ToArcGISPoint();
        cameraLocationComp.Rotation = location.ToArcGISRotation();

        // Optional: shift the map origin too
        mapComponent.OriginPosition = location.ToArcGISPoint();

        Debug.Log($"Moved to Location {currentIndex}: " +
                  $"Lat={location.latitude}, Lon={location.longitude}, Alt={location.altitude}, " +
                  $"Heading={location.heading}, Pitch={location.pitch}, Roll={location.roll}");
    }
}