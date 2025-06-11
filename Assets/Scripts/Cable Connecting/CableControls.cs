using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.MPCircuits
{
    public class CableControls : MonoBehaviour
    {
        [SerializeField] public GameObject redCablePrefab, blackCablePrefab;
        [SerializeField] public GameObject redCableBundle, blackCableBundle;
        [SerializeField] public GameObject transparentRedCableLeadPrefab, transparentBlackCableLeadPrefab;

        [SerializeField] public InteractionManager interactionManager;

        private HashSet<CableConnector> connectedCables;

        private GameObject currentlySelectedTransparentCableLead;

        public GameObject CurrentlySelectedTransparentCableLead
        {
            get => currentlySelectedTransparentCableLead;
            set => currentlySelectedTransparentCableLead = value;
        }

        private Vector3 selectedCableLeadOffset = new Vector3(0, 0.08f, 0);
        private const float leadMinY = 1.4f, leadMaxY = 1.75f;
        private Vector3 portCableLeadOffset = new Vector3(-0.018f, 0.0389f, -0.0068f);
        private Vector3 portCableLeadHeightOffset = new Vector3(0, 0.1f, 0);

        private PortBehaviour currentHoveredPort;

        private CableColors currentlySelectedColor = CableColors.NotSelected;

        private int maxCableNum = 0;

        private int currentlyCreatedCables = 0;

        [HideInInspector] public CableConnector currentlySelectedCable;
        [HideInInspector] public CableLead currentlySelectedLead;

        public bool IsCableColorSelected { get { return currentlySelectedColor != CableColors.NotSelected; } }

        public bool IsCableSelected { get { return currentlySelectedCable != null; } }

        public bool CanPlaceCables { get { return currentlyCreatedCables < maxCableNum; } }

        public int currentCablesPlaced { get; private set; }

        public UnityEvent<CableConnector> OnCablePlaced;
        public UnityEvent<CableConnector> OnCableDeleted;
        public UnityEvent<CableConnector> OnCableDisconnected;

        public enum CableColors
        {
            Red = 0,
            Black = 1,
            NotSelected = -1
        }

        private const int PrimaryMouseButton = 0;
        private const int SecondaryMouseButton = 1;

        private GameObject transparentRedCableLead, transparentBlackCableLead;

        private void Awake()
        {
            Cursor.visible = true;
            connectedCables = new HashSet<CableConnector>();
            InitializeTransparentCableLeads();
        }

        private void Update()
        {
            UpdateSelectedLeadPosition();
            UpdateTransparentLeadPosition();
            RotateOtherLead();
            DeleteCableOnMouseClick();
        }

        private void InitializeTransparentCableLeads()
        {
            transparentRedCableLead = Instantiate(transparentRedCableLeadPrefab);
            transparentRedCableLead.SetActive(false);

            transparentBlackCableLead = Instantiate(transparentBlackCableLeadPrefab);
            transparentBlackCableLead.SetActive(false);
        }

        public void OnPortHoverOff(PortBehaviour hoveredPort)
        {
            hoveredPort.gameObject.ShowOutline(false);
            currentHoveredPort = null;

            // Outline will always be disabled when hovered off
            SetCableOutlineState(hoveredPort, false);
        }

        public void OnPortHover(PortBehaviour hoveredPort)
        {
            currentHoveredPort = hoveredPort;

            if (currentlySelectedColor == CableColors.NotSelected)
                return;

            foreach (PortBehaviour port in FindObjectsOfType<PortBehaviour>())
            {
                port.gameObject.ShowOutline(false);
            }

            hoveredPort.gameObject.ShowOutline(true, interactionManager.InteractableHoverColor);
            SetCableOutlineState(hoveredPort);
        }

        public void OnPortClick(PortBehaviour port)
        {
            // Stopping the user from placing leads into ports when they right click without a lead and Deleting the cable when they right click
            // while having a lead selected
            if (Input.GetMouseButtonUp(SecondaryMouseButton))
            {
                if (currentlySelectedLead != null)
                {
                    UpdateCableCursor();
                    OnDeleteIconClicked(currentlySelectedCable, true);
                }
                return;
            }

            // Logic for repositioning by clicking port
            if (currentlySelectedColor == CableColors.NotSelected)
            {
                for (int i = 0; i < GetPortConnectionsArray(port).Length; i++)
                {
                    CableLead lead = GetPortConnectionAtIndex(port, i);

                    // Skips any inactive leads
                    if (!lead.GetComponent<Collider>().enabled) continue;

                    // Both return just in case an edge case could escape and try to create or
                    // add a duplicate lead to the connected ports list
                    if (currentlySelectedCable == null)
                    {
                        // Reposition
                        DisconnectAndSelectLead(lead);
                        return;
                    }
                    else
                    {
                        // Finish placement
                        FinishCableConnection(port);
                        return;
                    }
                }
            }

            if (currentlySelectedCable == null)
            {
                CreateAndSelectNewCable(port);
            }
            else
            {
                FinishCableConnection(port);
            }
        }

        public void OnLeadClicked(CableLead lead)
        {
            // Can't click on the other lead of our current cable, or when clicking with right click
            if (currentlySelectedCable != null && currentlySelectedLead == lead.GetOtherLead() || Input.GetMouseButtonUp(SecondaryMouseButton))
                return;

            if (currentlySelectedLead == null && lead.Parent().CanMoveLeads && currentlySelectedColor == CableColors.NotSelected)
            {
                DisconnectAndSelectLead(lead);
            }
            else
            {
                OnPortClick(lead.ConnectedPort);
            }
        }

        private void DisconnectAndSelectLead(CableLead lead)
        {
            OnCableDisconnected?.Invoke(lead.Parent());

            currentlyCreatedCables--;
            Cursor.visible = false;

            currentlySelectedCable = lead.Parent();
            currentlySelectedCable.EnableDeleteIcon(false);

            currentlySelectedCable.gameObject.ShowOutline(true);

            currentlySelectedCable.cableStart.gameObject.ShowOutline(true);
            currentlySelectedCable.cableEnd.gameObject.ShowOutline(true);

            currentlySelectedLead = lead;

            LowerAllLeadsAboveByOneCableHeight(lead);

            lead.DisconnectPort();
            lead.EnableSelection(false);
        }

        public void OnDeleteIconClicked(CableConnector cable, bool disconnectedViaClickingNothing = false)
        {
            List<CableLead> allLeadsAboveStart = (cable.cableStart.ConnectedPort == null) ?
                null : cable.cableStart.ConnectedPort.GetAllLeadsAbove(cable.cableStart);

            List<CableLead> allLeadsAboveEnd = (cable.cableEnd.ConnectedPort == null) ?
                null : cable.cableEnd.ConnectedPort.GetAllLeadsAbove(cable.cableEnd);

            if (!disconnectedViaClickingNothing)
            {
                currentlyCreatedCables--;
                UpdateCableCursor();
            }

            OnCableDeleted?.Invoke(cable);
            OnCableDisconnected?.Invoke(cable);

            connectedCables.Remove(cable);
            cable.Disconnect();

            if (allLeadsAboveStart != null)
            {
                foreach (var l in allLeadsAboveStart)
                {
                    l.transform.position -= portCableLeadHeightOffset;
                }
            }

            if (allLeadsAboveEnd != null)
            {
                foreach (var l in allLeadsAboveEnd)
                {
                    l.transform.position -= portCableLeadHeightOffset;
                }
            }
        }

        private void LowerAllLeadsAboveByOneCableHeight(CableLead lead)
        {
            if (lead.ConnectedPort == null)
                return;

            foreach (var l in lead.ConnectedPort.GetAllLeadsAbove(lead))
            {
                l.transform.position -= portCableLeadHeightOffset;
            }
        }

        public void OnCableHovered(CableConnector cable)
        {
            if (currentlySelectedCable != null)
                return;

            cable.gameObject.ShowOutline(Cursor.visible, interactionManager.InteractableHoverColor);
            cable.EnableDeleteIcon(Cursor.visible);
        }

        public void OnCableHoveredOff(CableConnector cable)
        {
            if (currentlySelectedCable != null) return;

            cable.gameObject.ShowOutline(false);
            cable.EnableDeleteIcon(false);
        }

        public void OnCableBunchHovered(bool isGround)
        {
            //hover colour
            GameObject hoveredCableBunch = isGround ? blackCableBundle : redCableBundle;
            hoveredCableBunch.ShowOutline(true, interactionManager.InteractableHoverColor);

            //if hovering over a selected cable bunch, keep the selected color
            if (currentlySelectedColor == CableColors.Red)
            {
                redCableBundle.ShowOutline(true);
            }
            if (currentlySelectedColor == CableColors.Black)
            {
                blackCableBundle.ShowOutline(true);
            }
        }

        public void OnCableBunchHoveredOff()
        {
            redCableBundle.ShowOutline(false);
            blackCableBundle.ShowOutline(false);

            //if a cable bunch is selected, leave the outline shown/on
            if (currentlySelectedColor == CableColors.Red)
            {
                redCableBundle.ShowOutline(true);
            }
            if (currentlySelectedColor == CableColors.Black)
            {
                blackCableBundle.ShowOutline(true);
            }
        }

        public void OnCableBundleClicked(bool isGround)
        {
            // Only allow cable bundles to be clicked when the current cables are less then the max cables, and there currently isn't a selected lead
            if (currentlyCreatedCables >= maxCableNum || currentlySelectedLead != null)
            {
                return;
            }

            var newColor = isGround ? CableColors.Black : CableColors.Red;

            currentlySelectedColor = currentlySelectedColor == newColor ? CableColors.NotSelected : newColor;

            EnableTransparentLeadByType();

            EnableCableBundleOutlineByType(redCableBundle, currentlySelectedColor == CableColors.Red);
            EnableCableBundleOutlineByType(blackCableBundle, currentlySelectedColor == CableColors.Black);
        }

        /// <summary>
        /// Enables the transparent lead associated with the boolean passed in
        /// (isGround being true if black, false if red)
        /// </summary>
        public void EnableTransparentLeadByType()
        {
            transparentRedCableLead.SetActive(currentlySelectedColor == CableColors.Red);
            transparentBlackCableLead.SetActive(currentlySelectedColor == CableColors.Black);

            currentlySelectedTransparentCableLead =
                currentlySelectedColor == CableColors.Black ?
                transparentBlackCableLead : currentlySelectedColor == CableColors.Red ?
                transparentRedCableLead : null;

            Cursor.visible = currentlySelectedTransparentCableLead == null;
        }

        /// <summary>
        /// Decoupled this functionality from OnCableBundleClicked to isolate the outlining
        /// into the SetCableBundleColor and EnableSelectedCableBundleOutline functions
        /// </summary>
        public void EnableCableBundleOutlineByType(GameObject cableBundle, bool enabled = false)
        {
            cableBundle.ShowOutline(enabled);
        }

        /// <summary>
        /// Resets the current cable color to be not selected 
        /// (this is used to reset active lead color between labs)
        /// </summary>
        public void ResetCableBundleState()
        {
            if (currentlySelectedColor == CableColors.NotSelected) return;

            currentlySelectedColor = CableColors.NotSelected;

            EnableCableBundleOutlineByType(blackCableBundle, false);
            EnableCableBundleOutlineByType(redCableBundle, false);
        }

        /// <summary>
        /// Made this function so that the objective panel can reenable the previously 
        /// selected cable color outline without requiring access to the bundle gameobject.
        /// </summary>
        public void EnableSelectedCableBundleOutline(bool enabled = false)
        {
            if (currentlySelectedColor == CableColors.NotSelected) return;

            if (currentlySelectedColor == CableColors.Black)
            {
                EnableCableBundleOutlineByType(blackCableBundle, enabled);
                EnableCableBundleOutlineByType(redCableBundle, !enabled);
            }
            else
            {
                EnableCableBundleOutlineByType(blackCableBundle, !enabled);
                EnableCableBundleOutlineByType(redCableBundle, enabled);
            }
        }

        public void UpdateCableCursor()
        {
            if (currentlyCreatedCables >= maxCableNum)
            {
                SetCableBundleColor(CableColors.NotSelected);
            }
            else
            {
                SetCableBundleColor(currentlySelectedColor);
            }
        }

        /// <summary>
        /// Sets the current cable color based on the color passed in
        /// </summary>
        public void SetCableBundleColor(CableControls.CableColors color)
        {
            currentlySelectedColor = color;

            EnableCableBundleOutlineByType(redCableBundle, false);
            EnableCableBundleOutlineByType(blackCableBundle, false);

            EnableTransparentLeadByType();

            if (color == CableColors.Black)
            {
                EnableCableBundleOutlineByType(blackCableBundle, true);
            }
            else if (color == CableColors.Red)
            {
                EnableCableBundleOutlineByType(redCableBundle, true);
            }
        }

        public void FinishCableConnection(PortBehaviour endingPort)
        {
            if (endingPort == currentlySelectedLead.GetOtherLead().ConnectedPort)
            {
                return;
            }

            currentlyCreatedCables = Math.Min(currentlyCreatedCables + 1, maxCableNum);
            currentCablesPlaced = currentlyCreatedCables;

            if (currentlySelectedTransparentCableLead == null)
            {
                Cursor.visible = true;
            }
            else
            {
                currentlySelectedTransparentCableLead?.SetActive(true);
            }

            UpdateCableCursor();

            currentlySelectedLead.ConnectPort(endingPort);

            // current position + lead offset + (number of leads connected - 1) * height offset of a lead)
            // (number of leads connected - 1) because the leads number has been increased by 1 earlier in ConnectPort()
            currentlySelectedLead.transform.position =
                endingPort.transform.position + portCableLeadOffset +
                (endingPort.NumberLeadsConnected - 1) * portCableLeadHeightOffset;

            currentlySelectedLead.EnableSelection(true);
            currentlySelectedLead.GetOtherLead().EnableSelection(true);

            currentlySelectedCable.gameObject.ShowOutline(false);

            RotateOtherLead();

            currentlySelectedCable.CreateMesh();

            currentlySelectedCable.cableStart.gameObject.ShowOutline(false);
            currentlySelectedCable.cableEnd.gameObject.ShowOutline(false);

            connectedCables.Add(currentlySelectedCable);

            OnCablePlaced?.Invoke(currentlySelectedCable);

            currentlySelectedCable = null;
            currentlySelectedLead = null;
        }

        /// <summary>
        /// Sets the cable outline state based on if the lead collider state matches the bool passed in
        /// (true by default)
        /// </summary>
        private void SetCableOutlineState(PortBehaviour port, bool enabled = true)
        {
            // If enabled has been overridden to be false, disables all lead outlines in the stack (see OnPortHoverOff).
            // This is because in the scenario that an incorrect placement has happened, the lead collider will still be enabled
            // meaning that it would pass the condition in the below foreach.
            if (enabled == false)
            {
                foreach (CableLead lead in GetPortConnectionsArray(port))
                {
                    lead.gameObject.ShowOutline(false);
                }

                return;
            }

            // Enables the outline of only the active lead within a stack
            foreach (CableLead lead in GetPortConnectionsArray(port))
            {
                lead.gameObject.ShowOutline(false);

                if (lead.GetComponent<Collider>().enabled)
                {
                    lead.gameObject.ShowOutline(true, interactionManager.InteractableHoverColor);
                }
            }
        }

        /// <summary>
        /// Uses the public function from the port class to return all leads connected to the passed in port
        /// </summary>
        private CableLead[] GetPortConnectionsArray(PortBehaviour port)
        {
            CableLead[] leadsConnectedToThisPort = port.GetAllConnectedLeads();

            return leadsConnectedToThisPort;
        }

        /// <summary>
        /// Uses the public function from the port class to return the lead at the requested index
        /// </summary>
        private CableLead GetPortConnectionAtIndex(PortBehaviour port, int index)
        {
            return port.GetConnectedLeadAtIndex(index);
        }

        public void OnLeadHovered(CableLead lead)
        {
            if (lead.ConnectedPort == null)
                return;

            OnPortHover(lead.ConnectedPort);
        }

        public void OnLeadHoveredOff(CableLead lead)
        {
            if (lead.ConnectedPort == null)
                return;

            OnPortHoverOff(lead.ConnectedPort);
        }


        public void CreateAndSelectNewCable(PortBehaviour startingPort, bool isFaultyCable = false)
        {
            if (currentlyCreatedCables >= maxCableNum)
                return;
            if (currentlySelectedColor == CableColors.NotSelected)
                return;

            Cursor.visible = false;

            if (currentlySelectedTransparentCableLead != null)
            {
                currentlySelectedTransparentCableLead.SetActive(false);
            }

            GameObject cablePrefab = currentlySelectedColor == CableColors.Red ? redCablePrefab : blackCablePrefab;
            Vector3 spawnPosition = startingPort.transform.position;

            currentlySelectedCable = Instantiate(cablePrefab, spawnPosition, cablePrefab.transform.rotation).GetComponent<CableConnector>();

            // this would be assigned as true via the circuit importer scriptable object if faulty
            currentlySelectedCable.IsFaulty = isFaultyCable;

            // current position + lead offset + (number of leads connected * height offset of a lead)
            currentlySelectedCable.cableStart.transform.localPosition =
                portCableLeadOffset + startingPort.NumberLeadsConnected * portCableLeadHeightOffset;

            currentlySelectedCable.cableStart.ConnectPort(startingPort);

            currentlySelectedLead = currentlySelectedCable.cableEnd;
            currentlySelectedLead.EnableSelection(false);
        }

        private void DeleteCableOnMouseClick()
        {
            if (currentlySelectedCable == null)
                return;

            // Only allow deletion of the cable on mouse click when the user right clicks
            if (Input.GetMouseButtonUp(SecondaryMouseButton))
            {
                // Sets the selected color of the transparent lead to be the current cables color
                currentlySelectedColor = currentlySelectedCable.Color;
                EnableTransparentLeadByType();

                // Delete the selected cable
                UpdateCableCursor();
                OnDeleteIconClicked(currentlySelectedCable, true);
            }
        }

        public void UpdateSelectedLeadPosition()
        {
            if (currentlySelectedLead == null)
            {
                // Enabling interactable components (L1, L2, Fuse and Switch) so that they do not enable on click with the lead enabled
                interactionManager.ToggleComponentInteractions(true);

                return;
            }

            // Disabling interactable components (L1, L2, Fuse and Switch) so that they do not enable on click with the lead enabled
            interactionManager.ToggleComponentInteractions(false);

            SetMousePositionAndSnapping(currentlySelectedLead.gameObject);
        }

        public void UpdateTransparentLeadPosition()
        {
            if (currentlySelectedColor == CableColors.NotSelected) return;
            if (currentlySelectedCable != null) return;
            if (currentlyCreatedCables >= maxCableNum) return;
            if (currentlySelectedTransparentCableLead == null) return;

            // Disabling interactable components (L1, L2, Fuse and Switch) so that they do not enable on click with the transparent lead enabled
            interactionManager.ToggleComponentInteractions(false);

            SetMousePositionAndSnapping(currentlySelectedTransparentCableLead);

            if (Input.GetMouseButtonUp(SecondaryMouseButton))
            {
                RemoveCurrentTransparentLead();
            }
        }

        private void RemoveCurrentTransparentLead()
        {
            currentlySelectedTransparentCableLead?.SetActive(false);
            currentlySelectedTransparentCableLead = null;
            Cursor.visible = true;
            ResetCableBundleState();
            if (currentHoveredPort != null)
            {
                OnPortHoverOff(currentHoveredPort);
                currentHoveredPort = null;
            }
        }

        /// <summary>
        /// Sets the mouse position and snapping for the lead or the transparent lead.
        /// </summary>
        /// <param name="obj"> The game object that need to follow the mouse and snap to a port. </param>
        private void SetMousePositionAndSnapping(GameObject obj)
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane * 4));

            mouseWorldPosition = PreventLeadGoingThroughJarOfShame(mouseWorldPosition);

            mouseWorldPosition.y = Mathf.Clamp(mouseWorldPosition.y, leadMinY, leadMaxY);

            obj.transform.position = mouseWorldPosition;

            if (currentHoveredPort == null)
            {
                obj.transform.position = mouseWorldPosition + selectedCableLeadOffset;
            }
            else
            {
                obj.transform.position =
                    currentHoveredPort.transform.position + portCableLeadOffset +
                    currentHoveredPort.NumberLeadsConnected * portCableLeadHeightOffset;
            }
        }

        private void RotateOtherLead()
        {
            if (currentlySelectedCable == null)
                return;

            Vector3 target = currentlySelectedCable.cableStart.transform.position - currentlySelectedCable.cableEnd.transform.position;
            target.y = 0f;

            if (target == Vector3.zero) return;

            var lookTowardsStart = Quaternion.LookRotation(-target);
            var lookTowardsEnd = Quaternion.LookRotation(target);

            currentlySelectedCable.cableStart.transform.rotation = lookTowardsEnd;
            currentlySelectedCable.cableEnd.transform.rotation = lookTowardsStart;
        }

        public void RemoveAllCables()
        {
            if (connectedCables == null)
            {
                return;
            }
            foreach (CableConnector c in connectedCables)
            {
                c.Disconnect();
            }

            connectedCables = new HashSet<CableConnector>();

            currentlySelectedTransparentCableLead?.SetActive(false);
        }

        /// <summary>
        /// Sets the value for can create cable and displays the ghost cable if it's a cable objective.
        /// </summary>
        /// <param name="value"> can cable be created </param>
        public void CanCreateCables(int maxAmount)
        {
            maxCableNum = maxAmount;
            currentlyCreatedCables = 0;
            currentCablesPlaced = 0;
            //Debug.Log(currentlyCreatedCables);
            //UpdateCableCursor();

            /*
            if (currentlySelectedTransparentCableLead == null)
                return;

            Debug.Log("set");

            // If there is a selected transparent cable lead the game object will be set to the
            // appropriate value.
            currentlySelectedTransparentCableLead.SetActive(currentlyCreatedCables < maxCableNum);
            */
        }

        /// <summary>
        /// Keeps lead out of top right corner of screen to prevent it from going through jar of shame
        /// </summary>
        /// <param name="_mousePosition"> the input mouse position in worldspace coordinates </param>
        /// <returns></returns>
        private Vector3 PreventLeadGoingThroughJarOfShame(Vector3 _mousePosition)
        {
            float leftSideOfJar = 4.375f, bottomOfJar = 0.350f, slightlyAboveBotttomOfJar = 0.27f, slightlyBelowBotttomOfJar = 0.365f;

            // z = sideways/left & right, x = up & down (in worldspace coordinates) -- doesn't mess up position when aspect ratio changes
            float mouseSidewaysInputPosition = _mousePosition.z, mouseUpAndDownPosition = _mousePosition.x;
            float mouseSidewaysTempPosition = _mousePosition.z;

            // Mouse in top right corner where jar is --> restrict mouse sideways position to the left of the jar
            if (mouseSidewaysInputPosition >= leftSideOfJar && mouseUpAndDownPosition <= bottomOfJar)
            {
                mouseSidewaysTempPosition = leftSideOfJar;
            }

            // This statement allows mouse/lead to slide left & right when close to bottom of jar
            if (mouseSidewaysInputPosition >= leftSideOfJar && mouseUpAndDownPosition >= slightlyAboveBotttomOfJar && mouseUpAndDownPosition <= slightlyBelowBotttomOfJar)
            {
                mouseUpAndDownPosition = bottomOfJar;
                mouseSidewaysTempPosition = mouseSidewaysInputPosition;
            }

            _mousePosition.x = mouseUpAndDownPosition;
            _mousePosition.z = mouseSidewaysTempPosition;

            return _mousePosition;
        }

    }
}