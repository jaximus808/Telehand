using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

//make a check for an existing client 

public class Client 
{
    public int id;
    public int armId;

    public UDP udp; 

    public Client(int _id, int _armId )
    {
        id = _id;
        udp = new UDP(id);
        armId = _armId;
    }

    public class UDP
    {
        public IPEndPoint endPoint;
        //public IPEndPoint remoteEndPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        public void SendData(Packet _packet)
        {
            UDPServer.SendUDPData(endPoint, _packet);
        }

        public void HandleData(Packet _packetData)
        {
            int _packetId = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetData.RelativeLength());

            UDPServer.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {

                    UDPServer.packetHandlers[_packetId](id, _packet);
                }
            });
        }
    }
}