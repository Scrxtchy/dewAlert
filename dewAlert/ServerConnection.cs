using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace dewAlert
{
	public delegate void ServerEventHandler(object sender, string message);
	public delegate void ServerCloseHandler(object sender, int code);
	public delegate void ServerOpenHandler(object sender);
	public class ServerConnection
	{
		public WebSocket ws;

		public event ServerEventHandler ServerMessage;
		public event ServerCloseHandler ServerClose;
		public event ServerOpenHandler ServerOpen;
		public ServerConnection(string address, string password)
		{
			this.Address = address;
			this.Password = password;
			try
			{
				ws = new WebSocket(address, "dew-rcon");

				ws.OnOpen += Ws_OnOpen;
				ws.OnMessage += Ws_OnMessage;
				ws.OnClose += Ws_OnClose;
				ws.Connect();
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		private void Ws_OnMessage(object sender, MessageEventArgs e)
		{
			if (ServerMessage != null)
				ServerMessage(this, e.Data);
		}

		private void Ws_OnOpen(object sender, System.EventArgs e)
		{
			if (ServerOpen != null)
				ServerOpen(this);

			ws.Send(Encoding.ASCII.GetBytes(Password));
			ws.Send(Encoding.ASCII.GetBytes("Server.SendChatToRconClients 1"));
		}

		private void Ws_OnClose(object sender, CloseEventArgs e)
		{
			if (ServerMessage != null)
			{
				ServerClose(this, e.Code);
				Restart();
			}
		}

		public void Command(string cmd)
		{
			if (!ws.IsAlive)
				Restart();

			try
			{
				ws.Send(Encoding.ASCII.GetBytes(cmd));
			}
			catch (Exception)
			{
			}
		}

		public void Close()
		{
			if (ws.IsAlive)
				ws.Close();
		}

		private void Restart()
		{
			Console.WriteLine("Restarting Websocket");
			ws = new WebSocket(Address, "dew-rcon");
			ws.OnOpen += Ws_OnOpen;
			ws.OnMessage += Ws_OnMessage;
			ws.Connect();

		}

		private string Password { get; set; }
		private string Address { get; set; }
	}

	class ServerEvent : EventArgs
	{
		public ServerEvent(string message)
		{
			this.message = message;
		}

		private string message;

		public string Message
		{
			get { return message; }
		}
	}
}