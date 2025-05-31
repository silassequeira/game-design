using UnityEngine;

// This tool helps create particle systems at runtime or design time
public class ParticleSystemCreator : MonoBehaviour
{
    // Create this GameObject in your scene to easily configure particle effects
    // or call the static methods to create particle systems at runtime

    [Header("Jump Particles")]
    public Color jumpColor = new Color(0.5f, 0.8f, 1.0f, 0.8f);
    public float jumpParticleSize = 0.3f;
    public int jumpParticleCount = 15;
    public float jumpParticleSpeed = 3f;
    public float jumpParticleLifetime = 0.7f;

    [Header("Double Jump Particles")]
    public Color doubleJumpColor = new Color(0.3f, 1.0f, 0.8f, 0.8f);
    public float doubleJumpParticleSize = 0.4f;
    public int doubleJumpParticleCount = 25;
    public float doubleJumpParticleSpeed = 4f;
    public float doubleJumpParticleLifetime = 1.0f;

    [Header("Land Particles")]
    public Color landColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
    public float landParticleSize = 0.25f;
    public int landParticleCount = 10;
    public float landParticleSpeed = 2f;
    public float landParticleLifetime = 0.8f;

    [Header("Speed Particles")]
    public Color speedColor = new Color(1.0f, 0.5f, 0.2f, 0.6f);
    public float speedParticleSize = 0.2f;
    public int speedEmissionRate = 15;
    public float speedParticleLifetime = 0.5f;

    [Header("Special Effects")]
    public Color specialEffectColor = new Color(0f, 1f, 1f, 0.8f);
    public float specialParticleSize = 0.5f;
    public int specialParticleCount = 40;
    public float specialParticleSpeed = 5f;
    public float specialParticleLifetime = 1.5f;

    [Header("Creation")]
    public bool createOnStart = false;
    public Transform playerTransform;
    public bool attachToPlayer = true;

    private ParticleSystem jumpParticleSystem;
    private ParticleSystem doubleJumpParticleSystem;
    private ParticleSystem landParticleSystem;
    private ParticleSystem speedParticleSystem;
    private ParticleSystem specialParticleSystem;

    void Start()
    {
        if (createOnStart && playerTransform != null)
        {
            CreateAllParticleSystems();
        }
    }

    [ContextMenu("Create All Particle Systems")]
    public void CreateAllParticleSystems()
    {
        jumpParticleSystem = CreateJumpParticleSystem();
        doubleJumpParticleSystem = CreateDoubleJumpParticleSystem();
        landParticleSystem = CreateLandParticleSystem();
        speedParticleSystem = CreateSpeedParticleSystem();
        specialParticleSystem = CreateSpecialEffectParticleSystem();

        if (attachToPlayer && playerTransform != null)
        {
            jumpParticleSystem.transform.SetParent(playerTransform);
            doubleJumpParticleSystem.transform.SetParent(playerTransform);
            landParticleSystem.transform.SetParent(playerTransform);
            speedParticleSystem.transform.SetParent(playerTransform);
            specialParticleSystem.transform.SetParent(playerTransform);

            jumpParticleSystem.transform.localPosition = Vector3.zero;
            doubleJumpParticleSystem.transform.localPosition = Vector3.zero;
            landParticleSystem.transform.localPosition = new Vector3(0, -0.5f, 0);
            speedParticleSystem.transform.localPosition = new Vector3(0, -0.3f, 0);
            specialParticleSystem.transform.localPosition = Vector3.zero;
        }
    }

    public ParticleSystem CreateJumpParticleSystem()
    {
        GameObject particleObj = new GameObject("JumpParticles");
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.startColor = jumpColor;
        main.startSize = jumpParticleSize;
        main.startSpeed = jumpParticleSpeed;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = jumpParticleLifetime;

        // Emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, (short)jumpParticleCount)
        });

        // Shape module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        shape.radiusThickness = 1f;
        shape.arc = 180f; // Half circle
        shape.rotation = new Vector3(0, 0, -90); // Point downward

        // Add a basic renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 1;

        return ps;
    }

    public ParticleSystem CreateDoubleJumpParticleSystem()
    {
        GameObject particleObj = new GameObject("DoubleJumpParticles");
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.startColor = doubleJumpColor;
        main.startSize = doubleJumpParticleSize;
        main.startSpeed = doubleJumpParticleSpeed;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = doubleJumpParticleLifetime;

        // Emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, (short)doubleJumpParticleCount)
        });

        // Shape module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
        shape.arc = 360f; // Full circle

        // Add a trail module for fancier particles
        var trails = ps.trails;
        trails.enabled = true;
        trails.ratio = 0.5f;
        trails.lifetime = 0.2f;

        // Add a basic renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 2;

        return ps;
    }

    public ParticleSystem CreateLandParticleSystem()
    {
        GameObject particleObj = new GameObject("LandParticles");
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.startColor = landColor;
        main.startSize = landParticleSize;
        main.startSpeed = landParticleSpeed;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = landParticleLifetime;

        // Emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, (short)landParticleCount)
        });

        // Shape module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(1f, 0.1f, 1f);

        // Add a basic renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 0;

        return ps;
    }

    public ParticleSystem CreateSpeedParticleSystem()
    {
        GameObject particleObj = new GameObject("SpeedParticles");
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.startColor = speedColor;
        main.startSize = speedParticleSize;
        main.startSpeed = 0f; // Will use velocity over lifetime
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = speedParticleLifetime;

        // Emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = speedEmissionRate;

        // Shape module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(0.1f, 0.5f, 1f);

        // Velocity over lifetime module
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = -2f; // Always trail behind player

        // Add a basic renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = -1;

        return ps;
    }

    public ParticleSystem CreateSpecialEffectParticleSystem()
    {
        GameObject particleObj = new GameObject("SpecialParticles");
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.startColor = specialEffectColor;
        main.startSize = specialParticleSize;
        main.startSpeed = specialParticleSpeed;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.duration = 1.0f;
        main.loop = false;
        main.startLifetime = specialParticleLifetime;

        // Emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, (short)specialParticleCount)
        });

        // Shape module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // Add a trail module for fancy particles
        var trails = ps.trails;
        trails.enabled = true;
        trails.ratio = 0.7f;
        trails.lifetime = 0.3f;

        // Add a color over lifetime module
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        // Create a gradient for color transition
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(specialEffectColor, 0.0f),
                new GradientColorKey(Color.white, 0.5f),
                new GradientColorKey(specialEffectColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;

        // Add a basic renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 3;

        return ps;
    }

    private Material CreateParticleMaterial()
    {
        // Create a simple additive particle material
        Material material = new Material(Shader.Find("Particles/Standard Unlit"));
        material.SetFloat("_Glossiness", 0.5f);
        material.SetFloat("_Mode", 2); // Fade mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        return material;
    }

    // Static methods for easy access
    public static ParticleSystem CreateJumpEffect(Vector3 position)
    {
        ParticleSystemCreator creator = new GameObject("TempCreator").AddComponent<ParticleSystemCreator>();
        ParticleSystem ps = creator.CreateJumpParticleSystem();
        ps.transform.position = position;
        Destroy(creator.gameObject);
        return ps;
    }

    public static ParticleSystem CreateDoubleJumpEffect(Vector3 position)
    {
        ParticleSystemCreator creator = new GameObject("TempCreator").AddComponent<ParticleSystemCreator>();
        ParticleSystem ps = creator.CreateDoubleJumpParticleSystem();
        ps.transform.position = position;
        Destroy(creator.gameObject);
        return ps;
    }
}

