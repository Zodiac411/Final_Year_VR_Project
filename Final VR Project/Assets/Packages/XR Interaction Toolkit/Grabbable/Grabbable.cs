using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactable component that allows for basic grab functionality.
    /// When this behavior is selected (grabbed) by an Interactor, this behavior will follow it around
    /// and inherit velocity when released.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behavior is responsible for applying the position, rotation, and local scale calculated
    /// by one or more <see cref="IXRGrabTransformer"/> implementations. A default set of grab transformers
    /// are automatically added by Unity, but this functionality can be disabled to manually set those
    /// used by this behavior, allowing you to customize where this component should move and rotate to.
    /// </para>
    /// <para>
    /// Grab transformers are classified into two different types: Single and Multiple.
    /// Those added to the Single Grab Transformers list are used when there is a single interactor selecting this object.
    /// Those added to the Multiple Grab Transformers list are used when there are multiple interactors selecting this object.
    /// You can add multiple grab transformers in a category and they will each be processed in sequence.
    /// The Multiple Grab Transformers are given first opportunity to process when there are multiple grabs, and
    /// the Single Grab Transformer processing will be skipped if a Multiple Grab Transformer can process in that case.
    /// </para>
    /// <para>
    /// There are fallback rules that could allow a Single Grab Transformer to be processed when there are multiple grabs,
    /// and for a Multiple Grab Transformer to be processed when there is a single grab (though a grab transformer will never be
    /// processed if its <see cref="IXRGrabTransformer.canProcess"/> returns <see langword="false"/>).
    /// <list type="bullet">
    /// <item>
    /// <description>When there is a single interactor selecting this object, the Multiple Grab Transformer will be processed only
    ///  if the Single Grab Transformer list is empty or if all transformers in the Single Grab Transformer list return false during processing.</description>
    /// </item>
    /// <item>
    /// <description>When there are multiple interactors selecting this object, the Single Grab Transformer will be processed only
    /// if the Multiple Grab Transformer list is empty or if all transformers in the Multiple Grab Transformer list return false during processing.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IXRGrabTransformer"/>
    [SelectionBase]

    public class Grabbable : MonoBehaviour
    {
        public bool BeingHeld = false;

        public bool BeingHeldWithTwoHands
        {
            get
            {
                if (heldByGrabbers != null && heldByGrabbers.Count > 1 && SecondaryGrabBehavior == OtherGrabBehavior.DualGrab)
                {
                    return true;
                }
                else if (BeingHeld && SecondaryGrabbable != null && SecondaryGrabbable.BeingHeld == true)
                {
                    return true;
                }

                return false;
            }
        }

        List<Grabber> validGrabbers;
        protected List<Grabber> heldByGrabbers;

        public List<Grabber> HeldByGrabbers
        {
            get
            {
                return heldByGrabbers;
            }
        }

        protected bool wasKinematic;
        protected bool usedGravity;

        protected CollisionDetectionMode initialCollisionMode;
        protected RigidbodyInterpolation initialInterpolationMode;

        public bool RemoteGrabbing
        {
            get
            {
                return remoteGrabbing;
            }
        }

        protected bool remoteGrabbing;

        [Header("Grab Settings")]
        /// <summary>
        /// Configure which button is used to initiate the grab
        /// </summary>
        [Tooltip("Configure which button is used to initiate the grab")]
        public GrabButton GrabButton = GrabButton.Inherit;

        /// <summary>
        /// 'Inherit' will inherit this setting from the Grabber. 'Hold' requires the user to hold the GrabButton down. 'Toggle' will drop / release the Grabbable on button activation.
        /// </summary>
        [Tooltip("'Inherit' will inherit this setting from the Grabber. 'Hold' requires the user to hold the GrabButton down. 'Toggle' will drop / release the Grabbable on button activation.")]
        public HoldType Grabtype = HoldType.Inherit;

        /// <summary>
        /// Kinematic Physics locks the object in place on the hand / grabber. PhysicsJoint allows collisions with the environment.
        /// </summary>
        [Tooltip("Kinematic Physics locks the object in place on the hand / grabber. Physics Joint and Velocity types allow collisions with the environment.")]
        public GrabPhysics GrabPhysics = GrabPhysics.Velocity;

        /// <summary>
        /// Snap to a location or grab anywhere on the object
        /// </summary>
        [Tooltip("Snap to a location or grab anywhere on the object")]
        public GrabType GrabMechanic = GrabType.Precise;

        /// <summary>
        /// How fast to Lerp the object to the hand
        /// </summary>
        [Tooltip("How fast to Lerp the object to the hand")]
        public float GrabSpeed = 15f;

        /// <summary>
        /// Can the object be picked up from far away. Must be within RemoteGrabber Trigger
        /// </summary>
        [Header("Remote Grab")]
        [Tooltip("Can the object be picked up from far away. Must be within RemoteGrabber Trigger")]
        public bool RemoteGrabbable = false;

        public RemoteGrabMovement RemoteGrabMechanic = RemoteGrabMovement.Linear;

        /// <summary>
        /// Max Distance Object can be Remote Grabbed. Not applicable if RemoteGrabbable is false
        /// </summary>
        [Tooltip("Max Distance Object can be Remote Grabbed. Not applicable if RemoteGrabbable is false")]
        public float RemoteGrabDistance = 2f;

        /// <summary>
        /// Multiply controller's velocity times this when throwing
        /// </summary>
        [Header("Throwing")]
        [Tooltip("Multiply controller's velocity times this when throwing")]
        public float ThrowForceMultiplier = 2f;

        /// <summary>
        /// Multiply controller's angular velocity times this when throwing
        /// </summary>
        [Tooltip("Multiply controller's angular velocity times this when throwing")]
        public float ThrowForceMultiplierAngular = 1.5f; // Multiply Angular Velocity times this

        /// <summary>
        /// Drop the item if object's center travels this far from the Grabber's Center (in meters). Set to 0 to disable distance break.
        /// </summary>
        [Tooltip("Drop the item if object's center travels this far from the Grabber's Center (in meters). Set to 0 to disable distance break.")]
        public float BreakDistance = 0;

        /// <summary>
        /// Enabling this will hide the Transform specified in the Grabber's HandGraphics property
        /// </summary>
        [Header("Hand Options")]
        [Tooltip("Enabling this will hide the Transform specified in the Grabber's HandGraphics property")]
        public bool HideHandGraphics = false;

        /// <summary>
        ///  Parent this object to the hands for better stability.
        ///  Not recommended for child grabbers
        /// </summary>
        [Tooltip("Parent this object to the hands for instantaneous movement. Object will travel 1:1 with the controller but may have trouble detecting fast moving collisions.")]
        public bool ParentToHands = false;

        /// <summary>
        /// If true, the hand model will be attached to the grabbed object
        /// </summary>
        [Tooltip("If true, the hand model will be attached to the grabbed object. This separates it from a 1:1 match with the controller, but may look more realistic.")]
        public bool ParentHandModel = true;

        [Tooltip("If true, the hand model will snap to the nearest GrabPoint. Otherwise the hand model will stay with the Grabber.")]
        public bool SnapHandModel = true;

        /// <summary>
        /// Set to false to disable dropping. If false, will be permanently attached to whatever grabs this.
        /// </summary>
        [Header("Misc")]
        [Tooltip("Set to false to disable dropping. If false, will be permanently attached to whatever grabs this.")]
        public bool CanBeDropped = true;

        /// <summary>
        /// Can this object be snapped to snap zones? Set to false if you never want this to be snappable. Further filtering can be done on the SnapZones
        /// </summary>
        [Tooltip("Can this object be snapped to snap zones? Set to false if you never want this to be snappable. Further filtering can be done on the SnapZones")]
        public bool CanBeSnappedToSnapZone = true;

        [Tooltip("If true, the object will always have kinematic disabled when dropped, even if it was initially kinematic.")]
        public bool ForceDisableKinematicOnDrop = false;

        [Tooltip("If true, the object will instantly position / rotate to the grabber instead of using velocity / force. This will only happen if no collisions have recently occurred. When using this method, the Grabbable's Rigidbody willbe instantly rotated / moved, taking in to account the interpolation settings. May clip through objects if moving fast enough.")]
        public bool InstantMovement = false;

        [Tooltip("If true, all child colliders will be considered Grabbable. If false, you will need to add the 'GrabbableChild' component to any child colliders that you wish to also be considered grabbable.")]
        public bool MakeChildCollidersGrabbable = false;

        [Header("Default Hand Pose")]
        [Tooltip("A hand controller can read this value to determine how to animate when grabbing this object. 'AnimatorID' = specify an Animator ID to be set on the hand animator after grabbing this object. 'HandPose' = use a HandPose scriptable object. 'AutoPoseOnce' = DO an auto pose one time upon grabbing this object. 'AutoPoseContinuous' = Keep running attempting an autopose while grabbing this item.")]
        public HandPoseType handPoseType = HandPoseType.HandPose;
        protected HandPoseType initialHandPoseType;

        [Tooltip("If HandPoseType = 'HandPose', this HandPose object will be applied to the hand on pickup")]
        public HandPose SelectedHandPose;
        protected HandPose initialHandPose;

        /// <summary>
        /// Animator ID of the Hand Pose to use
        /// </summary>
        [Tooltip("This HandPose Id will be passed to the Hand Animator when equipped. You can add new hand poses in the HandPoseDefinitions.cs file.")]
        public HandPoseId CustomHandPose = HandPoseId.Default;
        protected HandPoseId initialHandPoseId;

        /// <summary>
        /// What to do if another grabber grabs this while equipped. DualGrab is currently unsupported.
        /// </summary>
        [Header("Two-Handed Grab Behavior")]
        [Tooltip("What to do if another grabber grabs this while equipped.")]
        public OtherGrabBehavior SecondaryGrabBehavior = OtherGrabBehavior.None;

        [Tooltip("How to behave when two hands are grabbing this object. LookAt = Have the primary Grabber 'LookAt' the secondary grabber. For example, holding a rifle in the right controller will have it rotate towards the left controller. AveragePositionRotation = Use a point and rotation in space that is half-way between both grabbers.")]
        public TwoHandedPositionType TwoHandedPosition = TwoHandedPositionType.Lerp;

        [Tooltip("How far to lerp between grabber positions. For example, 0.5 = halfway between the primary and secondary grabber. 0 = use the primary grabber's position, 1 = use the secondary grabber's position.")]
        [Range(0.0f, 1f)]
        public float TwoHandedPostionLerpAmount = 0.5f;

        [Tooltip("How to behave when two hands are grabbing this object. LookAt = Have the primary Grabber 'LookAt' the secondary grabber. For example, holding a rifle in the right controller will have it rotate towards the left controller. AveragePositionRotation = Use a point and rotation in space that is half-way between both grabbers.")]
        public TwoHandedRotationType TwoHandedRotation = TwoHandedRotationType.Slerp;

        [Tooltip("How far to lerp / slerp between grabber rotation. For example, 0.5 = halfway between the primary and secondary grabber. 0 = use the primary grabber's rotation, 1 = use the secondary grabber's position.")]
        [Range(0.0f, 1f)]
        public float TwoHandedRotationLerpAmount = 0.5f;

        [Tooltip("How to repond if you are holding an object with two hands, and then drop the primary grabber. For example, you may want to drop the object, transfer it to the second hand, or do nothing at all.")]
        public TwoHandedDropMechanic TwoHandedDropBehavior = TwoHandedDropMechanic.Drop;

        [Tooltip("Which vector to use when TwoHandedRotation = LookAtSecondary. Ex : Horizontal = A rifle type setup where you want to aim down the sites; Vertical = A melee type setup where the object is vertical")]
        public TwoHandedLookDirection TwoHandedLookVector = TwoHandedLookDirection.Horizontal;

        [Tooltip("How quickly to Lerp towards the SecondaryGrabbable if TwoHandedGrabBehavior = LookAt")]
        public float SecondHandLookSpeed = 40f;

        [Header("Secondary Grabbale Object")]
        [Tooltip("If specified, this object will be used as a secondary grabbable instead of relying on grab points on this object. If 'TwoHandedGrabBehavior' is specified as LookAt, this is the object the grabber will be rotated towards. If 'TwoHandedGrabBehavior' is specified as AveragePositionRotation, this is the object the grabber use to calculate position.")]
        public Grabbable SecondaryGrabbable;

        /// <summary>
        /// The Grabbable can only be grabbed if this grabbable is being held.
        /// Example : If you only want a weapon part to be grabbable if the weapon itself is being held.
        /// </summary>
        [Header("Grab Restrictions")]
        [Tooltip("The Grabbable can only be grabbed if this grabbable is being held. Example : If you only want a weapon part to be grabbable if the weapon itself is being held.")]
        public Grabbable OtherGrabbableMustBeGrabbed = null;

        [Header("Physics Joint Settings")]
        /// <summary>
        /// How much Spring Force to apply to the joint when something comes in contact with the grabbable
        /// A higher Spring Force will make the Grabbable more rigid
        /// </summary>
        [Tooltip("A higher Spring Force will make the Grabbable more rigid")]
        public float CollisionSpring = 3000;

        /// <summary>
        /// How much Slerp Force to apply to the joint when something is in contact with the grabbable
        /// </summary>
        [Tooltip("How much Slerp Force to apply to the joint when something is in contact with the grabbable")]
        public float CollisionSlerp = 500;

        [Tooltip("How to restrict the Configurable Joint's xMotion when colliding with an object. Position can be free, completely locked, or limited.")]
        public ConfigurableJointMotion CollisionLinearMotionX = ConfigurableJointMotion.Free;

        [Tooltip("How to restrict the Configurable Joint's yMotion when colliding with an object. Position can be free, completely locked, or limited.")]
        public ConfigurableJointMotion CollisionLinearMotionY = ConfigurableJointMotion.Free;

        [Tooltip("How to restrict the Configurable Joint's zMotion when colliding with an object. Position can be free, completely locked, or limited.")]
        public ConfigurableJointMotion CollisionLinearMotionZ = ConfigurableJointMotion.Free;

        [Tooltip("Restrict the rotation around the X axes to be Free, completely Locked, or Limited when colliding with an object.")]
        public ConfigurableJointMotion CollisionAngularMotionX = ConfigurableJointMotion.Free;

        [Tooltip("Restrict the rotation around the Y axes to be Free, completely Locked, or Limited when colliding with an object.")]
        public ConfigurableJointMotion CollisionAngularMotionY = ConfigurableJointMotion.Free;

        [Tooltip("Restrict the rotation around Z axes to be Free, completely Locked, or Limited when colliding with an object.")]
        public ConfigurableJointMotion CollisionAngularMotionZ = ConfigurableJointMotion.Free;


        [Tooltip("If true, the object's velocity will be adjusted to match the grabber. This is in addition to any forces added by the configurable joint.")]
        public bool ApplyCorrectiveForce = true;

        [Header("Velocity Grab Settings")]
        public float MoveVelocityForce = 3000f;
        public float MoveAngularVelocityForce = 90f;

        /// <summary>
        /// Time in seconds (Time.time) when we last grabbed this item
        /// </summary>
        [HideInInspector]
        public float LastGrabTime;

        /// <summary>
        /// Time in seconds (Time.time) when we last dropped this item
        /// </summary>
        [HideInInspector]
        public float LastDropTime;

        /// <summary>
        /// Set to True to throw the Grabbable by applying the controller velocity to the grabbable on drop. 
        /// Set False if you don't want the object to be throwable, or want to apply your own force manually
        /// </summary>
        [HideInInspector]
        public bool AddControllerVelocityOnDrop = true;

        // Total distance between the Grabber and Grabbable.
        float journeyLength;

        public Vector3 OriginalScale { get; private set; }

        // Keep track of objects that are colliding with us
        [Header("Shown for Debug : ")]
        [SerializeField]
        public List<Collider> collisions;

        // Last time in seconds (Time.time) since we had a valid collision
        public float lastCollisionSeconds { get; protected set; }

        /// <summary>
        /// How many seconds we've gone without collisions
        /// </summary>
        public float lastNoCollisionSeconds { get; protected set; }

        /// <summary>
        /// Have we recently collided with an object
        /// </summary>
        public bool RecentlyCollided
        {
            get
            {
                if (Time.time - lastCollisionSeconds <= 0.1f)
                {
                    return true;
                }

                if (collisions != null && collisions.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        // If Time.time < requestSpringTime, force joint to be springy
        public float requestSpringTime { get; protected set; }

        /// <summary>
        /// If Grab Mechanic is set to Snap, set position and rotation to this Transform on the primary Grabber
        /// </summary>
        protected Transform primaryGrabOffset;
        protected Transform secondaryGrabOffset;

        /// <summary>
        /// Returns the active GrabPoint component if object is held and a GrabPoint has been assigneed
        /// </summary>
        [HideInInspector]
        public GrabPoint ActiveGrabPoint;

        [HideInInspector]
        public Vector3 SecondaryLookOffset;

        [HideInInspector]
        public Transform SecondaryLookAtTransform;

        [HideInInspector]
        public Transform LocalOffsetTransform;

        Vector3 grabPosition
        {
            get
            {
                if (primaryGrabOffset != null)
                {
                    return primaryGrabOffset.position;
                }
                else
                {
                    return transform.position;
                }
            }
        }

        [HideInInspector]
        public Vector3 GrabPositionOffset
        {
            get
            {
                if (primaryGrabOffset)
                {
                    return primaryGrabOffset.transform.localPosition;
                }

                return Vector3.zero;
            }
        }

        [HideInInspector]
        public Vector3 GrabRotationOffset
        {
            get
            {
                if (primaryGrabOffset)
                {
                    return primaryGrabOffset.transform.localEulerAngles;
                }
                return Vector3.zero;
            }
        }

        private Transform _grabTransform;

        public Transform grabTransform
        {
            get
            {
                if (_grabTransform != null)
                {
                    return _grabTransform;
                }

                _grabTransform = new GameObject().transform;
                _grabTransform.parent = this.transform;
                _grabTransform.name = "Grab Transform";
                _grabTransform.localPosition = Vector3.zero;

                return _grabTransform;
            }
        }

        private Transform _grabTransformSecondary;

        public Transform grabTransformSecondary
        {
            get
            {
                if (_grabTransformSecondary != null)
                {
                    return _grabTransformSecondary;
                }

                _grabTransformSecondary = new GameObject().transform;
                _grabTransformSecondary.parent = this.transform;
                _grabTransformSecondary.name = "Grab Transform Secondary";
                _grabTransformSecondary.localPosition = Vector3.zero;
                _grabTransformSecondary.hideFlags = HideFlags.HideInHierarchy;

                return _grabTransformSecondary;
            }
        }

        [Header("Grab Points")]
        /// <summary>
        /// If Grab Mechanic is set to Snap, the closest GrabPoint will be used. Add a SnapPoint Component to a GrabPoint to specify custom hand poses and rotation.
        /// </summary>
        [Tooltip("If Grab Mechanic is set to Snap, the closest GrabPoint will be used. Add a SnapPoint Component to a GrabPoint to specify custom hand poses and rotation.")]
        public List<Transform> GrabPoints;

        /// <summary>
        /// Can the object be moved towards a Grabber. 
        /// Levers, buttons, doorknobs, and other types of objects cannot be moved because they are attached to another object or are static.
        /// </summary>
        public bool CanBeMoved
        {
            get
            {
                return _canBeMoved;
            }
        }
        private bool _canBeMoved;

        protected Transform originalParent;
        protected XRInput input;
        protected ConfigurableJoint connectedJoint;
        protected Vector3 previousPosition;
        protected float lastItemTeleportTime;
        protected bool recentlyTeleported;

        /// <summary>
        /// Set this to false if you need to see Debug field or don't want to use the custom inspector
        /// </summary>
        [HideInInspector]
        public bool UseCustomInspector = true;

        /// <summary>
        /// If a BNGPlayerController is provided we can check for player movements and make certain adjustments to physics.
        /// </summary>
        protected PlayerController player
        {
            get
            {
                return GetPlayerController();
            }
        }
        private PlayerController _player;
        protected Collider col;
        protected Rigidbody rb;

        public Grabber FlyingToGrabber
        {
            get
            {
                return flyingTo;
            }
        }
        protected Grabber flyingTo;

        protected List<GrabbableEvents> events;

        public bool DidParentHands
        {
            get
            {
                return didParentHands;
            }
        }
        protected bool didParentHands = false;

        protected void Awake()
        {
            col = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            input = XRInput.Instance;

            events = GetComponents<GrabbableEvents>().ToList();
            collisions = new List<Collider>();

            // Try parent if no rigid found here
            if (rb == null && transform.parent != null)
            {
                rb = transform.parent.GetComponent<Rigidbody>();
            }

            // Store initial rigidbody properties so we can reset them later as needed
            if (rb)
            {
                initialCollisionMode = rb.collisionDetectionMode;
                initialInterpolationMode = rb.interpolation;
                wasKinematic = rb.isKinematic;
                usedGravity = rb.useGravity;

                // Allow our rigidbody to rotate quickly
                rb.maxAngularVelocity = 25f;
            }

            // Store initial parent so we can reset later if needed
            UpdateOriginalParent(transform.parent);

            validGrabbers = new List<Grabber>();

            // Set Original Scale based in World coordinates if available
            if (transform.parent != null)
            {
                OriginalScale = transform.lossyScale; // OriginalScale = transform.parent.TransformVector(transform.localScale);
            }
            else
            {
                OriginalScale = transform.localScale;
            }

            initialHandPoseId = CustomHandPose;
            initialHandPose = SelectedHandPose;
            initialHandPoseType = handPoseType;

            // Store movement status
            _canBeMoved = canBeMoved();

            // Set up any Child Grabbable Objects
            if (MakeChildCollidersGrabbable)
            {
                Collider[] cols = GetComponentsInChildren<Collider>();
                for (int x = 0; x < cols.Length; x++)
                {
                    // Make child Grabbable if it isn't already
                    if (cols[x].GetComponent<Grabbable>() == null && cols[x].GetComponent<GrabbableChild>() == null)
                    {
                        var gc = cols[x].gameObject.AddComponent<GrabbableChild>();
                        gc.ParentGrabbable = this;
                    }
                }
            }
        }

        public virtual void Update()
        {

            if (BeingHeld)
            {

                // ResetLockResets();

                // Something happened to our Grabber. Drop the item
                if (heldByGrabbers == null)
                {
                    DropItem(null, true, true);
                    return;
                }

                // Make sure all collisions are valid
                filterCollisions();

                // Cache PrimaryGrabber designation
                _priorPrimaryGrabber = GetPrimaryGrabber();

                // Update collision time
                if (collisions != null && collisions.Count > 0)
                {
                    lastCollisionSeconds = Time.time;
                    lastNoCollisionSeconds = 0;
                }
                else if (collisions != null && collisions.Count <= 0)
                {
                    lastNoCollisionSeconds += Time.deltaTime;
                }

                // Update item recently teleported time
                if (Vector3.Distance(transform.position, previousPosition) > 0.1f)
                {
                    lastItemTeleportTime = Time.time;
                }
                recentlyTeleported = Time.time - lastItemTeleportTime < 0.2f;

                // Loop through held grabbers and see if we need to drop the item, fire off events, etc.
                for (int x = 0; x < heldByGrabbers.Count; x++)
                {
                    Grabber g = heldByGrabbers[x];

                    // Should we drop the item if it's too far away?
                    if (!recentlyTeleported && BreakDistance > 0 && Vector3.Distance(grabPosition, g.transform.position) > BreakDistance)
                    {
                        Debug.Log("Break Distance Exceeded. Dropping item.");
                        DropItem(g, true, true);
                        break;
                    }

                    // Should we drop the item if no longer holding the required Grabbable?
                    if (OtherGrabbableMustBeGrabbed != null && !OtherGrabbableMustBeGrabbed.BeingHeld)
                    {
                        // Fixed joints work ok. Configurable Joints have issues
                        if (GetComponent<ConfigurableJoint>() != null)
                        {
                            DropItem(g, true, true);
                            break;
                        }
                    }

                    // Fire off any relevant events
                    callEvents(g);
                }

                // Check to parent the hand models to the Grabbable
                if (ParentHandModel && !didParentHands)
                {
                    checkParentHands(GetPrimaryGrabber());
                }

                // Position Hands in proper place
                positionHandGraphics(GetPrimaryGrabber());

                // Rotate the grabber to look at our secondary object
                // JPTODO : Move this to physics updates
                if (TwoHandedRotation == TwoHandedRotationType.LookAtSecondary && GrabPhysics == GrabPhysics.PhysicsJoint)
                {
                    checkSecondaryLook();
                }

                // Keep track of where we were each frame
                previousPosition = transform.position;
            }
        }

        public virtual void FixedUpdate()
        {

            if (remoteGrabbing)
            {
                UpdateRemoteGrab();
            }

            if (BeingHeld)
            {

                // Reset all collisions every physics update
                // These are then populated in OnCollisionEnter / OnCollisionStay to make sure we have the most up to date collision info
                // This can create garbage so only do this if we are holding the object
                if (BeingHeld && collisions != null && collisions.Count > 0)
                {
                    collisions = new List<Collider>();
                }

                // Update any physics properties here
                if (GrabPhysics == GrabPhysics.PhysicsJoint)
                {
                    UpdatePhysicsJoints();
                }
                else if (GrabPhysics == GrabPhysics.FixedJoint)
                {
                    UpdateFixedJoints();
                }
                else if (GrabPhysics == GrabPhysics.Kinematic)
                {
                    UpdateKinematicPhysics();
                }
                else if (GrabPhysics == GrabPhysics.Velocity)
                {
                    UpdateVelocityPhysics();
                }
            }
        }

        public virtual Vector3 GetGrabberWithGrabPointOffset(Grabber grabber, Transform grabPoint)
        {
            // Sanity check
            if (grabber == null)
            {
                return Vector3.zero;
            }

            // Get the Grabber's position, offset by a grab point
            Vector3 grabberPosition = grabber.transform.position;
            if (grabPoint != null)
            {
                grabberPosition += transform.position - grabPoint.position;
            }

            return grabberPosition;

        }

        public virtual Quaternion GetGrabberWithOffsetWorldRotation(Grabber grabber)
        {

            if (grabber != null)
            {
                return grabber.transform.rotation;
            }

            return Quaternion.identity;
        }

        protected void positionHandGraphics(Grabber g)
        {
            if (ParentHandModel && didParentHands)
            {
                if (GrabMechanic == GrabType.Snap)
                {
                    if (g != null)
                    {
                        g.HandsGraphics.localPosition = g.handsGraphicsGrabberOffset;
                        g.HandsGraphics.localEulerAngles = Vector3.zero;
                    }
                }
            }
        }

        /// <summary>
        /// Is this object able to be grabbed. Does not check for valid Grabbers, only if it isn't being held, is active, etc.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsGrabbable()
        {

            // Not valid if not active
            if (!isActiveAndEnabled)
            {
                return false;
            }

            // Not valid if being held and the object has no secondary grab behavior
            if (BeingHeld == true && SecondaryGrabBehavior == OtherGrabBehavior.None)
            {
                return false;
            }

            // Not Grabbable if set as DualGrab, but secondary grabbable has been specified. This means we can't use a grab point on this object
            if (BeingHeld == true && SecondaryGrabBehavior == OtherGrabBehavior.DualGrab && SecondaryGrabbable != null)
            {
                return false;
            }

            // Make sure grabbed conditions are met
            if (OtherGrabbableMustBeGrabbed != null && !OtherGrabbableMustBeGrabbed.BeingHeld)
            {
                return false;
            }

            return true;
        }

        public virtual void UpdateRemoteGrab()
        {

            // Linear Movement
            if (RemoteGrabMechanic == RemoteGrabMovement.Linear)
            {
                CheckRemoteGrabLinear();
            }
            else if (RemoteGrabMechanic == RemoteGrabMovement.Velocity)
            {
                CheckRemoteGrabVelocity();
            }
            else if (RemoteGrabMechanic == RemoteGrabMovement.Flick)
            {
                CheckRemoteGrabFlick();
            }
        }

        public virtual void CheckRemoteGrabLinear()
        {
            // Bail early if we're not remote grabbing this item
            if (!remoteGrabbing)
            {
                return;
            }

            // Move the object linearly as a kinematic rigidbody
            if (rb && !rb.isKinematic)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.isKinematic = true;
            }

            Vector3 grabberPosition = GetGrabberWithGrabPointOffset(flyingTo, GetClosestGrabPoint(flyingTo));
            Quaternion remoteRotation = getRemoteRotation(flyingTo);
            float distance = Vector3.Distance(transform.position, grabberPosition);

            // reached destination, snap to final transform position
            // Typically this won't be hit as the Grabber trigger will pick it up first
            if (distance <= 0.002f)
            {
                movePosition(grabberPosition);
                moveRotation(grabTransform.rotation);

                SetRigidVelocity(Vector3.zero);

                if (flyingTo != null)
                {
                    flyingTo.GrabGrabbable(this);
                }
            }
            // Getting close so speed up
            else if (distance < 0.03f)
            {
                movePosition(Vector3.MoveTowards(transform.position, grabberPosition, Time.fixedDeltaTime * GrabSpeed * 2f));
                moveRotation(Quaternion.Slerp(transform.rotation, remoteRotation, Time.fixedDeltaTime * GrabSpeed * 2f));
            }
            // Normal Lerp
            else
            {
                movePosition(Vector3.Lerp(transform.position, grabberPosition, Time.fixedDeltaTime * GrabSpeed));
                moveRotation(Quaternion.Slerp(transform.rotation, remoteRotation, Time.fixedDeltaTime * GrabSpeed));
            }
        }

        public virtual void CheckRemoteGrabVelocity()
        {
            if (remoteGrabbing)
            {

                Vector3 grabberPosition = GetGrabberWithGrabPointOffset(flyingTo, GetClosestGrabPoint(flyingTo));
                Quaternion remoteRotation = getRemoteRotation(flyingTo);
                float distance = Vector3.Distance(transform.position, grabberPosition);

                // Move the object with velocity, without using gravity
                if (rb && rb.useGravity)
                {
                    rb.useGravity = false;

                    // Snap rotation once
                    // transform.rotation = remoteRotation;
                }

                // reached destination, snap to final transform position
                // Typically this won't be hit as the Grabber trigger will pick it up first
                if (distance <= 0.0025f)
                {
                    movePosition(grabberPosition);
                    moveRotation(grabTransform.rotation);

                    SetRigidVelocity(Vector3.zero);

                    if (flyingTo != null)
                    {
                        flyingTo.GrabGrabbable(this);
                    }
                }
                else
                {
                    // Move with velocity
                    Vector3 positionDelta = grabberPosition - transform.position;

                    // Move towards hand using velocity
                    SetRigidVelocity(Vector3.MoveTowards(rb.velocity, (positionDelta * MoveVelocityForce) * Time.fixedDeltaTime, 1f));

                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, GetGrabbersAveragedRotation(), Time.fixedDeltaTime * GrabSpeed));
                    //rigid.angularVelocity = Vector3.zero;
                    //moveRotation(Quaternion.Slerp(transform.rotation, remoteRotation, Time.fixedDeltaTime * GrabSpeed));
                }
            }
        }


        bool initiatedFlick = false;
        // Angular Velocity required to start the flick force
        float flickStartVelocity = 1.5f;

        /// <summary>
        /// How long in seconds the object should take to jump to the grabber when using the Flick remote grab type
        /// </summary>
        float FlickSpeed = 0.5f;

        public float lastFlickTime;

        public virtual void InitiateFlick()
        {

            initiatedFlick = true;

            lastFlickTime = Time.time;

            Vector3 grabberPosition = flyingTo.transform.position;// GetGrabberWithGrabPointOffset(flyingTo, GetClosestGrabPoint(flyingTo));
            Quaternion remoteRotation = getRemoteRotation(flyingTo);
            float distance = Vector3.Distance(transform.position, grabberPosition);

            float timeToGrab = FlickSpeed;
            if (distance < 1f)
            {
                timeToGrab = FlickSpeed / 1.5f;
            }
            else if (distance < 0.5f)
            {
                timeToGrab = FlickSpeed / 3;
            }

            Vector3 vel = GetVelocityToHitTargetByTime(transform.position, grabberPosition, Physics.gravity * 1.1f, timeToGrab);
            SetRigidVelocity(vel);
            initiatedFlick = false;
        }

        public Vector3 GetVelocityToHitTargetByTime(Vector3 startPosition, Vector3 targetPosition, Vector3 gravityBase, float timeToTarget)
        {

            Vector3 direction = targetPosition - startPosition;
            Vector3 horizontal = Vector3.Project(direction, Vector3.Cross(gravityBase, Vector3.Cross(direction, gravityBase)));

            float horizontalDistance = horizontal.magnitude;
            float horizontalSpeed = horizontalDistance / timeToTarget;

            Vector3 vertical = Vector3.Project(direction, gravityBase);
            float verticalDistance = vertical.magnitude * Mathf.Sign(Vector3.Dot(vertical, -gravityBase));
            float verticalSpeed = (verticalDistance + ((0.5f * gravityBase.magnitude) * (timeToTarget * timeToTarget))) / timeToTarget;

            return (horizontal.normalized * horizontalSpeed) - (gravityBase.normalized * verticalSpeed);
        }

        public virtual void CheckRemoteGrabFlick()
        {
            if (remoteGrabbing)
            {
                if (!initiatedFlick)
                {
                    if (XRInput.Instance.GetControllerAngularVelocity(flyingTo.HandSide).magnitude >= flickStartVelocity)
                    {
                        if (Time.time - lastFlickTime >= 0.1f)
                        {
                            InitiateFlick();
                        }
                    }
                }
            }
            else
            {
                initiatedFlick = false;
            }
        }

        public float FlickForce = 1f;

        public virtual void UpdateFixedJoints()
        {
            if (rb != null && rb.isKinematic)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            else
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if (ApplyCorrectiveForce)
            {
                moveWithVelocity();
            }
        }

        public virtual void UpdatePhysicsJoints()
        {
            if (connectedJoint == null || rb == null)
            {
                return;
            }

            if (rb.isKinematic)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            else
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if (GrabMechanic == GrabType.Snap)
            {
                connectedJoint.anchor = Vector3.zero;
                connectedJoint.connectedAnchor = GrabPositionOffset;
            }

            bool forceSpring = Time.time < requestSpringTime;
            bool afterCollision = collisions.Count == 0 && lastNoCollisionSeconds >= 0.1f;

            if ((BeingHeldWithTwoHands || afterCollision) && !forceSpring)
            {
                connectedJoint.rotationDriveMode = RotationDriveMode.Slerp;
                connectedJoint.xMotion = ConfigurableJointMotion.Limited;
                connectedJoint.yMotion = ConfigurableJointMotion.Limited;
                connectedJoint.zMotion = ConfigurableJointMotion.Limited;
                connectedJoint.angularXMotion = ConfigurableJointMotion.Limited;
                connectedJoint.angularYMotion = ConfigurableJointMotion.Limited;
                connectedJoint.angularZMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit sjl = connectedJoint.linearLimit;
                sjl.limit = 15f;

                SoftJointLimitSpring sjlsp = connectedJoint.linearLimitSpring;
                sjlsp.spring = 3000;
                sjlsp.damper = 10f;

                setPositionSpring(CollisionSpring, 10f);

                setSlerpDrive(CollisionSlerp, 10f);

                if (ApplyCorrectiveForce)
                {
                    moveWithVelocity();
                }
            }
            else
            {
                connectedJoint.rotationDriveMode = RotationDriveMode.Slerp;
                connectedJoint.xMotion = CollisionLinearMotionX;
                connectedJoint.yMotion = CollisionLinearMotionY;
                connectedJoint.zMotion = CollisionLinearMotionZ;
                connectedJoint.angularXMotion = CollisionAngularMotionX;
                connectedJoint.angularYMotion = CollisionAngularMotionY;
                connectedJoint.angularZMotion = CollisionAngularMotionZ;

                SoftJointLimitSpring sp = connectedJoint.linearLimitSpring;
                sp.spring = 5000;
                sp.damper = 5;

                setPositionSpring(CollisionSpring, 5f);
                setSlerpDrive(CollisionSlerp, 5f);
            }

            if (BeingHeldWithTwoHands && SecondaryLookAtTransform != null)
            {
                connectedJoint.angularXMotion = ConfigurableJointMotion.Free;

                setSlerpDrive(1000f, 2f);
                connectedJoint.angularYMotion = ConfigurableJointMotion.Limited;


                connectedJoint.angularZMotion = ConfigurableJointMotion.Limited;

                if (TwoHandedRotation == TwoHandedRotationType.LookAtSecondary)
                {
                    checkSecondaryLook();
                }
            }
        }

        void setPositionSpring(float spring, float damper)
        {

            if (connectedJoint == null)
            {
                return;
            }

            JointDrive xDrive = connectedJoint.xDrive;
            xDrive.positionSpring = spring;
            xDrive.positionDamper = damper;
            connectedJoint.xDrive = xDrive;

            JointDrive yDrive = connectedJoint.yDrive;
            yDrive.positionSpring = spring;
            yDrive.positionDamper = damper;
            connectedJoint.yDrive = yDrive;

            JointDrive zDrive = connectedJoint.zDrive;
            zDrive.positionSpring = spring;
            zDrive.positionDamper = damper;
            connectedJoint.zDrive = zDrive;
        }

        void setSlerpDrive(float slerp, float damper)
        {
            if (connectedJoint)
            {
                JointDrive slerpDrive = connectedJoint.slerpDrive;
                slerpDrive.positionSpring = slerp;
                slerpDrive.positionDamper = damper;
                connectedJoint.slerpDrive = slerpDrive;
            }
        }

        public virtual Vector3 GetGrabberVector3(Grabber grabber, bool isSecondary)
        {
            if (GrabMechanic == GrabType.Snap)
            {
                return GetGrabberWithGrabPointOffset(grabber, isSecondary ? secondaryGrabOffset : primaryGrabOffset);
            }
            else
            {
                if (isSecondary)
                {
                    return grabTransformSecondary.position;
                }

                return grabTransform.position;
            }
        }

        public virtual Quaternion GetGrabberQuaternion(Grabber grabber, bool isSecondary)
        {

            if (GrabMechanic == GrabType.Snap)
            {
                return GetGrabberWithOffsetWorldRotation(grabber);
            }
            else
            {
                if (isSecondary)
                {
                    return grabTransformSecondary.rotation;
                }

                return grabTransform.rotation;
            }
        }

        public virtual void SetRigidVelocity(Vector3 newVelocity)
        {
            if (rb == null || rb.isKinematic)
            {
                return;
            }
            else
            {
                rb.velocity = newVelocity;
            }
        }

        public virtual void SetRigidAngularVelocity(Vector3 newVelocity)
        {
            if (rb == null || rb.isKinematic)
            {
                return;
            }
            else
            {
                rb.angularVelocity = newVelocity;
            }
        }

        void moveWithVelocity()
        {

            if (rb == null) { return; }

            Vector3 destination = GetGrabbersAveragedPosition();

            float distance = Vector3.Distance(transform.position, destination);

            if (distance > 0.002f)
            {
                Vector3 positionDelta = destination - transform.position;

                SetRigidVelocity(Vector3.MoveTowards(rb.velocity, (positionDelta * MoveVelocityForce) * Time.fixedDeltaTime, 1f));
            }
            else
            {
                rb.MovePosition(destination);
                SetRigidVelocity(Vector3.zero);
            }
        }

        float angle;
        Vector3 axis, angularTarget, angularMovement;

        void rotateWithVelocity()
        {

            if (rb == null)
            {
                return;
            }

            bool noRecentCollisions = collisions != null && collisions.Count == 0 && lastNoCollisionSeconds >= 0.5f;
            bool moveInstantlyOneHand = InstantMovement;
            bool moveInstantlyTwoHands = BeingHeldWithTwoHands && InstantMovement;

            if (InstantMovement == true && noRecentCollisions && (moveInstantlyOneHand || moveInstantlyTwoHands))
            {
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, GetGrabbersAveragedRotation(), Time.fixedDeltaTime * SecondHandLookSpeed));

                return;
            }

            Quaternion rotationDelta = GetGrabbersAveragedRotation() * Quaternion.Inverse(transform.rotation);
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
            {
                angle -= 360;
            }

            if (angle != 0)
            {
                angularTarget = angle * axis;
                angularTarget = (angularTarget * MoveAngularVelocityForce) * Time.fixedDeltaTime;

                angularMovement = Vector3.MoveTowards(rb.angularVelocity, angularTarget, MoveAngularVelocityForce);

                if (angularMovement.magnitude > 0.05f)
                {
                    rb.angularVelocity = angularMovement;
                }

                if (angle < 1)
                {
                    rb.MoveRotation(GetGrabbersAveragedRotation());
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        public Vector3 GetGrabbersAveragedPosition()
        {
            Vector3 destination = GetGrabberVector3(GetPrimaryGrabber(), false);

            if (SecondaryGrabBehavior == OtherGrabBehavior.DualGrab && TwoHandedPosition == TwoHandedPositionType.Lerp)
            {
                if (SecondaryGrabbable != null && SecondaryGrabbable.BeingHeld)
                {
                    destination = Vector3.Lerp(destination, SecondaryGrabbable.GetGrabberVector3(SecondaryGrabbable.GetPrimaryGrabber(), false), TwoHandedPostionLerpAmount);
                }
                else if (heldByGrabbers != null && heldByGrabbers.Count > 1)
                {
                    destination = Vector3.Lerp(destination, GetGrabberVector3(heldByGrabbers[1], true), TwoHandedPostionLerpAmount);
                }
            }

            return destination;
        }

        public Quaternion GetGrabbersAveragedRotation()
        {
            Quaternion destination = GetGrabberQuaternion(GetPrimaryGrabber(), false);

            if (SecondaryGrabBehavior == OtherGrabBehavior.DualGrab && TwoHandedRotation == TwoHandedRotationType.Lerp || TwoHandedRotation == TwoHandedRotationType.Slerp)
            {
                if (SecondaryGrabbable != null && SecondaryGrabbable.BeingHeld)
                {
                    if (TwoHandedRotation == TwoHandedRotationType.Lerp)
                    {
                        destination = Quaternion.Lerp(destination, SecondaryGrabbable.GetGrabberQuaternion(SecondaryGrabbable.GetPrimaryGrabber(), false), TwoHandedRotationLerpAmount);
                    }
                    else
                    {
                        destination = Quaternion.Slerp(destination, SecondaryGrabbable.GetGrabberQuaternion(SecondaryGrabbable.GetPrimaryGrabber(), false), TwoHandedRotationLerpAmount);
                    }
                }
                else if (heldByGrabbers != null && heldByGrabbers.Count > 1)
                {
                    if (TwoHandedRotation == TwoHandedRotationType.Lerp)
                    {
                        destination = Quaternion.Lerp(destination, GetGrabberQuaternion(heldByGrabbers[1], true), TwoHandedRotationLerpAmount);
                    }
                    else
                    {
                        destination = Quaternion.Slerp(destination, GetGrabberQuaternion(heldByGrabbers[1], true), TwoHandedRotationLerpAmount);
                    }
                }
            }
            else if (SecondaryGrabBehavior == OtherGrabBehavior.DualGrab && TwoHandedRotation == TwoHandedRotationType.LookAtSecondary)
            {
                if (SecondaryGrabbable != null && SecondaryGrabbable.BeingHeld)
                {

                    Vector3 targetVector = GetGrabberVector3(SecondaryGrabbable.GetPrimaryGrabber(), false) - GetGrabberVector3(GetPrimaryGrabber(), false);

                    if (TwoHandedLookVector == TwoHandedLookDirection.Horizontal)
                    {
                        destination = Quaternion.LookRotation(targetVector, -GetPrimaryGrabber().transform.up) * Quaternion.AngleAxis(180f, Vector3.up) * Quaternion.AngleAxis(180f, Vector3.forward);
                    }
                    else if (TwoHandedLookVector == TwoHandedLookDirection.Vertical)
                    {
                        destination = Quaternion.LookRotation(targetVector, -GetPrimaryGrabber().transform.right) * Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.forward) * Quaternion.AngleAxis(-90f, Vector3.up);
                    }
                }
                else if (heldByGrabbers != null && heldByGrabbers.Count > 1) { }
            }

            return destination;
        }

        public virtual void UpdateKinematicPhysics()
        {

            float distCovered = (Time.time - LastGrabTime) * GrabSpeed;
            float fractionOfJourney = distCovered / journeyLength;

            Vector3 destination = GetGrabbersAveragedPosition();
            Quaternion destRotation = grabTransform.rotation;

            bool realtime = Application.isEditor;
            if (realtime)
            {
                destination = getRemotePosition(GetPrimaryGrabber());
                rotateGrabber(false);
            }

            if (GrabMechanic == GrabType.Snap)
            {
                Grabber g = GetPrimaryGrabber();

                if (g != null)
                {
                    if (ParentToHands)
                    {
                        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero - GrabPositionOffset, fractionOfJourney);
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, grabTransform.localRotation, Time.deltaTime * 10);
                    }
                    else
                    {
                        movePosition(Vector3.Lerp(transform.position, destination, fractionOfJourney));
                        moveRotation(Quaternion.Lerp(transform.rotation, destRotation, Time.deltaTime * 20));
                    }
                }
                else
                {
                    movePosition(destination);
                    transform.localRotation = grabTransform.localRotation;
                }
            }
            else if (GrabMechanic == GrabType.Precise)
            {
                movePosition(grabTransform.position);
                moveRotation(grabTransform.rotation);
            }
        }

        public virtual void UpdateVelocityPhysics()
        {

            if (connectedJoint != null)
            {
                connectedJoint.xMotion = ConfigurableJointMotion.Free;
                connectedJoint.yMotion = ConfigurableJointMotion.Free;
                connectedJoint.zMotion = ConfigurableJointMotion.Free;
                connectedJoint.angularXMotion = ConfigurableJointMotion.Free;
                connectedJoint.angularYMotion = ConfigurableJointMotion.Free;
                connectedJoint.angularZMotion = ConfigurableJointMotion.Free;
            }

            setPositionSpring(0, 0.5f);
            setSlerpDrive(5, 0.5f);

            if (rb && rb.isKinematic)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            else if (rb)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            moveWithVelocity();
            rotateWithVelocity();

            if (ParentToHands)
            {
                bool afterCollision = collisions.Count == 0 && lastNoCollisionSeconds >= 0.2f;
                if (afterCollision)
                {
                    Grabber g = GetPrimaryGrabber();
                    transform.parent = g.transform;
                }
                else
                {
                    transform.parent = null;
                }
            }
        }

        void checkParentHands(Grabber g)
        {

            if (ParentHandModel && g != null)
            {

                if (GrabMechanic == GrabType.Precise)
                {
                    parentHandGraphics(g);
                }
                else
                {
                    Vector3 grabberPosition = grabTransform.position;
                    Vector3 grabbablePosition = transform.position;

                    float distance = Vector3.Distance(grabbablePosition, grabberPosition);

                    if (CanBeMoved)
                    {
                        if (distance < 0.001f)
                        {
                            parentHandGraphics(g);

                            if (g.HandsGraphics != null)
                            {
                                g.HandsGraphics.localEulerAngles = Vector3.zero;
                                g.HandsGraphics.localPosition = g.handsGraphicsGrabberOffset;
                            }
                        }
                    }
                    else
                    {
                        if (grabTransform != null && distance < 0.1f)
                        {
                            parentHandGraphics(g);
                            positionHandGraphics(g);

                            if (g.HandsGraphics != null)
                            {
                                g.HandsGraphics.localEulerAngles = Vector3.zero;
                                g.HandsGraphics.localPosition = g.handsGraphicsGrabberOffset;
                            }
                        }
                    }
                }
            }
        }

        bool canBeMoved()
        {

            if (GetComponent<Rigidbody>() == null)
            {
                return false;
            }

            if (GetComponent<Joint>() != null)
            {
                return false;
            }

            return true;
        }

        void checkSecondaryLook()
        {
            if (BeingHeldWithTwoHands)
            {
                if (SecondaryLookAtTransform == null)
                {
                    Grabber thisGrabber = GetPrimaryGrabber();
                    Grabber secondaryGrabber = SecondaryGrabbable.GetPrimaryGrabber();
                    GameObject o = new GameObject();
                    SecondaryLookAtTransform = o.transform;
                    SecondaryLookAtTransform.name = "LookAtTransformTemp";

                    if (SecondaryGrabbable.GrabMechanic == GrabType.Precise)
                    {
                        SecondaryLookAtTransform.position = secondaryGrabber.transform.position;
                    }
                    else
                    {
                        Transform grabPoint = SecondaryGrabbable.GetGrabPoint();
                        if (grabPoint)
                        {
                            SecondaryLookAtTransform.position = grabPoint.position;
                        }
                        else
                        {
                            SecondaryLookAtTransform.position = SecondaryGrabbable.transform.position;
                        }

                        SecondaryLookAtTransform.position = SecondaryGrabbable.transform.position;
                    }

                    if (SecondaryLookAtTransform && thisGrabber)
                    {
                        SecondaryLookAtTransform.parent = thisGrabber.transform;
                        SecondaryLookAtTransform.localEulerAngles = Vector3.zero;
                        SecondaryLookAtTransform.localPosition = new Vector3(0, 0, SecondaryLookAtTransform.localPosition.z);
                        SecondaryLookAtTransform.parent = secondaryGrabber.transform;
                    }
                }
            }

            if (SecondaryGrabbable != null && !SecondaryGrabbable.BeingHeld && SecondaryLookAtTransform != null)
            {
                clearLookAtTransform();
            }

            Grabber heldBy = GetPrimaryGrabber();
            if (heldBy)
            {
                Transform grabberTransform = heldBy.transform;

                if (SecondaryLookAtTransform != null)
                {
                    Vector3 initialRotation = grabberTransform.localEulerAngles;
                    Quaternion dest = Quaternion.LookRotation(SecondaryLookAtTransform.position - grabberTransform.position, Vector3.up);
                    grabberTransform.rotation = Quaternion.Slerp(grabberTransform.rotation, dest, Time.deltaTime * SecondHandLookSpeed);
                    grabberTransform.localEulerAngles = new Vector3(grabberTransform.localEulerAngles.x, grabberTransform.localEulerAngles.y, initialRotation.z);
                }
                else
                {
                    rotateGrabber(true);
                }
            }
        }

        void rotateGrabber(bool lerp = false)
        {
            Grabber heldBy = GetPrimaryGrabber();
            if (heldBy != null)
            {
                Transform grabberTransform = heldBy.transform;

                if (lerp)
                {
                    grabberTransform.localRotation = Quaternion.Slerp(grabberTransform.localRotation, Quaternion.Inverse(Quaternion.Euler(GrabRotationOffset)), Time.deltaTime * 20);
                }
                else
                {
                    grabberTransform.localRotation = Quaternion.Inverse(Quaternion.Euler(GrabRotationOffset));
                }
            }
        }

        public Transform GetGrabPoint()
        {
            return primaryGrabOffset;
        }

        public virtual void GrabItem(Grabber grabbedBy)
        {

            if (BeingHeld && SecondaryGrabBehavior != OtherGrabBehavior.DualGrab)
            {
                DropItem(false, true);
            }

            bool isPrimaryGrab = !BeingHeld;
            bool isSecondaryGrab = BeingHeld && SecondaryGrabBehavior == OtherGrabBehavior.DualGrab;

            BeingHeld = true;
            LastGrabTime = Time.time;

            if (isPrimaryGrab)
            {
                ResetGrabbing();

                primaryGrabOffset = GetClosestGrabPoint(grabbedBy);
                secondaryGrabOffset = null;

                if (primaryGrabOffset)
                {
                    ActiveGrabPoint = primaryGrabOffset.GetComponent<GrabPoint>();
                }
                else
                {
                    ActiveGrabPoint = null;
                }
                if (primaryGrabOffset != null && ActiveGrabPoint != null)
                {
                    CustomHandPose = primaryGrabOffset.GetComponent<GrabPoint>().HandPose;
                    SelectedHandPose = primaryGrabOffset.GetComponent<GrabPoint>().SelectedHandPose;
                    handPoseType = primaryGrabOffset.GetComponent<GrabPoint>().handPoseType;
                }
                else
                {
                    CustomHandPose = initialHandPoseId;
                    SelectedHandPose = initialHandPose;
                    handPoseType = initialHandPoseType;
                }

                // Update held by properties
                addGrabber(grabbedBy);
                grabTransform.parent = grabbedBy.transform;
                rotateGrabber(false);

                // Use center of grabber if snapping
                if (GrabMechanic == GrabType.Snap)
                {
                    grabTransform.localEulerAngles = Vector3.zero;
                    grabTransform.localPosition = -GrabPositionOffset;
                }
                else if (GrabMechanic == GrabType.Precise)
                {
                    grabTransform.position = transform.position;
                    grabTransform.rotation = transform.rotation;
                }

                var projectile = GetComponent<Projectile>();
                if (projectile)
                {
                    var fj = GetComponent<FixedJoint>();
                    if (fj)
                    {
                        Destroy(fj);
                    }
                }

                if (GrabPhysics == GrabPhysics.PhysicsJoint)
                {
                    setupConfigJointGrab(grabbedBy, GrabMechanic);
                }
                else if (GrabPhysics == GrabPhysics.Velocity)
                {
                    setupVelocityGrab(grabbedBy, GrabMechanic);
                }
                else if (GrabPhysics == GrabPhysics.FixedJoint)
                {
                    setupFixedJointGrab(grabbedBy, GrabMechanic);
                }
                else if (GrabPhysics == GrabPhysics.Kinematic)
                {
                    setupKinematicGrab(grabbedBy, GrabMechanic);
                }

                if (rb && !rb.isKinematic)
                {


                    SetRigidVelocity(Vector3.zero);
                    SetRigidAngularVelocity(Vector3.zero);
                }

                for (int x = 0; x < events.Count; x++)
                {
                    events[x].OnGrab(grabbedBy);
                }

                checkParentHands(grabbedBy);

                if (GrabMechanic == GrabType.Precise && SnapHandModel && primaryGrabOffset != null && grabbedBy.HandsGraphics != null)
                {
                    grabbedBy.HandsGraphics.transform.parent = primaryGrabOffset;
                    grabbedBy.HandsGraphics.localPosition = grabbedBy.handsGraphicsGrabberOffset;
                    grabbedBy.HandsGraphics.localEulerAngles = grabbedBy.handsGraphicsGrabberOffsetRotation;
                }

                SubscribeToMoveEvents();

            }
            else if (isSecondaryGrab)
            {
                secondaryGrabOffset = GetClosestGrabPoint(grabbedBy);
                addGrabber(grabbedBy);
                grabTransformSecondary.parent = grabbedBy.transform;

                if (GrabMechanic == GrabType.Snap)
                {
                    grabTransformSecondary.localEulerAngles = Vector3.zero;
                    grabTransformSecondary.localPosition = GrabPositionOffset;
                }
                else if (GrabMechanic == GrabType.Precise)
                {
                    grabTransformSecondary.position = transform.position;
                    grabTransformSecondary.rotation = transform.rotation;
                }

                checkParentHands(grabbedBy);

                if (GrabMechanic == GrabType.Precise && SnapHandModel && secondaryGrabOffset != null && grabbedBy.HandsGraphics != null)
                {
                    grabbedBy.HandsGraphics.transform.parent = secondaryGrabOffset;
                    grabbedBy.HandsGraphics.localPosition = grabbedBy.handsGraphicsGrabberOffset;
                    grabbedBy.HandsGraphics.localEulerAngles = grabbedBy.handsGraphicsGrabberOffsetRotation;
                }
            }

            if (HideHandGraphics)
            {
                grabbedBy.HideHandGraphics();
            }

            journeyLength = Vector3.Distance(grabPosition, grabbedBy.transform.position);
        }

        protected virtual void setupConfigJointGrab(Grabber grabbedBy, GrabType grabType)
        {
            if (GrabMechanic == GrabType.Precise)
            {
                connectedJoint = grabbedBy.GetComponent<ConfigurableJoint>();
                connectedJoint.connectedBody = rb;
                connectedJoint.autoConfigureConnectedAnchor = true;
            }

            else if (GrabMechanic == GrabType.Snap)
            {
                transform.rotation = grabTransform.rotation;

                setupConfigJoint(grabbedBy);

                rb.MoveRotation(grabTransform.rotation);
            }
        }

        protected virtual void setupFixedJointGrab(Grabber grabbedBy, GrabType grabType)
        {
            FixedJoint joint = grabbedBy.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = rb;

            if (GrabMechanic == GrabType.Precise)
            {
                joint.autoConfigureConnectedAnchor = true;
            }
            else if (GrabMechanic == GrabType.Snap)
            {
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = GrabPositionOffset;
            }
        }

        protected virtual void setupKinematicGrab(Grabber grabbedBy, GrabType grabType)
        {
            if (ParentToHands)
            {
                transform.parent = grabbedBy.transform;
            }

            if (rb != null)
            {

                if (rb.collisionDetectionMode == CollisionDetectionMode.Continuous || rb.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic)
                {
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
                rb.isKinematic = true;
            }
        }

        protected virtual void setupVelocityGrab(Grabber grabbedBy, GrabType grabType)
        {
            bool addJointToVelocityGrabbable = false;
            if (addJointToVelocityGrabbable)
            {
                if (GrabMechanic == GrabType.Precise)
                {
                    connectedJoint = grabbedBy.GetComponent<ConfigurableJoint>();
                    connectedJoint.connectedBody = rb;
                    connectedJoint.autoConfigureConnectedAnchor = true;
                }
                else if (GrabMechanic == GrabType.Snap)
                {
                    transform.rotation = grabTransform.rotation;
                    setupConfigJoint(grabbedBy);
                    rb.MoveRotation(grabTransform.rotation);
                }
            }

            rb.useGravity = false;
        }

        public virtual void GrabRemoteItem(Grabber grabbedBy)
        {
            flyingTo = grabbedBy;
            grabTransform.parent = grabbedBy.transform;
            grabTransform.localEulerAngles = Vector3.zero;
            grabTransform.localPosition = -GrabPositionOffset;

            grabTransform.localEulerAngles = GrabRotationOffset;

            remoteGrabbing = true;
        }

        public virtual void ResetGrabbing()
        {
            if (rb)
            {
                rb.isKinematic = wasKinematic;
            }

            flyingTo = null;

            remoteGrabbing = false;

            collisions = new List<Collider>();
        }

        public virtual void DropItem(Grabber droppedBy, bool resetVelocity, bool resetParent)
        {

            if (heldByGrabbers == null)
            {
                BeingHeld = false;
                return;
            }

            bool isPrimaryGrabber = droppedBy == GetPrimaryGrabber();
            bool isSecondaryGrabber = !isPrimaryGrabber && heldByGrabbers.Count > 1;

            if (isPrimaryGrabber)
            {
                bool wasHeldWithTwoHands = BeingHeldWithTwoHands;
                bool releaseItem = true;

                if (resetParent)
                {
                    ResetParent();
                }

                removeConfigJoint();

                if (GrabPhysics == GrabPhysics.FixedJoint && droppedBy != null)
                {
                    FixedJoint joint = droppedBy.gameObject.GetComponent<FixedJoint>();
                    if (joint)
                    {
                        GameObject.Destroy(joint);
                    }
                }

                if (droppedBy)
                {
                    droppedBy.DidDrop();
                }

                UnsubscribeFromMoveEvents();
                primaryGrabOffset = null;
                clearLookAtTransform();
                removeGrabber(droppedBy);
                didParentHands = false;

                if (wasHeldWithTwoHands)
                {
                    if (TwoHandedDropBehavior == TwoHandedDropMechanic.Drop)
                    {
                        if (SecondaryGrabbable != null && SecondaryGrabbable.BeingHeld)
                        {
                            SecondaryGrabbable.DropItem(false, false);
                        }
                        else
                        {
                            DropItem(heldByGrabbers[0]);
                        }
                    }
                    else if (TwoHandedDropBehavior == TwoHandedDropMechanic.Transfer)
                    {
                        releaseItem = false;

                        var newGrabber = heldByGrabbers[0];
                        Vector3 localHandsPos = Vector3.zero;
                        Vector3 localHandsRot = Vector3.zero;

                        if (newGrabber.HandsGraphics != null)
                        {
                            Transform prev = newGrabber.HandsGraphics.parent;
                            newGrabber.HandsGraphics.parent = transform;
                            localHandsPos = newGrabber.HandsGraphics.localPosition;
                            localHandsRot = newGrabber.HandsGraphics.localEulerAngles;
                            newGrabber.HandsGraphics.parent = prev;
                        }

                        DropItem(newGrabber);
                        newGrabber.GrabGrabbable(this);

                        if (newGrabber.HandsGraphics != null && ParentHandModel == true && GrabMechanic == GrabType.Precise)
                        {
                            Transform prev = newGrabber.HandsGraphics.parent;
                            newGrabber.HandsGraphics.parent = transform;
                            newGrabber.HandsGraphics.localPosition = localHandsPos;
                            newGrabber.HandsGraphics.localEulerAngles = localHandsRot;
                            newGrabber.HandsGraphics.parent = prev;
                        }
                    }
                }
                if (releaseItem)
                {

                    BeingHeld = false;

                    LastDropTime = Time.time;

                    if (rb != null && GrabPhysics != GrabPhysics.None)
                    {
                        rb.isKinematic = wasKinematic;
                        rb.useGravity = usedGravity;
                        rb.interpolation = initialInterpolationMode;
                        rb.collisionDetectionMode = initialCollisionMode;
                    }

                    if (ForceDisableKinematicOnDrop)
                    {
                        rb.isKinematic = false;
                        if (rb.constraints == RigidbodyConstraints.FreezeAll)
                        {
                            rb.constraints = RigidbodyConstraints.None;
                        }
                    }

                    // On release event
                    if (events != null)
                    {
                        for (int x = 0; x < events.Count; x++)
                        {
                            events[x].OnRelease();
                        }
                    }

                    CustomHandPose = initialHandPoseId;
                    SelectedHandPose = initialHandPose;
                    handPoseType = initialHandPoseType;

                    if (rb && resetVelocity && droppedBy && AddControllerVelocityOnDrop && GrabPhysics != GrabPhysics.None)
                    {
                        Vector3 velocity = droppedBy.GetGrabberAveragedVelocity() + droppedBy.GetComponent<Rigidbody>().velocity;
                        Vector3 angularVelocity = droppedBy.GetGrabberAveragedAngularVelocity() + droppedBy.GetComponent<Rigidbody>().angularVelocity;

                        if (gameObject.activeSelf)
                        {
                            Release(velocity, angularVelocity);
                        }
                    }
                }
            }
            else if (isSecondaryGrabber)
            {
                if (droppedBy)
                {
                    droppedBy.DidDrop();
                }

                removeGrabber(droppedBy);

                secondaryGrabOffset = null;
            }

            BeingHeld = heldByGrabbers != null && heldByGrabbers.Count > 0;
        }

        void clearLookAtTransform()
        {
            if (SecondaryLookAtTransform != null && SecondaryLookAtTransform.transform.name == "LookAtTransformTemp")
            {
                GameObject.Destroy(SecondaryLookAtTransform.gameObject);
            }

            SecondaryLookAtTransform = null;
        }

        void callEvents(Grabber g)
        {
            if (events.Any())
            {
                ControllerHand hand = g.HandSide;

                if (hand == ControllerHand.Right)
                {
                    foreach (var e in events)
                    {
                        e.OnGrip(input.RightGrip);
                        e.OnTrigger(input.RightTrigger);

                        if (input.RightTriggerUp)
                        {
                            e.OnTriggerUp();
                        }
                        if (input.RightTriggerDown)
                        {
                            e.OnTriggerDown();
                        }
                        if (input.AButton)
                        {
                            e.OnButton1();
                        }
                        if (input.AButtonDown)
                        {
                            e.OnButton1Down();
                        }
                        if (input.AButtonUp)
                        {
                            e.OnButton1Up();
                        }
                        if (input.BButton)
                        {
                            e.OnButton2();
                        }
                        if (input.BButtonDown)
                        {
                            e.OnButton2Down();
                        }
                        if (input.BButtonUp)
                        {
                            e.OnButton2Up();
                        }
                    }
                }

                if (hand == ControllerHand.Left)
                {
                    for (int x = 0; x < events.Count; x++)
                    {
                        GrabbableEvents e = events[x];
                        e.OnGrip(input.LeftGrip);
                        e.OnTrigger(input.LeftTrigger);

                        if (input.LeftTriggerUp)
                        {
                            e.OnTriggerUp();
                        }
                        if (input.LeftTriggerDown)
                        {
                            e.OnTriggerDown();
                        }
                        if (input.XButton)
                        {
                            e.OnButton1();
                        }
                        if (input.XButtonDown)
                        {
                            e.OnButton1Down();
                        }
                        if (input.XButtonUp)
                        {
                            e.OnButton1Up();
                        }
                        if (input.YButton)
                        {
                            e.OnButton2();
                        }
                        if (input.YButtonDown)
                        {
                            e.OnButton2Down();
                        }
                        if (input.YButtonUp)
                        {
                            e.OnButton2Up();
                        }
                    }
                }
            }
        }

        public virtual void DropItem(Grabber droppedBy)
        {
            DropItem(droppedBy, true, true);
        }

        public virtual void DropItem(bool resetVelocity, bool resetParent)
        {
            DropItem(GetPrimaryGrabber(), resetVelocity, resetParent);
        }

        public void ResetScale()
        {
            transform.localScale = OriginalScale;
        }

        public void ResetParent()
        {
            transform.parent = originalParent;
        }

        public void UpdateOriginalParent(Transform newOriginalParent)
        {
            originalParent = newOriginalParent;
        }

        public void UpdateOriginalParent()
        {
            UpdateOriginalParent(transform.parent);
        }

        public ControllerHand GetControllerHand(Grabber g)
        {
            if (g != null)
            {
                return g.HandSide;
            }

            return ControllerHand.None;
        }

        public virtual Grabber GetPrimaryGrabber()
        {
            if (heldByGrabbers != null)
            {
                for (int x = 0; x < heldByGrabbers.Count; x++)
                {
                    if (heldByGrabbers[x] != null && heldByGrabbers[x].HeldGrabbable == this)
                    {
                        return heldByGrabbers[x];
                    }
                }
            }

            return null;
        }

        public virtual Grabber GetClosestGrabber()
        {

            Grabber closestGrabber = null;
            float lastDistance = 9999;

            if (validGrabbers != null)
            {

                for (int x = 0; x < validGrabbers.Count; x++)
                {
                    Grabber g = validGrabbers[x];
                    if (g != null)
                    {
                        float dist = Vector3.Distance(grabPosition, g.transform.position);
                        if (dist < lastDistance)
                        {
                            closestGrabber = g;
                        }
                    }
                }
            }

            return closestGrabber;
        }

        public virtual Transform GetClosestGrabPoint(Grabber grabber)
        {
            Transform grabPoint = null;
            float lastDistance = 9999;
            float lastAngle = 360;
            if (GrabPoints != null)
            {
                int grabCount = GrabPoints.Count;
                for (int x = 0; x < grabCount; x++)
                {
                    Transform g = GrabPoints[x];

                    if (g == null)
                    {
                        continue;
                    }

                    float thisDist = Vector3.Distance(g.transform.position, grabber.transform.position);
                    if (thisDist <= lastDistance)
                    {

                        GrabPoint gp = g.GetComponent<GrabPoint>();
                        if (gp)
                        {

                            if ((grabber.HandSide == ControllerHand.Left && !gp.LeftHandIsValid) || (grabber.HandSide == ControllerHand.Right && !gp.RightHandIsValid))
                            {
                                continue;
                            }

                            float currentAngle = Quaternion.Angle(grabber.transform.rotation, g.transform.rotation);
                            if (currentAngle > gp.MaxDegreeDifferenceAllowed)
                            {
                                continue;
                            }

                            if (currentAngle > lastAngle && gp.MaxDegreeDifferenceAllowed != 360)
                            {
                                continue;
                            }

                            lastAngle = currentAngle;
                        }

                        grabPoint = g;
                        lastDistance = thisDist;
                    }
                }
            }

            return grabPoint;
        }

        public virtual void Release(Vector3 velocity, Vector3 angularVelocity)
        {
            Vector3 releaseVelocity = velocity * ThrowForceMultiplier;

            if (float.IsInfinity(releaseVelocity.x) || float.IsNaN(releaseVelocity.x))
            {
                return;
            }

            SetRigidVelocity(releaseVelocity);
            SetRigidAngularVelocity(angularVelocity);
        }

        public virtual bool IsValidCollision(Collision collision)
        {
            return IsValidCollision(collision.collider);
        }

        public virtual bool IsValidCollision(Collider col)
        {

            string transformName = col.transform.name;
            if (transformName.Contains("Projectile") || transformName.Contains("Bullet") || transformName.Contains("Clip"))
            {
                return false;
            }

            if (transformName.Contains("Joint"))
            {
                return false;
            }

            CharacterController cc = col.gameObject.GetComponent<CharacterController>();
            if (cc && col)
            {
                Physics.IgnoreCollision(col, cc, true);
                return false;
            }

            return true;
        }

        public virtual void parentHandGraphics(Grabber grab)
        {
            if (grab.HandsGraphics != null)
            {
                if (primaryGrabOffset != null)
                {
                    grab.HandsGraphics.transform.parent = primaryGrabOffset;
                    didParentHands = true;
                }
                else
                {
                    grab.HandsGraphics.transform.parent = transform;
                    didParentHands = true;
                }
            }
        }

        void setupConfigJoint(Grabber g)
        {
            connectedJoint = g.GetComponent<ConfigurableJoint>();
            connectedJoint.autoConfigureConnectedAnchor = false;
            connectedJoint.connectedBody = rb;
            connectedJoint.anchor = Vector3.zero;
            connectedJoint.connectedAnchor = GrabPositionOffset;
        }

        void removeConfigJoint()
        {
            if (connectedJoint != null)
            {
                connectedJoint.anchor = Vector3.zero;
                connectedJoint.connectedBody = null;
            }
        }

        void addGrabber(Grabber g)
        {
            if (heldByGrabbers == null)
            {
                heldByGrabbers = new List<Grabber>();
            }

            if (!heldByGrabbers.Contains(g))
            {
                heldByGrabbers.Add(g);
            }
        }

        void removeGrabber(Grabber g)
        {
            if (heldByGrabbers == null)
            {
                heldByGrabbers = new List<Grabber>();
            }
            else if (heldByGrabbers.Contains(g))
            {
                heldByGrabbers.Remove(g);
            }

            Grabber removeGrabber = null;
            for (int x = 0; x < heldByGrabbers.Count; x++)
            {
                Grabber grab = heldByGrabbers[x];
                if (grab.HeldGrabbable == null || grab.HeldGrabbable != this)
                {
                    removeGrabber = grab;
                }
            }

            if (removeGrabber)
            {
                heldByGrabbers.Remove(removeGrabber);
            }
        }

        void movePosition(Vector3 worldPosition)
        {
            if (rb)
            {
                rb.MovePosition(worldPosition);
            }
            else
            {
                transform.position = worldPosition;
            }
        }

        void moveRotation(Quaternion worldRotation)
        {
            if (rb)
            {
                rb.MoveRotation(worldRotation);
            }
            else
            {
                transform.rotation = worldRotation;
            }
        }

        protected Vector3 getRemotePosition(Grabber toGrabber)
        {
            return GetGrabberWithGrabPointOffset(toGrabber, GetClosestGrabPoint(toGrabber));
        }

        protected Quaternion getRemoteRotation(Grabber grabber)
        {

            if (grabber != null)
            {
                Transform point = GetClosestGrabPoint(grabber);
                if (point)
                {
                    Quaternion originalRot = grabTransform.rotation;
                    grabTransform.localRotation *= Quaternion.Inverse(point.localRotation);
                    Quaternion result = grabTransform.rotation;

                    grabTransform.rotation = originalRot;

                    return result;
                }
            }

            return grabTransform.rotation;
        }

        void filterCollisions()
        {
            for (int x = 0; x < collisions.Count; x++)
            {
                if (collisions[x] == null || !collisions[x].enabled || !collisions[x].gameObject.activeSelf)
                {
                    collisions.Remove(collisions[x]);
                    break;
                }
            }
        }

        public virtual PlayerController GetPlayerController()
        {

            if (_player != null)
            {
                return _player;
            }

            if (GameObject.FindGameObjectWithTag("Player"))
            {
                return _player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
            }
            else
            {
                return _player = FindObjectOfType<PlayerController>();
            }
        }

        public virtual void RequestSpringTime(float seconds)
        {
            float requested = Time.time + seconds;

            if (requested > requestSpringTime)
            {
                requestSpringTime = requested;
            }
        }

        public virtual void AddValidGrabber(Grabber grabber)
        {

            if (validGrabbers == null)
            {
                validGrabbers = new List<Grabber>();
            }

            if (!validGrabbers.Contains(grabber))
            {
                validGrabbers.Add(grabber);
            }
        }

        public virtual void RemoveValidGrabber(Grabber grabber)
        {
            if (validGrabbers != null && validGrabbers.Contains(grabber))
            {
                validGrabbers.Remove(grabber);
            }
        }

        bool subscribedToEvents = false;
        bool grabbableIsLocked = false;

        public virtual void SubscribeToMoveEvents()
        {

            if (!CanBeMoved || subscribedToEvents == true || GrabPhysics == GrabPhysics.None)
            {
                return;
            }

            PlayerRotation.OnBeforeRotate += LockGrabbableWithRotation;
            PlayerRotation.OnAfterRotate += UnlockGrabbable;

            if (GrabPhysics == GrabPhysics.Velocity || GrabPhysics == GrabPhysics.PhysicsJoint)
            {
                SmoothLocomotion.OnBeforeMove += LockGrabbable;
                SmoothLocomotion.OnAfterMove += UnlockGrabbable;
            }

            if (GrabPhysics == GrabPhysics.Kinematic && ParentToHands == true)
            {
                SmoothLocomotion.OnBeforeMove += LockGrabbableWithRotation;
                SmoothLocomotion.OnAfterMove += UnlockGrabbable;
            }
            else if (GrabPhysics == GrabPhysics.Kinematic && ParentToHands == false)
            {
                SmoothLocomotion.OnBeforeMove += LockGrabbable;
                SmoothLocomotion.OnAfterMove += UnlockGrabbable;
            }

            subscribedToEvents = true;
        }

        public virtual void UnsubscribeFromMoveEvents()
        {
            if (subscribedToEvents)
            {
                PlayerRotation.OnBeforeRotate -= LockGrabbableWithRotation;
                PlayerRotation.OnAfterRotate -= UnlockGrabbable;

                if (GrabPhysics == GrabPhysics.Velocity || GrabPhysics == GrabPhysics.PhysicsJoint)
                {
                    SmoothLocomotion.OnBeforeMove -= LockGrabbable;
                    SmoothLocomotion.OnAfterMove -= UnlockGrabbable;
                }

                if (GrabPhysics == GrabPhysics.Kinematic && ParentToHands == true)
                {
                    SmoothLocomotion.OnBeforeMove -= LockGrabbableWithRotation;
                    SmoothLocomotion.OnAfterMove -= UnlockGrabbable;
                }
                else if (GrabPhysics == GrabPhysics.Kinematic && ParentToHands == false)
                {
                    SmoothLocomotion.OnBeforeMove -= LockGrabbable;
                    SmoothLocomotion.OnAfterMove -= UnlockGrabbable;
                }

                lockRequests = 0;
                subscribedToEvents = false;
            }
        }

        private Transform _priorParent;

        private Vector3 _priorLocalOffsetPosition;
        private Quaternion _priorLocalOffsetRotation;

        private Grabber _priorPrimaryGrabber;
        bool lockPos, lockRot;
        int lockRequests = 0;

        public virtual void LockGrabbable()
        {
            LockGrabbable(true, false, false);
        }

        public virtual void LockGrabbableWithRotation()
        {
            LockGrabbable(true, true, true);
        }

        public virtual void RequestLockGrabbable()
        {

            if (RecentlyCollided)
            {
                return;
            }

            lockRequests++;

            if (lockRequests == 1)
            {
                if (_priorPrimaryGrabber != null)
                {
                    _priorParent = transform.parent;
                    transform.parent = _priorPrimaryGrabber.transform;
                }
            }

            if (lockRequests > 0)
            {
                if (_priorPrimaryGrabber != null)
                {

                    _priorParent = transform.parent;
                    transform.parent = _priorPrimaryGrabber.transform;

                    _priorLocalOffsetPosition = _priorPrimaryGrabber.transform.InverseTransformPoint(transform.position);
                }
            }
        }

        public virtual void RequestUnlockGrabbable()
        {

            if (RecentlyCollided)
            {
                return;
            }

            ResetLockResets();
        }

        public virtual void ResetLockResets()
        {
            if (lockRequests > 0)
            {

                if (transform.parent != _priorParent)
                {
                    transform.parent = _priorParent;
                }

                lockRequests = 0;
            }
        }

        public virtual void LockGrabbable(bool lockPosition, bool lockRotation, bool overridePriorLock)
        {

            if (BeingHeld && (!grabbableIsLocked || overridePriorLock))
            {

                if (_priorPrimaryGrabber != null)
                {

                    lockPos = lockPosition;
                    lockRot = lockRotation;

                    if (lockPosition && lockRotation)
                    {
                        _priorLocalOffsetPosition = _priorPrimaryGrabber.transform.InverseTransformPoint(transform.position);

                        _priorParent = transform.parent;
                        transform.parent = _priorPrimaryGrabber.transform;
                    }
                    else
                    {
                        if (lockPos)
                        {
                            _priorLocalOffsetPosition = _priorPrimaryGrabber.transform.InverseTransformPoint(transform.position);
                        }

                        if (lockRot)
                        {
                            _priorLocalOffsetRotation = Quaternion.FromToRotation(transform.forward, _priorPrimaryGrabber.transform.forward);
                        }
                    }

                    grabbableIsLocked = true;
                }
            }
        }

        public virtual void UnlockGrabbable()
        {
            if (BeingHeld && grabbableIsLocked)
            {
                if (lockPos && lockRot)
                {
                    Vector3 dest = _priorPrimaryGrabber.transform.TransformPoint(_priorLocalOffsetPosition);
                    float dist = Vector3.Distance(transform.position, dest);
                    if (dist > 0.001f)
                    {
                        transform.position = _priorPrimaryGrabber.transform.TransformPoint(_priorLocalOffsetPosition);
                    }

                    if (transform.parent != _priorParent)
                    {
                        transform.parent = _priorParent;
                    }
                }
                else
                {
                    if (lockPos)
                    {
                        Vector3 dest = _priorPrimaryGrabber.transform.TransformPoint(_priorLocalOffsetPosition);
                        float dist = Vector3.Distance(transform.position, dest);
                        if (dist > 0.0005f)
                        {
                            transform.position = dest;
                        }
                    }

                    if (lockRot)
                    {
                        transform.rotation = _priorPrimaryGrabber.transform.rotation * _priorLocalOffsetRotation;
                    }
                }

                grabbableIsLocked = false;
            }
        }

        private void OnCollisionStay(Collision collision)
        {

            if (!BeingHeld)
            {
                return;
            }

            for (int x = 0; x < collision.contacts.Length; x++)
            {
                ContactPoint contact = collision.contacts[x];
                if (BeingHeld && IsValidCollision(contact.otherCollider) && !collisions.Contains(contact.otherCollider))
                {
                    collisions.Add(contact.otherCollider);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (BeingHeld && IsValidCollision(collision) && !collisions.Contains(collision.collider))
            {
                collisions.Add(collision.collider);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (BeingHeld && collisions.Contains(collision.collider))
            {
                collisions.Remove(collision.collider);
            }
        }

        bool quitting = false;
        void OnApplicationQuit()
        {
            quitting = true;
        }

        void OnDestroy()
        {
            if (BeingHeld && !quitting)
            {
                DropItem(false, false);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);

            if (GrabPoints != null && GrabPoints.Count > 0)
            {
                for (int i = 0; i < GrabPoints.Count; i++)
                {
                    Transform p = GrabPoints[i];
                    if (p != null)
                    {
                        Gizmos.DrawSphere(p.position, 0.02f);
                    }
                }
            }
            else
            {
                Gizmos.DrawSphere(transform.position, 0.02f);
            }
        }
    }

    #region enums
    public enum GrabType
    {
        Snap,
        Precise
    }

    public enum RemoteGrabMovement
    {
        Linear,
        Velocity,
        Flick
    }

    public enum GrabPhysics
    {
        None = 2,
        PhysicsJoint = 0,
        FixedJoint = 3,
        Velocity = 4,
        Kinematic = 1
    }

    public enum OtherGrabBehavior
    {
        None,
        SwapHands,
        DualGrab
    }

    public enum TwoHandedPositionType
    {
        Lerp,
        None
    }

    public enum TwoHandedRotationType
    {
        Lerp,
        Slerp,
        LookAtSecondary,
        None
    }

    public enum TwoHandedDropMechanic
    {
        Drop,
        Transfer,
        None
    }

    public enum TwoHandedLookDirection
    {
        Horizontal,
        Vertical
    }

    public enum HandPoseType
    {
        AnimatorID,
        HandPose,
        AutoPoseOnce,
        AutoPoseContinuous,
        None
    }

    #endregion
}