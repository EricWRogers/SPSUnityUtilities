using UnityEngine;

public class CollisionTester : MonoBehaviour
{
    [Range(0.1f, 5.0f)]
    public float sphereRadius = 1.0f;
    public LayerMask mask;
    private Color m_hitColor = Color.red;
    private Color m_missColor = Color.green;
    private Renderer m_renderer;
    void Start()
    {
        m_renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(sphereRadius, sphereRadius, sphereRadius);
        Collider[] collisions = new Collider[100];
        if (Physics.OverlapSphereNonAlloc(transform.position, sphereRadius*0.5f, collisions, mask, QueryTriggerInteraction.Collide) > 0)
            m_renderer.material.color = m_hitColor;
        else
            m_renderer.material.color = m_missColor;
    }
}
