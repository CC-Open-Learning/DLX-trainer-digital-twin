using EPOOutline;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Interactions;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Outlinable))]
    public class PortBehaviour : MonoBehaviour
    {
        public CircuitBoard.PortNames PortName;

        public ToolTip toolTip;

        public int NumberLeadsConnected { get { return connectedLeads.Count; } }

        private List<CableLead> connectedLeads;

        private Interactable interactable;

        public UnityEvent<PortBehaviour> OnPortClicked;
        public UnityEvent<PortBehaviour> OnPortHovered;
        public UnityEvent<PortBehaviour> OnPortHoveredOff;

        public Action OnCircuitImportStart;

        private Port port;

        private bool interactionEnabled = true;

        public double Voltage;

        public bool InteractionEnabled { get => interactionEnabled; }

        private void Awake()
        {
            connectedLeads = new List<CableLead>();
            interactable = GetComponent<Interactable>();
        }

        private void Start()
        {
            gameObject.ShowOutline(false);

            // Added as a guard clause for integration testing
#pragma warning disable UNT0008 // Null propagation on Unity objects
            port = FindFirstObjectByType<DigitalTwinManager>()?.CircuitBoard.GetPort(PortName);
#pragma warning restore UNT0008 // Null propagation on Unity objects

            if (port != null)
            {
                port.VoltageChanged += OnPortVoltageChanged;
            }
            OnCircuitImportStart?.Invoke();
        }

        private void OnEnable()
        {
            interactable.MouseClick.AddListener((_) => { if (interactionEnabled) { OnPortClicked?.Invoke(this); } });
            interactable.MouseEnter.AddListener((_) => { if (interactionEnabled) { OnPortHovered?.Invoke(this); } });
            interactable.MouseExit.AddListener((_) => OnPortHoveredOff?.Invoke(this));
        }

        private void OnDisable()
        {
            if (port == null) { return; }

            port.VoltageChanged -= OnPortVoltageChanged;
        }

        private void OnMouseEnter()
        {
            if (port == null) { return; }

            CheckForToolTip(true);
        }

        private void OnMouseExit()
        {
            if (port == null) { return; }

            CheckForToolTip(false);
        }

        private void CheckForToolTip(bool enabled)
        {
            if (toolTip == null) return;

            toolTip.MinimizeAndEnlargeTargetSprites(enabled);
        }

        private void OnPortVoltageChanged(double voltage)
        {
            Voltage = voltage;
        }

        public void EnableInteraction(bool enabled)
        {
            interactionEnabled = enabled;
        }

        public List<CableLead> GetAllLeadsAbove(CableLead l)
        {
            int index = connectedLeads.FindIndex(a => a == l);

            List<CableLead> leadsAbove = new();

            for (int i = index + 1; i < connectedLeads.Count; i++)
            {
                leadsAbove.Add(connectedLeads[i]);
            }

            return leadsAbove;
        }

        public void ConnectLead(CableLead lead)
        {
            connectedLeads.Add(lead);
        }

        public void DisconnectLead(CableLead lead)
        {
            connectedLeads.Remove(lead);
        }

        //returns all of the connected leads to this port in an array
        public CableLead[] GetAllConnectedLeads()
        {
            return connectedLeads.ToArray();
        }

        // returns the cable lead at the selected index within the connectedLeads array
        public CableLead GetConnectedLeadAtIndex(int index)
        {
            CableLead[] connectedLeadsArray = GetAllConnectedLeads();

            for (int i = 0; i < connectedLeadsArray.Length; i++)
            {
                if (i == index)
                {
                    return connectedLeadsArray[i];
                }
            }

            return null;
        }
    }
}
