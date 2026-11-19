
using UnityEngine;

public class FrustumCulling : MonoBehaviour
{
    [Header("Frustum Settings")]

    public float FOV = 60f;
    public float aspectRatio = 16f / 9f;
    public float renderDistance = 50f;
    public float nearDistance = 0.1f;

    [Header("Objects to Check")]

    private Camera targetCamera;

    private MeshFilter[] objects;
    private Renderer[] renderers;
    private Vector3[][] vertices;

    private bool[] originalRendererState;
    private bool[] previousVisibility;
    private bool[] hasChecked;

    void Start()
    {
        // Obtenemos la camara que tiene este script
        targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
        {
            Debug.LogError("No se encontro una camara en este objeto.");
            return;
        }

        // Configuramos la camara con nuestros datos
        targetCamera.fieldOfView = FOV;
        targetCamera.aspect = aspectRatio;
        targetCamera.nearClipPlane = nearDistance;
        targetCamera.farClipPlane = renderDistance;

        // Buscamos todos los objetos con MeshFilter
        objects = FindObjectsByType<MeshFilter>(
            FindObjectsSortMode.None
        );

        // Reservamos memoria para los datos de cada objeto
        renderers = new Renderer[objects.Length];
        vertices = new Vector3[objects.Length][];

        originalRendererState = new bool[objects.Length];
        previousVisibility = new bool[objects.Length];
        hasChecked = new bool[objects.Length];

        // Guardamos los vertices y renderers
        for (int i = 0; i < objects.Length; i++)
        {
            renderers[i] = objects[i].GetComponent<Renderer>();

            vertices[i] = objects[i].sharedMesh.vertices;

            if (renderers[i] != null)
            {
                originalRendererState[i] = renderers[i].enabled;
            }
        }

        Debug.Log("Frustum Culling iniciado correctamente.");
    }

    void Update()
    {
        if (targetCamera == null)
            return;

        // Calculamos la mitad del angulo del FOV
        float halfFOV = FOV * 0.5f;

        // Convertimos el angulo a radianes
        float radians = halfFOV * Mathf.Deg2Rad;

        // Calculamos la tangente
        float tangent = Mathf.Tan(radians);

        // Obtenemos los vectores de orientacion de la camara
        Vector3 cameraPosition = targetCamera.transform.position;

        Vector3 cameraForward = targetCamera.transform.forward;
        Vector3 cameraRight = targetCamera.transform.right;
        Vector3 cameraUp = targetCamera.transform.up;

        // Revisamos cada objeto de la escena
        for (int i = 0; i < objects.Length; i++)
        {
            bool isVisible = false;

            // Revisamos todos los vertices del objeto
            for (int j = 0; j < vertices[i].Length; j++)
            {
                // Pasamos el vertice de coordenadas locales
                // a coordenadas del mundo
                Vector3 worldVertex = objects[i].transform.TransformPoint(
                    vertices[i][j]
                );

                // Calculamos la distancia y posicion relativa
                // del vertice respecto a la camara
                Vector3 direction = worldVertex - cameraPosition;

                // Obtenemos las coordenadas respecto a la camara
                float z = Vector3.Dot(direction, cameraForward);
                float x = Vector3.Dot(direction, cameraRight);
                float y = Vector3.Dot(direction, cameraUp);

                // Si esta demasiado cerca o demasiado lejos,
                // no puede estar dentro del frustum
                if (z < nearDistance || z > renderDistance)
                    continue;

                // Calculamos la altura y anchura del frustum
                // en la profundidad donde se encuentra el vertice
                float halfHeight = z * tangent;
                float halfWidth = halfHeight * aspectRatio;

                // Comprobamos si el vertice esta dentro
                // de los limites horizontales y verticales
                if (Mathf.Abs(x) <= halfWidth &&
                    Mathf.Abs(y) <= halfHeight)
                {
                    isVisible = true;
                    break;
                }
            }

            // Activamos o desactivamos el renderer
            if (renderers[i] != null)
            {
                renderers[i].enabled =
                    originalRendererState[i] && isVisible;
            }

            // Notificamos cuando cambia la visibilidad
            if (!hasChecked[i] || previousVisibility[i] != isVisible)
            {
                if (isVisible)
                {
                    Debug.Log(
                        objects[i].gameObject.name +
                        " esta DENTRO del Frustum."
                    );
                }
                else
                {
                    Debug.Log(
                        objects[i].gameObject.name +
                        " esta FUERA del Frustum."
                    );
                }

                previousVisibility[i] = isVisible;
                hasChecked[i] = true;
            }
        }
    }
}
