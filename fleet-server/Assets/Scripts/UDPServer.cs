using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Linq; 
using CandyCoded.env; 
public class UDPServer : MonoBehaviour
{
    // Start is called before the first frame update

    //going to need a python communicator object reference 
    //public HandRendererOne[] HandManagers;
    public GameObject sethand; 

    public static GameObject HandRenderer; 

    public static Dictionary<int, HandRendererOne[]> HandManagers = new Dictionary<int, HandRendererOne[]>();
    public static Dictionary<int, Client> ConnectedClients = new Dictionary<int, Client>();

    public static Dictionary<int, ArmClient> ConnectedArmClients = new Dictionary<int, ArmClient>(); 

    private static readonly List<Action> MainThreadQueue = new List<Action>();
    private static readonly List<Action> MainCopyThreadQueue = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;

    private static List<int> pingedUserIds = new List<int>();

    private static List<int> pingedArmIds = new List<int>();

    public delegate void Handler(int _fromClient, Packet _packet);

    public static Dictionary<int, Handler> packetHandlers;

    public bool active = false;
    public static bool pinging = false; 

    public int inPort = 8000; //port to receive data
    public int outPort = 8001; //port to send data

    public float setPingTimer;
    private float curPingTimer;
    public float setMasterAttemptTimer;
    private float curMasterAttemptTimer;

    public float setPingCheckTimer;
    private float curPingCheckTimer;

    private static UdpClient client; 
    private static IPEndPoint remoteEndPoint; 
    private static Thread receiveThread;

    private static Dictionary<string, string> envContent; 

    private bool connectedToMasterServer = false;     

    public string MasterTargetIp; 

    public void SendData(string message)
    {
        try 
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch(Exception error)
        {
            Debug.Log(error.ToString());
        }
    }

    private void Awake()
    {
        envContent = env.ParseEnvironmentFile();
        WebCommunicator.setHost(MasterTargetIp);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, outPort);
        HandRenderer = sethand;
        client = new UdpClient(inPort);
        client.BeginReceive(ReceiveUDPData,null);
        //0 connect
        //1 read and handle vector data
        curPingTimer = setPingTimer;
        curPingCheckTimer = setPingCheckTimer; 
        curMasterAttemptTimer = 0f;
        packetHandlers = new Dictionary<int, Handler>()
        {
            {0, PacketHandler.HandleNewConnection},
            {1, PacketHandler.HandleHand},
            {2, PacketHandler.HandleNewArmConnection},
            {3, PacketHandler.PingCheck}
        };


        //receiveThread = new Thread(new ThreadStart(ReceiveData));
        //receiveThread.IsBackground = true; 
        //receiveThread.Start();
        Debug.Log("Listening");

    }

    public static Vector3[] ReadHandPoints(Packet packet)
    {
        Vector3[] data = new Vector3[21];
        for(int i = 0; i < 21; i++)
        {
            data[i] = packet.ReadVector3();
            //Debug.Log(data[i]);
        }
        return data;
    }

    public async static void CreateArmClient(int _id, IPEndPoint endPoint, string _armPass, string _nodePass )
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            //hide this later
            {"fleetPass", UDPServer.envContent["FleetServerPass"]},
            {"pass", _nodePass },
            {"id", _id.ToString() }
        };
        Debug.Log("WAHH??");
        ReturnData content = await WebCommunicator.PostSend("/server/auth/connectedArmClient",data);
        Debug.Log(content.error);
        Debug.Log(content.message);
        if (content.error) return;
        ConnectedArmClients.Add(_id, new ArmClient(_id, _armPass));
        ConnectedArmClients[_id].udp.Connect(endPoint);
        Debug.Log($"ArmClient created with id: {ConnectedArmClients[_id].id} created with armPass: {ConnectedArmClients[_id].armPass}");
        using (Packet _packet = new Packet())
        {
            _packet.Write(0);
            _packet.Write(1);
            ConnectedArmClients[_id].udp.SendData(_packet);
        }
    }

    public static void CreateClient(int _fromClient, IPEndPoint endpoint, int armId)
    {
        int _id = 0;
        foreach (KeyValuePair<int, Client> _client in ConnectedClients)
        {
            if ((_client.Key + 1) - _id == 1)
            {
                _id++;
            }
            else
            {
                break;
            }
        }
        ConnectedClients.Add(_id, new Client(_id, armId));
        ConnectedClients[_id].udp.Connect(endpoint);
        HandRendererOne leftHand = Instantiate(HandRenderer, new Vector3(-2f, 10f, 3.2f), Quaternion.identity).GetComponent<HandRendererOne>();
        HandRendererOne rightHand = Instantiate(HandRenderer, new Vector3(-7.7f, 10f, 3.2f), Quaternion.identity).GetComponent<HandRendererOne>();
        HandRendererOne[] newHands = new HandRendererOne[2] { leftHand, rightHand };

        rightHand.SetArmId(armId); 
        HandManagers.Add(_id, newHands);
        Debug.Log("New Client Created!");
        
        //maybe create a better way of doing this lol
        using (Packet packet = new Packet())
        {
            packet.Write(0);
            packet.Write(_id);
            ConnectedClients[_id].udp.SendData(packet);
        }
    }


    private void ReceiveUDPData(IAsyncResult _result)
    {
        
        try
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.EndReceive(_result,ref anyIP);
            
            client.BeginReceive(ReceiveUDPData, null);
                // bool hnads = Encoding.UTF8.GetString(data);
            Packet packet = new Packet(data, anyIP);

            //check for new client connection
            int typeOfConnection = packet.ReadInt();
            
            Debug.Log(typeOfConnection);

            
            if(typeOfConnection == -1)
            {
                string pass = packet.ReadString(); 

                Debug.Log(pass);
                if(pass != "Nwifugu31393g2HSDUg18173d_fb3yja")
                {
                    Debug.Log("wrong arm pass");
                    return;
                }
                int packetID = packet.ReadInt();

                Debug.Log(packetID);
                int armid = packet.ReadInt();
                if(packetID == 0) 
                {
                    //string armPass = packet.ReadString();
                    UDPServer.ExecuteOnMainThread(() =>
                    {
                        UDPServer.packetHandlers[2](armid, packet);
                    });
                }
                else if(packetID == 1)
                {
                    Packet _packet = new Packet();
                    
                    _packet.Write(true); // 0 means arm
                    _packet.ToArray();
                    UDPServer.ExecuteOnMainThread(() =>
                    {
                        UDPServer.packetHandlers[3](armid, _packet);
                    });
                    
                    
                }
                
                return;
            }


            int id = packet.ReadInt();


            if(id == -1 )
            {
                UDPServer.ExecuteOnMainThread(() =>
                {
                    UDPServer.packetHandlers[0](id, packet);
                });

            }
            else if(ConnectedClients[id].udp.endPoint.ToString() == anyIP.ToString() )
            {
                
                ConnectedClients[id].udp.HandleData(packet);
                ProcessInput(packet);
            }
                //handle here 
        
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
        
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                client.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    private void ProcessInput(Packet input)
    {
        // PROCESS INPUT RECEIVED STRING HERE

        if (!active) // First data arrived so tx started
        {
            active = true;
        }
    }

    private void FixedUpdate()
    {
            if (curPingTimer <= 0f)
            {
                StartClientPing();
                curPingTimer = setPingTimer;
                Debug.Log("start pinging");
            }
            else
            {
                curPingTimer -= Time.fixedDeltaTime;

            }
        
        if (pinging)
        {
            if (curPingCheckTimer <= 0f)
            {
                //start clientCheck
                CheckClientDisconnect();
                curPingCheckTimer = setPingCheckTimer;
                pinging = false;
            }
            else
            {
                curPingCheckTimer -= Time.fixedDeltaTime;
            }
        }
        if(!connectedToMasterServer)
        {
            if(curMasterAttemptTimer <= 0f)
            {
                ConnectToMasterServer();
                curMasterAttemptTimer = setMasterAttemptTimer; 
            }
            else
            {
                curMasterAttemptTimer -= Time.fixedDeltaTime; 
            }

        }       

            UpdateMain();
    }

        

    private async void DisconnectFromMasterServer(string delUsers)
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            //hide this later
            {"fleetPass", envContent["FleetServerPass"]},
            {"delete", delUsers },
        };
        ReturnData content = await WebCommunicator.PostSend("/server/auth/disconnectFleet", data);
        Debug.Log(content.error);
        Debug.Log(content.message);

    }

    private async void ConnectToMasterServer()
    {

        try
        {
            Debug.Log(envContent["FleetServerPass"]);
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"fleetPass", envContent["FleetServerPass"]},
                {"port", inPort.ToString() },
            };
            ReturnData content = await WebCommunicator.PostSend("/server/auth/registerFleet", data);
            if(!content.error){
                connectedToMasterServer = true; 
                Debug.Log("connected to master server!");
            } 
            
        }
        catch(Exception _ex)
        {
            Debug.Log($"Failed Connecting to Master Server with error {_ex}");
        }
        
    }

    private void CheckClientDisconnect()
    {
        for (int i = 0; i < pingedUserIds.Count; i++)
        {
            ConnectedClients.Remove(pingedUserIds[i]);
            Debug.Log($"Connected Client ${i} Timed out");
        }
        string deletedUsers = "";
        for (int i = 0; i < pingedArmIds.Count; i++)
        {
            ConnectedArmClients.Remove(pingedArmIds[i]);
            deletedUsers += $"{pingedArmIds[i]},"; 
            Debug.Log($"Connected Arm Client ${i} Timed out");
        }


        if (pingedUserIds.Count >0) pingedUserIds.Clear();
        if (pingedArmIds.Count > 0)
        {
            DisconnectFromMasterServer(deletedUsers);
            pingedArmIds.Clear();
        }
            Debug.Log("pinging done");
    }

    private void StartClientPing()
    {

        pingedUserIds.AddRange(ConnectedClients.Keys.ToArray());
        pingedArmIds.AddRange(ConnectedArmClients.Keys.ToArray());

        for(int i = 0; i < pingedUserIds.Count; i++)
        {
            using (Packet _packet = new Packet())
            {
                _packet.Write(1);
                ConnectedClients[pingedUserIds[i]].udp.SendData(_packet);
            }
                
        }
        Debug.Log(ConnectedArmClients.Count); 
        for (int i = 0; i < pingedArmIds.Count; i++)
        {
            using (Packet _packet = new Packet())
            {
                _packet.Write(1);
                ConnectedArmClients[pingedArmIds[i]].udp.SendData(_packet);
            }

        }
        pinging = true; 

    }

    public static void HandlePing(int _id, bool arm)
    {
        if (!pinging) return; 
        if(arm)
        {
            int index = pingedArmIds.IndexOf(_id);
            if (index == -1) return;
            pingedArmIds.RemoveAt(index); 

        }
        else
        {
            int index = pingedUserIds.IndexOf(_id);
            if (index == -1) return;
            pingedUserIds.RemoveAt(index);
        }
    }

    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            Console.WriteLine("No action to execute on main thread!");
            return;
        }
        lock(MainThreadQueue)
        {
            MainThreadQueue.Add(_action);
            actionToExecuteOnMainThread = true; 
        }
    }

    public static void UpdateMain()
    {
        if(actionToExecuteOnMainThread)
        {
            MainCopyThreadQueue.Clear();
            lock(MainThreadQueue)
            {
                MainCopyThreadQueue.AddRange(MainThreadQueue);
                MainThreadQueue.Clear();
                actionToExecuteOnMainThread = false; 
            }
            for (int i = 0; i < MainCopyThreadQueue.Count; i++)
            {
                MainCopyThreadQueue[i]();
            }
        }
    }

    private void HandleServerClose()
    {
        Packet packet = new Packet();
        packet.Write(10);

        foreach(KeyValuePair<int, Client> _client in ConnectedClients)
        {
            _client.Value.udp.SendData(packet);
        }
        string deletedUsers = "";
        foreach (KeyValuePair<int, ArmClient> _client in ConnectedArmClients)
        {
            _client.Value.udp.SendData(packet);
            deletedUsers += $"{_client.Key},";
        }
        DisconnectFromMasterServer(deletedUsers); 


    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        HandleServerClose(); 
        client.Close();
        
    }
}
