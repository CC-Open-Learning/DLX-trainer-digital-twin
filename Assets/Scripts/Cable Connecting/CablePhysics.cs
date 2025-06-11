using UnityEngine;

namespace VARLab.MPCircuits
{
    public class CablePhysics : MonoBehaviour
    {
        public Vector3 _position, _oldPosition;
        public Transform _boundTo = null;
        public Rigidbody _boundRigid = null;

        // Getting and setting the current position of a cable
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        // Getting the velocity of the current cable
        public Vector3 Velocity
        {
            get { return (_position - _oldPosition); }
        }

        // Setting the initial position of a cable
        public CablePhysics(Vector3 tempPosition)
        {
            _oldPosition = _position = tempPosition;
        }

        public void UpdateVerlet(Vector3 gravityDisplacement)
        {
            // Logic for if the cable is bound to the board
            if (IsBound())
            {
                if (_boundRigid == null)
                {
                    UpdatePosition(_boundTo.position);
                }
                else
                {
                    switch (_boundRigid.interpolation)
                    {
                        case RigidbodyInterpolation.Interpolate:
                            UpdatePosition(_boundRigid.position + (_boundRigid.velocity * Time.fixedDeltaTime) / 2);
                            break;

                        case RigidbodyInterpolation.None:
                        default:
                            UpdatePosition(_boundRigid.position + _boundRigid.velocity * Time.fixedDeltaTime);
                            break;
                    }
                }
            }
            else
            {
                Vector3 tempPosition = Position + Velocity + gravityDisplacement;
                UpdatePosition(tempPosition);
            }
        }

        // Store the previous position in _oldPosition and change the current position using the parameter
        public void UpdatePosition(Vector3 pos)
        {
            _oldPosition = _position;
            _position = pos;
        }

        // Used to bind the start or end lead game objects
        public void Bind(Transform to)
        {
            _boundTo = to;
            _boundRigid = to.GetComponent<Rigidbody>();
            _oldPosition = _position = _boundTo.position;
        }

        // Reset the boundings of the cable
        public void UnBind()
        {
            _boundTo = null;
            _boundRigid = null;
        }

        // True if the cable particles are free
        public bool IsFree()
        {
            return (_boundTo == null);
        }

        // True if the cable is bound to the board
        public bool IsBound()
        {
            return (_boundTo != null);
        }
    }
}