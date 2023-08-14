using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Entity : MonoBehaviour
{
    
    public enum ControlMode
    {
        off,
        Position,
        Velocity
    }
    public enum SimulationMode
    {
        Kinematic,
        Physical
    }

    public enum InteractionMode
    {
        off,
        Trigger,
        Collision
    }

    public ControlMode TranslationalControlMode;
    public Vector3 TranslationalSetpoint;
    public float TranslationalAcceleration;
    public float TranslationalMagnitude;
    private Vector3 TranslationalProcessVector;

    public ControlMode RotationalControlMode;
    public Vector3 RotationalSetpoint;
    public float RotationalAcceleration;
    public float RotationalMagnitude;
    private Vector3 RotationalProcessVector;

    public SimulationMode Simulation;
    private Rigidbody physicsBody;
    private Vector3 kinematicVelocity;
    private Vector3 kinematicAngularVelocity;

    public InteractionMode Interaction;
    public Collider InteractionBox;

    private void Start()
    {
        physicsBody = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        physicsBody.drag = 0;
        physicsBody.angularDrag = 0;
        InteractionBox = GetComponent<Collider>();
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        physicsBody.isKinematic = Simulation == SimulationMode.Kinematic;
        TranslationalProcessVector = getTranslationalProcessVector();
        fixedUpdateTranslationalProcessVector();
        RotationalProcessVector = getRotationalProcessVector();
        fixedUpdateRotationalProcessVector();
    }

    /***** PUBLIC *****/

    public void ImpulseControlVector(Vector3 impulseVector)
    {
        if (Simulation == SimulationMode.Physical)
        {
            physicsBody.velocity += impulseVector;
        }
        else if (Simulation == SimulationMode.Kinematic)
        {
            kinematicVelocity += impulseVector;
        }
    }


    /***** PRIVATE *****/
    private Vector3 getTranslationalProcessVector()
    {
        if (TranslationalControlMode == ControlMode.Position)
        {
            return transform.localPosition;
        }
        else if (TranslationalControlMode == ControlMode.Velocity)
        {
            if (Simulation == SimulationMode.Physical)
            {
                return physicsBody.velocity;
            }
            else if (Simulation == SimulationMode.Kinematic)
            {
                return kinematicVelocity;
            }
        }
        return Vector3.zero;
    }

    private void fixedUpdateTranslationalProcessVector()
    {
        Vector3 processSetpointDelta = TranslationalSetpoint - TranslationalProcessVector;
        Vector3 desiredControlVector = processSetpointDelta.normalized * TranslationalMagnitude;
        float controlIncrement = TranslationalAcceleration * Time.fixedDeltaTime;
        if (Simulation == SimulationMode.Physical)
        {
            physicsBody.velocity = Vector3.MoveTowards(physicsBody.velocity, desiredControlVector, controlIncrement);
        }
        else if (Simulation == SimulationMode.Kinematic)
        {
            kinematicVelocity = Vector3.MoveTowards(kinematicVelocity, desiredControlVector, controlIncrement);
            transform.position += kinematicVelocity;
        }
    }

    private void fixedUpdateRotationalProcessVector()
    {
        if (Simulation == SimulationMode.Physical)
        {

        }
        else if (Simulation == SimulationMode.Kinematic)
        {

        }
    }

    private Vector3 getRotationalProcessVector()
    {
        if (RotationalControlMode == ControlMode.off)
        {
            return Vector3.zero;
        }
        else if (RotationalControlMode == ControlMode.Position)
        {
            return Vector3.zero;
        }
        else if (RotationalControlMode == ControlMode.Velocity)
        {
            return Vector3.zero;
        }
        else
        {
            return Vector3.zero;
        }
    }





}
