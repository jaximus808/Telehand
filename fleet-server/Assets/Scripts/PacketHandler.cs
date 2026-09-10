using System.Collections;
using System.Collections.Generic;
using System; 
using UnityEngine;

public static class PacketHandler 
{
    //packet 0
    public static void HandleNewConnection(int _fromClient, Packet _packet)
    {
        int armId = _packet.ReadInt();
        UDPServer.CreateClient(_fromClient,_packet.remoteEndPoint, armId );

    }

    //packet 1
    public static void HandleHand(int _fromClient, Packet _packet )
    {
        bool hands = _packet.ReadBool();
        if (hands)
        {
            //can make this smaller
            int handCount = _packet.ReadInt();
            Debug.Log(handCount);
            bool right = _packet.ReadBool();
            Debug.Log(handCount);
            int side = 0;
            if (right) side = 1;
            //test floatsZyy
            Vector3[] handPoints1 = UDPServer.ReadHandPoints(_packet);
            //would then apply to hands but do later rn;
            UDPServer.HandManagers[_fromClient][side].UpdateHands(handPoints1);
            UDPServer.HandManagers[_fromClient][side].PrintDifference();
            if (handCount == 2)
            {
                Vector3[] handPoints2 = UDPServer.ReadHandPoints(_packet);
                UDPServer.HandManagers[_fromClient][Math.Abs(side - 1)].UpdateHands(handPoints2);
            }
        }
    }
    public static void HandleNewArmConnection(int _fromClient, Packet _packet)
    {
        Debug.Log("HELLO WTF LOL");
        string _armPass = _packet.ReadString();
        string _nodePass = _packet.ReadString(); 
        UDPServer.CreateArmClient(_fromClient, _packet.remoteEndPoint,_armPass, _nodePass);

    }

    public static void PingCheck(int _fromClient, Packet _packet)
    {
        Debug.Log($"pinged recieved from client ${_fromClient}");
        bool arm = _packet.ReadBool(); 
        UDPServer.HandlePing(_fromClient, arm);
    }
}