using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Messages
{
    public static void SendInput(int moveX, int moveY, int braking, string playerName)
    {
        InputMessage msg = new InputMessage()
        {
            playerName = playerName,
            moveX = moveX,
            moveY = moveY,
            breaking = braking
        };
        NetworkClient.Send(msg);
    }
}

public struct InputMessage : NetworkMessage
{
    public string playerName;
    public int moveX;
    public int moveY;
    public int breaking;
}