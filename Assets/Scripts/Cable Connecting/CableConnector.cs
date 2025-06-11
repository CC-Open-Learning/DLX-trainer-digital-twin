using System;
using UnityEngine;
using VARLab.Interactions;

namespace VARLab.MPCircuits
{
    public class CableConnector : MonoBehaviour
    {

        public bool CanMoveLeads = true;

        public Transform endPoint;
        public Transform startPoint;
        public CapsuleCollider capsuleCollider;

        [SerializeField] private Material cableMaterial;
        [SerializeField] private int totalSegments = 5;
        [SerializeField] private float segmentsPerUnit = 2f;
        [SerializeField] private float cableWidth = 0.1f;


        public int segments = 0;
        private int verletIterations = 2;
        private int solverIterations = 2;
        private Quaternion InitialDeleteIconRotation;

        public LineRenderer line;
        public CablePhysics[] points;

        public CableLead cableStart;
        public CableLead cableEnd;

        public float cableLength = 0.5f;
        public Interactable deleteIcon;
        public CableControls.CableColors Color;

        public Guid CableID { get; set; } = Guid.NewGuid();

        public bool IsFaulty { get; set; } = false;


        private void Awake()
        {
            InitialDeleteIconRotation = deleteIcon.transform.rotation;
            deleteIcon.MouseClick.AddListener((_) => FindObjectOfType<CableControls>().OnDeleteIconClicked(this));
        }

        private void Start()
        {
            InitCableParticles();
            InitLineRenderer();
        }

        private void Update()
        {
            RenderCable();
        }

        private void FixedUpdate()
        {
            for (int verletIdx = 0; verletIdx < verletIterations; verletIdx++)
            {
                VerletIntegrate();
                SolveConstraints();
            }
        }

        public void EnableInteraction(bool enabled)
        {
            GetComponent<Interactable>().enabled = enabled;

            cableStart.GetComponent<Interactable>().enabled = enabled;
            cableEnd.GetComponent<Interactable>().enabled = enabled;

            capsuleCollider.GetComponent<Interactable>().enabled = enabled;
            capsuleCollider.enabled = enabled;


            cableStart.GetComponent<Collider>().enabled = enabled;
            cableEnd.GetComponent<Collider>().enabled = enabled;

            capsuleCollider.GetComponent<Collider>().enabled = enabled;

            CanMoveLeads = enabled;
        }

        public void EnableDeleteIcon(bool enabled)
        {
            if (enabled)
            {
                Vector3 iconPosition = line.bounds.center;

                deleteIcon.transform.position = iconPosition;
            }

            deleteIcon.gameObject.SetActive(enabled);
        }

        public void OnCableColliderHoveredOn()
        {
            FindObjectOfType<CableControls>().OnCableHovered(this);
        }

        public void OnCableColliderHoveredOff()
        {
            FindObjectOfType<CableControls>().OnCableHoveredOff(this);
        }

        public void CreateMesh()
        {
            // Place cable in the middle of the 2 leads
            Vector3 distanceBetween = (cableStart.transform.position - cableEnd.transform.position);
            Vector3 middlePosition = endPoint.position + (startPoint.position - endPoint.position) / 2;

            capsuleCollider.gameObject.transform.position = middlePosition;

            // Rotate the cable
            capsuleCollider.gameObject.transform.forward = distanceBetween;

            deleteIcon.gameObject.transform.rotation = InitialDeleteIconRotation;

            // Stretch the cable
            capsuleCollider.height = distanceBetween.magnitude * 0.9f;
        }

        public void Disconnect()
        {
            cableEnd.DisconnectPort();
            cableStart.DisconnectPort();
            Destroy(this.gameObject);
        }

        public void InitCableParticles()
        {
            // Calculate segments to use
            if (totalSegments > 0)
                segments = totalSegments;
            else
                segments = Mathf.CeilToInt(cableLength * segmentsPerUnit);

            Vector3 cableDirection = (endPoint.position - startPoint.transform.position).normalized;
            float initialSegmentLength = cableLength / segments;
            points = new CablePhysics[segments + 1];

            // Foreach point
            for (int pointIdx = 0; pointIdx <= segments; pointIdx++)
            {
                // Initial position
                Vector3 initialPosition = startPoint.transform.position + (cableDirection * (initialSegmentLength * pointIdx));

                points[pointIdx] = gameObject.AddComponent<CablePhysics>();
                points[pointIdx]._oldPosition = initialPosition;
                points[pointIdx]._position = initialPosition;
            }

            // Bind start and end particles with their respective gameobjects
            CablePhysics start = points[0];
            CablePhysics end = points[segments];
            start.Bind(startPoint.transform);
            end.Bind(endPoint.transform);
        }

        public void InitLineRenderer()
        {
            line = gameObject.GetComponent<LineRenderer>();
            line.startWidth = cableWidth;
            line.endWidth = cableWidth;
            line.positionCount = segments + 1;
            line.material = cableMaterial;
            line.GetComponent<Renderer>().enabled = true;
        }

        private void RenderCable()
        {
            for (int pointIdx = 0; pointIdx < segments + 1; pointIdx++)
            {
                line.SetPosition(pointIdx, points[pointIdx].Position);
            }
        }

        private void VerletIntegrate()
        {
            Vector3 gravityDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
            foreach (CablePhysics particle in points)
            {
                particle.UpdateVerlet(gravityDisplacement);
            }
        }

        private void SolveConstraints()
        {
            // For each solver iteration..
            for (int iterationIdx = 0; iterationIdx < solverIterations; iterationIdx++)
            {
                SolveDistanceConstraint();
            }
        }

        private void SolveDistanceConstraint()
        {
            float segmentLength = cableLength / segments;
            for (int SegIdx = 0; SegIdx < segments; SegIdx++)
            {
                CablePhysics particleA = points[SegIdx];
                CablePhysics particleB = points[SegIdx + 1];

                // Solve for this pair of particles
                SolveDistanceConstraint(particleA, particleB, segmentLength);
            }
        }

        private void SolveDistanceConstraint(CablePhysics particleA, CablePhysics particleB, float segmentLength)
        {
            // Find current vector between particles
            Vector3 delta = particleB.Position - particleA.Position;

            float currentDistance = delta.magnitude;
            float errorFactor = (currentDistance - segmentLength) / currentDistance;

            // Only move free particles to satisfy constraints
            if (particleA.IsFree() && particleB.IsFree())
            {
                particleA.Position += errorFactor * 0.5f * delta;
                particleB.Position -= errorFactor * 0.5f * delta;
            }
            else if (particleA.IsFree())
            {
                particleA.Position += errorFactor * delta;
            }
            else if (particleB.IsFree())
            {
                particleB.Position -= errorFactor * delta;
            }
        }
    }
}