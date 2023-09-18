using UnityEngine;
using Obi;

// ReSharper disable once IdentifierTypo
namespace Dev_LSG.Scripts.Interactables
{
    [RequireComponent(typeof(ObiCollider))]
    public class RopeGrabber : MonoBehaviour
    {
        public bool canGrab = true;
        private ObiSolver _solver;
        private ObiCollider _obiCollider;
        public ObiRope rope;
        private ObiSolver.ObiCollisionEventArgs _collisionEvent;
        private ObiPinConstraintsBatch _newBatch;
        private ObiConstraints<ObiPinConstraintsBatch> _pinConstraints;

        private void Awake()
        {
            _obiCollider = GetComponent<ObiCollider>();
        }
        
        private void Start()
        {
            _pinConstraints =
                rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        }
        
        private void OnEnable()
        {
            _solver = rope.solver;
            if (_solver != null)
                _solver.OnCollision += Solver_OnCollision;
        }

        private void OnDisable()
        {
            if (_solver != null)
                _solver.OnCollision -= Solver_OnCollision;
        }

        void Solver_OnCollision(object sender, ObiSolver.ObiCollisionEventArgs e)
        {
            _collisionEvent = e;
        }

        public void Grab()
        {
            var world = ObiColliderWorld.GetInstance();
            Debug.Log(_pinConstraints);

            if (_solver != null && _collisionEvent != null)
            {
                Debug.Log("Collision");
                foreach (Oni.Contact contact in _collisionEvent.contacts)
                {
                    if (contact.distance < 0.01f)
                    {
                        var contactController = world.colliderHandles[contact.bodyB].owner;
                        ObiSolver.ParticleInActor pa = _solver.particleToActor[contact.bodyA];

                        Debug.Log($"{pa}hit{contactController}");
                        if (canGrab)
                        {
                            if (contactController == _obiCollider)
                            {
                                Debug.Log("Hand Collision");
                                var batch = new ObiPinConstraintsBatch();
                                int solverIndex = rope.solverIndices[contact.bodyA];
                                Vector3 positionWs = _solver.transform.TransformPoint(_solver.positions[solverIndex]);
                                Vector3 positionCs = _obiCollider.transform.InverseTransformPoint(positionWs);
                                batch.AddConstraint(rope.solverIndices[contact.bodyA], _obiCollider, positionCs, Quaternion.identity, 0, 0, float.PositiveInfinity);
                                batch.activeConstraintCount = 1;
                                _newBatch = batch;
                                _pinConstraints.AddBatch(_newBatch);

                                canGrab = false;

                                // this will cause the solver to rebuild pin constraints at the beginning of the next frame:
                                rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
                            }
                        }
                    }
                }
            }
        }

        public void Release()
        {
            if (!canGrab)
            {
                Debug.Log("Release");
                _pinConstraints.RemoveBatch(_newBatch);
                rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
                canGrab = true;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G)) 
                Grab();
            if (Input.GetKeyDown(KeyCode.R)) 
                Release();
        }
    }
}
