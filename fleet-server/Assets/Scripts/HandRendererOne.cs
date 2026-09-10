using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRendererOne : MonoBehaviour
{
    

    public float scale;

    public GameObject LandMark; 
    public Transform[] landmarkPos = new Transform[21];
    public Vector3 Offset = new Vector3(0f, 0f, 0f); //test for now

    private int[] previousAngle = new int[4] { 0, 0, 0, 0 };
    private int[] currentDifference = new int[4] { 0, 0, 0, 0 };
    private bool updated = false; 
    private Vector3[] buffer;
    private int armId;

    public void SetArmId(int _armId)
    {
        armId = _armId;
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 21; i++)
        {
            landmarkPos[i] = Instantiate(LandMark, transform.position,Quaternion.identity).transform;
            landmarkPos[i].gameObject.name = $"LandMark: {i}";
            //landmarkPos[i].position = 50f;
        }
    }


    // Update is called once per frame
    public void UpdateHands(Vector3[] newPos)
    {
        updated = true; 
        
        buffer = newPos;
        
    }

    public void PrintDifference()
    {
    }

    public int[] CalculateRotationJoints()
    {
        Debug.Log("Start");
        for(int i = 0; i < 4; i++)
        {
            float c = Vector3.Distance(landmarkPos[5+i*4].position, landmarkPos[6 + i * 4].position);
            float b = Vector3.Distance(landmarkPos[5+i*4].position, landmarkPos[0].position);
            float a = Vector3.Distance(landmarkPos[6 + i * 4].position, landmarkPos[0].position);

            float ang = Mathf.Acos((b*b + c*c -a*a )/(2*b*c)); 

            previousAngle[i] = (int)(((Mathf.Rad2Deg*(ang)-80f)/90 *360 ));
            Debug.Log(previousAngle[i]);
        }

        Debug.Log("End");
        return previousAngle; 
    }


    private void FixedUpdate()
    {
        if(!updated) return;
        updated = false; 
        for(int i = 0; i < 21; i++)
        {
            //Debug.Log("balls");
            //Debug.Log(buffer[i]);
            //Debug.Log(transform.position + scale * new Vector3(buffer[i].x, buffer[i].y * -1, buffer[i].z));
            landmarkPos[i].position = transform.position+scale* new Vector3(buffer[i].x, buffer[i].y*-1, buffer[i].z);
            //Debug.Log(landmarkPos[i].position);
        }
        if (armId == 0) return;
        CalculateRotationJoints();
        using (Packet _packet = new Packet())
        {
            _packet.Write(2);
            _packet.Write(previousAngle[0]);
            _packet.Write(previousAngle[1]);
            _packet.Write(previousAngle[2]);
            UDPServer.ConnectedArmClients[armId].udp.SendData(_packet);

        }
        
    }
}
