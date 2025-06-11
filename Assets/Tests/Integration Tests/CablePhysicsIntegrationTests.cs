using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class CablePhysicsIntegrationTests
{
    CablePhysics cablePhysics;
    Vector3 testPosition, testGravityDisplacement;
    Transform testTransformA;
    Rigidbody bodyA;

    [SetUp]
    public void Setup()
    {
        IntegrationTestHelper.ClearScene();

        SetUpCablePhysics();
        SetTestVariables();
    }

    [UnityTest]
    public IEnumerator Confirm_Default_Values_And_Return_Statements()
    {
        // Checking to see if default values are not set incorrectly
        Assert.AreEqual(Vector3.zero, cablePhysics._position);
        Assert.AreEqual(Vector3.zero, cablePhysics._oldPosition);
        Assert.AreEqual(null, cablePhysics._boundTo);
        Assert.AreEqual(null, cablePhysics._boundRigid);

        yield return null;

        // Confirm the above using CablePhysics lines
        Assert.AreEqual(Vector3.zero, cablePhysics.Position);
        Assert.AreEqual(Vector3.zero, cablePhysics.Velocity);

        yield return null;

        // Checking to see if cable is currently free, should be true at default
        Assert.AreEqual(true, cablePhysics.IsFree());

        yield return null;

        // Checking to see if cable is bound, should be false at default
        Assert.AreEqual(false, cablePhysics.IsBound());

        yield return null;
    }

    [UnityTest]
    public IEnumerator SetCableValues_And_Verify_Physics()
    {
        testPosition = Vector3.up;

        cablePhysics = new CablePhysics(testPosition);

        // Check to see if cablePhysics Position method returns the temp position set in the previous line
        Assert.AreEqual(testPosition, cablePhysics.Position);

        yield return null;

        // Velocity should equal 0 as both _oldPosition and _position are set to the same value
        Assert.AreEqual(Vector3.zero, cablePhysics.Velocity);

        yield return null;

        // Set _position using Position
        cablePhysics.Position = testPosition;

        // Should be equal as position was just set
        Assert.AreEqual(testPosition, cablePhysics._position);

        // Update position to change velocity value, _oldPosition now set to (0.00, 1.00, 0.00) and _position now set to (-1.00, 0.00, 0.00)
        cablePhysics.UpdatePosition(Vector3.left);

        // Check to see if this updates the velocity when called
        Assert.AreEqual(new Vector3(-1.00f, -1.00f, 0.00f), cablePhysics.Velocity);

        yield return null;

        testGravityDisplacement = Vector3.right;

        // Update verlet of cablePhysics class, should go past the first path as IsBound is false
        cablePhysics.UpdateVerlet(testGravityDisplacement);

        // Check to make sure _oldPosition and _position have been updated from updated the verlet
        Assert.AreNotEqual(cablePhysics._oldPosition, cablePhysics._position);

        yield return null;

        // Set transform values to bind class to
        testTransformA.position = new Vector3(3.00f, 3.00f, 5.00f);

        cablePhysics.Bind(testTransformA);

        // Check to see if cablePhysics values are updated
        Assert.AreEqual(testTransformA, cablePhysics._boundTo);
        Assert.AreEqual(bodyA, cablePhysics._boundRigid);
        Assert.AreEqual(cablePhysics._oldPosition, cablePhysics._boundTo.position);

        yield return null;

        // Call the update verlet again to follow the IsBound path, make _boundRigid null to see first path
        Rigidbody tempRigid = cablePhysics._boundRigid;
        cablePhysics._boundRigid = null;

        cablePhysics.UpdateVerlet(testGravityDisplacement);

        Assert.AreEqual(cablePhysics._boundTo.position, cablePhysics._position);

        yield return null;

        cablePhysics._boundRigid = tempRigid;

        // Call the update verlet again with interpolation to Interpolate
        cablePhysics._boundRigid.interpolation = RigidbodyInterpolation.Interpolate;

        cablePhysics.UpdateVerlet(testGravityDisplacement);

        // Assert calculation is correctly being called in switch case
        Assert.AreEqual(cablePhysics._boundRigid.position + (cablePhysics._boundRigid.velocity * Time.fixedDeltaTime) / 2, cablePhysics._position);

        yield return null;

        // Call the update verlet again with interpolation set to None
        cablePhysics._boundRigid.interpolation = RigidbodyInterpolation.None;

        cablePhysics.UpdateVerlet(testGravityDisplacement);

        // Assert calculation is correctly being called in switch case
        Assert.AreEqual(cablePhysics._boundRigid.position + cablePhysics._boundRigid.velocity * Time.fixedDeltaTime, cablePhysics._position);

        yield return null;

        // Check _boundTo, and _boundRigid values after calling UnBind method
        cablePhysics.UnBind();

        Assert.AreEqual(null, cablePhysics._boundRigid);
        Assert.AreEqual(null, cablePhysics._boundTo);

        yield return null;
    }

    private void SetUpCablePhysics()
    {
        GameObject cablePhysicsGO = new();
        cablePhysicsGO.SetActive(false);

        cablePhysics = cablePhysicsGO.AddComponent<CablePhysics>();
        cablePhysics._position = new();
        cablePhysics._oldPosition = new();

        cablePhysicsGO.SetActive(true);
    }

    private void SetTestVariables()
    {
        testPosition = new();
        testGravityDisplacement = new();

        GameObject transformAGO = new();
        bodyA = transformAGO.AddComponent<Rigidbody>();
        testTransformA = transformAGO.GetComponent<Transform>();
    }

    [TearDown]
    public void Teardown()
    {
        IntegrationTestHelper.ClearScene();
    }
}
