using System.Collections.Generic;
using UnityEngine;

namespace Tanks.Complete
{
    public class CameraControl : MonoBehaviour
    {
        public float m_DampTime = 0.2f;
        public float m_ScreenEdgeBuffer = 4f;
        public float m_MinFOV = 40f; // Zoomed in (Solo)
        public float m_MaxFOV = 75f; // Zoomed out (Brawl)
        public float m_ZoomLimit = 30f; // Distance spread for Max Zoom

        [HideInInspector] public List<Transform> m_Targets = new List<Transform>();

        private Camera m_Camera;
        private float m_ZoomSpeed;
        private Vector3 m_MoveVelocity;
        private Vector3 m_DesiredPosition;

        public static CameraControl instance;

        private void Awake()
        {
            instance = this;
            m_Camera = GetComponentInChildren<Camera>();
        }

        // Helper methods for your Player Prefab script
        public void AddTarget(Transform target) { if (!m_Targets.Contains(target)) m_Targets.Add(target); }
        public void RemoveTarget(Transform target) { m_Targets.Remove(target); }

        private void FixedUpdate()
        {
            m_Targets.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy);

            if (m_Targets.Count > 0)
            {
                Move();
                Zoom();
            }
        }

        private void Move()
        {
            FindAveragePosition();
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }

        private void FindAveragePosition()
        {
            Vector3 averagePos = new Vector3();
            for (int i = 0; i < m_Targets.Count; i++)
            {
                averagePos += m_Targets[i].position;
            }
            averagePos /= m_Targets.Count;
            averagePos.y = transform.position.y; // Keep current Rig height
            m_DesiredPosition = averagePos;
        }

        private void Zoom()
        {
            float greatestDistance = GetGreatestDistance();

            // This maps the distance between players to the FOV range
            float targetFOV = Mathf.Lerp(m_MinFOV, m_MaxFOV, greatestDistance / m_ZoomLimit);

            // Smoothly move the FOV
            m_Camera.fieldOfView = Mathf.SmoothDamp(m_Camera.fieldOfView, targetFOV, ref m_ZoomSpeed, m_DampTime);
        }

        private float GetGreatestDistance()
        {
            var bounds = new Bounds(m_Targets[0].position, Vector3.zero);
            for (int i = 0; i < m_Targets.Count; i++)
            {
                bounds.Encapsulate(m_Targets[i].position);
            }
            return Mathf.Max(bounds.size.x, bounds.size.z);
        }
    }
}