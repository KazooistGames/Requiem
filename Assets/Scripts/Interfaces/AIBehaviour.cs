using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using UnityEngine.Events;

public class AIBehaviour : MonoBehaviour
{
    public enum AIState
    {
        none,
        guard,
        seek,
        passive,
        aggro,
        enthralled,
    }
    public AIState State = AIState.none;
    public float stateRunTimer;

    public float ReflexRate = 0.20f;
    public float Intelligence;
    public bool Enthralled = false;

    public Vector3 actualMovementDirection = Vector3.zero;
    public float actualMovementAngle;
    public Vector3 DesiredDirection = Vector3.zero;
    public float TotalWeight;
    public float LookAngle = 0;

    protected Entity entity;
    protected Vector3 lastDirection = Vector3.zero;

    public float excitement = 0f;

    public delegate void behaviour(BehaviourType key);
    public Dictionary<BehaviourType, behaviour> behaviours;
    public Dictionary<BehaviourType, (bool, float)> behaviourParams;

    public Dictionary<BehaviourType, (Vector3, float)> Directives = new Dictionary<BehaviourType, (Vector3, float)> { };
    public Dictionary<BehaviourType, float> LookDirectives = new Dictionary<BehaviourType, float> { };

    public enum BehaviourType
    {
        meander,
        patrol,
        sensory,
        pursue,
        tracking,
        grab,
        martial,
        dashing,
        tango,
        follow,
        waypoint,
        itemManagement,
        wallCrawl,
    }

    public AIState RestingState = AIState.passive;

    /********* unity funcs *********/

    protected virtual void Awake()
    {
        behaviours = new Dictionary<BehaviourType, behaviour>()
        {
            {BehaviourType.meander, new behaviour(meander)},
            {BehaviourType.patrol, new behaviour(patrol)},
            {BehaviourType.sensory, new behaviour(sensory)},
            {BehaviourType.pursue, new behaviour(pursue)},
            {BehaviourType.tracking, new behaviour(tracking)},
            {BehaviourType.grab, new behaviour(grab)},
            {BehaviourType.martial, new behaviour(martial)},
            {BehaviourType.dashing, new behaviour(dashing)},
            {BehaviourType.tango, new behaviour(tango)},
            {BehaviourType.follow, new behaviour(follow)},
            {BehaviourType.waypoint, new behaviour(waypoint)},
            {BehaviourType.itemManagement, new behaviour(itemManagement)},
            {BehaviourType.wallCrawl, new behaviour(wallCrawl)},
        };
        behaviourParams = new Dictionary<BehaviourType, (bool, float)>();
        foreach (BehaviourType key in behaviours.Keys)
        {
            behaviourParams[key] = (false, 0);
        }
        entity = GetComponent<Entity>();
    }

    protected virtual void Start()
    {
        //meanderPauseFrequency = Random.value;
        trackingObstructionMask = (1 << Requiem.layerWall) + (1 << Requiem.layerObstacle);
        meanderAvoidMask = ~((1 << Requiem.layerTile) + (1 << Requiem.layerItem));
        sensorySightMask = (1 << Requiem.layerObstacle) + (1 << Requiem.layerEntity) + (1 << Requiem.layerWall) + (1 << Requiem.layerItem);
        patrolMask = 1 << Requiem.layerWall + (1 << Requiem.layerObstacle);
        StartCoroutine(Think());
        StartCoroutine(stateHandler());
        entity.EventWounded.AddListener((float damage) => sensoryAlerted = true);
        martialFoeVulnerable.AddListener(reactToFoeVulnerable);
        martialFoeAttacking.AddListener(reactToIncomingAttack);
        martialFoeDashing.AddListener(reactToIncomingDash);
        sensoryFoeSpotted.AddListener(reactToFoeChange);
        sensoryFoeLost.AddListener(reactToFoeChange);
        _MartialController.INSTANCE.ClearedQueue.AddListener(queueNextRoundOfActions);
    }

    protected virtual void Update()
    {
        if (Enthralled)
        {
            StateTransition(AIState.enthralled);
        }
        stateRunTimer += Time.deltaTime;
        actualMovementAngle = actualMovementDirection != Vector3.zero ? getAngle(actualMovementDirection) % 360 : LookAngle;
        if (DesiredDirection != Vector3.zero)
        {
            lastDirection = DesiredDirection;
        }
        if (LookDirectives.Count > 0)
        {
            float total = 0;
            foreach (float value in LookDirectives.Values)
            {
                total += value;
            }
            total /= LookDirectives.Count;
            LookAngle = total;
        }
        else if (entity.Foe)
        {
            LookAngle = getAngle(entity.Foe.transform.position - transform.position);
        }
        else if(trackingTrailingTimer > 0)
        {
            LookAngle = getAngle(trackingLastKnownLocation - transform.position);
        }
        else if (actualMovementDirection == Vector3.zero)
        {
            LookAngle = getAngle(lastDirection);
        }
        else
        {
            LookAngle = getAngle(actualMovementDirection);
        }
        entity.WalkDirection = actualMovementDirection;
        entity.LookDirection = angleToDirection(LookAngle);
        if (Enthralled)
        {
            StateTransition(AIState.enthralled);
        }
        switch (State)
        {
            case AIState.none:
                StateTransition(RestingState);
                break;
            case AIState.passive:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                else
                {
                    ReflexRate = 0.2f;
                }
                break;
            case AIState.aggro:
                if (trackingTrailCold)
                {
                    StateTransition(AIState.seek);
                }
                else
                {
                    ReflexRate = 0.05f;
                    if (!mainWep)
                    {
                        martialCurrentState = martialState.none;
                    }
                    else if (mainWep.Action == Weapon.ActionAnim.Guarding)
                    {
                        martialCurrentState = martialState.defending;
                    }
                    else if (mainWep.Action == Weapon.ActionAnim.Aiming)
                    {
                        martialCurrentState = martialState.throwing;
                    }
                    else if (mainWep.Action == Weapon.ActionAnim.Idle || mainWep.Action == Weapon.ActionAnim.Sheathed)
                    {
                        martialCurrentState = martialState.none;
                    }
                    else
                    {
                        martialCurrentState = martialState.attacking;
                    }
                }
                break;
            case AIState.seek:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                else if (stateRunTimer > 5)
                {
                    StateTransition(RestingState);
                }
                break;
        }
    }

    protected virtual void OnDisable()
    {
        entity.modSpeed["AIState"] = 0;
        StateTransition(AIState.none);
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
        if (entity.Foe)
        {
            entity.Foe.modSpeed["grabbed" + gameObject.GetHashCode().ToString()] = 0;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && !wallCrawlObstacles.Contains(other.gameObject))
        {
            int objLayer = 1 << other.gameObject.layer;
            bool bingo = (objLayer & wallCrawlObstaclesMask) > 0 && !wallCrawlObstacles.Contains(other.gameObject);
            if (bingo && other.gameObject != gameObject && other.gameObject != entity.Foe)
            {
                wallCrawlObstacles.Add(other.gameObject);
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            int objLayer = 1 << other.gameObject.layer;
            bool bingo = (objLayer & wallCrawlObstaclesMask) > 0 && wallCrawlObstacles.Contains(other.gameObject); ;
            if (bingo && other.gameObject != gameObject && wallCrawlObstacles.Count > 0)
            {
                wallCrawlObstacles.Remove(other.gameObject);
            }
        }
    }

    /***** PUBLIC *****/

    /***** PROTECTED *****/
    protected virtual void SetTangoParameters()
    {
        if (mainWep || offWep)
        {
            Weapon wep = mainWep ? mainWep : offWep;
            switch (martialCurrentState)
            {
                case martialState.none:
                    tangoInnerRange = entity.personalBox.radius * entity.scaleActual;
                    tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    break;
                case martialState.attacking:
                    tangoInnerRange = wep.Range * 0.75f;
                    tangoOuterRange = wep.Range * 1.0f;
                    break;
                case martialState.defending:
                    tangoInnerRange = wep.Range * 1.5f;
                    tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    break;
                case martialState.throwing:
                    tangoInnerRange = wep.Range * 2.0f;
                    tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    break;
            }
            pursueStoppingDistance = tangoOuterRange;
        }
        else
        {
            tangoInnerRange = entity.personalBox.radius * entity.scaleActual;
            tangoOuterRange = sensorySightRangeScalar * sensoryBaseRange;
        }
    }
    protected bool checkMyWeaponInRange()
    {
        if (mainWep && entity.Foe)
        {
            float disposition = (entity.Foe.transform.position - transform.position).magnitude;
            return disposition <= mainWep.Range;
        }
        else
        {
            return false;
        }
    }
    protected float getPausePeriod(float min = 1, float randScalar = 1)
    {
        return min + Random.value * randScalar;
    }
    protected virtual void queueNextRoundOfActions(Weapon weapon)
    {

    }

    protected virtual void reactToFoeVulnerable()
    {

    }

    protected virtual void reactToFoeChange()
    {

    }

    protected virtual void reactToIncomingAttack()
    {

    }

    protected virtual void reactToIncomingDash()
    {

    }

    /***** PRIVATE *****/

    //BEHAVIOURS!!!

    //meander
    public Vector3 meanderDirection = Vector3.zero;
    public bool meanderPaused = false;
    public float meanderPauseFrequency = 0.5f;
    protected float meanderTimer = 0f;
    protected float meanderPeriod = 0f;
    protected int meanderAvoidMask;
    protected void meander(BehaviourType key)
    {
        if (behaviourParams[key].Item1)
        {
            meanderTimer += ReflexRate;
            if (meanderTimer >= meanderPeriod || Physics.Raycast(transform.position, DesiredDirection, entity.personalBox.radius, layerMask: meanderAvoidMask, queryTriggerInteraction: QueryTriggerInteraction.Ignore))
            {
                meanderPeriod = Mullet.BellCurve(2.0f, 0.5f);
                meanderTimer = 0;
                if (Random.value >= meanderPauseFrequency)
                {
                    int tries = 0;
                    meanderPaused = false;
                    do
                    {
                        tries++;
                        meanderDirection = Vector3.Lerp(meanderDirection, RandomDirection(), 0.25f);
                    } while (Physics.Raycast(transform.position, meanderDirection, entity.personalBox.radius, layerMask: meanderAvoidMask, queryTriggerInteraction: QueryTriggerInteraction.Ignore) && tries < 10);
                }
                else
                {
                    meanderPaused = true;
                    meanderDirection = Vector3.zero;
                }
                Directives[key] = (meanderDirection, behaviourParams[key].Item2);
            }
            behaviourParams[key] = (false, 0);
        }
        else
        {
            DeEnergize(key);
        }
    }


    //patrol
    protected float patrolScanAngle = 90f;
    protected int patrolMask;
    protected float patrolCurrentAngle = 0f;
    protected void patrol(BehaviourType key)
    {
        if (behaviourParams[key].Item1)
        {
            float startAngle = Mathf.Lerp(LookAngle, actualMovementAngle, 0.5f);
            float bestAngle = startAngle;
            float bestDistance = 0.0f;
            float lookRange = 1;
            int flipflop = Random.value >= 0.5f ? 1 : -1;
            for (int increment = -(int)(flipflop * (patrolScanAngle / 2)); increment != (int)(flipflop * (patrolScanAngle / 2)); increment += flipflop)
            {
                RaycastHit rayhit;
                bool wideOpen = !Physics.SphereCast(new Ray(transform.position, angleToDirection(startAngle + increment)), entity.berthActual * entity.scaleActual ,out rayhit, maxDistance: lookRange, layerMask: patrolMask, QueryTriggerInteraction.Ignore);
                if (wideOpen)
                {
                    if (lookRange > bestDistance || increment == 0)
                    {
                        bestAngle = startAngle + increment;
                        bestDistance = lookRange;
                    }
                }
                else if (rayhit.distance > bestDistance)
                {
                    bestAngle = startAngle + increment;
                    bestDistance = rayhit.distance;
                }
            }
            patrolCurrentAngle = bestAngle % 360;
            Vector3 outputDirection = angleToDirection(patrolCurrentAngle);
            Directives[key] = (outputDirection, behaviourParams[key].Item2);
            behaviourParams[key] = (false, 0);
        }
        else
        {
            patrolCurrentAngle = LookAngle;
            DeEnergize(key);
        }
    }

    //survey
    public UnityEvent sensoryFoeSpotted = new UnityEvent();
    public UnityEvent sensoryFoeLost = new UnityEvent();
    public bool sensoryAlerted = false;
    protected float sensoryAlertedTimer = 0.0f;
    protected float sensoryAlertedPeriod = 5.0f;
    protected bool sensorySightDetection = true;
    protected int sensorySightMask;
    protected float sensoryBaseRange = 1.0f;
    protected float sensorySightRangeScalar = 1.0f;
    protected float sensorySightFOV = 120f;
    protected bool sensoryAudioDetection = true;
    protected bool sensoryAudioApproach = false;
    protected float sensoryAudioRangeScalar = 1.5f;
    protected float sensoryAudioTimer = 0f;
    protected float sensoryAudioPeriod = 3.0f;
    protected Vector3 sensoryAudioDirection = Vector3.zero;
    protected void sensory(BehaviourType key)
    {
        if (behaviourParams[key].Item1)
        {
            if (entity.Foe)
            {
                sensoryAlerted = true;
            }
            else if (sensoryAlerted)
            {
                if((sensoryAlertedTimer += ReflexRate) >= sensoryAlertedPeriod)
                {
                    sensoryAlertedTimer = 0.0f;
                    sensoryAlerted = false;
                }
            }
            else
            {
                sensoryAlertedTimer = 0.0f;
            }
            sensoryBaseRange = (sensoryAlerted ? 7.5f : 5f) * Entity.Scale;
            Entity potentialFoe;
            bool enemiesAfoot = false;
            if (!entity.Foe && entity.Aggressive)
            {
                if(sensorySightDetection)
                {
                    float sightRangeActual = sensoryBaseRange * sensorySightRangeScalar;
                    float sightFOVactual = sensoryAlerted ? sensorySightFOV * 1.5f : sensorySightFOV;
                    RaycastHit[] firstPass = Physics.SphereCastAll(transform.position + Vector3.up * sightRangeActual * 2, sightRangeActual, Vector3.down, sightRangeActual * 2, 1 << Requiem.layerEntity, QueryTriggerInteraction.Ignore);
                    foreach (RaycastHit entityHit in firstPass)
                    {
                        float angle = getAngle(entityHit.transform.position - transform.position);
                        Entity nearbyEntity = entityHit.collider.GetComponent<Entity>();
                        enemiesAfoot = nearbyEntity ? nearbyEntity.Allegiance != entity.Allegiance : enemiesAfoot;
                        if (nearbyEntity && Mathf.Abs(LookAngle - angle) <= sightFOVactual / 2 ? nearbyEntity.Allegiance != entity.Allegiance : false)
                        {
                            RaycastHit[] secondPass = Physics.RaycastAll(transform.position, nearbyEntity.transform.position - transform.position, sightRangeActual, sensorySightMask, QueryTriggerInteraction.Ignore);
                            float closestObject = float.MaxValue;
                            foreach (RaycastHit objectHit in secondPass)
                            {
                                if (objectHit.distance < closestObject)
                                {
                                    potentialFoe = objectHit.collider.GetComponent<Entity>();
                                    if (potentialFoe ? potentialFoe.Allegiance != entity.Allegiance : false)
                                    {
                                        entity.Foe = potentialFoe;
                                        sensoryFoeSpotted.Invoke();
                                        closestObject = objectHit.distance;
                                    }
                                    else if (!potentialFoe)
                                    {
                                        entity.Foe = null;
                                        closestObject = objectHit.distance;
                                    }
                                }
                            }
                        }
                    }
                }
                if (sensoryAudioDetection && !entity.Foe && entity.Aggressive)
                {
                    float audioRangeActual = sensoryBaseRange * sensoryAudioRangeScalar;
                    Vector3 newAudioDirection = Vector3.zero;
                    AudioSource[] sources = FindObjectsOfType<AudioSource>();
                    for (int i = 0; i < sources.Length; i++)
                    {
                        if (sources[i] ? sources[i].isPlaying && (!sensoryAlerted || sources[i].gameObject.layer == Requiem.layerEntity) : false)
                        {
                            Vector3 dispo = sources[i].transform.position - transform.position;
                            dispo.y = 0;
                            if (dispo.magnitude <= audioRangeActual)
                            {
                                newAudioDirection += dispo;
                                sensoryAudioTimer = 0;
                                if (sources[i].gameObject.layer == Requiem.layerEntity)
                                {
                                    sensoryAlerted = true;
                                }
                            }
                        }
                    }
                    sensoryAudioDirection = newAudioDirection != Vector3.zero ? newAudioDirection : sensoryAudioDirection;
                }
            }
            else if(entity.Foe)
            {
                RaycastHit[] rayHits;
                Vector3 disposition = (entity.Foe.transform.position - transform.position);
                LookDirectives.Remove(key);
                if(disposition.magnitude > sensoryBaseRange)
                {
                    entity.Foe = null;
                    sensoryFoeLost.Invoke();
                }
                else
                {
                    float temp = float.MaxValue;
                    rayHits = Physics.RaycastAll(transform.position, entity.Foe.transform.position - transform.position, maxDistance: sensoryBaseRange, layerMask: sensorySightMask, QueryTriggerInteraction.Ignore);                   
                    foreach (RaycastHit hit in rayHits)
                    {
                        if (hit.distance < temp)
                        {
                            if (hit.distance < temp)
                            {
                                potentialFoe = hit.collider.GetComponent<Entity>();
                                if (potentialFoe ? potentialFoe.Allegiance != entity.Allegiance : false)
                                {
                                    entity.Foe = potentialFoe;
                                    temp = hit.distance;
                                }
                                else if (!potentialFoe)
                                {
                                    entity.Foe = null;
                                    sensoryFoeLost.Invoke();
                                    temp = hit.distance;
                                }
                            }
                        }
                    }
                }          
            }
            if ((sensoryAudioTimer += ReflexRate) > sensoryAudioPeriod || entity.Staggered || entity.Foe || sensoryAudioDirection == Vector3.zero)
            {
                sensoryAudioTimer = 0;
                sensoryAudioDirection = Vector3.zero;
                LookDirectives.Remove(key);
            }
            else if (sensoryAudioDirection != Vector3.zero)
            {
                LookDirectives[key] = getAngle(sensoryAudioDirection);
                behaviourParams[key] = (true, 1);
            }
        }
        else
        {
            sensoryAudioTimer = 0;
            LookDirectives.Remove(key);
            //behaviourParams[key] = (true, 0);
        }
    }

    //pursue
    public float pursueStoppingDistance = 0.0f;
    protected void pursue(BehaviourType key)
    {
        if (behaviourParams[key].Item1 && entity.Foe)
        {
            Vector3 outputDirection;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            disposition.y = 0;
            if(disposition.magnitude > pursueStoppingDistance)
            {
                Rigidbody foeBody = entity.Foe.GetComponent<Rigidbody>();
                Vector3 targetTrajectory = new Vector3(foeBody.velocity.x, 0, foeBody.velocity.z).normalized;
                float angleTrajectory = getAngle(targetTrajectory);
                float angleToTarget = getAngle(disposition);
                float angularDeficit = Mathf.Abs((angleTrajectory + 360) % 360 - (angleToTarget + 360) % 360);
                float speedRatio = GetComponent<Rigidbody>().velocity.magnitude / entity.Foe.GetComponent<Rigidbody>().velocity.magnitude;
                float outputRatio = angularDeficit * speedRatio / 180;
                Vector3 interceptionRoute = foeBody.velocity.magnitude > 0 && (angularDeficit < 175 || 185 < angularDeficit) ? Vector3.Lerp(targetTrajectory, disposition, outputRatio) : disposition;
                outputDirection = interceptionRoute.normalized;
            }
            else
            {
                outputDirection = Vector3.zero;
            }     
            Directives[key] = (outputDirection, behaviourParams[key].Item2);
            behaviourParams[key] = (false, 0);
        }
        else
        {
            DeEnergize(key);
        }
    }





    //tracking
    public bool trackingEyesOnTarget = false;
    public bool trackingTrailCold = true;
    protected bool trackingTrailingEnabled = true;
    protected float trackingTrailingTimer = 0.0f;
    protected float trackingTrailingPeriod = 5.0f;
    protected Vector3 trackingLastKnownLocation = Vector3.zero;
    protected Vector3 trackingLastKnownDirection = Vector3.zero;
    protected int trackingObstructionMask;
    protected void tracking(BehaviourType key)
    {
        if (behaviourParams[key].Item1)
        {
            Vector3 actualDispo = entity.Foe ? entity.Foe.transform.position - transform.position : Vector3.zero;
            Vector3 trailDispo =  (entity.Foe && trackingEyesOnTarget? entity.Foe.transform.position : trackingLastKnownLocation) - transform.position;
            trackingEyesOnTarget = entity.Foe ? !Physics.Raycast(transform.position, actualDispo, actualDispo.magnitude, trackingObstructionMask, QueryTriggerInteraction.Ignore) : false;
            trackingLastKnownLocation = entity.Foe && trackingEyesOnTarget ? entity.Foe.transform.position : trackingLastKnownLocation; 
            trackingTrailCold = (trackingTrailingTimer >= trackingTrailingPeriod)||(!trackingEyesOnTarget && (trackingLastKnownLocation - transform.position).magnitude <= entity.personalBox.radius * entity.scaleActual);
            trackingLastKnownDirection = entity.Foe ? entity.Foe.WalkDirection : trackingLastKnownDirection;
            if (trackingTrailCold)
            {
                trackingTrailingTimer = 0.0f;
                if(trackingLastKnownDirection != Vector3.zero)
                {
                    LookDirectives[key] = getAngle(trackingLastKnownDirection);
                    trackingLastKnownDirection = Vector3.zero;
                }
            }
            else if (!trackingTrailCold && !trackingEyesOnTarget && trackingTrailingEnabled)
            {
                Directives[key] = (trailDispo, behaviourParams[key].Item2);
                if (trackingTrailingTimer < trackingTrailingPeriod)
                {
                    trackingTrailingTimer += ReflexRate;
                }
            }
            else
            {
                trackingTrailingTimer = 0.0f;
                DeEnergize(key);
            }
        }
        else
        {
            trackingTrailingTimer = 0.0f;
            trackingTrailCold = false;
            trackingEyesOnTarget = false;
            trackingLastKnownLocation = transform.position;
            DeEnergize(key);
        }
    }

    //grab
    protected bool grabEnabled = false;
    protected float grabSlowScalar = 1.0f;
    public float grabDPS = 15f;
    protected Entity grabLastVictim;
    protected void grab(BehaviourType key)
    {
        if (behaviourParams[key].Item1 && entity.Foe && !entity.Staggered)
        {
            grabLastVictim = entity.Foe;
            Vector3 disposition = grabLastVictim.transform.position - transform.position;
            if (disposition.magnitude < entity.personalBox.radius * entity.scaleActual && !entity.Dashing)
            {
                grabLastVictim.modSpeed["grabbed" + gameObject.GetHashCode().ToString()] = -(grabSlowScalar * Entity.Strength_Ratio(entity,entity.Foe));
                if (grabEnabled)
                {
                    grabLastVictim.Damage(ReflexRate * behaviourParams[key].Item2 * grabDPS);
                }
            }
            else
            {
                grabLastVictim.modSpeed["grabbed" + gameObject.GetHashCode().ToString()] = 0;
            }
        }
        else
        {
            if (grabLastVictim)
            {
                grabLastVictim.modSpeed["grabbed" + gameObject.GetHashCode().ToString()] = 0;
                grabLastVictim = null;
            }
            DeEnergize(key);
        }
    }

    //tango
    public bool tangoDeadbanded = false;
    protected bool tangoDeadbandingEnabled = true;
    protected float tangoTargetDisposition;
    protected float tangoInnerRange = 0f;
    protected float tangoOuterRange = 1f;
    protected float tangoTimer = 0;
    protected float tangoPeriod = 1;
    protected float tangoPeriodScalar = 1.0f;
    public bool tangoStrafeEnabled = true;
    protected float tangoStrafePauseFreq = 0.5f;
    private bool tangoStrafePaused = false;
    private int tangoStrafeFlipFlop = 1;
    private bool tangoNearStart = false;
    private bool tangoFarStart = false;
    protected void tango(BehaviourType key)
    {
        if (behaviourParams[key].Item1 && entity.Foe)
        {
            Vector3 outputDirection;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            if (tangoTimer >= tangoPeriod)
            {
                tangoTimer = 0;
                tangoPeriod = (Random.value + 1.5f) * tangoPeriodScalar;
                tangoStrafePaused = Random.value <= tangoStrafePauseFreq;
                tangoTargetDisposition = Mathf.Lerp(tangoInnerRange, tangoOuterRange, Random.value);
                tangoStrafeFlipFlop *= -1;
                tangoDeadbanded = false;
                tangoNearStart = disposition.magnitude <= tangoTargetDisposition;
                tangoFarStart = disposition.magnitude > tangoTargetDisposition;
            }
            else
            {
                tangoTimer += ReflexRate;
            }
            bool deadbandHold = tangoDeadbanded && (tangoOuterRange >= disposition.magnitude && disposition.magnitude >= tangoInnerRange);
            bool deadbandSet = !tangoDeadbanded && ((tangoNearStart && disposition.magnitude > tangoTargetDisposition) || (tangoFarStart && disposition.magnitude <= tangoTargetDisposition));
            tangoDeadbanded = tangoDeadbanded || deadbandSet;
            tangoNearStart = tangoDeadbanded ? disposition.magnitude <= tangoTargetDisposition : tangoNearStart;
            tangoFarStart = tangoDeadbanded ?  disposition.magnitude > tangoTargetDisposition : tangoFarStart;
            float delta = disposition.magnitude - tangoTargetDisposition;
            float approachOffset = tangoDeadbanded ? 0 : Mathf.Clamp(delta / tangoTargetDisposition + Mathf.Sign(delta)*0.15f, -1, 1);
            if (tangoStrafeEnabled && !tangoStrafePaused)
            {
                outputDirection = angleToDirection(getAngle(disposition) - (90 * tangoStrafeFlipFlop) + (approachOffset * 90 * tangoStrafeFlipFlop));
            }
            else
            {
                outputDirection = (disposition * approachOffset).normalized;
            }
            Directives[key] = (outputDirection, behaviourParams[key].Item2);
            behaviourParams[key] = (false, 0);
        }
        else
        {
            tangoStrafePaused = false;
            tangoDeadbanded = false;
            DeEnergize(key);
        }
    }

    //martial
    public enum martialState
    {
        none = 0,
        defending = 1,
        attacking = 2,
        throwing = 3,
    }
    public martialState martialCurrentState = 0;
    public martialState martialPreferredState = martialState.none;

    protected float martialStateTimer = 0;
    protected float martialStatePeriod = 0;
    protected bool martialStateBouncing = true;
    protected bool martialAttackingONS = true;
    protected bool martialDefendingONS = true;
    public UnityEvent martialFoeVulnerable = new UnityEvent();
    public UnityEvent martialFoeAttacking = new UnityEvent();
    public UnityEvent martialFoeDashing = new UnityEvent();    
    public UnityEvent martialFoeEnteredRange = new UnityEvent();    
    public UnityEvent martialEnteredRange = new UnityEvent();
    protected bool martialFoeVulnerableLatch = false;
    protected bool martialFoeAttackingLatch = false;
    protected bool martialFoeChargingLatch = false;
    protected bool martialFoeEnteredRangeLatch = false;
    protected bool martialEnteredRangeLatch = false;
    protected Weapon mainWep;
    protected Weapon offWep;
    protected Weapon matchupMain;
    protected Weapon matchupOff;
    protected void martial(BehaviourType key)
    {
        mainWep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
        offWep = entity.OffHand ? entity.OffHand.GetComponent<Weapon>() : null;
        if (behaviourParams[key].Item1 && entity.Foe && (mainWep || offWep))
        {
            matchupMain = entity.Foe.MainHand ? entity.Foe.MainHand.GetComponent<Weapon>() : null;
            matchupOff = entity.Foe.OffHand ? entity.Foe.OffHand.GetComponent<Weapon>() : null;
            Vector3 disposition = entity.Foe.transform.position - transform.position;

            bool foeFacing = Vector3.Dot(disposition.normalized, entity.Foe.LookDirection.normalized) <= -0.5f;
            bool foeRebuked = matchupMain ? matchupMain.Action == Weapon.ActionAnim.Recoiling : true && matchupOff ? matchupOff.Action == Weapon.ActionAnim.Recoiling : true;

            bool inRange = disposition.magnitude <= Mathf.Max(mainWep ? mainWep.Range : 0.0f, offWep ? offWep.Range : 0.0f);
            float rangeBoost = 1.2f;
            bool foeInRange = (matchupMain ? disposition.magnitude <= matchupMain.Range * rangeBoost : false) || (matchupOff ? disposition.magnitude <= matchupOff.Range * rangeBoost : false);

            bool foeAttacking = (matchupMain ? matchupMain.Action == Weapon.ActionAnim.QuickAttack : false) || (matchupOff ? matchupOff.Action == Weapon.ActionAnim.QuickAttack : false);
            bool foeCoiling = (matchupMain ? matchupMain.Action == Weapon.ActionAnim.QuickCoil || matchupMain.Action == Weapon.ActionAnim.StrongCoil : false) || (matchupOff ? matchupOff.Action == Weapon.ActionAnim.QuickCoil || matchupOff.Action == Weapon.ActionAnim.StrongCoil : false);
            bool foeWindingUp = (matchupMain ? matchupMain.Action == Weapon.ActionAnim.QuickWindup || matchupMain.Action == Weapon.ActionAnim.StrongWindup : false) || (matchupOff ? matchupOff.Action == Weapon.ActionAnim.QuickWindup || matchupOff.Action == Weapon.ActionAnim.StrongWindup : false);          
            bool foeAiming = (matchupMain ? matchupMain.Action == Weapon.ActionAnim.Aiming : false) || (matchupOff ? matchupMain.Action == Weapon.ActionAnim.Aiming : false);         
            //bool foeCharging = (matchupMain ? matchupMain.ActionAnimated == Weapon.ActionAnimation.StrongWindup : false) || (matchupOff ? matchupOff.ActionAnimated == Weapon.ActionAnimation.StrongWindup : false);

            if(!martialEnteredRangeLatch && inRange)
            {
                martialEnteredRange.Invoke();
            }
            martialEnteredRangeLatch = inRange;
            if(!martialFoeEnteredRangeLatch && foeInRange)
            {
                martialFoeEnteredRange.Invoke();
            }
            martialFoeEnteredRangeLatch = inRange;

            bool instantaneousFoeCharging = foeInRange && !entity.Foe.Staggered && entity.Foe.Dashing;
            if (!martialFoeChargingLatch && instantaneousFoeCharging)
            {
                martialFoeChargingLatch = true;
                martialFoeDashing.Invoke();
            }
            else if (!instantaneousFoeCharging)
            {
                martialFoeChargingLatch = false;
            }

            bool instantaneousFoeVulnerable = foeRebuked || !foeFacing || (!matchupMain && !matchupOff);
            if (!martialFoeVulnerableLatch && instantaneousFoeVulnerable)
            {
                martialFoeVulnerableLatch = true;
                martialFoeVulnerable.Invoke();        
            }
            else if (!instantaneousFoeVulnerable)
            {
                martialFoeVulnerableLatch = false;
            }

            bool instantaneousReactiveDefend = (foeAttacking || foeWindingUp) && foeInRange && foeFacing;
            if (!martialFoeAttackingLatch && instantaneousReactiveDefend)
            {
                martialFoeAttackingLatch = true;
                martialFoeAttacking.Invoke();         
            }
            else if (!instantaneousReactiveDefend)
            {
                martialFoeAttackingLatch = false;
            }
        }
    }

    protected Coroutine dashCycle;
    protected float dashingCooldownTimer = 0.0f;
    protected float dashingChargePeriod = 0.0f;
    protected float dashingChargeTimer = 0.0f;
    protected Vector3 dashingDesiredDirection;
    protected void dashing(BehaviourType key) 
    {
        if (behaviourParams[key].Item1)
        {
            dashingCooldownTimer += ReflexRate;
            if (dashCycle == null)
            {
                dashCycle = StartCoroutine(dashingCycle());
            }
        }
        else
        {
            DeEnergize(key);
        }
    }

    //itemManagement
    public bool itemManagementSeekItems = false;
    public bool itemManagementGreedy = false;
    protected bool itemManagementSeeking = false;
    protected bool itemManagementNoDoubles = false;
    protected bool itemManagementNoSingles = false;
    public Entity.WieldMode itemManagementPreferredType = Entity.WieldMode.none;
    protected Weapon itemManagementTarget;
    protected float itemManagementDelayTimer = 0.0f;
    protected float itemManagementDelayPeriod = 1f;
    protected void itemManagement(BehaviourType key)
    {
        if (behaviourParams[key].Item1 && !entity.Shoved)
        {
            if (itemManagementTarget ? !itemManagementTarget.Wielder : false)
            {
                itemManagementDelayTimer = 0.0f;
                itemManagementSeeking = true;
                Vector3 disposition = itemManagementTarget.transform.position - transform.position;
                Directives[key] = (disposition.normalized, behaviourParams[key].Item2);
                if (disposition.magnitude <= entity.personalBox.radius * entity.scaleActual * 1.5f)
                {
                    itemManagementTarget.PickupItem(entity);
                }
            }
            else if(itemManagementGreedy || itemManagementSeekItems)
            {
                itemManagementSeeking = false;
                itemManagementTarget = null;
                DeEnergize(key);
                bool singlesOpen = !itemManagementNoSingles && !(entity.leftStorage && entity.rightStorage);
                bool doublesOpen = !itemManagementNoDoubles && !entity.backStorage;
                bool needWeapon = itemManagementSeekItems && !(entity.leftStorage || entity.rightStorage || entity.backStorage);
                bool wantWeapon = itemManagementGreedy && (singlesOpen || doublesOpen);
                if  (needWeapon || wantWeapon)
                {
                    if ((itemManagementDelayTimer += ReflexRate) >= itemManagementDelayPeriod)
                    {
                        RaycastHit[] firstPass = Physics.SphereCastAll(transform.position + Vector3.up * sensoryBaseRange * 2, sensoryBaseRange, Vector3.down, sensoryBaseRange * 2, 1 << Requiem.layerItem, QueryTriggerInteraction.Ignore);
                        Weapon closestWeapon = null;
                        float closestDistance = float.MaxValue;
                        foreach (RaycastHit nearbyHit in firstPass)
                        {
                            Weapon nearbyWeapon = nearbyHit.collider.GetComponent<Weapon>();
                            if (nearbyWeapon ? !nearbyWeapon.Wielder : false)
                            {
                                RaycastHit[] secondPass = Physics.RaycastAll(transform.position, nearbyWeapon.transform.position - transform.position, sensoryBaseRange, (1 << Requiem.layerWall) + (1 << Requiem.layerObstacle) + (1 << Requiem.layerItem), QueryTriggerInteraction.Ignore);
                                float minDistance = float.MaxValue;
                                Weapon potentialWeapon = null;
                                foreach (RaycastHit objectHit in secondPass)
                                {
                                    if (objectHit.distance < minDistance)
                                    {
                                        minDistance = objectHit.distance;
                                        potentialWeapon = objectHit.collider.GetComponent<Weapon>();
                                    }
                                }
                                if (potentialWeapon && minDistance < closestDistance)
                                {
                                    bool match = (potentialWeapon.equipType == Wieldable.EquipType.OneHanded && singlesOpen) || (potentialWeapon.equipType == Wieldable.EquipType.TwoHanded && doublesOpen);
                                    if (match && !potentialWeapon.Wielder)
                                    {
                                        closestWeapon = potentialWeapon;
                                        closestDistance = minDistance;
                                    }
                                }
                            }
                        }
                        if (closestWeapon)
                        {
                            if ((closestWeapon.equipType == Wieldable.EquipType.OneHanded && singlesOpen) || (closestWeapon.equipType == Wieldable.EquipType.TwoHanded && doublesOpen))
                            {
                                itemManagementTarget = closestWeapon;
                            }
                        }
                    }
                }
                else
                {
                    itemManagementDelayTimer = 0.0f;
                }
            }
            
            if (entity.leftStorage || entity.rightStorage || entity.backStorage)
            {
                if (!sensoryAlerted)
                {
                    entity.wieldMode = Entity.WieldMode.EmptyHanded;
                }
                else if(itemManagementPreferredType == Entity.WieldMode.OneHanders && (entity.leftStorage || entity.rightStorage))
                {
                    entity.wieldMode = Entity.WieldMode.OneHanders;
                }
                else if (itemManagementPreferredType == Entity.WieldMode.TwoHanders && (entity.backStorage))
                {
                    entity.wieldMode = Entity.WieldMode.TwoHanders;
                }
                else if (!entity.MainHand && (entity.leftStorage || entity.rightStorage))
                {
                    entity.wieldMode = Entity.WieldMode.OneHanders;
                }
                else if (!entity.MainHand && entity.backStorage)
                {
                    entity.wieldMode = Entity.WieldMode.TwoHanders;
                }               
            }              
        }
        else
        {
            itemManagementSeeking = false;
            DeEnergize(key);
        }
    }


    //follow
    public GameObject followVIP;
    public bool followRecall = false;
    public bool followNutHug = false;
    public float followDistance;
    protected Vector3 followVIPlastCoordinates;
    protected float followInnerDeadband;
    protected float followOuterDeadband;
    protected int followLayerMask;
    protected void follow(BehaviourType key)
    {
        if (behaviourParams[key].Item1 && followVIP)
        {
            followLayerMask = (1 << followVIP.layer) + (1 << Requiem.layerWall) + (1 << Requiem.layerObstacle);
            followInnerDeadband = entity.personalBox.radius * entity.scaleActual * 1.5f;
            followOuterDeadband = sensoryBaseRange;
            followDistance = Mathf.Lerp(followInnerDeadband, followOuterDeadband, 0.25f);
            Vector3 VIPdispo = followVIP.transform.position - transform.position;
            if(Physics.Raycast(new Ray(transform.position, VIPdispo), sensoryBaseRange, followLayerMask, QueryTriggerInteraction.Ignore))
            {
                followVIPlastCoordinates = followVIP.transform.position;
                followRecall = VIPdispo.magnitude <= (followNutHug ? followInnerDeadband : followDistance) ? false : followRecall;
            }
            else
            {
                followRecall = true;
            }
            Vector3 output = followRecall ? (followVIPlastCoordinates - transform.position).normalized : Vector3.zero;
            Directives[key] = (output, behaviourParams[key].Item2);
            behaviourParams[key] = (false, 0);
        }
        else
        {
            DeEnergize(key);
        }
    }


    //waypoint
    public Vector3 waypointCoordinates;
    public bool waypointCommanded = false;
    public bool waypointDeadbanded = false;
    public float waypointDeadbandingScalar = 2.0f;
    public float waypointInnerlimit = Hextile.Radius;
    public float waypointOuterLimit = Hextile.Radius * 2;
    protected void waypoint(BehaviourType key)
    {
        if (behaviourParams[key].Item1)
        {
            float minimumDisposition = waypointDeadbanded ? waypointOuterLimit : entity.personalBox.radius * entity.scaleActual * waypointDeadbandingScalar;
            Vector3 dispo = (waypointCoordinates - transform.position);
            dispo.y = 0;
            waypointDeadbanded = dispo.magnitude <= minimumDisposition;
            Vector3 output = waypointCommanded && !waypointDeadbanded ? dispo.normalized : Vector3.zero;
            Directives[key] = (output, behaviourParams[key].Item2);
        }
        else
        {
            waypointCommanded = false;
            waypointDeadbanded = false;
            DeEnergize(key);
        }
    }


    //wallCrawl
    public bool wallCrawlDrawRays = false;
    public bool wallCrawlCrowding = false;
    public bool wallCrawlAvoidFoe = true;
    protected int wallCrawlObstaclesMask;
    protected int wallCrawlAvoidAngle;
    protected float wallCrawlTimer = 0;
    protected float wallCrawlPeriod = 3;
    public List<GameObject> wallCrawlObstacles = new List<GameObject>();
    protected void wallCrawl(BehaviourType key)
    {
        wallCrawlObstaclesMask = (1 << Requiem.layerWall) + (1 << Requiem.layerObstacle) + (wallCrawlCrowding ? 0 : (1 << Requiem.layerEntity));
        if (behaviourParams[key].Item1 && wallCrawlObstacles.Count > 0)
        {
            float trueRadius = (entity.personalBox.radius - entity.berthActual) * entity.scaleActual;
            bool obstaclesPresent = false;
            Vector3 weightedAvoidVector = Vector3.zero;
            Vector3 outputDirection = Vector3.zero;
            float outputWeight = 0f;
            int colliderCount = 0;
            wallCrawlObstacles.RemoveAll(x => !x);
            if (!wallCrawlAvoidFoe && entity.Foe)
            {
                wallCrawlObstacles.RemoveAll(x => x.gameObject == entity.Foe.gameObject);
            }

            foreach (GameObject obstacle in wallCrawlObstacles)
            {
                if (obstacle)
                { 
                    obstaclesPresent = true;
                    Collider[] colliders = obstacle.GetComponents<Collider>();
                    Vector3 penetrationVector;
                    float penetrationMagnitude;
                    foreach(Collider col in colliders.ToList())
                    {
                        if(Physics.ComputePenetration(entity.personalBox, transform.position, transform.rotation, col, col.transform.position, col.transform.rotation, out penetrationVector, out penetrationMagnitude))
                        {
                            penetrationVector.y = 0;
                            float weight = Mathf.Pow(penetrationMagnitude/trueRadius, 2);
                            weightedAvoidVector += (penetrationVector).normalized * weight;
                            colliderCount++;
                            if (wallCrawlDrawRays)
                            {
                                Debug.DrawLine(transform.position, transform.position - penetrationVector.normalized * weight * 5, Color.red, ReflexRate);
                            }
                        }               
                    }
                }
            }
            if (obstaclesPresent)
            {
                wallCrawlTimer += ReflexRate;
                float scalar = 1.5f;
                outputWeight = TotalWeight * scalar * weightedAvoidVector.magnitude / colliderCount;        
                outputDirection = weightedAvoidVector.normalized;
                if (wallCrawlDrawRays)
                {
                    Debug.DrawRay(transform.position, weightedAvoidVector * outputWeight / TotalWeight * scalar, Color.blue, ReflexRate);
                    Debug.Log(outputWeight / TotalWeight * scalar);
                }
            }
            else
            {
                wallCrawlTimer = 0f;
            }
            Directives[key] = (outputDirection, outputWeight);
        }
        else
        {
            wallCrawlTimer = 0f;
            DeEnergize(key);
            behaviourParams[key] = (true, 0);
        }
    }

    /*********** emergent AI **************/

    private IEnumerator Think()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            if (!Requiem.INSTANCE.Paused && enabled)
            {
                foreach (KeyValuePair<BehaviourType, behaviour> behaviour in behaviours)
                {
                    behaviour.Value(behaviour.Key);
                }
                Vector3 output = Vector3.zero;
                Vector3 output2 = Vector3.zero;
                List<Vector3> vectors = new List<Vector3>();
                List<Vector3> vectors2 = new List<Vector3>();
                foreach (BehaviourType key in Directives.Keys)
                {
                    Vector3 temp = Directives[key].Item1 * Directives[key].Item2;
                    vectors.Add(temp);
                    if (key != BehaviourType.wallCrawl)
                    {
                        vectors2.Add(temp);
                    }
                }
                foreach (Vector3 target in vectors)
                {
                    output += target;
                }
                foreach (Vector3 target in vectors2)
                {
                    output2 += target;
                }
                output = vectors.Count > 0 ? output / vectors.Count : Vector3.zero;
                actualMovementDirection = output.normalized;
                DesiredDirection = output2.normalized;
                TotalWeight = 0;
                TotalWeight = Directives.Aggregate(0.0f, (result, x) => result += (x.Key != BehaviourType.wallCrawl ? x.Value.Item2 : 0));
            }
            yield return new WaitForSecondsRealtime(ReflexRate);
        }
    }

    private IEnumerator stateHandler()
    {
        Vector3 disposition;
        float excitement;
        bool inRange;
        bool leashed;
        while (true)
        {
            if (!Requiem.INSTANCE.Paused && enabled)
            {
                disposition = entity.Foe ? entity.Foe.transform.position - transform.position : Vector3.zero;
                switch (State)
                {
                    case AIState.none:
                        entity.modSpeed["AIState"] = 0f;
                        break;
                    case AIState.passive:
                        stateRunTimer = sensoryAlerted ? 0 : stateRunTimer;
                        excitement = Mathf.Lerp(1, 0, stateRunTimer / 20);
                        entity.modSpeed["AIState"] = Mathf.Lerp(-0.75f, 0f, excitement);
                        behaviourParams[BehaviourType.patrol] = (!meanderPaused || excitement > 0, Intelligence);
                        behaviourParams[BehaviourType.meander] = (true, 1);
                        break;
                    case AIState.aggro:
                        trackingTrailingEnabled = true;
                        wallCrawlAvoidFoe = (mainWep || offWep);
                        behaviourParams[BehaviourType.tracking] = (true, 1);
                        behaviourParams[BehaviourType.pursue] = (trackingEyesOnTarget && disposition.magnitude > tangoOuterRange && !tangoDeadbanded, 1);
                        behaviourParams[BehaviourType.tango] = (trackingEyesOnTarget, 1);
                        behaviourParams[BehaviourType.martial] = (entity.MainHand && trackingEyesOnTarget, Intelligence);
                        behaviourParams[BehaviourType.grab] = (!entity.MainHand && trackingEyesOnTarget, Intelligence);
                        SetTangoParameters();
                        break;
                    case AIState.guard:
                        entity.modSpeed["AIState"] = entity.Foe ? 0 : -0.5f;
                        behaviourParams[BehaviourType.waypoint] = (true, 1);
                        behaviourParams[BehaviourType.meander] = (waypointDeadbanded && !entity.Foe, 1);
                        behaviourParams[BehaviourType.martial] = (entity.MainHand, Intelligence);
                        break;
                    case AIState.seek:
                        behaviourParams[BehaviourType.patrol] = (true, 1);
                        behaviourParams[BehaviourType.meander] = (true, 1 - Intelligence);
                        //entity.modSpeed["AIState"] = Mathf.Lerp(-1.0f, 0.0f, Intelligence);
                        break;
                    case AIState.enthralled:
                        leashed = followRecall || (waypointCommanded && !waypointDeadbanded);
                        inRange = entity.Foe ? disposition.magnitude < tangoOuterRange : false;
                        entity.modSpeed["AIState"] = leashed || entity.Foe ? 0 : -0.5f;
                        wallCrawlCrowding = true;
                        meanderPauseFrequency = 0.75f;
                        waypointCommanded = followRecall ? false : waypointCommanded;
                        followNutHug = !waypointCommanded && !entity.Foe;
                        behaviourParams[BehaviourType.follow] = (true, 1);
                        behaviourParams[BehaviourType.waypoint] = (!followRecall, 1);
                        behaviourParams[BehaviourType.meander] = (!leashed && !inRange, 1);
                        behaviourParams[BehaviourType.pursue] = (!leashed && !inRange, 1 - Intelligence);
                        behaviourParams[BehaviourType.tango] = (!leashed && inRange && entity.rightStorage, 1);
                        behaviourParams[BehaviourType.martial] = (entity.MainHand, Intelligence);
                        behaviourParams[BehaviourType.grab] = (!entity.MainHand, Intelligence);
                        if (!Enthralled)
                        {
                            State = AIState.none;
                        }
                        break;
                }
            }
            yield return new WaitForSecondsRealtime(ReflexRate);
        }
    }

    protected void StateTransition(AIState newState)
    {
        stateRunTimer = 0;
        State = newState;
        List<BehaviourType> keys = new List<BehaviourType>();
        foreach (BehaviourType key in behaviourParams.Keys)
        {
            keys.Add(key);
        }
        foreach (BehaviourType key in keys)
        {
            if (behaviourParams.ContainsKey(key))
            {
                behaviourParams[key] = (false, 0);
            }
        }
        entity.modSpeed["AIState"] = 0;
        behaviourParams[BehaviourType.sensory] = (true, 0);
        behaviourParams[BehaviourType.wallCrawl] = (true, 0);
        behaviourParams[BehaviourType.dashing] = (true, 0);
        behaviourParams[BehaviourType.itemManagement] = (true, 5);
        trackingTrailingEnabled = false;
        wallCrawlCrowding = false;
    }

    /***** helper coroutines *****/
    private IEnumerator dashingCycle()
    {
        while (true)
        {
            yield return new WaitUntil(() => dashingDesiredDirection != Vector3.zero);
            do
            {
                entity.dashDirection = dashingDesiredDirection.normalized;
                entity.DashCharging = true;
                yield return null;
            } while ((dashingChargeTimer += Time.deltaTime) < dashingChargePeriod);
            entity.dashDirection = dashingDesiredDirection.normalized;
            entity.DashCharging = false;
            yield return new WaitUntil(() => entity.Dashing);
            dashingCooldownTimer = 0;
            dashingChargeTimer = 0;
            dashingDesiredDirection = Vector3.zero;
            yield return null;
        }
    }

    /********** QOL Functions *************/

    private void DeEnergize(BehaviourType key)
    {
        if (Directives.ContainsKey(key))
        {
            Directives.Remove(key);
        }
        if (LookDirectives.ContainsKey(key))
        {
            LookDirectives.Remove(key);
        }
    }

    public static Vector3 angleToDirection(float degrees)
    {
        degrees = degrees % 360f;
        float rads = degrees * (Mathf.PI / 180);
        Vector3 temp = new Vector3(Mathf.Cos(rads), 0, Mathf.Sin(rads));
        return temp;
    }

    public static float getAngle(Vector3 Direction)
    {
        float output = 0;
        float x = Direction.x;
        float z = Direction.z;
        if (z >= 0 && x >= 0)
        {
            output = Vector3.Angle(new Vector3(1, 0, 0), Direction);
        }
        else if (z >= 0 && x < 0)
        {
            output = Vector3.Angle(new Vector3(1, 0, 0), Direction);
        }
        else if (z < 0 && x >= 0)
        {
            output = 0 - Vector3.Angle(new Vector3(1, 0, 0), Direction);
        }
        else
        {
            output = 0 - Vector3.Angle(new Vector3(1, 0, 0), Direction);
        }
        return output;
    }


    public static Vector3 RandomDirection()
    {
        float randRads = (Random.value * 360) * (Mathf.PI / 180);
        return new Vector3(Mathf.Sin(randRads), 0, Mathf.Cos(randRads)).normalized;
    }




}
