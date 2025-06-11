using UnityEngine;
using VARLab.Interactions;

namespace VARLab.MPCircuits
{
    public class CableLead : MonoBehaviour
    {
        private Interactable interactable;

        private PortBehaviour connectedPort;
        public PortBehaviour ConnectedPort => connectedPort;

        private void Start()
        {
            interactable = GetComponent<Interactable>();

            //adds mouse listeners to this CableLead
            interactable.MouseClick.AddListener((_) => FindObjectOfType<CableControls>().OnLeadClicked(this));
            interactable.MouseEnter.AddListener((_) => FindObjectOfType<CableControls>().OnLeadHovered(this));
            interactable.MouseExit.AddListener((_) => FindObjectOfType<CableControls>().OnLeadHoveredOff(this));
        }

        /// <summary>
        /// Assigns this CableLead to the port that was clicked, and adds the lead to that ports connection list
        /// </summary>
        /// <param name="p"></param>
        public void ConnectPort(PortBehaviour p)
        {
            connectedPort = p;
            p.ConnectLead(this);
        }

        /// <summary>
        /// resets the connectedPort for this CableLead and removes the lead from that ports connection list
        /// </summary>
        public void DisconnectPort()
        {
            if (connectedPort == null)
                return;

            connectedPort.DisconnectLead(this);
            connectedPort = null;
        }

        public void EnableOutline(bool enabled)
        {
            this.gameObject.ShowOutline(enabled);
        }

        public void EnableSelection(bool enabled)
        {
            EnableOutline(enabled);
            this.GetComponent<Collider>().enabled = enabled;
        }

        /// <summary>
        /// Returns the other end of the cable lead passed in
        /// </summary>
        /// <returns></returns>
        public CableLead GetOtherLead()
        {
            CableConnector parent = this.Parent();

            return this == parent.cableStart ? parent.cableEnd : parent.cableStart;
        }

        /// <summary>
        /// Returns the CableConnector that this CableLead is a child of
        /// </summary>
        /// <returns></returns>
        public CableConnector Parent()
        {
            return this.transform.parent.gameObject.GetComponent<CableConnector>();
        }
    }
}
