using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Diagnostic tool to identify physics issues in helicopter explosion
/// Tracks all forces, velocities, and physics settings on explosion shards
/// </summary>
public class ExplosionPhysicsDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool enableDiagnostics = true;
    [SerializeField] private bool logPhysicsSettings = true;
    [SerializeField] private bool logForceApplications = true;
    [SerializeField] private bool logVelocityChanges = true;
    [SerializeField] private float diagnosticDuration = 10f;
    
    [Header("Real-time Monitoring")]
    [SerializeField] private bool showRealTimeStats = true;
    [SerializeField] private int maxShardsToTrack = 10;
    
    private List<ShardDiagnosticData> trackedShards = new List<ShardDiagnosticData>();
    private float diagnosticStartTime;
    private bool isDiagnosticActive = false;
    
    [System.Serializable]
    public class ShardDiagnosticData
    {
        public string name;
        public Rigidbody rigidbody;
        public Vector3 initialPosition;
        public Vector3 lastPosition;
        public Vector3 lastVelocity;
        public float maxVelocity;
        public float totalDistanceTraveled;
        public bool hasExploded;
        public float explosionTime;
        public List<string> forceEvents = new List<string>();
        public PhysicsSettings physicsSettings;
    }
    
    [System.Serializable]
    public class PhysicsSettings
    {
        public float mass;
        public float drag;
        public float angularDrag;
        public bool useGravity;
        public bool isKinematic;
        public string physicsMaterial;
        public Vector3 centerOfMass;
    }
    
    void Start()
    {
        if (enableDiagnostics)
        {
            StartDiagnostics();
        }
    }
    
    void Update()
    {
        if (!isDiagnosticActive) return;
        
        // Update tracked shards
        UpdateShardTracking();
        
        // Check if diagnostic period is over
        if (Time.time - diagnosticStartTime > diagnosticDuration)
        {
            CompleteDiagnostics();
        }
    }
    
    public void StartDiagnostics()
    {
        if (isDiagnosticActive) return;
        
        isDiagnosticActive = true;
        diagnosticStartTime = Time.time;
        trackedShards.Clear();
        
        Debug.Log("=== EXPLOSION PHYSICS DIAGNOSTICS STARTED ===");
        
        // Find all explosion shards
        HelicopterExplosion explosion = FindObjectOfType<HelicopterExplosion>();
        if (explosion != null)
        {
            // Get all rigidbodies in the explosion
            Rigidbody[] shardRigidbodies = explosion.GetComponentsInChildren<Rigidbody>();
            
            int shardCount = 0;
            foreach (Rigidbody rb in shardRigidbodies)
            {
                if (rb.transform == explosion.transform) continue; // Skip parent
                if (shardCount >= maxShardsToTrack) break;
                
                ShardDiagnosticData data = new ShardDiagnosticData();
                data.name = rb.gameObject.name;
                data.rigidbody = rb;
                data.initialPosition = rb.transform.position;
                data.lastPosition = rb.transform.position;
                data.lastVelocity = Vector3.zero;
                data.maxVelocity = 0f;
                data.totalDistanceTraveled = 0f;
                data.hasExploded = false;
                data.explosionTime = 0f;
                data.physicsSettings = CapturePhysicsSettings(rb);
                
                trackedShards.Add(data);
                shardCount++;
                
                if (logPhysicsSettings)
                {
                    LogPhysicsSettings(data);
                }
            }
            
            Debug.Log($"DIAGNOSTIC: Tracking {trackedShards.Count} shards out of {shardRigidbodies.Length} total");
        }
        else
        {
            Debug.LogError("DIAGNOSTIC: No HelicopterExplosion found!");
        }
    }
    
    private void UpdateShardTracking()
    {
        foreach (ShardDiagnosticData data in trackedShards)
        {
            if (data.rigidbody == null) continue;
            
            Vector3 currentPosition = data.rigidbody.transform.position;
            Vector3 currentVelocity = data.rigidbody.velocity;
            
            // Calculate distance traveled
            float distanceThisFrame = Vector3.Distance(data.lastPosition, currentPosition);
            data.totalDistanceTraveled += distanceThisFrame;
            
            // Track max velocity
            float currentSpeed = currentVelocity.magnitude;
            if (currentSpeed > data.maxVelocity)
            {
                data.maxVelocity = currentSpeed;
            }
            
            // Detect explosion (sudden velocity increase)
            if (!data.hasExploded && currentSpeed > 5f && data.lastVelocity.magnitude < 1f)
            {
                data.hasExploded = true;
                data.explosionTime = Time.time - diagnosticStartTime;
                
                if (logForceApplications)
                {
                    Debug.Log($"EXPLOSION DETECTED: {data.name} - Velocity jumped from {data.lastVelocity.magnitude:F1} to {currentSpeed:F1} at time {data.explosionTime:F2}s");
                }
            }
            
            // Log significant velocity changes
            if (logVelocityChanges && Vector3.Distance(currentVelocity, data.lastVelocity) > 2f)
            {
                Debug.Log($"VELOCITY CHANGE: {data.name} - From {data.lastVelocity.magnitude:F1} to {currentSpeed:F1} (Change: {(currentSpeed - data.lastVelocity.magnitude):F1})");
            }
            
            // Update tracking data
            data.lastPosition = currentPosition;
            data.lastVelocity = currentVelocity;
        }
    }
    
    private PhysicsSettings CapturePhysicsSettings(Rigidbody rb)
    {
        PhysicsSettings settings = new PhysicsSettings();
        settings.mass = rb.mass;
        settings.drag = rb.drag;
        settings.angularDrag = rb.angularDrag;
        settings.useGravity = rb.useGravity;
        settings.isKinematic = rb.isKinematic;
        settings.centerOfMass = rb.centerOfMass;
        
        // Get physics material
        Collider collider = rb.GetComponent<Collider>();
        if (collider != null && collider.material != null)
        {
            settings.physicsMaterial = collider.material.name;
        }
        else
        {
            settings.physicsMaterial = "None";
        }
        
        return settings;
    }
    
    private void LogPhysicsSettings(ShardDiagnosticData data)
    {
        PhysicsSettings s = data.physicsSettings;
        Debug.Log($"PHYSICS SETTINGS: {data.name}" +
            $"\n  Mass: {s.mass}" +
            $"\n  Drag: {s.drag}" +
            $"\n  Angular Drag: {s.angularDrag}" +
            $"\n  Use Gravity: {s.useGravity}" +
            $"\n  Is Kinematic: {s.isKinematic}" +
            $"\n  Physics Material: {s.physicsMaterial}" +
            $"\n  Center of Mass: {s.centerOfMass}");
    }
    
    private void CompleteDiagnostics()
    {
        isDiagnosticActive = false;
        
        Debug.Log("=== EXPLOSION PHYSICS DIAGNOSTICS COMPLETE ===");
        
        foreach (ShardDiagnosticData data in trackedShards)
        {
            if (data.rigidbody == null) continue;
            
            Debug.Log($"FINAL STATS: {data.name}" +
                $"\n  Max Velocity: {data.maxVelocity:F1}" +
                $"\n  Total Distance: {data.totalDistanceTraveled:F1}" +
                $"\n  Exploded: {data.hasExploded}" +
                $"\n  Explosion Time: {data.explosionTime:F2}s" +
                $"\n  Final Velocity: {data.lastVelocity.magnitude:F1}" +
                $"\n  Final Position: {data.lastPosition}");
        }
        
        // Identify the problem
        AnalyzeDiagnosticResults();
    }
    
    private void AnalyzeDiagnosticResults()
    {
        Debug.Log("=== DIAGNOSTIC ANALYSIS ===");
        
        int shardsWithHighVelocity = 0;
        int shardsWithNoMovement = 0;
        int shardsWithAbnormalPhysics = 0;
        
        foreach (ShardDiagnosticData data in trackedShards)
        {
            if (data.maxVelocity > 20f)
            {
                shardsWithHighVelocity++;
                Debug.LogWarning($"HIGH VELOCITY DETECTED: {data.name} reached {data.maxVelocity:F1} units/sec");
            }
            
            if (data.totalDistanceTraveled < 1f)
            {
                shardsWithNoMovement++;
                Debug.LogWarning($"NO MOVEMENT DETECTED: {data.name} only moved {data.totalDistanceTraveled:F1} units");
            }
            
            PhysicsSettings s = data.physicsSettings;
            if (s.drag > 5f || !s.useGravity || s.mass > 10f)
            {
                shardsWithAbnormalPhysics++;
                Debug.LogWarning($"ABNORMAL PHYSICS: {data.name} - Drag:{s.drag}, Gravity:{s.useGravity}, Mass:{s.mass}");
            }
        }
        
        // Provide recommendations
        if (shardsWithHighVelocity > 0)
        {
            Debug.LogError($"PROBLEM IDENTIFIED: {shardsWithHighVelocity} shards had excessive velocity (>20 units/sec). This suggests explosion forces are too high or there's a force multiplication bug.");
        }
        
        if (shardsWithNoMovement > 0)
        {
            Debug.LogError($"PROBLEM IDENTIFIED: {shardsWithNoMovement} shards barely moved. This suggests forces aren't being applied or physics is disabled.");
        }
        
        if (shardsWithAbnormalPhysics > 0)
        {
            Debug.LogError($"PROBLEM IDENTIFIED: {shardsWithAbnormalPhysics} shards have abnormal physics settings that could cause floating or excessive forces.");
        }
    }
    
    void OnGUI()
    {
        if (!showRealTimeStats || !isDiagnosticActive) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== EXPLOSION DIAGNOSTICS ===", GUI.skin.box);
        
        GUILayout.Label($"Time: {Time.time - diagnosticStartTime:F1}s / {diagnosticDuration:F1}s");
        GUILayout.Label($"Tracking: {trackedShards.Count} shards");
        
        GUILayout.Space(10);
        
        foreach (ShardDiagnosticData data in trackedShards)
        {
            if (data.rigidbody == null) continue;
            
            float currentSpeed = data.rigidbody.velocity.magnitude;
            Color originalColor = GUI.color;
            
            if (currentSpeed > 20f)
                GUI.color = Color.red;
            else if (currentSpeed > 10f)
                GUI.color = Color.yellow;
            else
                GUI.color = Color.green;
            
            GUILayout.Label($"{data.name}: Speed {currentSpeed:F1} (Max: {data.maxVelocity:F1})");
            GUI.color = originalColor;
        }
        
        GUILayout.EndArea();
    }
    
    [ContextMenu("Start Diagnostics")]
    public void StartDiagnosticsManual()
    {
        StartDiagnostics();
    }
    
    [ContextMenu("Stop Diagnostics")]
    public void StopDiagnostics()
    {
        if (isDiagnosticActive)
        {
            CompleteDiagnostics();
        }
    }
}
