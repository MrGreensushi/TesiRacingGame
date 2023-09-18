using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSync : NetworkBehaviour
{
    [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
    public bool clientAuthority = false;
    [SerializeField] internal Transform target;


    [SyncVar(hook = nameof(OnPositionChanged))]
    Vector3 position;
    [SyncVar(hook = nameof(OnRotationChanged))]
    Quaternion rotation;

    bool ClientWithAuthority => clientAuthority && hasAuthority;
    bool IgnoreSync => isServer || ClientWithAuthority;

    public bool discardClientPosition = false;
    void OnPositionChanged(Vector3 _, Vector3 newValue)
    {
        if (IgnoreSync)
            return;
        target.position = newValue;
    }
    void OnRotationChanged(Quaternion _, Quaternion newValue)
    {
        if (IgnoreSync)
            return;
        target.rotation = newValue;
    }

    void Update()
    {
        if (isServer)
        {
            SyncToClients();
        }
        else if (ClientWithAuthority)
        {
            SendToServer();
        }

    }

    [Server]
    void SyncToClients()
    {
        // only update if they have changed more than Sensitivity

        Vector3 currentPosition = target.position;
        position = currentPosition;
        Quaternion currentRotation = target.rotation;
        rotation = currentRotation;
    }

    /// <summary>
    /// Uses Command to send values to server
    /// </summary>
    [Client]
    void SendToServer()
    {
        if (!hasAuthority)
        {
            Debug.LogWarning("SendToServer called without authority");
            return;
        }

        SendPosition();
        SendRotation();
    }

    [Client]
    void SendPosition()
    {
        //float now = Time.time;
        //if (now < previousValue.nextSyncTime)
        //    return;

        Vector3 currentPosition = target.position;


        CmdSendPosition(currentPosition);

    }
    [Client]
    void SendRotation()
    {
        //float now = Time.time;
        //if (now < previousValue.nextSyncTime)
        //    return;

        Quaternion currentRotation = target.rotation;


        CmdSendRotation(currentRotation);

    }


    /// <summary>
    /// Called when Position has changed on the client
    /// </summary>
    [Command]
    void CmdSendPosition(Vector3 position)
    {
        // Ignore messages from client if not in client authority mode
        if (!clientAuthority)
            return;

        //Se voglio che il server scarti qualunque informazione del client 
        if (discardClientPosition)
            return;

        this.position = position;
        target.position = position;
    }
    /// <summary>
    /// Called when Rotation has changed on the client
    /// </summary>
    [Command]
    void CmdSendRotation(Quaternion rotation)
    {
        // Ignore messages from client if not in client authority mode
        if (!clientAuthority)
            return;

        //Se voglio che il server scarti qualunque informazione del client 
        if (discardClientPosition)
            return;

        this.rotation = rotation;
        target.rotation = rotation; ;
    }
}
