using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Net.Sockets;

public class Network : MonoBehaviour
{

	public static Network Instance;

	[Header("Network Settings")] 
	public string ServerIP = "127.0.0.1";

	public int ServerPort = 5500;

	public bool IsConnected;
	public TcpClient PlayerSocket;
	public NetworkStream MyStream;
	public StreamWriter myWritter;
	public StreamReader myReader;

	private byte[] asyncBuff;
	public bool shouldHandleData;
	
	private void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		ConnectToGameServer();
	}

	void ConnectToGameServer()
	{
		if (PlayerSocket != null)
		{
			if (PlayerSocket.Connected || IsConnected)
			{
				return;
			}
			PlayerSocket.Close();
			PlayerSocket = null;
		}
		
		PlayerSocket = new TcpClient();
		PlayerSocket.ReceiveBufferSize = 4096;
		PlayerSocket.SendBufferSize = 4096;
		PlayerSocket.NoDelay = false;
		Array.Resize(ref asyncBuff, 8192);
		PlayerSocket.BeginConnect(ServerIP, ServerPort, new AsyncCallback(ConnectCallback), PlayerSocket);
		IsConnected = true;
	}

	void ConnectCallback(IAsyncResult result)
	{
		if (PlayerSocket != null)
		{
			PlayerSocket.EndConnect(result);
			if (PlayerSocket.Connected == false)
			{
				IsConnected = false;
				return;
			}
			else
			{
				PlayerSocket.NoDelay = true;
				MyStream = PlayerSocket.GetStream();
				MyStream.BeginRead(asyncBuff, 0, 8192, OnReceive, null);
			}
		}
	}

	void OnReceive(IAsyncResult result)
	{
		if (PlayerSocket != null)
		{
			if (PlayerSocket == null)
			{
				return;
			}

			int byteArray = MyStream.EndRead(result);
			byte[] myBytes = null;
			Array.Resize(ref myBytes, byteArray);
			Buffer.BlockCopy(asyncBuff, 0 ,myBytes,0,byteArray);

			if (byteArray == 0)
			{
				Debug.Log("You got disconnected from the server.");
				PlayerSocket.Close();
				return;
			}
			
			//HandleData

			if (PlayerSocket == null)
			{
				return;
			}

			MyStream.BeginRead(asyncBuff, 0, 8192, OnReceive, null);
		}
	}
}
