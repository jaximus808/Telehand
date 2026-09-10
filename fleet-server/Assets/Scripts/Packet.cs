using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>Sent from server to client.</summary>
/*
public enum ServerPackets
{
    welcome = 1,
    
    
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    welcomeReceived = 1,
    
}
*/

public class Packet : IDisposable
{

    public List<byte> buffer; 
    public byte[] bytes {get; set;}
    public int readPos = 0;
    public IPEndPoint remoteEndPoint; 

    public Packet()
    {
        buffer = new List<byte>(); // Initialize buffer
        readPos = 0; // Set readPos to 0
    }
    public Packet(IPEndPoint endPoint)
    {
        remoteEndPoint = endPoint; 
        buffer = new List<byte>(); // Initialize buffer
        readPos = 0; // Set readPos to 0
    }
    public Packet(byte[] packet, IPEndPoint endPoint)
    {
        remoteEndPoint = endPoint;
        buffer = new List<byte>();
        readPos = 0;

        SetBytes(packet);
    }
    public Packet(byte[] packet)
    {
        buffer = new List<byte>();
        readPos = 0;
        SetBytes(packet);
    }
    public void SetBytes(byte[] _data)
    {
        Write(_data);
        bytes = buffer.ToArray();
    }
    //maybe add move read pos
    public byte[] ReadBytes(int _length)
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            byte[] _value = buffer.GetRange(readPos, _length).ToArray(); // Get the bytes at readPos' position with a range of _length
            readPos += _length;
            return _value; // Return the bytes
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }

    public bool ReadBool()
    {
        if(bytes.Length < readPos) throw new Exception("Cannot read Bool: You are out of range");
        bool data = BitConverter.ToBoolean(bytes, readPos);
        readPos++;
        return data; 
    }

    public float ReadFloat()
    {
        if(bytes.Length < readPos) throw new Exception("Cannot read Float: You are out of range");
        float data = BitConverter.ToSingle(bytes, readPos);
        readPos+=4;
        return data; 
    }

    public int ReadInt()
    {
        if(bytes.Length < readPos) throw new Exception("Cannot read Int: You are out of range");
        
        int data = BitConverter.ToInt32(bytes, readPos);
        readPos+=4;
        return data; 
    }

    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(),ReadFloat(),ReadFloat());
    }

    public String ReadString()
    {
        try
        {
            int _length = ReadInt();
            string _value = Encoding.ASCII.GetString(bytes, readPos, _length);
            if (_value.Length > 0)
            {
                // If _moveReadPos is true string is not empty
                readPos += _length; // Increase readPos by the length of the string
            }
            return _value; // Return the string
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    #region Writing

    public void Write(byte _value)
    {
        buffer.Add(_value);
    }
    /// <summary>Adds an array of bytes to the packet.</summary>
    /// <param name="_value">The byte array to add.</param>
    public void Write(byte[] _value)
    {
        buffer.AddRange(_value);
    }
    /// <summary>Adds a short to the packet.</summary>
    /// <param name="_value">The short to add.</param>
    public void Write(short _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds an int to the packet.</summary>
    /// <param name="_value">The int to add.</param>
    public void Write(int _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a long to the packet.</summary>
    /// <param name="_value">The long to add.</param>
    public void Write(long _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a float to the packet.</summary>
    /// <param name="_value">The float to add.</param>
    public void Write(float _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a bool to the packet.</summary>
    /// <param name="_value">The bool to add.</param>
    public void Write(bool _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a string to the packet.</summary>
    /// <param name="_value">The string to add.</param>
    public void Write(string _value)
    {
        Write(_value.Length); // Add the length of the string to the packet
        buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
    }

    #endregion


    public byte[] ToArray()
    {
        bytes = buffer.ToArray();
        return bytes; 
    }

    public int Length()
    {
        return buffer.Count;
    }
    public int RelativeLength()
    {
        return buffer.Count-readPos;
    }
    private bool disposed = false;

    protected virtual void Dispose(bool _disposing)
    {
        if (!disposed)
        {
            if (_disposing)
            {
                buffer = null;
                bytes = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
